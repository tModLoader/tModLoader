using Terraria.ModLoader;

namespace Terraria.ID;

public partial class NPCID
{
	public static partial class Sets
	{
		public partial struct NPCBestiaryDrawModifiers
		{
			/// <inheritdoc cref="NPCBestiaryDrawModifiers(int)"/>
#pragma warning disable CS0618
			public NPCBestiaryDrawModifiers() : this(0) { }
#pragma warning restore CS0618
		}

		//IDs taken from start of NPC.NewNPC when determining num
		/// <summary>
		/// Whether or not the spawned NPC will start looking for a suitable slot from the end of <seealso cref="Main.npc"/>, ignoring the Start parameter of <see cref="NPC.NewNPC"/>.
		/// Useful if you have a multi-segmented boss and want its parts to draw over the main body (body will be in this set).
		/// </summary>
		public static bool[] SpawnFromLastEmptySlot = Factory.CreateBoolSet(222, 245);

		/// <summary>
		/// If true, the given Town NPC won't drop a tombstone on death in hardcore mode and will have the "NPC has left!" death message unless specified otherwise by <see cref="ModNPC.DeathMessage"/>.
		/// <br/> This does NOT affect the gore spawned when the NPC dies.
		/// <para/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] IsTownChild = Factory.CreateBoolSet(Angler, Princess);

		//Default ID is the skeleton merchant
		/// <summary>
		/// Whether or not a given NPC will act like a town NPC in terms of AI, animations, and attacks, but not in other regards, such as appearing on the minimap, like the bone merchant in vanilla.
		/// </summary>
		public static bool[] ActsLikeTownNPC = Factory.CreateBoolSet(453);

		//Default ID is the skeleton merchant, traveling merchant, old man
		/// <summary>
		/// If true, the given NPC will not count towards town NPC happiness and won't have a happiness button. Pets (<see cref="NPCID.Sets.IsTownPet"/>) do not need to set this.
		/// </summary>
		public static bool[] NoTownNPCHappiness = Factory.CreateBoolSet(37, 368, 453);

		//Default ID is the skeleton merchant
		/// <summary>
		/// Whether or not a given NPC will spawn with a custom name like a town NPC. In order to determine what name will be selected, override the TownNPCName hook.
		/// True will force a name to be rolled regardless of vanilla behavior. False will have vanilla handle the naming.
		/// </summary>
		public static bool[] SpawnsWithCustomName = Factory.CreateBoolSet(453);

		// IDs taken from NPC.AI_007_TryForcingSitting
		/// <summary>
		/// Whether or not a given NPC can sit on suitable furniture (<see cref="TileID.Sets.CanBeSatOnForNPCs"/>)
		/// </summary>
		public static bool[] CannotSitOnFurniture = Factory.CreateBoolSet(638, 656);

		// IDs taken from Conditions.SoulOfWhateverConditionCanDrop
		/// <summary>
		/// Whether or not a given NPC is excluded from dropping hardmode souls (Soul of Night/Light)
		/// <br/>Contains vanilla NPCs that are easy to spawn in large numbers, preventing easy soul farming
		/// <br/>Do not add your NPC to this if it would be excluded automatically (i.e. critter, town NPC, or no coin drops)
		/// </summary>
		public static bool[] CannotDropSouls = Factory.CreateBoolSet(1, 13, 14, 15, 121, 535);

		//No Default IDs, as there is no vanilla precedent for this functionality
		/// <summary>
		/// Whether or not this NPC can still interact with doors if they use the Vanilla TownNPC aiStyle (AKA aiStyle == 7)
		/// but are not actually marked as Town NPCs (AKA npc.townNPC == true).
		/// </summary>
		/// <remarks>
		/// Note: This set DOES NOT DO ANYTHING if your NPC doesn't use the Vanilla TownNPC aiStyle (aiStyle == 7).
		/// </remarks>
		public static bool[] AllowDoorInteraction = Factory.CreateBoolSet();

		/// <summary>
		/// If <see langword="true"/>, this NPC type (<see cref="NPC.type"/>) will be immune to all debuffs and "tag" buffs by default.<br/><br/>
		/// Use this for special NPCs that cannot be hit at all, such as fairy critters, container NPCs like Martian Saucer and Pirate Ship, bound town slimes, and Blazing Wheel. Dungeon Guardian also is in this set to prevent the bonus damage from "tag" buffs.<br/><br/>
		/// If the NPC should be attacked, it is recommended to set <see cref="ImmuneToRegularBuffs"/> to <see langword="true"/> instead. This will prevent all debuffs except "tag" buffs (<see cref="BuffID.Sets.IsATagBuff"/>), which are intended to affect enemies typically seen as immune to all debuffs. Tag debuffs are special debuffs that facilitate combat mechanics, they are not something that adversely affects NPC.<br/><br/>
		/// Modders can specify specific buffs to be vulnerable to by assigning <see cref="SpecificDebuffImmunity"/> to false.
		/// </summary>
		public static bool[] ImmuneToAllBuffs; // derived from DebuffImmunitySets

		/// <summary>
		/// If <see langword="true"/>, this NPC type (<see cref="NPC.type"/>) will be immune to all debuffs except tag debuffs (<see cref="BuffID.Sets.IsATagBuff"/>) by default.<br/><br/>
		/// Use this for NPCs that can be attacked that should be immune to all normal debuffs. Tag debuffs are special debuffs that facilitate combat mechanics, such as the "summon tag damage" applied by whip weapons. Wraith, Reaper, Lunatic Cultist, the Celestial Pillars, The Destroyer, and the Martian Saucer Turret/Cannon/Core are examples of NPCs that use this setting.<br/><br/>
		/// Modders can specify specific buffs to be vulnerable to by assigning <see cref="SpecificDebuffImmunity"/> to false.
		/// </summary>
		public static bool[] ImmuneToRegularBuffs; // derived from DebuffImmunitySets

		/// <summary>
		/// Indexed by NPC type and then Buff type. If <see langword="true"/>, this NPC type (<see cref="NPC.type"/>) will be immune (<see cref="NPC.buffImmune"/>) to the specified buff type. If <see langword="false"/>, the NPC will not be immune.<br/><br/>
		/// By default, NPCs aren't immune to any buffs, but <see cref="ImmuneToRegularBuffs"/> or <see cref="ImmuneToAllBuffs"/> can make an NPC immune to all buffs. The values in this set override those settings.<br/><br/>
		/// Additionally, the effects of <see cref="BuffID.Sets.GrantImmunityWith"/> will also be applied. Inherited buff immunities do not need to be specifically assigned, as they will be automatically applied. Setting an inherited debuff to false in this set can be used to undo the effects of <see cref="BuffID.Sets.GrantImmunityWith"/>, if needed.<br/><br/>
		/// Defaults to <see langword="null"/>, indicating no immunity override.<br/>
		/// </summary>
		public static bool?[][] SpecificDebuffImmunity; // derived from DebuffImmunitySets

		static Sets()
		{
			ImmuneToAllBuffs = Factory.CreateBoolSet();
			ImmuneToRegularBuffs  = Factory.CreateBoolSet();
			SpecificDebuffImmunity = Factory.CreateCustomSet<bool?[]>(null);
			for (int type = 0; type < NPCLoader.NPCCount; type++) {
				SpecificDebuffImmunity[type] = new bool?[BuffLoader.BuffCount];
				if (DebuffImmunitySets.TryGetValue(type, out var data) && data != null) {
					ImmuneToAllBuffs[type] = data.ImmuneToAllBuffsThatAreNotWhips && data.ImmuneToWhips;
					ImmuneToRegularBuffs[type] = data.ImmuneToAllBuffsThatAreNotWhips;				
					if (data.SpecificallyImmuneTo != null) {
						foreach (var buff in data.SpecificallyImmuneTo) {
							SpecificDebuffImmunity[type][buff] = true;
						}
					}
				}
				SpecificDebuffImmunity[type][BuffID.Shimmer] = ShimmerImmunity[type];
			}
		}
		
		// All BelongsToInvasion set IDs taken from NPC.GetNPCInvasionGroup
		/// <summary>
		/// If <see langword="true"/> for a given NPC type (<see cref="NPC.type"/>), then that NPC belongs to the Goblin Army invasion.
		/// <br/> During the Goblin Army invasion, NPCs in this set will decrement <see cref="Main.invasionSize"/> by the amount specified in <see cref="InvasionSlotCount"/> when killed.
		/// <br/> If any NPC in this set is alive and <see cref="InvasionSlotCount"/> is above 0, the Goblin Army music will play.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] BelongsToInvasionGoblinArmy = Factory.CreateBoolSet(26, 27, 28, 29, 111, 471, 472);

		/// <summary>
		/// If <see langword="true"/> for a given NPC type (<see cref="NPC.type"/>), then that NPC belongs to the Frost Legion invasion.
		/// <br/> During the Frost Legion invasion, NPCs in this set will decrement <see cref="Main.invasionSize"/> by the amount specified in <see cref="InvasionSlotCount"/> when killed.
		/// <br/> If any NPC in this set is alive and <see cref="InvasionSlotCount"/> is above 0, the Boss 3 music will play.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] BelongsToInvasionFrostLegion = Factory.CreateBoolSet(143, 144, 145);

		/// <summary>
		/// If <see langword="true"/> for a given NPC type (<see cref="NPC.type"/>), then that NPC belongs to the Pirate invasion.
		/// <br/> During the Pirate invasion, NPCs in this set will decrement <see cref="Main.invasionSize"/> by the amount specified in <see cref="InvasionSlotCount"/> when killed.
		/// <br/> If any NPC in this set is alive and <see cref="InvasionSlotCount"/> is above 0, the Pirate Invasion music will play.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] BelongsToInvasionPirate = Factory.CreateBoolSet(212, 213, 214, 215, 216, 491);

		/// <summary>
		/// If <see langword="true"/> for a given NPC type (<see cref="NPC.type"/>), then that NPC belongs to the Martian Madness invasion.
		/// <br/> During the Martian Madness invasion, NPCs in this set will decrement <see cref="Main.invasionSize"/> by the amount specified in <see cref="InvasionSlotCount"/> when killed.
		/// <br/> If any NPC in this set is alive and <see cref="InvasionSlotCount"/> is above 0, the Martian Madness music will play.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] BelongsToInvasionMartianMadness = Factory.CreateBoolSet(381, 382, 383, 385, 386, 387, 388, 389, 390, 391, 395, 520);

		// IDs taken from Main.UpdateAudio_DecideOnNewMusic, only if it doesn't appear in any BelongsToInvasion set
		/// <summary>
		/// If <see langword="true"/> for a given NPC type (<see cref="NPC.type"/>), then that NPC will not play its associated invasion music.
		/// <br/> By default, alive NPCs in any BelongsToInvasion set will automatically play the associated invasion music if <see cref="InvasionSlotCount"/> is above 0.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] NoInvasionMusic = Factory.CreateBoolSet(387);

		// IDs taken from NPC.checkDead
		/// <summary>
		/// If above 0 for a given NPC type (<see cref="NPC.type"/>), and its associated invasion is NOT a wave-based one, then that NPC will decrement <see cref="Main.invasionSize"/> by that amount when killed.
		/// <br/> If this NPC's entry is 0, it won't play its associated invasion's music when alive.
		/// </summary>
		/// <remarks>
		///	Note: Even though this defaults to 1, this set should only be checked if <see cref="NPC.GetNPCInvasionGroup(int)"/> is above 0 or if any BelongsToInvasion sets are <see langword="true"/>.
		/// </remarks>
		public static int[] InvasionSlotCount = Factory.CreateIntSet(1, 216, 5, 395, 10, 491, 10, 471, 10, 472, 0, 387, 0);

		// IDs taken from Player.GetPettingInfo
		/// <summary>
		/// While petting, the number of pixels away the player stands from the NPC. Defaults to 36 pixels.
		/// </summary>
		public static int[] PlayerDistanceWhilePetting = Factory.CreateIntSet(36, TownCat, 28, TownBunny, 24, TownSlimeBlue, 26, TownSlimeGreen, 26, TownSlimeOld, 26, TownSlimePurple, 26, TownSlimeRainbow, 26, TownSlimeYellow, 26, TownSlimeRed, 22, TownSlimeCopper, 20);

		// IDs taken from Player.GetPettingInfo
		/// <summary>
		/// While petting, the player's arm will be angled up by default. If the NPC is in this set, the player's armor will be angled down instead. Defaults to false.
		/// </summary>
		public static bool[] IsPetSmallForPetting = Factory.CreateBoolSet(TownCat, TownBunny, TownSlimeBlue, TownSlimeGreen, TownSlimeOld, TownSlimePurple, TownSlimeRainbow, TownSlimeYellow, TownSlimeRed, TownSlimeCopper);

		/// <summary>
		/// NPC in this set do not drop resource pickups, such as hearts or star items. Vanilla entries in this set include MotherSlime, CorruptSlime, and Slimer, all of which spawn other NPC when killed, suggesting that they split apart rather than died, hinting at why they shouldn't drop resource pickups.
		/// <para/> Defaults to false.
		/// </summary>
		public static bool[] NeverDropsResourcePickups = Factory.CreateBoolSet(MotherSlime, CorruptSlime, Slimer);

		/// <summary>
		/// This NPC will not despawn due to being far offscreen, but will still count towards <see cref="Player.nearbyActiveNPCs"/>. Unlike returning false in <see cref="ModNPC.CheckActive"/>, this NPC still counts towards spawn limits. 
		/// </summary>
		public static bool[] DoesntDespawnToInactivityAndCountsNPCSlots = Factory.CreateBoolSet(Deerclops);
	}
}
