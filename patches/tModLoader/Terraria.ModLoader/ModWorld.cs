using System;
using System.Collections.Generic;
using System.IO;
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

		public virtual void SaveCustomData(BinaryWriter writer)
		{
		}

		public virtual void LoadCustomData(BinaryReader reader)
		{
		}

		public virtual void SendCustomData(BinaryWriter writer)
		{
		}

		public virtual void ReceiveCustomData(BinaryReader reader)
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
	}
}
