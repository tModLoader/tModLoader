using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

/// <summary>
/// Represents an informational display toggle typically provided by <see href="https://terraria.wiki.gg/wiki/Informational_Accessories">informational accessories</see>.<para/>
/// The <see cref="Active"/> property determines if the InfoDisplay could be shown to the user. The game tracks the players desired visibility of active InfoDisplay through the <see cref="Player.hideInfo"/> array.
/// </summary>
public abstract class InfoDisplay : ModTexturedType, ILocalizedModType
{
	public static InfoDisplay Watches { get; private set; } = new WatchesInfoDisplay();
	public static InfoDisplay WeatherRadio { get; private set; } = new WeatherRadioInfoDisplay();
	public static InfoDisplay Sextant { get; private set; } = new SextantInfoDisplay();
	public static InfoDisplay FishFinder { get; private set; } = new FishFinderInfoDisplay();
	public static InfoDisplay MetalDetector { get; private set; } = new MetalDetectorInfoDisplay();
	public static InfoDisplay LifeformAnalyzer { get; private set; } = new LifeformAnalyzerInfoDisplay();
	public static InfoDisplay Radar { get; private set; } = new RadarInfoDisplay();
	public static InfoDisplay TallyCounter { get; private set; } = new TallyCounterInfoDisplay();
	internal static InfoDisplay Dummy { get; private set; } = new DummyInfoDisplay();
	public static InfoDisplay DPSMeter { get; private set; } = new DPSMeterInfoDisplay();
	public static InfoDisplay Stopwatch { get; private set; } = new StopwatchInfoDisplay();
	public static InfoDisplay Compass { get; private set; } = new CompassInfoDisplay();
	public static InfoDisplay DepthMeter { get; private set; } = new DepthMeterInfoDisplay();

	/// <summary>
	/// The color when no valuable information is displayed.
	/// </summary>
	public static Color InactiveInfoTextColor => new(100, 100, 100, Main.mouseTextColor);

	/// <summary>
	/// The golden color variant of the displays text.<br/>
	/// Used by the Lifeform Analyzer.
	/// </summary>
	public static Color GoldInfoTextColor => new(Main.OurFavoriteColor.R, Main.OurFavoriteColor.G, Main.OurFavoriteColor.B, Main.mouseTextColor);

	/// <summary>
	/// The golden color variant of the displays text shadow.<br/>
	/// Used by the Lifeform Analyzer.
	/// </summary>
	public static Color GoldInfoTextShadowColor => new(Main.OurFavoriteColor.R / 10, Main.OurFavoriteColor.G / 10, Main.OurFavoriteColor.B / 10, Main.mouseTextColor);

	/// <summary>
	/// The path to the texture vanilla info displays use when hovering over an info display.
	/// </summary>
	public static string VanillaHoverTexture => "Terraria/Images/UI/InfoIcon_13";

	/// <summary>
	/// This is the internal ID of this InfoDisplay.<para/>
	/// Also serves as the index for <see cref="Player.hideInfo"/>.
	/// </summary>
	public int Type { get; internal set; }

	public virtual string LocalizationCategory => "InfoDisplays";

	/// <summary>
	/// The outline texture drawn when the icon is hovered and toggleable. By default a circular outline texture is used. Override this method and return <c>Texture + "_Hover"</c> or any other texture path to specify a custom outline texture for use with icons that are not circular.
	/// </summary>
	public virtual string HoverTexture => VanillaHoverTexture;

	/// <summary>
	/// This is the name that will show up when hovering over this info display.
	/// </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// This dictates whether or not this info display should be active.<para/>
	/// This is usually determined by player-specific values, typically set in <see cref="ModItem.UpdateInventory"/>.
	/// </summary>
	public virtual bool Active() => false;

	[Obsolete]
	private string DisplayValue_Obsolete(ref Color displayColor)
		=> DisplayValue(ref displayColor);

	// Obsoleted on 2023-09-18
	// TODO: Remove a few months after obsoletion, convert the modern DisplayValue overload back to 'abstract', and remove validation in ValidateType().
	[Obsolete("Use the (ref Color displayColor, ref Color displayShadowColor) overload", error: true)]
	public virtual string DisplayValue(ref Color displayColor)
		=> throw new NotImplementedException();

	/// <summary>
	/// This is the value that will show up when viewing this display in normal play, right next to the icon.
	/// <br/>Set <paramref name="displayColor"/> to <see cref="InactiveInfoTextColor"/> if your display value is "zero"/shows no valuable information.
	/// </summary>
	/// <param name="displayColor">The color the text is displayed as.</param>
	/// <param name="displayShadowColor">The outline color text is displayed as.</param>
	public virtual string DisplayValue(ref Color displayColor, ref Color displayShadowColor)
		=> DisplayValue_Obsolete(ref displayColor);

	public sealed override void SetupContent()
	{
		_ = DisplayName;
		ModContent.Request<Texture2D>(Texture);
		SetStaticDefaults();
	}

	protected override void ValidateType()
	{
		base.ValidateType();
		MustOverrideDisplayValue();
	}

	private delegate string DisplayValueMethodType(ref Color displayColor, ref Color displayShadowColor);
	private delegate string OldDisplayValueMethodType(ref Color displayColor);
	[Obsolete]
	private void MustOverrideDisplayValue()
	{
		if (!LoaderUtils.HasOverride(this, t => (DisplayValueMethodType)t.DisplayValue) && !LoaderUtils.HasOverride(this, t => (OldDisplayValueMethodType)t.DisplayValue))
			throw new Exception($"{GetType()} must override {nameof(DisplayValue)}.");
	}

	protected sealed override void Register()
	{
		ModTypeLookup<InfoDisplay>.Register(this);

		Type = InfoDisplayLoader.Add(this);
	}
}
