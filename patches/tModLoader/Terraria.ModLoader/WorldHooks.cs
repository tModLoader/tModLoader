using System;
using System.IO;
using System.Collections.Generic;
using Terraria.World.Generation;

namespace Terraria.ModLoader
{
	public static class WorldHooks
	{
		internal static readonly IList<ModWorld> worlds = new List<ModWorld>();

		internal static void Add(ModWorld modWorld)
		{
			worlds.Add(modWorld);
		}

		internal static void Unload()
		{
			worlds.Clear();
		}

		internal static void SetupWorld()
		{
			foreach (ModWorld world in worlds)
			{
				world.Initialize();
			}
		}

		public static void SendCustomData(BinaryWriter writer)
		{
			ushort count = 0;
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter customWriter = new BinaryWriter(stream))
				{
					foreach (var modWorld in worlds)
					{
						if (SendCustomData(modWorld, customWriter))
						{
							count++;
						}
					}
					customWriter.Flush();
					data = stream.ToArray();
				}
			}
			writer.Write(count);
			writer.Write(data);
		}

		private static bool SendCustomData(ModWorld modWorld, BinaryWriter writer)
		{
			byte[] data;
			modWorld.PreSaveCustomData();
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter customWriter = new BinaryWriter(stream))
				{
					modWorld.SendCustomData(customWriter);
					customWriter.Flush();
					data = stream.ToArray();
				}
			}
			if (data.Length > 0)
			{
				writer.Write(modWorld.mod.Name);
				writer.Write(modWorld.Name);
				writer.Write((ushort)data.Length);
				writer.Write(data);
				return true;
			}
			return false;
		}

		public static void ReceiveCustomData(BinaryReader reader)
		{
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++)
			{
				string modName = reader.ReadString();
				string name = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadUInt16());
				Mod mod = ModLoader.GetMod(modName);
				ModWorld modWorld = mod == null ? null : mod.GetModWorld(name);
				if (modWorld != null)
				{
					using (MemoryStream stream = new MemoryStream(data))
					{
						using (BinaryReader customReader = new BinaryReader(stream))
						{
							try
							{
								modWorld.ReceiveCustomData(customReader);
							}
							catch
							{
							}
						}
					}
				}
			}
		}

		public static void PreWorldGen()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PreWorldGen();
			}
		}

		public static void ModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ModifyWorldGenTasks(passes, ref totalWeight);
			}
		}

		public static void PostWorldGen()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PostWorldGen();
			}
		}

		public static void ResetNearbyTileEffects()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ResetNearbyTileEffects();
			}
		}

		public static void PreUpdate()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PreUpdate();
			}
		}

		public static void PostUpdate()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PostUpdate();
			}
		}

		public static void TileCountsAvailable(int[] tileCounts)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.TileCountsAvailable(tileCounts);
			}
		}

		public static void ChooseWaterStyle(ref int style)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ChooseWaterStyle(ref style);
			}
		}
	}
}