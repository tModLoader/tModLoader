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
	private uint _candPageStart;
	private uint _candPageSize;
	private bool _isCandDirty;

	public override string CompositionString => _compString;

	public override bool IsCandidateListVisible => CandidateCount > 0;

	public override uint SelectedCandidate => _candSelection - _candPageStart;

	public override uint CandidateCount {
		get {
			// Lazily update candidate list at most once per frame to avoid race conditions.
			// CandidateCount is always read first so we use it.
			if (_isCandDirty) {
				UpdateCandidateList();
			}

			return Math.Min((uint)_candList.Length - _candPageStart, _candPageSize);
		}
	}

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

	private string GetCompositionString(uint dwIndex)
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		try {
			int size = NativeMethods.ImmGetCompositionString(hImc, dwIndex,
				ref MemoryMarshal.GetReference(Span<byte>.Empty), 0);
			if (size == 0) {
				return "";
			}

			Span<byte> buf = stackalloc byte[size];
			NativeMethods.ImmGetCompositionString(hImc, dwIndex, ref MemoryMarshal.GetReference(buf), size);

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
				// This usually means candidate list is not ready, wait for next frame
				_candList = Array.Empty<string>();
				_candPageStart = 0;
				_candPageSize = 0;
				_candSelection = 0;
				return;
			}

			Span<byte> buf = stackalloc byte[size];
			NativeMethods.ImmGetCandidateList(hImc, 0, ref MemoryMarshal.GetReference(buf), size);

			ref CandidateList candList = ref MemoryMarshal.AsRef<CandidateList>(buf);
			var offsets = MemoryMarshal.CreateReadOnlySpan(ref candList.dwOffset, (int)candList.dwCount);

			string[] candStrList = new string[candList.dwCount];

			for (int i = 0; i < (int)candList.dwCount; i++) {
				int start = (int)offsets[i];
				int end = start;

				// Note that strings are not always fully packed, we need to search from each offset
				// for a UTF-16 null terminator (0x00 0x00). Length of UTF-16 sequences are always even.
				while (end < buf.Length - 1) {
					if (buf[end] == 0 && buf[end + 1] == 0) {
						break;
					}

					end += 2;
				}

				candStrList[i] = Encoding.Unicode.GetString(buf[start..end]);
			}

			_isCandDirty = false;
			_candList = candStrList;
			_candPageStart = candList.dwPageStart;
			_candPageSize = candList.dwPageSize;
			_candSelection = candList.dwSelection;
		}
		catch (Exception e) when (e is ArgumentOutOfRangeException or IndexOutOfRangeException) {
			// Some IME occasionally send malformed candidate buffers due to race conditions.
			// Ignore them until next correct buffer is available.
			Console.WriteLine($"Failed to parse candidate list: {e}");
		}
		finally {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
	}

	private void ClearCandidateList()
	{
		_isCandDirty = false;
		_candList = Array.Empty<string>();
		_candPageStart = 0;
		_candPageSize = 0;
		_candSelection = 0;
	}

	public override string GetCandidate(uint index)
	{
		if (index < CandidateCount) {
			return _candList[index + _candPageStart];
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
		switch (message.Msg) {
			case Msg.WM_KILLFOCUS:
				SetEnabled(bEnable: false);
				_isFocused = false;
				return true;

			case Msg.WM_SETFOCUS:
				if (IsEnabled)
					SetEnabled(bEnable: true);
				_isFocused = true;
				return true;

			case Msg.WM_IME_SETCONTEXT:
				// Hides the system IME. Should always be called on application startup.
				message.LParam = IntPtr.Zero;
				return false;
		}

		if (!IsEnabled)
			return false;

		switch (message.Msg) {
			case Msg.WM_INPUTLANGCHANGE:
				_compString = "";
				ClearCandidateList();
				return true;

			case Msg.WM_IME_STARTCOMPOSITION:
				_compString = "";
				return true;

			case Msg.WM_IME_COMPOSITION:
				if ((message.LParam.ToInt32() & Imm.GCS_RESULTSTR) != 0) {
					var resultString = GetCompositionString(Imm.GCS_RESULTSTR);
					foreach (var c in resultString) {
						OnKeyPress(c);
					}
				}

				if ((message.LParam.ToInt32() & Imm.GCS_COMPSTR) != 0) {
					_compString = GetCompositionString(Imm.GCS_COMPSTR);
					_isCandDirty = true;
				}

				return true;

			case Msg.WM_IME_ENDCOMPOSITION:
				_compString = "";
				ClearCandidateList();
				return true;

			case Msg.WM_IME_NOTIFY:
				switch (message.WParam.ToInt32()) {
					case Imm.IMN_OPENCANDIDATE:
					case Imm.IMN_CHANGECANDIDATE:
						_isCandDirty = true;
						break;

					case Imm.IMN_CLOSECANDIDATE:
						ClearCandidateList();
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
			if (IsEnabled)
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