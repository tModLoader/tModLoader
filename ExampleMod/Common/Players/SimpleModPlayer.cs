using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// This file shows the very basics of using ModPlayer classes.
	// The comments in this file and https://github.com/tModLoader/tModLoader/wiki/ModPlayer are useful for learning how to use ModPlayer in your mod.

	// ModPlayer classes provide a way to attach data to Players and act on that data.
	// This example will hopefully provide you with an understanding of the basic building blocks of how ModPlayer works.
	// This example will teach the most commonly sought after effect: "How to do X if the player has Y?"
	// X in this example will be "Apply a debuff to enemies."
	// Y in this example will be "Wearing an accessory."
	// After studying this example, you can change X to other effects by changing the "hook" you use or the code within the hook you use. For example, you could use OnHitByNPC and call Projectile.NewProjectile within that hook to change X to "When the player is hit by NPC, spawn Projectiles".
	// We can change Y to other conditions as well. For example, you could give the player the effect by having a "potion" ModItem give a ModBuff that sets the ModPlayer variable in ModBuff.Update
	// Another example would be an armor set effect. Simply use the ModItem.UpdateArmorSet hook.
	// The point is, each of these effects follow the same pattern.

	// Below you will see the ModPlayer class (SimpleModPlayer), and below that will be a ModItem class called SimpleAccessory which is an accessory item. These are both in the same file for your reading convenience. This accessory will give our effect to our ModPlayer.

	// This is the ModPlayer class. Make note of the classname, which is SimpleModPlayer, since we will be using this in the accessory item below.
	public class SimpleModPlayer : ModPlayer
	{
		// Here we declare the FrostBurnSummon variable which will represent whether this player has the effect or not.
		public bool FrostBurnSummon;

		// ResetEffects is used to reset effects back to their default value. Terraria resets all effects every frame back to defaults so we will follow this design. (You might think to set a variable when an item is equipped and un-assign the value when the item in unequipped, but Terraria is not designed that way.)
		public override void ResetEffects() {
			FrostBurnSummon = false;
		}

		// Here we use a "hook" to actually let our FrostBurnSummon status take effect. This hook is called anytime a player owned projectile hits an enemy.
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			// FrostBurnSummon, as its name suggests, applies frostBurn to enemy NPC but only for Summon projectiles (minions, sentries, minion shots, and sentry shots).
			// In this if statement we check several conditions. We first check to make sure the projectile that hit the NPC is either a minion, sentry, minion shot, or sentry shot projectile.
			// We then check that FrostBurnSummon is set to true. The last check for not noEnchantments is because some projectiles don't allow enchantments and we want to honor that restriction.
			if (proj.IsMinionOrSentryRelated && FrostBurnSummon && !proj.noEnchantments) {
				// If all those checks pass, we apply FrostBurn for some random duration.
				target.AddBuff(BuffID.Frostburn, 60 * Main.rand.Next(3, 6));
			}
		}

		// As a recap. Make a class variable, reset that variable in ResetEffects, and use that variable in the logic of whatever hooks you use.
	}
}

// Below is SimpleAccessory, the ModItem that gives the player the FrostBurnSummon effect when worn as an accessory.
// Usually we would put this in the ExampleMod/Content/Items/Accessories folder for organization, but we include it here for learning purposes. Since the namespace is ExampleMod.Content.Items.Accessories, the textures will still be loaded from that folder as expected, since textures are loaded from the folder based off of the namespace.
namespace ExampleMod.Content.Items.Accessories
{
	// Assigning multiple EquipType/Animation textures is easily done.
	[AutoloadEquip(EquipType.Neck, EquipType.Balloon)]
	public class SimpleAccessory : ModItem
	{
		public override void SetDefaults() {
			Item.width = 34;
			Item.height = 34;
			Item.accessory = true;
			Item.value = Item.buyPrice(gold: 15);
			Item.rare = ItemRarityID.Pink;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// To assign the player the FrostBurnSummon effect, we can't do player.FrostBurnSummon = true because Player doesn't have FrostBurnSummon. Be sure to remember to call the GetModPlayer method to retrieve the ModPlayer instance attached to the specified Player.
			player.GetModPlayer<SimpleModPlayer>().FrostBurnSummon = true;
		}
	}
}
