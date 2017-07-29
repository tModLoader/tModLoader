using System;
using System.ComponentModel;
using Terraria.ModLoader;

namespace ExampleMod
{
	// This file contains 2 ModConfigs. One is set to MultiplayerSyncMode.ServerDictates and the other MultiplayerSyncMode.UniquePerPlayer
	// ModConfigs contain Public Fields and Properties that represent the choices available to the user. 
	// Those Fields or Properties will be presented to users in the Config menu.
	// DONT use static members anywhere in this class, tModLoader maintains several instances of ModConfig classes which will not work well with static properties or fields.

	/// <summary>
	/// ExampleConfigServer has Server-wide effects. Things that happen on the server, on the world, or influence autoload go here
	/// MultiplayerSyncMode.ServerDictates ModConfigs are SHARED from the server to all clients connecting in MP.
	/// </summary>
	public class ExampleConfigServer : ModConfig
	{
		// You MUST specify a MultiplayerSyncMode.
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.ServerDictates;
			}
		}

		// We will use attributes to annotate our fields or properties so tModLoader can properly handle them.

		// First, we will learn about DefaultValue. You might assume "public bool BoolExample = true;" to work, 
		// but because tModLoader is overwriting with JSON, that value will be overwritten when the mod loads.
		// We must use the DefaultValue Attribute instead of setting the value normally:
		[DefaultValue(true)]    
		public bool BoolExample;

		// This is private. You'll notice that it doesn't show up in the config menu. Don't set something private.
		private bool PrivateFieldBoolExample;

		// You'll notice this next one is a Property instead of a field. That works too.
		// Here we see an attribute added by tModLoader: LabelAttribute. This one allows us to add a label so the user knows more about the setting they are changing.
		[Label("Disable Example Wings Item")]
		// ReloadRequired hints that if this value is changed, a reload is required for the mod to properly work. 
		// Here we use it so if we disable ExampleWings from being loaded, we can properly prevent autoload in ExampleWings.cs
		// Failure to properly use ReloadRequired will cause many, many problems including ID desync.
		[ReloadRequired]
		public bool DisableExampleWings { get; set; }

		// While ReloadRequired is sufficient for most, some may require more logic in deciding if a reload is required. Here is an incomplete example
		/*public override bool NeedsReload(ModConfig old)
		{
			bool defaultDecision = base.NeedsReload(old);
			bool otherLogic = IntExample > (old as ExampleConfigServer).IntExample; // This is just a random example. Your logic depends on your mod.
			return defaultDecision || otherLogic;
		}*/

		// Here I use PostAutoLoad to assign a static variable in ExampleMod to make it a little easier to access config values.
		// This reduces code from "mod.GetConfig<ExampleConfigServer>().DisableExampleWings" to "ExampleMod.exampleServerConfig.DisableExampleWings". It's just a style choice.
		// Note that PostAutoLoad happens before AutoLoad and Mod.Load.
		public override void PostAutoLoad()
		{
			ExampleMod.exampleServerConfig = this;
		}
	}

	/// <summary>
	/// This config operates on a per-client basis. 
	/// These parameters are local to this computer and are NOT synced from the server.
	/// </summary>
	public class ExampleConfigClient : ModConfig
	{
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.UniquePerPlayer;
			}
		}

		[Label("Show the coin rate UI")]
		public bool ShowCoinUI;

		[Label("Show mod origin in tooltip")]
		public bool ShowModOriginTooltip;

		public override void PostAutoLoad()
		{
			ExampleMod.exampleClientConfig = this;
			UI.ExampleUI.visible = ShowCoinUI;
		}

		public override void PostSave()
		{
			// Here we use the PostSave hook to initialize ExampleUI.visible with the new values.
			// I maintain both ExampleUI.visible and ShowCoinUI as separate values so ShowCoinUI can act as a default while ExampleUI.visible can change within a play session.
			UI.ExampleUI.visible = ShowCoinUI;
		}
	}
}
