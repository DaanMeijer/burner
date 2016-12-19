using System;

using System.IO.Ports;
using Gtk;
using System.Collections.Generic;
using Gdk;
using System.Drawing;

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

			this.btnHalt.Clicked += new EventHandler (this.clickHalt);

			this.btnBurn.Clicked += new EventHandler (this.demo);
			this.btnUnknown.Clicked += new EventHandler (this.unknownFunction);
		
			this.sclPower.ChangeValue += new ChangeValueHandler (this.changePower);

			laserCutter = new LaserCutter ("/dev/ttyUSB0");
			laserCutter.sendLog += new LaserCutter.Log (this.handleLasercutterLog);

			this.KeyPressEvent += new KeyPressEventHandler (this.onKeyPress); 
			this.KeyReleaseEvent += new KeyReleaseEventHandler (this.onKeyRelease); 

			this.Destroyed += new EventHandler (this.OnDestroy);


			fileImage.SelectionChanged += new EventHandler (this.selectImage);
		}

		private Bitmap bitmapImage; 
		public void selectImage(object o, EventArgs args){

			bitmapImage = new Bitmap(fileImage.Filename);

			var buffer = System.IO.File.ReadAllBytes (fileImage.Filename);

			var pixbuf = new Pixbuf (buffer);
			imgPreview.Pixbuf = pixbuf;
		
		}

		private void OnDestroy(object o, EventArgs args)
		{
			this.laserCutter.close ();
			this.laserCutter = null;
			Application.Quit();
		}


		public void handleLasercutterLog(string line){
			Gtk.Application.Invoke (delegate {
				string text = line + "\n" + this.txtLog.Buffer.Text;
				if(text.Length > 1024){
					int index = text.IndexOf('\n', 1024);
					if(index > 0){
						text = text.Substring(1, index);
					}
				}
				txtLog.Buffer.Text = text;
			});
		}

		public void unknownFunction(object sender, EventArgs evt){
			laserCutter.wt = 50;
			laserCutter.setInitialSettings();

		}

		public void burnBitmap(){


			int pulses = (byte)(int)sclLinger.Value;
			int power = (int)sclBitmapPower.Value;

			sendSettings ();
			List<Command> commands = new List<Command> ();


			var pixels = bitmapImage;

			for (int y = 0; y < pixels.Height; y++) {

				Command[] line = new Command[pixels.Width];

				for (int _x = 0; _x < pixels.Width; _x++) {

					int x = _x;
					if(y%2 == 1){
						x = pixels.Width - _x - 1;
					}

					var color = pixels.GetPixel (x, y);

					//byte a = (byte)(color >> 24);
					double r = color.R / 255d;
					double g = color.G / 255d;
					double b = color.B / 255d;

					double avg = 1 - ((r + g + b) / 3);

					avg *= (color.A / 255d);

					int powerScale = Math.Max(0, Math.Min(255, power)) - 255;

						
					byte bytePower = (byte)(int)( 255 + (avg * powerScale));

					//System.Console.WriteLine(String.Format("R: {0}, G: {1}, B: {2}, A: {3}, power: {4}", color.R, color.G, color.B, color.A, power));

					line[_x] = (new PulseCommand (x, y, bytePower, pulses));
				
				}

				for (int a = 1; a < line.Length - 1; a++) {

					if (
						false && 
						line[a].power > 240 &&
						(line[a-1].power > 240) &&
						(line[a+1].power > 240)
					) {
						//Console.WriteLine (String.Format("Removed node [{0},{1}]", line[a].x, line[a].y));
					} else {
						//Console.WriteLine (String.Format("Keeping node [{0},{1}]", line[a].x, line[a].y));
						commands.Add (line[a]);
					}

				}

			}
				
			laserCutter.queueCommands (commands.ToArray());
		}

		private void sendSettings(){

			laserCutter.speed = (byte)(int)sclSpeed.Value;
			laserCutter.dotSizeInSteps = (byte)(int)sclStepSize.Value;
			laserCutter.wt = (int)sclWT.Value;
			laserCutter.setInitialSettings ();

		}

		public void demo(object sender, EventArgs evt){
			burnBitmap ();
		}

		private void doSquare(){
			sendSettings ();


			byte power = (byte)(int)sclBitmapPower.Value;
			int pulses = (byte)(int)sclLinger.Value;

			laserCutter.queueCommands (new Command[] {
				new PulseCommand (0, 0, power, pulses), 
				new PulseCommand (1, 0, power, pulses), 
				new PulseCommand (2, 0, power, pulses),
				new PulseCommand (3, 0, power, pulses),
				new PulseCommand (4, 0, power, pulses), 
				new PulseCommand (5, 0, power, pulses), 
				new PulseCommand (6, 0, power, pulses),
				new PulseCommand (7, 0, power, pulses),


				new PulseCommand (7, 1, power, pulses), 
				new PulseCommand (6, 1, power, pulses), 
				new PulseCommand (5, 1, power, pulses),
				new PulseCommand (4, 1, power, pulses),
				new PulseCommand (3, 1, power, pulses), 
				new PulseCommand (2, 1, power, pulses), 
				new PulseCommand (1, 1, power, pulses),
				new PulseCommand (0, 1, power, pulses),

				new PulseCommand (0, 2, power, pulses), 
				new PulseCommand (1, 2, power, pulses), 
				new PulseCommand (2, 2, power, pulses),
				new PulseCommand (3, 2, power, pulses),
				new PulseCommand (4, 2, power, pulses), 
				new PulseCommand (5, 2, power, pulses), 
				new PulseCommand (6, 2, power, pulses),
				new PulseCommand (7, 2, power, pulses),


				new PulseCommand (7, 3, power, pulses), 
				new PulseCommand (6, 3, power, pulses), 
				new PulseCommand (5, 3, power, pulses),
				new PulseCommand (4, 3, power, pulses),
				new PulseCommand (3, 3, power, pulses), 
				new PulseCommand (2, 3, power, pulses), 
				new PulseCommand (1, 3, power, pulses),
				new PulseCommand (0, 3, power, pulses),

				new PulseCommand (0, 4, power, pulses), 
				new PulseCommand (1, 4, power, pulses), 
				new PulseCommand (2, 4, power, pulses),
				new PulseCommand (3, 4, power, pulses),
				new PulseCommand (4, 4, power, pulses), 
				new PulseCommand (5, 4, power, pulses), 
				new PulseCommand (6, 4, power, pulses),
				new PulseCommand (7, 4, power, pulses),


				new PulseCommand (7, 5, power, pulses), 
				new PulseCommand (6, 5, power, pulses), 
				new PulseCommand (5, 5, power, pulses),
				new PulseCommand (4, 5, power, pulses),
				new PulseCommand (3, 5, power, pulses), 
				new PulseCommand (2, 5, power, pulses), 
				new PulseCommand (1, 5, power, pulses),
				new PulseCommand (0, 5, power, pulses),

				new PulseCommand (0, 6, power, pulses), 
				new PulseCommand (1, 6, power, pulses), 
				new PulseCommand (2, 6, power, pulses),
				new PulseCommand (3, 6, power, pulses),
				new PulseCommand (4, 6, power, pulses), 
				new PulseCommand (5, 6, power, pulses), 
				new PulseCommand (6, 6, power, pulses),
				new PulseCommand (7, 6, power, pulses),


				new PulseCommand (7, 7, power, pulses), 
				new PulseCommand (6, 7, power, pulses), 
				new PulseCommand (5, 7, power, pulses),
				new PulseCommand (4, 7, power, pulses),
				new PulseCommand (3, 7, power, pulses), 
				new PulseCommand (2, 7, power, pulses), 
				new PulseCommand (1, 7, power, pulses),
				new PulseCommand (0, 7, power, pulses),


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
				laserCutter.startMovingUp (30);
				break;
			case Gdk.Key.Down:
				laserCutter.startMovingDown (30);
				break;
			case Gdk.Key.Left:
				laserCutter.startMovingLeft (30);
				break;
			case Gdk.Key.Right:
				laserCutter.startMovingRight (30);
				break;
			case Gdk.Key.space:
				laserCutter.power (10);
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

			case Gdk.Key.space:
				laserCutter.power (250);
				break;
			}
		}

		public void clickHalt(object sender, EventArgs evt){
			laserCutter.reset ();
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

