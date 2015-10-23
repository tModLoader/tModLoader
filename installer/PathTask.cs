using System;
using System.IO;
using System.Windows.Forms;

namespace Installer
{
	class PathTask : Task
	{
		protected override bool DoTask(DoWorkArgs args)
		{
			return (bool)args.main.Invoke((Func<bool>)ChoosePath);
		}

		private bool ChoosePath()
		{
			if (Installer.platform == Platform.MAC || Installer.platform == Platform.LINUX)
			{
				MessageBox.Show("Please drag and drop the Terraria executable you wish to mod onto the installer window.",
					"Choose File", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return true;
			}
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Executable (.exe)|*.exe";
			string initial = Path.GetDirectoryName(Installer.GetPath());
			if (Directory.Exists(initial))
			{
				dialog.InitialDirectory = initial;
			}
			dialog.Title = "Select Terraria executable";
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return false;
			}
			string file = dialog.FileName;
			if (!File.Exists(file))
			{
				MessageBox.Show(file + " does not exist", "Choose File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			Installer.SetTerrariaPath(file);
			return true;
		}
	}
}
