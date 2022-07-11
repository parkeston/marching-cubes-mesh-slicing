using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 _rotate;
    [SerializeField] private float _speed;

    private float _sign = 1f;

    void Update()
    {
        var curRot = transform.rotation;
        curRot *= Quaternion.Euler(_sign * _rotate * Time.deltaTime * _speed);
        transform.rotation = curRot;
    }

    public void Inverse()
    {
        _sign *= -1f;
    }
}