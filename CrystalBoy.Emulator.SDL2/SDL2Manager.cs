using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CrystalBoy.Core;
using SDL2;

namespace CrystalBoy.Emulator.SDL2
{
	public class SDL2Manager
	{
		Thread loopThread;

		Configuration.Settings settings;
		
		string loadedRom;
		
		public bool IsPaused
		{
			get;
			private set;
		}
		
		bool quit;
		
		SDL2VideoRenderer videoRenderer;

		SDL2AudioRenderer audioRenderer;

		SDL2InputJoypad inputJoypad;
		
		SDL2KeyStateManager keyStateManager;
		
		//Emulator
		EmulatedGameBoy emulatedGameBoy;
		
		BinaryWriter ramSaveWriter;
		BinaryReader ramSaveReader;
		
		//readonly long speedUpdateTicks = Stopwatch.Frequency / 10;
		
		//readonly Stopwatch speedUpdateStopwatch;
		
		//Window
		IntPtr window;

		SDL.SDL_Event e;
		
		//Events
		public event EventHandler OnEmulatorStarted;
		
		public event EventHandler OnEmulatorPaused;

		public event EventHandler OnEmulatorResumed;
		
		public event EventHandler OnEmulatorStop;

		public SDL2Manager(Configuration.Settings emuSettings)
		{
			settings = emuSettings;
		}

		void InitializeEmulator()
		{
			emulatedGameBoy = new EmulatedGameBoy();

			emulatedGameBoy.TryUsingBootRom = settings.UseBootstrapRom;
			emulatedGameBoy.EnableFramerateLimiter = settings.LimitSpeed;

			emulatedGameBoy.Bus.AudioRenderer = audioRenderer = new SDL2AudioRenderer();;
			
			emulatedGameBoy.Bus.VideoRenderer = videoRenderer = new SDL2VideoRenderer(window);

			emulatedGameBoy.Bus.SetJoypad(0, inputJoypad = new SDL2InputJoypad(keyStateManager = new SDL2KeyStateManager()));

			try
			{
				emulatedGameBoy.Reset(settings.HardwareType);
			}
			catch
			{
				//Todo: Add something
			}	
		}

		public void Start(string rom)
		{
			loadedRom = rom;
			
			loopThread = new Thread(ThreadedStart);

			loopThread.Start();
		}

		void ThreadedStart()
		{
			if (SynchronizationContext.Current == null)
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			
			SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
			
			window = IntPtr.Zero;                        
			window = SDL.SDL_CreateWindow("CrystalBoy SDL2",
				SDL.SDL_WINDOWPOS_CENTERED,
				SDL.SDL_WINDOWPOS_CENTERED,
				settings.WindowWidth,
				settings.WindowHeight,
				SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
			);

			InitializeEmulator();
			
			keyStateManager.LoadSettings(settings);
			
			LoadRom(loadedRom);

			OnEmulatorStarted?.Invoke(null, null);

			Loop();

			CleanUp();
		}

		public void SetSettings(Configuration.Settings emulatorSettings)
		{
			//Used for runtime settings
			settings = emulatorSettings;

			if (emulatedGameBoy == null)
				return;

			bool alreadyPaused = IsPaused;
			
			emulatedGameBoy.Pause();
			
			emulatedGameBoy.EnableFramerateLimiter = settings.LimitSpeed;

			keyStateManager.LoadSettings(settings);

			if(!alreadyPaused)
				emulatedGameBoy.Run();
		}

		public void SetPause(bool pauseState)
		{
			if (pauseState && !IsPaused)
				Pause();
			else if (!pauseState && IsPaused)
				Resume();
		}

		public void FrameStep()
		{
			if (!IsPaused)
				return;

			emulatedGameBoy.RunFrame();
		}

		public void Quit()
		{
			quit = true;
		}

		void Pause()
		{
			emulatedGameBoy.Pause();
			
			IsPaused = true;

			OnEmulatorPaused?.Invoke(null, null);
		}

		void Resume()
		{
			emulatedGameBoy.Run();

			IsPaused = false;

			OnEmulatorResumed?.Invoke(null, null);
		}

		void Loop()
		{
			while (!quit)
			{
				HandleEvents();
			}
		}
		
		void HandleEvents()
		{
			while (SDL.SDL_PollEvent(out e) != 0)
			{
				switch (e.type)
				{
					case SDL.SDL_EventType.SDL_QUIT:
						quit = true;
						break;

					case SDL.SDL_EventType.SDL_KEYDOWN:
						keyStateManager.SetState(e.key.keysym.sym, true);
						break;
						
					case SDL.SDL_EventType.SDL_KEYUP:
						keyStateManager.SetState(e.key.keysym.sym, false);
						break;
					
					case SDL.SDL_EventType.SDL_WINDOWEVENT:
						videoRenderer.RecalculateDrawRectangle();
						break;
				}
			}
		}
		
		void UnloadRom()
		{
			emulatedGameBoy.Pause();

			if (ramSaveWriter != null)
			{
				WriteRam();

				ramSaveWriter.Close();
				ramSaveWriter = null;

				if (ramSaveReader != null)
					ramSaveReader.Close();
				
				ramSaveReader = null;
			}

			emulatedGameBoy.UnloadRom();
		}
		
		void LoadRom(string fileName)
		{
			var romFileInfo = new FileInfo(fileName);

			// Open only existing rom files
			if (!romFileInfo.Exists)
				throw new FileNotFoundException();
			if (romFileInfo.Length < 512)
				throw new InvalidOperationException("ROM files must be at least 512 bytes.");
			if (romFileInfo.Length > 8 * 1024 * 1024)
				throw new InvalidOperationException("ROM files cannot exceed 8MB.");

			emulatedGameBoy.LoadRom(MemoryUtility.ReadFile(romFileInfo, true));

			if (emulatedGameBoy.RomInformation.HasRam && emulatedGameBoy.RomInformation.HasBattery)
			{
				FileInfo ramFileInfo = new FileInfo(System.IO.Path.Combine(romFileInfo.DirectoryName, System.IO.Path.GetFileNameWithoutExtension(romFileInfo.Name)) + ".sav");
				FileStream ramSaveStream = ramFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
				
				ramSaveStream.SetLength(emulatedGameBoy.Mapper.SavedRamSize + (emulatedGameBoy.RomInformation.HasTimer ? 48 : 0));
				ramSaveStream.Read(emulatedGameBoy.ExternalRam, 0, emulatedGameBoy.Mapper.SavedRamSize);
				ramSaveWriter = new BinaryWriter(ramSaveStream);

				if (emulatedGameBoy.RomInformation.HasTimer)
				{
					var mbc3 = emulatedGameBoy.Mapper as CrystalBoy.Emulation.Mappers.MemoryBankController3;

					if (mbc3 != null)
					{
						var rtcState = mbc3.RtcState;
						ramSaveReader = new BinaryReader(ramSaveStream);

						rtcState.Frozen = true;

						rtcState.Seconds = (byte)ramSaveReader.ReadInt32();
						rtcState.Minutes = (byte)ramSaveReader.ReadInt32();
						rtcState.Hours = (byte)ramSaveReader.ReadInt32();
						rtcState.Days = (short)((byte)ramSaveReader.ReadInt32() + ((byte)ramSaveReader.ReadInt32() << 8));

						rtcState.LatchedSeconds = (byte)ramSaveReader.ReadInt32();
						rtcState.LatchedMinutes = (byte)ramSaveReader.ReadInt32();
						rtcState.LatchedHours = (byte)ramSaveReader.ReadInt32();
						rtcState.LatchedDays = (short)((byte)ramSaveReader.ReadInt32() + ((byte)ramSaveReader.ReadInt32() << 8));

						rtcState.DateTime = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(ramSaveReader.ReadInt64());

						rtcState.Frozen = false;
					}
				}

				emulatedGameBoy.Mapper.RamUpdated += Mapper_RamUpdated;
			}

			emulatedGameBoy.Run();
		}
		
		void Mapper_RamUpdated(object sender, EventArgs e) { if (ramSaveWriter != null) WriteRam(); }
		
		void WriteRam()
		{
			ramSaveWriter.Seek(0, SeekOrigin.Begin);
			ramSaveWriter.Write(emulatedGameBoy.ExternalRam, 0, emulatedGameBoy.Mapper.SavedRamSize);
			if (emulatedGameBoy.RomInformation.HasTimer)
			{
				var mbc3 = emulatedGameBoy.Mapper as CrystalBoy.Emulation.Mappers.MemoryBankController3;

				if (mbc3 != null)
				{
					var rtcState = mbc3.RtcState;

					// I'll save the date using the same format as VBA in order to be more compatible, but i originally planned to store it whithout wasting bytes…
					// Luckily enough, it seems we use the same iternal representation… (But there probably is no other way to do it)

					ramSaveWriter.Write((int)rtcState.Seconds & 0xFF);
					ramSaveWriter.Write((int)rtcState.Minutes & 0xFF);
					ramSaveWriter.Write((int)rtcState.Hours & 0xFF);
					ramSaveWriter.Write((int)rtcState.Days & 0xFF);
					ramSaveWriter.Write((rtcState.Days >> 8) & 0xFF);

					ramSaveWriter.Write((int)rtcState.LatchedSeconds & 0xFF);
					ramSaveWriter.Write((int)rtcState.LatchedMinutes & 0xFF);
					ramSaveWriter.Write((int)rtcState.LatchedHours & 0xFF);
					ramSaveWriter.Write((int)rtcState.LatchedDays & 0xFF);
					ramSaveWriter.Write((rtcState.LatchedDays >> 8) & 0xFF);

					ramSaveWriter.Write((long)((rtcState.DateTime - new DateTime(1970, 1, 1)).TotalSeconds));
				}
			}
		}

		void CleanUp()
		{
			OnEmulatorStarted = null;
			OnEmulatorPaused = null;
			OnEmulatorResumed = null;

			emulatedGameBoy.Pause();
			UnloadRom();
			
			emulatedGameBoy.Dispose();
			emulatedGameBoy = null;

			videoRenderer.Dispose();
			videoRenderer = null;
			
			audioRenderer.Dispose();
			audioRenderer = null;
			
			inputJoypad.Dispose();
			inputJoypad = null;
			
			SDL.SDL_DestroyWindow(window);
			window = IntPtr.Zero;

			SDL.SDL_Quit();
			
			OnEmulatorStop?.Invoke(null, null);
		}
	}
}