using System.Windows.Forms;
using Terraria.ModLoader.Setup.Properties;

namespace Terraria.ModLoader.Setup
{
	public class RegenSourceTask : CompositeTask
	{
		public RegenSourceTask(ITaskInterface taskInterface, params SetupOperation[] tasks) : base(taskInterface, tasks) { }

		public override bool StartupWarning() {
			if (Settings.Default.PatchMode == 2) {
				if (MessageBox.Show(
						"Patch mode will be reset from fuzzy to offset.\r\n",
						"Strict Patch Mode", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
					return false;
			}

			return MessageBox.Show(
					"Any changes in /src will be lost.\r\n",
					"Ready for Setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
				== DialogResult.OK;
		}

		public override void Run() {
			if (Settings.Default.PatchMode == 2) {
				Settings.Default.PatchMode = 1;
				Settings.Default.Save();
			}

			base.Run();
		}
	}
}