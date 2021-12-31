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

  
      

        private void pictureBox1_Click(object sender, EventArgs e)
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



        private void button2_Click(object sender, EventArgs e)//Reset
        {

            if (ImageMatrix == null)
                ImageOperations.alert(2);
            else
            {
                pictureBox2.Image = null;//remove image from picbox2
                ImageMatrix = OrginalImgMatrix.Clone() as byte[,];
                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);

            }



        }





 

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

            int windowSize = 3;
            int maxSize = 21;
            if (textBox1.Text != "") { windowSize = Convert.ToInt32(textBox1.Text); }
            if (textBox2.Text != "") { maxSize = Convert.ToInt32(textBox2.Text); }

            if (ImageMatrix == null)
                ImageOperations.alert(2);
            else
            {
                    if (radioButton1.Checked)    //alpha + countsort
                    ImageOperations.AlphaFilter(ImageMatrix, windowSize, pictureBox2, "countingSort");
                else if (radioButton2.Checked)//alpha + kthsort
                    ImageOperations.AlphaFilter(ImageMatrix, windowSize, pictureBox2, "kthElementSort");
                else if (radioButton3.Checked) //adaptive + quicksort
                    ImageOperations.AdaptiveFilter(ImageMatrix, windowSize, maxSize, pictureBox2, "countingSort");
                else if (radioButton4.Checked) //adaptive+countsort
                    ImageOperations.AdaptiveFilter(ImageMatrix, windowSize, maxSize, pictureBox2, "quickSort");
            }

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
                ImageOperations.alert(1);
            if (ImageMatrix == null)
                ImageOperations.alert(2);


            if (radioButton5.Checked)    //alpha filter graph
            {
                int windowSize = Convert.ToInt32(textBox1.Text);
                int maxSize = Convert.ToInt32(textBox2.Text);
                // Make up some data points from the N, N log(N) functions
                int N = 50;
                int c = 0;
                double[] x_values = new double[N];
                double[] y_values_count = new double[N];
                double[] y_values_kthELement = new double[N];
                for (int i = windowSize; i <= maxSize; i += 2)
                {
                    Console.WriteLine("counter is : " + c + " windows size is : " + i);
                    x_values[c] = i;
                    int Start = System.Environment.TickCount;
                    ImageOperations.AlphaFilter(ImageMatrix, windowSize, pictureBox2, "countSort");
                    int End = System.Environment.TickCount;
                    double Time = End - Start;
                    Time /= 1000;
                    y_values_count[c] = Time;
                    int Start2 = System.Environment.TickCount;
                    ImageOperations.AlphaFilter(ImageMatrix, windowSize, pictureBox2, "kthElementSort");
                    int End2 = System.Environment.TickCount;
                    double Time2 = End2 - Start2;
                    Time2 /= 1000;
                    y_values_kthELement[c] = Time2;
                    c++;
                }
                //Create a graph and add two curves to it
                ZGraphForm ZGF = new ZGraphForm("The Z-graph of alpha-trim filter", "N", "f(N)");
                ZGF.add_curve("f(N) = counting", x_values, y_values_count, Color.Red);
                ZGF.add_curve("f(N) = selecting kth element", x_values, y_values_kthELement, Color.Blue);
                ZGF.Show();

            }
               
            else if (radioButton6.Checked)//adpative filter graph
            {

                int windowSize = Convert.ToInt32(textBox1.Text);
                int maxSize = Convert.ToInt32(textBox2.Text);
                Console.WriteLine("max size is : " + maxSize);
                // Make up some data points from the N, N log(N) functions
                int N = 50;
                int c = 0;
                double[] x_values = new double[N];
                double[] y_values_count = new double[N];
                double[] y_values_quick = new double[N];
                for (int i = windowSize; i <= maxSize; i += 2)
                {
                    Console.WriteLine("counter is : " + c + " windows size is : " + i);
                    x_values[c] = i;
                    int Start = System.Environment.TickCount;
                    ImageOperations.AdaptiveFilter(ImageMatrix, 3, maxSize, pictureBox2, "countingSort");
                    int End = System.Environment.TickCount;
                    double Time = End - Start;
                    Time /= 1000;
                    y_values_count[c] = Time;
                    // System.Threading.Thread.Sleep(5300);
                    int Start2 = System.Environment.TickCount;
                    ImageOperations.AdaptiveFilter(ImageMatrix, 3, maxSize, pictureBox2, "quickSort");
                    int End2 = System.Environment.TickCount;
                    double Time2 = End2 - Start2;
                    Time2 /= 1000;
                    y_values_quick[c] = Time2;
                    c++;
                }
                //Create a graph and add two curves to it
                ZGraphForm ZGF = new ZGraphForm("The Z-graph of alpha-trim filter", "N", "f(N)");
                ZGF.add_curve("f(N) = countingsort", x_values, y_values_count, Color.Red);
                ZGF.add_curve("f(N) = quicksort", x_values, y_values_quick, Color.Blue);
                ZGF.Show();

            }
               

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}