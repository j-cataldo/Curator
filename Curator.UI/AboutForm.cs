using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Curator.UI
{
    /// <summary>
    /// About Dialog, implemented as a singleton class object
    /// Only one instance of this class can exist at a time.
    /// </summary>

    public partial class AboutForm : Form
    {
        private static AboutForm _formInstance;
        private static readonly object _formInstanceSync = new object(); // In case we want to multithread UI later

        private AboutForm()
        {
            InitializeComponent();
        }

        public static AboutForm GetInstance
        {
            get
            {
                if (_formInstance == null || !_formInstance.Visible)
                {
                    lock (_formInstanceSync)
                    {
                        if (_formInstance == null || !_formInstance.Visible)
                        {
                            _formInstance = new AboutForm();
                            _formInstance.ShowDialog();
                        }
                    }
                }

                // Somewhat of a hack to bring the dialog to the front
                // Remember to have caller set TopMost property to false afterwards
                _formInstance.TopMost = true;
                return _formInstance;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
