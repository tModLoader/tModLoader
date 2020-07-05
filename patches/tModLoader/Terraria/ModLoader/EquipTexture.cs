using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as a place for you to program behaviors of equipment textures. This is useful for equipment slots that do not have any item associated with them (for example, the Werewolf buff). Note that this class is purely for visual effects.
	/// </summary>
	public class EquipTexture
	{
		/// <summary>
		/// The name and folders of the texture file used by this equipment texture.
		/// </summary>
		public string Texture {
			get;
			internal set;
		}

		/// <summary>
		/// The mod that added this equipment texture.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The internal name of this equipment texture.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The type of equipment that this equipment texture is used as.
		/// </summary>
		public EquipType Type {
			get;
			internal set;
		}

		/// <summary>
		/// The slot (internal ID) of this equipment texture.
		/// </summary>
		public int Slot {
			get;
			internal set;
		}

		/// <summary>
		/// The item that is associated with this equipment texture. Null if no item is associated with this.
		/// </summary>
		public ModItem item {
			get;
			internal set;
		}

		/// <summary>
		/// Allows you to create special effects (such as dust) when this equipment texture is displayed on the player under the given equipment type. By default this will call the associated ModItem's UpdateVanity if there is an associated ModItem.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="type"></param>
		public virtual void UpdateVanity(Player player, EquipType type) {
			if (item != null) {
				item.UpdateVanity(player, type);
			}
		}

		/// <summary>
		/// Returns whether or not the head armor, body armor, and leg armor textures make up a set. This hook is used for the PreUpdateVanitySet, UpdateVanitySet, and ArmorSetShadow hooks. By default this will return the same thing as the associated ModItem's IsVanitySet, or false if no ModItem is associated.
		/// </summary>
		/// <param name="head"></param>
		/// <param name="body"></param>
		/// <param name="legs"></param>
		/// <returns></returns>
		public virtual bool IsVanitySet(int head, int body, int legs) {
			if (item == null) {
				return false;
			}
			return item.IsVanitySet(head, body, legs);
		}

		/// <summary>
		/// Allows you to create special effects (such as the necro armor's hurt noise) when the player wears this equipment texture's vanity set. This hook is called regardless of whether the player is frozen in any way. By default this will call the associated ModItem's PreUpdateVanitySet if there is an associated ModItem.
		/// </summary>
		/// <param name="player"></param>
		public virtual void PreUpdateVanitySet(Player player) {
			if (item != null) {
				item.PreUpdateVanitySet(player);
			}
		}

		/// <summary>
		/// Allows you to create special effects (such as dust) when the player wears this equipment texture's vanity set. This hook will only be called if the player is not frozen in any way. By default this will call the associated ModItem's UpdateVanitySet if there is an associated ModItem.
		/// </summary>
		/// <param name="player"></param>
		public virtual void UpdateVanitySet(Player player) {
			if (item != null) {
				item.UpdateVanitySet(player);
			}
		}

		/// <summary>
		/// Allows you to determine special visual effects this vanity set has on the player without having to code them yourself. By default this will call the associated ModItem's ArmorSetShadows if there is an associated ModItem.
		/// </summary>
		/// <param name="player"></param>
		public virtual void ArmorSetShadows(Player player) {
			if (item != null) {
				item.ArmorSetShadows(player);
			}
		}

		/// <summary>
		/// Allows you to modify the equipment that the player appears to be wearing. This hook will only be called for head, body and leg textures. Note that equipSlot is not the same as the item type of the armor the player will appear to be wearing. Worn equipment has a separate set of IDs. You can find the vanilla equipment IDs by looking at the headSlot, bodySlot, and legSlot fields for items, and modded equipment IDs by looking at EquipLoader.
		///If this hook is called on body armor, equipSlot allows you to modify the leg armor the player appears to be wearing. If you modify it, make sure to set robes to true. If this hook is called on leg armor, equipSlot allows you to modify the leg armor the player appears to be wearing, and the robes parameter is useless.
		///By default, if there is an associated ModItem, this will call that ModItem's SetMatch.
		/// </summary>
		/// <param name="male"></param>
		/// <param name="equipSlot"></param>
		/// <param name="robes"></param>
		public virtual void SetMatch(bool male, ref int equipSlot, ref bool robes) {
			if (item != null) {
				item.SetMatch(male, ref equipSlot, ref robes);
			}
		}

		/// <summary>
		/// Allows you to determine whether the skin/shirt on the player's arms and hands are drawn when this body equipment texture is worn. By default both flags will be false. Note that if drawHands is false, the arms will not be drawn either. If there is an associated ModItem, by default this will call that ModItem's DrawHands.
		/// </summary>
		/// <param name="drawHands"></param>
		/// <param name="drawArms"></param>
		public virtual void DrawHands(ref bool drawHands, ref bool drawArms) {
			if (item != null) {
				item.DrawHands(ref drawHands, ref drawArms);
			}
		}

		/// <summary>
		/// Allows you to determine whether the player's hair or alt (hat) hair draws when this head equipment texture is worn. By default both flags will be false. If there is an associated ModItem, by default this will call that ModItem's DrawHair.
		/// </summary>
		/// <param name="drawHair"></param>
		/// <param name="drawAltHair"></param>
		public virtual void DrawHair(ref bool drawHair, ref bool drawAltHair) {
			if (item != null) {
				item.DrawHair(ref drawHair, ref drawAltHair);
			}
		}

		/// <summary>
		/// Return false to hide the player's head when this head equipment texture is worn. By default this will return the associated ModItem's DrawHead, or true if there is no associated ModItem.
		/// </summary>
		/// <returns></returns>
		public virtual bool DrawHead() {
			return item == null || item.DrawHead();
		}

		/// <summary>
		/// Return false to hide the player's body when this body equipment texture is worn. By default this will return the associated ModItem's DrawBody, or true if there is no associated ModItem.
		/// </summary>
		/// <returns></returns>
		public virtual bool DrawBody() {
			return item == null || item.DrawBody();
		}

		/// <summary>
		/// Return false to hide the player's legs when this leg or shoe equipment texture is worn. By default this will return the associated ModItem's DrawLegs, or true if there is no associated ModItem.
		/// </summary>
		/// <returns></returns>
		public virtual bool DrawLegs() {
			return item == null || item.DrawLegs();
		}

		/// <summary>
		/// Allows you to modify the colors in which this armor texture and surrounding accessories are drawn, in addition to which glow mask and in what color is drawn. By default this will call the associated ModItem's DrawArmorColor if there is an associated ModItem.
		/// </summary>
		/// <param name="drawPlayer"></param>
		/// <param name="shadow"></param>
		/// <param name="color"></param>
		/// <param name="glowMask"></param>
		/// <param name="glowMaskColor"></param>
		public virtual void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
			if (item != null) {
				item.DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
			}
		}

		/// <summary>
		/// Allows you to modify which glow mask and in what color is drawn on the player's arms. Note that this is only called for body equipment textures. By default this will call the associated ModItem's ArmorArmGlowMask if there is an associated ModItem.
		/// </summary>
		/// <param name="drawPlayer"></param>
		/// <param name="shadow"></param>
		/// <param name="glowMask"></param>
		/// <param name="color"></param>
		public virtual void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
			if (item != null) {
				item.ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);
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
	ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			if (item != null) {
				item.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
					ref maxAscentMultiplier, ref constantAscend);
			}
		}

		/// <summary>
		/// Allows you to modify horizontal wing speeds.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="speed"></param>
		/// <param name="acceleration"></param>
		public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) {
			if (item != null) {
				item.HorizontalWingSpeeds(player, ref speed, ref acceleration);
			}
		}

		/// <summary>
		/// Allows for wing textures to do various things while in use. "inUse" is whether or not the jump button is currently pressed. Called when this wing texture visually appears on the player. Use to animate wings, create dusts, invoke sounds, and create lights. By default this will call the associated ModItem's WingUpdate if there is an associated ModItem.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="inUse"></param>
		/// <returns></returns>
		public virtual bool WingUpdate(Player player, bool inUse) {
			return item?.WingUpdate(player, inUse) ?? false;
		}
	}
}
