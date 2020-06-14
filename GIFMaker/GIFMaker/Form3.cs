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
        private string Form3_value;
        public string Passvalue
        {
            get { return this.Form3_value; }
            set { this.Form3_value = value; }
        }// 다른폼(Form1)에서 전달받은 값을 쓰기

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            metroLabel1.Text = Passvalue;//폼1에서 입력받은 URL을 받아옴.
        }

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            string fileName;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "저장경로 지정하세요";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "JPEG File(*.jpg)|*.jpg |Bitmap File(*.bmp)|*.bmp |PNG File(*.png)|*.png";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
                // gif.Image.Save(fileName);
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
            int MilliSec = (dt.Hour * 3600 + dt.Minute + dt.Second) * 1000;
            return MilliSec;
        }

        private void MetroButton2_Click(object sender, EventArgs e)
        {
            DateTime start_dt = metroDateTime1.Value; //시작시간
            DateTime end_dt = metroDateTime2.Value;   //종료시간

            int start_time = getMilliSec(start_dt);//시작시간 밀리초로 변환
            int end_time = getMilliSec(end_dt);//종료시간 밀리초로 변환

            int x = Convert.ToInt32(metroTextBox1.Text); //가로 길이
            int y = Convert.ToInt32(metroTextBox2.Text); //세로 길이
            int fps = Convert.ToInt32(metroTextBox3.Text);//프레임 크기
        }
    }
}
