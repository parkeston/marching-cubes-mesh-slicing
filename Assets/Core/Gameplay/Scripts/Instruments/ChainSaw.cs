using System;
using DG.Tweening;
using PathCreation.Examples;
using UnityEngine;

public class ChainSaw : InstrumentController
{
    public static event Action<Vector3,Vector3> OnMove;
    public static event Action OnMoveEnd;
    
    [Range(0, 1)] [SerializeField] private float rotationDamping;

    [Header("Chain")] [SerializeField] private Transform _teethRoot;
    [SerializeField] private float _idleTeethSpeed;
    [SerializeField] private float _teethSpeed;
    [SerializeField] private ParticleSystem _curParts;

    [Header("Red Button")] [SerializeField]
    private Transform _redButton;

    [SerializeField] private Vector3 _buttonPosOff;
    [SerializeField] private Vector3 _buttonPosOn;
    [SerializeField] private float _buttonTime;

    [SerializeField] private Transparentor _transparentor;
    [SerializeField] private GameObject slicingHint;

    private Quaternion _targetRotation;
    private PathFollower[] _teeth;

    private bool _isRotationLocked;

    private GameObject _hint;

    private void Start()
    {
        _teeth = _teethRoot.GetComponentsInChildren<PathFollower>();
    }

    private void SetHint(bool visible = true)
    {
        if (_hint != null) Destroy(_hint);
        if (visible)
        {
            _hint = Instantiate(slicingHint);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        MarchingCubes.OnSlicing += StartSlicing;
        MarchingCubes.OffSlicing += StopSlicing;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        MarchingCubes.OnSlicing -= StartSlicing;
        MarchingCubes.OffSlicing -= StopSlicing;
    }

    private void StartSlicing()
    {
        SoundManager.Instance.SawSlicing();
        _curParts.Play();
    }

    private void StopSlicing()
    {
        SoundManager.Instance.StopSawSlicing();
        _curParts.Stop();
    }

    protected override void StopInstrument()
    {
        base.StopInstrument();
        SetHint(false);
        SoundManager.Instance.ChainSawOff();
        PushButton(false, () => SetChainSpeed(0));
    }

    protected override void StartInstrument()
    {
        base.StartInstrument();
        SetHint();
        SoundManager.Instance.ChainSawOn();
        PushButton(true, () => SetChainSpeed(_idleTeethSpeed));
    }

    private void SetChainSpeed(float speed)
    {
        foreach (var tooth in _teeth)
        {
            DOTween.To(() => tooth.speed, value => tooth.speed = value, speed, 1f).SetEase(Ease.OutSine);
        }
    }

    protected override void InputStart()
    {
        base.InputStart();
        _transparentor.Fade();
        _sp.Vibrate = false;
        SetChainSpeed(_teethSpeed);
        SoundManager.Instance.StartCut();
    }

    protected override void InputEnd()
    {
        _pressed = false;
        _transparentor.Fade(false);
        _sp.Vibrate = true;
        SetChainSpeed(_idleTeethSpeed);
        SoundManager.Instance.StopCut();
        
        OnMoveEnd?.Invoke();
    }

    protected override void Move()
    {
        Vector3 desiredPosition = GetTouchProjectionPosition()+touchOffset;
        desiredPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - movementDamping);
        Vector2 targetVelocity = desiredPosition - transform.position; //instant velocity per frame time
        
        if (targetVelocity.magnitude <=0)
        {
            OnMoveEnd?.Invoke();
            return;
        }

        targetVelocity = Vector2.ClampMagnitude(targetVelocity, targetVelocity.magnitude/MarchingCubes.SlowDownFactor);
        bool isOppositeDirection = !(Vector3.Dot(targetVelocity.normalized, velocity.normalized) > -0.7f);
        velocity = targetVelocity;

        Vector3 targetPosition = transform.position;
        _isRotationLocked = _isRotationLocked
            ? !(Math.Abs(Vector3.Dot(transform.right, targetVelocity.normalized)) < 0.7f)
            : isOppositeDirection;
        
        if (!_isRotationLocked)
        {
            _targetRotation = Vector3.Angle(-transform.right, velocity.normalized) > 
                              Vector3.Angle(transform.right, velocity.normalized) 
                ? Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, velocity.normalized).normalized) 
                : Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, -velocity.normalized).normalized);
            _targetRotation = Quaternion.Lerp(transform.rotation, _targetRotation, 1 - rotationDamping);

            targetPosition += (Vector3) velocity;
            transform.SetPositionAndRotation(targetPosition, _targetRotation);
        }
        else
        {
            // Restrict instrument movement along its direction, also stop rotation

            var dir = Vector3.Dot(transform.right, velocity.normalized);
            targetPosition += transform.right* velocity.magnitude*dir;
            transform.position = targetPosition;
        }

        //slicing edge of blade from velocity direction ,0.2f - width of blade
        OnMove?.Invoke(targetPosition+transform.right * (Mathf.Sign(Vector3.Dot(transform.right.normalized,velocity.normalized)) * 0.2f),
            velocity.normalized);
    }

    private void PushButton(bool turnOn, Action callback = null)
    {
        if (turnOn)
        {
            _redButton.DOLocalMove(_buttonPosOn, _buttonTime).From(_buttonPosOff).SetEase(Ease.OutBack, 5).OnComplete(() => callback?.Invoke());
            return;
        }

        _redButton.DOLocalMove(_buttonPosOff, _buttonTime).From(_buttonPosOn).SetEase(Ease.InBack, 5).OnComplete(() => callback?.Invoke());
    }
    
}