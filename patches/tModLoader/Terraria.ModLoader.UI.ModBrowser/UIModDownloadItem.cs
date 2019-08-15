using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.ModBrowser
{
	internal class UIModDownloadItem : UIPanel
	{
		private const float PADDING = 5f;

		public readonly string ModName;
		public readonly string DisplayName;
		public readonly string DisplayNameClean; // No chat tags: for search and sort functionality.
		public readonly string DownloadUrl;
		public readonly bool HasUpdate;
		public readonly bool UpdateIsDowngrade;
		public readonly LocalMod Installed;

		private readonly string _author;
		private readonly string _modIconUrl;
		private ModIconStatus _modIconStatus = ModIconStatus.UNKNOWN;
		private readonly string _timeStamp;
		private readonly string _modReferences;
		private readonly ModSide _modSide;
		private readonly int _downloads;
		private readonly int _hot;
		private readonly Texture2D _dividerTexture;
		private readonly Texture2D _innerPanelTexture;
		private readonly UIText _modName;
		private readonly UIImage _updateButton;
		private readonly UIImage _updateWithDepsButton;
		private readonly UIImage _moreInfoButton;
		private readonly UIAutoScaleTextTextPanel<string> tMLUpdateRequired;
		private UIImage _modIcon;
		internal string tooltip;

		private bool HasModIcon => _modIconUrl != null;
		private float ModIconAdjust => 85f;
		private bool IsInstalled => Installed != null;

		private string ViewModInfoText => Language.GetTextValue("tModLoader.ModsMoreInfo");

		private string UpdateText => HasUpdate
			? UpdateIsDowngrade
				? Language.GetTextValue("tModLoader.MBDowngrade")
				: Language.GetTextValue("tModLoader.MBUpdate")
			: Language.GetTextValue("tModLoader.MBDownload");

		private string UpdateWithDepsText => HasUpdate
			? UpdateIsDowngrade
				? Language.GetTextValue("tModLoader.MBDowngradeWithDependencies")
				: Language.GetTextValue("tModLoader.MBUpdateWithDependencies")
			: Language.GetTextValue("tModLoader.MBDownloadWithDependencies");

		public UIModDownloadItem(string displayName, string name, string version, string author, string modReferences, ModSide modSide, string modIconUrl, string downloadUrl, int downloads, int hot, string timeStamp, bool hasUpdate, bool updateIsDowngrade, LocalMod installed, string modloaderversion) {
			ModName = name;
			DisplayName = displayName;
			DisplayNameClean = string.Join("", ChatManager.ParseMessage(displayName, Color.White).Where(x=> x.GetType() == typeof(TextSnippet)).Select(x => x.Text));
			DownloadUrl = downloadUrl;

			_author = author;
			_modReferences = modReferences;
			_modSide = modSide;
			_modIconUrl = modIconUrl;
			if (UIModBrowser.AvoidImgur)
				_modIconUrl = null;
			_downloads = downloads;
			_hot = hot;
			_timeStamp = timeStamp;
			HasUpdate = hasUpdate;
			UpdateIsDowngrade = updateIsDowngrade;
			Installed = installed;

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = UICommon.DividerTexture;
			_innerPanelTexture = UICommon.InnerPanelTexture;
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);

			float leftOffset = HasModIcon ? ModIconAdjust : 0f;

			_modName = new UIText(displayName) {
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

			if (modloaderversion != null) {
				tMLUpdateRequired = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MBRequiresTMLUpdate", modloaderversion)).WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
				tMLUpdateRequired.BackgroundColor = Color.Orange * 0.7f;
				tMLUpdateRequired.CopyStyle(_moreInfoButton);
				tMLUpdateRequired.Width.Pixels = 340;
				tMLUpdateRequired.Left.Pixels += 36 + PADDING;
				tMLUpdateRequired.OnClick += (a, b) => {
					Process.Start("https://github.com/tModLoader/tModLoader/releases/latest");
				};
				Append(tMLUpdateRequired);
			}
			else if (hasUpdate || installed == null) {
				_updateButton = new UIImage(UpdateIsDowngrade ? UICommon.ButtonDowngradeTexture : UICommon.ButtonDownloadTexture);
				_updateButton.CopyStyle(_moreInfoButton);
				_updateButton.Left.Pixels += 36 + PADDING;
				_updateButton.OnClick += DownloadMod;
				Append(_updateButton);

				if (_modReferences.Length > 0) {
					_updateWithDepsButton = new UIImage(UICommon.ButtonDownloadMultipleTexture);
					_updateWithDepsButton.CopyStyle(_updateButton);
					_updateWithDepsButton.Left.Pixels += 36 + PADDING;
					_updateWithDepsButton.OnClick += DownloadWithDeps;
					Append(_updateWithDepsButton);
				}
			}

			if (modReferences.Length > 0) {
				var icon = UICommon.ButtonExclamationTexture;
				var modReferenceIcon = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MBClickToViewDependencyMods", string.Join("\n", modReferences.Split(',').Select(x => x.Trim())))) {
					Left = { Pixels = -icon.Width - PADDING, Percent = 1f }
				};
				modReferenceIcon.OnClick += ShowModDependencies;
				Append(modReferenceIcon);
			}

			OnDoubleClick += ViewModInfo;
		}

		private void ShowModDependencies(UIMouseEvent evt, UIElement element) {
			var modListItem = (UIModDownloadItem)element.Parent;
			Interface.modBrowser.SpecialModPackFilter = modListItem._modReferences.Split(',').Select(x => x.Trim()).ToList();
			Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterDependencies"); // Toolong of \n" + modListItem.modName.Text;
			Interface.modBrowser.FilterTextBox.Text = "";
			Interface.modBrowser.UpdateNeeded = true;
			Main.PlaySound(SoundID.MenuOpen);
		}

		internal static UIModDownloadItem FromJson(LocalMod[] installedMods, JObject mod) {
			string displayname = (string)mod["displayname"];
			//reloadButton.SetText("Adding " + displayname + "...");
			string name = (string)mod["name"];
			string version = (string)mod["version"];
			string author = (string)mod["author"];
			string download = (string)mod["download"] ?? $"http://javid.ddns.net/tModLoader/download.php?Down=mods/{name}.tmod{(ModBrowser.UIModBrowser.PlatformSupportsTls12 && !ModBrowser.UIModBrowser.AvoidGithub ? "&tls12=y" : "")}";
			int downloads = (int)mod["downloads"];
			int hot = (int)mod["hot"]; // for now, hotness is just downloadsYesterday
			string timeStamp = (string)mod["updateTimeStamp"];
			//string[] modreferences = ((string)mod["modreferences"]).Split(',');
			string modreferences = ((string)mod["modreferences"])?.Replace(" ", string.Empty) ?? string.Empty;
			ModSide modside = ModSide.Both; // TODO: add filter option for modside.
			string modIconURL = (string)mod["iconurl"];
			string modsideString = (string)mod["modside"];
			if (modsideString == "Client") modside = ModSide.Client;
			if (modsideString == "Server") modside = ModSide.Server;
			if (modsideString == "NoSync") modside = ModSide.NoSync;
			string modloaderversion = (string)mod["modloaderversion"];
			//bool exists = false; // unused?
			bool update = false;
			bool updateIsDowngrade = false;
			var installed = installedMods.FirstOrDefault(m => m.Name == name);
			if (installed != null) {
				//exists = true;
				var cVersion = new Version(version.Substring(1));
				if (cVersion > installed.modFile.version)
					update = true;
				else if (cVersion < installed.modFile.version)
					update = updateIsDowngrade = true;
			}

			return new UIModDownloadItem(displayname, name, version, author, modreferences, modside, modIconURL, download, downloads, hot, timeStamp, update, updateIsDowngrade, installed, modloaderversion);
		}

		public override int CompareTo(object obj) {
			var item = obj as UIModDownloadItem;
			switch (Interface.modBrowser.SortMode) {
				default:
					return base.CompareTo(obj);
				case ModBrowserSortMode.DisplayNameAtoZ:
					return string.Compare(DisplayNameClean, item?.DisplayNameClean, StringComparison.Ordinal);
				case ModBrowserSortMode.DisplayNameZtoA:
					return -1 * string.Compare(DisplayNameClean, item?.DisplayNameClean, StringComparison.Ordinal);
				case ModBrowserSortMode.DownloadsAscending:
					return _downloads.CompareTo(item?._downloads);
				case ModBrowserSortMode.DownloadsDescending:
					return -1 * _downloads.CompareTo(item?._downloads);
				case ModBrowserSortMode.RecentlyUpdated:
					return -1 * string.Compare(_timeStamp, item?._timeStamp, StringComparison.Ordinal);
				case ModBrowserSortMode.Hot:
					return -1 * _hot.CompareTo(item?._hot);
			}
		}

		public bool PassFilters() {
			if (Interface.modBrowser.SpecialModPackFilter != null && !Interface.modBrowser.SpecialModPackFilter.Contains(ModName))
				return false;

			if (!string.IsNullOrEmpty(Interface.modBrowser.Filter)) {
				if (Interface.modBrowser.SearchFilterMode == SearchFilter.Author) {
					if (_author.IndexOf(Interface.modBrowser.Filter, StringComparison.OrdinalIgnoreCase) == -1)
						return false;
				}
				else if (DisplayNameClean.IndexOf(Interface.modBrowser.Filter, StringComparison.OrdinalIgnoreCase) == -1
					&& ModName.IndexOf(Interface.modBrowser.Filter, StringComparison.OrdinalIgnoreCase) == -1)
					return false;
			}

			if (Interface.modBrowser.ModSideFilterMode != ModSideFilter.All
				&& (int)_modSide != (int)Interface.modBrowser.ModSideFilterMode - 1)
				return false;

			switch (Interface.modBrowser.UpdateFilterMode) {
				default:
				case UpdateFilter.All:
					return true;
				case UpdateFilter.Available:
					return HasUpdate || Installed == null;
				case UpdateFilter.UpdateOnly:
					return HasUpdate;
				case UpdateFilter.InstalledOnly:
					return Installed != null;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (HasModIcon && _modIconStatus == ModIconStatus.UNKNOWN) {
				_modIconStatus = ModIconStatus.WANTED;
			}

			CalculatedStyle innerDimensions = GetInnerDimensions();
			float leftOffset = HasModIcon ? ModIconAdjust : 0f;
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f + leftOffset, innerDimensions.Y + 30f);
			spriteBatch.Draw(_dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - leftOffset) / 8f, 1f), SpriteEffects.None, 0f);

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
				UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.ModsByline", _author), GetInnerDimensions().ToRectangle());
			}
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			switch (_modIconStatus) {
				case ModIconStatus.WANTED:
					RequestModIcon();
					break;
				case ModIconStatus.READY:
					AppendModIcon();
					break;
			}
		}

		private void RequestModIcon() {
			_modIconStatus = ModIconStatus.REQUESTED;
			using (var client = new WebClient()) {
				client.DownloadDataCompleted += IconDownloadComplete;
				client.DownloadDataAsync(new Uri(_modIconUrl));
			}
		}

		private void AppendModIcon() {
			_modIconStatus = ModIconStatus.APPENDED;
			Append(_modIcon);
		}

		private void IconDownloadComplete(object sender, DownloadDataCompletedEventArgs e) {
			bool success = false;
			try {
				if (!e.Cancelled && e.Error == null) {
					byte[] data = e.Result;
					using (var buffer = new MemoryStream(data)) {
						var iconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, buffer);
						_modIcon = new UIImage(iconTexture) {
							Left = { Percent = 0f },
							Top = { Percent = 0f }
						};
						_modIconStatus = ModIconStatus.READY;
						success = true;
					}
				}
			}
			catch {
				// country- wide imgur blocks, cannot load icon
			}
			if (!success) {
				_modIconStatus = ModIconStatus.APPENDED;
				_modName.Left.Pixels -= ModIconAdjust;
				_moreInfoButton.Left.Pixels -= ModIconAdjust;
				_updateButton.Left.Pixels -= ModIconAdjust;
				_updateWithDepsButton.Left.Pixels -= ModIconAdjust;
			}
		}

		private void DrawTimeText(SpriteBatch spriteBatch, Vector2 drawPos) {
			if (_timeStamp == "0000-00-00 00:00:00") {
				return;
			}

			const int baseWidth = 125; // something like 1 days ago is ~110px, XX minutes ago is ~120 px (longest)
			spriteBatch.Draw(_innerPanelTexture, drawPos, new Rectangle(0, 0, 8, _innerPanelTexture.Height), Color.White);
			spriteBatch.Draw(_innerPanelTexture, new Vector2(drawPos.X + 8f, drawPos.Y), new Rectangle(8, 0, 8, _innerPanelTexture.Height), Color.White, 0f, Vector2.Zero, new Vector2((baseWidth - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(_innerPanelTexture, new Vector2(drawPos.X + baseWidth - 8f, drawPos.Y), new Rectangle(16, 0, 8, _innerPanelTexture.Height), Color.White);

			drawPos += new Vector2(0f, 2f);

			try {
				var myDateTime = DateTime.Parse(_timeStamp); // parse date
				string text = TimeHelper.HumanTimeSpanString(myDateTime); // get time text
				int textWidth = (int)Main.fontMouseText.MeasureString(text).X; // measure text width
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

		private void DownloadMod(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuTick);
			var modDownload = GetModDownload();
			Interface.downloadProgress.gotoMenu = Interface.modBrowserID;
			Interface.downloadProgress.HandleDownloads(modDownload);
		}

		private void DownloadWithDeps(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuTick);
			var downloads = new HashSet<DownloadModFile> { GetModDownload() };
			GetDependenciesRecursive(this, ref downloads);
			Interface.downloadProgress.gotoMenu = Interface.modBrowserID;
			Interface.downloadProgress.HandleDownloads(downloads.ToArray());
		}

		private IEnumerable<DownloadModFile> GetDependencies() {
			return _modReferences.Split(',')
				.Select(Interface.modBrowser.FindModDownloadItem)
				.Where(item => item != null && (!item.IsInstalled || (item.HasUpdate && !item.UpdateIsDowngrade)))
				.Select(x => x.GetModDownload());
		}

		private void GetDependenciesRecursive(UIModDownloadItem item, ref HashSet<DownloadModFile> set) {
			var deps = item.GetDependencies();
			set.UnionWith(deps);
			// Cyclic dependency should never happen, as it's not allowed
			// TODO: What if the same mod is a dependency twice, but different versions?
			foreach (var dep in deps) {
				GetDependenciesRecursive(dep.ModBrowserItem, ref set);
			}
		}

		private void ViewModInfo(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Interface.modInfo.Show(ModName, DisplayName, Interface.modBrowserID, Installed, loadFromWeb: true);
		}

		public DownloadModFile GetModDownload() {
			var modDownload = new DownloadModFile(DownloadUrl, $"{ModLoader.ModPath}{Path.DirectorySeparatorChar}{ModName}.tmod", DisplayName) {
				ModBrowserItem = this
			};
			return modDownload;
		}
	}
}