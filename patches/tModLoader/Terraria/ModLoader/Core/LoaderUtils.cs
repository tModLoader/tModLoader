using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Terraria.ModLoader.Exceptions;

#nullable enable

namespace Terraria.ModLoader.Core;

public static class LoaderUtils
{
	/// <summary> Calls static constructors on the provided type and, optionally, its nested types. </summary>
	public static void ResetStaticMembers(Type type, bool recursive = true)
	{
#if NETCORE
		var typeInitializer = type.TypeInitializer;

		if (typeInitializer != null) {
			var field = typeInitializer
				.GetType()
				.GetField("m_invocationFlags", BindingFlags.NonPublic | BindingFlags.Instance)!;

			object? previousValue = field.GetValue(typeInitializer);

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

	public static void ForEachAndAggregateExceptions<T>(IEnumerable<T> enumerable, Action<T> action)
	{
		var exceptions = new List<Exception>();

		foreach (var e in enumerable) {
			try {
				action(e);
			}
			catch (Exception ex) {
				ex.Data["contentType"] = e is Type ? e : e.GetType();
				exceptions.Add(ex);
			}
		}

		if (exceptions.Count == 1)
			ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

		if (exceptions.Count > 0)
			throw new MultipleException(exceptions);
	}

	public static bool HasMethod(Type type, Type declaringType, string method, params Type[] args)
	{
		var methodInfo = type.GetMethod(method, args);

		if (methodInfo == null)
			return false;

		return methodInfo.DeclaringType != declaringType;
	}

	public static MethodInfo ToMethodInfo<T, F>(this Expression<Func<T, F>> expr) where F : Delegate
	{
		MethodInfo? method;

		try {
			var convert = expr.Body as UnaryExpression;
			var makeDelegate = convert?.Operand as MethodCallExpression;
			var methodArg = makeDelegate?.Object as ConstantExpression;

			method = methodArg?.Value as MethodInfo;

			if (method == null)
				throw new NullReferenceException();
		}
		catch (Exception e) {
			throw new ArgumentException("Invalid hook expression " + expr, e);
		}

		return method;
	}

	public static MethodInfo GetDerivedDefinition(Type t, MethodInfo baseMethod)
		=> t.GetMethods().Single(m => m.GetBaseDefinition() == baseMethod);

	public static bool HasOverride(Type t, MethodInfo baseMethod)
		=> baseMethod.DeclaringType!.IsInterface ? t.IsAssignableTo(baseMethod.DeclaringType) : GetDerivedDefinition(t, baseMethod).DeclaringType != baseMethod.DeclaringType;

	public static bool HasOverride<T>(T t, Expression<Func<T, Delegate>> expr) => HasOverride<T, Delegate>(t, expr);
	public static bool HasOverride<T, F>(T t, Expression<Func<T, F>> expr) where F : Delegate
		=> HasOverride(t!.GetType(), expr.ToMethodInfo());

	public static IEnumerable<T> WhereMethodIsOverridden<T>(this IEnumerable<T> providers, MethodInfo method)
	{
		if (!method.IsVirtual)
			throw new ArgumentException("Non-virtual method: " + method);

		return providers.Where(p => HasOverride(p.GetType(), method));
	}

	public static IEnumerable<T> WhereMethodIsOverridden<T>(this IEnumerable<T> providers, Expression<Func<T, Delegate>> expr) => WhereMethodIsOverridden<T, Delegate>(providers, expr);
	public static IEnumerable<T> WhereMethodIsOverridden<T, F>(this IEnumerable<T> providers, Expression<Func<T, F>> expr) where F : Delegate
		=> WhereMethodIsOverridden(providers, expr.ToMethodInfo());

	public static void MustOverrideTogether<T>(T t, params Expression<Func<T, Delegate>>[] methods)
		=> MustOverrideTogether(t!.GetType(), methods.Select(m => m.ToMethodInfo()).ToArray());

	private static void MustOverrideTogether(Type type, params MethodInfo[] methods)
	{
		int c = methods.Count(m => HasOverride(type, m));
		
		if (c > 0 && c < methods.Length)
			throw new Exception($"{type} must override all of ({string.Join('/', methods.Select(m => m.Name))}) or none");
	}

	private static readonly HashSet<Type> validatedTypes = new();

	internal static bool IsValidated(Type type)
		=> !validatedTypes.Add(type);

	static LoaderUtils()
	{
		TypeCaching.OnClear += validatedTypes.Clear;
	}
}
