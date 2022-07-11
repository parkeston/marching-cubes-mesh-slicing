using System;
using UnityEngine;

public class GameplayScreen : MonoBehaviour
{
    [SerializeField] private ElementAnimated _circles;
    [SerializeField] private CirclesProgressBar _circlesBar;

    public void Show()
    {
        _circlesBar.UpdateBar();
        _circles.Show();
    }


    public void Hide(bool immediate = false, Action OnComplete = null)
    {
        _circles.Hide(immediate, OnComplete);
    }
}