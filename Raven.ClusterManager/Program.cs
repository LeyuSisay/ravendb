﻿using System;
using Nancy.Hosting.Self;
using Raven.Database.Server;

namespace Raven.ClusterManager
{
	public class Program
	{
		public static void Main(string[] args)
		{
			const int port = 8888;
			NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(port);

			var host = new NancyHost(new Uri(string.Format("http://127.0.0.1:{0}/", port)));
			host.Start();

			while (true)
			{
				Console.WriteLine("Available commands: q.");
				var line = Console.ReadLine();

				if (line == "q")
				{
					host.Stop();
					break;
				}
			}

			Console.WriteLine("You have stopped the cluster manager.");
		}
	}
}