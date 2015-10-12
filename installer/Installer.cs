using System;
using System.Windows.Forms;

namespace Installer
{
    public partial class Installer : Form
    {
        public Installer()
        {
            InitializeComponent();
        }

        private void Init(object sender, EventArgs e)
        {
#if SETUP
            this.choosePath.Enabled = false;
            this.install.Enabled = false;
            this.devSetup.Enabled = false;
            this.restoreVanilla.Enabled = false;
            this.restoreMod.Enabled = false;
#else
            this.setup.Enabled = false;
            this.setup.Visible = false;
#endif
        }

        private void ChoosePath(object sender, EventArgs e)
        {
            
        }

        private void Install(object sender, EventArgs e)
        {

        }

        private void SetupDevEnv(object sender, EventArgs e)
        {

        }

        private void RestoreVanilla(object sender, EventArgs e)
        {

        }

        private void RestoreMod(object sender, EventArgs e)
        {

        }

        private void Setup(object sender, EventArgs e)
        {
            SetupTask.Setup();
        }
    }
}
