using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO.Ports;

namespace sixwheel
{
	public partial class Form1 : Form
	{

		private List<byte> RxBuffer = new List<byte>(4096);
		private const int redundancy_count = 4;
		private const int length_index = 2;

		List<short>[] speed_back_list = new List<short>[6];
		List<short>[] torque_back_list = new List<short>[6];
		
		private int GridlinesOffset_motor = 0;
		private int framecount;

		System.Timers.Timer updatechart = new System.Timers.Timer(50);
		System.Timers.Timer updatefps = new System.Timers.Timer(1000);


		public Form1()
		{
			InitializeComponent();
			InitSerialPort();
			Init();
		}

		private void InitSerialPort()
		{
			serialport1 = new SerialPort();

			//serialport1.Parity = Parity.None;
			//serialport1.StopBits = StopBits.One;
			//serialport1.DataBits = 8;
			serialport1.DataReceived += Serialport1_DataReceived;
		}

		private void Serialport1_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			int n = serialport1.BytesToRead;
			byte[] buf = new byte[n];
			serialport1.BaseStream.Read(buf, 0, n);
			//1.缓存数据
			RxBuffer.AddRange(buf);

			while (RxBuffer.Count >= redundancy_count) // FE ,FE, type|length, data, check
			{
				//2.1 查找数据头
				if ((RxBuffer[0] == 0xFE) && (RxBuffer[1] == 0xFE)) //传输数据有帧头，用于判断
				{
					int len = RxBuffer[length_index] & 0x1f;	//后5bit代表数据长度
					if (RxBuffer.Count < len + redundancy_count) //数据区尚未接收完整
					{
						break;  //跳出while (RxBuffer.Count >= 4)
					}
					//得到完整的数据，复制到ReceiveBytes中进行校验
					byte[] ReceiveBytes = new byte[len + redundancy_count];
					RxBuffer.CopyTo(0, ReceiveBytes, 0, len + redundancy_count);
					//ReceiveBytes.AddRange()
					//buffer.CopyTo(0, ReceiveBytes.ToArray(), 0, len + 5);
					//ReceiveBytes.AddRange(len + 5);
					if (!receive_check(ReceiveBytes))
					{
						RxBuffer.RemoveRange(0, len + redundancy_count);
						//MessageBox.Show("数据包不正确！");
						continue;
					}
					RxBuffer.RemoveRange(0, len + redundancy_count);

					//Console.WriteLine(BitConverter.ToString(ReceiveBytes));

					try
					{
						this.Invoke((EventHandler)(delegate
						{

							//串口数据处理

							receive_parse(ReceiveBytes);
						}));
					}
					catch (Exception)
					{

						//throw;
					}

				}
				else //帧头不正确时，记得清除
				{
					RxBuffer.RemoveAt(0);
				}
			}
		}

		private void receive_parse(byte[] s)
		{
			nRF_frame C = new nRF_frame();
			byte[] temp = new byte[s[length_index]&0x1f];
			for (int i = 0; i < (s[length_index] & 0x1f); i++)
				temp[i] = s[i + length_index+1];
			C = (nRF_frame)BytesToStruct(temp, C.GetType());

			framecount++;

			Int16[] speed = new Int16[6];
			Int16[] tq = new Int16[6];
			Int32[] power = new Int32[6];
			for (int i = 0; i < 6; i++)
			{
				speed[i] = (short)(C.data[i * 2] * (i > 2 ? -1 : 1)) ;	//	右边的轮子反向
				tq[i] = (short)((C.data[i * 2 + 1]) * (i > 2 ? -1 : 1));
				speed_back_list[i].Add(speed[i]);
				torque_back_list[i].Add(tq[i]);
				power[i] = speed[i] * tq[i];
				label_power[i].Text = (power[i]).ToString();
			}
			//speed_back_list_1.Add(speed[0]);
			//speed_back_list_2.Add(speed[1]);
			//speed_back_list_3.Add(speed[2]);
			//speed_back_list_4.Add(speed[3]);
			//speed_back_list_5.Add(speed[4]);
			//speed_back_list_6.Add(speed[5]);

			//torque_back_list_1.Add(tq[0]);
			//torque_back_list_2.Add(tq[1]);
			//torque_back_list_3.Add(tq[2]);
			//torque_back_list_4.Add(tq[3]);
			//torque_back_list_5.Add(tq[4]);
			//torque_back_list_6.Add(tq[5]);

			label_Lpower.Text = (power[0] + power[1] + power[2]).ToString();
			label_Rpower.Text = (power[3] + power[4] + power[5]).ToString();
			label_Spower.Text = (power[0] + power[1] + power[2] + power[3] + power[4] + power[5]).ToString();
			

			
			
		}

		private bool receive_check(byte[] s)
		{
			int data_count = s[length_index]&0x1f;
			int sum = 0;
			int i = 0;
			for (i = length_index + 1; i < length_index + data_count + 1; i++)
				sum += s[i];
			if ((byte)sum == s[i])
				return true;
			else
				return false;
		}

		public static object BytesToStruct(byte[] bytes, Type type)
		{
			//得到结构体的大小
			int size = Marshal.SizeOf(type);
			//byte数组的长度小于结构的大小，不能完全的初始化结构体
			if (size > bytes.Length)
			{
				//返回空
				return null;
			}

			//分配结构大小的内存空间
			IntPtr structPtr = Marshal.AllocHGlobal(size);
			//将byte数组拷贝到分配好的内存空间
			Marshal.Copy(bytes, 0, structPtr, size);
			//将内存空间转换为目标结构
			object obj = Marshal.PtrToStructure(structPtr, type);
			//释放内存空间
			Marshal.FreeHGlobal(structPtr);
			//返回结构
			return obj;
		}



		string[] wheel_position_tag = new string[] { "LF", "LM", "LR", "RF", "RM", "RR", };

		[StructLayout(LayoutKind.Sequential,Pack =1)]
		public class nRF_frame
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
			public Int16[] data;

			public nRF_frame()
			{
				data = new Int16[12];
			}
		}

		public void Init()
		{
			for (int i = 0; i < 6; i++)
			{
				speed_back_list[i] = new List<short>();
				torque_back_list[i] = new List<short>();
			}


			this.Size = new Size(1300, 900);
			//this.Name = "小车监控";
			this.Text = "小车监控";
			this.FormBorderStyle = FormBorderStyle.Fixed3D;

			updatechart.Elapsed += Updatechart_Elapsed;
			updatechart.Start();

			updatefps.Elapsed += Updatefps_Elapsed;
			updatefps.Start();

			panel1 = new Panel();
			panel1.Padding = new Padding(10, 10, 10, 10);
			panel1.BorderStyle = BorderStyle.FixedSingle;
			panel1.Dock = DockStyle.Fill;

			panel2 = new Panel();
			panel2.Width = 400;
			panel2.Dock = DockStyle.Right;

			chart1 = new Chart();
			chart1.Dock = DockStyle.Fill;
			

			comboBox_serialport = new ComboBox();
			comboBox_serialport.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_serialport.Width = 70;
			comboBox_serialport.Location = new Point(20, 20);
			panel2.Controls.Add(comboBox_serialport);

			button_serial = new Button();
			button_serial.Text = "打开";
			button_serial.Size = new Size(70, 25);
			button_serial.Location = new Point(100, 20-1);
			panel2.Controls.Add(button_serial);
			button_serial.Click += Button_serial_Click;

			label_fps = new Label();
			label_fps.Location = new Point(200, 15);
			label_fps.TextAlign = ContentAlignment.BottomRight;
			label_fps.Text = "0 fps";
			panel2.Controls.Add(label_fps);

			label_power = new Label[7];
			for (int i = 0; i < 6; i++)
			{
				label_power[i] = new Label();
				label_power[i].BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
				label_power[i].TextAlign = ContentAlignment.BottomRight;
				label_power[i].Font = new Font("Arial", 22, FontStyle.Bold);
				label_power[i].Size = new Size(150, 40);
				label_power[i].Location = new Point(30 + i / 3 * 180, 50 + 60 * (i % 3));
				label_power[i].Text = "0";
				panel2.Controls.Add(label_power[i]);
			}

			label_Lpower = new Label();
			label_Lpower.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			label_Lpower.TextAlign = ContentAlignment.BottomRight;
			label_Lpower.Font = new Font("Arial", 22, FontStyle.Bold);
			label_Lpower.Size = new Size(150, 40);
			label_Lpower.Location = new Point(30, 240);
			label_Lpower.Text = "0";
			panel2.Controls.Add(label_Lpower);

			label_Rpower = new Label();
			label_Rpower.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			label_Rpower.TextAlign = ContentAlignment.BottomRight;
			label_Rpower.Font = new Font("Arial", 22, FontStyle.Bold);
			label_Rpower.Size = new Size(150, 40);
			label_Rpower.Location = new Point(210, 240);
			label_Rpower.Text = "0";
			panel2.Controls.Add(label_Rpower);

			label_Spower = new Label();
			label_Spower.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			label_Spower.TextAlign = ContentAlignment.BottomRight;
			label_Spower.Font = new Font("Arial", 22, FontStyle.Bold);
			label_Spower.Size = new Size(200, 40);
			label_Spower.Location = new Point(100, 290);
			label_Spower.Text = "0";
			panel2.Controls.Add(label_Spower);

			ca = new ChartArea[6];
			for (int i = 0; i < 6; i++)
			{
				ca[i] = new ChartArea();
				chart1.ChartAreas.Add(ca[i]);
				//ca[i].re
				ca[i].AxisX.Enabled = AxisEnabled.True;
				ca[i].AxisX.Minimum = 0;
				ca[i].AxisX.Maximum = 500;
				ca[i].AxisX.LabelStyle.Enabled = false;
				ca[i].AxisX.LineWidth = 2;
				ca[i].AxisY.LineWidth = 2;
				ca[i].AxisY.LineColor = Color.Blue;
				ca[i].AxisY.Minimum = -500;
				ca[i].AxisY.Maximum = 500;
				ca[i].AxisY2.Minimum = -500;
				ca[i].AxisY2.Maximum = 500;
				ca[i].AxisY2.LineWidth = 2;
				ca[i].AxisY2.LineColor = Color.Red;
				ca[i].AxisX.MajorGrid.Interval = 100;
				ca[i].AxisX.MajorGrid.LineColor= Color.LightGray;
				ca[i].AxisY.MajorGrid.Enabled = false;
				ca[i].AxisY2.MajorGrid.Enabled = false;

				ca[i].AxisY.Enabled = AxisEnabled.True;
				//ca[i].AxisY.LineColor = Color.Blue;
				ca[i].AxisY2.Enabled = AxisEnabled.True;
				//ca[i].AxisY2.LineColor = Color.Red;

				ca[i].InnerPlotPosition.Auto = false;
				ca[i].InnerPlotPosition.Width = 80;
				ca[i].InnerPlotPosition.Height = 90;
				ca[i].InnerPlotPosition.X = 10;
				ca[i].InnerPlotPosition.Y = 0;

				ca[i].IsSameFontSizeForAllAxes = true;
				ca[i].Name = wheel_position_tag[i];
				Debug.WriteLine(ca[i].Name);
				chart1.Titles.Add(wheel_position_tag[i]);
				chart1.Titles[i].DockedToChartArea = ca[i].Name;
				Debug.WriteLine(ca[i].InnerPlotPosition.X+","+ca[i].InnerPlotPosition.Y);
			}

			ss_s = new Series[6];
			for (int i = 0; i < 6; i++)
			{
				ss_s[i] = new Series();
				ss_s[i].ChartArea = ca[i].Name;
				ss_s[i].Name = "ss_s[" + i + "]";
				ss_s[i].ChartType = SeriesChartType.FastLine;
				ss_s[i].Color = Color.Blue;
				ss_s[i].YAxisType = AxisType.Primary;
				//Debug.WriteLine("ca[" + i  + "]");
				chart1.Series.Add(ss_s[i]);
				for (int j = 0; j < 500; j++)
					chart1.Series[i].Points.AddY(0);
				Debug.WriteLine(chart1.Series[i].Name);
			}

			ss_t = new Series[6];
			for (int i = 0; i < 6; i++)
			{
				ss_t[i] = new Series();
				ss_t[i].ChartArea = ca[i].Name;
				ss_t[i].Name = "ss_t[" + i + "]";
				ss_t[i].ChartType = SeriesChartType.FastLine;
				ss_t[i].Color = Color.Red;
				ss_t[i].YAxisType = AxisType.Secondary;
				//Debug.WriteLine("ca[" + i + "]");
				chart1.Series.Add(ss_t[i]);
				for (int j = 0; j < 500; j++)
					chart1.Series[6 + i].Points.AddY(0);
				Debug.WriteLine(chart1.Series[6+i].Name);
			}

			panel1.Controls.Add(chart1);
			this.Controls.Add(panel1);

			listSerialport();
			
			this.Controls.Add(panel2);
		}

		private void changeYScala(object chart,int i)
		{
			double[] max = { Double.MinValue, Double.MinValue };
			double[] min = { Double.MaxValue, Double.MaxValue };

			Chart tmpChart = (Chart)chart;

			double leftLimit = tmpChart.ChartAreas[i].AxisX.Minimum;
			double rightLimit = tmpChart.ChartAreas[i].AxisX.Maximum;

			foreach (DataPoint dp in tmpChart.Series[i].Points)
			{
				//if (dp.XValue >= leftLimit && dp.XValue <= rightLimit)
				//{
				min[0] = Math.Min(min[0], dp.YValues[0]);
				max[0] = Math.Max(max[0], dp.YValues[0]);
				//}
			}
			tmpChart.ChartAreas[i].AxisY.Maximum = (Math.Ceiling(((max[0]+5) / 10)) * 10);
			tmpChart.ChartAreas[i].AxisY.Minimum = (Math.Floor(((min[0]-5) / 10)) * 10);

			foreach (DataPoint dp in tmpChart.Series[i + 6].Points)
			{
				//if (dp.XValue >= leftLimit && dp.XValue <= rightLimit)
				//{
				min[1] = Math.Min(min[1], dp.YValues[0]);
				max[1] = Math.Max(max[1], dp.YValues[0]);
				//}
			}
			tmpChart.ChartAreas[i].AxisY2.Maximum = (Math.Ceiling(((max[1] + 5) / 10)) * 10);
			tmpChart.ChartAreas[i].AxisY2.Minimum = (Math.Floor(((min[1] - 5) / 10)) * 10);
		}

		private void Updatefps_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			this.Invoke((EventHandler)(delegate
			{
				label_fps.Text = framecount.ToString() + " fps";
			}));
			framecount = 0;
		}

		private void Button_serial_Click(object sender, EventArgs e)
		{
			//根据当前串口对象，来判断操作  
			if (serialport1.IsOpen)
			{
				//timer_DataLog.Stop();
				System.Threading.Thread CloseDown = new System.Threading.Thread(new System.Threading.ThreadStart(CloseSerialOnExit));
				CloseDown.Start();
				comboBox_serialport.Enabled = true;
				button_serial.Text = "打开";
			}
			else
			{
				//关闭时点击，则设置好端口，波特率后打开
				//serialport1.PortName = comboBox_serialport.Text;
				try
				{
					serialport1.PortName = comboBox_serialport.Text;
					serialport1.BaudRate = 115200;
					serialport1.Open();
					comboBox_serialport.Enabled = false;
					serialport1.DiscardInBuffer();
					serialport1.DiscardOutBuffer();

					//SerialRead_timer.Start();
					//timer_DataLog.Start();
					button_serial.Text = "关闭";
				}
				catch (Exception ex)
				{
					//捕获到异常信息，创建一个新的comm对象，之前的不能用了。  
					serialport1 = new SerialPort();
					//现实异常信息给客户。  
					MessageBox.Show(ex.Message);
				}
			}
			//设置按钮的状态  
			//button_serial.Text = serialPort1.IsOpen ? "关闭" : "打开";
		}

		private void CloseSerialOnExit()
		{
			try
			{
				serialport1.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Updatechart_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{

			this.Invoke((EventHandler)(delegate
			{
				while (speed_back_list[0].Count > 0)
				{
					for (int i = 0; i < 6; i++)
					{
						chart1.Series[i].Points.AddY(speed_back_list[i].First());
						chart1.Series[i].Points.RemoveAt(0);
						chart1.Series[i + 6].Points.AddY(torque_back_list[i].First());
						chart1.Series[i + 6].Points.RemoveAt(0);
						speed_back_list[i].RemoveAt(0);
						torque_back_list[i].RemoveAt(0);
						//chart1.ChartAreas[0].AxisX.MajorGrid.IntervalOffset = -GridlinesOffset_motor;
						//GridlinesOffset_motor++;
						//GridlinesOffset_motor %= (int)chart1.ChartAreas[0].AxisX.MajorGrid.Interval;

						changeYScala(chart1,i);
					}

				}
			}));
		}

		private void listSerialport()
		{
			comboBox_serialport.Items.Clear();
			string[] ports = System.IO.Ports.SerialPort.GetPortNames();
			var sortedList = ports.OrderBy(port => Convert.ToInt32(port.Replace("COM", string.Empty)));
			foreach (string port in sortedList)
				comboBox_serialport.Items.Add(port);
			//comboBox_serialport.Items.AddRange(sortedList);
			comboBox_serialport.SelectedIndex = comboBox_serialport.Items.Count > 0 ? 0 : -1;
		}

		private ComboBox comboBox_serialport;
		private Button button_serial;
		private Panel panel1;
		private Panel panel2;
		private Chart chart1;
		private ChartArea[] ca;
		private Series[] ss_s;
		private Series[] ss_t;
		private Label[] label_power;
		private Label label_Lpower;
		private Label label_Rpower;
		private Label label_Spower;
		private SerialPort serialport1;
		private Label label_fps;
	}
}
