using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Terraria.Unified.Startup;

public interface IPreJitPolicy
{
	bool FinishedLoading { get; }

	bool InitializeStatics { get; }

	bool RunInBackground { get; }

	void InitializeAssemblies();
}

internal sealed class DefaultPreJitPolicy(ILogger<DefaultPreJitPolicy> logger) : IPreJitPolicy
{
	public bool FinishedLoading { get; private set; }

	public bool InitializeStatics => true;

	public bool RunInBackground => true;

	void IPreJitPolicy.InitializeAssemblies()
	{
		if (Main.SkipAssemblyLoad) {
			logger.LogDebug("Pre-JIT assembly load is configured to be skipped; skipping...");
			FinishedLoading = true;
			return;
		}

		logger.LogDebug("Beginning assembly pre-Load; on background thread: {0}", RunInBackground);
		if (RunInBackground) {
			var thread = new Thread(ForceLoadThread) {
				IsBackground = true,
			};
			thread.Start();
		}
		else {
			ForceLoadThread();
		}
	}

	private void ForceLoadThread()
	{
		ForceLoadAssembly(Assembly.GetExecutingAssembly());
		FinishedLoading = true;
	}

	private void ForceLoadAssembly(Assembly assembly)
	{
		logger.LogDebug("Pre-JITing assembly: {0}", assembly.FullName);
		ForceJitOnAssembly(assembly);

		if (InitializeStatics) {
			logger.LogDebug("Initializing static members for assembly: {0}", assembly.FullName);
			ForceStaticInitializers(assembly);
		}
	}

	private static void ForceJitOnAssembly(Assembly assembly)
	{
		foreach (Type type in assembly.GetTypes()) {
			var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods) {
				if (methodInfo.IsAbstract || methodInfo.ContainsGenericParameters || methodInfo.GetMethodBody() == null) {
					continue;
				}

				RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
			}
		}
	}

	private static void ForceStaticInitializers(Assembly assembly)
	{
		foreach (Type type in assembly.GetTypes()) {
			if (type.IsGenericType) {
				return;
			}

			RuntimeHelpers.RunClassConstructor(type.TypeHandle);
		}
	}
}
