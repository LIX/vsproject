using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace AHRS_chart
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			f();
		}

		static AHRS.MadgwickAHRS AHRS = new AHRS.MadgwickAHRS(1f / 5f, 0.1f);

		static float[] gravity_compensate(float[] q,float[] acc)
		{
			float[] g = { 0,0,0};
			g[0] = acc[0] - 2 * (q[1] * q[3] - q[0] * q[2]);
			g[1] = acc[1] - 2 * (q[0] * q[1] + q[2] * q[3]);
			g[2] = acc[2] - q[0] * q[0] - q[1] * q[1] - q[2] * q[2] + q[3] * q[3];
			return g;
		}

		static void f()
		{
			
			List<double> t = new List<double>();
			List<double> AccelX = new List<double>();
			List<double> AccelY = new List<double>();
			List<double> AccelZ = new List<double>();
			List<double> GyroX = new List<double>();
			List<double> GyroY = new List<double>();
			List<double> GyroZ = new List<double>();
			List<double> MagX = new List<double>();
			List<double> MagY = new List<double>();
			List<double> MagZ = new List<double>();
			double speed = 0;
			string pwd = @"..\..\";
			string filename = @"201907141651.txt";
			if (File.Exists(pwd + filename))
			{
				StreamReader sr = new StreamReader(pwd + filename, Encoding.UTF8);
				String text = sr.ReadToEnd();
				foreach (String line in text.Split('\n'))
				{
					String[] StrArray;
					if (line[0] == '\0') //单片机保存数据时候每一行结尾多了一个\0，处理数据时需要去掉
						StrArray = line.Remove(0, 1).Split(new char[4] { ':', ';', ',', '\r' });
					else
						StrArray = line.Split(new char[4] { ':', ';', ',', '\r' });

					if (StrArray.Length >= 13)
					{
						t.Add(Convert.ToDouble(StrArray[0]));
						AccelX.Add(Convert.ToDouble(StrArray[2]));
						AccelY.Add(Convert.ToDouble(StrArray[3]));
						AccelZ.Add(Convert.ToDouble(StrArray[4]));
						GyroX.Add(Convert.ToDouble(StrArray[6]));
						GyroY.Add(Convert.ToDouble(StrArray[7]));
						GyroZ.Add(Convert.ToDouble(StrArray[8]));
						MagX.Add(Convert.ToDouble(StrArray[10]));
						MagY.Add(Convert.ToDouble(StrArray[11]));
						MagZ.Add(Convert.ToDouble(StrArray[12]));
					}

				}

				filename = filename.Split( '.')[0];
				StreamWriter SW = new StreamWriter(filename+"_c.txt");
				for (int i = 0; i < t.Count; i++)
				{
					AHRS.Update((float)GyroX[i], (float)GyroY[i], (float)GyroZ[i], (float)AccelX[i], 
						(float)AccelY[i], (float)AccelZ[i], (float)MagX[i], (float)MagY[i], (float)MagZ[i]);

					float[] acc = { (float)AccelX[i], (float)AccelY[i], (float)AccelZ[i] };
					float[] compensate_acc= gravity_compensate(AHRS.Quaternion, acc);
					speed += compensate_acc[2] * 9.8 * 0.2;	//重力加速度g，采样周期0.2s
					//System.Diagnostics.Debug.WriteLine(t[i].ToString((".00")) + ": speed:" + speed.ToString(".00") + "  theta:" +
					//	Math.Acos(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[1] - AHRS.Quaternion[2] * AHRS.Quaternion[3])).ToString(".###") + "°");
					SW.WriteLine(t[i].ToString(".00") + ":\t theta:" +
						Math.Acos(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[1] - AHRS.Quaternion[2] * AHRS.Quaternion[3])).ToString(".000") + "°");
				}
				
			}	// end of fileexist
		}	//end of f()

	}
}
