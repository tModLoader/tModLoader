using System;
using System.Runtime.InteropServices;

namespace ReLogic.Localization.IME.WinImm32;

internal static class NativeMethods
{
	[DllImport("Imm32.dll")]
	public static extern bool ImmSetOpenStatus(IntPtr hImc, bool bOpen);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr ImmGetContext(IntPtr hWnd);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hImc);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr ImmCreateContext();

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern bool ImmDestroyContext(IntPtr hImc);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hImc);

	[DllImport("imm32.dll", CharSet = CharSet.Unicode)]
	public static extern int ImmGetCompositionString(IntPtr hImc, uint dwIndex, ref byte lpBuf, int dwBufLen);

	[DllImport("imm32.dll", CharSet = CharSet.Unicode)]
	public static extern bool ImmSetCompositionString(IntPtr hImc, uint dwIndex, string lpComp, int dwCompLen,
		string lpRead, int dwReadLen);

	[DllImport("imm32.dll", CharSet = CharSet.Unicode)]
	public static extern int ImmGetCandidateList(IntPtr hImc, uint dwIndex, ref byte lpCandList, int dwBufLen);

	[DllImport("imm32.dll")]
	public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern bool ImmNotifyIME(IntPtr hImc, uint dwAction, uint dwIndex, uint dwValue);
}