using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace SpritesheetCropper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private BitmapImage CurrentSpritesheet { get; set; }
		private Rectangle RedPointPosition { get; set; }
		private List<Sprite> Sprites { get; set; }

		private Point startPoint;
		private Rectangle rect;

		private Sprite CurrentSprite { get; set; }
		private bool isInPivotSelection;
		private bool isSpriteDrawnable;
		private bool isPopupOpen;
		private bool isInFrameSelection;

		public const double ScaleRate = 1.1;

		public MainWindow()
		{
			InitializeComponent();
			this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
		}

		private void HandleEsc(object sender, KeyEventArgs e)
		{
			if (isInPivotSelection)
			{
				MainCanvas.Children.Remove(rect);
				MainCanvas.Children.Remove(CurrentSprite.Pivot);
				isInPivotSelection = false;
			}

			if (isInFrameSelection)
			{
				MainCanvas.Children.Remove(CurrentSprite.Pivot);

				PopupFrameValue.Text = "20";
				PopupFrame.IsOpen = false;
				Mouse.OverrideCursor = Cursors.Cross;
				isInFrameSelection = false;
				isInPivotSelection = true;
			}

		}

		private void InitializeCanvas(BitmapImage image)
		{

			Image img = new Image();
			img.Source = image;

			Sprites = new List<Sprite>();

			MainCanvas.Background = Brushes.White;
			MainCanvas.SnapsToDevicePixels = true;

			MainCanvas.MouseWheel += new MouseWheelEventHandler(this.MainCanvasMouseWheel);

			RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
			img.SnapsToDevicePixels = true;

			MainCanvas.Children.Insert(0, img);

			MainCanvas.Height = CurrentSpritesheet.Height;
			MainCanvas.Width = CurrentSpritesheet.Width;
		}

		private void PopupFrameMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var frame = int.Parse(PopupFrameValue.Text);

			if (e.Delta > 0)
				frame += 1;
			else
				frame -= 1;

			if (frame < 0)
				frame = 0;

			PopupFrameValue.Text = frame.ToString();
		}



		private void MainCanvasMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (isPopupOpen) return;

			if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
				return;

			ScaleTransform scale = new ScaleTransform(MainCanvas.ActualWidth, MainCanvas.ActualHeight); ;

			if (e.Delta > 0)
			{
				scale = new ScaleTransform(MainCanvas.LayoutTransform.Value.M11 * ScaleRate, MainCanvas.LayoutTransform.Value.M22 * ScaleRate);
			}
			else
			{
				scale = new ScaleTransform(MainCanvas.LayoutTransform.Value.M11 / ScaleRate, MainCanvas.LayoutTransform.Value.M22 / ScaleRate);
			}

			PopupFrameValue.FontSize = 50 / scale.ScaleX;
			MainCanvas.LayoutTransform = scale;
			MainCanvas.UpdateLayout();
		}

		private void MenuItem_OnSave(object sender, RoutedEventArgs e)
		{
			if (isPopupOpen) return;

			if (SpriteGridView.Items.Count == 0)
				return;

			var fileText = String.Empty;
			List<DataGridRow> rows = new List<DataGridRow>();

			var itemsSource = SpriteGridView.ItemsSource as IEnumerable;
			if (itemsSource != null)
			{
				foreach (var item in itemsSource)
				{
					var row = SpriteGridView.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
					if (row != null || (row.DataContext as Sprite) != null)
					{
						var sprite = (row.DataContext as Sprite);
						var line = string.Join(":", sprite.X.ToString(), sprite.Y.ToString(), sprite.Width.ToString(), sprite.Height.ToString(), sprite.xPivot.ToString(), sprite.yPivot.ToString(), sprite.Frames.ToString());
						fileText += line + ";";
					}

				}
				fileText = fileText.Replace(";", ";" + System.Environment.NewLine);
			}

			SaveFileDialog dialog = new SaveFileDialog()
			{
				Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
			};

			if (dialog.ShowDialog() == true)
			{
				System.IO.File.WriteAllText(dialog.FileName, fileText);
			}
		}


		private void MenuItem_OnOpen(object sender, RoutedEventArgs e)
		{
			if (isPopupOpen) return;

			OpenFileDialog dialogResult = new OpenFileDialog();

			// Set filter for file extension and default file extension 
			dialogResult.DefaultExt = ".png";
			dialogResult.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


			// Display OpenFileDialog by calling ShowDialog method 
			bool? result = dialogResult.ShowDialog();

			if (!result.HasValue || !result.Value)
				return;
			// Open document 

			CurrentSpritesheet = new BitmapImage(new Uri(dialogResult.FileName));
			DataGridHolder.Visibility = Visibility.Visible;
			InitializeCanvas(CurrentSpritesheet);
			InitializeScale();
		}

		private void InitializeScale()
		{

			ScaleTransform scale = new ScaleTransform(MainCanvas.ActualWidth, MainCanvas.ActualHeight); ;

			do
			{
				scale = new ScaleTransform(MainCanvas.LayoutTransform.Value.M11 * ScaleRate,
					MainCanvas.LayoutTransform.Value.M22 * ScaleRate);

				PopupFrameValue.FontSize = 50 / scale.ScaleX;
				MainCanvas.LayoutTransform = scale;
				MainCanvas.UpdateLayout();
			} while (CanvasHolderScrollBar.ComputedHorizontalScrollBarVisibility == Visibility.Collapsed
			 && CanvasHolderScrollBar.ComputedVerticalScrollBarVisibility == Visibility.Collapsed);
		}

		private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (isPopupOpen) return;

			if (isInPivotSelection || isSpriteDrawnable || isInFrameSelection)
				return;

			startPoint = new Point(Math.Round(e.GetPosition(MainCanvas).X), Math.Round(e.GetPosition(MainCanvas).Y));

			CurrentSprite = new Sprite
			{
				X = Convert.ToInt32(startPoint.X),
				Y = Convert.ToInt32(startPoint.Y),
			};

			rect = new Rectangle
			{
				Stroke = Brushes.LightBlue,
				StrokeThickness = 1,
				SnapsToDevicePixels = true,

			};

			rect.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
			Canvas.SetLeft(rect, startPoint.X);
			Canvas.SetTop(rect, startPoint.Y);

			MainCanvas.Children.Add(rect);
		}

		private void PopupFrameMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			CurrentSprite.Frames = int.Parse(PopupFrameValue.Text);
			PopupFrameValue.Text = "20";
			PopupFrame.IsOpen = false;
			isPopupOpen = false;
		}

		private void Window_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var isInsideCanvas = Mouse.GetPosition(MainCanvas).X >= 0 && Mouse.GetPosition(MainCanvas).Y >= 0;
			if (isInsideCanvas) return;

			Canvas_MouseUp(sender, e);
		}

		private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (isPopupOpen) return;

			if (rect == null)
				return;

			if (double.IsNaN(rect.Width) || double.IsNaN(rect.Height) || rect.Width <= 1 || rect.Height <= 1)
			{
				MainCanvas.Children.Remove(rect);
				isInPivotSelection = false;
				Mouse.OverrideCursor = Cursors.Arrow;
				return;
			}

			if (!isInPivotSelection && !isInFrameSelection && !isSpriteDrawnable)
			{
				isInPivotSelection = true;

				var mousePos = new Point(Math.Round(e.GetPosition(MainCanvas).X), Math.Round(e.GetPosition(MainCanvas).Y));
				var pos = GetLeftCornerByDiagonalPoints(mousePos, new Point(CurrentSprite.X, CurrentSprite.Y));
				if (pos.X < CurrentSprite.X || pos.Y < CurrentSprite.Y)
				{
					CurrentSprite.X = (int)pos.X;
					CurrentSprite.Y = (int)pos.Y;
				}

				Mouse.OverrideCursor = Cursors.Cross;
				CurrentSprite.Height = Convert.ToInt32(rect.Height);
				CurrentSprite.Width = Convert.ToInt32(rect.Width);
				CurrentSprite.Rectangle = rect;
				CurrentSprite.Id = (Sprites.Count > 0) ? Sprites.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1 : 1;
				var color = new SolidColorBrush(Color.FromRgb(255, 255, 255));
				color.Opacity = 0.5;
				CurrentSprite.Rectangle.Fill = color;
				return;
			}

			if (isInPivotSelection)
			{
				isInPivotSelection = false;

				Mouse.OverrideCursor = Cursors.Arrow;
				isInFrameSelection = true;

				var pivot = DrawPoint((int)e.GetPosition(MainCanvas).X, (int)e.GetPosition(MainCanvas).Y);
				CurrentSprite.Pivot = pivot;
				MainCanvas.Children.Add(pivot);

				CurrentSprite.xPivot = (int)e.GetPosition(MainCanvas).X;
				CurrentSprite.yPivot = (int)e.GetPosition(MainCanvas).Y;

				ShowPopup(CurrentSprite.Width, CurrentSprite.Height, System.Windows.Controls.Primitives.PlacementMode.MousePoint);

				isSpriteDrawnable = true;
				return;
			}

			if (isSpriteDrawnable)
			{
				isInFrameSelection = false;

				Sprites.Add(CurrentSprite);
				SpriteGridView.ItemsSource = null;

				SpriteGridView.ItemsSource = Sprites;
				SpriteGridView.UpdateLayout();
				isSpriteDrawnable = false;
				RemoveAllTrasparentDots();
				rect = new Rectangle();
				return;
			}

		}

		private void ShowPopup(int width, int height, System.Windows.Controls.Primitives.PlacementMode mousePoint)
		{
			PopupFrame.Height = CurrentSprite.Height;
			PopupFrame.Width = CurrentSprite.Width;
			PopupFrame.IsOpen = true;
			PopupFrame.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
			isPopupOpen = true;
		}

		private void RemoveAllTrasparentDots()
		{
			List<Rectangle> trasparentRectangles = new List<Rectangle>();

			foreach (UIElement child in MainCanvas.Children)
			{
				if (child is Rectangle && (child as Rectangle).Name == "TrasparentDot")
					trasparentRectangles.Add((Rectangle)child);
			}

			trasparentRectangles.ForEach(x => MainCanvas.Children.Remove(x));
		}

		private void Canvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (isPopupOpen) return;

			var pos = new Point(Math.Round(e.GetPosition(MainCanvas).X), Math.Round(e.GetPosition(MainCanvas).Y));
			bottomCoordinates.Text = "X: " + pos.X + "; Y: " + pos.Y;

			if (!isInPivotSelection && e.LeftButton == MouseButtonState.Released || rect == null)
				return;

			if (isInFrameSelection)
				return;

			if (isInPivotSelection)
			{
				if (RedPointPosition != null)
				{
					RedPointPosition.Fill = new SolidColorBrush(Colors.Transparent);
					RedPointPosition.Name = "TrasparentDot";
				}

				RedPointPosition = DrawPoint((int)e.GetPosition(MainCanvas).X, (int)e.GetPosition(MainCanvas).Y, true);
				RedPointPosition.Name = "RedPointPosition";


				MainCanvas.Children.Add(RedPointPosition);
				return;
			}

			var x = Math.Min(pos.X, startPoint.X);
			var y = Math.Min(pos.Y, startPoint.Y);

			var w = Math.Max(pos.X, startPoint.X) - x;
			var h = Math.Max(pos.Y, startPoint.Y) - y;

			rect.Width = w;
			rect.Height = h;

			Canvas.SetLeft(rect, x);
			Canvas.SetTop(rect, y);
		}

		private void DeleteSprite_MouseClick(object sender, RoutedEventArgs e)
		{
			if (isPopupOpen) return;

			object id = ((Button)sender).CommandParameter;

			if (id == null)
				return;

			UIElement rectangleToDelete = null;
			UIElement pivotToDelete = null;

			foreach (UIElement child in MainCanvas.Children)
			{
				if (child as Rectangle != null)
				{
					if (Equals(child, Sprites.FirstOrDefault(x => x.Id == int.Parse(id.ToString())).Rectangle))
					{
						rectangleToDelete = child;
					}
					else if (Equals(child, Sprites.FirstOrDefault(x => x.Id == int.Parse(id.ToString())).Pivot))
					{
						pivotToDelete = child;
					}
				}
			}

			MainCanvas.Children.Remove(rectangleToDelete);
			MainCanvas.Children.Remove(pivotToDelete);

			Sprites.Remove(Sprites.FirstOrDefault(x => x.Id == int.Parse(id.ToString())));

			SpriteGridView.ItemsSource = null;
			SpriteGridView.ItemsSource = Sprites;
		}

		private Rectangle DrawPoint(int x, int y, bool isRed = false)
		{
			Rectangle rec = new Rectangle();
			Canvas.SetTop(rec, y);
			Canvas.SetLeft(rec, x);
			rec.Width = 1;
			rec.Height = 1;
			rec.Fill = isRed ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
			return rec;
		}

		public Point GetLeftCornerByDiagonalPoints(Point firstPoint, Point secondPoint)
		{
			return new Point(Math.Min(firstPoint.X, secondPoint.X), Math.Min(firstPoint.Y, secondPoint.Y));
		}
	}
}
