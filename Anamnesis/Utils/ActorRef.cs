// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ActorRef<T> : INotifyPropertyChanged
		where T : ActorBasicMemory
	{
		private readonly T memory;
		private readonly uint objectId;
		private readonly IntPtr address;

		private bool isValid;

		public ActorRef(T memory)
		{
			this.memory = memory;
			this.objectId = memory.ObjectId;
			this.address = memory.Address;

			this.UpdateIsValid();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public T? Memory
		{
			get
			{
				if(this.IsValid)
					return this.memory;

				return null;
			}
		}

		public bool IsValid
		{
			get
			{
				this.UpdateIsValid();
				return this.isValid;
			}
		}

		public bool TryGetMemory([NotNullWhen(true)] out T? outMemory)
		{
			outMemory = this.Memory;
			return outMemory != null;
		}

		public override bool Equals(object? obj)
		{
			if ((obj == null) || !this.GetType().Equals(obj.GetType()))
			{
				return false;
			}
			else
			{
				ActorRef<T> p = (ActorRef<T>)obj;
				return this.objectId == p.objectId && this.address == p.address;
			}
		}

		public override int GetHashCode()
		{
			return Tuple.Create(this.address, this.objectId).GetHashCode();
		}

		private void UpdateIsValid()
		{
			var wasValid = this.isValid;
			this.isValid = this.address != IntPtr.Zero &&
					this.memory.Address != IntPtr.Zero &&
					this.memory.Address == this.address &&
					this.memory.ObjectId == this.objectId &&
					ActorService.Instance.ActorTable[this.memory.ObjectIndex] == this.address;

			if(wasValid != this.isValid)
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsValid)));
		}
	}
}
