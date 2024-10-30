using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [Header("Constrains :")]
    [SerializeField] bool _isXAxisLocked;
    [SerializeField] bool _isYAxisLocked;
    [SerializeField] bool _isZAxisLocked;

    Transform _cameraTransform;

    float _newXAxisEulerAngle;
    float _newYAxisEulerAngle;
    float _newZAxisEulerAngle;

    // Start is called before the first frame update
    void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        _newXAxisEulerAngle = 0;
        _newYAxisEulerAngle = 0;
        _newZAxisEulerAngle = 0;

        if (!_isXAxisLocked)
            _newXAxisEulerAngle = _cameraTransform.rotation.eulerAngles.x;

        if (!_isYAxisLocked)
            _newYAxisEulerAngle = _cameraTransform.rotation.eulerAngles.y;

        if (!_isZAxisLocked)
            _newZAxisEulerAngle = _cameraTransform.rotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(_newXAxisEulerAngle, _newYAxisEulerAngle, _newZAxisEulerAngle);
    }
}