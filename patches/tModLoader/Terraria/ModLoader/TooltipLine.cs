using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

/// <summary>
/// This class serves as a way to store information about a line of tooltip for an item. You will create and manipulate objects of this class if you use the ModifyTooltips hook.
/// </summary>
public class TooltipLine
{
	/// <summary>
	/// The name of the mod adding this tooltip line. This will be "Terraria" for all vanilla tooltip lines.
	/// </summary>
	public readonly string Mod;

	/// <summary>
	/// The name of the tooltip, used to help you identify its function.
	/// </summary>
	public readonly string Name;

	/// <summary>
	/// => $"{Mod}/{Name}"
	/// </summary>
	public string FullName => $"{Mod}/{Name}";

	/// <summary>
	/// The actual text that this tooltip displays.
	/// </summary>
	public string Text;

	/// <summary>
	/// The color the tooltip is drawn in. Defaults to White.
	/// <para/> <see cref="Terraria.ID.Colors.PrefixGood"/> and <see cref="Terraria.ID.Colors.PrefixBad"/> are the colors used by prefix stat changes.
	/// <para/> The actual final color will be affected slightly by the mouse pulse effect (<see cref="Main.mouseTextColor"/>).
	/// </summary>
	public Color Color;

	internal bool OneDropLogo;

	/// <summary>
	/// Creates a tooltip line object with the given mod, identifier name, and text.<para />
	/// These are the names of the vanilla tooltip lines, in the order in which they appear, along with their functions. All of them will have a mod name of "Terraria". Remember that most of these tooltip lines will not exist depending on the item.<para />
	/// <list type="bullet">
	/// <item><description>"ItemName" - The name of the item.</description></item>
	/// <item><description>"Favorite" - Tells if the item is favorited.</description></item>
	/// <item><description>"FavoriteDesc" - Tells what it means when an item is favorited.</description></item>
	/// <item><description>"NoTransfer" - Warning that this item cannot be placed inside itself, used by Money Trough and Void Bag/Vault.</description></item>
	/// <item><description>"Social" - Tells if the item is in a social slot.</description></item>
	/// <item><description>"Damage" - The damage value and type of the weapon.</description></item>
	/// <item><description>"CritChance" - The critical strike chance of the weapon.</description></item>
	/// <item><description>"Speed" - The use speed of the weapon.</description></item>
	/// <item><description>"NoSpeedScaling" - Whether this item does not scale with attack speed, added by tModLoader.</description></item>
	/// <item><description>"SpecialSpeedScaling" - The multiplier this item applies to attack speed bonuses, added by tModLoader.</description></item>
	/// <item><description>"Knockback" - The knockback of the weapon.</description></item>
	/// <item><description>"FishingPower" - Tells the fishing power of the fishing pole.</description></item>
	/// <item><description>"NeedsBait" - Tells that a fishing pole requires bait.</description></item>
	/// <item><description>"BaitPower" - The bait power of the bait.</description></item>
	/// <item><description>"Equipable" - Tells that an item is equipable.</description></item>
	/// <item><description>"WandConsumes" - Tells what item a tile wand consumes.</description></item>
	/// <item><description>"Quest" - Tells that this is a quest item.</description></item>
	/// <item><description>"Vanity" - Tells that this is a vanity item.</description></item>
	/// <item><description>"Defense" - Tells how much defense the item gives when equipped.</description></item>
	/// <item><description>"PickPower" - The item's pickaxe power.</description></item>
	/// <item><description>"AxePower" - The item's axe power.</description></item>
	/// <item><description>"HammerPower" - The item's hammer power.</description></item>
	/// <item><description>"TileBoost" - How much farther the item can reach than normal items.</description></item>
	/// <item><description>"HealLife" - How much health the item recovers when used.</description></item>
	/// <item><description>"HealMana" - How much mana the item recovers when used.</description></item>
	/// <item><description>"UseMana" - Tells how much mana the item consumes upon usage.</description></item>
	/// <item><description>"Placeable" - Tells if the item is placeable.</description></item>
	/// <item><description>"Ammo" - Tells if the item is ammo.</description></item>
	/// <item><description>"Consumable" - Tells if the item is consumable.</description></item>
	/// <item><description>"Material" - Tells if the item can be used to craft something.</description></item>
	/// <item><description>"Wireable" - Tells if the item is wireable.</description></item>
	/// <item><description>"Container" - The "Collects nearby dropped items when signaled" line.</description></item>
	/// <item><description>"WireTrigger" - The "{InputTrigger_InteractWithTile} when placed to signal" line.</description></item>
	/// <item><description>"Tooltip#" - A tooltip line of the item. # will be 0 for the first line, 1 for the second, etc.</description></item>
	/// <item><description>"WizardHatDuringAnniversary" - The "Increases your max number of minions by 1" line.</description></item>
	/// <item><description>"BurningBlock" - The "Burns you when touched without special protection" line.</description></item>
	/// <item><description>"MechSummonDuringEverything" - The "'Part of a set'" line.</description></item>
	/// <item><description>"MechdusaSummonNotDuringEverything" - The "'It has no effect in this world'" line.</description></item>
	/// <item><description>"EtherianManaWarning" - Warning about how the item can't be used without Etherian Mana until the Eternia Crystal has been defeated.</description></item>
	/// <item><description>"WellFedExpert" - In expert mode, tells that food increases life regeneration.</description></item>
	/// <item><description>"BuffTime" - Tells how long the item's buff lasts.</description></item>
	/// <item><description>"OneDropLogo" - The One Drop logo for yoyos.This is a specially-marked tooltip line that has no text.</description></item>
	/// <item><description>"PrefixDamage" - The damage modifier of the prefix.</description></item>
	/// <item><description>"PrefixSpeed" - The usage speed modifier of the prefix.</description></item>
	/// <item><description>"PrefixCritChance" - The critical strike chance modifier of the prefix.</description></item>
	/// <item><description>"PrefixUseMana" - The mana consumption modifier of the prefix.</description></item>
	/// <item><description>"PrefixSize" - The melee size modifier of the prefix.</description></item>
	/// <item><description>"PrefixShootSpeed" - The shootSpeed modifier of the prefix.</description></item>
	/// <item><description>"PrefixKnockback" - The knockback modifier of the prefix.</description></item>
	/// <item><description>"PrefixArmorPenetration" - The armor penetration modifier of the prefix.</description></item>
	/// <item><description>"PrefixTagDamage" - The tag damage modifier of the prefix.</description></item>
	/// <item><description>"PrefixAccDefense" - The defense modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccMaxMana" - The maximum mana modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccCritChance" - The critical strike chance modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccDamage" - The damage modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccMoveSpeed" - The movement speed modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccMeleeSpeed" - The melee speed modifier of the accessory prefix.</description></item>
	/// <item><description>"SetBonusSinglePiece" - The set bonus description from a single piece of armor that is not equipped.</description></item>
	/// <item><description>"SetBonus" - The set bonus description of the armor set.</description></item>
	/// <item><description>"Expert" - Tells whether the item is from expert-mode.</description></item>
	/// <item><description>"Master" - Whether the item is exclusive to Master Mode.</description></item>
	/// <item><description>"JourneyResearch" - How many more items need to be researched to unlock duplication in Journey Mode.</description></item>
	/// <item><description>"JourneyResearchTeammate" - The "Research completed by {0}" line.</description></item>
	/// <item><description>"ModifiedByMods" - Whether the item has been modified by any mods and what mods when holding shift, added by tModLoader.</description></item>
	/// <item><description>"BestiaryNotes" - Any bestiary notes, used when hovering items in the bestiary.</description></item>
	/// <item><description>"SpecialPrice" - Tells the alternate currency price of an item.</description></item>
	/// <item><description>"Price" - Tells the price of an item.</description></item>
	/// <item><description>"MissingRequirements" - Tells the crafting requirements not met for crafting this item.</description></item>
	/// </list>
	/// </summary>
	/// <param name="mod">The mod instance</param>
	/// <param name="name">The name of the tooltip</param>
	/// <param name="text">The content of the tooltip</param>
	public TooltipLine(Mod mod, string name, string text)
	{
		Mod = mod.Name;
		Name = name;
		Text = text;
		Color = Color.White;
	}

	internal TooltipLine(string mod, string name, string text)
	{
		Mod = mod;
		Name = name;
		Text = text;
		Color = Color.White;
	}

	internal TooltipLine(string name, string text)
	{
		Mod = "Terraria";
		Name = name;
		Text = text;
		Color = Color.White;
	}

	public bool Visible { get; private set; } = true;

	public void Hide() => Visible = false;
}
