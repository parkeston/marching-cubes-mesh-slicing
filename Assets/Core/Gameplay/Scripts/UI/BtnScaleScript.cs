using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BtnScaleScript : MonoBehaviour
{
    [SerializeField] private Transform[] _objectsToScale;
    [SerializeField] private float _scaleOffset = 0.95f;
    [SerializeField] private Button _refBtn;

    [Header("Pulse")] public bool animatePulse;

    [SerializeField] private Transform _objectToAnimate;
    [SerializeField] private float _maxScale = 1.2f;
    [SerializeField] private float _minScale = 0.95f;
    [SerializeField] private float _normalScale = 1f;
    [SerializeField] private float _pauseTime = 1f;
    [SerializeField] private Image _bgImage;
    [SerializeField] private Transform[] _objectsToAnimate;

    private EventTrigger _trigger;
    private Button _btn;
    private bool _pressed;
    private Sequence _pulseSequence;
    private Sequence _pulseSequenceInner;
    private float[] _objectsDefScale;
    private Color _bgStartColor;
    private Color _bgPulseColor;
    public bool IsIgnoreRelease;
    public Ease ReleaseEase = Ease.OutElastic;
    private bool _isBGColorInited;
    private float _startScale;

    private bool _inited;

    private void Awake()
    {
        LazyInit();
    }

    private void LazyInit()
    {
        if (_inited) return;
        _inited = true;

        if (_objectsToScale != null && _objectsToScale.Length > 0)
        {
            _objectsDefScale = new float[_objectsToScale.Length];
            for (int i = 0; i < _objectsDefScale.Length; i++)
            {
                _objectsDefScale[i] = _objectsToScale[i].localScale.x;
            }
        }

        SaveBGColor();
        StartInit();
        _startScale = transform.localScale.x;
    }

    private Button Btn
    {
        get
        {
            if (_btn == null)
            {
                if (_refBtn != null)
                {
                    _btn = _refBtn;
                }
                else if (GetComponent<Button>() != null)
                {
                    _btn = GetComponent<Button>();
                }

                LazyInit();
            }

            return _btn;
        }
    }

    private void StartInit()
    {
        var trigger = Btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = Btn.gameObject.AddComponent<EventTrigger>();
        }

        AddEventTriggerListener(trigger, EventTriggerType.PointerDown, data => { OnPointerDown(); });
        AddEventTriggerListener(trigger, EventTriggerType.PointerUp, data => { OnPointerUp(); });
        AddEventTriggerListener(trigger, EventTriggerType.PointerExit, data => { OnPointerUp(); });
    }

    private void SaveBGColor()
    {
        if (_bgImage != null)
        {
            _bgStartColor = _bgImage.color;

            Color.RGBToHSV(_bgStartColor, out float h, out float s, out float v);
            h *= 0.7f;
            _bgPulseColor = Color.HSVToRGB(h, s, v);
            _isBGColorInited = true;
        }
    }

    private void OnEnable()
    {
        if (animatePulse)
            StartAnimatingPulse();
    }

    private void OnDisable()
    {
        if (_pulseSequence != null)
        {
            _pulseSequence.Kill();

            if (_objectToAnimate != null)
            {
                _objectToAnimate.localScale = new Vector3(_normalScale, _normalScale);
            }
        }

        if (_pulseSequenceInner != null)
        {
            if (_objectsToAnimate != null)
            {
                foreach (var obj in _objectsToAnimate)
                {
                    obj.DOKill();
                    obj.rotation = Quaternion.identity;
                }
            }
        }
    }

    private static void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType, callback = new EventTrigger.TriggerEvent()
        };
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
        trigger.triggers.Add(entry);
    }

    #region BTN EVENTS

    private void OnPointerDown()
    {
        StopAnimatingPulse();
        _pressed = true;
        PressObjects();
    }

    private void OnPointerUp()
    {
        if (IsIgnoreRelease) return;
        if (_pressed)
        {
            ReleaseObjects(() => { _pressed = false; });
        }
    }

    public void OnPointerEnter()
    {
        StopAnimatingPulse();
        if (_pressed)
        {
            PressObjects();
        }
    }

    #endregion

    #region BTN SCALE

    private bool _ignoreBtn;

    private void PressObjects()
    {
        if (!Btn.interactable) return;
        // if (_ignoreBtn) return;
        // _ignoreBtn = true;

        // SoundManager.Instance.PlayButtonClick();

        for (int i = 0; i < _objectsToScale.Length; i++)
        {
            _objectsToScale[i].DOKill();
            _objectsToScale[i].DOScale(_objectsDefScale[i] * _scaleOffset, 0.2f);
        }
    }

    private IEnumerator Wait()
    {
        Btn.enabled = false;
        yield return new WaitForSeconds(0.5f);
        Btn.enabled = true;
    }

    private void ReleaseObjects(Action callback)
    {
        for (int i = 0; i < _objectsToScale.Length; i++)
            _objectsToScale[i].DOScale(_objectsDefScale[i], 0.4f).SetEase(ReleaseEase, 2).OnComplete(() =>
            {
                if (animatePulse && enabled)
                    StartAnimatingPulse();

                callback?.Invoke();
            });
    }

    #endregion

    #region PUBLIC METHODS

    public void SetInteractive(bool interactable)
    {
        Btn.interactable = interactable;
    }

    public void SetImageSprite(Sprite sprite)
    {
        Btn.image.sprite = sprite;
        Btn.image.SetNativeSize();
    }

    public void Appear(bool isInteractable = true)
    {
        LazyInit();

        Btn.interactable = isInteractable;
        IsIgnoreRelease = false;
        gameObject.SetActive(true);
        transform.DOKill();
        transform.DOScale(_startScale, 0.4f).From(0.2f).SetEase(Ease.OutBack).OnComplete(() => { StartAnimatingPulse(); });
    }

    public void Disappear(Action callback = null, bool forced = false)
    {
        LazyInit();

        IsIgnoreRelease = true;
        Btn.interactable = false;
        StopAnimatingPulse();
        transform.DOKill();
        transform.DOScale(0f, forced ? 0 : 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);

            callback?.Invoke();
        });
    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }

    private void StartAnimatingPulse()
    {
        LazyInit();

        if (!animatePulse) return;

        if (_objectToAnimate != null)
        {
            if (!_isBGColorInited)
            {
                SaveBGColor();
            }

            _pulseSequence.Kill(true);
            _pulseSequence = DOTween.Sequence();

            _objectToAnimate.localScale = new Vector3(_normalScale, _normalScale);

            _pulseSequence.AppendInterval(_pauseTime);
            _pulseSequence.Append(_objectToAnimate.DOScale(_maxScale, 0.2f).From(_normalScale).SetEase(Ease.OutSine));
            _pulseSequence.Append(_objectToAnimate.DOScale(_minScale, 0.1f).SetEase(Ease.OutSine));
            _pulseSequence.Append(_objectToAnimate.DOScale(_normalScale, 0.2f).SetEase(Ease.OutSine));

            _pulseSequence.Insert(0, _bgImage?.DOColor(_bgPulseColor, 0.2f).SetEase(Ease.OutSine));
            _pulseSequence.Insert(0.3f, _bgImage?.DOColor(_bgStartColor, 0.2f).SetEase(Ease.OutSine));

            _pulseSequence.SetLoops(-1);
        }

        if (_objectsToAnimate != null)
        {
            // animate inner objects
            _pulseSequenceInner.Kill();
            _pulseSequenceInner = DOTween.Sequence();


            foreach (var obj in _objectsToAnimate)
            {
                obj.rotation = Quaternion.identity;
                _pulseSequenceInner.Append(obj.DORotate(Vector3.forward * -2.5f, 0.3f).SetEase(Ease.Linear));
                _pulseSequenceInner.Append(obj.DORotate(Vector3.zero, 0.3f).SetEase(Ease.Linear));
                _pulseSequenceInner.Append(obj.DORotate(Vector3.forward * 2.5f, 0.3f).SetEase(Ease.Linear));
                _pulseSequenceInner.Append(obj.DORotate(Vector3.zero, 0.3f).SetEase(Ease.Linear));
            }

            _pulseSequenceInner.SetLoops(-1);
        }
    }

    private void StopAnimatingPulse(Action callback = null)
    {
        LazyInit();

        if (!animatePulse) return;

        if (_objectToAnimate != null)
        {
            _pulseSequence?.Kill();
            _objectToAnimate.DOScale(new Vector3(_normalScale, _normalScale), 0).OnComplete(() => callback?.Invoke());
        }

        if (_objectsToAnimate != null)
        {
            _pulseSequenceInner?.Kill();
            foreach (var obj in _objectsToAnimate)
            {
                obj.DORotate(Vector3.zero, 0);
            }
        }
    }

    #endregion
}