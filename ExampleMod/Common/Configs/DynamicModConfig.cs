using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ExampleMod.Common.Configs;

public class DynamicModConfig : ModConfig
{
	// This config will not be automatically loaded. You need to call AddConfig at an appropriate time
	public static DynamicModConfig Instance;
	public override ConfigScope Mode => ConfigScope.ServerSide;

	public override bool Autoload(ref string name) {
		return false;
	}

	[DefaultValue(false)]
	public bool UltraRapidFire;
}