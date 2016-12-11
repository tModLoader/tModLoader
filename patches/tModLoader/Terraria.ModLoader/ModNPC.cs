using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModNPC
	{
		//add modNPC property to Terraria.NPC (internal set)
		//set modNPC to null at beginning of Terraria.NPC.SetDefaults
		public NPC npc
		{
			get;
			internal set;
		}

		public Mod mod
		{
			get;
			internal set;
		}

		internal string texture;
		internal string[] altTextures;
		public int aiType = 0;
		public int animationType = 0;
		public int bossBag = -1;
		//make changes to Terraria.Main.UpdateMusic (see patch files)
		public int music = -1;
		//in Terraria.Main.NPCAddHeight at end of else if chain add
		//  else if(Main.npc[i].modNPC != null) { num = Main.npc[i].modNPC.drawOffsetY; }
		public float drawOffsetY = 0f;
		//in Terraria.Item.NPCToBanner before returning 0 add
		//  if(i >= NPCID.Count) { return NPCLoader.npcs[i].banner; }
		//in Terraria.Item.BannerToNPC before returning 0 add
		//  if(i >= NPCID.Count) { return i; }
		public int banner = 0;
		//in Terraria.NPC.NPCLoot after if statements setting num6 add
		//  if(num3 >= NPCID.Count) { num6 = NPCLoader.npcs[num3].bannerItem; }
		public int bannerItem = 0;

		public ModNPC()
		{
			npc = new NPC();
		}

		public virtual bool Autoload(ref string name, ref string texture, ref string[] altTextures)
		{
			return mod.Properties.Autoload;
		}

		public virtual void AutoloadHead(ref string headTexture, ref string bossHeadTexture)
		{
		}

		internal void SetupNPC(NPC npc)
		{
			ModNPC newNPC = (ModNPC)(CloneNewInstances ? MemberwiseClone() : Activator.CreateInstance(GetType()));
			newNPC.npc = npc;
			npc.modNPC = newNPC;
			newNPC.mod = mod;
			newNPC.SetDefaults();
		}

		public virtual bool CloneNewInstances => false;

		public virtual void SetDefaults()
		{
		}

		public virtual void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
		}

		public virtual void ResetEffects()
		{
		}

		public virtual bool PreAI()
		{
			return true;
		}

		public virtual void AI()
		{
		}

		public virtual void PostAI()
		{
		}

		public virtual void SendExtraAI(BinaryWriter writer)
		{
		}

		public virtual void ReceiveExtraAI(BinaryReader reader)
		{
		}

		public virtual void FindFrame(int frameHeight)
		{
		}

		public virtual void HitEffect(int hitDirection, double damage)
		{
		}

		public virtual void UpdateLifeRegen(ref int damage)
		{
		}

		public virtual bool CheckActive()
		{
			return true;
		}

		public virtual bool CheckDead()
		{
			return true;
		}

		public virtual bool PreNPCLoot()
		{
			return true;
		}

		public virtual void NPCLoot()
		{
		}

		public virtual void BossLoot(ref string name, ref int potionType)
		{
		}

		public virtual bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return true;
		}

		public virtual void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPlayer(Player target, int damage, bool crit)
		{
		}

		public virtual bool? CanHitNPC(NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanBeHitByItem(Player player, Item item)
		{
			return null;
		}

		public virtual void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanBeHitByProjectile(Projectile projectile)
		{
			return null;
		}

		public virtual void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
		}

		public virtual void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
		}

		public virtual bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			return true;
		}

		public virtual void BossHeadSlot(ref int index)
		{
		}

		public virtual void BossHeadRotation(ref float rotation)
		{
		}

		public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
		{
		}

		public virtual Color? GetAlpha(Color drawColor)
		{
			return null;
		}

		public virtual void DrawEffects(ref Color drawColor)
		{
		}

		public virtual bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			return true;
		}

		public virtual void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
		}

		public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return null;
		}

		public virtual float CanSpawn(NPCSpawnInfo spawnInfo)
		{
			return 0f;
		}

		public virtual int SpawnNPC(int tileX, int tileY)
		{
			return NPC.NewNPC(tileX * 16 + 8, tileY * 16, npc.type);
		}

		public virtual bool CanTownNPCSpawn(int numTownNPCs, int money)
		{
			return false;
		}

		public virtual bool CheckConditions(int left, int right, int top, int bottom)
		{
			return true;
		}

		public virtual string TownNPCName()
		{
			return "No-Name";
		}

		public virtual bool UsesPartyHat()
		{
			return true;
		}

		public virtual string GetChat()
		{
			return "My modder forgot to give me a chat message.";
		}

		public virtual void SetChatButtons(ref string button, ref string button2)
		{
		}

		public virtual void OnChatButtonClicked(bool firstButton, ref bool shop)
		{
		}

		public virtual void SetupShop(Chest shop, ref int nextSlot)
		{
		}

		public virtual void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
		}

		public virtual void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
		}

		public virtual void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
		}

		public virtual void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
		}

		public virtual void TownNPCAttackShoot(ref bool inBetweenShots)
		{
		}

		public virtual void TownNPCAttackMagic(ref float auraLightMultiplier)
		{
		}

		public virtual void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight)
		{
		}

		public virtual void DrawTownAttackGun(ref float scale, ref int item, ref int closeness)
		{
		}

		public virtual void DrawTownAttackSwing(ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset)
		{
		}
	}
}
