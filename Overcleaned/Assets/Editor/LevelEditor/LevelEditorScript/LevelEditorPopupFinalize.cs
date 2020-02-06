using UnityEngine;
using UnityEditor;
using UnityEngine.Custom.LevelEditor;

public class LevelEditorPopupSceneFinalize : PopupWindowContent 
{

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("WARNING:", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Label("Are you sure you want to finalize the scene? This would mean being permanently unable to adjust all the tiles currently in the scene.", EditorStyles.helpBox);

        GUILayout.Space(20);

        using (var h1 = new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.red;
            if ((GUILayout.Button("FINALIZE", GUILayout.Width(100), GUILayout.Height(20)))) 
            {

                this.editorWindow.Close();
                LevelEditor.FinalizeTiles();
                Debug.Log("Finalized scene...");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
        }

        using (var h2 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();
            if ((GUILayout.Button("Close", GUILayout.Width(100), GUILayout.Height(20))))
            {
                this.editorWindow.Close();
            }
            GUILayout.FlexibleSpace();
        }
    }
}
