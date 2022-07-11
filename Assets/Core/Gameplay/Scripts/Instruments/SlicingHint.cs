using UnityEngine;

public class SlicingHint : MonoBehaviour
{
    [SerializeField] private Material slicingHintMaterial;

    private void OnEnable()
    {
        slicingHintMaterial.SetTexture("_Volume",LevelsConfig.GetSlicingShape());
    }
}
