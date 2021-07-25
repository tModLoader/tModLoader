using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a biome added by a mod. It exists to centralize various biome related hooks, handling a lot of biome boilerplate.
	/// </summary>
	public abstract class ModBiome : ModSceneEffect
	{
		// Basic Biome information
		/// <summary>
		/// Whether or not this biome impacts NPC shop prices.
		/// </summary>
		public virtual bool IsPrimaryBiome => false;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
		public override int Music => 0;

		internal int ZeroIndexType => Type - BiomeLoader.VanillaPrimaryBiomeCount; 

		// Bestiary properties
		/// <summary>
		/// The display name for this biome in the bestiary.
		/// </summary>
		public ModTranslation DisplayName { get; private set; }
		/// <summary>
		/// The path to the 30x30 texture that will appear for this biome in the bestiary.
		/// </summary>
		public virtual string BestiaryIcon => null;
		/// <summary>
		/// The path to the background texture that will appear for this biome behind npcs in the bestiary.
		/// </summary>
		public virtual string BackgroundPath => null;
		/// <summary>
		/// The color of the bestary background.
		/// </summary>
		public virtual Color? BackgroundColor => null;

		public GameContent.Bestiary.ModBiomeBestiaryInfoElement ModBiomeBestiaryInfoElement { get; internal set; }

		protected sealed override void Register() {
			Type = LoaderManager.Get<BiomeLoader>().Register(this);
			RegisterSceneEffect(this);

			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"BiomeName.{Name}");

			ModBiomeBestiaryInfoElement = new GameContent.Bestiary.ModBiomeBestiaryInfoElement(Mod, DisplayName.Key, BestiaryIcon, BackgroundPath, BackgroundColor);
		}

		public sealed override void SetupContent() {
			SetStaticDefaults();
		}

		/// <summary>
		/// IsSceneEffectActive is auto-forwarded to read the result of IsBiomeActive.
		/// Do not need to implement when creating your ModBiome.
		/// </summary>
		public sealed override bool IsSceneEffectActive(Player player) => player.modBiomeFlags[ZeroIndexType];

		/// <summary>
		/// This is where you can set values for DisplayName.
		/// </summary>
		public virtual void SetStaticDefaults() {
		}

		/// <summary>
		/// Return true if the player is in the biome.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsBiomeActive(Player player) => false;

		/// <summary>
		/// Override this hook to make things happen when the player enters the biome.
		/// </summary>
		public virtual void OnEnter(Player player) {
		}

		/// <summary>
		/// Override this hook to make things happen when the player is in the biome.
		/// </summary>
		public virtual void OnInBiome(Player player) {}

		/// <summary>
		/// Override this hook to make things happen when the player leaves the biome.
		/// </summary>
		public virtual void OnLeave(Player player) {
		}

		/// <summary>
		/// Allows you to create special visual effects in the area around the player. For example, the blood moon's red filter on the screen or the slime rain's falling slime in the background. You must create classes that override Terraria.Graphics.Shaders.ScreenShaderData or Terraria.Graphics.Effects.CustomSky, add them in your mod's Load hook, then call Player.ManageSpecialBiomeVisuals. See the ExampleMod if you do not have access to the source code.
		/// </summary>
		public virtual void BiomeVisuals(Player player) {
		}
	}
}
