using System;
using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME.Windows;
using ReLogic.OS.Windows;

namespace ReLogic.Localization.IME;

internal class WindowsIme : PlatformIme, IMessageFilter
{
	private IntPtr _windowHandle;
	private bool _isFocused;
	private WindowsMessageHook _wndProcHook;
	private readonly ReLogic.Localization.IME.Windows.ImeWrapper _imeWrapper;
	private bool _disposedValue;

	public override uint CandidateCount => _imeWrapper.ImeUi_GetCandidatePageSize();

	public override string CompositionString => _imeWrapper.ImeUi_GetCompositionString();

	public override bool IsCandidateListVisible => _imeWrapper.ImeUi_IsCandidateListVisible();

	public override uint SelectedCandidate => _imeWrapper.ImeUi_GetCandidateSelection();

	public WindowsIme(WindowsMessageHook wndProcHook, IntPtr windowHandle)
	{
		_wndProcHook = wndProcHook;
		_windowHandle = windowHandle;
		_isFocused = ReLogic.OS.Windows.NativeMethods.GetForegroundWindow() == _windowHandle;
		_imeWrapper = new ImeWrapper();
		_wndProcHook.AddMessageFilter(this);
		_imeWrapper.ImeUi_Initialize(windowHandle, true);
	}

	protected override void OnEnable()
	{
		_imeWrapper.ImeUi_Enable(bEnable: true);
	}

	protected override void OnDisable()
	{
		_imeWrapper.ImeUi_FinalizeString();
		_imeWrapper.ImeUi_Enable(bEnable: false);
	}

	public override string GetCandidate(uint index) => _imeWrapper.ImeUi_GetCandidate(index);

	public bool PreFilterMessage(ref Message message)
	{
		if (message.msg == 8) {
			_imeWrapper.ImeUi_Enable(bEnable: false);
			_isFocused = false;
			return true;
		}

		if (message.msg == 7) {
			if (base.IsEnabled)
				_imeWrapper.ImeUi_Enable(bEnable: true);

			_isFocused = true;
			return true;
		}

		if (message.msg == 641) {
			message.lParam = IntPtr.Zero;
			return false;
		}

		if (!base.IsEnabled)
			return false;

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
				ReLogic.OS.Windows.NativeMethods.TranslateMessage(ref message);
				break;
		}

		return false;
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposedValue) {
			if (base.IsEnabled)
				Disable();

			_imeWrapper.ImeUi_Uninitialize();
			_disposedValue = true;
		}
	}

	~WindowsIme()
	{
		Dispose(disposing: false);
	}
}