using System;
using System.Runtime.InteropServices;
using System.Text;
using ReLogic.Localization.IME.WinImm32;
using ReLogic.OS.Windows;
using NativeMethods = ReLogic.Localization.IME.WinImm32.NativeMethods;

namespace ReLogic.Localization.IME;

internal class WinImm32Ime : PlatformIme, IMessageFilter
{
	private IntPtr _hWnd;
	private IntPtr _hImc;
	private bool _isFocused;
	private WindowsMessageHook _wndProcHook;
	private bool _disposedValue;

	public override string CompositionString => GetCompositionString();

	public override bool IsCandidateListVisible => GetCandidate(0) != "";

	public override uint SelectedCandidate => GetCandidateSelection();

	public override uint CandidateCount => GetCandidatePageSize();

	public WinImm32Ime(WindowsMessageHook wndProcHook, IntPtr hWnd)
	{
		_wndProcHook = wndProcHook;
		_hWnd = hWnd;
		_hImc = NativeMethods.ImmGetContext(_hWnd);
		NativeMethods.ImmReleaseContext(_hWnd, _hImc);
		_isFocused = ReLogic.OS.Windows.NativeMethods.GetForegroundWindow() == _hWnd;
		_wndProcHook.AddMessageFilter(this);
		SetEnabled(false);
	}
	
	private void SetEnabled(bool bEnable)
	{
		NativeMethods.ImmAssociateContext(_hWnd, bEnable ? _hImc : IntPtr.Zero);
	}
	
	private void FinalizeString(bool bSend = false)
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			NativeMethods.ImmSetCompositionString(hImc, Imm.SCS_SETSTR, "", 0, null, 0);
			NativeMethods.ImmNotifyIME(hImc, Imm.NI_COMPOSITIONSTR, 0, 0);
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}

	private string GetCompositionString()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			int size = NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, IntPtr.Zero, 0);
			if (size == 0) {
				return "";
			}

			IntPtr buffer = Marshal.AllocHGlobal(size);
			NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, buffer, size);

			byte[] buf = new byte[size];
			Marshal.Copy(buffer, buf, 0, size);
			Marshal.FreeHGlobal(buffer);
			return Encoding.Unicode.GetString(buf, 0, size);
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}
	
	public override string GetCandidate(uint index)
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
			if (size == 0) {
				return "";
			}

			IntPtr candListBuffer = Marshal.AllocHGlobal(size);
			NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);

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
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}
	
	private uint GetCandidateSelection()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
			if (size == 0) {
				return 0;
			}

			IntPtr candListBuffer = Marshal.AllocHGlobal(size);
			NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);

			CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
			return candList.dwSelection;
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}
	
	private uint GetCandidatePageSize()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
			if (size == 0) {
				return 0;
			}

			IntPtr candListBuffer = Marshal.AllocHGlobal(size);
			NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);

			CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
			return candList.dwPageSize;
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}
	
	protected override void OnEnable()
	{
		if (_isFocused)
			SetEnabled(bEnable: true);
	}

	protected override void OnDisable()
	{
		FinalizeString();
		SetEnabled(bEnable: false);
	}

	public bool PreFilterMessage(ref Message message)
	{
		if (message.msg == Msg.WM_KILLFOCUS) {
			SetEnabled(bEnable: false);
			_isFocused = false;
			return true;
		}

		if (message.msg == Msg.WM_SETFOCUS) {
			if (base.IsEnabled)
				SetEnabled(bEnable: true);

			_isFocused = true;
			return true;
		}

		// Hides the system IME. Should always be called on application startup.
		if (message.msg == Msg.WM_IME_SETCONTEXT) {
			message.lParam = IntPtr.Zero;
			return false;
		}

		if (!base.IsEnabled)
			return false;

		switch (message.msg) {
			case Msg.WM_INPUTLANGCHANGE:
				return true;
			case Msg.WM_IME_STARTCOMPOSITION:
				return true;
			case Msg.WM_IME_NOTIFY:
				return true;
			case Msg.WM_CHAR:
				OnKeyPress((char)message.wParam.ToInt32());
				break;
			case Msg.WM_KEYDOWN:
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
			NativeMethods.ImmAssociateContext(_hWnd, _hImc);
			_disposedValue = true;
		}
	}

	~WinImm32Ime()
	{
		Dispose(disposing: false);
	}
}