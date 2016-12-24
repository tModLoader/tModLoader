using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public abstract class ModTileEntity : TileEntity
	{
		public const int numVanilla = 3;
		private static int nextTileEntity = numVanilla;
		internal static readonly List<ModTileEntity> tileEntities = new List<ModTileEntity>();

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

		public int Type
		{
			get;
			internal set;
		}

		internal static int ReserveTileEntityID()
		{
			if (ModNet.AllowVanillaClients) throw new Exception("Adding tile entities breaks vanilla client compatiblity");

			int reserveID = nextTileEntity;
			nextTileEntity++;
			if (reserveID > Byte.MaxValue)
			{
				throw new Exception("Too many tile entities have been added");
			}
			return reserveID;
		}

		public static ModTileEntity GetTileEntity(int type)
		{
			return type >= numVanilla && type < nextTileEntity ? tileEntities[type - numVanilla] : null;
		}

		internal static void Unload()
		{
			nextTileEntity = numVanilla;
			tileEntities.Clear();
		}

		public static int CountInWorld()
		{
			int count = 0;
			foreach (KeyValuePair<int, TileEntity> pair in ByID)
			{
				if (pair.Value.type >= numVanilla)
				{
					count++;
				}
			}
			return count;
		}

		public static void Initialize()
		{
			_UpdateStart += UpdateStartInternal;
			_UpdateEnd += UpdateEndInternal;
			_NetPlaceEntity += NetPlaceEntity;
		}

		private static void UpdateStartInternal()
		{
			foreach (ModTileEntity tileEntity in tileEntities)
			{
				tileEntity.PreGlobalUpdate();
			}
		}

		private static void UpdateEndInternal()
		{
			foreach (ModTileEntity tileEntity in tileEntities)
			{
				tileEntity.PostGlobalUpdate();
			}
		}

		public static void NetPlaceEntity(int i, int j, int type)
		{
			ModTileEntity tileEntity = GetTileEntity(type);
			if (tileEntity == null)
			{
				return;
			}
			if (!tileEntity.ValidTile(i, j))
			{
				return;
			}
			int id = tileEntity.Place(i, j);
			ModTileEntity newEntity = (ModTileEntity)ByID[id];
			newEntity.OnNetPlace();
			NetMessage.SendData(86, -1, -1, "", id, i, j, 0f, 0, 0, 0);
		}

		public static ModTileEntity ConstructFromType(int type)
		{
			ModTileEntity tileEntity = GetTileEntity(type);
			if (tileEntity == null)
			{
				return null;
			}
			return ConstructFromBase(tileEntity);
		}

		public static ModTileEntity ConstructFromBase(ModTileEntity tileEntity)
		{
			ModTileEntity newEntity = (ModTileEntity)Activator.CreateInstance(tileEntity.GetType());
			newEntity.mod = tileEntity.mod;
			newEntity.Name = tileEntity.Name;
			newEntity.Type = tileEntity.Type;
			return newEntity;
		}

		public int Place(int i, int j)
		{
			ModTileEntity newEntity = ConstructFromBase(this);
			newEntity.Position = new Point16(i, j);
			newEntity.ID = AssignNewID();
			newEntity.type = (byte)Type;
			ByID[newEntity.ID] = newEntity;
			ByPosition[newEntity.Position] = newEntity;
			return newEntity.ID;
		}

		public void Kill(int i, int j)
		{
			Point16 pos = new Point16(i, j);
			if (ByPosition.ContainsKey(pos))
			{
				TileEntity tileEntity = ByPosition[pos];
				if (tileEntity.type == Type)
				{
					((ModTileEntity)tileEntity).OnKill();
					ByID.Remove(tileEntity.ID);
					ByPosition.Remove(pos);
				}
			}
		}

		public int Find(int i, int j)
		{
			Point16 pos = new Point16(i, j);
			if (ByPosition.ContainsKey(pos))
			{
				TileEntity tileEntity = ByPosition[pos];
				if (tileEntity.type == Type)
				{
					return tileEntity.ID;
				}
			}
			return -1;
		}

		public override sealed void WriteExtraData(BinaryWriter writer, bool networkSend)
		{
			if (networkSend)
			{
				NetSend(writer);
			}
		}

		public override sealed void ReadExtraData(BinaryReader reader, bool networkSend)
		{
			if (networkSend)
			{
				NetReceive(reader);
			}
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual TagCompound Save()
		{
			return null;
		}

		public virtual void Load(TagCompound tag)
		{
		}

		public virtual void NetSend(BinaryWriter writer)
		{
		}

		public virtual void NetReceive(BinaryReader reader)
		{
		}

		public abstract bool ValidTile(int i, int j);

		public virtual int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			return -1;
		}

		public virtual void OnNetPlace()
		{
		}

		public virtual void PreGlobalUpdate()
		{
		}

		public virtual void PostGlobalUpdate()
		{
		}

		public virtual void OnKill()
		{
		}
	}
}
