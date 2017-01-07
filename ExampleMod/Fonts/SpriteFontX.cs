
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace ExampleMod.Fonts
{
    public class SpriteFontX
    {
        protected List<Texture2D> Tex2Ds;

        protected IGraphicsDeviceService Gds;

        protected Texture2D CurrentTex2D;

        protected int CurrentTop;

        protected int CurrentLeft;

        protected int CurrentMaxHeight;

        protected SizeF Sizef;

        protected Bitmap Bitmap;

        protected Graphics Graphic;

        private static Bitmap _tempBp;

        private static Graphics _tempGr;

        private static Brush _brush;

        private Font _font;

        private PrivateFontCollection _pfc;

        private readonly object _lockObj = new object();

        private TextRenderingHint _textRenderingHint;

        public Dictionary<char, CharTile> CharTiles;

        public float LineSpacing = 21f;

        public Vector2 Spacing;

        public Font Font
        {
            get
            {
                return _font;
            }
        }

        public SpriteFontX(Font font, IGraphicsDeviceService gds, TextRenderingHint trh)
        {
            Initialize(font, gds, trh);
        }

        private void Initialize(Font font, IGraphicsDeviceService gds, TextRenderingHint trh)
        {
            _font = font;
            Gds = gds;
            _textRenderingHint = trh;
            if (_brush == null)
            {
                _brush = Brushes.White;
                _tempBp = new Bitmap(1, 1);
                _tempGr = Graphics.FromImage(_tempBp);
            }
            CharTiles = new Dictionary<char, CharTile>();
            Tex2Ds = new List<Texture2D>();
            NewTex();
        }
        protected void NewTex()
        {
            CurrentTex2D = new Texture2D(Gds.GraphicsDevice, 256, 256);
            Tex2Ds.Add(CurrentTex2D);
            CurrentTop = 0;
            CurrentLeft = 0;
            CurrentMaxHeight = 0;
        }

        protected void AddTex(char chr)
        {
            lock (_lockObj)
            {
                if (!CharTiles.ContainsKey(chr))
                {
                    string text = chr.ToString();
                    Sizef = _tempGr.MeasureString(text, Font, PointF.Empty, StringFormat.GenericTypographic);
                    if (Sizef.Width <= 0f)
                    {
                        Sizef.Width = Sizef.Height / 2f;
                    }
                    if (Bitmap == null || (int)Math.Ceiling(Sizef.Width) != Bitmap.Width || (int)Math.Ceiling(Sizef.Height) != Bitmap.Height)
                    {
                        Bitmap = new Bitmap((int)Math.Ceiling(Sizef.Width), (int)Math.Ceiling(Sizef.Height), PixelFormat.Format32bppArgb);
                        Graphic = Graphics.FromImage(Bitmap);
                    }
                    else
                    {
                        Graphic.Clear(System.Drawing.Color.Empty);
                    }
                    Graphic.TextRenderingHint = _textRenderingHint;
                    Graphic.DrawString(text, Font, _brush, 0f, 0f, StringFormat.GenericTypographic);
                    if (Bitmap.Height > CurrentMaxHeight)
                    {
                        CurrentMaxHeight = Bitmap.Height;
                    }
                    if (CurrentLeft + Bitmap.Width + 1 > CurrentTex2D.Width)
                    {
                        CurrentTop += CurrentMaxHeight + 1;
                        CurrentLeft = 0;
                    }
                    if (CurrentTop + CurrentMaxHeight > CurrentTex2D.Height)
                    {
                        NewTex();
                    }
                    CharTile charTile = new CharTile(CurrentTex2D, new Microsoft.Xna.Framework.Rectangle(CurrentLeft, CurrentTop, Bitmap.Width, Bitmap.Height));
                    CharTiles.Add(chr, charTile);
                    int[] imageArr = new int[Bitmap.Width * Bitmap.Height];
                    for (int i = 0; i < imageArr.Length; i++)
                    {
                        imageArr[i] = Bitmap.GetPixel(i % Bitmap.Width, i / Bitmap.Width).ToArgb();
                    }
                    Gds.GraphicsDevice.Textures[0] = null;
                    CurrentTex2D.SetData(0, charTile.Rectangle, imageArr, 0, imageArr.Length);
                    CurrentLeft += charTile.Rectangle.Width + 1;
                }
            }
        }

        public Vector2 Draw(SpriteBatch sb, string str, Vector2 position, Microsoft.Xna.Framework.Color color)
        {
            return Draw(sb, str.ToCharArray(), position, new Vector2(3.40282347E+38f, 3.40282347E+38f), Vector2.One, color);
        }

        public Vector2 Draw(SpriteBatch sb, char[] chars, Vector2 position, Microsoft.Xna.Framework.Color color)
        {
            return Draw(sb, chars, position, new Vector2(3.40282347E+38f, 3.40282347E+38f), Vector2.One, color);
        }

        public Vector2 Draw(SpriteBatch sb, string str, Vector2 position, Vector2 maxBound, Vector2 scale, Microsoft.Xna.Framework.Color color)
        {
            return Draw(sb, str.ToCharArray(), position, maxBound, scale, color);
        }

        public Vector2 Draw(SpriteBatch sb, char[] chars, Vector2 position, Vector2 maxBound, Vector2 scale, Microsoft.Xna.Framework.Color color)
        {
            maxBound = new Vector2((maxBound.X == 0f) ? (maxBound.X = 3.40282347E+38f) : (maxBound.X += position.X), (maxBound.Y == 0f) ? (maxBound.Y = 3.40282347E+38f) : (maxBound.Y += position.Y));
            Vector2 pos = position;
            float MaxY = 0f;
            float maxX = 0f;
            int i = 0;
            int length = chars.Length;
            while (i < length)
            {
                char c = chars[i];
                AddTex(c);
                CharTile charTile = CharTiles[c];
                if (c == '\r' || pos.X + charTile.Rectangle.Width * scale.X > maxBound.X)
                {
                    if (pos.X > maxX)
                    {
                        maxX = pos.X;
                    }
                    pos.X = position.X;
                    pos.Y += MaxY * scale.Y + Spacing.Y * scale.X;
                    MaxY = 0f;
                }
                else if (c != '\n')
                {
                    if (charTile.Rectangle.Height > MaxY)
                    {
                        MaxY = charTile.Rectangle.Height;
                        if (pos.Y + MaxY * scale.Y > maxBound.Y)
                        {
                            break;
                        }
                    }
                    if (sb != null)
                    {
                        sb.Draw(charTile.Texture, pos, charTile.Rectangle, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                    pos.X += charTile.Rectangle.Width * scale.X + Spacing.X * scale.X;
                }
                i++;
            }
            if (pos.X > maxX)
            {
                maxX = pos.X;
            }
            pos = new Vector2(maxX - Spacing.X * scale.X, pos.Y + MaxY * scale.Y);
            return pos - position;
        }

        public Vector2 MeasureString(string str)
        {
            return MeasureString(str.ToCharArray());
        }

        public Vector2 MeasureString(char[] chars)
        {
            return Draw(null, chars, Vector2.Zero, Microsoft.Xna.Framework.Color.White);
        }
    }
}
