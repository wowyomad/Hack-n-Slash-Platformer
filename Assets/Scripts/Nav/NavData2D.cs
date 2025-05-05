using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;

namespace Nav2D
{

    public class NavData2D : MonoBehaviour
    {
        private Dictionary<Vector3Int, NavPoint> m_NavPointLookup = new Dictionary<Vector3Int, NavPoint>();
        private Dictionary<Vector3Int, TileType> m_TileTypeLookup = new Dictionary<Vector3Int, TileType>();

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
        [SerializeField] private float m_JumpHeight = 4.0f;
        [SerializeField] private float m_FallHeight = 8.0f;
        [SerializeField] private float m_FallJumpDistance = 5.0f;

        [Header("Raycast")]
        [SerializeField] private float m_ConeAngle = 150.0f;
        [SerializeField] private int m_NumberOfRays = 11;
        [SerializeField] private float m_CastDistance = 32.0f;

        [Header("Debug")]
        [SerializeField] private bool m_DrawGizmos = true;

        public static readonly float VERY_SMALL_FLOAT = 0.005f;
        private const float GROUND_NORMAL_THRESHOLD = 0.5f; //cos 60

        private Vector2 m_TileAnchor;
        private float m_TileSize => m_TileAnchor.x * 2.0f;
        private float m_NavPointVerticalOffset;
        public float VerticalTileOffset => m_NavPointVerticalOffset;
        private Vector2 m_CellSize;

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

            m_CellSize = m_GroundTilemap.cellSize;

            DeserializeNavPoints();
            Generate();
        }

        private void OnDisable()
        {
            SerializeNavPoints();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

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



        public NavPoint GetClosestNavPoint(Vector2 target)
        {
            if (HasGroundTile(target))
            {
                Debug.Log($"HasGroundTile at {target}");
                return null;
            }

            Vector2 closestPoint = target;
            float closestDistanceToTarget = float.MaxValue;
            bool anyHit = false;

            if (m_NumberOfRays <= 1)
            {
                Vector2 rayDirection = Vector2.down;
                RaycastHit2D hit = Physics2D.Raycast(target, rayDirection, m_CastDistance, m_GroundLayerMask);

                if (hit.collider != null)
                {
                    if (Vector2.Dot(hit.normal, Vector2.up) > GROUND_NORMAL_THRESHOLD)
                    {
                        closestPoint = hit.point;
                        anyHit = true;
                    }
                }
            }
            else
            {
                float halfAngleRad = m_ConeAngle * Mathf.Deg2Rad / 2f;
                float centerAngleRad = Mathf.Atan2(-1, 0);

                for (int i = 0; i < m_NumberOfRays; i++)
                {
                    float angleOffsetRad = Mathf.Lerp(-halfAngleRad, halfAngleRad, (float)i / (m_NumberOfRays - 1));
                    float rayAngleRad = centerAngleRad + angleOffsetRad;
                    Vector2 rayDirection = new Vector2(Mathf.Cos(rayAngleRad), Mathf.Sin(rayAngleRad));

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
                Vector3 tilePosition = new Vector3(closestPoint.x, closestPoint.y - 0.01f, 0f);
                var navPoint = GetNavPoint(tilePosition, false);

                if (navPoint != null)
                {
                    return navPoint;
                }
                else
                {
                    //Make a list of 8 tile position around tilePosition with offset of 1.0 for each direction.
                    List<Vector3> offsets = new List<Vector3>
                    {
                        new Vector3(-0.5f, 0.0f, 0.0f),
                        new Vector3(0.5f, 0.0f, 0.0f),
                        new Vector3(0.0f, -0.5f, 0.0f),
                        new Vector3(0.0f, 0.5f, 0.0f),
                        new Vector3(-0.5f, -0.5f, 0.0f),
                        new Vector3(0.5f, -0.5f, 0.0f),
                        new Vector3(-0.5f, 0.5f, 0.0f),
                        new Vector3(0.5f, 0.5f, 0.0f)
                    };
                    foreach (var offset in offsets)
                    {
                        tilePosition = new Vector3(closestPoint.x + offset.x, closestPoint.y + offset.y, 0f);
                        navPoint = GetNavPoint(tilePosition, false);
                        if (navPoint != null)
                        {
                            return navPoint;
                        }
                    }

                    //tilePosition = new Vector3(closestPoint.x - 0.5f, closestPoint.y - 0.01f, 0f);
                    //navPoint = GetNavPoint(tilePosition, false);
                    //if (navPoint != null)
                    //{
                    //    return navPoint;
                    //}
                    //else
                    //{
                    //    tilePosition = new Vector3(closestPoint.x + 0.5f, closestPoint.y - 0.01f, 0f);
                    //    navPoint = GetNavPoint(tilePosition, false);
                    //    if (navPoint != null)
                    //    {
                    //        return navPoint;
                    //    }
                    //}
                }
            }

            return null;
        }


        public void Generate()
        {
            Debug.Log("Generating NavPoints...");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            m_NavPointLookup.Clear();

            GenerateBaseNavPoints(m_TransparentGround, NavPoint.Type.Surface | NavPoint.Type.Transparent);
            GenerateBaseNavPoints(m_GroundTilemap, NavPoint.Type.Surface);

            GenerateConnections();

            stopwatch.Stop();
            string durationStr = stopwatch.Elapsed.TotalSeconds.ToString("N3");
            Debug.Log($"Generated {m_NavPointLookup.Count} points in {durationStr}s.");
        }

        private void GenerateBaseNavPoints(Tilemap tilemap, NavPoint.Type baseType)
        {
            if (tilemap == null) return;

            BoundsInt bounds = tilemap.cellBounds;
            Vector3 cellSize = tilemap.layoutGrid ? tilemap.layoutGrid.cellSize : tilemap.cellSize;
            Vector3 tileAnchor = tilemap.tileAnchor;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    if (tilemap.HasTile(cellPos))
                    {
                        if (!m_TileTypeLookup.ContainsKey(cellPos))
                        {
                            m_TileTypeLookup[cellPos] = (baseType & NavPoint.Type.Transparent) != 0 ? TileType.Transparent : TileType.Ground;
                        }

                        Vector3Int cellAbove = cellPos + Vector3Int.up;

                        if (HasTileInAnyLayer(cellAbove))
                        {
                            continue;
                        }

                        Vector3 tileWorldPos = tilemap.CellToWorld(cellPos);
                        Vector2 tileSurfaceCenter = new Vector2(tileWorldPos.x + tileAnchor.x, tileWorldPos.y + tileAnchor.y);
                        Vector2 tileSurface = new Vector2(tileSurfaceCenter.x, tileSurfaceCenter.y + m_NavPointVerticalOffset);

                        RaycastHit2D hit = Physics2D.Raycast(tileSurface, Vector2.up, m_ActorSize.y, m_GroundLayerMask);


                        if (hit.collider != null)
                        {
                            var tilePosition = tilemap.WorldToCell(new Vector3(hit.point.x, hit.point.y + VERY_SMALL_FLOAT));

                            if (m_NavPointLookup.TryGetValue(tilePosition, out NavPoint point) && point.HasFlag(NavPoint.Type.Transparent))
                            {
                                //ok
                            }
                            else
                            {
                                //won't fit, skip 
                                continue;
                            }
                        }

                        NavPoint navPoint = new NavPoint
                        {
                            CellPos = cellPos,
                            TypeMask = baseType,
                            Position = new Vector2(tileSurface.x, tileSurface.y + m_ActorSize.y / 2),
                        };

                        bool isLeftEdge = !HasTileInAnyLayer(cellPos + Vector3Int.left) && !HasTileInAnyLayer(cellPos + Vector3Int.left + Vector3Int.up);
                        bool isRightEdge = !HasTileInAnyLayer(cellPos + Vector3Int.right) && !HasTileInAnyLayer(cellPos + Vector3Int.right + Vector3Int.up);

                        bool isEdge = isLeftEdge || isRightEdge;
                        bool isSlope = IsSlopeAtCell(cellPos);

                        if (isEdge && !isSlope)
                        {
                            navPoint.TypeMask |= NavPoint.Type.Edge;
                            navPoint.TypeMask |= isLeftEdge ? NavPoint.Type.LeftEdge : 0;
                            navPoint.TypeMask |= isRightEdge ? NavPoint.Type.RightEdge : 0;
                        }

                        if (isSlope)
                        {
                            navPoint.TypeMask |= NavPoint.Type.Slope;
                        }

                        m_NavPointLookup[cellPos] = navPoint;
                    }
                }
            }
        }

        private void GenerateConnections()
        {
            var keys = new List<Vector3Int>(m_NavPointLookup.Keys);

            foreach (var key in keys)
            {
                NavPoint navPoint = m_NavPointLookup[key];

                if (navPoint.HasFlag(NavPoint.Type.Surface))
                {
                    ConnectSurfaceNeighbors(navPoint);
                    ConnectJumps(navPoint);
                    ConnectTransparentJumpAbove(navPoint);
                }

                if (navPoint.HasFlag(NavPoint.Type.Edge))
                {
                    ConnectFalls(navPoint);
                }

                if (navPoint.HasFlag(NavPoint.Type.Transparent))
                {
                    ConnectTransparentFallBelow(navPoint);
                }

                if (navPoint.HasFlag(NavPoint.Type.Slope))
                {
                    ConnectSlopes(navPoint);
                }

                m_NavPointLookup[key] = navPoint;
            }
        }

        private void ConnectSurfaceNeighbors(NavPoint navPoint)
        {
            if (navPoint.HasFlag(NavPoint.Type.Slope)) return;

            Vector3Int[] neighborOffsets = { Vector3Int.left, Vector3Int.right };
            foreach (var offset in neighborOffsets)
            {
                Vector3Int neighborPos = navPoint.CellPos + offset;
                if (m_NavPointLookup.TryGetValue(neighborPos, out NavPoint neighbor) && neighbor.HasFlag(NavPoint.Type.Surface))
                {
                    navPoint.Connections.Add(new Connection
                    {
                        Point = neighbor,
                        Type = ConnectionType.Surface,
                        Weight = m_NavWeights.SurfaceWeight
                    });
                }
            }

            Vector3Int[] neighborOffsetsVertical = { Vector3Int.left + Vector3Int.up, Vector3Int.right + Vector3Int.up, Vector3Int.left + Vector3Int.down, Vector3Int.right + Vector3Int.down };
            foreach (var offset in neighborOffsetsVertical)
            {
                Vector3Int neighborPos = navPoint.CellPos + offset;
                if (m_NavPointLookup.TryGetValue(neighborPos, out NavPoint neighbor) && neighbor.HasFlag(NavPoint.Type.Slope))
                {
                    navPoint.Connections.Add(new Connection
                    {
                        Point = neighbor,
                        Type = ConnectionType.Surface,
                        Weight = m_NavWeights.SurfaceWeight
                    });
                }
            }
        }

        private void ConnectSlopes(NavPoint navPoint)
        {
            Vector3Int[] neighborOffsets = { Vector3Int.left + Vector3Int.up, Vector3Int.right + Vector3Int.up, Vector3Int.left + Vector3Int.down, Vector3Int.right + Vector3Int.down };
            foreach (var offset in neighborOffsets)
            {
                Vector3Int neighborPos = navPoint.CellPos + offset;
                if (m_NavPointLookup.TryGetValue(neighborPos, out NavPoint neighbor))
                {
                    if (neighbor.HasFlag(NavPoint.Type.Slope) || neighbor.HasFlag(NavPoint.Type.Surface))
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

            Vector3Int[] endNeighborOffsets = { Vector3Int.left, Vector3Int.right };
            foreach (var offset in endNeighborOffsets)
            {
                Vector3Int neighborPos = navPoint.CellPos + offset;
                if (m_NavPointLookup.TryGetValue(neighborPos, out NavPoint neighbor) && neighbor.HasFlag(NavPoint.Type.Surface))
                {
                    navPoint.Connections.Add(new Connection
                    {
                        Point = neighbor,
                        Type = ConnectionType.Surface,
                        Weight = m_NavWeights.SurfaceWeight
                    });
                }
            }
        }

        private void ConnectJumps(NavPoint navPoint)
        {
            if (navPoint.TypeMask.HasFlag(NavPoint.Type.Slope)) return;

            float fromX = navPoint.CellPos.x;
            float fromY = navPoint.CellPos.y;

            foreach (var kvp in m_NavPointLookup)
            {
                NavPoint target = kvp.Value;

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


                //If there's surface neighbor that is closer to the target, skip 
                var closerPoint = navPoint.Connections.Find(connection =>
                {
                    var offset = direction.x > 0 ? -m_TileSize : m_TileSize;
                    return connection.Type == ConnectionType.Surface && !connection.Point.HasFlag(NavPoint.Type.Slope)
                    && Mathf.Abs(connection.Point.Position.x - target.Position.x) <= horizontalDistance;
                });
                if (closerPoint != null)
                {
                    //Костылевский
                    if (closerPoint.Point.CellPos.x != target.CellPos.x)
                        continue;
                }


                if (direction.y >= 0)
                {
                    if (verticalDistance > m_JumpHeight)
                    {
                        continue;
                    }

                    //Tak Nado.
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

            foreach (var kvp in m_NavPointLookup)
            {
                NavPoint target = kvp.Value;

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
            Vector2 cellPosition = new Vector3(navPoint.CellPos.x + m_TileAnchor.x, navPoint.CellPos.y + m_TileAnchor.y, navPoint.CellPos.z);
            Vector2 belowRaycastOrigin = new Vector2(cellPosition.x, cellPosition.y - m_NavPointVerticalOffset);

            // Connect below
            RaycastHit2D hitBelow = Physics2D.Raycast(belowRaycastOrigin, Vector2.down, Mathf.Infinity, m_GroundLayerMask | m_TransparentLayerMask);
            if (hitBelow.collider != null)
            {
                Vector3Int belowPos = m_GroundTilemap.WorldToCell(new Vector3(hitBelow.point.x, hitBelow.point.y - VERY_SMALL_FLOAT)); //before hitBelow.point.y - m_NavPointVerticalOffset
                if (m_NavPointLookup.TryGetValue(belowPos, out NavPoint below) && below.HasFlag(NavPoint.Type.Surface))
                {
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
            Vector2 cellPosition = new Vector3(fromNavPoint.CellPos.x + m_TileAnchor.x, fromNavPoint.CellPos.y + m_TileAnchor.y, fromNavPoint.CellPos.z);
            Vector2 aboveRaycastOrigin = new Vector2(cellPosition.x, cellPosition.y + m_NavPointVerticalOffset + 0.1f);

            RaycastHit2D hitAbove = Physics2D.Raycast(aboveRaycastOrigin, Vector2.up, m_JumpHeight, m_TransparentLayerMask);
            if (hitAbove.collider != null)
            {
                Vector3Int abovePos = m_GroundTilemap.WorldToCell(new Vector3(hitAbove.point.x, hitAbove.point.y + m_NavPointVerticalOffset));
                if (m_NavPointLookup.TryGetValue(abovePos, out NavPoint above) && above.HasFlag(NavPoint.Type.Transparent))
                {
                    Debug.Log($"Connecting {fromNavPoint.Position} to {above.Position} above (transparent jump)");
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

        public bool HasTileInAnyLayer(Vector3Int cellPos)
        {
            return m_TileTypeLookup.ContainsKey(cellPos) && m_TileTypeLookup[cellPos] != TileType.None;

        }

        public bool HasGroundTile(Vector3Int cellPos)
        {
            return m_TileTypeLookup.TryGetValue(cellPos, out var type) && type == TileType.Ground;
        }
        public bool HasTransparentGroundTile(Vector3Int cellPos)
        {
            return m_TileTypeLookup.TryGetValue(cellPos, out var type) && type == TileType.Transparent;
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
            TileBase tile;
            Tilemap tilemap = GetTilemapForCell(cellPos, out tile);

            if (tilemap == null || tile == null)
            {
                return false;
            }

            Vector3 worldTilePositoin = tilemap.CellToWorld(cellPos) + tilemap.cellSize / 2;
            Vector2 raycastOrigin = new Vector2(worldTilePositoin.x, worldTilePositoin.y + m_NavPointVerticalOffset);
            float rayLength = m_CellSize.y + VERY_SMALL_FLOAT;
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, rayLength, m_GroundLayerMask);

            if (hit.collider != null)
            {
                if ((hit.normal - Vector2.up).sqrMagnitude >= VERY_SMALL_FLOAT)
                {
                    return true;
                }
            }
            return false;
        }

        public NavPoint GetNavPoint(Vector3 worldPosition, bool useOffset = true)
        {
            Vector3 offset = new Vector3(0, m_ActorSize.y / 2 + m_NavPointVerticalOffset, 0);
            Vector3Int cellPos = m_GroundTilemap.WorldToCell(worldPosition - (useOffset ? offset : Vector2.zero));
            if (m_NavPointLookup.TryGetValue(cellPos, out NavPoint navPoint))
            {
                return navPoint;
            }
            return null;
        }

        private void SerializeNavPoints()
        {
            m_SerializedNavPoints.Clear();
            foreach (var kvp in m_NavPointLookup)
            {
                m_SerializedNavPoints.Add(new NavPointEntry { Key = kvp.Key, Value = kvp.Value });
            }
        }

        private void DeserializeNavPoints()
        {
            if (m_SerializedNavPoints.Count > 0)
            {
                m_NavPointLookup.Clear();
                foreach (var entry in m_SerializedNavPoints)
                {
                    m_NavPointLookup[entry.Key] = entry.Value;
                }
            }
        }

        private bool IsJumpPathClear(Vector2 from, Vector2 to)
        {
            var point = GetNavPoint(to, true);

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
            if (m_NavPointLookup == null) return;
            if (!m_DrawGizmos) return;

            foreach (var navPoint in m_NavPointLookup.Values)
            {
                bool isEdge = navPoint.HasFlag(NavPoint.Type.Edge);
                bool isTransparent = navPoint.HasFlag(NavPoint.Type.Transparent);
                bool isSlope = navPoint.HasFlag(NavPoint.Type.Slope);

                if (isEdge && isTransparent)
                {
                    Gizmos.color = Color.cyan;
                }
                else if (isEdge)
                {
                    Gizmos.color = Color.blue;
                }
                else if (isTransparent)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.green;
                }

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
        }
    }


    [System.Serializable]
    public class NavPoint
    {
        public Vector3Int CellPos;
        public Vector2 Position;
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