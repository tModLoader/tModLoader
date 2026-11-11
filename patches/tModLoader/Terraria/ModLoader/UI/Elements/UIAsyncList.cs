using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.Localization;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Terraria.ModLoader.UI.Elements;

/**
 * <remarks>
 *   Remember to set GenElement is not provided in the constructor and TResource is not a TUIElement.
 *   DO NOT USE Add/AddRange directly, always use the provider methods.
 * </remarks>
 */
public abstract class UIAsyncList<TResource, TUIElement> : UIList where TUIElement : UIElement
{
	private bool ProviderChanged = false;
	private AsyncProvider<TResource> Provider = null;

	protected abstract TUIElement GenElement(TResource resource);

	// Graphical elements set on OnInitialize
	private UIText EndItem = null;

	// null Provider is empty so completed
	public AsyncProviderState State { get; private set; } = AsyncProviderState.Completed; // Expose only syncronous state
	private AsyncProviderState RealtimeState => Provider?.State ?? AsyncProviderState.Completed;

	public delegate void StateDelegate(AsyncProviderState state);
	public delegate void StateDelegateWithException(AsyncProviderState state, Exception e);
	public event StateDelegate OnStartLoading;
	public event StateDelegateWithException OnFinished;

	public IEnumerable<TUIElement> ReceivedItems {
		get {
			foreach (var el in this) {
				if (el != EndItem)
					yield return el as TUIElement;
			}
		}
	}

	public UIAsyncList() : base()
	{
		// Make sure not to sort
		ManualSortMethod = (l) => { };
	}

	/**
	 * <remarks>
	 *   SetProvider will delegate all UI actions to next Update,
	 *   so it NOT SAFE to be called out of the main thread,
	 *   because having an assignment to ProviderChanged it CAN
	 *   cause problems in case the list is cleared before the provider
	 *   is swapped and the old provider is partially read giving unwanted
	 *   elements, same if you do the other way around (the provider can be
	 *   partially consumed before the clear)
	 * </remarks>
	 */
	public void SetProvider(AsyncProvider<TResource> provider = null)
	{
		Provider?.Cancel();
		ProviderChanged = true;
		Provider = provider;
	}

	public void SetEnumerable(IAsyncEnumerable<TResource> enumerable = null)
	{
		if (enumerable is not null) {
			SetProvider(new(enumerable));
		} else {
			SetProvider(null);
		}
	}

	public override void Update(GameTime gameTime)
	{
		bool endItemTextNeedUpdate = false;
		AsyncProviderState providerState;

		// Before normal update add extra elements
		if (ProviderChanged) {
			Clear();
			Add(EndItem);
			ProviderChanged = false;

			InternalOnUpdateState(AsyncProviderState.Loading);
			endItemTextNeedUpdate = true;
		}

		if (Provider is not null) {
			var uiels = Provider.GetData().Select(GenElement).ToArray();
			if (uiels.Length > 0) {
				// Use AddRange because it's the only *Range that works correctly on UIList
				Remove(EndItem);
				AddRange(uiels);
				Add(EndItem);
			}
		}

		providerState = RealtimeState;
		if (providerState != State) {
			InternalOnUpdateState(providerState);
			endItemTextNeedUpdate = true;
		}
		if (endItemTextNeedUpdate)
			EndItem.SetText(GetEndItemText());

		base.Update(gameTime);
	}

	private void InternalOnUpdateState(AsyncProviderState state)
	{
		State = state;
		if (State.IsFinished()) {
			OnFinished(State, Provider?.Exception);
		} else {
			OnStartLoading(State);
		}
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		EndItem = new UIText(GetEndItemText()) {
			HAlign = 0.5f
		}.WithPadding(15f);
		Add(EndItem);
	}

	public virtual string GetEndItemText()
	{
		switch (State) {
			case AsyncProviderState.Loading:
				return Language.GetTextValue("tModLoader.ALLoading");
			case AsyncProviderState.Completed:
				return ReceivedItems.Any() ? "" : Language.GetTextValue("tModLoader.ALNoEntries");
			case AsyncProviderState.Canceled:
				return Language.GetTextValue("tModLoader.ALCancel");
			case AsyncProviderState.Aborted:
				return Language.GetTextValue("tModLoader.ALError");
		}
		return "Invalid state";
	}

	public void Cancel()
	{
		Provider?.Cancel();
	}
}
