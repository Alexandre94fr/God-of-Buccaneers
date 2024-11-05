using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [Header("References :")]
    [SerializeField] Transform _spriteTransform;

    [Header("Constrains :")]
    [SerializeField] bool _isXAxisLocked;
    [SerializeField] bool _isYAxisLocked;
    [SerializeField] bool _isZAxisLocked;

    Transform _cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (_spriteTransform == null)
            Debug.LogError("ERROR ! The _spriteTransform Transform variable is not set.");

        _cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpriteRotation(_cameraTransform, _spriteTransform);
    }

    void UpdateSpriteRotation(Transform p_cameraTransform, Transform p_spriteTransform)
    {
        float newXAxisEulerAngle = 0;
        float newYAxisEulerAngle = 0;
        float newZAxisEulerAngle = 0;

        if (!_isXAxisLocked)
            newXAxisEulerAngle = p_cameraTransform.rotation.eulerAngles.x;

        if (!_isYAxisLocked)
            newYAxisEulerAngle = p_cameraTransform.rotation.eulerAngles.y;

        if (!_isZAxisLocked)
            newZAxisEulerAngle = p_cameraTransform.rotation.eulerAngles.z;

        p_spriteTransform.rotation = Quaternion.Euler(newXAxisEulerAngle, newYAxisEulerAngle, newZAxisEulerAngle);
    }
}