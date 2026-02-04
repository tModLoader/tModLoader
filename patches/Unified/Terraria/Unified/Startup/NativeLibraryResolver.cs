using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Terraria.Unified.Startup;

public interface INativeLibraryResolver
{
	void Initialize();
}

internal sealed partial class NativeLibraryResolver(ILogger<NativeLibraryResolver> logger) : INativeLibraryResolver
{
	private readonly Dictionary<string, nint> moduleMap = [];

	private static string NativePlatformDir =>
		OperatingSystem.IsWindows() ? "Windows" :
		OperatingSystem.IsLinux() ? (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "Linux-arm64" : "Linux") :
		OperatingSystem.IsMacOS() ? "OSX" :
		throw new PlatformNotSupportedException("Could not resolve native libraries for your operating system");

	private static string NativesDir => Path.Combine(Environment.CurrentDirectory, "Libraries", "Native", NativePlatformDir);

	void INativeLibraryResolver.Initialize()
	{
		logger.LogInformation("Initializing native library resolver...");

		try {
			var nativesDir = NativesDir;
			LogNativesDir(nativesDir);

			if (!Directory.Exists(nativesDir)) {
				throw new DirectoryNotFoundException($"The expected natives directory does not exist: {nativesDir}");
			}
		}
		catch (Exception e) {
			logger.LogError(e, "Failed to determine the native library directory to load from");
			throw;
		}

		AssemblyLoadContext.Default.ResolvingUnmanagedDll += ResolveNativeLibrary;
		logger.LogInformation("Initialized native library resolver!");
	}

	private nint ResolveNativeLibrary(Assembly assembly, string name)
	{
		lock (moduleMap) {
			if (moduleMap.TryGetValue(name, out var handle)) {
				return handle;
			}

			LogNativeResolveAttempt(assembly.FullName, name);
			if (name.StartsWith("steam_api")) {
				logger.LogDebug("    ...delegating to Steamworks.NET.AnyCPU resolver");
				return moduleMap[name] = nint.Zero;
			}

			var files = Directory.GetFiles(NativesDir, $"*{name}*", SearchOption.AllDirectories);
			if (files.FirstOrDefault() is not { } path) {
				logger.LogDebug("    ...not found");
				return moduleMap[name] = nint.Zero;
			}

			LogNativeLoadAttempt(path);
			try {
				handle = NativeLibrary.Load(path);
			}
			catch (Exception e) {
				logger.LogError(e, "Failed to load native library");
				return moduleMap[name] = nint.Zero;
			}

			logger.LogDebug("    ...success!");
			return moduleMap[name] = handle;
		}
	}

	[LoggerMessage(Level = LogLevel.Information, Message = "Native libraries will be resolved from the following directory: {nativesDir}")]
	private partial void LogNativesDir(string nativesDir);

	[LoggerMessage(Level = LogLevel.Debug, Message = "Got native resolve request: {assemblyName} -> {moduleName}")]
	private partial void LogNativeResolveAttempt(string assemblyName, string moduleName);

	[LoggerMessage(Level = LogLevel.Debug, Message = "    ...attempting load: {path}")]
	private partial void LogNativeLoadAttempt(string path);
}
