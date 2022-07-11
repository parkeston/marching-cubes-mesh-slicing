using UnityEngine;
#if UNITY_EDITOR
	using System.IO;
	using UnityEditor;
#endif
#if ODIN_INSPECTOR
	using Sirenix.OdinInspector;
#endif


#if ODIN_INSPECTOR
	public abstract class HMScriptableSingleton<T> : SerializedScriptableObject where T : SerializedScriptableObject
#else
	public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
#endif
{
    #region Variables

    static T cachedInstance;

    #endregion
    


    #region Properties

	protected static string FileName
	{
		get
		{
			return typeof(T).Name;
		}
	}


	public static T Instance
	{
		get
		{
			if (cachedInstance == null)
			{
                cachedInstance = Resources.Load(FileName) as T;
			}

			#if UNITY_EDITOR
				if (cachedInstance == null)
				{
					cachedInstance = CreateAndSave();
				}
			#endif

			if (cachedInstance == null)
			{
                Debug.LogWarning("No instance of " + FileName + " found, using default values");
				cachedInstance = CreateInstance<T>();
			}

			return cachedInstance;
		}
	}   


	public static bool DoesInstanceExist
	{
        get 
        {
            if (cachedInstance == null)
            {
                cachedInstance = Resources.Load(FileName) as T;
            }

            return cachedInstance != null;
        }
	}

    #endregion
    
    
    
    #region Private methods

	#if UNITY_EDITOR
		static T CreateAndSave()
		{
			T instance = CreateInstance<T>();
	
			//Saving during Awake() will crash Unity, delay saving until next editor frame
			//Saving during Build will call PreProcessBuildAttribute
			if ((EditorApplication.isPlayingOrWillChangePlaymode) || 
				(BuildPipeline.isBuildingPlayer) || (EditorApplication.isCompiling))
			{
				EditorApplication.delayCall += () =>
				{
					instance = CreateInstance<T>();
					SaveAsset(instance); 
				};
			}
			else
			{
				SaveAsset(instance);
			}
			
			Debug.LogWarning("Create singleton scriptable object: " + typeof(T).Name);
			
			return instance;
		}
		
	
		static void SaveAsset(T obj)
		{
			string defaultAssetPath = "Assets/Resources/" + FileName + ".asset";
			string dirName = Path.GetDirectoryName(defaultAssetPath);
			if(!Directory.Exists(dirName))
			{
				Directory.CreateDirectory(dirName);
			}
			AssetDatabase.CreateAsset(obj, defaultAssetPath);
			AssetDatabase.SaveAssets();
	
			Debug.Log("Saved " + FileName + " instance");
		}
	#endif
	
	#endregion
}
