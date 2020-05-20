using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime;

/// <summary>
/// Container class with the use to inject references in subclasses.
/// </summary>
public static class ServiceLocator 
{

    //----Dictionary containing managers based on Types----//
    private static Dictionary<Type, IServiceOfType> services = new Dictionary<Type, IServiceOfType>();

    //---- Callbacks you can subscribe to ----//
    public static event Action OnAddedService;
    public static event Action OnRemovedService;
    //---------------------------------------//

    
    public static bool TryAddServiceOfType(IServiceOfType service)
    {
        if (!services.ContainsKey(service.GetType())) 
        {
            services.Add(service.GetType(), service); 
            OnAddedService?.Invoke();
            return true;
        }

        return false;
    }

    public static T GetServiceOfType<T>() where T : IServiceOfType 
    {
        if(services.ContainsKey(typeof(T))) 
        {
            services.TryGetValue(typeof(T), out IServiceOfType service);
            return (T)service;
        }

        Debug.LogErrorFormat($"A service of type { typeof(T) }, has not been added to the ServiceLocator.");
        return default;
    }

    public static bool TryRemoveServiceOfType<T>(T service) 
    {
        if (services.ContainsKey(service.GetType()))
        {
            IServiceOfType containedService;
            services.TryGetValue(typeof(T), out containedService);

            if (containedService.Equals(service))
            {
                services.Remove(service.GetType());
                OnRemovedService?.Invoke();
                return true;
            }

            Debug.LogWarningFormat($"The service of type {typeof(T)} could not be removed due to it not matching the one stored in the dictionary.");
            return false;
        }

        Debug.LogWarningFormat($"A service of type { typeof(T) }, couldn't be removed due to it not existing within the ServiceLocator.");
        return false;
    }
}
