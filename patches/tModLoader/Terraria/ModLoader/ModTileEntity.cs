using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Tile Entities are Entities tightly coupled with tiles, allowing the possibility of tiles to exhibit cool behavior. TileEntity.Update is called in SP and on Server, not on Clients.
	/// </summary>
	/// <seealso cref="Terraria.DataStructures.TileEntity" />
	public abstract class ModTileEntity : TileEntity, IModType
	{
		public static readonly int NumVanilla = Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(TileEntity)) && !typeof(ModTileEntity).IsAssignableFrom(t))
			.Count();

		// TODO: public bool netUpdate;

		/// <summary>
		/// The mod that added this ModTileEntity.
		/// </summary>
		public Mod Mod {get;internal set;}

		/// <summary>
		/// The internal name of this ModTileEntity.
		/// </summary>
		public string Name => GetType().Name;

		public string FullName => $"{Mod.Name}/{Name}";

		/// <summary>
		/// The numeric type used to identify this kind of tile entity.
		/// </summary>
		public int Type {get;internal set;}

		public ModTileEntity() { }

		/// <summary>
		/// Returns the number of modded tile entities that exist in the world currently being played.
		/// </summary>
		public static int CountInWorld() {
			int count = 0;
			foreach (KeyValuePair<int, TileEntity> pair in ByID) {
				if (pair.Value.type >= NumVanilla) {
					count++;
				}
			}
			return count;
		}

		internal static void Initialize() {
			_UpdateStart += UpdateStartInternal;
			_UpdateEnd += UpdateEndInternal;
		}

		private static void UpdateStartInternal() {
			foreach (ModTileEntity tileEntity in manager.EnumerateEntities().OfType<ModTileEntity>()) {
				tileEntity.PreGlobalUpdate();
			}
		}

		private static void UpdateEndInternal() {
			foreach (ModTileEntity tileEntity in manager.EnumerateEntities().OfType<ModTileEntity>()) {
				tileEntity.PostGlobalUpdate();
			}
		}

		/// <summary>
		/// You should never use this. It is only included here for completion's sake.
		/// </summary>
		public override void NetPlaceEntityAttempt(int i, int j) {
			if (!manager.TryGetTileEntity(type, out ModTileEntity modTileEntity) || !modTileEntity.ValidTile(i, j)) {
				return;
			}

			int id = modTileEntity.Place(i, j);
			ModTileEntity newEntity = (ModTileEntity)ByID[id];
			newEntity.OnNetPlace();
			NetMessage.SendData(86, -1, -1, null, id, i, j, 0f, 0, 0, 0);
		}

		/// <summary>
		/// Returns a new ModTileEntity with the same class, mod, name, and type as the ModTileEntity with the given type. It is very rare that you should have to use this.
		/// </summary>
		public static ModTileEntity ConstructFromType(int type) {
			if (!manager.TryGetTileEntity(type, out ModTileEntity modTileEntity)) {
				return null;
			}

			return ConstructFromBase(modTileEntity);
		}

		/// <summary>
		/// Returns a new ModTileEntity with the same class, mod, name, and type as the parameter. It is very rare that you should have to use this.
		/// </summary>
		public static ModTileEntity ConstructFromBase(ModTileEntity tileEntity) {
			ModTileEntity newEntity = (ModTileEntity)Activator.CreateInstance(tileEntity.GetType());
			newEntity.Mod = tileEntity.Mod;
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

		// The base implementations of these methods call Read/WriteExtraData, and those in ModTileEntity's case now call NetSend/Receive.
		public override void NetSend(BinaryWriter writer, bool lightSend) { }
		public override void NetReceive(BinaryReader reader, bool lightReceive) { }

		public sealed override TileEntity GenerateInstance() => ConstructFromBase(this);
		public sealed override void RegisterTileEntityID(int assignedID) => Type = assignedID;

		public virtual void Load(Mod mod) {
			Mod = mod;

			if (!Mod.loading)
				throw new Exception("AddTileEntity can only be called from Mod.Load or Mod.Autoload");

			manager.Register(this);
			ModTypeLookup<ModTileEntity>.Register(this);
		}

		public virtual void Unload(){}

		/// <summary>
		/// Whether or not this tile entity is allowed to survive at the given coordinates. You should check whether the tile is active, as well as the tile's type and frame.
		/// </summary>
		public abstract bool ValidTile(int i, int j);

		/// <summary>
		/// This method does not get called by tModLoader, and is only included for you convenience so you do not have to cast the result of Mod.GetTileEntity.
		/// </summary>
		public virtual int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
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
