using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjectorConfig {

    public static bool Filter(TypeDefinition type)
    {
        if (type.Namespace.Contains("ILRuntime"))
            return false;

        return true;
    }
	
}
