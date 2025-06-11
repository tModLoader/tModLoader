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
	private string[] _candList = Array.Empty<string>();
	private uint _candSelection;
	private uint _candPageSize;

	public uint SelectedPage => _candPageSize == 0 ? 0 : _candSelection / _candPageSize;

	public override string CompositionString => _compString;

	public override bool IsCandidateListVisible => CandidateCount > 0;

	public override uint SelectedCandidate => _candSelection % _candPageSize;

	public override uint CandidateCount => Math.Min((uint)_candList.Length - SelectedPage * _candPageSize, _candPageSize);

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
			int size = NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, ref MemoryMarshal.GetReference(Span<byte>.Empty), 0);
			if (size == 0) {
				return "";
			}

			Span<byte> buf = stackalloc byte[size];
			NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, ref MemoryMarshal.GetReference(buf), size);

			return Encoding.Unicode.GetString(buf.ToArray());
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}

	private void UpdateCandidateList()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			int size = NativeMethods.ImmGetCandidateList(hImc, 0, ref MemoryMarshal.GetReference(Span<byte>.Empty), 0);
			if (size == 0) {
				_candList = Array.Empty<string>();
				_candPageSize = 0;
				_candSelection = 0;
				return;
			}

			Span<byte> buf = stackalloc byte[size];
			NativeMethods.ImmGetCandidateList(hImc, 0, ref MemoryMarshal.GetReference(buf), size);

			ref CandidateList candList = ref MemoryMarshal.AsRef<CandidateList>(buf);
			var offsets = MemoryMarshal.CreateReadOnlySpan(ref candList.dwOffset, (int)candList.dwCount);

			string[] candStrList = new string[candList.dwCount];
			int next = buf.Length;
			for (int i = (int)candList.dwCount-1; i >= 0; i--) {
				int start = (int)offsets[i];
				// Assume all strings are fully packed, with 2 byte null char at the end
				candStrList[i] = Encoding.Unicode.GetString(buf[start..(next-2)]);
				next = start;
			}

			_candList = candStrList;
			_candPageSize = candList.dwPageSize;
			_candSelection = candList.dwSelection;
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}

	public override string GetCandidate(uint index)
	{
		if (index < CandidateCount) {
			return _candList[index + SelectedPage * _candPageSize];
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
		if (message.Msg == Msg.WM_KILLFOCUS) {
			SetEnabled(bEnable: false);
			_isFocused = false;
			return true;
		}

		if (message.Msg == Msg.WM_SETFOCUS) {
			if (base.IsEnabled)
				SetEnabled(bEnable: true);

			_isFocused = true;
			return true;
		}

		// Hides the system IME. Should always be called on application startup.
		if (message.Msg == Msg.WM_IME_SETCONTEXT) {
			message.LParam = IntPtr.Zero;
			return false;
		}

		if (!base.IsEnabled)
			return false;

		switch (message.Msg) {
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
				switch (message.WParam.ToInt32()) {
					case Imm.IMN_OPENCANDIDATE:
					case Imm.IMN_CHANGECANDIDATE:
					case Imm.IMN_CLOSECANDIDATE:
						UpdateCandidateList();
						break;
				}

				return true;

			case Msg.WM_CHAR:
				OnKeyPress((char)message.WParam.ToInt32());
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