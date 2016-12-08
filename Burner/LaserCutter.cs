using System;
using System.IO.Ports;
using System.Threading;

namespace Burner
{
	public class LaserCutter
	{
		private SerialPort serialPort;

		public LaserCutter (string port)
		{
			serialPort = new SerialPort (port);
			serialPort.BaudRate = 38400;

			serialPort.Open ();
			if (!serialPort.IsOpen) {
				throw new Exception ("Port not open");
			}


			checkFS ();

			serialPort.DataReceived += new SerialDataReceivedEventHandler(this.serialDataEventHandler);


		}

		public void stop(){
			this.serialPort.WriteLine("#t00000000");
		}


		private byte[] buf = new byte[]
		{
			35,
			1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			10
		};

		private bool isOk = false;

		public void moveRight(byte speed = 50){
			this.move ('w', speed);
		}

		public void moveLeft(byte speed = 50){
			this.move ('s', speed);
		}

		public void moveDown(byte speed = 50){
			this.move ('a', speed);
		}

		public void moveUp(byte speed = 50){
			this.move ('d', speed);
		}

		public void move(char direction, byte speed){
			if (speed == 0)	{
				stop ();
			} else {
				this.buf[1] = (byte)'p';
				this.buf[2] = speed;
				go ();

				Thread.Sleep(5);
				this.buf[1] = (byte)direction;
				go ();
				Thread.Sleep(5);
			}
		}

		public void power(byte strength){
			this.buf[1] = 106;
			this.buf[2] = strength;
			go ();
		}

		private void go(bool force = false){
			if (serialPort.IsOpen && (isOk || force))
			{
				serialPort.Write(this.buf, 0, 11);
				while (serialPort.BytesToWrite > 0)			{
				}
			}
		}

		public void sendCommand(byte command, byte[] parameters){
			
			byte[] cmd = new byte[parameters.Length + 1];

			cmd[0] = command;
			for(int a=0; a<parameters.Length; a++){
				cmd [1 + a] = parameters [a];
			}

			this.sendRawCommand (cmd);
		}

		private void sendRawCommand(byte[] cmd){


			if (!serialPort.IsOpen) {
				throw new Exception ("Unable to send command while port is not open.");
			}


			serialPort.Write (cmd, 0, cmd.Length);


			while (serialPort.BytesToWrite > 0) {
				Thread.Sleep (1);
			}

		}

		private Random ran = new Random ();
		private DateTime dateTime = new DateTime();

		private byte magicNumber = 0;
		private byte lastByte = 0;
		private byte privateMagicNumber = 86;

		// I do not know what this does, I'm sorry...
		public void checkFS()
		{
			this.buf[1] = 108;
			this.buf[2] = 106;
			this.magicNumber = (byte)this.ran.Next(5, 101);
			this.buf[3] = this.magicNumber;
			this.buf[4] = (byte)this.ran.Next(1, 200);
			this.buf[5] = (byte)this.dateTime.Day;

			go (true);

			Thread.Sleep (70);

			if (serialPort.BytesToRead > 0) {
				lastByte = (byte)serialPort.ReadByte ();
				isOk = lastByte == (magicNumber ^ privateMagicNumber);
			}
		}

		/*
		public void listenForSerialData(){
			while (serialPort.IsOpen) {
				if (serialPort.BytesToRead > 0) {
					serialPort.DataReceived +=
				}
				Thread.Sleep (25);
			}
		}

		~LaserCutter(){
			if (listenerThread != null) {
				listenerThread.stop ();

			}
		}
		*/

		public void serialDataEventHandler(object sender, SerialDataReceivedEventArgs evt){
			int len = serialPort.BytesToRead;
			byte[] buffer = new byte[len];
			serialPort.Read (buffer, 0, len);
			this.handleSerialData (buffer);
		}

		public void handleSerialData(byte[] data){
			received = data;
		}

	}


}

