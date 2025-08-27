using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace ReLogic.OS.Windows;

static partial class NativeMethods
{
	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("kernel32.dll")]
	public static extern bool FreeConsole();

	[DllImport("kernel32.dll")]
	public static extern bool AllocConsole();

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetStdHandle(uint nStdHandle);

	public static void HideConsole()
	{
		IntPtr hWnd = GetConsoleWindow();

		ShowWindow(hWnd, 0);
	}

	public static void ShowConsole()
	{
		IntPtr hWnd = GetConsoleWindow();

		ShowWindow(hWnd, 1); // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
	}
}
