using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public partial class LevelEditorWindow
{

    private int _selectedTheme = 0;
    private int _oldSelectedTheme = 0;


    private void DrawThemeDropdown()
    {
        EditorGUILayout.HelpBox("By selecting one of the drop down menus, you can manually completely change the level theme.", MessageType.Info);
        _selectedTheme = EditorGUILayout.Popup("Level Themes", _selectedTheme, GetAllCurrentThemeNameOptions);

        if (_selectedTheme != _oldSelectedTheme) {
            _sharedTileMaterial.shader = Shader.Find(_currentConfiguration.TileThemes[_selectedTheme].shader.name);
            _sharedTileMaterial.color = _currentConfiguration.TileThemes[_selectedTheme].color;
            _sharedTileMaterial.mainTexture = _currentConfiguration.TileThemes[_selectedTheme].mainTexture;
            _currentConfiguration._lastSelectedTheme = _selectedTheme;
            DrawCurrentselectedTile(true);
        }

        _oldSelectedTheme = _selectedTheme;
    }

    private string[] GetAllCurrentThemeNameOptions 
    {
        get 
        {
            List<string> dropdownOptions = new List<string>();

            if (_currentConfiguration)
            {
                foreach (Material theme in _currentConfiguration.TileThemes) 
                {
                    dropdownOptions.Add(theme.name);
                }
            }

            return dropdownOptions.ToArray();
        }
    }
}
