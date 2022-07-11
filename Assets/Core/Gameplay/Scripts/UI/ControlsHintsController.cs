using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControlsHintsController : MonoBehaviour
{
    [SerializeField] private GameObject infinityMoveHint;
    [SerializeField] private GameObject stickerHint;


    private void Awake()
    {
        infinityMoveHint.SetActive(false);
        stickerHint.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.OnStartInstrument += ShowControlsHint;
    }

    private void OnDisable()
    {
        GameManager.OnStartInstrument -= ShowControlsHint;
    }

    private void ShowControlsHint()
    {
        StopCoroutine(ShowControlsHintRoutine());
        StartCoroutine(ShowControlsHintRoutine());
    }

    private IEnumerator ShowControlsHintRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        if (!LevelsConfig.IsSpray)
            infinityMoveHint.SetActive(true);
        else
            stickerHint.SetActive(true);
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && (infinityMoveHint.activeSelf || stickerHint.activeSelf))
        {
            infinityMoveHint.SetActive(false);
            stickerHint.SetActive(false);
        }
    }
}
