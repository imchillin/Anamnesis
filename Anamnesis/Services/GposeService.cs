// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Core;
using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A service that continously checks for changes to the game's GPose state.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class GposeService : ServiceBase<GposeService>
{
	private const int TASK_DELAY = 32; // ms (~30 fps)

	/// <summary>
	/// The delegate object for the <see cref="GposeService.GposeStateChanged"/> event.
	/// </summary>
	/// <param name="newState">The new GPose state (i.e. true if player actor is in GPose, false otherwise).</param>
	public delegate void GposeEvent(bool newState);

	/// <summary>
	/// Event that is triggered when the GPose state changes.
	/// </summary>
	public static event GposeEvent? GposeStateChanged;

	/// <summary>
	/// Gets a value indicating whether the signed-in character is currently in the GPose photo mode.
	/// </summary>
	/// <remarks>
	/// This is a cached value that is updated by a continuous background task.
	/// </remarks>
	public bool IsGpose { get; private set; } = false;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies => [AddressService.Instance, GameService.Instance];

	/// <summary>
	/// Checks if the user is in GPose photo mode by probing the game process' memory.
	/// </summary>
	/// <returns>True if the user is in GPose, false otherwise.</returns>
	public static bool GetIsGPose()
	{
		if (AddressService.GposeCheck == IntPtr.Zero)
			return false;

		// Character select screen counts as gpose.
		if (!GameService.Instance.IsSignedIn)
			return true;

		byte check1 = MemoryService.Read<byte>(AddressService.GposeCheck);
		byte check2 = MemoryService.Read<byte>(AddressService.GposeCheck2);

		return check1 == 1 && check2 == 4;
	}

	/// <inheritdoc/>
	protected override async Task OnStart()
	{
		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.CheckThread(this.CancellationToken));
		await base.OnStart();
	}

	private async Task CheckThread(CancellationToken cancellationToken)
	{
		while (this.IsInitialized && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				bool newGpose = GetIsGPose();

				if (newGpose != this.IsGpose)
				{
					this.IsGpose = newGpose;
					GposeStateChanged?.Invoke(newGpose);
				}

				await Task.Delay(TASK_DELAY, cancellationToken);
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop.
				break;
			}
		}
	}
}
