using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	abstract class DefinitionElement<T> : ConfigElement<T> where T : EntityDefinition
	{
		protected bool updateNeeded;
		protected bool selectionExpanded;
		protected UIPanel chooserPanel;
		protected NestedUIGrid chooserGrid;
		protected UIFocusInputTextField chooserFilter;
		protected UIFocusInputTextField chooserFilterMod;
		protected float optionScale = 0.5f;
		protected List<DefinitionOptionElement<T>> options;
		protected DefinitionOptionElement<T> optionChoice;
		public override void OnBind() {
			base.OnBind();
			Height.Set(30f, 0f);

			optionChoice = CreateDefinitionOptionElement();
			optionChoice.Top.Set(2f, 0f);
			optionChoice.Left.Set(-30, 1f);
			optionChoice.OnClick += (a, b) => {
				selectionExpanded = !selectionExpanded;
				updateNeeded = true;
			};
			TweakDefinitionOptionElement(optionChoice);
			Append(optionChoice);

			chooserPanel = new UIPanel();
			chooserPanel.Top.Set(30, 0);
			chooserPanel.Height.Set(200, 0);
			chooserPanel.Width.Set(0, 1);
			chooserPanel.BackgroundColor = Color.CornflowerBlue;

			UIPanel textBoxBackgroundA = new UIPanel();
			textBoxBackgroundA.Width.Set(160, 0f);
			textBoxBackgroundA.Height.Set(30, 0f);
			textBoxBackgroundA.Top.Set(-6, 0);
			textBoxBackgroundA.PaddingTop = 0;
			textBoxBackgroundA.PaddingBottom = 0;
			chooserFilter = new UIFocusInputTextField("Filter by Name");
			chooserFilter.OnTextChange += (a, b) => {
				updateNeeded = true;
			};
			chooserFilter.OnRightClick += (a, b) => chooserFilter.SetText("");
			chooserFilter.Width = StyleDimension.Fill;
			chooserFilter.Height.Set(-6, 1f);
			chooserFilter.Top.Set(6, 0f);
			textBoxBackgroundA.Append(chooserFilter);
			chooserPanel.Append(textBoxBackgroundA);

			UIPanel textBoxBackgroundB = new UIPanel();
			textBoxBackgroundB.CopyStyle(textBoxBackgroundA);
			textBoxBackgroundB.Left.Set(180, 0);
			chooserFilterMod = new UIFocusInputTextField("Filter by Mod");
			chooserFilterMod.OnTextChange += (a, b) => {
				updateNeeded = true;
			};
			chooserFilterMod.OnRightClick += (a, b) => chooserFilterMod.SetText("");
			chooserFilterMod.Width = StyleDimension.Fill;
			chooserFilterMod.Height.Set(-6, 1f);
			chooserFilterMod.Top.Set(6, 0f);
			textBoxBackgroundB.Append(chooserFilterMod);
			chooserPanel.Append(textBoxBackgroundB);

			chooserGrid = new NestedUIGrid();
			chooserGrid.Top.Set(30, 0);
			chooserGrid.Height.Set(-30, 1);
			chooserGrid.Width.Set(-12, 1);
			chooserPanel.Append(chooserGrid);

			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(-30f, 1f);
			scrollbar.Top.Set(30f, 0f);
			scrollbar.Left.Pixels += 8;
			scrollbar.HAlign = 1f;
			chooserGrid.SetScrollbar(scrollbar);
			chooserPanel.Append(scrollbar);
			//Append(chooserPanel);

			UIModConfigHoverImageSplit upDownButton = new UIModConfigHoverImageSplit(upDownTexture, "Zoom in", "Zoom out");
			upDownButton.Recalculate();
			upDownButton.Top.Set(-4f, 0f);
			upDownButton.Left.Set(-18, 1f);
			upDownButton.OnClick += (a, b) => {
				Rectangle r = b.GetDimensions().ToRectangle();
				if (a.MousePosition.Y < r.Y + r.Height / 2) {
					optionScale = Math.Min(1f, optionScale + 0.1f);
				}
				else {
					optionScale = Math.Max(0.5f, optionScale - 0.1f);
				}
				foreach (var choice in options) {
					choice.SetScale(optionScale);
				}
			};
			chooserPanel.Append(upDownButton);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!updateNeeded) return;
			updateNeeded = false;
			if (selectionExpanded && options == null) {
				options = CreateDefinitionOptionElementList();
			}
			if (!selectionExpanded)
				chooserPanel.Remove();
			else
				Append(chooserPanel);
			float newHeight = selectionExpanded ? 240 : 30;
			Height.Set(newHeight, 0f);
			if (Parent != null && Parent is UISortableElement) {
				Parent.Height.Pixels = newHeight;
			}
			if (selectionExpanded) {
				var passed = GetPassedOptionElements();
				chooserGrid.Clear();
				chooserGrid.AddRange(passed);
			}
			//itemChoice.SetItem(_GetValue()?.GetID() ?? 0);
			optionChoice.SetItem(Value);
		}

		protected abstract List<DefinitionOptionElement<T>> GetPassedOptionElements();
		protected abstract List<DefinitionOptionElement<T>> CreateDefinitionOptionElementList();
		protected abstract DefinitionOptionElement<T> CreateDefinitionOptionElement();
		protected virtual void TweakDefinitionOptionElement(DefinitionOptionElement<T> optionElement) { }
	}

	class DefinitionOptionElement<T> : UIElement where T : EntityDefinition
	{
		public static Texture2D defaultBackgroundTexture = Main.inventoryBack9Texture;
		public Texture2D backgroundTexture = defaultBackgroundTexture;
		public string tooltip;
		internal float scale = .75f;
		protected bool unloaded;
		public int type;
		public T definition;

		public DefinitionOptionElement(T definition, float scale = .75f) {
			SetItem(definition);

			this.scale = scale;
			this.Width.Set(defaultBackgroundTexture.Width * scale, 0f);
			this.Height.Set(defaultBackgroundTexture.Height * scale, 0f);
		}

		public virtual void SetItem(T item) {
			definition = item;
			type = definition?.Type ?? 0;
			unloaded = definition?.IsUnloaded ?? false;
			if (definition == null || (type == 0 && !unloaded))
				tooltip = "Nothing";
			else {
				tooltip = $"{definition.name} [{definition.mod}]{(unloaded ? $" ({Language.GetTextValue("tModLoader.UnloadedItemItemName")})" : "")}";
			}
		}

		public virtual void SetScale(float scale) {
			this.scale = scale;
			this.Width.Set(defaultBackgroundTexture.Width * scale, 0f);
			this.Height.Set(defaultBackgroundTexture.Height * scale, 0f);
		}

		public override int CompareTo(object obj) {
			var other = obj as DefinitionOptionElement<T>;
			return type.CompareTo(other.type);
		}
	}
}
