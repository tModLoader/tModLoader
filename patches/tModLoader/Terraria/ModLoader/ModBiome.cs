using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a biome added by a mod. It exists to centralize various biome related hooks, handling a lot of biome boilerplate. Use its various logic hooks act as if they were in ModPlayer, and use ModifyWorldGenTasks as if it were in ModWorld.
	/// </summary>
	public abstract class ModBiome : ModSystem
	{
		internal int index;

		// Basic Biome information
		public bool isPrimaryBiome = false;

		// Capture Camera
		public virtual CaptureBiome.TileColorStyle tileColorStyle => CaptureBiome.TileColorStyle.Normal;
		public virtual int waterStyle => 0;
		internal int backgroundTextureSlot => BackgroundTextureLoader.GetBackgroundSlot(biomeTexture);

		// Biome Atmosphere
		public virtual string biomeTexture => null;
		public virtual Texture2D GetBiomeMapBackground() => null;
		public virtual int biomeMusic() => 0;


		// Bestiary properties
		public virtual string translatedNPCMoodBiome => "the UnknownBiome";
		public virtual string translatedBiome => "UnknownBiome";
		public virtual string translatedBiomeActiveCondition => "SomeUnknownCondition";
		public virtual List<string> namesOfYourMusicTracks => new List<string>() { "SomeUnknownAudio" };

		protected override void Register() {
			ModTypeLookup<ModBiome>.Register(this);
			BiomeLoader.Add(this);
		}

		/// <summary>
		/// Return true if the player is in the biome.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsBiomeActive(Player player) => false;

		/// <summary>
		/// Returns the strength to which this biome is active. 0% < val < 200%. Defaults to 100% (ie 100) 
		/// Typically based on how many tiles are present relative to requisite.
		/// </summary>
		/// <returns></returns>
		public virtual byte GetBiomeStrength(Player player) => 100;

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
		public virtual void PostUpdateBiome(Player player) {
		}

		/// <summary>
		/// Allows you to modify the prices of NPC shops while in this biome.
		/// Use shopHelperInstance.LikeBiome, LoveBiome, etc on your NPC of choice in helperInfo.npc.type
		/// </summary>
		/// <param name="shopHelperInstance"></param>
		public virtual void ModifyShopPrices(HelperInfo helperInfo, ShopHelper shopHelperInstance) {

		}

		internal BiomeLoader.BiomeAtmosphere GetBiomeAtmosphere(Player player) {
			var result = new BiomeLoader.BiomeAtmosphere() {
				anyActive = true,
				mapBG = GetBiomeMapBackground,
				music = biomeMusic,
				weight = GetBiomeStrength(player),
				captureStyle = new CaptureBiome(backgroundTextureSlot, waterStyle, tileColorStyle)
			};

			if (!player.modBiomeFlags[index]) {
				result.weight = 0;  
			}

			return result;
		} 
	}
}
