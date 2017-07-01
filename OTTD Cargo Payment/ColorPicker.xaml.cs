using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using OxyPlot;

using static System.Math;

using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace OTTD_Cargo_Payment {
    /// <summary>
    /// Interaction logic for WPFColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window {
        public OxyColor SelectedColor;

        public ColorPicker(OxyColor selectedColor) {
            InitializeComponent();

            SelectedColor = selectedColor;

            ColorImage.Source = new WriteableBitmap(400, 400, 96, 96, PixelFormats.Bgra32, null);
            HueImage.Source = new WriteableBitmap(30, 400, 96, 96, PixelFormats.Bgra32, null);
            RedImage.Source = new WriteableBitmap(110, 10, 96, 96, PixelFormats.Bgra32, null);
            GreenImage.Source = new WriteableBitmap(110, 10, 96, 96, PixelFormats.Bgra32, null);
            BlueImage.Source = new WriteableBitmap(110, 10, 96, 96, PixelFormats.Bgra32, null);
            AlphImage.Source = new WriteableBitmap(110, 10, 96, 96, PixelFormats.Bgra32, null);


            RedrawColorImages();
            DrawHue();
            DrawCross();
        }

        private void DrawTransparencyGrid() {
            var alt = false;
            var renderBmp = new RenderTargetBitmap(
                (int)Floor(ColorImageGrid.ActualWidth / 20) * 20 + 20,
                (int)Floor(ColorImageGrid.ActualHeight / 20) * 20 + 20,
                96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen()) {
                for (var y = 0; y < renderBmp.PixelHeight; y += 20) {
                    for (var x = 0; x < renderBmp.PixelWidth; x += 20) {
                        context.DrawRectangle(new SolidColorBrush(alt ? Colors.White : Colors.Gray), null, new Rect(x, y, 20, 20));
                        alt = !alt;
                    }
                    //alt = !alt;
                }
            }
            renderBmp.Render(drawingVisual);
            var brush = new ImageBrush(renderBmp) { Stretch = Stretch.None };
            ColorImageGrid.Background = brush;
            SelectedColorGrid.Background = brush;
        }

        private void UpdateSelectedColor(bool withTextBoxes = true) {
            var hsv = SelectedColor.ToHsv();
            ColorCross.Margin = new Thickness(hsv[1] * ColorImage.ActualWidth - 5, (1 - hsv[2]) * ColorImage.ActualHeight - 5, 0, 0);
            SelectedColorLabel.Background = new SolidColorBrush(new Color {
                ScA = SelectedColor.A / 255f,
                ScR = SelectedColor.R / 255f,
                ScG = SelectedColor.G / 255f,
                ScB = SelectedColor.B / 255f
            });

            if (withTextBoxes) {
                RedText.TextChanged -= TextsChanged;
                GreenText.TextChanged -= TextsChanged;
                BlueText.TextChanged -= TextsChanged;
                AlphaText.TextChanged -= TextsChanged;
                RedText.Text = SelectedColor.R.ToString();
                GreenText.Text = SelectedColor.G.ToString();
                BlueText.Text = SelectedColor.B.ToString();
                AlphaText.Text = SelectedColor.A.ToString();
                RedText.TextChanged += TextsChanged;
                GreenText.TextChanged += TextsChanged;
                BlueText.TextChanged += TextsChanged;
                AlphaText.TextChanged += TextsChanged;
            }

            HueMarkerImage.Margin = new Thickness(2, 400.0 * hsv[0] - HueMarkerImage.ActualHeight / 2, 2, 0);
        }

        private void DrawCross() {
            var drawingGroup = new DrawingGroup();
            using (var context = drawingGroup.Open()) {
                const double radius = 5;
                context.DrawEllipse(null, new Pen(Brushes.White, 3), new Point(radius, radius), radius, radius);
                context.DrawEllipse(null, new Pen(Brushes.Black, 1), new Point(radius, radius), radius, radius);
            }
            ColorCross.Source = new DrawingImage(drawingGroup);
        }

        private void RedrawColorImages() {
            var hsv = SelectedColor.ToHsv();
            //hsv[0] = 1;

            //var col = new HSVColor(SelectedColor.Hue, SelectedColor.Saturation, SelectedColor.Value, SelectedColor.A);
            var bmp = (WriteableBitmap)ColorImage.Source;
            var pix = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];

            for (var y = 0; y < bmp.PixelHeight; y++) {
                hsv[2] = 1.0 - y / (double)bmp.PixelHeight;
                for (var x = 0; x < bmp.PixelWidth; x++) {
                    hsv[1] = x / (double)bmp.PixelWidth;
                    var col = OxyColor.FromHsv(hsv);

                    var offset = x * 4 + y * bmp.PixelWidth * 4;
                    pix[offset++] = col.B;
                    pix[offset++] = col.G;
                    pix[offset++] = col.R;
                    pix[offset] = col.A;
                }
            }

            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pix, 4 * bmp.PixelWidth, 0);

            RedrawColorSliders();
        }

        private void RedrawColorSliders() {
            ReddrawColorSlider(RedImage);
            ReddrawColorSlider(GreenImage);
            ReddrawColorSlider(BlueImage);
            ReddrawColorSlider(AlphImage);
        }

        private void ReddrawColorSlider(Image slider) {
            //var col = new HSVColor(SelectedColor.Hue, SelectedColor.Saturation, SelectedColor.Value, SelectedColor.A);
            var col = new[] {
                          SelectedColor.A,
                          SelectedColor.R,
                          SelectedColor.G,
                          SelectedColor.B,
                      };
            var bmp = (WriteableBitmap)slider.Source;
            var pix = new Byte[bmp.PixelWidth * bmp.PixelHeight * 4];

            for (var x = 0; x < bmp.PixelWidth; x++) {
                if (slider.Equals(AlphImage))
                    col[0] = (byte)(x / (double)bmp.PixelWidth * 255);
                if (slider.Equals(RedImage))
                    col[1] = (byte)(x / (double)bmp.PixelWidth * 255);
                if (slider.Equals(GreenImage))
                    col[2] = (byte)(x / (double)bmp.PixelWidth * 255);
                if (slider.Equals(BlueImage))
                    col[3] = (byte)(x / (double)bmp.PixelWidth * 255);
                for (var y = 0; y < bmp.PixelHeight; y++) {
                    var offset = x * 4 + y * bmp.PixelWidth * 4;
                    pix[offset++] = col[3];
                    pix[offset++] = col[2];
                    pix[offset++] = col[1];
                    pix[offset] = col[0];
                }
            }

            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pix, 4 * bmp.PixelWidth, 0);
        }

        private void DrawHue() {
            //var col = new HSVColor();
            var bmp = (WriteableBitmap)HueImage.Source;
            var pix = new Byte[bmp.PixelWidth * bmp.PixelHeight * 4];
            var hueSteps = 360.0 / bmp.PixelHeight;
            
            //col.Hue = 0.0;
            var hue = 0d;

            for (var y = 0; y < bmp.PixelHeight; y++) {
                hue += hueSteps;
                //col.Hue += hueSteps;
                var col = OxyColor.FromHsv((double)y / bmp.PixelHeight, 1, 1);
                for (var x = 0; x < bmp.PixelWidth; x++) {
                    var offset = x * 4 + y * bmp.PixelWidth * 4;
                    pix[offset++] = col.B;
                    pix[offset++] = col.G;
                    pix[offset++] = col.R;
                    pix[offset] = col.A;
                }
            }

            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pix, 4 * bmp.PixelWidth, 0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            UpdateSelectedColor();
            DrawTransparencyGrid();
        }

        private void ColorImage_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed) {
                Mouse.Capture(null);
                return;
            }

            Mouse.Capture(ColorImage);

            var hsv = SelectedColor.ToHsv();
            var pos = Mouse.GetPosition(ColorImage);
            pos.X = Max(0, Min(ColorImage.ActualWidth, pos.X));
            pos.Y = Max(0, Min(ColorImage.ActualHeight, pos.Y));
            hsv[1] = pos.X / ColorImage.ActualWidth;
            hsv[2] = 1 - pos.Y / ColorImage.ActualHeight;
            //SelectedColor.Saturation = pos.X / ColorImage.ActualWidth;
            //SelectedColor.Brightness = 1 - pos.Y / ColorImage.ActualHeight;
            SelectedColor = OxyColor.FromHsv(hsv);
            UpdateSelectedColor();
            RedrawColorSliders();
        }

        private void HueImage_OnMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed) {
                Mouse.Capture(null);
                return;
            }

            HueImage.CaptureMouse();

            var pos = Mouse.GetPosition(HueImage);
            var hsv = SelectedColor.ToHsv();
            hsv[0] = Max(0, Min(1, pos.Y / HueImage.ActualHeight));
            SelectedColor = OxyColor.FromHsv(hsv);
            UpdateSelectedColor();
            RedrawColorImages();
        }

        private void TextsChanged(object sender, TextChangedEventArgs e) {
            var res = 0.0;
            var box = sender as TextBox;
            if (box == null)
                return;

            if (!double.TryParse(box.Text, out res) || res < 0 || res > 1) {
                box.Background = Brushes.LightCoral;
                return;
            }
            box.Background = Brushes.White;

            var argb = new[] {
                           SelectedColor.A,
                           SelectedColor.R,
                           SelectedColor.G,
                           SelectedColor.B,
                       };
            if (Equals(box, AlphaText))
                argb[0] = (byte)(res * 255);
            if (Equals(box, RedText))
                argb[1] = (byte)(res * 255);
            if (Equals(box, GreenText))
                argb[2] = (byte)(res * 255);
            if (Equals(box, BlueText))
                argb[3] = (byte)(res * 255);
            SelectedColor = OxyColor.FromArgb(argb[0], argb[1], argb[2], argb[3]);

            UpdateSelectedColor(false);
            RedrawColorImages();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ColorSliders_OnMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed) {
                Mouse.Capture(null);
                return;
            }

            var slider = sender as Image;
            if (slider == null)
                return;

            var value = Max(0, Min(1, Mouse.GetPosition(slider).X / slider.ActualWidth));
            Mouse.Capture(slider);

            if (slider.Equals(RedImage))
                RedText.Text = value.ToString(CultureInfo.InvariantCulture);
            if (slider.Equals(GreenImage))
                GreenText.Text = value.ToString(CultureInfo.InvariantCulture);
            if (slider.Equals(BlueImage))
                BlueText.Text = value.ToString(CultureInfo.InvariantCulture);
            if (slider.Equals(AlphImage))
                AlphaText.Text = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
