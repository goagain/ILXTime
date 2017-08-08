using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjectorConfig {

    public static bool Filter(TypeDefinition type)
    {
        if (type.Namespace.Contains("ILRuntime"))
            return false;
        if (type.FullName.Contains("LitJson"))
            return false;
        if (type.FullName.StartsWith("<") && type.FullName.EndsWith(">"))
            return false;
        Debug.Log(type.FullName);

        return true;

    }
	
}
