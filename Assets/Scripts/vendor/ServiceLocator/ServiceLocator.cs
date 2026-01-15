using System;
using System.Collections.Generic;
using UnityEngine;

namespace vendor.ServiceLocator
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (Services.TryAdd(type, service)) 
                return;
            
            Debug.LogWarning($"Service of type {type} is already registered.");
        }

        public static T Get<T>()
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            Debug.LogError($"Service of type {type} is not registered.");
            return default;
        }

        public static void Unregister<T>()
        {
            var type = typeof(T);
            if (Services.ContainsKey(type))
            {
                Services.Remove(type);
            }
        }
    }
}