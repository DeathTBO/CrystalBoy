using CrystalBoy.Emulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CrystalBoy.Emulator.SDL2
{
	public abstract class ControlVideoRenderer : ControlBasedPlugin, IVideoRenderer
	{
		private volatile bool _borderVisible;

		public bool BorderVisible
		{
			get { return _borderVisible; }
			set
			{
				if (_borderVisible != (_borderVisible = value))
				{
					OnBorderVisibilityChanged();
				}
			}
		}

		public ControlVideoRenderer()
		{
			
		}
		
		protected virtual void OnBorderVisibilityChanged() { }

		public abstract void Refresh();

		public abstract Task RenderBorderAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken);

		public abstract Task RenderFrameAsync(VideoFrameRenderer renderer, VideoFrameData frame, CancellationToken cancellationToken);

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}