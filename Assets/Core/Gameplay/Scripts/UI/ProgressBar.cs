using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private enum Type
    {
        Accuracy,
        Bonus
    }

    [SerializeField] private Type _type;
    [SerializeField] private float _time;
    [SerializeField] private Ease _ease;
    [SerializeField] private Image _filled;

    private StatePersister _sp;

    private void Start()
    {
        _sp = StatePersister.Instance;
    }

    public void UpdateBar(Action onComplete = null)
    {
        var from = 0f;
        if (_type == Type.Bonus) from = _filled.fillAmount;
        var endValue = GetProgress(_type);
        _filled.DOFillAmount(endValue, _time).From(from).SetEase(_ease).OnComplete(() => onComplete?.Invoke());
    }

    private float GetProgress(Type type)
    {
        switch (type)
        {
            case Type.Accuracy:
                return _sp.AccuracyProgress;
            case Type.Bonus:
                return GetBonusProgressValue();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private float GetBonusProgressValue()
    {
        if (_sp.LevelNumber == 0) return 0;
        var ost = _sp.LevelNumber % _sp.BonusLevel;
        return ost == 0 ? 1 : ost / (float)_sp.BonusLevel;
    }

    public void Reset()
    {
        _filled.fillAmount = 0;
    }
}