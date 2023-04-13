using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME.WinImm32;
using ReLogic.OS.Windows;
using NativeMethods = ReLogic.Localization.IME.WinImm32.NativeMethods;

namespace ReLogic.Localization.IME;

internal class WinImm32Ime : PlatformIme, IMessageFilter
{
	private IntPtr _hWnd;
	private bool _isFocused;
	private WindowsMessageHook _wndProcHook;
	private bool _disposedValue;

	public override string CompositionString => GetCompositionString();

	public override bool IsCandidateListVisible => GetCandidate(0) != "";

	public override uint SelectedCandidate => GetCandidateSelection();

	public override uint CandidateCount => GetCandidatePageSize();

	private void SetEnabled(bool bEnable)
	{
		if (bEnable) {
			IntPtr hImc = NativeMethods.ImmCreateContext();
			NativeMethods.ImmAssociateContext(_hWnd, hImc);
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
		else {
			IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
			NativeMethods.ImmAssociateContext(_hWnd, IntPtr.Zero);
			NativeMethods.ImmDestroyContext(hImc);
		}
	}
	
	private void FinalizeString(bool bSend = false)
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		NativeMethods.ImmSetCompositionString(hImc, Imm.SCS_SETSTR, "", 0, null, 0);
		NativeMethods.ImmNotifyIME(hImc, Imm.NI_COMPOSITIONSTR, 0, 0);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);
	}

	private string GetCompositionString()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return "";
		}

		IntPtr buffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, buffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		byte[] buf = new byte[size];
		Marshal.Copy(buffer, buf, 0, size);
		Marshal.FreeHGlobal(buffer);
		return Encoding.Unicode.GetString(buf, 0, size);
	}
	
	public override string GetCandidate(uint index)
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return "";
		}

		IntPtr candListBuffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
		byte[] buf = new byte[size];
		Marshal.Copy(candListBuffer, buf, 0, size);
		Marshal.FreeHGlobal(candListBuffer);

		if (index >= candList.dwCount) {
			return "";
		}

		uint offsetI = BitConverter.ToUInt32(buf, ((int)index + 6) * sizeof(uint));
		uint offsetJ = 0;
		if (index == candList.dwCount - 1) {
			offsetJ = candList.dwSize;
		}
		else {
			offsetJ = BitConverter.ToUInt32(buf, ((int)index + 7) * sizeof(uint));
		}

		int strLen = (int)(offsetJ - offsetI - 2);
		string candidate = Encoding.Unicode.GetString(buf, (int)offsetI, strLen);
		return candidate;
	}
	
	private uint GetCandidateSelection()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return 0;
		}

		IntPtr candListBuffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
		return candList.dwSelection;
	}
	
	private uint GetCandidatePageSize()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return 0;
		}

		IntPtr candListBuffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
		return candList.dwPageSize;
	}
	
	public WinImm32Ime(WindowsMessageHook wndProcHook, IntPtr hWnd)
	{
		_wndProcHook = wndProcHook;
		_hWnd = hWnd;
		_isFocused = ReLogic.OS.Windows.NativeMethods.GetForegroundWindow() == _hWnd;
		_wndProcHook.AddMessageFilter(this);
		SetEnabled(false);
	}

	protected override void OnEnable()
	{
		if (_isFocused)
			// ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_Enable(bEnable: true);
			SetEnabled(bEnable: true);
	}

	protected override void OnDisable()
	{
		/*
		ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_FinalizeString();
		ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_Enable(bEnable: false);
		*/
		FinalizeString();
		SetEnabled(bEnable: false);
	}

	public bool PreFilterMessage(ref Message message)
	{
		if (message.msg == 8) {
			// ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_Enable(bEnable: false);
			SetEnabled(bEnable: false);
			_isFocused = false;
			return true;
		}

		if (message.msg == 7) {
			if (base.IsEnabled)
				// ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_Enable(bEnable: true);
				SetEnabled(bEnable: true);

			_isFocused = true;
			return true;
		}

		// Hides the system IME. Should always be called on application startup.
		if (message.msg == 641) {
			message.lParam = IntPtr.Zero;
			return false;
		}

		if (!base.IsEnabled)
			return false;

		// There is actually no ProcessMessage method in Relogic.Native.dll so this is not needed
		/*
		bool trapped = false;
		IntPtr lParam = message.LParam;
		ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_ProcessMessage(message.HWnd, message.Msg, message.WParam, ref lParam, ref trapped);
		message.LParam = lParam;
		if (trapped)
			return true;
		*/

		switch (message.msg) {
			case 81:
				return true;
			case 641:
				message.lParam = IntPtr.Zero;
				break;
			case 269:
				return true;
			case 642:
				return true;
			case 258:
				OnKeyPress((char)message.wParam.ToInt32());
				break;
			case 256:
				// System key events should always be ignored whenever the IME is active
				/*
				if (!ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_ShouldIgnoreHotKey(ref message))
					ReLogic.OS.Windows.NativeMethods.TranslateMessage(ref message);
				*/
				break;
		}

		return false;
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposedValue) {
			if (base.IsEnabled)
				Disable();

			_wndProcHook.RemoveMessageFilter(this);
			// ReLogic.Localization.IME.Windows.NativeMethods.ImeUi_Uninitialize();
			IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
			NativeMethods.ImmDestroyContext(hImc);
			_disposedValue = true;
		}
	}

	~WinImm32Ime()
	{
		Dispose(disposing: false);
	}
}