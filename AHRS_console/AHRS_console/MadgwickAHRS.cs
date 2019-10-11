using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AHRS
{
    /// <summary>
    /// MadgwickAHRS class. Implementation of Madgwick's IMU and AHRS algorithms.
    /// </summary>
    /// <remarks>
    /// See: http://www.x-io.co.uk/node/8#open_source_ahrs_and_imu_algorithms
    /// </remarks>
    public class MadgwickAHRS
    {
        /// <summary>
        /// Gets or sets the sample period.
        /// </summary>
        public double SamplePeriod { get; set; }

        /// <summary>
        /// Gets or sets the algorithm gain beta.
        /// </summary>
        public double Beta { get; set; }

        /// <summary>
        /// Gets or sets the Quaternion output.
        /// </summary>
        public double[] Quaternion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MadgwickAHRS"/> class.
        /// </summary>
        /// <param name="samplePeriod">
        /// Sample period.
        /// </param>
        public MadgwickAHRS(double samplePeriod)
            : this(samplePeriod, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MadgwickAHRS"/> class.
        /// </summary>
        /// <param name="samplePeriod">
        /// Sample period.
        /// </param>
        /// <param name="beta">
        /// Algorithm gain beta.
        /// </param>
        public MadgwickAHRS(double samplePeriod, double beta)
        {
            SamplePeriod = samplePeriod;
            Beta = beta;
            Quaternion = new double[] { 1.0, 0.0, 0.0, 0.0 };
        }

		/// <summary>
		/// Algorithm AHRS update method. Requires only gyroscope and accelerometer data.
		/// </summary>
		/// <param name="gx">
		/// Gyroscope x axis measurement in radians/s.
		/// </param>
		/// <param name="gy">
		/// Gyroscope y axis measurement in radians/s.
		/// </param>
		/// <param name="gz">
		/// Gyroscope z axis measurement in radians/s.
		/// </param>
		/// <param name="ax">
		/// Accelerometer x axis measurement in any calibrated units.
		/// </param>
		/// <param name="ay">
		/// Accelerometer y axis measurement in any calibrated units.
		/// </param>
		/// <param name="az">
		/// Accelerometer z axis measurement in any calibrated units.
		/// </param>
		/// <param name="mx">
		/// Magnetometer x axis measurement in any calibrated units.
		/// </param>
		/// <param name="my">
		/// Magnetometer y axis measurement in any calibrated units.
		/// </param>
		/// <param name="mz">
		/// Magnetometer z axis measurement in any calibrated units.
		/// </param>
		/// <remarks>
		/// Optimised for minimal arithmetic.
		/// Total ¡À: 160
		/// Total *: 172
		/// Total /: 5
		/// Total sqrt: 5
		/// </remarks> 
		public void Update(double gx, double gy, double gz, double ax, double ay, double az, double mx, double my, double mz)
		{
			double q1 = Quaternion[0], q2 = Quaternion[1], q3 = Quaternion[2], q4 = Quaternion[3];   // short name local variable for readability
			double norm;
			double hx, hy, bx, bz;
			double s1, s2, s3, s4;
			double qDot1, qDot2, qDot3, qDot4;

			// Auxiliary variables to avoid repeated arithmetic
			double _2q1mx;
			double _2q1my;
			double _2q1mz;
			double _2q2mx;
			double _2bx, _2bz;
			double _4bx;
			double _4bz;
			double _2q1 = 2.0 * q1;
			double _2q2 = 2.0 * q2;
			double _2q3 = 2.0 * q3;
			double _2q4 = 2.0 * q4;
			double _2q1q3 = 2.0 * q1 * q3;
			double _2q3q4 = 2.0 * q3 * q4;
			double q1q1 = q1 * q1;
			double q1q2 = q1 * q2;
			double q1q3 = q1 * q3;
			double q1q4 = q1 * q4;
			double q2q2 = q2 * q2;
			double q2q3 = q2 * q3;
			double q2q4 = q2 * q4;
			double q3q3 = q3 * q3;
			double q3q4 = q3 * q4;
			double q4q4 = q4 * q4;

			// Normalise accelerometer measurement
			norm = (double)Math.Sqrt(ax * ax + ay * ay + az * az);
			if (norm == 0f) return; // handle NaN
			norm = 1 / norm;        // use reciprocal for division
			ax *= norm;
			ay *= norm;
			az *= norm;

			// Normalise magnetometer measurement
			norm = (double)Math.Sqrt(mx * mx + my * my + mz * mz);
			if (norm == 0f) return; // handle NaN
			norm = 1 / norm;        // use reciprocal for division
			mx *= norm;
			my *= norm;
			mz *= norm;

			// Reference direction of Earth's magnetic field
			_2q1mx = 2.0 * q1 * mx;
			_2q1my = 2.0 * q1 * my;
			_2q1mz = 2.0 * q1 * mz;
			_2q2mx = 2.0 * q2 * mx;
			hx = mx * q1q1 - _2q1my * q4 + _2q1mz * q3 + mx * q2q2 + _2q2 * my * q3 + _2q2 * mz * q4 - mx * q3q3 - mx * q4q4;
			hy = _2q1mx * q4 + my * q1q1 - _2q1mz * q2 + _2q2mx * q3 - my * q2q2 + my * q3q3 + _2q3 * mz * q4 - my * q4q4;
			bx = (double)Math.Sqrt(hx * hx + hy * hy);
			bz = -_2q1mx * q3 + _2q1my * q2 + mz * q1q1 + _2q2mx * q4 - mz * q2q2 + _2q3 * my * q4 - mz * q3q3 + mz * q4q4;
			_2bx = 2.0 * bx;
			_2bz = 2.0 * bz;
			_4bx = 2.0 * _2bx;
			_4bz = 2.0 * _2bz;

			// Gradient decent algorithm corrective step
			s1 = -_2q3 * (2.0 * q2q4 - _2q1q3 - ax) + _2q2 * (2.0 * q1q2 + _2q3q4 - ay)													- _2bz * q3 * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q4 + _2bz * q2) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q3 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
			s2 = _2q4 * (2.0 * q2q4 - _2q1q3 - ax) + _2q1 * (2.0 * q1q2 + _2q3q4 - ay) - 4.0 * q2 * (1 - 2.0 * q2q2 - 2.0 * q3q3 - az) + _2bz * q4 * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q3 + _2bz * q1) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q4 - _4bz * q2) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
			s3 = -_2q1 * (2.0 * q2q4 - _2q1q3 - ax) + _2q4 * (2.0 * q1q2 + _2q3q4 - ay) - 4.0 * q3 * (1 - 2.0 * q2q2 - 2.0 * q3q3 - az) + (-_4bx * q3 - _2bz * q1) * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q2 + _2bz * q4) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q1 - _4bz * q3) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
			s4 = _2q2 * (2.0 * q2q4 - _2q1q3 - ax) + _2q3 * (2.0 * q1q2 + _2q3q4 - ay)													+ (-_4bx * q4 + _2bz * q2) * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q1 + _2bz * q3) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q2 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
			norm = 1.0 / (double)Math.Sqrt(s1 * s1 + s2 * s2 + s3 * s3 + s4 * s4);    // normalise step magnitude
			s1 *= norm;
			s2 *= norm;
			s3 *= norm;
			s4 *= norm;

			// Compute rate of change of quaternion
			qDot1 = 0.5 * (-q2 * gx - q3 * gy - q4 * gz) - Beta * s1;
			qDot2 = 0.5 * (q1 * gx + q3 * gz - q4 * gy) - Beta * s2;
			qDot3 = 0.5 * (q1 * gy - q2 * gz + q4 * gx) - Beta * s3;
			qDot4 = 0.5 * (q1 * gz + q2 * gy - q3 * gx) - Beta * s4;

			// Integrate to yield quaternion
			q1 += qDot1 * SamplePeriod;
			q2 += qDot2 * SamplePeriod;
			q3 += qDot3 * SamplePeriod;
			q4 += qDot4 * SamplePeriod;
			norm = 1.0 / (double)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);    // normalise quaternion
			Quaternion[0] = q1 * norm;
			Quaternion[1] = q2 * norm;
			Quaternion[2] = q3 * norm;
			Quaternion[3] = q4 * norm;
		}

		//public double[] quaternProduct(double[] a,double[] b)
		//{
		//	double[] ab = { 0, 0, 0, 0 };
		//	ab[0] = a[0] * b[0] - a[1] * b[1] - a[2] * b[2] - a[3] * b[3];
		//	ab[1] = a[0] * b[1] + a[1] * b[0] + a[2] * b[3] - a[3] * b[2];
		//	ab[2] = a[0] * b[2] - a[1] * b[3] + a[2] * b[0] + a[3] * b[1];
		//	ab[3] = a[0] * b[3] + a[1] * b[2] - a[2] * b[1] + a[3] * b[0];

		//	return ab;
		//}

		//public void Update(double gx, double gy, double gz, double ax, double ay, double az, double mx, double my, double mz)
		//{
		//	double q1 = Quaternion[0], q2 = Quaternion[1], q3 = Quaternion[2], q4 = Quaternion[3];   // short name local variable for readability
		//	double norm;
		//	double hx, hy, _2bx, _2bz;
		//	double s1, s2, s3, s4;
		//	double qDot1, qDot2, qDot3, qDot4;

		//	// Auxiliary variables to avoid repeated arithmetic
		//	double _2q1mx;
		//	double _2q1my;
		//	double _2q1mz;
		//	double _2q2mx;
		//	double _4bx;
		//	double _4bz;
		//	double _2q1 = 2.0 * q1;
		//	double _2q2 = 2.0 * q2;
		//	double _2q3 = 2.0 * q3;
		//	double _2q4 = 2.0 * q4;
		//	double _2q1q3 = 2.0 * q1 * q3;
		//	double _2q3q4 = 2.0 * q3 * q4;
		//	double q1q1 = q1 * q1;
		//	double q1q2 = q1 * q2;
		//	double q1q3 = q1 * q3;
		//	double q1q4 = q1 * q4;
		//	double q2q2 = q2 * q2;
		//	double q2q3 = q2 * q3;
		//	double q2q4 = q2 * q4;
		//	double q3q3 = q3 * q3;
		//	double q3q4 = q3 * q4;
		//	double q4q4 = q4 * q4;

		//	// Normalise accelerometer measurement
		//	norm = (double)Math.Sqrt(ax * ax + ay * ay + az * az);
		//	if (norm == 0f) return; // handle NaN
		//	norm = 1 / norm;        // use reciprocal for division
		//	ax *= norm;
		//	ay *= norm;
		//	az *= norm;

		//	// Normalise magnetometer measurement
		//	norm = (double)Math.Sqrt(mx * mx + my * my + mz * mz);
		//	if (norm == 0f) return; // handle NaN
		//	norm = 1.0 / norm;        // use reciprocal for division
		//	mx *= norm;
		//	my *= norm;
		//	mz *= norm;

		//	// Reference direction of Earth's magnetic field
		//	//_2q1mx = 2.0 * q1 * mx;
		//	//_2q1my = 2.0 * q1 * my;
		//	//_2q1mz = 2.0 * q1 * mz;
		//	//_2q2mx = 2.0 * q2 * mx;
		//	//hx = mx * q1q1 - _2q1my * q4 + _2q1mz * q3 + mx * q2q2 + _2q2 * my * q3 + _2q2 * mz * q4 - mx * q3q3 - mx * q4q4;
		//	//hy = _2q1mx * q4 + my * q1q1 - _2q1mz * q2 + _2q2mx * q3 - my * q2q2 + my * q3q3 + _2q3 * mz * q4 - my * q4q4;
		//	//_2bx = (double)Math.Sqrt(hx * hx + hy * hy);
		//	//_2bz = -_2q1mx * q3 + _2q1my * q2 + mz * q1q1 + _2q2mx * q4 - mz * q2q2 + _2q3 * my * q4 - mz * q3q3 + mz * q4q4;
		//	//_4bx = 2.0 * _2bx;
		//	//_4bz = 2.0 * _2bz;

		//	double[] x = { 0, mx, my, mz };
		//	double[] qqq = { q1, q2, q3, q4 };
		//	double[] y = { q1, -q2, -q3, -q4 };
		//	double[] h = quaternProduct(qqq, quaternProduct(x, y));
		//	double[] b = { 0, Math.Sqrt(h[1] * h[1] + h[2] * h[2]), 0, h[3] };

		//	// Gradient decent algorithm corrective step
		//	//s1 = -_2q3 * (2.0 * q2q4 - _2q1q3 - ax) + _2q2 * (2.0 * q1q2 + _2q3q4 - ay) - _2bz * q3 * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q4 + _2bz * q2) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q3 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
		//	//s2 = _2q4 * (2.0 * q2q4 - _2q1q3 - ax) + _2q1 * (2.0 * q1q2 + _2q3q4 - ay) - 4.0 * q2 * (1 - 2.0 * q2q2 - 2.0 * q3q3 - az) + _2bz * q4 * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q3 + _2bz * q1) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q4 - _4bz * q2) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
		//	//s3 = -_2q1 * (2.0 * q2q4 - _2q1q3 - ax) + _2q4 * (2.0 * q1q2 + _2q3q4 - ay) - 4.0 * q3 * (1 - 2.0 * q2q2 - 2.0 * q3q3 - az) + (-_4bx * q3 - _2bz * q1) * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (_2bx * q2 + _2bz * q4) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + (_2bx * q1 - _4bz * q3) * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
		//	//s4 = _2q2 * (2.0 * q2q4 - _2q1q3 - ax) + _2q3 * (2.0 * q1q2 + _2q3q4 - ay) + (-_4bx * q4 + _2bz * q2) * (_2bx * (0.5 - q3q3 - q4q4) + _2bz * (q2q4 - q1q3) - mx) + (-_2bx * q1 + _2bz * q3) * (_2bx * (q2q3 - q1q4) + _2bz * (q1q2 + q3q4) - my) + _2bx * q2 * (_2bx * (q1q3 + q2q4) + _2bz * (0.5 - q2q2 - q3q3) - mz);
		//	double[,] F = new double[,]
		//	{
		//		{ 2*(q2q4-q1q3)-ax },
		//		{2*(q1q2+q3q4)-ay },
		//		{2*(0.5-q2q2-q3q3)-az },
		//		{2*b[1]*(0.5-q3q3-q4q4)+2*b[3]*(q2q4-q1q3)-mx },
		//		{2*b[2]*(q2q3-q1q4)+2*b[3]*(q1q2+q3q4)-my },
		//		{2*b[1]*(q1q3+q2q4)+2*b[3]*(0.5-q2q2-q3q3)-mz },
		//	};

		//	double[,] J_t = new double[,]
		//	{
		//		{-2*q3,	2*q2,	0,			-2*b[3]*q3,				-2*b[1]*q4+2*b[3]*q2,	2*b[1]*q3				},
		//		{2*q4,	2*q1,	-4*q2,		2*b[1]*q4,				2*b[1]*q3+2*b[3]*q1,	2*b[1]*q4-4*b[3]*q2,	},
		//		{-2*q1,	2*q4,	-4*q3,		-4*b[1]*q3-2*b[3]*q1,	2*b[1]*q2+2*b[3]*q4,	2*b[1]*q1-4*b[3]*q3,	},
		//		{2*q2 ,	2*q3,	0,			-4*b[1]*q4+2*b[3]*q2,	-2*b[1]*q1+2*b[3]*q3,	2*b[1]*q2				},
		//	};

		//	double[] c = { 0,0,0,0};
		//	for (int i = 0; i < 4; i++)
		//	{
		//			c[i] = 0;
		//			for (int k = 0; k < 6; k++)
		//			{
		//				c[i] += J_t[i, k] * F[k,0];
		//			}
		//	}
		//	s1 = c[0]; s2 = c[1]; s3 = c[2]; s4 = c[3];

		//	norm = 1.0 / (double)Math.Sqrt(s1 * s1 + s2 * s2 + s3 * s3 + s4 * s4);    // normalise step magnitude
		//	s1 *= norm;
		//	s2 *= norm;
		//	s3 *= norm;
		//	s4 *= norm;

		//	// Compute rate of change of quaternion
		//	//qDot1 = 0.5 * (-q2 * gx - q3 * gy - q4 * gz) - Beta * s1;
		//	//qDot2 = 0.5 * (q1 * gx + q3 * gz - q4 * gy) - Beta * s2;
		//	//qDot3 = 0.5 * (q1 * gy - q2 * gz + q4 * gx) - Beta * s3;
		//	//qDot4 = 0.5 * (q1 * gz + q2 * gy - q3 * gx) - Beta * s4;
		//	double[] qDot = { 0, 0, 0, 0 };
		//	double[] ggg = { 0, gx, gy, gz };
		//	qDot = quaternProduct(qqq, ggg);

		//	// Integrate to yield quaternion
		//	q1 += (0.5*qDot[0]- Beta* s1) * SamplePeriod;
		//	q2 += (0.5*qDot[1] - Beta * s2) * SamplePeriod;
		//	q3 += (0.5*qDot[2] - Beta * s3) * SamplePeriod;
		//	q4 += (0.5*qDot[3] - Beta * s4) * SamplePeriod;
		//	norm = 1.0 / (double)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);    // normalise quaternion
		//	Quaternion[0] = q1 * norm;
		//	Quaternion[1] = q2 * norm;
		//	Quaternion[2] = q3 * norm;
		//	Quaternion[3] = q4 * norm;
		//}


		/// <summary>
		/// Algorithm IMU update method. Requires only gyroscope and accelerometer data.
		/// </summary>
		/// <param name="gx">
		/// Gyroscope x axis measurement in radians/s.
		/// </param>
		/// <param name="gy">
		/// Gyroscope y axis measurement in radians/s.
		/// </param>
		/// <param name="gz">
		/// Gyroscope z axis measurement in radians/s.
		/// </param>
		/// <param name="ax">
		/// Accelerometer x axis measurement in any calibrated units.
		/// </param>
		/// <param name="ay">
		/// Accelerometer y axis measurement in any calibrated units.
		/// </param>
		/// <param name="az">
		/// Accelerometer z axis measurement in any calibrated units.
		/// </param>
		/// <remarks>
		/// Optimised for minimal arithmetic.
		/// Total ¡À: 45
		/// Total *: 85
		/// Total /: 3
		/// Total sqrt: 3
		/// </remarks>
		public void Update(double gx, double gy, double gz, double ax, double ay, double az)
        {
            double q1 = Quaternion[0], q2 = Quaternion[1], q3 = Quaternion[2], q4 = Quaternion[3];   // short name local variable for readability
            double norm;
            double s1, s2, s3, s4;
            double qDot1, qDot2, qDot3, qDot4;

            // Auxiliary variables to avoid repeated arithmetic
            double _2q1 = 2.0 * q1;
            double _2q2 = 2.0 * q2;
            double _2q3 = 2.0 * q3;
            double _2q4 = 2.0 * q4;
            double _4q1 = 4.0 * q1;
            double _4q2 = 4.0 * q2;
            double _4q3 = 4.0 * q3;
            double _8q2 = 8f * q2;
            double _8q3 = 8f * q3;
            double q1q1 = q1 * q1;
            double q2q2 = q2 * q2;
            double q3q3 = q3 * q3;
            double q4q4 = q4 * q4;

            // Normalise accelerometer measurement
            norm = (double)Math.Sqrt(ax * ax + ay * ay + az * az);
            if (norm == 0f) return; // handle NaN
            norm = 1 / norm;        // use reciprocal for division
            ax *= norm;
            ay *= norm;
            az *= norm;

            // Gradient decent algorithm corrective step
            s1 = _4q1 * q3q3 + _2q3 * ax + _4q1 * q2q2 - _2q2 * ay;
            s2 = _4q2 * q4q4 - _2q4 * ax + 4.0 * q1q1 * q2 - _2q1 * ay - _4q2 + _8q2 * q2q2 + _8q2 * q3q3 + _4q2 * az;
            s3 = 4.0 * q1q1 * q3 + _2q1 * ax + _4q3 * q4q4 - _2q4 * ay - _4q3 + _8q3 * q2q2 + _8q3 * q3q3 + _4q3 * az;
            s4 = 4.0 * q2q2 * q4 - _2q2 * ax + 4.0 * q3q3 * q4 - _2q3 * ay;
            norm = 1.0 / (double)Math.Sqrt(s1 * s1 + s2 * s2 + s3 * s3 + s4 * s4);    // normalise step magnitude
            s1 *= norm;
            s2 *= norm;
            s3 *= norm;
            s4 *= norm;

            // Compute rate of change of quaternion
            qDot1 = 0.5 * (-q2 * gx - q3 * gy - q4 * gz) - Beta * s1;
            qDot2 = 0.5 * (q1 * gx + q3 * gz - q4 * gy) - Beta * s2;
            qDot3 = 0.5 * (q1 * gy - q2 * gz + q4 * gx) - Beta * s3;
            qDot4 = 0.5 * (q1 * gz + q2 * gy - q3 * gx) - Beta * s4;

            // Integrate to yield quaternion
            q1 += qDot1 * SamplePeriod;
            q2 += qDot2 * SamplePeriod;
            q3 += qDot3 * SamplePeriod;
            q4 += qDot4 * SamplePeriod;
            norm = 1.0 / (double)Math.Sqrt(q1 * q1 + q2 * q2 + q3 * q3 + q4 * q4);    // normalise quaternion
            Quaternion[0] = q1 * norm;
            Quaternion[1] = q2 * norm;
            Quaternion[2] = q3 * norm;
            Quaternion[3] = q4 * norm;
        }
    }
}