﻿using System;
using System.Threading;
using System.Diagnostics;
using CrystalBoy.Core;
using CrystalBoy.Emulation;
using System.ComponentModel;

namespace CrystalBoy.Emulator.SDL2
{
	[DesignerCategory("Component")]
	sealed class EmulatedGameBoy : IComponent, IClockManager, IDisposable
	{
		public const double ReferenceFrameRate = 60;

		private readonly SynchronizationContext synchronizationContext;
		
		private GameBoyMemoryBus bus;
		private EmulationStatus emulationStatus;
		private Stopwatch frameStopwatch;
		private Stopwatch frameRateStopwatch;
		private double lastFrameTime;
		private double currentFrameTime;
		private bool enableFramerateLimiter;

		public event EventHandler AfterReset;
		public event EventHandler RomChanged;
		public event EventHandler Paused;
		public event EventHandler Break;
		public event EventHandler EmulationStatusChanged;

		/// <summary>Notify the execution of a frame.</summary>
		/// <remarks>
		/// This event is raised on the processor emulation thread.
		/// This event should be handled in a thread-safe manner, and as lightly as possible, to not stall the emulation.
		/// </remarks>
		public event EventHandler NewFrame
		{
			add { bus.FrameDone += value; }
			remove { bus.FrameDone -= value; }
		}

		/// <summary>Notify the change of the SGB border.</summary>
		public event EventHandler BorderChanged;

		public EmulatedGameBoy() : this(null) { }

		public EmulatedGameBoy(IContainer container)
		{
			synchronizationContext = SynchronizationContext.Current;
			bus = new GameBoyMemoryBus();
			frameStopwatch = new Stopwatch();
			frameRateStopwatch = new Stopwatch();
			bus.EmulationStarted += OnEmulationStarted;	
			bus.EmulationStopped += OnEmulationStopped;
			bus.BorderChanged += OnBorderChanged;
			bus.ClockManager = this;
			emulationStatus = bus.UseBootRom ? EmulationStatus.Paused : EmulationStatus.Stopped;
			container?.Add(this);
		}

		public void Dispose()
		{
			if (bus != null)
			{
				bus.Dispose();
				bus = null;
				Disposed?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool IsDisposed => bus == null;
		
		/// <summary>Raises an event on the main thread.</summary>
		/// <param name="handler"></param>
		private void NotifyMainThread(EventHandler handler)
		{
			if (handler != null)
			{
				synchronizationContext.Post(state => ((EventHandler)state)(this, EventArgs.Empty), handler);
			}
		}

		#region IComponent Implementation

		public event EventHandler Disposed;

		public ISite Site { get; set; }

		#endregion

		#region IClockManager Implementation

		void IClockManager.Reset()
		{
			lastFrameTime = 0;
			currentFrameTime = 0;

			frameRateStopwatch.Reset();
			frameRateStopwatch.Start();
			frameStopwatch.Reset();
			frameStopwatch.Start();
		}

		void IClockManager.Wait()
		{
			if (enableFramerateLimiter)
			{
				long timer = frameStopwatch.ElapsedTicks;

				if (timer < GameBoyClockManager.ApproximateFrameTickDuration)  // Exact timing for one frame at 60fps is 16⅔ ms
				{
					// Windows System Timer runs at 15.6ms by default, so we only use Sleep() when there are more than 15ms remaining
					if (timer < TimeSpan.TicksPerMillisecond)
					{
						// Request to sleep for 1ms, thus waking at the next timer interval (max 15.6ms of wait)
						// Note that this will suck if another application did increase the timer precision…
						Thread.Sleep(1);
					}

					// Do some active wait, even though this is bad…
					while (frameStopwatch.ElapsedTicks < GameBoyClockManager.ApproximateFrameTickDuration)
					{
						Thread.SpinWait(1000);
					}

					frameStopwatch.Stop();
					frameStopwatch.Start();
				}
			}

			lastFrameTime = currentFrameTime;
			currentFrameTime = frameRateStopwatch.ElapsedTicks;

			frameStopwatch.Reset();
			frameStopwatch.Start();
		}

		#endregion

		private void OnEmulationStarted(object sender, EventArgs e) => synchronizationContext.Post(HandleEmulationStarted, null);
		private void OnEmulationStopped(object sender, EventArgs e) => synchronizationContext.Post(HandleEmulationStopped, null);
		private void OnBorderChanged(object sender, EventArgs e) => NotifyMainThread(BorderChanged);

		private void HandleEmulationStarted(object state) => EmulationStatus = EmulationStatus.Running;
		private void HandleEmulationStopped(object state) => Pause(!IsDisposed && Processor.Status == ProcessorStatus.Running);

		public void Reset() { Reset(bus.HardwareType); }

		public void Reset(HardwareType hardwareType)
		{
			bus.Reset(hardwareType);
			if (emulationStatus == EmulationStatus.Stopped && bus.UseBootRom) EmulationStatus = EmulationStatus.Paused;
			OnAfterReset(EventArgs.Empty);
		}

		public void LoadRom(MemoryBlock rom)
		{
			emulationStatus = EmulationStatus.Stopped;
			bus.LoadRom(rom);
			emulationStatus = EmulationStatus.Paused;
			OnRomChanged(EventArgs.Empty);
		}

		public void UnloadRom()
		{
			emulationStatus = EmulationStatus.Stopped;
			bus.UnloadRom();
			emulationStatus = bus.UseBootRom ? EmulationStatus.Paused : EmulationStatus.Stopped;
		}

		public bool TryUsingBootRom
		{
			get { return bus.TryUsingBootRom; }
			set { bus.TryUsingBootRom = value; }
		}

		public HardwareType HardwareType => bus.HardwareType;
		public RomInformation RomInformation => bus.RomInformation;
		public bool IsRomLoaded => bus.RomLoaded;
		public bool HasCustomBorder => bus.HasCustomBorder;
		public GameBoyMemoryBus Bus => bus;
		public Mapper Mapper => bus.Mapper;
		public Processor Processor => bus.Processor;
		public MemoryBlock ExternalRam => bus.ExternalRam;

		public EmulationStatus EmulationStatus
		{
			get { return emulationStatus; }
			private set
			{
				if (value != emulationStatus)
				{
					emulationStatus = value;
					OnEmulationStatusChanged(EventArgs.Empty);
				}
			}
		}

		public double EmulatedFrameRate => emulationStatus == EmulationStatus.Running ? Stopwatch.Frequency / (currentFrameTime - lastFrameTime) : 0;

		public double EmulatedSpeed => EmulatedFrameRate / ReferenceFrameRate;

		public bool EnableFramerateLimiter
		{
			get { return enableFramerateLimiter; }
			set { enableFramerateLimiter = value; }
		}

		public void Step()
		{
			if (EmulationStatus == EmulationStatus.Paused)
			{
				Bus.Step();
				OnBreak(EventArgs.Empty);
			}
		}

		public void RunFrame()
		{
			if (EmulationStatus == EmulationStatus.Paused)
				RunFrameInternal();
		}

		public void Run()
		{
			bus.Run();
		}

		public void Pause() { if (!IsDisposed) bus.Stop(); }

		private void Pause(bool breakpoint)
		{
			EmulationStatus = EmulationStatus.Paused;

			if (breakpoint) OnBreak(EventArgs.Empty);
			else OnPause(EventArgs.Empty);
		}

		private void RunFrameInternal() { bus.RunFrame(); }

		private void OnRomChanged(EventArgs e) => RomChanged?.Invoke(this, e);
		private void OnPause(EventArgs e) => Paused?.Invoke(this, e);
		private void OnBreak(EventArgs e) => Break?.Invoke(this, e);
		private void OnAfterReset(EventArgs e) => AfterReset?.Invoke(this, e);
		private void OnEmulationStatusChanged(EventArgs e) => EmulationStatusChanged?.Invoke(this, e);
	}
}
