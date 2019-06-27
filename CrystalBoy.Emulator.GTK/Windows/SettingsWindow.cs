using System;
using CrystalBoy.Emulation;
using CrystalBoy.Emulator.GTK.Widgets;
using Gtk;
using Settings = CrystalBoy.Emulator.Configuration.Settings;

namespace CrystalBoy.Emulator.GTK.Windows
{
	public class SettingsWindow : Window
	{
		Grid masterContainer;

		Label windowSizeLabel;
		NumberEntry windowSizeWidth;
		NumberEntry windowSizeHeight;

		Label hardwareTypeLabel;
		ComboBox hardwareTypeList;

		Label limitSpeedLabel;
		CheckButton limitSpeedButton;
		
		Label useBootstrapLabel;
		CheckButton useBootstrapButton;

		Label romSearchPathLabel;
		Entry romSearchPathEntry;
		
		Button openControllerSettings;

		Button saveSettingsButton;
		Button cancelSettingsButton;
		Button resetSettingsButton;
		
		public SettingsWindow(Window parent) : base("CrystalBoy Settings")
		{
			int row = 0;
			
			masterContainer = new Grid
			{
				ColumnSpacing = 5,
				RowSpacing = 10
			};

			Add(masterContainer);
			
			//Window Size
			windowSizeLabel = new Label("Window Size:");
			
			windowSizeWidth = new NumberEntry(Program.settings.WindowWidth, 1, 999999999)
			{
				InputPurpose = InputPurpose.Digits
			};
			
			windowSizeHeight = new NumberEntry(Program.settings.WindowHeight, 1, 999999999)
			{
				InputPurpose = InputPurpose.Digits
			};
			
			masterContainer.Attach(windowSizeLabel, 0, row, 1, 1);
			masterContainer.Attach(windowSizeWidth, 1, row, 1, 1);
			masterContainer.Attach(windowSizeHeight, 2, row, 1, 1);
			row++;
			
			//Hardware Type
			hardwareTypeLabel = new Label("Hardware Type:");

			hardwareTypeList = new ComboBox(Enum.GetNames(typeof(HardwareType)));
			hardwareTypeList.Active = (int) Program.settings.HardwareType;
			hardwareTypeList.Changed += (sender, args) => { Program.settings.HardwareType = (HardwareType) hardwareTypeList.Active; };
			
			masterContainer.Attach(hardwareTypeLabel, 0, row, 1, 1);
			masterContainer.Attach(hardwareTypeList, 1, row, 1, 1);
			row++;

			//Use Bootstrap
			useBootstrapLabel = new Label("Use Bootstrap Rom:");

			useBootstrapButton = new CheckButton {Active = Program.settings.UseBootstrapRom};
			useBootstrapButton.Toggled += (sender, args) => { Program.settings.UseBootstrapRom = useBootstrapButton.Active; }; 
			
			masterContainer.Attach(useBootstrapLabel, 0, row, 1, 1);
			masterContainer.Attach(useBootstrapButton, 1, row, 1, 1);
			row++;
			
			//Limit Speed
			limitSpeedLabel = new Label("Limit Speed:");

			limitSpeedButton = new CheckButton {Active = Program.settings.LimitSpeed};
			limitSpeedButton.Toggled += (sender, args) => { Program.settings.LimitSpeed = limitSpeedButton.Active; }; 
			
			masterContainer.Attach(limitSpeedLabel, 0, row, 1, 1);
			masterContainer.Attach(limitSpeedButton, 1, row, 1, 1);
			row++;
			
			//Rom Search Path
			romSearchPathLabel = new Label("Rom Search Path:");

			romSearchPathEntry = new Entry();

			masterContainer.Attach(romSearchPathLabel, 0, row, 1, 1);
			masterContainer.Attach(romSearchPathEntry, 1, row, 1, 1);
			row++;
			
			//Open Controller Configuration
			openControllerSettings = new Button("Controller Configuration");
			openControllerSettings.Pressed += (sender, args) => { ControllerSettingsWindow controllerSettingsWindow = new ControllerSettingsWindow(this); };

			masterContainer.Attach(openControllerSettings, 1, row, 1, 1);
			row++;

			//Save, Cancel, and Reset Buttons
			resetSettingsButton = new Button { Label = "Reset to Defaults" };
			resetSettingsButton.Pressed += (sender, args) =>
			{
				Program.settings = Settings.Default;
				ReloadInterfaceValues();
			};

			cancelSettingsButton = new Button { Label = "Cancel" };
			cancelSettingsButton.Pressed += (sender, args) =>
			{
				Program.settings = Settings.Load();
				Close();
			};
			
			saveSettingsButton = new Button { Label = "Save" };
			saveSettingsButton.Pressed += Save;
			
			masterContainer.Attach(resetSettingsButton, 0, row, 1, 1);
			masterContainer.Attach(cancelSettingsButton, 2, row, 1, 1);
			masterContainer.Attach(saveSettingsButton, 3, row, 30, 1);

			//Show
			ShowAll();

			Resize(400, 300);
			
			DeleteEvent += (o, args) => { parent.Sensitive = true; };

			ReloadInterfaceValues();
			
			parent.Sensitive = false;

			parent.GetPosition(out int x, out int y);
			Move(x, y);
		}

		void Save(object sender, EventArgs args)
		{
			//GTK Entry events aren't playing nice, so instead set the values when save is pressed
			Program.settings.WindowWidth = int.Parse(windowSizeWidth.Text);
			Program.settings.WindowHeight = int.Parse(windowSizeHeight.Text);
			Program.settings.RomsSearchPath = romSearchPathEntry.Text;
				
			Settings.Save(Program.settings);
			MainWindow.Instance.RefreshRomsList(this, null);
		}

		void ReloadInterfaceValues()
		{
			windowSizeWidth.Value = Program.settings.WindowWidth;
			
			windowSizeHeight.Value = Program.settings.WindowHeight;

			useBootstrapButton.Active = Program.settings.UseBootstrapRom;

			limitSpeedButton.Active = Program.settings.LimitSpeed;

			romSearchPathEntry.Text = Program.settings.RomsSearchPath;

			hardwareTypeList.Active = (int)Program.settings.HardwareType;
		}
	}
}