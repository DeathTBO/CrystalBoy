using System;
using CrystalBoy.Emulator.GTK.Windows;
using Gtk;
using Settings = CrystalBoy.Emulator.Configuration.Settings;

namespace CrystalBoy.Emulator.GTK
{
	internal static class Program
	{
		public static Settings settings;
		
		static void Main(string[] args)
		{
			settings = Settings.Load();
			
			Application.Init();

			MainWindow window = new MainWindow();

			window.DeleteEvent += OnQuit;

			Application.Run();
		}

		static void OnQuit(object o, EventArgs e)
		{
			Application.Quit();
		}
	}
}