using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

namespace Nav2D
{
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
        public NavPoint Target;
        public ConnectionType Type;
        public float Weight;
    }

    public enum ConnectionType
    {
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


    public class NavData2D : MonoBehaviour
    {
        private Dictionary<Vector3Int, NavPoint> m_NavPointLookup = new Dictionary<Vector3Int, NavPoint>();
        [SerializeField] private List<NavPointEntry> m_SerializedNavPoints = new List<NavPointEntry>();
        [SerializeField] private NavWeights m_NavWeights;
        [SerializeField] private GameObject TestActor;

        [Header("Tilemap")]
        [SerializeField] private Tilemap m_GroundTilemap;
        [SerializeField] private Tilemap m_TransparentGround;
        [SerializeField] private LayerMask m_GroundLayerMask;
        [SerializeField] private LayerMask m_TransparentLayerMask;

        [Header("Actor")]
        [SerializeField] private Vector2 m_ActorSize = new Vector2(1.0f, 2.0f);
        [SerializeField] private float m_MaxJumpHeight = 5.0f;
        [SerializeField] private float m_MaxJumpDistance = 5.0f;
        [SerializeField] private float m_MaxFallHeight = 10.0f;
        [SerializeField] private float m_MaxFallDistance = 10.0f;


        public static readonly float VerySmallFloat = 0.005f;
        private Vector2 m_TileAnchor;
        private float m_NavPointVerticalOffset;
        private Vector2 m_CellSize;

        private void OnEnable()
        {
            if (TestActor.TryGetComponent<Collider2D>(out Collider2D collider))
            {
                m_ActorSize = collider.bounds.size;
            }

            m_TileAnchor = m_GroundTilemap.tileAnchor;
            m_NavPointVerticalOffset = m_TileAnchor.y + VerySmallFloat;

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
            Generate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Generate();
            }

            if (Input.GetKeyDown(KeyCode.F) && TestActor != null)
            {
                Vector3 playerPos = TestActor.transform.position;
                NavPoint navPoint = GetNavPoint(playerPos);
                if (navPoint != null)
                {
                    Debug.Log($"NavPoint found at {navPoint.Position}. Cell: {navPoint.CellPos}, Type: {navPoint.TypeMask}");
                }
                else
                {
                    Debug.Log("No NavPoint found.");
                }
            }
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
                            var tilePosition = tilemap.WorldToCell(new Vector3(hit.point.x, hit.point.y + VerySmallFloat));

                            if (m_NavPointLookup.TryGetValue(tilePosition, out NavPoint point) && point.HasFlag(NavPoint.Type.Transparent))
                            {
                                //ok
                            }
                            else
                            {
                                //bad, skip 
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

                // Connect Surface neighbors
                if (navPoint.HasFlag(NavPoint.Type.Surface))
                {
                    ConnectSurfaceNeighbors(navPoint);
                    ConnectJumps(navPoint);
                }

                // Connect Edges
                if (navPoint.HasFlag(NavPoint.Type.Edge))
                {
                    ConnectFalls(navPoint);
                }

                // Connect Transparent vertically
                if (navPoint.HasFlag(NavPoint.Type.Transparent))
                {
                    ConnectTransparentVertically(navPoint);
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
                        Target = neighbor,
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
                        Target = neighbor,
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
                            Target = neighbor,
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
                        Target = neighbor,
                        Type = ConnectionType.Surface,
                        Weight = m_NavWeights.SurfaceWeight
                    });
                }
            }
        }

        private void ConnectJumps(NavPoint navPoint)
        {
            float fromX = navPoint.CellPos.x;
            float fromY = navPoint.CellPos.y;

            //fsdfds


            foreach (var kvp in m_NavPointLookup)
            {
                NavPoint target = kvp.Value;

                if (!target.HasFlag(NavPoint.Type.Edge)) continue;

                float verticalDistance = Mathf.Abs(navPoint.Position.y - target.Position.y);
                float horizontalDistance = Mathf.Abs(navPoint.Position.x - target.Position.x);

                float toX = target.CellPos.x;
                float toY = target.CellPos.y;

                if (verticalDistance > m_MaxJumpHeight || horizontalDistance > m_MaxJumpDistance
                    || horizontalDistance <= float.Epsilon)
                {
                    continue;
                }

                // Проверяем, что прыжок возможен только с правильной стороны
                bool isLeftEdge = !HasTileInAnyLayer(target.CellPos + Vector3Int.left);
                bool isRightEdge = !HasTileInAnyLayer(target.CellPos + Vector3Int.right);

                if (!(isLeftEdge && isRightEdge))
                {
                    if (isRightEdge && navPoint.Position.x < target.Position.x) continue; // Прыжок слева на правый edge запрещен
                    if (isLeftEdge && navPoint.Position.x > target.Position.x) continue; // Прыжок справа на левый edge запрещен
                }


                if (IsOnSamePlatform(navPoint, target)) continue;

                Vector2 direction = (target.CellPos - navPoint.CellPos).ToVector2().normalized;

                if (direction.y >= 0)
                {
                    if (verticalDistance > m_MaxJumpHeight)
                    {
                        continue;
                    }

                    //1 horizontal distance jump handled specially
                    if (horizontalDistance <= 1.0f + float.Epsilon)
                    {
                        navPoint.Connections.Add(new Connection
                        {
                            Target = target,
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
                                Target = target,
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
            if (!navPoint.HasFlag(NavPoint.Type.Edge)) return;

            foreach (var kvp in m_NavPointLookup)
            {
                NavPoint target = kvp.Value;

                // Falls are only possible from edges

                float verticalDistance = Mathf.Abs(navPoint.Position.y - target.Position.y);
                float horizontalDistance = Mathf.Abs(navPoint.Position.x - target.Position.x);

                if (target.Position.y >= navPoint.Position.y
                    || Mathf.Abs(target.Position.x - navPoint.Position.x) <= float.Epsilon
                    || verticalDistance > m_MaxFallHeight
                    || horizontalDistance > m_MaxFallDistance)
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
                            Target = target,
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
                            Target = target,
                            Type = ConnectionType.Fall,
                            Weight = m_NavWeights.FallWeight
                        });
                    }
                }

            }
        }


        private void ConnectTransparentVertically(NavPoint navPoint)
        {
            Vector2 cellPosition = new Vector3(navPoint.CellPos.x + m_TileAnchor.x, navPoint.CellPos.y + m_TileAnchor.y, navPoint.CellPos.z);
            Vector2 belowRaycastOrigin = new Vector2(cellPosition.x, cellPosition.y - m_NavPointVerticalOffset);

            // Connect below
            RaycastHit2D hitBelow = Physics2D.Raycast(belowRaycastOrigin, Vector2.down, Mathf.Infinity, m_GroundLayerMask | m_TransparentLayerMask);
            if (hitBelow.collider != null)
            {
                Vector3Int belowPos = m_GroundTilemap.WorldToCell(new Vector3(hitBelow.point.x, hitBelow.point.y - VerySmallFloat)); //before hitBelow.point.y - m_NavPointVerticalOffset
                if (m_NavPointLookup.TryGetValue(belowPos, out NavPoint below) && below.HasFlag(NavPoint.Type.Surface))
                {
                    navPoint.Connections.Add(new Connection
                    {
                        Target = below,
                        Type = ConnectionType.TransparentFall,
                        Weight = m_NavWeights.TransparentFallWeight
                    });
                }
            }

            Vector2 aboveRaycastOrigin = new Vector2(cellPosition.x, cellPosition.y + m_NavPointVerticalOffset);

            // Connect above
            RaycastHit2D hitAbove = Physics2D.Raycast(aboveRaycastOrigin, Vector2.up, m_MaxJumpHeight, m_TransparentLayerMask);
            if (hitAbove.collider != null)
            {
                Vector3Int abovePos = m_GroundTilemap.WorldToCell(new Vector3(hitBelow.point.x, hitBelow.point.y + m_NavPointVerticalOffset));
                if (m_NavPointLookup.TryGetValue(abovePos, out NavPoint above) && above.HasFlag(NavPoint.Type.Transparent))
                {
                    navPoint.Connections.Add(new Connection
                    {
                        Target = above,
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

        private bool HasTileInAnyLayer(Vector3Int cellPos)
        {
            bool groundHas = m_GroundTilemap != null && m_GroundTilemap.HasTile(cellPos);
            bool transparentHas = m_TransparentGround != null && m_TransparentGround.HasTile(cellPos);
            return groundHas || transparentHas;
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
            float rayLength = m_CellSize.y + VerySmallFloat;
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, rayLength, m_GroundLayerMask);

            if (hit.collider != null)
            {
                if ((hit.normal - Vector2.up).sqrMagnitude >= VerySmallFloat)
                {
                    return true;
                }
            }
            return false;
        }

        public NavPoint GetNavPoint(Vector3 worldPosition)
        {
            Vector3 offset = new Vector3(0, m_ActorSize.y / 2 + m_NavPointVerticalOffset, 0);
            Vector3Int cellPos = m_GroundTilemap.WorldToCell(worldPosition - offset);
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

                if (Mathf.Abs(to.y - from.y) > Mathf.Abs(to.x - from.x)) // Главная ось — Y
                {
                    if (Mathf.Abs(hit.point.x - to.x) > m_ActorSize.x / 2 + m_TileAnchor.x + VerySmallFloat)
                    {
                        return false;
                    }
                }
                else // Главная ось — X
                {
                    if (Mathf.Abs(hit.point.y - to.y) > m_ActorSize.y / 2 + m_TileAnchor.y + VerySmallFloat)
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
                return false;
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_NavPointLookup == null) return;

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
                    Gizmos.DrawLine(navPoint.Position, connection.Target.Position);
                }
            }
        }
    }
}