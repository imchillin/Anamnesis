// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	[Flags]
	public enum MemoryModes
	{
		None = 0,

		Read = 1,
		Write = 2,

		ReadWrite = Read | Write,
	}

	public interface IMemoryViewModel : IStructViewModel, IDisposable
	{
		IntPtr? Pointer { get; set; }
		void ReadChanges();
		bool WriteToMemory(bool force = false);
		bool ReadFromMemory(bool force = false);

		void SetMemoryMode(MemoryModes mode);

		void AddChild(IMemoryViewModel child);
		void RemoveChild(IMemoryViewModel child);
	}
}
