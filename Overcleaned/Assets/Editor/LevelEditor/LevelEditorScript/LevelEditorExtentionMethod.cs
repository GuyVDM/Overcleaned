using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace UnityEngine.Custom.LevelEditor 
{
    public static class LevelEditorExtentionMethod 
    {

        public static List<T> GetObjectsInSceneOfType<T>() where T : MonoBehaviour 
        {
            List<T> listOfCollectedTypes = new List<T>();

            foreach (GameObject gameObject in SceneManager.GetActiveScene().GetRootGameObjects()) 
            {
                T[] collectedTypesOfRootObject = gameObject.GetComponentsInChildren<T>();

                foreach (T type in collectedTypesOfRootObject)
                {
                    listOfCollectedTypes.Add(type);
                }
            }

            return listOfCollectedTypes;
        }
    }
}
