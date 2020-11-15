using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a biome added by a mod. It exists to centralize various biome related hooks, handling a lot of biome boilerplate. Use its various logic hooks act as if they were in ModPlayer, and use ModifyWorldGenTasks as if it were in ModWorld.
	/// </summary>
	public abstract class ModBiome : ModType
	{
		/// <summary>
		/// The player associated with this ModBiome object. This does not relate to the local player in any way.
		/// </summary>
		public Player player { get; internal set; }

		internal int index;

		/// <summary>
		/// Whether or not the player is in this specific biome.
		/// </summary>
		public bool Active { get; internal set; }

		protected override void Register() {
			ModTypeLookup<ModBiome>.Register(this);
			BiomeLoader.Add(this);
		}

		internal void Update() {
			bool prev = Active;
			Active = IsBiomeActive();

			if (!prev && Active)
				OnEnter();
			else if (!Active)
				OnLeave();
		}

		/// <summary>
		/// Return true if the player is in the biome.
		/// </summary>
		/// <returns></returns>
		protected virtual bool IsBiomeActive() => false;

		/// <summary>
		/// Override this hook to make things happen when the player enters the biome.
		/// </summary>
		protected virtual void OnEnter() {
		}

		/// <summary>
		/// Override this hook to make things happen when the player leaves the biome.
		/// </summary>
		protected virtual void OnLeave() {
		}

		/// <summary>
		/// Allows you to create special visual effects in the area around the player. For example, the blood moon's red filter on the screen or the slime rain's falling slime in the background. You must create classes that override Terraria.Graphics.Shaders.ScreenShaderData or Terraria.Graphics.Effects.CustomSky, add them in your mod's Load hook, then call Player.ManageSpecialBiomeVisuals. See the ExampleMod if you do not have access to the source code.
		/// </summary>
		public virtual void UpdateBiomeVisuals() {
		}

		/// <summary>
		/// Exactly the same as ModWorld.ModifyWorldGenTasks except it's tied to this ModBiome.
		/// </summary>
		public virtual void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
		}
	}
}
