using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class ModEmoteBubble : ModType<EmoteBubble, ModEmoteBubble>
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
		public EmoteBubble EmoteBubble { get; internal set; }

		/// <summary>
		/// This is the translation that is used behind <see cref="DisplayName"/>. The translation will show up when hovering over this emote in the emotes menu. This is the name of the emote command as well.
		/// </summary>
		public ModTranslation EmoteName { get; internal set; }

		/// <summary>
		/// This is the name that will show up when hovering over this emote. This is the name of the emote command as well.
		/// </summary>
		public string DisplayName => DisplayNameInternal;

		internal protected virtual string DisplayNameInternal => EmoteName.GetTranslation(Language.ActiveCulture);


		public sealed override void SetupContent() {
			ModContent.Request<Texture2D>(Texture);
			SetStaticDefaults();
		}

		protected sealed override void Register() {
			EmoteName = LocalizationLoader.GetOrCreateTranslation(Mod, $"EmoteName.{Name}");

			ModTypeLookup<ModEmoteBubble>.Register(this);

			Type = EmoteBubbleLoader.Add(this);
		}

		protected override EmoteBubble CreateTemplateEntity() => new(Type, new WorldUIAnchor()) { ModEmoteBubble = this };

		public virtual bool IsUnlocked() => true;

		public virtual void OnSpawn() { }

		public virtual bool UpdateFrame() => true;

		public virtual bool PreDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, SpriteEffects spriteEffects) => true;

		public virtual void PostDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, SpriteEffects spriteEffects) { }
		
		public virtual Rectangle? GetFrame() => null;

		public virtual Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) => null;
	}
}
