// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Iced.Intel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides functionality to scan for specific byte signature patterns within the memory of a process module.
/// </summary>
/// <remarks>
/// It is designed to search through the .text and .data sections of a process module to locate specific byte
/// patterns. Unlike Dalamud's implementation, this class uses the Win32 API to read memory. This is because
/// Anamnesis does not have access to protected memory regions.
/// <para>
/// This class is bastard child of Dalamud and Penumbra's signature scanners, which can be found at:
/// - https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/SigScanner.cs.
/// - https://github.com/xivdev/Penumbra/blob/master/Penumbra/Interop/Hooks/ResourceLoading/PeSigScanner.cs.
/// </para>
/// </remarks>
public sealed unsafe class SignatureScanner
{
	private readonly MemoryMappedFile file;
	private readonly MemoryMappedViewAccessor textSectionAccessor;
	private readonly MemoryMappedViewAccessor dataSectionAccessor;

	private readonly nint moduleBaseAddress;
	private readonly uint textSectionVirtualAddress;
	private readonly uint dataSectionVirtualAddress;

	/// <summary>
	/// Initializes a new instance of the <see cref="SignatureScanner"/> class.
	/// </summary>
	/// <param name="module">The process to be used for scanning.</param>
	public SignatureScanner(ProcessModule module)
	{
		Debug.Assert(module != null, "Process module cannot be null.");
		this.Module = module;
		this.moduleBaseAddress = module.BaseAddress;

		string? fileName = module.FileName;
		if (string.IsNullOrEmpty(fileName))
			throw new ArgumentException("Unable to obtain module path.");

		// Open the file on disk to map it
		this.file = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

		// Limit the search space to .text section.
		SetupSearchSpace(module, out SectionInfo textSection, out SectionInfo dataSection);
		this.textSectionVirtualAddress = textSection.VirtualAddress;
		this.dataSectionVirtualAddress = dataSection.VirtualAddress;

		this.textSectionAccessor = this.file.CreateViewAccessor(textSection.Offset, textSection.Size, MemoryMappedFileAccess.Read);
		this.dataSectionAccessor = this.file.CreateViewAccessor(dataSection.Offset, dataSection.Size, MemoryMappedFileAccess.Read);

		Log.Information($"Signature Scanner (MMF) Created: {module.ModuleName}");
		Log.Information($"Text Section: Offset=0x{textSection.Offset:X}, Virtual Address=0x{textSection.VirtualAddress:X}, Size=0x{textSection.Size:X}");
		Log.Information($"Data Section: Offset=0x{dataSection.Offset:X}, Virtual Address=0x{dataSection.VirtualAddress:X}, Size=0x{dataSection.Size:X}");

		if (textSection.Size <= 0 || dataSection.Size <= 0)
		{
			throw new ArgumentException("Process module is invalid");
		}
	}

	/// <summary>Gets the process module the scanner is attached to.</summary>
	public ProcessModule Module { get; }

	/// <summary>
	/// Resolve a RVA address.
	/// </summary>
	/// <param name="nextInstAddr">The address of the next instruction.</param>
	/// <param name="relOffset">The relative offset.</param>
	/// <returns>The calculated offset.</returns>
	public static IntPtr ResolveRelativeAddress(IntPtr nextInstAddr, int relOffset)
	{
		if (!Environment.Is64BitProcess)
			throw new NotSupportedException("32-bit processes are not supported.");

		return nextInstAddr + relOffset;
	}

	/// <summary>
	/// Scans a specified memory region for a given byte signature.
	/// </summary>
	/// <param name="section">The memory-mapped view accessor for the memory region to scan.</param>
	/// <param name="signature">The byte signature to search for (IDA format).</param>
	/// <returns>
	/// The address where the signature is found, or <see cref="IntPtr.Zero"/> if the process
	/// is not alive.
	/// </returns>
	/// <exception cref="KeyNotFoundException">
	/// Thrown if the signature is not found in the specified memory region.
	/// </exception>
	/// <note>
	/// - The signature must be in the format of a string of hexadecimal bytes separated
	///   by spaces.
	/// - The signature can contain wildcard characters represented by "??".
	/// - This algorithm uses the Boyer-Moore algorithm for fast searching by employing
	///   a bad character shift table.
	/// </note>
	public IntPtr Scan(MemoryMappedViewAccessor section, string signature)
	{
		if (!MemoryService.IsProcessAlive)
			return IntPtr.Zero;

		var (needle, mask, badCharShift) = ParseSignature(signature);

		var index = IndexOf(section, needle, mask, badCharShift);
		if (index < 0)
			throw new KeyNotFoundException($"Signature \"{signature}\" not found.");

		return (nint)(this.moduleBaseAddress + index - section.PointerOffset + this.textSectionVirtualAddress);
	}

	/// <summary>
	/// Scan for a byte signature in the .text section.
	/// </summary>
	/// <param name="signature">The signature.</param>
	/// <returns>The real offset of the found signature.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the signature is null.</exception>
	/// <exception cref="KeyNotFoundException">
	/// Thrown if the signature is not found in the .text section or a conflict is encountered.
	/// </exception>
	/// <remarks>
	/// The signature needs to follow the IDA format.
	/// </remarks>
	public IntPtr ScanText(string signature)
	{
		ArgumentNullException.ThrowIfNull(signature);

		IntPtr scanRet = this.Scan(this.textSectionAccessor, signature);
		var startByte = MemoryService.ReadByte(scanRet);
		if (startByte == 0xE8 || startByte == 0xE9)
		{
			scanRet = ReadJmpCallSig(scanRet);
			var rel = scanRet.ToInt64() - this.moduleBaseAddress.ToInt64();
			if (rel < 0 || rel >= (int)this.textSectionAccessor.Capacity)
			{
				throw new KeyNotFoundException(
					$"Signature \"{signature}\" resolved to 0x{rel:X} which is outside the .text section. Possible signature conflicts?");
			}
		}

		return scanRet;
	}

	/// <summary>
	/// Scan for a byte signature in the .data section.
	/// </summary>
	/// <param name="signature">The signature.</param>
	/// <returns>The real offset of the found signature.</returns>
	public nint ScanData(string signature) => this.Scan(this.dataSectionAccessor, signature);

	/// <summary>
	/// Get a .data address by scanning for the signature in the .text memory region.
	/// </summary>
	/// <param name="signature">The signature of the function using the data. </param>
	/// <param name="offset">The offset from function start of the instruction using the data. </param>
	/// <returns> A pointer to the static address. </returns>
	/// <remarks>
	/// To create a signature, use IDA to find the function calling the static address.
	/// Then place your cursor on the line calling the address and create the signature.
	/// </remarks>
	public unsafe IntPtr GetStaticAddressFromSig(string signature, int offset = 0)
	{
		var instructionAddress = (byte*)this.ScanText(signature);
		instructionAddress += offset;

		try
		{
			var reader = new UnsafeCodeReader(instructionAddress, signature.Length + 8);
			var decoder = Decoder.Create(64, reader, (ulong)instructionAddress, DecoderOptions.AMD);
			while (reader.CanReadByte)
			{
				var instruction = decoder.Decode();
				if (instruction.IsInvalid)
					continue;

				if (instruction.Op0Kind is OpKind.Memory || instruction.Op1Kind is OpKind.Memory)
				{
					if (instruction.IsIPRelativeMemoryOperand)
					{
						// If RIP/EIP relative, we can use the displacement directly.
						return (IntPtr)instruction.MemoryDisplacement64; // For RIP/EIP, the displacement property represents the static address.
					}
					else
					{
						// Otherwise, resolve the static address by reading the memory displacement and adding it to the instruction address.
						return IntPtr.Add((nint)instructionAddress, MemoryService.ReadInt32((nint)instructionAddress) + 4);
					}
				}
			}
		}
		catch
		{
			// ignored
		}

		throw new KeyNotFoundException($"Can't find any referenced address in the given signature {signature}.");
	}

	/// <summary>
	/// Build a bad character shift table for the Boyer-Moore algorithm, taking into account the bitmask.
	/// </summary>
	/// <param name="needle">The byte signature to search for.</param>
	/// <param name="mask">The mask to indicate wildcard bytes.</param>
	/// <returns>The bad character shift table.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int[] BuildBadCharTable(byte[] needle, bool[] mask)
	{
		int idx;
		var last = needle.Length - 1;
		var badShift = new int[256];
		for (idx = last; idx > 0 && !mask[idx]; --idx)
		{
		}

		var diff = last - idx;
		if (diff == 0)
			diff = 1;

		for (idx = 0; idx <= 255; ++idx)
			badShift[idx] = diff;
		for (idx = last - diff; idx < last; ++idx)
			badShift[needle[idx]] = last - idx;
		return badShift;
	}

	/// <summary>
	/// Parse the string-form byte signature into a byte array, mask, and bad character shift table.
	/// </summary>
	/// <param name="signature">The byte signature to parse (IDA format).</param>
	/// <returns>The parsed byte signature, mask, and bad character shift table.</returns>
	/// <exception cref="ArgumentException">The signature is not even in length.</exception>
	private static (byte[] Needle, bool[] Mask, int[] BadCharShift) ParseSignature(string signature)
	{
		// Strip all whitespaces
		signature = signature.Replace(" ", string.Empty);
		if (signature.Length % 2 != 0)
			throw new ArgumentException("Stripped signatures must be even in length", nameof(signature));

		int needleLength = signature.Length / 2;
		byte[] needle = new byte[needleLength];
		bool[] mask = new bool[needleLength];

		ReadOnlySpan<char> sigSpan = signature.AsSpan();
		for (int i = 0; i < needleLength; i++)
		{
			ReadOnlySpan<char> hexString = sigSpan.Slice(i * 2, 2);
			if (hexString.SequenceEqual("??") || hexString.SequenceEqual("**"))
			{
				needle[i] = 0;
				mask[i] = true;
				continue;
			}

			needle[i] = byte.Parse(hexString, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			mask[i] = false;
		}

		return (needle, mask, BuildBadCharTable(needle, mask));
	}

	/// <summary>
	/// A helper method to ScanText to get the correct address for
	/// IDA signatures that mark the first CALL location.
	/// </summary>
	/// <param name="sigLocation">The address the CALL sig resolved to.</param>
	/// <returns>The real offset of the signature.</returns>
	private static IntPtr ReadJmpCallSig(IntPtr sigLocation)
	{
		int jumpOffset = MemoryService.ReadInt32(IntPtr.Add(sigLocation, 1));
		return IntPtr.Add(sigLocation, 5 + jumpOffset);
	}

	private static int IndexOf(MemoryMappedViewAccessor section, byte[] needle, bool[] mask, int[] badCharShift)
	{
		if (needle.Length > section.Capacity)
			return -1;

		var badShift = BuildBadCharTable(needle, mask);
		var last = needle.Length - 1;
		var offset = 0;
		var maxOffset = section.Capacity - needle.Length;

		byte* buffer = null;
		section.SafeMemoryMappedViewHandle.AcquirePointer(ref buffer);
		try
		{
			while (offset <= maxOffset)
			{
				int position;
				for (position = last; needle[position] == *(buffer + position + offset) || mask[position]; position--)
				{
					if (position == 0)
						return offset;
				}

				offset += badShift[*(buffer + offset + last)];
			}
		}
		finally
		{
			section.SafeMemoryMappedViewHandle.ReleasePointer();
		}

		return -1;
	}

	private static void SetupSearchSpace(ProcessModule module, out SectionInfo textSection, out SectionInfo dataSection)
	{
		textSection = default;
		dataSection = default;

		IntPtr baseAddress = module.BaseAddress;

		// We don't want to read all of IMAGE_DOS_HEADER or IMAGE_NT_HEADER stuff so we cheat here.
		int ntNewOffset = MemoryService.ReadInt32(baseAddress, 0x3C);
		IntPtr ntHeader = baseAddress + ntNewOffset;

		// IMAGE_NT_HEADER
		IntPtr fileHeader = ntHeader + 4;
		short numSections = MemoryService.ReadInt16(ntHeader, 6);
		short sizeOfOptionalHeader = MemoryService.ReadInt16(ntHeader, 20);
		IntPtr sectionHeaderStart = ntHeader + 24 + sizeOfOptionalHeader; // IMAGE_OPTIONAL_HEADER

		// IMAGE_SECTION_HEADER
		for (int i = 0; i < numSections; i++)
		{
			IntPtr sectionCursor = sectionHeaderStart + (i * 40);
			long sectionName = MemoryService.ReadInt64(sectionCursor);
			switch (sectionName)
			{
				case 0x747865742E: // .text
					var textSize = MemoryService.ReadInt32(sectionCursor, 8);               // VirtualSize
					var textVirtualAddr = (uint)MemoryService.ReadInt32(sectionCursor, 12); // VirtualAddress
					var textOffset = MemoryService.ReadInt32(sectionCursor, 20);            // PointerToRawData
					textSection = new SectionInfo(textOffset, textSize, textVirtualAddr);
					break;
				case 0x617461642E: // .data
					var dataSize = MemoryService.ReadInt32(sectionCursor, 8);               // VirtualSize
					var dataVirtualAddr = (uint)MemoryService.ReadInt32(sectionCursor, 12); // VirtualAddress
					var dataOffset = MemoryService.ReadInt32(sectionCursor, 20);            // PointerToRawData
					dataSection = new SectionInfo(dataOffset, dataSize, dataVirtualAddr);
					break;
			}
		}

		Debug.Assert(textSection.Size > 0, "Text section size must be greater than 0.");
		Debug.Assert(dataSection.Size > 0, "Data section size must be greater than 0.");
	}

	internal readonly struct SectionInfo(long offset, int size, uint virtualAddress)
	{
		public long Offset { get; } = offset;
		public int Size { get; } = size;
		public uint VirtualAddress { get; } = virtualAddress;
	}

	private unsafe class UnsafeCodeReader(byte* address, int length) : CodeReader
	{
		private int pos = 0;

		public bool CanReadByte => this.pos < length;

		public override int ReadByte()
		{
			if (this.pos >= length)
				return -1;

			return MemoryService.ReadByte((nint)(address + this.pos++));
		}
	}
}
