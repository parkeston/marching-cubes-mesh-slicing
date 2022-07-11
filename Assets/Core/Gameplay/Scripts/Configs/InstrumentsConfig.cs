using System;
using UnityEngine;
using static GameManager;

[CreateAssetMenu(fileName = "InstrumentsConfig")]
public class InstrumentsConfig : ScriptableSingleton<InstrumentsConfig>
{
    [Header("ChainSaw camera")]
    public Vector3 csCameraLeftPos;
    public Vector3 csCameraLeftRot;
    public Vector3 csCameraRightPos;
    public Vector3 csCameraRightRot;
    public Vector3 csCameraMiddlePos;
    public Vector3 csCameraMiddleRot;

    [Header("ChainSaw places")]
    public Vector3 csLeftIdlePos;
    public Vector3 csRightIdlePos;
    public Vector3 csMiddleIdlePos;

    public Vector3 csLeftIdleRot;
    public Vector3 csRightIdleRot;

    public Vector3 csLeftStartPos;
    public Vector3 csRightStartPos;

    public Vector3 csLeftStartRot;
    public Vector3 csRightStartRot;

    
    [Header("Dryer places")]
    public Vector3 drHidePos;
    public Vector3 drIdlePos;
    public Vector3 drIdleRot;
    public Vector3 drStartPos;
    public Vector3 drStartRot;
    
    
    // Selective properties methods

    public Vector3 GetStartRot()
    {
        switch (StatePersister.Instance.ViewState)
        {
            case ViewState.Middle:
                return LevelsConfig.IsDryer ? drStartRot : Vector3.zero;
            case ViewState.Right:
                return csRightStartRot;
            case ViewState.Left:
                return csLeftStartRot;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Vector3 GetIdlePos()
    {
        switch (StatePersister.Instance.ViewState)
        {
            case ViewState.Middle:
                return LevelsConfig.IsDryer ? drIdlePos : csMiddleIdlePos;
            case ViewState.Right:
                return csRightIdlePos;
            case ViewState.Left:
                return csLeftIdlePos;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Vector3 GetStartPos()
    {
        switch (StatePersister.Instance.ViewState)
        {
            case ViewState.Middle:
                return LevelsConfig.IsDryer ? drStartPos : csMiddleIdlePos;
            case ViewState.Right:
                return csRightStartPos;
            case ViewState.Left:
                return csLeftStartPos;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Vector3 GetIdleRot()
    {
        switch (StatePersister.Instance.ViewState)
        {
            case ViewState.Middle:
                return LevelsConfig.IsDryer ? drIdleRot : Vector3.zero;
            case ViewState.Right:
                return csRightIdleRot;
            case ViewState.Left:
                return csLeftIdleRot;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public Vector3 GetHidePos()
    {
        if (LevelsConfig.IsDryer) return drHidePos;
        return csMiddleIdlePos;
    }
}