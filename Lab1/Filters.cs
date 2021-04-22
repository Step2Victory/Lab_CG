using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace Lab1
{
    abstract class Filters
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public virtual Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; ++i)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
    }

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                255 - sourceColor.G,
                255 - sourceColor.B);
            return resultColor;
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter()
        { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; ++l)
                for (int k = -radiusX; k <= radiusX; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; ++i)
                for (int j = 0; j < sizeY; ++j)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; ++i)
                for (int j = -radius; j <= radius; ++j)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; ++i)
                for (int j = 0; j < size; ++j)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }
    class GreyWorld : Filters
    {

        protected float R_mean = 0;
        protected float G_mean = 0;
        protected float B_mean = 0;

       
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color_x_y = sourceImage.GetPixel(x, y);
            float resultR = color_x_y.R * (R_mean + G_mean + B_mean) / (3 * R_mean);
            float resultG = color_x_y.G * (R_mean + G_mean + B_mean) / (3 * G_mean);
            float resultB = color_x_y.B * (R_mean + G_mean + B_mean) / (3 * B_mean);
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; ++i)
                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    Color color_x_y = sourceImage.GetPixel(i, j);
                    R_mean += color_x_y.R;
                    G_mean += color_x_y.G;
                    B_mean += color_x_y.B;
                }
            R_mean /= (float)(sourceImage.Width * sourceImage.Height);
            G_mean /= (float)(sourceImage.Width * sourceImage.Height);
            B_mean /= (float)(sourceImage.Width * sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; ++i)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }


    }

    class LinearCorrection : Filters
    {
        protected float R_max = 0;
        protected float G_max = 0;
        protected float B_max = 0;
        protected float R_min = 255;
        protected float G_min = 255;
        protected float B_min = 255;

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color_x_y = sourceImage.GetPixel(x, y);
            float resultR = (color_x_y.R - R_min) * 255 / (R_max - R_min);
            float resultG = (color_x_y.G - G_min) * 255 / (G_max - G_min);
            float resultB = (color_x_y.B - B_min) * 255 / (B_max - B_min);
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; ++i)
                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    Color color_x_y = sourceImage.GetPixel(i, j);
                    if (color_x_y.R > R_max)
                        R_max = color_x_y.R;
                    if (color_x_y.G > G_max)
                        G_max = color_x_y.G;
                    if (color_x_y.B > B_max)
                        B_max = color_x_y.B;
                    if (color_x_y.R < R_min)
                        R_min = color_x_y.R;
                    if (color_x_y.G < G_min)
                        G_min = color_x_y.G;
                    if (color_x_y.B < B_min)
                        B_min = color_x_y.B;


                }
            for (int i = 0; i < sourceImage.Width; ++i)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

    }

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color_x_y = sourceImage.GetPixel(x, y);
            float intensity = 0.299f * color_x_y.R + 0.587f * color_x_y.G + 0.114f * color_x_y.B;
            float resultR = intensity;
            float resultG = intensity;
            float resultB = intensity;
            
                return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }
    class Sepia : Filters
    {
        float k = 1;
        public Sepia(float k_ = 1)
        {
            k = k_;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color color_x_y = sourceImage.GetPixel(x, y);
            float intensity = 0.299f * color_x_y.R + 0.587f * color_x_y.G + 0.114f * color_x_y.B;
            float resultR = intensity + 2f * k;
            float resultG = intensity + 0.5f * k;
            float resultB = intensity - k;

            return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
        }


    }

    class MedainFilter : Filters
    {
        int radiusX;
        int radiusY;

        public MedainFilter(int radiusX_ = 3, int radiusY_ = 3)
        {
            radiusX = radiusX_;
            radiusY = radiusY_;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            
            float[] arr_R = new float[(2 * radiusX + 1) * (2 * radiusY + 1)];
            float[] arr_G = new float[(2 * radiusX + 1) * (2 * radiusY + 1)];
            float[] arr_B = new float[(2 * radiusX + 1) * (2 * radiusY + 1)];
            
            for (int l = -radiusY; l <= radiusY; ++l)
                for (int k = -radiusX; k <= radiusX; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    
                    arr_R[(l + radiusY) * (2 * radiusX + 1) + k + radiusX] = neighborColor.R;
                    arr_G[(l + radiusY) * (2 * radiusX + 1) + k + radiusX] = neighborColor.G;
                    arr_B[(l + radiusY) * (2 * radiusX + 1) + k + radiusX] = neighborColor.B;
                    
                }
            
            Array.Sort(arr_R, 0, (2 * radiusX + 1) * (2 * radiusY + 1));
            Array.Sort(arr_G, 0, (2 * radiusX + 1) * (2 * radiusY + 1));
            Array.Sort(arr_B, 0, (2 * radiusX + 1) * (2 * radiusY + 1));
            float resultR = arr_R[(2 * radiusX + 1) * (2 * radiusY + 1) / 2];
            float resultG = arr_G[(2 * radiusX + 1) * (2 * radiusY + 1) / 2];
            float resultB = arr_B[(2 * radiusX + 1) * (2 * radiusY + 1) / 2];
            
     
            return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage);
            for (int i = radiusX; i < sourceImage.Width - radiusX; ++i)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = radiusY; j < sourceImage.Height - radiusY; ++j)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

    }
    class EdgeDetection : MatrixFilter
    {
        protected float[,] kernelY = null;
        public void createKernels()
        {
            kernel = new float[3, 3] { {-1, -1, -1 }, {0, 0, 0 }, {1, 1, 1 } };
            kernelY = new float[3, 3] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
        }
        public EdgeDetection()
        {
            createKernels();
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR_x = 0;
            float resultG_x = 0;
            float resultB_x = 0;
            float resultR_y = 0;
            float resultG_y = 0;
            float resultB_y = 0;
            for (int l = -radiusY; l <= radiusY; ++l)
                for (int k = -radiusX; k <= radiusX; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR_x += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG_x += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB_x += neighborColor.B * kernel[k + radiusX, l + radiusY];
                    resultR_y += neighborColor.R * kernelY[k + radiusX, l + radiusY];
                    resultG_y += neighborColor.G * kernelY[k + radiusX, l + radiusY];
                    resultB_y += neighborColor.B * kernelY[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
                Clamp((int)Math.Sqrt(resultR_x * resultR_x + resultR_y * resultR_y), 0, 255),
                Clamp((int)Math.Sqrt(resultG_x * resultG_x + resultG_y * resultG_y), 0, 255),
                Clamp((int)Math.Sqrt(resultB_x * resultB_x + resultB_y * resultB_y), 0, 255)
                );
        }

    }
    class MaximumFilter : Filters
    {
        int radiusX;
        int radiusY;

        public MaximumFilter(int radiusX_ = 3, int radiusY_ = 3)
        {
            radiusX = radiusX_;
            radiusY = radiusY_;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {


            float max_R = 0;
            float max_G = 0;
            float max_B = 0;

            for (int l = -radiusY; l <= radiusY; ++l)
                for (int k = -radiusX; k <= radiusX; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    if (neighborColor.R > max_R)
                        max_R = neighborColor.R;
                    if (neighborColor.G > max_G)
                        max_G = neighborColor.G;
                    if (neighborColor.B > max_B)
                        max_B = neighborColor.B;

                }


            return Color.FromArgb(
            Clamp((int)max_R, 0, 255),
            Clamp((int)max_G, 0, 255),
            Clamp((int)max_B, 0, 255)
            );
        }
    }
    class Embossing : MatrixFilter
    {
        public Embossing()
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; ++l)
                for (int k = -radiusX; k <= radiusX; ++k)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
                Clamp((int)((resultR + 255f) / 2) , 0, 255),
                Clamp((int)((resultG + 255f) / 2), 0, 255),
                Clamp((int)((resultB + 255f) / 2), 0, 255)
                );
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            GrayScaleFilter gray = new GrayScaleFilter();
            Bitmap newSourceImage = gray.proccessImage(sourceImage, worker);
            for (int i = 0; i < newSourceImage.Width; ++i)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < newSourceImage.Height; ++j)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(newSourceImage, i, j));
                }
            }
            return resultImage;
        }
    }
    class LightingEdges : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        { return Color.FromArgb(0, 0, 0); }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            
            MedainFilter median = new MedainFilter(1, 1);
            Bitmap newSourceImage_1 = median.proccessImage(sourceImage, worker);
            EdgeDetection edges = new EdgeDetection();
            Bitmap newSourceImage_2 = edges.proccessImage(newSourceImage_1, worker);
            MaximumFilter max = new MaximumFilter();
            Bitmap resultImage = max.proccessImage(newSourceImage_2, worker);
            return resultImage;
        }
    }
    class MathMorphology : Filters 
    {
        protected bool[,] structureElement = null;
        protected float threshold;
        protected MathMorphology()
        { }
        public MathMorphology(bool[,] structureElement, float threshold = 0.5f)
        {
            this.structureElement = structureElement;
            this.threshold = threshold;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            throw new NotImplementedException();
        }
        protected bool makeBinary(Bitmap sourceImage, int x, int y)
        {
            if (sourceImage.GetPixel(x, y).R <= threshold * 255)
                return true;
            return false;
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            
            GrayScaleFilter gray = new GrayScaleFilter();
            Bitmap sourceImage1 = gray.proccessImage(sourceImage, worker);
            bool[,] binaryImage = new bool[sourceImage.Width, sourceImage.Height];
            Bitmap resultImage = new Bitmap(sourceImage1);
            for (int i = 0; i < sourceImage.Width; ++i)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 50));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    binaryImage[i, j] = makeBinary(sourceImage1, i, j);
                }
            }
            int height = structureElement.GetLength(0) / 2;
            int width = structureElement.GetLength(1) / 2;
            
            for (int i = width; i < sourceImage.Width - width; ++i)
            {
                worker.ReportProgress((int)((float)(resultImage.Width + i) / resultImage.Width * 50));
                if (worker.CancellationPending)
                    return null;
                for (int j = height; j < sourceImage.Height - height; ++j)
                {
                    bool pos = calculateNewPos(binaryImage, i, j);
                    if (pos)
                    {
                        resultImage.SetPixel(i, j, Color.Black);
                    }
                    else
                    {
                        resultImage.SetPixel(i, j, Color.White);
                    }
                }
            }
            return resultImage;
        }
        virtual protected bool calculateNewPos(bool[,] binaryImage, int x, int y)
        {
            return false;
        }

    }


    class Dilation : MathMorphology
    {
        public Dilation(bool[,] structureElement, float threshold = 0.5f)
        {
            this.structureElement = structureElement;
            this.threshold = threshold;
        }
        protected override bool calculateNewPos(bool[,] binaryImage, int x, int y)
        {
            int radiusX = structureElement.GetLength(0) / 2;
            int radiusY = structureElement.GetLength(1) / 2;
            
            for (int l = -radiusY; l <= radiusY; ++l)
                for (int k = -radiusX; k <= radiusX; ++k)
                {
                    int idX = x + k;
                    int idY = y + l;
                    if (binaryImage[idX, idY] && structureElement[k + radiusX, l + radiusY])
                        return true;
                }
            return false;
        }
    }

    class Erosion : MathMorphology
    {
        public Erosion(bool[,] structureElement, float threshold = 0.5f)
        {
            this.structureElement = structureElement;
            this.threshold = threshold;
        }
        protected override bool calculateNewPos(bool[,] binaryImage, int x, int y)
        {
            int radiusX = structureElement.GetLength(0) / 2;
            int radiusY = structureElement.GetLength(1) / 2;

            for (int l = -radiusY; l <= radiusY; ++l)
                for (int k = -radiusX; k <= radiusX; ++k)
                {
                    int idX = x + k;
                    int idY = y + l;
                    if (!(binaryImage[idX, idY] && structureElement[k + radiusX, l + radiusY]))
                        return false;
                }
            return true;
        }
    }

    class Opening : MathMorphology
    {
        public Opening(bool[,] structureElement, float threshold = 0.5f)
        {
            this.structureElement = structureElement;
            this.threshold = threshold;
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Erosion er = new Erosion(structureElement, threshold);
            Dilation dil = new Dilation(structureElement, threshold);
            Bitmap img = dil.proccessImage(sourceImage, worker);
            return er.proccessImage(img, worker);

        }
    }
    class Closing : MathMorphology
    {
        public Closing(bool[,] structureElement, float threshold = 0.5f)
        {
            this.structureElement = structureElement;
            this.threshold = threshold;
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Erosion er = new Erosion(structureElement, threshold);
            Dilation dil = new Dilation(structureElement, threshold);
            Bitmap img = er.proccessImage(sourceImage, worker);
            return dil.proccessImage(img, worker);

        }
    }
    class Grad : MathMorphology
    {
        public Grad(bool[,] structureElement, float threshold = 0.5f)
        {
            this.structureElement = structureElement;
            this.threshold = threshold;
        }
        public override Bitmap proccessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Erosion er = new Erosion(structureElement, threshold);
            Dilation dil = new Dilation(structureElement, threshold);
            Bitmap img1 = er.proccessImage(sourceImage, worker);
            Bitmap img2 = dil.proccessImage(sourceImage, worker);
            Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; ++i)
            {
                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    Color c1 = img1.GetPixel(i, j);
                    Color c2 = img2.GetPixel(i, j);
                    Color c = Color.FromArgb(Clamp(c1.R - c2.R, 0, 255), Clamp(c1.G - c2.G, 0, 255), Clamp(c1.B - c2.B, 0, 255));
                    result.SetPixel(i, j, c);
                }
            }
            return result;

        }
    }
}

