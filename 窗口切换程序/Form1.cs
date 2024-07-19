using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace 窗口切换程序
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool IsZoomed(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
       
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);
        // Token: 0x0600004D RID: 77
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, uint flags);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(int uAction, int uParam, IntPtr lpvParam, int fuWinIni);


        public Form1()
        {
            InitializeComponent();
        }
    
        MouseHook mh = new MouseHook();
        bool LeftTag = false;
        bool MiddleTag = false;
        
        bool RightTag = false;


        int isThatShow = 1;

        // 要切换窗口的租
        Dictionary<int, IntPtr> key_page = new Dictionary<int, IntPtr>();


        // 要隐藏 的窗体 内容
        public class Grab_key_page_item {
            private string _title;
            public string title
            {
                get { return _title; }
                set { _title = value; }
            }
            private string _grab_key;
            public string grab_key
            {
                get { return _grab_key; }
                set { _grab_key = value; }
            }
            private int _grabKey_int;
            public int grabKey_int
            {
                get { return _grabKey_int; }
                set { _grabKey_int = value; }
            }
            private IntPtr _intPtr;
            public IntPtr intPtr
            {
                get { return _intPtr; }
                set { _intPtr = value; }
            }
            private int _isshow;
            public int isshow
            {
                get { return _isshow; }
                set { _isshow = value; }
            }


        }
        BindingList< Grab_key_page_item> Grab_key_page = new BindingList<Grab_key_page_item>();

        int[] keys_arr = {49,50,51,52};// 数字键 1234

    
     

        private void button1_Click(object sender, EventArgs e)
        {
            
            //安装键盘钩子
            if (this.button1.Text == "启动")
            {

                that_start_hook();
            }
            else {
                Hook_Clear();
                mh.UnHook();
                this.button1.Text = "启动";
                this.timer.Enabled = false;
            }
        }

        private void that_start_hook() {
            Hook_Start();



            mh.SetHook();
            mh.MouseDownEvent += mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
            this.button1.Text = "停止";
            this.timer.Enabled = true;
        }



        //按下鼠标键触发的事件
        private void mh_MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                LeftTag = true;
            
            }
            if (e.Button == MouseButtons.Middle)
            {
                MiddleTag = true;
            }
            if (e.Button == MouseButtons.Right)
            {
                RightTag = true;
             
            }
      
        }
        //松开鼠标键触发的事件
        private void mh_MouseUpEvent(object sender, MouseEventArgs e)
        {


            if (e.Button == MouseButtons.Left)
            {
                LeftTag = false;
            }

            if (e.Button == MouseButtons.Middle)
            {
                MiddleTag = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                RightTag = false;
            }
           
        
           
        }






    

        private void Form1_Load(object sender, EventArgs e)
        {
            that_start_hook();


            SetWindowPos(this.Handle, -1, 0, 0, 0, 0, 0x0001 | 0x0002);
        }












        #region 屏蔽键盘第一步:声明API
        //设置钩子
        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //抽掉钩子
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll")]
        //调用下一个钩子
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);
        #endregion

        #region 屏蔽键盘第二步: 定义委托
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        static int hHook = 0;
        public const int WH_KEYBOARD_LL = 13;

        //LowLevel键盘截获，如果是WH_KEYBOARD＝2，并不能对系统键盘截取，Acrobat Reader会在你截取之前获得键盘。
        HookProc KeyBoardHookProcedure;
     

        //键盘Hook结构函数
        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        #endregion

        #region 屏蔽键盘第三步：编写钩子子程
        //钩子要做的事，你要处理什么？
        public  int KeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyBoardHookStruct kbh = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));


                if (Control.ModifierKeys.ToString() == "Control, Alt" && LeftTag)
                {
                    SetWindowPos(GetForegroundWindow(), -1, 0, 0, 0, 0, 0x0001 | 0x0002);
                    return 1;
                }
                if (Control.ModifierKeys.ToString() == "Control, Alt" && RightTag)
                {
                    SetWindowPos(GetForegroundWindow(), -2, 0, 0, 0, 0, 0x0001 | 0x0002);
                    return 1;
                }


                // 判断是否隐藏当前软件
                if (kbh.vkCode == 192 && (int)Control.ModifierKeys == (int)Keys.Alt) {
                    if (wParam.ToString().Substring(wParam.ToString().Length - 1, 1) == "0")
                    {
                        if (isThatShow == 1)
                        {
                            isThatShow = 0;
                        }
                        else
                        {
                            isThatShow = 1;
                        }

                        ShowWindow(this.Handle, isThatShow);
                        return 1;
                    }
                    
                }



                // 将当前窗体与 按键关联

                if (Array.IndexOf(keys_arr, kbh.vkCode) != -1)
                {



                    // 没有触发 隐藏显示效果
                    if (LeftTag)
                    {
                        if ((int)Control.ModifierKeys == (int)Keys.Alt)
                        {
                            if (Array.IndexOf(key_page.Keys.ToArray(), kbh.vkCode) != -1)
                            {
                                key_page.Remove(kbh.vkCode);

                            }
                            key_page.Add(kbh.vkCode, GetForegroundWindow());

                            return 1;

                        }
                        else
                        {
                            if (Array.IndexOf(key_page.Keys.ToArray(), kbh.vkCode) != -1)
                            {
                                if (IsZoomed(key_page[kbh.vkCode]) == true)
                                {
                                    ShowWindow(key_page[kbh.vkCode], 3);
                                }
                                else
                                {
                                    ShowWindow(key_page[kbh.vkCode], 1);
                                }

                                //SetForegroundWindow(key_page[e.KeyValue]);

                                SetWindowPos(key_page[kbh.vkCode], -1, 0, 0, 0, 0, 0x0001 | 0x0002);
                                SetWindowPos(key_page[kbh.vkCode], -2, 0, 0, 0, 0, 0x0001 | 0x0002);

                                return 1;
                            }
                        }



                    }
                    else {

                        // 如果 触发了 隐藏显示 效果
                        if ((int)Control.ModifierKeys == (int)Keys.Alt)
                        {
                            //https://blog.csdn.net/weixin_30616969/article/details/102426057
                            // 判断是否是按下 wParam = = 0x100 // 键盘按下 wParam = = 0x101 // 键盘抬起
                            // MessageBox.Show(wParam.ToString());
                            if (wParam.ToString().Substring(wParam.ToString().Length - 1, 1) == "0")
                            {
                                foreach (Grab_key_page_item that_item in this.Grab_key_page)
                                {

                                    if (that_item.grabKey_int == kbh.vkCode)
                                    {
                                        if (that_item.isshow == 1)
                                        {
                                            that_item.isshow = 0;
                                        }
                                        else
                                        {
                                            that_item.isshow = 1;
                                        }
                                        ShowWindow(that_item.intPtr, that_item.isshow);
                                    }

                                }
                            }

                            return 1;
                        }
                    }
           
                   
                    
                    return 0;
                }
                
                return 0;


                //if (kbh.vkCode == 91) // 截获左win(开始菜单键)
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == 92)// 截获右win
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.Escape && (int)Control.ModifierKeys == (int)Keys.Control) //截获Ctrl+Esc
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.F4 && (int)Control.ModifierKeys == (int)Keys.Alt) //截获alt+f4
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.F4 && (int)Control.ModifierKeys == (int)Keys.Alt + (int)Keys.Shift) //截获Alt+Shift+f4
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.Tab && (int)Control.ModifierKeys == (int)Keys.Alt) //截获alt+tab
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.Tab && (int)Control.ModifierKeys == (int)Keys.Alt + (int)Keys.Shift) //截获Alt+Shift+tab
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.Escape && (int)Control.ModifierKeys == (int)Keys.Alt)//截获alt+esc
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.Escape && (int)Control.ModifierKeys == (int)Keys.Alt + (int)Keys.Shift) //截获Alt+Shift+esc
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.Escape && (int)Control.ModifierKeys == (int)Keys.Control + (int)Keys.Shift) //截获Ctrl+Shift+Esc
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == (int)Keys.Space && (int)Control.ModifierKeys == (int)Keys.Alt) //截获alt+空格
                //{
                //    return 1;
                //}
                //if (kbh.vkCode == 241) //截获F1
                //{
                //    return 1;
                //}

                //if (kbh.vkCode == (int)Keys.Space && (int)Control.ModifierKeys == (int)Keys.Control + (int)Keys.Alt) //截获Ctrl+Alt+空格
                //{
                // return 1;
                //}
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }
        #endregion

        #region 屏蔽键盘第四步：调用的方法
        //打开钩子 ,并用流屏蔽任务管理器
        public void Hook_Start()
        {
            // 安装键盘钩子
            if (hHook == 0)
            {
                KeyBoardHookProcedure = new HookProc(KeyBoardHookProc);

                hHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyBoardHookProcedure,
                   GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

                //如果设置钩子失败.
                if (hHook == 0)
                {
                    Hook_Clear();
                 
                    //throw new Exception("设置Hook失败!");
                }
           
            }
        }
        //PS：也可以通过将[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System]
        //下的DisableTaskmgr项的值设为"1”来屏蔽任务管理器。

        //取消钩子事件 ,并关闭流，取消对任务管理器的屏蔽
        public void Hook_Clear()
        {
            bool retKeyboard = true;
            if (hHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }
        
            //如果去掉钩子失败.
            //if (!retKeyboard) throw new Exception("UnhookWindowsHookEx failed.");
        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hook_Clear();
            mh.UnHook();


            // 恢复显示
            foreach (Grab_key_page_item that_item in this.Grab_key_page)
            {
                ShowWindow(that_item.intPtr, 1);
            }
        }


        //获取窗口标题
        [DllImport("user32", SetLastError = true)]
        public static extern int GetWindowText(
        IntPtr hWnd, //窗口句柄
        StringBuilder lpString, //标题
        int nMaxCount  //最大值
        );
        //获取类的名字
        [DllImport("user32.dll")]
        private static extern int GetClassName(
            IntPtr hWnd, //句柄
            StringBuilder lpString, //类名
            int nMaxCount //最大值
        );
        //根据坐标获取窗口句柄
        [DllImport("user32")]
        private static extern IntPtr WindowFromPoint(
        Point Point  //坐标
        );

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]

        /// <summary>

        /// 该函数获得一个指定子窗口的父窗口句柄。

        /// </summary>

        public static extern IntPtr GetParent(IntPtr hWnd);

        // 抓取 串口句柄 按下
        private void GrabBtn_MouseDown(object sender, MouseEventArgs e)
        {
            this.GrabTimer.Enabled = !this.GrabTimer.Enabled;
        }
        // 抓取 串口句柄 松手
        private void GrabBtn_MouseUp(object sender, MouseEventArgs e)
        {
            this.GrabTimer.Enabled = !this.GrabTimer.Enabled;
            //MessageBox.Show("窗口句柄:" + grab_formHandle.ToString() + Environment.NewLine + "窗口标题:" + grab_title + Environment.NewLine);

            while (0 != GetParent(grab_formHandle).ToInt32())
            {
                
                grab_formHandle = GetParent(grab_formHandle);
            };

            GetWindowText(grab_formHandle, grab_title, grab_title.Capacity);//得到窗口的标题



            // 显示 添加窗口

            AddGrab add_Grab = new AddGrab(grab_formHandle, grab_title);

            add_Grab.ShowDialog();
            if (add_Grab.DialogResult == DialogResult.OK)
            {
                Grab_key_page_item that_item = new Grab_key_page_item();
                that_item.intPtr = add_Grab.GrabformHandle;
                that_item.grabKey_int = Convert.ToInt32(add_Grab.GrabKey);
                that_item.title = add_Grab.GrabTitle.ToString();
                that_item.grab_key = "Alt + " + add_Grab.GrabKey;
                that_item.isshow = 1;
                Grab_key_page.Add(that_item);

            }
            dataGridView1.DataSource = Grab_key_page;
            //dataGridView1.DataSource = new BindingSource(new BindingList<Grab_key_page_item>(Grab_key_page), null);
            dataGridView1.Columns["grabKey_int"].Visible = false;
            dataGridView1.Columns["intPtr"].Visible = false;
            dataGridView1.Columns["isshow"].Visible = false;
        }


        IntPtr grab_formHandle = new IntPtr();
        StringBuilder grab_title = new StringBuilder(256);

        private void GrabTimer_Tick(object sender, EventArgs e)
        {
            int x = Cursor.Position.X;
            int y = Cursor.Position.Y;
            Point p = new Point(x, y);
            grab_formHandle = WindowFromPoint(p);//得到窗口句柄

            

            //StringBuilder className = new StringBuilder(256);
            //GetClassName(formHandle, className, className.Capacity);//得到窗口的类名
            //this.textBox1.Text = "窗口句柄:" + formHandle.ToString() + Environment.NewLine + "窗口标题:" + title + Environment.NewLine;// + "类名:" + className;
        }

        private void GrabBtn_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DataGridViewSelectedRowCollection rows = this.dataGridView1.SelectedRows;

            foreach (DataGridViewRow row in rows)
            {
                this.Grab_key_page.RemoveAt(row.Index);
            }

          
        }

      

        private void button3_Click(object sender, EventArgs e)
        {
            AddListGrab add_listGrab = new AddListGrab();

            add_listGrab.ShowDialog();
            if (add_listGrab.DialogResult == DialogResult.OK)
            {
                Grab_key_page_item that_item = new Grab_key_page_item();
                that_item.intPtr = add_listGrab.GrabformHandle;
                that_item.grabKey_int = Convert.ToInt32(add_listGrab.GrabKey);
                that_item.title = add_listGrab.GrabTitle;
                that_item.grab_key ="Alt + "+add_listGrab.GrabKey;
                that_item.isshow = 1;
                Grab_key_page.Add(that_item);

            }
            dataGridView1.DataSource = Grab_key_page;
            //dataGridView1.DataSource = new BindingSource(new BindingList<Grab_key_page_item>(Grab_key_page), null);
            dataGridView1.Columns["grabKey_int"].Visible = false;
            dataGridView1.Columns["intPtr"].Visible = false;
            dataGridView1.Columns["isshow"].Visible = false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Hook_Clear();
            mh.UnHook();

            Hook_Start();


            mh.hProc = null;
            mh.SetHook();

            mh.MouseDownEvent += mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
        }



        // Token: 0x06000047 RID: 71
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
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

      
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form1_FormClosing(sender,null);
            Environment.Exit(0);
        }

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


        bool is_form_height = false;
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            is_form_height = !is_form_height;
            if (is_form_height)
            {
                this.Height = 530;
            }
            else {
                this.Height = 220;
            }
        }

        // 缩小
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowWindow(this.Handle, 1);
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1_FormClosing(sender, null);
            Environment.Exit(0);
        }






        ////窗口移动
        //[DllImport("user32.dll")]
        //public static extern bool ReleaseCapture();
        //[DllImport("user32.dll")]
        //public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        //public const int WM_SYSCOMMAND = 0x0112;
        //public const int SC_MOVE = 0xF010;
        //public const int HTCAPTION = 0x0002;

        //private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    ReleaseCapture();
        //    SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        //}

        //private void label1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    pictureBox1_MouseDown(sender,e);
        //}
    }

}
