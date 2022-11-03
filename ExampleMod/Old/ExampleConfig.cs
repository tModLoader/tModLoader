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

		// We will use attributes to annotate our fields or properties so tModLoader can properly handle them.

		// First, we will learn about DefaultValue. You might assume "public bool BoolExample = true;" to work, 
		// but because tModLoader is overwriting with JSON, that value will be overwritten when the mod loads.
		// We must use the DefaultValue attribute instead of setting the value normally:
		[DefaultValue(true)]
		public bool UselessBoolExample;

		// This is private. You'll notice that it doesn't show up in the config menu. Don't set something private.
#pragma warning disable CS0169 // Unused field
		private bool PrivateFieldBoolExample;
#pragma warning restore CS0169

		// This is ignored, it also shouldn't show up in the config menu despite being public.
		[JsonIgnore]
		public bool IgnoreExample;

		// You'll notice this next one is a Property instead of a field. That works too.
		// Here we see an attribute added by tModLoader: LabelAttribute. This one allows us to add a label so the user knows more about the setting they are changing. Without a label, the name of the field or property is displayed.
		[Label("Disable Example Wings Item")]
		// Similar to Label, this sets the tooltip. Tooltips are useful for slightly longer and more detailed explanations of config options.
		[Tooltip("Prevents Loading the ExampleWings item. Requires a Reload")]
		// ReloadRequired hints that if this value is changed, a reload is required for the mod to properly work. 
		// Here we use it so if we disable ExampleWings from being loaded, we can properly prevent autoload in ExampleWings.cs
		// Failure to properly use ReloadRequired will cause many, many problems including ID desync.
		[ReloadRequired]
		public bool DisableExampleWings { get; set; }

		[Label("Disable Volcanoes")]
		// Our game logic can handle toggling this setting in-game, so you'll notice we do NOT decorate this property with ReloadRequired
		public bool DisableVolcanoes { get; set; }

		// Watch in action: https://gfycat.com/SickTerribleHoatzin
		[Label("Example Person free gift list")]
		[Tooltip("Each player can claim one free item from this list from Example Person\nSell the item back to Example Person to take a new item")]
		public List<ItemDefinition> ExamplePersonFreeGiftList { get; set; } = new List<ItemDefinition>();

		// AcceptClientChanges is called on the server when a Client player attempts to change ServerSide settings in-game. By default, client changes are accepted. (As long as they don't necessitate a Reload)
		// With more effort, a mod could implement more control over changing mod settings.
		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) {
			if (Main.player[whoAmI].name == "jopojelly") {
				message = "Sorry, players named jopojelly aren't allowed to change settings.";
				return false;
			}
			return true;
		}

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
