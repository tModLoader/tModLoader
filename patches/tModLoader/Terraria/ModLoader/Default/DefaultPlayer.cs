using System.Linq;
using Terraria.ModLoader.IO;
using System;
using System.IO;

namespace Terraria.ModLoader.Default
{
	public class DefaultPlayer : ModPlayer
	{
		public override bool CloneNewInstances => false;

		public DefaultPlayer() {
			exAccessorySlot = new Item[2] { new Item(), new Item() };
			exDyesAccessory = new Item[1] { new Item() };
			exHideAccessory = new bool[1] { false };
			this.ResizeAccesoryArrays(ModPlayer.moddedAccSlots.Count);
		}
		
		public override TagCompound Save() {
			return new TagCompound {
				["size"] = ModPlayer.moddedAccSlots.Count,
				["items"] = exAccessorySlot.Select(ItemIO.Save).ToList(),
				["dyes"] = exDyesAccessory.Select(ItemIO.Save).ToList(),
				["visible"] = exHideAccessory.ToList()
			};
		}

		internal void ResizeAccesoryArrays(int newSize) {
			int oldLen = exDyesAccessory.Length;
			if (newSize <= oldLen)
				return;

			Array.Resize<Item>(ref exAccessorySlot, 2 * newSize);
			Array.Resize<Item>(ref exDyesAccessory, newSize);
			Array.Resize<bool>(ref exHideAccessory, newSize);

			for (int i = oldLen; i < newSize; i++) {
				exDyesAccessory[i] = new Item();
				exHideAccessory[i] = false;

				exAccessorySlot[i * 2] = new Item();
				exAccessorySlot[i * 2 + 1] = new Item();
			}

			if (newSize == moddedAccSlots.Count) {
				return;
			}
			for (int i = oldLen; i < newSize; i++) {
				moddedAccSlots.Add("unloaded");
			}
		}

		public override void Load(TagCompound tag) {
			ResizeAccesoryArrays(tag.Get<int>("size"));

			tag.GetList<TagCompound>("items").Select(ItemIO.Load).ToList().CopyTo(exAccessorySlot);
			tag.GetList<TagCompound>("dyes").Select(ItemIO.Load).ToList().CopyTo(exDyesAccessory);
			tag.GetList<bool>("visible").ToList().CopyTo(exHideAccessory);
		}

		// The following netcode is adapted from Chicken-Bone's UtilitySlots:
		public override void clientClone(ModPlayer clientClone) {
			var defaultInv = (DefaultPlayer)clientClone;
			for (int i = 0; i < exAccessorySlot.Length; i++)
				defaultInv.exAccessorySlot[i] = exAccessorySlot[i].Clone();
			for (int i = 0; i < exDyesAccessory.Length; i++) {
				defaultInv.exDyesAccessory[i] = exDyesAccessory[i].Clone();
				defaultInv.exHideAccessory[i] = exHideAccessory[i];
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			for (int i = 0; i < exAccessorySlot.Length; i++)
				NetHandler.SendSlot(toWho, Player.whoAmI, i, exAccessorySlot[i]);

			for (int i = 0; i < exDyesAccessory.Length; i++) {
				NetHandler.SendSlot(toWho, Player.whoAmI, i, exDyesAccessory[i]);
				NetHandler.SendVisualState(toWho, Player.whoAmI, i, exHideAccessory[i]);
			}
		}

		public override void SendClientChanges(ModPlayer clientPlayer) {
			var clientInv = (DefaultPlayer)clientPlayer;
			for (int i = 0; i < exAccessorySlot.Length; i++)
				if (exAccessorySlot[i].IsNotTheSameAs(clientInv.exAccessorySlot[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, i, exAccessorySlot[i]);

			for (int i = 0; i < exDyesAccessory.Length; i++) {
				if (exDyesAccessory[i].IsNotTheSameAs(clientInv.exDyesAccessory[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, -i - 1, exDyesAccessory[i]);

				if (exHideAccessory[i] != clientInv.exHideAccessory[i])
					NetHandler.SendVisualState(-1, Player.whoAmI, i, exHideAccessory[i]);
			}
		}

		internal class NetHandler
		{
			public const byte InventorySlot = 1;
			public const byte VisualState = 2;

			public static void SendSlot(int toWho, int plr, int slot, Item item) {
				var p = ModContent.GetInstance<ModLoaderMod>().GetPacket();

				p.Write(InventorySlot);

				if (Main.netMode == 2)
					p.Write((sbyte)plr);

				p.Write((sbyte)slot);

				ItemIO.Send(item, p, true);
				p.Send(toWho, plr);
			}

			private static void HandleSlot(BinaryReader r, int fromWho) {
				if (Main.netMode == 1)
					fromWho = r.ReadByte();

				ModPlayer dPlayer = Main.player[fromWho].GetModPlayer<DefaultPlayer>();

				sbyte slot = r.ReadSByte();
				var item = ItemIO.Receive(r, true);

				NetHandler.SetSlot(slot, item, dPlayer);

				if (Main.netMode == 2)
					SendSlot(-1, fromWho, slot, item);
			}

			public static void SendVisualState(int toWho, int plr, int slot, bool hideVisual) {
				var p = ModContent.GetInstance<ModLoaderMod>().GetPacket();

				p.Write(VisualState);

				if (Main.netMode == 2)
					p.Write((byte)plr);

				p.Write((sbyte)slot);

				p.Write(hideVisual);
				p.Send(toWho, plr);
			}

			private static void HandleVisualState(BinaryReader r, int fromWho) {
				if (Main.netMode == 1)
					fromWho = r.ReadByte();

				ModPlayer dPlayer = Main.player[fromWho].GetModPlayer<DefaultPlayer>();

				sbyte slot = r.ReadSByte();
				
				dPlayer.exHideAccessory[slot] = r.ReadBoolean();

				if (Main.netMode == 2)
					SendVisualState(-1, fromWho, slot, dPlayer.exHideAccessory[slot]);
			}

			public static void HandlePacket(BinaryReader r, int fromWho) {
				switch (r.ReadByte()) {
					case InventorySlot:
						HandleSlot(r, fromWho);
						break;
					case VisualState:
						HandleVisualState(r, fromWho);
						break;
				}
			}

			public static void SetSlot(sbyte slot, Item item, ModPlayer dPlayer) {
				if (slot < 0)
					dPlayer.exDyesAccessory[-(slot + 1)] = item;
				else
					dPlayer.exAccessorySlot[slot] = item;
			}
		}
	}
}
