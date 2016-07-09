using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;

namespace SpritesheetCropper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public BitmapImage CurrentSpritesheet { get; set; }
		private Line[] MainGridLines { get; set; }
		public const double ScaleRate = 1.1;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void InitalizeCanvas(BitmapImage image)
		{
			//MainGridLines = new Line[imageWidth + imageHeight];
			MainCanvas.Background = Brushes.White;
			//MainCanvas.MouseDown += new MouseButtonEventHandler(this.MainCanvasMouseDown);
			//MainCanvas.MouseUp += new MouseButtonEventHandler(this.MainCanvasMouseUp);
			//MainCanvas.MouseMove += new MouseEventHandler(this.MainCanvasMouseMove);

			MainCanvas.MouseWheel += new MouseWheelEventHandler(this.MainCanvasMouseWheel);

			ImageBrush brush = new ImageBrush();
			brush.Stretch = Stretch.Uniform;
			brush.ImageSource = CurrentSpritesheet;

			MainCanvas.Background = brush;
			MainCanvas.Height = CurrentSpritesheet.Height;
			MainCanvas.Width = CurrentSpritesheet.Width;
			(CanvasHolder.Children[0] as PixelRuler).Width = MainCanvas.Width;


			MainGridLines = new Line[(int)image.Width+(int)image.Height];
			GenerateGridLine();

			//MainGridLines.ToList().ForEach(x => x.Stroke = Brushes.Transparent);
		}

		private void MainCanvasMouseWheel(object sender, MouseWheelEventArgs e)
		{
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

			MainCanvas.LayoutTransform = scale;
			MainCanvas.UpdateLayout();
		}

		private void MenuItem_OnOpen(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialogResult = new OpenFileDialog();

			// Set filter for file extension and default file extension 
			dialogResult.DefaultExt = ".png";
			dialogResult.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


			// Display OpenFileDialog by calling ShowDialog method 
			Nullable<bool> result = dialogResult.ShowDialog();


			// Get the selected file name and display in a TextBox 
			if (!result.HasValue || !result.Value)
				return;
			// Open document 
			
			CurrentSpritesheet = new BitmapImage(new Uri(dialogResult.FileName));

			InitalizeCanvas(CurrentSpritesheet);
		}

		/// <summary>
		/// Genera un reticolato di Linee per evidenziare il mosaico di tile.
		/// Le linee generate vengono automaticamente inserite nel canvas principale
		/// </summary>
		private void GenerateGridLine()
		{
			var j = 0;
			for (int i = 0; i < MainCanvas.Height; i++)
			{
				var line = new Line();
				line.Width = 0.5f;
				line.Stroke = Brushes.Black;

				line.X1 = 0;
				line.X2 = MainCanvas.Width;
				line.Y1 = i;
				line.Y2 = i;
				line.StrokeThickness = 1;
				MainGridLines[j++] = line;
				MainCanvas.Children.Add(line);
			}

			for (int i = 0; i < MainCanvas.Width; i++)
			{
				var line = new Line();
				line.Stroke = Brushes.Black;
				line.Width = 0.005f;
				line.X1 = i;
				line.X2 = i;
				line.Y1 = 0;
				line.Y2 = MainCanvas.Height;
				line.StrokeThickness = 1;
				MainGridLines[i++] = line;
				MainCanvas.Children.Add(line);
			}
		}
	}
}
