using System;

namespace Burner
{
	public class RunCommand: Command
	{
		public int speed;
		public RunCommand (int x, int y, int power, int speed = 65136) : base(x,y,power)
		{
			this.speed = speed;
		}

	}
}

