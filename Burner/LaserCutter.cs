using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;

namespace Burner
{
	public class LaserCutter
	{
		private SerialPort serialPort;
		private Thread listenForDataThread;

		public LaserCutter (string port)
		{

			sendLog = new Log (this.nullLog);


			serialPort = new SerialPort (port);
			serialPort.BaudRate = 38400;

			serialPort.Open ();
			if (!serialPort.IsOpen) {
				throw new Exception ("Port not open");
			}
				
			checkFS ();

			listenForDataThread = new Thread (this.listenForData);
			listenForDataThread.Start ();


			serialPort.DataReceived += new SerialDataReceivedEventHandler(this.serialDataEventHandler);


		}

		public void close(){
			this.reset();
			this.serialPort.Close ();
		}

		public void nullLog (string a){
		}

		public void listenForData(){
			while (serialPort.IsOpen) {
				try {
				int bytesToRead = serialPort.BytesToRead;
				if (bytesToRead > 0) {

					byte[] buffer = new byte[bytesToRead];
					serialPort.Read (buffer, 0, bytesToRead);

					this.handleSerialData (buffer);

				}
				}catch(Exception e){
					System.Console.WriteLine (e.ToString ());
				}
			}
			Thread.Sleep(1);
		}

		public void stopMoving(){
			this.serialPort.WriteLine("#t00000000");
		}

		int movedX = 0;
		int movedY = 0;

		public void reset(){
			this.commands.Clear ();
			this.buf[1] = 71;
			this.buf[2] = BitConverter.GetBytes(this.movedX)[1];
			this.buf[3] = BitConverter.GetBytes(this.movedX)[0];
			this.buf[4] = BitConverter.GetBytes(this.movedY)[1];
			this.buf[5] = BitConverter.GetBytes(this.movedY)[0];
			send ();
			Thread.Sleep(20);
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


		public void doRun(int x, int y, int speed = 65136, byte power = 250){

			this.buf[1] = 113;
			this.buf[2] = BitConverter.GetBytes(x)[1];
			this.buf[3] = BitConverter.GetBytes(x)[0];
			this.buf[4] = BitConverter.GetBytes(y)[1];
			this.buf[5] = BitConverter.GetBytes(y)[0];
			this.buf[6] = BitConverter.GetBytes(speed)[1];
			this.buf[7] = BitConverter.GetBytes(speed)[0];
			this.buf[8] = power;

			this.send ();

		}

		public double pulseTime = 20;
		public void executePulseCommand(PulseCommand cmd){


			this.buf[1] = 103;
			this.buf[2] = BitConverter.GetBytes(cmd.x)[1];
			this.buf[3] = BitConverter.GetBytes(cmd.x)[0];
			this.buf[4] = BitConverter.GetBytes(cmd.y)[1];
			this.buf[5] = BitConverter.GetBytes(cmd.y)[0];
			double sweepTime = cmd.pulses * pulseTime;
			byte timefen = (byte)((sweepTime / 65535.0) + 1);
			this.buf[6] = timefen;

			sweepTime /= (double)timefen;

			int sweepTimeInt = 65536 - (int)sweepTime;

			this.buf[7] = BitConverter.GetBytes(sweepTimeInt)[1];
			this.buf[8] = BitConverter.GetBytes(sweepTimeInt)[0];
			this.buf[9] = cmd.power;
			send ();
		}

		public void startMovingRight(byte speed = 50){
			this.move ('w', speed);
		}

		public void startMovingLeft(byte speed = 50){
			this.move ('s', speed);
		}

		public void startMovingDown(byte speed = 50){
			this.move ('a', speed);
		}

		public void startMovingUp(byte speed = 50){
			this.move ('d', speed);
		}

		public void move(char direction, byte speed){
			if (speed == 0)	{
				stopMoving ();
			} else {
				this.buf[1] = (byte)'p';
				this.buf[2] = speed;
				send ();

				Thread.Sleep(5);
				this.buf[1] = (byte)direction;
				send ();
				Thread.Sleep(5);
			}
		}

		public void power(byte strength){
			this.buf[1] = 106;
			this.buf[2] = strength;
			send ();
		}

		private void send(bool force = false){
			if (serialPort.IsOpen && (isOk || force))
			{
				serialPort.Write(this.buf, 0, 11);
				while (serialPort.BytesToWrite > 0){
				}
				this.sendLog("Done writing " + BitConverter.ToString (this.buf));
			}
		}

		public Log sendLog;

		public delegate void Log(string message);

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
			this.sendLog ("Initializing ...");

			this.buf[1] = 108;
			this.buf[2] = 106;
			this.magicNumber = (byte)this.ran.Next(5, 101);
			this.buf[3] = this.magicNumber;
			this.buf[4] = (byte)this.ran.Next(1, 200);
			this.buf[5] = (byte)this.dateTime.Day;

			send (true);

			Thread.Sleep (70);

			if (serialPort.BytesToRead > 0) {
				lastByte = (byte)serialPort.ReadByte ();
				isOk = lastByte == (magicNumber ^ privateMagicNumber);
			}

		}

		~LaserCutter(){
			try{
				serialPort.Close();
			}catch(Exception e){

			}
		}

		public void serialDataEventHandler(object sender, SerialDataReceivedEventArgs evt){
			int len = serialPort.BytesToRead;
			byte[] buffer = new byte[len];
			serialPort.Read (buffer, 0, len);
			this.handleSerialData (buffer);
		}

		private bool isPaused = false;

		public void pause(){
			isPaused = true;
		}

		public void unpause(){
			isPaused = false;
		}

		public void handleSerialData(byte[] data){
			this.sendLog ("Received data: " + BitConverter.ToString (data));
			bool shouldContinue = false;
			foreach (byte single in data) {
				if (single == 153) {
					shouldContinue = true;
				}
				break;
			}

			if(!isPaused && shouldContinue){
				this.sendNextCommand ();
			}
		}

		private Queue<Command> commands = new Queue<Command>();

		private void executeRunCommand(RunCommand command){
			this.doRun (command.x, command.y, command.speed, command.power);
		}

		private void sendNextCommand(){
			if (commands.Count > 0) {
				Command command = commands.Dequeue();
				if (command.GetType () == typeof(RunCommand)) {
					this.executeRunCommand ((RunCommand)command);
				}else if(command.GetType() == typeof(PulseCommand)){
					this.executePulseCommand ((PulseCommand)command);
				}
			}
		}

		public void queueCommands(Command[] commands){
			foreach (Command command in commands) {
				this.commands.Enqueue (command);
			}
			this.sendNextCommand ();
		}


		public int wt;
		int posX = 0;
		int posY = 0;
		public int speed = 4;
		public byte dotSizeInSteps = 24;

		public void setInitialSettings(){

			serialPort.WriteLine("#T00000000");
			send ();
			
			Thread.Sleep(20);

			this.buf[1] = 83;
			this.buf[2] = BitConverter.GetBytes(this.posX)[1];
			this.buf[3] = BitConverter.GetBytes(this.posX)[0];
			this.buf[4] = BitConverter.GetBytes(this.posY)[1];
			this.buf[5] = BitConverter.GetBytes(this.posY)[0];
			send ();
			Thread.Sleep(20);


			this.buf[1] = 89;
			this.buf[2] = (byte)this.speed;
			this.buf[3] = 0;
			send ();
			Thread.Sleep(20);


			this.buf[1] = 111;
			this.buf[2] = this.dotSizeInSteps;
			send ();
			Thread.Sleep(20);


			this.buf[1] = 104;
			this.buf[2] = (byte)this.wt;
			this.buf[3] = 6;
			this.buf[4] = 6;
			this.buf[5] = 5;
			this.buf[6] = 5;
			send ();
			Thread.Sleep(20);
		}

	}


}

