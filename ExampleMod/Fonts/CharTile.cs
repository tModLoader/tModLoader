using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExampleMod.Fonts
{
    public struct CharTile
    {
        public Rectangle Rectangle;
        public Texture2D Texture;
        public CharTile(Texture2D tex, Rectangle rect)
        {
            Texture = tex;
            Rectangle = rect;
        }
    }
}
