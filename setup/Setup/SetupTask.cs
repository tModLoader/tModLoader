using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
	public class SetupTask : CompositeTask
	{
		public SetupTask(ITaskInterface taskInterface, params SetupOperation[] tasks) : base(taskInterface, tasks) {}

		public override bool StartupWarning() {
#if AUTO
			return true;
#endif
			return MessageBox.Show(
				       "Any changes in /src will be lost.\r\n",
				       "Ready for Setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
			       == DialogResult.OK;
		}
	}
}
