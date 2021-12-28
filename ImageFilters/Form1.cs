using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZGraphTools;

namespace ImageFilters
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        byte[,] ImageMatrix;
        byte[,] OrginalImgMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                OrginalImgMatrix = ImageMatrix.Clone() as byte[,]; //save it to reset

            }
        }

        private void btnZGraph_Click(object sender, EventArgs e)
        {
            // Make up some data points from the N, N log(N) functions
            int N = 40;
            double[] x_values = new double[N];
            double[] y_values_N = new double[N];
            double[] y_values_NLogN = new double[N];

            for (int i = 0; i < N; i++)
            {
                x_values[i] = i;
                y_values_N[i] = i;
                y_values_NLogN[i] = i * Math.Log(i);
            }

            //Create a graph and add two curves to it
             ZGraphForm ZGF = new ZGraphForm("Sample Graph", "N", "f(N)");
            ZGF.add_curve("f(N) = N", x_values, y_values_N,Color.Red);
            ZGF.add_curve("f(N) = N Log(N)", x_values, y_values_NLogN, Color.Blue);
            ZGF.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)//AlphaCountSort
        {
            int windowSize = 3;
         
            if (textBox1.Text != "") { windowSize = Convert.ToInt32(textBox1.Text); }
            Console.WriteLine(windowSize);
            ImageOperations.AlphaFilter(ImageMatrix, windowSize, pictureBox2, "countingSort");


        }

        private void button2_Click(object sender, EventArgs e)//Reset
        {
            pictureBox2.Image = null;//remove image from picbox2
            ImageMatrix = OrginalImgMatrix.Clone() as byte[,];
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);


        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            int windowSize = 3;
            int maxSize = 9;
            if (textBox2.Text != "") { maxSize = Convert.ToInt32(textBox2.Text); }
            Console.WriteLine(windowSize);
            ImageOperations.AdaptiveFilter(ImageMatrix, windowSize, maxSize, pictureBox2, "countingSort");
        }
    }
}