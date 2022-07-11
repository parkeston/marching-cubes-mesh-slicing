using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour
{
    [SerializeField] private TMP_Text[] texts;
    [SerializeField] private new Renderer renderer;

    private UnityAction onClickAction;
    public void SetOnClickAction(UnityAction action) => onClickAction = action;
    

    public void UpdateAppearance(int voxelValue, int label)
    {
        renderer.material.color = voxelValue == 0 ? Color.black : Color.white;
        UpdateTexs(voxelValue,label);
    }

    private void UpdateTexs(int voxelValue, int label)
    {
        foreach (var text in texts)
        {
            text.text = label.ToString();
            text.color = voxelValue == 0 ? Color.white : Color.black;
        }
    }

    private void OnMouseDown()
    {
        onClickAction?.Invoke();
    }
}
