// Tool made by Alexandre94.fr (https://github.com/Alexandre94fr)

using System;
using System.Reflection;
using UnityEngine;

public class Instantiator : MonoBehaviour
{
    #region Variables

    public enum InstanceConflictResolutions
    {
        Warning,
        WarningAndPause,
        WarningAndDestructionOfTheSecondOne,
        DestructionOfTheSecondOne,
        WarningAndDestructionOfTheSecondOneParent,
        DestructionOfTheSecondOneParent,
    }
    #endregion

    #region Methods

    /// <summary> 
    /// If there is no existing Instance, returns the Instance of the specified script type,
    /// else it handles the conflict according to the specified resolution type set. 
    /// 
    /// <para> Method utilization example: </para>
    /// <example>
    /// <code> 
    /// public class CLASS_NAME : MonoBehaviour
    /// {
    ///     public static CLASS_NAME Instance;
    /// 
    ///     public void Awake()
    ///     {
    ///         Instance = Instantiator.ReturnInstance(this, Instantiator.p_instanceConflictResolution.WarningAndPause);
    ///     }
    /// }
    /// </code> </example> </summary>
    /// <typeparam name = "T"> The type of the script to return an Instance of. </typeparam>
    /// <param name = "p_classInstance"> An instance of the type T, representing the current class instance. </param>
    /// <param name = "p_instanceConflictResolution"> Defines how to resolve conflicts when multiple instances are detected. </param>
    /// <returns> The Instance of the specified script type.</returns>
    public static T ReturnInstance<T>(T p_classInstance, InstanceConflictResolutions p_instanceConflictResolution) where T : MonoBehaviour
    {
        #region Getting the Instance variable value

        // Get and set the given Class type into a variable
        Type givenClassType = typeof(T);

        // Look for the "Instance" variable who is Public and Static
        FieldInfo instanceVariable = givenClassType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);

        // If we didn't find the so called named "Instance" variable we return an error
        if (instanceVariable == null)
        {
            Debug.LogError($"ERROR ! The class {givenClassType} does not have a public static 'Instance' variable.");
            return null;
        }

        // Get the value of the "Instance" variable
        T instanceVariableValue = (T)instanceVariable.GetValue(null);
        #endregion

        // If an Instance value already exists, we handle the conflict
        if (instanceVariableValue != null)
        {
            HandleInstanceConflict(p_classInstance, p_instanceConflictResolution);

            return instanceVariableValue;
        }
        else
        {
            return p_classInstance;
        }
    }

    static void HandleInstanceConflict<T>(T p_classInstance, InstanceConflictResolutions p_instanceConflictResolution) where T : MonoBehaviour
    {
        switch (p_instanceConflictResolution)
        {
            case InstanceConflictResolutions.Warning:
                Debug.LogWarning("WARNING! There are multiple [" + p_classInstance.ToString() + "] scripts in the scene.");
                break;

            case InstanceConflictResolutions.WarningAndPause:
                Debug.LogWarning("WARNING! There are multiple [" + p_classInstance.ToString() + "] scripts in the scene. UNITY IS PAUSED.");
                Debug.Break();
                break;

            case InstanceConflictResolutions.WarningAndDestructionOfTheSecondOne:
                Debug.LogWarning("WARNING! There are multiple [" + p_classInstance.ToString() + "] scripts in the scene. THE SECOND SCRIPT'S GAMEOBJECT HAS BEEN DESTROYED.");
                Destroy(p_classInstance.gameObject);
                break;

            case InstanceConflictResolutions.DestructionOfTheSecondOne:
                Destroy(p_classInstance.gameObject);
                break;

            case InstanceConflictResolutions.WarningAndDestructionOfTheSecondOneParent:
                Debug.LogWarning("WARNING! There are multiple [" + p_classInstance.ToString() + "] scripts in the scene. THE SECOND SCRIPT'S GAMEOBJECT PARENT HAS BEEN DESTROYED.");
                Destroy(p_classInstance.transform.parent.gameObject);
                break;

            case InstanceConflictResolutions.DestructionOfTheSecondOneParent:
                Destroy(p_classInstance.transform.parent.gameObject);
                break;

            default:
                Debug.LogError("The conflict resolution type given [" + p_instanceConflictResolution.ToString() + "] is not planned in the switch.");
                break;
        }
    }
    #endregion
}