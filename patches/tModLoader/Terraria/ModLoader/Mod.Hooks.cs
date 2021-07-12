using ReLogic.Content.Sources;
using System.IO;
using Terraria.ModLoader.Assets;

namespace Terraria.ModLoader
{
	partial class Mod
	{
		public virtual IContentSource CreateDefaultContentSource() {
			return new TModContentSource(File);
		}

		/// <summary>
		/// Override this method to add most of your content to your mod. Here you will call other methods such as AddItem. This is guaranteed to be called after all content has been autoloaded.
		/// </summary>
		public virtual void Load() {
		}

		/// <summary>
		/// Allows you to load things in your mod after its content has been setup (arrays have been resized to fit the content, etc).
		/// </summary>
		public virtual void PostSetupContent() {
		}

		/// <summary>
		/// This is called whenever this mod is unloaded from the game. Use it to undo changes that you've made in Load that aren't automatically handled (for example, modifying the texture of a vanilla item). Mods are guaranteed to be unloaded in the reverse order they were loaded in.
		/// </summary>
		public virtual void Unload() {
		}

		/// <summary>
		/// The amount of extra buff slots this mod desires for Players. This value is checked after Mod.Load but before Mod.PostSetupContent. The actual number of buffs the player can use will be 22 plus the max value of all enabled mods. In-game use Player.MaxBuffs to check the maximum number of buffs.
		/// </summary>
		public virtual uint ExtraPlayerBuffSlots { get; }

		/// <summary>
		/// Override this method to add recipe groups to this mod. You must add recipe groups by calling the RecipeGroup.RegisterGroup method here. A recipe group is a set of items that can be used interchangeably in the same recipe.
		/// </summary>
		public virtual void AddRecipeGroups() {
		}

		/// <summary>
		/// Override this method to add recipes to the game. It is recommended that you do so through instances of Recipe, since it provides methods that simplify recipe creation.
		/// </summary>
		public virtual void AddRecipes() {
		}

		/// <summary>
		/// This provides a hook into the mod-loading process immediately after recipes have been added. You can use this to edit recipes added by other mods.
		/// </summary>
		public virtual void PostAddRecipes() {
		}

		/// <summary>
		/// Close is called before Unload, and may be called at any time when mod unloading is imminent (such as when downloading an update, or recompiling)
		/// Use this to release any additional file handles, or stop streaming music. 
		/// Make sure to call `base.Close()` at the end
		/// May be called multiple times before Unload
		/// </summary>
		public virtual void Close() {
			fileHandle?.Dispose();

			if (File != null && File.IsOpen)
				throw new IOException($"TModFile has open handles: {File.path}");
		}

		/// <summary>
		/// Called whenever a net message / packet is received from a client (if this is a server) or the server (if this is a client). whoAmI is the ID of whomever sent the packet (equivalent to the Main.myPlayer of the sender), and reader is used to read the binary data of the packet.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="whoAmI">The player the message is from.</param>
		public virtual void HandlePacket(BinaryReader reader, int whoAmI) {
		}
	}
}
