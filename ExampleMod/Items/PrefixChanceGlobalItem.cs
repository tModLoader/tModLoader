using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace ExampleMod.Items
{
	/// <summary>
	/// This class demonstrates how to manipulate the chances of prefixes given to items.
	/// For other prefix related hooks and their usage, see also:
	/// <seealso cref="Accessories.ManaHeart"/>
	/// <seealso cref="Weapons.ExampleYoyo"/>
	/// </summary>
	public class PrefixChanceGlobalItem : GlobalItem
	{
		public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand) {
			// pre: The prefix being applied to the item, or the roll mode
			// -1 is when an item is naturally generated in a chest, crafted, purchased from an NPC, looted from a grab bag (excluding presents), or dropped by a slain enemy
			// -2 is when an item is rolled in the tinkerer
			// -3 determines if an item can be placed in the tinkerer slot

			// To prevent putting an item in the tinkerer slot, return false when pre is -3
			if (pre == -3 && item.type == ItemID.LaserRifle) {
				// This will make the Laser Rifle not be reforgeable at all (useful if you want your item to preserve its custom name color)
				return false;
			}

			// To make an item reset its prefix when reforging
			if (pre == -2) {
				if (Main.LocalPlayer.HasBuff(BuffID.Cursed)) {
					// If the player is cursed, make it remove the prefix
					return false;
				}
			}

			// To prevent rolling of a prefix on spawn, return false when pre is -1
			if (pre == -1) {
				if (item.melee && item.modItem?.mod == mod) {
					// All melee weapons from ExampleMod won't have a prefix when they are crafted, bought, taken from a generated chest, opened, or dropped by an enemy
					return false;
				}
			}

			// For the following code, this is useful to know (from the terraria wiki):
			// Nearly all weapons and accessories have a 75% chance of receiving a random modifier upon the item's creation
			// (naturally generated in a chest, crafted, purchased from an NPC, looted from a grab bag (excluding presents), or dropped by a slain enemy).

			// To change the chance of a prefix being rolled or not, return true or false depending on some condition
			if (pre == -1 && item.type == ItemID.Shackle) {
				// Force rolling
				// return true;

				// When using random numbers, make sure to use the rand object passed into this method, and not Main.rand.
				// This will make it consistent with worldgen should this item be spawned in a chest
				if (rand.NextFloat() < 0.5f) {
					// Increase the chance of not receiving any prefix on spawn by 50%
					return false;
				}
				// Keep in mind that if the code arrives here, there is still a 25% chance that it won't get a modifier.
				// If you want a more controlled approach, return true in an else block
			}

			return null;
		}
	}
}
