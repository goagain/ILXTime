using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ILXTimeInjector
{
    public class Injector
    {
        //static System.Reflection.Assembly externalAssembly;
        public static void InjectAssembly(string assembly_path)
        {
            var readerParameters = new ReaderParameters { ReadSymbols = true };
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assembly_path, readerParameters);

            //externalAssembly = System.Reflection.Assembly.LoadFile(assembly_path);

            if(assembly.Modules.Any(module => module.Types.Any(x=>x.Namespace == "__ILXTime" && x.Name == "INJECTED")))
            {
                Debug.LogError("This Assembly is already injected!");
                return;
            }
            foreach (var module in assembly.Modules)
            {
                foreach(var typ in module.Types.Where(InjectorConfig.Filter))
                {
                    foreach(var method in typ.Methods)
                    {
                        InjectMethod(typ, method);
                    }
                }
            }
            var objType = assembly.MainModule.ImportReference(typeof(object));
            assembly.MainModule.Types.Add(new TypeDefinition("__ILXTime", "INJECTED", TypeAttributes.Class, objType));

            FileUtil.CopyFileOrDirectory(assembly_path, assembly_path + ".bak");

            var writerParameters = new WriterParameters { WriteSymbols = true };
            assembly.Write(assembly_path, writerParameters);

            Debug.Log("Inject Success!!!");

            if (assembly.MainModule.SymbolReader != null)
            {
                assembly.MainModule.SymbolReader.Dispose();
            }
        }

        public static void InjectMethod(TypeDefinition type, MethodDefinition method)
        {
            TypeReference delegateTypeRef = null;
            Type genericDelegateType = typeof(HotFixBridge);

            if (genericDelegateType != null)
            {
                delegateTypeRef = type.Module.Types.Single(x => x.Name == "HotFixBridge");
                string delegateFieldName = GenerateMethodName(method);
                FieldDefinition item = new FieldDefinition(delegateFieldName, FieldAttributes.Static | FieldAttributes.Public, delegateTypeRef);
                FieldReference parameter = item.FieldType.Resolve().Fields.Single(field => field.Name == "Parameters");

                type.Fields.Add(item);

                var invokeDeclare = type.Module.ImportReference(genericDelegateType.GetMethod("Invoke"));
                if (!method.HasBody)
                    return;
                var insertPoint = method.Body.Instructions[0];
                var ilGenerator = method.Body.GetILProcessor();
                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ldsfld, item));
                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Brfalse, insertPoint));

                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ldsfld, item));
                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ldfld, parameter));
                ilGenerator.InsertBefore(insertPoint, CreateLoadIntConst(ilGenerator, 0));
                if (method.IsStatic)
                {
                    ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ldnull));
                }
                else
                {
                    ilGenerator.InsertBefore(insertPoint, CreateLoadArg(ilGenerator, 0));
                }
                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Stelem_Ref));

                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ldsfld, item));
                    ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ldfld, parameter));

                    int index = (i + (method.IsStatic ? 0 : 1));
                    ilGenerator.InsertBefore(insertPoint, CreateLoadIntConst(ilGenerator, i + 1));
                    ilGenerator.InsertBefore(insertPoint, CreateLoadArg(ilGenerator, index));

                    if(method.Parameters[i].ParameterType.IsValueType)
                    {
                        ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Box, method.Parameters[i].ParameterType));
                    }

                    ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Stelem_Ref));
                }

                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ldsfld, item));
                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Call, invokeDeclare));

                if(method.ReturnType.Name == "Void")
                    ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Pop));
                else if(method.ReturnType.IsValueType)
                {
                    ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Unbox_Any, method.ReturnType));
                }
                ilGenerator.InsertBefore(insertPoint, ilGenerator.Create(OpCodes.Ret));
            }
        }

        public static Instruction CreateLoadIntConst(ILProcessor ilGenerator ,int c)
        {
            switch(c)
            {
                case 0:
                    return ilGenerator.Create(OpCodes.Ldc_I4_0);
                case 1:
                    return ilGenerator.Create(OpCodes.Ldc_I4_1);
                case 2:
                    return ilGenerator.Create(OpCodes.Ldc_I4_2);
                case 3:
                    return ilGenerator.Create(OpCodes.Ldc_I4_3);
                case 4:
                    return ilGenerator.Create(OpCodes.Ldc_I4_4);
                case 5:
                    return ilGenerator.Create(OpCodes.Ldc_I4_5);
                case 6:
                    return ilGenerator.Create(OpCodes.Ldc_I4_6);
                case 7:
                    return ilGenerator.Create(OpCodes.Ldc_I4_7);
                case 8:
                    return ilGenerator.Create(OpCodes.Ldc_I4_8);
                case -1:
                    return ilGenerator.Create(OpCodes.Ldc_I4_M1);
            }
            if (c >= sbyte.MinValue && c <= sbyte.MaxValue)
                return ilGenerator.Create(OpCodes.Ldc_I4_S, (sbyte)c);

            return ilGenerator.Create(OpCodes.Ldc_I4, c);
        }
        public static Instruction CreateLoadArg(ILProcessor ilGenerator, int c)
        {
            switch (c)
            {
                case 0:
                    return ilGenerator.Create(OpCodes.Ldarg_0);
                case 1:
                    return ilGenerator.Create(OpCodes.Ldarg_1);
                case 2:
                    return ilGenerator.Create(OpCodes.Ldarg_2);
                case 3:
                    return ilGenerator.Create(OpCodes.Ldarg_3);
            }
            if (c > 0 && c < byte.MaxValue)
                return ilGenerator.Create(OpCodes.Ldarg_S, (byte)c);

            return ilGenerator.Create(OpCodes.Ldarg, c);
        }
        public static Type GetGenericDelegateType(MethodDefinition method)
        {
            try
            {
                if (method.ReturnType.Name == "Void") //Action
                {
                    int ActionParamCount = method.Parameters.Count + 1;
                    Type pattern = GetTypeByFullName("System.Action`" + ActionParamCount);
                    var paramTypes = new List<Type> { GetTypeByFullName(method.DeclaringType.FullName) };
                    paramTypes.AddRange(method.Parameters.Select(p => GetTypeByFullName(p.ParameterType.FullName) ?? typeof(object)));
                    return pattern.MakeGenericType(paramTypes.ToArray());
                }
                else //Func
                {
                    int ActionParamCount = method.Parameters.Count + 2;
                    Type pattern = GetTypeByFullName("System.Func`" + ActionParamCount);
                    var paramTypes = new List<Type> { GetTypeByFullName(method.DeclaringType.FullName) };
                    paramTypes.AddRange(method.Parameters.Select(
                        p =>
                        GetTypeByFullName(p.ParameterType.FullName) ?? typeof(object)
                        ));
                    paramTypes.Add(GetTypeByFullName(method.ReturnType.FullName));
                    return pattern.MakeGenericType(paramTypes.ToArray());
                }
            }
            catch (Exception )
            {
                return null;
            }
        }

        public static Type GetTypeByFullName(string fullName)
        {
            Type type = Type.GetType(fullName);
            if (type != null)
                return type;

            return null;
            //return externalAssembly.GetType(fullName);
        }

        public static string GenerateMethodName(MethodDefinition method)
        {
            string delegateFieldName = "__" + method.Name;
            for(int i = 0; i < method.Parameters.Count; i++)
            {
                delegateFieldName += "_" + method.Parameters[i].ParameterType.Name;
            }
            delegateFieldName += "__Delegate";
            delegateFieldName = delegateFieldName.Replace(".", "_").Replace("`", "_");

            return delegateFieldName;
        }
    }
}
