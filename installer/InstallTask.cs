using System;
using System.IO;
using System.Windows.Forms;

namespace Installer
{
	class InstallTask : Task
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
			pass.header = "";
			pass.maxProgress = 6;
			if (Installer.platform == Platform.WINDOWS)
			{
				pass.maxProgress += 5;
			}
			else
			{
				pass.maxProgress += 10;
			}
			pass.message = "Backing up Terraria";
			ReportProgress(args.background, 0, pass);
			string directory = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
			string backupFile = directory + "Terraria_" + Installer.version + ".exe";
			if (!File.Exists(backupFile))
			{
				File.Copy(file, backupFile);
			}
			pass.message = "Backing up Terraria even more";
			ReportProgress(args.background, -1, pass);
			Directory.CreateDirectory("Resources");
			ZipFile zip = new ZipFile("Resources" + Path.DirectorySeparatorChar + "Backups");
			zip["Vanilla"] = File.ReadAllBytes(file);
			pass.header = "Reading Install resources";
			string resourceFile = "Resources" + Path.DirectorySeparatorChar + "Install";
			if (!File.Exists(resourceFile))
			{
				MessageBox.Show("Could not find installation resource file", "Installation Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			ZipFile resources = ZipFile.Read(resourceFile, this, args, pass);
			pass.header = " ";
			pass.message = "Installing...";
			ReportProgress(args.background, -1, pass);
			if (!HasAllFiles(Installer.platform, resources))
			{
				return false;
			}
			string contentFolder = directory + "Content" + Path.DirectorySeparatorChar + "ModLoader";
			Directory.CreateDirectory(contentFolder);
			contentFolder += Path.DirectorySeparatorChar;
			if (Installer.platform == Platform.WINDOWS)
			{
				File.WriteAllBytes(file, resources["Windows.exe"]);
				ReportProgress(args.background, -1, pass);
				File.WriteAllBytes(contentFolder + "MysteryItem.png", resources["Content/MysteryItem.png"]);
			}
			else if (Installer.platform == Platform.MAC)
			{
				File.WriteAllBytes(file, resources["Mac.exe"]);
				ReportProgress(args.background, -1, pass);
				File.WriteAllBytes(directory, resources["MP3Sharp.dll"]);
				ReportProgress(args.background, -1, pass);
				File.WriteAllBytes(contentFolder + "MysteryItem.png", resources["Content/MysteryItem.png"]);
			}
			else if (Installer.platform == Platform.LINUX)
			{
				File.WriteAllBytes(file, resources["Linux.exe"]);
				ReportProgress(args.background, -1, pass);
				File.WriteAllBytes(directory, resources["MP3Sharp.dll"]);
				ReportProgress(args.background, -1, pass);
				File.WriteAllBytes(contentFolder + "MysteryItem.png", resources["Content/MysteryItem.png"]);
			}
			pass.message = "Backing up tModLoader";
			ReportProgress(args.background, -1, pass);
			pass.header = "Saving backups";
			zip["tModLoader"] = File.ReadAllBytes(file);
			zip.Write(this, args, pass);
			MessageBox.Show("Success!", "Install", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return true;
		}

		private bool HasAllFiles(Platform platform, ZipFile zip)
		{
			string[] files;
			if (platform == Platform.WINDOWS)
			{
				files = new string[] { "Windows.exe", "Content/MysteryItem.png" };
			}
			else if (platform == Platform.MAC)
			{
				files = new string[] { "Mac.exe", "MP3Sharp.dll", "Content/MysteryItem.png" };
			}
			else if (platform == Platform.LINUX)
			{
				files = new string[] { "Linux.exe", "MP3Sharp.dll", "Content/MysteryItem.png" };
			}
			else
			{
				return false;
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
