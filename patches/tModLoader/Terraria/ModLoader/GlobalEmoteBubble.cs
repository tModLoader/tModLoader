using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader;

public abstract class GlobalEmoteBubble : GlobalType<EmoteBubble, GlobalEmoteBubble>
{
	protected sealed override void Register()
	{
		ModTypeLookup<GlobalEmoteBubble>.Register(this);

		// Index = (ushort)EmoteBubbleLoader.globalEmoteBubbles.Count;

		EmoteBubbleLoader.globalEmoteBubbles.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Gets called when emote bubbles spawn in world.
	/// </summary>
	/// <param name="emoteBubble"></param>
	public virtual void OnSpawn(EmoteBubble emoteBubble) { }

	/// <summary>
	/// Allows you to modify the frame of this emote bubble. Return false to stop vanilla frame update code from running. Returns true by default.
	/// </summary>
	/// <param name="emoteBubble"></param>
	/// <returns>If false, the vanilla frame update code will not run.</returns>
	public virtual bool UpdateFrame(EmoteBubble emoteBubble) => true;

	/// <summary>
	/// Allows you to modify the frame of this emote bubble which displays in emotes menu. Return false to stop vanilla frame update code from running. Returns true by default.
	/// <br/>Do note that 
	/// </summary>
	/// <param name="emoteType">The emote id for this emote.</param>
	/// <param name="frameCounter"></param>
	/// <returns>If false, the vanilla frame update code will not run.</returns>
	public virtual bool UpdateFrameInEmoteMenu(int emoteType, ref int frameCounter) => true;

	/// <summary>
	/// Allows you to draw things behind this emote bubble, or to modify the way this emote bubble is drawn. Return false to stop the game from drawing the emote bubble (useful if you're manually drawing the emote bubble). Returns true by default.
	/// </summary>
	/// <param name="emoteBubble"></param>
	/// <param name="spriteBatch"></param>
	/// <param name="texture"></param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	/// <param name="spriteEffects"></param>
	/// <returns>If false, the vanilla drawing code will not run.</returns>
	public virtual bool PreDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) => true;

	/// <summary>
	/// Allows you to draw things in front of this emote bubble. This method is called even if PreDraw returns false.
	/// </summary>
	/// <param name="emoteBubble"></param>
	/// <param name="spriteBatch"></param>
	/// <param name="texture"></param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	/// <param name="spriteEffects"></param>
	public virtual void PostDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) { }

	/// <summary>
	/// Allows you to draw things behind this emote bubble that displays in emotes menu, or to modify the way this emote bubble is drawn. Return false to stop the game from drawing the emote bubble (useful if you're manually drawing the emote bubble). Returns true by default.
	/// </summary>
	/// <param name="emoteType"></param>
	/// <param name="spriteBatch"></param>
	/// <param name="uiEmoteButton">The <see cref="EmoteButton"/> instance. You can get useful textures and frameCounter from it.</param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	/// <returns>If false, the vanilla drawing code will not run.</returns>
	public virtual bool PreDrawInEmoteMenu(int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) => true;

	/// <summary>
	/// Allows you to draw things in front of this emote bubble. This method is called even if PreDraw returns false.
	/// </summary>
	/// <param name="emoteType"></param>
	/// <param name="spriteBatch"></param>
	/// <param name="uiEmoteButton">The <see cref="EmoteButton"/> instance. You can get useful textures and frameCounter from it.</param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	public virtual void PostDrawInEmoteMenu(int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) { }

	/// <summary>
	/// Allows you to modify the frame rectangle for drawing this emote. Useful for emote bubbles that share the same texture.
	/// </summary>
	/// <param name="emoteBubble"></param>
	/// <returns></returns>
	public virtual Rectangle? GetFrame(EmoteBubble emoteBubble) => null;

	/// <summary>
	/// Allows you to modify the frame rectangle for drawing this emote in emotes menu. Useful for emote bubbles that share the same texture.
	/// </summary>
	/// <param name="emoteType"></param>
	/// <param name="frame"></param>
	/// <param name="frameCounter"></param>
	/// <returns></returns>
	public virtual Rectangle? GetFrameInEmoteMenu(int emoteType, int frame, int frameCounter) => null;
}
