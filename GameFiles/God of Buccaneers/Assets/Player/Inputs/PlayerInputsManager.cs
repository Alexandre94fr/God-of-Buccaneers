using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputsManager : MonoBehaviour
{
    #region -= Variables =-


    #endregion

    #region -= Methods =-

    #region - Recieve inputs -

    public void RecieveCameraMovementInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            print("RecieveCameraMovementInputs");
        }
    }

    public void RecieveCameraZoomInZoomOutInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            print("RecieveCameraZoomInZoomOutInputs");
        }
    }

    public void RecieveInteractLaunchDivineCapacityUnselectInputs(InputAction.CallbackContext p_callbackContext)
    {
        if (p_callbackContext.performed)
        {
            print("RecieveInteractLaunchDivineCapacityUnselectInputs");
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