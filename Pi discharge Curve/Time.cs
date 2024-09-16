using System.Diagnostics;

namespace Pi_discharge_Curve
{
	internal class Time
	{
		private static Stopwatch sw = new Stopwatch();

		public static void start()
		{
			sw.Start();
		}

		public static void Reset()
		{
			sw.Reset();
			sw.Start();
		}

		public static float time()
		{
			return (float)sw.Elapsed.Ticks / 10000000;
		}

		public static void stop()
		{
			sw.Stop();
		}
	}
}
