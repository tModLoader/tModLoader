using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Common
{
	public class ExamplePlayerDrawLayer : PlayerDrawLayer
	{
		private Asset<Texture2D> exampleItemTexture;

		//Returning true in this property makes this layer appear on the minimap player head icon.
		public override bool IsHeadLayer => true;

		//This sets the layer's parent. Layers don't get drawn if their parent layer is not visible, so smart use of this could help you improve compatibility with other mods.
		public override PlayerDrawLayer Parent => Head;

		public override bool GetDefaultVisiblity(PlayerDrawSet drawInfo) {
			//The layer will be visible only if the player is holding an ExampleItem in their hands. Or if another modder forces this layer to be visible.
			return drawInfo.drawPlayer.HeldItem?.type == ModContent.ItemType<ExampleItem>();

			// If you'd like to reference another PlayerDrawLayer's visibility,
			// you can do so by getting its instance via ModContent.GetInstance<OtherDrawLayer>(), and calling GetDefaultVisiblity on it
		}

		public override IEnumerable<LayerConstraint> GetConstraints() {
			yield return LayerConstraint.After(NeckAcc);
			//The layer will be drawn right before the vanilla 'Head' layer.
			yield return LayerConstraint.Before(Head);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			//The following code draws ExampleItem's texture behind the player's head.

			if (exampleItemTexture == null) {
				exampleItemTexture = ModContent.GetTexture("ExampleMod/Content/Items/ExampleItem");
			}

			var position = drawInfo.Center + new Vector2(0f, -20f) - Main.screenPosition;
			position = new Vector2((int)position.X, (int)position.Y); //You'll sometimes want to do this, to avoid quivering.

			//Queues a drawing of a sprite. Do not use SpriteBatch in drawlayers!
			drawInfo.DrawDataCache.Add(new DrawData(
				exampleItemTexture.Value, //The texture to render.
				position, //Position to render at.
				null, //Source rectangle.
				Color.White, //Color.
				0f, //Rotation.
				exampleItemTexture.Size() * 0.5f, //Origin. Uses the texture's center.
				1f, //Scale.
				SpriteEffects.None, //SpriteEffects.
				0 //'Layer'. This is always 0 in Terraria.
			));
		}
	}
}
