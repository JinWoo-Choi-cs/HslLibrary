using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslBase.HDatabase;
using HslBase.HXml;

namespace FormTest
{
    public partial class Form1 : Form
    {
        HslMySql _hMySql;
        HslXml _hXml;

        public Form1()
        {
            InitializeComponent();

            //int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            //style |= NativeWinAPI.WS_EX_COMPOSITED;
            //NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            capture(chart1, @"c:\temp.png");
        }

        private void capture(Control ctrl, string fileName)
        {
            Rectangle bounds = ctrl.Bounds;
            Point pt = ctrl.PointToScreen(bounds.Location);
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(new Point(pt.X - ctrl.Location.X, pt.Y - ctrl.Location.Y), Point.Empty, bounds.Size);
            }

            bitmap.Save(fileName, ImageFormat.Png);
        }

        int count1 = 1;
        int count2 = 1;

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = count1.ToString();
            label2.Text = count2.ToString();

            count1 = count1 + 1;
            count2 = count2 + 5;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HslBase.HData.HslData.CreateFolderIfNotExists(@"C:\TPTP\TestPP\wow");
        }
    }
}
