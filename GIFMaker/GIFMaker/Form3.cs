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

        private void Form3_Load(object sender, EventArgs e)       {
            metroLabel1.Text = title;//폼1에서 입력받은 URL을 받아옴.
            metroLabel2.Text = outputPath;//폼1에서 입력받은 URL을 받아옴.
            //vManager = new VideoManager(filePath);
            //metroTextBox1.Text = (vManager.width / 2).ToString();
            //metroTextBox2.Text = (vManager.height / 2).ToString();
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

        private void MetroDateTime1_ValueChanged(object sender, EventArgs e)
        {


            /*
             *dt1.Hour : 시간 가져오기
             * dt1.Minute : 분 가져오기
             * dt1.Second : 초 가져오기
             */

        }

        //밀리sec 구하는 함수
        int getMilliSec(DateTime dt)
        {
            int MilliSec = (dt.Hour * 3600 + dt.Minute * 60 + dt.Second) * 1000;
            return MilliSec;
        }

        private void MetroButton2_Click(object sender, EventArgs e)
        {
            //DateTime start_dt = metroDateTime1.Value; //시작시간
            //DateTime end_dt = metroDateTime2.Value;   //종료시간

            //int start_time = getMilliSec(start_dt);//시작시간 밀리초로 변환
            //int end_time = getMilliSec(end_dt);//종료시간 밀리초로 변환

            int start_h = Convert.ToInt32(metroTextBox4.Text);
            int start_m = Convert.ToInt32(metroTextBox5.Text);
            int start_s = Convert.ToInt32(metroTextBox6.Text);
            int end_h = Convert.ToInt32(metroTextBox7.Text);
            int end_m = Convert.ToInt32(metroTextBox8.Text);
            int end_s = Convert.ToInt32(metroTextBox9.Text);

            int start_MilliSec = (start_h * 3600 + start_m * 60 + start_s) * 1000;
            int end_MilliSec = (end_h * 3600 + end_m * 60 + end_s) * 1000;

            int x = Convert.ToInt32(metroTextBox1.Text); //가로 길이
            int y = Convert.ToInt32(metroTextBox2.Text); //세로 길이
            //int fps = Convert.ToInt32(metroTextBox3.Text);//프레임 크기

            using (vManager)
            {
                // SaveGIF를 이용한 GIF 저장 가능
                GifOption option = new GifOption();
                option.delay = 1000 / 15;
                option.start = 180 * 1000;
                option.end = 190 * 1000;
                option.width = x;
                option.height = y;

                vManager.SaveGif(option, outputPath + @"\test.gif");

                // Seek 및 NextBitmapFrame을 이용한 비트맵 받아오기 및 영상 재생 가능
            }
        }

       
    }
}
