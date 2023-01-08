/*
 * 由SharpDevelop创建。
 * 用户： a
 * 日期: 2022/12/18
 * 时间: 10:29
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;

namespace tinybasic
{
	class Program
	{
		public static void Main(string[] args)
		{
//			Console.WriteLine("Hello World!");
			
			// TODO: Implement Functionality Here
			
			TinyBasic.CharPtr[] arrArgs = null;
			if (true) {
				arrArgs = new TinyBasic.CharPtr[args.Length + 1];
				arrArgs[0] = "tinybasic.exe";
				for (int i = 0; i < args.Length; ++i)
				{
					arrArgs[i + 1] = args[i];
				}
			} else {
				arrArgs = new TinyBasic.CharPtr[]{ "tinybasic.exe", "BAS-EX1.BAS" };
			}
			int code = TinyBasic.main(arrArgs.Length, arrArgs);
		
//			Console.Write("Press any key to continue . . . ");
//			Console.ReadKey(true);
			Environment.Exit(code);
		}
	}
}