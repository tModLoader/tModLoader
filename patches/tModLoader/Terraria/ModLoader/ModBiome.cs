using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a biome added by a mod. It exists to centralize various biome related hooks, handling a lot of biome boilerplate. Use its various logic hooks act as if they were in ModPlayer, and use ModifyWorldGenTasks as if it were in ModWorld.
	/// </summary>
	public abstract class ModBiome : ModType
	{
		internal int index;

		protected override void Register() {
			ModTypeLookup<ModBiome>.Register(this);
			BiomeLoader.Add(this);
		}

		internal void Update(Player player, ref bool value) {
			bool prev = value;
			value = IsBiomeActive(player);

			if (!prev && value)
				OnEnter(player);
			else if (!value)
				OnLeave(player);
		}

		/// <summary>
		/// Return true if the player is in the biome.
		/// </summary>
		/// <returns></returns>
		protected virtual bool IsBiomeActive(Player player) => false;

		/// <summary>
		/// Override this hook to make things happen when the player enters the biome.
		/// </summary>
		protected virtual void OnEnter(Player player) {
		}

		/// <summary>
		/// Override this hook to make things happen when the player leaves the biome.
		/// </summary>
		protected virtual void OnLeave(Player player) {
		}

		/// <summary>
		/// Allows you to create special visual effects in the area around the player. For example, the blood moon's red filter on the screen or the slime rain's falling slime in the background. You must create classes that override Terraria.Graphics.Shaders.ScreenShaderData or Terraria.Graphics.Effects.CustomSky, add them in your mod's Load hook, then call Player.ManageSpecialBiomeVisuals. See the ExampleMod if you do not have access to the source code.
		/// </summary>
		public virtual void UpdateBiomeVisuals(Player player) {
		}
	}
}
