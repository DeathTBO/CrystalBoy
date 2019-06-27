using CrystalBoy.Core;
using Gtk;
using Key = Gdk.Key;

namespace CrystalBoy.Emulator.GTK.Widgets
{
    public class InputKeyButton : Button
    {
        public GameBoyKeys GameBoyKey;

        public Key GtkKey;
    }
}