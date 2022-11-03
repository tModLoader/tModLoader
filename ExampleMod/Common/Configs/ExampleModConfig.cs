using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ExampleMod.Common.Configs
{
	public class ExampleModConfig : ModConfig
	{
		// ConfigScope.ClientSide should be used for client side, usually visual or audio tweaks.
		// ConfigScope.ServerSide should be used for basically everything else, including disabling items or changing NPC behaviours
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// The "$" character before a name means it should interpret the name as a translation key and use the loaded translation with the same key.
		// The things in brackets are known as "Attributes".
		[Header("$Mods.ExampleMod.Config.ItemHeader")] // Headers are like titles in a config. You only need to declare a header on the item it should appear over, not every item in the category.
		[Label("$Mods.ExampleMod.Config.ExampleWingsToggle.Label")] // A label is the text displayed next to the option. This should usually be a short description of what it does.
		[Tooltip("$Mods.ExampleMod.Config.ExampleWingsToggle.Tooltip")] // A tooltip is a description showed when you hover your mouse over the option. It can be used as a more in-depth explanation of the option.
		[DefaultValue(true)] // This sets the configs default value.
		[ReloadRequired] // Marking it with [ReloadRequired] makes tModLoader force a mod reload if the option is changed. It should be used for things like item toggles, which only take effect during mod loading
		public bool ExampleWingsToggle; // To see the implementation of this option, see ExampleWings.cs
	}
}
