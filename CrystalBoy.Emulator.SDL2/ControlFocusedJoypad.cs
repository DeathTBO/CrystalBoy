using CrystalBoy.Core;
using System;
using System.Threading;

namespace CrystalBoy.Emulator.SDL2
{
	public abstract class ControlFocusedJoypad : ControlBasedPlugin, IJoypad
	{
		public abstract GameBoyKeys DownKeys { get; }

		protected int JoypadIndex { get; }

		public ControlFocusedJoypad(int joypadIndex) : base ()
		{
			JoypadIndex = joypadIndex;
		}
		
		public override void Dispose()
		{
			base.Dispose();
		}
	}
}