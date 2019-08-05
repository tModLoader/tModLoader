using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	internal class UIModInfo : UIState
	{
		private UIElement _uIElement;
		private UIMessageBox _modInfo;
		private UITextPanel<string> _uITextPanel;
		private UIAutoScaleTextTextPanel<string> _modHomepageButton;
		private UIAutoScaleTextTextPanel<string> _extractButton;
		private UIAutoScaleTextTextPanel<string> _deleteButton;
		private UIAutoScaleTextTextPanel<string> _fakeDeleteButton; // easier than making new OnMouseOver code.
		private readonly UILoaderAnimatedImage _loaderElement = new UILoaderAnimatedImage(0.5f, 0.5f);

		private int _gotoMenu;
		private LocalMod _localMod;
		private string _url = string.Empty;
		private string _info = string.Empty;
		private string _modName = string.Empty;
		private string _modDisplayName = string.Empty;
		private bool _loadFromWeb;
		private bool _loading;
		private bool _ready;

		private CancellationTokenSource _cts;
		
		public override void OnInitialize() {
			_uIElement = new UIElement {
				Width = {Percent = 0.8f},
				MaxWidth = UICommon.MaxPanelWidth,
				Top = {Pixels = 220},
				Height = {Pixels = -220, Percent = 1f},
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = {Percent = 1f},
				Height = {Pixels = -110, Percent = 1f},
				BackgroundColor = UICommon.MainPanelBackground
			};
			_uIElement.Append(uIPanel);

			_modInfo = new UIMessageBox(string.Empty) {
				Width = {Pixels = -25, Percent = 1f},
				Height = {Percent = 1f}
			};
			uIPanel.Append(_modInfo);

			var uIScrollbar = new UIScrollbar {
				Height = {Pixels = -20, Percent = 1f},
				VAlign = 0.5f,
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			_modInfo.SetScrollbar(uIScrollbar);
			_uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModInfoHeader"), 0.8f, true) {
				HAlign = 0.5f,
				Top = {Pixels = -35},
				BackgroundColor = UICommon.DefaultUIBlue
			}.WithPadding(15f);
			_uIElement.Append(_uITextPanel);

			_modHomepageButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoVisitHomepage")) {
				Width = {Percent = 1f},
				Height = {Pixels = 40},
				VAlign = 1f,
				Top = {Pixels = -65}
			}.WithFadedMouseOver();
			_modHomepageButton.OnClick += VisitModHomePage;

			var backButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = {Pixels = -10, Percent = 0.333f},
				Height = {Pixels = 40},
				VAlign = 1f,
				Top = {Pixels = -20}
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			_uIElement.Append(backButton);

			_extractButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoExtract")) {
				Width = {Pixels = -10, Percent = 0.333f},
				Height = {Pixels = 40},
				VAlign = 1f,
				HAlign = 0.5f,
				Top = {Pixels = -20}
			}.WithFadedMouseOver();
			_extractButton.OnClick += ExtractMod;

			_deleteButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Delete")) {
				Width = {Pixels = -10, Percent = 0.333f},
				Height = {Pixels = 40},
				VAlign = 1f,
				HAlign = 1f,
				Top = {Pixels = -20}
			}.WithFadedMouseOver();
			_deleteButton.OnClick += DeleteMod;

			_fakeDeleteButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Delete")) {
				Width = { Pixels = -10, Percent = 0.333f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				HAlign = 1f,
				Top = { Pixels = -20 }
			};
			_fakeDeleteButton.BackgroundColor = Color.Gray;

			Append(_uIElement);
		}

		internal void Show(string modName, string displayName, int gotoMenu, LocalMod localMod, string description = "", string url = "", bool loadFromWeb = false) {
			_modName = modName;
			_modDisplayName = displayName;
			_gotoMenu = gotoMenu;
			_localMod = localMod;
			_info = description;
			if (_info.Equals("") && !loadFromWeb) {
				_info = Language.GetTextValue("tModLoader.ModInfoNoDescriptionAvailable");
			}
			_url = url;
			_loadFromWeb = loadFromWeb;

			Main.gameMenu = true;
			Main.menuMode = Interface.modInfoID;
		}

		public override void OnDeactivate() {
			base.OnDeactivate();

			_cts?.Cancel(false);
			_info = string.Empty;
			_localMod = null;
			_gotoMenu = 0;
			_modName = string.Empty;
			_modDisplayName = string.Empty;
			_url = string.Empty;
			_modHomepageButton.Remove();
			_deleteButton.Remove();
			_fakeDeleteButton.Remove();
			_extractButton.Remove();
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = _gotoMenu;
		}

		private void ExtractMod(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Interface.extractMod.Show(_localMod, _gotoMenu);
		}

		private void DeleteMod(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			File.Delete(_localMod.modFile.path);
			Main.menuMode = _gotoMenu;
		}

		private void VisitModHomePage(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Process.Start(_url);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = _gotoMenu;
			
			if (_modHomepageButton.IsMouseHovering) {
				UICommon.DrawHoverStringInBounds(spriteBatch, _url);
			}
			if (_fakeDeleteButton.IsMouseHovering) {
				UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.ModInfoDisableModToDelete"));
			}
		}

		public override void OnActivate() {
			_modInfo.SetText(_info);
			_uITextPanel.SetText(Language.GetTextValue("tModLoader.ModInfoHeader") + _modDisplayName, 0.8f, true);
			if (_loadFromWeb) {
				_modInfo.Append(_loaderElement);
				_loading = true;
				_ready = false;
				
				_cts = new CancellationTokenSource();
				
				Task.Factory.StartNew(() => {
					try {
						ServicePointManager.Expect100Continue = false;
						const string url = "http://javid.ddns.net/tModLoader/moddescription.php";
						var values = new NameValueCollection {
							{"modname", _modName}
						};
						using (WebClient client = new WebClient()) {
							ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => policyErrors == SslPolicyErrors.None;
							client.UploadValuesCompleted += ReceiveModInfo;
							client.UploadValuesAsync(new Uri(url), "POST", values);
						}
					}
					catch (Exception e) {
						UIModBrowser.LogModBrowserException(e);
					}
				}, _cts.Token);
			}
			else {
				_loading = false;
				_ready = true;
			}
		}

		public override void Update(GameTime gameTime) {
			if (!_loading && _ready) {
				_modInfo.SetText(_info);
				
				if (!string.IsNullOrEmpty(_url)){
					_uIElement.Append(_modHomepageButton);
				}

				if (_localMod != null) {
					bool realDeleteButton = ModLoader.Mods.All(x => x.Name != _localMod.Name);
					_uIElement.AddOrRemoveChild(_deleteButton, realDeleteButton);
					_uIElement.AddOrRemoveChild(_fakeDeleteButton, !realDeleteButton);
					_uIElement.Append(_extractButton);
				}
				Recalculate();
				_modInfo.RemoveChild(_loaderElement);
				_ready = false;
			}
		}

		private void ReceiveModInfo(object sender, UploadValuesCompletedEventArgs e) {
			_loading = false;
			string description = Language.GetTextValue("tModLoader.ModInfoProblemTryAgain");
			string homepage = "";
			if (!e.Cancelled) {
				try {
					string response = Encoding.UTF8.GetString(e.Result);
					if (!string.IsNullOrEmpty(response)) {
						try {
							JObject joResponse = JObject.Parse(response);
							description = (string)joResponse["description"];
							homepage = (string)joResponse["homepage"];
						}
						catch (Exception err) {
							Logging.tML.Error($"Problem during JSON parse of mod info for {_modDisplayName}", err);
						}
					}
				}
				catch (Exception err) {
					Logging.tML.Error($"There was a problem trying to receive the result of a mod info request for {_modDisplayName}", err);
				}
			}

			_info = description;
			if (_info.Equals("")) {
				_info = Language.GetTextValue("tModLoader.ModInfoNoDescriptionAvailable");
			}
			_url = homepage;
			_ready = true;
		}
	}
}