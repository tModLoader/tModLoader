using System;
using System.Runtime.InteropServices;

namespace ReLogic.Localization.IME.WinImm32;

internal static class NativeMethods
{
#if NETCORE
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
	public static extern int ImmGetCompositionString(IntPtr hImc, uint dwIndex, IntPtr lpBuf, int dwBufLen);

	[DllImport("imm32.dll", CharSet = CharSet.Unicode)]
	public static extern bool ImmSetCompositionString(IntPtr hImc, uint dwIndex, string lpComp, int dwCompLen,
		string lpRead, int dwReadLen);

	[DllImport("imm32.dll", CharSet = CharSet.Unicode)]
	public static extern int ImmGetCandidateList(IntPtr hImc, uint dwIndex, IntPtr lpCandList, int dwBufLen);

	[DllImport("imm32.dll")]
	public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

	[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
	public static extern bool ImmNotifyIME(IntPtr hImc, uint dwAction, uint dwIndex, uint dwValue);
#else
	private const string DLL_NAME = "ReLogic.Native.dll";

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ImeUi_Initialize(IntPtr hWnd, [MarshalAs(UnmanagedType.I1)] bool bDisabled = false);

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ImeUi_Uninitialize();

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode, EntryPoint = "ImeUi_EnableIme")]
	public static extern void ImeUi_Enable([MarshalAs(UnmanagedType.I1)] bool bEnable);

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ImeUi_IsEnabled();

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr ImeUi_ProcessMessage(IntPtr hWnd, int msg, IntPtr wParam, ref IntPtr lParam, [MarshalAs(UnmanagedType.I1)] ref bool trapped);

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr ImeUi_GetCompositionString();

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr ImeUi_GetCandidate(uint index);

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	public static extern uint ImeUi_GetCandidateSelection();

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	public static extern uint ImeUi_GetCandidateCount();

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	public static extern void ImeUi_FinalizeString([MarshalAs(UnmanagedType.I1)] bool bSend = false);

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode)]
	public static extern uint ImeUi_GetCandidatePageSize();

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode, EntryPoint = "ImeUi_IsShowCandListWindow")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ImeUi_IsCandidateListVisible();

	[DllImport("ReLogic.Native.dll", CharSet = CharSet.Unicode, EntryPoint = "ImeUi_IgnoreHotKey")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ImeUi_ShouldIgnoreHotKey(ref Message message);
#endif
}