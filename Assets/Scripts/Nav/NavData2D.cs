using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

namespace Nav2D
{
    public struct NavPoint
    {
        public Vector3Int CellPos;
        public Vector2 Position;
        public Type TypeMask;

        [Flags]
        public enum Type : uint
        {
            None = 0,
            Walkable = 1 << 0,
            Jumpable = 1 << 1,
            Transparent = 1 << 2,
        }

        public bool HasFlag(Type flag)
        {
            return (TypeMask & flag) == flag;
        }
    }

    public class NavData2D : MonoBehaviour
    {
        private List<NavPoint> m_NavPoints = new List<NavPoint>();

        [Header("Tilemap")]
        [SerializeField] private Tilemap m_GroundTilemap;
        [SerializeField] private Tilemap m_TransparentGround;
        [SerializeField] private Vector2 m_ActorSize = new Vector2(1.0f, 2.0f);
        [SerializeField] private LayerMask m_GroundLayerMask;

        private const float VerySmallFloat = 0.005f;
        private const float NavPointVerticalOffset = 0.5f + VerySmallFloat;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Generate();
            }
        }

        public void Generate()
        {
            Debug.Log("Generating NavPoints...");
            m_NavPoints.Clear();

            GenerateBaseNavPoints(m_GroundTilemap, NavPoint.Type.Walkable);
            GenerateBaseNavPoints(m_TransparentGround, NavPoint.Type.Walkable | NavPoint.Type.Transparent);

            CalculateJumpableEdges();

            Debug.Log("NavPoints generated: " + m_NavPoints.Count);
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
                        // --- Check 1: Is the cell directly above empty? ---
                        Vector3Int cellAbove = cellPos + Vector3Int.up;
                        if (HasTileInAnyLayer(cellAbove))
                        {
                            continue; // Skip this tile if there's another tile directly above it
                        }

                        Vector3 tileWorldPos = tilemap.CellToWorld(cellPos);
                        Vector2 tileSurfaceCenter = new Vector2(tileWorldPos.x + tileAnchor.x, tileWorldPos.y + tileAnchor.y);
                        Vector2 navPointPos = new Vector2(tileSurfaceCenter.x, tileSurfaceCenter.y + NavPointVerticalOffset);

                        // --- Check 2: Is there enough space above for the actor? ---
                        // Use original Raycast logic, starting from the nav point position
                        RaycastHit2D hit = Physics2D.Raycast(navPointPos, Vector2.up, m_ActorSize.y, m_GroundLayerMask);

                        if (hit.collider == null)
                        {
                            NavPoint navPoint = new NavPoint
                            {
                                CellPos = cellPos,
                                TypeMask = baseType,
                                Position = navPointPos
                            };
                            m_NavPoints.Add(navPoint);
                        }
                    }
                }
            }
        }

        private void CalculateJumpableEdges()
        {
            List<NavPoint> updatedNavPoints = new List<NavPoint>(m_NavPoints.Count);

            for (int i = 0; i < m_NavPoints.Count; i++)
            {
                NavPoint currentPoint = m_NavPoints[i];
                Vector3Int cellPos = currentPoint.CellPos;

                bool hasNoLeftNeighbor = !HasTileInAnyLayer(cellPos + Vector3Int.left);
                bool hasNoRightNeighbor = !HasTileInAnyLayer(cellPos + Vector3Int.right);

                bool isEdge = hasNoLeftNeighbor || hasNoRightNeighbor;

                if (isEdge)
                {
                    if (!IsSlopeAtCell(cellPos))
                    {
                        currentPoint.TypeMask |= NavPoint.Type.Jumpable;
                    }
                }

                updatedNavPoints.Add(currentPoint);
            }
            m_NavPoints = updatedNavPoints;
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
            Vector2 raycastOrigin = new Vector2(worldTilePositoin.x, worldTilePositoin.y + 0.5f + 0.1f);
            float rayLength = 1.0f + 0.1f;
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, rayLength, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                //if normal is not straight up, it's a slope
                if (hit.normal != Vector2.up)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_NavPoints == null) return;

            foreach (var navPoint in m_NavPoints)
            {
                bool isJumpable = navPoint.HasFlag(NavPoint.Type.Jumpable);
                bool isTransparent = navPoint.HasFlag(NavPoint.Type.Transparent);

                if (isJumpable && isTransparent)
                {
                    Gizmos.color = Color.cyan;
                }
                else if (isJumpable)
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

                Gizmos.DrawSphere(navPoint.Position, 0.2f);
            }
        }
    }
}