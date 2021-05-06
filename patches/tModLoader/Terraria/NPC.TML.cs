using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class NPC : IEntityWithGlobals<GlobalNPC>
	{
		public ModNPC ModNPC { get; internal set; }

		internal Instanced<GlobalNPC>[] globalNPCs = Array.Empty<Instanced<GlobalNPC>>();

		public RefReadOnlyArray<Instanced<GlobalNPC>> Globals => new RefReadOnlyArray<Instanced<GlobalNPC>>(globalNPCs);

		/// <summary>
		/// Assign a special boss bar, vanilla or modded. Not used by vanilla.
		/// <para>To assign a modded boss bar, use NPC.BossBar = ModContent.GetInstance<ExampleBossBar>(); where ExampleBossBar is a ModBossBar</para>
		/// <para>To assign a vanilla boss bar for whatever reason, fetch it first through the NPC type using Main.BigBossProgressBar.TryGetSpecialVanillaBossBar</para>
		/// </summary>
		public IBigProgressBar BossBar { get; set; }

		// Get

		/// <summary> Gets the instance of the specified GlobalNPC type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetGlobalNPC<T>(bool exactType = true) where T : GlobalNPC
			=> GlobalType.GetGlobal<NPC, GlobalNPC, T>(globalNPCs, exactType);

		/// <summary> Gets the local instance of the type of the specified GlobalNPC instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetGlobalNPC<T>(T baseInstance) where T : GlobalNPC
			=> GlobalType.GetGlobal<NPC, GlobalNPC, T>(globalNPCs, baseInstance);

		/// <summary> Gets the instance of the specified GlobalNPC type. </summary>
		public bool TryGetGlobalNPC<T>(out T result, bool exactType = true) where T : GlobalNPC
			=> GlobalType.TryGetGlobal<GlobalNPC, T>(globalNPCs, exactType, out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified GlobalNPC instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetGlobalNPC<T>(T baseInstance, out T result) where T : GlobalNPC
			=> GlobalType.TryGetGlobal<GlobalNPC, T>(globalNPCs, baseInstance, out result);
	}
}
