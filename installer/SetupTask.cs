using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Ionic.Zlib;

namespace Installer
{
	class SetupTask : Task
	{
		protected override bool DoTask(DoWorkArgs args)
		{
			ProgressChangedArgs pass = new ProgressChangedArgs();
			pass.maxProgress = 1;
			pass.header = "";
			pass.message = "";
			pass.main = args.main;
			ReportProgress(args.background, 0, pass);
			Directory.CreateDirectory("Setup");
			if (!CreateResources(args, pass, "Windows", new string[]
				{
					"Windows.exe",
					"Mac.exe",
					"FNA.dll",
					"Content" + Path.DirectorySeparatorChar + "MysteryItem.png"
				}))
			{
				return false;
			}
			if (!CreateResources(args, pass, "Mac", new string[]
				{
					"Windows.exe",
					"Mac.exe",
					"Microsoft.Xna.Framework.dll",
					"Microsoft.Xna.Framework.Game.dll",
					"Microsoft.Xna.Framework.Graphics.dll",
					"Microsoft.Xna.Framework.Xact.dll",
					"MP3Sharp.dll",
					"Content" + Path.DirectorySeparatorChar + "MysteryItem.png"
				}))
			{
				return false;
			}
			if (!CreateResources(args, pass, "Linux", new string[]
				{
					"Windows.exe",
					"Linux.exe",
					"Microsoft.Xna.Framework.dll",
					"Microsoft.Xna.Framework.Game.dll",
					"Microsoft.Xna.Framework.Graphics.dll",
					"Microsoft.Xna.Framework.Xact.dll",
					"MP3Sharp.dll",
					"Content" + Path.DirectorySeparatorChar + "MysteryItem.png"
				}))
			{
				return false;
			}
			MessageBox.Show("Success!", "Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return true;
		}

		private bool CreateResources(DoWorkArgs args, ProgressChangedArgs pass, string platform, string[] files)
		{
			int progress = 0;
			pass.maxProgress = 2 * files.Length + 1;
			pass.header = "Creating " + platform + " installation resources";
			pass.message = "Finding files...";
			ReportProgress(args.background, progress, pass);
			string prefix = "Setup" + Path.DirectorySeparatorChar;
			for (int k = 0; k < files.Length; k++)
			{
				files[k] = prefix + files[k];
				if (!File.Exists(files[k]))
				{
					MessageBox.Show("The file " + files[k] + " does not exist.", "Setup Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}
			Directory.CreateDirectory("Resources");
			try
			{
				ZipFile zip = new ZipFile("Resources" + Path.DirectorySeparatorChar + "Install_" + platform);
				foreach (string file in files)
				{
					string fileName = file.Substring(prefix.Length + 1);
					fileName = fileName.Replace(Path.DirectorySeparatorChar, '/');
					pass.message = "Packing " + fileName + "...";
					progress++;
					ReportProgress(args.background, progress, pass);
					zip[fileName] = File.ReadAllBytes(file);
				}
				pass.message = "Saving installation resources...";
				progress++;
				ReportProgress(args.background, progress, pass);
				zip.Write(this, args);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Setup Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			return true;
		}
	}
}
