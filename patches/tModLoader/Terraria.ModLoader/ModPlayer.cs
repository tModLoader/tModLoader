using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	public class ModPlayer
	{
		public Mod mod
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			internal set;
		}

		public Player player
		{
			get;
			internal set;
		}

		internal ModPlayer CreateFor(Player newPlayer)
		{
			ModPlayer modPlayer = (ModPlayer)(CloneNewInstances ? MemberwiseClone() : Activator.CreateInstance(GetType()));
			modPlayer.Name = Name;
			modPlayer.mod = mod;
			modPlayer.player = newPlayer;
			modPlayer.Initialize();
			return modPlayer;
		}

		public bool TypeEquals(ModPlayer other)
		{
			return mod == other.mod && Name == other.Name;
		}

		public virtual bool CloneNewInstances => true;

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual void Initialize()
		{
		}

		public virtual void ResetEffects()
		{
		}

		public virtual void UpdateDead()
		{
		}

		public virtual void SaveCustomData(BinaryWriter writer)
		{
		}

		public virtual void LoadCustomData(BinaryReader reader)
		{
		}

		public virtual void SetupStartInventory(IList<Item> items)
		{
		}

		public virtual void UpdateBiomes()
		{
		}

		public virtual void UpdateBiomeVisuals()
		{
		}

		public virtual Texture2D GetMapBackgroundImage()
		{
			return null;
		}

		public virtual void UpdateBadLifeRegen()
		{
		}

		public virtual void UpdateLifeRegen()
		{
		}

		public virtual void NaturalLifeRegen(ref float regen)
		{
		}

		public virtual void PreUpdate()
		{
		}

		public virtual void SetControls()
		{
		}

		public virtual void PreUpdateBuffs()
		{
		}

		public virtual void PostUpdateBuffs()
		{
		}

		public virtual void PostUpdateEquips()
		{
		}

		public virtual void PostUpdateMiscEffects()
		{
		}

		public virtual void PostUpdateRunSpeeds()
		{
		}

		public virtual void PostUpdate()
		{
		}

		public virtual void FrameEffects()
		{
		}

		public virtual bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref string deathText)
		{
			return true;
		}

		public virtual void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
		}

		public virtual void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
		}

		public virtual bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore,
			ref string deathText)
		{
			return true;
		}

		public virtual void Kill(double damage, int hitDirection, bool pvp, string deathText)
		{
		}

		public virtual bool PreItemCheck()
		{
			return true;
		}

		public virtual void PostItemCheck()
		{
		}

		public virtual void GetWeaponDamage(Item item, ref int damage)
		{
		}

		public virtual void GetWeaponKnockback(Item item, ref float knockback)
		{
		}

		public virtual bool ConsumeAmmo(Item weapon, Item ammo)
		{
			return true;
		}

		public virtual bool Shoot(Item item, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			return true;
		}

		public virtual void MeleeEffects(Item item, Rectangle hitbox)
		{
		}

		public virtual void OnHitAnything(float x, float y, Entity victim)
		{
		}

		public virtual bool? CanHitNPC(Item item, NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanHitNPCWithProj(Projectile proj, NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
		}

		public virtual void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool CanHitPvp(Item item, Player target)
		{
			return true;
		}

		public virtual void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPvp(Item item, Player target, int damage, bool crit)
		{
		}

		public virtual bool CanHitPvpWithProj(Projectile proj, Player target)
		{
			return true;
		}

		public virtual void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
		{
		}

		public virtual bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
		{
			return true;
		}

		public virtual void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitByNPC(NPC npc, int damage, bool crit)
		{
		}

		public virtual bool CanBeHitByProjectile(Projectile proj)
		{
			return true;
		}

		public virtual void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitByProjectile(Projectile proj, int damage, bool crit)
		{
		}

		public virtual void CatchFish(Item fishingRod, Item bait, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
		{
		}

		public virtual void GetFishingLevel(Item fishingRod, Item bait, ref int fishingLevel)
		{
		}

		public virtual void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
		{
		}

		public virtual void GetDyeTraderReward(List<int> rewardPool)
		{
		}

		public virtual void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
		}

		public virtual void ModifyDrawLayers(List<PlayerLayer> layers)
		{
		}

		public virtual void ModifyDrawHeadLayers(List<PlayerHeadLayer> layers)
		{
		}
	}
}
