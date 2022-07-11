using System;
using UnityEngine;

public class UnlockablesManager : Singleton<UnlockablesManager>
{
    private const string SaveFileName = "unlockables";
    
    public Unlockable[] StickerUnlockables { get; private set; }
    public Unlockable[] SprayUnlockables { get; private set; }


    private void Awake()
    {
        StickerUnlockables = Resources.LoadAll<Unlockable>("Unlockables/Stickers/");
        SprayUnlockables = Resources.LoadAll<Unlockable>("Unlockables/Sprays/");
        
        if (DataSaver.LoadFile(SaveFileName, out  UnlockablesStateSerialized unlockablesStateSerialized))
        {
            for (int i = 0; i < StickerUnlockables.Length; i++)
                StickerUnlockables[i].IsUnlocked = unlockablesStateSerialized.stickerUnlockables[i];
            
            for (int i = 0; i < SprayUnlockables.Length; i++)
                SprayUnlockables[i].IsUnlocked = unlockablesStateSerialized.sprayUnlockables[i];
        }
        
        UpdateState();
    }

    public int GetMaxUnlockablesCount()
    {
        return StickerUnlockables.Length > SprayUnlockables.Length
            ? StickerUnlockables.Length
            : SprayUnlockables.Length;
    }

    public void UpdateState()
    {
        foreach (var stickerUnlockable in StickerUnlockables)
            stickerUnlockable.UpdateState();

        foreach (var sprayUnlockable in SprayUnlockables)
            sprayUnlockable.UpdateState();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if(pauseStatus)
            DataSaver.SaveFile(SaveFileName,new UnlockablesStateSerialized(StickerUnlockables,SprayUnlockables));
    }

    private void OnApplicationQuit()
    {
        DataSaver.SaveFile(SaveFileName,new UnlockablesStateSerialized(StickerUnlockables,SprayUnlockables));
    }

    [Serializable]
    private struct UnlockablesStateSerialized
    {
        [SerializeField] public bool[] stickerUnlockables;
        [SerializeField] public bool[] sprayUnlockables;

        public UnlockablesStateSerialized(Unlockable[] stickers, Unlockable[] sprays)
        {
            stickerUnlockables = new bool[stickers.Length];
            sprayUnlockables = new bool[sprays.Length];

            for (int i = 0; i < stickers.Length; i++)
                stickerUnlockables[i] = stickers[i].IsUnlocked;
            
            for (int i = 0; i < sprays.Length; i++)
                sprayUnlockables[i] = sprays[i].IsUnlocked;
        }
    }
}
