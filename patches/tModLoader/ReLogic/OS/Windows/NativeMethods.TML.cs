using System;
using System.Runtime.InteropServices;

namespace ReLogic.OS.Windows;

static partial class NativeMethods
{
	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	public static void HideConsole()
	{
		IntPtr hWnd = GetConsoleWindow();

		ShowWindow(hWnd, 0);
	}
}
