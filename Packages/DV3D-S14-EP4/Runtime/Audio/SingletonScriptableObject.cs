using UnityEngine;
public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                T[] results = Resources.FindObjectsOfTypeAll<T>();
                if (results.Length == 0)
                {
                    Debug.LogError("SingletonScriptableObject<" + typeof(T).Name + "> not found in Resources.");
                    return null;
                }
                else if (results.Length > 1)
                {
                    Debug.LogError("SingletonScriptableObject<" + typeof(T).Name + "> found multiple in Resources.");
                    return null;
                }
                instance = results[0];
                instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            return instance;
        }
    }
}