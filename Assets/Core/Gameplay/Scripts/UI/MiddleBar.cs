using System;
using UnityEngine;

public class MiddleBar : MonoBehaviour
{
    [SerializeField] private ElementAnimated _retryBtn;
    [SerializeField] private ElementAnimated _nextBtn;

    public void Show()
    {
        _retryBtn.Show();
        _nextBtn.Show();
    }

    public void Hide(bool immediate = false, Action onComplete = null)
    {
        _retryBtn.Hide(immediate);
        _nextBtn.Hide(immediate, onComplete);
    }
}