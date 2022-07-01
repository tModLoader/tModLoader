using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.Core
{
	public static class LoaderUtils
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
				ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

			if (exceptions.Count > 0)
				throw new MultipleException(exceptions);
		}

		[Obsolete("Use ReadOnlySpan or List variant", true)]
		public static void InstantiateGlobals<TGlobal, TEntity>(TEntity entity, IEnumerable<TGlobal> globals, ref Instanced<TGlobal>[] entityGlobals, Action midInstantiationAction) where TGlobal : GlobalType<TEntity, TGlobal>
			=> InstantiateGlobals(entity, globals.ToArray().AsSpan(), ref entityGlobals, midInstantiationAction);

		public static void InstantiateGlobals<TGlobal, TEntity>(TEntity entity, List<TGlobal> globals, ref Instanced<TGlobal>[] entityGlobals, Action midInstantiationAction) where TGlobal : GlobalType<TEntity, TGlobal>
			=> InstantiateGlobals(entity, CollectionsMarshal.AsSpan(globals), ref entityGlobals, midInstantiationAction);

		public static void InstantiateGlobals<TGlobal, TEntity>(TEntity entity, ReadOnlySpan<TGlobal> globals, ref Instanced<TGlobal>[] entityGlobals, Action midInstantiationAction) where TGlobal : GlobalType<TEntity, TGlobal> {
			var mem = GlobalInstantiationArrayPool<TGlobal>.Pool.Rent(globals.Length);
			try {
				var set = mem.AsSpan(0, globals.Length);
				entityGlobals = null;
				InstantiateGlobals(entity, globals, ref entityGlobals, set, late: false);
				midInstantiationAction();
				InstantiateGlobals(entity, globals, ref entityGlobals, set, late: true);
			}
			finally {
				GlobalInstantiationArrayPool<TGlobal>.Pool.Return(mem, clearArray: true);
			}
		}

		private static void InstantiateGlobals<TGlobal, TEntity>(TEntity entity, ReadOnlySpan<TGlobal> globals, ref Instanced<TGlobal>[] entityGlobals, Span<TGlobal> set, bool late) where TGlobal : GlobalType<TEntity, TGlobal> {
			int n = 0;
			for (int i = 0; i < globals.Length; i++) {
				var g = globals[i];
				if (set[i] == null && g.AppliesToEntity(entity, late)) {
					set[i] = g.InstancePerEntity ? g.NewInstance(entity) : g;
					n++;
				}
			}

			if (n > 0) {
				entityGlobals = new Instanced<TGlobal>[(entityGlobals?.Length ?? 0) + n];
				int j = 0;
				foreach (var g in set) {
					if (g != null)
						entityGlobals[j++] = new(g.Index, g);
				}
			}
			else {
				entityGlobals ??= Array.Empty<Instanced<TGlobal>>();
			}
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

		public static MethodInfo GetDerivedDefinition(Type t, MethodInfo baseMethod) =>
			t.GetMethods().Single(m => m.GetBaseDefinition() == baseMethod);

		public static bool HasOverride(Type t, MethodInfo baseMethod) =>
			baseMethod.DeclaringType.IsInterface ? t.IsAssignableTo(baseMethod.DeclaringType) :
			GetDerivedDefinition(t, baseMethod).DeclaringType != baseMethod.DeclaringType;

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

		static LoaderUtils() {
			TypeCaching.OnClear += validatedTypes.Clear;
		}
	}

	internal class GlobalInstantiationArrayPool<T> {
		public static ArrayPool<T> Pool = ArrayPool<T>.Create();

		static GlobalInstantiationArrayPool() {
			// honestly, this should go in 'OnResizeArrays'
			TypeCaching.OnClear += () => LoaderUtils.ResetStaticMembers(typeof(GlobalInstantiationArrayPool<T>), false);
		}
	}
}
