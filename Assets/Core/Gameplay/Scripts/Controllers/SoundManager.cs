using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{

    [SerializeField] private AudioLowPassFilter _filter;
    
    [Header("UI")]
    [SerializeField] private AudioClip _buttonClick;

    [Header("ChainSaw")]
    [SerializeField] private AudioClip _pullCordSaw;
    [SerializeField] private AudioClip _idleSaw;
    [SerializeField] private AudioClip _growSaw;
    [SerializeField] private AudioClip _planeSaw;
    [SerializeField] private AudioClip _fallSaw;
    [SerializeField] private float _slicingTapticStep;
    [SerializeField] private bool _alternateSawSound;

    [Header("Dryer")]
    [SerializeField] private AudioClip _dryer;
    
    [Header("Spray")]
    [SerializeField] private AudioClip _spray;

    private AudioSource _sourceUI;
    private AudioSource _sourceTool;


    private void Start()
    {
        _sourceUI = gameObject.AddComponent<AudioSource>();
        _sourceTool = gameObject.AddComponent<AudioSource>();
    }


    public void PlayButtonClick()
    {
        _sourceUI.PlayOneShot(_buttonClick);
        TapticManager.Impact();
    }


    public void ChainSawOn()
    {
        _filter.cutoffFrequency = 5000;
        _sourceTool.Stop();
        _sourceTool.pitch = 1;
        _sourceTool.loop = false;
        _sourceTool.clip = _pullCordSaw;
        _sourceTool.Play();
        DOVirtual.DelayedCall(_pullCordSaw.length, () =>
        {
            _sourceTool.clip = _idleSaw;
            _sourceTool.loop = true;
            _sourceTool.Play();
        });
    }
    
    public void ChainSawOff()
    {
        if (_sawCoroutine != null) StopCoroutine(_sawCoroutine);
        
        _sourceTool.Stop();
        _sourceTool.pitch = 1;
        _sourceTool.loop = false;
        _sourceTool.clip = _pullCordSaw;
        _sourceTool.Play();
        DOVirtual.DelayedCall(_pullCordSaw.length, () =>
        {
            _sourceTool.Stop();
            _filter.cutoffFrequency = 15000;
        });
    }

    #region SawSounds

    private Coroutine _sawCoroutine;

    public void StartCut()
    {
        if (_alternateSawSound) return;
        if (_sawCoroutine != null) StopCoroutine(_sawCoroutine);
        _sawCoroutine = StartCoroutine(StartCutCoroutine());
    }

    public void StopCut()
    {
        if (_alternateSawSound) return;
        if (_sawCoroutine != null) StopCoroutine(_sawCoroutine);
        _sawCoroutine = StartCoroutine(StopCutCoroutine());
    }
    
    private bool _growedEnough;

    private IEnumerator StartCutCoroutine()
    {
        _growedEnough = false;
        _sourceTool.Stop();
        _sourceTool.pitch = 1;
        _sourceTool.clip = _growSaw;
        _sourceTool.loop = false;
        _sourceTool.Play();

        yield return new WaitForSeconds(_growSaw.length * 0.5f);
        _growedEnough = true;
        yield return new WaitForSeconds(_growSaw.length * 0.5f);

        _sourceTool.clip = _planeSaw;
        _sourceTool.loop = true;
        _sourceTool.Play();
    }

    private IEnumerator StopCutCoroutine()
    {
        _sourceTool.Stop();
        _sourceTool.pitch = 1;
        
        if (_growedEnough)
        {
            _sourceTool.clip = _fallSaw;
            _sourceTool.loop = false;
            _sourceTool.Play();

            yield return new WaitForSeconds(_fallSaw.length);
        }

        _sourceTool.clip = _idleSaw;
        _sourceTool.loop = true;
        _sourceTool.Play();
    }

    private Coroutine _slicingTapticCoroutine;

    public void SawSlicing()
    {
        if (_alternateSawSound)
        {
            _sourceTool.clip = _planeSaw;
            _sourceTool.loop = true;
            _sourceTool.pitch = Random.Range(0.6f, 0.68f);
            _sourceTool.Play();
        }
        else
        {
            _sourceTool.pitch = Random.Range(0.9f, 0.98f); 
        }
        

        if (_slicingTapticCoroutine != null) StopCoroutine(_slicingTapticCoroutine);
        _slicingTapticCoroutine = StartCoroutine(SlicingTapticCoroutine());
    }

    private IEnumerator SlicingTapticCoroutine()
    {
        var time = 0f;
        do
        {
            time += Time.deltaTime;
            if (time > _slicingTapticStep)
            {
                // #if UNITY_EDITOR
                // _sourceUI.PlayOneShot(_buttonClick);
                // #endif
                TapticManager.Impact(TapticImpact.Light);
                time = 0;
            }

            yield return null;
        } while (true);
    }

    public void StopSawSlicing()
    {
        if (_alternateSawSound)
        {
            _sourceTool.clip = _idleSaw;
            _sourceTool.loop = true;
            _sourceTool.Play();  
        }
        
        _sourceTool.pitch = 1f;
        if (_slicingTapticCoroutine != null) StopCoroutine(_slicingTapticCoroutine);
    }

    public void DryerOn()
    {
        _sourceTool.clip = _dryer;
        _sourceTool.loop = true;
        _sourceTool.pitch = 1;
        _sourceTool.Play();
    }
    
    public void DryerOff()
    {
        _sourceTool.Stop();
    }

    public void SprayOn()
    {
        _sourceTool.clip = _spray;
        _sourceTool.loop = true;
        _sourceTool.pitch = 1;
        _sourceTool.Play(); 
    }
    
    public void SprayOff()
    {
        _sourceTool.Stop();
    }
    
    
    #endregion
}