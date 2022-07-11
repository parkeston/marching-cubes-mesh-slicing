using UnityEngine;
using UnityEngine.UI;

public class CirclesProgressBar : MonoBehaviour
{
    [SerializeField] private Image[] _fillImages;

    public void UpdateBar()
    {
        var instrumentsCount = LevelsConfig.GetInstrumentsCount();
        var instrumentIndex = LevelsConfig.GetInstrumentIndex();
        for (int i = 0; i < _fillImages.Length; i++)
        {
            _fillImages[i].transform.parent.gameObject.SetActive(i < instrumentsCount);
            var enable = i <= instrumentIndex;
            _fillImages[i].enabled = !enable;
        }
    }
}