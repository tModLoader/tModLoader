using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
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

		public virtual void Initialize()
		{
		}

		public virtual TagCompound Save()
		{
			return null;
		}

		public virtual void Load(TagCompound tag)
		{
		}

		public virtual void LoadLegacy(BinaryReader reader)
		{
		}

		public virtual void NetSend(BinaryWriter writer)
		{
		}

		public virtual void NetReceive(BinaryReader reader)
		{
		}

		public virtual void PreWorldGen()
		{
		}

		public virtual void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
		{
		}

		public virtual void PostWorldGen()
		{
		}

		public virtual void ResetNearbyTileEffects()
		{
		}

		public virtual void PreUpdate()
		{
		}

		public virtual void PostUpdate()
		{
		}

		public virtual void TileCountsAvailable(int[] tileCounts)
		{
		}

		public virtual void ChooseWaterStyle(ref int style)
		{
		}
		//TODO - New Hook - WorldGenModifyHardmodeTaskList
		//Called after queuing world generation tasks when switching the world to hardmode, can be used to modify which tasks should be done and/or add custom tasks.
		//public virtual void WorldGenModifyHardmodeTaskList(List<GenPass> list)
		//{
		//}

		public virtual void PostDrawTiles()
		{
		}
	}
}
