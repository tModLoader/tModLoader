using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class BooleanElement : ConfigElement<bool>
{
	private Asset<Texture2D> _toggleTexture;

	// TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
	public override void OnBind()
	{
		base.OnBind();
		_toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

		OnLeftClick += (ev, v) => Value = !Value;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		CalculatedStyle dimensions = base.GetDimensions();
		// "Yes" and "No" since no "True" and "False" translation available
		Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Value ? Lang.menu[126].Value : Lang.menu[124].Value, new Vector2(dimensions.X + dimensions.Width - 60, dimensions.Y + 8f), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
		Rectangle sourceRectangle = new Rectangle(Value ? ((_toggleTexture.Width() - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width() - 2) / 2, _toggleTexture.Height());
		Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
		spriteBatch.Draw(_toggleTexture.Value, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
	}
}
