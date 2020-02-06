using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Configurations/LevelEditor", menuName = "LevelEditorSettings")]
public class LevelEditorConfiguration : ScriptableObject
{
    public IReadOnlyList<GameObject> TileSets => _tileSets;
    public IReadOnlyList<Material> TileThemes => _tileThemes;

    internal int _lastSelectedTheme;

    [SerializeField]
    private GameObject[] _tileSets;

    [SerializeField]
    private Material[] _tileThemes; 
}
