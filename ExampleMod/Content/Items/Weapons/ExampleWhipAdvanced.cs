using ExampleMod.Content.Buffs;
using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Items;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleWhipAdvanced : ModItem
	{
		public static readonly int ExampleWhipAdvancedTagDamagePercent = 30;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ExampleWhipAdvancedTagDamagePercent);

		public override void SetStaticDefaults() {
			// Here is where we define how much TagDamage the whip does.
			// TagDuration and CritChance can be modified, too.
			// In this case, we've created our own whip tag effect that has extra functionality. (See below)
			ItemID.Sets.UniqueTagEffects[Type] = new WhipTagEffect_ExampleWhipAdvanced() {
				TagDamagePercent = ExampleWhipAdvancedTagDamagePercent,
				ProcDamageMultiplier = 1.75f,
				// Here's how to add buff that benefits the player.
				PlayerBuffId = ModContent.BuffType<ExampleWhipBuff>(),
				PlayerBuffTime = 180
			};
		}

		public override void SetDefaults() {
			// Call this method to quickly set some of the properties below.
			//Item.DefaultToWhip(ModContent.ProjectileType<ExampleWhipProjectileAdvanced>(), 20, 2, 4);

			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 20;
			Item.knockBack = 2;
			Item.rare = ItemRarityID.Orange;

			Item.shoot = ModContent.ProjectileType<ExampleWhipProjectileAdvanced>();
			Item.shootSpeed = 4;

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.UseSound = SoundID.Item152;
			Item.value = Item.buyPrice(gold: 2);
			Item.channel = true; // This is used for the charging functionality. Remove it if your whip shouldn't be chargeable.
			Item.noMelee = true;
			Item.noUseGraphic = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// This gives some visual variance on how fast the whip swinging animation plays out.
			// This has no effect on the actual collision.
			float swingDirection = 0.6f + (0.4f * Main.rand.NextFloat());
			// 1/3 of the time, swing the whip from the bottom to top instead of from top to bottom.
			// The Dark Harvest is the only whip that doesn't have the chance of swinging from the button up.
			if (Main.rand.NextBool(3)) {
				swingDirection *= -2.5f;
			}
			// Set swingDirection to 1f for the pre-1.4.5 behavior.

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, swingDirection);
			return false; // Return false because we've already spawned the projectile.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		// Makes the whip receive melee prefixes
		public override bool MeleePrefix() {
			return true;
		}
	}

	/// <summary>
	/// This is a custom whip tag effect that allows us to customize the tag effect.
	/// </summary>
	public class WhipTagEffect_ExampleWhipAdvanced : WhipTagEffect {
		/// <summary> This will do percentage based bonus damage instead of flat damage like the normal TagDamage. </summary>
		public int TagDamagePercent;
		public float TagDamageMultiplier => TagDamagePercent / 100f;

		public float ProcDamageMultiplier;

		// This hook runs when a tagged enemy takes damage from a minion or sentry and allows us to change the damage dealt.
		public override void ModifyTaggedHit(Player owner, Projectile optionalProjectile, NPC npcHit, ref NPC.HitModifiers damageDealt, ref bool crit) {
			// Running base here is very important, or else the existing TagDamage code will not run.
			base.ModifyTaggedHit(owner, optionalProjectile, npcHit, ref damageDealt, ref crit);

			float projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[optionalProjectile.type]; // Get the minion's tag multiplier if it has one.
			damageDealt.ScalingBonusDamage += TagDamageMultiplier * projTagMultiplier; // Add the addition percentage based damage.
		}

		// OnTaggedHit will run every time a tagged enemy takes damage from a minion or sentry.
		public override void OnTaggedHit(Player owner, Projectile optionalProjectile, NPC npcHit, float calcDamage) {
			// Create some particles.
			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.BlackLightningHit, new ParticleOrchestraSettings {
				PositionInWorld = npcHit.Center
			});
		}

		// OnProcHit will run when TryEnableProcOnNPC is true for the NPC. See ExampleWhipProjectileAdvanced.OnHitNPC for how to apply that.
		// Procs will be removed from the NPC once they activate.
		public override void OnProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, float calcDamage) {
			// Display some combat text when the tag procs.
			CombatText.NewText(optionalProjectile.Hitbox, Color.Purple, "BAM!");

			// This is how the Firecracker's explosion works.
			int explosionDamage = (int)((float)calcDamage * ProcDamageMultiplier);
			int explosionProj = Projectile.NewProjectile(optionalProjectile.GetSource_FromThis(), npcHit.Center, Vector2.Zero, ProjectileID.FireWhipProj, explosionDamage, 0f, optionalProjectile.owner);
			Main.projectile[explosionProj].localNPCImmunity[npcHit.whoAmI] = -1;
		}
		public override void ModifyProcHit(Player owner, Projectile optionalProjectile, NPC npcHit, ref NPC.HitModifiers damageDealt, ref bool crit) {
			// This is how the Firecracker's damage scaling works.
			damageDealt.ScalingBonusDamage += ProcDamageMultiplier * ProjectileID.Sets.SummonTagDamageMultiplier[optionalProjectile.type];
		}

		// There are number of other useful hooks including OnTagAppliedToNPC and OnSetToPlayer.
	}
}
