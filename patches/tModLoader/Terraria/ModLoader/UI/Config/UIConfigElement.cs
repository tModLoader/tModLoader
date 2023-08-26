using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;
public abstract class UIConfigElement<T> : UIConfigElement
{
	public new T Value {
		get => (T)base.Value;
		set => base.Value = value;
	}

	public override bool FitsType(Type type) => type == typeof(T);
}

public abstract class UIConfigElement : UIElement
{
	#region Core
	/// <summary>
	/// If true, then this <see cref="UIConfigElement"/> is a dummy instance used in loading.
	/// </summary>
	public bool IsDummy { get; internal set; }
	/// <summary>
	/// The object containing the <see cref="MemberInfo"/> of this.<br/>
	/// Usually a <see cref="ModConfig"/> or an object inside of a <see cref="ModConfig"/>.
	/// </summary>
	public object ParentObject { get; internal set; }
	public int IndexInCollection { get; internal set; } // The index of this value in its parent if its parent is a collection
	public PropertyFieldWrapper MemberInfo { get; internal set; } // The variable info of this
	public object Value {
		get {
			if (Collection != null)
				return Collection[IndexInCollection];
			return MemberInfo.GetValue(ParentObject);
		}

		set {
			if (Collection != null)
				Collection[IndexInCollection] = value;
			else if (MemberInfo.CanWrite)
				MemberInfo.SetValue(ParentObject, value);

			ConfigManager.CheckSaveButton();
		}
	}
	public object DefaultValue;
	public bool ValueChanged => !ConfigManager.ObjectEquals(DefaultValue, Value);
	public IList Collection => IndexInCollection != -1 ? MemberInfo.GetValue(ParentObject) as IList : null;
	#endregion

	#region Attributes
	public LabelKeyAttribute LabelKeyAttribute { get; internal set; }
	public TooltipKeyAttribute TooltipKeyAttribute { get; internal set; }
	public BackgroundColorAttribute BackgroundColorAttribute { get; internal set; }

	// TODO move the below block to the elements where it is actually used
	public RangeAttribute RangeAttribute { get; internal set; }
	public IncrementAttribute IncrementAttribute { get; internal set; }
	public SliderAttribute SliderAttribute { get; internal set; }
	public DrawTicksAttribute DrawTicksAttribute { get; internal set; }
	public SliderColorAttribute SliderColorAttribute { get; internal set; }
	public JsonDefaultValueAttribute JsonDefaultValueAttribute { get; internal set; }

	public bool NullAllowed;
	public bool ShowReloadRequiredLabel;
	public bool ShowReloadRequiredTooltip;
	#endregion

	#region Textures
	public static Asset<Texture2D> PlayTexture { get; internal set; } = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay");
	public static Asset<Texture2D> DeleteTexture { get; internal set; } = Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete");
	public static Asset<Texture2D> PlusTexture { get; internal set; } = UICommon.ButtonPlusTexture;
	public static Asset<Texture2D> UpDownTexture { get; internal set; } = UICommon.ButtonUpDownTexture;
	public static Asset<Texture2D> CollapsedTexture { get; internal set; } = UICommon.ButtonCollapsedTexture;
	public static Asset<Texture2D> ExpandedTexture { get; internal set; } = UICommon.ButtonExpandedTexture;
	#endregion

	#region UI
	public UIText Label;
	public Color BackgroundColor {
		get {
			var col = BackgroundColorAttribute?.Color ?? UICommon.DefaultUIBlue;

			if (!MemberInfo?.CanWrite ?? false)
				col = Color.Gray;

			if (!IsMouseHovering)
				col *= 0.8f;

			return col;
		}
	}
	public bool DrawPanel = true;
	public bool DrawLabel = true;
	public bool DrawTooltip = true;
	#endregion

	internal void Bind(object parent, PropertyFieldWrapper memberInfo, int indexInCollection = -1)
	{
		// Helper method to reduced repeated code
		T GetAttribute<T>() where T : Attribute
			=> ConfigManager.GetCustomAttribute<T>(MemberInfo);

		if (IsDummy)
			return;

		ParentObject = parent;
		MemberInfo = memberInfo;
		IndexInCollection = indexInCollection;

		LabelKeyAttribute = GetAttribute<LabelKeyAttribute>();
		TooltipKeyAttribute = GetAttribute<TooltipKeyAttribute>();
		BackgroundColorAttribute = GetAttribute<BackgroundColorAttribute>();

		RangeAttribute = GetAttribute<RangeAttribute>();
		IncrementAttribute = GetAttribute<IncrementAttribute>();
		SliderAttribute = GetAttribute<SliderAttribute>();
		JsonDefaultValueAttribute = GetAttribute<JsonDefaultValueAttribute>();

		NullAllowed = GetAttribute<NullAllowedAttribute>() != null;
		ShowReloadRequiredTooltip = GetAttribute<ReloadRequiredAttribute>() != null;

		if (ShowReloadRequiredTooltip && Collection == null && ParentObject is ModConfig modConfig) {
			// Default ModConfig.NeedsReload logic currently only checks members of the ModConfig class, this mirrors that logic.
			ShowReloadRequiredLabel = true;
			// We need to check against the value in the load time config, not the value at the time of binding.
			ModConfig loadTimeConfig = ConfigManager.GetLoadTimeConfig(modConfig.Mod, modConfig.Name);
			DefaultValue = MemberInfo.GetValue(loadTimeConfig);
		}

		// UI
		this.WithPadding(4);
		Width.Set(0, 1f);
		Height.Set(30, 0f);

		Label = new UIText(DrawLabel ? GetLabel() : "") {
			HAlign = 0f,
			VAlign = 0.5f,
		};
		Append(Label);

		CreateUI();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (DrawPanel)
			DrawBackgroundPanel(spriteBatch);

		Label.SetText(DrawLabel ? GetLabel() : "");

		if (IsMouseHovering && DrawTooltip)
			UICommon.TooltipMouseText(GetTooltip());
	}

	private void DrawBackgroundPanel(SpriteBatch sb)
	{
		var dimensions = GetDimensions();
		var texture = TextureAssets.SettingsPanel;
		var color = BackgroundColor;

		sb.Draw(texture.Value, dimensions.ToRectangle(), color);
		// TODO
		/*
		// left edge
		sb.Draw(texture, position, new Rectangle(0, 0, 2, texture.Height), color);
		sb.Draw(texture, new Vector2(position.X + 2, position.Y), new Rectangle(2, 0, texture.Width - 4, texture.Height), color, 0f, Vector2.Zero, new Vector2((width - 4) / (texture.Width - 4), (height - 4) / (texture.Height - 4)), SpriteEffects.None, 0f);
		sb.Draw(texture, new Vector2(position.X + width - 2, position.Y), new Rectangle(texture.Width - 2, 0, 2, texture.Height), color);

		// width and height include border
		sb.Draw(texture, position + new Vector2(0, 2), new Rectangle(0, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(2, height - 4), SpriteEffects.None, 0f);
		sb.Draw(texture, position + new Vector2(width - 2, 2), new Rectangle(0, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(2, height - 4), SpriteEffects.None, 0f);
		sb.Draw(texture, position + new Vector2(2, 0), new Rectangle(2, 0, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, 2), SpriteEffects.None, 0f);
		sb.Draw(texture, position + new Vector2(2, height - 2), new Rectangle(2, 0, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, 2), SpriteEffects.None, 0f);

		sb.Draw(texture, position + new Vector2(2, 2), new Rectangle(2, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, (height - 4) / 2), SpriteEffects.None, 0f);
		sb.Draw(texture, position + new Vector2(2, 2 + ((height - 4) / 2)), new Rectangle(2, 16, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, (height - 4) / 2), SpriteEffects.None, 0f);*/
	}

	public abstract bool FitsType(Type type);

	public abstract void CreateUI();

	public abstract void RefreshUI();

	public virtual string GetLabel()
	{
		string label = ConfigManager.GetLocalizedLabel(MemberInfo);

		if (ShowReloadRequiredLabel && ValueChanged) {
			label += " - [c/FF0000:" + Language.GetTextValue("tModLoader.ModReloadRequired") + "]";
		}

		return label;
	}

	public virtual string GetTooltip()
	{
		string tooltip = ConfigManager.GetLocalizedTooltip(MemberInfo);

		if (ShowReloadRequiredTooltip) {
			tooltip += string.IsNullOrEmpty(tooltip) ? "" : "\n";
			tooltip += $"[c/{Color.Orange.Hex3()}:" + Language.GetTextValue("tModLoader.ModReloadRequiredMemberTooltip") + "]";
		}

		return tooltip;
	}
}