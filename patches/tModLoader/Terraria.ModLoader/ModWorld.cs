using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.World.Generation;

namespace Terraria.ModLoader
{
	public class ModWorld
	{
		public Mod mod
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			internal set;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}
		//public virtual void WorldGenPostInit()
		//{
		//}
		public virtual void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
		{
		}

		public virtual void PostWorldGen()
		{
		}
		//public virtual void WorldGenModifyHardmodeTaskList(List<GenPass> list)
		//{
		//}
	}
}
