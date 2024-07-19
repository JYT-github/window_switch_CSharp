using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 窗口切换程序
{
    public partial class AddGrab : Form
    {
      
        public IntPtr grab_formHandle;
        public IntPtr GrabformHandle
        {
            get { return grab_formHandle; }
            set { grab_formHandle = value; }
        }

        public StringBuilder grab_title;
        public StringBuilder GrabTitle
        {
            get { return grab_title; }
            set { grab_title = value; }
        }

        public char grab_key;
        public char GrabKey
        {
            get { return grab_key; }
            set { grab_key = value; }
        }

        public AddGrab(IntPtr grab_formHandle_value, StringBuilder grab_title_value)
        {
            InitializeComponent();

            GrabformHandle = grab_formHandle_value;
            this.grab_title_label.Text = "窗口名称："+ grab_title_value.ToString();
            GrabTitle = grab_title_value;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);

        protected override void OnLoad(EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            SetWindowPos(this.Handle, -1, 0, 0, 0, 0, 1 | 2); //最后参数也有用1 | 4 p1.MainForm.Handle  我记得这个是MainForm吧  反正是主窗口
                                                              // SetWindowPos(this.printPreviewDialog1.Handle, -1, 0, 0, 0, 0, 1 | 2); //最后参数也有用1 | 4 p1.MainForm.Handle  我记得这个是MainForm吧  反正是主窗口

        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            GrabKey = this.comboBox1.SelectedItem.ToString()[0];
            this.DialogResult = DialogResult.OK;
            
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int command);
        private void button2_Click(object sender, EventArgs e)
        {
            ShowWindow(GrabformHandle, 3);
        }
    }
}
