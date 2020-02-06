using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Custom.LevelEditor;

[CanEditMultipleObjects]
public partial class LevelEditorWindow : EditorWindow 
{
    private enum ButtonSize
    {
        Sub,
        Main
    }

    private LevelEditorConfiguration _currentConfiguration;

    private LevelEditorKeyBindings _keyBindings = new LevelEditorKeyBindings();

    private Transform _parentToTiles;

    private Material _sharedTileMaterial;

    private Vector2 _scrollPos;

    private const int _pixelSpaceBetweenSections = 50;
    private const int _pixelSpaceBetweenSectionHeaders = 25;

    private const int _mainButtonSize = 40;
    private const int _subButtonSize = 25;

    private const int _minTileOffset = 1;
    private const int _maxTileOffset = 10;

    public static int _tilePositionOffsetInUnits = 1;

    private int _currentPreviewedTileId = -1;

    private Editor _tilePreviewWindow = null;
    private Texture2D GetPreviewBackgroundWindow
    {
        get 
        {
            Texture2D previewBackground = Resources.Load("PreviewWindow_Background") as Texture2D;

            return previewBackground ? previewBackground : EditorGUIUtility.whiteTexture;
        }
    }

    [MenuItem("LevelEditor/LevelEditorWindow")]
    private static void ShowWindow()
    {
        LevelEditorWindow window = GetWindow<LevelEditorWindow>();
    }

    private void Awake()
    {
        LevelEditor.allSceneTiles = LevelEditorExtentionMethod.GetObjectsInSceneOfType<Tile>();
        _keyBindings.EnableOrDisableInput(true);

        _sharedTileMaterial = Resources.Load("SharedTileMaterial") as Material;

        SelectLatestTileInList();

        if (SceneView.lastActiveSceneView) 
        {
            SceneView.lastActiveSceneView.orthographic = true;
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }

        var configurationPaths = AssetDatabase.FindAssets("t:LevelEditorConfiguration");

        if (configurationPaths.Length > 0) 
        {
            _currentConfiguration = AssetDatabase.LoadAssetAtPath<LevelEditorConfiguration>(AssetDatabase.GUIDToAssetPath(configurationPaths[0]));
        }
        else
        {
            Debug.LogError("No configuration found for LevelEditorInspector.");
        }

        _selectedTheme = _currentConfiguration._lastSelectedTheme;
        minSize = new Vector2(440, 500);
        titleContent.text = "Level Editor Window";
        titleContent.tooltip = "Used to create levels out of modulair pieces.";
    }

    public static void SelectLatestTileInList()
    {
        if (LevelEditor.allSceneTiles.Count > 0) 
        {
            Tile _latestTile = LevelEditor.allSceneTiles.Last();

            if (_latestTile) 
            {
                if (_latestTile.gameObject) 
                {
                    Selection.activeGameObject = LevelEditor.SelectTile(_latestTile.transform);
                }
            }
        }
    }

    private void DrawCurrentselectedTile(bool _forceUpdate = false) 
    {
        GUIStyle bgColor = new GUIStyle();
        bgColor.normal.background = GetPreviewBackgroundWindow;

        if ((_currentPreviewedTileId != _selectedTile) || _forceUpdate) 
        {
            _currentPreviewedTileId = _selectedTile;
            _tilePreviewWindow = Editor.CreateEditor(_currentConfiguration.TileSets[_selectedTile]);
        }


        if (_tilePreviewWindow && GetPreviewBackgroundWindow)
        {
            _tilePreviewWindow.OnPreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
        }
    }

    public void PlaceTile() 
    {
        Selection.activeGameObject = LevelEditor.SpawnNewTile(_currentConfiguration.TileSets[_selectedTile]);
        if (SceneView.sceneViews.Count > 0) 
        {
            SceneView sceneView = (SceneView)SceneView.sceneViews[0];
            sceneView.Focus();
        }
    }

    public void OnGUI() 
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        if (GUILayout.Button("Spawn Tile"))
        {
            PlaceTile();
            if(_parentToTiles) 
            {
                if (_parentToTiles != Selection.activeGameObject.transform) 
                {
                    Selection.activeGameObject.transform.SetParent(_parentToTiles);
                }
            }
        }

        DrawTileOptionDropdown();
        DrawCurrentselectedTile();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("[INPUTS] \n W.A.S.D & Arrowkeys = MoveTile \n K.L = Previous/Next tile \n Q.E = Rotate Counter/Clockwise \n Backspace = CTRL Z \n Enter = PlaceTile.", MessageType.Info);

        DrawTileMovementButtons();

        _tilePositionOffsetInUnits = EditorGUILayout.IntField ("Movement in Units:", _tilePositionOffsetInUnits);
        _tilePositionOffsetInUnits = Mathf.Clamp(_tilePositionOffsetInUnits, _minTileOffset, _maxTileOffset);

        GUILayout.Space(_pixelSpaceBetweenSections);
        DrawThemeDropdown();

        GUILayout.Space(_pixelSpaceBetweenSectionHeaders);

        EditorGUILayout.EndScrollView();
    }

    private void DrawTileMovementButtons()
    {
        using (var h1 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();

            if ((GUILayout.Button(Resources.Load("RotateClockwiseIcon") as Texture2D, Sizes(ButtonSize.Main)))) 
            {
                LevelEditor.RotateTile(LevelEditor.Direction.Right);
            }

            if ((GUILayout.Button(Resources.Load("RotateCounterClockwiseIcon") as Texture2D, Sizes(ButtonSize.Main)))) 
            {
                LevelEditor.RotateTile(LevelEditor.Direction.Left);
            }

            GUILayout.FlexibleSpace();
        }

        GUILayout.Space(_pixelSpaceBetweenSectionHeaders);

        using (var h2 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();

            if ((GUILayout.Button(Resources.Load("Arrow_Up") as Texture2D, Sizes(ButtonSize.Sub))))
            {
                LevelEditor.MoveSelectedObject(new Vector3(0, 0, _tilePositionOffsetInUnits));
            }

            GUILayout.FlexibleSpace();
        }

        using (var h3 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();

            if ((GUILayout.Button(Resources.Load("Arrow_Up") as Texture2D, Sizes(ButtonSize.Sub))))
            {
                LevelEditor.MoveSelectedObject(new Vector3(0, 1, 0));
            }

            GUILayout.Space(20);

            if ((GUILayout.Button(Resources.Load("Arrow_Left") as Texture2D, Sizes(ButtonSize.Sub)))) 
            {
                LevelEditor.MoveSelectedObject(new Vector3(-_tilePositionOffsetInUnits, 0, 0));
            }

            GUILayout.Space(20);

            if ((GUILayout.Button(Resources.Load("Arrow_Right") as Texture2D, Sizes(ButtonSize.Sub))))
            {
                LevelEditor.MoveSelectedObject(new Vector3(_tilePositionOffsetInUnits, 0, 0));
            }

            GUILayout.Space(20);

            if ((GUILayout.Button(Resources.Load("Arrow_Down") as Texture2D, Sizes(ButtonSize.Sub)))) 
            {
                LevelEditor.MoveSelectedObject(new Vector3(0, -1, 0));
            }
            GUILayout.FlexibleSpace();
        }

        using (var h4 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();

            if ((GUILayout.Button(Resources.Load("Arrow_Down") as Texture2D, Sizes(ButtonSize.Sub))))
            {
                LevelEditor.MoveSelectedObject(new Vector3(0, 0, -_tilePositionOffsetInUnits));
            }

            GUILayout.FlexibleSpace();
        }


        GUILayout.Space(20);

        using (var h5 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();

            if ((GUILayout.Button("Place Tile", GUILayout.Width(100), GUILayout.Height(20))))
            {
                Selection.activeGameObject = LevelEditor.SpawnNewTile(_currentConfiguration.TileSets[_selectedTile]);
            }

            if ((GUILayout.Button("Remove Selected Tile", GUILayout.Width(140), GUILayout.Height(20)))) 
            {
                LevelEditor.RemoveSelectedTile();
                SelectLatestTileInList();
            }

            GUILayout.FlexibleSpace();
        }

        GUILayout.Space(10);

        _parentToTiles = EditorGUILayout.ObjectField(_parentToTiles, typeof(Transform), true) as Transform;

        GUILayout.Space(10);

        using (var h6 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();

            if ((GUILayout.Button("Assign Tiles To Parent", GUILayout.Width(200), GUILayout.Height(20)))) 
            {
                ChildAllTilesToParent();
            }

            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
        }

        GUILayout.Space(10);

        using (var h7 = new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;

            if ((GUILayout.Button(Resources.Load("WarningSymbol") as Texture2D, GUILayout.Width(180), GUILayout.Height(20))))
            {
                PopupWindowContent content = new LevelEditorPopupSceneFinalize();

                Vector2 popWindowPos = new Vector2(GetWindow<LevelEditorWindow>().position.size.x / 2 - 100, 440);
                Vector2 popupWindowSize = new Vector2(40, 40);

                Rect popupWindow = new Rect(popWindowPos, popupWindowSize);
                PopupWindow.Show(popupWindow, content);
            }
            GUILayout.FlexibleSpace();
        }

        GUILayout.Space(10);

        if (EditorApplication.isSceneDirty) 
        {
            GUI.backgroundColor = Color.red;
            EditorGUILayout.HelpBox("[WARNING] There are currently some unsaved changes within the Scene.", MessageType.Warning);
        }
        else 
        {
            GUI.backgroundColor = Color.green;
        }

        using (var h8 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();

            if ((GUILayout.Button("Save Scene", GUILayout.Width(80), GUILayout.Height(20))))
            {
                PopupWindowContent content = new LevelEditorPopupScene();

                Vector2 popWindowPos = new Vector2(GetWindow<LevelEditorWindow>().position.size.x / 2 - 100, 520);
                Vector2 popupWindowSize = new Vector2(40, 40);
     
                Rect popupWindow = new Rect(popWindowPos, popupWindowSize);
                PopupWindow.Show(popupWindow, content);

            }

            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
        }
    }

    private void ChildAllTilesToParent() 
    {
        if(!_parentToTiles) 
        {
            Debug.LogWarning("Please assign a Parent for the tiles to be assigned to.");
            return;
        }

        if (LevelEditor.allSceneTiles.Count > 0) 
        {
            foreach (Tile tile in LevelEditor.allSceneTiles) 
            {
                if(tile.transform == _parentToTiles) 
                {
                    Debug.LogErrorFormat($"A tile called [{ tile.gameObject.name }] tried to child it's transform to itself, please refer to another transform.");
                }
                else 
                {
                    tile.transform.SetParent(_parentToTiles);
                }
            }
        }
    }

    private GUILayoutOption[] Sizes(ButtonSize buttonSize)
    {
        int size = buttonSize == ButtonSize.Main ? _mainButtonSize : _subButtonSize;
        return new GUILayoutOption[] { GUILayout.Width(size), GUILayout.Height(size) };
    }

    public void OnDestroy()
    {
        _keyBindings.EnableOrDisableInput(false);
    }

}
