using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	public static class ConfigManager
	{
		internal static readonly IDictionary<Mod, List<ModConfig>> Configs = new Dictionary<Mod, List<ModConfig>>();

		internal static void Add(ModConfig config)
		{
			List<ModConfig> configList;
			if (!Configs.TryGetValue(config.mod, out configList))
				Configs.Add(config.mod, configList = new List<ModConfig>());

			configList.Add(config);
		}

		internal static void Unload()
		{
			Configs.Clear();
		}
	}
}
