using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;

// Man, I just want these warnings gone. This needs to be entirely rewritten anyway.
#pragma warning disable CA1051 // Do not declare visible instance fields
#nullable disable

namespace MonoMod.RuntimeDetour.HookGen
{
    public class HookGenerator
    {
        static readonly Dictionary<Type, string> ReflTypeNameMap = new Dictionary<Type, string>() {
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(sbyte), "sbyte" },
            { typeof(float), "float" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };
        static readonly Dictionary<string, string> TypeNameMap = new Dictionary<string, string>();

        static HookGenerator()
        {
            foreach (var pair in ReflTypeNameMap)
                TypeNameMap[pair.Key.FullName] = pair.Value;
        }

        public MonoModder Modder;

        public ModuleDefinition OutputModule;

        public string Namespace;
        public string NamespaceIL;
        public bool HookOrig;
        public bool HookPrivate;
        public string HookExtName;

        public ModuleDefinition module_RuntimeDetour;
        public ModuleDefinition module_Utils;

        public TypeReference t_MulticastDelegate;
        public TypeReference t_IAsyncResult;
        public TypeReference t_AsyncCallback;
        public TypeReference t_MethodBase;
        public TypeReference t_RuntimeMethodHandle;
        public TypeReference t_EditorBrowsableState;

        public MethodReference m_Object_ctor;
        public MethodReference m_ObsoleteAttribute_ctor;
        public MethodReference m_EditorBrowsableAttribute_ctor;

        public MethodReference m_GetMethodFromHandle;
        public MethodReference m_Add;
        public MethodReference m_Remove;
        public MethodReference m_Modify;
        public MethodReference m_Unmodify;

        public TypeReference t_ILManipulator;

        public HookGenerator(MonoModder modder, string name)
        {
            Modder = modder;

            OutputModule = ModuleDefinition.CreateModule(name, new ModuleParameters
            {
                Architecture = modder.Module.Architecture,
                AssemblyResolver = modder.Module.AssemblyResolver,
                Kind = ModuleKind.Dll,
                Runtime = modder.Module.Runtime
            });

            // Copy all assembly references from the input module.
            // Cecil + .NET Standard libraries + .NET 5.0 = weirdness.
            modder.MapDependencies();

			// Removed for tML, better to only add dependencies as needed via the resolver
            // OutputModule.AssemblyReferences.AddRange(modder.Module.AssemblyReferences);
            // modder.DependencyMap[OutputModule] = new List<ModuleDefinition>(modder.DependencyMap[modder.Module]);

            Namespace = Environment.GetEnvironmentVariable("MONOMOD_HOOKGEN_NAMESPACE");
            if (string.IsNullOrEmpty(Namespace))
                Namespace = "On";
            NamespaceIL = Environment.GetEnvironmentVariable("MONOMOD_HOOKGEN_NAMESPACE_IL");
            if (string.IsNullOrEmpty(NamespaceIL))
                NamespaceIL = "IL";
            HookOrig = Environment.GetEnvironmentVariable("MONOMOD_HOOKGEN_ORIG") == "1";
            HookPrivate = Environment.GetEnvironmentVariable("MONOMOD_HOOKGEN_PRIVATE") == "1";

            modder.MapDependency(modder.Module, "MonoMod.RuntimeDetour");
            if (!modder.DependencyCache.TryGetValue("MonoMod.RuntimeDetour", out module_RuntimeDetour))
                throw new FileNotFoundException("MonoMod.RuntimeDetour not found!");

            modder.MapDependency(modder.Module, "MonoMod.Utils");
            if (!modder.DependencyCache.TryGetValue("MonoMod.Utils", out module_Utils))
                throw new FileNotFoundException("MonoMod.Utils not found!");

            t_MulticastDelegate = OutputModule.ImportReference(modder.FindType("System.MulticastDelegate"));
            t_IAsyncResult = OutputModule.ImportReference(modder.FindType("System.IAsyncResult"));
            t_AsyncCallback = OutputModule.ImportReference(modder.FindType("System.AsyncCallback"));
            t_MethodBase = OutputModule.ImportReference(modder.FindType("System.Reflection.MethodBase"));
            t_RuntimeMethodHandle = OutputModule.ImportReference(modder.FindType("System.RuntimeMethodHandle"));
            t_EditorBrowsableState = OutputModule.ImportReference(modder.FindType("System.ComponentModel.EditorBrowsableState"));

            var td_HookEndpointManager = module_RuntimeDetour.GetType("MonoMod.RuntimeDetour.HookGen.HookEndpointManager");

            t_ILManipulator = OutputModule.ImportReference(
                module_Utils.GetType("MonoMod.Cil.ILContext/Manipulator")
            );

            m_Object_ctor = OutputModule.ImportReference(modder.FindType("System.Object").Resolve().FindMethod("System.Void .ctor()"));
            m_ObsoleteAttribute_ctor = OutputModule.ImportReference(modder.FindType("System.ObsoleteAttribute").Resolve().FindMethod("System.Void .ctor(System.String,System.Boolean)"));
            m_EditorBrowsableAttribute_ctor = OutputModule.ImportReference(modder.FindType("System.ComponentModel.EditorBrowsableAttribute").Resolve().FindMethod("System.Void .ctor(System.ComponentModel.EditorBrowsableState)"));

            m_GetMethodFromHandle = OutputModule.ImportReference(
                new MethodReference("GetMethodFromHandle", t_MethodBase, t_MethodBase)
                {
                    Parameters = {
                        new ParameterDefinition(t_RuntimeMethodHandle)
                    }
                }
            );
            m_Add = OutputModule.ImportReference(td_HookEndpointManager.FindMethod("Add"));
            m_Remove = OutputModule.ImportReference(td_HookEndpointManager.FindMethod("Remove"));
            m_Modify = OutputModule.ImportReference(td_HookEndpointManager.FindMethod("Modify"));
            m_Unmodify = OutputModule.ImportReference(td_HookEndpointManager.FindMethod("Unmodify"));

        }

        public void Generate()
        {
            foreach (var type in Modder.Module.Types)
            {
                GenerateFor(type, out var hookType, out var hookILType);
                if (hookType == null || hookILType == null || hookType.IsNested)
                    continue;
                OutputModule.Types.Add(hookType);
                OutputModule.Types.Add(hookILType);
            }
        }

        public void GenerateFor(TypeDefinition type, out TypeDefinition hookType, out TypeDefinition hookILType)
        {
            hookType = hookILType = null;

            if (type.HasGenericParameters ||
                type.IsRuntimeSpecialName ||
                type.Name.StartsWith("<", StringComparison.Ordinal))
                return;

            if (!HookPrivate && type.IsNotPublic)
                return;

            Modder.LogVerbose($"[HookGen] Generating for type {type.FullName}");

            hookType = new TypeDefinition(
                type.IsNested ? null : (Namespace + (string.IsNullOrEmpty(type.Namespace) ? "" : ("." + type.Namespace))),
                type.Name,
                (type.IsNested ? TypeAttributes.NestedPublic : TypeAttributes.Public) | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class,
                OutputModule.TypeSystem.Object
            );

            hookILType = new TypeDefinition(
                type.IsNested ? null : (NamespaceIL + (string.IsNullOrEmpty(type.Namespace) ? "" : ("." + type.Namespace))),
                type.Name,
                (type.IsNested ? TypeAttributes.NestedPublic : TypeAttributes.Public) | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class,
                OutputModule.TypeSystem.Object
            );

            var add = false;

            foreach (var method in type.Methods) {
                add |= GenerateFor(hookType, hookILType, method);
                if (method.HasCustomAttribute("Terraria.ModLoader.OriginalOverloadAttribute"))
                    add |= GenerateFor(hookType, hookILType, method, skipOverloadSuffix: true);
            }

            foreach (var nested in type.NestedTypes)
            {
                GenerateFor(nested, out var hookNestedType, out var hookNestedILType);
                if (hookNestedType == null || hookNestedILType == null)
                    continue;
                add = true;
                hookType.NestedTypes.Add(hookNestedType);
                hookILType.NestedTypes.Add(hookNestedILType);
            }

            if (!add)
            {
                hookType = hookILType = null;
            }
        }

        public bool GenerateFor(TypeDefinition hookType, TypeDefinition hookILType, MethodDefinition method, bool skipOverloadSuffix = false)
        {
            if (method.HasGenericParameters ||
                method.IsAbstract ||
                (method.IsSpecialName && !method.IsConstructor))
                return false;

            if (!HookOrig && method.Name.StartsWith("orig_", StringComparison.Ordinal))
                return false;
            if (!HookPrivate && method.IsPrivate)
                return false;

            var name = HookGenerator.GetFriendlyName(method);
            var suffix = true;
            if (method.Parameters.Count == 0)
            {
                suffix = false;
            }

            IEnumerable<MethodDefinition> overloads = null;
            if (suffix)
            {
                overloads = method.DeclaringType.Methods.Where(other => !other.HasGenericParameters && HookGenerator.GetFriendlyName(other) == name && other != method);
                if (!overloads.Any() || skipOverloadSuffix)
                {
                    suffix = false;
                }
                if (skipOverloadSuffix) {
                    if (overloads.Any(x => x.HasCustomAttribute("Terraria.ModLoader.OriginalOverloadAttribute"))) {
                          throw new Exception($"Multiple overloads for {method.DeclaringType.Name}.{method.Name} with OriginalOverloadAttribute");
                    }
                }
            }

            if (suffix)
            {
                var builder = new StringBuilder();
                for (var parami = 0; parami < method.Parameters.Count; parami++)
                {
                    var param = method.Parameters[parami];
                    if (!TypeNameMap.TryGetValue(param.ParameterType.FullName, out var typeName))
                        typeName = GetFriendlyName(param.ParameterType, false);

                    if (overloads.Any(other =>
                    {
                        var otherParam = other.Parameters.ElementAtOrDefault(parami);
                        return
                            otherParam != null &&
                            GetFriendlyName(otherParam.ParameterType, false) == typeName &&
                            otherParam.ParameterType.Namespace != param.ParameterType.Namespace;
                    }))
                        typeName = GetFriendlyName(param.ParameterType, true);

                    builder.Append('_');
                    builder.Append(typeName.Replace(".", "", StringComparison.Ordinal).Replace("`", "", StringComparison.Ordinal));
                }
                name += builder.ToString();
            }

            if (hookType.FindEvent(name) != null)
            {
                string nameTmp;
                for (
                    var i = 1;
                    hookType.FindEvent(nameTmp = name + "_" + i) != null;
                    i++
                )
                    ;
                name = nameTmp;
            }

            bool obsolete = method.HasCustomAttribute("System.ObsoleteAttribute"); // Do we need to check if error is true?

            // TODO: Fix possible conflict when other members with the same names exist.

            var delOrig = GenerateDelegateFor(method);
            delOrig.Name = "orig_" + name;
            delOrig.CustomAttributes.Add(GenerateEditorBrowsable(EditorBrowsableState.Never));
            hookType.NestedTypes.Add(delOrig);

            var delHook = GenerateDelegateFor(method);
            delHook.Name = "hook_" + name;
            var delHookInvoke = delHook.FindMethod("Invoke");
            delHookInvoke.Parameters.Insert(0, new ParameterDefinition("orig", ParameterAttributes.None, delOrig));
            var delHookBeginInvoke = delHook.FindMethod("BeginInvoke");
            delHookBeginInvoke.Parameters.Insert(0, new ParameterDefinition("orig", ParameterAttributes.None, delOrig));
            delHook.CustomAttributes.Add(GenerateEditorBrowsable(EditorBrowsableState.Never));
            hookType.NestedTypes.Add(delHook);

            ILProcessor il;
            GenericInstanceMethod endpointMethod;

            var methodRef = OutputModule.ImportReference(method);

            #region Hook

            var addHook = new MethodDefinition(
                "add_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            addHook.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, delHook));
            addHook.Body = new MethodBody(addHook);
            il = addHook.Body.GetILProcessor();
            if (obsolete) {
                if (LogObsoleteHookSubscribed != null) {
                    il.Emit(OpCodes.Ldtoken, methodRef);
                    il.Emit(OpCodes.Call, m_GetMethodFromHandle);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, LogObsoleteHookSubscribed);
                }
                il.Emit(OpCodes.Ret);
            }
            else {
                il.Emit(OpCodes.Ldtoken, methodRef);
                il.Emit(OpCodes.Call, m_GetMethodFromHandle);
                il.Emit(OpCodes.Ldarg_0);
                endpointMethod = new GenericInstanceMethod(m_Add);
                endpointMethod.GenericArguments.Add(delHook);
                il.Emit(OpCodes.Call, endpointMethod);
                il.Emit(OpCodes.Ret);
            }
            hookType.Methods.Add(addHook);

            var removeHook = new MethodDefinition(
                "remove_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            removeHook.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, delHook));
            removeHook.Body = new MethodBody(removeHook);
            il = removeHook.Body.GetILProcessor();
            if (obsolete) {
                il.Emit(OpCodes.Ret);
            }
            else { 
                il.Emit(OpCodes.Ldtoken, methodRef);
                il.Emit(OpCodes.Call, m_GetMethodFromHandle);
                il.Emit(OpCodes.Ldarg_0);
                endpointMethod = new GenericInstanceMethod(m_Remove);
                endpointMethod.GenericArguments.Add(delHook);
                il.Emit(OpCodes.Call, endpointMethod);
                il.Emit(OpCodes.Ret);
            }
            hookType.Methods.Add(removeHook);

            var evHook = new EventDefinition(name, EventAttributes.None, delHook)
            {
                AddMethod = addHook,
                RemoveMethod = removeHook
            };
            if (obsolete) {
                evHook.CustomAttributes.Add(GenerateObsolete("The hooked method is Obsolete, this detour should not be used.", true));
            }
            hookType.Events.Add(evHook);

            #endregion

            #region Hook IL

            var addIL = new MethodDefinition(
                "add_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            addIL.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, t_ILManipulator));
            addIL.Body = new MethodBody(addIL);
            il = addIL.Body.GetILProcessor();
            if (obsolete) {
                if (LogObsoleteHookSubscribed != null) {
                    il.Emit(OpCodes.Ldtoken, methodRef);
                    il.Emit(OpCodes.Call, m_GetMethodFromHandle);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, LogObsoleteHookSubscribed);
                }
                il.Emit(OpCodes.Ret);
            }
            else {
                il.Emit(OpCodes.Ldtoken, methodRef);
                il.Emit(OpCodes.Call, m_GetMethodFromHandle);
                il.Emit(OpCodes.Ldarg_0);
                endpointMethod = new GenericInstanceMethod(m_Modify);
                endpointMethod.GenericArguments.Add(delHook);
                il.Emit(OpCodes.Call, endpointMethod);
                il.Emit(OpCodes.Ret);
            }
            hookILType.Methods.Add(addIL);

            var removeIL = new MethodDefinition(
                "remove_" + name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static,
                OutputModule.TypeSystem.Void
            );
            removeIL.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, t_ILManipulator));
            removeIL.Body = new MethodBody(removeIL);
            il = removeIL.Body.GetILProcessor();
            if (obsolete) {
                il.Emit(OpCodes.Ret);
            }
            else {
                il.Emit(OpCodes.Ldtoken, methodRef);
                il.Emit(OpCodes.Call, m_GetMethodFromHandle);
                il.Emit(OpCodes.Ldarg_0);
                endpointMethod = new GenericInstanceMethod(m_Unmodify);
                endpointMethod.GenericArguments.Add(delHook);
                il.Emit(OpCodes.Call, endpointMethod);
                il.Emit(OpCodes.Ret);
            }
            hookILType.Methods.Add(removeIL);

            var evIL = new EventDefinition(name, EventAttributes.None, t_ILManipulator)
            {
                AddMethod = addIL,
                RemoveMethod = removeIL
            };
            if (obsolete) {
                evIL.CustomAttributes.Add(GenerateObsolete("The hooked method is Obsolete, this IL edit should not be used.", true));
            }
            hookILType.Events.Add(evIL);

            #endregion

            return true;
        }

        public TypeDefinition GenerateDelegateFor(MethodDefinition method)
        {
            var name = HookGenerator.GetFriendlyName(method);
            var index = method.DeclaringType.Methods.Where(other => !other.HasGenericParameters && HookGenerator.GetFriendlyName(other) == name).ToList().IndexOf(method);
            if (index != 0)
            {
                var suffix = index.ToString(CultureInfo.InvariantCulture);
                do
                {
                    name = name + "_" + suffix;
                } while (method.DeclaringType.Methods.Any(other => !other.HasGenericParameters && HookGenerator.GetFriendlyName(other) == (name + suffix)));
            }
            name = "d_" + name;

            var del = new TypeDefinition(
                null, null,
                TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.Class,
                t_MulticastDelegate
            );

            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.ReuseSlot,
                OutputModule.TypeSystem.Void
            )
            {
                ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
                HasThis = true
            };
            ctor.Parameters.Add(new ParameterDefinition(OutputModule.TypeSystem.Object));
            ctor.Parameters.Add(new ParameterDefinition(OutputModule.TypeSystem.IntPtr));
            ctor.Body = new MethodBody(ctor);
            del.Methods.Add(ctor);

            var invoke = new MethodDefinition(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                ImportVisible(method.ReturnType)
            )
            {
                ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
                HasThis = true
            };
            if (!method.IsStatic)
            {
                var selfType = ImportVisible(method.DeclaringType);
                if (method.DeclaringType.IsValueType)
                    selfType = new ByReferenceType(selfType);
                invoke.Parameters.Add(new ParameterDefinition("self", ParameterAttributes.None, selfType));
            }
            foreach (var param in method.Parameters)
                invoke.Parameters.Add(new ParameterDefinition(
                    param.Name,
                    param.Attributes & ~ParameterAttributes.Optional & ~ParameterAttributes.HasDefault,
                    ImportVisible(param.ParameterType)
                ));
            invoke.Body = new MethodBody(invoke);
            del.Methods.Add(invoke);

            var invokeBegin = new MethodDefinition(
                "BeginInvoke",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                t_IAsyncResult
            )
            {
                ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
                HasThis = true
            };
            foreach (var param in invoke.Parameters)
                invokeBegin.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
            invokeBegin.Parameters.Add(new ParameterDefinition("callback", ParameterAttributes.None, t_AsyncCallback));
            invokeBegin.Parameters.Add(new ParameterDefinition(null, ParameterAttributes.None, OutputModule.TypeSystem.Object));
            invokeBegin.Body = new MethodBody(invokeBegin);
            del.Methods.Add(invokeBegin);

            var invokeEnd = new MethodDefinition(
                "EndInvoke",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                OutputModule.TypeSystem.Object
            )
            {
                ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed,
                HasThis = true
            };
            invokeEnd.Parameters.Add(new ParameterDefinition("result", ParameterAttributes.None, t_IAsyncResult));
            invokeEnd.Body = new MethodBody(invokeEnd);
            del.Methods.Add(invokeEnd);

            return del;
        }

        static string GetFriendlyName(MethodReference method)
        {
            var name = method.Name;
            if (name.StartsWith(".", StringComparison.Ordinal))
                name = name.Substring(1);
            name = name.Replace('.', '_');
            return name;
        }

        string GetFriendlyName(TypeReference type, bool full)
        {
            if (type is TypeSpecification)
            {
                var builder = new StringBuilder();
                BuildFriendlyName(builder, type, full);
                return builder.ToString();
            }

            return full ? type.FullName : type.Name;
        }
        void BuildFriendlyName(StringBuilder builder, TypeReference type, bool full)
        {
            if (!(type is TypeSpecification))
            {
                builder.Append((full ? type.FullName : type.Name).Replace("_", "", StringComparison.Ordinal));
                return;
            }

            if (type.IsByReference)
            {
                builder.Append("ref");
            }
            else if (type.IsPointer)
            {
                builder.Append("ptr");
            }

            BuildFriendlyName(builder, ((TypeSpecification)type).ElementType, full);

            if (type.IsArray)
            {
                builder.Append("Array");
            }
        }

        static bool IsPublic(TypeDefinition typeDef)
        {
            return typeDef != null && (typeDef.IsNestedPublic || typeDef.IsPublic) && !typeDef.IsNotPublic;
        }

        bool HasPublicArgs(GenericInstanceType typeGen)
        {
            foreach (var arg in typeGen.GenericArguments)
            {
                // Generic parameter references are local.
                if (arg.IsGenericParameter)
                    return false;

                if (arg is GenericInstanceType argGen && !HasPublicArgs(argGen))
                    return false;

                if (!HookGenerator.IsPublic(arg.SafeResolve()))
                    return false;
            }

            return true;
        }

        TypeReference ImportVisible(TypeReference typeRef)
        {
            // Check if the declaring type is accessible.
            // If not, use its base type instead.
            // Note: This will break down with type specifications!
            var type = typeRef?.SafeResolve();
            goto Try;

            Retry:
            typeRef = type.BaseType;
            type = typeRef?.SafeResolve();

            Try:
            if (type == null) // Unresolvable - probably private anyway.
                return OutputModule.TypeSystem.Object;

            // Generic instance types are special. Try to match them exactly or baseify them.
            if (typeRef is GenericInstanceType typeGen && !HasPublicArgs(typeGen))
                goto Retry;

            // Check if the type and all of its parents are public.
            // Generic return / param types are too complicated at the moment and will be simplified.
            for (var parent = type; parent != null; parent = parent.DeclaringType)
            {
                if (HookGenerator.IsPublic(parent) && (parent == type || !parent.HasGenericParameters))
                    continue;
                // If it isn't public, ...

                if (type.IsEnum)
                {
                    // ... try the enum's underlying type.
                    typeRef = type.FindField("value__").FieldType;
                    break;
                }

                // ... try the base type.
                goto Retry;
            }

            try
            {
                return OutputModule.ImportReference(typeRef);
            }
            catch
            {
                // Under rare circumstances, ImportReference can fail, f.e. Private<K> : Public<K, V>
                return OutputModule.TypeSystem.Object;
            }
        }

        CustomAttribute GenerateObsolete(string message, bool error)
        {
            var attrib = new CustomAttribute(m_ObsoleteAttribute_ctor);
            attrib.ConstructorArguments.Add(new CustomAttributeArgument(OutputModule.TypeSystem.String, message));
            attrib.ConstructorArguments.Add(new CustomAttributeArgument(OutputModule.TypeSystem.Boolean, error));
            return attrib;
        }

        CustomAttribute GenerateEditorBrowsable(EditorBrowsableState state)
        {
            var attrib = new CustomAttribute(m_EditorBrowsableAttribute_ctor);
            attrib.ConstructorArguments.Add(new CustomAttributeArgument(t_EditorBrowsableState, state));
            return attrib;
        }

        MethodDefinition LogObsoleteHookSubscribed;

        public void GenerateObsoleteLogging(Terraria.ModLoader.Setup.Core.HookGenTask.ProgressReportingMonoModder modder)
        {
            // Generated on https://cecilifier.me/
            /*
            using System;
            using System.Reflection;

            namespace TerrariaHooks {
                public static class Logging
                {
                    public static Action<MethodBase, Delegate> OnObsoleteHookSubscribed;

                    public static void LogObsoleteHookSubscribed(MethodBase methodBase, Delegate methodDelegate) {
                        if(OnObsoleteHookSubscribed != null) {
                            OnObsoleteHookSubscribed.Invoke(methodBase, methodDelegate);
                        }
                    }
                }
            }
            */

            var cls_Logging_0 = new TypeDefinition("TerrariaHooks", "Logging", TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, OutputModule.TypeSystem.Object);
            OutputModule.Types.Add(cls_Logging_0);

            OutputModule.ImportReference(modder.FindType("System.Reflection.MethodBase").Resolve());
            OutputModule.ImportReference(modder.FindType("System.Delegate").Resolve());

            var fld_OnObsoleteHookSubscribed_1 = new FieldDefinition("OnObsoleteHookSubscribed", FieldAttributes.Public | FieldAttributes.Static, OutputModule.ImportReference(modder.FindType("System.Action`2").Resolve().MakeGenericInstanceType(modder.FindType("System.Reflection.MethodBase").Resolve(), modder.FindType("System.Delegate").Resolve())));
            //var fld_OnObsoleteHookSubscribed_1 = new FieldDefinition("OnObsoleteHookSubscribed", FieldAttributes.Public | FieldAttributes.Static, OutputModule.ImportReference(typeof(System.Action<>)).MakeGenericInstanceType(OutputModule.TypeSystem.String)); // Generated code that causes assembly reference issue

            cls_Logging_0.Fields.Add(fld_OnObsoleteHookSubscribed_1);

            //Method : LogObsoleteHookSubscribed
            var md_LogObsoleteHookSubscribed_2 = new MethodDefinition("LogObsoleteHookSubscribed", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, OutputModule.TypeSystem.Void);
            cls_Logging_0.Methods.Add(md_LogObsoleteHookSubscribed_2);
            md_LogObsoleteHookSubscribed_2.Body.InitLocals = true;
            var il_LogObsoleteHookSubscribed_3 = md_LogObsoleteHookSubscribed_2.Body.GetILProcessor();

            //Parameters of 'public static void LogObsoleteHookSubscribed(MethodBase methodBase, Delegate methodDelegate) {...'
            var p_MethodBase_4 = new ParameterDefinition("methodBase", ParameterAttributes.None, OutputModule.ImportReference(modder.FindType("System.Reflection.MethodBase").Resolve()));
            md_LogObsoleteHookSubscribed_2.Parameters.Add(p_MethodBase_4);
            var p_MethodDelegate_5 = new ParameterDefinition("methodDelegate", ParameterAttributes.None, OutputModule.ImportReference(modder.FindType("System.Delegate").Resolve()));
            md_LogObsoleteHookSubscribed_2.Parameters.Add(p_MethodDelegate_5);

            LogObsoleteHookSubscribed = md_LogObsoleteHookSubscribed_2; // Store reference for later

            //if(OnObsoleteHookSubscribed != null) {...
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ldsfld, fld_OnObsoleteHookSubscribed_1);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ldnull);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ceq);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ldc_I4_0);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ceq);
            var lbl_ElseEntryPoint_5 = il_LogObsoleteHookSubscribed_3.Create(OpCodes.Nop);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Brfalse, lbl_ElseEntryPoint_5);
            //if body

            //OnObsoleteHookSubscribed.Invoke(methodBase, methodDelegate);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ldsfld, fld_OnObsoleteHookSubscribed_1);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ldarg_0);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ldarg_1);
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Callvirt, OutputModule.ImportReference(TypeHelpers.ResolveMethod(typeof(System.Action<System.Reflection.MethodBase, System.Delegate>), "Invoke", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "System.Reflection.MethodBase", "System.Delegate")));
            var lbl_ElseEnd_6 = il_LogObsoleteHookSubscribed_3.Create(OpCodes.Nop);
            il_LogObsoleteHookSubscribed_3.Append(lbl_ElseEntryPoint_5);
            il_LogObsoleteHookSubscribed_3.Append(lbl_ElseEnd_6);
            md_LogObsoleteHookSubscribed_2.Body.OptimizeMacros();
            // end if (if(OnObsoleteHookSubscribed != null) {...)
            il_LogObsoleteHookSubscribed_3.Emit(OpCodes.Ret);

            //** Constructor: Logging() **
            var ctor_Logging_7 = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, OutputModule.TypeSystem.Void);
            cls_Logging_0.Methods.Add(ctor_Logging_7);
            var il_Ctor_Logging_8 = ctor_Logging_7.Body.GetILProcessor();
            il_Ctor_Logging_8.Emit(OpCodes.Ldarg_0);
            il_Ctor_Logging_8.Emit(OpCodes.Call, OutputModule.ImportReference(TypeHelpers.DefaultCtorFor(cls_Logging_0.BaseType)));
            il_Ctor_Logging_8.Emit(OpCodes.Ret);
        }
    }

    public class TypeHelpers
    {
        public static MethodReference DefaultCtorFor(TypeReference type)
        {
            var resolved = type.Resolve();
            if (resolved == null)
                return null;

            var ctor = resolved.Methods.SingleOrDefault(m => m.IsConstructor && m.Parameters.Count == 0 && !m.IsStatic);
            if (ctor == null)
                return DefaultCtorFor(resolved.BaseType);

            return new MethodReference(".ctor", type.Module.TypeSystem.Void, type) { HasThis = true };
        }

        public static System.Reflection.MethodBase ResolveMethod(Type declaringType, string methodName, System.Reflection.BindingFlags bindingFlags, params string[] paramTypes)
        {
            if (methodName == ".ctor") {
                var resolvedCtor = declaringType.GetConstructor(
                    bindingFlags,
                    null,
                    paramTypes.Select(Type.GetType).ToArray(),
                    null);

                if (resolvedCtor == null) {
                    throw new InvalidOperationException($"Failed to resolve ctor [{declaringType}({string.Join(',', paramTypes)})");
                }

                return resolvedCtor;
            }

            var resolvedMethod = declaringType.GetMethod(methodName,
                bindingFlags,
                null,
                paramTypes.Select(Type.GetType).ToArray(),
                null);

            if (resolvedMethod == null) {
                throw new InvalidOperationException($"Failed to resolve method {declaringType}.{methodName}({string.Join(',', paramTypes)})");
            }

            return resolvedMethod;
        }
    }
}

#nullable restore