using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace ExampleMod
{
	// This file contains 2 real ModConfigs (and also a bunch of fake ModConfigs showcasing various ideas). One is set to ConfigScope.ServerSide and the other ConfigScope.ClientSide
	// ModConfigs contain Public Fields and Properties that represent the choices available to the user. 
	// Those Fields or Properties will be presented to users in the Config menu.
	// DONT use static members anywhere in this class (except for an automatically assigned field named Instance with the same Type as the ModConfig class, if you'd rather write "MyConfigClass.Instance" instead of "ModContent.GetInstance<MyConfigClass>()"), tModLoader maintains several instances of ModConfig classes which will not work well with static properties or fields.

	/// <summary>
	/// ExampleConfigServer has Server-wide effects. Things that happen on the server, on the world, or influence autoload go here
	/// ConfigScope.ServerSide ModConfigs are SHARED from the server to all clients connecting in MP.
	/// </summary>
	public class ExampleConfigServer : ModConfig
	{
		// You MUST specify a ConfigScope.
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Label("Disable Volcanoes")]
		// Our game logic can handle toggling this setting in-game, so you'll notice we do NOT decorate this property with ReloadRequired
		public bool DisableVolcanoes { get; set; }

		// Watch in action: https://gfycat.com/SickTerribleHoatzin
		[Label("Example Person free gift list")]
		[Tooltip("Each player can claim one free item from this list from Example Person\nSell the item back to Example Person to take a new item")]
		public List<ItemDefinition> ExamplePersonFreeGiftList { get; set; } = new List<ItemDefinition>();

		// While ReloadRequired is sufficient for most, some may require more logic in deciding if a reload is required. Here is an incomplete example
		/*public override bool NeedsReload(ModConfig pendingConfig)
		{
			bool defaultDecision = base.NeedsReload(pendingConfig);
			bool otherLogic = IntExample > (pendingConfig as ExampleConfigServer).IntExample; // This is just a random example. Your logic depends on your mod.
			return defaultDecision || otherLogic; // reload needed if either condition is met.
		}*/
	}

	/// <summary>
	/// This config operates on a per-client basis. 
	/// These parameters are local to this computer and are NOT synced from the server.
	/// </summary>
	public class ExampleConfigClient : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Show the coin rate UI")]
		public bool ShowCoinUI;

		[Label("Show mod origin in tooltip")]
		public bool ShowModOriginTooltip;

		public override void OnChanged() {
			// Here we use the OnChanged hook to initialize ExampleUI.visible with the new values.
			// We maintain both ExampleUI.visible and ShowCoinUI as separate values so ShowCoinUI can act as a default while ExampleUI.visible can change within a play session.
			UI.ExampleUI.Visible = ShowCoinUI;
		}
	}
}
