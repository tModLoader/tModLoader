using System.Collections.Generic;

namespace Terraria.GameContent.UI.BigProgressBar
{
	public partial class BigProgressBarSystem
	{
		/// <summary>
		/// Assign this bar to an NPC if you explicitely don't want to have one displayed. Alternatively, if you don't have a boss head assigned, the bar won't show up either
		/// </summary>
		public static NeverValidProgressBar NeverValid => _neverValid;

		/// <summary>
		/// Keeps track of added bars by mods. _bossBarsByNpcNetId only tracks special vanilla bars
		/// </summary>
		internal Dictionary<int, IBigProgressBar> overrideBossBarsByNpcNetId = new Dictionary<int, IBigProgressBar>();

		//Main.BigBossProgressBar seems to never be re-initialized (neither does the static constructor get called again), so do custom unloading here
		internal void Unload() => overrideBossBarsByNpcNetId.Clear();

		/// <summary>
		/// Overrides or adds a boss bar for an NPC
		/// </summary>
		/// <param name="netID">The NPCs netID (most of the time the same as its type) to register to</param>
		/// <param name="bar">The IBigProgressBar</param>
		/// <returns>true if added, false if overridden</returns>
		internal bool AddBar(int netID, IBigProgressBar bar) {
			//This is callable anywhere at any point in the game
			if (!BarOverridden(netID) && !SpecialVanillaBarExists(netID)) {
				overrideBossBarsByNpcNetId.Add(netID, bar);
				return true;
			}
			else {
				overrideBossBarsByNpcNetId[netID] = bar;
				return false;
			}
		}

		/// <summary>
		/// Checks if an overridden bar exists for an NPCs netID
		/// </summary>
		/// <param name="netID">The netID to check for a bar with</param>
		/// <returns>true if an overridden bar exists</returns>
		public bool BarOverridden(int netID) => overrideBossBarsByNpcNetId.ContainsKey(netID);

		/// <summary>
		/// Checks if a special bar exists for a vanilla NPCs netID
		/// </summary>
		/// <param name="netID">The netID to check for a bar with</param>
		/// <returns>true if a special vanilla bar exists</returns>
		public bool SpecialVanillaBarExists(int netID) => _bossBarsByNpcNetId.ContainsKey(netID);

		/// <summary>
		/// Gets the bar associated with the NPCs netID. Prioritizes overridden/added bars
		/// </summary>
		/// <param name="netID">The netID associated with a bar to get</param>
		/// <param name="bar">When this method returns, contains the bar associated with the specified netID; otherwise, null for the bar: The resulting bar would turn into one of type CommonBossBigProgressBar</param>
		/// <returns>true if there is a bar associated with the netID; otherwise, false</returns>
		public bool TryGetBar(int netID, out IBigProgressBar bar) {
			bar = null;

			//Prioritize overridden bars
			if (overrideBossBarsByNpcNetId.TryGetValue(netID, out IBigProgressBar over))
				bar = over;
			else if (_bossBarsByNpcNetId.TryGetValue(netID, out IBigProgressBar value))
				bar = value;

			return bar != null;
		}
	}
}
