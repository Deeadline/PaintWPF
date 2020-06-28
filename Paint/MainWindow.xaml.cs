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
                    DrawPencil((Canvas) sender, e);
                    break;
                case DrawMode.Erase:
                    DrawErase((Canvas) sender, e);
                    break;
                case DrawMode.Fill:
                    break;
                case DrawMode.Shapes:
                    DrawShapes((Canvas) sender, e);
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
                    Y2 = e.GetPosition(DrawBox).Y,
                };


                start = e.GetPosition(canvas);

                DrawBox.Children.Add(line);
            }
        }

        private void DrawPencil(Canvas canvas, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pencil = new Line
                {
                    Stroke = new SolidColorBrush(selectedColor),
                    StrokeThickness = (double) selectedThick,
                    X1 = start.X,
                    Y1 = start.Y,
                    X2 = e.GetPosition(canvas).X,
                    Y2 = e.GetPosition(canvas).Y,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Focusable = true
                };

                start = e.GetPosition(canvas);

                DrawBox.Children.Add(pencil);
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
            var rectangle = new Rectangle
            {
                Stroke = new SolidColorBrush(selectedColor),
                StrokeThickness = (double) selectedThick,
                Height = 10,
                Width = 10,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Focusable = true
            };
            if (end.X >= start.X)
            {
                rectangle.SetValue(Canvas.LeftProperty, start.X);
                rectangle.Width = end.X - start.X;
            }
            else
            {
                rectangle.SetValue(Canvas.LeftProperty, end.X);
                rectangle.Width = start.X - end.X;
            }

            if (end.Y >= start.Y)
            {
                rectangle.SetValue(Canvas.TopProperty, start.Y - 20);
                rectangle.Height = end.Y - start.Y;
            }
            else
            {
                rectangle.SetValue(Canvas.TopProperty, end.Y - 20);
                rectangle.Height = start.Y - end.Y;
            }

            DrawBox.Children.Add(rectangle);
        }

        private void DrawEllipse()
        {
            var ellipse = new Ellipse
            {
                Stroke = new SolidColorBrush(selectedColor),
                StrokeThickness = (double) selectedThick,
                Height = 10,
                Width = 10,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Focusable = true
            };
            if (end.X >= start.X)
            {
                ellipse.SetValue(Canvas.LeftProperty, start.X);
                ellipse.Width = end.X - start.X;
            }
            else
            {
                ellipse.SetValue(Canvas.LeftProperty, end.X);
                ellipse.Width = start.X - end.X;
            }

            if (end.Y >= start.Y)
            {
                ellipse.SetValue(Canvas.TopProperty, start.Y - 20);
                ellipse.Height = end.Y - start.Y;
            }
            else
            {
                ellipse.SetValue(Canvas.TopProperty, end.Y - 20);
                ellipse.Height = start.Y - end.Y;
            }

            DrawBox.Children.Add(ellipse);
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
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Focusable = true
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
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Focusable = true,
                };
                polygon.Points.Add(e.GetPosition(DrawBox));
                polygon.Points.Add(e.GetPosition(DrawBox));
                DrawBox.Children.Add(polygon);
            }
        }

        private void DrawLine()
        {
            var line = new Line
            {
                Stroke = new SolidColorBrush(selectedColor),
                X1 = start.X,
                Y1 = start.Y - 20,
                X2 = end.X,
                Y2 = end.Y - 20,
                StrokeThickness = (double) selectedThick,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Focusable = true
            };
            DrawBox.Children.Add(line);
        }

        #endregion

        #region Menu

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            DrawBox.Children.Clear();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog(this).GetValueOrDefault() && openDialog.CheckFileExists)
            {
                var brush = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(openDialog.FileName, UriKind.Relative))
                };
                DrawBox.Background = brush;
                openDialog.Reset();
            }
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
            Info infoWindow = new Info();
            infoWindow.Show();
        }

        private void MenuPrint_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog().GetValueOrDefault())
            {
            }
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

        private Double zoomMax = 5;
        private Double zoomMin = 1;
        private Double zoomSpeed = 0.001;
        private Double zoom = 1;

        private void DrawBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoom += zoomSpeed * e.Delta; 
            if (zoom < zoomMin) 
            { 
                zoom = zoomMin; 
            } 
            if (zoom > zoomMax) 
            {
                zoom = zoomMax; 
            } 

            Point mousePos = e.GetPosition(DrawBox);

            if (zoom > 1)
            {
                DrawBox.RenderTransform = new ScaleTransform(zoom, zoom, mousePos.X, mousePos.Y);
            }
            else
            {
                DrawBox.RenderTransform = new ScaleTransform(zoom, zoom);
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

        private void MenuNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MenuCopy_Click(object sender, ExecutedRoutedEventArgs e)
        {
            CopyToClipboard();
        }

        private void CopyToClipboard()
        {
            var rtb = new RenderTargetBitmap((int) DrawBox.RenderSize.Width,
                (int) DrawBox.RenderSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(DrawBox);
            Clipboard.SetImage(rtb);
        }

        private void MenuCut_Click(object sender, ExecutedRoutedEventArgs e)
        {
            CopyToClipboard();
            DrawBox.Background = null;
            DrawBox.Children.Clear();
        }

        private void MenuPaste_Click(object sender, ExecutedRoutedEventArgs e)
        {
            DrawBox.Children.Clear();
            DrawBox.Background = new ImageBrush
            {
                ImageSource = Clipboard.GetImage()
            };
        }

        private void FlipVertical_OnClick(object sender, RoutedEventArgs e)
        {
            var canvasChildren = DrawBox.Children;
            foreach (UIElement child in canvasChildren)
            {
                var oldTransform = child.RenderTransform as ScaleTransform;
                child.RenderTransform = new ScaleTransform
                {
                    ScaleY = -(oldTransform?.ScaleY ?? 1)
                };
            }
        }

        private void FlipHorizontal_OnClick(object sender, RoutedEventArgs e)
        {
            var canvasChildren = DrawBox.Children;
            foreach (UIElement child in canvasChildren)
            {
                var oldTransform = child.RenderTransform as ScaleTransform;
                child.RenderTransform = new ScaleTransform
                {
                    ScaleX = -(oldTransform?.ScaleX ?? 1)
                };
            }
        }
    }
}