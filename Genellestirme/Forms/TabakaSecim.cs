using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Genellestirme
{
    public partial class TabakaSecim : Form
    {
        public TabakaSecim()
        {
            InitializeComponent();
        }

        private void SecimFormu_Load(object sender, EventArgs e)
        {
            comboBox1.Focus();
            this.AcceptButton = button1;
            this.CancelButton = button2;
        }
    }
}
