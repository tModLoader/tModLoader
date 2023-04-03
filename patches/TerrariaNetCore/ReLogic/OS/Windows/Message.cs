using System;
using System.Runtime.InteropServices;

namespace ReLogic.OS.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct Message
{
	public IntPtr hWnd;
	public int msg;
	public IntPtr wParam;
	public IntPtr lParam;
	public IntPtr result;
}