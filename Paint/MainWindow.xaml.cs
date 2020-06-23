using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private enum DrawMode
        {
            Pencil,
            Erase,
            Fill,
            Shapes
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
        private DrawMode selectedDrawMode = DrawMode.Pencil;

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
            selectedShape = MyShape.Line;
            selectedDrawMode = DrawMode.Shapes;

            ClearSelectedTools();
            ClearSelectedShapes(((MenuItem) sender).Name);
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            selectedShape = MyShape.Ellipse;
            selectedDrawMode = DrawMode.Shapes;

            ClearSelectedTools();
            ClearSelectedShapes(((MenuItem) sender).Name);
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            selectedShape = MyShape.Rectangle;
            selectedDrawMode = DrawMode.Shapes;

            ClearSelectedTools();
            ClearSelectedShapes(((MenuItem) sender).Name);
        }

        private void PolylineButton_Click(object sender, RoutedEventArgs e)
        {
            selectedShape = MyShape.Polyline;
            selectedDrawMode = DrawMode.Shapes;

            ClearSelectedTools();
            ClearSelectedShapes(((MenuItem) sender).Name);
        }

        private void PolygonButton_Click(object sender, RoutedEventArgs e)
        {
            selectedShape = MyShape.Polygon;
            selectedDrawMode = DrawMode.Shapes;

            ClearSelectedTools();
            ClearSelectedShapes(((MenuItem) sender).Name);
        }

        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDrawMode = DrawMode.Erase;

            ClearSelectedTools(((MenuItem) sender).Name);
            ClearSelectedShapes();
        }

        private void FillButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDrawMode = DrawMode.Fill;

            ClearSelectedTools(((MenuItem) sender).Name);
            ClearSelectedShapes();
        }

        private void PencilButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDrawMode = DrawMode.Pencil;

            ClearSelectedTools(((MenuItem) sender).Name);
            ClearSelectedShapes();
        }

        private void ClearSelectedShapes(string shapeName = null)
        {
            var shapeItems = Shapes.Items.Cast<MenuItem>().ToArray();
            foreach (var shapeItem in shapeItems)
            {
                shapeItem.IsChecked = !string.IsNullOrEmpty(shapeName) && shapeItem.Name == shapeName;
            }
        }

        private void ClearSelectedTools(string toolName = null)
        {
            var toolItems = Tools.Items.Cast<MenuItem>().ToArray();
            foreach (var toolItem in toolItems)
            {
                toolItem.IsChecked = !string.IsNullOrEmpty(toolName) && toolItem.Name == toolName;
            }
        }

        #endregion

        #region DrawBox MouseEvents

        private void DrawBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                start = e.GetPosition((Canvas) sender);
            }
        }

        private void DrawBox_OnMouseMove(object sender, MouseEventArgs e)
        {
            switch (selectedDrawMode)
            {
                case DrawMode.Pencil:
                    DrawPencil((Canvas)sender, e);
                    break;
                case DrawMode.Erase:
                    DrawErase((Canvas)sender, e);
                    break;
                case DrawMode.Fill:
                    break;
                case DrawMode.Shapes:
                    DrawShapes((Canvas)sender, e);
                    break;
            }
        }

        private void DrawErase(Canvas canvas, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var line = new Line
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    StrokeThickness = (double) selectedThick,
                    X1 = start.X,
                    Y1 = start.Y,
                    X2 = e.GetPosition(DrawBox).X,
                    Y2 = e.GetPosition(DrawBox).Y
                };


                start = e.GetPosition(canvas);

                DrawBox.Children.Add(line);
            }
        }

        private void DrawPencil(Canvas canvas, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var line = new Line
                {
                    Stroke = new SolidColorBrush(selectedColor),
                    StrokeThickness = (double) selectedThick,
                    X1 = start.X,
                    Y1 = start.Y,
                    X2 = e.GetPosition(canvas).X,
                    Y2 = e.GetPosition(canvas).Y
                };


                start = e.GetPosition(canvas);

                DrawBox.Children.Add(line);
            }
        }

        private void DrawShapes(Canvas canvas, MouseEventArgs e)
        {
            switch (selectedShape)
            {
                case MyShape.Polyline when polyLine == null:
                    return;
                case MyShape.Polyline:
                    polyLine.Points[^1] = e.GetPosition(canvas);
                    break;
                case MyShape.Polygon when polygon == null:
                    return;
                case MyShape.Polygon:
                    polygon.Points[^1] = e.GetPosition(canvas);
                    break;
                default:
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        end = e.GetPosition(canvas);
                    }

                    break;
                }
            }
        }

        private void DrawBox_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedDrawMode == DrawMode.Shapes)
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
        }

        #endregion

        #region ShapeDraw

        private void DrawRectangle()
        {
            // kolor zmieni sie z panelu wyboru kolorów
            var newLine = new Rectangle()
            {
                Stroke = new SolidColorBrush(selectedColor),
                StrokeThickness = (double) selectedThick,
                Height = 10,
                Width = 10
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
            var newLine = new Ellipse
            {
                Stroke = new SolidColorBrush(selectedColor),
                StrokeThickness = (double) selectedThick,
                Height = 10,
                Width = 10
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            popuwWindow.IsOpen = ture;
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

        #region Set color and thick

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

        private void ColorPicker_Change(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (ColorPicker.SelectedColor.HasValue)
            {
                selectedColor = ColorPicker.SelectedColor.Value;
            }
        }

        #endregion

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
    }
}