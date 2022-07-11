using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementAnimated : MonoBehaviour
{
    [SerializeField] private Vector3 _hidePos;
    [SerializeField] private bool _scaleAnimated;
    [SerializeField] private bool _bounceScale;
    [SerializeField] private bool _fadeAnimated;
    [SerializeField] private float _speedKoeff = 1f;
    [SerializeField] private float _alpha = 0.5f;

    private bool _active;
    private RectTransform _rt;
    private Vector3 _showPos;
    private Vector3 _showScale;
    private Image[] _images;
    private float _time;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _showPos = _rt.anchoredPosition;
        _showScale = _rt.localScale;
        _active = gameObject.activeInHierarchy;
        _images = _rt.GetComponents<Image>();
        _time = UIManager.UIAnimationTime;
    }

    public void Show(Action onComplete)
    {
        Show(false, onComplete);
    }
    
    public void Show(bool immediate = false, Action onComplete = null)
    {
        if (_active) return;
        _active = true;
        gameObject.SetActive(true);

        _rt.DOKill();

        // scale

        if (_scaleAnimated)
        {
            if (immediate)
            {
                _rt.localScale = _showScale;
                AfterShow(onComplete);
                return;
            }

            if (_bounceScale)
            {
                _rt.DOScale(_showScale, _time * _speedKoeff).From(0f).SetEase(Ease.OutElastic, Single.MinValue).OnComplete(() => AfterShow(onComplete));
            }
            else
            {
                _rt.DOScale(_showScale, _time * _speedKoeff).From(0f).SetEase(Ease.OutBack).OnComplete(() => AfterShow(onComplete));
            }
           
            return;
        }

        // fade

        if (_fadeAnimated)
        {
            foreach (var img in _images)
            {
                img.DOKill();
                if (immediate)
                {
                    img.SetAlpha(_alpha);
                    continue;
                }

                img.DOFade(_alpha, _time * _speedKoeff).SetEase(Ease.OutSine).OnComplete(() => AfterShow(onComplete));
            }

            if (immediate)
            {
                AfterShow(onComplete);
            }

            return;
        }

        // position

        if (immediate)
        {
            _rt.anchoredPosition = _showPos;
            AfterShow(onComplete);
            return;
        }

        _rt.DOAnchorPos(_showPos, _time * _speedKoeff).From(_hidePos).SetEase(Ease.OutBack).OnComplete(() => AfterShow(onComplete));
    }

    private void AfterShow(Action onComplete)
    {
        onComplete?.Invoke();
    }

    public void Hide(bool immediate = false, Action onComplete = null)
    {
        if (!_active)
        {
            onComplete?.Invoke();
            return;
        }

        var hideTime = _time * 0.5f;

        _rt.DOKill();

        // scale

        if (_scaleAnimated)
        {
            if (immediate)
            {
                _rt.localScale = Vector3.zero;
                AfterHide(onComplete);
            }
            else
            {
                _rt.DOScale(0f, hideTime).SetEase(Ease.InSine).OnComplete(() =>
                {
                    _rt.localScale = Vector3.zero;
                    AfterHide(onComplete);
                });
            }

            return;
        }

        // fade

        if (_fadeAnimated)
        {
            foreach (var img in _images)
            {
                img.DOKill();
                if (immediate)
                {
                    img.SetAlpha(0);
                    continue;
                }

                img.DOFade(0, hideTime).SetEase(Ease.InSine).OnComplete(() => { AfterHide(onComplete); });
            }

            if (immediate)
            {
                AfterHide(onComplete);
            }

            return;
        }

        // position

        if (immediate)
        {
            _rt.anchoredPosition = _hidePos;
            AfterHide(onComplete);
        }
        else
        {
            _rt.DOAnchorPos(_hidePos, hideTime).From(_showPos).SetEase(Ease.InSine).OnComplete(() => AfterHide(onComplete));
        }
    }

    private void AfterHide(Action onComplete)
    {
        _active = false;
        onComplete?.Invoke();
    }

    public static void UpdateText(TMP_Text text, int from, int to, string pattern = null)
    {
        var sp = StatePersister.Instance;
        if (sp.RewardMoney == 0) return;
        DOTween.To(() => from, x =>
        {
            from = x;
            text.text = string.Format(pattern ?? "{0}", from.ToString());
        }, to, 1.5f).SetEase(Ease.OutExpo);
    }
}