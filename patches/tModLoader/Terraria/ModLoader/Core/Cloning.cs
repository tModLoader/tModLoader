using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader.Core;

public static class Cloning
{
	private class TypeCloningInfo
	{
		public Type type;
		public bool overridesClone;
		public FieldInfo[] fieldsWhichMightNeedDeepCloning;

		public TypeCloningInfo baseTypeInfo;
		public bool warnCheckDone;

		public bool IsCloneable => overridesClone || fieldsWhichMightNeedDeepCloning.Length == 0 && baseTypeInfo.IsCloneable;

		public void Warn()
		{
			if (warnCheckDone)
				return;

			if (!IsCloneable) {
				if (fieldsWhichMightNeedDeepCloning.Length == 0) {
					baseTypeInfo.Warn();
				}
				else {

					IEnumerable<FieldInfo> fields = fieldsWhichMightNeedDeepCloning;
					var b = baseTypeInfo;
					while (!b.overridesClone) {
						fields = fields.Concat(b.fieldsWhichMightNeedDeepCloning);
						b = b.baseTypeInfo;
					}

					var msg = $"{type.FullName} has reference fields ({string.Join(", ", fields.Select(f => f.Name))}) that may not be safe to share between clones." + Environment.NewLine +
							$"For deep-cloning, add a custom Clone override and make proper copies of these fields. If shallow (memberwise) cloning is acceptable, mark the fields with [{nameof(CloneByReference)}] or properties with [field: {nameof(CloneByReference)}]";
					Logging.tML.Warn(msg);
				}

			}
			warnCheckDone = true;
		}
	}

	private static Dictionary<Type, TypeCloningInfo> typeInfos = new();
	private static ConditionalWeakTable<Type, object> immutableTypes = new();

	public static bool IsCloneable<T>(T t, Expression<Func<T, Delegate>> cloneMethod) => IsCloneable<T, Delegate>(t, cloneMethod);
	public static bool IsCloneable<T, F>(T t, Expression<Func<T, F>> cloneMethod) where F : Delegate
	{
		var type = t.GetType();
		return typeInfos.TryGetValue(type, out var typeInfo) ? typeInfo.IsCloneable : ComputeInfo(t.GetType(), cloneMethod.ToMethodInfo()).IsCloneable;
	}

	public static bool IsCloneable(Type type, MethodInfo cloneMethod) => GetOrComputeInfo(type, cloneMethod).IsCloneable;

	private static TypeCloningInfo GetOrComputeInfo(Type type, MethodInfo cloneMethod) =>
		typeInfos.TryGetValue(type, out var typeInfo) ? typeInfo : ComputeInfo(type, cloneMethod);

	private static TypeCloningInfo ComputeInfo(Type type, MethodInfo cloneMethod)
	{
		var info = new TypeCloningInfo {
			type = type,
			overridesClone = LoaderUtils.GetDerivedDefinition(type, cloneMethod).DeclaringType == type
		};

		if (!info.overridesClone) {
			info.fieldsWhichMightNeedDeepCloning =
					type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					.Where(f => f.DeclaringType == type && !IsCloneByReference(f))
					.ToArray();
			info.baseTypeInfo = GetOrComputeInfo(type.BaseType, cloneMethod);
		}

		typeInfos[type] = info;
		return info;
	}

	private static bool IsCloneByReference(FieldInfo f)
	{
		return f.GetCustomAttribute<CloneByReference>() != null || IsCloneByReference(f.FieldType);
	}

	// note that value typed fields could still contain references... maybe detect later
	private static bool IsCloneByReference(Type type) => type.IsValueType || type.GetCustomAttribute<CloneByReference>() != null || IsImmutable(type);

	public static bool IsImmutable(Type type)
	{
		if (type.IsGenericType && !type.IsGenericTypeDefinition && IsImmutable(type.GetGenericTypeDefinition()))
			return true;

		lock (immutableTypes) {
			return immutableTypes.TryGetValue(type, out _);
		}
	}

	public static void AddImmutableType(Type type)
	{
		lock (immutableTypes) {
			immutableTypes.AddOrUpdate(type, null);
		}
	}

	public static void WarnNotCloneable(Type type) => typeInfos[type].Warn();

	static Cloning()
	{
		TypeCaching.OnClear += typeInfos.Clear;
		AddImmutableType(typeof(string));
		AddImmutableType(typeof(Asset<>));
	}
}
