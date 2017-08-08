using ILXTimeInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

class InjectorHelper
{
    //[PostProcessScene]
    public static bool autoInjected = true;

    [PostProcessScene]
    [MenuItem("ILXTime_Injector/Inject Game Assembly")]
    public static void InjectUnityAssembly()
    {
        //Debug.Log(EditorApplication.isCompiling);
        //Debug.Log(Application.isPlaying);
        //Debug.Log(autoInjected);

        if (EditorApplication.isCompiling || Application.isPlaying || !autoInjected)
            return;
        Debug.Log("Injecting...");
        Injector.InjectAssembly(@"Library\ScriptAssemblies\Assembly-CSharp.dll");
    }

    [MenuItem("ILXTime_Injector/Selected Assembly to Inject...")]
    public static void InjectSelectedAssembly()
    {
        string filepath = EditorUtility.OpenFilePanel("open an dll", "", "dll");
        if (!string.IsNullOrEmpty(filepath))
        {
            Injector.InjectAssembly(filepath);
        }
    }

    //public static void DidReloadScripts()
    //{
    //    if (File.Exists(@"Library\ScriptAssemblies\Assembly-CSharp.dll.bak"))
    //        File.Delete(@"Library\ScriptAssemblies\Assembly-CSharp.dll.bak");
    //    Debug.Log("Delete old backup");
    //}

    [MenuItem("ILXTime_Injector/ForceReload")]
    //[UnityEditor.Callbacks.DidReloadScripts]
    public static void ForceReCompile()
    {
        MonoScript[] cMonoScript = MonoImporter.GetAllRuntimeMonoScripts();
        
        UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
        AssetDatabase.Refresh();
    }

    [MenuItem("ILXTime_Injector/Recover uninjected Assembly")]
    //[UnityEditor.Callbacks.DidReloadScripts]
    public static void RecoverAssembly()
    {
        if (File.Exists(@"Library\ScriptAssemblies\Assembly-CSharp.dll.bak"))
        {
            File.Delete(@"Library\ScriptAssemblies\Assembly-CSharp.dll");
            FileUtil.MoveFileOrDirectory(@"Library\ScriptAssemblies\Assembly-CSharp.dll.bak", @"Library\ScriptAssemblies\Assembly-CSharp.dll");
        }
        UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
        Debug.Log("Recover uninjected Assembly");
    }
}

