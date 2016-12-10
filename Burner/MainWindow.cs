using System;

using System.IO.Ports;
using Gtk;
using System.Collections.Generic;


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

			this.btnDemo.Clicked += new EventHandler (this.demo);
			this.btnUnknown.Clicked += new EventHandler (this.unknownFunction);
		
			this.sclPower.ChangeValue += new ChangeValueHandler (this.changePower);

			laserCutter = new LaserCutter ("/dev/ttyUSB0");
			laserCutter.sendLog += new LaserCutter.Log (this.handleLasercutterLog);

			this.KeyPressEvent += new KeyPressEventHandler (this.onKeyPress); 
			this.KeyReleaseEvent += new KeyReleaseEventHandler (this.onKeyRelease); 

			this.Destroyed += new EventHandler (this.OnDestroy);
		}

		private void OnDestroy(object o, EventArgs args)
		{
			this.laserCutter.close ();
			this.laserCutter = null;
			Application.Quit();
		}


		public void handleLasercutterLog(string line){
			Gtk.Application.Invoke (delegate {
				txtLog.Buffer.Text = line + "\n" + this.txtLog.Buffer.Text;
			});
		}

		public void unknownFunction(object sender, EventArgs evt){
			laserCutter.wt = 50;
			laserCutter.setInitialSettings();

		}


		public void demo(object sender, EventArgs evt){
			byte power = 10;
			int pulses = 100;

			laserCutter.setInitialSettings ();

			laserCutter.queueCommands (new Command[] {
				new PulseCommand (0, 0, power, pulses), 
				new PulseCommand (1, 0, power, pulses), 
				new PulseCommand (2, 0, power, pulses),
				new PulseCommand (3, 0, power, pulses),
				new PulseCommand (4, 0, power, pulses), 
				new PulseCommand (5, 0, power, pulses), 
				new PulseCommand (6, 0, power, pulses),
				new PulseCommand (7, 0, power, pulses),

				new PulseCommand (0, 1, power, pulses), 
				new PulseCommand (1, 1, power, pulses), 
				new PulseCommand (2, 1, power, pulses),
				new PulseCommand (3, 1, power, pulses),
				new PulseCommand (4, 1, power, pulses), 
				new PulseCommand (5, 1, power, pulses), 
				new PulseCommand (6, 1, power, pulses),
				new PulseCommand (7, 1, power, pulses),

				new PulseCommand (0, 2, power, pulses), 
				new PulseCommand (1, 2, power, pulses), 
				new PulseCommand (2, 2, power, pulses),
				new PulseCommand (3, 2, power, pulses),
				new PulseCommand (4, 2, power, pulses), 
				new PulseCommand (5, 2, power, pulses), 
				new PulseCommand (6, 2, power, pulses),
				new PulseCommand (7, 2, power, pulses),

				new PulseCommand (0, 3, power, pulses), 
				new PulseCommand (1, 3, power, pulses), 
				new PulseCommand (2, 3, power, pulses),
				new PulseCommand (3, 3, power, pulses),
				new PulseCommand (4, 3, power, pulses), 
				new PulseCommand (5, 3, power, pulses), 
				new PulseCommand (6, 3, power, pulses),
				new PulseCommand (7, 3, power, pulses),

				new PulseCommand (0, 4, power, pulses), 
				new PulseCommand (1, 4, power, pulses), 
				new PulseCommand (2, 4, power, pulses),
				new PulseCommand (3, 4, power, pulses),
				new PulseCommand (4, 4, power, pulses), 
				new PulseCommand (5, 4, power, pulses), 
				new PulseCommand (6, 4, power, pulses),
				new PulseCommand (7, 4, power, pulses),

				new PulseCommand (0, 5, power, pulses), 
				new PulseCommand (1, 5, power, pulses), 
				new PulseCommand (2, 5, power, pulses),
				new PulseCommand (3, 5, power, pulses),
				new PulseCommand (4, 5, power, pulses), 
				new PulseCommand (5, 5, power, pulses), 
				new PulseCommand (6, 5, power, pulses),
				new PulseCommand (7, 5, power, pulses),

				new PulseCommand (0, 6, power, pulses), 
				new PulseCommand (1, 6, power, pulses), 
				new PulseCommand (2, 6, power, pulses),
				new PulseCommand (3, 6, power, pulses),
				new PulseCommand (4, 6, power, pulses), 
				new PulseCommand (5, 6, power, pulses), 
				new PulseCommand (6, 6, power, pulses),
				new PulseCommand (7, 6, power, pulses),

				new PulseCommand (0, 7, power, pulses), 
				new PulseCommand (1, 7, power, pulses), 
				new PulseCommand (2, 7, power, pulses),
				new PulseCommand (3, 7, power, pulses),
				new PulseCommand (4, 7, power, pulses), 
				new PulseCommand (5, 7, power, pulses), 
				new PulseCommand (6, 7, power, pulses),
				new PulseCommand (7, 7, power, pulses),


				new RunCommand(0, 0, 250)
			});
		}

		public void demo1(object sender, EventArgs evt){
			byte power = 250;
			int speed = 0xfe10;

			laserCutter.queueCommands (new Command[] {
				new RunCommand (3000, 0, power, speed), 
				new RunCommand (3000, 3000, power, speed), 
				new RunCommand (30, 30, power, speed),
				new RunCommand (0, 0, 250, speed),
			});
		}

		public void changePower(object sender, ChangeValueArgs args){
			laserCutter.power((byte)(255 - sclPower.Value));
		}

		[GLib.ConnectBefore ()]
		public void onKeyPress(object sender, KeyPressEventArgs evt){
			switch (evt.Event.Key) {
			case Gdk.Key.Up:
				laserCutter.startMovingUp (50);
				break;
			case Gdk.Key.Down:
				laserCutter.startMovingDown (50);
				break;
			case Gdk.Key.Left:
				laserCutter.startMovingLeft (50);
				break;
			case Gdk.Key.Right:
				laserCutter.startMovingRight (50);
				break;

			default:
				return;
			}

		}

		


		[GLib.ConnectBefore ()]
		public void onKeyRelease(object sender, KeyReleaseEventArgs evt){
			switch (evt.Event.Key) {
			case Gdk.Key.Up:
			case Gdk.Key.Down:
			case Gdk.Key.Left:
			case Gdk.Key.Right:
				laserCutter.stopMoving ();
				break;
			}
		}

			

		public void clickStop(object sender, EventArgs evt){
			laserCutter.stopMoving();
		}

		public void clickUp(object sender, EventArgs evt){
			laserCutter.startMovingUp(50);
		}

		public void clickDown(object sender, EventArgs evt){
			laserCutter.startMovingDown(50);
		}

		public void clickLeft(object sender, EventArgs evt){
			laserCutter.startMovingLeft(50);
		}

		public void clickRight(object sender, EventArgs evt){
			laserCutter.startMovingRight(50);
		}


	}
}

