using Microsoft.Xna.Framework;
using Terraria.ModLoader.Default;
using Terraria.UI;

//NOTE: Does not fully support: ItemLoader.GetGamepadInstructions and related gamepad stuff.
//TODO: Documentation?

namespace Terraria.ModLoader;

/// <summary>
/// A ModAccessorySlot instance represents a net new accessory slot instance. You can store fields in the ModAccessorySlot class.
/// </summary>
public abstract class ModAccessorySlot : ModType
{
	public int Type { get; internal set; }

	/// <summary>
	/// The player that this ModAccessorySlot is currently accessing, equivalent to <see cref="Main.CurrentPlayer"/>. ModAccessorySlot are singletons, the behavior of properties like <see cref="FunctionalItem"/> depends on the current player.
	/// <br/><br/> <b>Main.CurrentPlayer documentation:</b><br/> <inheritdoc cref="Main.CurrentPlayer"/>
	/// </summary>
	public static Player Player => Main.CurrentPlayer;

	public ModAccessorySlotPlayer ModSlotPlayer => AccessorySlotLoader.ModSlotPlayer(Player);

	// Properties to preset a location for the accessory slot
	public virtual Vector2? CustomLocation => null;

	// Properties to change the Default Background Texture
	public virtual string DyeBackgroundTexture => null;
	public virtual string VanityBackgroundTexture => null;
	public virtual string FunctionalBackgroundTexture => null;

	// Properties to change the Default Style Image Texture
	public virtual string DyeTexture => null;
	public virtual string VanityTexture => null;
	public virtual string FunctionalTexture => null;

	// Properties to control which parts of accessory slot draw
	public virtual bool DrawFunctionalSlot => true;
	public virtual bool DrawVanitySlot => true;
	public virtual bool DrawDyeSlot => true;

	/// <summary>
	/// Gets or sets a value indicating whether this slot supports equipment loadouts. If <see langword="false"/>,
	/// the slot's item is shared between all loadouts.
	/// <br/><br/> Defaults to <see langword="true"/>.
	/// <br/><br/> Changing this value requires a reload. This value is not allowed to be different between multiplayer clients or issues will occur.
	/// <br/><br/> Changing the value from <see langword="true"/> to <see langword="false"/> will cause the extra items to be spawned on the player when they enter the world.
	/// <br/> Changing the value from <see langword="false"/> to <see langword="true"/> will result in the currently selected loadout holding the items.
	/// <br/><br/> Slots that don't support loadouts will appear with the default green background texture, as if they were an accessory in loadout #1 or from before loadout support was added to the game.
	/// </summary>
	public virtual bool HasEquipmentLoadoutSupport => true;

	// Get/Set Properties for fetching slot information
	/// <summary>
	/// The functional item of this equipment slot.
	/// <br/><br/> This property always accesses the equipment of the current player, see <see cref="Player"/> for more info.
	/// </summary>
	public Item FunctionalItem {
		get => ModSlotPlayer.exAccessorySlot[Type];
		set => ModSlotPlayer.exAccessorySlot[Type] = value;
	}

	/// <summary>
	/// The vanity item of this equipment slot.
	/// <br/><br/> This property always accesses the equipment of the current player, see <see cref="Player"/> for more info.
	/// </summary>
	public Item VanityItem {
		get => ModSlotPlayer.exAccessorySlot[Type + ModSlotPlayer.SlotCount];
		set => ModSlotPlayer.exAccessorySlot[Type + ModSlotPlayer.SlotCount] = value;
	}

	/// <summary>
	/// The dye item of this equipment slot.
	/// <br/><br/> This property always accesses the equipment of the current player, see <see cref="Player"/> for more info.
	/// </summary>
	public Item DyeItem {
		get => ModSlotPlayer.exDyesAccessory[Type];
		set => ModSlotPlayer.exDyesAccessory[Type] = value;
	}

	/// <summary>
	/// Indicates if the corresponding <see cref="FunctionalItem"/> is set to be hidden or not.
	/// <br/><br/> This property always accesses the equipment of the current player, see <see cref="Player"/> for more info.
	/// </summary>
	public bool HideVisuals {
		get => ModSlotPlayer.exHideAccessory[Type];
		set => ModSlotPlayer.exHideAccessory[Type] = value;
	}

	public bool IsEmpty => FunctionalItem.IsAir && VanityItem.IsAir && DyeItem.IsAir;

	// Functionality and Hooks
	protected sealed override void Register() => Type = LoaderManager.Get<AccessorySlotLoader>().Register(this);

	/// <summary>
	/// Allows drawing prior to vanilla ItemSlot.Draw code. Return false to NOT call ItemSlot.Draw
	/// </summary>
	public virtual bool PreDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered)
	{
		return true;
	}

	public virtual void PostDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) { }

	/// <summary>
	/// Override to replace the vanilla effect behavior of the slot with your own.
	/// By default calls:
	/// Player.VanillaUpdateEquips(FunctionalItem), Player.ApplyEquipFunctional(FunctionalItem, ShowVisuals), Player.ApplyEquipVanity(VanityItem)
	/// </summary>
	public virtual void ApplyEquipEffects()
	{
		if (FunctionalItem.accessory)
			Player.GrantPrefixBenefits(FunctionalItem);

		Player.GrantArmorBenefits(FunctionalItem);
		Player.ApplyEquipFunctional(FunctionalItem, HideVisuals);
		Player.ApplyEquipVanity(VanityItem);
	}

	/// <summary>
	/// Override to set conditions on what can be placed in the slot. Default is to return false only when item property FitsAccessoryVanity says can't go in to a vanity slot.
	/// Return false to prevent the item going in slot. Return true for dyes, if you want dyes. Example: only wings can go in slot.
	/// Receives data:
	/// <para><paramref name="checkItem"/> :: the item that is attempting to enter the slot </para>
	/// </summary>
	public virtual bool CanAcceptItem(Item checkItem, AccessorySlotType context)
	{
		if (context == AccessorySlotType.VanitySlot) {
			return checkItem.FitsAccessoryVanitySlot;
		}

		return true;
	}

	/// <summary>
	/// After checking for empty slots in ItemSlot.AccessorySwap, this allows for changing what the default target slot (accSlotToSwapTo) will be.
	/// DOES NOT affect vanilla behavior of swapping items like for like where existing in a slot
	/// Return true to set this slot as the default targeted slot.
	/// </summary>
	public virtual bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) => false;

	/// <summary>
	/// Override to control whether or not drawing will be skipped during the given frame.
	/// NOTE: Nothing will be drawn, nor will subsequent drawing hooks be called on this slot for the frame while true
	/// </summary>
	public virtual bool IsHidden() => false;

	/// <summary>
	/// Override to set conditions on when the slot is valid for stat/vanity calculations and player usage.
	/// Example: the demonHeart is consumed and in Expert mode in Vanilla.
	/// </summary>
	public virtual bool IsEnabled() => true;

	/// <summary>
	/// Override to change the condition on when the slot is visible, but otherwise non-functional for stat/vanity calculations.
	/// Defaults to check 'property' IsEmpty
	/// </summary>
	/// <returns></returns>
	public virtual bool IsVisibleWhenNotEnabled() => !IsEmpty;

	/// <summary>
	/// Allows you to do stuff while the player is hovering over this slot.
	/// </summary>
	public virtual void OnMouseHover(AccessorySlotType context) { }

	/// <summary>
	/// Allows customizing the background texture draw color.
	/// <br/><br/> For <see cref="HasEquipmentLoadoutSupport"/> slots without a custom texture, the color will already have been adjusted for the current loadout (ItemSlot.GetColorByLoadout), otherwise the color will be <see cref="Main.inventoryBack"/>, which will oscillate all values between 180 and 240.
	/// </summary>
	public virtual void BackgroundDrawColor(AccessorySlotType context, ref Color color) { }
}
