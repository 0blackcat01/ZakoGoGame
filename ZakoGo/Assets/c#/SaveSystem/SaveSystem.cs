using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    // Start is called before the first frame update
    public static void SaveByJson(string savaFileName, object data)
    {
        var json = JsonUtility.ToJson(data);
        var path = Path.Combine(Application.persistentDataPath,savaFileName);


        try
        {
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            Debug.Log($"Susscessfully saved data to{path}.");
#endif
        }
        catch(System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Failed saved data to{path}.\n{ e}");
#endif
        }
    }
    public static T LoadFromJson<T>(string saveFileName)
    {
        var path = Path.Combine(Application.persistentDataPath, saveFileName);

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<T>(json);
#if UNITY_EDITOR
            Debug.Log($"Susscessfully loaded data to{path}.");
#endif
            return data;
        }
        catch(System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Failed loaded data to{path}.\n{e}");
#endif
            return default;
        }
    }
    public static void DeleteSaveFile(string saveFileName)
    {
        var path = Path.Combine(Application.persistentDataPath, saveFileName);
        
        try
        {
            File.Delete(path);
        }
        catch(System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Failed deleted data to{path}.\n{e}");
#endif

        }
    }
}
