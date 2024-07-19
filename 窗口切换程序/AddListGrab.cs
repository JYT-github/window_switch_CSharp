using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 窗口切换程序
{
    public partial class AddListGrab : Form
    {

        public IntPtr grab_formHandle;
        public IntPtr GrabformHandle
        {
            get { return grab_formHandle; }
            set { grab_formHandle = value; }
        }

        public string grab_title;
        public string GrabTitle
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



        public AddListGrab()
        {
            InitializeComponent();
        }


        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(String className, String WindowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);


        Process[] ps;

        protected override void OnLoad(EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            SetWindowPos(this.Handle, -1, 0, 0, 0, 0, 1 | 2); //最后参数也有用1 | 4 p1.MainForm.Handle  我记得这个是MainForm吧  反正是主窗口
                                                              // SetWindowPos(this.printPreviewDialog1.Handle, -1, 0, 0, 0, 0, 1 | 2); //最后参数也有用1 | 4 p1.MainForm.Handle  我记得这个是MainForm吧  反正是主窗口
            getAllExe();
        }


        /**
         * 获取 当前系统所有 应用程序 
         * */
        public void getAllExe() {

            ps = Process.GetProcesses();
            listBox1.Items.Clear();
            foreach (Process p in ps)
            {
                listBox1.Items.Add(p.ProcessName + " - " + p.MainWindowTitle + " - " + p.Id);
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("请选择应用进程");
            }
            else {
                try
                {
                    GrabTitle = ps[listBox1.SelectedIndex].MainWindowTitle==""? ps[listBox1.SelectedIndex].ProcessName : ps[listBox1.SelectedIndex].MainWindowTitle;
                    GrabformHandle = FindWindow(null, ps[listBox1.SelectedIndex].MainWindowTitle);//(IntPtr)ps[listBox1.SelectedIndex].Id;
                    GrabKey = this.comboBox1.SelectedItem.ToString()[0];
                    this.DialogResult = DialogResult.OK;

                }
                catch {
                    MessageBox.Show("获取失败");
                }
                

            }
            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            getAllExe();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }



       
        // Token: 0x06000043 RID: 67
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        // Token: 0x0200000E RID: 14
        public struct RECT
        {
            // Token: 0x04000030 RID: 48
            public int Left;

            // Token: 0x04000031 RID: 49
            public int Top;

            // Token: 0x04000032 RID: 50
            public int Right;

            // Token: 0x04000033 RID: 51
            public int Bottom;
        }
        // Token: 0x04000005 RID: 5
        private bool isInMove = false;

        // Token: 0x04000006 RID: 6
        private Point oldPoint;


      

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            this.isInMove = true;
            this.oldPoint = base.PointToScreen(e.Location);
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            bool flag = !this.isInMove;
            if (!flag)
            {
                Point point = base.PointToScreen(e.Location);
                bool flag2 = point.X == this.oldPoint.X && point.Y == this.oldPoint.Y;
                if (!flag2)
                {
                    base.Location = new Point(base.Location.X + point.X - this.oldPoint.X, base.Location.Y + point.Y - this.oldPoint.Y);
                    this.oldPoint = point;

                }
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            this.isInMove = false;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ?
                          new SolidBrush(Color.FromArgb(93, 111, 141)) : new SolidBrush(e.BackColor);
            g.FillRectangle(brush, e.Bounds);
            e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font,
                     new SolidBrush(e.ForeColor), e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }
    }
}
