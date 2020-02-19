using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Tile Entities are Entities tightly coupled with tiles, allowing the possibility of tiles to exhibit cool behavior. TileEntity.Update is called in SP and on Server, not on Clients.
	/// </summary>
	/// <seealso cref="Terraria.DataStructures.TileEntity" />
	public abstract class ModTileEntity : TileEntity
	{
		public const int numVanilla = 3;
		private static int nextTileEntity = numVanilla;
		internal static readonly List<ModTileEntity> tileEntities = new List<ModTileEntity>();
		// TODO: public bool netUpdate;

		/// <summary>
		/// The mod that added this ModTileEntity.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The internal name of this ModTileEntity.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The numeric type used to identify this kind of tile entity.
		/// </summary>
		public int Type {
			get;
			internal set;
		}

		internal static int ReserveTileEntityID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding tile entities breaks vanilla client compatibility");

			int reserveID = nextTileEntity;
			nextTileEntity++;
			if (reserveID > Byte.MaxValue) {
				throw new Exception("Too many tile entities have been added");
			}
			return reserveID;
		}

		/// <summary>
		/// Gets the base ModTileEntity object with the given type.
		/// </summary>
		public static ModTileEntity GetTileEntity(int type) {
			return type >= numVanilla && type < nextTileEntity ? tileEntities[type - numVanilla] : null;
		}

		internal static void Unload() {
			nextTileEntity = numVanilla;
			tileEntities.Clear();
		}

		/// <summary>
		/// Returns the number of modded tile entities that exist in the world currently being played.
		/// </summary>
		public static int CountInWorld() {
			int count = 0;
			foreach (KeyValuePair<int, TileEntity> pair in ByID) {
				if (pair.Value.type >= numVanilla) {
					count++;
				}
			}
			return count;
		}

		/// <summary>
		/// You should never use this. It is only included here for completion's sake.
		/// </summary>
		public static void Initialize() {
			_UpdateStart += UpdateStartInternal;
			_UpdateEnd += UpdateEndInternal;
			_NetPlaceEntity += NetPlaceEntity;
		}

		private static void UpdateStartInternal() {
			foreach (ModTileEntity tileEntity in tileEntities) {
				tileEntity.PreGlobalUpdate();
			}
		}

		private static void UpdateEndInternal() {
			foreach (ModTileEntity tileEntity in tileEntities) {
				tileEntity.PostGlobalUpdate();
			}
		}

		/// <summary>
		/// You should never use this. It is only included here for completion's sake.
		/// </summary>
		public static void NetPlaceEntity(int i, int j, int type) {
			ModTileEntity tileEntity = GetTileEntity(type);
			if (tileEntity == null) {
				return;
			}
			if (!tileEntity.ValidTile(i, j)) {
				return;
			}
			int id = tileEntity.Place(i, j);
			ModTileEntity newEntity = (ModTileEntity)ByID[id];
			newEntity.OnNetPlace();
			NetMessage.SendData(86, -1, -1, null, id, i, j, 0f, 0, 0, 0);
		}

		/// <summary>
		/// Returns a new ModTileEntity with the same class, mod, name, and type as the ModTileEntity with the given type. It is very rare that you should have to use this.
		/// </summary>
		public static ModTileEntity ConstructFromType(int type) {
			ModTileEntity tileEntity = GetTileEntity(type);
			if (tileEntity == null) {
				return null;
			}
			return ConstructFromBase(tileEntity);
		}

		/// <summary>
		/// Returns a new ModTileEntity with the same class, mod, name, and type as the parameter. It is very rare that you should have to use this.
		/// </summary>
		public static ModTileEntity ConstructFromBase(ModTileEntity tileEntity) {
			ModTileEntity newEntity = (ModTileEntity)Activator.CreateInstance(tileEntity.GetType());
			newEntity.mod = tileEntity.mod;
			newEntity.Name = tileEntity.Name;
			newEntity.Type = tileEntity.Type;
			return newEntity;
		}

		/// <summary>
		/// A helper method that places this kind of tile entity in the given coordinates for you.
		/// </summary>
		public int Place(int i, int j) {
			ModTileEntity newEntity = ConstructFromBase(this);
			newEntity.Position = new Point16(i, j);
			newEntity.ID = AssignNewID();
			newEntity.type = (byte)Type;
			ByID[newEntity.ID] = newEntity;
			ByPosition[newEntity.Position] = newEntity;
			return newEntity.ID;
		}

		/// <summary>
		/// A helper method that removes this kind of tile entity from the given coordinates for you.
		/// </summary>
		public void Kill(int i, int j) {
			Point16 pos = new Point16(i, j);
			if (ByPosition.ContainsKey(pos)) {
				TileEntity tileEntity = ByPosition[pos];
				if (tileEntity.type == Type) {
					((ModTileEntity)tileEntity).OnKill();
					ByID.Remove(tileEntity.ID);
					ByPosition.Remove(pos);
				}
			}
		}

		/// <summary>
		/// Returns the entity ID of this kind of tile entity at the given coordinates for you.
		/// </summary>
		public int Find(int i, int j) {
			Point16 pos = new Point16(i, j);
			if (ByPosition.ContainsKey(pos)) {
				TileEntity tileEntity = ByPosition[pos];
				if (tileEntity.type == Type) {
					return tileEntity.ID;
				}
			}
			return -1;
		}

		/// <summary>
		/// Don't use this. It is included only for completion's sake.
		/// </summary>
		public sealed override void WriteExtraData(BinaryWriter writer, bool networkSend) {
			NetSend(writer, networkSend);
		}

		/// <summary>
		/// Don't use this. It is included only for completion's sake.
		/// </summary>
		public sealed override void ReadExtraData(BinaryReader reader, bool networkSend) {
			NetReceive(reader, networkSend);
		}

		/// <summary>
		/// Allows you to automatically load a tile entity instead of using Mod.AddTileEntity. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this method to either force or stop an autoload, or change the default display name.
		/// </summary>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Allows you to save custom data for this tile entity.
		/// </summary>
		/// <returns></returns>
		public virtual TagCompound Save() {
			return null;
		}

		/// <summary>
		/// Allows you to load the custom data you have saved for this tile entity.
		/// </summary>
		public virtual void Load(TagCompound tag) {
		}

		/// <summary>
		/// Allows you to send custom data for this tile entity between client and server. This is called on the server while sending tile data (!lightSend) and when a MessageID.TileEntitySharing message is sent (lightSend)
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="lightSend">If true, send only data that can change. Otherwise, send the full information.</param>
		public virtual void NetSend(BinaryWriter writer, bool lightSend) {
		}

		/// <summary>
		/// Receives the data sent in the NetSend hook. Called on MP Client when receiving tile data (!lightReceive) and when a MessageID.TileEntitySharing message is sent (lightReceive)
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="lightReceive">If true, read only data that can change. Otherwise, read the full information.</param>
		public virtual void NetReceive(BinaryReader reader, bool lightReceive) {
		}

		/// <summary>
		/// Whether or not this tile entity is allowed to survive at the given coordinates. You should check whether the tile is active, as well as the tile's type and frame.
		/// </summary>
		public abstract bool ValidTile(int i, int j);

		/// <summary>
		/// This method does not get called by tModLoader, and is only included for you convenience so you do not have to cast the result of Mod.GetTileEntity.
		/// </summary>
		public virtual int Hook_AfterPlacement(int i, int j, int type, int style, int direction) {
			return -1;
		}

		/// <summary>
		/// Code that should be run when this tile entity is placed by means of server-syncing. Called on Server only.
		/// </summary>
		public virtual void OnNetPlace() {
		}

		/// <summary>
		/// Code that should be run before all tile entities in the world update.
		/// </summary>
		public virtual void PreGlobalUpdate() {
		}

		/// <summary>
		/// Code that should be run after all tile entities in the world update.
		/// </summary>
		public virtual void PostGlobalUpdate() {
		}

		/// <summary>
		/// This method only gets called in the Kill method. If you plan to use that, you can put code here to make things happen when it is called.
		/// </summary>
		public virtual void OnKill() {
		}
	}
}
