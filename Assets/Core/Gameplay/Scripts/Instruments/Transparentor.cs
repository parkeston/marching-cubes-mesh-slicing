using UnityEngine;

public class Transparentor : MonoBehaviour
{
    private const string COLOR = "_Color";

    [SerializeField] private float _alpha = 0.5f;

    [SerializeField] private Renderer[] _renderers;

    public void Fade(bool on = true)
    {
        foreach (var rend in _renderers)
        {
            var mats = rend.sharedMaterials;
            foreach (var mat in mats)
            {
                if (!mat.HasProperty(COLOR)) continue;
                var color = mat.GetColor(COLOR);
                color.a = on ? _alpha : 1f;
                mat.SetColor(COLOR, color);
            }
        }
    }
}