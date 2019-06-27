using System;
using Gtk;

namespace CrystalBoy.Emulator.GTK.Windows
{
	public class AboutWindow : Window
	{
		public AboutWindow(int x, int y) : base("About CrystalBoy")
		{
			Label about = new Label();

			about.LineWrap = false;

			about.UseMarkup = true;

			about.Markup = "CrystalBoy is a Gameboy Emulator developed in C#." + Environment.NewLine + Environment.NewLine +
			               "<b>This version is a fork of the original project</b>." + Environment.NewLine + Environment.NewLine +
			               "Original CrystalBoy Project Page: <span foreground=\"blue\">https://github.com/GoldenCrystal/CrystalBoy</span>" + Environment.NewLine + Environment.NewLine +
			               
			               "<b>Forked Project Page</b>: <span foreground=\"blue\">https://github.com/DeathTBO/CrystalBoy</span>" + Environment.NewLine + Environment.NewLine +
			               
			               "GtkSharp is used to create the main window and configuration windows." + Environment.NewLine +
			               "GtkSharp Project page: <span foreground=\"blue\">https://github.com/GtkSharp/GtkSharp</span>" + Environment.NewLine + Environment.NewLine +
			               
			               "SDL2-CS is used to render the emulator. It also handles input and audio." + Environment.NewLine +
			               "SDL2-CS Project page: <span foreground=\"blue\">https://github.com/flibitijibibo/SDL2-CS</span>";

			
			Add(about);

			ShowAll();

			Resize(550, 400);

			Move(x, y);
		}
	}
}