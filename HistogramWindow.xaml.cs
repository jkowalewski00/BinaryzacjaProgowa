using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BinaryzacjaProgowa
{
    /// <summary>
    /// Logika interakcji dla klasy HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : Window
    {
        private BitmapImage bitmapImage;

        public HistogramWindow(BitmapImage bitmapImage)
        {
            InitializeComponent();
            this.bitmapImage = bitmapImage;
            DisplayHistogram(bitmapImage);
        }

        private void DisplayHistogram(BitmapImage bitmapImage)
        {
            if(bitmapImage != null)
            {
                // Utworzenie nowego obiektu BitmapSource na podstawie BitmapImage
                FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(bitmapImage, PixelFormats.Rgb24, null, 0);
                byte[] pixels = new byte[convertedBitmap.PixelWidth * convertedBitmap.PixelHeight * 3];
                convertedBitmap.CopyPixels(pixels, convertedBitmap.PixelWidth * 3, 0);

                // Utworzenie tablicy z wartościami histogramu
                int[] histogramValues = new int[256];

                // Obliczenie wartości histogramu dla średniej kanałów RGB
                for (int i = 0; i < pixels.Length; i += 3)
                {
                    int value = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
                    histogramValues[value]++;
                }

                // Utworzenie nowego płótna dla histogramu
                Canvas histogramCanvas = new Canvas();
                histogramCanvas.Width = 300;
                histogramCanvas.Height = 200;

                // Dodanie podziałek na osi Y
                for (int i = 0; i <= 10; i++)
                {
                    double y = histogramCanvas.Height - i * histogramCanvas.Height / 10.0;
                    Line line = new Line();
                    line.X1 = 0;
                    line.X2 = histogramCanvas.Width;
                    line.Y1 = y;
                    line.Y2 = y;
                    line.Stroke = Brushes.LightGray;
                    histogramCanvas.Children.Add(line);
                }

                // Wyznaczenie maksymalnej wartości histogramu
                int maxValue = histogramValues.Max();

                // Rysowanie prostokątów dla każdej wartości histogramu
                for (int i = 0; i < histogramValues.Length; i++)
                {
                    double height = histogramValues[i] * histogramCanvas.Height / (double)maxValue;
                    Rectangle rectangle = new Rectangle();
                    rectangle.Width = histogramCanvas.Width / 256.0;
                    rectangle.Height = height;
                    rectangle.Fill = Brushes.Black;
                    Canvas.SetLeft(rectangle, i * histogramCanvas.Width / 256.0);
                    Canvas.SetTop(rectangle, histogramCanvas.Height - height);
                    histogramCanvas.Children.Add(rectangle);
                }

                // Dodanie etykiet na osi X
                for (int i = 0; i <= 255; i += 32)
                {
                    double x = i * histogramCanvas.Width / 256.0;
                    TextBlock label = new TextBlock();
                    label.Text = i.ToString();
                    label.FontSize = 8;
                    Canvas.SetLeft(label, x);
                    Canvas.SetTop(label, histogramCanvas.Height + 5);
                    histogramCanvas.Children.Add(label);
                }
                HistogramCanvas.Children.Clear();
                HistogramCanvas.Children.Add(histogramCanvas);
            }
        }
    }
}
