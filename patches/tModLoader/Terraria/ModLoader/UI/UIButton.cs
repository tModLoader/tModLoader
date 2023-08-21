using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.UI;

namespace Terraria.ModLoader.UI;
/// <summary>
/// A text panel that supports hover and click sounds, hover colours, and alternate colours.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UIButton<T> : UIAutoScaleTextTextPanel<T>
{
	public SoundStyle? HoverSound = null;
	public SoundStyle? ClickSound = null;

	public T HoverText = default;
	public T AltHoverText = default;
	public bool TooltipText = false;

	public Color HoverPanelColor = UICommon.DefaultUIBlue;
	public Color HoverBorderColor = UICommon.DefaultUIBorderMouseOver;

	public Color? AltPanelColor = null;
	public Color? AltBorderColor = null;

	public Color? AltHoverPanelColor = null;
	public Color? AltHoverBorderColor = null;

	public Func<bool> UseAltColours = () => false;

	private Color? _panelColor = null;
	private Color? _borderColor = null;

	public UIButton(T text, float textScaleMax = 1, bool large = false) : base(text, textScaleMax, large)
	{
	}

	public override void Recalculate()
	{
		base.Recalculate();

		_panelColor ??= BackgroundColor;
		_borderColor ??= BorderColor;

		AltPanelColor ??= BackgroundColor;
		AltBorderColor ??= BorderColor;

		AltHoverPanelColor ??= HoverPanelColor;
		AltHoverBorderColor ??= HoverBorderColor;
	}

	protected void SetPanelColours()
	{
		bool altCondition = UseAltColours();
		if (IsMouseHovering)
		{
			BackgroundColor = altCondition ? AltHoverPanelColor.Value : HoverPanelColor;
			BorderColor = altCondition ? AltHoverBorderColor.Value : HoverBorderColor;
		}
		else
		{
			BackgroundColor = altCondition ? AltPanelColor.Value : _panelColor.Value;
			BorderColor = altCondition ? AltBorderColor.Value : _borderColor.Value;
		}
	}

	public override void OnActivate()
	{
		SetPanelColours();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		SetPanelColours();

		if (IsMouseHovering)
		{
			string text = (UseAltColours() ? AltHoverText : HoverText).ToString();
			if (TooltipText)
				UICommon.TooltipMouseText(text);
			else
				Main.instance.MouseText(text);
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);

		if (HoverSound != null)
			SoundEngine.PlaySound(HoverSound.Value);
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		base.LeftClick(evt);

		if (ClickSound != null)
			SoundEngine.PlaySound(ClickSound.Value);
	}
}
