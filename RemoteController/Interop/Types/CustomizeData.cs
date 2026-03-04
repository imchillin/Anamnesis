// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = SIZE)]
public struct CustomizeData
{
	public const int SIZE = 0x1A;

	[FieldOffset(0x00)] public byte Race;
	[FieldOffset(0x01)] public byte Gender;
	[FieldOffset(0x02)] public byte Age;
	[FieldOffset(0x03)] public byte Height;
	[FieldOffset(0x04)] public byte Tribe;
	[FieldOffset(0x05)] public byte Head;
	[FieldOffset(0x06)] public byte Hair;
	[FieldOffset(0x07)] public byte HighlightType;
	[FieldOffset(0x08)] public byte Skintone;
	[FieldOffset(0x09)] public byte REyeColor;
	[FieldOffset(0x0A)] public byte HairTone;
	[FieldOffset(0x0B)] public byte Highlights;
	[FieldOffset(0x0C)] public byte FacialFeatures;
	[FieldOffset(0x0D)] public byte FacialFeatureColor;
	[FieldOffset(0x0E)] public byte Eyebrows;
	[FieldOffset(0x0F)] public byte LEyeColor;
	[FieldOffset(0x10)] public byte Eyes;
	[FieldOffset(0x11)] public byte Nose;
	[FieldOffset(0x12)] public byte Jaw;
	[FieldOffset(0x13)] public byte Mouth;
	[FieldOffset(0x14)] public byte LipsToneFurPattern;
	[FieldOffset(0x15)] public byte EarMuscleTailSize;
	[FieldOffset(0x16)] public byte TailEarsType;
	[FieldOffset(0x17)] public byte Bust;
	[FieldOffset(0x18)] public byte FacePaint;
	[FieldOffset(0x19)] public byte FacePaintColor;
}
