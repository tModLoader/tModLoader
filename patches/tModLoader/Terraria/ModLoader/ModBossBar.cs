using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;

namespace Terraria.ModLoader;

/// <summary>
/// A class that is used to create custom boss health bars for modded and vanilla NPCs.
/// </summary>
public abstract class ModBossBar : ModTexturedType, IBigProgressBar
{
	internal int index;

	private float life;
	public float Life => life;

	private float lifeMax;
	public float LifeMax => lifeMax;

	private float shield;
	public float Shield => shield;

	private float shieldMax;
	public float ShieldMax => shieldMax;

	protected sealed override void Register() => BossBarLoader.AddBossBar(this);

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to specify the icon texture, and optionally the frame it should be displayed on. The frame defaults to the entire texture otherwise.
	/// </summary>
	/// <param name="iconFrame">The frame the texture should be displayed on</param>
	/// <returns>The icon texture</returns>
	public virtual Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) => null;

	/// <summary>
	/// Allows you to handle the logic for when and how this ModBossBar should work. You want to override this if you have a multi-segment NPC. Returns null by default. Failing to return false otherwise will lead to your bar being displayed at wrong times.
	/// <para>Return null to let the basic logic run after this hook is called (index validity check and assigning lifePercent to match the health of the NPC) and then allowing it to be drawn.</para>
	/// <para>Return true to allow this ModBossBar to be drawn.</para>
	/// <para>Return false to prevent this ModBossBar from being drawn so that the game will try to pick a different one.</para>
	/// </summary>
	/// <param name="info">Contains the index of the NPC the game decided to focus on</param>
	/// <param name="life">The current life of the boss</param>
	/// <param name="lifeMax">The max (initial) life of the boss</param>
	/// <param name="shield">The current shield of the boss</param>
	/// <param name="shieldMax">The max shield for the boss (may be 0 if the boss has no shield)</param>
	/// <returns><see langword="null"/> for "single-segment" NPC logic, <see langword="true"/> for allowing drawing, <see langword="false"/> for preventing drawing</returns>
	public virtual bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) => null;

	/// <summary>
	/// Allows you to draw things before the default draw code is ran. Return false to prevent drawing the ModBossBar. Returns true by default.
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="npc">The NPC this ModBossBar is focused on</param>
	/// <param name="drawParams">The draw parameters for the boss bar</param>
	/// <returns><see langword="true"/> for allowing drawing, <see langword="false"/> for preventing drawing</returns>
	public virtual bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) => true;

	/// <summary>
	/// Allows you to draw things after the bar has been drawn. skipped is true if you or another mod has skipped drawing the bar in PreDraw (possibly hiding it or in favor of new visuals).
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="npc">The NPC this ModBossBar is focused on</param>
	/// <param name="drawParams">The draw parameters for the boss bar</param>
	public virtual void PostDraw(SpriteBatch spriteBatch, NPC npc, BossBarDrawParams drawParams)
	{
	}

	public bool ValidateAndCollectNecessaryInfo(ref BigProgressBarInfo info)
	{
		if (info.npcIndexToAimAt < 0 || info.npcIndexToAimAt > Main.maxNPCs)
			return false;

		//No GlobalBossBar hook for this as info should not be modified from the outside
		bool? modify = ModifyInfo(ref info, ref life, ref lifeMax, ref shield, ref shieldMax);
		if (!modify.HasValue) {
			NPC npc = Main.npc[info.npcIndexToAimAt];

			if (!npc.active)
				return false;

			life = Utils.Clamp(npc.life, 0f, npc.lifeMax);
			lifeMax = npc.lifeMax;

			return true;
		}
		return modify.Value;
	}

	public void Draw(ref BigProgressBarInfo info, SpriteBatch spriteBatch)
	{
		//Questionmark icon as fallback
		Rectangle? iconFrame = null;
		Texture2D iconTexture = (GetIconTexture(ref iconFrame) ?? TextureAssets.NpcHead[0]).Value;
		iconFrame ??= iconTexture.Frame();

		//TML handles modifying draw parameters inside of it
		BigProgressBarHelper.DrawFancyBar(spriteBatch, life, lifeMax, iconTexture, iconFrame.Value, shield, shieldMax);
	}
}
