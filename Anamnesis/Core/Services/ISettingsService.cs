// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Threading.Tasks;

	#pragma warning disable SA1201

	public interface ISettingsService : IService
	{
		Task<T> Load<T>()
			where T : SettingsBase, new();
	}

	public delegate void SettingsEvent(SettingsBase settings);

	[Serializable]
	public abstract class SettingsBase
	{
		public event SettingsEvent? Changed;

		public void NotifyChanged()
		{
			this.Changed?.Invoke(this);
		}
	}
}
