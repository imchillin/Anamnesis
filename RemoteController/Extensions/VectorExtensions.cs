// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Extensions;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Extension methods for <see cref="Vector3"/> math operations.
/// </summary>
public static class VectorExtensions
{
	/// <summary>
	/// Default component-wise tolerance for floating-point vector comparisons (matching <c>Bone.EQUALITY_TOLERANCE</c>).
	/// </summary>
	public const float DEFAULT_TOLERANCE = 0.00001f;

	/// <summary>
	/// Determines whether two 3D vectors are approximately equal within the specified component-wise tolerance.
	/// </summary>
	/// <param name="lhs">The first vector.</param>
	/// <param name="rhs">The second vector.</param>
	/// <param name="tolerance">The maximum allowed difference per component.</param>
	/// <returns>True if all components are within <paramref name="tolerance"/>; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsApproximately(this Vector3 lhs, Vector3 rhs, float tolerance = DEFAULT_TOLERANCE)
	{
		return MathF.Abs(lhs.X - rhs.X) < tolerance
			&& MathF.Abs(lhs.Y - rhs.Y) < tolerance
			&& MathF.Abs(lhs.Z - rhs.Z) < tolerance;
	}

	/// <summary>
	/// Divides two vectors component-wise, substituting <paramref name="fallback"/> where a denominator component is near zero.
	/// </summary>
	/// <param name="numerator">The numerator vector.</param>
	/// <param name="denominator">The denominator vector.</param>
	/// <param name="fallback">The fallback value used when a denominator component is below <paramref name="epsilon"/>.</param>
	/// <param name="epsilon">The minimum absolute denominator value required to divide.</param>
	/// <returns>The component-wise quotient vector.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 SafeDivide(this Vector3 numerator, Vector3 denominator, float fallback = 1.0f, float epsilon = DEFAULT_TOLERANCE)
	{
		return new Vector3(
			MathF.Abs(denominator.X) > epsilon ? numerator.X / denominator.X : fallback,
			MathF.Abs(denominator.Y) > epsilon ? numerator.Y / denominator.Y : fallback,
			MathF.Abs(denominator.Z) > epsilon ? numerator.Z / denominator.Z : fallback);
	}
}
