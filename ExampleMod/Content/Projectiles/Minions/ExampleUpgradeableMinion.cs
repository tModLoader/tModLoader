using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace ExampleModTesting.Items
{
	public class ExampleUpgradeableMinionBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
			Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
		}

		public override void Update(Player player, ref int buffIndex) {
			// If the minions exist reset the buff time, otherwise remove the buff from the player
			if (player.ownedProjectileCounts[ModContent.ProjectileType<ExampleUpgradeableMinion>()] > 0) {
			    player.buffTime[buffIndex] = 18000;
			}
			else {
			    player.DelBuff(buffIndex);
			    buffIndex--;
			}
		}
    }

	public class ExampleUpgradeableMinionItem : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

			ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f; // The default value is 1, but other values are supported. See the docs for more guidance. 
		}

		public override void SetDefaults() {
			Item.damage = 30;
			Item.knockBack = 3f;
			Item.mana = 10; // mana cost
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing; // how the player's arm moves when using the item
			Item.value = Item.sellPrice(gold: 30);
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item44; // What sound should play when using the item

			// These below are needed for a minion weapon
			Item.noMelee = true; // this item doesn't do any melee damage
			Item.DamageType = DamageClass.Summon; // Makes the damage register as summon. If your item does not have any damage type, it becomes true damage (which means that damage scalars will not affect it). Be sure to have a damage type
			Item.buffType = ModContent.BuffType<ExampleUpgradeableMinionBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			Item.shoot = ModContent.ProjectileType<ExampleUpgradeableMinion>(); // This item creates the minion projectile
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position
			position = Main.MouseWorld;
		}

		// Disables resummoning for this minion
		public override bool CanUseItem(Player player) {
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (Main.projectile[i].type == ModContent.ProjectileType<ExampleUpgradeableMinion>() && Main.projectile[i].ai[0] == player.maxMinions && Main.projectile[i].owner == Main.myPlayer) {
					return false;
				}
			}
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// Spawn the projectile for the first time if the player doesn't have a minion of that kind yet
			if (!player.HasBuff(ModContent.BuffType<ExampleUpgradeableMinionBuff>())) {
				// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
				player.AddBuff(Item.buffType, 2);

				// Minions have to be spawned manually, then have originalDamage assigned to the damage of the summon item
				var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
				projectile.originalDamage = Item.damage;
				
				// Return to not run the rest of the Shoot() code when spawning the projectile
				// Since we spawned the projectile manually already, we do not need the game to spawn it for ourselves anymore, so return false
				return false;
			}
			
			// If there is enough room for the next summon, raise the damage of it, and make it take up more minion slots
			for (int i = 0; i< Main.maxProjectiles; i++) {
				if (Main.projectile[i].type == ModContent.ProjectileType<ExampleUpgradeableMinion>() && Main.projectile[i].ai[0] < player.maxMinions && Main.projectile[i].owner == Main.myPlayer) {
					Main.projectile[i].ai[0]++;
					Main.projectile[i].minionSlots++;
				}
			}

			// Since we spawned the projectile manually already, we do not need the game to spawn it for ourselves anymore, so return false
			return false;
		}
	
		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Wood, 20)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class ExampleUpgradeableMinion : ModProjectile
	{
		public override void SetStaticDefaults() {
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			Main.projPet[Projectile.type] = true; // Denotes that this projectile is a pet or minion

			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
		}

		public override void SetDefaults() {
			Projectile.Size = new Vector2(40, 40);
			Projectile.tileCollide = false;

			Projectile.friendly = true; // Only controls if it deals damage to enemies on contact (more on that later)
			Projectile.minion = true; // Declares this as a minion (has many effects)
			Projectile.DamageType = DamageClass.Summon; // Declares the damage type (needed for it to deal damage)
			Projectile.minionSlots = 1f; // Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			Projectile.penetrate = -1; // Needed so the minion doesn't despawn on collision with enemies or tiles
			Projectile.timeLeft = 2; // Time left
			Projectile.aiStyle = ProjAIStyleID.Finch; // Finch staff AI
		}
		
		// Setup the counter for how many times have you summoned this minion
		public override void OnSpawn(IEntitySource data) {
			Projectile.ai[0] = 1;
		}
		
		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage
		public override bool MinionContactDamage() {
			return true;
		}
		
		// As this minion uses the Finch AI, both of the things are done in PreAI and PostAI, if you would use custom AI just put them in the AI() code itself
		public override bool PreAI() {
			if (CheckActive(Main.player[Projectile.owner])) {
				SearchForTargets(Main.player[Projectile.owner], out bool ft, out float dft, out Vector2 tc);
				Projectile.damage = Projectile.originalDamage * (int)Projectile.ai[0]; // Basically, originalDamage * how many time you summoned the minion
				return true;
			}

			//Doesnt run the Finch AI code
			return false;
		}
		
		public override void PostAI() {
			Visuals();
		}
		
		// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
		private bool CheckActive(Player owner) {
			if (owner.dead || !owner.active) {
				owner.ClearBuff(ModContent.BuffType<ExampleUpgradeableMinionBuff>());

				return false;
			}

			if (owner.HasBuff(ModContent.BuffType<ExampleUpgradeableMinionBuff>())) {
				Projectile.timeLeft = 2;
			}

			return true;
		}

		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter) {
			// Starting search distance
			distanceFromTarget = 700f;
			targetCenter = Projectile.position;
			foundTarget = false;

			// This code is required if your minion weapon has the targeting feature
			if (owner.HasMinionAttackTargetNPC) {
			    NPC npc = Main.npc[owner.MinionAttackTargetNPC];
			    float between = Vector2.Distance(npc.Center, Projectile.Center);

			    // Reasonable distance away so it doesn't target across multiple screens
			    if (between < 2000f) {
			    	distanceFromTarget = between;
			    	targetCenter = npc.Center;
			    	foundTarget = true;
			    }
			}

			if (!foundTarget) {
				// This code is required either way, used for finding a target
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];

					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 100f;

						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			Projectile.friendly = foundTarget;
		}

		private void Visuals() {
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			// Some visuals here
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
		}
	}
}