using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI.Chat;

namespace Terraria.ModLoader;

[TestClass]
[DoNotParallelize]
public class ChatManagerTests
{
	[ClassInitialize]
	public static void ClassInit(TestContext context) {
		Program.SavePath ??= ".";
		Main.dedServ = true;
	}

	[DataTestMethod]
	[DataRow(255)]
	[DataRow(128)]
	public void ColoredShadowedTextPreservesSnippetColors(int alpha) {
		var snippets = CreateSnippets();
		var color = new Color(255, 255, 255, alpha);

		ChatManager.DrawColorCodedStringWithShadow(null, null, snippets, Vector2.Zero, color, 0f, Vector2.Zero, Vector2.One, out _);

		AssertDraws(snippets, color.MultiplyRGBA(Color.Black));
	}

	[TestMethod]
	public void ShadowedTextWithoutColorPreservesSnippetColors() {
		var snippets = CreateSnippets();

		ChatManager.DrawColorCodedStringWithShadow(null, null, snippets, Vector2.Zero, 0f, Vector2.Zero, Vector2.One, out _);

		AssertDraws(snippets, Color.Black);
	}

	[TestMethod]
	public void ShadowedTextWithCustomShadowPreservesSnippetColors() {
		var snippets = CreateSnippets();
		var shadowColor = new Color(10, 20, 30, 128);

		ChatManager.DrawColorCodedStringWithShadow(null, null, snippets, Vector2.Zero, Color.White, shadowColor, 0f, Vector2.Zero, Vector2.One, out _);

		AssertDraws(snippets, shadowColor);
	}

	[TestMethod]
	public void ColoredShadowedTextPreservesHoveredSnippetIndex() {
		int mouseX = Main.mouseX;
		int mouseY = Main.mouseY;
		try {
			Main.mouseX = 115;
			Main.mouseY = 105;
			var snippets = CreateSnippets();
			snippets[1].CheckForHover = true;

			ChatManager.DrawColorCodedStringWithShadow(null, null, snippets, new Vector2(100, 100), Color.White, 0f, Vector2.Zero, Vector2.One, out int hoveredSnippet);

			Assert.AreEqual(1, hoveredSnippet);
		}
		finally {
			Main.mouseX = mouseX;
			Main.mouseY = mouseY;
		}
	}

	private static RecordingSnippet[] CreateSnippets() => [new(Color.Red), new(Color.Green)];

	private static void AssertDraws(RecordingSnippet[] snippets, Color shadowColor) {
		foreach (var snippet in snippets) {
			Assert.AreEqual(5, snippet.Draws.Count);
			for (int i = 0; i < 4; i++) {
				Assert.AreEqual(shadowColor, snippet.Draws[i].Color);
				Assert.IsTrue(snippet.Draws[i].IsShadow);
			}

			Assert.AreEqual(snippet.Color, snippet.Draws[4].Color, "The foreground must use the snippet's visible color.");
			Assert.IsFalse(snippet.Draws[4].IsShadow);
		}
		Assert.IsFalse(ChatManager.IsDrawingShadow);
	}

	private sealed class RecordingSnippet(Color color) : TextSnippet("probe", color)
	{
		public readonly List<(Color Color, bool IsShadow)> Draws = [];

		public override Color GetVisibleColor() => Color;

		public override bool UniqueDraw(bool justCheckingSize, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f) {
			size = new Vector2(12, 10) * scale;
			if (!justCheckingSize)
				Draws.Add((color, ChatManager.IsDrawingShadow));
			return true;
		}
	}
}
