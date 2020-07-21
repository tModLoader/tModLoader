using ExampleMod.Content.Items;
using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content
{
	public class ExampleLightPetItem : ModItem
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Items/ExampleLightPet";
		
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Annoying Light");
			Tooltip.SetDefault("Summons an annoying light");
		}

		public override void SetDefaults() {
			item.damage = 0;
			item.useStyle = ItemUseStyleID.Swing;
			item.shoot = ProjectileType<ExampleLightPetProjectile>();
			item.width = 16;
			item.height = 30;
			item.UseSound = SoundID.Item2;
			item.useAnimation = 20;
			item.useTime = 20;
			item.rare = ItemRarityID.Yellow;
			item.noMelee = true;
			item.value = Item.sellPrice(0, 5, 50);
			item.buffType = BuffType<ExampleLightPetBuff>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddRecipeGroup(RecipeGroupID.Fireflies, 10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}

		public override void UseStyle(Player player) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(item.buffType, 3600);
			}
		}
	}

	public class ExampleLightPetBuff : ModBuff
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Buffs/ExampleLightPet";

		public override void SetDefaults() {
			DisplayName.SetDefault("Annoying Light");
			Description.SetDefault("Ugh, soooo annoying");
			Main.buffNoTimeDisplay[Type] = true;
			Main.lightPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExampleLightPetPlayer>().ExampleLightPet = true;
			player.buffTime[buffIndex] = 18000;
			int type = ProjectileType<ExampleLightPetProjectile>();
			bool petProjectileNotSpawned = player.ownedProjectileCounts[type] <= 0;
			if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer) {
				Projectile.NewProjectile(player.position.X + (player.width * 0.5f), player.position.Y + (player.height * 0.5f), 0f, 0f, type, 0, 0f, player.whoAmI);
			}

			if (player.controlDown && player.releaseDown) {
				if (player.doubleTapCardinalTimer[0] > 0 && player.doubleTapCardinalTimer[0] != 15) {
					for (int j = 0; j < Main.projectile.Length; j++) {
						if (Main.projectile[j].active && Main.projectile[j].type == type && Main.projectile[j].owner == player.whoAmI) {
							Projectile lightpet = Main.projectile[j];
							Vector2 vectorToMouse = Main.MouseWorld - lightpet.Center;
							lightpet.velocity += 5f * Vector2.Normalize(vectorToMouse);
						}
					}
				}
			}
		}
	}

	public class ExampleLightPetProjectile : ModProjectile
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Projectiles/ExampleLightPet";
		
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Annoying Light");
			Main.projFrames[projectile.type] = 1;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
			ProjectileID.Sets.LightPet[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.width = 30;
			projectile.height = 30;
			projectile.penetrate = -1;
			projectile.netImportant = true;
			projectile.timeLeft *= 5;
			projectile.friendly = true;
			projectile.ignoreWater = true;
			projectile.scale = 0.8f;
			projectile.tileCollide = false;
		}

		private const int FadeInTicks = 30;
		private const int FullBrightTicks = 200;
		private const int FadeOutTicks = 30;
		private const int Range = 500;
		
		// This comes from the formula for calculating the diagonal of a square (a * √2)
		private const int RangeHypoteneus = (int)(1.41421356237f * Range);

		public override void AI() {
			Player player = Main.player[projectile.owner];
			ExampleLightPetPlayer modPlayer = player.GetModPlayer<ExampleLightPetPlayer>();
			if (!player.active) {
				projectile.active = false;
				return;
			}

			if (player.dead) {
				modPlayer.ExampleLightPet = false;
			}

			if (modPlayer.ExampleLightPet) {
				projectile.timeLeft = 2;
			}

			projectile.ai[1]++;
			if (projectile.ai[1] > 1000 && (int)projectile.ai[0] % 100 == 0) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					if (Main.npc[i].active && !Main.npc[i].friendly && player.Distance(Main.npc[i].Center) < RangeHypoteneus) {
						Vector2 vectorToEnemy = Main.npc[i].Center - projectile.Center;
						projectile.velocity += 10f * Vector2.Normalize(vectorToEnemy);
						projectile.ai[1] = 0f;
						// todo: 
						// SoundEngine.PlaySound(SoundLoader.customSoundType, -1, -1, Mod.GetSoundSlot(Terraria.ModLoader.SoundType.Custom, "Sounds/Custom/WatchOut"));
						break;
					}
				}
			}

			projectile.rotation += projectile.velocity.X / 20f;
			Lighting.AddLight(projectile.Center, ((255 - projectile.alpha) * 0.9f) / 255f, ((255 - projectile.alpha) * 0.1f) / 255f, ((255 - projectile.alpha) * 0.3f) / 255f);
			if (projectile.velocity.Length() > 1f) {
				projectile.velocity *= .98f;
			}

			if (projectile.velocity.Length() == 0) {
				projectile.velocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat() * Math.PI * 2);
				projectile.velocity *= 2f;
			}

			projectile.ai[0]++;
			if (projectile.ai[0] < FadeInTicks) {
				projectile.alpha = (int)(255 - ((255 * projectile.ai[0]) / FadeInTicks));
			}
			else if (projectile.ai[0] < FadeInTicks + FullBrightTicks) {
				projectile.alpha = 0;
				if (Main.rand.NextBool(6)) {
					int num145 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 73, 0f, 0f, 200, default, 0.8f);
					Main.dust[num145].velocity *= 0.3f;
				}
			}
			else if (projectile.ai[0] < FadeInTicks + FullBrightTicks + FadeOutTicks) {
				projectile.alpha = (int)((255 * (projectile.ai[0] - FadeInTicks - FullBrightTicks)) / FadeOutTicks);
			}
			else {
				projectile.Center = new Vector2(Main.rand.Next((int)player.Center.X - Range, (int)player.Center.X + Range), Main.rand.Next((int)player.Center.Y - Range, (int)player.Center.Y + Range));
				projectile.ai[0] = 0;
				Vector2 vectorToPlayer = player.Center - projectile.Center;
				projectile.velocity = 2f * Vector2.Normalize(vectorToPlayer);
			}

			if (Vector2.Distance(player.Center, projectile.Center) > RangeHypoteneus) {
				projectile.Center = new Vector2(Main.rand.Next((int)player.Center.X - Range, (int)player.Center.X + Range), Main.rand.Next((int)player.Center.Y - Range, (int)player.Center.Y + Range));
				projectile.ai[0] = 0;
				Vector2 vectorToPlayer = player.Center - projectile.Center;
				projectile.velocity += 2f * Vector2.Normalize(vectorToPlayer);
			}

			if ((int)projectile.ai[0] % 100 == 0) {
				projectile.velocity = projectile.velocity.RotatedByRandom(MathHelper.ToRadians(90));
			}
		}
	}

	public class ExampleLightPetPlayer : ModPlayer
	{
		public bool ExampleLightPet;

		public override void ResetEffects() => ExampleLightPet = false;
	}
}