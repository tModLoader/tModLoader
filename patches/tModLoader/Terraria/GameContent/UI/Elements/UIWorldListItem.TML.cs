using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.IO;
using Terraria.UI;
using Terraria.Utilities;

namespace Terraria.GameContent.UI.Elements;

public partial class UIWorldListItem : AWorldListItem
{
	private ulong _fileSize;
	private Asset<Texture2D> _configTexture;

	private void InitializeTmlFields(WorldFileData data)
	{
		_fileSize = (ulong)FileUtilities.GetFileSize(data.Path, data.IsCloudSave);
	}

	private void LoadTmlTextures()
	{
		_configTexture = ModLoader.UI.UICommon.ButtonConfigTexture;
	}

	private void AddTmlElements(WorldFileData data, ref float offset)
	{
		/*
		if (ConfigManager.Configs.Count > 0) {
			UIImageButton configButton = new UIImageButton(_configTexture);
			configButton.VAlign = 1f;
			configButton.Left.Set(offset, 0f);
			configButton.OnClick += new UIElement.MouseEvent(ConfigButtonClick);
			configButton.OnMouseOver += new UIElement.MouseEvent(ConfigMouseOver);
			configButton.OnMouseOut += new UIElement.MouseEvent(ButtonMouseOut);
			Append(configButton);
			offset += 24f;
		}
		*/
	}

	internal static Action PlayReload()
	{
		// Main.ActivePlayerFileData gets cleared during reload
		string path = Main.ActivePlayerFileData.Path;
		bool isCloudSave = Main.ActivePlayerFileData.IsCloudSave;

		return () => {
			// Re-select the current player
			Player.GetFileData(path, isCloudSave).SetAsActive();
			WorldGen.playWorld();
		};
	}

	private void ConfigMouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		_buttonLabel.SetText("Edit World Config");
	}

	private void ConfigButtonClick(UIMouseEvent evt, UIElement listeningElement)
	{

	}
}
