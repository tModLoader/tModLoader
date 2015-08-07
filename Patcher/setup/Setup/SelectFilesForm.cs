using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
    public partial class SelectFilesForm : Form
    {
        private string filter;

        public SelectFilesForm(string path = "", string filter = "") {
            InitializeComponent();
            textBoxPath.Text = path;
            this.filter = filter;
            ValidatePaths();
        }

        public string[] GetPaths() {
            return textBoxPath.Text.Split('|');
        }

        public string GetDirectory() {
            var dir = GetPaths()[0];
            return Directory.Exists(dir) ? dir : File.Exists(dir) ? Path.GetDirectoryName(dir) : Program.baseDir;
        }

        private void ValidatePaths() {
            buttonOk.Enabled = GetPaths().All(p => File.Exists(p) || Directory.Exists(p));
        }

        private void buttonFolder_Click(object sender, System.EventArgs e) {
            var dialog = new FolderBrowserDialog {
                SelectedPath = Path.GetFullPath(GetDirectory()),
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            textBoxPath.Text = dialog.SelectedPath;
        }

        private void buttonFile_Click(object sender, System.EventArgs e) {
            var dialog = new OpenFileDialog {
                Multiselect = true,
                InitialDirectory = Path.GetFullPath(GetDirectory()),
                Filter = filter
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            textBoxPath.Text = string.Join("|", dialog.FileNames);
        }

        private void textBoxPath_TextChanged(object sender, System.EventArgs e) {
            ValidatePaths();
        }
    }
}
