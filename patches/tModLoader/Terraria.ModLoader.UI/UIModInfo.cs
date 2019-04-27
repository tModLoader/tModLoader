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
		private readonly UILoaderAnimatedImage _loaderElement = new UILoaderAnimatedImage(0.5f, 0.5f);

		private int _gotoMenu;
		private LocalMod _localMod;
		private string _url = string.Empty;
		private string _info = string.Empty;
		private string _modDisplayName = string.Empty;

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
				BackgroundColor = UICommon.mainPanelBackground
			};
			_uIElement.Append(uIPanel);

			_modInfo = new UIMessageBox(string.Empty) {
				Width = {Pixels = -25, Percent = 1f},
				Height = {Percent = 1f}
			};
			uIPanel.Append(_modInfo);

			// TODO broken
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
				BackgroundColor = UICommon.defaultUIBlue
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
			_extractButton.OnClick += ExtractClick;

			_deleteButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Delete")) {
				Width = {Pixels = -10, Percent = 0.333f},
				Height = {Pixels = 40},
				VAlign = 1f,
				HAlign = 1f,
				Top = {Pixels = -20}
			}.WithFadedMouseOver();
			_deleteButton.OnClick += DeleteClick;

			Append(_uIElement);
		}

		// TODO use Show pattern
		internal void SetModInfo(string text) {
			_info = text;
			if (_info.Equals("")) {
				_info = Language.GetTextValue("tModLoader.ModInfoNoDescriptionAvailable");
			}
		}

		internal void SetModName(string text) {
			_modDisplayName = text;
		}

		internal void SetGotoMenu(int gotoMenu) {
			_gotoMenu = gotoMenu;
		}

		internal void SetUrl(string url) {
			_url = url;
		}

		internal void SetMod(LocalMod mod) {
			_localMod = mod;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			_cts?.Cancel(false);
			Main.PlaySound(11);
			Main.menuMode = _gotoMenu;
			_info = string.Empty;
			SetMod(null);
			SetGotoMenu(0);
			SetModName(string.Empty);
			SetUrl(string.Empty);
			_modHomepageButton.Remove();
			_deleteButton.Remove();
			_extractButton.Remove();
		}

		private void ExtractClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			Interface.extractMod.Show(_localMod, _gotoMenu);
		}

		private void DeleteClick(UIMouseEvent evt, UIElement listeningElement) {
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
		}

		private bool _loading;
		private bool _ready;

		public override void OnActivate() {
			if (string.IsNullOrEmpty(_info)) {
				_uIElement.Append(_loaderElement);
				_loading = true;
				_ready = false;
				
				_cts = new CancellationTokenSource();
				
				Task.Factory.StartNew(() => {
					try {
						ServicePointManager.Expect100Continue = false;
						const string url = "http://javid.ddns.net/tModLoader/moddescription.php";
						var values = new NameValueCollection {
							{"modname", _modDisplayName}
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
				_uITextPanel.SetText(Language.GetTextValue("tModLoader.ModInfoHeader") + _modDisplayName, 0.8f, true);
				_modInfo.SetText(_info);
				
				if (!string.IsNullOrEmpty(_url)){
					_uIElement.Append(_modHomepageButton);
				}

				if (_localMod != null) {
					_uIElement.AddOrRemoveChild(_deleteButton, ModLoader.Mods.All(x => x.Name != _localMod.Name));
					_uIElement.Append(_extractButton);
				}
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

			SetModInfo(description);
			SetUrl(homepage);
			_uIElement.RemoveChild(_loaderElement);
			_ready = true;
		}
	}
}