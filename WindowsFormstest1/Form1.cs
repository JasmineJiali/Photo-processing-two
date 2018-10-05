using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Resources;
using System.Threading;


namespace WindowsFormstest1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private String curFileName = null;
        //当前图像变量  
        public Bitmap curBitmap = null;
        //原始图像变量  
        public Bitmap srcBitmap = null;

        public Bitmap[] wavingBitmap = new Bitmap[5];
        public int count = 4;

        private int flag = 0;
        private int method = 0;
        public void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif;" +
                   "*.tif; *.ico; *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf|" +
                   "位图( *.bmp; *.jpg; *.png;...) | *.bmp; *.pcx; *.png; *.jpg; *.gif; *.tif; *.ico|" +
                   "矢量图( *.wmf; *.eps; *.emf;...) | *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf";
            ofd.ShowHelp = true;
            ofd.Title = "打开图像文件";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                curFileName = ofd.FileName;
                try
                {
                    curBitmap = (Bitmap)System.Drawing.Image.FromFile(curFileName);
                    srcBitmap = new Bitmap(curBitmap);
                }
                catch (Exception exp)
                { MessageBox.Show(exp.Message); }
            }
        }//图像读取
        public Color neighbor(Bitmap srcImage, double newx, double newy)//最近邻插值
        {
            int x = srcImage.Width;
            int y = srcImage.Height;

            int x1 = (int)Math.Round(newx);
            int y1 = (int)Math.Round(newy);

            if (x1 > x - 1)//边界检查
                x1 = x - 1;
            else if (x1 < 0)
                x1 = 0;
            if (y1 > y - 1)
                y1 = y - 1;
            else if (y1 < 0)
                y1 = 0;
            return srcImage.GetPixel(x1, y1);
        }
        public Color bilinear(Bitmap srcImage, double newx, double newy)//双线性插值
        {
            int x = srcImage.Width;
            int y = srcImage.Height;

            int x1 = (int)Math.Floor(newx);
            int y1 = (int)Math.Floor(newy);

            if (x1 > x - 2)//边界检查
                x1 = x - 2;
            else if (x1 < 0)
                x1 = 0;
            if (y1 > y - 2)
                y1 = y - 2;
            else if (y1 < 0)
                y1 = 0;

            int x2 = x1 + 1;
            int y2 = y1 + 1;

            Color newcolor;
            //双线性内插RGB通道实现
            double B = srcImage.GetPixel(x1, y1).B * (x2 - newx) * (y2 - newy) +
                srcImage.GetPixel(x2, y1).B * (newx - x1) * (y2 - newy) +
                srcImage.GetPixel(x1, y2).B * (x2 - newx) * (newy - y1) +
                srcImage.GetPixel(x2, y2).B * (newx - x1) * (newy - y1);
            double R = srcImage.GetPixel(x1, y1).R * (x2 - newx) * (y2 - newy) +
               srcImage.GetPixel(x2, y1).R * (newx - x1) * (y2 - newy) +
               srcImage.GetPixel(x1, y2).R * (x2 - newx) * (newy - y1) +
               srcImage.GetPixel(x2, y2).R * (newx - x1) * (newy - y1);
            double G = srcImage.GetPixel(x1, y1).G * (x2 - newx) * (y2 - newy) +
               srcImage.GetPixel(x2, y1).G * (newx - x1) * (y2 - newy) +
               srcImage.GetPixel(x1, y2).G * (x2 - newx) * (newy - y1) +
               srcImage.GetPixel(x2, y2).G * (newx - x1) * (newy - y1);

            newcolor = Color.FromArgb((int)R, (int)G, (int)B);
            return newcolor;
        }
        public Color bicubic(Bitmap srcImage, double newx, double newy)//双三次插值
        {
            int x = srcImage.Width;
            int y = srcImage.Height;

            int x1 = (int)Math.Floor(newx);
            int y1 = (int)Math.Floor(newy);

            if (x1 > x - 3)//边界检查
                x1 = x - 3;
            else if (x1 < 1)
                x1 = 1;
            if (y1 > y - 3)
                y1 = y - 3;
            else if (y1 < 1)
                y1 = 1;

            double u = newx - x1;
            double v = newy - y1;
            //插值基函数
            double[] get_u = new double[4];
            double[] get_v = new double[4];
            //插值基函数实现
            get_u[0] = 4 - 8 * (u + 1) + 5 * (u + 1) * (u + 1) - (u + 1) * (u + 1) * (u + 1);
            get_u[1] = 1 - 2 * u * u + u * u * u;
            get_u[2] = 1 - 2 * (1 - u) * (1 - u) + (1 - u) * (1 - u) * (1 - u);
            get_u[3] = 4 - 8 * (2 - u) + 5 * (2 - u) * (2 - u) - (2 - u) * (2 - u) * (2 - u);

            get_v[0] = 4 - 8 * (v + 1) + 5 * (v + 1) * (v + 1) - (v + 1) * (v + 1) * (v + 1);
            get_v[1] = 1 - 2 * v * v + v * v * v;
            get_v[2] = 1 - 2 * (1 - v) * (1 - v) + (1 - v) * (1 - v) * (1 - v);
            get_v[3] = 4 - 8 * (2 - v) + 5 * (2 - v) * (2 - v) - (2 - v) * (2 - v) * (2 - v);

            double R = 0;
            double G = 0;
            double B = 0;
            //双三次内插实现
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    R = R + srcImage.GetPixel(x1 + i - 1, y1 + j - 1).R * get_u[i] * get_v[j];
                    G = G + srcImage.GetPixel(x1 + i - 1, y1 + j - 1).G * get_u[i] * get_v[j];
                    B = B + srcImage.GetPixel(x1 + i - 1, y1 + j - 1).B * get_u[i] * get_v[j];
                }
            }
            //RGB通道边界判断
            if (R > 255) R = 255;
            if (G > 255) G = 255;
            if (B > 255) B = 255;

            if (R < 0) R = 0;
            if (G < 0) G = 0;
            if (B < 0) B = 0;

            Color newcolor;
            newcolor = Color.FromArgb((int)R, (int)G, (int)B);
            return newcolor;
        }

        public Bitmap rotating(Bitmap srcImage, double R, double theta, int flag)//旋转扭曲
        {
            int x = srcImage.Width;
            int y = srcImage.Height;

            double zx = x / 2;
            double zy = y / 2;

            int corex = (int)Math.Floor(zx);
            int corey = (int)Math.Floor(zy);//旋转中心点

            Bitmap dstImage = new Bitmap(512, 512);
            //遍历
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    double r = Math.Sqrt((i - corex) * (i - corex) + (j - corey) * (j - corey));//极坐标r

                    if (R > r && r != 0)//在旋转圆内进行操作
                    {
                        double alpha = Math.Asin((corey - j) / r);
                        if (i < corex)
                        {
                            if (j < corey) alpha = 3.1415926 - alpha;
                            else alpha = -3.1415926 - alpha;
                        }//极坐标alpha

                        double newx = r * Math.Cos(alpha + theta * (R - r) / R) + corex;
                        double newy = -r * Math.Sin(alpha + theta * (R - r) / R) + corey;//旋转后的坐标（在原图中的对应点）
                        Color newcolor;
                        if (flag == 0)//最近邻
                        {
                            newcolor = neighbor(srcImage, newx, newy);
                            dstImage.SetPixel(i, j, newcolor);
                        }
                        else if (flag == 1)//双线性
                        {
                            newcolor = bilinear(srcImage, newx, newy);
                            dstImage.SetPixel(i, j, newcolor);
                        }
                        else if (flag == 2)//双三次
                        {
                            newcolor = bicubic(srcImage, newx, newy);
                            dstImage.SetPixel(i, j, newcolor);
                        }
                    }
                    else dstImage.SetPixel(i, j, srcImage.GetPixel(i, j));//旋转圆外不做变化
                }
            }
            return dstImage;
        }
        public Bitmap waving(Bitmap srcImage, double R, double theta, double rho, int flag)//水波纹
        {
            int x = srcImage.Width;
            int y = srcImage.Height;

            double zx = x / 2;
            double zy = y / 2;

            int corex = (int)Math.Floor(zx);
            int corey = (int)Math.Floor(zy);//旋转中心点

            Bitmap dstImage = new Bitmap(512, 512);

            //遍历
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    double r = Math.Sqrt((i - corex) * (i - corex) + (j - corey) * (j - corey));//极坐标r

                    if (R > r && r != 0)//水波纹圆内进行操作
                    {
                        double alpha = Math.Asin((corey - j) / r);
                        if (i < corex)
                        {
                            if (j < corey) alpha = 3.1415926 - alpha;
                            else alpha = -3.1415926 - alpha;
                        }//极坐标alpha

                        double newx = r * Math.Cos(alpha + 0.05 * Math.Sin(r / R * rho + theta)) + corex;
                        double newy = -r * Math.Sin(alpha + 0.05 * Math.Sin(r / R * rho + theta)) + corey;//变换坐标（在原图中的对应点）

                        Color newcolor;
                        if (flag == 0)//最近邻
                        {
                            newcolor = neighbor(srcImage, newx, newy);
                            dstImage.SetPixel(i, j, newcolor);
                        }
                        else if (flag == 1)//双线性
                        {
                            newcolor = bilinear(srcImage, newx, newy);
                            dstImage.SetPixel(i, j, newcolor);
                        }
                        else if (flag == 2)//双三次
                        {
                            newcolor = bicubic(srcImage, newx, newy);
                            dstImage.SetPixel(i, j, newcolor);
                        }
                    }
                    else dstImage.SetPixel(i, j, srcImage.GetPixel(i, j));//水波纹圆外不做变换
                }
            }
            return dstImage;
        }

        public Bitmap resize(Bitmap srcImage, int width, int height)//重新调整大小至窗口大小，默认使用双线性插值
        {
            int cols = srcImage.Width;
            int rows = srcImage.Height;

            Bitmap dstImage = new Bitmap(width, height);

            int dstRows = dstImage.Height;
            int dstCols = dstImage.Width;

            float scale_x = (float)cols / dstCols;//缩放倍数
            float scale_y = (float)rows / dstRows;
            for (int i = 0; i < dstCols; i++)
            {
                for (int j = 0; j < dstRows; j++)
                {
                    double newx = i * scale_x;
                    double newy = j * scale_y;//插值坐标
                                              //if (flag == 0)
                    Color newcolor = bilinear(srcImage, newx, newy);
                    dstImage.SetPixel(i, j, newcolor);
                    //else if (flag == 1)
                    //  dstImage.at<cv::Vec3b>(i, j) = bilinear(srcImage, newx, newy);
                    // else if (flag == 2)
                    //  dstImage.at<cv::Vec3b>(i, j) = bicubic(srcImage, newx, newy);
                }
            }
            return dstImage;
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        public void button1_Click(object sender, EventArgs e)//打开文件按钮实现
        {
            OpenFile();
            srcBitmap = resize(srcBitmap, 512, 512);
            if (curBitmap != null)
            {
                pictureBox1.Image = (Image)srcBitmap;
            }
        }

        //选择变换方法
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            method = 1;
        }//选择旋转扭曲
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            method = 2;
        }//选择水波纹

        //选择插值方法
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            flag = 1;
        }//选择最近邻插值
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            flag = 2;
        }//选择双线性插值
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            flag = 3;
        }//选择双三次插值

        private void button2_Click(object sender, EventArgs e)//图像执行变换
        {

            if ((checkBox1.Checked == false && checkBox2.Checked == false) || (checkBox3.Checked == false&& checkBox4.Checked == false&& checkBox5.Checked == false))
            {
                this.timer1.Enabled = false;//关闭Timer
                MessageBox.Show("请选择变换方法或插值方法", "提示");
            }
            else if (checkBox1.Checked == true)//旋转
            {
                this.timer1.Enabled = false;
                if (checkBox3.Checked == true)//最近邻
                    curBitmap = rotating(srcBitmap, 250, 2, 0);
                else if (checkBox4.Checked == true)//双线性
                    curBitmap = rotating(srcBitmap, 250, 2, 1);
                else if (checkBox5.Checked == true)//双三次
                    curBitmap = rotating(srcBitmap, 250, 2, 1);

                pictureBox1.Image = (Image)curBitmap;

            }
            else if (checkBox2.Checked == true)//水波纹
            {
                this.timer1.Enabled = true;//打开Timer
                if (checkBox3.Checked == true)//最近邻
                {
                    wavingBitmap[0] = waving(srcBitmap, 250, -3.1415926 / 2, 50, 0);
                    wavingBitmap[1] = waving(srcBitmap, 250, 0, 50, 0);
                    wavingBitmap[2] = waving(srcBitmap, 250, 3.1415926 / 2, 50, 0);
                    wavingBitmap[3] = waving(srcBitmap, 250, 3.1415926, 50, 0);
                    wavingBitmap[4] = waving(srcBitmap, 250, 3.1415926 * 3 / 2, 50, 0);
                    curBitmap = waving(srcBitmap, 250, 3.1415926 * 3 / 2, 50, 0);//存储所用的图片
                }
                else if (checkBox4.Checked == true)//双线性
                {
                    wavingBitmap[0] = waving(srcBitmap, 250, -3.1415926 / 2, 50, 1);
                    wavingBitmap[1] = waving(srcBitmap, 250, 0, 50, 1);
                    wavingBitmap[2] = waving(srcBitmap, 250, 3.1415926 / 2, 50, 1);
                    wavingBitmap[3] = waving(srcBitmap, 250, 3.1415926, 50, 1);
                    wavingBitmap[4] = waving(srcBitmap, 250, 3.1415926 * 3 / 2, 50, 1);
                    curBitmap = waving(srcBitmap, 250, 3.1415926 * 3 / 2, 50, 1);//存储所用图片
                }
                else if (checkBox5.Checked == true)//双三次
                {
                    wavingBitmap[0] = waving(srcBitmap, 250, -3.1415926 / 2, 50, 2);
                    wavingBitmap[1] = waving(srcBitmap, 250, 0, 50, 2);
                    wavingBitmap[2] = waving(srcBitmap, 250, 3.1415926 / 2, 50, 2);
                    wavingBitmap[3] = waving(srcBitmap, 250, 3.1415926, 50, 2);
                    wavingBitmap[4] = waving(srcBitmap, 250, 3.1415926 * 3 / 2, 50, 2);
                    curBitmap = waving(srcBitmap, 250, 3.1415926 * 3 / 2, 50, 2);//存储所用图片
                }
            }

        }
        private void button3_Click(object sender, EventArgs e)//清除变换
        {
            this.timer1.Stop();//关闭Timer
            pictureBox1.Image = (Image)srcBitmap;//显示原图
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            flag = 0;method = 0;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)//计时器动态显示图片形成波动效果，Timer为200ms
        {
            pictureBox1.Image = (Image)wavingBitmap[count--];//循环显示
            if (count == 0) count = 4;
        }

        private void button4_Click(object sender, EventArgs e)//存储图像
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存为";
            sfd.OverwritePrompt = true;
            sfd.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif;" +
                   "*.tif; *.ico; *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf|" +
                   "位图( *.bmp; *.jpg; *.png;...) | *.bmp; *.pcx; *.png; *.jpg; *.gif; *.tif; *.ico|" +
                   "矢量图( *.wmf; *.eps; *.emf;...) | *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    curBitmap.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);//保存当前图片
                    MessageBox.Show("保存成功！", "提示");
                }
                catch (Exception exp)
                { MessageBox.Show(exp.Message); }
            }
        }

        private void button5_Click(object sender, EventArgs e)//退出程序
        {
            System.Environment.Exit(0);
        }
        
    }
}


