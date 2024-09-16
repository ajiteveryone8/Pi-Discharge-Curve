using System;
using System.Collections.Generic;
using System.Text;



//////

using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;

using System.Diagnostics;
using System.Linq;
using ZedGraph;
using System.Drawing;
using System.Web.Script.Serialization;


namespace Pi_discharge_Curve
{
    public partial class Form1 : Form
    {
        GraphPane _batteryCurrentPane = new GraphPane();
        GraphPane _batteryVoltagePane = new GraphPane();
		GraphPane _tempPane = new GraphPane();
		PointPairList _batteryCurrentPPL = new PointPairList();
        PointPairList _batteryVoltagePPL = new PointPairList();
		PointPairList _tempPPL = new PointPairList();

		private string _streamURL = "http://192.168.5.102:5000";

		private float _temp;
		private bool _tilt;
		private float _batteryVoltage;
		private float _batteryCurrent;
		private Queue<float> _batteryCurrentQueue = new Queue<float>();
		private Queue<float> _batteryVoltageQueue = new Queue<float>();
		private Queue<float> _tempQueue = new Queue<float>();



		public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MasterPane masterPane = this.zedGraphControl1.MasterPane;
            masterPane.PaneList.Clear();

            _batteryCurrentPane.AddCurve("Battery Current", _batteryCurrentPPL, Color.Red, SymbolType.None);
			_batteryVoltagePane.AddCurve("Battery Voltage", _batteryVoltagePPL, Color.Green, SymbolType.None);
			_tempPane.AddCurve("Temperature", _tempPPL, Color.Black, SymbolType.None);




			masterPane.Add(_tempPane);
			masterPane.Add(_batteryCurrentPane);
            masterPane.Add(_batteryVoltagePane);


            using (Graphics g = CreateGraphics())
            {
                masterPane.SetLayout(g, PaneLayout.SingleColumn);
            }

            refresh_graph();
        }

        public void refresh_graph()
        {
            try
            {
                this.zedGraphControl1.AxisChange();
                this.zedGraphControl1.Invalidate();
            }
            catch (Exception)
            {

            }

        }

		public bool ping_wifi_camera(string url)
		{
			try
			{

				Uri myUri = new Uri(url);
				var ip = Dns.GetHostAddresses(myUri.Host)[0];

				Ping myPing = new Ping();
				PingReply reply = myPing.Send(ip, 1000);
				if (reply.Status == IPStatus.Success)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception e1)
			{
				return false;
			}

		}

		public string web_request(string url)
		{
			try
			{
				if (ping_wifi_camera(_streamURL))
				{
					WebClient client = new WebClient();
					Stream stream = client.OpenRead(url);
					StreamReader reader = new StreamReader(stream);
					string str = "";
					string data = "";
					while ((str = reader.ReadLine()) != null)
					{
						data += str;
					}
					stream.Close();

					return data;
				}
				else
				{
					return null;
				}
			}
			catch (WebException exp)
			{
				//MessageBox.Show(exp.Message, "Exception");
				return null;
			}
		}

		public string web_request_with_timeout(string url, int timeout)
		{
			try
			{
				if (ping_wifi_camera(_streamURL))
				{
					WebRequest request = WebRequest.Create(url);
					request.Timeout = timeout;
					WebClient client = new WebClient();

					WebResponse webResponse = request.GetResponse();
					StreamReader reader = new StreamReader(webResponse.GetResponseStream());
					string str = "";
					str = reader.ReadLine();
					webResponse.Close();

					return str;
				}
				else
				{
					return null;
				}
			}
			catch (WebException exp)
			{
				//MessageBox.Show(exp.Message, "Exception");
				return null;
			}

		}

		public string web_post_request(string url, string data)
		{
			try
			{

				if (ping_wifi_camera(_streamURL))
				{
					WebClient client = new WebClient();
					client.Headers["content-type"] = "application/json";
					byte[] dataArray = Encoding.ASCII.GetBytes(data);
					byte[] responseBytes = client.UploadData(url, "POST", dataArray);
					string responseString = Encoding.UTF8.GetString(responseBytes, 0, responseBytes.Length);
					//Stream data = client.OpenRead(url);
					//StreamReader reader = new StreamReader(data);
					//string str = "";
					//str = reader.ReadLine();
					//data.Close();

					return responseString;
				}
				else
				{
					return null;
				}
			}
			catch (WebException e1)
			{
				Logs.writeLogs(e1.ToString());
				return null;
			}
			catch (Exception e1)
			{
				Logs.writeLogs(e1.ToString());
				return null;
			}

		}

		public string get_device_states(ref float temp, ref bool tilt, ref float batteryVoltage, ref float batteryCurrent)
		{
			string value = "NA";
			try
			{
				string start_url = _streamURL + "/states";
				string response = web_request(start_url);

				if (!string.IsNullOrEmpty(response))
				{
					Logs.writeLogs("Json Data.txt", response);
					Dictionary<string, string> JSONObj = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(response);

					if (JSONObj.ContainsKey("temperature"))
					{
						value = JSONObj["temperature"];
						float.TryParse(value, out temp);
					}

					if (JSONObj.ContainsKey("tilt"))
					{
						bool tempValue = false;
						bool.TryParse(JSONObj["tilt"], out tempValue);

						if (JSONObj["tilt"].Equals("1"))
						{
							tempValue = true;
						}
						else if (JSONObj["tilt"].Equals("0"))
						{
							tempValue = false;
						}

						tilt = tempValue;

						value += $"  - Tilt : {JSONObj["tilt"]}";
					}

					if (JSONObj.ContainsKey("bt_lvl"))
					{
						value = JSONObj["bt_lvl"];
						float.TryParse(value, out batteryVoltage);
					}

					if (JSONObj.ContainsKey("bt_cur"))
					{
						value = JSONObj["bt_cur"];
						float.TryParse(value, out batteryCurrent);
					}

				}
				else
				{
					//MessageBox.Show("Issue in camera stop.");
				}


			}
			catch (Exception e1)
			{
				//Logs.writeLogs(e1.ToString());
			}

			return value;
		}

		public void shutdown_pi()
		{

			try
			{
				string start_url = _streamURL + "/shutdown";
				web_request_with_timeout(start_url, 1000);

			}
			catch (Exception e1)
			{
				Logs.writeLogs(e1.ToString());
			}
		}

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

		private void timer1_Tick(object sender, EventArgs e)
		{
			try
			{

				get_device_states(ref _temp, ref _tilt, ref _batteryVoltage, ref _batteryCurrent);

				if (_batteryCurrentQueue.Count > 30)
				{
					_batteryCurrentQueue.Dequeue();
				}

				if (_batteryVoltageQueue.Count > 30)
				{
					_batteryVoltageQueue.Dequeue();
				}

				if (_tempQueue.Count > 30)
				{
					_tempQueue.Dequeue();
				}

				_batteryCurrentQueue.Enqueue(_batteryCurrent);
				_batteryVoltageQueue.Enqueue(_batteryVoltage);
				_tempQueue.Enqueue(_temp);

				float time = Time.time();

				_batteryCurrentPPL.Add(time,  Math.Round(_batteryCurrentQueue.Average(),2));
				_batteryVoltagePPL.Add(time, Math.Round(_batteryVoltageQueue.Average(), 2));
				_tempPPL.Add(time, Math.Round(_tempQueue.Average(), 2));

				Logs.writeLogs("Current Voltage Data.txt", $"{_batteryCurrent}\t{_batteryVoltage}");

				refresh_graph();
			}
			catch (Exception e1)
			{
				Logs.writeLogs(e1.ToString());
			}
		}

        private void button1_Click(object sender, EventArgs e)
        {
			Time.Reset();
			Time.start();
			timer1.Start();
        }
    }
}
