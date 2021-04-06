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
	public abstract class ModBiome : IAtmosphericType
	{
		internal int index;

		// Basic Biome information
		public bool isPrimaryBiome = false;

		// Bestiary properties
		public virtual string translatedNPCMoodBiome => "the UnknownBiome";
		public virtual string translatedBiome => "UnknownBiome";
		public virtual string translatedBiomeActiveCondition => "SomeUnknownCondition";
		public virtual List<string> namesOfYourMusicTracks => new List<string>() { "SomeUnknownAudio" };

		protected override void Register() {
			RegisterAtmosphere();
			ModTypeLookup<ModBiome>.Register(this);
			BiomeLoader.Add(this);
		}

		public sealed override bool IsActive(Player player) => player.modBiomeFlags[index];

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
		public virtual void PostUpdateBiome(Player player) {
		}

		/// <summary>
		/// Allows you to modify the prices of NPC shops while in this biome.
		/// Use shopHelperInstance.LikeBiome, LoveBiome, etc on your NPC of choice in helperInfo.npc.type
		/// </summary>
		/// <param name="shopHelperInstance"></param>
		public virtual void ModifyShopPrices(HelperInfo helperInfo, ShopHelper shopHelperInstance) {

		}
	}
}
