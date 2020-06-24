// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis;
	using Anamnesis.Offsets;
	using Anamnesis.Process;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.GUI.Windows;

	public class InjectionService : IInjectionService
	{
		private AnamnesisService service;

		public string GamePath
		{
			get
			{
				return this.service.GamePath;
			}
		}

		public IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset[] offsets)
		{
			return this.service.GetMemory<T>(baseOffset, offsets);
		}

		public IMemory<T> GetMemory<T>(IBaseMemoryOffset baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return this.service.GetMemory<T>(baseOffset, offsets);
		}

		public IMemory<T> GetMemory<T>(IBaseMemoryOffset<T> baseOffset, params IMemoryOffset<T>[] offsets)
		{
			return this.service.GetMemory<T>(baseOffset, offsets);
		}

		public async Task Initialize()
		{
			this.service = new AnamnesisService();
			this.service.ErrorCallback = this.OnError;
			this.service.LogCallback = this.OnLog;
			this.service.SelectProcessCallback = this.OnSelectProcess;

			await this.service.Initialize<WinProcess>();
		}

		public async Task Shutdown()
		{
			await this.service.Shutdown();
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public Task WaitForMemoryTick()
		{
			return this.service.WaitForMemoryTick();
		}

		private void OnError(Exception ex)
		{
			Log.Write(ex, "Injection");
		}

		private void OnLog(string message)
		{
			Log.Write(message, "Injection", Log.Severity.Log);
		}

		private async Task<Process> OnSelectProcess()
		{
			return await App.Current.Dispatcher.InvokeAsync<Process>(() =>
			{
				Process proc = ProcessSelector.FindProcess();

				if (proc == null)
					Application.Current.Shutdown();

				return proc;
			});
		}
	}
}
