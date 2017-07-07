using System.Drawing;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
	public partial class DualPaneHTMLDisplay : UserControl
	{
		public readonly TitledHTMLDisplay left;
		public readonly TitledHTMLDisplay right;

		public DualPaneHTMLDisplay() {
			InitializeComponent();

			left = new TitledHTMLDisplay {Dock = DockStyle.Fill};
			right = new TitledHTMLDisplay {Dock = DockStyle.Fill};

			tableLayoutPanel.Controls.Add(left, 0, 0);
			tableLayoutPanel.Controls.Add(right, 1, 0);

			left.WebBrowser.DocumentCompleted += (_1, _2) => left.WebBrowser.Document.Window.AttachEventHandler("onscroll", (_3, _4) => ScrollSync(left.WebBrowser, right.WebBrowser));
			right.WebBrowser.DocumentCompleted += (_1, _2) => right.WebBrowser.Document.Window.AttachEventHandler("onscroll", (_3, _4) => ScrollSync(right.WebBrowser, left.WebBrowser));
		}

		private void ScrollSync(WebBrowser from, WebBrowser to) {
			var elemFrom = from.Document.Body;
			var elemTo = to.Document.Body;
			elemTo.ScrollTop = elemFrom.ScrollTop;
			elemTo.ScrollLeft = elemFrom.ScrollLeft;
		}

		public void SetSingleContent(string title, Color color, string html) {
			left.Title = title;
			left.TitleColor = color;
			left.WebBrowser.DocumentText = html;

			tableLayoutPanel.ColumnStyles[0].Width = 100;
			tableLayoutPanel.ColumnStyles[1].Width = 0;
		}

		public void SetContent(string leftTitle, Color leftColor, string leftHtml,
				string rightTitle, Color rightColor, string rightHtml) {
			left.Title = leftTitle;
			left.TitleColor = leftColor;
			left.WebBrowser.DocumentText = leftHtml;

			right.Title = rightTitle;
			right.TitleColor = rightColor;
			right.WebBrowser.DocumentText = rightHtml;

			tableLayoutPanel.ColumnStyles[0].Width = 50;
			tableLayoutPanel.ColumnStyles[1].Width = 50;
		}

		public void ScrollToOnLoad(string id) {
			WebBrowserDocumentCompletedEventHandler handler = null;
			handler = (sender, args) => {
				var b = (WebBrowser) sender;
				b.Document.GetElementById(id)?.ScrollIntoView(true);
				b.Document.Body.ScrollLeft = 0;
				b.DocumentCompleted -= handler;
			};
			left.WebBrowser.DocumentCompleted += handler;
			//right.WebBrowser.DocumentCompleted += handler;
		}
	}
}
