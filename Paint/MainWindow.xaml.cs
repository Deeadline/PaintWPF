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
using Microsoft.Win32;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum MyShape
        {
            Line,
            Ellipse,
            Rectangle,
            Polyline,
            Polygon
        }

        private enum MyThickness
        {
            Light = 2,
            Normal = 4,
            Bold = 6,
            Bolder = 8
        }

        private const string GifFilter = "Gif image (*.gif)|*.gif";
        private const string JpegFilter = "Jpeg image (*.jpeg)|*.jpeg";
        private const string BitmapFilter = "Bitmap file (*.bmp)|*.bmp";
        private const string PngFilter = "Png image (*.png)|*.png";

        private MyShape selectedShape = MyShape.Line;

        private Point start;
        private Point end;
        private Polyline polyLine = null;
        private Polygon polygon = null;
        private MyThickness selectedThick = MyThickness.Normal;
        private Color selectedColor = Color.FromRgb(0, 0, 0);

        public MainWindow()
        {
            InitializeComponent();
        }

        #region ShapeClick events

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            var x = Shapes.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == Line.Name;
            }

            selectedShape = MyShape.Line;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            var x = Shapes.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == Ellipsis.Name;
            }

            selectedShape = MyShape.Ellipse;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            var x = Shapes.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == Rectangle.Name;
            }

            selectedShape = MyShape.Rectangle;
        }

        private void PolylineButton_Click(object sender, RoutedEventArgs e)
        {
            var x = Shapes.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == Polyline.Name;
            }

            selectedShape = MyShape.Polyline;
        }

        private void PolygonButton_Click(object sender, RoutedEventArgs e)
        {
            var x = Shapes.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == Polyline.Name;
            }

            selectedShape = MyShape.Polygon;
        }

        #endregion

        #region DrawBox MouseEvents

        private void DrawBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(this);
        }

        private void DrawBox_OnMouseMove(object sender, MouseEventArgs e)
        {
            switch (selectedShape)
            {
                case MyShape.Polyline when polyLine == null:
                    return;
                case MyShape.Polyline:
                    polyLine.Points[^1] = e.GetPosition(DrawBox);
                    break;
                case MyShape.Polygon when polygon == null:
                    return;
                case MyShape.Polygon:
                    polygon.Points[^1] = e.GetPosition(DrawBox);
                    break;
                default:
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        end = e.GetPosition(this);
                    }

                    break;
                }
            }
        }

        private void DrawBox_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (selectedShape)
            {
                case MyShape.Line:
                    DrawLine();
                    break;
                case MyShape.Ellipse:
                    DrawEllipse();
                    break;
                case MyShape.Rectangle:
                    DrawRectangle();
                    break;
                case MyShape.Polygon:
                    DrawPolygon(e);
                    break;
                case MyShape.Polyline:
                    DrawPolyLine(e);
                    break;
            }
        }

        #endregion

        #region ShapeDraw

        private void DrawRectangle()
        {
            // kolor zmieni sie z panelu wyboru kolorów
            Rectangle newLine = new Rectangle()
            {
                Stroke = new SolidColorBrush(selectedColor),
                StrokeThickness = (double) selectedThick,
                Height = 10, // w zaleznosci od przyszlego wyboru rozmiaru
                Width = 10 // w zaleznosci od wyboru
            };
            if (end.X >= start.X)
            {
                newLine.SetValue(Canvas.LeftProperty, start.X);
                newLine.Width = end.X - start.X;
            }
            else
            {
                newLine.SetValue(Canvas.LeftProperty, end.X);
                newLine.Width = start.X - end.X;
            }

            if (end.Y >= start.Y)
            {
                newLine.SetValue(Canvas.TopProperty, start.Y - 20);
                newLine.Height = end.Y - start.Y;
            }
            else
            {
                newLine.SetValue(Canvas.TopProperty, end.Y - 20);
                newLine.Height = start.Y - end.Y;
            }

            DrawBox.Children.Add(newLine);
        }

        private void DrawEllipse()
        {
            // kolor zmieni sie z panelu wyboru kolorów
            Ellipse newLine = new Ellipse
            {
                Stroke = new SolidColorBrush(selectedColor),
                StrokeThickness = (double) selectedThick,
                Height = 10, // w zaleznosci od przyszlego wyboru rozmiaru
                Width = 10 // w zaleznosci od wyboru
            };
            if (end.X >= start.X)
            {
                newLine.SetValue(Canvas.LeftProperty, start.X);
                newLine.Width = end.X - start.X;
            }
            else
            {
                newLine.SetValue(Canvas.LeftProperty, end.X);
                newLine.Width = start.X - end.X;
            }

            if (end.Y >= start.Y)
            {
                newLine.SetValue(Canvas.TopProperty, start.Y - 20);
                newLine.Height = end.Y - start.Y;
            }
            else
            {
                newLine.SetValue(Canvas.TopProperty, end.Y - 20);
                newLine.Height = start.Y - end.Y;
            }

            DrawBox.Children.Add(newLine);
        }

        private void DrawPolyLine(MouseButtonEventArgs e)
        {
            if (polyLine != null)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    polyLine.Points.Add(e.GetPosition(DrawBox));
                }
                else
                {
                    polyLine = null;
                }
            }
            else
            {
                polyLine = new Polyline
                {
                    Stroke = new SolidColorBrush(selectedColor),
                    StrokeThickness = (double) selectedThick,
                };
                polyLine.Points.Add(e.GetPosition(DrawBox));
                polyLine.Points.Add(e.GetPosition(DrawBox));
                DrawBox.Children.Add(polyLine);
            }
        }

        private void DrawPolygon(MouseButtonEventArgs e)
        {
            if (polygon != null)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    polygon.Points.Add(e.GetPosition(DrawBox));
                }
                else
                {
                    polygon = null;
                }
            }
            else
            {
                polygon = new Polygon
                {
                    Stroke = new SolidColorBrush(selectedColor),
                    StrokeThickness = (double) selectedThick,
                };
                polygon.Points.Add(e.GetPosition(DrawBox));
                polygon.Points.Add(e.GetPosition(DrawBox));
                DrawBox.Children.Add(polygon);
            }
        }

        private void DrawLine()
        {
            // kolor zmieni sie z panelu wyboru kolorów
            var newLine = new Line
            {
                Stroke = new SolidColorBrush(selectedColor),
                X1 = start.X,
                Y1 = start.Y - 20,
                X2 = end.X,
                Y2 = end.Y - 20,
                StrokeThickness = (double) selectedThick
            };

            DrawBox.Children.Add(newLine);
        }

        #endregion

        #region Menu

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            if (DrawBox.Children.Count > 0)
            {
            }

            DrawBox.Children.Clear();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            new OpenFileDialog().ShowDialog();
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = $"{JpegFilter}|{PngFilter}|{GifFilter}|{BitmapFilter}",
                DefaultExt = "jpeg",
                AddExtension = true
            };
            if (saveDialog.ShowDialog(this).GetValueOrDefault(false))
            {
                SaveCanvas(saveDialog);
            }
        }

        private void SaveCanvas(SaveFileDialog saveDialog)
        {
            if (saveDialog == null) throw new ArgumentNullException(nameof(saveDialog));
            var rtb = new RenderTargetBitmap((int) DrawBox.RenderSize.Width,
                (int) DrawBox.RenderSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(DrawBox);
            var encoder = saveDialog.SafeFileName.Split(".")[1] switch
            {
                "png" => (BitmapEncoder) new PngBitmapEncoder(),
                "jpeg" => new JpegBitmapEncoder(),
                "gif" => new GifBitmapEncoder(),
                "bmp" => new BmpBitmapEncoder(),
                _ => null
            };
            encoder?.Frames.Add(BitmapFrame.Create(rtb));
            var path = saveDialog.FileName;
            using var fs = File.OpenWrite(path);
            encoder?.Save(fs);
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuPrint_Click(object sender, RoutedEventArgs e)
        {
            new PrintDialog().ShowDialog();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuPNGSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = $"{PngFilter}",
                DefaultExt = "png",
                AddExtension = true
            };
            if (saveDialog.ShowDialog(this).GetValueOrDefault(false))
            {
                SaveCanvas(saveDialog);
            }
        }

        private void MenuJPEGSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = $"{JpegFilter}",
                DefaultExt = "jpeg",
                AddExtension = true
            };
            if (saveDialog.ShowDialog(this).GetValueOrDefault(false))
            {
                SaveCanvas(saveDialog);
            }
        }

        private void MenuGifSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = $"{GifFilter}",
                DefaultExt = "gif",
                AddExtension = true
            };
            if (saveDialog.ShowDialog(this).GetValueOrDefault(false))
            {
                SaveCanvas(saveDialog);
            }
        }

        private void MenuBitmapSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = $"{BitmapFilter}",
                DefaultExt = "bmp",
                AddExtension = true
            };
            if (saveDialog.ShowDialog(this).GetValueOrDefault(false))
            {
                SaveCanvas(saveDialog);
            }
        }

        #endregion

        #region Thickness events

        private void NormalThickButton_Click(object sender, RoutedEventArgs e)
        {
            var x = BrushThickness.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == NormalThick.Name;
            }

            selectedThick = MyThickness.Normal;
        }

        private void BoldThickButton_Click(object sender, RoutedEventArgs e)
        {
            var x = BrushThickness.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == BoldThick.Name;
            }

            selectedThick = MyThickness.Bold;
        }

        private void BolderThick_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            var x = BrushThickness.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == BolderThick.Name;
            }

            selectedThick = MyThickness.Bolder;
        }

        private void LightThick_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            var x = BrushThickness.Items.Cast<MenuItem>().ToArray();
            foreach (var menuItem in x)
            {
                menuItem.IsChecked = menuItem.Name == LightThick.Name;
            }

            selectedThick = MyThickness.Light;
        }

        #endregion

        private void ColorPicker_Change(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (ColorPicker.SelectedColor.HasValue)
            {
                selectedColor = ColorPicker.SelectedColor.Value;
            }
        }
    }
}