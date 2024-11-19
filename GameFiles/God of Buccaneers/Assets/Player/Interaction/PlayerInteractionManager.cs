using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    public static PlayerInteractionManager Instance;

    [SerializeField] LayerMask _interactableLayerMask = 64;

    Camera _camera;

    void Awake()
    {
        Instance = Instantiator.ReturnInstance(this, Instantiator.InstanceConflictResolutions.WarningAndPause);
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
    }

    /// <summary>
    /// Tries to find a GameObject (2D or 3D) with the layer Interactable at the given screen position </summary>
    /// <param name = "p_screenPosition"> The screen position where the raycast will start. </param>
    /// <returns> Return true if it hits a Interactable GameObject. </returns>
    public bool LaunchInteractionRaycast(Vector2 p_screenPosition)
    {
        RaycastHit2D raycastHit2D = default;
        bool isLastRaycast3D = true;

        #region Raycasting 

        // We begin by making a 3D raycast,
        // if we don't get a GameObject with the Interactable layer then we will make a 2D raycast, why ?
        // Because our interactables are currently in 2D, but maybe one day, they will be 3D.
        if (!Physics.Raycast(
                _camera.ScreenPointToRay(p_screenPosition),
                out RaycastHit raycastHit,
                Mathf.Infinity,
                _interactableLayerMask)
            )
        {
            // The 3D failed so we do a 2D raycast
            raycastHit2D = Physics2D.Raycast(
                _camera.ScreenToWorldPoint(p_screenPosition),
                Vector2.down,
                Mathf.Infinity,
                _interactableLayerMask
            );

            if (!raycastHit2D)
                return false;

            isLastRaycast3D = false;
        }
        #endregion

        #region Getting the InteractableBase component

        InteractableBase interactableBase = null;

        // BEWARE : To match the infrastructure used, we do : 'raycastHit.transform.parent.gameObject' NOT 'raycastHit.transform.gameObject'
        //          it's because our Sprite (he has the collider) is the children of the interactable object.
        if (isLastRaycast3D)
        {
            if (!raycastHit.transform.parent.gameObject.TryGetComponent(out interactableBase))
                return false;
        }
        else
        {
            if (!raycastHit2D.transform.parent.gameObject.TryGetComponent(out interactableBase))
                return false;
        }
        #endregion

        interactableBase.Interact();

        return true;
    }
}