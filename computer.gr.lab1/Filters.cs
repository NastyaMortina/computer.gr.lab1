using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace computer.gr.lab1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
        public int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    class InvertFilter : Filters //Инверсия
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }
    class MatrixFilter : Filters //Матричные фильтры
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
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
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }

    }
    class BlurFilter : MatrixFilter //Размытие
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }

    class GaussianFilter : MatrixFilter //Фильтр Гаусса
    {
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }

        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
    }

    class GrayScaleFilter : Filters //Черно-белое
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceIntensity = sourceImage.GetPixel(x, y);
            int intensity = (int)((sourceIntensity.R * 0.36) + (sourceIntensity.G * 0.51) + (sourceIntensity.B * 0.11));
            Color resultIntencity = Color.FromArgb(intensity, intensity, intensity);
            return resultIntencity;
        }
    }

    class SepiaFilter : Filters //Сепия
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceIntensity = sourceImage.GetPixel(x, y);
            int intensity = (int)((sourceIntensity.R * 0.36) + (sourceIntensity.G * 0.51) + (sourceIntensity.B * 0.11));
            Color resultIntencity = Color.FromArgb(Clamp(intensity + 2 * 20, 0, 255), Clamp(intensity + (int)0.5 * 20, 0, 255), Clamp(intensity - 1 * 20, 0, 255));
            return resultIntencity;
        }
    }

    class BrightnessFilter : Filters //Изменение яркости
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceIntensity = sourceImage.GetPixel(x, y);
            Color resultIntencity = Color.FromArgb(Clamp(sourceIntensity.R + 40, 0, 255), Clamp(sourceIntensity.G + 40, 0, 255), Clamp(sourceIntensity.B + 40, 0, 255));
            return resultIntencity;
        }
    }

    class SharpnessFilter : MatrixFilter //Резкость
    {
        public SharpnessFilter()
        {
            kernel = new float[,]
            {
                {-1,-1,-1},{-1,9,-1},{-1,-1,-1}
            };
        }
    }
    
    class EmbossFilter : MatrixFilter //Тиснение
    {
        public EmbossFilter()
        {
            kernel = new float[3, 3]
               {
                   {  0,  -1,  0 },
                   { -1,   0,  1 },
                   {  0,   1,  0 }
               };
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 128;
            float resultG = 128;
            float resultB = 128;

            for (int l = -radiusX; l <= radiusX; l++)
            {
                for (int k = -radiusY; k <= radiusY; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighbourColor = sourceImage.GetPixel(idX, idY);

                    resultR += neighbourColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighbourColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighbourColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }
    class GlassFilet : Filters //Эффект "стекла"
    {
        protected Random rand = new Random();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newX = x + (int)((rand.NextDouble() - 0.5) * 10);
            int newY = y + (int)((rand.NextDouble() - 0.5) * 10);
            // .NextDouble - возвращает случайное число типа double, которое больше или равно 0.0 и меньше 1.0

            newX = Clamp(newX, 0, sourceImage.Width - 1);
            newY = Clamp(newY, 0, sourceImage.Height - 1);

            return sourceImage.GetPixel(newX, newY);
        }
    }

    class MoveFilter : Filters //Перенос
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (x + 50 < sourceImage.Width)
            {
                return sourceImage.GetPixel(x + 50, y);
            }
            else { return Color.FromArgb(0, 0, 0); }
        }
    }

    abstract class DoubleMatrixFilters : Filters 
    {
        protected float[,] kernel1 = null;
        protected float[,] kernel2 = null;
        protected DoubleMatrixFilters() { }
        public DoubleMatrixFilters(float[,] kernel1, float[,] kernel2)
        {
            this.kernel1 = kernel1;
            this.kernel2 = kernel2;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel1.GetLength(0) / 2;
            int radiusY = kernel1.GetLength(1) / 2;

            float resultR1 = 0;
            float resultG1 = 0;
            float resultB1 = 0;

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    Color neighbor = sourceImage.GetPixel(idX, idY);

                    resultR1 += neighbor.R * kernel1[k + radiusX, i + radiusY];
                    resultG1 += neighbor.G * kernel1[k + radiusX, i + radiusY];
                    resultB1 += neighbor.B * kernel1[k + radiusX, i + radiusY];
                }
            }

            int radiusX2 = kernel2.GetLength(0) / 2;
            int radiusY2 = kernel2.GetLength(1) / 2;

            float resultR2 = 0;
            float resultG2 = 0;
            float resultB2 = 0;

            for (int i = -radiusY2; i <= radiusY2; i++)
            {
                for (int k = -radiusX2; k <= radiusX2; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    Color neighbor = sourceImage.GetPixel(idX, idY);

                    resultR2 += neighbor.R * kernel2[k + radiusX2, i + radiusY2];
                    resultG2 += neighbor.G * kernel2[k + radiusX2, i + radiusY2];
                    resultB2 += neighbor.B * kernel2[k + radiusX2, i + radiusY2];
                }
            }

            return Color.FromArgb(
            Clamp((int)Math.Sqrt(resultR1 * resultR1 + resultR2 * resultR2), 0, 255),
            Clamp((int)Math.Sqrt(resultG1 * resultG1 + resultG2 * resultG2), 0, 255),
            Clamp((int)Math.Sqrt(resultB1 * resultB1 + resultB2 * resultB2), 0, 255));
        }
    }
    class SobelFilter : DoubleMatrixFilters //Собель
    {
        public SobelFilter()
        {
            kernel1 = new float[3, 3]
               {
                   { -1,  0,  1 },
                   { -2,  0,  2 },
                   { -1,  0,  1 }
               };
            kernel2 = new float[3, 3]
               {
                   { -1, -2, -1 },
                   {  0,  0,  0 },
                   {  1,  2,  1 }
               };
        }
    }

    class SharraFilter : DoubleMatrixFilters //оператор Щарра
    {
        public SharraFilter()
        {
            kernel1 = new float[3, 3]
               {
                   { 3,  0, -3  },
                   { 10, 0, -10 },
                   { 3,  0, -3  }
               };

            kernel2 = new float[3, 3]
               {
                   {  3,  10,  3 },
                   {  0,   0,  0 },
                   { -3, -10, -3 }
               };
        }
    }

    class PruittaFilter : DoubleMatrixFilters //оператор Прюитта
    {
        public PruittaFilter()
        {
            kernel1 = new float[3, 3]
               {
                   { -1,  0, 1 },
                   { -1,  0, 1 },
                   { -1,  0, 1 }
               };

            kernel2 = new float[3, 3]
               {
                   { -1, -1, -1 },
                   {  0,  0,  0 },
                   {  1,  1,  1 }
               };
        }
    }

    class MedianFilter : Filters //Медианный фильтр
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            List<int> AllR = new List<int>();
            List<int> AllG = new List<int>();
            List<int> AllB = new List<int>();

            int radiusX = 1;
            int radiusY = 1;

            for (int k = -radiusX; k <= radiusX; k++)
            {
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color color = sourceImage.GetPixel(idX, idY);

                    AllR.Add(color.R);
                    AllG.Add(color.G);
                    AllB.Add(color.B);
                }
            }

            AllR.Sort();
            AllG.Sort();
            AllB.Sort();

            return Color.FromArgb(AllR[AllR.Count() / 2], AllG[AllG.Count() / 2], AllB[AllB.Count() / 2]);

        }
    }

    class GrayWorldFilter : Filters //Серый мир  
    {
        float R = 0f;
        float G = 0f;
        float B = 0f;

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color resultColor = sourceImage.GetPixel(x, y);
            resultColor = Color.FromArgb(Clamp((int)(resultColor.R * R), 0, 255),
                                           Clamp((int)(resultColor.G * G), 0, 255),
                                           Clamp((int)(resultColor.B * B), 0, 255));
            return resultColor;
        }
   
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            GetCoeff(sourceImage);
            for (int x = 0; x < sourceImage.Width; x++)
            {
                worker.ReportProgress((int)((float)x / resultImage.Width * 100));
                if (worker.CancellationPending) return null;
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    resultImage.SetPixel(x, y, calculateNewPixelColor(sourceImage, x, y));
                }
            }
            return resultImage;
        }

        protected void GetCoeff(Bitmap sourceImage)
        {
            Color colorPixel;
            int sizeOfImage = sourceImage.Height * sourceImage.Width;
            for (int idX = 0; idX < sourceImage.Width; idX++)
                for (int idY = 0; idY < sourceImage.Height; idY++)
                {
                    colorPixel = sourceImage.GetPixel(idX, idY);
                    R += colorPixel.R;
                    G += colorPixel.G;
                    B += colorPixel.B;
                }
            R /= sizeOfImage;
            G /= sizeOfImage;
            B /= sizeOfImage;
            float avg = (R + G + B) / 3f;
            R = avg / R;
            G = avg / G;
            B = avg / B;
        }
    }

    class HistogramStretchFilter : Filters //Линейное растяжение гистограммы
    {
        protected int maxR = 0;
        protected int maxG = 0;
        protected int maxB = 0;

        protected int minR = 255;
        protected int minG = 255;
        protected int minB = 255;

        public void GetMaxColor(Bitmap sourceImage)
        {
            maxR = 0;
            maxG = 0;
            maxB = 0;
      
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color color = sourceImage.GetPixel(i, j);

                    maxR = Math.Max(maxR, color.R);
                    maxB = Math.Max(maxB, color.B);
                    maxG = Math.Max(maxG, color.G);
                }
            }
        }

        public void GetMinColor(Bitmap sourceImage)
        {
            minR = 255;
            minG = 255;
            minB = 255;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color color = sourceImage.GetPixel(i, j);

                    minR = Math.Min(minR, color.R);
                    minB = Math.Min(minB, color.B);
                    minG = Math.Min(minG, color.G);
                }
            }
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color pixel = sourceImage.GetPixel(x, y);

            float newR = (pixel.R - minR) * 255 / (maxR - minR);
            float newG = (pixel.G - minG) * 255 / (maxG - minG);
            float newB = (pixel.B - minB) * 255 / (maxB - minB);

            return Color.FromArgb(Clamp((int)newR, 0, 255),
                                  Clamp((int)newG, 0, 255),
                                  Clamp((int)newB, 0, 255));
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            GetMinColor(sourceImage);
            GetMaxColor(sourceImage);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
    } 
}