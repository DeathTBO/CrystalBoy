using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CrystalBoy.Core;
using CrystalBoy.Emulation;
using SDL2;

namespace CrystalBoy.Emulator.SDL2
{
	public class SDL2VideoRenderer : ControlVideoRenderer
	{
		readonly SynchronizationContext synchronizationContext;

		readonly IntPtr window;
		
		IntPtr renderer;
		
		//Border
		readonly byte[] borderBuffer;
		IntPtr borderPtr;
		SDL.SDL_Surface borderSurface;
		SDL.SDL_Rect sBorderRect;
		SDL.SDL_Rect tBorderRect;

		//Screen
		readonly byte[] screenBuffer;
		IntPtr screenPtr;
		SDL.SDL_Surface screenSurface;
		SDL.SDL_Rect sScreenRect;
		SDL.SDL_Rect tScreenRect;

		public SDL2VideoRenderer(IntPtr window)
		{
			this.window = window;
			
			synchronizationContext = SynchronizationContext.Current;
			
			borderBuffer = new byte[256 * 224 * 4];
			
			screenBuffer = new byte[160 * 144 * 4];
			
			renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

			InitializeSurfaces();
		}

		static T Struct<T>(IntPtr pointer)
		{
			return (T)Marshal.PtrToStructure(pointer, typeof(T));
		}

		void InitializeSurfaces()
		{
			SDL.SDL_GetWindowSize(window, out int w, out int h);
			
			//Border
			sBorderRect = new SDL.SDL_Rect
			{
				w = 256,
				h = 224,
				x = 0,
				y = 0
			};
			
			tBorderRect = new SDL.SDL_Rect
			{
				w = w,
				h = h,
				x = 0,
				y = 0
			};
			
			borderPtr = SDL.SDL_CreateRGBSurface(0, 256, 224, 32, 0, 0,0, 0);

			borderSurface = Struct<SDL.SDL_Surface>(borderPtr);

			borderSurface.pitch = 256 * 4;

			//Screen
			sScreenRect = new SDL.SDL_Rect
			{
				w = 160,
				h = 144,
				x = 0,
				y = 0
			};
			
			tScreenRect = new SDL.SDL_Rect
			{
				w = 160,
				h = 144,
				x = 0,
				y = 0
			};
			
			screenPtr = SDL.SDL_CreateRGBSurface(0, 160, 144, 32, 0, 0,0, 0);

			screenSurface = Struct<SDL.SDL_Surface>(screenPtr);

			screenSurface.pitch = 160 * 4;
		}

		public void RecalculateDrawRectangle()
		{
			SDL.SDL_GetWindowSize(window, out int w, out int h);

			int s;

			if ((s = h * 256 / 224) <= w)
				tBorderRect = new SDL.SDL_Rect {x = (w - s) / 2, y = 0, w = s, h = h};
			else if ((s = w * 224 / 256) <= h)
				tBorderRect = new SDL.SDL_Rect{x = 0, y = (h - s) / 2, w = w, h = s};
			else
				tBorderRect = new SDL.SDL_Rect{x = 0, y = 0, w = w, h = h};
			
			if ((s = h * 160 / 144) <= w)
				tScreenRect = new SDL.SDL_Rect {x = (w - s) / 2, y = 0, w = s, h = h };
			else if ((s = w * 144 / 160) <= h)
				tScreenRect = new SDL.SDL_Rect{x= 0, y = (h - s) / 2, w = w, h = s};
			else
				tScreenRect = new SDL.SDL_Rect{x = 0, y = 0, w = w, h = h};
		}
		
		void Render()
		{
			//100, 149, 237, 255 = Cornflower Blue
			
			//Clear the screen
			SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
			SDL.SDL_RenderClear(renderer);
			
			//Create textures from surfaces
			IntPtr border = SDL.SDL_CreateTextureFromSurface(renderer, borderPtr);
			IntPtr screen = SDL.SDL_CreateTextureFromSurface(renderer, screenPtr);

			//Paint the screen
			SDL.SDL_RenderCopy(renderer, border, ref sBorderRect, ref tBorderRect);
			SDL.SDL_RenderCopy(renderer, screen, ref sScreenRect, ref tScreenRect);

			SDL.SDL_RenderPresent(renderer);
			
			//Cleanup
			SDL.SDL_DestroyTexture(border);
			SDL.SDL_DestroyTexture(screen);
		}

		void WriteBorderBuffer()
		{
			for (int i = 0; i < borderBuffer.Length; i += 4)
			{
				Marshal.WriteByte(borderSurface.pixels, i, borderBuffer[i]);

				Marshal.WriteByte(borderSurface.pixels, i + 1, borderBuffer[i + 1]);

				Marshal.WriteByte(borderSurface.pixels, i + 2, borderBuffer[i + 2]);

				Marshal.WriteByte(borderSurface.pixels, i + 3, borderBuffer[i + 3]);
			}
		}

		void WriteScreenBuffer()
		{
			for (int i = 0; i < screenBuffer.Length; i += 4)
			{
				Marshal.WriteByte(screenSurface.pixels, i, screenBuffer[i]);

				Marshal.WriteByte(screenSurface.pixels, i + 1, screenBuffer[i + 1]);

				Marshal.WriteByte(screenSurface.pixels, i + 2, screenBuffer[i + 2]);

				Marshal.WriteByte(screenSurface.pixels, i + 3, screenBuffer[i + 3]);
			}
		}

		void UpdateScreenAndRender(object state)
		{
			WriteScreenBuffer();

			Render();

			HandleCompletion(state);
		}

		void UpdateBorderAndRender(object state)
		{
			WriteBorderBuffer();
			
			Render();

			HandleCompletion(state);
		}

		void HandleCompletion(object state)
		{
			TaskCompletionSource<bool> tcs = state as TaskCompletionSource<bool>;

			if (tcs != null)
			{
				if(renderer == IntPtr.Zero)
					tcs.TrySetCanceled();
				else
					tcs.TrySetResult(true);
			}
		}
		
		public override unsafe Task RenderFrameAsync(VideoFrameRenderer vfRenderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;
			
			fixed (void* p = screenBuffer)
			{
				vfRenderer.RenderVideoFrame32(frame, (IntPtr)p, 160 * 4);
			}

			if (cancellationToken.IsCancellationRequested) return TaskHelper.CanceledTask;

			// Setting up a task completion source may not be needed here, but I don't know if there's something to gain by not doing so.
			if (synchronizationContext != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				synchronizationContext.Post(UpdateScreenAndRender, tcs);
				return tcs.Task;
			}
			else
			{
				UpdateScreenAndRender(null);
				return TaskHelper.TrueTask;
			}
		}
		
		public override unsafe Task RenderBorderAsync(VideoFrameRenderer vfRenderer, VideoFrameData frame, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			fixed (void* p = borderBuffer)
			{
				vfRenderer.RenderVideoBorder32(frame, (IntPtr)p, 256 * 4);
			}
			
			cancellationToken.ThrowIfCancellationRequested();

			// Setting up a task completion source may not be needed here, but I don't know if there's something to gain by not doing so.
			if (synchronizationContext != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				synchronizationContext.Post(UpdateBorderAndRender, tcs);
				return tcs.Task;
			}
			else
			{
				UpdateBorderAndRender(null);
				return TaskHelper.TrueTask;
			}
		}
		
		public override void Refresh()
		{
			//This doesn't seem to be called ever
		}
		
		public override void Dispose()
		{
			SDL.SDL_free(SDL.SDL_malloc(borderSurface.pixels));
			
			SDL.SDL_FreeSurface(borderPtr);
			borderPtr = IntPtr.Zero;
			
			SDL.SDL_free(SDL.SDL_malloc(screenSurface.pixels));
			
			SDL.SDL_FreeSurface(screenPtr);
			screenPtr = IntPtr.Zero;

			SDL.SDL_DestroyRenderer(renderer);
			renderer = IntPtr.Zero;
			
			base.Dispose();
		}
	}
}