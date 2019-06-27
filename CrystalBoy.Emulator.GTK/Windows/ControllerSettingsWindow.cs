using System;
using CrystalBoy.Core;
using CrystalBoy.Emulator.GTK.Widgets;
using Gtk;
using Key = Gdk.Key;
using Settings = CrystalBoy.Emulator.Configuration.Settings;

namespace CrystalBoy.Emulator.GTK.Windows
{
    public class ControllerSettingsWindow : Window
    {
        Configuration.Settings.KeyBind[] tempKeyBinds;
        
        Grid masterContainer;
        
        //The GameBoy has only 8 buttons
        Label aKeyLabel;
        InputKeyButton aKey;

        Label bKeyLabel;
        InputKeyButton bKey;
        
        Label upKeyLabel;
        InputKeyButton upKey;
        
        Label downKeyLabel;
        InputKeyButton downKey;
        
        Label leftKeyLabel;
        InputKeyButton leftKey;
        
        Label rightKeyLabel;
        InputKeyButton rightKey;
        
        Label startKeyLabel;
        InputKeyButton startKey;
        
        Label selectKeyLabel;
        InputKeyButton selectKey;

        int buttonCount;
        
        //Save/Cancel/Reset
        Button okSettingsButton;
        Button cancelSettingsButton;
        Button resetSettingsButton;
        
        public ControllerSettingsWindow(Window parent) : base("CrystalBoy Controller Settings")
        {
            tempKeyBinds = new Settings.KeyBind[8];

            for (int i = 0; i < tempKeyBinds.Length; i++)
                tempKeyBinds[i] = new Settings.KeyBind(Program.settings.KeyBinds[i]);
            
            masterContainer = new Grid
            {
                ColumnSpacing = 5,
                RowSpacing = 10
            };
            
            Add(masterContainer);
            
            SetupInputButton(ref aKeyLabel, ref aKey, GameBoyKeys.A);
            SetupInputButton(ref bKeyLabel, ref bKey, GameBoyKeys.B);
            SetupInputButton(ref upKeyLabel, ref upKey, GameBoyKeys.Up);
            SetupInputButton(ref downKeyLabel, ref downKey, GameBoyKeys.Down);
            SetupInputButton(ref leftKeyLabel, ref leftKey, GameBoyKeys.Left);
            SetupInputButton(ref rightKeyLabel, ref rightKey, GameBoyKeys.Right);
            SetupInputButton(ref startKeyLabel, ref startKey, GameBoyKeys.Start);
            SetupInputButton(ref selectKeyLabel, ref selectKey, GameBoyKeys.Select);
            
            //Save, Cancel, and Reset Buttons
            resetSettingsButton = new Button { Label = "Reset to Defaults" };
            resetSettingsButton.Pressed += (sender, args) =>
            {
                Program.settings.KeyBinds = Settings.GetKeyBindDefaults();
                ReloadInterfaceValues();
            };

            cancelSettingsButton = new Button { Label = "Cancel" };
            cancelSettingsButton.Pressed += (sender, args) =>
            {
                Program.settings.KeyBinds = tempKeyBinds;
                Close();
            };
			
            okSettingsButton = new Button { Label = "Ok" };
            okSettingsButton.Pressed += (sender, args) => { Close(); };
			
            masterContainer.Attach(resetSettingsButton, 0, 9, 1, 1);
            masterContainer.Attach(cancelSettingsButton, 2, 9, 1, 1);
            masterContainer.Attach(okSettingsButton, 3, 9, 30, 1);
            
            //Configure Window
            Resize(300, 300);
            
            ShowAll();

            DeleteEvent += (o, args) => { parent.Sensitive = true; }; 
            
            parent.Sensitive = false;
            
            parent.GetPosition(out int x, out int y);
            Move(x, y);
        }

        void SetupInputButton(ref Label label, ref InputKeyButton button, GameBoyKeys key)
        {
            label = new Label(key.ToString());
            button = new InputKeyButton { Label = Program.settings.GetKey(key).ToString(), GameBoyKey = key};
            button.Pressed += ActivateButton;
            button.KeyPressEvent += OnKeyPress;

            masterContainer.Attach(label, 0, buttonCount, 1, 1);
            masterContainer.Attach(button, 1, buttonCount, 1, 1);

            buttonCount++;
        }
        
        void ReloadInterfaceValues()
        {
            aKey.Label = Program.settings.GetKey(GameBoyKeys.A).ToString();
            bKey.Label = Program.settings.GetKey(GameBoyKeys.B).ToString();
            upKey.Label = Program.settings.GetKey(GameBoyKeys.Up).ToString();
            downKey.Label = Program.settings.GetKey(GameBoyKeys.Down).ToString();
            leftKey.Label = Program.settings.GetKey(GameBoyKeys.Left).ToString();
            rightKey.Label = Program.settings.GetKey(GameBoyKeys.Right).ToString();
            startKey.Label = Program.settings.GetKey(GameBoyKeys.Start).ToString();
            selectKey.Label = Program.settings.GetKey(GameBoyKeys.Select).ToString();
        }

        void ActivateButton(object sender, EventArgs args) //This will focus the button
        {
            ((Button)sender).Label = "";
        }

        void OnKeyPress(object sender, KeyPressEventArgs args)
        {
            InputKeyButton ikb = (InputKeyButton)sender;
            
            if (Configuration.Settings.MapGtkToSDL(args.Event.Key) == null)
                return;
            
            Focus = null; //Unfocus the button
            
            ikb.Label = args.Event.Key.ToString();

            Program.settings.SetKey(ikb.GameBoyKey, args.Event.Key);
        }
    }
}