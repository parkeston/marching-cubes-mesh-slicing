using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();
    private static bool _applicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
                return null;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T) FindObjectOfType(typeof(T));
                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject("(hm_singleton) " + typeof(T));
                        _instance = singleton.AddComponent<T>();
                        DontDestroyOnLoad(singleton);
                    }
                }

                return _instance;
            }
        }
    }

    private void OnDestroy()
    {
        _applicationIsQuitting = true;
    }
}