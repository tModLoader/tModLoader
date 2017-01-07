using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExampleMod.Fonts
{
    public static class FontExtensions
    {
        public static Vector2 DrawString(this SpriteBatch sb, SpriteFontX sfx, string str, Vector2 position, Color color)
        {
            return sfx.Draw(sb, str, position, color);
        }

        public static Vector2 DrawString(this SpriteBatch sb, SpriteFontX sfx, char[] chars, Vector2 position, Color color)
        {
            return sfx.Draw(sb, chars, position, color);
        }

        public static Vector2 DrawString(this SpriteBatch sb, SpriteFontX sfx, string str, Vector2 position, Vector2 maxBound, Vector2 scale, Color color)
        {
            return sfx.Draw(sb, str, position, maxBound, scale, color);
        }

        public static Vector2 DrawString(this SpriteBatch sb, SpriteFontX sfx, char[] chars, Vector2 position, Vector2 maxBound, Vector2 scale, Color color)
        {
            return sfx.Draw(sb, chars, position, maxBound, scale, color);
        }

        public static Vector2 DrawString(this SpriteBatch sb, SpriteFontX sfx, string str, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            return sfx.Draw(sb, str, position - origin, new Vector2(3.40282347E+38f, 3.40282347E+38f), new Vector2(scale, scale), color);
        }
        public static Vector2 DrawString(this SpriteBatch sb, SpriteFontX sfx, string str, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            return sfx.Draw(sb, str, position - origin, new Vector2(3.40282347E+38f, 3.40282347E+38f), scale, color);
        }
    }
}
