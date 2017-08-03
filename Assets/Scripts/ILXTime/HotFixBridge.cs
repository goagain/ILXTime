using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HotFixBridge
{
    public static ILRuntime.Runtime.Enviorment.AppDomain appdomain;

    public HotFixBridge(IMethod method)
    {
        this.method = method;
        Parameters = new object[method.Parameters.Count];
    }

    public IMethod method;
    public object[] Parameters;
    public object Invoke()
    {
        if (appdomain != null)
        {
            return appdomain.Invoke(method, null, Parameters);
        }
        return null;
    }

    
}
