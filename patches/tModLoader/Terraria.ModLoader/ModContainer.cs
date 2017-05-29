using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	// run send/receive automatically
	public abstract class ModContainer : ModTileEntity
	{
		public static readonly Dictionary<Point16, ModContainer> ContainersByPosition = new Dictionary<Point16, ModContainer>();
		public static readonly Dictionary<int, ModContainer> ContainersByID = new Dictionary<int, ModContainer>();

		public Dictionary<int, Item> inventory = new Dictionary<int, Item>();

		/// <summary>
		/// Determines whether inventory should be dropped on kill (Defaults to True)
		/// </summary>
		public bool dropItemsOnKill = true;
		
		public List<TagCompound> SaveStorage()
		{
			List<TagCompound> storage = new List<TagCompound>();

			if (inventory.Count > 0)
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					storage.Add(new TagCompound
					{
						["slot"] = i,
						["item"] = ItemIO.Save(inventory[i])
					});
				}
			}
			return storage;
		}

		public void LoadStorage(IList<TagCompound> tag)
		{
			inventory.Clear();
			for (int i = 0; i < tag.Count; i++) inventory.Add(tag[i].GetInt("slot"), tag[i].Get<Item>("item"));
		}

		public void SendStorage(BinaryWriter writer)
		{
			if (inventory.Count > 0)
			{
				writer.Write(inventory.Count);
				for (int i = 0; i < inventory.Count; i++) ItemIO.Send(inventory[i], writer, true, true);
			}
		}

		public void ReceiveStorage(BinaryReader reader)
		{
			inventory.Clear();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++) inventory[i] = ItemIO.Receive(reader, true, true);
		}

		/// <summary>
		/// Adds a slot to the inventory
		/// </summary>
		/// <param name="id">ID of the slot</param>
		public void AddSlot(int id)
		{
			if (!inventory.ContainsKey(id)) inventory.Add(id, new Item());
		}

		public virtual void Setup()
		{
		}
	}
}