using System;
using System.Runtime.InteropServices;

namespace ReLogic.OS.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct Message
{
	public IntPtr HWnd;
	public int Msg;
	public IntPtr WParam;
	public IntPtr LParam;
	public IntPtr Result;

	public static Message Create(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) => new Message()
	{
		HWnd = hWnd,
		Msg = msg,
		WParam = wparam,
		LParam = lparam,
		Result = IntPtr.Zero
	};
}
