using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class InfoDisplay : ModTexturedType
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
		/// This is the internal ID of this InfoDisplay.
		/// </summary>
		public int Type { get; internal set; }

		/// <summary>
		/// This is the translation that is used behind <see cref="DisplayName"/>. The translation will show up when hovering over this info display.
		/// </summary>
		public ModTranslation InfoName { get; internal set; }

		/// <summary>
		/// This is the name that will show up when hovering over this info display.
		/// </summary>
		public string DisplayName => DisplayNameInternal;

		internal protected virtual string DisplayNameInternal => InfoName.GetTranslation(Language.ActiveCulture);

		/// <summary>
		/// This dictates whether or not this info display should be active.
		/// </summary>
		public virtual bool Active() => false;

		/// <summary>
		/// This is the value that will show up when viewing this display in normal play, right next to the icon.
		/// </summary>
		public abstract string DisplayValue();

		public sealed override void SetupContent() {
			ModContent.Request<Texture2D>(Texture);
			SetDefaults();
		}

		/// <summary>
		/// You can assign values to InfoName here.
		/// </summary>
		public virtual void SetDefaults() {
		}

		protected override void Register() {
			InfoName = LocalizationLoader.GetOrCreateTranslation(Mod, $"InfoDisplayName.{Name}");

			ModTypeLookup<InfoDisplay>.Register(this);

			Type = InfoDisplayLoader.Add(this);
		}
	}
}
