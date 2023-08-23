using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// Represents an emote. Emotes are typically used by players or NPC. Players can use the emotes menu or chat commands to display an emote, while town NPC spawn emotes when talking to each other.<para/>
/// </summary>
public abstract class ModEmoteBubble : ModType<EmoteBubble, ModEmoteBubble>, ILocalizedModType
{
	/// <summary>
	/// The file name of this emote's texture file in the mod loader's file space.
	/// </summary>
	public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

	/// <summary>
	/// The internal ID of this EmoteBubble.
	/// </summary>
	public int Type { get; internal set; }

	/// <summary>
	/// This is the <see cref="EmoteBubble"/> instance.
	/// </summary>
	public EmoteBubble EmoteBubble => Entity;

	public virtual string LocalizationCategory => "Emotes";

	/// <summary>
	/// This is the name that will show up as the emote command.
	/// </summary>
	public virtual LocalizedText Command => this.GetLocalization(nameof(Command), () => Name.ToLower());

	public sealed override void SetupContent()
	{
		ModContent.Request<Texture2D>(Texture);
		SetStaticDefaults();
	}

	protected sealed override void Register()
	{
		ModTypeLookup<ModEmoteBubble>.Register(this);
		Type = EmoteBubbleLoader.Add(this);
	}

	protected override EmoteBubble CreateTemplateEntity() => new(Type, new WorldUIAnchor()) { ModEmoteBubble = this };

	/// <summary>
	/// Allows you to add this emote to a specific vanilla category.
	/// <br><b>This should only be called in <see cref="ModType.SetStaticDefaults"/></b></br>
	/// </summary>
	/// <param name="categoryId">The category to which this emote will be added. Use <see cref="EmoteID.Category"/> to select the category you want.</param>
	public void AddToCategory(int categoryId)
	{
		if (!EmoteBubbleLoader.categoryEmoteLookup.TryGetValue(categoryId, out var list)) {
			EmoteBubbleLoader.categoryEmoteLookup.Add(categoryId, new() { this });
			return;
		}
		list.Add(this);
	}

	/// <summary>
	/// Allows you to determine whether or not this emote can be seen in emotes menu. Returns true by default.
	/// <br/>Do note that this doesn't effect emote command and NPC using.
	/// </summary>
	/// <returns>If true, this emote will be shown in emotes menu.</returns>
	public virtual bool IsUnlocked() => true;

	/// <summary>
	/// Gets called when your emote bubble spawns in world.
	/// </summary>
	public virtual void OnSpawn() { }

	/// <summary>
	/// Allows you to modify the frame of this emote bubble. Return false to stop vanilla frame update code from running. Returns true by default.
	/// </summary>
	/// <returns>If false, the vanilla frame update code will not run.</returns>
	public virtual bool UpdateFrame() => true;

	/// <summary>
	/// Allows you to modify the frame of this emote bubble which displays in emotes menu. Return false to stop vanilla frame update code from running. Returns true by default.
	/// <br/>Do note that you should <b>NEVER</b> use the <see cref="EmoteBubble"/> field in this method because it's null.
	/// </summary>
	/// <param name="frameCounter"></param>
	/// <returns>If false, the vanilla frame update code will not run.</returns>
	public virtual bool UpdateFrameInEmoteMenu(ref int frameCounter) => true;

	/// <summary>
	/// Allows you to draw things behind this emote bubble, or to modify the way this emote bubble is drawn. Return false to stop the game from drawing the emote bubble (useful if you're manually drawing the emote bubble). Returns true by default.
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="texture"></param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	/// <param name="spriteEffects"></param>
	/// <returns>If false, the vanilla drawing code will not run.</returns>
	public virtual bool PreDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) => true;

	/// <summary>
	/// Allows you to draw things in front of this emote bubble. This method is called even if PreDraw returns false.
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="texture"></param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	/// <param name="spriteEffects"></param>
	public virtual void PostDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) { }

	/// <summary>
	/// Allows you to draw things behind this emote bubble that displays in emotes menu, or to modify the way this emote bubble is drawn. Return false to stop the game from drawing the emote bubble (useful if you're manually drawing the emote bubble). Returns true by default.
	/// <br/>Do note that you should <b>NEVER</b> use the <see cref="EmoteBubble"/> field in this method because it's null.
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="uiEmoteButton">The <see cref="EmoteButton"/> instance. You can get useful textures and frameCounter from it.</param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	/// <returns>If false, the vanilla drawing code will not run.</returns>
	public virtual bool PreDrawInEmoteMenu(SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) => true;

	/// <summary>
	/// Allows you to draw things in front of this emote bubble. This method is called even if PreDraw returns false.
	/// <br/>Do note that you should <b>NEVER</b> use the <see cref="EmoteBubble"/> field in this method because it's null.
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="uiEmoteButton">The <see cref="EmoteButton"/> instance. You can get useful textures and frameCounter from it.</param>
	/// <param name="position"></param>
	/// <param name="frame"></param>
	/// <param name="origin"></param>
	public virtual void PostDrawInEmoteMenu(SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) { }

	/// <summary>
	/// Allows you to modify the frame rectangle for drawing this emote. Useful for emote bubbles that share the same texture.
	/// </summary>
	/// <returns></returns>
	public virtual Rectangle? GetFrame() => null;

	/// <summary>
	/// Allows you to modify the frame rectangle for drawing this emote in emotes menu. Useful for emote bubbles that share the same texture.
	/// </summary>
	/// <param name="frame"></param>
	/// <param name="frameCounter"></param>
	/// <returns></returns>
	public virtual Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) => null;
}
