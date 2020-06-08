using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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


    }
}
