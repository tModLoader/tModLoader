using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Config;
public abstract class UIConfigElement<T> : UIConfigElement
{
	public new T Value {
		get => (T)base.Value;
		set => base.Value = value;
	}

	public override bool FitsType(Type type) => type == typeof(T);
}

// TODO: revert and restore buttons
// TODO: text max width
public abstract class UIConfigElement : UIElement
{
	#region Attributes
	public LabelKeyAttribute LabelKeyAttribute { get; internal set; }
	public TooltipKeyAttribute TooltipKeyAttribute { get; internal set; }
	public BackgroundColorAttribute BackgroundColorAttribute { get; internal set; }
	public JsonDefaultValueAttribute JsonDefaultValueAttribute { get; internal set; }

	public bool NullAllowed;
	public bool ShowReloadRequiredLabel;
	public bool ShowReloadRequiredTooltip;
	#endregion

	#region Textures
	// TODO: improve these textures
	public static Asset<Texture2D> PlayTexture { get; internal set; } = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay");
	public static Asset<Texture2D> DeleteTexture { get; internal set; } = Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete");
	public static Asset<Texture2D> PlusTexture { get; internal set; } = UICommon.ButtonPlusTexture;
	public static Asset<Texture2D> UpDownTexture { get; internal set; } = UICommon.ButtonUpDownTexture;
	public static Asset<Texture2D> CollapsedTexture { get; internal set; } = UICommon.ButtonCollapsedTexture;
	public static Asset<Texture2D> ExpandedTexture { get; internal set; } = UICommon.ButtonExpandedTexture;
	#endregion

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

			UIModConfig.Instance.RefreshUI();
		}
	}
	public object DefaultValue;
	public bool ValueChanged => !ConfigManager.ObjectEquals(DefaultValue, Value);
	public IList Collection => IndexInCollection != -1 ? MemberInfo.GetValue(ParentObject) as IList : null;
	public int IndexInUIList { get; internal set; } // See CompareTo for more info on why this exists

	// Aautomatically determining which config elements go on which fields
	public abstract bool FitsType(Type type);

	// Basically, config elements can have a weird order in a uilist because of the CompareTo of default ui elements
	// To counter this, we store the index in the ui list when it is added, so we can sort them properly
	public override int CompareTo(object obj)
	{
		if (obj is UIConfigElement element)
			return IndexInUIList.CompareTo(element.IndexInUIList);
		return base.CompareTo(obj);
	}

	protected T GetAttribute<T>() where T : Attribute
			=> ConfigManager.GetCustomAttribute<T>(MemberInfo);

	// Called to setup the element
	public UIConfigElement Bind(object parent, PropertyFieldWrapper memberInfo, int indexInUIList, int indexInCollection = -1)
	{
		if (IsDummy)
			return this;

		ParentObject = parent;
		MemberInfo = memberInfo;
		IndexInUIList = indexInUIList;
		IndexInCollection = indexInCollection;

		LabelKeyAttribute = GetAttribute<LabelKeyAttribute>();
		TooltipKeyAttribute = GetAttribute<TooltipKeyAttribute>();
		BackgroundColorAttribute = GetAttribute<BackgroundColorAttribute>();
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

		// Activate isn't normally called during append but we need to initialize ui
		Activate();

		return this;
	}
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

	public float TextScale = 0.8f;
	public bool DrawPanel = true;
	public bool DrawLabel = true;
	public bool DrawTooltip = true;

	public override void OnInitialize()
	{
		Width.Set(0, 1f);
		Height.Set(30, 0f);
		SetPadding(4);
		PaddingLeft = 8;

		Label = new UIText(DrawLabel ? GetLabel() : "", TextScale) {
			HAlign = 0f,
			VAlign = 0.5f,
		};
		Append(Label);
	}

	public override void Recalculate()
	{
		Label.SetText(DrawLabel ? GetLabel() : "");

		base.Recalculate();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (DrawPanel)
			DrawBackgroundPanel(spriteBatch);

		if (IsMouseHovering && DrawTooltip)
			UICommon.TooltipMouseText(GetTooltip());
	}

	protected void DrawBackgroundPanel(SpriteBatch sb)
	{
		// TODO: finish panel drawing
		var dimensions = GetDimensions();
		Utils.DrawSettingsPanel(sb, dimensions.Position(), dimensions.Width, BackgroundColor);
	}

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

		// TODO: default value tooltip?

		return tooltip;
	}
	#endregion
}