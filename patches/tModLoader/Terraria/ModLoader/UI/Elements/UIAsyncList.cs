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
public class UIAsyncList<TResource, TUIElement> : UIList where TUIElement : UIElement
{
	private bool ProviderChanged = false;
	private bool UpdateRequested = false;
	private AsyncProvider<TResource> Provider = null;

	public Func<TResource, TUIElement> GenElement;
	public Action<TUIElement> UpdateElement;

	// Graphical elements set on OnInitialize
	private UIText EndItem = null;
	// null Provider is empty so completed
	private AsyncProviderState LastProviderState = AsyncProviderState.Completed;

	// null Provider is empty so completed
	public AsyncProviderState State => Provider?.State ?? AsyncProviderState.Completed;

	public delegate void StateChangedDelegate(AsyncProviderState newState, AsyncProviderState oldState);
	public event StateChangedDelegate OnStateChanged;

	public IEnumerable<TUIElement> ReceivedItems {
		get {
			foreach (var el in this) {
				if (el != EndItem)
					yield return el as TUIElement;
			}
		}
	}

	public UIAsyncList(Func<TResource, TUIElement> genElement, Action<TUIElement> updateElement) : base()
	{
		// Make sure not to sort
		this.ManualSortMethod = (l) => { };

		GenElement = genElement;
		UpdateElement = updateElement;
	}

	public UIAsyncList() : this(res => res as TUIElement, el => { })
	{
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
	public void SetProvider(AsyncProvider<TResource> provider, bool cancelPrevious = true)
	{
		if (cancelPrevious && Provider is not null) {
			Provider.Cancel();
		}

		ProviderChanged = true;
		Provider = provider;
	}

	public override void Update(GameTime gameTime)
	{
		bool endItemTextNeedUpdate = false;
		AsyncProviderState providerState;

		// Before normal update add extra elements
		if (ProviderChanged) {
			this.Clear();
			ProviderChanged = false;

			// Force a state change in case of changed provider so it's clear a change happened
			// In general you'd have a cancelled if was not finished, then a loading state (GUARANTEED)
			// And in case the completion if already finished (handled automatically later in Update
			// given that LastProviderState is changed)
			if (!LastProviderState.IsFinished()) {
				providerState = AsyncProviderState.Canceled;
				OnStateChanged(providerState, LastProviderState);
				LastProviderState = providerState;
			}
			providerState = AsyncProviderState.Loading;
			OnStateChanged(providerState, LastProviderState);
			LastProviderState = providerState;
			endItemTextNeedUpdate = true;
		}

		if (Provider is not null) {
			var uiels = Provider.GetData().Select(GenElement).ToArray();
			if (uiels.Length > 0) {
				this.Remove(EndItem);
				this.AddRange(uiels);
				this.Add(EndItem);
			}
		}

		if (UpdateRequested) {
			foreach (var item in ReceivedItems) {
				UpdateElement(item);
			}
			UpdateRequested = false;
		}

		// null Provider is empty so completed
		providerState = this.State;
		if (providerState != LastProviderState) {
			OnStateChanged(providerState, LastProviderState);
			LastProviderState = providerState;
			endItemTextNeedUpdate = true;
		}
		if (endItemTextNeedUpdate)
			EndItem.SetText(this.GetEndItemText());

		base.Update(gameTime);
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		EndItem = new UIText(this.GetEndItemText()) {
			HAlign = 0.5f
		}.WithPadding(15f);
		Add(EndItem);
	}

	protected virtual string GetEndItemText()
	{
		switch (this.LastProviderState) {
			case AsyncProviderState.Loading:
				return Language.GetTextValue("tModLoader.ALLoading");
			case AsyncProviderState.Completed:
				return this.ReceivedItems.Count() > 0 ? "" : Language.GetTextValue("tModLoader.ALNoEntries");
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
