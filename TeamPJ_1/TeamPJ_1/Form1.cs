using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace TeamPJ_1
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            string url = metroTextBox1.Text;
            Process p = new Process();
            p.StartInfo.FileName = "__main__.exe";
            p.StartInfo.Arguments = "-f bestvideo " + url;
            p.Start();
            p.WaitForExit();
            //파일입출력
            string fileName = getFileName();
            if (Directory.Exists(@".\video") == false)
            {
                Directory.CreateDirectory(@".\video");
            }
            string filePath = ".\\" + fileName;
            string movePath = ".\\video\\" + fileName;
            File.Move(filePath, movePath);
            //파일 이동완료
            Form3 mainForm = new Form3();
            mainForm.Passvalue = fileName;
            mainForm.Show();
        }

        private void MetroButton2_Click(object sender, EventArgs e)
        {
            string fileName;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "파일 찾기";
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Filter = "mp4 File(*.mp4)|*.mp4 |webm File(*.webm)|*.webm";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
                // gif.Image.Save(fileName);
            }
        }

        private void MetroTextBox1_Click(object sender, EventArgs e)
        {

        }

        private string getFileName()
        {
            string path = @".\info.txt";
            string[] textValue = System.IO.File.ReadAllLines(path);
            return textValue[0];
        }
    }

       

}
