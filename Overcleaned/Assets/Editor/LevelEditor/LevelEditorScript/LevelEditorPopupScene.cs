using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class LevelEditorPopupScene : PopupWindowContent
{
    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("Warning:", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Label("Are you sure you want to save?", EditorStyles.helpBox);

        GUILayout.Space(20);

        using (var h1 = new EditorGUILayout.HorizontalScope()) 
        {
            GUILayout.FlexibleSpace();
            if ((GUILayout.Button("Save Scene", GUILayout.Width(100), GUILayout.Height(20))))
            {

                this.editorWindow.Close();
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                Debug.Log("Scene saved...");
            }
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
