using System;
using DG.Tweening;
using UnityEngine;
using static GameManager;

public class CameraController : MonoBehaviour
{
    public static event Action OnChangeCameraState;

    [SerializeField] private Transform _target;
    [SerializeField] private Transform chainsaw;

    [SerializeField] private AnimationCurve positionCurve;
    [SerializeField] private AnimationCurve rotationCurve;
    
    [SerializeField] private Vector3 minCameraPos;
    [SerializeField] private Vector3 maxCameraPos;
    [SerializeField] private Vector3 cameraSpeed;
    [SerializeField] private float _resetStateDuration;

    [Header("Positions")] [SerializeField] private Transform _camTr;
    [SerializeField] private float _time;
    [SerializeField] private Ease _ease;

    [SerializeField]
    private float _finalRotateSpeed;

    private Vector3 _prevMousePos;
    private Quaternion _defaultRot;
    private Vector3 _defaultPos;
    private InstrumentsConfig _cfg;
    private StatePersister _sp;


    private void Awake()
    {
        _sp = StatePersister.Instance;
        _cfg = InstrumentsConfig.Instance;
        _defaultRot = transform.rotation;
        _defaultPos = transform.position;
    }
    
    public bool IsFinalRotate;

    private void LateUpdate()
    {
        if (IsFinalRotate)
        {
            DoFinalRotate();
            return;
        }
        
        if(!_sp.CanPlay)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            _prevMousePos = Input.mousePosition;
            return;
        }

        if (Input.GetMouseButtonUp(0))
            return;

        // Apply instrument

        Vector3 deltaPos = Vector3.zero;
        if (Input.GetMouseButton(0))
        {
            if(!LevelsConfig.IsDryer)
                FollowChainsaw();
            else
            {
                deltaPos = Input.mousePosition - _prevMousePos;
                _prevMousePos = Input.mousePosition;
                if (deltaPos.magnitude < 0.02f) return;

                deltaPos = Vector3.Scale(deltaPos,new Vector3(1f/Screen.width,1f/Screen.height,1));

                Vector3 targetPosition = transform.position+Vector3.Scale(deltaPos,cameraSpeed);
                targetPosition.x = Mathf.Clamp(targetPosition.x, minCameraPos.x, maxCameraPos.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minCameraPos.y, maxCameraPos.y);
                transform.position = targetPosition;
                transform.LookAt(_target);
            }
        }
    }

    private void FollowChainsaw()
    {
        Vector3 chainsawPos = chainsaw.transform.position;
        chainsawPos.x = Mathf.Clamp(chainsawPos.x, _cfg.csLeftStartPos.x, _cfg.csRightStartPos.x);
        chainsawPos.y = Mathf.Clamp(chainsawPos.y, -5f, 5f);
            
        Vector3 distance = (chainsawPos - _cfg.csLeftStartPos);
        distance.y = 0; 
        Vector3 totalDistance = (_cfg.csRightStartPos - _cfg.csLeftStartPos);

        float t = (distance.magnitude / totalDistance.magnitude);
            
        Quaternion targetRotation = Quaternion.Lerp(Quaternion.Euler(_cfg.csCameraLeftRot), Quaternion.Euler(_cfg.csCameraRightRot), rotationCurve.Evaluate(t));
        Vector3 targetPos = Vector3.Lerp(_cfg.csCameraLeftPos, _cfg.csCameraRightPos,  positionCurve.Evaluate(t));
            
        targetPos.y = Mathf.Lerp(minCameraPos.y, maxCameraPos.y, (chainsawPos.y + 5f) / (10f));
        Vector3 yLookRotation = _target.position - targetPos;
        yLookRotation.x = 0;
        targetRotation*=Quaternion.LookRotation(yLookRotation);

        transform.position = Vector3.Lerp(transform.position, targetPos, 0.02f);
        transform.rotation = Quaternion.Lerp(transform.rotation,targetRotation,0.02f);
    }

    private void DoFinalRotate()
    {
        transform.RotateAround(_target.position, Vector3.up, Time.deltaTime * _finalRotateSpeed);
    }

    public void ResetState()
    {
        _sp.ViewState = ViewState.Middle;
        UpdateState();
    }

    public void NextState()
    {
        _sp.ViewState = LevelsConfig.Instance.activities[_sp.ActivityIndex].viewState;
        UpdateState();
    }

    private void UpdateState()
    {
        _camTr.DOKill();

        switch (_sp.ViewState)
        {
            case ViewState.Middle:
                _camTr.DOLocalMove(_cfg.csCameraMiddlePos, _time).SetEase(_ease);
                _camTr.DOLocalRotate(_cfg.csCameraMiddleRot, _time).SetEase(_ease);
                break;
            case ViewState.Right:
                _camTr.DOLocalMove(_cfg.csCameraRightPos, _time).SetEase(_ease);
                _camTr.DOLocalRotate(_cfg.csCameraRightRot, _time).SetEase(_ease);
                break;
            case ViewState.Left:
                _camTr.DOLocalMove(_cfg.csCameraLeftPos, _time).SetEase(_ease);
                _camTr.DOLocalRotate(_cfg.csCameraLeftRot, _time).SetEase(_ease);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        OnChangeCameraState?.Invoke();
    }

}