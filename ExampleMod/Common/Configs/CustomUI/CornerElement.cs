using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

// ATTENTION: Below this point is custom config UI element.
// Be aware that mods using custom config elements will break with the next few tModLoader updates until their design is finalized.
// You will need to be very active in updating your mod if you use these as they can break in any update.

// This file defines a custom ConfigElement based on Corner enum
// with custom drawing implemented that can be used in ModConfig classes.
namespace ExampleMod.Common.Configs.CustomUI
{
	// This custom config UI element shows a completely custom config element that handles setting and getting the values in addition to custom drawing.
	[JsonConverter(typeof(StringEnumConverter))]
	[CustomModConfigItem(typeof(CornerElement))]
	public enum Corner
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	class CornerElement : ConfigElement
	{
		Texture2D circleTexture;
		string[] valueStrings;

		public override void OnBind() {
			base.OnBind();
			circleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			valueStrings = Enum.GetNames(MemberInfo.Type);
			TextDisplayFunction = () => Label + ": " + GetStringValue();
		}

		void SetValue(Corner value) => SetObject(value);

		Corner GetValue() => (Corner)GetObject();

		string GetStringValue() {
			return valueStrings[(int)GetValue()];
		}

		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);
			SetValue(GetValue().NextEnum());
		}

		public override void RightClick(UIMouseEvent evt) {
			base.RightClick(evt);
			SetValue(GetValue().PreviousEnum());
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			CalculatedStyle dimensions = GetDimensions();
			var circleSourceRectangle = new Rectangle(0, 0, (circleTexture.Width - 2) / 2, circleTexture.Height);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(dimensions.X + dimensions.Width - 25), (int)(dimensions.Y + 4), 22, 22), Color.LightGreen);
			Corner corner = GetValue();
			var circlePositionOffset = new Vector2((int)corner % 2 * 8, (int)corner / 2 * 8);
			spriteBatch.Draw(circleTexture, new Vector2(dimensions.X + dimensions.Width - 25, dimensions.Y + 4) + circlePositionOffset, circleSourceRectangle, Color.White);
		}
	}
}
