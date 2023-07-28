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
	private bool UpdateRequested = false;
	private AsyncProvider<TResource> Provider = null;

	protected abstract TUIElement GenElement(TResource resource);
	protected virtual void UpdateElement(TUIElement element) { }

	// Graphical elements set on OnInitialize
	private UIText EndItem = null;
	// null Provider is empty so completed
	private AsyncProviderState LastProviderState = AsyncProviderState.Completed;

	// null Provider is empty so completed
	public AsyncProviderState State => Provider?.State ?? AsyncProviderState.Completed;

	public delegate void StateDelegate(AsyncProviderState state);
	public event StateDelegate OnStartLoading;
	public event StateDelegate OnFinished;

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

	public void SetEnumerable(IAsyncEnumerable<TResource> enumerable = null, bool forceSeparateThread = false)
	{
		if (enumerable is not null) {
			SetProvider(new(enumerable, forceSeparateThread));
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

			// Force a state change in case of changed provider so it's clear a change happened
			// In general you'd have a cancelled if was not finished, then a loading state (GUARANTEED)
			// And in case the completion if already finished (handled automatically later in Update
			// given that LastProviderState is changed)
			/*
			if (!LastProviderState.IsFinished())
				InternalOnUpdateState(AsyncProviderState.Canceled);
			*/
			InternalOnUpdateState(AsyncProviderState.Loading);
			endItemTextNeedUpdate = true;
		}

		if (Provider is not null) {
			var uiels = Provider.GetData().Select(GenElement).ToArray();
			if (uiels.Length > 0) {
				Remove(EndItem);
				AddRange(uiels);
				Add(EndItem);
			}
		}

		if (UpdateRequested) {
			foreach (var item in ReceivedItems) {
				UpdateElement(item);
			}
			UpdateRequested = false;
		}

		providerState = State;
		if (providerState != LastProviderState) {
			InternalOnUpdateState(providerState);
			endItemTextNeedUpdate = true;
		}
		if (endItemTextNeedUpdate)
			EndItem.SetText(GetEndItemText());

		base.Update(gameTime);
	}

	private void InternalOnUpdateState(AsyncProviderState state)
	{
		LastProviderState = state;
		if (LastProviderState.IsFinished()) {
			OnFinished(LastProviderState);
		} else {
			OnStartLoading(LastProviderState);
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

	protected virtual string GetEndItemText()
	{
		switch (LastProviderState) {
			case AsyncProviderState.Loading:
				return Language.GetTextValue("tModLoader.ALLoading");
			case AsyncProviderState.Completed:
				return ReceivedItems.Count() > 0 ? "" : Language.GetTextValue("tModLoader.ALNoEntries");
			case AsyncProviderState.Canceled:
				// @TODO: Maybe distinguish aborted for cancel and aborted for error
				return Language.GetTextValue("tModLoader.ALAborted");
			case AsyncProviderState.Aborted:
				return Language.GetTextValue("tModLoader.ALAborted");
		}
		return "Invalid state";
	}

	public void Cancel()
	{
		Provider?.Cancel();
	}

	public void UpdateData()
	{
		UpdateRequested = true;
	}
}
