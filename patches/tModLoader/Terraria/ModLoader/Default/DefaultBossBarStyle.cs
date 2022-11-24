namespace Terraria.ModLoader.Default;

/// <summary>
/// This is the default boss bar style - the way vanilla draws boss bars.
/// </summary>
[Autoload(false)]
internal class DefaultBossBarStyle : ModBossBarStyle
{
	public override string DisplayName => "Vanilla";
}
