using System;
using System.IO;
namespace Pi_discharge_Curve
{
	public class Logs
	{
		static public void writeLogs(string log)
		{
			try
			{
				string date = DateTime.Today.ToString("dd MMM yyyy");
				string path = System.IO.Path.GetTempPath() + "NeuroEquilibrium\\Pi discharge Curve\\" + date + "\\";
				string file_path = path + "Rotary Chair.logs";
				string time = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				using (StreamWriter sw = new StreamWriter(file_path, true))
				{
					sw.WriteLine(time + "\t" + log + "\n");
					sw.Close();
				}
			}
			catch
			{
			}
		}

		static public void writeLogs(string filePath,string log)
		{
			try
			{
				string date = DateTime.Today.ToString("dd MMM yyyy");
				string path =  date + "\\" + filePath;
				
				string time = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");

				if (!Directory.Exists(date))
				{
					Directory.CreateDirectory(date);
				}

				using (StreamWriter sw = new StreamWriter(path, true))
				{
					sw.WriteLine(time + "\t" + log + "\n");
					sw.Close();
				}
			}
			catch
			{
			}
		}
	}
}
