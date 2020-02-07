using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container class with the use to inject references in subclasses.
/// </summary>
public static class ServiceLocator 
{

    //----Dictionary containing managers based on Types----//
    private static Dictionary<Type, ServiceOfType> services = new Dictionary<Type, ServiceOfType>();


    //----If true, will print debug logs related to the ServiceLocator----/
    public static bool debuggingMode = true;


    //---- Callbacks you can subscribe to ----////
    public static event Action OnAddedService;
    public static event Action OnRemovedService;
    //---------------------------------------////

    
    public static bool TryAddServiceOfType(ServiceOfType service)
    {
        if (!services.ContainsKey(service.GetType())) 
        {
            services.Add(service.GetType(), service);
            OnAddedService?.Invoke();
            return true;
        }

        return false;
    }

    public static bool TryAddServiceOfType<T>(T service) where T : ServiceOfType 
    {
        if(!services.ContainsKey(service.GetType())) 
        {
            services.Add(service.GetType(), service);
            OnAddedService?.Invoke();
            return true;
        }

        Debug.LogErrorFormat($"A service of type { typeof(T) }, has already been added to the ServiceLocator.");
        return false;
    }

    public static T GetServiceOfType<T>() where T : ServiceOfType 
    {
        if(services.ContainsKey(typeof(T))) 
        {
            services.TryGetValue(typeof(T), out ServiceOfType service);
            return (T)service;
        }

        Debug.LogErrorFormat($"A service of type { typeof(T) }, has not been added to the ServiceLocator.");
        return null;
    }

    public static bool TryRemoveServiceOfType<T>(T service) 
    {
        if (services.ContainsKey(service.GetType()))
        {
            services.Remove(service.GetType());
            OnRemovedService?.Invoke();
            return true;
        }

        Debug.LogWarningFormat($"A service of type { typeof(T) }, couldn't be removed due to it not existing within the ServiceLocator.");
        return false;
    }
}
