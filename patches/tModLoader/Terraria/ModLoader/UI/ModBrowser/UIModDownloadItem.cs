using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.ModBrowser
{
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
		private UIImage _modIcon;
		internal string tooltip;

		private const int MaxImgurFails = 3;
		private static int ModIconDownloadFailCount = 0;
		private bool HasModIcon => ModDownload.ModIconUrl != null;
		private float ModIconAdjust => 85f;
		private bool IsInstalled => ModDownload.Installed != null;

		private string ViewModInfoText => Language.GetTextValue("tModLoader.ModsMoreInfo");

		private string UpdateText => ModDownload.HasUpdate
			? ModDownload.UpdateIsDowngrade
				? Language.GetTextValue("tModLoader.MBDowngrade")
				: Language.GetTextValue("tModLoader.MBUpdate")
			: Language.GetTextValue("tModLoader.MBDownload");

		private string UpdateWithDepsText => ModDownload.HasUpdate
			? ModDownload.UpdateIsDowngrade
				? Language.GetTextValue("tModLoader.MBDowngradeWithDependencies")
				: Language.GetTextValue("tModLoader.MBUpdateWithDependencies")
			: Language.GetTextValue("tModLoader.MBDownloadWithDependencies");

		public UIModDownloadItem(ModDownloadItem modDownloadItem) {
			ModDownload = modDownloadItem;

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = UICommon.DividerTexture;
			_innerPanelTexture = UICommon.InnerPanelTexture;
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);

			float leftOffset = HasModIcon ? ModIconAdjust : 0f;

			_modName = new UIText(ModDownload.DisplayName) {
				Left = new StyleDimension(leftOffset + PADDING, 0f),
				Top = { Pixels = 5 }
			};
			Append(_modName);

			_moreInfoButton = new UIImage(UICommon.ButtonModInfoTexture) {
				Width = { Pixels = 36 },
				Height = { Pixels = 36 },
				Left = { Pixels = leftOffset },
				Top = { Pixels = 40 }
			};
			_moreInfoButton.OnClick += ViewModInfo;
			Append(_moreInfoButton);

			if ((BuildInfo.IsRelease || BuildInfo.IsBeta) && !!ModLoader.versionedName.Contains(ModDownload.ModloaderVersion)) {
				tMLUpdateRequired = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MBRequiresTMLUpdate", ModDownload.ModloaderVersion)).WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
				tMLUpdateRequired.BackgroundColor = Color.Orange * 0.7f;
				tMLUpdateRequired.CopyStyle(_moreInfoButton);
				tMLUpdateRequired.Width.Pixels = 340;
				tMLUpdateRequired.Left.Pixels += 36 + PADDING;
				tMLUpdateRequired.OnClick += (a, b) => {
					Utils.OpenToURL("https://github.com/tModLoader/tModLoader/releases/latest");
				};
				Append(tMLUpdateRequired);
			}
			else if (ModDownload.HasUpdate || ModDownload.Installed == null) {
				_updateWithDepsButton = new UIImage(UICommon.ButtonDownloadMultipleTexture);
				_updateWithDepsButton.CopyStyle(_moreInfoButton);
				_updateWithDepsButton.Left.Pixels += 36 + PADDING;
				_updateWithDepsButton.OnClick += DownloadWithDeps;
				Append(_updateWithDepsButton);
			}

			if (ModDownload.ModReferences?.Length > 0) {
				var icon = UICommon.ButtonExclamationTexture;
				var modReferenceIcon = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MBClickToViewDependencyMods", string.Join("\n", ModDownload.ModReferences.Split(',').Select(x => x.Trim())))) {
					Left = { Pixels = -icon.Width() - PADDING, Percent = 1f }
				};
				modReferenceIcon.OnClick += ShowModDependencies;
				Append(modReferenceIcon);
			}

			OnDoubleClick += ViewModInfo;
		}

		private void ShowModDependencies(UIMouseEvent evt, UIElement element) {
			var modListItem = (UIModDownloadItem)element.Parent;
			Interface.modBrowser.SpecialModPackFilter = modListItem.ModDownload.ModReferences.Split(',').Select(x => x.Trim()).ToList();
			Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterDependencies"); // Toolong of \n" + modListItem.modName.Text;
			Interface.modBrowser.FilterTextBox.Text = "";
			Interface.modBrowser.UpdateNeeded = true;
			SoundEngine.PlaySound(SoundID.MenuOpen);
		}

		public override int CompareTo(object obj) {
			var item = obj as UIModDownloadItem;
			switch (Interface.modBrowser.SortMode) {
				default:
					return base.CompareTo(obj);
				case ModBrowserSortMode.DisplayNameAtoZ:
					return string.Compare(ModDownload.DisplayNameClean, item?.ModDownload.DisplayNameClean, StringComparison.Ordinal);
				case ModBrowserSortMode.DisplayNameZtoA:
					return -1 * string.Compare(ModDownload.DisplayNameClean, item?.ModDownload.DisplayNameClean, StringComparison.Ordinal);
				case ModBrowserSortMode.DownloadsAscending:
					return ModDownload.Downloads.CompareTo(item?.ModDownload.Downloads);
				case ModBrowserSortMode.DownloadsDescending:
					return -1 * ModDownload.Downloads.CompareTo(item?.ModDownload.Downloads);
				case ModBrowserSortMode.RecentlyUpdated:
					return -1 * ModDownload.TimeStamp.CompareTo(item?.ModDownload.TimeStamp);
				case ModBrowserSortMode.Hot:
					return -1 * ModDownload.Hot.CompareTo(item?.ModDownload.Hot);
			}
		}

		public bool PassFilters() {
			if (Interface.modBrowser.SpecialModPackFilter != null && !Interface.modBrowser.SpecialModPackFilter.Contains(ModDownload.ModName))
				return false;

			if (!string.IsNullOrEmpty(Interface.modBrowser.Filter)) {
				if (Interface.modBrowser.SearchFilterMode == SearchFilter.Author) {
					if (ModDownload.Author.IndexOf(Interface.modBrowser.Filter, StringComparison.OrdinalIgnoreCase) == -1)
						return false;
				}
				else if (ModDownload.DisplayNameClean.IndexOf(Interface.modBrowser.Filter, StringComparison.OrdinalIgnoreCase) == -1
					&& ModDownload.ModName.IndexOf(Interface.modBrowser.Filter, StringComparison.OrdinalIgnoreCase) == -1)
					return false;
			}

			if (Interface.modBrowser.ModSideFilterMode != ModSideFilter.All
				&& (int)ModDownload.ModSide != (int)Interface.modBrowser.ModSideFilterMode - 1)
				return false;

			switch (Interface.modBrowser.UpdateFilterMode) {
				default:
				case UpdateFilter.All:
					return true;
				case UpdateFilter.Available:
					return ModDownload.HasUpdate || ModDownload.Installed == null;
				case UpdateFilter.UpdateOnly:
					return ModDownload.HasUpdate;
				case UpdateFilter.InstalledOnly:
					return ModDownload.Installed != null;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (HasModIcon && ModDownload.ModIconStatus == ModIconStatus.UNKNOWN) {
				ModDownload.ModIconStatus = ModIconStatus.WANTED;
				if (ModIconDownloadFailCount >= MaxImgurFails)
					AdjustPositioningFailedIcon();
			}

			CalculatedStyle innerDimensions = GetInnerDimensions();
			float leftOffset = HasModIcon ? ModIconAdjust : 0f;
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f + leftOffset, innerDimensions.Y + 30f);
			spriteBatch.Draw(_dividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - leftOffset) / 8f, 1f), SpriteEffects.None, 0f);

			drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 125, innerDimensions.Y + 45);
			DrawTimeText(spriteBatch, drawPos);

			if (_updateButton?.IsMouseHovering == true) {
				tooltip = UpdateText;
			}
			else if (_updateWithDepsButton?.IsMouseHovering == true) {
				tooltip = UpdateWithDepsText;
			}
			else if (_moreInfoButton?.IsMouseHovering == true) {
				tooltip = ViewModInfoText;
			}
		}

		public override void Draw(SpriteBatch spriteBatch) {
			tooltip = null;
			base.Draw(spriteBatch);
			if (!string.IsNullOrEmpty(tooltip)) {
				var bounds = GetOuterDimensions().ToRectangle();
				bounds.Height += 16;
				UICommon.DrawHoverStringInBounds(spriteBatch, tooltip, bounds);
			}
		}

		protected override void DrawChildren(SpriteBatch spriteBatch) {
			base.DrawChildren(spriteBatch);
			if (tMLUpdateRequired?.IsMouseHovering == true) {
				UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.MBClickToUpdate"), GetInnerDimensions().ToRectangle());
			}
			if (_modName.IsMouseHovering) {
				UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.ModsByline", ModDownload.Author), GetInnerDimensions().ToRectangle());
			}
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			switch (ModDownload.ModIconStatus) {
				case ModIconStatus.WANTED:
					RequestModIcon();
					break;
				case ModIconStatus.READY:
					AppendModIcon();
					break;
			}
		}

		private void RequestModIcon() {
			ModDownload.ModIconStatus = ModIconStatus.REQUESTED;
			using (var client = new WebClient()) {
				client.DownloadDataCompleted += IconDownloadComplete;
				client.DownloadDataAsync(new Uri(ModDownload.ModIconUrl));
			}
		}

		private void AppendModIcon() {
			ModDownload.ModIconStatus = ModIconStatus.APPENDED;
			Append(_modIcon);
		}

		private void IconDownloadComplete(object sender, DownloadDataCompletedEventArgs e) {
			bool success = false;

			try {
				if (!e.Cancelled && e.Error == null) {
					byte[] data = e.Result;
					using (var buffer = new MemoryStream(data)) {
						var iconTexture = Main.Assets.CreateUntracked<Texture2D>(buffer, ".png");

						_modIcon = new UIImage(iconTexture) {
							Left = { Percent = 0f },
							Top = { Percent = 0f },
							MaxWidth = { Pixels = 80f, Percent = 0f },
							MaxHeight = { Pixels = 80f, Percent = 0f },
							ScaleToFit = true
						};
						ModDownload.ModIconStatus = ModIconStatus.READY;
						success = true;
					}
				}
			}
			catch {
				// country- wide imgur blocks, cannot load icon
			}

			if (!success) {
				AdjustPositioningFailedIcon();
				ModIconDownloadFailCount++;

				if(ModIconDownloadFailCount == MaxImgurFails)
					Logging.tML.Error("tModLoader has repeatedly failed to connect to imgur.com to download mod icons. If you know imgur is not accessibile in your country, you can set AvoidImgur found in config.json to true to avoid attempting to download mod icons in the future.");
			}
		}

		private void AdjustPositioningFailedIcon() {
			ModDownload.ModIconStatus = ModIconStatus.APPENDED;
			_modName.Left.Pixels -= ModIconAdjust;
			_moreInfoButton.Left.Pixels -= ModIconAdjust;
			_updateButton.Left.Pixels -= ModIconAdjust;
			if (_updateWithDepsButton != null)
				_updateWithDepsButton.Left.Pixels -= ModIconAdjust;
		}

		private void DrawTimeText(SpriteBatch spriteBatch, Vector2 drawPos) {
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

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			BackgroundColor = UICommon.DefaultUIBlue;
			BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);
			BackgroundColor = new Color(63, 82, 151) * 0.7f;
			BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		private void DownloadWithDeps(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			ModDownload.InnerDownloadWithDeps();
		}

		private void ViewModInfo(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Interface.modInfo.Show(ModDownload.ModName, ModDownload.DisplayName, Interface.modBrowserID, ModDownload.Installed, url: ModDownload.Homepage, loadFromWeb: true, publishedFileId: ModDownload.PublishId);
		}
	}
}