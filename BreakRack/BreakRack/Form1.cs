using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
using System.Timers;
using System.Threading;
using System.Collections;
using Microsoft.Win32;         //For System Events

namespace BreakRack
{

	public partial class Form1 : Form
	{
		private StringBuilder builder = new StringBuilder();//避免在事件处理方法中反复的创建，定义到外面。  
		private long received_count = 0;//接收计数  
		private long received_count_last = 0;
		private List<byte> RxBuffer = new List<byte>(4096);
		private List<byte> TxBuffer = new List<byte>(4096);
		private uint CAN_ID;

		s_1x CAN_1x = new s_1x();

		s_BrakeUnit CAN_brake = new s_BrakeUnit();
		private int GridlinesOffset_motor = 0;
		private int GridlinesOffset_Oil = 0;
		//private int scheduledrunning = 0;

		private static System.Timers.Timer aTimer;
		private static System.Timers.Timer timer_DataLog;
		private static System.Timers.Timer WatchDogTimer;
		private static System.Timers.Timer SerialRead_timer;
		private static System.Timers.Timer OilFlowRate_timer;
		private static System.Timers.Timer OilFlowRate2_timer;
		private static System.Timers.Timer scheduler_timer;
		//private static System.Timers.Timer FrameTimer;
		private int secondcounter;
		private long framespeed_last_ms = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
		private bool speedupDone, speeddownDone, OilPreIncDone, OilPreDecDone;
		private int testingstage = 1;
		private int schedulecounter = 0;
		private bool isInTimer = false;
		private bool ScheduleRunning;
		private int accele_step;

		//SerialPort serialPort1 = new SerialPort();

		//byte[] ReceiveBytes = new byte[0x300];
		List<int> speed_list = new List<int>();
		List<int> torque_list = new List<int>();
		List<double> master_pressure_list = new List<double>();
		List<double> wheel_pressure_list = new List<double>();

		private short brakeinteval;
		double master_OilPre_back = 0;
		double wheel_OilPre_back = 0;
		int speedback = 0;
		int torqueback = 0;
		private short targetspeed = 0;
		bool EnableMotor_flag = false;
		//private short brakeinteval = 0;

		//禁止程序休眠
		[FlagsAttribute()]
		public enum EXECUTION_STATE : uint //Determine Monitor State
		{
			ES_AWAYMODE_REQUIRED = 0x40,
			ES_CONTINUOUS = 0x80000000u,
			ES_DISPLAY_REQUIRED = 0x2,
			ES_SYSTEM_REQUIRED = 0x1
			// Legacy flag, should not be used.
			// ES_USER_PRESENT = 0x00000004
		}

		//Enables an application to inform the system that it is in use, thereby preventing the system from entering sleep or turning off the display while the application is running.
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

		//This function queries or sets system-wide parameters, and updates the user profile during the process.
		[DllImport("user32", EntryPoint = "SystemParametersInfo", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

		private const Int32 SPI_SETSCREENSAVETIMEOUT = 15;
		//禁止程序休眠

		//private volatile bool is_serial_listening = false;//串口正在监听标记
		//private volatile bool is_serial_closing = false;//串口正在关闭标记
		//private Thread CloseDown;

		//MultimediaTimer timer_100ms = new MultimediaTimer() { Interval = 1000 };

		//s_1x CAN_1x = new s_1x
		//{
		//	Speed = 0,
		//	Torque = 0,
		//	ModeDem = 0,
		//	HVPowerDem = 0,
		//	DischargeDem = 1,
		//	counter = 0,
		//	CheckValue = 0
		//};

		public Form1()
		{
			InitializeComponent();
			FormInit();
			comboBox_serialport.Focus();

			//aTimer.Start();
			WatchDogTimer.Start();

			SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS); //Do not Go To Sleep

			this.SetStyle(      //双缓冲
			  ControlStyles.AllPaintingInWmPaint |
			  ControlStyles.UserPaint |
			  ControlStyles.DoubleBuffer, true);
		}


		public void FormInit()
		{
			listSerialport();

			//部分textbox初始化
			comboBox_frametype.Items.Add("标准帧");
			comboBox_frametype.Items.Add("扩展帧");
			comboBox_frametype.SelectedIndex = 0;

			textBox_targetspeed.Text = "0";
			textBox_targettorque.Text = "0";

			textBox_CANID.Text = "11";
			textBox_DCDCID.Text = "52C";
			textBox_BrkPedl.Text = "100";

			textBox_pulse_1.Text = "0";
			textBox_pulse_2.Text = "0";
			textBox_wheel_pressure.Text = "0";
			textBox_speedback.Text = "0";
			textBox_torqueback.Text = "0";
			textBox_motortemp.Text = "0";
			textBox_coldOiltemp.Text = "0";
			textBox_wheel_pressure.Text = "0";
			textBox_totalCounter.Text = "5";
			textBox_brakeinteval.Text = "60";
			textBox_OilPumpDutycycle.Text = "15";
			textBox_OilPumpDutycycle2.Text = "60";

			if (textBox_CANID.Text != "")
				CAN_ID = Convert.ToUInt32(textBox_CANID.Text, 16);
			else
				CAN_ID = 0;

			//定时器
			//aTimer = new System.Timers.Timer(100);
			//// Hook up the Elapsed event for the timer. 
			//aTimer.Elapsed += OnTimedEvent;

			//scheduler_timer = new System.Timers.Timer(5000);
			//scheduler_timer.Elapsed += Scheduler_timer_Elapsed;

			SerialRead_timer = new System.Timers.Timer(30);
			SerialRead_timer.Elapsed += serialReadEvent;

			timer_DataLog = new System.Timers.Timer(1000);
			timer_DataLog.Elapsed += DataLog_1s;

			WatchDogTimer = new System.Timers.Timer(100);
			WatchDogTimer.Elapsed += WatchDogTimedEvent;

			//OilFlowRate_timer = new System.Timers.Timer(5000);
			//OilFlowRate_timer.Elapsed += OilFlowRate_timerEvent;

			//OilFlowRate2_timer = new System.Timers.Timer(30000);
			//OilFlowRate2_timer.Elapsed += OilFlowRate2_timerEvent;



			//绘图
			Color axisColor = Color.FromArgb(100, 100, 100);
			Color gridColor = Color.FromArgb(200, 200, 200);
			Color backColor = Color.FromArgb(246, 246, 246);
			Color lineColor = Color.FromArgb(50, 50, 200);
			Color speedcolor = Color.Blue;
			Color torquecolor = Color.Red;



			//图框1
			chart1.ChartAreas[0].BackColor = backColor;
			//chart1.ChartAreas[0].BorderWidth = 1;
			//chart1.ChartAreas[0].BorderColor = axisColor;
			chart1.ChartAreas[0].BorderDashStyle =
				System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
			//X轴
			chart1.ChartAreas[0].AxisX.Minimum = 0;
			chart1.ChartAreas[0].AxisX.Maximum = 1000;
			chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 100;
			//chart1.ChartAreas[0].AxisX.LineColor = axisColor;
			chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = gridColor;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
			chart1.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
			//chart1.ChartAreas[0].AxisX.Title = "1s/格";
			//Y1轴
			chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = gridColor;
			chart1.ChartAreas[0].AxisY.MajorGrid.Interval = 2000;
			chart1.ChartAreas[0].AxisY.Minimum = 0;
			chart1.ChartAreas[0].AxisY.Maximum = 13000;
			chart1.ChartAreas[0].AxisY.LineColor = Color.Blue;
			chart1.ChartAreas[0].AxisY.LineWidth = 2;
			chart1.ChartAreas[0].AxisY.Title = "转速/rpm";
			chart1.ChartAreas[0].AxisY.ArrowStyle = AxisArrowStyle.Triangle;
			//chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = true;
			//chart1.ChartAreas[0].AxisY.MajorTickMark.Enabled = true;
			//Y2轴
			chart1.ChartAreas[0].AxisY2.Title = "扭矩/Nm";
			chart1.ChartAreas[0].AxisY2.LineColor = Color.Red;
			chart1.ChartAreas[0].AxisY2.LineWidth = 2;
			chart1.ChartAreas[0].AxisY2.Minimum = 0;
			chart1.ChartAreas[0].AxisY2.Maximum = 130;
			chart1.ChartAreas[0].AxisY2.MajorGrid.Interval = 200;
			chart1.ChartAreas[0].AxisY2.ArrowStyle = AxisArrowStyle.Triangle;

			//曲线1
			chart1.Series[0].Color = speedcolor;
			chart1.Series[0].ChartType = SeriesChartType.FastLine;
			chart1.Series[0].BorderWidth = 1;
			//曲线2
			chart1.Series[1].Color = torquecolor;
			chart1.Series[1].ChartType = SeriesChartType.FastLine;
			chart1.Series[1].BorderWidth = 1;

			for (int i = 0; i < 1000; i++)
			{
				chart1.Series["Series1"].Points.AddY(0);
				chart1.Series["Series2"].Points.AddY(0);
			}

			///*************************************************///
			//图框2
			chart1.ChartAreas[1].BackColor = backColor;
			chart1.ChartAreas[1].BorderWidth = 1;
			chart1.ChartAreas[1].BorderColor = axisColor;
			chart1.ChartAreas[1].BorderDashStyle =
				System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
			//X轴
			chart1.ChartAreas[1].AxisX.Minimum = 0;
			chart1.ChartAreas[1].AxisX.Maximum = 200;
			chart1.ChartAreas[1].AxisX.MajorGrid.Interval = 20;
			chart1.ChartAreas[1].AxisX.LineColor = axisColor;
			chart1.ChartAreas[1].AxisX.MajorGrid.LineColor = gridColor;
			chart1.ChartAreas[1].AxisX.LabelStyle.Enabled = false;
			chart1.ChartAreas[1].AxisX.MajorTickMark.Enabled = false;
			//Y1轴
			chart1.ChartAreas[1].AxisY.MajorGrid.LineColor = gridColor;
			chart1.ChartAreas[1].AxisY.MajorGrid.Interval = 0.5;
			chart1.ChartAreas[1].AxisY.Minimum = 0;
			chart1.ChartAreas[1].AxisY.Maximum = 5;
			//chart1.ChartAreas[1].AxisY.LineColor = Color.Blue;
			chart1.ChartAreas[1].AxisY.LineWidth = 2;
			chart1.ChartAreas[1].AxisY.Title = "压力/Mpa";
			chart1.ChartAreas[1].AxisY.LabelStyle.Enabled = true;
			chart1.ChartAreas[1].AxisY.MajorTickMark.Enabled = true;
			chart1.ChartAreas[1].AxisY.ArrowStyle = AxisArrowStyle.Triangle;
			//Y2轴
			//chart1.ChartAreas[1].AxisY2.Title = "扭矩/Nm";
			//chart1.ChartAreas[1].AxisY2.LineColor = Color.Red;
			//chart1.ChartAreas[1].AxisY2.LineWidth = 2;
			//chart1.ChartAreas[1].AxisY2.Minimum = 0;
			//chart1.ChartAreas[1].AxisY2.Maximum = 130;
			//chart1.ChartAreas[1].AxisY2.MajorGrid.Interval = 200;

			//曲线1
			chart1.Series[2].Color = Color.Blue;
			chart1.Series[2].ChartType = SeriesChartType.FastLine;
			chart1.Series[2].BorderWidth = 1;
			//曲线2
			chart1.Series[3].Color = Color.Red;
			chart1.Series[3].ChartType = SeriesChartType.FastLine;
			chart1.Series[3].BorderWidth = 1;

			for (int i = 0; i < 200; i++)
			{
				chart1.Series["Series3"].Points.AddY(0);
				chart1.Series["Series4"].Points.AddY(0);
			}
			//timer_100ms.Elapsed += (o, e) => timerElapsed();
			//timer_100ms.Start();
			listView1.Columns.Add("ID(Hex)", 0, HorizontalAlignment.Center);
			listView1.Columns.Add("ID(Hex)", listView1.Width / 8, HorizontalAlignment.Right);
			listView1.Columns.Add("Data", listView1.Width / 8 * 7 - 18, HorizontalAlignment.Center);
			listView1.ListViewItemSorter = null;
		}

		private void Scheduler_timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Invoke(new MethodInvoker(() => { button_setOilPumpCycle2.PerformClick(); }));
			Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	开始大流量冷却\n", DateTime.Now.ToLongTimeString().ToString())); }));
			Thread.Sleep(5000);
			Invoke(new MethodInvoker(() => { button_BrkPedl.PerformClick(); }));
			Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	开始制动\n", DateTime.Now.ToLongTimeString().ToString())); }));
			Thread.Sleep(1000);
			Invoke(new MethodInvoker(() => { button_OilDrainage.PerformClick(); }));
			Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	解除制动\n", DateTime.Now.ToLongTimeString().ToString())); }));

			Thread.Sleep(2000);
			if (schedulecounter > 1)
			{
				while (wheel_OilPre_back >= 0.05)
				{
					Invoke(new MethodInvoker(() => { button_OilDrainage.PerformClick(); }));
					Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	解除制动\n", DateTime.Now.ToLongTimeString().ToString())); }));
					Thread.Sleep(2000);
				}
				if (wheel_OilPre_back < 0.05)    //油压解除
				{
					Invoke(new MethodInvoker(() => { button_enableDem.PerformClick(); }));
					Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	使能轮毂电机\n", DateTime.Now.ToLongTimeString().ToString())); }));
					Invoke(new MethodInvoker(() => { accele_step = Convert.ToInt16(textBox_targetspeed.Text) / 100;}));

					int step_count = 0;
					while (accele_step > 0)	//阶梯加速
					{
						accele_step--;
						step_count++;
						
						try
						{
							CAN_1x.Speed = (short)(100* step_count);
							Invoke(new MethodInvoker(() => {
								button_OilDrainage.PerformClick();
								datatosend(CAN_1x_packed(), 1);
								richTextBox2.AppendText(String.Format(" {0}	设定转速{1}\n", DateTime.Now.ToLongTimeString().ToString(),CAN_1x.Speed));
							}));
						}
						catch (Exception)
						{
							throw;
						}
						Thread.Sleep(100);
					}
					Invoke(new MethodInvoker(() => { button_setspeed.PerformClick(); }));
					Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	设置转速\n", DateTime.Now.ToLongTimeString().ToString())); }));
					schedulecounter--;
				}
				else
				{
					Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	制动压力过高，任务停止\n", DateTime.Now.ToLongTimeString().ToString())); }));
					scheduler_timer.Stop();
					ScheduleRunning = false;
				}
			}
			else
			{
				scheduler_timer.Stop();
				ScheduleRunning = false;
				schedulecounter--;
				Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	计划任务完成\n", DateTime.Now.ToLongTimeString().ToString())); }));
			}
			Invoke(new MethodInvoker(() => textBox_executedCounter.Text = (Convert.ToInt16(textBox_totalCounter.Text) - schedulecounter).ToString()));
			Invoke(new MethodInvoker(() => { button_scheduled.Text = (ScheduleRunning == false) ? "开始" : "停止"; }));
			int delay = 10;
			try { delay=Convert.ToInt16(textBox_Oil2Delay.Text); } catch { }
			if (delay <= 2) delay = 2;
			delay = delay * 1000 - 2000;
			Thread.Sleep(delay);
			Invoke(new MethodInvoker(() => { button_setOilPumpCycle.PerformClick(); }));
			Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	设置小流量冷却\n", DateTime.Now.ToLongTimeString().ToString())); }));

		}

		//private void OilFlowRate2_timerEvent(object sender, ElapsedEventArgs e)
		//{
		//	Invoke(new MethodInvoker(() => { button_setOilPumpCycle.PerformClick(); }));
		//	Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	设置小流量冷却\n", DateTime.Now.ToLongTimeString().ToString())); }));
		//	OilFlowRate2_timer.Stop();
		//}

		//private void OilFlowRate_timerEvent(object sender, ElapsedEventArgs e)
		//{
		//	//Invoke(new MethodInvoker(() => { button_setOilPumpCycle.PerformClick(); }));
		//	//OilFlowRate2_timer.Start();
		//	Invoke(new MethodInvoker(() => { button_BrkPedl.PerformClick(); }));
		//	Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	开始制动\n", DateTime.Now.ToLongTimeString().ToString())); }));
		//	testingstage = 2;
		//	secondcounter = 0;
		//	OilFlowRate_timer.Stop();
		//}

		private void serialReadEvent(object sender, ElapsedEventArgs e)
		{

			//更新转速和扭矩曲线
			//int n = speed_queue.Count;
			//int[] a = new int[n];
			//int[] b = new int[n];
			//for (int i = 0; i < n; i++)
			//{
			//	a[i] = (int)speed_queue.Dequeue();
			//	b[i] = (int)torque_queue.Dequeue();
			//}
			this.Invoke((EventHandler)(delegate
			{
				while(speed_list.Count>0)
				{
						chart1.Series["Series1"].Points.AddY(speed_list.First());
						chart1.Series["Series1"].Points.RemoveAt(0);
						chart1.Series["Series2"].Points.AddY(torque_list.First());
						chart1.Series["Series2"].Points.RemoveAt(0);

					speed_list.RemoveAt(0);
					torque_list.RemoveAt(0);
					chart1.ChartAreas[0].AxisX.MajorGrid.IntervalOffset = -GridlinesOffset_motor;
					GridlinesOffset_motor++;
					//GridlinesOffset_motor= GridlinesOffset_motor+10;
					GridlinesOffset_motor %= (int)chart1.ChartAreas[0].AxisX.MajorGrid.Interval;
				}

				while (master_pressure_list.Count > 0)
				{
					chart1.Series["Series3"].Points.AddY(master_pressure_list.First());
					chart1.Series["Series3"].Points.RemoveAt(0);
					chart1.Series["Series4"].Points.AddY(wheel_pressure_list.First());
					chart1.Series["Series4"].Points.RemoveAt(0);
					// Make gridlines move.
					master_pressure_list.RemoveAt(0);
					wheel_pressure_list.RemoveAt(0);
					chart1.ChartAreas[1].AxisX.MajorGrid.IntervalOffset = -GridlinesOffset_Oil;
					GridlinesOffset_Oil++;
					GridlinesOffset_Oil %= (int)chart1.ChartAreas[1].AxisX.MajorGrid.Interval;
				}

			}));


		}

		private void DataLog_1s(object sender, ElapsedEventArgs e)
		{
			//StringBuilder str = new StringBuilder();
			//for (int i = 0; i < C.DLC; i++)
			//	str.Append(String.Format("{0:X2} ", C.Data[i]));
			////Console.WriteLine("2   "+str);
			this.BeginInvoke((EventHandler)(delegate
			{
				richTextBox3.AppendText(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\n", DateTime.Now.ToLongTimeString().ToString(), textBox_speedback.Text.Replace("rpm", ""), textBox_torqueback.Text.Replace("Nm", ""), textBox_motortemp.Text.Replace("°C", ""), textBox_coldOiltemp.Text.Replace("°C", ""), textBox_wheel_pressure.Text.Replace("MPa", ""), label_OilDutyCycleBack.Text.Replace("%", "")));
				label58.Text = DateTime.Now.ToLongTimeString();
				//richTextBox3.AppendText(String.Format("{0,10}\t{1,8}\t{2,8}\t{3,8}\t{4,8}\t{5,8}\n", DateTime.Now.ToLongTimeString().ToString(), textBox_speedback.Text,textBox_targetspeed.Text,textBox_motortemp.Text,textBox_coldOiltemp.Text,textBox_wheel_pressure.Text));
				//richTextBox3.AppendText("1\n");
			}));
		}

		private void WatchDogTimedEvent(object sender, ElapsedEventArgs e)
		{
			byte[] s = new byte[] { 1, 2 };
			datatosend(s, 3);
		}

		private void listSerialport()
		{
			//串口相关
			//string[] ports = SerialPort.GetPortNames();
			//Array.Sort(ports);
			comboBox_serialport.Items.Clear();
			string[] ports = System.IO.Ports.SerialPort.GetPortNames();
			var sortedList = ports.OrderBy(port => Convert.ToInt32(port.Replace("COM", string.Empty)));
			foreach (string port in sortedList)
			{
				comboBox_serialport.Items.Add(port);
			}
			//comboBox_serialport.Items.AddRange(sortedList);
			comboBox_serialport.SelectedIndex = comboBox_serialport.Items.Count > 0 ? 0 : -1;
		}

		private bool receive_check(byte[] s)
		{
			int temp = 0;
			for (int i = 0; i < s[3] + 4; i++)
				temp += s[i];
			if (s[s[3] + 4] == (byte)temp) //+5-1
				return true;
			else
				return false;
		}

		private void ListViewUpdate(CAN_obj temp)
		{
			listView1.BeginUpdate();
			ListViewItem item = new ListViewItem();
			StringBuilder s = new StringBuilder(); ;
			for (int i = 0; i < temp.DLC; i++)
				String.Format("{0:X} ", temp.Data[i]);
			item.Text = "";
			item.SubItems.Add(String.Format("{0:X8}", temp.ID));
			item.SubItems.Add(s.ToString());
			listView1.Items.Add(item);
			listView1.EnsureVisible(listView1.Items.Count - 1);
			listView1.EndUpdate();
		}


		private void receive_parse(byte[] s)
		{
			CAN_obj C = new CAN_obj();
			byte[] temp = new byte[s[3]];
			for (int i = 0; i < s[3]; i++)
				temp[i] = s[i + 4];
			C = (CAN_obj)BytesToStruct(temp, C.GetType());
			//
			if (checkBox1.Checked == true)
			{
				StringBuilder str = new StringBuilder();
				for (int i = 0; i < C.DLC; i++)
					str.Append(String.Format("{0:X2} ", C.Data[i]));
				richTextBox1.AppendText(String.Format("   {0,8:X}   {1}\n", C.ID, str));
			}

			//接收计数
			received_count++;
			label_frameCount.Text = received_count.ToString();

			if (received_count % 20000 == 0)
			{
				richTextBox1.Clear();
			}
			//计算帧率
			long current_ms = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
			if (current_ms - framespeed_last_ms > 1000)
			{
				label_framespeed.Text = String.Format("{0:f1} f/s", (float)(received_count - received_count_last) / ((current_ms - framespeed_last_ms) / 1000.0));
				received_count_last = received_count;
				framespeed_last_ms = current_ms;
			}
			if (s[2] == 0xAA)
			{


				//

				//int linelength = String.Format("   {0,8:X}   {1}\n", C.ID, str).Length;
				//if (richTextBox1.TextLength > linelength * 200)
				//	richTextBox1.Text = richTextBox1.Text.Remove(0, linelength);



				//while (richTextBox1.Lines.Length > 10000)
				//{
				//	richTextBox1.SelectionStart = 0;
				//	richTextBox1.SelectionLength = richTextBox1.Text.IndexOf("\n", 1000) + 1;  //删除richtextbox最前一行
				//	richTextBox1.SelectedText = "";
				//}

				//this.Invoke((MethodInvoker)delegate
				//{
				//	richTextBox1.ScrollToCaret();
				//});
				if (C.ID == 0x320)
				{
					master_OilPre_back = (((ushort)((C.Data[3] << 8) + C.Data[2])) & 0x3ff) / 113f;
					wheel_OilPre_back = (((ushort)((C.Data[5] << 8) + C.Data[4])) & 0x3ff) / 113f;
					textBox_master_pressure.Text = String.Format("{0:f2}MPa", master_OilPre_back);
					textBox_wheel_pressure.Text = String.Format("{0:f2}MPa", wheel_OilPre_back);

					master_pressure_list.Add(master_OilPre_back);
					wheel_pressure_list.Add(wheel_OilPre_back);
					/*
					chart1.Series["Series3"].Points.AddY(master);
					chart1.Series["Series3"].Points.RemoveAt(0);
					chart1.Series["Series4"].Points.AddY(wheel);
					chart1.Series["Series4"].Points.RemoveAt(0);
					// Make gridlines move.
					chart1.ChartAreas[1].AxisX.MajorGrid.IntervalOffset = -GridlinesOffset_Oil;
					GridlinesOffset_Oil++;
					GridlinesOffset_motor %= (int)chart1.ChartAreas[1].AxisX.MajorGrid.Interval;
					*/
				}
				else if (C.ID == 0x302)
				{
					label_OilDutyCycleBack.Text= String.Format("{0}%", C.Data[0]);
				}
				else if ((C.ID & 0xFFFFFFF0) == 0x520)
				{
					if (((C.Data[0]) & 0x1) == 0)
						label_OT.ForeColor = Color.Lime;
					else
						label_OT.ForeColor = Color.Red;

					if (((C.Data[0] >> 1) & 0x1) == 0)
						label_OutOC.ForeColor = Color.Lime;
					else
						label_OutOC.ForeColor = Color.Red;

					if (((C.Data[0] >> 2) & 0x1) == 0)
						label_OutOV.ForeColor = Color.Lime;
					else
						label_OutOV.ForeColor = Color.Red;

					if (((C.Data[0] >> 3) & 0x1) == 0)
						label_OutLV.ForeColor = Color.Lime;
					else
						label_OutLV.ForeColor = Color.Red;

					if (((C.Data[0] >> 4) & 0x1) == 0)
						label_InOV.ForeColor = Color.Lime;
					else
						label_InOV.ForeColor = Color.Red;

					if (((C.Data[0] >> 5) & 0x1) == 0)
						label_InLV.ForeColor = Color.Lime;
					else
						label_InLV.ForeColor = Color.Red;

					if (((C.Data[0] >> 6) & 0x1) == 0)
						label_OutSC.ForeColor = Color.Lime;
					else
						label_OutSC.ForeColor = Color.Red;

					if (((C.Data[0] >> 7) & 0x1) == 0)
						label_FE.ForeColor = Color.Lime;
					else
						label_FE.ForeColor = Color.Red;

					if (((C.Data[1]) & 0x1) == 0)
						label_COT.ForeColor = Color.Lime;
					else
						label_COT.ForeColor = Color.Red;

					if (((C.Data[1] >> 1) & 0x1) == 0)
						label_ST.ForeColor = Color.Lime;
					else
						label_ST.ForeColor = Color.Red;

					switch ((C.ID >> 2) & 0x3)
					{
						case 0:
							textBox_DCState.Text = "初始化";
							break;
						case 1:
							textBox_DCState.Text = "运行";
							break;
						case 2:
							textBox_DCState.Text = "故障";
							break;
						default:
							break;
					}

					textBox_DCOutC.Text = String.Format("{0:f1}A", (C.Data[2] + ((C.Data[3] & 0xF) << 8)) * 0.1);
					textBox_DCOutV.Text = String.Format("{0:f1}V", (C.Data[4] * 0.2));
					textBox_DCTemp.Text = String.Format("{0}°C", (ushort)C.Data[5] - 50);
					textBox_DCInV.Text = String.Format("{0}V", (C.Data[6] << 1));
				}

				if ((C.ID & 0xF) != ((Convert.ToUInt32(textBox_CANID.Text, 16) & 0xF)))
					return;
				if ((C.ID & 0xFFFFFF80) == 0)
				{
					switch ((C.ID >> 4) & 0x7)
					{
						case 2:
							speedback = (short)((ushort)((C.Data[1] << 8) + C.Data[0]) - 32768);
							torqueback = (short)((ushort)((C.Data[3] & 0x3) << 8) + C.Data[2] - 512);
							textBox_speedback.Text = String.Format("{0}rpm", speedback);
							textBox_torqueback.Text = String.Format("{0}Nm", torqueback);
							textBox_MaxPosTorque.Text = String.Format("{0}Nm", (ushort)(((C.Data[4] & 0xF) << 6) + (C.Data[3] >> 2)));
							textBox_MaxNegTorque.Text = String.Format("{0}Nm", (ushort)(((C.Data[5] << 2) + (C.Data[4] >> 6)) & 0x3ff));
							textBox_MaxSpeedAtCurTor.Text = String.Format("{0}rpm", (ushort)((ushort)((C.Data[7] << 8) + C.Data[6]) - 32768));


							//BeginInvoke((MethodInvoker)delegate ()
							//{
							speed_list.Add(speedback);
							torque_list.Add(torqueback);
							/*
							//更新转速和扭矩曲线
							chart1.Series["Series1"].Points.AddY((short)((C.Data[1] << 8) + C.Data[0] - 32768));
							chart1.Series["Series1"].Points.RemoveAt(0);
							chart1.Series["Series2"].Points.AddY((short)(((C.Data[3] & 0x3) << 8) + C.Data[2] - 512));
							chart1.Series["Series2"].Points.RemoveAt(0);
							// Make gridlines move.
							chart1.ChartAreas[0].AxisX.MajorGrid.IntervalOffset = -GridlinesOffset_motor;
							GridlinesOffset_motor++;
							//GridlinesOffset_motor= GridlinesOffset_motor+10;
							GridlinesOffset_motor %= (int)chart1.ChartAreas[0].AxisX.MajorGrid.Interval;
							*/
					//});

					break;
						case 3:
							if ((C.Data[0] & 0x1) == 1)
								label_InitDone.ForeColor = Color.Lime;
							else
								label_InitDone.ForeColor = Color.Red;

							if (((C.Data[0] >> 1) & 0x1) == 1)
								label_HVOK.ForeColor = Color.Lime;
							else
								label_HVOK.ForeColor = Color.Red;

							if (((C.Data[0] >> 2) & 0x1) == 1)
							{
								label_motorenable.ForeColor = Color.Lime;
								EnableMotor_flag = true;
							}
							else
							{
								label_motorenable.ForeColor = Color.Red;
								EnableMotor_flag = false;
							}

							if (((C.Data[0] >> 3) & 0x1) == 1)
								label_HVReady.ForeColor = Color.Lime;
							else
								label_HVReady.ForeColor = Color.Red;

							if ((C.Data[1] & 0x1) == 1)
								label_speedvalid.ForeColor = Color.Lime;
							else
								label_speedvalid.ForeColor = Color.Red;

							if (((C.Data[1] >> 1) & 0x1) == 1)
								label_torquevalid.ForeColor = Color.Lime;
							else
								label_torquevalid.ForeColor = Color.Red;

							switch ((C.Data[0] >> 4) & 0x3)
							{
								case 0:
									textBox_motorstate.Text = "待机";
									break;
								case 1:
									textBox_motorstate.Text = "工作";
									break;
								case 2:
									textBox_motorstate.Text = "故障";
									break;
								case 3:
									textBox_motorstate.Text = "降功率";
									break;
								default:
									break;
							}
							switch (C.Data[0] >> 6)
							{
								case 0:
									textBox_motormode.Text = "初始化";
									break;
								case 1:
									textBox_motormode.Text = "扭矩控制";
									break;
								case 2:
									textBox_motormode.Text = "转速控制";
									break;
								case 3:
									textBox_motormode.Text = "快速放电";
									break;
								default:
									break;
							}
							textBox_faultlevel.Text = String.Format("{0}", (ushort)((C.Data[1] >> 2) & 0x3));
							break;
						case 4:
							textBox_IdCurrent.Text = String.Format("{0}A", (ushort)(((C.Data[1] & 0x3) << 8) + C.Data[0]) - 512);
							textBox_IqCurrent.Text = String.Format("{0}A", (ushort)(((C.Data[2] & 0x3F) << 4) + (C.Data[1] >> 4)) - 512);
							textBox_BusCurrent.Text = String.Format("{0}A", (ushort)((C.Data[3] << 2) + (C.Data[2] >> 6)) - 512);
							textBox_BusVoltage.Text = String.Format("{0}V", (ushort)(((C.Data[5] & 0x1) << 8) + C.Data[4]));
							textBox_PhaseCurrent.Text = String.Format("{0}A", (ushort)(((C.Data[6] & 0xF) << 6) + (C.Data[5] >> 2)) - 512);
							break;
						case 5:
							textBox_motortemp.Text = String.Format("{0}°C", (ushort)(C.Data[0]) - 50);
							textBox_controllertemp.Text = String.Format("{0}°C", (ushort)(C.Data[1]) - 50);
							textBox_IGBTtemp.Text = String.Format("{0}°C", (ushort)(C.Data[2]) - 50);
							//textBox_coldOiltemp.Text = String.Format("{0}°C", (ushort)(C.Data[3]) - 50, "°C");
							textBox_FirmwareVersion.Text = String.Format("V{0}.{1}", (ushort)(C.Data[5]), (ushort)(C.Data[4]));
							break;
						case 6:
							textBox_faultcode1.Text = String.Format("0x{0:X}", (ushort)((C.Data[1] << 8) + C.Data[0]));
							textBox_faultcode2.Text = String.Format("0x{0:X}", (ushort)((C.Data[3] << 8) + C.Data[2]));
							textBox_faultcode3.Text = String.Format("0x{0:X}", (ushort)((C.Data[5] << 8) + C.Data[4]));
							textBox_faultcode4.Text = String.Format("0x{0:X}", (ushort)((C.Data[7] << 8) + C.Data[6]));
							break;
						default:
							break;
					}


				}

			}
			else if (s[2] == 0xBB)
			{
				double ad = (C.Data[0] + (C.Data[1] << 8)) * 1.943 / 1.922;
				int full = 4028;
				double current = 0.001243;
				//double current = 0.000961;
				double v = 3.28 * (full - ad) / full;
				double resistence = v / current;

				double temperature = resistence * 0.2616 - 261.7580;
				textBox_coldOiltemp.Text = String.Format("{0:f0}°C", temperature);

				//textBox_coldOiltemp.Text = String.Format("{0}°C", C.Data[0] + (C.Data[1] << 8));
				//richTextBox1.AppendText(String.Format("   {0}\t{1}\n", C.Data[0] + (C.Data[1] << 8),C.Data[2] + (C.Data[3] << 8)));

				//textBox_motortemp.Text = String.Format("{0:f3}V", v);
				//textBox_controllertemp.Text = String.Format("{0:f1}欧姆", resistence);


				//double ad = (C.Data[0] + (C.Data[1] << 8));
				//int full = 4028;
				////double current=0.001359;
				//double current = 0.000984;
				//double v = 3.28 * (full - ad) / full;
				//double resistence = v / current;

				//double temperature = resistence * 0.2616 - 261.7580;
				//textBox_coldOiltemp.Text = String.Format("{0:f0}°C", temperature);

				//textBox_motortemp.Text = String.Format("{0:f3}V", v);
				//textBox_controllertemp.Text = String.Format("{0:f1}欧姆", resistence);

			}
		}

		private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{

			int n = serialPort1.BytesToRead;
			byte[] buf = new byte[n];
			serialPort1.BaseStream.Read(buf, 0, n);
			//1.缓存数据
			RxBuffer.AddRange(buf);

			while (RxBuffer.Count >= 5) // AA ,BB, AA/BB/CC, length, check
			{
				//2.1 查找数据头
				if ((RxBuffer[0] == 0xAA) && (RxBuffer[1] == 0xBB)) //传输数据有帧头，用于判断
				{
					int len = RxBuffer[3];
					if (RxBuffer.Count < len + 5) //数据区尚未接收完整
					{
						break;
					}
					//得到完整的数据，复制到ReceiveBytes中进行校验
					byte[] ReceiveBytes = new byte[len + 5];
					RxBuffer.CopyTo(0, ReceiveBytes, 0, len + 5);
					//ReceiveBytes.AddRange()
					//buffer.CopyTo(0, ReceiveBytes.ToArray(), 0, len + 5);
					//ReceiveBytes.AddRange(len + 5);
					if (!receive_check(ReceiveBytes))
					{
						RxBuffer.RemoveRange(0, len + 5);
						//MessageBox.Show("数据包不正确！");
						continue;
					}
					RxBuffer.RemoveRange(0, len + 5);

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

		//private void OnTimedEvent(Object source, ElapsedEventArgs e)    //100ms周期
		//{
		//	if (ScheduleRunning == true)
		//	{
		//		Invoke(new MethodInvoker(() => textBox_executedCounter.Text = (Convert.ToInt16(textBox_totalCounter.Text) - schedulecounter).ToString()));
		//		if (schedulecounter > 0)       //循环次数
		//		{
		//			secondcounter++;
		//			if (testingstage == 1)  //阶段1
		//			{
		//				if (wheel_OilPre_back < 0.05)    //油压解除
		//					OilPreDecDone = true;
		//				else
		//					OilPreDecDone = false;

		//				if (speedback >= targetspeed-20)     //转速高于设定值
		//					speedupDone = true;
		//				else
		//					speedupDone = false;

		//				if (OilPreDecDone && speedupDone)
		//				{
		//					if (secondcounter %(brakeinteval*10) == 0)
		//					{
		//						Invoke(new MethodInvoker(() => { button_setOilPumpCycle2.PerformClick(); }));
		//						OilFlowRate_timer.Start();
		//						Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	开始大流量冷却\n", DateTime.Now.ToLongTimeString().ToString())); }));
		//						//Invoke(new MethodInvoker(() => { button_BrkPedl.PerformClick(); }));
		//						//testingstage = 2;
		//					}
		//				}
		//			}
		//			else if (testingstage==2)	//延时1s
		//			{
		//				if (secondcounter == 20)
		//					testingstage = 3;
		//			}
		//			else if(testingstage == 3)
		//			{
		//				if (speedback <= 0)     //转速低于1rpm
		//					speeddownDone = true;
		//				else
		//					speeddownDone = false;

		//				if (wheel_OilPre_back > 1.9)    //油压建立
		//					OilPreIncDone = true;
		//				else
		//					OilPreIncDone = false;
		//				if (speeddownDone && OilPreIncDone)
		//				{
		//					Invoke(new MethodInvoker(() => { button_OilDrainage.PerformClick(); }));
		//					Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	解除制动\n", DateTime.Now.ToLongTimeString().ToString())); }));
		//					OilFlowRate2_timer.Start();
		//					testingstage = 4;
		//					secondcounter = 0;
		//				}
		//			}
		//			else if (testingstage == 4)	//延时1s
		//			{
		//				if (secondcounter == 20)
		//					testingstage = 5;
		//			}
		//			else if (testingstage == 5)
		//			{
						
		//				if (wheel_OilPre_back < 0.05)    //油压解除
		//				{
		//					if (schedulecounter > 1)
		//					{
		//						Invoke(new MethodInvoker(() => { button_enableDem.PerformClick(); }));
		//						Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	使能轮毂电机\n", DateTime.Now.ToLongTimeString().ToString())); }));
		//						if (EnableMotor_flag)
		//						{
		//							Invoke(new MethodInvoker(() => { button_setspeed.PerformClick(); }));
		//							Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	设置转速\n", DateTime.Now.ToLongTimeString().ToString())); }));
		//							schedulecounter--;
		//							testingstage = 1;
		//							secondcounter = 0;
		//						}
		//					}
		//					else            //最后一次刹车后不自动启动电机
		//					{
		//						schedulecounter--;
		//						testingstage = 1;
		//					}
		//				}
		//			}
		//		}
		//		else
		//		{
		//			ScheduleRunning = false;
		//			Invoke(new MethodInvoker(() => {button_scheduled.Text = "开始" ;}));
		//			textBox_executedCounter.Invoke(new Action(() => textBox_executedCounter.Text = textBox_totalCounter.Text));
		//		}
		//	}
		//}



		private void CloseSerialOnExit()
		{

			try
			{
					//serialPort1.DiscardInBuffer();
					//serialPort1.DiscardOutBuffer();
					serialPort1.Close();
					


			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				;
			}

		}

		private void button_serial_Click(object sender, EventArgs e)
		{

			//根据当前串口对象，来判断操作  
			if (serialPort1.IsOpen)
			{

				//serialPort1.Close();
				textBox_CANID.Enabled = true;
				comboBox_frametype.Enabled = true;
				timer_DataLog.Stop();
				Thread  CloseDown = new System.Threading.Thread(new System.Threading.ThreadStart(CloseSerialOnExit));
				CloseDown.Start();
				button_serial.Text = "打开";
			}
			else
			{
				//关闭时点击，则设置好端口，波特率后打开
				serialPort1.PortName = comboBox_serialport.Text;
				serialPort1.BaudRate = 500000;
				try
				{
					serialPort1.Open();
					serialPort1.DiscardInBuffer();
					serialPort1.DiscardOutBuffer();

					SerialRead_timer.Start();
					if (textBox_CANID.Text != "")
						CAN_ID = Convert.ToUInt32(textBox_CANID.Text, 16);
					else
						CAN_ID = 0;
					textBox_CANID.Enabled = false;
					comboBox_frametype.Enabled = false;
					timer_DataLog.Start();
					button_serial.Text = "关闭";
				}
				catch (Exception ex)
				{
					//捕获到异常信息，创建一个新的comm对象，之前的不能用了。  
					serialPort1 = new SerialPort();
					//现实异常信息给客户。  
					MessageBox.Show(ex.Message);
				}
			}
			//设置按钮的状态  
			//button_serial.Text = serialPort1.IsOpen ? "关闭" : "打开";


		}

		private byte[] CAN_1x_packed()
		{
			CAN_obj can = new CAN_obj();
			can.type = (byte)comboBox_frametype.SelectedIndex;
			can.ID = CAN_ID;
			can.DLC = 8;
			can.Data[0] = (byte)(CAN_1x.Speed + 32768);
			can.Data[1] = (byte)((CAN_1x.Speed + 32768) >> 8);
			can.Data[2] = 0;
			can.Data[3] = 0;
			can.Data[4] = (byte)(CAN_1x.Torque + 512);
			can.Data[5] = (byte)((byte)(((CAN_1x.Torque + 512) >> 8) & 0x03) + (byte)((CAN_1x.ModeDem << 4) | (CAN_1x.HVPowerDem << 5) | (CAN_1x.DischargeDem << 6) | (CAN_1x.EnableDem << 7)));
			can.Data[6] = 0;
			can.Data[7] = 0;

			//StringBuilder str = new StringBuilder();
			//for (int i = 0; i < can.DLC; i++)
			//	str.Append(String.Format("{0:X2} ", can.Data[i]));
			////Console.WriteLine("2   "+str);
			//richTextBox2.AppendText(String.Format(" {0}  {1,8:X}   {2}\n", DateTime.Now.ToLongTimeString().ToString(), can.ID, str));
			////设置光标的位置到文本尾 
			//richTextBox2.Select(richTextBox2.TextLength, 0);
			////滚动到控件光标处 
			//richTextBox2.ScrollToCaret();

			return StructToBytes(can);

		}

		private byte[] CAN_break_packed()
		{
			CAN_obj can = new CAN_obj();
			can.type = (byte)comboBox_frametype.SelectedIndex;
			can.ID = 0x201;
			can.DLC = 8;
			can.Data[0] = (byte)((CAN_brake.can_brake_flag << 7) | (CAN_brake.brake_value & 0x7f));
			can.Data[1] = 0;
			can.Data[2] = (byte)(CAN_brake.OilPressure);
			can.Data[3] = (byte)(CAN_brake.set_oilpressure_flag << 7);
			can.Data[4] = 0;
			can.Data[5] = 0;
			can.Data[6] = 0;
			can.Data[7] = 0;

			//StringBuilder str = new StringBuilder();
			//for (int i = 0; i < can.DLC; i++)
			//	str.Append(String.Format("{0:X2} ", can.Data[i]));
			////Console.WriteLine("2   "+str);
			//richTextBox2.AppendText(String.Format("   {0,8:X}   {1}\n", can.ID, str));
			////设置光标的位置到文本尾 
			//richTextBox2.Select(richTextBox2.TextLength, 0);
			////滚动到控件光标处 
			//richTextBox2.ScrollToCaret();

			return StructToBytes(can);
		}

		private byte[] CAN_PumpDutyCycle_packed()
		{
			CAN_obj can = new CAN_obj();
			can.type = (byte)comboBox_frametype.SelectedIndex;
			byte d;
			try
			{
				d = Convert.ToByte(textBox_OilPumpDutycycle.Text);
				if (d >= 100)
					d = 100;
			}
			catch (Exception)
			{
				throw;
			}
			can.ID = 0x301;
			can.DLC = 8;
			can.Data[0] = d;
			can.Data[1] = 0;
			can.Data[2] = 0;
			can.Data[3] = 0;
			can.Data[4] = 0;
			can.Data[5] = 0;
			can.Data[6] = 0;
			can.Data[7] = 0;

			//StringBuilder str = new StringBuilder();
			//for (int i = 0; i < can.DLC; i++)
			//	str.Append(String.Format("{0:X2} ", can.Data[i]));
			////Console.WriteLine("2   "+str);
			//richTextBox2.AppendText(String.Format("   {0,8:X}   {1}\n", can.ID, str));
			////设置光标的位置到文本尾 
			//richTextBox2.Select(richTextBox2.TextLength, 0);
			////滚动到控件光标处 
			//richTextBox2.ScrollToCaret();

			return StructToBytes(can);
		}

		private byte[] CAN_PumpDutyCycle2_packed()
		{
			CAN_obj can = new CAN_obj();
			can.type = (byte)comboBox_frametype.SelectedIndex;
			byte d;
			try
			{
				d = Convert.ToByte(textBox_OilPumpDutycycle2.Text);
				if (d >= 100)
					d = 100;
			}
			catch (Exception)
			{
				throw;
			}
			can.ID = 0x301;
			can.DLC = 8;
			can.Data[0] = d;
			can.Data[1] = 0;
			can.Data[2] = 0;
			can.Data[3] = 0;
			can.Data[4] = 0;
			can.Data[5] = 0;
			can.Data[6] = 0;
			can.Data[7] = 0;

			//StringBuilder str = new StringBuilder();
			//for (int i = 0; i < can.DLC; i++)
			//	str.Append(String.Format("{0:X2} ", can.Data[i]));
			////Console.WriteLine("2   "+str);
			//richTextBox2.AppendText(String.Format("   {0,8:X}   {1}\n", can.ID, str));
			////设置光标的位置到文本尾 
			//richTextBox2.Select(richTextBox2.TextLength, 0);
			////滚动到控件光标处 
			//richTextBox2.ScrollToCaret();

			return StructToBytes(can);
		}

		private void datatosend(byte[] s, int flag)
		{
			if (((flag & 0x3) == 0) || (s == null))
				return;
			List<byte> head = new List<byte>();
			switch (flag)
			{
				case 1:
					head.Add(0xAA);
					break;
				case 2:
					head.Add(0xBB);
					break;
				case 3:
					head.Add(0xCC);
					break;
				default:
					break;
			}
			head.Add((byte)s.Length);
			head.AddRange(s);
			int sum = 0;
			foreach (var item in head)
				sum += item;
			head.Add((byte)sum);
			if (serialPort1.IsOpen)
			{
				byte[] text = head.ToArray();
				serialPort1.Write(text, 0, text.Count());
				//serialPort1.Write(head.ToString());
			}
		}
		/// <summary>
		/// 将结构转化为字节数组
		/// </summary>
		/// <param name="obj">结构对象</param>
		/// <returns>字节数组</returns>
		public static byte[] StructToBytes(object obj)
		{
			//得到结构体的大小
			int size = Marshal.SizeOf(obj);
			//分配结构体大小的内容空间
			IntPtr structPtr = Marshal.AllocHGlobal(size);
			//将结构体copy到分配好的内存空间
			Marshal.StructureToPtr(obj, structPtr, false);

			//创建byte数组
			byte[] bytes = new byte[size];

			//从内存空间拷贝到byte数组
			Marshal.Copy(structPtr, bytes, 0, size);

			//释放内存空间
			Marshal.FreeHGlobal(structPtr);
			//返回byte数组
			return bytes;
		}//StructToBytes

		/// <summary>
		/// byte数组转换为结构
		/// </summary>
		/// <param name="bytes">byte数组</param>
		/// <param name="type">结构类型</param>
		/// <returns>转换后的结构</returns>
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

		private void button_HV_Click(object sender, EventArgs e)
		{
			CAN_1x.HVPowerDem = (byte)((CAN_1x.HVPowerDem == 1) ? 0 : 1);
			datatosend(CAN_1x_packed(), 1);
		}

		private void button_discharge_Click(object sender, EventArgs e)
		{
			CAN_1x.DischargeDem = (byte)((CAN_1x.DischargeDem == 1) ? 0 : 1);
			datatosend(CAN_1x_packed(), 1);
		}

		private void button_enableDem_Click(object sender, EventArgs e)
		{
			CAN_1x.Speed = 0;
			CAN_1x.Torque = 0;
			//textBox_targetspeed.Text = "0";
			//textBox_targettorque.Text = "0";
			CAN_1x.EnableDem = 1;//(byte)((CAN_1x.EnableDem == 1) ? 0 : 1);
								 //if (CAN_1x.EnableDem == 1)
								 //{
								 //	button_enableDem.Text = "关闭";
								 //aTimer.Start();
								 //}
								 //else
								 //{
								 //	button_enableDem.Text = "使能";
								 //	aTimer.Stop();
								 //}

			datatosend(CAN_1x_packed(), 1);
		}



		private void button_setspeed_Click(object sender, EventArgs e)
		{
			CAN_1x.ModeDem = 1;
			CAN_1x.Torque = 0;
			short targetspeed = Convert.ToInt16(textBox_targetspeed.Text);
			try
			{
				CAN_1x.Speed = targetspeed;
				button_OilDrainage.PerformClick();
				datatosend(CAN_1x_packed(), 1);
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void button_settorque_Click(object sender, EventArgs e)
		{

			CAN_1x.ModeDem = 0;
			CAN_1x.Speed = 0;
			try
			{
				CAN_1x.Torque = (short)((Convert.ToInt16(textBox_targettorque.Text)) & 0x3FF);
				button_OilDrainage.PerformClick();
				datatosend(CAN_1x_packed(), 1);
			}
			catch (Exception)
			{

				throw;
			}

		}

		private void textBox_targetspeed_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button_setspeed.PerformClick();
		}

		private void textBox_targettorque_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button_settorque.PerformClick();
		}

		//private void button_OilPressure_Click(object sender, EventArgs e)
		//{
		//	try
		//	{
		//		CAN_brake.set_oilpressure_flag = 1;
		//		CAN_brake.OilPressure = Convert.ToByte((byte)(Convert.ToByte(textBox_MaxOilPressure.Text) / 20.0 * 113.0)); //255对应5.11Mpa
		//		if (CAN_brake.OilPressure >= 170) //最高压力限制在3MPa
		//			CAN_brake.OilPressure = 170;
		//		datatosend(CAN_break_packed(), 1);
		//	}
		//	catch (Exception)
		//	{

		//		throw;
		//	}
		//}

		private void button_BrkPedl_Click(object sender, EventArgs e)
		{
			button_disableDem.PerformClick();

			try
			{
				CAN_brake.can_brake_flag = 1;
				CAN_brake.brake_value = Convert.ToByte(textBox_BrkPedl.Text);
				datatosend(CAN_break_packed(), 1);
			}
			catch (Exception)
			{

				throw;
			}
		}

		//private void textBox_MaxOilPressure_KeyDown(object sender, KeyEventArgs e)
		//{
		//	if (e.KeyCode == Keys.Enter)
		//		button_OilPressure.PerformClick();
		//}

		private void textBox_BrkPedl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button_BrkPedl.PerformClick();
		}

		private void button_DCEnable_Click(object sender, EventArgs e)
		{
			CAN_obj can = new CAN_obj();
			try
			{
				can.ID = Convert.ToUInt32(textBox_DCDCID.Text, 16);
				can.DLC = 4;
				can.Data[0] = 0x1;  //第0bit为1时候使能DC-DC
				datatosend(StructToBytes(can), 1);
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void button_brake_Click(object sender, EventArgs e)
		{
			try
			{
				byte[] b = new byte[2];
				b[0] = (byte)(Convert.ToInt16(textBox_pulse_1.Text));
				b[1] = (byte)(Convert.ToInt16(textBox_pulse_1.Text) >> 8);
				datatosend(b, 2);
			}
			catch (Exception)
			{

				throw;
			}
		}

		private void button_release_Click(object sender, EventArgs e)
		{
			try
			{
				byte[] b = new byte[2];
				b[0] = (byte)(Convert.ToInt16(textBox_pulse_2.Text));
				b[1] = (byte)(Convert.ToInt16(textBox_pulse_2.Text) >> 8);
				datatosend(b, 2);
			}
			catch (Exception)
			{

				throw;
			}
		}

		private void textBox_pulse_1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button_brake.PerformClick();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			richTextBox1.Clear();
			received_count = 0;
			label_frameCount.Text = received_count.ToString();
		}


		private void comboBox_serialport_Click(object sender, EventArgs e)
		{
			listSerialport();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			richTextBox2.Clear();
		}

		private void button_disableDem_Click(object sender, EventArgs e)
		{
			CAN_1x.Speed = 0;
			CAN_1x.Torque = 0;
			//textBox_targetspeed.Text = "0";
			//textBox_targettorque.Text = "0";
			CAN_1x.EnableDem = 0;//(byte)((CAN_1x.EnableDem == 1) ? 0 : 1);

			//aTimer.Stop();
			datatosend(CAN_1x_packed(), 1);
		}

		private void richTextBox3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{

		}

		private void button3_Click(object sender, EventArgs e)
		{
			richTextBox3.Clear();
		}

		private void richTextBox3_KeyDown(object sender, KeyEventArgs e)
		{

		}

		private void button4_Click(object sender, EventArgs e)
		{
			try
			{
				if (richTextBox3.Text != "")
					Clipboard.SetText(richTextBox3.Text);
			}
			catch (Exception ex)
			{
				Application.DoEvents();
				if (richTextBox3.Text != "")
					Clipboard.SetText(richTextBox3.Text);
			}
		}

		private void button_setOilPumpCycle_Click(object sender, EventArgs e)
		{
			datatosend(CAN_PumpDutyCycle_packed(), 1);
		}

		private void textBox_OilPumpDutycycle_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button_setOilPumpCycle.PerformClick();
		}

		private void button_setOilPumpCycle2_Click(object sender, EventArgs e)
		{
			datatosend(CAN_PumpDutyCycle2_packed(), 1);
		}

		private void textBox_OilPumpDutycycle2_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button_setOilPumpCycle2.PerformClick();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			try
			{
				if (richTextBox1.Text != "")
					Clipboard.SetText(richTextBox1.Text);
			}
			catch (Exception ex)
			{
				Application.DoEvents();
				if (richTextBox1.Text != "")
					Clipboard.SetText(richTextBox1.Text);
			}
		}

		private void textBox_pulse_2_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				button_release.PerformClick();
		}

		private void button_OilDrainage_Click(object sender, EventArgs e)
		{
			CAN_brake.can_brake_flag = 1;
			CAN_brake.brake_value = 0;
			datatosend(CAN_break_packed(), 1);

			CAN_brake.can_brake_flag = 0;
			CAN_brake.brake_value = 0;
			datatosend(CAN_break_packed(), 1);
		}

		private void button_scheduled_Click(object sender, EventArgs e)
		{
			if (ScheduleRunning == false)
			{
				//string str = null;
				//Invoke(new MethodInvoker(() => { str = textBox_brakeinteval.Text; }));
				brakeinteval = Convert.ToInt16(textBox_brakeinteval.Text);

				schedulecounter = Convert.ToInt16(textBox_totalCounter.Text);
				//secondcounter = 0;
				ScheduleRunning = true;
				scheduler_timer = new System.Timers.Timer(brakeinteval*1000);
				scheduler_timer.Elapsed += Scheduler_timer_Elapsed;
				scheduler_timer.Start();
				Invoke(new MethodInvoker(() => { richTextBox2.AppendText(String.Format(" {0}	开始自动程序\n", DateTime.Now.ToLongTimeString().ToString())); }));
				
				//OilFlowRate_timer.Stop();
				//OilFlowRate2_timer.Stop();
			}
			else
			{
				ScheduleRunning = false;
				scheduler_timer.Stop();
			}
			Invoke(new MethodInvoker(() => textBox_executedCounter.Text = (Convert.ToInt16(textBox_totalCounter.Text) - schedulecounter).ToString()));

			button_scheduled.Text = (ScheduleRunning == false) ? "开始" : "停止";
		}
	}
	public class s_1x
	{
		public short Speed;
		public short Torque;
		public byte ModeDem;
		public byte HVPowerDem;
		public byte DischargeDem;
		public byte EnableDem;
		public byte counter;
		public byte CheckValue;
		public s_1x()
		{
			Speed = 0;
			Torque = 0;
			ModeDem = 0;
			HVPowerDem = 0;
			DischargeDem = 0;
			EnableDem = 0;
			counter = 0;
			CheckValue = 0;
		}
	};


	public class s_BrakeUnit
	{
		public byte can_brake_flag;
		public byte brake_value;
		public byte set_oilpressure_flag;
		public byte OilPressure;
		public s_BrakeUnit()
		{
			can_brake_flag = 0;
			brake_value = 0;
			set_oilpressure_flag = 0;
			OilPressure = 0;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]//按1字节对齐
	public class CAN_obj
	{
		public byte type;
		public uint ID;
		public byte DLC;
		//public fixed byte Data[8];
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] Data;

		public CAN_obj()
		{
			type = 0;
			ID = 0;
			DLC = 0;
			Data = new byte[8];
		}
	}

	////多媒体定时器
	//public class MultimediaTimer : IDisposable
	//{
	//	private bool disposed = false;
	//	private int interval, resolution;
	//	private UInt32 timerId;

	//	// Hold the timer callback to prevent garbage collection.
	//	private readonly MultimediaTimerCallback Callback;

	//	public MultimediaTimer()
	//	{
	//		Callback = new MultimediaTimerCallback(TimerCallbackMethod);
	//		Resolution = 1;
	//		Interval = 10;
	//	}

	//	~MultimediaTimer()
	//	{
	//		Dispose(false);
	//	}

	//	public int Interval
	//	{
	//		get
	//		{
	//			return interval;
	//		}
	//		set
	//		{
	//			CheckDisposed();

	//			if (value < 0)
	//				throw new ArgumentOutOfRangeException("value");

	//			interval = value;
	//			if (Resolution > Interval)
	//				Resolution = value;
	//		}
	//	}

	//	// Note minimum resolution is 0, meaning highest possible resolution.
	//	public int Resolution
	//	{
	//		get
	//		{
	//			return resolution;
	//		}
	//		set
	//		{
	//			CheckDisposed();

	//			if (value < 0)
	//				throw new ArgumentOutOfRangeException("value");

	//			resolution = value;
	//		}
	//	}

	//	public bool IsRunning
	//	{
	//		get { return timerId != 0; }
	//	}

	//	public void Start()
	//	{
	//		CheckDisposed();

	//		if (IsRunning)
	//			throw new InvalidOperationException("Timer is already running");

	//		// Event type = 0, one off event
	//		// Event type = 1, periodic event
	//		UInt32 userCtx = 0;
	//		timerId = NativeMethods.TimeSetEvent((uint)Interval, (uint)Resolution, Callback, ref userCtx, 1);
	//		if (timerId == 0)
	//		{
	//			int error = Marshal.GetLastWin32Error();
	//			throw new Win32Exception(error);
	//		}
	//	}

	//	public void Stop()
	//	{
	//		CheckDisposed();

	//		if (!IsRunning)
	//			throw new InvalidOperationException("Timer has not been started");

	//		StopInternal();
	//	}

	//	private void StopInternal()
	//	{
	//		NativeMethods.TimeKillEvent(timerId);
	//		timerId = 0;
	//	}

	//	public event EventHandler Elapsed;

	//	public void Dispose()
	//	{
	//		Dispose(true);
	//	}

	//	private void TimerCallbackMethod(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2)
	//	{
	//		var handler = Elapsed;
	//		if (handler != null)
	//		{
	//			handler(this, EventArgs.Empty);
	//		}
	//	}

	//	private void CheckDisposed()
	//	{
	//		if (disposed)
	//			throw new ObjectDisposedException("MultimediaTimer");
	//	}

	//	private void Dispose(bool disposing)
	//	{
	//		if (disposed)
	//			return;

	//		disposed = true;
	//		if (IsRunning)
	//		{
	//			StopInternal();
	//		}

	//		if (disposing)
	//		{
	//			Elapsed = null;
	//			GC.SuppressFinalize(this);
	//		}
	//	}
	//}

	//internal delegate void MultimediaTimerCallback(UInt32 id, UInt32 msg, ref UInt32 userCtx, UInt32 rsv1, UInt32 rsv2);

	//internal static class NativeMethods
	//{
	//	[DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeSetEvent")]
	//	internal static extern UInt32 TimeSetEvent(UInt32 msDelay, UInt32 msResolution, MultimediaTimerCallback callback, ref UInt32 userCtx, UInt32 eventType);

	//	[DllImport("winmm.dll", SetLastError = true, EntryPoint = "timeKillEvent")]
	//	internal static extern void TimeKillEvent(UInt32 uTimerId);
	//}
}
