using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

/// <summary>
/// This serves as a place for you to program behaviors of equipment textures. This is useful for equipment slots that do not have any item associated with them (for example, the Werewolf buff). Note that this class is purely for visual effects.
/// </summary>
public class EquipTexture
{
	/// <summary>
	/// The name and folders of the texture file used by this equipment texture.
	/// </summary>
	public string Texture { get; internal set; }

	/// <summary>
	/// The internal name of this equipment texture.
	/// </summary>
	public string Name { get; internal set; }

	/// <summary>
	/// The type of equipment that this equipment texture is used as.
	/// </summary>
	public EquipType Type { get; internal set; }

	/// <summary>
	/// The slot (internal ID) of this equipment texture.
	/// </summary>
	public int Slot { get; internal set; }

	/// <summary>
	/// The item that is associated with this equipment texture. Null if no item is associated with this.
	/// </summary>
	public ModItem Item { get; internal set; }

	/// <summary>
	/// Allows you to create special effects (such as dust) when this equipment texture is displayed on the player under the given equipment type. By default this will call the associated ModItem's <see cref="ModItem.UpdateVanity(Player)"/> if there is an associated ModItem.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="type"></param>
	public virtual void FrameEffects(Player player, EquipType type)
	{
		if (Item != null) {
			Item.EquipFrameEffects(player, type);
		}
	}

	/// <summary>
	/// Returns whether or not the head armor, body armor, and leg armor textures make up a set. This hook is used for the <see cref="PreUpdateVanitySet(Player)"/>, <see cref="UpdateVanitySet(Player)"/>, and <see cref="ArmorSetShadows(Player)"/> hooks. By default this will return the same thing as the associated ModItem's <see cref="ModItem.IsVanitySet(int, int, int)"/>, or false if no ModItem is associated.
	/// </summary>
	/// <param name="head">The visible head equipment slot (<see cref="Player.head"/>).</param>
	/// <param name="body">The visible body equipment slot (<see cref="Player.body"/>).</param>
	/// <param name="legs">The visible legs equipment slot (<see cref="Player.legs"/>).</param>
	/// <returns></returns>
	public virtual bool IsVanitySet(int head, int body, int legs)
	{
		if (Item == null) {
			return false;
		}
		return Item.IsVanitySet(head, body, legs);
	}

	/// <summary>
	/// Allows you to create special effects (such as the necro armor's hurt noise) when the player wears this equipment texture's vanity set. This hook is called regardless of whether the player is frozen in any way. By default this will call the associated ModItem's <see cref="ModItem.PreUpdateVanitySet(Player)"/> if there is an associated ModItem.
	/// </summary>
	/// <param name="player"></param>
	public virtual void PreUpdateVanitySet(Player player)
	{
		if (Item != null) {
			Item.PreUpdateVanitySet(player);
		}
	}

	/// <summary>
	/// Allows you to create special effects (such as dust) when the player wears this equipment texture's vanity set. This hook will only be called if the player is not frozen in any way. By default this will call the associated ModItem's <see cref="ModItem.UpdateVanitySet(Player)"/> if there is an associated ModItem.
	/// </summary>
	/// <param name="player"></param>
	public virtual void UpdateVanitySet(Player player)
	{
		if (Item != null) {
			Item.UpdateVanitySet(player);
		}
	}

	/// <summary>
	/// Allows you to determine special visual effects this vanity set has on the player without having to code them yourself. By default this will call the associated ModItem's <see cref="ModItem.ArmorSetShadows(Player)"/> if there is an associated ModItem.
	/// </summary>
	/// <param name="player"></param>
	public virtual void ArmorSetShadows(Player player)
	{
		if (Item != null) {
			Item.ArmorSetShadows(player);
		}
	}

	/// <summary>
	/// Allows you to modify the equipment that the player appears to be wearing. This is most commonly used to add legs to robes and for swapping to female variant textures if <paramref name="male"/> is false for head and leg armor. This hook will only be called for head, body, and leg textures. Note that equipSlot is not the same as the item type of the armor the player will appear to be wearing. Worn equipment has a separate set of IDs. You can find the vanilla equipment IDs by looking at the <see cref="Item.headSlot"/>, <see cref="Item.bodySlot"/>, and <see cref="Item.legSlot"/> fields for items, and modded equipment IDs by looking at EquipLoader.
	/// <para/> If this hook is called on body armor, equipSlot allows you to modify the leg armor the player appears to be wearing. If you modify it, make sure to set robes to true. If this hook is called on leg armor, equipSlot allows you to modify the leg armor the player appears to be wearing, and the robes parameter is useless. The same is true for head armor.
	/// <para/> By default, if there is an associated ModItem, this will call that ModItem's <see cref="ModItem.SetMatch(bool, ref int, ref bool)"/>.
	/// </summary>
	/// <param name="male"></param>
	/// <param name="equipSlot"></param>
	/// <param name="robes"></param>
	public virtual void SetMatch(bool male, ref int equipSlot, ref bool robes)
	{
		if (Item != null) {
			Item.SetMatch(male, ref equipSlot, ref robes);
		}
	}

	/// <summary>
	/// Allows you to modify the colors in which this armor texture and surrounding accessories are drawn, in addition to which glow mask and in what color is drawn. By default this will call the associated ModItem's <see cref="ModItem.DrawArmorColor(Player, float, ref Color, ref int, ref Color)"/> if there is an associated ModItem.
	/// </summary>
	/// <param name="drawPlayer"></param>
	/// <param name="shadow"></param>
	/// <param name="color"></param>
	/// <param name="glowMask"></param>
	/// <param name="glowMaskColor"></param>
	public virtual void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
	{
		if (Item != null) {
			Item.DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
		}
	}

	/// <summary>
	/// Allows you to modify which glow mask and in what color is drawn on the player's arms. Note that this is only called for body equipment textures. By default this will call the associated ModItem's <see cref="ModItem.ArmorArmGlowMask(Player, float, ref int, ref Color)"/> if there is an associated ModItem.
	/// </summary>
	/// <param name="drawPlayer"></param>
	/// <param name="shadow"></param>
	/// <param name="glowMask"></param>
	/// <param name="color"></param>
	public virtual void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color)
	{
		if (Item != null) {
			Item.ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);
		}
	}

	/// <summary>
	/// Allows you to modify vertical wing speeds.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="ascentWhenFalling"></param>
	/// <param name="ascentWhenRising"></param>
	/// <param name="maxCanAscendMultiplier"></param>
	/// <param name="maxAscentMultiplier"></param>
	/// <param name="constantAscend"></param>
	public virtual void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
	{
		if (Item != null) {
			Item.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
				ref maxAscentMultiplier, ref constantAscend);
		}
	}

	/// <summary>
	/// Allows you to modify horizontal wing speeds.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="speed"></param>
	/// <param name="acceleration"></param>
	public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
	{
		if (Item != null) {
			Item.HorizontalWingSpeeds(player, ref speed, ref acceleration);
		}
	}

	/// <summary>
	/// Allows for wing textures to do various things while in use. "inUse" is whether or not the jump button is currently pressed. Called when this wing texture visually appears on the player. Use to animate wings, create dusts, invoke sounds, and create lights. By default this will call the associated ModItem's <see cref="ModItem.WingUpdate(Player, bool)"/> if there is an associated ModItem.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="inUse"></param>
	/// <returns></returns>
	public virtual bool WingUpdate(Player player, bool inUse)
	{
		return Item?.WingUpdate(player, inUse) ?? false;
	}
}
