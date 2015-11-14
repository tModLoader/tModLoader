using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;

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

		internal ModPlayer Clone()
		{
			return (ModPlayer)MemberwiseClone();
		}

		public bool TypeEquals(ModPlayer other)
		{
			return mod == other.mod && Name == other.Name;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
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

		public virtual void OnFishSelected(Item fishingRod, Item bait, int liquidType, int poolCount, int worldLayer, int questFish, ref int caughtType)
		{
		}

		public virtual void GetFishingLevel(Item fishingRod, Item bait, ref int fishingLevel)
		{
		}

		public virtual void AnglerQuestReward(float quality, List<Item> rewardItems)
		{
		}

		public virtual void GetDyeTraderReward(List<int> dyeItemIDsPool)
		{
		}

		public virtual void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
		{
		}

		public virtual void ModifyHitNPC(NPC target, int damage, float knockback, bool crit)
		{
		}

		public virtual bool? CanHitNPC(NPC target)
		{
			return null;
		}

		public virtual void OnHitByNPC(NPC npc, int damage, bool crit)
		{
		}

		public virtual bool CanBeHitByNPC(NPC npc)
		{
			return true;
		}

		public virtual void ModifyHitByNPC(NPC npc, int damage, bool crit)
		{
		}

		public virtual bool Shoot(Vector2 position, float speedX, float speedY, int type, int damage, float knockback)
		{
			return true;
		}

		public virtual void OnHitAnything(float x, float y, Entity victim)
		{
		}

		public virtual void PreHurt(int damage, int hitDirection, bool pvp, bool quiet, bool crit)
		{
		}

		public virtual void Hurt(int damage, int hitDirection, bool pvp, bool quiet, bool crit)
		{
		}

		public virtual void MeleeEffects(Item item, Rectangle hitbox)
		{
		}

		public virtual bool CanHitPvp(Player attacked)
		{
			return true;
		}

		public virtual void ModifyHitPvp(Player target, int damage, bool crit)
		{
		}

		public virtual void OnHitPvp(Player target, int damage, bool crit)
		{
		}

		public virtual int GetWeaponDamage(Item sItem)
		{
			return sItem.damage;
		}

		public virtual bool ConsumeAmmo(Item item)
		{
			return true;
		}

		public virtual float GetWeaponKnockback(Item sItem)
		{
			return sItem.knockBack;
		}

		//TODO
		//hooks for grappling hooks
	}
}
