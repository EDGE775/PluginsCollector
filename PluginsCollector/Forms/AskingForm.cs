using PluginsCollector.Commands.ExecutableCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginsCollector.Forms
{
    public partial class AskingForm : Form
    {
        public int firstId = 0;
        public int lastId = 0;
        public bool clear = false;
        public AskingForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int.TryParse(textBoxLastId.Text, out lastId);
            int.TryParse(textBoxFirstId.Text, out firstId);

            //this.DialogResult = DialogResult.OK;
            //this.Close();
            clear = false;
            KPLN_Loader.Preferences.CommandQueue.Enqueue(new OverrideGraphicsByIdCommand(this));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBoxFirstId_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnReOver_Click(object sender, EventArgs e)
        {
            int.TryParse(textBoxLastId.Text, out lastId);
            int.TryParse(textBoxFirstId.Text, out firstId);

            //this.DialogResult = DialogResult.OK;
            //this.Close();
            clear = true;
            KPLN_Loader.Preferences.CommandQueue.Enqueue(new OverrideGraphicsByIdCommand(this));
        }
    }
}
