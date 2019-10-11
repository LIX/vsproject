using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CsharpLearning
{
	class Rectangle
	{
		protected double width, height;
		public Rectangle(double a, double b)
		{ width = a; height = b; }
		public double getAera()
		{ return width * height; }
		public void display()
		{
			Console.WriteLine("width={0}", width);
			Console.WriteLine("height={0}", height);
			Console.WriteLine("Aera={0}", getAera());
		}
	}
	class Tabletop : Rectangle
	{
		public Tabletop(double a, double b) : base(a, b)
		{ }
		public double cost()
		{
			return 70 * getAera();
		}
		public new void display()
		{
			base.display();
			Console.WriteLine("cost={0}", cost());
		}
	}
	class Point
	{
		double x, y;
		public Point()
		{

		}
		public Point(double i, double j)
		{
			x = i;
			y = j;
		}
		public void Init(double i,double j)
		{
			x = i;
			y = j;
		}
		public static Point operator +(Point A,Point B)
		{
			Point p = new Point();
			p.x = A.x + B.x;
			p.y = A.y + B.y;
			return p;
		}

		public void display()
		{
			Console.WriteLine("Point.x={0},Point.y={1}", x, y);
		}
	}
	class Program
	{
		public static int FindMax(int a, int b)
		{
			return (a >= b) ? a : b;
		}

		public static void getValue(out int a)
		{
			a = 100;
		}
		private static System.Timers.Timer t1;
		private static System.Timers.Timer t2;
		static int i = 0;
		static int j = 0;
		static void Main(string[] args)
		{
			//Point A = new Point(1,2);
			//Point B = new Point(3,4);
			//Point C = new Point();
			//C = A + B;
			//C.display();
			//Console.ReadLine();
			t1 = new System.Timers.Timer(1000);//实例化Timer类，设置时间间隔
			t2 = new System.Timers.Timer(1000);//实例化Timer类，设置时间间隔
			t1.Elapsed += T1_Elapsed;
			t2.Elapsed += T2_Elapsed;
			int count = 3;
			
				t1.Start();
			Thread.Sleep(1500);
			//t1.Stop();
			//t2.Stop();
			Console.ReadLine();

		}

		private static void T2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			
			Console.WriteLine("\tj={0},{1}", j++, DateTime.Now.ToLongTimeString().ToString());
		}

		private static void T1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine("{0},i={1}", DateTime.Now.ToLongTimeString().ToString(), i++);
			
			t1.Stop();
			Thread.Sleep(3000);
			Console.WriteLine("{0},j={1}", DateTime.Now.ToLongTimeString().ToString(),j++);
			//t2.Start();
			//t1.Stop();
		}
	}

	public interface a
	{
		
	}
}
