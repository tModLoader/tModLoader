using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
		public readonly string ModName;
		public readonly string DisplayName;
		public readonly string DownloadUrl;
		public readonly bool HasUpdate;
		public readonly bool UpdateIsDowngrade;
		public readonly LocalMod Installed;

		private string _version;
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
		private readonly UIAutoScaleTextTextPanel<string> _updateButton;
		private readonly UIAutoScaleTextTextPanel<string> _updateWithDepsButton;
		private readonly UIAutoScaleTextTextPanel<string> _moreInfoButton;
		private readonly UIAutoScaleTextTextPanel<string> tMLUpdateRequired;
		private readonly UIText _authorText;
		private UIImage _modIcon;

		private bool HasModIcon => _modIconUrl != null;
		private float ModIconAdjust => _modIconStatus == ModIconStatus.APPENDED ? 85f : 0f;

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
			DownloadUrl = downloadUrl;

			_version = version;
			_author = author;
			_modReferences = modReferences;
			_modSide = modSide;
			_modIconUrl = modIconUrl;
			_downloads = downloads;
			_hot = hot;
			_timeStamp = timeStamp;
			HasUpdate = hasUpdate;
			UpdateIsDowngrade = updateIsDowngrade;
			Installed = installed;

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = TextureManager.Load("Images/UI/Divider");
			_innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			Height.Pixels = 120;
			Width.Percent = 1f;
			SetPadding(6f);

			float left = HasModIcon ? 85f : 0f;
			_modName = new UIText(displayName) {
				Left = new StyleDimension(left + 5, 0f),
				Top = { Pixels = 5 }
			};
			Append(_modName);

			_authorText = new UIText(Language.GetTextValue("tModLoader.ModsByline", author)) {
				Top = { Pixels = 83 }
			};
			Append(_authorText);

			if (_timeStamp != "0000-00-00 00:00:00") {
				try {
					var myDateTime = DateTime.Parse(_timeStamp);
					string text = TimeHelper.HumanTimeSpanString(myDateTime);
					int textWidth = (int)Main.fontMouseText.MeasureString(text).X;
					_authorText.Left.Set(125 + 5, 0f);
				}
				catch (Exception e) {
					Logging.tML.Error(e.ToString());
				}
			}

			_moreInfoButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModsMoreInfo")) {
				Width = { Pixels = 100 },
				Height = { Pixels = 36 },
				Left = { Pixels = left },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			_moreInfoButton.PaddingTop -= 2f;
			_moreInfoButton.PaddingBottom -= 2f;
			_moreInfoButton.OnClick += RequestMoreInfo;
			Append(_moreInfoButton);

			if (modloaderversion != null) {
				tMLUpdateRequired = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MBRequiresTMLUpdate", modloaderversion)).WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
				tMLUpdateRequired.BackgroundColor = Color.Orange * 0.7f;
				tMLUpdateRequired.CopyStyle(_moreInfoButton);
				tMLUpdateRequired.Width.Pixels = 340;
				tMLUpdateRequired.Left.Pixels = _moreInfoButton.Width.Pixels + _moreInfoButton.Left.Pixels + 5f;
				tMLUpdateRequired.OnClick += (a, b) => {
					Process.Start("https://github.com/tModLoader/tModLoader/releases/latest");
				};
				Append(tMLUpdateRequired);
			}
			else if (hasUpdate || installed == null) {
				_updateButton = new UIAutoScaleTextTextPanel<string>(UpdateText).WithFadedMouseOver();
				_updateButton.CopyStyle(_moreInfoButton);
				_updateButton.Width.Pixels = 120;
				_updateButton.Left.Pixels = _moreInfoButton.Width.Pixels + _moreInfoButton.Left.Pixels + 5f;
				_updateButton.OnClick += DownloadMod;
				Append(_updateButton);

				if (_modReferences.Length > 0) {
					_updateWithDepsButton = new UIAutoScaleTextTextPanel<string>(UpdateWithDepsText).WithFadedMouseOver();
					_updateWithDepsButton.CopyStyle(_updateButton);
					_updateWithDepsButton.Width.Pixels = 220;
					_updateWithDepsButton.Left.Pixels = _updateButton.Width.Pixels + _updateButton.Left.Pixels + 5f;
					_updateWithDepsButton.OnClick += DownloadWithDeps;
					Append(_updateWithDepsButton);
				}
			}

			if (modReferences.Length > 0) {
				var icon = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.ButtonExclamation.png"));
				var modReferenceIcon = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MBClickToViewDependencyMods", string.Join("\n", modReferences.Split(',').Select(x => x.Trim())))) {
					Left = { Pixels = -icon.Width, Percent = 1f },
					Top = { Pixels = 83 }
				};
				modReferenceIcon.OnClick += (s, e) => {
					var modListItem = (UIModDownloadItem)e.Parent;
					Interface.modBrowser.SpecialModPackFilter = modListItem._modReferences.Split(',').Select(x => x.Trim()).ToList();
					Interface.modBrowser.SpecialModPackFilterTitle = Language.GetTextValue("tModLoader.MBFilterDependencies"); // Toolong of \n" + modListItem.modName.Text;
					Interface.modBrowser.FilterTextBox.Text = "";
					Interface.modBrowser.UpdateNeeded = true;
					Main.PlaySound(SoundID.MenuOpen);
				};
				Append(modReferenceIcon);
			}

			OnDoubleClick += RequestMoreInfo;
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
			string modreferences = (string)mod["modreferences"] ?? "";
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
					return string.Compare(DisplayName, item?.DisplayName, StringComparison.Ordinal);
				case ModBrowserSortMode.DisplayNameZtoA:
					return -1 * string.Compare(DisplayName, item?.DisplayName, StringComparison.Ordinal);
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
				else if (DisplayName.IndexOf(Interface.modBrowser.Filter, StringComparison.OrdinalIgnoreCase) == -1
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
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (HasModIcon && _modIconStatus == ModIconStatus.UNKNOWN) {
				_modIconStatus = ModIconStatus.WANTED;
			}

			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f + ModIconAdjust, innerDimensions.Y + 30f);
			spriteBatch.Draw(_dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f - ModIconAdjust) / 8f, 1f), SpriteEffects.None, 0f);

			drawPos = new Vector2(innerDimensions.X, innerDimensions.Y + 80);
			DrawTimeText(spriteBatch, drawPos);
		}

		protected override void DrawChildren(SpriteBatch spriteBatch) {
			base.DrawChildren(spriteBatch);
			if (tMLUpdateRequired?.IsMouseHovering == true) {
				UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.MBClickToUpdate"), GetInnerDimensions().ToRectangle());
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
			try {
				byte[] data = e.Result;
				using (var buffer = new MemoryStream(data)) {
					var iconTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, buffer);
					_modIcon = new UIImage(iconTexture) {
						Left = { Percent = 0f },
						Top = { Percent = 0f }
					};
					_modIconStatus = ModIconStatus.READY;
				}
			}
			catch {
				// country- wide imgur blocks, cannot load icon
				_modIconStatus = ModIconStatus.APPENDED;
				_modName.Left.Set(5f, 0f);
				_moreInfoButton.Left.Set(0f, 0f);
				_updateButton.Left.Set(_moreInfoButton.Width.Pixels + 5f, 0f);
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
			BackgroundColor = UICommon.defaultUIBlue;
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
			modDownload.OnComplete += () => {
				Interface.modBrowser.ProcessDownloadedMod(modDownload);
			};
			Interface.downloadProgress.HandleDownloads(modDownload);
		}

		private void DownloadWithDeps(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuTick);
			var modDownload = GetModDownload();
			var mods = _modReferences.Split(',')
				.Select(Interface.modBrowser.FindModDownloadItem)
				.Where(item => item != null)
				.Select(x => x.GetModDownload())
				.ToList();
			mods.Add(modDownload);
			foreach (var mod in mods) {
				mod.OnComplete += () => {
					Interface.modBrowser.ProcessDownloadedMod(mod);
				};
			}
			Interface.downloadProgress.HandleDownloads(mods.ToArray());
		}

		private void RequestMoreInfo(UIMouseEvent evt, UIElement listeningElement) {
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