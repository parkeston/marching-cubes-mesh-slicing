using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MenuScript : MonoBehaviour
{
    private const string SPLASH_SCENE = "Assets/_IC/Scenes/Splash.unity";

    private const string GAME_SCENE = "Assets/_IC/Scenes/Main.unity";

    [MenuItem("Scenes/Splash - RUN #_1")]
    public static void StartSceneRun()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(GAME_SCENE);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Scenes/Game - Edit #_2")]
    public static void StartSceneEdit()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(GAME_SCENE);
    }

   
}