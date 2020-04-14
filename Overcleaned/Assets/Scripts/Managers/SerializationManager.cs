using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializableData
{

}

public static class SerializationManager {

    public enum SerializationMode
    {
        Binary,
        JSON,
    }

    /// <summary>
    /// Use this function to save a single file inheriting from SerializableData.
    /// </summary>
    /// <param name="toSave">File deriving from SerializableData you want to save</param>
    /// <param name="directory">File directory you want to save the file to. If the directory does not exist, it will be made</param>
    /// <param name="fileName">How will the file be named?</param>
    /// <param name="fileExtension">What will the extension of the file be?</param>
    /// <param name="saveMode">What kind of file will it be?</param>
    public static void SaveFile(SerializableData toSave, string directory, string fileName, string fileExtension, SerializationMode saveMode)
    {
        CheckDirectory(directory, true);

        switch (saveMode)
        {
            case SerializationMode.Binary:
                SaveByBinary(new SerializableData[] { toSave }, Path.Combine(directory, fileName + fileExtension));
                break;
            case SerializationMode.JSON:
                SaveByJSON(new SerializableData[] { toSave }, Path.Combine(directory, fileName + fileExtension));
                break;
        }

        Debug.Log("File saved to " + directory);
    }

    /// <summary>
    /// Use this function to save an array of files inheriting from SerializableData.
    /// </summary>
    /// <param name="toSave">An array of files deriving from SerializableData you want to save</param>
    /// <param name="directory">File directory you want to save the file to. If the directory does not exist, it will be made</param>
    /// <param name="fileName">How will the file be named?</param>
    /// <param name="fileExtension">What will the extension of the file be?</param>
    /// <param name="saveMode">What kind of file will it be?</param>
    public static void SaveFiles(SerializableData[] toSave, string directory, string fileName, string fileExtension, SerializationMode saveMode)
    {
        CheckDirectory(directory, true);

        switch (saveMode)
        {
            case SerializationMode.Binary:
                SaveByBinary(toSave, Path.Combine(directory, fileName + fileExtension));
                break;
            case SerializationMode.JSON:
                SaveByJSON(toSave, Path.Combine(directory, fileName + fileExtension));
                break;
        }

        Debug.Log("Files saved to " + directory);
    }

    /// <summary>
    /// Use this function to load a single file. Cast the file as the type you want. Type must derive from SerializableData.
    /// </summary>
    /// <param name="directory">Where is the file located?</param>
    /// <param name="fileName">What is the name of the file?</param>
    /// <param name="fileExtension">What is the extension of the file?</param>
    /// <param name="loadMode">What kind of file is it?</param>
    /// <returns></returns>
    public static SerializableData LoadFile(string directory, string fileName, string fileExtension, SerializationMode loadMode)
    {
        if (File.Exists(Path.Combine(directory, fileName + fileExtension)))
        {
            SerializableData toReturn = null;

            switch (loadMode)
            {
                case SerializationMode.Binary:
                    toReturn = LoadByBinary(Path.Combine(directory, fileName + fileExtension))[0];
                    break;
                case SerializationMode.JSON:
                    toReturn = LoadByJSON(Path.Combine(directory, fileName + fileExtension))[0];
                    break;
            }

            return toReturn;
        }

        Debug.LogError("The file you are trying to load does not exist. File name " + fileName + ". Directory: " + directory);
        return null;
    }

    /// <summary>
    /// Use this function to load an array of files.
    /// </summary>
    /// <param name="directory">Where is the file located?</param>
    /// <param name="fileName">What is the name of the file?</param>
    /// <param name="fileExtension">What is the extension of the file?</param>
    /// <param name="loadMode">What kind of file is it?</param>
    /// <returns></returns>
    public static SerializableData[] LoadFiles(string directory, string fileName, string fileExtension, SerializationMode loadMode)
    {
        if (File.Exists(Path.Combine(directory, fileName + fileExtension)))
        {
            SerializableData[] toReturn = null;

            switch (loadMode)
            {
                case SerializationMode.Binary:
                    toReturn = LoadByBinary(Path.Combine(directory, fileName + fileExtension));
                    break;
                case SerializationMode.JSON:
                    toReturn = LoadByJSON(Path.Combine(directory, fileName + fileExtension));
                    break;
            }

            return toReturn;
        }

        Debug.LogError("The file you are trying to load does not exist. File name " + fileName + ". Directory: " + directory);
        return null;
    }

    /// <summary>
    /// Get the names of all files in a folder.
    /// </summary>
    /// <param name="directory">the folder path the files are located in.</param>
    /// <returns>Returns an array of file names</returns>
    public static string[] GetFileNamesInFolder(string directory)
    {
        List<string> toReturn = new List<string>();
        DirectoryInfo info = new DirectoryInfo(directory);
        FileInfo[] fileInfos = info.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            toReturn.Add(fileInfos[i].Name);
        }

        return toReturn.ToArray();
    }

    public static bool FileExists(string directory, string fileName, string fileExtension)
    {
        return File.Exists(Path.Combine(directory, fileName + fileExtension));
    }

    /// <summary>
    /// Will save files with the binary formatter to a given path.
    /// </summary>
    /// <param name="toSave"></param>
    /// <param name="path"></param>
    private static void SaveByBinary(SerializableData[] toSave, string path)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fileStream = File.Create(path);

        bf.Serialize(fileStream, toSave);
        fileStream.Close();
    }

    /// <summary>
    /// Will save files to a JSON file to a given path.
    /// </summary>
    /// <param name="toSave"></param>
    /// <param name="path"></param>
    private static void SaveByJSON(SerializableData[] toSave, string path)
    {
        string json = string.Empty;
        //List save = new List();

        for (int i = 0; i < toSave.Length; i++)
        {
            string temp = JsonUtility.ToJson(toSave[i], true);
            json += temp;
        }

        StreamWriter sw = File.CreateText(path);
        sw.Close();

        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Loads binary files from a given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static SerializableData[] LoadByBinary(string path)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fileStream = File.Open(path, FileMode.Open);

        SerializableData[] toReturn = (SerializableData[])bf.Deserialize(fileStream);
        fileStream.Close();

        return toReturn;
    }

    /// <summary>
    /// Loads JSON files from a given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static SerializableData[] LoadByJSON(string path)
    {
        string json = File.ReadAllText(path);
        SerializableData[] toReturn = JsonUtility.FromJson<SerializableData[]>(json);

        return toReturn;
    }

    /// <summary>
    /// Checks the given directory if it exists and makes creates it if it doesn't.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="force"></param>
    /// <returns></returns>
    private static bool CheckDirectory(string directory, bool force)
    {
        if (!Directory.Exists(directory))
        {
            if (force)
            {
                Debug.LogWarning("The directory " + directory + " did not exist, but has now been created.");
                Directory.CreateDirectory(directory);
                return true;
            }
            else
            {
                Debug.LogError("The directory " + directory + " Does not exist.");
                return false;
            }
        }

        return false;
    }
}
