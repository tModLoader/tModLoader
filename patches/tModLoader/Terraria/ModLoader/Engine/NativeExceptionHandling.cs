using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Terraria.Utilities;

namespace Terraria.ModLoader.Engine;

internal static class NativeExceptionHandling
{
	internal static void Init(){
		if (!OperatingSystem.IsWindows())
			return;

		// Note: Only called when not being debugged!
		SetUnhandledExceptionFilter(OurUnhandledExceptionFilter);
	}

	// Unhandled exception filter callback
	static IntPtr OurUnhandledExceptionFilter(IntPtr exceptionInfo)
	{
		Logging.tML.Fatal("Native exception has occurred, attempting to determine erroring module...");

		// System.Diagnostics.Debugger.Launch(); // To debug this code, uncomment this line and start tModLoader without debugging.

		// Cast the pointer to EXCEPTION_POINTERS
		var exPointers = Marshal.PtrToStructure<EXCEPTION_POINTERS>(exceptionInfo);

		// Get the exception address
		var ExceptionRecord = Marshal.PtrToStructure<EXCEPTION_RECORD>(exPointers.ExceptionRecord);
		IntPtr exceptionAddress = ExceptionRecord.ExceptionAddress;

		// Try to get the module handle
		IntPtr moduleHandle;
		bool success = GetModuleHandleEx(
			GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
			exceptionAddress, out moduleHandle);

		if (success && moduleHandle != IntPtr.Zero) {
			// Get the module file name
			StringBuilder moduleName = new StringBuilder(260);
			GetModuleFileName(moduleHandle, moduleName, moduleName.Capacity);

			// Log the module file path
			Logging.tML.Fatal("Exception occurred in module: " + moduleName.ToString());
			// These end up in the console or also natives.log if launched through launch script
		}
		else {
			Logging.tML.Fatal("Failed to retrieve module information.");
		}

		// Delete old .dmp.zip files
		Logging.tML.Fatal("Attempting to save minidump...");
		var dumpOptions = CrashDump.Options.Normal | CrashDump.Options.WithThreadInfo; // There might be more to add here.
		if (Main.instance?.LaunchParameters?.ContainsKey("-fulldump") == true)
			dumpOptions = CrashDump.Options.WithFullMemory;

		string minidumpPath = CrashDump.WriteExceptionAsZipAndClearOld(dumpOptions, exceptionInfo);
		if (minidumpPath == null) {
			Logging.tML.Fatal($"Minidump saving failed, either this isn't Windows or the logs folder could not be created."); // Shouldn't be possible with current code.
		}
		else {
			Logging.tML.Fatal($"Minidump saved to: \'{Path.GetFullPath(minidumpPath)}\'");
			Logging.tML.Fatal("This file can be provided to tModLoader developers to help diagnose the issue.");
		}

		// Return EXCEPTION_EXECUTE_HANDLER to let the OS handle the exception
		return (IntPtr)1;
	}

	// P/Invoke declarations
	[DllImport("kernel32.dll")]
	static extern IntPtr SetUnhandledExceptionFilter(UnhandledExceptionFilter filter);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	static extern bool GetModuleHandleEx(uint dwFlags, IntPtr lpModuleName, out IntPtr phModule);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);


	// Constants for GetModuleHandleEx
	const uint GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;
	const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002;

	// Structure definitions
	[StructLayout(LayoutKind.Sequential)]
	struct EXCEPTION_RECORD
	{
		public uint ExceptionCode;
		public uint ExceptionFlags;
		public IntPtr ExceptionRecord;
		public IntPtr ExceptionAddress;
		public uint NumberParameters;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
		public IntPtr[] ExceptionInformation;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct EXCEPTION_POINTERS
	{
		public IntPtr ExceptionRecord;
		public IntPtr ContextRecord;
	}

	// Delegate for the unhandled exception filter
	delegate IntPtr UnhandledExceptionFilter(IntPtr exceptionInfo);
}