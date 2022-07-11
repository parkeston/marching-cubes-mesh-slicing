using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class MenuScreen : MonoBehaviour
{
    [SerializeField] private ElementAnimated _progressBarElem;
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private ElementAnimated _tapToPlay;
    [SerializeField] private BonusPopup _bonusPopup;


    public void Show()
    {
        _progressBarElem.Show(() => _progressBar.UpdateBar(CheckBonus));
        DOVirtual.DelayedCall(UIManager.UIAnimationTime, () => _tapToPlay.Show());
    }

    public void Hide(bool immediate, Action OnComplete = null)
    {
        _bonusPopup.Hide(immediate);
        _progressBarElem.Hide(immediate);
        _tapToPlay.Hide(immediate, OnComplete);
    }

    private void CheckBonus()
    {
        var sp = StatePersister.Instance;
        if (sp.LevelNumber > 0 && sp.LevelNumber % sp.BonusLevel != 0) return;
        if (sp.BonusShowedLevel == sp.LevelNumber) return;

        sp.RewardMoney = Random.Range(200, 500);
        sp.Money += sp.RewardMoney;
        
        _bonusPopup.Show();
        _progressBar.Reset();
        sp.BonusShowedLevel = sp.LevelNumber;
    }
}