using System.IO;
using System.Reflection;
using CrystalBoy.Emulator.GTK.Widgets;
using Gdk;
using Gtk;

namespace CrystalBoy.Emulator.GTK.Windows
{
	public partial class MainWindow
	{
		//Widgets
		VBox masterContainer;

		MenuBar toolbar;
		
		HBox emulatorControls;

		Button playPauseButton;

		Button stopButton;

		Button frameStepButton;
		
		ListBox romListBox;

		//Icons
		Image playIcon;

		Image pauseIcon;

		Image frameStepIcon;
		
		Image stopIcon;
		
		void InitializeWidgets()
		{
			//Load Icons first
			Pixbuf playBuf = new Pixbuf(GetType().Assembly, "CrystalBoy.Emulator.GTK.Graphics.Play.png");

			playBuf = playBuf.ScaleSimple(16, 16, InterpType.Bilinear);
			
			playIcon = new Image(playBuf);
			
			Pixbuf pauseBuf = new Pixbuf(GetType().Assembly, "CrystalBoy.Emulator.GTK.Graphics.Pause.png");

			pauseBuf = pauseBuf.ScaleSimple(16, 16, InterpType.Bilinear);
			
			pauseIcon = new Image(pauseBuf);
			
			Pixbuf stepBuf = new Pixbuf(GetType().Assembly, "CrystalBoy.Emulator.GTK.Graphics.FrameStep.png");

			stepBuf = stepBuf.ScaleSimple(16, 16, InterpType.Bilinear);
			
			frameStepIcon = new Image(stepBuf);
			
			Pixbuf stopBuf = new Pixbuf(GetType().Assembly, "CrystalBoy.Emulator.GTK.Graphics.Stop.png");

			stopBuf = stopBuf.ScaleSimple(16, 16, InterpType.Bilinear);
			
			stopIcon = new Image(stopBuf);
			
			//InitialzieWidgets
			masterContainer = new VBox(false, 1);

			Add(masterContainer);
			
			//Toolbar
			toolbar = new MenuBar();
			masterContainer.PackStart(toolbar, false, false, 0);

			//File Toolber
			Menu fileMenu = new Menu();

			MenuItem refresh = new MenuItem("Refresh Roms");
			refresh.Activated += RefreshRomsList;

			fileMenu.Append(refresh);
			
			MenuItem quit = new MenuItem("Quit");
			quit.Activated += Quit;

			fileMenu.Append(quit);

			MenuItem fileItem = new MenuItem("File") {Submenu = fileMenu};
			toolbar.Add(fileItem);

			//Settings Toolbar
			Menu settingsMenu = new Menu();

			MenuItem settingsItem = new MenuItem("Settings") { Submenu = settingsMenu};
			toolbar.Add(settingsItem);

			MenuItem openSettings = new MenuItem("Configure");
			openSettings.Activated += OpenSettings;
			settingsMenu.Append(openSettings);
			
			//About Toolbar
			Menu aboutMenu = new Menu();

			MenuItem helpItem = new MenuItem("Help") { Submenu = aboutMenu };
			toolbar.Add(helpItem);

			MenuItem aboutItem = new MenuItem("About");
			aboutItem.Activated += OpenAbout;

			aboutMenu.Append(aboutItem);

			//Emulator Controls
			emulatorControls = new HBox {Halign = Align.Center};
			
			playPauseButton = new Button { Image = playIcon };

			frameStepButton = new Button {Image = frameStepIcon, Sensitive = false}; //Sensitive = false will gray out the button

			stopButton = new Button {Image = stopIcon, Sensitive = false};
			
			//Pack Emulator Controls
			emulatorControls.PackStart(playPauseButton, false, false, 0);
			emulatorControls.PackStart(frameStepButton, false, false, 0);
			emulatorControls.PackStart(stopButton, false, false, 0);
			masterContainer.PackStart(emulatorControls, false, false, 0);
			
			//Roms list
			romListBox = new ListBox();
			
			masterContainer.PackStart(romListBox, true, true, 0);

			ShowAll();
		}
	}
}