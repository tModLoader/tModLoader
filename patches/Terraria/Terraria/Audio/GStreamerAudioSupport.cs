using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Terraria.Audio;

/// <summary>
/// We require a WMAUDIO2/wmav2 decoder to read the windows wavebank. FFMPEG has the only one available to us.
/// GStreamer replaces FFMPEG support in the latest FAudio releases.
/// Unfortunately GStreamer just ends up using FFMPEG as a backend to the gstlibav.dll plugin anyway
/// Slimming the FFMPEG for GStreamer would _probably_ require rebuilding the gstlibav plugin, or writing our own.
///
/// If we ever need GStreamer support for FAudio in the future... add the following directory structure to Libraries/Native/Windows
/// gstreamer
/// │   avcodec-58.dll
/// │   avfilter-7.dll
/// │   avformat-58.dll
/// │   avutil-56.dll
/// │   bz2.dll
/// │   ffi-7.dll
/// │   glib-2.0-0.dll
/// │   gmodule-2.0-0.dll
/// │   gobject-2.0-0.dll
/// │   gstapp-1.0-0.dll
/// │   gstaudio-1.0-0.dll
/// │   gstbase-1.0-0.dll
/// │   gstpbutils-1.0-0.dll
/// │   gstreamer-1.0-0.dll
/// │   gsttag-1.0-0.dll
/// │   gstvideo-1.0-0.dll
/// │   intl-8.dll
/// │   libwinpthread-1.dll
/// │   orc-0.4-0.dll
/// │   swresample-3.dll
/// │   z-1.dll
/// │
/// └───plugins
///         gstapp.dll
///         gstaudioconvert.dll
///         gstaudioresample.dll
///         gstcoreelements.dll
///         gstlibav.dll
///         gstplayback.dll
/// </summary>
internal static class GStreamerAudioSupport
{
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool SetDefaultDllDirectories(int directoryFlags);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	static extern void AddDllDirectory(string lpPathName);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool SetDllDirectory(string lpPathName);

	/// <summary>
	/// Call this before loading the FAudio dll
	/// </summary>
	public static void Init()
	{
		var gstreamer_dir = Path.Combine(Environment.CurrentDirectory, "Libraries", "Native", "Windows", "gstreamer");
		Environment.SetEnvironmentVariable("GST_REGISTRY", Path.Combine(gstreamer_dir, "registry"));
		Environment.SetEnvironmentVariable("GST_PLUGIN_PATH", Path.Combine(gstreamer_dir, "plugins"));

		const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

		if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
			try {
				SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
				AddDllDirectory(gstreamer_dir);
			}
			catch {
				// Pre-Windows 7, KB2533623
				SetDllDirectory(gstreamer_dir);
			}
		}
	}
}
