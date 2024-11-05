using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interactable values :")]
    [SerializeField] LayerMask _interactableLayerMask = 64; // Set to the layer Interactable

    protected virtual void Start()
    {
        LayerSecurity(gameObject);
    }

    /// <summary>
    /// Method you should call when you want the object to be consider interacted.
    /// 
    /// <para> Will call the OnInteraction method if you don't override this method. </para> </summary>
    public virtual void Interact()
    {
        Debug.Log($"The '{gameObject.name}' GameObject has been interacted. \nThe method OnInteraction will be called.");

        OnInteraction();
    }

    /// <summary>
    /// The behaviour of your object when interacted.
    /// 
    /// <para> This method will be automatically called when the not override Interact method is called. </para>
    /// 
    /// <para> This method can be override. </para> </summary>
    protected virtual void OnInteraction()
    {
        Debug.Log("Running the basic behaviour of an interacted object.");
    }

    /// <summary>
    /// Check if the given GameObject has one of the selected layers, if not, a warning will be sent. 
    /// 
    /// <para> This method is automaticaly called insind the Unity Start method, 
    /// if and only if you don't called the Unity Start method insind the child of InteractableBase, 
    /// if you want to have your own Start implementation you can override the Start, 
    /// but you will have to called the LayerSecurity method yourself. </para> </summary>
    protected void LayerSecurity(GameObject p_gameObjectToCheck)
    {
        bool hasOneOfSelectedLayers = false;
        string selectedLayers = "";
        
        // We use 32 as maximum value because the maximum number of layers we can create in Unity is 32
        for (int i = 0; i < 32; i++)
        {
            // Verify if the layer i is activated (on 1) in _interactableLayerMask
            // NOTE : Don't forget that we read bits right to left, not left to right.
            if ((_interactableLayerMask.value & (1 << i)) != 0)
            {
                selectedLayers += $"layer n°{i} ({LayerMask.LayerToName(i)}), ";

                if (p_gameObjectToCheck.layer == i)
                {
                    hasOneOfSelectedLayers = true;
                }
            }
        }

        if (!hasOneOfSelectedLayers)
        {
            if (selectedLayers == "")
                Debug.LogWarning(
                    $"You want the GameObject '{p_gameObjectToCheck.name}' to have no layers (layer 'Nothing')" +
                    $" but you can't set the layer Nothing to a GameObject in the inspector."
                );
            else
                Debug.LogWarning(
                    $"The GameObject '{p_gameObjectToCheck.name}' doesn't have the selected layers.\n" +
                    $"    Selected layers : {selectedLayers.TrimEnd(',', ' ')}."
                );
        }

        // You can uncomment this, if you want to know _interactableLayerMask in binary
        //Debug.Log($"_interactableLayerMask in binary : {Convert.ToString(_interactableLayerMask.value, 2).PadLeft(32, '0')}");
    }
}