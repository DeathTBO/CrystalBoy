using System;
using System.Drawing;
using System.IO;
using CrystalBoy.Core;
using Newtonsoft.Json;
using CrystalBoy.Emulation;
using Gdk;

namespace CrystalBoy.Emulator.Configuration 
{
    public class Settings
    {
        static readonly Settings defaultInstance = new Settings();

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        static string SaveDirectory
        {
            get
            {
                string searchPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                searchPath = Path.Join(searchPath, "CrystalBoy");

                return searchPath;
            }
        }

        static string SaveFilePath
        {
            get
            {
                return Path.Join(SaveDirectory, "Settings.json");
            }
        }

        //Todo: Add options for different renderers later
        
        // public string VideoRenderer
        // {
        //     get;
        //     set;
        // }
        //
        // string AudioRenderer
        // {
        //     get;
        //     set;
        // }
        //
        // public string MainJoypadPlugin
        // {
        //     get;
        //     set;
        // }
        
        // public System.Collections.Specialized.StringCollection PluginAssemblies
        // {
        //     get;
        //     set;
        // }
        
        public int WindowWidth
        {
            get;
            set;
        }
        
        public int WindowHeight
        {
            get;
            set;
        }
        
        public int ZoomFactor
        {
            get;
            set;
        }
        
        public bool LimitSpeed
        {
            get;
            set;
        }
        
        public HardwareType HardwareType
        {
            get;
            set;
        }
        
        public bool UseBootstrapRom
        {
            get;
            set;
        }
        
        public bool Interpolation
        {
            get;
            set;
        }
        
        public BorderVisibility BorderVisibility
        {
            get;
            set;
        }

        public string RomsSearchPath
        {
            get;
            set;
        }

        public KeyBind[] KeyBinds
        {
            get;
            set;
        }

        public struct KeyBind
        {
            public Gdk.Key gtkKey;

            public SDL2.SDL.SDL_Keycode sdlKey;
            
            public GameBoyKeys gameBoyKey;

            public KeyBind(Gdk.Key gtk, SDL2.SDL.SDL_Keycode sdl, GameBoyKeys gbk)
            {
                gtkKey = gtk;

                sdlKey = sdl;

                gameBoyKey = gbk;
            }

            public KeyBind(KeyBind keyBind)
            {
                gtkKey = keyBind.gtkKey;

                sdlKey = keyBind.sdlKey;

                gameBoyKey = keyBind.gameBoyKey;
            }
        }

        public Settings()
        {
            // VideoRenderer = "Direct2DRenderer";
            //
            // AudioRenderer = "XAudio2Renderer";
            //
            // MainJoypadPlugin = "Win32KeyboardJoypad";

            WindowWidth = 256 * 3;

            WindowHeight = 224 * 3;

            ZoomFactor = 2;

            LimitSpeed = true;

            HardwareType = HardwareType.GameBoyColor;

            UseBootstrapRom = true;

            Interpolation = false;

            BorderVisibility = BorderVisibility.Auto;

            KeyBinds = GetKeyBindDefaults();
        }

        public void SetKey(GameBoyKeys gbk, Gdk.Key gtk)
        {
            for (int i = 0; i < KeyBinds.Length; i++)
            {
                if(KeyBinds[i].gameBoyKey != gbk)
                    continue;

                KeyBinds[i].gtkKey = gtk;

                KeyBinds[i].sdlKey = MapGtkToSDL(gtk) ?? SDL2.SDL.SDL_Keycode.SDLK_F24;
            }
        }

        public Gdk.Key GetKey(GameBoyKeys gbk)
        {
            for (int i = 0; i < KeyBinds.Length; i++)
            {
                if(KeyBinds[i].gameBoyKey != gbk)
                    continue;

                return KeyBinds[i].gtkKey;
            }

            return Key.F24;
        }
        
        public static KeyBind[] GetKeyBindDefaults()
        {
            KeyBind[] keyBinds = new KeyBind[8];
            
            //KeyBinds, ignore none and all
            keyBinds[0] = new KeyBind( Gdk.Key.a, SDL2.SDL.SDL_Keycode.SDLK_a, GameBoyKeys.A);
            keyBinds[1] = new KeyBind(Gdk.Key.b, SDL2.SDL.SDL_Keycode.SDLK_b, GameBoyKeys.B);
            keyBinds[2] = new KeyBind(Gdk.Key.Up, SDL2.SDL.SDL_Keycode.SDLK_UP, GameBoyKeys.Up);
            keyBinds[3] = new KeyBind(Gdk.Key.Down, SDL2.SDL.SDL_Keycode.SDLK_DOWN, GameBoyKeys.Down);
            keyBinds[4] = new KeyBind(Gdk.Key.Left, SDL2.SDL.SDL_Keycode.SDLK_LEFT, GameBoyKeys.Left);
            keyBinds[5] = new KeyBind(Gdk.Key.Right, SDL2.SDL.SDL_Keycode.SDLK_RIGHT, GameBoyKeys.Right);
            keyBinds[6] = new KeyBind(Gdk.Key.Return, SDL2.SDL.SDL_Keycode.SDLK_RETURN, GameBoyKeys.Start);
            keyBinds[7] = new KeyBind(Gdk.Key.backslash, SDL2.SDL.SDL_Keycode.SDLK_BACKSLASH, GameBoyKeys.Select);

            return keyBinds;
        }

        public static Settings Load()
        {
            Settings settings;

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            if (!File.Exists(SaveFilePath))
            {
                settings = Default;

                string jsonText = JsonConvert.SerializeObject(settings);

                File.WriteAllText(SaveFilePath, jsonText);

                return settings;
            }

            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SaveFilePath));

            return settings;
        }

        public static void Save(Settings settings)
        {
            if (settings == null)
                return;
            
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            string json = JsonConvert.SerializeObject(settings);

            File.WriteAllText(SaveFilePath, json);
        }

        public static SDL2.SDL.SDL_Keycode? MapGtkToSDL(Gdk.Key key)
        {
            switch (key)
            {
                case Key.a:
                case Key.A:
                    return SDL2.SDL.SDL_Keycode.SDLK_a;
                
                case Key.b:
                case Key.B:
                    return SDL2.SDL.SDL_Keycode.SDLK_b;
                
                case Key.c:
                case Key.C:
                    return SDL2.SDL.SDL_Keycode.SDLK_c;
                
                case Key.d:
                case Key.D:
                    return SDL2.SDL.SDL_Keycode.SDLK_d;
                
                case Key.e:
                case Key.E:
                    return SDL2.SDL.SDL_Keycode.SDLK_e;
                
                case Key.f:
                case Key.F:
                    return SDL2.SDL.SDL_Keycode.SDLK_f;
                
                case Key.g:
                case Key.G:
                    return SDL2.SDL.SDL_Keycode.SDLK_g;
                
                case Key.h:
                case Key.H:
                    return SDL2.SDL.SDL_Keycode.SDLK_h;
                
                case Key.i:
                case Key.I:
                    return SDL2.SDL.SDL_Keycode.SDLK_i;
                
                case Key.j:
                case Key.J:
                    return SDL2.SDL.SDL_Keycode.SDLK_j;
                
                case Key.k:
                case Key.K:
                    return SDL2.SDL.SDL_Keycode.SDLK_k;
                
                case Key.l:
                case Key.L:
                    return SDL2.SDL.SDL_Keycode.SDLK_l;
                
                case Key.m:
                case Key.M:
                    return SDL2.SDL.SDL_Keycode.SDLK_m;
                
                case Key.n:
                case Key.N:
                    return SDL2.SDL.SDL_Keycode.SDLK_n;
                
                case Key.o:
                case Key.O:
                    return SDL2.SDL.SDL_Keycode.SDLK_o;
                
                case Key.p:
                case Key.P:
                    return SDL2.SDL.SDL_Keycode.SDLK_p;
                
                case Key.q:
                case Key.Q:
                    return SDL2.SDL.SDL_Keycode.SDLK_q;
                
                case Key.r:
                case Key.R:
                    return SDL2.SDL.SDL_Keycode.SDLK_r;
                
                case Key.s:
                case Key.S:
                    return SDL2.SDL.SDL_Keycode.SDLK_s;
                
                case Key.t:
                case Key.T:
                    return SDL2.SDL.SDL_Keycode.SDLK_t;
                
                case Key.u:
                case Key.U:
                    return SDL2.SDL.SDL_Keycode.SDLK_u;
                
                case Key.v:
                case Key.V:
                    return SDL2.SDL.SDL_Keycode.SDLK_v;
                
                case Key.w:
                case Key.W:
                    return SDL2.SDL.SDL_Keycode.SDLK_w;
                
                case Key.x:
                case Key.X:
                    return SDL2.SDL.SDL_Keycode.SDLK_x;
                
                case Key.y:
                case Key.Y:
                    return SDL2.SDL.SDL_Keycode.SDLK_y;
                
                case Key.z:
                case Key.Z:
                    return SDL2.SDL.SDL_Keycode.SDLK_z;
                
                case Key.Up:
                    return SDL2.SDL.SDL_Keycode.SDLK_UP;
                
                case Key.Down:
                    return SDL2.SDL.SDL_Keycode.SDLK_DOWN;
                
                case Key.Left:
                    return SDL2.SDL.SDL_Keycode.SDLK_LEFT;
                
                case Key.Right:
                    return SDL2.SDL.SDL_Keycode.SDLK_RIGHT;
                
                case Key.BackSpace:
                    return SDL2.SDL.SDL_Keycode.SDLK_BACKSPACE;
                
                case Key.backslash:
                    return SDL2.SDL.SDL_Keycode.SDLK_BACKSLASH;
                
                case Key.Return:
                    return SDL2.SDL.SDL_Keycode.SDLK_RETURN;
                
                case Key.bracketleft:
                    return SDL2.SDL.SDL_Keycode.SDLK_LEFTBRACKET;
                
                case Key.bracketright:
                    return SDL2.SDL.SDL_Keycode.SDLK_RIGHTBRACKET;
                
                case Key.semicolon:
                    return SDL2.SDL.SDL_Keycode.SDLK_SEMICOLON;
                
                case Key.quoteright: //Single Quote
                    return SDL2.SDL.SDL_Keycode.SDLK_QUOTE;
                
                case Key.comma:
                    return SDL2.SDL.SDL_Keycode.SDLK_COMMA;
                
                case Key.period:
                    return SDL2.SDL.SDL_Keycode.SDLK_PERIOD;
                
                case Key.slash:
                    return SDL2.SDL.SDL_Keycode.SDLK_SLASH;
                
                case Key.Key_0:
                    return SDL2.SDL.SDL_Keycode.SDLK_0;
                
                case Key.Key_1:
                    return SDL2.SDL.SDL_Keycode.SDLK_1;
                
                case Key.Key_2:
                    return SDL2.SDL.SDL_Keycode.SDLK_2;
                
                case Key.Key_3:
                    return SDL2.SDL.SDL_Keycode.SDLK_3;
                
                case Key.Key_4:
                    return SDL2.SDL.SDL_Keycode.SDLK_4;
                
                case Key.Key_5:
                    return SDL2.SDL.SDL_Keycode.SDLK_5;
                
                case Key.Key_6:
                    return SDL2.SDL.SDL_Keycode.SDLK_6;
                
                case Key.Key_7:
                    return SDL2.SDL.SDL_Keycode.SDLK_7;
                
                case Key.Key_8:
                    return SDL2.SDL.SDL_Keycode.SDLK_8;
                
                case Key.Key_9:
                    return SDL2.SDL.SDL_Keycode.SDLK_9;
                
                case Key.minus:
                    return SDL2.SDL.SDL_Keycode.SDLK_MINUS;
                
                case Key.equal:
                    return SDL2.SDL.SDL_Keycode.SDLK_EQUALS;
                
                case Key.quoteleft:
                    return SDL2.SDL.SDL_Keycode.SDLK_BACKQUOTE;
                
                case Key.space:
                    return SDL2.SDL.SDL_Keycode.SDLK_SPACE;
                
                case Key.F1:
                    return SDL2.SDL.SDL_Keycode.SDLK_F1;
                
                case Key.F2:
                    return SDL2.SDL.SDL_Keycode.SDLK_F2;
                
                case Key.F3:
                    return SDL2.SDL.SDL_Keycode.SDLK_F3;
                
                case Key.F4:
                    return SDL2.SDL.SDL_Keycode.SDLK_F4;
                
                case Key.F5:
                    return SDL2.SDL.SDL_Keycode.SDLK_F5;
                
                case Key.F6:
                    return SDL2.SDL.SDL_Keycode.SDLK_F6;
                
                case Key.F7:
                    return SDL2.SDL.SDL_Keycode.SDLK_F7;
                
                case Key.F8:
                    return SDL2.SDL.SDL_Keycode.SDLK_F8;
                
                case Key.F9:
                    return SDL2.SDL.SDL_Keycode.SDLK_F9;
                
                case Key.F10:
                    return SDL2.SDL.SDL_Keycode.SDLK_F10;
                
                case Key.F11:
                    return SDL2.SDL.SDL_Keycode.SDLK_F11;
                
                case Key.F12:
                    return SDL2.SDL.SDL_Keycode.SDLK_F12;
                
                case Key.KP_0:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_0;
                
                case Key.KP_1:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_1;
                
                case Key.KP_2:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_2;
                
                case Key.KP_3:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_3;
                
                case Key.KP_4:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_4;
                
                case Key.KP_5:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_5;
                
                case Key.KP_6:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_6;
                
                case Key.KP_7:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_7;
                
                case Key.KP_8:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_8;
                
                case Key.KP_9:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_9;
                
                case Key.KP_Divide:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_DIVIDE;
                
                case Key.KP_Multiply:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_MULTIPLY;
                
                case Key.KP_Subtract:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_MINUS;
                
                case Key.KP_Add:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_PLUS;
                
                case Key.KP_Enter:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_ENTER;
                
                case Key.KP_Decimal:
                    return SDL2.SDL.SDL_Keycode.SDLK_KP_DECIMAL;
                
                case Key.Shift_L:
                    return SDL2.SDL.SDL_Keycode.SDLK_LSHIFT;
                
                case Key.Shift_R:
                    return SDL2.SDL.SDL_Keycode.SDLK_RSHIFT;
                
                case Key.Control_L:
                    return SDL2.SDL.SDL_Keycode.SDLK_LCTRL;
                
                case Key.Control_R:
                    return SDL2.SDL.SDL_Keycode.SDLK_RCTRL;
                
                case Key.Alt_L:
                    return SDL2.SDL.SDL_Keycode.SDLK_LALT;
                
                case Key.Alt_R:
                    return SDL2.SDL.SDL_Keycode.SDLK_RALT;
                
                case Key.Tab:
                    return SDL2.SDL.SDL_Keycode.SDLK_TAB;
                
                case Key.Escape:
                    return SDL2.SDL.SDL_Keycode.SDLK_ESCAPE;
            }

            return null;
        }
    }
}
