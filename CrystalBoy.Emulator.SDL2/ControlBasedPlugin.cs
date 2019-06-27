using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalBoy.Emulator.SDL2
{
	public abstract class ControlBasedPlugin : IDisposable
	{
		protected SynchronizationContext SynchronizationContext { get; }

		internal ControlBasedPlugin()
		{
			SynchronizationContext = SynchronizationContext.Current;
		}

		public virtual void Dispose() { }
	}
}