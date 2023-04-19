using System;
using System.Runtime.InteropServices;

namespace ReLogic.OS.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct Message
{
	public IntPtr hWnd;
	public int msg;
	public IntPtr wparam;
	public IntPtr lparam;
	public IntPtr result;

	public static Message Create(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) => new Message()
	{
		hWnd = hWnd,
		msg = msg,
		wparam = wparam,
		lparam = lparam,
		result = IntPtr.Zero
	};
}
