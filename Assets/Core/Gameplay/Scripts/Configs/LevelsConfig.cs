using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsConfig")]
public class LevelsConfig : ScriptableSingleton<LevelsConfig>
{
    public Level[] levels;

    public Activity[] activities;


    //----- Types

    [Serializable]
    public struct Level
    {
        public Texture3D slicingShape;
    }


    [Serializable]
    public enum InstrumentType
    {
        ChainSaw,
        Dryer,
        Spray,
        Sticker,
        Observer
    }


    [Serializable]
    public struct Activity
    {
        public InstrumentType type;
        public GameManager.ViewState viewState;
        public bool sameInstrument;
    }

    public static Texture3D GetSlicingShape()
    {
        return Instance.levels[StatePersister.Instance.LevelNumber].slicingShape;
    }

    private static InstrumentType GetCurrentInstrumentType => Instance.activities[StatePersister.Instance.ActivityIndex].type;

    public static int GetInstrumentIndex()
    {
        return GetInstrumentIndex(StatePersister.Instance.ActivityIndex);
    }
    
    public static int GetInstrumentIndex(int testIndex)
    {
        var index = 0;
        for (int i = 0; i <= testIndex; i++)
        {
            if (Instance.activities[i].sameInstrument)
            {
                continue;
            }
            index++;
        }

        return index-1;
    }

    public static int GetInstrumentsCount()
    {
        var counter = 0;
        for (int i = 0; i < Instance.activities.Length; i++)
        {
            if (Instance.activities[i].sameInstrument) continue;
            counter++;
        }

        return counter;
    }

    public static bool IsDryer => GetCurrentInstrumentType == InstrumentType.Dryer || GetCurrentInstrumentType == InstrumentType.Spray;
    public static bool IsSpray =>GetCurrentInstrumentType == InstrumentType.Spray;
}