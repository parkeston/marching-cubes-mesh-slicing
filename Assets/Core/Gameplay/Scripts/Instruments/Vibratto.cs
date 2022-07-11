using UnityEngine;
using Random = UnityEngine.Random;

public class Vibratto : MonoBehaviour
{
    [SerializeField] private float _magnitude;
    [SerializeField] private float _speed;

    private void Update()
    {
        if (!StatePersister.Instance.CanPlay || !StatePersister.Instance.Vibrate) return;
        transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.Euler(Random.onUnitSphere * _magnitude), _speed * Time.deltaTime);
    }
}