using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to place all your properties and hooks for each NPC. Create instances of ModNPC (preferably overriding this class) to pass as parameters to Mod.AddNPC.
	/// </summary>
	public abstract class ModNPC : ModTexturedType
	{
		/// <summary> The NPC object that this ModNPC controls. </summary>
		public NPC NPC { get; internal set; }

		/// <summary> Shorthand for npc.type; </summary>
		public int Type => NPC.type;

		/// <summary> The translations for the display name of this NPC. </summary>
		public ModTranslation DisplayName { get; internal set; }

		/// <summary> The file name of this NPC's head texture file, to be used in autoloading. </summary>
		public virtual string HeadTexture => Texture + "_Head";

		/// <summary> This file name of this NPC's boss head texture file, to be used in autoloading. </summary>
		public virtual string BossHeadTexture => Texture + "_Head_Boss";

		/// <summary> Determines which type of vanilla NPC this ModNPC will copy the behavior (AI) of. Leave as 0 to not copy any behavior. Defaults to 0. </summary>
		public int AIType { get; set; }
		
		/// <summary> Determines which type of vanilla NPC this ModNPC will copy the animation (FindFrame) of. Leave as 0 to not copy any animation. Defaults to 0. </summary>
		public int AnimationType { get; set; }

		/// <summary> The item type of the boss bag that is dropped when DropBossBags is called for this NPC. </summary>
		public int BossBag { get; set; } = -1;

		/// <summary> The ID of the music that plays when this NPC is on or near the screen. Defaults to -1, which means music plays normally. </summary>
		/// Will be superceded by ModSceneEffect. Kept for legacy.
		public int Music { get; set; } = -1;
		
		/// <summary> The priority of the music that plays when this NPC is on or near the screen. </summary>
		/// Will be superceded by ModSceneEffect. Kept for legacy.
		public SceneEffectPriority SceneEffectPriority { get; set; } = SceneEffectPriority.BossLow;
		
		/// <summary> The vertical offset used for drawing this NPC. Defaults to 0. </summary>
		public float DrawOffsetY { get; set; }
		
		/// <summary> The type of NPC that this NPC will be considered as when determining banner drops and banner bonuses. By default this will be 0, which means this NPC is not associated with any banner. To give your NPC its own banner, set this field to the NPC's type. </summary>
		public int Banner { get; set; }

		/// <summary> The type of the item this NPC drops for every 50 times it is defeated. For any ModNPC whose banner field is set to the type of this NPC, that ModNPC will drop this banner. </summary>
		public int BannerItem { get; set; }

		//TODO: Find a better solution in the future.
		/// <summary> The ModBiome Types associated with this NPC spawning, if applicable. Used in Bestiary </summary>
		public int[] SpawnModBiomes { get; set; } = new int[0];

		public ModNPC() {
			NPC = new NPC{ModNPC = this};
		}

		protected sealed override void Register() {
			ModTypeLookup<ModNPC>.Register(this);

			NPC.type = NPCLoader.ReserveNPCID();
			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"NPCName.{Name}");

			NPCLoader.npcs.Add(this);

			Type type = GetType();
			var autoloadHead = type.GetAttribute<AutoloadHead>();
			if (autoloadHead != null) {
				Mod.AddNPCHeadTexture(NPC.type, HeadTexture);
			}
			var autoloadBossHead = type.GetAttribute<AutoloadBossHead>();
			if (autoloadBossHead != null) {
				Mod.AddBossHeadTexture(BossHeadTexture, NPC.type);
			}
		}

		public sealed override void SetupContent() {
			NPCLoader.SetDefaults(NPC, false);
			AutoStaticDefaults();
			SetStaticDefaults();

			NPCID.Search.Add(FullName, Type);
		}

		internal void SetupNPC(NPC npc) {
			ModNPC newNPC = (ModNPC)(CloneNewInstances ? MemberwiseClone() : Activator.CreateInstance(GetType()));
			newNPC.NPC = npc;
			npc.ModNPC = newNPC;
			newNPC.Mod = Mod;
			newNPC.SetDefaults();
		}

		/// <summary>
		/// Whether instances of this ModNPC are created through a memberwise clone or its constructor. Defaults to false.
		/// </summary>
		public virtual bool CloneNewInstances => false;

		/// <summary>
		/// Returns a clone of this ModNPC. 
		/// Allows you to decide which fields of your ModNPC class are copied over when a new NPC is created. 
		/// By default this will return a memberwise clone; you will want to override this if your ModNPC contains object references. 
		/// Only called if CloneNewInstances is set to true.
		/// </summary>
		public virtual ModNPC Clone() => (ModNPC)MemberwiseClone();

		/// <summary>
		/// Create a new instance of this ModNPC for an NPC instance. 
		/// Called at the end of NPC.SetDefaults.
		/// If CloneNewInstances is true, just calls Clone()
		/// Otherwise calls the default constructor and copies fields
		/// </summary>
		public virtual ModNPC NewInstance(NPC npcClone) {
			if (CloneNewInstances) {
				ModNPC clone = Clone();
				clone.NPC = npcClone;
				return clone;
			}

			ModNPC copy = (ModNPC)Activator.CreateInstance(GetType());
			copy.NPC = npcClone;
			copy.Mod = Mod;
			copy.AIType = AIType;
			copy.AnimationType = AnimationType;
			copy.BossBag = BossBag;
			copy.Music = Music;
			copy.DrawOffsetY = DrawOffsetY;
			copy.Banner = Banner;
			copy.BannerItem = BannerItem;
			return copy;
		}

		/// <summary>
		/// Allows you to set all your NPC's properties, such as width, damage, aiStyle, lifeMax, etc.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Automatically sets certain static defaults. Override this if you do not want the properties to be set for you.
		/// </summary>
		public virtual void AutoStaticDefaults() {
			TextureAssets.Npc[NPC.type] = ModContent.Request<Texture2D>(Texture);

			if (Banner != 0 && BannerItem != 0) {
				NPCLoader.bannerToItem[Banner] = BannerItem;
			}
			else if (Banner != 0 || BannerItem != 0) {
				Logging.tML.Warn(Language.GetTextValue("tModLoader.LoadWarningBannerOrBannerItemNotSet", Mod.DisplayName, Name));
			}

			if (NPC.lifeMax > 32767 || NPC.boss) {
				Main.npcLifeBytes[NPC.type] = 4;
			}
			else if (NPC.lifeMax > 127) {
				Main.npcLifeBytes[NPC.type] = 2;
			}
			else {
				Main.npcLifeBytes[NPC.type] = 1;
			}

			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(Name, "([A-Z])", " $1").Trim());
		}

		/// <summary>
		/// Allows you to customize this NPC's stats in expert mode. This is useful because expert mode's doubling of damage and life might be too much sometimes (for example, with bosses). Also useful for scaling life with the number of players in the world.
		/// </summary>
		/// <param name="numPlayers"></param>
		/// <param name="bossLifeScale"></param>
		public virtual void ScaleExpertStats(int numPlayers, float bossLifeScale) {
		}

		/// <summary>
		/// Allows you to set an NPC's information in the Bestiary.
		/// </summary>
		/// <param name="database"></param>
		/// <param name="bestiaryEntry"></param>
		public virtual void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		}

		/// <summary>
		/// This is where you reset any fields you add to your subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied. (Note: This hook is only really useful for GlobalNPC, but is included in ModNPC for completion.)
		/// </summary>
		public virtual void ResetEffects() {
		}

		/// <summary>
		/// Allows you to determine how this NPC behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
		/// </summary>
		/// <returns></returns>
		public virtual bool PreAI() {
			return true;
		}

		/// <summary>
		/// Allows you to determine how this NPC behaves. This will only be called if PreAI returns true.
		/// </summary>
		public virtual void AI() {
		}

		//Allows you to determine how this NPC behaves. This will be called regardless of what PreAI returns.
		public virtual void PostAI() {
		}

		/// <summary>
		/// If you are storing AI information outside of the npc.ai array, use this to send that AI information between clients and servers.
		/// </summary>
		/// <param name="writer"></param>
		public virtual void SendExtraAI(BinaryWriter writer) {
		}

		/// <summary>
		/// Use this to receive information that was sent in SendExtraAI.
		/// </summary>
		/// <param name="reader"></param>
		public virtual void ReceiveExtraAI(BinaryReader reader) {
		}

		/// <summary>
		/// Allows you to modify the frame from this NPC's texture that is drawn, which is necessary in order to animate NPCs.
		/// </summary>
		/// <param name="frameHeight"></param>
		public virtual void FindFrame(int frameHeight) {
		}

		/// <summary>
		/// Allows you to make things happen whenever this NPC is hit, such as creating dust or gores. This hook is client side. Usually when something happens when an npc dies such as item spawning, you use NPCLoot, but you can use HitEffect paired with a check for `if (npc.life <= 0)` to do client-side death effects, such as spawning dust, gore, or death sounds.
		/// </summary>
		/// <param name="hitDirection"></param>
		/// <param name="damage"></param>
		public virtual void HitEffect(int hitDirection, double damage) {
		}

		/// <summary>
		/// Allows you to make the NPC either regenerate health or take damage over time by setting npc.lifeRegen. Regeneration or damage will occur at a rate of half of npc.lifeRegen per second. The damage parameter is the number that appears above the NPC's head if it takes damage over time.
		/// </summary>
		/// <param name="damage"></param>
		public virtual void UpdateLifeRegen(ref int damage) {
		}

		/// <summary>
		/// Whether or not to run the code for checking whether this NPC will remain active. Return false to stop this NPC from being despawned and to stop this NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckActive() {
			return true;
		}

		/// <summary>
		/// Whether or not this NPC should be killed when it reaches 0 health. You may program extra effects in this hook (for example, how Golem's head lifts up for the second phase of its fight). Return false to stop this NPC from being killed. Returns true by default.
		/// </summary>
		/// <returns></returns>
		public virtual bool CheckDead() {
			return true;
		}

		/// <summary>
		/// Allows you to call OnKill on your own when the NPC dies, rather then letting vanilla call it on its own. Returns false by default.
		/// </summary>
		/// <returns>Return true to stop vanilla from calling OnKill on its own. Do this if you call OnKill yourself.</returns>
		public virtual bool SpecialOnKill() {
			return false;
		}

		/// <summary>
		/// Allows you to determine whether or not this NPC will do anything upon death (besides dying). Returns true by default.
		/// </summary>
		/// <returns></returns>
		public virtual bool PreKill() {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when this NPC dies (for example, dropping items and setting ModSystem fields). This hook runs on the server/single player. For client-side effects, such as dust, gore, and sounds, see HitEffect
		/// </summary>
		public virtual void OnKill() {
		}

        /// <summary>
        /// Allows you to make things happen when this NPC is caught. Ran Serverside
        /// </summary>
        /// <param name="player">The player catching this NPC</param>
        /// <param name="item">The item that will be spawned</param>
        public virtual void OnCatchNPC(Player player, Item item) {
		}

		/// <summary>
		/// Allows you to add and modify NPC loot tables to drop on death and to appear in the Bestiary.
		/// </summary>
		/// <param name="npcLoot"></param>
		public virtual void ModifyNPCLoot(NPCLoot npcLoot) {
		}

		/// <summary>
		/// Allows you to customize what happens when this boss dies, such as which name is displayed in the defeat message and what type of potion it drops.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="potionType"></param>
		public virtual void BossLoot(ref string name, ref int potionType) {
		}

		/// <summary>
		/// Allows you to determine whether this NPC can hit the given player. Return false to block this NPC from hitting the target. Returns true by default. CooldownSlot determines which of the player's cooldown counters to use (-1, 0, or 1), and defaults to -1.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="cooldownSlot"></param>
		/// <returns></returns>
		public virtual bool CanHitPlayer(Player target, ref int cooldownSlot) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that this NPC does to a player.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this NPC hits a player (for example, inflicting debuffs).
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitPlayer(Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether this NPC can hit the given friendly NPC. Return true to allow hitting the target, return false to block this NPC from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool? CanHitNPC(NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that this NPC does to a friendly NPC.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this NPC hits a friendly NPC.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether this NPC can be hit by the given melee weapon when swung. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool? CanBeHitByItem(Player player, Item item) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that this NPC takes from a melee weapon.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this NPC is hit by a melee weapon.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether this NPC can be hit by the given projectile. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public virtual bool? CanBeHitByProjectile(Projectile projectile) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that this NPC takes from a projectile. This method is only called for the owner of the projectile, meaning that in multi-player, projectiles owned by a player call this method on that client, and projectiles owned by the server such as enemy projectiles call this method on the server.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		/// <param name="hitDirection"></param>
		public virtual void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
		}

		/// <summary>
		/// Allows you to create special effects when this NPC is hit by a projectile.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to use a custom damage formula for when this NPC takes damage from any source. For example, you can change the way defense works or use a different crit multiplier. Return false to stop the game from running the vanilla damage formula; returns true by default.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="defense"></param>
		/// <param name="knockback"></param>
		/// <param name="hitDirection"></param>
		/// <param name="crit"></param>
		/// <returns></returns>
		public virtual bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			return true;
		}

		/// <summary>
		/// Allows you to customize the boss head texture used by an NPC based on its state. Set index to -1 to stop the texture from being displayed.
		/// </summary>
		/// <param name="index">The index for NPCID.Sets.BossHeadTextures</param>
		public virtual void BossHeadSlot(ref int index) {
		}

		/// <summary>
		/// Allows you to customize the rotation of this NPC's boss head icon on the map.
		/// </summary>
		/// <param name="rotation"></param>
		public virtual void BossHeadRotation(ref float rotation) {
		}

		/// <summary>
		/// Allows you to flip this NPC's boss head icon on the map.
		/// </summary>
		/// <param name="spriteEffects"></param>
		public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects) {
		}

		/// <summary>
		/// Allows you to determine the color and transparency in which this NPC is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
		/// </summary>
		/// <param name="drawColor"></param>
		/// <returns></returns>
		public virtual Color? GetAlpha(Color drawColor) {
			return null;
		}

		/// <summary>
		/// Allows you to add special visual effects to this NPC (such as creating dust), and modify the color in which the NPC is drawn.
		/// </summary>
		/// <param name="drawColor"></param>
		public virtual void DrawEffects(ref Color drawColor) {
		}

		/// <summary>
		/// Allows you to draw things behind this NPC, or to modify the way this NPC is drawn. Substract screenPos from the draw position before drawing. Return false to stop the game from drawing the NPC (useful if you're manually drawing the NPC). Returns true by default.
		/// </summary>
		/// <param name="spriteBatch">The spritebatch to draw on</param>
		/// <param name="screenPos">The screen position used to translate world position into screen position</param>
		/// <param name="drawColor">The color the NPC is drawn in</param>
		/// <returns></returns>
		public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of this NPC. Substract screenPos from the draw position before drawing. This method is called even if PreDraw returns false.
		/// </summary>
		/// <param name="spriteBatch">The spritebatch to draw on</param>
		/// <param name="screenPos">The screen position used to translate world position into screen position</param>
		/// <param name="drawColor">The color the NPC is drawn in</param>
		public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		}

		/// <summary>
		/// When used in conjunction with "npc.hide = true", allows you to specify that this npc should be drawn behind certain elements. Add the index to one of Main.DrawCacheNPCsMoonMoon, DrawCacheNPCsOverPlayers, DrawCacheNPCProjectiles, or DrawCacheNPCsBehindNonSolidTiles.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="index"></param>
		public virtual void DrawBehind(int index)
		{
		}

		/// <summary>
		/// Allows you to control how the health bar for this NPC is drawn. The hbPosition parameter is the same as Main.hbPosition; it determines whether the health bar gets drawn above or below the NPC by default. The scale parameter is the health bar's size. By default, it will be the normal 1f; most bosses set this to 1.5f. Return null to let the normal vanilla health-bar-drawing code to run. Return false to stop the health bar from being drawn. Return true to draw the health bar in the position specified by the position parameter (note that this is the world position, not screen position).
		/// </summary>
		/// <param name="hbPosition"></param>
		/// <param name="scale"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return null;
		}

		/// <summary>
		/// Whether or not this NPC can spawn with the given spawning conditions. Return the weight for the chance of this NPC to spawn compared to vanilla mobs. All vanilla mobs combined have a total weight of 1. Returns 0 by default, which disables natural spawning. Remember to always use spawnInfo.player and not Main.LocalPlayer when checking Player or ModPlayer fields, otherwise your mod won't work in Multiplayer.
		/// </summary>
		/// <param name="spawnInfo"></param>
		/// <returns></returns>
		public virtual float SpawnChance(NPCSpawnInfo spawnInfo) {
			return 0f;
		}

		/// <summary>
		/// Allows you to customize how this NPC is created when it naturally spawns (for example, its position or ai array). Return the return value of NPC.NewNPC. By default this method spawns this NPC on top of the tile at the given coordinates.
		/// </summary>
		/// <param name="tileX"></param>
		/// <param name="tileY"></param>
		/// <returns></returns>
		public virtual int SpawnNPC(int tileX, int tileY) {
			return NPC.NewNPC(tileX * 16 + 8, tileY * 16, NPC.type);
		}

		/// <summary>
		/// Whether or not the conditions have been met for this town NPC to be able to move into town. For example, the Demolitionist requires that any player has an explosive.
		/// </summary>
		/// <param name="numTownNPCs"></param>
		/// <param name="money"></param>
		/// <returns></returns>
		public virtual bool CanTownNPCSpawn(int numTownNPCs, int money) {
			return false;
		}

		/// <summary>
		/// Allows you to define special conditions required for this town NPC's house. For example, Truffle requires the house to be in an aboveground mushroom biome.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		/// <returns></returns>
		public virtual bool CheckConditions(int left, int right, int top, int bottom) {
			return true;
		}

		/// <summary>
		/// Allows you to give this town NPC any name when it spawns. By default returns something embarrassing.
		/// </summary>
		/// <returns></returns>
		public virtual string TownNPCName() {
			return Language.GetTextValue("tModLoader.DefaultTownNPCName");
		}

		/// <summary>
		/// Allows you to determine whether this town NPC wears a party hat during a party. Returns true by default.
		/// </summary>
		/// <returns></returns>
		public virtual bool UsesPartyHat() {
			return true;
		}

		/// <summary>
		/// Allows you to determine whether this NPC can talk with the player. By default, returns if the NPC is a town NPC.
		/// </summary>
		/// <returns></returns>
		public virtual bool CanChat() {
			return NPC.townNPC;
		}

		/// <summary>
		/// Allows you to give this NPC a chat message when a player talks to it. By default returns something embarrassing.
		/// </summary>
		/// <returns></returns>
		public virtual string GetChat() {
			return Language.GetTextValue("tModLoader.DefaultTownNPCChat");
		}

		/// <summary>
		/// Allows you to set the text for the buttons that appear on this NPC's chat window. A parameter left as an empty string will not be included as a button on the chat window.
		/// </summary>
		/// <param name="button"></param>
		/// <param name="button2"></param>
		public virtual void SetChatButtons(ref string button, ref string button2) {
		}

		/// <summary>
		/// Allows you to make something happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked. Set the shop parameter to true to open this NPC's shop.
		/// </summary>
		/// <param name="firstButton"></param>
		/// <param name="shop"></param>
		public virtual void OnChatButtonClicked(bool firstButton, ref bool shop) {
		}

		/// <summary>
		/// Allows you to add items to this NPC's shop. Add an item by setting the defaults of shop.item[nextSlot] then incrementing nextSlot. In the end, nextSlot must have a value of 1 greater than the highest index in shop.item that contains an item.
		/// </summary>
		/// <param name="shop"></param>
		/// <param name="nextSlot"></param>
		public virtual void SetupShop(Chest shop, ref int nextSlot) {
		}

		/// <summary>
		/// Whether this NPC can be telported to a King or Queen statue. Returns false by default.
		/// </summary>
		/// <param name="toKingStatue">Whether the NPC is being teleported to a King or Queen statue.</param>
		public virtual bool CanGoToStatue(bool toKingStatue) {
			return false;
		}

		/// <summary>
		/// Allows you to make things happen when this NPC teleports to a King or Queen statue.
		/// This method is only called server side.
		/// </summary>
		/// <param name="toKingStatue">Whether the NPC was teleported to a King or Queen statue.</param>
		public virtual void OnGoToStatue(bool toKingStatue) {
		}

		/// <summary>
		/// Allows you to determine the damage and knockback of this town NPC's attack before the damage is scaled. (More information on scaling in GlobalNPC.BuffTownNPCs.)
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		public virtual void TownNPCAttackStrength(ref int damage, ref float knockback) {
		}

		/// <summary>
		/// Allows you to determine the cooldown between each of this town NPC's attack. The cooldown will be a number greater than or equal to the first parameter, and less then the sum of the two parameters.
		/// </summary>
		/// <param name="cooldown"></param>
		/// <param name="randExtraCooldown"></param>
		public virtual void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
		}

		/// <summary>
		/// Allows you to determine the projectile type of this town NPC's attack, and how long it takes for the projectile to actually appear. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
		/// </summary>
		/// <param name="projType"></param>
		/// <param name="attackDelay"></param>
		public virtual void TownNPCAttackProj(ref int projType, ref int attackDelay) {
		}

		/// <summary>
		/// Allows you to determine the speed at which this town NPC throws a projectile when it attacks. Multiplier is the speed of the projectile, gravityCorrection is how much extra the projectile gets thrown upwards, and randomOffset allows you to randomize the projectile's velocity in a square centered around the original velocity. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
		/// </summary>
		/// <param name="multiplier"></param>
		/// <param name="gravityCorrection"></param>
		/// <param name="randomOffset"></param>
		public virtual void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
		}

		/// <summary>
		/// Allows you to tell the game that this town NPC has already created a projectile and will still create more projectiles as part of a single attack so that the game can animate the NPC's attack properly. Only used when the town NPC has an attack type of 1 (shooting).
		/// </summary>
		/// <param name="inBetweenShots"></param>
		public virtual void TownNPCAttackShoot(ref bool inBetweenShots) {
		}

		/// <summary>
		/// Allows you to control the brightness of the light emitted by this town NPC's aura when it performs a magic attack. Only used when the town NPC has an attack type of 2 (magic)
		/// </summary>
		/// <param name="auraLightMultiplier"></param>
		public virtual void TownNPCAttackMagic(ref float auraLightMultiplier) {
		}

		/// <summary>
		/// Allows you to determine the width and height of the item this town NPC swings when it attacks, which controls the range of this NPC's swung weapon. Only used when the town NPC has an attack type of 3 (swinging).
		/// </summary>
		/// <param name="itemWidth"></param>
		/// <param name="itemHeight"></param>
		public virtual void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) {
		}

		/// <summary>
		/// Allows you to customize how this town NPC's weapon is drawn when this NPC is shooting (this NPC must have an attack type of 1). Scale is a multiplier for the item's drawing size, item is the ID of the item to be drawn, and closeness is how close the item should be drawn to the NPC.
		/// </summary>
		/// <param name="scale"></param>
		/// <param name="item"></param>
		/// <param name="closeness"></param>
		public virtual void DrawTownAttackGun(ref float scale, ref int item, ref int closeness) {
		}

		/// <summary>
		/// Allows you to customize how this town NPC's weapon is drawn when this NPC is swinging it (this NPC must have an attack type of 3). Item is the Texture2D instance of the item to be drawn (use Main.itemTexture[id of item]), itemSize is the width and height of the item's hitbox (the same values for TownNPCAttackSwing), scale is the multiplier for the item's drawing size, and offset is the offset from which to draw the item from its normal position.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="itemSize"></param>
		/// <param name="scale"></param>
		/// <param name="offset"></param>
		public virtual void DrawTownAttackSwing(ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset) {
		}
	}
}
