using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu]
public class Unlockable : ScriptableObject
{
    public static event Action<Color, Texture2D> OnUnlockableActivated;
    
    [Header("Content")]
    [SerializeField] private Texture2D texture;
    [SerializeField] private Color color;

    [Header("Unlock conditions")]
    [SerializeField] private bool rewardUnlock;
    [SerializeField] private int levelUnlock;
    [SerializeField] [ConditionalHide("rewardUnlock",Inverse = true)] private int moneyUnlock;

    [Header("Debug")]
    [SerializeField] private bool isUnlocked;
    
    public Texture2D Texture => texture;
    public Color Color => color;
    
    public bool RewardUnlock => rewardUnlock;
    public int LevelUnlock => levelUnlock;
    public int MoneyUnlock => moneyUnlock;
    
    public bool IsUnlocked { get=>isUnlocked; set => isUnlocked=value; }

    public bool TryToActivate()
    {
        if (!IsUnlocked)
        {
            if (LevelUnlock > StatePersister.Instance.LevelNumber)
                return false;
            
            if (RewardUnlock)
            {
                //show reward ad, on complete set as unlocked and set shader properties
                isUnlocked = true;
            }
            else if (MoneyUnlock <= StatePersister.Instance.Money)
            {
                StatePersister.Instance.Money -= MoneyUnlock;
                isUnlocked = true;
            }
        }

        if (IsUnlocked)
        {
            if(Texture!=null) Shader.SetGlobalTexture("_UnlockableTexture", Texture);
            Shader.SetGlobalColor("_UnlockableColor", Color);
            OnUnlockableActivated?.Invoke(Color,Texture);
        }
        return isUnlocked;
    }

    public void UpdateState()
    {
        if(IsUnlocked)
            return;
        
        if (LevelUnlock > StatePersister.Instance.LevelNumber)
            isUnlocked = false;
        else if (RewardUnlock)
            isUnlocked = false;
        else if (MoneyUnlock >0)
            isUnlocked = false;
        else
            isUnlocked = true;
    }
}
