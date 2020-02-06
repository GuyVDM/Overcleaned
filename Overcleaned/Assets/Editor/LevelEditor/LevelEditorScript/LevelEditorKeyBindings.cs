using UnityEngine.Custom.LevelEditor;
using UnityEngine;
using UnityEditor;

public class LevelEditorKeyBindings 
{

    private float TileMovementOffset 
    {
        get 
        {
            return LevelEditorWindow._tilePositionOffsetInUnits;
        }
    }


    public void EnableOrDisableInput(bool isEnabled)
    {
        if (isEnabled) 
        {
            SceneView.onSceneGUIDelegate += CheckInput;
            TryFocusScene();
        }
        else
        {
            SceneView.onSceneGUIDelegate -= CheckInput;
        }
    }

    private void CheckInput(SceneView view)
    {
        Vector3 direction = Vector3.zero;

        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Q:
                    LevelEditor.RotateTile(LevelEditor.Direction.Left);
                    break;

                case KeyCode.E:
                    LevelEditor.RotateTile(LevelEditor.Direction.Right);
                    break;

                case KeyCode.W:
                case KeyCode.UpArrow:
                    direction = new Vector3(0, 0, TileMovementOffset);
                    LevelEditor.MoveSelectedObject(direction);
                    return;

                case KeyCode.S:
                case KeyCode.DownArrow:
                    direction = new Vector3(0, 0, -TileMovementOffset);
                    LevelEditor.MoveSelectedObject(direction);
                    return;

                case KeyCode.A:
                case KeyCode.LeftArrow:
                    direction = new Vector3(-TileMovementOffset, 0, 0);
                    LevelEditor.MoveSelectedObject(direction);
                    return;

                case KeyCode.D:
                case KeyCode.RightArrow:
                    direction = new Vector3(TileMovementOffset, 0, 0);
                    LevelEditor.MoveSelectedObject(direction);
                    return;

                case KeyCode.Return:
                    EditorWindow.GetWindow<LevelEditorWindow>().PlaceTile();
                    break;

                case KeyCode.Backspace:
                    LevelEditor.RemoveSelectedTile();
                    LevelEditorWindow.SelectLatestTileInList();
                    break;

                case KeyCode.K:
                    EditorWindow.GetWindow<LevelEditorWindow>().SelectOtherTile(LevelEditorWindow.SelectTile.Previous);
                    TryFocusScene();
                    break;

                case KeyCode.L:
                    EditorWindow.GetWindow<LevelEditorWindow>().SelectOtherTile(LevelEditorWindow.SelectTile.Next);
                    TryFocusScene();
                    break;              
            }
        }
    }

    private void TryFocusScene() 
    {
        if (SceneView.GetAllSceneCameras().Length > 0)
        {
            if (SceneView.sceneViews.Count > 0) 
            {
                SceneView sceneView = (SceneView)SceneView.sceneViews[0];
                sceneView.Focus();
            }

            return;
        }
    }
}
