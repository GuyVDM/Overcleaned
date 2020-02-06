using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Custom.LevelEditor;

[ExecuteInEditMode]
public class Tile : MonoBehaviour
{
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (LevelEditor.currentlySelectedTile) 
        {
            if (LevelEditor.currentlySelectedTile.GetComponent<Tile>() == this) 
            {
                var centeredStyle = GUI.skin.GetStyle("Label");
                centeredStyle.alignment = TextAnchor.UpperCenter;
                centeredStyle.normal.textColor = Color.blue;

                Handles.Label(transform.position + new Vector3(0, 0, 0.5f), "Current Selected Tile:", centeredStyle);
                Handles.Label(transform.position, transform.position.ToString(), centeredStyle);
            }
        }
    }

    public void Dispose() 
    {
        DestroyImmediate(this);
    }
#endif
}
