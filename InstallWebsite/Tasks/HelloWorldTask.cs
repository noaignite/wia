using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InstallWebsite.Tasks
{
	public class HelloWorldTask : ITask
	{
		public void Execute(WebsiteContext context)
		{
			Console.WriteLine("hello");
		}
	}
}
