using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static event Action OnStopInstrument;
    public static event Action OnStartInstrument;


    public enum State
    {
        Menu,
        Gameplay,
        Result
    }
    
    public enum ViewState
    {
        Middle = 0,
        Right = 1,
        Left = 2
    }


    [SerializeField] private UIManager _ui;

    [SerializeField] private CameraController _camController;

    [SerializeField] private MarchingCubes _levelShape;

    [SerializeField] private InstrumentController[] _instruments;


    private StatePersister _sp;


    private void Awake()
    {
        _sp = StatePersister.Instance;
    }

    private void Start()
    {
        ResetLevel();
        RespawnIceBox();
    }

    private void ResetLevel()
    {
        _sp.ActivityIndex = 0;
        _sp.InstrumentIndex = 0;
        //ActivateInstrument();
    }

    private void ActivateInstrument()
    {
        for (int i = 0; i < _instruments.Length; i++)
        {
            _instruments[i].gameObject.SetActive(i == _sp.InstrumentIndex);
        }
    }

    private void DeactivateInstrument(Action callback = null)
    {
        if (_sp.InstrumentIndex >= _instruments.Length) // todo: add instruments
        {
            callback?.Invoke();
            return;
        }
        OnStopInstrument?.Invoke();
        callback?.Invoke();
    }

    private void DeactivateInstruments()
    {
        foreach (var instrument in _instruments)
        {
            instrument.gameObject.SetActive(false);
        }
    }

    public void TapToPlay()
    {
        _camController.NextState();
        ActivateInstrument();
        OnStartInstrument?.Invoke();
        _sp.CanPlay = true;
        _sp.GameState = State.Gameplay;
        _ui.UpdateState();
    }

    public void Retry()
    {
        SoundManager.Instance.PlayButtonClick();
        
        _sp.GameState = State.Menu;
        DeactivateInstrument();
        DeactivateInstruments();
        _camController.IsFinalRotate = false;
        
        _sp.ActivityIndex = 0;
        _sp.InstrumentIndex = 0;
        _sp.CanPlay = false;
        // OnStopInstrument?.Invoke();
        _ui.Retry();
        _camController.ResetState();
        RespawnIceBox();
    }

    public void NextActivity()
    {
        SoundManager.Instance.PlayButtonClick();

        if (_sp.GameState == State.Result)
        {
            _camController.IsFinalRotate = false;
            _sp.GameState = State.Menu;
            _ui.UpdateState();
            RespawnIceBox();
            return;
        }


        var nextActivityIndex = _sp.ActivityIndex + 1;

        if (nextActivityIndex == LevelsConfig.Instance.activities.Length)
        {
            OnStopInstrument?.Invoke();
            _sp.ActivityIndex = 0;
            _sp.InstrumentIndex = 0;
            _sp.CanPlay = false;
            DeactivateInstruments();
            ProcessResult();
            return;
        }
        
        // Change instrument
        var index = LevelsConfig.GetInstrumentIndex(nextActivityIndex);
        if (index != _sp.InstrumentIndex)
        {
            DeactivateInstrument(() =>
            {
                DOVirtual.DelayedCall(1f, ()=>
                {
                    _sp.ActivityIndex = nextActivityIndex;
                    _sp.InstrumentIndex = index;
                    ActivateInstrument();
                    OnStartInstrument?.Invoke();
                    NextState();
                });
                
            });
            return;
        }

        _sp.ActivityIndex = nextActivityIndex;
        NextState();
    }

    private void NextState()
    {
        _camController.NextState();
        _ui.UpdateState();
    }

    private void RespawnIceBox()
    {
        _levelShape.RespawnShape();
        _camController.ResetState();
    }

    private void ProcessResult()
    {
        _sp.LevelNumber++;

        _sp.RewardMoney = Random.Range(100, 300);
        _sp.AccuracyProgress = _levelShape.GetProgress();
        _sp.Money += _sp.RewardMoney;

        _sp.GameState = State.Result;
        UnlockablesManager.Instance.UpdateState();
        _camController.ResetState();
        StartCoroutine(UpdateResultScreenUI(1));
    }

    private IEnumerator UpdateResultScreenUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        _camController.IsFinalRotate = true;
        _ui.UpdateState();
    }

    public void ClaimX3()
    {
        SoundManager.Instance.PlayButtonClick();

        _sp.RewardMoney *= 2;
        _sp.Money += _sp.RewardMoney;
        _ui.ProcessClaimX3();
    }
}