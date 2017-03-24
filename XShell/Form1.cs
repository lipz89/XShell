using System;
using System.Windows.Forms;

namespace XShell
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
            this.Text = "XShell";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "可执行文件|*.exe"
            };
            var da = ofd.ShowDialog();
            if (da == DialogResult.OK)
            {
                var file = ofd.FileName;
                label1.Text = file;
                label2.Text = "准备就绪。";
                button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(label1.Text))
            {
                try
                {
                    Shell.Press(label1.Text);
                    label2.Text = "加壳成功。";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message, "XShell");
                }
            }
        }
    }
}
