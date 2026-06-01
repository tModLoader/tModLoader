using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI.ModBrowser;

internal class UIModDownloadItem : UIPanel
{
	private const float PADDING = 5f;
	public readonly ModDownloadItem ModDownload;

	private readonly Asset<Texture2D> _dividerTexture;
	private readonly Asset<Texture2D> _innerPanelTexture;
	private readonly UIText _modName;
	private readonly UIImage _updateButton;
	private readonly UIImage _updateWithDepsButton;
	private readonly UIImage _moreInfoButton;
	private readonly UIAutoScaleTextTextPanel<string> tMLUpdateRequired;
	internal ModIconStatus ModIconStatus = ModIconStatus.UNKNOWN;
	private UIImage _modIcon;
	internal string tooltip;

	private static int MaxFails = 3;
	private static int ModIconDownloadFailCount = 0;
	private bool HasModIcon => !string.IsNullOrEmpty(ModDownload.ModIconUrl);
	private float ModIconAdjust => 85f;
	private bool UpdateIsDowngrade = false;

	private string ViewModInfoText => Language.GetTextValue("tModLoader.ModsMoreInfo");

	private string UpdateWithDepsText => ModDownload.NeedUpdate
		? UpdateIsDowngrade
			? Language.GetTextValue("tModLoader.MBDowngradeWithDependencies")
			: Language.GetTextValue("tModLoader.MBUpdateWithDependencies")
		: Language.GetTextValue("tModLoader.MBDownloadWithDependencies");

	private readonly bool tMLNeedUpdate;

	private static ConcurrentDictionary<string, Texture2D> TextureDownloadCache = new();

	public UIModDownloadItem(ModDownloadItem modDownloadItem)
	{
		ModDownload = modDownloadItem;

		BorderColor = new Color(89, 116, 213) * 0.7f;
		_dividerTexture = UICommon.DividerTexture;
		_innerPanelTexture = UICommon.InnerPanelTexture;
		Height.Pixels = 92;
		Width.Percent = 1f;
		SetPadding(6f);

		float leftOffset = HasModIcon ? ModIconAdjust : 0f;

		_modName = new UIText(ModDownload.DisplayName) {
			Left = new StyleDimension(leftOffset + PADDING, 0f),
			Top = { Pixels = 5 }
		};
		Append(_modName);

		_moreInfoButton = new UIImage(UICommon.ButtonModInfoTexture) {
			RemoveFloatingPointsFromDrawPosition = true,
			Width = { Pixels = 36 },
			Height = { Pixels = 36 },
			Left = { Pixels = leftOffset },
			Top = { Pixels = 40 }
		};
		_moreInfoButton.OnLeftClick += ViewModInfo;
		Append(_moreInfoButton);

		var modBuildVersion = ModDownload.ModloaderVersion;
		tMLNeedUpdate = !BuildInfo.IsDev && BuildInfo.tMLVersion.MajorMinorBuild() < modBuildVersion.MajorMinorBuild();
		if (tMLNeedUpdate) {
			string updateVersion = $"v{modBuildVersion}";

			if (modBuildVersion.Build == 2)
				updateVersion = $"Preview {updateVersion}";

			tMLUpdateRequired = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MBRequiresTMLUpdate", updateVersion)).WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
			tMLUpdateRequired.BackgroundColor = Color.Orange * 0.7f;
			tMLUpdateRequired.CopyStyle(_moreInfoButton);
			tMLUpdateRequired.Width.Pixels = 340;
			tMLUpdateRequired.Left.Pixels += 36 + PADDING;
			tMLUpdateRequired.OnLeftClick += (a, b) => {
				Utils.OpenToURL("https://github.com/tModLoader/tModLoader/releases/latest");
			};
			Append(tMLUpdateRequired);
		}

		_updateButton = new UIImage(UICommon.ButtonExclamationTexture);
		_updateButton.CopyStyle(_moreInfoButton);
		_updateButton.Left.Pixels += 36 + PADDING;
		_updateButton.OnLeftClick += ShowGameNeedsRestart;

		_updateWithDepsButton = new UIImage(UICommon.ButtonDownloadMultipleTexture);
		_updateWithDepsButton.CopyStyle(_moreInfoButton);
		_updateWithDepsButton.Left.Pixels += 36 + PADDING;
		_updateWithDepsButton.OnLeftClick += DownloadWithDeps;

		if (ModDownload.ModReferencesBySlug?.Length > 0) {
			var icon = UICommon.ButtonDepsTexture;
			var modReferenceIcon = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MBClickToViewDependencyMods", string.Join("\n", ModDownload.ModReferencesBySlug.Split(',').Select(x => x.Trim())))) {
				RemoveFloatingPointsFromDrawPosition = true,
				UseTooltipMouseText = true,
				Left = { Pixels = -icon.Width() - PADDING, Percent = 1f }
			};
			modReferenceIcon.OnLeftClick += ShowModDependencies;
			Append(modReferenceIcon);
		}

		OnLeftDoubleClick += ViewModInfo;

		UpdateInstallDisplayState();
	}

	public void UpdateInstallDisplayState()
	{
		if (tMLNeedUpdate)
			return;

		_updateWithDepsButton.Remove();
		_updateButton.Remove();

		if (ModDownload.AppNeedRestartToReinstall) {
			Append(_updateButton);
		}
		else if (ModDownload.NeedUpdate || !ModDownload.IsInstalled) {
			Append(_updateWithDepsButton);
		}
	}

	private void ShowModDependencies(UIMouseEvent evt, UIElement element)
	{
		var modListItem = (UIModDownloadItem)element.Parent;
		Interface.modBrowser.SpecialModPackFilter = modListItem.ModDownload.ModReferenceByModId.ToList();
		Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterDependencies"); // Toolong of \n" + modListItem.modName.Text;
		Interface.modBrowser.FilterTextBox.Text = "";
		Interface.modBrowser.ResetTagFilters();
		Interface.modBrowser.UpdateNeeded = true; // Is done by updating the above but not in case of modpacks
		SoundEngine.PlaySound(SoundID.MenuOpen);
	}

	private void ShowGameNeedsRestart(UIMouseEvent evt, UIElement element)
	{
		Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.SteamRejectUpdate", ModDownload.DisplayName), Interface.modBrowserID);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		if (HasModIcon && ModIconStatus == ModIconStatus.UNKNOWN)
			RequestModIcon();

		CalculatedStyle innerDimensions = GetInnerDimensions();
		float leftOffset = HasModIcon ? ModIconAdjust : 0f;
		Vector2 drawPos = new Vector2(innerDimensions.X + 5f + leftOffset, innerDimensions.Y + 30f);
		spriteBatch.Draw(_dividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - leftOffset) / 8f, 1f), SpriteEffects.None, 0f);

		drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 125, innerDimensions.Y + 45);
		DrawTimeText(spriteBatch, drawPos);

		if (_updateButton?.IsMouseHovering == true) {
			tooltip = Language.GetTextValue("tModLoader.BrowserRejectWarning");
		}
		else if (_updateWithDepsButton?.IsMouseHovering == true) {
			tooltip = UpdateWithDepsText;
		}
		else if (_moreInfoButton?.IsMouseHovering == true) {
			tooltip = ViewModInfoText;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		tooltip = null;
		base.Draw(spriteBatch);
		if (!string.IsNullOrEmpty(tooltip)) {
			var bounds = GetOuterDimensions().ToRectangle();
			bounds.Height += 16;
			UICommon.TooltipMouseText(tooltip);
		}
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		base.DrawChildren(spriteBatch);
		if (tMLUpdateRequired?.IsMouseHovering == true) {
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.MBClickToUpdate"));
		}
		if (_modName.IsMouseHovering) {
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.ModsByline", ModDownload.Author));
		}
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (ModIconStatus == ModIconStatus.READY) {
			if (_modIcon == null)
				AdjustPositioningFailedIcon();
			else
				Append(_modIcon);

			ModIconStatus = ModIconStatus.DISPLAYED_OR_FAILED;
		}
	}

	private async void RequestModIcon()
	{
		if (ModIconDownloadFailCount < MaxFails) {
			ModIconStatus = ModIconStatus.REQUESTED;
			if (await GetOrDownloadTextureAsync(ModDownload.ModIconUrl) is Texture2D texture) {
				_modIcon = new UIImage(texture) {
					Left = { Percent = 0f },
					Top = { Percent = 0f },
					MaxWidth = { Pixels = 80f, Percent = 0f },
					MaxHeight = { Pixels = 80f, Percent = 0f },
					ScaleToFit = true
				};
			}
		}

		ModIconStatus = ModIconStatus.READY;
	}

	private static async Task<Texture2D?> GetOrDownloadTextureAsync(string url)
	{
		if (TextureDownloadCache.TryGetValue(url, out var texture))
			return texture;

		try {
#pragma warning disable SYSLIB0014 // WebClient is obsolete, but it saves us pooling HttpClient ourselves, and seems to have more reliable TLS headers
			var data = await new WebClient().DownloadDataTaskAsync(url);
#pragma warning restore SYSLIB0014
			texture = Main.Assets.CreateUntracked<Texture2D>(new MemoryStream(data), ".png").Value;
			TextureDownloadCache.TryAdd(url, texture);
			return texture;
		}
		catch {
			Interlocked.Increment(ref ModIconDownloadFailCount);
			return null;
		}
	}

	private void AdjustPositioningFailedIcon()
	{
		_modName.Left.Pixels -= ModIconAdjust;
		_moreInfoButton.Left.Pixels -= ModIconAdjust;
		if (_updateButton != null)
			_updateButton.Left.Pixels -= ModIconAdjust;
		if (_updateWithDepsButton != null)
			_updateWithDepsButton.Left.Pixels -= ModIconAdjust;
	}

	private void DrawTimeText(SpriteBatch spriteBatch, Vector2 drawPos)
	{
		if (ModDownload.TimeStamp == DateTime.MinValue) {
			return;
		}

		const int baseWidth = 125; // something like 1 days ago is ~110px, XX minutes ago is ~120 px (longest)
		spriteBatch.Draw(_innerPanelTexture.Value, drawPos, new Rectangle(0, 0, 8, _innerPanelTexture.Height()), Color.White);
		spriteBatch.Draw(_innerPanelTexture.Value, new Vector2(drawPos.X + 8f, drawPos.Y), new Rectangle(8, 0, 8, _innerPanelTexture.Height()), Color.White, 0f, Vector2.Zero, new Vector2((baseWidth - 16f) / 8f, 1f), SpriteEffects.None, 0f);
		spriteBatch.Draw(_innerPanelTexture.Value, new Vector2(drawPos.X + baseWidth - 8f, drawPos.Y), new Rectangle(16, 0, 8, _innerPanelTexture.Height()), Color.White);

		drawPos += new Vector2(0f, 2f);

		try {
			string text = TimeHelper.HumanTimeSpanString(ModDownload.TimeStamp); // get time text
			int textWidth = (int)FontAssets.MouseText.Value.MeasureString(text).X; // measure text width
			int diffWidth = baseWidth - textWidth; // get difference
			drawPos.X += diffWidth * 0.5f; // add difference as padding
			Utils.DrawBorderString(spriteBatch, text, drawPos, Color.White);
		}
		catch (Exception e) {
			Logging.tML.Error("Problem during drawing of time text", e);
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		BackgroundColor = UICommon.DefaultUIBlue;
		BorderColor = new Color(89, 116, 213);
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		BackgroundColor = new Color(63, 82, 151) * 0.7f;
		BorderColor = new Color(89, 116, 213) * 0.7f;
	}

	private async void DownloadWithDeps(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuTick);

		bool success = await Interface.modBrowser.DownloadMods(new[] { ModDownload });
		if (success)
			Main.QueueMainThreadAction(() => Main.menuMode = Interface.modBrowserID);
		//TODO: Some code to add the 'Installed' item to the UIModDownloaditem and redraw?
	}

	private void ViewModInfo(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Utils.OpenToURL(Interface.modBrowser.SocialBackend.GetModWebPage(ModDownload.PublishId));
	}
}