using System;
using TMPro;
using UnityEngine;

public class TopBar : MonoBehaviour
{
    [SerializeField] private ElementAnimated _settingsBtn;
    [SerializeField] private ElementAnimated _moneyBlock;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private ElementAnimated _levelHeader;
    [SerializeField] private TMP_Text _levelText;


    public void Show()
    {
        _settingsBtn.Show();
        _moneyBlock.Show();
        _levelHeader.Show();

        UpdateMoneyText();
        UpdateLevelText();
    }


    public void Hide(bool immediate = false, Action onComplete = null)
    {
        _settingsBtn.Hide(immediate);
        _moneyBlock.Hide(immediate);
        _levelHeader.Hide(immediate, onComplete);
    }

    public void UpdateMoneyText()
    {
        var sp = StatePersister.Instance;
        if (sp.RewardMoney == 0)
        {
            _moneyText.text = sp.Money.ToString();
        }
        else
        {
            var to = sp.Money;
            var from = sp.Money - sp.RewardMoney;
            ElementAnimated.UpdateText(_moneyText, from, to);
        }
    }

    private void UpdateLevelText()
    {
        _levelText.text = StatePersister.Instance.LevelNumber.ToString();
    }
}