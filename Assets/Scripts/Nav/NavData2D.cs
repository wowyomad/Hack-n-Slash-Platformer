using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Nav2D
{

    public class NavData2D : MonoBehaviour
    {
        private void Update()
        {
            var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                var navPoint = GetClosestNavPoint(mouse);
                if (navPoint != null)
                {
                    Debug.Log($"NavPoint: {navPoint.CellPos}, TypeMask: {navPoint.TypeMask}, Position: {navPoint.Position}, Normal: {navPoint.Normal}");
                    for (int i = 0; i < navPoint.Connections.Count; i++)
                    {
                        var connection = navPoint.Connections[i];
                        Debug.Log($"Connection {i}: {connection.Point.CellPos}, Type: {connection.Type}, Weight: {connection.Weight}");
                    }
                }
            }
        }

        [System.Serializable]
        public class NavCell
        {
            public NavPoint Transparent;
            public NavPoint Ground;
        }

        private Dictionary<Vector3Int, NavCell> m_NavCells = new Dictionary<Vector3Int, NavCell>();

        [SerializeField] private List<NavPointEntry> m_SerializedNavPoints = new List<NavPointEntry>();
        [SerializeField] private NavWeights m_NavWeights;

        [Header("Tilemap")]
        [SerializeField] private Tilemap m_GroundTilemap;
        [SerializeField] private Tilemap m_TransparentGround;
        [SerializeField] private LayerMask m_GroundLayerMask;
        [SerializeField] private LayerMask m_TransparentLayerMask;
        public LayerMask CollisionMask => m_GroundLayerMask | m_TransparentLayerMask;
        public Vector2 CellSize => m_CellSize;

        [Header("Actor")]
        [SerializeField] private Vector2 m_ActorSize = new Vector2(1.0f, 2.0f);
        public Vector2 ActorSize => m_ActorSize;
        [SerializeField] private float m_JumpHeight = 4.0f;
        [SerializeField] private float m_FallHeight = 8.0f;
        [SerializeField] private float m_FallJumpDistance = 5.0f;

        [Header("Raycast")]
        [SerializeField] private float m_ConeAngle = 150.0f;
        [SerializeField] private int m_NumberOfRays = 11;
        [SerializeField] private float m_CastDistance = 32.0f;

        [Header("Debug")]
        [SerializeField] private bool m_DrawGizmos = true;

        [Header("Slope Settings")]
        [Tooltip("Maximum slope angle (in degrees) your AI can walk")]
        [SerializeField] private float m_MaxWalkableSlopeAngle = 45f;
        [Tooltip("Minimum slope angle (in degrees) to consider as a slope nav‐point")]
        [SerializeField] private float m_MinSlopeAngle = 1f;
        // small offset to avoid self‐hits
        private float RaycastSkin => VERY_SMALL_FLOAT;

        // compute ground‐normal threshold from max walkable angle
        private float GroundNormalThreshold => Mathf.Cos(m_MaxWalkableSlopeAngle * Mathf.Deg2Rad);

        public static readonly float VERY_SMALL_FLOAT = 0.005f;
        private const float GROUND_NORMAL_THRESHOLD = 0.5f; //cos 60

        private Vector2 m_TileAnchor;
        private float m_TileSize => m_TileAnchor.x * 2.0f;
        private float m_NavPointVerticalOffset;
        public float VerticalTileOffset => m_NavPointVerticalOffset;
        private Vector2 m_CellSize;
        private bool m_Awaken;

        private void OnEnable()
        {
            m_TileAnchor = m_GroundTilemap.tileAnchor;
            m_NavPointVerticalOffset = m_TileAnchor.y + VERY_SMALL_FLOAT;

            if (m_GroundTilemap.cellSize != m_TransparentGround.cellSize)
            {
                string message = "NavData2D: Tilemaps have different cell sizes!";
                Debug.LogError(message, this);
                throw new System.ArgumentException(message);
            }

            if (m_GroundLayerMask == 0 || m_TransparentLayerMask == 0)
            {
                string message = "NavData2D: Layer masks are not set!";
                Debug.LogError(message, this);
                throw new System.ArgumentException(message);
            }

            if (m_NavWeights == null)
            {
                string message = "NavData2D: NavWeights is not set!";
                Debug.LogError(message, this);
                throw new System.ArgumentException(message);
            }

            DeserializeNavPoints();
        }

        private void Awake()
        {
            m_CellSize = m_GroundTilemap.cellSize;

            Generate();
            m_Awaken = true;
        }

        private void OnDisable()
        {

        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (!m_Awaken) return;

            Generate();
        }

        private float GetConnectionWeight(Connection connection, NavPoint from, NavPoint to)
        {
            switch (connection.Type)
            {
                case ConnectionType.Surface:
                    return m_NavWeights.SurfaceWeight;
                case ConnectionType.Jump:
                    return m_NavWeights.JumpWeight;
                case ConnectionType.Fall:
                    return m_NavWeights.FallWeight;
                case ConnectionType.TransparentJump:
                    return m_NavWeights.TransparentJumpWeight;
                case ConnectionType.TransparentFall:
                    return m_NavWeights.TransparentFallWeight;
                case ConnectionType.Slope:
                    float dy = to.Position.y - from.Position.y;
                    if (dy > 0.01f)
                        return m_NavWeights.SlopeWeight * m_NavWeights.SlopeUpMultiplier;
                    else if (dy < -0.01f)
                        return m_NavWeights.SlopeWeight * m_NavWeights.SlopeDownMultiplier;
                    else
                        return m_NavWeights.SlopeWeight;
                default:
                    return 1f;
            }
        }


        public List<NavPoint> GetPath(Vector2 from, Vector2 to)
        {
            List<NavPoint> buffer = new List<NavPoint>();
            return GetPath(from, to, buffer);
        }

        public List<NavPoint> GetPath(Vector2 from, Vector2 to, List<NavPoint> buffer)
        {
            buffer.Clear();

            NavPoint startNavPoint = GetClosestNavPoint(from);
            NavPoint endNavPoint = GetClosestNavPoint(to);

            return GetPath_ThreadSafe(startNavPoint, endNavPoint, from, to, buffer);
        }

        public List<NavPoint> GetPath_ThreadSafe(NavPoint startNavPoint, NavPoint endNavPoint, Vector2 from, Vector2 to, List<NavPoint> buffer)
        {
            if (startNavPoint == null || endNavPoint == null)
            {
                Debug.LogWarning($"NavData2D: No path found. Start ({startNavPoint}) or end {endNavPoint} point is null: ");
                return null;
            }

            float initialDistance = Vector2.Distance(startNavPoint.Position, endNavPoint.Position);
            if (initialDistance < m_TileAnchor.x * 3.0f)
            {
                buffer.Add(new NavPoint { Position = new Vector2(to.x, endNavPoint.Position.y) });
                return buffer;
            }

            var openSet = new List<NavPoint> { startNavPoint };
            var cameFrom = new Dictionary<NavPoint, NavPoint>();
            var gScore = new Dictionary<NavPoint, float> { [startNavPoint] = 0f };
            var fScore = new Dictionary<NavPoint, float> { [startNavPoint] = initialDistance };

            while (openSet.Count > 0)
            {
                NavPoint current = openSet[0];
                float minF = fScore.ContainsKey(current) ? fScore[current] : float.PositiveInfinity;
                foreach (var node in openSet)
                {
                    float f = fScore.ContainsKey(node) ? fScore[node] : float.PositiveInfinity;
                    if (f < minF)
                    {
                        minF = f;
                        current = node;
                    }
                }

                if (current == endNavPoint)
                {
                    buffer.Add(current);
                    while (cameFrom.ContainsKey(current))
                    {
                        current = cameFrom[current];
                        buffer.Add(current);
                    }
                    buffer.Reverse();

                    if (buffer.Count >= 2)
                    {
                        var start = buffer[0];
                        var next = buffer[1];
                        var surfaceConn = start.Connections.Find(c => c.Point == next && (c.Type == ConnectionType.Surface || c.Type == ConnectionType.Slope));
                        if (surfaceConn != null)
                        {
                            float startDist = Vector2.Distance(start.Position, to);
                            float nextDist = Vector2.Distance(next.Position, to);
                            if (nextDist < startDist)
                            {
                                buffer.RemoveAt(0);
                            }
                        }
                    }


                    if (Vector2.Distance(new Vector2(to.x, endNavPoint.Position.y), endNavPoint.Position) < 1.0f
                        && buffer.Count >= 2)
                    {
                        var lastConnection = buffer[buffer.Count - 2].Connections.Find(c => c.Point.CellPos == buffer[buffer.Count - 1].CellPos);

                        if (lastConnection == null ||
                            (lastConnection.Type != ConnectionType.Jump &&
                             lastConnection.Type != ConnectionType.Fall &&
                             lastConnection.Type != ConnectionType.TransparentJump &&
                             lastConnection.Type != ConnectionType.TransparentFall))
                        {
                            buffer[buffer.Count - 1] = new NavPoint
                            {
                                Position = new Vector2(to.x, endNavPoint.Position.y),
                                CellPos = endNavPoint.CellPos,
                                Connections = endNavPoint.Connections,
                                TypeMask = endNavPoint.TypeMask
                            };
                        }
                    }

                    if (Vector2.Distance(buffer[0].Position, new Vector2(from.x, buffer[0].Position.y)) < 1.0f
                        && buffer.Count >= 2)
                    {
                        var firstConnection = buffer[0].Connections.Find(c => c.Point.CellPos == buffer[1].CellPos);

                        if (firstConnection == null ||
                            (firstConnection.Type != ConnectionType.Jump &&
                             firstConnection.Type != ConnectionType.Fall &&
                             firstConnection.Type != ConnectionType.TransparentJump &&
                             firstConnection.Type != ConnectionType.TransparentFall))
                        {
                            buffer[0] = new NavPoint
                            {
                                Position = new Vector2(from.x, buffer[0].Position.y),
                                CellPos = buffer[0].CellPos,
                                Connections = buffer[0].Connections,
                                TypeMask = buffer[0].TypeMask
                            };
                        }
                    }
                    return buffer;
                }

                openSet.Remove(current);

                foreach (var connection in current.Connections)
                {
                    var neighbor = connection.Point;
                    float tentativeGScore = gScore[current] + GetConnectionWeight(connection, current, neighbor);

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        float hScore = Vector2.Distance(neighbor.Position, endNavPoint.Position);
                        fScore[neighbor] = tentativeGScore + hScore;
                        openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }



        public NavPoint GetClosestNavPoint(Vector2 intent)
        {
            Vector2 target = intent;
            {
                if (GetCell(target, out var cell, false))
                {
                    if (cell.Transparent != null)
                        target.y = (float)cell.Transparent.CellPos.y - 0.01f;
                }
            }

            Vector2 closestPoint = target;
            float closestDistanceToTarget = float.MaxValue;
            bool anyHit = false;

            Vector2 mainRayDirection = Vector2.down;
            RaycastHit2D mainHit = Physics2D.Raycast(target, mainRayDirection, m_CastDistance, CollisionMask);

            Vector2 surfaceNormal = Vector2.up;
            if (mainHit.collider != null && Vector2.Dot(mainHit.normal, Vector2.up) > GROUND_NORMAL_THRESHOLD)
            {
                closestPoint = mainHit.point;
                closestDistanceToTarget = Vector2.Distance(mainHit.point, target);
                anyHit = true;
                surfaceNormal = mainHit.normal;
            }

            if (m_NumberOfRays > 1)
            {
                Vector2 perp = new Vector2(-surfaceNormal.y, surfaceNormal.x);

                float halfAngleRad = m_ConeAngle * Mathf.Deg2Rad / 2f;
                for (int i = 0; i < m_NumberOfRays; i++)
                {
                    float t = (float)i / (m_NumberOfRays - 1);
                    float angleOffsetRad = Mathf.Lerp(-halfAngleRad, halfAngleRad, t);

                    float cos = Mathf.Cos(angleOffsetRad);
                    float sin = Mathf.Sin(angleOffsetRad);
                    Vector2 rayDirection = -surfaceNormal * cos + perp * sin;

                    RaycastHit2D hit = Physics2D.Raycast(target, rayDirection, m_CastDistance, CollisionMask);

                    if (hit.collider != null && Vector2.Dot(hit.normal, Vector2.up) > GROUND_NORMAL_THRESHOLD)
                    {
                        float distance = Vector2.Distance(hit.point, target);
                        if (distance < closestDistanceToTarget)
                        {
                            closestDistanceToTarget = distance;
                            closestPoint = hit.point;
                            anyHit = true;
                        }
                    }
                }
            }

            if (anyHit)
            {
                {
                    Vector3 tilePosition = new Vector3(closestPoint.x, closestPoint.y - RaycastSkin, 0f);

                    if (GetCell(tilePosition, out var cell, false))
                    {
                        return cell.Transparent ?? cell.Ground;
                    }
                    else
                    {
                        List<Vector2> offsets = new List<Vector2>
                        {
                            new Vector2(-0.5f, 0.0f),
                            new Vector2(0.5f, 0.0f),
                            new Vector2(0.0f, -0.5f),
                            new Vector2(0.0f, 0.5f),
                            new Vector2(-0.5f, -0.5f),
                            new Vector2(0.5f, -0.5f),
                            new Vector2(-0.5f, 0.5f),
                            new Vector2(0.5f, 0.5f)
                        };

                        //Applying offsets to the closest point, find the closest one to the target
                        foreach (var offset in offsets)
                        {
                            Vector3 offsetPos = closestPoint + offset;
                            if (GetCell(offsetPos, out var cellOffset, false))
                            {
                                if (cellOffset.Transparent != null)
                                    return cellOffset.Transparent;
                                else if (cellOffset.Ground != null)
                                    return cellOffset.Ground;
                            }
                        }
                    }
                }

            }
            return null;

        }


        public void Generate()
        {
            Debug.Log("Generating NavPoints...");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            if (m_NavCells == null)
            {
                m_NavCells = new Dictionary<Vector3Int, NavCell>();
            }
            m_NavCells.Clear();

            GenerateNavPoints(m_TransparentGround, NavPoint.Type.Surface | NavPoint.Type.Transparent);
            GenerateNavPoints(m_GroundTilemap, NavPoint.Type.Surface);

            string durationStr = stopwatch.Elapsed.TotalSeconds.ToString("N3");

            Debug.Log($"Generated {m_NavCells.Count} points in {durationStr}s.");

            stopwatch.Reset();

            GenerateConnections();

            durationStr = stopwatch.Elapsed.TotalSeconds.ToString("N3");
            Debug.Log($"Generated connections in {durationStr}s.");

            stopwatch.Stop();
        }

        private void GenerateNavPoints(Tilemap tilemap, NavPoint.Type baseType)
        {
            if (tilemap == null) return;

            LayerMask layerMask = baseType.HasFlag(NavPoint.Type.Transparent) ? m_TransparentLayerMask : m_GroundLayerMask;

            BoundsInt bounds = tilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    if (tilemap.HasTile(cellPos))
                    {
                        if (cellPos == new Vector3Int(32, 1, 0))
                        {
                            Debug.Log("Debugging here");
                        }

                        Vector2 cellCenter = tilemap.GetCellCenterWorld(cellPos);
                        Vector2 rayOrigin = new Vector2(cellCenter.x, cellCenter.y + m_CellSize.y * 0.5f + RaycastSkin);

                        RaycastHit2D hitInfo = Physics2D.Raycast(rayOrigin, Vector2.down, m_CellSize.y + RaycastSkin * 2, layerMask);

                        bool surfaceFound = false;
                        Vector2 actualNormal = Vector2.up;
                        Vector2 actualHitPoint = Vector2.zero;
                        float bestAngleDeg = 0f;

                        if (hitInfo.collider != null)
                        {
                            bool hitIsGround = (m_GroundLayerMask.value & (1 << hitInfo.collider.gameObject.layer)) != 0;
                            bool hitIsTransparent = (m_TransparentLayerMask.value & (1 << hitInfo.collider.gameObject.layer)) != 0;


                            bool isProcessingTransparentLayer = (baseType & NavPoint.Type.Transparent) != 0;

                            if ((isProcessingTransparentLayer && (hitIsTransparent || hitIsGround)) || (!isProcessingTransparentLayer && hitIsGround))
                            {

                                surfaceFound = true;
                                actualNormal = hitInfo.normal;
                                actualHitPoint = hitInfo.point;
                                bestAngleDeg = Vector2.Angle(actualNormal, Vector2.up);
                            }
                        }

                        if (!surfaceFound) continue;

                        if (bestAngleDeg > m_MaxWalkableSlopeAngle)
                        {
                            continue;
                        }

                        bool isCategorizedAsSlope = bestAngleDeg >= m_MinSlopeAngle;
                        Vector2 navPointPosition = new Vector2(cellCenter.x, actualHitPoint.y + m_ActorSize.y / 2f);

                        Vector2 clearanceRayOrigin = new Vector2(navPointPosition.x, actualHitPoint.y + RaycastSkin);
                        RaycastHit2D clearanceHit = Physics2D.Raycast(clearanceRayOrigin, Vector2.up, m_ActorSize.y - RaycastSkin, CollisionMask);

                        if (clearanceHit.collider != null)
                        {
                            bool hitIsOnTransparentLayerByClearance = (m_TransparentLayerMask.value & (1 << clearanceHit.collider.gameObject.layer)) != 0;
                            if (isCategorizedAsSlope && hitIsOnTransparentLayerByClearance)
                            {
                                // Slope under transparent is allowed
                            }
                            else if (!isCategorizedAsSlope && hitIsOnTransparentLayerByClearance && (baseType & NavPoint.Type.Transparent) != 0)
                            {
                                // If this is a transparent platform, and clearance hits another transparent platform, it's still fine.
                                // (e.g. multi-level transparent platforms) - this case might need more thought
                                // For now, if it's not a slope, any clearance hit is a block, unless it's a slope under transparent.
                            }
                            else
                            {
                                // Blocked by ground, or by transparent if not a slope under transparent.
                                continue;
                            }
                        }

                        NavPoint navPoint = new NavPoint
                        {
                            CellPos = cellPos,
                            TypeMask = baseType,
                            Position = navPointPosition,
                            Normal = actualNormal
                        };

                        if (isCategorizedAsSlope)
                        {
                            navPoint.TypeMask |= NavPoint.Type.Slope;
                        }

                        // Edge detection logic (similar to your previous code)
                        // An edge exists if the adjacent cells (laterally and diagonally up) are empty.
                        // Slopes are not considered edges for jump/fall purposes in this context.
                        if (!isCategorizedAsSlope)
                        {
                            bool isLeftEdge = !HasTileInAnyLayer(cellPos + Vector3Int.left) &&
                                              !HasTileInAnyLayer(cellPos + Vector3Int.left + Vector3Int.up);
                            bool isRightEdge = !HasTileInAnyLayer(cellPos + Vector3Int.right) &&
                                               !HasTileInAnyLayer(cellPos + Vector3Int.right + Vector3Int.up);

                            if (isLeftEdge || isRightEdge)
                            {
                                navPoint.TypeMask |= NavPoint.Type.Edge;
                                if (isLeftEdge)
                                {
                                    navPoint.TypeMask |= NavPoint.Type.LeftEdge;
                                }
                                if (isRightEdge)
                                {
                                    navPoint.TypeMask |= NavPoint.Type.RightEdge;
                                }
                            }
                        }


                        if (!m_NavCells.ContainsKey(cellPos))
                        {
                            m_NavCells[cellPos] = new NavCell();
                        }

                        NavCell cell = m_NavCells[cellPos];
                        if (baseType.HasFlag(NavPoint.Type.Transparent))
                        {
                            cell.Transparent = navPoint;
                        }
                        else
                        {
                            cell.Ground = navPoint;
                        }
                    }
                }
            }
        }

        private void GenerateConnections()
        {
            var keys = new List<Vector3Int>(m_NavCells.Keys);

            foreach (var key in keys)
            {
                NavCell cell = m_NavCells[key];

                if (cell.Ground != null)
                {
                    var ground = cell.Ground;
                    ConnectSurfaceNeighbors(ground);
                    ConnectJumps(ground);
                    ConnectTransparentJumpAbove(ground);

                    if (ground.HasFlag(NavPoint.Type.Edge))
                    {
                        ConnectFalls(ground);
                    }
                    if (ground.HasFlag(NavPoint.Type.Slope))
                    {
                        ConnectSlopes(ground);
                    }
                }
                if (cell.Transparent != null)
                {
                    var transparent = cell.Transparent;
                    ConnectSurfaceNeighbors(transparent);
                    ConnectJumps(transparent);
                    ConnectTransparentJumpAbove(transparent);
                    ConnectTransparentFallBelow(transparent);

                    if (transparent.HasFlag(NavPoint.Type.Edge))
                    {
                        ConnectFalls(transparent);
                    }
                }
                m_NavCells[key] = cell;
            }
        }

        private void ConnectSurfaceNeighbors(NavPoint navPoint)
        {
            var cell = m_NavCells[navPoint.CellPos];
            bool hasGorundTile = cell.Ground != null;
            bool hasTransparentTile = cell.Transparent != null;
            if (navPoint.HasFlag(NavPoint.Type.Slope)) return; // Slopes handle their own connections via ConnectSlopes

            // Connect to adjacent flat surfaces (horizontally)
            Vector3Int[] horizontalOffsets = { Vector3Int.left, Vector3Int.right };

            foreach (var offset in horizontalOffsets)
            {
                if (navPoint.CellPos == new Vector3Int(26, 11))
                {
                    Debug.Log("Debugging here");
                }

                bool hasNeighbor = false;
                Vector3Int neighborPos = navPoint.CellPos + offset;
                if (m_NavCells.TryGetValue(neighborPos, out NavCell neighborCell))
                {
                    NavPoint neighbor;

                    if (neighborCell.Transparent != null)
                    {
                        neighbor = neighborCell.Transparent;
                        if (neighbor.HasFlag(NavPoint.Type.Surface) && !neighbor.HasFlag(NavPoint.Type.Slope))
                        {
                            navPoint.Connections.Add(new Connection
                            {
                                Point = neighbor,
                                Type = ConnectionType.Surface,
                                Weight = m_NavWeights.SurfaceWeight
                            });
                            hasNeighbor = true;
                        }
                    }

                    if (!hasNeighbor)
                    {
                        if (neighborCell.Ground != null)
                        {
                            neighbor = neighborCell.Ground;
                            if (!hasNeighbor && neighbor.HasFlag(NavPoint.Type.Surface) && !neighbor.HasFlag(NavPoint.Type.Slope))
                            {
                                navPoint.Connections.Add(new Connection
                                {
                                    Point = neighbor,
                                    Type = ConnectionType.Surface,
                                    Weight = m_NavWeights.SurfaceWeight
                                });
                            }
                            else if (neighbor.HasFlag(NavPoint.Type.Slope))
                            {
                                navPoint.Connections.Add(new Connection
                                {
                                    Point = neighbor,
                                    Type = ConnectionType.Slope,
                                    Weight = m_NavWeights.SlopeWeight
                                });

                            }
                        }
                    }
                    else if (hasGorundTile && !cell.Ground.TypeMask.HasFlag(NavPoint.Type.Slope))
                    {
                        neighbor = neighborCell.Ground;
                        if (neighbor != null && neighbor.HasFlag(NavPoint.Type.Slope))
                        {
                            navPoint.Connections.Add(new Connection
                            {
                                Point = neighbor,
                                Type = ConnectionType.Slope,
                                Weight = m_NavWeights.SlopeWeight
                            });

                        }
                    }
                }
            }

            Vector3Int[] diagonalSlopeOffsets =
            {
                    Vector3Int.left + Vector3Int.up, Vector3Int.right + Vector3Int.up
                };


            foreach (var offset in diagonalSlopeOffsets)
            {
                //TODO: the edge check is wrong, fix it.
                Vector3Int neighborPos = navPoint.CellPos + offset;
                if (m_NavCells.TryGetValue(neighborPos, out NavCell neighborCell1) &&
                    neighborCell1.Ground != null && neighborCell1.Ground.HasFlag(NavPoint.Type.Slope) &&
                    !neighborCell1.Ground.HasFlag(NavPoint.Type.Edge))
                {

                    //Check the normal of the slope. It should be in the direction of of the navpoint.
                    Vector2 slopeNormalDirection = neighborCell1.Ground.Normal;

                    if (slopeNormalDirection.x < -VERY_SMALL_FLOAT)
                    {
                        slopeNormalDirection = Vector2.left;
                    }
                    else if (slopeNormalDirection.x > VERY_SMALL_FLOAT)
                    {
                        slopeNormalDirection = Vector2.right;
                    }
                    else
                    {
                        slopeNormalDirection = Vector2.up;
                    }

                    if ((offset == Vector3Int.left + Vector3Int.up && slopeNormalDirection == Vector2.right) ||
                        (offset == Vector3Int.right + Vector3Int.up && slopeNormalDirection == Vector2.left))
                    {
                        navPoint.Connections.Add(new Connection
                        {
                            Point = neighborCell1.Ground,
                            Type = ConnectionType.Surface,
                            Weight = m_NavWeights.SurfaceWeight
                        });
                    }
                }
            }
        }

        private void ConnectSlopes(NavPoint navPoint)
        {
            int yOffsetForLeftNeighbor;
            int yOffsetForRightNeighbor;

            if (navPoint.Normal.x < -VERY_SMALL_FLOAT)
            {
                yOffsetForLeftNeighbor = -1;
                yOffsetForRightNeighbor = 1;
            }
            else if (navPoint.Normal.x > VERY_SMALL_FLOAT)
            {
                yOffsetForLeftNeighbor = 1;
                yOffsetForRightNeighbor = -1;

            }
            else
            {
                yOffsetForLeftNeighbor = 0;
                yOffsetForRightNeighbor = 0;
            }

            Vector3Int[] baseDirections = { Vector3Int.left, Vector3Int.right };
            int[] yOffsets = { yOffsetForLeftNeighbor, yOffsetForRightNeighbor };

            for (int i = 0; i < baseDirections.Length; i++)
            {
                Vector3Int dir = baseDirections[i];
                int yOffset = yOffsets[i];

                Vector3Int neighborCellPosForSlope = navPoint.CellPos + dir + new Vector3Int(0, yOffset, 0);
                Vector3Int neighborCellPosForSurface = navPoint.CellPos + dir;


                if (m_NavCells.TryGetValue(neighborCellPosForSlope, out NavCell neighborCell) && neighborCell.Ground != null)
                {
                    var ground = neighborCell.Ground;
                    var transparent = neighborCell.Transparent;

                    if (ground != null && !navPoint.Connections.Exists(c => c.Point == ground && c.Type == ConnectionType.Slope))
                    {
                        navPoint.Connections.Add(new Connection { Point = ground, Type = ConnectionType.Slope, Weight = m_NavWeights.SlopeWeight });
                    }

                    else if (transparent != null && !navPoint.Connections.Exists(c => c.Point == transparent && c.Type == ConnectionType.Slope))
                    {
                        navPoint.Connections.Add(new Connection { Point = transparent, Type = ConnectionType.Slope, Weight = m_NavWeights.SlopeWeight });
                    }
                }

                else if (m_NavCells.TryGetValue(neighborCellPosForSurface, out NavCell neighborCell2) && neighborCell2.Ground != null)
                {
                    var ground = neighborCell2.Ground;
                    var transparent = neighborCell2.Transparent;

                    if (ground != null && !navPoint.Connections.Exists(c => c.Point == ground))
                    {
                        navPoint.Connections.Add(new Connection { Point = ground, Type = ConnectionType.Slope, Weight = m_NavWeights.SurfaceWeight });
                    }
                    else if (transparent != null && !navPoint.Connections.Exists(c => c.Point == transparent))
                    {
                        navPoint.Connections.Add(new Connection { Point = transparent, Type = ConnectionType.Slope, Weight = m_NavWeights.SurfaceWeight });
                    }
                }
            }
        }

        private void ConnectJumps(NavPoint navPoint)
        {
            if (navPoint.TypeMask.HasFlag(NavPoint.Type.Slope)) return;

            float fromX = navPoint.CellPos.x;
            float fromY = navPoint.CellPos.y;

            foreach (var kvp in m_NavCells)
            {
                NavPoint target;
                if (kvp.Value.Transparent != null)
                {
                    target = kvp.Value.Transparent;
                }
                else
                {
                    target = kvp.Value.Ground;
                }

                if (!target.HasFlag(NavPoint.Type.Edge)) continue;

                float verticalDistance = Mathf.Abs(navPoint.Position.y - target.Position.y);
                float horizontalDistance = Mathf.Abs(navPoint.Position.x - target.Position.x);

                float toX = target.CellPos.x;
                float toY = target.CellPos.y;

                if (verticalDistance > m_JumpHeight || horizontalDistance > m_FallJumpDistance
                    || horizontalDistance <= VERY_SMALL_FLOAT)
                {
                    continue;
                }

                bool isLeftEdge = !HasTileInAnyLayer(target.CellPos + Vector3Int.left);
                bool isRightEdge = !HasTileInAnyLayer(target.CellPos + Vector3Int.right);

                if (!(isLeftEdge && isRightEdge))
                {
                    if (isRightEdge && navPoint.Position.x < target.Position.x) continue;
                    if (isLeftEdge && navPoint.Position.x > target.Position.x) continue; //
                }

                if (IsOnSamePlatform(navPoint, target)) continue;

                Vector2 direction = (target.CellPos - navPoint.CellPos).ToVector2().normalized;


                var closerPoint = navPoint.Connections.Find(connection =>
                {
                    var offset = direction.x > 0 ? -m_TileSize : m_TileSize;
                    return connection.Type == ConnectionType.Surface && !connection.Point.HasFlag(NavPoint.Type.Slope)
                    && Mathf.Abs(connection.Point.Position.x - target.Position.x) <= horizontalDistance;
                });
                if (closerPoint != null)
                {
                    if (closerPoint.Point.CellPos.x != target.CellPos.x)
                        continue;
                }


                if (direction.y >= 0)
                {
                    if (verticalDistance > m_JumpHeight)
                    {
                        continue;
                    }

                    if (horizontalDistance <= 1.0f + float.Epsilon)
                    {
                        navPoint.Connections.Add(new Connection
                        {
                            Point = target,
                            Type = ConnectionType.Jump,
                            Weight = m_NavWeights.JumpWeight
                        });
                    }
                    else
                    {
                        if (IsJumpPathClear(navPoint.Position, target.Position))
                        {
                            navPoint.Connections.Add(new Connection
                            {
                                Point = target,
                                Type = ConnectionType.Jump,
                                Weight = m_NavWeights.JumpWeight
                            });
                        }
                    }

                }
            }
        }

        private void ConnectFalls(NavPoint navPoint)
        {
            if (navPoint.TypeMask.HasFlag(NavPoint.Type.Slope)) return;

            foreach (var kvp in m_NavCells)
            {
                NavPoint target;
                if (kvp.Value.Transparent != null)
                {
                    target = kvp.Value.Transparent;
                }
                else
                {
                    target = kvp.Value.Ground;
                }

                if (target.HasFlag(NavPoint.Type.Slope)) continue;

                float verticalDistance = Mathf.Abs(navPoint.Position.y - target.Position.y);
                float horizontalDistance = Mathf.Abs(navPoint.Position.x - target.Position.x);

                if (target.Position.y >= navPoint.Position.y
                    || Mathf.Abs(target.Position.x - navPoint.Position.x) <= float.Epsilon
                    || verticalDistance > m_FallHeight
                    || horizontalDistance > m_FallJumpDistance)
                {
                    continue;
                }

                bool isLeftEdge = navPoint.HasFlag(NavPoint.Type.LeftEdge);
                bool isRightEdge = navPoint.HasFlag(NavPoint.Type.RightEdge);

                if (isLeftEdge && target.Position.x < navPoint.Position.x)
                {
                    if (IsFallPathClear(navPoint.Position + Vector2.left, target.Position))
                    {
                        navPoint.Connections.Add(new Connection
                        {
                            Point = target,
                            Type = ConnectionType.Fall,
                            Weight = m_NavWeights.FallWeight
                        });
                    }
                }
                else if (isRightEdge && target.Position.x > navPoint.Position.x)
                {
                    if (IsFallPathClear(navPoint.Position + Vector2.right, target.Position))
                    {
                        navPoint.Connections.Add(new Connection
                        {
                            Point = target,
                            Type = ConnectionType.Fall,
                            Weight = m_NavWeights.FallWeight
                        });
                    }
                }

            }
        }

        private void ConnectTransparentFallBelow(NavPoint navPoint)
        {
            return;
            if (!navPoint.TypeMask.HasFlag(NavPoint.Type.Transparent)) return;

            Vector2 cellPosition = new Vector3(navPoint.CellPos.x + m_TileAnchor.x, navPoint.CellPos.y + m_TileAnchor.y, navPoint.CellPos.z);
            Vector2 belowRaycastOrigin = new Vector2(cellPosition.x, cellPosition.y - m_NavPointVerticalOffset);

            RaycastHit2D hitBelow = Physics2D.Raycast(belowRaycastOrigin, Vector2.down, Mathf.Infinity, m_GroundLayerMask | m_TransparentLayerMask);
            if (hitBelow.collider != null)
            {
                Vector3Int belowPos = m_GroundTilemap.WorldToCell(new Vector3(hitBelow.point.x, hitBelow.point.y - VERY_SMALL_FLOAT)); //before hitBelow.point.y - m_NavPointVerticalOffset
                if (m_NavCells.TryGetValue(belowPos, out var belowCell) && belowCell.Ground != null)
                {
                    var below = belowCell.Ground;
                    Debug.Log($"Connecting {navPoint.Position} to {below.Position} below");
                    navPoint.Connections.Add(new Connection
                    {
                        Point = below,
                        Type = ConnectionType.TransparentFall,
                        Weight = m_NavWeights.TransparentFallWeight
                    });
                }
            }
        }
        private void ConnectTransparentJumpAbove(NavPoint fromNavPoint)
        {
            Vector2 aboveRaycastOrigin = fromNavPoint.Position;

            RaycastHit2D hitAbove = Physics2D.Raycast(aboveRaycastOrigin, Vector2.up, m_JumpHeight, m_TransparentLayerMask);
            if (hitAbove.collider != null)
            {
                Vector3Int abovePos = m_GroundTilemap.WorldToCell(hitAbove.point + Vector2.up * RaycastSkin);
                if (m_NavCells.TryGetValue(abovePos, out var aboveCell) && aboveCell.Transparent != null)
                {
                    var above = aboveCell.Transparent;
                    if (above.CellPos == fromNavPoint.CellPos) return;

                    fromNavPoint.Connections.Add(new Connection
                    {
                        Point = above,
                        Type = ConnectionType.TransparentJump,
                        Weight = m_NavWeights.TransparentJumpWeight
                    });
                }
            }
        }

        private bool IsOnSamePlatform(NavPoint a, NavPoint b)
        {
            Vector2 aPos = new Vector2(a.CellPos.x + m_TileAnchor.x, a.CellPos.y + m_TileAnchor.y);
            Vector2 bPos = new Vector2(b.CellPos.x + m_TileAnchor.x, b.CellPos.y + m_TileAnchor.y);

            Vector2 direction = (aPos - bPos).normalized;
            float distance = Vector2.Distance(aPos, bPos);

            if (distance <= float.Epsilon) return true;

            int expectedHits = Mathf.CeilToInt(distance / m_CellSize.x);
            var hits = Physics2D.RaycastAll(aPos, direction, distance, m_GroundLayerMask | m_TransparentLayerMask);

            if (hits.Length == expectedHits)
            {
                return true;
            }
            return false;
        }

        private bool IsNeighbor(NavPoint a, NavPoint b)
        {
            Vector3Int diff = a.CellPos - b.CellPos;
            return Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1;
        }
        public NavCell GetCell(Vector3 cellPos, bool useOffset)
        {
            GetCell(cellPos, out var cell, useOffset);
            return cell;
        }
        public bool GetCell(Vector3 worldPosition, out NavCell cell, bool useOffset)
        {
            Vector3 offset = useOffset ? new Vector3(0, m_ActorSize.y / 2f + m_NavPointVerticalOffset, 0) : Vector3.zero;
            Vector3Int cellPos = m_GroundTilemap.WorldToCell(worldPosition - offset);
            return m_NavCells.TryGetValue(cellPos, out cell);
        }

        public bool HasTileInAnyLayer(Vector3Int cellPos)
        {
            if (m_NavCells.TryGetValue(cellPos, out var cell) && (cell.Transparent != null || cell.Ground != null))
            {
                return true;
            }

            return m_GroundTilemap.HasTile(cellPos) || m_TransparentGround.HasTile(cellPos);
        }

        public bool HasGroundTile(Vector3Int cellPos)
        {
            return m_NavCells.TryGetValue(cellPos, out var cell) && cell.Ground != null;
        }
        public bool HasTransparentGroundTile(Vector3Int cellPos)
        {
            return m_NavCells.TryGetValue(cellPos, out var cell) && cell.Transparent != null;
        }

        public bool HasGroundTile(Vector3 cellPos)
        {
            Vector3Int intCellPos = new Vector3Int((int)cellPos.x, (int)cellPos.y);
            return HasGroundTile(intCellPos);
        }
        public bool HasTransparentGroundTile(Vector3 cellPos)
        {
            Vector3Int intCellPos = new Vector3Int((int)cellPos.x, (int)cellPos.y);
            return HasTransparentGroundTile(intCellPos);
        }

        private Tilemap GetTilemapForCell(Vector3Int cellPos, out TileBase tile)
        {
            tile = null;
            if (m_GroundTilemap != null && m_GroundTilemap.HasTile(cellPos))
            {
                tile = m_GroundTilemap.GetTile(cellPos);
                return m_GroundTilemap;
            }
            if (m_TransparentGround != null && m_TransparentGround.HasTile(cellPos))
            {
                tile = m_TransparentGround.GetTile(cellPos);
                return m_TransparentGround;
            }
            return null;
        }

        private bool IsSlopeAtCell(Vector3Int cellPos)
        {
            return false;
        }

        private void SerializeNavPoints()
        {
            return;
            //m_SerializedNavPoints.Clear();
            //foreach (var kvp in m_NavCells)
            //{
            //    m_SerializedNavPoints.Add(new NavPointEntry { Key = kvp.Key, Value = kvp.Value });
            //}
        }

        private void DeserializeNavPoints()
        {
            return;
            //if (m_SerializedNavPoints.Count > 0)
            //{
            //    m_NavPointLookup.Clear();
            //    foreach (var entry in m_SerializedNavPoints)
            //    {
            //        m_NavPointLookup[entry.Key] = entry.Value;
            //    }
            //}
        }

        private bool IsJumpPathClear(Vector2 from, Vector2 to)
        {
            var point = GetCell(to, true).Ground;
            if (point != null)
            {
                if (!point.HasFlag(NavPoint.Type.LeftEdge) && from.x < to.x)
                    return false;
                if (!point.HasFlag(NavPoint.Type.RightEdge) && from.x > to.x)
                    return false;
            }

            Vector2 pathDirection = (to - from).normalized;
            float distance = Vector2.Distance(from, to);

            var hits = Physics2D.RaycastAll(from, pathDirection, distance, m_GroundLayerMask | m_TransparentLayerMask);

            foreach (var hit in hits)
            {
                if (Mathf.Abs(hit.point.x - to.x) <= m_ActorSize.x / 2 + m_TileAnchor.x &&
                    Mathf.Abs(hit.point.y - to.y) <= m_ActorSize.y / 2 + m_TileAnchor.y)
                {
                    continue;
                }

                if (Mathf.Abs(to.y - from.y) > Mathf.Abs(to.x - from.x))
                {
                    if (Mathf.Abs(hit.point.x - to.x) > m_ActorSize.x / 2 + m_TileAnchor.x + VERY_SMALL_FLOAT)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Mathf.Abs(hit.point.y - to.y) > m_ActorSize.y / 2 + m_TileAnchor.y + VERY_SMALL_FLOAT)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsFallPathClear(Vector2 from, Vector2 to)
        {
            Vector2 pathDirection = (to - from).normalized;
            float distance = Vector2.Distance(from, to);

            var hits = Physics2D.RaycastAll(from, pathDirection, distance, m_GroundLayerMask | m_TransparentLayerMask);
            if (hits.Length > 0)
            {
                return false;
            }

            return true;
        }

        private void OnDrawGizmos()
        {
            if (m_NavCells == null) return;
            if (!m_DrawGizmos) return;

            foreach (var navCell in m_NavCells.Values)
            {
                if (navCell.Ground != null)
                {
                    DrawNavPoint(navCell.Ground);
                }
                if (navCell.Transparent != null)
                {
                    DrawNavPoint(navCell.Transparent);
                }
            }
        }


        public void DrawNavPoint(NavPoint navPoint)
        {
            bool isEdge = navPoint.HasFlag(NavPoint.Type.Edge);
            bool isTransparent = navPoint.HasFlag(NavPoint.Type.Transparent);
            bool isSlope = navPoint.HasFlag(NavPoint.Type.Slope);

            if (isEdge && isTransparent) Gizmos.color = Color.cyan;
            else if (isEdge) Gizmos.color = Color.blue;
            else if (isTransparent) Gizmos.color = Color.yellow;
            else if (isSlope) Gizmos.color = new Color(.5f, 0.85f, 0.2f);
            else Gizmos.color = Color.green;

            Gizmos.DrawSphere(navPoint.Position, 0.4f);

            foreach (var connection in navPoint.Connections)
            {

                Gizmos.color = connection.Type switch
                {
                    ConnectionType.Surface => Color.green,
                    ConnectionType.Jump => Color.blue,
                    ConnectionType.TransparentJump => Color.magenta,
                    ConnectionType.TransparentFall => Color.cyan,
                    ConnectionType.Fall => Color.red,
                    ConnectionType.Slope => Color.yellow,
                    _ => Color.white
                };
                Gizmos.DrawLine(navPoint.Position, connection.Point.Position);
            }
        }


        [System.Serializable]
        public class NavPoint
        {
            public Vector3Int CellPos;
            public Vector2 Position;
            public Vector2 Normal; // <–– new!
            public Type TypeMask;
            [SerializeReference] public List<Connection> Connections = new();

            [System.Flags]
            public enum Type : uint
            {
                None = 0,
                Surface = 1 << 0,
                Transparent = 1 << 1,
                Slope = 1 << 2,
                Edge = 1 << 3,
                LeftEdge = 1 << 4,
                RightEdge = 1 << 5,
            }

            public bool HasFlag(Type flag)
            {
                return (TypeMask & flag) == flag;
            }
        }
        [System.Serializable]
        public class Connection
        {
            public NavPoint Point;
            public ConnectionType Type;
            public float Weight;
        }

        public enum ConnectionType
        {
            None,
            Surface,
            Jump,
            Fall,
            TransparentJump,
            TransparentFall,
            Slope,
        }


        [System.Serializable]
        public class NavPointEntry
        {
            public Vector3Int Key;
            public NavPoint Value;
        }


        public enum TileType
        {
            None,
            Ground,
            Transparent
        }
    }

}