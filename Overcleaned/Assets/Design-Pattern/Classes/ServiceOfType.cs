using UnityEngine;

/// <summary>
/// Base template of Managers that are being collected by the ServiceLocator.
/// </summary>
public class ServiceOfType : MonoBehaviour, IServiceOfType
{

    protected virtual void Awake()
    {
        InitialiseService();
    }

    protected virtual void OnDestroy() 
    {
        RemoveService();
    }

    /// <summary>
    /// Adding the Manager based on type from the ServiceLocator container.
    /// </summary>
    public void InitialiseService()
    {
        if (ServiceLocator.TryAddServiceOfType(this))
        {
            Debug_PrintMessage($"ServiceLocator: Added Service of type '{ GetType() }'");
        }
    }

    /// <summary>
    /// Removing the Manager based on type from the ServiceLocator container.
    /// </summary>
    public void RemoveService() 
    {
        if (ServiceLocator.TryRemoveServiceOfType(this))
        {
            Debug_PrintMessage($"ServiceLocator: Removed Service of type '{ GetType() }'");
        }
    }

    /// <summary>
    /// Only print the debug messages when debugging mode is enabled.
    /// </summary>
    /// <param name="messageContents"></param>
    private static void Debug_PrintMessage(string messageContents) 
    {
        if(ServiceLocator.debuggingMode == true) 
        {
            Debug.Log(messageContents);
        }
    }
}
