using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstrumentController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0, 1)] [SerializeField] protected float movementDamping;
    [SerializeField] protected float extraMovementPlaneDistance=0f;

    protected Vector3 velocity;
    protected Vector3 touchOffset;
    protected bool _pressed;

    private static int _uILayer;
    
    private InstrumentsConfig _cfg;
    protected StatePersister _sp;
    
    protected void Awake()
    {
        _uILayer = SortingLayer.NameToID("UI");
        _cfg = InstrumentsConfig.Instance;
        _sp = StatePersister.Instance;
    }
    
    protected virtual void OnEnable()
    {
        GameManager.OnStartInstrument += StartInstrument;
        GameManager.OnStopInstrument += StopInstrument;
        CameraController.OnChangeCameraState += OnChangeCameraState;
    }

    protected virtual void OnDisable()
    {
        GameManager.OnStartInstrument -= StartInstrument;
        GameManager.OnStopInstrument -= StopInstrument;
        CameraController.OnChangeCameraState -= OnChangeCameraState;
    }

    protected virtual void StartInstrument()
    {
        _sp.Vibrate = true;
        InstrumentStartPos();
    }

    protected virtual void StopInstrument()
    {
        _sp.Vibrate = false;
        InstrumentHidePos();
    }

    protected virtual void OnChangeCameraState()
    {
        InstrumentStartPos();
    }
    

    protected bool IsTouchUI()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        return raycastResults.Count > 0 && raycastResults.Any(go => go.sortingLayer.CompareTo(_uILayer) == 0);
    }
    
    protected void InstrumentIdlePos(bool immediate = false)
    {
        transform.DOKill();
        var idlePos = _cfg.GetIdlePos();
        if (immediate)
        {
            transform.SetPositionAndRotation(idlePos, Quaternion.Euler(_cfg.GetIdleRot()));
            return;
        }
        
        if (LevelsConfig.IsDryer)
        {
            transform.DOMove(idlePos, 0.25f).SetEase(Ease.InSine);
            transform.DORotate(_cfg.GetIdleRot(), 0.25f).SetEase(Ease.InSine);
        }
        else
        {
            transform.DOMoveZ(idlePos.z, 0.2f).OnComplete(() => transform.DOMove(idlePos, 0.3f).SetEase(Ease.InSine));
            transform.DORotate(_cfg.GetIdleRot(), 0.5f).SetEase(Ease.InSine);
        }
        
    }

    protected void InstrumentStartPos()
    {
        transform.DOKill();

        if (LevelsConfig.IsDryer)
        {
            transform.DOMove(_cfg.GetStartPos(), 0.25f).From(_cfg.GetIdlePos()).SetEase(Ease.OutSine);
        }
        else
        {
            transform.DORotate(_cfg.GetStartRot(), 0.5f).From(_cfg.GetIdleRot()).SetEase(Ease.OutSine);
            transform.DOMove(_cfg.GetStartPos(), 0.5f).From(_cfg.GetIdlePos()).SetEase(Ease.OutSine);
        }
    }

    protected void InstrumentHidePos()
    {
        transform.DOKill();
        transform.DOMove(_cfg.GetHidePos(), 0.25f).SetEase(Ease.OutSine);
        transform.DORotate(Vector3.zero, 0.25f).SetEase(Ease.OutSine);
    }

    protected virtual void Update()
    {
        if (!StatePersister.Instance.CanPlay) return;

        if (Input.GetMouseButtonDown(0) && !IsTouchUI())
            InputStart();
        
        if (Input.GetMouseButtonUp(0) && !IsTouchUI())
            InputEnd();
        
        if (!_pressed) return;
        
        if (Input.GetMouseButton(0))
            Move();
    }

    protected virtual void InputStart()
    {
        _pressed = true;
        touchOffset = transform.position - GetTouchProjectionPosition();
    }

    protected virtual void InputEnd()
    {
        _pressed = false;
        velocity = Vector3.zero;
    }

    protected virtual void Move()
    {
        Vector3 desiredPosition = GetTouchProjectionPosition()+touchOffset;
        desiredPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - movementDamping);
        Vector2 targetVelocity = desiredPosition - transform.position; //instant velocity per frame time
        velocity = targetVelocity;
        
        transform.position += velocity;
    }
    
    
    //reworked to be independent from camera view angle changing at runtime affecting controls
    //less precise, todo: fix
    protected Vector3 GetTouchProjectionPosition()
    {
        float distance = (transform.position.z - Camera.main.transform.position.z)+extraMovementPlaneDistance;
        var frustumHeight = 2.0f * distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * Camera.main.aspect;
        
        Vector3 pos = Vector2.Scale(Input.mousePosition, new Vector2(frustumWidth / Screen.width, frustumHeight / Screen.height))
            - new Vector2(frustumWidth/2f,frustumHeight/2f);
        pos.z = transform.position.z;
        return pos;
    }
}