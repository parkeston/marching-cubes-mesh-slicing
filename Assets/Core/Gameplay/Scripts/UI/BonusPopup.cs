using System;
using TMPro;
using UnityEngine;

public class BonusPopup : MonoBehaviour
{
    [SerializeField] private ElementAnimated _content;
    [SerializeField] private ElementAnimated _bonusTextElem;
    [SerializeField] private TMP_Text _bonusText;
    [SerializeField] private ElementAnimated _claimBtnElem;
    [SerializeField] private GameObject _secondCamera;
    [SerializeField] private ElementAnimated _secondCameraImage;
    [SerializeField] private ElementAnimated _closeBtnElem;
    [SerializeField] private TopBar _topBar;

    public void Show()
    {
        _content.Show();
        _bonusTextElem.Hide(true);
        _secondCamera.SetActive(true);
        _secondCameraImage.Show();
        _closeBtnElem.Hide(true);
        _claimBtnElem.Show(true);
    }

    public void Hide(bool immediate, Action onComplete = null)
    {
        _bonusTextElem.Hide(immediate);
        _claimBtnElem.Hide(immediate);
        _content.Hide(immediate);
        _secondCameraImage.Hide(immediate, () => _secondCamera.SetActive(false));
        _closeBtnElem.Hide(immediate, () => _content.Hide(immediate, onComplete));
    }

    public void SwitchToReward()
    {
        SoundManager.Instance.PlayButtonClick();
        _secondCameraImage.Hide(false, () => _bonusTextElem.Show(false, () => { ElementAnimated.UpdateText(_bonusText, 0, StatePersister.Instance.RewardMoney, "$ {0}"); }));
        _claimBtnElem.Hide(false, () => _closeBtnElem.Show());
    }

    public void Close()
    {
        SoundManager.Instance.PlayButtonClick();
        UpdateBonusText();
        Hide(false, _topBar.UpdateMoneyText);
    }

    private void UpdateBonusText()
    {
        _bonusText.text = 0.ToString();
    }
}