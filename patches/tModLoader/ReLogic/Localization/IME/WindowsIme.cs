using System;
using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME.Windows;

namespace ReLogic.Localization.IME;

internal class WindowsIme : PlatformIme
{
	private readonly ReLogic.Localization.IME.Windows.ImeWrapper _imeWrapper;
	private bool _disposedValue;

	public override uint CandidateCount => _imeWrapper.ImeUi_GetCandidatePageSize();

	public override string CompositionString => _imeWrapper.ImeUi_GetCompositionString();

	public override bool IsCandidateListVisible => _imeWrapper.ImeUi_IsCandidateListVisible();

	public override uint SelectedCandidate => _imeWrapper.ImeUi_GetCandidateSelection();

	public WindowsIme(IntPtr windowHandle)
	{

		_imeWrapper = new ImeWrapper();
		_imeWrapper.ImeUi_Initialize(windowHandle, true);
		TextInputEXT.TextInput += OnCharCallback;
	}

	private void OnCharCallback(char key)
	{
		if (base.IsEnabled) {
			OnKeyPress(key);

		}
	}

	protected override void OnEnable()
	{
		_imeWrapper.ImeUi_EnableIme(bEnable: true);
	}

	protected override void OnDisable()
	{
		_imeWrapper.ImeUi_FinalizeString();
		_imeWrapper.ImeUi_EnableIme(bEnable: false);
	}

	public override string GetCandidate(uint index) => _imeWrapper.ImeUi_GetCandidate(index);

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