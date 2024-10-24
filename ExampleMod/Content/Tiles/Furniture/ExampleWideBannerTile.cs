using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles.Furniture {
   /// <summary>
   /// Banner, that unlike most vanilla banners, is wider than one tile.
   /// </summary>
   public class ExampleWideBannerTile : ModTile {

      public override void SetStaticDefaults() {
         Main.tileFrameImportant[Type] = true;
         Main.tileNoAttach[Type] = true;
         TileID.Sets.MultiTileSway[Type] = true;

         TileObjectData newTile = TileObjectData.newTile;
         // This default style defaults to 2x3, despite the name.
         newTile.CopyFrom(TileObjectData.Style2xX);
         newTile.LavaDeath = true;
         newTile.Origin = Point16.Zero;
         newTile.AnchorBottom = AnchorData.Empty;
         newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, newTile.Width, 0);
         newTile.DrawYOffset = -2;
         
         // This alternate allows for placing the banner on platforms, just like in vanilla.
         TileObjectData newAlternate = TileObjectData.newAlternate;
         newAlternate.CopyFrom(newTile);
         newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, newTile.Width, 0);
         newAlternate.DrawYOffset = -10;
         
         TileObjectData.addAlternate(0);
         TileObjectData.addTile(Type);
         
         AddMapEntry(Color.White, Language.GetText("MapObject.Banner"));
      }

      public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
         Tile tile = Main.tile[i, j];
         if (tile is {TileFrameX: 0, TileFrameY: 0}) { 
            // Makes our banner sway in the wind and with player interaction, in combo with TileID.Sets.MultiTileSway
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
         }

         // We must return false here, so that TileDrawing takes over and handles the drawing for us. Otherwise, you'll have a static tile drawing at all times.
         return false;
      }
   }
}