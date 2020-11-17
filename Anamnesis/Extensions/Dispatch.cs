// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Windows;

	public static class Dispatch
	{
		public static SwitchToUiAwaitable UiThread()
		{
			return default(SwitchToUiAwaitable);
		}

		public struct SwitchToUiAwaitable : INotifyCompletion
		{
			public bool IsCompleted => Application.Current.Dispatcher.CheckAccess();

			public SwitchToUiAwaitable GetAwaiter()
			{
				return this;
			}

			public void GetResult()
			{
			}

			public void OnCompleted(Action continuation)
			{
				Application.Current.Dispatcher.BeginInvoke(continuation);
			}
		}
	}
}
