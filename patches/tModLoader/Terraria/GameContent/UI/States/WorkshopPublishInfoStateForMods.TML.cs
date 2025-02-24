using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.Social;
using Terraria.Social.Base;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.Utilities;

namespace Terraria.GameContent.UI.States;

public class WorkshopPublishInfoStateForMods : AWorkshopPublishInfoState<TmodFile>
{
	public const string TmlRules = "https://forums.terraria.org/index.php?threads/player-created-game-enhancements-rules-guidelines.286/";

	private readonly NameValueCollection _buildData;
	protected UIText imageWarningText;
	internal string changeNotes;

	public WorkshopPublishInfoStateForMods(UIState stateToGoBackTo, TmodFile modFile, NameValueCollection buildData)
		: base(stateToGoBackTo, modFile)
	{
		_instructionsTextKey = "Workshop.ModPublishDescription";
		_publishedObjectNameDescriptorTexKey = "Workshop.ModName";
		_buildData = buildData;
		_previewImagePath = buildData["iconpath"];
		changeNotes = buildData["changelog"];
	}

	protected override string GetPublishedObjectDisplayName()
	{
		return _dataObject.Name;
	}

	protected override void GoToPublishConfirmation()
	{
		// If needed, create a resized image and use it instead.
		bool resizedPreviewImage = false;
		if (CheckPreviewImageNeedsResizing(out _, out int newWidth, out int newHeight)) {
			string srcPath = _previewImagePath;
			string dstPath = Path.Combine(Path.GetTempPath(), "icon_workshop.png." + DateTime.Now.Ticks);
			UpscaleAndSaveImageAsPng(srcPath, dstPath, newWidth, newHeight);
			_previewImagePath = dstPath;
			resizedPreviewImage = true;
		}

		/* if ( SocialAPI.Workshop != null) */
		SocialAPI.Workshop.PublishMod(_dataObject, _buildData, GetPublishSettings());

		if (Main.MenuUI.CurrentState?.GetType() != typeof(UIReportsPage)) {
			// Copy the used preview image to the mod's source directory if it's not a resize and if one isn't there already.
			string iconWorkshopPath = Path.Combine(_buildData["sourcesfolder"], "icon_workshop.png");
			if (_previewImagePath != iconWorkshopPath && !resizedPreviewImage && !File.Exists(iconWorkshopPath)) {
				try {
					File.Copy(_previewImagePath, iconWorkshopPath, overwrite: true);
					_previewImagePath = iconWorkshopPath;
				}
				catch { }
			}

			Main.menuMode = 888;
			Main.MenuUI.SetState(_previousUIState);
		}
	}

	protected override List<WorkshopTagOption> GetTagsToShow() => SteamedWraps.ModTags;

	protected override bool TryFindingTags(out FoundWorkshopEntryInfo info) => SocialAPI.Workshop.TryGetInfoForMod(_dataObject, out info);

	internal UIElement CreateTmlDisclaimer(string tagGroup)
	{
		float num = 60f;
		float num2 = 0f + num;

		GroupOptionButton<bool> groupOptionButton = new GroupOptionButton<bool>(option: true, null, null, Color.White, null, 1f, 0.5f, 16f) {
			HAlign = 0.5f,
			VAlign = 0f,
			Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
			Left = StyleDimension.FromPixels(0f),
			Height = StyleDimension.FromPixelsAndPercent(num2 + 4f, 0f),
			Top = StyleDimension.FromPixels(0f),
			ShowHighlightWhenSelected = false
		};

		groupOptionButton.SetCurrentOption(option: false);
		groupOptionButton.Width.Set(0f, 1f);

		UIElement uIElement = new UIElement {
			HAlign = 0.5f,
			VAlign = 1f,
			Width = new StyleDimension(0f, 1f),
			Height = new StyleDimension(num, 0f)
		};

		groupOptionButton.Append(uIElement);

		UIText uIText = new UIText(Language.GetText("tModLoader.WorkshopDisclaimer")) {
			HAlign = 0f,
			VAlign = 0f,
			Width = StyleDimension.FromPixelsAndPercent(-40f, 1f),
			Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
			TextColor = Color.Cyan,
			IgnoresMouseInteraction = true
		};

		uIText.PaddingLeft = 20f;
		uIText.PaddingRight = 20f;
		uIText.PaddingTop = 4f;
		uIText.IsWrapped = true;

		_tMLDisclaimerText = uIText;

		groupOptionButton.OnLeftClick += TmlDisclaimerText_OnClick;
		groupOptionButton.OnMouseOver += TmlDisclaimerText_OnMouseOver;
		groupOptionButton.OnMouseOut += TmlDisclaimerText_OnMouseOut;

		uIElement.Append(uIText);
		uIText.SetSnapPoint(tagGroup, 0);

		_tMLDisclaimerButton = uIText;

		return groupOptionButton;
	}

	private void TmlDisclaimerText_OnMouseOut(UIMouseEvent evt, UIElement listeningElement)
	{
		_tMLDisclaimerText.TextColor = Color.Cyan;
		ClearOptionDescription(evt, listeningElement);
	}

	private void TmlDisclaimerText_OnMouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(12);
		_tMLDisclaimerText.TextColor = Color.LightCyan;
		ShowOptionDescription(evt, listeningElement);
	}

	private void TmlDisclaimerText_OnClick(UIMouseEvent evt, UIElement listeningElement) =>	Utils.OpenToURL(TmlRules);

	public override void OnInitialize()
	{
		base.OnInitialize();

		// Update Localization Tags Automatically if the mod is loaded. (Can only publish if enabled, but just in case.)
		if (ModLoader.ModLoader.TryGetMod(_dataObject.Name, out ModLoader.Mod mod)) {
			var localizationCounts = ModLoader.LocalizationLoader.GetLocalizationCounts(mod);
			int countMaxEntries = localizationCounts.DefaultIfEmpty().Max(x => x.Value);
			ModLoader.Logging.tML.Info($"Determining localization progress for {mod.Name}:");
			foreach (GroupOptionButton<WorkshopTagOption> tagOption in _tagOptions) {
				if (tagOption.OptionValue.NameKey.StartsWith("tModLoader.TagsLanguage_")) {
					// I couldn't see any other way to convert this.
					var culture = tagOption.OptionValue.NameKey.Split('_')[1] switch {
						"English" => GameCulture.FromName("en-US"),
						"Spanish" => GameCulture.FromName("es-ES"),
						"French" => GameCulture.FromName("fr-FR"),
						"Italian" => GameCulture.FromName("it-IT"),
						"Russian" => GameCulture.FromName("ru-RU"),
						"Chinese" => GameCulture.FromName("zh-Hans"),
						"Portuguese" => GameCulture.FromName("pt-BR"),
						"German" => GameCulture.FromName("de-DE"),
						"Polish" => GameCulture.FromName("pl-PL"),
						_ => throw new NotImplementedException(),
					};

					int countOtherEntries;
					localizationCounts.TryGetValue(culture, out countOtherEntries);
					float localizationProgress = (float)countOtherEntries / countMaxEntries;
					ModLoader.Logging.tML.Info($"{culture.Name}, {countOtherEntries}/{countMaxEntries}, {localizationProgress:P0}, missing {countMaxEntries - countOtherEntries}");

					bool languageMostlyLocalized = localizationProgress > 0.75f; // 75% Threshold to be localized.
					bool languagePreviouslyLocalizedAndStillEnough = tagOption.IsSelected && localizationProgress > 0.5f; // If mod previously tagged as localized, persist selection as long as above 50%

					// Override existing selection. Existing selection will persist if still above 50% to accommodate temporarily falling below threshold.
					tagOption.SetCurrentOption(languageMostlyLocalized || languagePreviouslyLocalizedAndStillEnough ? tagOption.OptionValue : null);
					// Automatically set option slightly redder, indicating it was automatically selected. Even redder if below 75%
					tagOption.SetColor(tagOption.IsSelected ? (languageMostlyLocalized ? new Color(192, 175, 235) : new Color(255, 175, 235)) : Colors.InventoryDefaultColor, 1f);
				}
			}
		}
	}

	internal void AddNonModOwnerPublishWarning(UIList uiList)
	{
		var query = new QueryParameters() {
			searchModSlugs = new string[] { _dataObject.Name },
			queryType = QueryType.SearchDirect
		};

		if (!WorkshopHelper.TryGetModDownloadItem(_dataObject.Name, out var mod) || mod == null) {
			return;
		}

		ulong existingAuthorID = ulong.Parse(mod.OwnerId);
		if (existingAuthorID == 0 || existingAuthorID == Steamworks.SteamUser.GetSteamID().m_SteamID) {
			return;
		}

		float num = 180f;
		float num2 = 0f + num;

		GroupOptionButton<bool> groupOptionButton = new GroupOptionButton<bool>(option: true, null, null, Color.White, null, 1f, 0.5f, 16f) {
			HAlign = 0.5f,
			VAlign = 0f,
			Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
			Left = StyleDimension.FromPixels(0f),
			Height = StyleDimension.FromPixelsAndPercent(num2 + 4f, 0f),
			Top = StyleDimension.FromPixels(0f),
			ShowHighlightWhenSelected = false
		};

		groupOptionButton.SetCurrentOption(option: false);
		groupOptionButton.Width.Set(0f, 1f);

		UIElement uIElement = new UIElement {
			HAlign = 0.5f,
			VAlign = 1f,
			Width = new StyleDimension(0f, 1f),
			Height = new StyleDimension(num, 0f)
		};
		uIElement.OnLeftClick += (sender, e) => Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/Workshop#renaming-a-mod");

		groupOptionButton.Append(uIElement);

		UIText uIText = new UIText(Language.GetTextValue("tModLoader.NonModOwnerPublishWarning", _dataObject.Name)) {
			HAlign = 0f,
			VAlign = 0f,
			Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
			Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
			TextColor = Color.Yellow,
			IgnoresMouseInteraction = true
		};

		uIText.PaddingLeft = 20f;
		uIText.PaddingRight = 20f;
		uIText.PaddingTop = 4f;
		uIText.IsWrapped = true;

		uIElement.Append(uIText);
		uIText.SetSnapPoint("warning", 0);
		uiList.Add(groupOptionButton);
	}

	protected override void UpdateImagePreview()
	{
		base.UpdateImagePreview();

		if (imageWarningText == null) {
			imageWarningText = new UIText(string.Empty) {
				TextOriginX = 0f,
				TextOriginY = 0.5f,
				Width = _previewImagePathPlate.Width,
				Height = new(0f, 0f),
				Left = new(10f, 0f),
				Top = new(10f, 0f), //Top = new(0f, 0.675f),
				TextColor = new Color(106, 190, 48),
			};
			_previewImagePathPlate.Parent.Append(imageWarningText);
		}

		// Display a warning that the preview image will be resized.
		if (CheckPreviewImageNeedsResizing(out var tex, out int newWidth, out int newHeight)) {
			imageWarningText.SetText(Language.GetTextValue("tModLoader.ModWorkshopIconResizeWarning", $"{tex.Width}x{tex.Height}", $"{newWidth}x{newHeight}"));
		}
		else {
			imageWarningText.SetText(string.Empty);
		}
	}

	private bool CheckPreviewImageNeedsResizing(out Texture2D texture, out int newWidth, out int newHeight)
	{
		const int TargetDimensions = 480; // (512 % 80), lets Steam perform crispier downscaling for pixel art drawn at 40/80px resolution.
		if (_previewImagePath != null && _previewImageTransientTexture is Texture2D tex) {
			(texture, newWidth, newHeight) = (tex, TargetDimensions, TargetDimensions);
			return tex.Width < TargetDimensions || tex.Height < TargetDimensions;
		}

		(texture, newWidth, newHeight) = (default, default, default);
		return false;
	}

	internal static unsafe void UpscaleAndSaveImageAsPng(string srcImagePath, string dstImagePath, int dstWidth, int dstHeight)
	{
		using var srcStream = File.OpenRead(srcImagePath);
		Texture2D.TextureDataFromStreamEXT(srcStream, out int srcWidth, out int srcHeight, out byte[] srcBytes); 

		using var dstStream = File.OpenWrite(dstImagePath);
		PlatformUtilities.SavePng(dstStream, srcWidth, srcHeight, dstWidth, dstHeight, srcBytes);
	}
}
