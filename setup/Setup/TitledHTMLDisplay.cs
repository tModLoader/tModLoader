using System.Drawing;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
	public partial class TitledHTMLDisplay : UserControl
	{
		public TitledHTMLDisplay() {
			InitializeComponent();
		}

		public WebBrowser WebBrowser => webBrowser;
		public string Title { get { return textBoxTitle.Text; } set { textBoxTitle.Text = value; } }
		public Color TitleColor { get { return textBoxTitle.BackColor; } set { textBoxTitle.BackColor = value; } }
	}
}
