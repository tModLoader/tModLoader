using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
    public class SetupTask : CompositeTask
    {
        public SetupTask(ITaskInterface taskInterface, params Task[] tasks) : base(taskInterface, tasks) {}

		public override bool StartupWarning() {
			return MessageBox.Show(
					"Any changes in /src will be lost.\r\n" +
					"Decompilation may take a long time (1-3 hours) and consume a lot of RAM (2GB will not be enough)",
					"Ready for Setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                == DialogResult.OK;
	    }
    }
}
