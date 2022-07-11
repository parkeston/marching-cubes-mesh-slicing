using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnlockableView : MonoBehaviour
{
    public static event Action<UnlockableView> OnSelected;

    [SerializeField] private Button button;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color normalColor;
    
    [SerializeField] private Image icon;
    [SerializeField] private GameObject lockLayer;
    
    [Header("Lock Condition Icons")]
    [SerializeField] private GameObject levelLockIcon;
    [SerializeField] private GameObject moneyLockIcon;
    [SerializeField] private GameObject rewardLockIcon;

    public void SetData(Unlockable unlockable)
    {
        if (unlockable.Texture != null)
        {
            int width = unlockable.Texture.width;
            int height = unlockable.Texture.height;
            icon.sprite = Sprite.Create(unlockable.Texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
        }
        else
            icon.sprite = null;
        icon.color = unlockable.Color;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(()=>OnClicked(unlockable));
        UpdateLockState(unlockable);
        Deselect();
    }

    public void Select()
    {
        button.onClick.Invoke();
    }

    public void Deselect()
    {
        button.targetGraphic.color = normalColor;
    }

    private void OnClicked(Unlockable unlockable)
    {
        if (unlockable.TryToActivate())
        {
            UpdateLockState(unlockable);
            button.targetGraphic.color = selectedColor;
            OnSelected?.Invoke(this);
        }
    }

    private void UpdateLockState(Unlockable unlockable)
    {
        lockLayer.SetActive(!unlockable.IsUnlocked);
        if(unlockable.IsUnlocked)
            return;

        if (unlockable.LevelUnlock > StatePersister.Instance.LevelNumber)
        {
            levelLockIcon.SetActive(true);
            moneyLockIcon.SetActive(false);
            rewardLockIcon.SetActive(false);
        }
        else if (unlockable.RewardUnlock)
        {
            levelLockIcon.SetActive(false);
            moneyLockIcon.SetActive(false);
            rewardLockIcon.SetActive(true);
        }
        else if (unlockable.MoneyUnlock>0)
        {
            levelLockIcon.SetActive(false);
            moneyLockIcon.SetActive(true);
            rewardLockIcon.SetActive(false);
        }
    }
}
