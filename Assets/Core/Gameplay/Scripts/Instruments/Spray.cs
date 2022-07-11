using DG.Tweening;
using UnityEngine;

public class Spray : InstrumentController
{
    [Space]
    [SerializeField] private Transform _target;
    [SerializeField] private ParticleSystem _curParts;
    [SerializeField] private Material particleMaterial;
    [SerializeField] private Material sprayBodyMaterial;
    [SerializeField] private PaintTool paintTool;
    [SerializeField] private StickerRoll stickerPrefab;
    
    private StickerRoll sticker;
    private bool patternFinished;

    protected override void OnEnable()
    {
        base.OnEnable();
        paintTool.enabled = false;
        StickerRoll.OnStickerFinished += PatternFinished;
        Unlockable.OnUnlockableActivated += UpdateSprayVisuals;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StickerRoll.OnStickerFinished -= PatternFinished;
        Unlockable.OnUnlockableActivated -= UpdateSprayVisuals;
    }

    protected override void OnChangeCameraState()
    {
        InstrumentIdlePos(true);
        ShowPattern();
    }

    protected override void StartInstrument()
    {
        _sp.Vibrate = true;
        InstrumentIdlePos(true);
    }

    protected override void StopInstrument()
    {
        base.StopInstrument();
        HidePattern();
    }
    
    protected override void InputStart()
    { 
        if(!patternFinished)
            return;
        
        paintTool.enabled = true;
        base.InputStart();
        _curParts.Play();
        SoundManager.Instance.SprayOn();
    }

    protected override void InputEnd()
    { 
        base.InputEnd();
        _curParts.Stop();
        SoundManager.Instance.SprayOff();
    }

    protected override void Move()
    {
        transform.LookAt(_target);
        base.Move();
    }

    private void PatternFinished()
    {
        patternFinished = true;
        InstrumentStartPos();
    }

    private void ShowPattern()
    {
        sticker = Instantiate(stickerPrefab);
        var pos = sticker.transform.position;
        var rot = sticker.transform.eulerAngles;
        sticker.transform.DOMove(pos, 0.5f).From(pos*10).SetEase(Ease.OutSine);
        sticker.transform.DORotate(rot, 0.5f).From(new Vector3(-90,90,0)).SetEase(Ease.OutSine);
    }
    
    private void HidePattern()
    {
        if (_sp.GameState == GameManager.State.Menu)
        {
            sticker.gameObject.SetActive(false);
            Destroy(sticker.gameObject);
        }
        else
            sticker.Unroll(() =>
            {
                var pos = sticker.transform.position * 10;
                sticker.transform.DOMove(pos, 0.5f).SetEase(Ease.InSine).OnComplete(() =>
                {
                    sticker.gameObject.SetActive(false);
                    Destroy(sticker.gameObject);
                });
                sticker.transform.DORotate(new Vector3(-90, 90, 0), 0.5f).SetEase(Ease.InSine);

                patternFinished = false;
            });
    }
    
    private void UpdateSprayVisuals(Color color, Texture2D texture)
    {
        particleMaterial.color = color;
        sprayBodyMaterial.color = color;
    }
}