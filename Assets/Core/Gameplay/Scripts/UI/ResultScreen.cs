using System;
using TMPro;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    [SerializeField] private ElementAnimated _accuracyProgressBar;
    [SerializeField] private ProgressBar _accuracyProgress;
    [SerializeField] private ElementAnimated _money;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private ElementAnimated _claimButton;

    private StatePersister _sp;

    private void Start()
    {
        _sp = StatePersister.Instance;
    }

    public void Show()
    {
        _accuracyProgress.Reset();
        _accuracyProgressBar.Show(() =>
        {
            UpdateTextAnimated();
            _accuracyProgress.UpdateBar();
        });
        _money.Show();
        _claimButton.Show();
    }

    public void Hide(bool immediate, Action onComplete)
    {
        _accuracyProgressBar.Hide(immediate);
        _money.Hide(immediate);
        _claimButton.Hide(immediate, onComplete);
    }

    private void UpdateTextAnimated()
    {
        ElementAnimated.UpdateText(_moneyText, 0, _sp.RewardMoney, "$ {0}");
    }

    public void ProcessClaimX3()
    {
        _claimButton.Hide();
        UpdateTextAnimated();
    }
}