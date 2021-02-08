using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class DefaultPlayer : ModPlayer
	{
		//TODO: Should save all important datas; namely everthing to do with exAccessSlot, and the like 
		public override TagCompound Save() {
			return new TagCompound {
				["size"] = moddedAccSlots.Count,
				["items"] = exAccessorySlot.Select(ItemIO.Save).ToList(),
				["dyes"] = exDyesAccessory.Select(ItemIO.Save).ToList(),
				["visible"] = exHideAccessory.ToList()
			};
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
