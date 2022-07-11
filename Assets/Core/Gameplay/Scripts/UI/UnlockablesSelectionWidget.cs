using System;
using UnityEngine;
using UnityEngine.UI;

public class UnlockablesSelectionWidget : MonoBehaviour
{
    [SerializeField] private UnlockableView unlockableViewPrefab;
    [SerializeField] private ScrollRect unlockablesScroll;
    [SerializeField] private GameObject body;
    
    private UnlockableView[] unlockableSlotViews;
    private UnlockableView selectedUnlockable;
    
    private void OnEnable()
    {
        GameManager.OnStartInstrument += ShowStickerUnlockables;
        GameManager.OnStopInstrument += Hide;
        StickerRoll.OnStickerFinished += ShowSprayUnlockables;
        UnlockableView.OnSelected += UpdateSelection;
    }

    private void OnDisable()
    {
        GameManager.OnStartInstrument -= ShowStickerUnlockables;
        GameManager.OnStopInstrument -= Hide;
        StickerRoll.OnStickerFinished -= ShowSprayUnlockables;
        UnlockableView.OnSelected -= UpdateSelection;
    }

    private void Start()
    {
        unlockableSlotViews = new UnlockableView[UnlockablesManager.Instance.GetMaxUnlockablesCount()];

        for (int i = 0; i < unlockableSlotViews.Length; i++)
            unlockableSlotViews[i] = Instantiate(unlockableViewPrefab, unlockablesScroll.content, false);
        
        Hide();
    }

    private void ShowStickerUnlockables()
    {       
        if(LevelsConfig.IsSpray)
            ShowUnlockables(UnlockablesManager.Instance.StickerUnlockables);
    }

    private void ShowSprayUnlockables()=> ShowUnlockables(UnlockablesManager.Instance.SprayUnlockables);
    private void Hide() => body.SetActive(false);


    private void ShowUnlockables(Unlockable[] unlockables)
    {
        for (int i = 0; i < unlockableSlotViews.Length; i++)
        {
            if (i < unlockables.Length)
            {
                unlockableSlotViews[i].SetData(unlockables[i]);
                unlockableSlotViews[i].gameObject.SetActive(true);
            }
            else
                unlockableSlotViews[i].gameObject.SetActive(false);
        }
        
        body.SetActive(true);
        unlockableSlotViews[0].Select();
        unlockablesScroll.horizontalNormalizedPosition = 0;
    }

    private void UpdateSelection(UnlockableView selectedUnlockable)
    {
        if(this.selectedUnlockable!=null && this.selectedUnlockable!=selectedUnlockable)
            this.selectedUnlockable.Deselect();
        this.selectedUnlockable = selectedUnlockable;
    }
}
