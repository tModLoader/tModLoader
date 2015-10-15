using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Installer
{
    public partial class Installer : Form
    {
        public const string version = "v1.3.0.8";

        public Installer()
        {
            InitializeComponent();
        }

        private void Init(object sender, EventArgs e)
        {
            this.header.Text = "";
            this.message.Text = "";
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
            new SetupTask().Run(this);
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
            foreach(Control control in this.Controls)
            {
                Button button = control as Button;
                if(button != null && button.Enabled)
                {
                    button.Enabled = false;
                    reenable.Add(button);
                }
            }
        }

        public void ReenableButtons()
        {
            foreach(Button button in reenable)
            {
                button.Enabled = true;
            }
            reenable.Clear();
        }
    }
}
