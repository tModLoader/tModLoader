using System;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a way to store information about a line of tooltip for an item. You will create and manipulate objects of this class if you use the ModifyTooltips hook.
	/// </summary>
	public class TooltipLine
	{
		/// <summary>
		/// The name of the mod adding this tooltip line. This will be "Terraria" for all vanilla tooltip lines.
		/// </summary>
		public readonly string mod;
		/// <summary>
		/// The name of the tooltip, used to help you identify its function.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// The actual text that this tooltip displays.
		/// </summary>
		public string text;
		/// <summary>
		/// Whether or not this tooltip gives prefix information. This will make it so that the tooltip is colored either green or red.
		/// </summary>
		public bool isModifier = false;
		/// <summary>
		/// If isModifier is true, this determines whether the tooltip is colored green or red.
		/// </summary>
		public bool isModifierBad = false;
		/// <summary>
		/// This completely overrides the color the tooltip is drawn in. If it is set to null (the default value) then the tooltip's color will not be overridden.
		/// </summary>
		public Color? overrideColor = null;

		internal bool oneDropLogo = false;

		/// <summary>
		/// Creates a tooltip line object with the given mod, identifier name, and text.
		/// These are the names of the vanilla tooltip lines, in the order in which they appear, along with their functions. All of them will have a mod name of "Terraria". Remember that most of these tooltip lines will not exist depending on the item.
		///"ItemName" - The name of the item.
		///"Favorite" - Tells if the item is favorited.
		///"FavoriteDesc" - Tells what it means when an item is favorited.
		///"Social" - Tells if the item is in a social slot.
		///"SocialDesc" - Tells what it means for an item to be in a social slot.
		///"Damage" - The damage value and type of the weapon.
		///"CritChance" - The critical strike chance of the weapon.
		///"Speed" - The use speed of the weapon.
		///"Knockback" - The knockback of the weapon.
		///"FishingPower" - Tells the fishing power of the fishing pole.
		///"NeedsBait" - Tells that a fishing pole requires bait.
		///"BaitPower" - The bait power of the bait.
		///"Equipable" - Tells that an item is equipable.
		///"WandConsumes" - Tells what item a tile wand consumes.
		///"Quest" - Tells that this is a quest item.
		///"Vanity" - Tells that this is a vanity item.
		///"Defense" - Tells how much defense the item gives when equipped.
		///"PickPower" - The item's pickaxe power.
		///"AxePower" - The item's axe power.
		///"HammerPower" - The item's hammer power.
		///"TileBoost" - How much farther the item can reach than normal items.
		///"HealLife" - How much health the item recovers when used.
		///"HealMana" - How much mana the item recovers when used.
		///"UseMana" - Tells how much mana the item consumes upon usage.
		///"Placeable" - Tells if the item is placeable.
		///"Ammo" - Tells if the item is ammo.
		///"Consumable" - Tells if the item is consumable.
		///"Material" - Tells if the item can be used to craft something.
		///"Tooltip" - The tooltip field of the item.
		///"Tooltip2" - The tooltip2 field of the item.
		///"EtherianManaWarning" - Warning about how the item can't be used without Etherian Mana until the Eternia Crystal has been defeated.
		///"WellFedExpert" - In expert mode, tells that food increases life renegeration.
		///"BuffTime" - Tells how long the item's buff lasts.
		///"OneDropLogo" - The One Drop logo for yoyos.This is a specially-marked tooltip line that has no text.
		///"PrefixDamage" - The damage modifier of the prefix.
		///"PrefixSpeed" - The usage speed modifier of the prefix.
		///"PrefixCritChance" - The critical strike chance modifier of the prefix.
		///"PrefixUseMana" - The mana consumption modifier of the prefix.
		///"PrefixSize" - The melee size modifier of the prefix.
		///"PrefixShootSpeed" - The shootSpeed modifier of the prefix.
		///"PrefixKnockback" - The knockback modifier of the prefix.
		///"PrefixAccDefense" - The defense modifier of the accessory prefix.
		///"PrefixAccMaxMana" - The maximum mana modifier of the accessory prefix.
		///"PrefixAccCritChance" - The critical strike chance modifier of the accessory prefix.
		///"PrefixAccDamage" - The damage modifier of the accessory prefix.
		///"PrefixAccMoveSpeed" - The movement speed modifier of the accessory prefix.
		///"PrefixAccMeleeSpeed" - The melee speed modifier of the accessory prefix.
		///"SetBonus" - The set bonus description of the armor set.
		///"Expert" - Tells whether the item is from expert-mode.
		///"SpecialPrice" - Tells the alternate currency price of an item.
		///"Price" - Tells the price of an item.
		/// </summary>
		/// <param name="mod">The mod instance</param>
		/// <param name="name">The name of the tooltip</param>
		/// <param name="text">The content of the tooltip</param>
		public TooltipLine(Mod mod, string name, string text)
		{
			this.mod = mod.Name;
			this.Name = name;
			this.text = text;
		}

		internal TooltipLine(string name, string text)
		{
			this.mod = "Terraria";
			this.Name = name;
			this.text = text;
		}
	}
}
