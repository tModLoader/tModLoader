using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// This class serves as a place for you to define a new buff and how that buff behaves.
/// </summary>
public abstract class ModBuff : ModTexturedType, ILocalizedModType
{
	/// <summary> The buff id of this buff. </summary>
	public int Type { get; internal set; }

	public virtual string LocalizationCategory => "Buffs";

	/// <summary> The translations of this buff's display name. </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary> The translations of this buff's description. </summary>
	public virtual LocalizedText Description => this.GetLocalization(nameof(Description));

	protected override sealed void Register()
	{
		ModTypeLookup<ModBuff>.Register(this);
		Type = BuffLoader.ReserveBuffID();
		BuffLoader.buffs.Add(this);
	}

	public sealed override void SetupContent()
	{
		TextureAssets.Buff[Type] = ModContent.Request<Texture2D>(Texture);

		SetStaticDefaults();

		BuffID.Search.Add(FullName, Type);
	}

	/// <summary>
	/// Allows you to modify the properties after initial loading has completed.
	/// <br/> This is where all buff related assignments go.
	/// <br/> For example:
	/// <list type="bullet">
	/// <item> Main.debuff[Type] = true; </item>
	/// <item> Main.buffNoTimeDisplay[Type] = true; </item>
	/// <item> Main.pvpBuff[Type] = true; </item>
	/// <item> Main.vanityPet[Type] = true; </item>
	/// <item> Main.lightPet[Type] = true; </item>
	/// </list>
	/// </summary>
	public override void SetStaticDefaults() { }

	/// <summary>
	/// Allows you to make this buff give certain effects to the given player. If you remove the buff from the player, make sure the decrement the buffIndex parameter by 1.
	/// </summary>
	/// <param name="player">The player to update this buff on.</param>
	/// <param name="buffIndex">The index in <see cref="Player.buffType"/> and <see cref="Player.buffType"/> of this buff. For use with <see cref="Player.DelBuff(int)"/>.</param>
	public virtual void Update(Player player, ref int buffIndex)
	{
	}

	/// <summary>
	/// Allows you to make this buff give certain effects to the given NPC. If you remove the buff from the NPC, make sure to decrement the buffIndex parameter by 1.
	/// </summary>
	public virtual void Update(NPC npc, ref int buffIndex)
	{
	}

	/// <summary>
	/// Allows to you make special things happen when adding this buff to a player when the player already has this buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time.
	/// </summary>
	public virtual bool ReApply(Player player, int time, int buffIndex)
	{
		return false;
	}

	/// <summary>
	/// Allows to you make special things happen when adding this buff to an NPC when the NPC already has this buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time.
	/// </summary>
	public virtual bool ReApply(NPC npc, int time, int buffIndex)
	{
		return false;
	}

	/// <summary>
	/// Allows you to modify the name and tooltip that displays when the mouse hovers over the buff icon, as well as the color the buff's name is drawn in.
	/// </summary>
	public virtual void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
	}

	/// <summary>
	/// Allows you to draw things before the default draw code is ran. Return false to prevent drawing the buff. Returns true by default.
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="buffIndex">The index in Main.LocalPlayer.buffType and .buffTime of the buff</param>
	/// <param name="drawParams">The draw parameters for the buff</param>
	/// <returns><see langword="true"/> for allowing drawing, <see langword="false"/> for preventing drawing</returns>
	public virtual bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things after the buff has been drawn. skipped is true if you or another mod has skipped drawing the buff (possibly hiding it or in favor of new visuals).
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="buffIndex">The index in Main.LocalPlayer.buffType and .buffTime of the buff</param>
	/// <param name="drawParams">The draw parameters for the buff</param>
	public virtual void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
	}

	/// <summary>
	/// Allows you to make things happen when the buff icon is right-clicked. Return false to prevent the buff from being cancelled.
	/// </summary>
	/// <param name="buffIndex">The index in Main.LocalPlayer.buffType and .buffTime of the buff</param>
	/// <returns><see langword="true"/> for allowing the buff to be cancelled, <see langword="false"/> to prevent the buff from being cancelled</returns>
	public virtual bool RightClick(int buffIndex)
	{
		return true;
	}
}
