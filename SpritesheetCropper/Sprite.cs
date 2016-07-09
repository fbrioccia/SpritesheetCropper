using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SpritesheetCropper
{
	public class Sprite
	{
		public int Id { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int xPivot { get; set; }
		public int yPivot { get; set; }
		public Rectangle Pivot { get; set; }
		public Rectangle Rectangle { get; set; }
		public int Frames { get; set; }
		public Sprite() { }
	}
}
