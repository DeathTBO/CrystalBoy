using System;
using CrystalBoy.Core;
using SDL2;

namespace CrystalBoy.Emulator.SDL2
{
	public class SDL2KeyStateManager
	{ 
		readonly Keys[] keys;

		struct Keys
		{
			public bool state;

			public SDL.SDL_Keycode key;

			public GameBoyKeys gbKey;
		}
		
		public SDL2KeyStateManager()
		{
			keys = new Keys[10];

			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = new Keys
				{
					gbKey = MapGBKey(i)
				};
			}
		}

		public void LoadSettings(Configuration.Settings settings)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				for (int j = 0; j < settings.KeyBinds.Length; j++)
				{
					if(keys[i].gbKey != settings.KeyBinds[j].gameBoyKey)
						continue;

					keys[i].key = (SDL.SDL_Keycode)(settings.KeyBinds[j].sdlKey);
				}
			}
		}

		public void SetState(SDL.SDL_Keycode keyCode, bool state)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if(keys[i].key != keyCode)
					continue;

				keys[i].state = state;

				break;
			}
		}

		public bool GetState(SDL.SDL_Keycode keyCode)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if(keys[i].key != keyCode)
					continue;

				return keys[i].state;
			}

			return false;
		}
		
		public bool GetState(GameBoyKeys gbKey)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (keys[i].gbKey != gbKey)
					continue;

				Console.WriteLine(keys[i].key);
				
				return keys[i].state;
			}

			return false;
		}

		public int MapGBKey(GameBoyKeys gbKey)
		{
			switch (gbKey)
			{
				default:
				case GameBoyKeys.None: return 0;
				
				case GameBoyKeys.Right: return 1;
				
				case GameBoyKeys.Left: return 2;
				
				case GameBoyKeys.Up: return 3;
				
				case GameBoyKeys.Down: return 4;
				
				case GameBoyKeys.A: return 5;
				
				case GameBoyKeys.B: return 6;
				
				case GameBoyKeys.Start: return 7;
				
				case GameBoyKeys.Select: return 8;
				
				case GameBoyKeys.All: return 9;
			}
		}
		
		public GameBoyKeys MapGBKey(int index)
		{
			switch (index)
			{
				default:
				case 0: return GameBoyKeys.None;
				
				case 1: return GameBoyKeys.Right;
				
				case 2: return GameBoyKeys.Left;
				
				case 3: return GameBoyKeys.Up;
				
				case 4: return GameBoyKeys.Down;
				
				case 5: return GameBoyKeys.A;
				
				case 6: return GameBoyKeys.B;
				
				case 7: return GameBoyKeys.Start;
				
				case 8: return GameBoyKeys.Select;
				
				case 9: return GameBoyKeys.All;
			}
		}
	}
}