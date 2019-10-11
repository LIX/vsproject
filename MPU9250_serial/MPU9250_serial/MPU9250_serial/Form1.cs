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

namespace MPU9250_serial
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		static AHRS.MadgwickAHRS AHRS = new AHRS.MadgwickAHRS(1 / 5.0, 0.2);
		static double compAngleX;
		static double compAngleY;

		static float[] gravity_compensate(float[] q, float[] acc)
		{
			float[] g = { 0, 0, 0 };
			g[0] = acc[0] - 2 * (q[1] * q[3] - q[0] * q[2]);
			g[1] = acc[1] - 2 * (q[0] * q[1] + q[2] * q[3]);
			g[2] = acc[2] - q[0] * q[0] - q[1] * q[1] - q[2] * q[2] + q[3] * q[3];
			return g;
		}

		static double deg2rad(double degrees)
		{
			return (double)(Math.PI / 180) * degrees;
		}

		private void comboBox_ser_MouseClick(object sender, MouseEventArgs e)
		{
			comboBox_ser.Items.Clear();
			foreach (string item in SerialPort.GetPortNames())
			{
				comboBox_ser.Items.Add(item);
			}
		}

		private void button_ser_Click(object sender, EventArgs e)
		{
			if (serialPort1.IsOpen)
			{
				serialPort1.Close();
				button_ser.Text = "打开";
			}
			else
			{
				serialPort1.PortName = comboBox_ser.SelectedItem.ToString();
				serialPort1.Open();
				serialPort1.DiscardInBuffer();
				serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
				button_ser.Text = "关闭";
			}

		}
		static double[] quaternConj(double[] q)
		{
			q[0] = q[0];
			q[1] = -q[1];
			q[2] = -q[2];
			q[3] = -q[3];
			return q;
		}

		static double[] quatern2euler(double[] q)
		{
			double[] R = { 0, 0, 0, 0, 0 };
			double[] Angle = { 0, 0, 0 };
			R[0] = 2 * (q[0] * q[0] + q[1] * q[1]) - 1;
			R[1] = 2 * (q[1] * q[2] - q[0] * q[3]);
			R[2] = 2 * (q[1] * q[3] + q[0] * q[2]);
			R[3] = 2 * (q[2] * q[3] - q[0] * q[1]);
			R[4] = 2 * (q[0] * q[0] + q[3] * q[3]) - 1;

			Angle[0] = Math.Atan2(R[3], R[4]) / Math.PI * 180;		
			Angle[1] = -Math.Asin(R[2]) / Math.PI * 180;
			Angle[2] = Math.Atan2(R[1], R[0]) / Math.PI * 180;

			return Angle;
		}

		static double[] computeEulerAngles(double[] q)
		{
			double[] R = { 0, 0, 0, 0, 0 };
			double[] Angle = { 0, 0, 0 };
			R[0] = -2 * (q[2] * q[2] + q[3] * q[3]) + 1;
			R[1] = 2 * (q[1] * q[2] - q[0] * q[3]);
			R[2] = 2 * (q[1] * q[3] + q[0] * q[2]);
			R[3] = 2 * (q[2] * q[3] - q[0] * q[1]);
			R[4] = -2 * (q[0] * q[0] + q[2] * q[2]) + 1;

			Angle[0] = Math.Atan2(R[3], R[4]) / Math.PI * 180;	//roll
			Angle[1] = -Math.Atan(R[2] / Math.Sqrt(1 - R[2] * R[2])) / Math.PI * 180;       //pitch
			Angle[2] = Math.Atan2(R[1], R[0]) / Math.PI * 180;	//yaw
			return Angle;
		}

		private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			int n = serialPort1.BytesToRead;
			byte[] buf = new byte[n];
			serialPort1.Read(buf, 0, n);

			if (n >= 91)
			{
				String[] StrArray;
				StrArray = System.Text.Encoding.UTF8.GetString(buf).Split(new char[4] { ':', ';', ',', '\r' });
				if (StrArray.Length >= 12)
				{
					double AccelX = Convert.ToDouble(StrArray[1]);
					double AccelY = Convert.ToDouble(StrArray[2]);
					double AccelZ = Convert.ToDouble(StrArray[3]);
					double GyroX = Convert.ToDouble(StrArray[5]);
					double GyroY = Convert.ToDouble(StrArray[6]);
					double GyroZ = Convert.ToDouble(StrArray[7]);
					double MagX = Convert.ToDouble(StrArray[9]);
					double MagY = Convert.ToDouble(StrArray[10]);
					double MagZ = Convert.ToDouble(StrArray[11]);

					//AHRS.Update((float)deg2rad(GyroX), (float)deg2rad(GyroY), (float)deg2rad(GyroZ), (float)AccelX,
					//(float)AccelY, (float)AccelZ, (float)MagX, (float)MagY, (float)MagZ);
					//AHRS.Update((float)deg2rad(GyroX), (float)deg2rad(GyroY), (float)deg2rad(GyroZ), (float)AccelX,
					//	(float)AccelY, (float)AccelZ);

					//double[] q = { AHRS.Quaternion[0], AHRS.Quaternion[1], AHRS.Quaternion[2], AHRS.Quaternion[3] };

					////double[] Angle = quatern2euler(quaternConj(q));		//ARHS
					//double[] Angle = computeEulerAngles(quaternConj(q));                //Sparkfun

					double roll = Math.Atan2(AccelY, AccelZ) / Math.PI * 180;
					double pitch = Math.Atan(-AccelX / Math.Sqrt(AccelY * AccelY + AccelZ * AccelZ)) / Math.PI * 180;
					compAngleX = roll;
					compAngleY = pitch;
					double gyroXrate = deg2rad(GyroX);
					double gyroYrate = deg2rad(GyroY);

					compAngleX = 0.93 * (compAngleX + gyroXrate *0.2) + 0.07 * roll; // Calculate the angle using a Complimentary filter
					compAngleY = 0.93 * (compAngleY + gyroYrate *0.2) + 0.07 * pitch;
					Console.Write(" roll:  " + compAngleX.ToString("0.000") + ", pitch: " + compAngleX.ToString("0.000   "));

					this.BeginInvoke((EventHandler)(delegate
					{
						//richTextBox1.AppendText("phi:  " + Angle[0].ToString("###0.000") + ", theta: " + Angle[1].ToString("###0.000") + ", psi: " + Angle[2].ToString("###0.000\n"));
						//richTextBox1.AppendText(String.Format("phi:{0,7:0.000}, theta:{1,7:0.000},psi:{2,7:0.000}\n", Angle[0], Angle[1], Angle[2]));
						richTextBox1.AppendText(String.Format("roll:{0,7:0.000}, pitch:{1,7:0.000}\n", compAngleX, compAngleY));
						richTextBox1.ScrollToCaret();
					}));
					//float[] acc = { (float)AccelX, (float)AccelY, (float)AccelZ };
					//float[] compensate_acc = gravity_compensate(AHRS.Quaternion, acc);

					//四元数转换成欧拉角
					//phi = atan2(2(wz+xy),1-2*(z^2+x^2))
					//theta = arcsin(2*(wx-yz))
					//psi = atan2(2(wy+zx),1-2(x^2+y^2))
					//		double phi = Math.Atan2((2 * (AHRS.Quaternion[0] * AHRS.Quaternion[3])), (1 - 2 * (AHRS.Quaternion[3] * AHRS.Quaternion[3] + AHRS.Quaternion[1] * AHRS.Quaternion[1]))) / Math.PI * 180;
					//			double theta = Math.Asin(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[1] - AHRS.Quaternion[2] * AHRS.Quaternion[3])) / Math.PI * 180;
					//			double psi = Math.Atan2(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[2]), (1 - 2 * (AHRS.Quaternion[1] * AHRS.Quaternion[1] + AHRS.Quaternion[2] * AHRS.Quaternion[2]))) / Math.PI * 180;
					//			this.BeginInvoke((EventHandler)(delegate
					//			{
					//				richTextBox1.AppendText("phi:  " + phi.ToString("0.000") + ", theta: " + theta.ToString("0.000") + ", psi: " + psi.ToString("0.000\n"));
					//				richTextBox1.ScrollToCaret();
					//			}));
					//}

				}
				else
					serialPort1.DiscardInBuffer();

			}
		}

		private void button_rtbClear_Click(object sender, EventArgs e)
		{
			richTextBox1.Clear();
		}
	}
}
