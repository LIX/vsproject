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
using System.Timers;

namespace HighPerformenceSerialPort
{
	public partial class Form1 : Form
	{
		SerialPort serialPort1 = new SerialPort();
		private List<byte> RxBuffer = new List<byte>(4096);
		byte[] ReceiveBytes = new byte[0x100];
		private static System.Timers.Timer aTimer = new System.Timers.Timer(100);
		

		byte[] buffer = new byte[400];
		Action kickoffRead = null;
		private bool ScheduleRunning;

		public Form1()
		{
			InitializeComponent();
			listSerialport();
			textBox1.Text = "99";
			//serialPort1.DataReceived += new SerialDataReceivedEventHandler(datateceived);
			aTimer = new System.Timers.Timer(5000);
			aTimer.Elapsed += OnTimedEvent;
			aTimer.Start();

		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
				string str = null;
				Invoke(new MethodInvoker(() => { str = textBox1.Text; }));
				int temp = Convert.ToInt16(str);
				Invoke(new MethodInvoker(() => { textBox2.Text = temp.ToString(); }));
			Invoke(new MethodInvoker(() => { button3.PerformClick();  }));
		}

		private void datateceived(object sender, SerialDataReceivedEventArgs e)
		{
			int n = serialPort1.BytesToRead;
			byte[] buf = new byte[n];
			serialPort1.Read(buf, 0, n);
			RxBuffer.AddRange(buf);

			//richTextBox1.Invoke(new Action(()=>richTextBox1.Text=(BitConverter.ToString(buf))));
			//richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(serialPort1.ReadExisting())));

			//Array.Copy(serialPort1.ReadExisting(), 0, RxBuffer, 0, n);
			//RxBuffer.AddRange((serialPort1.ReadExisting().ToList()));
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

					RxBuffer.CopyTo(0, ReceiveBytes, 0, len + 5);
					//if (!receive_check(ReceiveBytes))
					//{
					//	RxBuffer.RemoveRange(0, len + 5);
					//	//MessageBox.Show("数据包不正确！");
					//	continue;
					//}
					RxBuffer.RemoveRange(0, len + 5);

					//Console.WriteLine(BitConverter.ToString(ReceiveBytes));
					try
					{
						this.Invoke((EventHandler)(delegate
						{

							//串口数据处理
							richTextBox1.Invoke(new Action(() => richTextBox1.Text = (BitConverter.ToString(ReceiveBytes))));
							//receive_parse(ReceiveBytes);
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

		private void listSerialport()
		{
			//串口相关
			//string[] ports = SerialPort.GetPortNames();
			//Array.Sort(ports);
			comboBox1.Items.Clear();
			string[] ports = System.IO.Ports.SerialPort.GetPortNames();
			var sortedList = ports.OrderBy(port => Convert.ToInt32(port.Replace("COM", string.Empty)));
			foreach (string port in sortedList)
			{
				comboBox1.Items.Add(port);
			}
			//comboBox_serialport.Items.AddRange(sortedList);
			comboBox1.SelectedIndex = comboBox1.Items.Count > 0 ? 0 : -1;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//根据当前串口对象，来判断操作  
			if (serialPort1.IsOpen)
			{
				while (!(serialPort1.BytesToRead == 0 && serialPort1.BytesToWrite == 0))
				{
					serialPort1.DiscardInBuffer();
					serialPort1.DiscardOutBuffer();
				}
				serialPort1.Close();
				//CloseDown = new System.Threading.Thread(new System.Threading.ThreadStart(CloseSerialOnExit));
				//CloseDown.Start();
			}
			else
			{
				//关闭时点击，则设置好端口，波特率后打开
				
				serialPort1.PortName = comboBox1.Text;
				serialPort1.BaudRate = 500000;
				//aTimer.Start();
				try
				{
					serialPort1.Open();
					serialPort1.DiscardInBuffer();
					serialPort1.DiscardOutBuffer();
					kickoffRead = delegate
					{
						serialPort1.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
						{

							int actualLength = serialPort1.BaseStream.EndRead(ar);
							byte[] received = new byte[actualLength];
							Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
							//richTextBox1.Invoke(new Action(() => richTextBox1.Text = (BitConverter.ToString(received))));
							RxBuffer.AddRange(received);

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

									RxBuffer.CopyTo(0, ReceiveBytes, 0, len + 5);
									//ReceiveBytes.AddRange()
									//buffer.CopyTo(0, ReceiveBytes.ToArray(), 0, len + 5);
									//ReceiveBytes.AddRange(len + 5);
									//if (!receive_check(ReceiveBytes))
									//{
									//	RxBuffer.RemoveRange(0, len + 5);
									//	//MessageBox.Show("数据包不正确！");
									//	continue;
									//}
									RxBuffer.RemoveRange(0, len + 5);

									//Console.WriteLine(BitConverter.ToString(ReceiveBytes));
									try
									{
										this.Invoke((EventHandler)(delegate
										{

											//串口数据处理
											richTextBox1.Invoke(new Action(() => richTextBox1.Text = (BitConverter.ToString(ReceiveBytes))));
											//receive_parse(ReceiveBytes);
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
							kickoffRead();
						}, null);
					};
					kickoffRead();
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
			button1.Text = serialPort1.IsOpen ? "关闭" : "打开";
		}

		private void button2_Click(object sender, EventArgs e)
		{
			string str = null;
			ScheduleRunning = true;
			this.Invoke(new MethodInvoker(() => { str = textBox1.Text; }));
			int a = Convert.ToInt16((str));
			if (a < 0)
				;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			MessageBox.Show("You clicked it!");
		}
	}
}
