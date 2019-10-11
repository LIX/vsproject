using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace AHRS_console
{
	class Program
	{
		static void Main(string[] args)
		{
			//System.Diagnostics.Debug.WriteLine("45°对应坡度值="+Math.Tan(
			//			Math.Asin(1/Math.Sqrt(2))));
			//System.Diagnostics.Debug.WriteLine("45°对应sin值=" + Math.Sin(45) );

			if (args.Length>0)
				f(args[0]);
		}
		static AHRS.MadgwickAHRS AHRS = new AHRS.MadgwickAHRS(1.0 / 256, 0.1);

		static double[] gravity_compensate(double[] q, double[] acc)
		{
			double[] g = { 0, 0, 0 };
			g[0] = acc[0] - 2 * (q[1] * q[3] - q[0] * q[2]);
			g[1] = acc[1] - 2 * (q[0] * q[1] + q[2] * q[3]);
			g[2] = acc[2] - q[0] * q[0] - q[1] * q[1] - q[2] * q[2] + q[3] * q[3];
			return g;
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
			Angle[1] = -Math.Atan(R[2] / Math.Sqrt(1 - R[2] * R[2])) / Math.PI * 180;
			Angle[2] = Math.Atan2(R[1], R[0]) / Math.PI * 180;

			return Angle;
		}

		static double deg2rad(double degrees)
		{
			return Math.PI / 180 * degrees;
		}

		static void f(string FullPath)
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
			string filename = @"201907260808.txt";
			string Path = System.IO.Path.GetDirectoryName(FullPath); //获取文件路径
			string Name = System.IO.Path.GetFileName(FullPath); //获取文件名
			//System.Diagnostics.Debug.WriteLine(Directory.GetCurrentDirectory());
			if (File.Exists(Path + Name))
			{
				StreamReader sr = new StreamReader(Path + Name, Encoding.UTF8);
				String text = sr.ReadToEnd();
				foreach (String line in text.Split('\n'))
				{
					String[] StrArray;
					if (line[0] == '\0') //单片机保存数据时候每一行结尾多了一个\0，处理数据时需要去掉
						StrArray = line.Remove(0, 1).Split(new char[4] { ':', ';', ',', '\r' });
					else
						StrArray = line.Split(new char[4] { ':', ';', ',', '\r' });

					if (StrArray.Length >= 10)
					{
						//t.Add(Convert.ToDouble(StrArray[0]));
						//AccelX.Add(Convert.ToDouble(StrArray[2]));
						//AccelY.Add(Convert.ToDouble(StrArray[3]));
						//AccelZ.Add(Convert.ToDouble(StrArray[4]));
						//GyroX.Add(Convert.ToDouble(StrArray[6]));
						//GyroY.Add(Convert.ToDouble(StrArray[7]));
						//GyroZ.Add(Convert.ToDouble(StrArray[8]));
						//MagX.Add(Convert.ToDouble(StrArray[10]));
						//MagY.Add(Convert.ToDouble(StrArray[11]));
						//MagZ.Add(Convert.ToDouble(StrArray[12]));

						t.Add(Convert.ToDouble(StrArray[0]));
						AccelX.Add(Convert.ToDouble(StrArray[1]));
						AccelY.Add(Convert.ToDouble(StrArray[2]));
						AccelZ.Add(Convert.ToDouble(StrArray[3]));
						GyroX.Add(Convert.ToDouble(StrArray[4]));
						GyroY.Add(Convert.ToDouble(StrArray[5]));
						GyroZ.Add(Convert.ToDouble(StrArray[6]));
						MagX.Add(Convert.ToDouble(StrArray[7]));
						MagY.Add(Convert.ToDouble(StrArray[8]));
						MagZ.Add(Convert.ToDouble(StrArray[9]));
					}

				}

				Name = Name.Split('.')[0];  //去掉文件后缀
											//StreamWriter SW = new StreamWriter(Name + "_c.txt");
				double compAngleX = 0;
				double compAngleY = 0;
				for (int i = 0; i < t.Count; i++)
				{
					//AHRS.Update(0, 0, 0, 0, 0.5f, 0.5f, 0.3f, 0.2f, 0.1f);
					AHRS.Update(deg2rad(GyroX[i]), deg2rad(GyroY[i]), deg2rad(GyroZ[i]), AccelX[i],
						AccelY[i], AccelZ[i], MagX[i], MagY[i], MagZ[i]);
					//AHRS.Update((double)deg2rad(GyroX[i]), (double)deg2rad(GyroY[i]), (double)deg2rad(GyroZ[i]), (double)AccelX[i],
					//	(double)AccelY[i], (double)AccelZ[i]);

					//double[] acc = { (double)AccelX[i], (double)AccelY[i], (double)AccelZ[i] };
					//double[] compensate_acc = gravity_compensate(AHRS.Quaternion, acc);
					//speed += compensate_acc[2] * 9.8 * 0.2; //重力加速度g，采样周期0.2s


					//Console.Write(i.ToString() + AHRS.Quaternion[0].ToString("  0.0000000  ")+ AHRS.Quaternion[1].ToString("0.0000000  ")+AHRS.Quaternion[2].ToString("0.0000000  ")+ AHRS.Quaternion[3].ToString("0.0000000  "));

					double[] q = { AHRS.Quaternion[0], AHRS.Quaternion[1], AHRS.Quaternion[2], AHRS.Quaternion[3] };

					double[] Angle = quatern2euler(quaternConj(q));


					double roll = Math.Atan2(AccelY[i], AccelZ[i]) / Math.PI * 180;
					double pitch = Math.Atan(-AccelX[i] / Math.Sqrt(AccelY[i] * AccelY[i] + AccelZ[i] * AccelZ[i])) / Math.PI * 180;
					compAngleX = roll;
					compAngleY = pitch;
					double gyroXrate = deg2rad(GyroX[i]);
					double gyroYrate = deg2rad(GyroY[i]);

					compAngleX = 0.93 * (compAngleX + gyroXrate /256) + 0.07 * roll; // Calculate the angle using a Complimentary filter
					compAngleY = 0.93 * (compAngleY + gyroYrate /256) + 0.07 * pitch;
					Console.Write(" roll:  " + compAngleX.ToString("0.000") + ", pitch: " + compAngleX.ToString("0.000   "));

					//System.Diagnostics.Debug.WriteLine(t[i].ToString((".00")) + ": speed:" + speed.ToString(".00") + "  theta:" +
					//	Math.Acos(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[1] - AHRS.Quaternion[2] * AHRS.Quaternion[3])).ToString(".###") + "°");
					//SW.WriteLine(t[i].ToString(".00") + ":\t theta:" +Math.Tan(
					//	Math.Asin(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[1] - AHRS.Quaternion[2] * AHRS.Quaternion[3]))).ToString(".000") + "%");

					//四元数转换成欧拉角


					//double phi = Math.Atan2((2 * (AHRS.Quaternion[0] * AHRS.Quaternion[3])), (1 - 2 * (AHRS.Quaternion[3] * AHRS.Quaternion[3] + AHRS.Quaternion[1] * AHRS.Quaternion[1]))) / Math.PI * 180;
					//double theta = Math.Asin(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[1] - AHRS.Quaternion[2] * AHRS.Quaternion[3])) / Math.PI * 180;
					//double psi = Math.Atan2(2 * (AHRS.Quaternion[0] * AHRS.Quaternion[2]), (1 - 2 * (AHRS.Quaternion[1] * AHRS.Quaternion[1] + AHRS.Quaternion[2] * AHRS.Quaternion[2]))) / Math.PI * 180;

					//double phi = Math.Atan2((2 * (AHRS.Quaternion[1] * AHRS.Quaternion[2] - AHRS.Quaternion[0] * AHRS.Quaternion[3])), 2 * (AHRS.Quaternion[0] * AHRS.Quaternion[0] + AHRS.Quaternion[1] * AHRS.Quaternion[1]) - 1) / Math.PI * 180;
					//double theta = -Math.Asin(2 * (AHRS.Quaternion[1] * AHRS.Quaternion[3] + AHRS.Quaternion[0] * AHRS.Quaternion[2])) / Math.PI * 180;
					//double psi = Math.Atan2(2 * (AHRS.Quaternion[2] * AHRS.Quaternion[3] + AHRS.Quaternion[0] * AHRS.Quaternion[1]), 2 * (AHRS.Quaternion[0] * AHRS.Quaternion[0] + AHRS.Quaternion[3] * AHRS.Quaternion[3] )- 1) / Math.PI * 180;

					Console.WriteLine(" phi:  " + Angle[0].ToString("0.000") + ", theta: " + Angle[1].ToString("0.000") + ", psi: " + Angle[2].ToString("0.000"));

					//SW.WriteLine("phi:  " + phi.ToString(".000") + ", theta: " + theta.ToString(".000") + ", psi: " + psi.ToString(".000"));
				}
				//SW.Close();

			}   // end of fileexist
			else
				Console.WriteLine("文件不存在");
		}   //end of f()
	}
}
