using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Custom.LevelEditor;

public partial class LevelEditorWindow
{
    public enum SelectTile
    {
        Previous, Next
    }

    private int _selectedTile = 0;
    private int _oldSelectedTile = 0;


    public void SelectOtherTile(SelectTile tileToSelect) 
    {
        if (_currentConfiguration.TileSets.Count > 0) 
        {
            _selectedTile = tileToSelect == SelectTile.Previous ? - 1 : + 1;
            _selectedTile = UnityEngine.Mathf.Clamp(_selectedTile, 0, _currentConfiguration.TileSets.Count - 1);
        }
    }

    private void DrawTileOptionDropdown() 
    {
        _selectedTile = EditorGUILayout.Popup("Tile Sorts", _selectedTile, GetAllCurrentTileNameOptions);

        if (_selectedTile != _oldSelectedTile) {
            Selection.activeGameObject = LevelEditor.SwapTileType(_currentConfiguration.TileSets[_selectedTile]);
        }

        _oldSelectedTile = _selectedTile;
    }

    private string[] GetAllCurrentTileNameOptions 
    {
        get 
        {
            List<string> dropdownOptions = new List<string>();

            if (_currentConfiguration)
            {
                foreach (GameObject tile in _currentConfiguration.TileSets)
                {
                    dropdownOptions.Add(tile.name);
                }
            }

            return dropdownOptions.ToArray();
        }
    }
}
