// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A driver class for actor management and actor-related operations.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class ActorDriver : DriverBase<ActorDriver>
{
	public const int OBJECT_TABLE_SIZE = 819;
	public const string OBJECT_TABLE_SIGNATURE = "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 0F B6 83";

	private readonly nint objTablePtr;

	private RedrawModule Redraw { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ActorDriver"/> class.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the function wrapper could not be created.
	/// </exception>
	public ActorDriver()
	{
		// NOTE: If the signature resolver is initialized, then so is the scanner, we don't need to check twice
		if (Controller.SigResolver == null)
			throw new InvalidOperationException("Cannot initialize actor driver: signature resolver is not available.");

		this.objTablePtr = Controller.Scanner?.GetStaticAddressFromSig(OBJECT_TABLE_SIGNATURE) ?? nint.Zero;
		if (this.objTablePtr == nint.Zero)
			throw new InvalidOperationException("Failed to resolve object table pointer.");

		this.Redraw = new RedrawModule(this.objTablePtr);
		this.RegisterInstance();
	}

	public bool RedrawActor(RedrawRequest request) => this.Redraw.RequestRedraw(request);
	public bool SetVisor(nint drawDataPtr, byte state) => this.Redraw.SetVisor(drawDataPtr, state);
	public bool SetHeadgearHidden(nint drawDataPtr, byte hide) => this.Redraw.SetHeadgearHidden(drawDataPtr, hide);
	public bool SetVieraEarsHidden(nint drawDataPtr, byte hide) => this.Redraw.SetVieraEarsHidden(drawDataPtr, hide);

	/// <inheritdoc/>
	protected override void OnDispose() { }
}
