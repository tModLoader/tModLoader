namespace Terraria.GameContent.UI.BigProgressBar
{
	public partial class BigProgressBarSystem
	{
		//If any mods need to fetch special vanilla bars
		public bool TryGetSpecialVanillaBossBar(int netID, out IBigProgressBar bossBar) =>
			_bossBarsByNpcNetId.TryGetValue(netID, out bossBar);
	}
}
