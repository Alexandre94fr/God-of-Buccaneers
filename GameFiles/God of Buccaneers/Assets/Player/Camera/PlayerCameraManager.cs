using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraManager : MonoBehaviour
{
    #region -= Variables =-

    public static PlayerCameraManager Instance;

    #region - Serialized variables -

    // X, Z movement axis
    [Header("Camera movement stats :")]

    [Range(0.1f, 0.5f)] [SerializeField] float _cameraMovementInputSignificanceThreshold = 0.1f;
    [Range(0.1f, 0.5f)] [SerializeField] float _cameraVelocityThreshold = 0.1f;

    [Range(0f, 25f)] [SerializeField] float _cameraMaxSpeed = 5f;
    [Range(0f, 25f)] [SerializeField] float _cameraAcceleration = 10f;
    [Range(0f, 25f)] [SerializeField] float _cameraDeceleration = 15f;
    [Range(0f, 25f)] [SerializeField] float _cameraZoomHeightSpeedMultiplicator = 10f;

    // Zoom-in, zoom-out
    [Header("Camera zoom stats :")]

    [Tooltip("BEWARE ! If you want to change the value of this variable in runtime, you should zoom to the minimum, otherwise the zoom-in, zoom-out will not work correctly.")]
    [SerializeField] bool _isCameraZoomAngleFixed = true;

    [Range(0.1f, 1f)] [SerializeField] float _cameraZoomInputSignificanceThreshold = 0.1f;
    [Range(1, 100)] [SerializeField] int _zoomInputScalingFactor = 100;

    [Range(0f, 25f)] [SerializeField] float _cameraZoomInStepPower = 2f;
    [Range(0f, 25f)] [SerializeField] float _cameraZoomOutStepPower = 7.5f;
    [Range(0f, 100f)] [SerializeField] float _cameraZoomMinimalHeight = 5f;
    [Range(0f, 100f)] [SerializeField] float _cameraZoomMaximalHeight = 50f;
    [Range(0f, 25f)] [SerializeField] float _cameraZoomSpeed = 2f;

    // Screen edge movement
    [Header("Camera screen edge movement stats :")]

    [SerializeField] bool _isScreenEgdeCameraMovementEnable = true;
    [Tooltip("The factor of screen edge used to move the camera (0.05f equal 5% of the screen edge)")]
    [Range(0f, 1f)] [SerializeField] float _edgeDetectionToleranceFactor = 0.05f;
    #endregion

    #region - Private variables -

    [Tooltip("This variable is used to store all the position modification that the camera will do at the next frame.")]
    Vector3 _targetGameObjectPosition;

    float _zoomHeight;

    // Those variables exist to avoid having to use a Rigidbody to keep track of camera's movement data
    Vector3 _cameraLastPosition;
    Vector3 _cameraHorizontalVelocity;

    float _cameraSpeed;

    Vector2 _environmentWorldSize;

    // References
    Vector2 _numberOfChunks;
    Vector2 _chunkSize;

    // Object references
    EnvironmentGenerator _environmentGenerator;
    Transform _transform;
    Transform _cameraTransform;

    #endregion

    #endregion

    #region -= Methods =-

    #region - Unity methods -

    void Awake()
    {
        Instance = Instantiator.ReturnInstance(this, Instantiator.InstanceConflictResolutions.WarningAndPause);
    }

    void Start()
    {
        // Getting components
        _cameraTransform = GetComponentInChildren<Camera>().transform;
        _transform = transform;

        // Getting outside components
        _environmentGenerator = EnvironmentGenerator.Instance;

        // Getting values
        _chunkSize = new(_environmentGenerator.EnvironmentOptions.ChunkSize, _environmentGenerator.EnvironmentOptions.ChunkSize);
        _numberOfChunks = _environmentGenerator.EnvironmentOptions.NumberOfChunks;

        // Compute the environment size
        _environmentWorldSize = _chunkSize * _numberOfChunks;

        // Security
        if (_cameraTransform == null)
        {
            Debug.LogError("ERROR ! The camera's transform as not been finded, " +
                "you should check if you have put the player's camera as a child of this object."
            );
        }

        _zoomHeight = _cameraTransform.localPosition.y;
        _cameraTransform.LookAt(_transform);

        _cameraLastPosition = _transform.position;
    }

    void Update()
    {
        if (_isScreenEgdeCameraMovementEnable)
            CheckMousePositionAtEdge();

        UpdateVelocity();

        UpdateCameraPosition();

        UpdateBasePosition();
    }
    #endregion

    #region - Camera movement -

    public void GetPlayerInputValues(Vector2 p_inputValues)
    {
        // Converting the player's inputs into a mouvement direction from the perspective of the camera,
        // if the player look at the left (-Z axis) and press the key that make the camera move forward,
        // the camera will move to the left.
        Vector3 convertedInputValues = p_inputValues.x * GetCameraRight() + p_inputValues.y * GetCameraForward();

        // We normalize the values to avoid the camera to go faster if we press Forward movement and Right movement keys at the same time
        convertedInputValues = convertedInputValues.normalized;

        // We check if the squared magnitude of the received inputs is above 0.03
        // (equivalent to a magnitude of 0.3) to ensure the input is significant enough
        if (convertedInputValues.sqrMagnitude > _cameraMovementInputSignificanceThreshold)
        {
            // We use " += " because we want to stack multiple inputs of target positions
            _targetGameObjectPosition += convertedInputValues;
        }
    }

    #region GetPlayerInputValues sub-methods

    Vector3 GetCameraRight()
    {
        // We get the right axis of the camera
        Vector3 cameraVectorRight = _cameraTransform.right;

        cameraVectorRight = RoundVector3ToVector3IntUpwardNormalized(cameraVectorRight);

        // We cancel all possible Y movement
        cameraVectorRight.y = 0;

        return cameraVectorRight;
    }

    Vector3 GetCameraForward()
    {
        // We get the right axis of the camera
        Vector3 cameraVectorForward = _cameraTransform.forward;

        cameraVectorForward = RoundVector3ToVector3IntUpwardNormalized(cameraVectorForward);

        // We cancel all possible Y movement
        cameraVectorForward.y = 0;

        return cameraVectorForward;
    }
    #endregion

    void UpdateVelocity()
    {
        // We divide the deplacement of the camera by the time past the last frame
        _cameraHorizontalVelocity = (_transform.position - _cameraLastPosition) / Time.deltaTime;

        // We cancel all possible Y movement
        _cameraHorizontalVelocity.y = 0;

        // We update the last position variable
        _cameraLastPosition = _transform.position;
    }

    void UpdateBasePosition()
    {
        // If the square distance left to travel is inferior than for exemple 0.1f then we slow down the movement speed to decelerate
        if (_targetGameObjectPosition.sqrMagnitude < _cameraVelocityThreshold)
        {
            // Linearly interpolates the camera's horizontal velocity towards zero, using a deceleration factor (_cameraDeceleration).
            _cameraHorizontalVelocity = Vector3.Lerp(_cameraHorizontalVelocity, Vector3.zero, Time.deltaTime * _cameraDeceleration);

            // If the velocity of the camera is faster than _cameraVelocityThreshold then we continue to move the camera
            if (_cameraHorizontalVelocity.sqrMagnitude > _cameraVelocityThreshold / 10)
                _transform.position += _cameraHorizontalVelocity * Time.deltaTime;
        }
        else
        {
            _cameraSpeed = Mathf.Lerp(_cameraSpeed, _cameraMaxSpeed, Time.deltaTime * _cameraAcceleration);
            
            // Adjust the camera's position by adding a movement step, scaled by the zoom height.
            // _zoomHeight / _cameraZoomHeightSpeedMultiplicator    : This increase the camera speed when zoomed out.
            // _cameraSpeed * Time.deltaTime                        : Ensures the movement is consistent over time, regardless of frame rate.
            // _targetGameObjectPosition                            : The direction and magnitude of the target position to move towards.
            _transform.position += _zoomHeight / _cameraZoomHeightSpeedMultiplicator * _cameraSpeed * Time.deltaTime * _targetGameObjectPosition;
        }

        HandleCameraBorder();

        // Resetting the target position
        _targetGameObjectPosition = Vector3.zero;
    }

    /// <summary>
    /// Will verify if the Main camera parent's position (_transform.position) is outside of the generated terrain chunks,
    /// if yes than he will update the (_transform.position) to the last position (_cameraLastPosition) </summary>
    void HandleCameraBorder()
    {
        // To optimize
        float positionX = _transform.position.x;
        float positionZ = _transform.position.z;

        if (positionX < 0 || positionZ < 0 || positionX > _environmentWorldSize.x || positionZ > _environmentWorldSize.y)
            _transform.position = _cameraLastPosition;
    }

    void CheckMousePositionAtEdge()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;
        Vector2 screenSize = new(Screen.width, Screen.height);

        // If the player cursor is at the left or right of the screen we move accordingly
        if (mousePosition.x <= _edgeDetectionToleranceFactor * screenSize.x && mousePosition.x >= 0)
            moveDirection += -GetCameraRight();
        else if (mousePosition.x >= (1f - _edgeDetectionToleranceFactor) * screenSize.x && mousePosition.x < screenSize.x)
            moveDirection += GetCameraRight();

        // If the player cursor is at the down or top of the screen we move accordingly
        if (mousePosition.y <= _edgeDetectionToleranceFactor * screenSize.y && mousePosition.y >= 0)
            moveDirection += -GetCameraForward();
        else if (mousePosition.y >= (1f - _edgeDetectionToleranceFactor) * screenSize.y && mousePosition.y < screenSize.y)
            moveDirection += GetCameraForward();

        _targetGameObjectPosition += moveDirection;
    }
    #endregion

    #region - Camera zoom -

    public void ZoomCamera(float p_zoomInputValue)
    {
        // The "100" value is there to avoid a really bit changement
        float convertedZoomInputValue = -p_zoomInputValue / _zoomInputScalingFactor;

        // Check if the absolute value (exemple : -0.5f -> 0.5f) of convertedZoomInputValue is strong enough
        if (Mathf.Abs(convertedZoomInputValue) > _cameraZoomInputSignificanceThreshold)
        {
            _zoomHeight = _cameraTransform.localPosition.y + convertedZoomInputValue * _cameraZoomInStepPower;

            // Clamping the camera height between the minimal and the maximal height
            _zoomHeight = Mathf.Clamp(_zoomHeight, _cameraZoomMinimalHeight, _cameraZoomMaximalHeight);
        }
    }

    void UpdateCameraPosition()
    {
        Vector3 zoomTarget = new(
            _cameraTransform.localPosition.x,
            _zoomHeight,
            _cameraTransform.localPosition.z
        );

        // If the camera angle is not fixed, adjust the zoom target to modify the forward position slightly,
        // making the camera move forward or backward as it zooms in or out.
        // _cameraZoomSpeed                                 : Determines how quickly the camera adjusts its forward position.
        // (_zoomHeight - _cameraTransform.localPosition.y) : Difference between the desired zoom level and current height.
        // Vector3.forward                                  : Applies the adjustment in the forward direction relative to the camera.
        if (!_isCameraZoomAngleFixed)
        {
            zoomTarget -= _cameraZoomSpeed * (_zoomHeight - _cameraTransform.localPosition.y) * Vector3.forward;
        }

        // Smoothly interpolate the camera's position towards the target zoom position over time.
        // _cameraTransform.localPosition           : Current position of the camera.
        // zoomTarget                               : The target position to move towards.
        // Time.deltaTime * _cameraZoomOutStepPower : Ensures smooth interpolation based on time and the zoom-out speed.
        _cameraTransform.localPosition = Vector3.Lerp(_cameraTransform.localPosition, zoomTarget, Time.deltaTime * _cameraZoomOutStepPower);

        _cameraTransform.LookAt(_transform);
    }
    #endregion

    #region - Tools -

    int RoundFloatToIntUpwardNormalized(float p_float)
    {
        if (p_float < 0)
            return -1;

        if (p_float > 0)
            return 1;

        return 0;
    }

    Vector3 RoundVector3ToVector3IntUpwardNormalized(Vector3 p_vector3)
    {
        return new Vector3(
            RoundFloatToIntUpwardNormalized(p_vector3.x),
            RoundFloatToIntUpwardNormalized(p_vector3.y),
            RoundFloatToIntUpwardNormalized(p_vector3.z)
        );
    }
    #endregion

    #endregion
}