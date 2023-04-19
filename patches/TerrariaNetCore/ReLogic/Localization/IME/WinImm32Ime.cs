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
	private string _compString;
	private string[] _candList;
	private uint _candSelection;
	private uint _candCount;

	public override string CompositionString => _compString;

	public override bool IsCandidateListVisible => CandidateCount > 0;

	public override uint SelectedCandidate => _candSelection;

	public override uint CandidateCount => _candCount;

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
			NativeMethods.ImmNotifyIME(hImc, Imm.NI_COMPOSITIONSTR, Imm.CPS_CANCEL, 0);
			NativeMethods.ImmSetCompositionString(hImc, Imm.SCS_SETSTR, "", 0, null, 0);
			NativeMethods.ImmNotifyIME(hImc, Imm.NI_CLOSECANDIDATE, 0, 0);
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

	private void UpdateCandidateList()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
			if (size == 0) {
				_candList = Array.Empty<string>();
				_candCount = 0;
				_candSelection = 0;
				return;
			}

			IntPtr candListBuffer = Marshal.AllocHGlobal(size);
			NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);

			CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
			byte[] buf = new byte[size];
			Marshal.Copy(candListBuffer, buf, 0, size);
			Marshal.FreeHGlobal(candListBuffer);

			string[] candStrList = new string[candList.dwCount];

			for (int i = 0; i < candList.dwCount; i++) {
				uint offsetI = BitConverter.ToUInt32(buf, (i + 6) * sizeof(uint));
				uint offsetJ = 0;
				if (i == candList.dwCount - 1) {
					offsetJ = candList.dwSize;
				}
				else {
					offsetJ = BitConverter.ToUInt32(buf, (i + 7) * sizeof(uint));
				}

				int strLen = (int)(offsetJ - offsetI - 2);
				candStrList[i] = Encoding.Unicode.GetString(buf, (int)offsetI, strLen);
			}

			_candList = candStrList;
			_candCount = candList.dwPageSize;
			_candSelection = candList.dwSelection;
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}

	public override string GetCandidate(uint index)
	{
		if (index < CandidateCount) {
			return _candList[index];
		}

		return "";
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
			message.lparam = IntPtr.Zero;
			return false;
		}

		if (!base.IsEnabled)
			return false;

		switch (message.msg) {
			case Msg.WM_INPUTLANGCHANGE:
				return true;

			case Msg.WM_IME_STARTCOMPOSITION:
				_compString = "";
				return true;

			case Msg.WM_IME_COMPOSITION:
				_compString = GetCompositionString();
				break;

			case Msg.WM_IME_ENDCOMPOSITION:
				_compString = "";
				UpdateCandidateList();
				break;

			case Msg.WM_IME_NOTIFY:
				switch (message.wparam.ToInt32()) {
					case Imm.IMN_OPENCANDIDATE:
					case Imm.IMN_CHANGECANDIDATE:
					case Imm.IMN_CLOSECANDIDATE:
						UpdateCandidateList();
						break;
				}

				return true;

			case Msg.WM_CHAR:
				OnKeyPress((char)message.wparam.ToInt32());
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