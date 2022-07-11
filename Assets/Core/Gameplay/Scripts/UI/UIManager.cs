using System;
using DG.Tweening;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public const float UIAnimationTime = 0.5f;


    [SerializeField] private MiddleBar _middleBar;
    [SerializeField] private TopBar _topBar;
    [SerializeField] private MenuScreen _menuScreen;
    [SerializeField] private ElementAnimated _secondCameraImage;
    [SerializeField] private GameplayScreen _gameplayScreen;
    [SerializeField] private ResultScreen _resultScreen;
    //[SerializeField] private ElementAnimated _fader;


    private StatePersister _sp;


    private void Awake()
    {
        _sp = StatePersister.Instance;
    }

    private void Start()
    {
        Hide(true, () =>
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                _sp.GameState = GameManager.State.Menu;
                UpdateState();
            });
        });
    }

    private void Hide(bool immediate, Action OnComplete = null)
    {
        _secondCameraImage.Hide(immediate);
        _middleBar.Hide(immediate);
        _topBar.Hide(immediate);
        _menuScreen.Hide(immediate);
        _gameplayScreen.Hide(immediate);
        _resultScreen.Hide(immediate, OnComplete);
        //_fader.Hide(immediate);
    }


    public void UpdateState()
    {
        var state = _sp.GameState;
        switch (state)
        {
            case GameManager.State.Menu:
                //_fader.Hide();
                _middleBar.Hide();
                _resultScreen.Hide(false, () =>
                {
                    _topBar.Show();
                    _menuScreen.Show();
                });
                break;

            case GameManager.State.Gameplay:
                _menuScreen.Hide(false, () =>
                {
                    _middleBar.Show();
                    _gameplayScreen.Show();
                });
                break;

            case GameManager.State.Result:
                _gameplayScreen.Hide();
                //_fader.Show();
                _resultScreen.Show();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public void Retry()
    {
        _middleBar.Hide();
        _gameplayScreen.Hide(false, UpdateState);
    }

    public void ProcessClaimX3()
    {
        _resultScreen.ProcessClaimX3();
    }
}