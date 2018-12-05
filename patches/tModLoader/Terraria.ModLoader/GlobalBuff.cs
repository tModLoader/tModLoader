using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to modify the behavior of any buff in the game.
	/// </summary>
	public class GlobalBuff
	{
		/// <summary>
		/// The mod to which this GlobalBuff belongs.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The internal name of this GlobalBuff instance.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// Allows you to automatically load a GlobalBuff instead of using Mod.AddGlobalBuff. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this method to either force or stop an autoload or to control the internal name.
		/// </summary>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Allows you to make the buff with the given ID give certain effects to a player. If you remove the buff from the player, make sure the decrement the buffIndex parameter by 1.
		/// </summary>
		public virtual void Update(int type, Player player, ref int buffIndex) {
		}

		/// <summary>
		/// Allows you to make the buff with the given ID give certain effects to an NPC. If you remove the buff from the NPC, make sure to decrement the buffIndex parameter by 1.
		/// </summary>
		public virtual void Update(int type, NPC npc, ref int buffIndex) {
		}

		/// <summary>
		/// Allows to you make special things happen when adding the given type of buff to a player when the player already has that buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time. (For Mana Sickness, the vanilla re-apply code adds the "time" argument to the current buff time.)
		/// </summary>
		public virtual bool ReApply(int type, Player player, int time, int buffIndex) {
			return false;
		}

		/// <summary>
		/// Allows to you make special things happen when adding the given buff type to an NPC when the NPC already has that buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time.
		/// </summary>
		public virtual bool ReApply(int type, NPC npc, int time, int buffIndex) {
			return false;
		}

		/// <summary>
		/// Allows you to modify the tooltip that displays when the mouse hovers over the buff icon, as well as the color the buff's name is drawn in.
		/// </summary>
		public virtual void ModifyBuffTip(int type, ref string tip, ref int rare) {
		}

		/// <summary>
		/// If you are using the DrawCustomBuffTip hook, then you must use this hook as well. Calculate the location (relative to the origin) of the bottom-right corner of everything you will draw, and add that location to the sizes parameter.
		/// </summary>
		public virtual void CustomBuffTipSize(string buffTip, List<Vector2> sizes) {
		}

		/// <summary>
		/// Allows you to draw whatever you want when a buff tooltip is drawn. The originX and originY parameters are the top-left corner of everything that's drawn; you should add these to the position argument passed to SpriteBatch.Draw.
		/// </summary>
		public virtual void DrawCustomBuffTip(string buffTip, SpriteBatch spriteBatch, int originX, int originY) {
		}
	}
}
