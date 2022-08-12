using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Terraria.ModLoader
{
	public abstract class ArmorSet : ModType
	{
		/// <summary>
		/// The numerical IDs of the equipment needed for the set bonus to take effect.<br/>
		/// Can store the IDs for a head piece, a body piece, and a leg piece.<br/>
		/// </summary>
		public abstract (int? head, int? body, int? legs) RequiredEquipment { get; }

		/// <summary>
		/// The text displayed to describe what this set bonus does.<br/>
		/// </summary>
		/// <param name="color">
		/// Allows the specification of a color in which to draw the description for this set bonus.<br/>
		/// </param>
		public abstract string SetBonusDescription(ref Color? color);

		/// <summary>
		/// The method used to apply FUNCTIONAL set effects.<br/>
		/// </summary>
		/// <param name="player">
		/// The player to apply these functional set effects to.<br/>
		/// </param>
		public abstract void ApplyFunctionalEffects(Player player);

		/// <summary>
		/// The method used to apply VANITY set effects.<br/>
		/// </summary>
		/// <param name="player">
		/// The player to apply these vanity set effects to.<br/>
		/// </param>
		public abstract void ApplyVanityEffects(Player player);

		/// <summary>
		/// Checks whether or not the player has the given armor set equipped in functional slots.<br/>
		/// By default, checks for all pieces included in the set; if at least one is missing, the functional effects of this set are not applied.<br/>
		/// Override this only if you need to make your own active check for this armor set's functional effects.<br/>
		/// </summary>
		/// <param name="player">
		/// The player to check set bonus validity for.<br/>
		/// </param>
		/// <returns>
		/// If the conditions to apply the functional armor set bonus are met, <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
		public virtual bool ActiveFunctional(Player player) {
			int? head = RequiredEquipment.head;
			int? body = RequiredEquipment.body;
			int? legs = RequiredEquipment.legs;
			if (head.HasValue) {
				if (player.armor[0].type != head.Value)
					return false;
			}
			if (body.HasValue) {
				if (player.armor[1].type != body.Value)
					return false;
			}
			if (legs.HasValue) {
				if (player.armor[2].type != legs.Value)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Checks whether or not the player has the given armor set equipped in vanity slots.<br/>
		/// By default, checks for all pieces included in the set; if at least one is missing, the vanity effects of this set are not applied.<br/>
		/// Override this only if you need to make your own active check for this armor set's vanity effects.<br/>
		/// </summary>
		/// <param name="player">
		/// The player to check set bonus validity for.<br/>
		/// </param>
		/// <returns>
		/// If the conditions to apply the visual armor set bonus are met, <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
		public virtual bool ActiveVanity(Player player) {
			int? head = RequiredEquipment.head;
			int? body = RequiredEquipment.body;
			int? legs = RequiredEquipment.legs;
			if (head.HasValue) {
				if (!player.armor[10].IsAir && player.armor[10].type != head.Value)
					return false;
			}
			if (body.HasValue) {
				if (!player.armor[11].IsAir && player.armor[11].type != body.Value)
					return false;
			}
			if (legs.HasValue) {
				if (!player.armor[12].IsAir && player.armor[12].type != legs.Value)
					return false;
			}
			return true;
		}

		protected sealed override void Register() {
			ModTypeLookup<ArmorSet>.Register(this);

			ArmorSetLoader.RegisterArmorSet(this);
		}
	}
}
