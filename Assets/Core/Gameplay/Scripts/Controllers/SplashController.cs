using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [SerializeField] private float _delay;

    private IEnumerator Start()
    {
        var time = Time.realtimeSinceStartup;
        var async = SceneManager.LoadSceneAsync("_IC/Scenes/Main");
        async.allowSceneActivation = false;
        yield return new WaitWhile(() => async.progress < 0.9f || Time.realtimeSinceStartup - time < _delay);
        async.allowSceneActivation = true;
    }
}