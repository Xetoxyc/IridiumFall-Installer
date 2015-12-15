using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace IridiumFallInstaller
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			byte[] ba = null;
			string resource = "Ionic.Zip.dll";
			Assembly curAsm = Assembly.GetExecutingAssembly();

			using (Stream stm = curAsm.GetManifestResourceStream(resource))
			{
				ba = new byte[(int)stm.Length];
				stm.Read(ba, 0, (int)stm.Length);

				Assembly.Load(ba);
			}
		}		
	}
}

