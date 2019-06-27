using System;
using System.IO;
using System.Text;
using Gtk;

namespace CrystalBoy.Emulator.GTK.Widgets
{
    public class RomItem : Box
    {
        public string romPath;
        
        public event EventHandler OnButtonPress;

        public event EventHandler OnButtonDoublePress;

        Button romClick;
        
        public RomItem(string romLocation) : base(Orientation.Horizontal, 5)
        {
            romPath = romLocation;

            romClick = new Button(romPath);

            romClick.Pressed += OnPress;
            
            romClick.ButtonPressEvent += OnMultiPress;
            
            PackStart(romClick, true, true, 5);

            ShowAll();
        }

        void OnPress(object sender, EventArgs args)
        {
            OnButtonPress?.Invoke(this, args);
        }
        
        void OnMultiPress(object sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type != Gdk.EventType.TwoButtonPress)
            {
                OnButtonPress?.Invoke(this, args);
                return;
            }
            
            OnButtonDoublePress?.Invoke(this, args);
        }

        //The idea was to just grab the titles from the header
        //Pokemon Yellow shows "POKEMON YELLOW", but Pokemon Silver shows up as "POKEMON_SLVA"
        //Zelda Oracle of Ages shows up as "ZELDA NAYRUA"
        string GetRomTitle(string romLocation)
        {
            if (!File.Exists(romLocation))
                return "";
            
            byte[] rom = File.ReadAllBytes(romLocation);
            
            byte[] romTitle = new byte[15];

            int offset = 308;
            
            for (int i = 0 + offset; i < 15 + offset; i++)
            {
                romTitle[i - offset] = rom[i];
            }
            
            string utf8 = Encoding.UTF8.GetString(romTitle);

            return utf8;
        }
    }
}