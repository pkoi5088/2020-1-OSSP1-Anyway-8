using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GIFMaker.Core;
using System.Threading;

namespace GIFMaker
{

    public partial class Form3 : MetroFramework.Forms.MetroForm
    {
        private VideoManager vManager;

        private string title;
        private string filePath;
        private string outputPath;
        //Passvalue 제목 movePath 파일 현재위치 imagePath 결과 위치
        public string Passvalue
        {
            get { return this.title; }
            set { this.title = value; }
        }
        public string movePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }
        public string imagePath
        {
            get { return this.outputPath; }
            set { this.outputPath = value; }
        }

        public Form3()
        {
            InitializeComponent();
            //dateTimePicker1.ShowUpDown = true;
            
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            metroLabel1.Text = title;//폼1에서 입력받은 URL을 받아옴.
            metroLabel2.Text = outputPath;//폼1에서 입력받은 URL을 받아옴.
            try
            {
                vManager = new VideoManager(filePath);
                numericUpDown1.Maximum = vManager.duration;
                numericUpDown2.Maximum = vManager.duration;
                numericUpDown3.Maximum = vManager.width;
                numericUpDown4.Maximum = vManager.height;
                numericUpDown1.Value = 0;
                numericUpDown2.Value = Convert.ToDecimal((double)vManager.duration / 1000);
                numericUpDown3.Value = Convert.ToDecimal(vManager.width);
                numericUpDown4.Value = Convert.ToDecimal(vManager.height);
            }catch(Exception)
            {
                System.Windows.Forms.MessageBox.Show("Form3_Load에서 예외발생");
                Application.ExitThread();
                Environment.Exit(0);
            }
        }

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBDialog = new FolderBrowserDialog();
            if (FBDialog.ShowDialog() == DialogResult.OK)
            {
                outputPath = FBDialog.SelectedPath;
                metroLabel2.Text = outputPath;//폼1에서 입력받은 URL을 받아옴.
            }
        }

        private void MetroButton2_Click(object sender, EventArgs e)
        {
            long start = (long)(Convert.ToDouble(numericUpDown1.Value) * 1000);
            long end = (long)(Convert.ToDouble(numericUpDown2.Value) * 1000);
            int w = Convert.ToInt32(numericUpDown3.Value);
            int h = Convert.ToInt32(numericUpDown4.Value);
            using (vManager)
            {
                // SaveGIF를 이용한 GIF 저장 가능
                GifOption option = new GifOption();
                option.delay = 1000 / 15;
                option.start = start;
                option.end = end;
                option.width = w;
                option.height = h;

                vManager.SaveGif(option, outputPath + '\\' + title + ".gif");
                System.Windows.Forms.MessageBox.Show("생성완료");
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > numericUpDown2.Value)
            {
                numericUpDown1.Value = numericUpDown2.Value;
            }
        }

        private void MetroButton3_Click(object sender, EventArgs e)
        {
            long start = (long)(Convert.ToDouble(numericUpDown1.Value) * 1000);
            long end = (long)(Convert.ToDouble(numericUpDown2.Value) * 1000);
            long delay = 1000 / 15;
            int slice = (int)((end - start));
            Bitmap[] board = new Bitmap[slice];
            using (vManager)
            {

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                numericUpDown3.Value = Convert.ToDecimal(vManager.width);
                numericUpDown4.Value = Convert.ToDecimal(vManager.height);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                numericUpDown3.Value = Convert.ToDecimal(vManager.width / 2);
                numericUpDown4.Value = Convert.ToDecimal(vManager.height / 2);
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                numericUpDown3.Value = Convert.ToDecimal((int)(vManager.width * 0.3));
                numericUpDown4.Value = Convert.ToDecimal((int)(vManager.height * 0.3));
            }
        }
    }
}
