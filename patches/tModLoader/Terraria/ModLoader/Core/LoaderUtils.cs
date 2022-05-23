﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.Core
{
	internal static class LoaderUtils
	{
		/// <summary> Calls static constructors on the provided type and, optionally, its nested types. </summary>
		public static void ResetStaticMembers(Type type, bool recursive) {
#if NETCORE
			var typeInitializer = type.TypeInitializer;

			if (typeInitializer != null) {
				var field = typeInitializer
					.GetType()
					.GetField("m_invocationFlags", BindingFlags.NonPublic | BindingFlags.Instance);

				object previousValue = field.GetValue(typeInitializer);

				//.NET Core uses invocation flags on static constructor to ensure that they're never called twice. We'll have to ignore the law, and remove these flags temporarily.
				field.SetValue(typeInitializer, (uint)0x00000001); //INVOCATION_FLAGS_INITIALIZED

				typeInitializer.Invoke(null, null);

				field.SetValue(typeInitializer, previousValue);
			}
#else
			type.TypeInitializer?.Invoke(null, null);
#endif

			if (recursive) {
				foreach (var nestedType in type.GetNestedTypes()) {
					ResetStaticMembers(nestedType, recursive);
				}
			}
		}

		public static void ForEachAndAggregateExceptions<T>(IEnumerable<T> enumerable, Action<T> action) {
			var exceptions = new List<Exception>();
			foreach (var t in enumerable) {
				try {
					action(t);
				}
				catch (Exception ex) {
					exceptions.Add(ex);
				}
			}

			if (exceptions.Count == 1)
				throw exceptions[0];

			if (exceptions.Count > 0)
				throw new MultipleException(exceptions);
		}

		public static void InstantiateGlobals<TGlobal, TEntity>(TEntity entity, IEnumerable<TGlobal> globals, ref Instanced<TGlobal>[] entityGlobals, Action midInstantiationAction) where TGlobal : GlobalType<TEntity, TGlobal> {
			entityGlobals = globals
				.Where(g => g.AppliesToEntity(entity, false))
				.Select(g => new Instanced<TGlobal>(g.index, g.NewInstance(entity)))
				.ToArray();

			midInstantiationAction();

			//Could potentially be sped up.
			var entityGlobalsCopy = entityGlobals;
			var lateInitGlobals = globals
				.Where(g => !entityGlobalsCopy.Any(i => i.Index == g.index) && g.AppliesToEntity(entity, true))
				.Select(g => new Instanced<TGlobal>(g.index, g.NewInstance(entity)));

			entityGlobals = entityGlobals
				.Union(lateInitGlobals)
				.OrderBy(i => i.Index)
				.ToArray();
		}

		public static bool HasMethod(Type type, Type declaringType, string method, params Type[] args) {
			var methodInfo = type.GetMethod(method, args);

			if (methodInfo == null)
				return false;

			return methodInfo.DeclaringType != declaringType;
		}

		public static MethodInfo ToMethodInfo<T, F>(this Expression<Func<T, F>> expr) where F : Delegate {
			MethodInfo method;

			try {
				var convert = expr.Body as UnaryExpression;
				var makeDelegate = convert.Operand as MethodCallExpression;
				var methodArg = makeDelegate.Object as ConstantExpression;
				method = methodArg.Value as MethodInfo;
				if (method == null)
					throw new NullReferenceException();
			}
			catch (Exception e) {
				throw new ArgumentException("Invalid hook expression " + expr, e);
			}

			return method;
		}

		public static bool HasOverride(Type t, MethodInfo baseMethod) =>
			baseMethod.DeclaringType.IsInterface ? t.IsAssignableTo(baseMethod.DeclaringType) :
			t.GetMethods().Single(m => m.GetBaseDefinition() == baseMethod).DeclaringType != baseMethod.DeclaringType;

		public static bool HasOverride<T, F>(Type t, Expression<Func<T, F>> expr) where F : Delegate =>
			HasOverride(t, expr.ToMethodInfo());

		public static IEnumerable<T> WhereMethodIsOverridden<T>(this IEnumerable<T> providers, MethodInfo method) {
			if (!method.IsVirtual)
				throw new ArgumentException("Non-virtual method: " + method);

			return providers.Where(p => HasOverride(p.GetType(), method));
		}

		public static IEnumerable<T> WhereMethodIsOverridden<T, F>(this IEnumerable<T> providers, Expression<Func<T, F>> expr) where F : Delegate =>
			WhereMethodIsOverridden(providers, expr.ToMethodInfo());

		public static void MustOverrideTogether<T>(T t, params Expression<Func<T, Delegate>>[] methods) =>
			MustOverrideTogether(t.GetType(), methods.Select(m => m.ToMethodInfo()).ToArray());

		private static void MustOverrideTogether(Type type, params MethodInfo[] methods) {
			int c = methods.Count(m => HasOverride(type, m));
			
			if (c > 0 && c < methods.Length)
				throw new Exception($"{type} must override all of ({string.Join('/', methods.Select(m => m.Name))}) or none");
		}

		private static HashSet<Type> validatedTypes = new();

		internal static bool IsValidated(Type type) => !validatedTypes.Add(type);

		private static Dictionary<Type, bool> typeIsCloneable = new();
		
		internal static bool IsCloneable<T, F>(T t, Expression<Func<T, F>> cloneMethod) where F : Delegate {
			var rootCloneableType = typeof(T);
			var type = t.GetType();
			
			if (typeIsCloneable.TryGetValue(type, out var cloneable))
				return cloneable;

			bool hasReferenceTypedFieldsOnSubclass = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Any(f => f.DeclaringType.IsSubclassOf(rootCloneableType) && !f.FieldType.IsValueType);

			return typeIsCloneable[type] = !hasReferenceTypedFieldsOnSubclass || HasOverride(type, cloneMethod);
		}

		internal static void ClearTypeInfo() {
			validatedTypes.Clear();
			typeIsCloneable.Clear();
		}
	}
}
