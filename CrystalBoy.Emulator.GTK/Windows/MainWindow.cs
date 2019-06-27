using System;
using System.IO;
using CrystalBoy.Emulator.GTK.Widgets;
using CrystalBoy.Emulator.SDL2;
using Gtk;

namespace CrystalBoy.Emulator.GTK.Windows
{
	public partial class MainWindow : Window
	{
		public static MainWindow Instance
		{
			get;
			private set;
		}
		
		SDL2Manager sdl2Manager;

		bool emulatorRunning;

		string[] romsList;

		string selectedRomPath;
		
		public MainWindow() : base("CrystalBoy GTK")
		{
			if (Instance == null)
				Instance = this;
			else
			{
				Close();
				return;
			}
			
			DeleteEvent += OnQuit;
			
			InitializeWidgets();
			
			InitializeWidgetEvents();

			RefreshRomsList(null, null);
			
			Resize(800, 600);
		}

		void InitializeWidgetEvents()
		{
			playPauseButton.Pressed += ButtonEvent_PlayPause;

			stopButton.Pressed += ButtonEvent_Stop;

			frameStepButton.Pressed += ButtonEvent_FrameStep;
		}
		
		public void RefreshRomsList(object o, EventArgs args)
		{
			//First clear the roms
			foreach (Widget child in romListBox.Children)
				romListBox.Remove(child);

			//The fetch roms list, if the search path isn't valid set it to null
			if (!string.IsNullOrWhiteSpace(Program.settings.RomsSearchPath))
			{
				try
				{
					romsList = Directory.GetFiles(Program.settings.RomsSearchPath);
				}
				catch
				{
					romsList = null;
				}
			}
			else
				romsList = null;
			
			//If there are no roms, create a list entry that opens the settings
			if (romsList == null || romsList.Length == 0)
			{
				RomItem romItem = new RomItem("Roms List Empty");

				romItem.OnButtonPress += OpenSettings;
				
				romListBox.Add(romItem);

				return;
			}

			//Populate the roms list with valid roms
			for (int i = 0; i < romsList.Length; i++)
			{
				if(!romsList[i].EndsWith(".gbc"))
					continue;
				
				RomItem romItem = new RomItem(romsList[i]);

				romItem.OnButtonPress += SelectRom;
				
				romItem.OnButtonDoublePress += LoadRom;
				
				romListBox.Add(romItem);
			}
		}
		
		void ButtonEvent_PlayPause(object o, EventArgs args)
		{
			if (emulatorRunning)
			{
				sdl2Manager.SetPause(!sdl2Manager.IsPaused);

				return;
			}

			StartEmulator(selectedRomPath);
		}
		
		void ButtonEvent_Stop(object o, EventArgs args)
		{
			StopEmulator();
		}
		
		void ButtonEvent_FrameStep(object o, EventArgs args)
		{
			sdl2Manager.FrameStep();
		}

		void OpenSettings(object o, EventArgs args)
		{
			Window.GetPosition(out int x, out int y);
			
			SettingsWindow settingsWindow = new SettingsWindow(this);
		}

		void OpenAbout(object o, EventArgs args)
		{
			Window.GetPosition(out int x, out int y);
			
			AboutWindow aboutWindow = new AboutWindow(x, y);
		}

		void SelectRom(object sender, EventArgs args)
		{
			selectedRomPath = ((RomItem) sender).romPath;
		}
		
		void LoadRom(object sender, EventArgs args)
		{
			StartEmulator(((RomItem) sender).romPath);
		}

		void StartEmulator(string romPath)
		{
			if (emulatorRunning)
				return;
			
			if (!File.Exists(romPath) || !romPath.EndsWith(".gbc"))
				return;

			emulatorRunning = true;

			Configuration.Settings settings = Program.settings;
			
			sdl2Manager = new SDL2Manager(settings);
			
			//Application.Invoke allows methods to be called on the main GUI thread.
			sdl2Manager.OnEmulatorStarted += (o, e) => Application.Invoke(OnEmulatorStart);
			
			sdl2Manager.OnEmulatorPaused += (o, e) => Application.Invoke(OnEmulatorPause);
			
			sdl2Manager.OnEmulatorResumed += (o, e) => Application.Invoke(OnEmulatorResume);
			
			sdl2Manager.OnEmulatorStop += (o, e) => Application.Invoke(OnEmulatorStop);

			sdl2Manager.Start(romPath);
		}

		void StopEmulator()
		{
			if (!emulatorRunning)
				return;

			sdl2Manager.Quit();
		}

		void Quit(object o, EventArgs args)
		{
			StopEmulator();

			Application.Quit();
		}
		
		void OnQuit(object o, EventArgs e)
		{
			StopEmulator();
		}

		void OnEmulatorStart(object o, EventArgs e)
		{
			emulatorRunning = true;
			
			playPauseButton.Image = pauseIcon;
			
			stopButton.Sensitive = true; //This enables/disables a widgets. It will appear grayed out when set to false
			
			toolbar.Sensitive = false;
		}

		void OnEmulatorPause(object o, EventArgs e)
		{
			playPauseButton.Image = playIcon;

			frameStepButton.Sensitive = true;
		}
		
		void OnEmulatorResume(object o, EventArgs e)
		{
			playPauseButton.Image = pauseIcon;
			
			frameStepButton.Sensitive = false;
		}
		
		void OnEmulatorStop(object o, EventArgs e)
		{
			emulatorRunning = false;

			playPauseButton.Image = playIcon;
			
			stopButton.Sensitive = false;

			toolbar.Sensitive = true;
			
			sdl2Manager = null;
		}
	}
}