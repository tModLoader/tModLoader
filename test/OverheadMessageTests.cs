using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace Terraria.ModLoader;

[TestClass]
[DoNotParallelize]
public class OverheadMessageTests
{
	private Asset<DynamicSpriteFont> originalFont;
	private DynamicSpriteFont originalDefaultFont;
	private Player originalPlayer;
	private int originalNetMode;
	private bool originalGameMenu;

	[ClassInitialize]
	public static void ClassInit(TestContext context)
	{
		Program.SavePath ??= ".";
		Main.dedServ = true;
		LanguageManager.Instance.SetLanguage("en-US");
		ChatManager.Register<ColorTagHandler>("c");
	}

	[TestInitialize]
	public void Initialize()
	{
		originalFont = FontAssets.MouseText;
		originalDefaultFont = Asset<DynamicSpriteFont>.DefaultValue;
		originalPlayer = Main.player[0];
		originalNetMode = Main.netMode;
		originalGameMenu = Main.gameMenu;

		// Only font metrics are needed to size overhead text; no graphics device or textures.
		var font = new DynamicSpriteFont(0f, 10, '?');
		var glyph = new DynamicSpriteFont.SpriteCharacterData(null, new Rectangle(0, 0, 6, 10), Rectangle.Empty, new Vector3(0, 6, 0));
		typeof(DynamicSpriteFont).GetField("_spriteCharacters", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(font, Array.Empty<DynamicSpriteFont.SpriteCharacterData>());
		typeof(DynamicSpriteFont).GetField("_defaultCharacterData", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(font, glyph);
		FontAssets.MouseText = Asset<DynamicSpriteFont>.Empty;
		Asset<DynamicSpriteFont>.DefaultValue = font;

		Main.player[0] = new Player { name = "sender" };
		// Cache the chat log entry so DisplayMessage can run without a client UI.
		Main.netMode = 1;
		Main.gameMenu = true;
	}

	[TestCleanup]
	public void Cleanup()
	{
		FontAssets.MouseText = originalFont;
		Asset<DynamicSpriteFont>.DefaultValue = originalDefaultFont;
		Main.player[0] = originalPlayer;
		Main.netMode = originalNetMode;
		Main.gameMenu = originalGameMenu;
		ChatHelper.ClearDelayedMessagesCache();
	}

	[DataTestMethod]
	[DataRow(255, 255, 255)]
	[DataRow(255, 0, 0)]
	[DataRow(0, 0, 255)]
	public void DisplayMessagePreservesOverheadBaseColorAndTags(int red, int green, int blue)
	{
		const string text = "hello [c/FF0000:RED] [c/00FF00:GREEN] tail";
		var color = new Color(red, green, blue);

		ChatHelper.DisplayMessage(NetworkText.FromLiteral(text), color, 0);

		var overhead = Main.player[0].chatOverhead;
		Assert.AreEqual(text, overhead.chatText);
		Assert.AreEqual(color, overhead.color);
		Assert.AreEqual(Main.PlayerOverheadChatMessageDisplayTime, overhead.timeLeft);
		Assert.AreEqual(5, overhead.snippets.Length);
		Assert.AreEqual(color, overhead.snippets[0].Color, "Plain overhead text must retain the message's base color.");
		Assert.AreEqual(Color.Red, overhead.snippets[1].Color);
		Assert.AreEqual(color, overhead.snippets[2].Color);
		Assert.AreEqual(new Color(0, 255, 0), overhead.snippets[3].Color);
		Assert.AreEqual(color, overhead.snippets[4].Color);
		Assert.IsTrue(overhead.messageSize.X > 0 && overhead.messageSize.Y > 0);
	}

	[TestMethod]
	public void NewMessageWithoutBaseColorStillDefaultsToWhite()
	{
		var overhead = new Player.OverheadMessage { color = Color.Red };

		overhead.NewMessage("hello", 120);

		Assert.AreEqual(Color.White, overhead.snippets[0].Color);
		Assert.AreEqual(120, overhead.timeLeft);
	}
}