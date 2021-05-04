using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Mono.Cecil.Cil;

namespace ExampleMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Back)]
	public class WaspNest : ModItem
	{
		// Only gets run once per type
		public override void Load() {
			IL.Terraria.Player.beeType += HookBeeType;
		}

		// This IL editing (Intermediate Language editing) example is walked through in the guide: https://github.com/tModLoader/tModLoader/wiki/Expert-IL-Editing#example---hive-pack-upgrade
		private static void HookBeeType(ILContext il) {
			ILCursor c = new ILCursor(il);

			// Try to find where 566 is placed onto the stack
			if (!c.TryGotoNext(i => i.MatchLdcI4(566))) {
				return; // Patch unable to be applied
			}

			// Move the cursor after 566 and onto the ret op.
			c.Index++;
			// Push the Player instance onto the stack
			c.Emit(OpCodes.Ldarg_0);
			// Call a delegate using the int and Player from the stack.
			c.EmitDelegate<Func<int, Player, int>>((returnValue, player) => {
				// Regular c# code
				if (player.GetModPlayer<WaspNestPlayer>().strongBeesUpgrade && Main.rand.NextBool(10) && Main.ProjectileUpdateLoopIndex == -1) {
					return ProjectileID.Beenade;
				}

				return returnValue;
			});
		}

		public override void SetStaticDefaults() {
			// We can use vanilla language keys to copy the tooltip from HiveBackpack
			Tooltip.SetDefault("{$ItemTooltip.HiveBackpack}");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			sbyte realBackSlot = Item.backSlot;
			Item.CloneDefaults(ItemID.HiveBackpack);
			Item.value = Item.sellPrice(0, 5);
			// CloneDefaults will clear out the autoloaded Back slot, so we need to preserve it this way.
			Item.backSlot = realBackSlot;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// The original Hive Pack sets strongBees.
			player.strongBees = true;
			// Here we add an additional effect
			player.GetModPlayer<WaspNestPlayer>().strongBeesUpgrade = true;
		}
	}

	public class WaspNestPlayer : ModPlayer
	{
		public bool strongBeesUpgrade;
	}
}