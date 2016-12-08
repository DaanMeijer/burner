using System;

using System.IO.Ports;
using Gtk;


namespace Burner
{
	public partial class MainWindow : Gtk.Window
	{

		private LaserCutter laserCutter;

		public MainWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();

			this.btnUp.Clicked +=  new EventHandler (this.clickUp); 
			this.btnDown.Clicked +=  new EventHandler (this.clickDown); 
			this.btnLeft.Clicked +=  new EventHandler (this.clickLeft); 
			this.btnRight.Clicked +=  new EventHandler (this.clickRight); 

			this.btnStop.Clicked += new EventHandler (this.clickStop); 
		
			this.sclPower.ChangeValue += new ChangeValueHandler (this.changePower);

			laserCutter = new LaserCutter ("/dev/ttyUSB0");

			this.KeyPressEvent += new KeyPressEventHandler (this.onKeyPress); 
			this.KeyReleaseEvent += new KeyReleaseEventHandler (this.onKeyRelease); 




		}

		public void changePower(object sender, ChangeValueArgs args){
			laserCutter.power((byte)(255 - sclPower.Value));
		}

		[GLib.ConnectBefore ()]
		public void onKeyPress(object sender, KeyPressEventArgs evt){
			switch (evt.Event.Key) {
			case Gdk.Key.Up:
				laserCutter.moveUp (50);
				break;
			case Gdk.Key.Down:
				laserCutter.moveDown (50);
				break;
			case Gdk.Key.Left:
				laserCutter.moveLeft (50);
				break;
			case Gdk.Key.Right:
				laserCutter.moveRight (50);
				break;
			}

		}


		[GLib.ConnectBefore ()]
		public void onKeyRelease(object sender, KeyReleaseEventArgs evt){
			switch (evt.Event.Key) {
			case Gdk.Key.Up:
			case Gdk.Key.Down:
			case Gdk.Key.Left:
			case Gdk.Key.Right:
				laserCutter.stop ();
				break;
			}
		}

			

		public void clickStop(object sender, EventArgs evt){
			laserCutter.stop();
		}

		public void clickUp(object sender, EventArgs evt){
			laserCutter.moveUp(50);
		}

		public void clickDown(object sender, EventArgs evt){
			laserCutter.moveDown(50);
		}

		public void clickLeft(object sender, EventArgs evt){
			laserCutter.moveLeft(50);
		}

		public void clickRight(object sender, EventArgs evt){
			laserCutter.moveRight(50);
		}


	}
}

