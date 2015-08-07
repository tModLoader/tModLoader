using System;
using System.Linq;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
    class SetupTask : Task
    {
	    private Task[] tasks;
	    private Task failed;

        public SetupTask(ITaskInterface taskInterface, params Task[] tasks) : base(taskInterface) {
	        this.tasks = tasks;
        }

	    public override bool ConfigurationDialog() {
		    return tasks.All(task => task.ConfigurationDialog());
	    }

		public override bool StartupWarning() {
			return MessageBox.Show(
					"Any changes in /src will be lost.\r\n" +
					"Decompilation may take a long time (1-3 hours) and consume a lot of RAM (2GB will not be enough)",
					"Ready for Setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                == DialogResult.OK;
	    }

	    public override bool Failed() {
		    return failed != null;
	    }

	    public override void FinishedDialog() {
		    if(failed != null)
				failed.FinishedDialog();
			else
				foreach(var task in tasks)
					task.FinishedDialog();
	    }

	    public override void Run() {
			foreach(var task in tasks)
		    {
			    task.Run();
				if (task.Failed()) {
					failed = task;
					return;
				}
			}
        }
    }
}
