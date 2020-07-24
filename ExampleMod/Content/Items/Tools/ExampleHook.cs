using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Tools
{
	internal class ExampleHookItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Hook");
		}

		public override void SetDefaults() {
			// Copy values from the Amethyst Hook
			item.CloneDefaults(ItemID.AmethystHook);
			item.shootSpeed = 18f; // how quickly the hook is shot.
			item.shoot = ProjectileType<ExampleHookProjectile>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(100)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}

	internal class ExampleHookProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("${ProjectileName.GemHookAmethyst}");
		}

		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
		}

		// Use this hook for hooks that can have multiple hooks mid-flight: Dual Hook, Web Slinger, Fish Hook, Static Hook, Lunar Hook
		public override bool? CanUseGrapple(Player player) {
			int hooksOut = 0;
			for (int l = 0; l < 1000; l++) {
				if (Main.projectile[l].active && Main.projectile[l].owner == Main.myPlayer && Main.projectile[l].type == projectile.type) {
					hooksOut++;
				}
			}

			return hooksOut <= 2;
		}

		// Return true if it is like: Hook, CandyCaneHook, BatHook, GemHooks
		//public override bool? SingleGrappleHook(Player player)
		//{
		//	return true;
		//}

		// Use this to kill oldest hook. For hooks that kill the oldest when shot, not when the newest latches on: Like SkeletronHand
		// You can also change the projectile like: Dual Hook, Lunar Hook
		//public override void UseGrapple(Player player, ref int type)
		//{
		//	int hooksOut = 0;
		//	int oldestHookIndex = -1;
		//	int oldestHookTimeLeft = 100000;
		//	for (int i = 0; i < 1000; i++)
		//	{
		//		if (Main.projectile[i].active && Main.projectile[i].owner == projectile.whoAmI && Main.projectile[i].type == projectile.type)
		//		{
		//			hooksOut++;
		//			if (Main.projectile[i].timeLeft < oldestHookTimeLeft)
		//			{
		//				oldestHookIndex = i;
		//				oldestHookTimeLeft = Main.projectile[i].timeLeft;
		//			}
		//		}
		//	}
		//	if (hooksOut > 1)
		//	{
		//		Main.projectile[oldestHookIndex].Kill();
		//	}
		//}

		// Amethyst Hook is 300, Static Hook is 600
		public override float GrappleRange() {
			return 500f;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks) {
			numHooks = 2;
		}

		// default is 11, Lunar is 24
		public override void GrappleRetreatSpeed(Player player, ref float speed) {
			speed = 18f;
		}

		public override void GrapplePullSpeed(Player player, ref float speed) {
			speed = 10;
		}

		private Asset<Texture2D> chainTexture = GetTexture("ExampleMod/Content/Items/Tools/ExampleHookChain");

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			Vector2 playerCenter = Main.player[projectile.owner].MountedCenter;
			Vector2 center = projectile.Center;
			Vector2 distToProj = playerCenter - projectile.Center;
			float projRotation = distToProj.ToRotation() - MathHelper.PiOver2;
			float distance = distToProj.Length();
			while (distance > 30f && !float.IsNaN(distance)) {
				distToProj.Normalize(); //get unit vector
				distToProj *= 24f; //speed = 24
				center += distToProj; //update draw position
				distToProj = playerCenter - center; //update distance
				distance = distToProj.Length();
				Color drawColor = lightColor;

				//Draw chain
				spriteBatch.Draw(chainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
					new Rectangle(0, 0, chainTexture.Width(), chainTexture.Height()), drawColor, projRotation,
					chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
			}

			return true;
		}
	}
}