using Terraria.Localization;
using Terraria.ModLoader.Config;

public class ModConfigTest : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;

	public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
	{
		return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
	}
}