using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO how is this different to UIInfoMessage?
	internal class UIUpdateMessage : UIState
	{
		private UIMessageBox message = new UIMessageBox("");
		private int gotoMenu = 0;
		private string url;

		public override void OnInitialize() {
			var area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			message.Width.Percent = 1f;
			message.Height.Percent = 0.8f;
			message.HAlign = 0.5f;
			area.Append(message);

			var button = new UITextPanel<string>("Ignore", 0.7f, true) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 50 },
				VAlign = 1f,
				Top = { Pixels = -30 }
			};
			button.WithFadedMouseOver();
			button.OnClick += IgnoreClick;
			area.Append(button);

			var button2 = new UITextPanel<string>("Download", 0.7f, true);
			button2.CopyStyle(button);
			button2.HAlign = 1f;
			button2.WithFadedMouseOver();
			button2.OnClick += OpenURL;
			area.Append(button2);
			Append(area);
		}

		internal void SetMessage(string text) {
			message.SetText(text);
		}

		internal void SetGotoMenu(int gotoMenu) {
			this.gotoMenu = gotoMenu;
		}

		internal void SetURL(string url) {
			this.url = url;
		}

		private void IgnoreClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = gotoMenu;
		}

		private void OpenURL(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(url);
		}
	}
}
