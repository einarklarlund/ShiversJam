using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T FindComponent<T>(this Component component)
        where T : Component
    {
        var targetComponent = component.GetComponent<T>();
        if(targetComponent)
            return targetComponent;
        
        if(targetComponent = component.GetComponentInChildren<T>())
            return targetComponent;

        if(targetComponent = component.GetComponentInParent<T>())
            return targetComponent;

        return null;
    }
    
    public static List<T> FindComponents<T>(this Component component)
        where T : Component
    {
        var allComponents = new List<T>();

        var components = component.GetComponents<T>();
        if(components != null)
            allComponents.AddRange(components);
        
        if((components = component.GetComponentsInChildren<T>()) != null)
            allComponents.AddRange(components);

        if((components = component.GetComponentsInParent<T>()) != null)
            allComponents.AddRange(components);

        if(allComponents.Count == 0)
            return null;

        return allComponents;
    }
}
