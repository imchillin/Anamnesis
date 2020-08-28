// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using ConceptMatrix.Memory.Marshalers;
	using ConceptMatrix.Memory.Offsets;
	using ConceptMatrix.Memory.Process;

	using SysProcess = System.Diagnostics.Process;

	public class MarshalerService
	{
		public Func<Task<SysProcess>>? SelectProcessCallback;
		public Action<Exception>? ErrorCallback;
		public Action<string>? LogCallback;

		private static MarshalerService? instance;
		private static Dictionary<Type, Type> marshalerLookup = new Dictionary<Type, Type>();

		private bool isActive;
		private ulong tickCount = 0;

		public bool ProcessIsAlive
		{
			get;
			private set;
		}

		public IProcess? Process
		{
			get;
			private set;
		}

		public string GamePath
		{
			get
			{
				if (this.Process == null)
					throw new Exception("No game process");

				return Path.GetDirectoryName(this.Process.ExecutablePath) + "\\..\\";
			}
		}

		internal static MarshalerService Instance
		{
			get
			{
				if (instance == null)
					throw new Exception("No Anamnesis Service");

				return instance;
			}
		}

		public static void AddMarshaler<TType, TMarshaler>()
			where TMarshaler : IMarshaler<TType>
		{
			Type memoryType = typeof(TType);

			if (marshalerLookup.ContainsKey(memoryType))
				throw new Exception("Marshaler already registered for type: " + memoryType);

			marshalerLookup.Add(memoryType, typeof(TMarshaler));
		}

		public async Task Initialize<TProcess>()
			where TProcess : IProcess
		{
			instance = this;
			this.isActive = true;

			AddMarshaler<ActorTypes, ActorTypesMarshaler>();
			AddMarshaler<Appearance, AppearanceMarshaler>();
			AddMarshaler<bool, BoolMarshaler>();
			AddMarshaler<byte, ByteMarshaler>();
			AddMarshaler<Color4, Color4Marshaler>();
			AddMarshaler<Color, ColorMarshaler>();
			AddMarshaler<Equipment, EquipmentMarshaler>();
			AddMarshaler<Flag, FlagMarshaler>();
			AddMarshaler<float, FloatMarshaler>();
			AddMarshaler<int, IntMarshaler>();
			AddMarshaler<Quaternion, QuaternionMarshaler>();
			AddMarshaler<short, ShortMarshaler>();
			AddMarshaler<string, StringMarshaler>();
			AddMarshaler<Transform, TransformMarshaler>();
			AddMarshaler<ushort, UShortMarshaler>();
			AddMarshaler<Vector2D, Vector2DMarshaler>();
			AddMarshaler<Vector, VectorMarshaler>();
			AddMarshaler<Weapon, WeaponMarshaler>();

			this.Process = Activator.CreateInstance<TProcess>();

			while (!this.ProcessIsAlive)
			{
				try
				{
					SysProcess[] processes = System.Diagnostics.Process.GetProcesses();
					SysProcess? proc = null;
					foreach (SysProcess process in processes)
					{
						if (process.ProcessName.ToLower().Contains("ffxiv_dx11"))
						{
							if (proc != null)
								throw new Exception("Multiple processes found");

							proc = process;
						}
					}

					if (proc == null)
						throw new Exception("No process found");

					this.Process.OpenProcess(proc);
					this.ProcessIsAlive = true;
				}
				catch (Exception ex)
				{
					SysProcess? proc = null;

					if (this.SelectProcessCallback != null)
						proc = await this.SelectProcessCallback.Invoke();

					if (proc == null)
						throw new Exception("Unable to locate FFXIV process", ex);

					this.Process.OpenProcess(proc);
					this.ProcessIsAlive = true;
				}
			}

			new Thread(new ThreadStart(this.TickMarshalersThread)).Start();
			new Thread(new ThreadStart(this.ProcessWatcherThread)).Start();
		}

		public Task Shutdown()
		{
			this.isActive = false;
			return Task.CompletedTask;
		}

		public IMarshaler<T> GetMarshaler<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets)
		{
			List<IMemoryOffset> newOffsets = new List<IMemoryOffset>();
			newOffsets.Add(baseOffset);
			newOffsets.AddRange(offsets);
			return this.GetMarshaler<T>(newOffsets.ToArray());
		}

		public IMarshaler<T> GetMarshaler<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return this.GetMarshaler<T>(baseOffset, (IMemoryOffset[])offsets);
		}

		public IMarshaler<T> GetMarshaler<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return this.GetMarshaler<T>((IBaseMemoryOffset)baseOffset, (IMemoryOffset[])offsets);
		}

		public IMarshaler<T> GetMarshaler<T>(params IMemoryOffset[] offsets)
		{
			Type marshalerType = this.GetMarshalerType(typeof(T));
			try
			{
				return (MarshalerBase<T>)Activator.CreateInstance(marshalerType, this.Process, offsets);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public async Task WaitForMarshalerTick()
		{
			// we wait for two ticks since we might be towards the end of a tick,
			// meaning the next tick (+1) will become active without ticking _all_ the memory.
			// wait for +2 guarantees that all currently tracked memory will get a chance to tick
			// before we return.
			ulong targetTick = this.tickCount + 2;

			while (this.tickCount < targetTick)
			{
				await Task.Delay(100);
			}
		}

		internal void OnError(Exception ex)
		{
			this.ErrorCallback?.Invoke(ex);
		}

		internal void OnLog(string message)
		{
			this.LogCallback?.Invoke(message);
		}

		private Type GetMarshalerType(Type type)
		{
			if (!marshalerLookup.ContainsKey(type))
				throw new Exception($"No memory wrapper for type: {type}");

			return marshalerLookup[type];
		}

		private void TickMarshalersThread()
		{
			try
			{
				while (this.isActive)
				{
					Thread.Sleep(16);

					if (!this.ProcessIsAlive)
						return;

					this.tickCount++;
					MarshalerBase.TickAllActive();
				}

				MarshalerBase.DisposeAll();
			}
			catch (Exception ex)
			{
				this.OnError(new Exception("Marshaler thread exception", ex));
			}
		}

		private void ProcessWatcherThread()
		{
			while (this.isActive && this.Process != null)
			{
				this.ProcessIsAlive = this.Process.IsAlive;

				if (!this.ProcessIsAlive)
				{
					this.OnError(new Exception("FFXIV Process has terminated"));
				}

				Thread.Sleep(100);
			}
		}
	}
}
