using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ReLogic.OS.Windows;

internal class WindowsMessageHook : IDisposable
{
	private delegate IntPtr WndProcCallback(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	private const int GWL_WNDPROC = -4;

	private IntPtr _windowHandle = IntPtr.Zero;
	private IntPtr _previousWndProc = IntPtr.Zero;
	private WndProcCallback _wndProc;
	private List<IMessageFilter> _filters = new List<IMessageFilter>();
	private bool disposedValue;

	public WindowsMessageHook(IntPtr windowHandle)
	{
		_windowHandle = windowHandle;
		_wndProc = WndProc;
		_previousWndProc = NativeMethods.SetWindowLongPtr(_windowHandle, GWL_WNDPROC,
			Marshal.GetFunctionPointerForDelegate((Delegate)_wndProc));
	}

	private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
	{
		Message message = new Message {
			hWnd = hWnd,
			msg = msg,
			wParam = wParam,
			lParam = lParam,
			result = IntPtr.Zero
		};
		if (InternalWndProc(ref message))
			return message.result;

		IntPtr result = NativeMethods.CallWindowProc(_previousWndProc, message.hWnd, message.msg, message.wParam,
			message.lParam);
		return result;
	}

	private bool InternalWndProc(ref Message message)
	{
		foreach (IMessageFilter filter in _filters) {
			if (filter.PreFilterMessage(ref message))
				return true;
		}

		return false;
	}

	public void AddMessageFilter(IMessageFilter filter)
	{
		_filters.Add(filter);
	}

	public void RemoveMessageFilter(IMessageFilter filter)
	{
		_filters.Remove(filter);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue) {
			NativeMethods.SetWindowLongPtr(_windowHandle, GWL_WNDPROC, _previousWndProc);
			disposedValue = true;
		}
	}

	~WindowsMessageHook()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}