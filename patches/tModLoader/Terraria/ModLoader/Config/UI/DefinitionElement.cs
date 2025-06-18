using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal abstract class DefinitionElement<T> : ConfigElement<T> where T : EntityDefinition
{
	protected bool UpdateNeeded { get; set; }
	protected bool SelectionExpanded { get; set; }
	protected UIPanel ChooserPanel { get; set; }
	protected NestedUIGrid ChooserGrid { get; set; }
	protected UIFocusInputTextField ChooserFilter { get; set; }
	protected UIFocusInputTextField ChooserFilterMod { get; set; }
	protected float OptionScale { get; set; } = 0.5f;
	protected List<DefinitionOptionElement<T>> Options { get; set; }
	protected DefinitionOptionElement<T> OptionChoice { get; set; }

	public override void OnBind()
	{
		base.OnBind();
		TextDisplayFunction = () => Label + ": " + OptionChoice.Tooltip;
		if (List != null) {
			TextDisplayFunction = () => Index + 1 + ": " + OptionChoice.Tooltip;
		}

		Height.Set(30f, 0f);

		OptionChoice = CreateDefinitionOptionElement();
		OptionChoice.Top.Set(2f, 0f);
		OptionChoice.Left.Set(-30, 1f);
		OptionChoice.OnLeftClick += (a, b) => {
			SelectionExpanded = !SelectionExpanded;
			UpdateNeeded = true;
		};
		TweakDefinitionOptionElement(OptionChoice);
		Append(OptionChoice);

		ChooserPanel = new UIPanel();
		ChooserPanel.Top.Set(30, 0);
		ChooserPanel.Height.Set(200, 0);
		ChooserPanel.Width.Set(0, 1);
		ChooserPanel.BackgroundColor = Color.CornflowerBlue;

		UIPanel textBoxBackgroundA = new UIPanel();
		textBoxBackgroundA.Width.Set(160, 0f);
		textBoxBackgroundA.Height.Set(30, 0f);
		textBoxBackgroundA.Top.Set(-6, 0);
		textBoxBackgroundA.PaddingTop = 0;
		textBoxBackgroundA.PaddingBottom = 0;
		ChooserFilter = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigFilterByName"));
		ChooserFilter.OnTextChange += (a, b) => {
			UpdateNeeded = true;
		};
		ChooserFilter.OnRightClick += (a, b) => ChooserFilter.SetText("");
		ChooserFilter.Width = StyleDimension.Fill;
		ChooserFilter.Height.Set(-6, 1f);
		ChooserFilter.Top.Set(6, 0f);
		textBoxBackgroundA.Append(ChooserFilter);
		ChooserPanel.Append(textBoxBackgroundA);

		UIPanel textBoxBackgroundB = new UIPanel();
		textBoxBackgroundB.CopyStyle(textBoxBackgroundA);
		textBoxBackgroundB.Left.Set(180, 0);
		ChooserFilterMod = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigFilterByMod"));
		ChooserFilterMod.OnTextChange += (a, b) => {
			UpdateNeeded = true;
		};
		ChooserFilterMod.OnRightClick += (a, b) => ChooserFilterMod.SetText("");
		ChooserFilterMod.Width = StyleDimension.Fill;
		ChooserFilterMod.Height.Set(-6, 1f);
		ChooserFilterMod.Top.Set(6, 0f);
		textBoxBackgroundB.Append(ChooserFilterMod);
		ChooserPanel.Append(textBoxBackgroundB);

		ChooserGrid = new NestedUIGrid();
		ChooserGrid.Top.Set(30, 0);
		ChooserGrid.Height.Set(-30, 1);
		ChooserGrid.Width.Set(-12, 1);
		ChooserPanel.Append(ChooserGrid);

		UIScrollbar scrollbar = new UIScrollbar();
		scrollbar.SetView(100f, 1000f);
		scrollbar.Height.Set(-30f, 1f);
		scrollbar.Top.Set(30f, 0f);
		scrollbar.Left.Pixels += 8;
		scrollbar.HAlign = 1f;
		ChooserGrid.SetScrollbar(scrollbar);
		ChooserPanel.Append(scrollbar);
		//Append(chooserPanel);

		UIModConfigHoverImageSplit upDownButton = new UIModConfigHoverImageSplit(UpDownTexture, Language.GetTextValue("LegacyMenu.168"), Language.GetTextValue("LegacyMenu.169")); // "Zoom in", "Zoom out"
		upDownButton.Recalculate();
		upDownButton.Top.Set(-4f, 0f);
		upDownButton.Left.Set(-18, 1f);
		upDownButton.OnLeftClick += (a, b) => {
			Rectangle r = b.GetDimensions().ToRectangle();
			if (a.MousePosition.Y < r.Y + r.Height / 2) {
				OptionScale = Math.Min(1f, OptionScale + 0.1f);
			}
			else {
				OptionScale = Math.Max(0.5f, OptionScale - 0.1f);
			}
			foreach (var choice in Options) {
				choice.SetScale(OptionScale);
			}
		};
		ChooserPanel.Append(upDownButton);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!UpdateNeeded)
			return;

		UpdateNeeded = false;

		if (SelectionExpanded && Options == null) {
			Options = CreateDefinitionOptionElementList();
		}

		if (!SelectionExpanded)
			ChooserPanel.Remove();
		else
			Append(ChooserPanel);

		float newHeight = SelectionExpanded ? 240 : 30;
		Height.Set(newHeight, 0f);

		if (Parent != null && Parent is UISortableElement) {
			Parent.Height.Pixels = newHeight;
		}

		if (SelectionExpanded) {
			var passed = GetPassedOptionElements();
			ChooserGrid.Clear();
			ChooserGrid.AddRange(passed);
		}

		//itemChoice.SetItem(_GetValue()?.GetID() ?? 0);
		OptionChoice.SetItem(Value);
	}

	protected abstract List<DefinitionOptionElement<T>> GetPassedOptionElements();
	protected abstract List<DefinitionOptionElement<T>> CreateDefinitionOptionElementList();
	protected abstract DefinitionOptionElement<T> CreateDefinitionOptionElement();

	protected virtual void TweakDefinitionOptionElement(DefinitionOptionElement<T> optionElement) { }
}

internal class DefinitionOptionElement<T> : UIElement where T : EntityDefinition
{
	public static Asset<Texture2D> DefaultBackgroundTexture { get; } = TextureAssets.InventoryBack9;

	public Asset<Texture2D> BackgroundTexture { get; set; } = DefaultBackgroundTexture;
	public string Tooltip { get; set; }
	public int Type { get; set; }
	public int NullID { get; set; } = 0;
	public T Definition { get; set; }

	internal float Scale { get; set; } = .75f;

	protected bool Unloaded { get; set; }

	public DefinitionOptionElement(T definition, float scale = .75f)
	{
		SetItem(definition);

		Scale = scale;
		Width.Set(DefaultBackgroundTexture.Width() * scale, 0f);
		Height.Set(DefaultBackgroundTexture.Height() * scale, 0f);
	}

	public virtual void SetItem(T item)
	{
		Definition = item;
		Type = Definition?.Type ?? NullID;
		Unloaded = Definition?.IsUnloaded ?? false;

		if (Definition == null || (Type == NullID && !Unloaded))
			Tooltip = Lang.inter[23].Value; // "None";
		else {
			if (Unloaded)
				Tooltip = $"{Definition.Name} [{Definition.Mod}] ({Language.GetTextValue("Mods.ModLoader.Unloaded")})";
			else
				Tooltip = $"{Definition.DisplayName} [{Definition.Mod}]";
		}
	}

	public virtual void SetScale(float scale)
	{
		Scale = scale;
		Width.Set(DefaultBackgroundTexture.Width() * scale, 0f);
		Height.Set(DefaultBackgroundTexture.Height() * scale, 0f);
	}

	public override int CompareTo(object obj)
	{
		var other = obj as DefinitionOptionElement<T>;

		return Type.CompareTo(other.Type);
	}

	public override string ToString() => Definition.ToString();
}
