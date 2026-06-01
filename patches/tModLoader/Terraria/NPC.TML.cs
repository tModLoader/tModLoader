using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Terraria;

public partial class NPC : IEntityWithGlobals<GlobalNPC>
{
	internal readonly IEntitySource thisEntitySourceCache;

	public ModNPC ModNPC { get; internal set; }

#region Globals
	int IEntityWithGlobals<GlobalNPC>.Type => type;
	internal GlobalNPC[] _globals;
	public RefReadOnlyArray<GlobalNPC> EntityGlobals => _globals;
	public EntityGlobalsEnumerator<GlobalNPC> Globals => new(this);

	/// <summary> Gets the instance of the specified GlobalNPC type. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="IndexOutOfRangeException"/>
	public T GetGlobalNPC<T>() where T : GlobalNPC
		=> GlobalNPC.GetGlobal<T>(type, EntityGlobals);

	/// <summary> Gets the local instance of the type of the specified GlobalNPC instance. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="NullReferenceException"/>
	public T GetGlobalNPC<T>(T baseInstance) where T : GlobalNPC
		=> GlobalNPC.GetGlobal(type, EntityGlobals, baseInstance);

	/// <summary> Gets the instance of the specified GlobalNPC type. </summary>
	public bool TryGetGlobalNPC<T>(out T result) where T : GlobalNPC
		=> GlobalNPC.TryGetGlobal(type, EntityGlobals, out result);

	/// <summary> Safely attempts to get the local instance of the type of the specified GlobalNPC instance. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public bool TryGetGlobalNPC<T>(T baseInstance, out T result) where T : GlobalNPC
		=> GlobalNPC.TryGetGlobal(type, EntityGlobals, baseInstance, out result);
#endregion

	/// <summary> Provides access to (static) happiness data associated with this NPC's type. </summary>
	public NPCHappiness Happiness => NPCHappiness.Get(type);

	public bool ShowNameOnHover { get; set; }

	/// <summary>
	/// Helper property for defense >= 9999. Extremely high defense is interpreted as 'super armor' where attacks will only do 1 damage (or 2 for crits), no matter how strong they are. <br/>
	/// Passed to <see cref="HitModifiers.SuperArmor"/> when doing damage calculations. See the docs there for more info. <br/>
	/// The only way to bypass super armor is to call <see cref="StrikeNPC(HitInfo, bool, bool)"/>, or set NPC life directly.
	/// </summary>
	public bool SuperArmor {
		get => defense >= 9999;
		set => defense = value ? 9999 : 0;
	}

	/// <summary>
	/// If true, damage combat text will not be shown by <see cref="StrikeNPC(HitInfo, bool, bool)"/> and dps meter will not record damage against this NPC. <br/>
	/// Recommended for use with <see cref="NPC.immortal"/>
	/// </summary>
	public bool HideStrikeDamage { get; set; }

	/// <summary>
	/// Assign a special boss bar, vanilla or modded. Not used by vanilla.
	/// <para>To assign a modded boss bar, use NPC.BossBar = ModContent.GetInstance&lt;ExampleBossBar&gt;(); where ExampleBossBar is a ModBossBar</para>
	/// <para>To assign a vanilla boss bar for whatever reason, fetch it first through the NPC type using Main.BigBossProgressBar.TryGetSpecialVanillaBossBar</para>
	/// </summary>
	public IBigProgressBar BossBar { get; set; }

	private bool catchableNPCOriginallyFriendly; // TML: Fix #3299, Allow npcCatchable to work with friendly npc.

	public NPC()
	{
		thisEntitySourceCache = new EntitySource_Parent(this);
	}

	/// <summary> Returns whether or not this NPC currently has a (de)buff of the provided type. </summary>
	public bool HasBuff(int type) => FindBuffIndex(type) != -1;

	/// <inheritdoc cref="HasBuff(int)" />
	public bool HasBuff<T>() where T : ModBuff
		=> HasBuff(ModContent.BuffType<T>());

	/// <summary>
	/// <inheritdoc cref="NPC.NewNPC(IEntitySource, int, int, int, int, float, float, float, float, int)"/>
	/// <br/><br/>This particular overload returns the actual NPC instance rather than the index of the spawned NPC within the <see cref="Main.npc"/> array.
	/// <br/> A short-hand for <code> Main.npc[NPC.NewNPC(...)] </code>
	/// </summary>
	public static NPC NewNPCDirect(IEntitySource source, int x, int y, int type, int start = 0, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f, int target = 255)
		=> Main.npc[NewNPC(source, x, y, type, start, ai0, ai1, ai2, ai3, target)];

	/// <summary>
	/// <inheritdoc cref="NPC.NewNPC(IEntitySource, int, int, int, int, float, float, float, float, int)"/>
	/// <br/><br/>This particular overload returns the actual NPC instance rather than the index of the spawned NPC within the <see cref="Main.npc"/> array. It also uses a Vector2 for the spawn position instead of X and Y.
	/// <br/> A short-hand for <code> Main.npc[NPC.NewNPC(...)] </code>
	/// </summary>
	public static NPC NewNPCDirect(IEntitySource source, Vector2 position, int type, int start = 0, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f, int target = 255)
		=> NewNPCDirect(source, (int)position.X, (int)position.Y, type, start, ai0, ai1, ai2, ai3, target);

	/// <summary>
	/// Helper method for getting the parameters for seating a town NPC. Assumes the tile at <paramref name="anchorTilePosition"/> is a valid tile for sitting
	/// </summary>
	public void SitDown(Point anchorTilePosition, out int direction, out Vector2 bottom)
	{
		Tile tile = Main.tile[anchorTilePosition.X, anchorTilePosition.Y];
		if (tile.type < TileID.Count)
			anchorTilePosition.Y -= 1; // Vanilla compatibility with new hook

		TileRestingInfo info = new TileRestingInfo(this, anchorTilePosition, Vector2.Zero, ((tile.frameX != 0) ? 1 : (-1)), 2);
		TileLoader.ModifySittingTargetInfo(anchorTilePosition.X, anchorTilePosition.Y, tile.type, ref info);
		int anchorX = info.AnchorTilePosition.X;
		int anchorY = info.AnchorTilePosition.Y;
		int directionOffset = info.DirectionOffset;
		direction = info.TargetDirection;
		Vector2 finalOffset = info.FinalOffset;

		bottom = new Point(anchorX, anchorY).ToWorldCoordinates(8f, 16f);
		bottom.X += direction * directionOffset; // Added to match PlayerSittingHelper
		bottom += finalOffset; // Added to match PlayerSittingHelper
	}

	/// <summary>
	/// Runs most code related to the process of checking whether or not an NPC can be caught.<br></br>
	/// After that, <see cref="CombinedHooks.OnCatchNPC"/> is run, followed by the code responsible for catching the NPC if applicable.<br></br>
	/// You will need to call this manually if you want to make an NPC-catching tool which acts differently from vanilla's, such as one that uses a projectile instead of an item.<br></br>
	/// As a note, if calling this manually, you will need to check <c>npc.active &amp;&amp; npc.catchItem &gt; 0</c> yourself.
	/// </summary>
	/// <param name="npc">The NPC which can potentially be caught.</param>
	/// <param name="catchToolRectangle">The hitbox of the tool being used to catch the NPC --- be it an item, a projectile, or something else entirely.</param>
	/// <param name="item">The item to be used as a reference for the purposes of <see cref="CombinedHooks.CanCatchNPC"/> and <see cref="CombinedHooks.OnCatchNPC"/>.</param>
	/// <param name="player">The player that owns the referenced item.</param>
	/// <param name="lavaProofTool">Whether or not the tool is lavaproof for the purposes of catching vanilla's Underworld critters. Defaults to false.</param>
	/// <returns>Whether or not the NPC was successfully caught.</returns>
	public static bool CheckCatchNPC(NPC npc, Rectangle catchToolRectangle, Item item, Player player, bool lavaProofTool = false)
	{
		Rectangle value = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
		if (!catchToolRectangle.Intersects(value))
			return false;

		bool? canCatch = CombinedHooks.CanCatchNPC(player, npc, item);
		if (canCatch.HasValue) {
			CombinedHooks.OnCatchNPC(player, npc, item, !canCatch.Value);
			if (canCatch.Value)
				CatchNPC(npc.whoAmI, player.whoAmI);

			return canCatch.Value;
		}

		if (!lavaProofTool && ItemID.Sets.IsLavaBait[npc.catchItem]) {
			CombinedHooks.OnCatchNPC(player, npc, item, failed: true);
			if (Main.myPlayer == player.whoAmI && player.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), 1, (npc.Center.X < player.Center.X) ? 1 : -1, cooldownCounter: 3) > 0.0 && !player.dead)
				player.AddBuff(24, 300);

			return false;
		}

		if (npc.type is NPCID.FairyCritterBlue or NPCID.FairyCritterPink or NPCID.FairyCritterGreen) {
			bool canCatchFairy = npc.ai[2] <= 1f;
			CombinedHooks.OnCatchNPC(player, npc, item, !canCatchFairy);
			if (canCatchFairy)
				CatchNPC(npc.whoAmI, player.whoAmI);
			return canCatchFairy;
		}

		CombinedHooks.OnCatchNPC(player, npc, item, failed: false);
		CatchNPC(npc.whoAmI, player.whoAmI);
		return true;
	}

	/// <summary>
	/// Returns the gore type of the party hat this NPC is currently wearing. If the NPC isn't wearing a party hat, 0 is returned.
	/// </summary>
	/// <returns></returns>
	public int GetPartyHatGore()
	{
		int num = 926;
		switch (GetPartyHatColor()) {
			case PartyHatColor.Cyan:
				num = 940;
				break;
			case PartyHatColor.Pink:
				num = 939;
				break;
			case PartyHatColor.Purple:
				num = 941;
				break;
			case PartyHatColor.White:
				num = 942;
				break;
			case PartyHatColor.None: // TML: added so method can be used without checking UsesPartyHat first
				num = 0;
				break;
		}
		return num;
	}

	/// <summary>
	/// Used to keep vanilla and modded gravity effects working neatly
	/// </summary>
	private float vanillaGravity = 0.3f;

	/// <summary>
	/// Multiply this value in order to change the NPCs active gravity, this can be done in AI as gravity values are reset slightly beforehand, and used slightly after.
	/// </summary>
	public MultipliableFloat GravityMultiplier = MultipliableFloat.One;

	/// <summary>
	/// The current change in velocity due to gravity applied every frame. <br/>
	/// Multiply <see cref="GravityMultiplier"/> to modify this value
	/// </summary>
	public float gravity {
		get => vanillaGravity * GravityMultiplier.Value;
		private set {
			GravityMultiplier = MultipliableFloat.One;
			vanillaGravity = value;
		}
	}

	/// <summary>
	/// Used to keep vanilla and modded gravity effects working neatly
	/// </summary>
	private float vanillaMaxFallSpeed = 10f;

	/// <summary>
	/// Multiply this value in order to change the NPCs active maxFallSpeed, this can be done in AI as gravity values are reset slightly beforehand
	/// </summary>
	public MultipliableFloat MaxFallSpeedMultiplier = MultipliableFloat.One;

	/// <summary>
	/// The current fall speed cap in velocity applied every frame. <br/>
	/// Multiply <see cref="MaxFallSpeedMultiplier"/> to modify this value
	/// </summary>
	public float maxFallSpeed {
		get => vanillaMaxFallSpeed * MaxFallSpeedMultiplier.Value;
		private set {
			MaxFallSpeedMultiplier = MultipliableFloat.One;
			vanillaMaxFallSpeed = value;
		}
	}

	/// <summary>
	/// The effect of different liquids on NPC gravity. Provided for reference only, modifying these will have no effect. <br/>
	/// Corresponds with wet, lavaWet, honetWet, and shimmerWet.
	///	</summary>
	public static float[] GravityWetMultipliers { get; } = new float[4] { 2f / 3f, 2f / 3f, 1f / 3f, 1.5f / 3f };

	/// <summary>
	/// The effect of different liquids on NPC maxFallSpeed. Provided for reference only, modifying these will have no effect. <br/>
	/// Corresponds with wet, lavaWet, honetWet, and shimmerWet.
	///	</summary>
	public static float[] MaxFallSpeedWetMultipliers { get; } = new float[4] { 0.7f, 0.7f, 0.4f, 0.55f };

	/// <summary>
	/// Set to disable vanilla type and AI based NPC gravity calculations. <br/>
	/// Affects types 258, 425, 576, 577, 427, 426, 541, and the aiStyle 7. <br/>
	/// Use with caution
	/// </summary>
	public bool GravityIgnoresType = false;

	/// <summary>
	/// Set to disable the effect of being in space on NPC gravity.
	/// </summary>
	public bool GravityIgnoresSpace = false;

	/// <summary>
	/// Set to disable the effect of being submerged in liquid on NPC gravity. <br/>
	/// Note that being submerged in liquid overrides both type and space effects.
	/// </summary>
	public bool GravityIgnoresLiquid = false;

	/// <summary>
	/// Adjusts <see cref="buffImmune"/> to make this NPC immune to the provided buff as well as all other buffs that inherit the immunity of that buff (via <see cref="BuffID.Sets.GrantImmunityWith"/>). This method can be followed by <see cref="ClearImmuneToBuffs(out bool)"/> if the NPC should clear any buff it currently has that it is now immune to.
	/// </summary>
	/// <param name="buffType"></param>
	public void BecomeImmuneTo(int buffType)
	{
		buffImmune[buffType] = true;

		for (int i = 0; i < BuffID.Sets.GrantImmunityWith.Length; i++) {
			var buffsToInherit = BuffID.Sets.GrantImmunityWith[i];
			// This could be sped up with a reverse lookup if this proves too slow.
			if (buffsToInherit.Contains(buffType)) {
				buffImmune[i] = true;
			}
		}
	}

	/// <summary>
	/// Clears all buffs on this NPC that the NPC is currently immune (<see cref="buffImmune"/>) to. The buff types and times will then be synced to clients. Use after manually changing <see cref="buffImmune"/> or using <see cref="BecomeImmuneTo(int)"/>.<br/><br/>
	/// <paramref name="anyBuffsCleared"/> will be true if any buffs have been cleared by this method, it can be used to decide to spawn visual effects. Since this method should not be called on multiplayer clients, modders will need to manually sync any visual effects of this. <br/><br/>
	/// This should not be called on multiplayer clients.
	/// </summary>
	/// <param name="anyBuffsCleared"></param>
	public void ClearImmuneToBuffs(out bool anyBuffsCleared)
	{
		anyBuffsCleared = false;

		// Modelled after DelBuff method.
		for (int i = maxBuffs - 1; i >= 0; i--) {
			if (buffImmune[buffType[i]]) {
				buffTime[i] = 0;
				buffType[i] = 0;
				anyBuffsCleared = true;
			}

			if (buffTime[i] == 0 || buffType[i] == 0) {
				for (int j = i + 1; j < maxBuffs; j++) {
					buffTime[j - 1] = buffTime[j];
					buffType[j - 1] = buffType[j];
					buffTime[j] = 0;
					buffType[j] = 0;
				}
			}
		}

		if (Main.netMode == 2)
			NetMessage.SendData(54, -1, -1, null, whoAmI);
	}
}
