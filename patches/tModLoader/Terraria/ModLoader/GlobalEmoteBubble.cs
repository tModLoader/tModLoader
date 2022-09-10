using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI;

namespace Terraria.ModLoader
{
	public abstract class GlobalEmoteBubble : GlobalType<EmoteBubble, GlobalEmoteBubble>
	{
		protected sealed override void Register() {
			ModTypeLookup<GlobalEmoteBubble>.Register(this);

			Index = (ushort)EmoteBubbleLoader.globalEmoteBubbles.Count;

			EmoteBubbleLoader.globalEmoteBubbles.Add(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();
		
		public virtual void OnSpawn(EmoteBubble emoteBubble) { }

		public virtual bool UpdateFrame(EmoteBubble emoteBubble) => true;

		public virtual bool PreDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, SpriteEffects spriteEffects) => true;

		public virtual void PostDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, SpriteEffects spriteEffects) { }
		
		public virtual Rectangle? GetFrame(EmoteBubble emoteBubble) => null;
	}
}
