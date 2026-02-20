using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    [SerializeField] Transform _target;
    Vector3 _offset;

    private void Start()
    {
        _offset = transform.position - _target.position;
    }
    private void LateUpdate()
    {
        transform.position = _target.position + _offset;
    }
}
