using UnityEngine;
using UnityEngine.Serialization;
using static GameManager;
using static LevelsConfig;

public class StatePersister : Singleton<StatePersister>
{
    #region Constants

    private const string kBonusLevel = "BonusLevel";
    private const string kLevelNumber = "LevelNumber";
    private const string kMoney = "Money";
    private const string kBonusShowedLevel = "BonusShowedLevel";
    private const string kTapticEnabled = "TapticEnabled";

    #endregion

    #region Remote properties

    public int BonusLevel => !RemoteSettings.HasKey(kBonusLevel) ? 10 : RemoteSettings.GetInt(kBonusLevel);

    #endregion


    #region Prefs properties

    public int Money
    {
        get => PlayerPrefs.GetInt(kMoney);
        set => PlayerPrefs.SetInt(kMoney, value);
    }

    public int LevelNumber
    {
        get => PlayerPrefs.GetInt(kLevelNumber);
        set
        {
            if (value >= LevelsConfig.Instance.levels.Length) value = 0;
            PlayerPrefs.SetInt(kLevelNumber, value);
            PlayerPrefs.Save();
        }
    }

    public int BonusShowedLevel
    {
        get => PlayerPrefs.GetInt(kBonusShowedLevel);
        set => PlayerPrefs.SetInt(kBonusShowedLevel, value);
    }

    public bool TapticEnabled
    {
        get => true; //PlayerPrefs.GetInt(kTapticEnabled) == 1;
        set => PlayerPrefs.SetInt(kTapticEnabled, value ? 1 : 0);
    }

    #endregion


    #region Public vars

    public State GameState;
    public float AccuracyProgress;
    public int ActivityIndex;
    public int InstrumentIndex;
    public InstrumentController Instrument;
    public int RewardMoney;
    public bool CanPlay;
    public bool Vibrate;
    public ViewState ViewState;

    #endregion
}