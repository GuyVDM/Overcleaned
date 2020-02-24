using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class DataStreamExtention
{
    public static byte[] ToByteArray(this object obj)
    {
        if (obj == null) 
        {
            return null;
        }

        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static T FromByteArray<T>(this byte[] arrBytes)
    {
        if (arrBytes == null) 
        {
            return default;
        }

        using (MemoryStream memStream = new MemoryStream()) 
        {
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = binForm.Deserialize(memStream);

            return (T)obj;
        }
    }
}
