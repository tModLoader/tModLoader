using ExampleMod.Content.Items;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content
{
	public class ExamplePetItem : ModItem
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Items/ExamplePet";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Paper Airplane");
			Tooltip.SetDefault("Summons a Paper Airplane to follow aimlessly behind you");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.ZephyrFish);
			item.shoot = ProjectileType<ExamplePetProjectile>();
			item.buffType = BuffType<ExamplePetBuff>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}

		public override void UseStyle(Player player) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(item.buffType, 3600);
			}
		}
	}

	public class ExamplePetBuff : ModBuff
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Buffs/ExamplePet";

		public override void SetDefaults() {
			DisplayName.SetDefault("Paper Airplane");
			Description.SetDefault("\"Let this pet be an example to you!\"");
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;
			player.GetModPlayer<ExamplePetPlayer>().ExamplePet = true;
			bool petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<ExamplePetProjectile>()] <= 0;
			if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer) {
				Projectile.NewProjectile(player.position.X + (player.width * 0.5f), player.position.Y + (player.height * 0.5f), 0f, 0f, ProjectileType<ExamplePetProjectile>(), 0, 0f, player.whoAmI);
			}
		}
	}

	public class ExamplePetProjectile : ModProjectile
	{
		public override string Texture => ExampleMod.AssetPath + "Textures/Projectiles/ExamplePet";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Paper Airplane");
			Main.projFrames[projectile.type] = 4;
			Main.projPet[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.ZephyrFish);
			aiType = ProjectileID.ZephyrFish;
		}

		public override bool PreAI() {
			Player player = Main.player[projectile.owner];
			player.zephyrfish = false; // Relic from aiType
			return true;
		}

		public override void AI() {
			Player player = Main.player[projectile.owner];
			ExamplePetPlayer modPlayer = player.GetModPlayer<ExamplePetPlayer>();
			if (player.dead) {
				modPlayer.ExamplePet = false;
			}

			if (modPlayer.ExamplePet) {
				projectile.timeLeft = 2;
			}
		}
	}

	public class ExamplePetPlayer : ModPlayer
	{
		public bool ExamplePet;

		public override void ResetEffects() => ExamplePet = false;
	}
}