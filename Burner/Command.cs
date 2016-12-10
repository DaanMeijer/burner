using System;

namespace Burner
{
	public class Command
	{
		public int x;
		public int y;
		public byte power;

		public Command (int x, int y, byte power = 250)
		{
			this.x = x;
			this.y = y;
			this.power = power;
		}

		public Command (int x, int y, int power = 250) : this(x,y,(byte)power)
		{
			
		}
	}
}

