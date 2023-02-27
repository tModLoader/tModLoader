using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader;

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
	/// The color when no valuable information is displayed
	/// </summary>
	public static Color InactiveInfoTextColor => new(100, 100, 100, Main.mouseTextColor);

	/// <summary>
	/// This is the internal ID of this InfoDisplay.
	/// </summary>
	public int Type { get; internal set; }

	public string LocalizationCategory => "InfoDisplays";

	/// <summary>
	/// This is the name that will show up when hovering over this info display.
	/// </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// This dictates whether or not this info display should be active.
	/// </summary>
	public virtual bool Active() => false;

	/// <summary>
	/// This is the value that will show up when viewing this display in normal play, right next to the icon.
	/// <br/>Set <paramref name="displayColor"/> to <see cref="InactiveInfoTextColor"/> if your display value is "zero"/shows no valuable information.
	/// </summary>
	/// <param name="displayColor">The color the text is displayed as.</param>
	public abstract string DisplayValue(ref Color displayColor);

	public sealed override void SetupContent()
	{
		ModContent.Request<Texture2D>(Texture);
		SetStaticDefaults();
	}

	protected sealed override void Register()
	{
		ModTypeLookup<InfoDisplay>.Register(this);

		Type = InfoDisplayLoader.Add(this);
	}
}
