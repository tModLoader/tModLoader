using ReLogic.Content.Sources;
using System;
using System.IO;
using System.Linq;
using Terraria.ModLoader.Assets;

namespace Terraria.ModLoader;

partial class Mod
{
	public virtual IContentSource CreateDefaultContentSource()
	{
		return new TModContentSource(File);
	}

	/// <summary>
	/// Override this method to run code after all content has been autoloaded. Here additional content can be manually loaded and Mod-wide tasks and setup can be done. For organization, it may be more suitable to split some things into various <see cref="ModType.Load"/> methods, such as in <see cref="ModSystem"/> classes, instead of doing everything here. <br/>
	/// Beware that mod content has not finished loading here, things like ModContent lookup tables or ID Sets are not fully populated. Use <see cref="PostSetupContent"/> for any logic that needs to act on all content being fully loaded.
	/// </summary>
	public virtual void Load()
	{
	}

	/// <summary>
	/// Allows you to load things in your mod after its content has been setup (arrays have been resized to fit the content, etc).
	/// </summary>
	public virtual void PostSetupContent()
	{
	}

	/// <summary>
	/// This is called whenever this mod is unloaded from the game. Use it to undo changes that you've made in Load that aren't automatically handled (for example, modifying the texture of a vanilla item). Mods are guaranteed to be unloaded in the reverse order they were loaded in.
	/// </summary>
	public virtual void Unload()
	{
	}

	/// <summary>
	/// The amount of extra buff slots this mod desires for Players. This value is checked after Mod.Load but before Mod.PostSetupContent. The actual number of buffs the player can use will be 44 plus the max value of all enabled mods. In-game use Player.MaxBuffs to check the maximum number of buffs.
	/// </summary>
	public virtual uint ExtraPlayerBuffSlots { get; }

	// TODO: Remove these recipe methods on August 1st, after stable release
	/// <summary>
	/// Override this method to add recipe groups to this mod. You must add recipe groups by calling the RecipeGroup.RegisterGroup method here. A recipe group is a set of items that can be used interchangeably in the same recipe.
	/// </summary>
	[Obsolete("Use ModSystem.AddRecipeGroups", true)]
	public virtual void AddRecipeGroups()
	{
	}

	// TODO: Remove these recipe methods on August 1st, after stable release
	/// <summary>
	/// Override this method to add recipes to the game. It is recommended that you do so through instances of Recipe, since it provides methods that simplify recipe creation.
	/// </summary>
	[Obsolete("Use ModSystem.AddRecipes", true)]
	public virtual void AddRecipes()
	{
	}

	// TODO: Remove these recipe methods on August 1st, after stable release
	/// <summary>
	/// This provides a hook into the mod-loading process immediately after recipes have been added. You can use this to edit recipes added by other mods.
	/// </summary>
	[Obsolete("Use ModSystem.PostAddRecipes", true)]
	public virtual void PostAddRecipes()
	{
	}

	/// <summary>
	/// Close is called before Unload, and may be called at any time when mod unloading is imminent (such as when downloading an update, or recompiling)
	/// Use this to release any additional file handles, or stop streaming music.
	/// Make sure to call `base.Close()` at the end
	/// May be called multiple times before Unload
	/// </summary>
	public virtual void Close()
	{
		MusicLoader.CloseModStreams(this);

		fileHandle?.Dispose();

		if (File != null && File.IsOpen)
			throw new IOException($"TModFile has open handles: {File.path}");
	}

	/// <summary>
	/// Called whenever a net message / packet pertaining to this mod is received from a client (if this is a server) or the server (if this is a client). whoAmI is the ID of whomever sent the packet (equivalent to the Main.myPlayer of the sender), and reader is used to read the binary data of the packet. <para/>
	/// Note that many packets are sent from a client to the server and then relayed to the remaining clients. The whoAmI when the packet arrives at the remaining clients will be the servers <see cref="Main.myPlayer"/>, not the original clients <see cref="Main.myPlayer"/>. For packets only sent from a client to the server, relying on <paramref name="whoAmI"/> to identify the clients player is fine, but for packets that are relayed, the clients player index will need to be part of the packet itself to correctly identify the client that sent the original packet. Use <c>packet.Write((byte) Main.myPlayer);</c> to write and <c>int player = reader.ReadByte();</c> to read. <para/>
	/// The <see cref="ModSystem.HijackGetData(ref byte, ref BinaryReader, int)"/> hook can be used to intercept any packet used by Terraria.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="whoAmI">The player the message is from. Only relevant for server code. For clients it will always be 256, the server. For the server it will be the whoAmI of the client.</param>
	public virtual void HandlePacket(BinaryReader reader, int whoAmI)
	{
	}
}
