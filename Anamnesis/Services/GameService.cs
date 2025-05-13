// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

[AddINotifyPropertyChangedInterface]
public class GameService : ServiceBase<GameService>
{
	private const int TaskWaitTime = 16; // ms

	private bool isSignedIn = false;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [AddressService.Instance, GameDataService.Instance];

	/// <summary>
	/// Gets a value indicating whether the game process is in a ready state.
	/// </summary>
	public static bool Ready => Instance.IsInitialized && Instance.IsSignedIn;

	/// <summary>
	/// Gets or sets a value indicating whether the user is signed into a character.
	/// </summary>
	public bool IsSignedIn
	{
		get => this.isSignedIn;
		private set
		{
			if (this.isSignedIn != value)
			{
				// Update the property on the UI thread as it likely bound to UI elements
				if (Application.Current != null && Application.Current.Dispatcher.CheckAccess())
					this.isSignedIn = value;
				else
					Application.Current?.Dispatcher.Invoke(() => this.isSignedIn = value);
			}
		}
	}

	/// <summary>
	/// Checks if the user is signed into a character by probing the game process' memory.
	/// </summary>
	/// <returns>True if the user is signed in, false otherwise.</returns>
	public static bool GetIsSignedIn()
	{
#if DEBUG
		if (MemoryService.Process == null)
			return true;
#endif

		try
		{
			if (GameDataService.Territories == null)
				return false;

			int territoryID = MemoryService.Read<int>(AddressService.Territory);

			if (territoryID == -1)
				return false;

			if (GameDataService.Territories.GetRowOrDefault((uint)territoryID) == null)
				return false;

			return true;
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to safely check for sign in");
			return false;
		}
	}

	/// <summary>
	/// Periodically updates the signed-in status of the user from the game process.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the task.</param>
	/// <returns>The task representing the asynchronous operation.</returns>
	public async Task CheckIsSignedIn(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				this.IsSignedIn = GetIsSignedIn();

				// Property is updated synchronously; No need to resume from the original sync context
				await Task.Delay(TaskWaitTime, cancellationToken).ConfigureAwait(false);
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop.
				break;
			}
		}
	}

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.CheckIsSignedIn(this.CancellationToken));
		await base.OnStart();
	}
}
