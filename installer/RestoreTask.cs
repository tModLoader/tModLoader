using System;
using System.IO;
using System.Windows.Forms;

namespace Installer
{
	class RestoreTask : Task
	{
		private bool vanilla;

		public RestoreTask(bool vanilla)
		{
			this.vanilla = vanilla;
		}

		protected override bool DoTask(DoWorkArgs args)
		{
			string file = Installer.GetPath();
			if (!File.Exists(file))
			{
				MessageBox.Show("The file " + file + " does not exist.", "Restore Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			string backupFile = "Resources" + Path.DirectorySeparatorChar + "Backups";
			if (!File.Exists(backupFile))
			{
				MessageBox.Show("Either tModLoader has not been installed or the backups resource file is missing.",
					"Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			ZipFile backups = ZipFile.Read(backupFile);
			string fetchFile = vanilla ? "Vanilla" : "tModLoader";
			if (!backups.HasFile(fetchFile))
			{
				MessageBox.Show("Missing " + file + " from Backups resources", "Installation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			File.WriteAllBytes(file, backups[fetchFile]);
			MessageBox.Show("Success!", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return true;
		}
	}
}
