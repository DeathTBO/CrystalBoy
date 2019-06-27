using CrystalBoy.Core;
using System;

namespace CrystalBoy.Emulator.SDL2
{
	public sealed class SDL2InputJoypad : ControlFocusedJoypad
	{
		readonly SDL2KeyStateManager keyStateManager;

		SDL2InputJoypad(int joypadIndex = 0) : base(joypadIndex)
		{
			if (joypadIndex < 0 || joypadIndex > 3) throw new ArgumentOutOfRangeException(nameof(joypadIndex));

			//_controller = new Controller((UserIndex) joypadIndex);
		}
		
		public SDL2InputJoypad(SDL2KeyStateManager sdl2KeyStateManager, int joypadIndex = 0) : base(joypadIndex)
		{
			keyStateManager = sdl2KeyStateManager;
			
			if (joypadIndex < 0 || joypadIndex > 3) throw new ArgumentOutOfRangeException(nameof(joypadIndex));

			//_controller = new Controller((UserIndex) joypadIndex);
		}

		public override GameBoyKeys DownKeys
		{
			get
			{
				GameBoyKeys keys = GameBoyKeys.None;

				for (int i = 0; i < 10; i++)
				{
					GameBoyKeys gbKey = keyStateManager.MapGBKey(i);
					
					bool state = keyStateManager.GetState(gbKey);
					
					if (state)
					{
						keys |= gbKey;
					}
				}

				//Console.WriteLine(keys);

				return keys;
			}
		}
	}
}
