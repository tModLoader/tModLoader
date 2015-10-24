using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Installer
{
	public partial class Installer : Form
	{
		public const string version = "v1.3.0.8";
		#if WINDOWS
        public const Platform platform = Platform.WINDOWS;

#elif MAC
        public const Platform platform = Platform.MAC;

#elif LINUX
        public const Platform platform = Platform.LINUX;

#elif SETUP
        public const Platform platform = Platform.SETUP;
#endif
		public Installer()
		{
			InitializeComponent();
		}

		private void Init(object sender, EventArgs e)
		{
			this.header.Text = "";
			this.message.Text = "";
			if (platform == Platform.SETUP)
			{
				this.choosePath.Enabled = false;
				this.install.Enabled = false;
				this.devSetup.Enabled = false;
				this.restoreVanilla.Enabled = false;
				this.restoreMod.Enabled = false;
			}
			else
			{
				this.setup.Enabled = false;
				this.setup.Visible = false;
			}
		}

		private void ChoosePath(object sender, EventArgs e)
		{
			new PathTask().Run(this);
		}

		private void Install(object sender, EventArgs e)
		{
			new InstallTask().Run(this);
		}

		private void SetupDevEnv(object sender, EventArgs e)
		{
			new DevSetupTask().Run(this);
		}

		private void RestoreVanilla(object sender, EventArgs e)
		{
			new RestoreTask(true).Run(this);
		}

		private void RestoreMod(object sender, EventArgs e)
		{
			new RestoreTask(false).Run(this);
		}

		private void Setup(object sender, EventArgs e)
		{
			new SetupTask().Run(this);
		}

		private void DragFile(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Link;
			}
		}

		private void DropFile(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			string file = null;
			foreach (string f in files)
			{
				if (Path.GetExtension(f) == ".exe")
				{
					file = f;
					break;
				}
			}
			if (file != null)
			{
				SetTerrariaPath(file);
			}
		}

		public void SetProgressVisible(bool visible)
		{
			this.progressBar.Visible = visible;
		}

		public int GetProgress()
		{
			return this.progressBar.Value;
		}

		public void SetProgress(int progress)
		{
			if (progress + 1 <= this.progressBar.Maximum)
			{
				this.progressBar.Value = progress + 1;
				this.progressBar.Value = progress;
			}
			else
			{
				this.progressBar.Maximum = this.progressBar.Value;
			}
		}

		public void IncrementProgress()
		{
			this.SetProgress(this.GetProgress() + 1);
		}

		public void SetMaxProgress(int maxProgress)
		{
			this.progressBar.Maximum = maxProgress;
		}

		public void SetHeader(string text)
		{
			this.header.Text = text;
		}

		public void SetMessage(string text)
		{
			this.message.Text = text;
		}

		private IList<Button> reenable = new List<Button>();

		public void DisableButtons()
		{
			foreach (Control control in this.Controls)
			{
				Button button = control as Button;
				if (button != null && button.Enabled)
				{
					button.Enabled = false;
					reenable.Add(button);
				}
			}
		}

		public void ReenableButtons()
		{
			foreach (Button button in reenable)
			{
				button.Enabled = true;
			}
			reenable.Clear();
		}

		public static string GetPath()
		{
			string file = "Resources" + Path.DirectorySeparatorChar + "Path.txt";
			if (!File.Exists(file))
			{
				return GetDefaultPath();
			}
			file = File.ReadAllText(file);
			if (!File.Exists(file))
			{
				return GetDefaultPath();
			}
			return file;
		}

		public static string GetDefaultPath()
		{
			if (platform == Platform.MAC)
			{
				string path = Environment.GetEnvironmentVariable("HOME");
				if (string.IsNullOrEmpty(path))
				{
					return ".";
				}
				path = Path.Combine(path, "Library", "Application Support");
				path = Path.Combine(path, "Steam", "steamapps", "common", "Terraria");
				path = Path.Combine(path, "Terraria.app", "Contents", "MacOS", "Terraria.exe");
				return path;
			}
			else if (platform == Platform.LINUX)
			{
				string path = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (string.IsNullOrEmpty(path))
				{
					path = Environment.GetEnvironmentVariable("HOME");
					if (string.IsNullOrEmpty(path))
					{
						return ".";
					}
					path = Path.Combine(path, ".local", "share");
				}
				path = Path.Combine(path, "Steam", "steamapps", "common", "Terraria", "Terraria.exe");
				return path;
			}
			else
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				bool use32Bit = false;
				if (path.Length == 0)
				{
					if (!Environment.Is64BitOperatingSystem)
					{
						return ".";
					}
					path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
					if (path.Length == 0)
					{
						return ".";
					}
					use32Bit = true;
				}
				path = Path.Combine(path, "Steam", "steamapps", "common", "Terraria", "Terraria.exe");
				if (!File.Exists(path))
				{
					if (use32Bit || !Environment.Is64BitOperatingSystem)
					{
						return ".";
					}
					path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
					path = Path.Combine(path, "Steam", "steamapps", "common", "Terraria", "Terraria.exe");
				}
				return path;
			}
		}

		public static void SetTerrariaPath(string file)
		{
			Directory.CreateDirectory("Resources");
			File.WriteAllText("Resources" + Path.DirectorySeparatorChar + "Path.txt", file);
			MessageBox.Show("Terraria path set to:" + Environment.NewLine + file, "Choose File",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
