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

		Clear();
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

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (_provider.HasNewData) {
			Clear();
			AddRange(_provider.GetData());
			Add(_endItem);
			Recalculate(); // @TODO: Needed?
		}

		var _tmpState = _provider.State;
		if (_lastState != _tmpState) {
			OnStateChanged?.Invoke(_tmpState, _lastState);
			_endItem.SetText(GetEndItemTextForState(_tmpState, _provider.Count <= 0));
			_lastState = _tmpState;
			Recalculate(); // @TODO: Needed?
		}
	}
}
