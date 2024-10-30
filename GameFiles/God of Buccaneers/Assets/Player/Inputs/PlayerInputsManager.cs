using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputsManager : MonoBehaviour
{
    #region -= Variables =-

    bool _isCameraMovementInputsHelded;
    Vector2 _cameraInputValues; 

    // References
    PlayerCameraManager _playerCameraManager;
    PlayerInteractionManager _playerInteractionManager;
    #endregion

    #region -= Methods =-

    private void Start()
    {
        _playerCameraManager = PlayerCameraManager.Instance;
        _playerInteractionManager = PlayerInteractionManager.Instance;
    }

    private void Update()
    {
        // If any of the camera mouvement keys are held then we move the camera
        if (_isCameraMovementInputsHelded)
        {
            _playerCameraManager.GetPlayerInputValues(_cameraInputValues);
        }
    }

    #region - Recieve inputs -

    public void RecieveCameraMovementInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            _cameraInputValues = p_callbackContext.ReadValue<Vector2>();
            _isCameraMovementInputsHelded = true;
        }
        else if (p_callbackContext.canceled)
        {
            _isCameraMovementInputsHelded = false;
        }
    }

    public void RecieveCameraZoomInZoomOutInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            _playerCameraManager.ZoomCamera(p_callbackContext.ReadValue<float>());
        }
    }

    public void RecieveInteractLaunchDivineCapacityUnselectInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            print("RecieveInteractLaunchDivineCapacityUnselectInputs");

            // If no DivineCapacity is selected :
            
            // Launch a raycast at the player cursor (will manage and return true if he touch an Interactable object)
            if (!_playerInteractionManager.LaunchInteractionRaycast(Input.mousePosition))
            {
                print("Interactable object not touched");
                // Launch selected divine capacity
            }
        }
    }
    public void RecieveAddTerrainInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            print("RecieveAddTerrainInputs");
        }
    }

    public void RecieveUnselectPauseUnpauseInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            print("RecieveUnselectPauseUnpauseInputs");
        }
    }
    #endregion

    #endregion
}