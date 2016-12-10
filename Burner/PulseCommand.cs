using System;

namespace Burner
{
	public class PulseCommand: Command
	{
		public int pulses;
		public PulseCommand (int x, int y, byte power, int pulses): base(x,y,power)
		{
			this.pulses = pulses;
		}
	}
}

