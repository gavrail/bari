using System;
using System.IO;

public static class Hello
{
	public static int Main(string[] args) 
	{
		if (Environment.Is64BitProcess)
		{
			Console.WriteLine("64 bit");
			Console.WriteLine(File.ReadAllText(
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generated.txt")));
			return 64;
		}
		else
		{
			Console.WriteLine("32 bit");
			Console.WriteLine(File.ReadAllText(
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generated.txt")));
			return 32;
		}
	}
}