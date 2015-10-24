using System;
using System.IO;
using System.Windows.Forms;

namespace Installer
{
	class DevSetupTask : Task
	{
		protected override bool DoTask(DoWorkArgs args)
		{
			string file = Installer.GetPath();
			if (!File.Exists(file))
			{
				MessageBox.Show("The file " + file + " does not exist.", "Installation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			ProgressChangedArgs pass = new ProgressChangedArgs();
			pass.main = args.main;
			pass.header = "Reading Install resources";
			pass.maxProgress = 2;
			if (Installer.platform == Platform.WINDOWS)
			{
				pass.maxProgress += 5;
			}
			else
			{
				pass.maxProgress += 12;
			}
			pass.message = "";
			ReportProgress(args.background, 0, pass);
			string resourceFile = "Resources" + Path.DirectorySeparatorChar + "Install";
			if (!File.Exists(resourceFile))
			{
				MessageBox.Show("Could not find installation resource file", "Installation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			ZipFile resources = ZipFile.Read(resourceFile, this, args, pass);
			pass.header = " ";
			pass.message = "Installing";
			ReportProgress(args.background, -1, pass);
			if (!HasAllFiles(Installer.platform, resources))
			{
				return false;
			}
			string directory = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
			if (Installer.platform == Platform.WINDOWS)
			{
				File.WriteAllBytes(directory + "TerrariaMac.exe", resources["Mac.exe"]);
				ReportProgress(args.background, -1, pass);
				File.WriteAllBytes(directory + "FNA.dll", resources["FNA.dll"]);
			}
			else
			{
				File.WriteAllBytes(directory + "TerrariaWindows.exe", resources["Windows.exe"]);
				ReportProgress(args.background, -1, pass);
				string fileToWrite = "Microsoft.Xna.Framework.dll";
				File.WriteAllBytes(directory + fileToWrite, resources[fileToWrite]);
				ReportProgress(args.background, -1, pass);
				fileToWrite = "Microsoft.Xna.Framework.Game.dll";
				File.WriteAllBytes(directory + fileToWrite, resources[fileToWrite]);
				ReportProgress(args.background, -1, pass);
				fileToWrite = "Microsoft.Xna.Framework.Graphics.dll";
				File.WriteAllBytes(directory + fileToWrite, resources[fileToWrite]);
				ReportProgress(args.background, -1, pass);
				fileToWrite = "Microsoft.Xna.Framework.Xact.dll";
				File.WriteAllBytes(directory + fileToWrite, resources[fileToWrite]);
			}
			pass.message = "Done";
			ReportProgress(args.background, -1, pass);
			MessageBox.Show("Success!", "Install", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return true;
		}

		private bool HasAllFiles(Platform platform, ZipFile zip)
		{
			string[] files;
			if (platform == Platform.WINDOWS)
			{
				files = new string[] { "Mac.exe", "FNA.dll" };
			}
			else
			{
				files = new string[]
				{ "Windows.exe", "Microsoft.Xna.Framework.dll", "Microsoft.Xna.Framework.Game.dll",
					"Microsoft.Xna.Framework.Graphics.dll", "Microsoft.Xna.Framework.Xact.dll"
				};
			}
			foreach (string file in files)
			{
				if (!zip.HasFile(file))
				{
					MessageBox.Show("Missing " + file + " from Install resources", "Installation Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}
			return true;
		}
	}
}
