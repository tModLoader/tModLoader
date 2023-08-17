namespace Terraria.GameContent.UI.BigProgressBar;

public partial class BigProgressBarSystem
{
	public NeverValidProgressBar NeverValid => _neverValid;

	//If any mods need to fetch special vanilla bars
	/// <summary>
	/// Gets the special IBigProgressBar associated with this vanilla NPCs netID (usually the type).
	/// <para> Reminder: If no special bar exists or NPC.BossBar is not assigned, any NPC with a boss head index will automatically display a common boss bar shared among all simple bosses.</para>
	/// </summary>
	/// <param name="netID">The NPC netID (usually the type)</param>
	/// <param name="bossBar">When this method returns, contains the IBigProgressBar associated with the specified NPC netID</param>
	/// <returns><see langword="true"/> if IBigProgressBar exists; otherwise, <see langword="false"/>.</returns>
	public bool TryGetSpecialVanillaBossBar(int netID, out IBigProgressBar bossBar)
		 => _bossBarsByNpcNetId.TryGetValue(netID, out bossBar);
}
