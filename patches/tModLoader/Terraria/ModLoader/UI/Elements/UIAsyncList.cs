using Microsoft.Xna.Framework;
using System.Threading;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.Localization;
using rail;

namespace Terraria.ModLoader.UI.Elements;

public class UIAsyncList : UIList
{
	// DON'T USE Add/AddRange!

	public delegate void StateChangedDelegate(AsyncProvider.State newState, AsyncProvider.State oldState);
	public event StateChangedDelegate OnStateChanged;

	CancellationTokenSource _token = new();
	IAsyncProvider<UIElement> _provider = new AsyncProvider.Empty<UIElement>();
	UIText _endItem;
	AsyncProvider.State _lastState = AsyncProvider.State.NotStarted;
	bool _forceUpdateData = false;

	public UIAsyncList() : base()
	{
		ManualSortMethod = (l) => { };
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		_endItem = new UIText(GetEndItemTextForState(_lastState, _provider.Count <= 0)) {
			HAlign = 0.5f
		}.WithPadding(15f);
	}

	public void SetProvider(IAsyncProvider<UIElement> provider)
	{
		_token.Cancel();

		// Clear the list in the Update in case SetProvider is called from a Task
		//Clear();
		ForceUpdateData();

		_token = new();
		_provider = provider;
		_provider.Start(_token.Token);
	}

	protected virtual string GetEndItemTextForState(AsyncProvider.State state, bool empty)
	{
		switch (state) {
			case AsyncProvider.State.NotStarted:
			case AsyncProvider.State.Loading:
				return Language.GetTextValue("tModLoader.ALLoading");
			case AsyncProvider.State.Completed:
				return empty ? Language.GetTextValue("tModLoader.ALNoEntries") : "";
			case AsyncProvider.State.Aborted:
				return Language.GetTextValue("tModLoader.ALAborted");
		}
		return "ERROR: Invalid State";
	}

	public void AbortLoading()
	{
		_token.Cancel();
	}

	public void ForceUpdateData()
	{
		_forceUpdateData = true;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		var _tmpState = AsyncProvider.State.Aborted;

		if (!_token.IsCancellationRequested) {
			_tmpState = _provider.State;

			if (_provider.HasNewData || _forceUpdateData) {
				_forceUpdateData = false;
				Clear();
				AddRange(_provider.GetData(true));
				Add(_endItem);
				//Recalculate(); // Not Needed, it's in UIList.DrawSelf
			}
		}

		if (_lastState != _tmpState) {
			OnStateChanged?.Invoke(_tmpState, _lastState);
			_endItem.SetText(GetEndItemTextForState(_tmpState, _provider.Count <= 0));
			_lastState = _tmpState;
			//Recalculate(); // Not Needed, it's in UIList.DrawSelf
		}
	}
}
