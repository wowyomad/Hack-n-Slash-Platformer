using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

namespace Nav2D
{
    [System.Serializable]
    public struct NavPoint
    {
        public Vector3Int CellPos;
        public Vector2 Position;
        public Type TypeMask;

        [System.Flags]
        public enum Type : uint
        {
            None = 0,
            Surface = 1 << 0,
            Edge = 1 << 1,
            Transparent = 1 << 2,
        }

        public bool HasFlag(Type flag)
        {
            return (TypeMask & flag) == flag;
        }
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
        private List<NavPointEntry> m_SerializedNavPoints = new List<NavPointEntry>();

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

        public static readonly float VerySmallFloat = 0.005f;
        private Vector2 m_TileAnchor;
        private float m_NavPointVerticalOffset;
        private Vector2 m_CellSize;

        private void Start()
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

            m_CellSize = m_GroundTilemap.cellSize;

            Generate();
        }

        private void OnEnable()
        {
            DeserializeNavPoints();
        }

        private void OnDisable()
        {
            SerializeNavPoints();
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
                NavPoint? navPoint = GetNavPoint(playerPos);
                if (navPoint != null)
                {
                    Debug.Log($"NavPoint found at {navPoint.Value.Position}. Cell: {navPoint.Value.CellPos}, Type: {navPoint.Value.TypeMask}");
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

            GenerateBaseNavPoints(m_GroundTilemap, NavPoint.Type.Surface);
            GenerateBaseNavPoints(m_TransparentGround, NavPoint.Type.Surface | NavPoint.Type.Transparent);

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

                        if (hit.collider == null)
                        {
                            NavPoint navPoint = new NavPoint
                            {
                                CellPos = cellPos,
                                TypeMask = baseType,
                                Position = new Vector2(tileSurface.x, tileSurface.y + m_ActorSize.y / 2),
                            };

                            bool hasNoLeftNeighbor = !HasTileInAnyLayer(cellPos + Vector3Int.left) && !HasTileInAnyLayer(cellPos + Vector3Int.left + Vector3Int.up);
                            bool hasNoRightNeighbor = !HasTileInAnyLayer(cellPos + Vector3Int.right) && !HasTileInAnyLayer(cellPos + Vector3Int.right + Vector3Int.up);

                            bool isEdge = hasNoLeftNeighbor || hasNoRightNeighbor;

                            if (isEdge && !IsSlopeAtCell(cellPos))
                            {
                                navPoint.TypeMask |= NavPoint.Type.Edge;
                            }

                            m_NavPointLookup[cellPos] = navPoint;
                        }
                    }
                }
            }
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

        public NavPoint? GetNavPoint(Vector3 worldPosition)
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
                }//
            }

        }

        private void OnDrawGizmosSelected()
        {
            if (m_NavPointLookup == null) return;

            foreach (var navPoint in m_NavPointLookup.Values)
            {
                bool isEdge = navPoint.HasFlag(NavPoint.Type.Edge);
                bool isTransparent = navPoint.HasFlag(NavPoint.Type.Transparent);

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
            }
        }
    }
}