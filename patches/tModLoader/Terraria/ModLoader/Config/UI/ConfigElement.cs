using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.Config.UI;

public abstract class ConfigElement<T> : ConfigElement
{
	protected virtual T Value {
		get => (T)GetObject();
		set => SetObject(value);
	}
}

public abstract class ConfigElement : UIElement
{
	private Color backgroundColor; // TODO inherit parent object color?

	public int Index { get; set; }

	public const int flashRate = 120;
	public bool Flashing { get; set; }

	protected Asset<Texture2D> PlayTexture { get; set; } = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay");
	protected Asset<Texture2D> DeleteTexture { get; set; } = Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete");
	protected Asset<Texture2D> PlusTexture { get; set; } = UICommon.ButtonPlusTexture;
	//protected Texture2D UpArrowTexture { get; set; } = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonIncrement.png"));
	//protected Texture2D DownArrowTexture { get; set; } = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));
	protected Asset<Texture2D> UpDownTexture { get; set; } = UICommon.ButtonUpDownTexture;
	protected Asset<Texture2D> CollapsedTexture { get; set; } = UICommon.ButtonCollapsedTexture;
	protected Asset<Texture2D> ExpandedTexture { get; set; } = UICommon.ButtonExpandedTexture;

	// Provides access to the field/property contained in the item
	protected internal PropertyFieldWrapper MemberInfo { get; set; }
	// The object that contains the memberInfo. This is usually a ModConfig instance or an object instance contained within a ModConfig instance.
	protected object Item { get; set; }
	// If non-null, the memberInfo actually refers to the collection containing this item and array and index need to be used to assign this data
	protected IList List { get; set; }
	// Attributes
	protected LabelKeyAttribute LabelAttribute;
	protected string Label;
	protected TooltipKeyAttribute TooltipAttribute;
	protected BackgroundColorAttribute BackgroundColorAttribute;
	protected RangeAttribute RangeAttribute;
	protected IncrementAttribute IncrementAttribute;
	protected JsonDefaultValueAttribute JsonDefaultValueAttribute;
	// Etc
	protected bool NullAllowed { get; set; }
	protected internal Func<string> TextDisplayFunction { get; set; }
	protected Func<string> TooltipFunction { get; set; }
	protected bool DrawLabel { get; set; } = true;
	protected bool ReloadRequired { get; set; }
	protected bool ShowReloadRequiredTooltip { get; set; }
	protected object OldValue { get; set; }
	protected bool ValueChanged => !ConfigManager.ObjectEquals(OldValue, GetObject());

	public ConfigElement()
	{
		Width.Set(0f, 1f);
		Height.Set(30f, 0f);
	}

	/// <summary>
	/// Bind must always be called after the ctor and serves to facilitate a convenient inheritance workflow for custom ConfigElemets from mods.
	/// </summary>
	public void Bind(PropertyFieldWrapper memberInfo, object item, IList array, int index)
	{
		MemberInfo = memberInfo;
		Item = item;
		List = array;
		Index = index;
		backgroundColor = UICommon.DefaultUIBlue;
	}

	public virtual void OnBind()
	{
		LabelAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<LabelKeyAttribute>(MemberInfo, Item, List);
		Label = ConfigManager.GetLocalizedLabel(MemberInfo);
		// Potential TODO if highly requested: Support interpolating value itself into label.
		TextDisplayFunction = () => Label;

		TooltipAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<TooltipKeyAttribute>(MemberInfo, Item, List);
		string tooltip = ConfigManager.GetLocalizedTooltip(MemberInfo);
		if (tooltip != null) {
			TooltipFunction = () => tooltip;
		}

		BackgroundColorAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<BackgroundColorAttribute>(MemberInfo, Item, List);

		if (BackgroundColorAttribute != null) {
			backgroundColor = BackgroundColorAttribute.Color;
		}

		RangeAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<RangeAttribute>(MemberInfo, Item, List);
		IncrementAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<IncrementAttribute>(MemberInfo, Item, List);
		NullAllowed = ConfigManager.GetCustomAttributeFromMemberThenMemberType<NullAllowedAttribute>(MemberInfo, Item, List) != null;
		JsonDefaultValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<JsonDefaultValueAttribute>(MemberInfo, Item, List);
		ShowReloadRequiredTooltip = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ReloadRequiredAttribute>(MemberInfo, Item, List) != null;

		if (ShowReloadRequiredTooltip && List == null && Item is ModConfig modConfig) {
			// Default ModConfig.NeedsReload logic currently only checks members of the ModConfig class, this mirrors that logic.
			ReloadRequired = true;
			// We need to check against the value in the load time config, not the value at the time of binding.
			ModConfig loadTimeConfig = ConfigManager.GetLoadTimeConfig(modConfig.Mod, modConfig.Name);
			OldValue = MemberInfo.GetValue(loadTimeConfig);
		 }
	}

	protected virtual void SetObject(object value)
	{
		if (List != null) {
			List[Index] = value;
			Interface.modConfig.SetPendingChanges();
			return;
		}

		if (!MemberInfo.CanWrite)
			return;

		MemberInfo.SetValue(Item, value);
		Interface.modConfig.SetPendingChanges();
	}

	protected virtual object GetObject()
	{
		if (List != null)
			return List[Index];

		return MemberInfo.GetValue(Item);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		CalculatedStyle dimensions = base.GetDimensions();
		float settingsWidth = dimensions.Width + 1f;
		Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
		Vector2 baseScale = new Vector2(0.8f);
		Color color = IsMouseHovering ? Color.White : Color.White;

		if (!MemberInfo.CanWrite)
			color = Color.Gray;

		//color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 1f : 0f);
		Color panelColor = base.IsMouseHovering ? backgroundColor : backgroundColor.MultiplyRGBA(new Color(180, 180, 180));
		Vector2 position = vector;

		if (Flashing) {
			float ratio = Utils.Turn01ToCyclic010(((Interface.modConfig.UpdateCount % flashRate) / (float)flashRate)) * 0.5f + 0.5f;
			panelColor = Color.Lerp(panelColor, Color.White, MathF.Pow(ratio, 2));
		}

		DrawPanel2(spriteBatch, position, TextureAssets.SettingsPanel.Value, settingsWidth, dimensions.Height, panelColor);

		if (DrawLabel) {
			position.X += 8f;
			position.Y += 8f;

			string label = TextDisplayFunction();
			if (ReloadRequired && ValueChanged) {
				label += " - [c/FF0000:" + Language.GetTextValue("tModLoader.ModReloadRequired") + "]";
			}

			// TODO: Support chat tag hover?
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, label, position, color, 0f, Vector2.Zero, baseScale, settingsWidth, 2f);
		}

		if (IsMouseHovering && TooltipFunction != null) {
			string tooltip = TooltipFunction();

			// TODO - Add line for default value?

			if (ShowReloadRequiredTooltip) {
				tooltip += string.IsNullOrEmpty(tooltip) ? "" : "\n";
				tooltip += $"[c/{Color.Orange.Hex3()}:" + Language.GetTextValue("tModLoader.ModReloadRequiredMemberTooltip") + "]";
			}

			UIModConfig.Tooltip = tooltip;
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		Flashing = false;
	}

	public static void DrawPanel2(SpriteBatch spriteBatch, Vector2 position, Texture2D texture, float width, float height, Color color)
	{
		// left edge
		//	spriteBatch.Draw(texture, position, new Rectangle(0, 0, 2, texture.Height), color);
		//	spriteBatch.Draw(texture, new Vector2(position.X + 2, position.Y), new Rectangle(2, 0, texture.Width - 4, texture.Height), color, 0f, Vector2.Zero, new Vector2((width - 4) / (texture.Width - 4), (height - 4) / (texture.Height - 4)), SpriteEffects.None, 0f);
		//	spriteBatch.Draw(texture, new Vector2(position.X + width - 2, position.Y), new Rectangle(texture.Width - 2, 0, 2, texture.Height), color);

		//width and height include border
		spriteBatch.Draw(texture, position + new Vector2(0, 2), new Rectangle(0, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(2, height - 4), SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, position + new Vector2(width - 2, 2), new Rectangle(0, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(2, height - 4), SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, position + new Vector2(2, 0), new Rectangle(2, 0, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, 2), SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, position + new Vector2(2, height - 2), new Rectangle(2, 0, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, 2), SpriteEffects.None, 0f);

		spriteBatch.Draw(texture, position + new Vector2(2, 2), new Rectangle(2, 2, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, (height - 4) / 2), SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, position + new Vector2(2, 2 + ((height - 4) / 2)), new Rectangle(2, 16, 1, 1), color, 0, Vector2.Zero, new Vector2(width - 4, (height - 4) / 2), SpriteEffects.None, 0f);
	}
}

internal class HeaderElement : UIElement
{
	private readonly string header;

	public HeaderElement(string header)
	{
		this.header = header;
		Vector2 size = ChatManager.GetStringSize(FontAssets.ItemStack.Value, this.header, Vector2.One, 532); // TODO: Max Width can't be known at this time.
		Width.Set(0f, 1f);
		Height.Set(size.Y + 6, 0f);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		CalculatedStyle dimensions = base.GetDimensions();
		float settingsWidth = dimensions.Width + 1f;
		Vector2 position = new Vector2(dimensions.X, dimensions.Y) + new Vector2(8);

		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dimensions.X + 10, (int)dimensions.Y + (int)dimensions.Height - 2, (int)dimensions.Width - 20, 1), Color.LightGray);

		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, header, position, Color.White, 0f, Vector2.Zero, new Vector2(1f), settingsWidth - 20, 2f);
	}
}