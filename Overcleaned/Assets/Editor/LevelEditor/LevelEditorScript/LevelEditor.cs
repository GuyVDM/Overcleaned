using System.Collections.Generic;

namespace UnityEngine.Custom.LevelEditor 
{
    public static class LevelEditor
    {
        public static Transform currentlySelectedTile;
        public static List<Tile> allSceneTiles = new List<Tile>();

        public enum Direction { Left, Right };

        public static void MoveSelectedObject(Vector3 direction)
        {
            if (currentlySelectedTile)
            {
                currentlySelectedTile.transform.position += direction;
            }
        }

        public static GameObject SpawnNewTile(GameObject tile)
        {
            Vector3 _spawnPos = currentlySelectedTile != null ? currentlySelectedTile.transform.position : Vector3.zero;
            Quaternion _spawnRotation = currentlySelectedTile != null ? currentlySelectedTile.transform.rotation : Quaternion.identity;

            Tile _newelySpawnedTile = Object.Instantiate(tile, _spawnPos, _spawnRotation).GetComponent<Tile>();

            currentlySelectedTile = _newelySpawnedTile.transform;
            _newelySpawnedTile.gameObject.isStatic = true;

            RemoveAllNullSpaces();
            allSceneTiles.Add(_newelySpawnedTile);

            return _newelySpawnedTile.gameObject;
        }

        public static GameObject SelectTile(Transform tile)
        {
            if (tile) 
            {
                currentlySelectedTile = tile;
                return currentlySelectedTile.gameObject;
            }

            return null;
        }

        public static GameObject SwapTileType(GameObject _newTile) 
        {
            if (currentlySelectedTile) 
            {
                Vector3 _spawnPos = currentlySelectedTile != null ? currentlySelectedTile.transform.position : Vector3.zero;
                Quaternion _spawnRotation = currentlySelectedTile != null ? currentlySelectedTile.transform.rotation : Quaternion.identity;

                Tile _newelySpawnedTile = Object.Instantiate(_newTile, _spawnPos, _spawnRotation).GetComponent<Tile>();
                Object.DestroyImmediate(currentlySelectedTile.gameObject);

                currentlySelectedTile = _newelySpawnedTile.transform;
                _newelySpawnedTile.gameObject.isStatic = true;

                RemoveAllNullSpaces();
                allSceneTiles.Add(_newelySpawnedTile);

                return _newelySpawnedTile.gameObject;
            }

            return null;
        }

        public static void RotateTile(Direction direction)
        {
            if (currentlySelectedTile) 
            {
                Vector3 rotation = new Vector3(0, direction == Direction.Left ? -90 : 90, 0);
                currentlySelectedTile.eulerAngles += rotation;
            }
        }

        public static void RemoveSelectedTile() 
        {
            if (currentlySelectedTile)
            {
                allSceneTiles.Remove(currentlySelectedTile.GetComponent<Tile>());
                RemoveAllNullSpaces();
                Object.DestroyImmediate(currentlySelectedTile.gameObject);
            }
        }

        private static void RemoveAllNullSpaces() 
        {
            if (allSceneTiles.Count > 0)
            {
                for (int i = allSceneTiles.Count - 1; i != 0; i--) 
                {
                    if (allSceneTiles[i] == null) 
                    {
                        allSceneTiles.RemoveAt(i);
                    }
                }
            }
        }

        public static void PlaceTile(GameObject tile) 
        {
            if (currentlySelectedTile) 
            {
                SpawnNewTile(tile);
            }
        }

        public static void FinalizeTiles() 
        {
            foreach(Tile tile in allSceneTiles) 
            {
                tile.Dispose();
            }

            allSceneTiles.Clear();
            currentlySelectedTile = null;
        }
    }
}
