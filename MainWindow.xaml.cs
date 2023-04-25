using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinaryzacjaProgowa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        public BitmapImage Original;
        public BitmapImage Converted;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void OpenPhoto(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a picture";
            ofd.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";
            if(ofd.ShowDialog() == true)
            {
                Original = new BitmapImage(new Uri(ofd.FileName));
                Converted = new BitmapImage(new Uri(ofd.FileName));
                Image.Source = Converted;
            }
        }

        public void SavePicture(object sender, RoutedEventArgs e)
        {
            if (Converted == null)
                return;
            Converted = (BitmapImage)Image.Source;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save image as";
            sfd.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (sfd.ShowDialog() == true)
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Converted));
                using(Stream strm = File.Create(sfd.FileName))
                {
                    encoder.Save(strm);
                }
            }
        }

        public void OriginalImage_ResetValue(object sender, RoutedEventArgs e)
        {
            Image.Source = Original;
        }

        private void ThresholdImage()
        {
            if (Converted != null)
            {
                // Pobierz obraz z kontrolki Image jako BitmapImage
                BitmapImage bitmapImage = (BitmapImage)Converted;

                // Utwórz nowy obiekt BitmapImage na podstawie oryginalnego
                BitmapImage thresholdedBitmapImage = new BitmapImage();

                // Pobierz wartość progu z suwaka
                int threshold = (int)Threshold.Value;

                // Ustawienie odpowiednich właściwości obiektu BitmapImage
                thresholdedBitmapImage.BeginInit();
                thresholdedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                thresholdedBitmapImage.CreateOptions = BitmapCreateOptions.None;
                thresholdedBitmapImage.UriSource = bitmapImage.UriSource;
                thresholdedBitmapImage.EndInit();

                // Przetworzenie pikseli obrazu
                FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(thresholdedBitmapImage, PixelFormats.Gray8, null, 0);
                byte[] pixels = new byte[convertedBitmap.PixelWidth * convertedBitmap.PixelHeight];
                convertedBitmap.CopyPixels(pixels, convertedBitmap.PixelWidth, 0);

                // Iteracja przez piksele i binaryzacja
                for (int i = 0; i < pixels.Length; i++)
                {
                    byte binary = pixels[i] > threshold ? (byte)255 : (byte)0;
                    pixels[i] = binary;
                }

                // Utworzenie nowego obiektu BitmapImage na podstawie zbinaryzowanych pikseli
                BitmapSource thresholdedBitmapSource = BitmapSource.Create(convertedBitmap.PixelWidth, convertedBitmap.PixelHeight, convertedBitmap.DpiX, convertedBitmap.DpiY, PixelFormats.Gray8, null, pixels, convertedBitmap.PixelWidth);
                thresholdedBitmapImage = new BitmapImage();
                using (MemoryStream stream = new MemoryStream())
                {
                    // Konwersja z BitmapSource na BitmapImage
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(thresholdedBitmapSource));
                    encoder.Save(stream);
                    thresholdedBitmapImage.BeginInit();
                    thresholdedBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    thresholdedBitmapImage.StreamSource = stream;
                    thresholdedBitmapImage.EndInit();
                }

                // Ustawienie obiektu BitmapImage jako źródło kontrolki Image
                Image.Source = thresholdedBitmapImage;
            }
        }

        private void BinarizeImage()
        {
            if(Converted != null)
            {
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(Converted, PixelFormats.Rgb24, null, 0);
                int stride = formatConvertedBitmap.PixelWidth * 3;
                int size = formatConvertedBitmap.PixelHeight * stride;
                byte[] pixels = new byte[size];
                formatConvertedBitmap.CopyPixels(pixels, stride, 0);

                int redThreshold = (int)Red.Value;
                int greenThreshold = (int)Green.Value;
                int blueThreshold = (int)Blue.Value;

                for (int i = 0; i < pixels.Length; i += 3)
                {
                    pixels[i] = (pixels[i] > redThreshold) ? (byte)255 : (byte)0; // red channel
                    pixels[i + 1] = (pixels[i + 1] > greenThreshold) ? (byte)255 : (byte)0; // green channel
                    pixels[i + 2] = (pixels[i + 2] > blueThreshold) ? (byte)255 : (byte)0; // blue channel
                }

                BitmapSource bitmapSource = BitmapSource.Create(formatConvertedBitmap.PixelWidth, formatConvertedBitmap.PixelHeight, formatConvertedBitmap.DpiX, formatConvertedBitmap.DpiY, PixelFormats.Rgb24, null, pixels, stride);
                BitmapImage binarizedBitmap = new BitmapImage();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);
                    binarizedBitmap.BeginInit();
                    binarizedBitmap.StreamSource = new MemoryStream(memoryStream.ToArray());
                    binarizedBitmap.EndInit();
                }
                Image.Source = binarizedBitmap;
            }
        }


        private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ThresholdValue.Content = "Threshold value: " + Threshold.Value;
            ThresholdImage();
        }

        private void RedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RedValue.Content = "Red: " + Red.Value;
            BinarizeImage();
        }

        private void GreenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GreenValue.Content = "Green: " + (int)Green.Value;
            BinarizeImage();
        }

        private void BlueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BlueValue.Content = "Blue: " + Blue.Value;
            BinarizeImage();
        }

        private void DisplayHistogram(object sender, RoutedEventArgs e)
        {
            Converted = (BitmapImage)Image.Source;
            HistogramWindow histogramWindow = new HistogramWindow(Converted) ;
            histogramWindow.ShowDialog();
        }

    }
}
