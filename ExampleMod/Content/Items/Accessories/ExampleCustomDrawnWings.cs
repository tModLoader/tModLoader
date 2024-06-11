using ExampleMod.Common.Configs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Wings)]
	public class ExampleCustomDrawnWings : ModItem
	{
		private int _wingFrame;
		private int _wingFrameCount;
		private int _frameCounter;
		private int _animationDelay;

		private Texture2D wingTexture;
		private Vector2 _wingFrameSize;


		// To see how this config option was added, see ExampleModConfig.cs
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<ExampleModConfig>().ExampleWingsToggle;
		}

		public override void SetStaticDefaults() {
			// These wings use the same values as the solar wings
			// Fly time: 180 ticks = 3 seconds
			// Fly speed: 9
			// Acceleration multiplier: 2.5
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(180, 9f, 2.5f);
		}

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 20;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;

			_animationDelay = 8;

			wingTexture = TextureAssets.Wings[Item.wingSlot].Value;
			_wingFrameCount = 4;
			_wingFrameSize = new Vector2(wingTexture.Width, wingTexture.Height / _wingFrameCount);
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f; // Falling glide speed
			ascentWhenRising = 0.15f; // Rising speed
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.135f;
		}

		public override bool PreDrawWings(ref PlayerDrawSet drawInfo) {
			// The Floor() call is important to avoid jittering.
			var position = (drawInfo.Position - Main.screenPosition).Floor();
			// Center on the player
			position += drawInfo.drawPlayer.Size / 2;
			// Add a little offset, making sure to respect the players direcion.
			position += (new Vector2(-7, 4) * drawInfo.drawPlayer.Directions);


			if (!drawInfo.drawPlayer.ShouldDrawWingsThatAreAlwaysAnimated()) {
				_wingFrame = 0;
			}
			else if (!WingsInUse()) {
				_wingFrame = 2;
			}
			else if (_frameCounter++ == _animationDelay) {
				_wingFrame++;
				_wingFrame %= _wingFrameCount;
				_frameCounter = 0;
			}

			var sourceRect = wingTexture.Frame(1, _wingFrameCount, 0, _wingFrame);

			var drawData = new DrawData(
				wingTexture,
				position,
				sourceRect,
				Color.White
			);

			drawData.origin = _wingFrameSize / 2;
			drawData.effect = drawInfo.playerEffect;


			drawInfo.DrawDataCache.Add(drawData);

			// Disable vanilla wings drawing
			return false;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.SortBefore(Main.recipe.First(recipe =>
					recipe.createItem.wingSlot !=
					-1)) // Places this recipe before any wing so every wing stays together in the crafting menu.
				.Register();
		}

		// This is how vanilla determines whether the wings are currently 'in use'. This is the same as the parameter passed in 'wingUpdate()'.
		private bool WingsInUse() {
			var player = Main.CurrentPlayer;

			return (player.controlJump && player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f);
		}
	}
}