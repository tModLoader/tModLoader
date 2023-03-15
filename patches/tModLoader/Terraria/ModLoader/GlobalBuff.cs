using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to modify the behavior of any buff in the game.
/// </summary>
public abstract class GlobalBuff : ModType
{
	protected sealed override void Register()
	{
		ModTypeLookup<GlobalBuff>.Register(this);
		BuffLoader.globalBuffs.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to make the buff with the given ID give certain effects to a player. If you remove the buff from the player, make sure the decrement the buffIndex parameter by 1.
	/// </summary>
	public virtual void Update(int type, Player player, ref int buffIndex)
	{
	}

	/// <summary>
	/// Allows you to make the buff with the given ID give certain effects to an NPC. If you remove the buff from the NPC, make sure to decrement the buffIndex parameter by 1.
	/// </summary>
	public virtual void Update(int type, NPC npc, ref int buffIndex)
	{
	}

	/// <summary>
	/// Allows to you make special things happen when adding the given type of buff to a player when the player already has that buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time. (For Mana Sickness, the vanilla re-apply code adds the "time" argument to the current buff time.)
	/// </summary>
	public virtual bool ReApply(int type, Player player, int time, int buffIndex)
	{
		return false;
	}

	/// <summary>
	/// Allows to you make special things happen when adding the given buff type to an NPC when the NPC already has that buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time.
	/// </summary>
	public virtual bool ReApply(int type, NPC npc, int time, int buffIndex)
	{
		return false;
	}

	/// <summary>
	/// Allows you to modify the name and tooltip that displays when the mouse hovers over the buff icon, as well as the color the buff's name is drawn in.
	/// </summary>
	public virtual void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
	{
	}

	/// <summary>
	/// If you are using the DrawCustomBuffTip hook, then you must use this hook as well. Calculate the location (relative to the origin) of the bottom-right corner of everything you will draw, and add that location to the sizes parameter.
	/// </summary>
	public virtual void CustomBuffTipSize(string buffTip, List<Vector2> sizes)
	{
	}

	/// <summary>
	/// Allows you to draw whatever you want when a buff tooltip is drawn. The originX and originY parameters are the top-left corner of everything that's drawn; you should add these to the position argument passed to SpriteBatch.Draw.
	/// </summary>
	public virtual void DrawCustomBuffTip(string buffTip, SpriteBatch spriteBatch, int originX, int originY)
	{
	}

	/// <summary>
	/// Allows you to draw things before the default draw code is ran. Return false to prevent drawing the buff. Returns true by default.
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="type">The buff type</param>
	/// <param name="buffIndex">The index in Main.LocalPlayer.buffType and .buffTime of the buff</param>
	/// <param name="drawParams">The draw parameters for the buff</param>
	/// <returns><see langword="true"/> for allowing drawing, <see langword="false"/> for preventing drawing</returns>
	public virtual bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things after the buff has been drawn. skipped is true if you or another mod has skipped drawing the buff (possibly hiding it or in favor of new visuals).
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="type">The buff type</param>
	/// <param name="buffIndex">The index in Main.LocalPlayer.buffType and .buffTime of the buff</param>
	/// <param name="drawParams">The draw parameters for the buff</param>
	public virtual void PostDraw(SpriteBatch spriteBatch, int type, int buffIndex, BuffDrawParams drawParams)
	{
	}

	/// <summary>
	/// Allows you to make things happen when the buff icon is right-clicked. Return false to prevent the buff from being cancelled.
	/// </summary>
	/// <param name="type">The buff type</param>
	/// <param name="buffIndex">The index in Main.LocalPlayer.buffType and .buffTime of the buff</param>
	/// <returns><see langword="true"/> for allowing the buff to be cancelled, <see langword="false"/> to prevent the buff from being cancelled</returns>
	public virtual bool RightClick(int type, int buffIndex)
	{
		return true;
	}
}
