using System.Linq;
using Terraria.ModLoader.IO;
using System;

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

		//TODO BUG: This default? uses LocalPlayer instead of playerX. This leads to if you swap characters, the new takes the accessories you had.
		public override TagCompound Save() {
			return new TagCompound {
				["size"] = moddedAccSlots.Count,
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


			if (moddedAccSlots.Count == newSize)
				return;

			//Create and apply unloadedSlot where there was one last time.
			ModContent.TryFind<ModAccessorySlot>("ModLoader/UnloadedAccessorySlot", out ModAccessorySlot unloadedSlot);
			unloadedSlot.xColumn = (int)((newSize - 1) / ModAccessorySlot.accessoryPerColumn) + 1;
			unloadedSlot.yRow = (newSize - 1) % ModAccessorySlot.accessoryPerColumn;
			unloadedSlot.slot = newSize - 1;
			moddedAccSlots.Add(unloadedSlot);
		}

		public override void Load(TagCompound tag) {
			ResizeAccesoryArrays(tag.Get<int>("size"));

			tag.GetList<TagCompound>("items").Select(ItemIO.Load).ToList().CopyTo(exAccessorySlot);
			tag.GetList<TagCompound>("dyes").Select(ItemIO.Load).ToList().CopyTo(exDyesAccessory);
			tag.GetList<bool>("visible").ToList().CopyTo(exHideAccessory);
		}

		//The below code won't work... because there's nothing implemented for it.
		/*
		public override void clientClone(ModPlayer clientClone) {
			var defaultInv = (DefaultPlayer) clientClone;
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
				NetHandler.SendSlot(toWho, Player.whoAmI, i, exHideAccessory[i]);
			}
		}

		public override void SendClientChanges(ModPlayer clientPlayer) {
			var clientInv = (DefaultPlayer) clientPlayer;
			for (int i = 0; i < exAccessorySlot.Length; i++)
				if (exAccessorySlot[i].IsNotTheSameAs(clientInv.exAccessorySlot[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, i, exAccessorySlot[i]);

			for (int i = 0; i < exDyesAccessory.Length; i++) {
				if (exDyesAccessory[i].IsNotTheSameAs(clientInv.exDyesAccessory[i]))
					NetHandler.SendSlot(-1, Player.whoAmI, i, exDyesAccessory[i]);

				if (exHideAccessory[i] != clientInv.exHideAccessory[i])
					NetHandler.SendSlot(-1, Player.whoAmI, i, exHideAccessory[i]);
			}
		}
		*/
	}
}
