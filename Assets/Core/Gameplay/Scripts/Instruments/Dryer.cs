using System;
using UnityEngine;

public class Dryer : InstrumentController
{
    public static event Action OnDryer;
    
    [Space]
    [SerializeField] private Transform _target;
    [SerializeField] private ParticleSystem _curParts;

    protected override void InputStart()
    {
        base.InputStart();
        SoundManager.Instance.DryerOn();
        _curParts.Play();
    }

    protected override void InputEnd()
    {
        base.InputEnd();
        SoundManager.Instance.DryerOff();
        _curParts.Stop();
    }

    protected override void Move()
    {
        transform.LookAt(_target);
        base.Move(); 
        OnDryer?.Invoke();
    }
}