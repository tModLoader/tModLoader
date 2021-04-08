using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a biome added by a mod. It exists to centralize various biome related hooks, handling a lot of biome boilerplate.
	/// </summary>
	public abstract class ModBiome : ModAVFX
	{
		internal int index;

		// Basic Biome information
		/// <summary>
		/// Whether or not this biome impacts NPC shop prices.
		/// </summary>
		public virtual bool IsPrimaryBiome => false;
		public override AVFXPriority Priority => AVFXPriority.BiomeLow;

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

		public GameContent.Bestiary.ModBiomeBestiaryInfoElement ModBiomeBestiaryInfoElement;

		protected sealed override void Register() {
			ModTypeLookup<ModBiome>.Register(this);
			BiomeLoader.Add(this);
			RegisterAVFX(this);

			DisplayName = Mod.GetOrCreateTranslation($"Mods.{Mod.Name}.BiomeName.{Name}");

			ModBiomeBestiaryInfoElement = new GameContent.Bestiary.ModBiomeBestiaryInfoElement(Mod, DisplayName.Key, BestiaryIcon, BackgroundPath, BackgroundColor);
		}

		public sealed override void SetupContent() {
			SetStaticDefaults();
		}

		public sealed override bool IsActive(Player player) => player.modBiomeFlags[index];

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

		/// <summary>
		/// Allows you to modify the prices of NPC shops while in this biome.
		/// Use shopHelperInstance.LikeBiome, LoveBiome, etc on your NPC of choice in helperInfo.npc.type
		/// </summary>
		/// <param name="helperInfo"></param>
		/// <param name="shopHelperInstance"></param>
		public virtual void ModifyShopPrices(HelperInfo helperInfo, ShopHelper shopHelperInstance) {
		}
	}
}
