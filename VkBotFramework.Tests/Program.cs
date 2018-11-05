using System;
using System.Reflection;
using NUnit;
using NUnit.Common;
using NUnitLite;

namespace VkBotFramework.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
			var writter = new ExtendedTextWrapper(Console.Out);
			new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args, writter, Console.In);
			Console.ReadLine();
		}
    }
}
