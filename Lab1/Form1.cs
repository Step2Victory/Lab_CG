using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        public Form1()
        {
            InitializeComponent();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void точечныеToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.png;*.jpg;*.bmp;*.jpeg|All files(*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }

        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
            /*Bitmap resultImage = filter.proccessImage(image, backgroundWorker1);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();*/
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).proccessImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
                image = newImage;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void гауссовФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GreyWorld();
            backgroundWorker1.RunWorkerAsync(filter);
            
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new LinearCorrection();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void grayScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sepia(20);
            backgroundWorker1.RunWorkerAsync(filter);
        }


        

        private void медианныйФильтрToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Filters filter = new MedainFilter(1, 1);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрМаксимумаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MaximumFilter(1, 1);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Embossing();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void светящиесяКраяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new LightingEdges();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void выделениеГраницToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Filters filter = new EdgeDetection();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool[,] structureElement = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };
            Filters filter = new Dilation(structureElement, 0.6f);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сужениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool[,] structureElement = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };
            Filters filter = new Erosion(structureElement, 0.6f);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void градиентToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool[,] structureElement = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };
            Filters filter = new Grad(structureElement, 0.6f);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void открытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool[,] structureElement = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };
            Filters filter = new Opening(structureElement, 0.6f);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void закрытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool[,] structureElement = new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } };
            Filters filter = new Closing(structureElement, 0.6f);
            backgroundWorker1.RunWorkerAsync(filter);
        }
    }
}
