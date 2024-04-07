using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ModLoader.Default;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

// Potential improvements:
// * track automatic/ModSystem/etc sends per class or mod
// * sort by most active
// * moving average
// * NetStats console command in ModLoaderMod
// * Separate page for non-vanilla MessageIDs + ModLoaderMod packets

// Mirror of NetDiagnosticsUI, but mod specific
// All code in this class assumes to only run clientside
public class UIModNetDiagnostics : INetDiagnosticsUI
{
	// Copied from vanilla, adjusted
	private struct CounterForMessage
	{
		public int TimesReceived;
		public int TimesSent;
		public int BytesReceived;
		public int BytesSent;

		public void Reset()
		{
			TimesReceived = 0;
			TimesSent = 0;
			BytesReceived = 0;
			BytesSent = 0;
		}

		public void CountReadMessage(int messageLength)
		{
			TimesReceived++;
			BytesReceived += messageLength;
		}

		public void CountSentMessage(int messageLength)
		{
			TimesSent++;
			BytesSent += messageLength;
		}
	}

	private const float TextScale = 0.7f;
	private const string Suffix = ": ";
	private const string ModString = "Mod";
	private const string RxTxString = "Received(#, Bytes)       Sent(#, Bytes)";

	private Asset<DynamicSpriteFont> fontAsset;
	private Asset<DynamicSpriteFont> FontAsset => fontAsset ??= FontAssets.MouseText;

	private readonly Mod[] Mods;
	private CounterForMessage[] CounterByModNetId;
	private int HighestFoundSentBytes = 1;
	private int HighestFoundReadBytes = 1;
	private float FirstColumnWidth;

	public UIModNetDiagnostics(IEnumerable<Mod> mods)
	{
		Mods = mods.Where(mod => mod != ModContent.GetInstance<ModLoaderMod>()).ToArray(); // Need to exclude ModLoaderMod
		Reset();
	}

	public void Reset()
	{
		CounterByModNetId = new CounterForMessage[Mods.Length];

		var font = FontAsset.Value;
		FirstColumnWidth = font.MeasureString(ModString).X; // Default in case no mods are loaded

		for (int i = 0; i < Mods.Length; i++) {
			float length = font.MeasureString(Mods[i].Name).X;
			if (FirstColumnWidth < length)
				FirstColumnWidth = length;
		}

		FirstColumnWidth += font.MeasureString(Suffix).X + 2;
		FirstColumnWidth *= TextScale;
	}

	public void CountReadMessage(int messageId, int messageLength)
	{
		int index = Array.FindIndex(Mods, mod => mod.netID == messageId);
		if (index > -1)
			CounterByModNetId[index].CountReadMessage(messageLength);
	}

	public void CountSentMessage(int messageId, int messageLength)
	{
		int index = Array.FindIndex(Mods, mod => mod.netID == messageId);
		if (index > -1)
			CounterByModNetId[index].CountSentMessage(messageLength);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		int count = CounterByModNetId.Length;
		const int maxLinesPerCol = 50;
		int numCols = (count - 1) / maxLinesPerCol;
		int x = 190;
		int xBuf = x + 10;
		int y = 110;
		int yBuf = y + 10;

		int width = 232;
		// Adjust based on left column width and right column width
		width += (int)(FirstColumnWidth + fontAsset.Value.MeasureString(HighestFoundSentBytes.ToString()).X * TextScale);
		int widthBuf = width + 10;
		int lineHeight = 13;

		for (int i = 0; i <= numCols; i++) {
			int lineCountInCol = i == numCols ? 1 + (count - 1) % maxLinesPerCol : maxLinesPerCol;
			int height = lineHeight * (lineCountInCol + 2);
			int heightBuf = height + 10;
			if (i == 0) {
				Utils.DrawInvBG(spriteBatch, x + widthBuf * i, y - 20, width, heightBuf + 20);
				spriteBatch.DrawString(FontAssets.MouseText.Value, Localization.Language.GetTextValue("tModLoader.PressXToClose", Microsoft.Xna.Framework.Input.Keys.F8), new Vector2(200, 96), Color.White, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
			}
			else {
				Utils.DrawInvBG(spriteBatch, x + widthBuf * i, y, width, heightBuf);
			}

			Vector2 modPos = new Vector2(xBuf + widthBuf * i, yBuf);
			Vector2 headerPos = modPos + new Vector2(FirstColumnWidth, 0);
			DrawText(spriteBatch, RxTxString, headerPos, Color.White);
			DrawText(spriteBatch, ModString, modPos, Color.White);
		}

		Vector2 position = default;
		for (int j = 0; j < count; j++) {
			int colNum = j / maxLinesPerCol;
			int lineNum = j - colNum * maxLinesPerCol;
			position.X = xBuf + colNum * widthBuf;
			position.Y = yBuf + lineHeight + lineNum * lineHeight;

			DrawCounter(spriteBatch, CounterByModNetId[j], Mods[j].Name, position);
		}
	}

	// Copied from vanilla, adjusted
	private void DrawCounter(SpriteBatch spriteBatch, CounterForMessage counter, string title, Vector2 position)
	{
		if (HighestFoundSentBytes < counter.BytesSent)
			HighestFoundSentBytes = counter.BytesSent;

		if (HighestFoundReadBytes < counter.BytesReceived)
			HighestFoundReadBytes = counter.BytesReceived;

		Vector2 pos = position;
		string lineName = title + Suffix;
		float num = Utils.Remap(counter.BytesReceived, 0f, HighestFoundReadBytes, 0f, 1f);
		Color color = Main.hslToRgb(0.3f * (1f - num), 1f, 0.5f);

		string drawText = lineName;
		DrawText(spriteBatch, drawText, pos, color);
		pos.X += FirstColumnWidth;
		drawText = "rx:" + string.Format("{0,0}", counter.TimesReceived);
		DrawText(spriteBatch, drawText, pos, color);
		pos.X += 70f;
		drawText = string.Format("{0,0}", counter.BytesReceived);
		DrawText(spriteBatch, drawText, pos, color);
		pos.X += 70f;
		drawText = "tx:" + string.Format("{0,0}", counter.TimesSent);
		DrawText(spriteBatch, drawText, pos, color);
		pos.X += 70f;
		drawText = string.Format("{0,0}", counter.BytesSent);
		DrawText(spriteBatch, drawText, pos, color);
	}

	private void DrawText(SpriteBatch spriteBatch, string text, Vector2 pos, Color color) => 
		spriteBatch.DrawString(FontAsset.Value, text, pos, color, 0f, Vector2.Zero, TextScale, SpriteEffects.None, 0f);

	// Not needed
	public void CountReadModuleMessage(int moduleMessageId, int messageLength) => throw new NotImplementedException();
	public void CountSentModuleMessage(int moduleMessageId, int messageLength) => throw new NotImplementedException();
}
