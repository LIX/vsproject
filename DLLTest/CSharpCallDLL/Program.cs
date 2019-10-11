using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CSharpCallDLL
{
	class Program
	{
		[DllImport(@"../CreateDLL.dll")]
		extern static int test01(int a, int b, int c);

		[DllImport(@"../CreateDLL.dll")]
		extern static int test02(int a, int b);
		static void Main(string[] args)
		{
			int r1 = test01(1, 2, 3);
			int r2 = test02(4, 2);
			Console.WriteLine("test01=" + r1.ToString());
			Console.WriteLine("test02=" + r2.ToString());
			Console.ReadKey();
		}
	}
}
