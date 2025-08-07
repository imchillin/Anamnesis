// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core.Memory;
using Anamnesis.Memory;
using Iced.Intel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides functionality to scan for specific byte signature patterns within the memory of a process module.
/// </summary>
/// <remarks>
/// It is designed to search through the .text and .data sections of a process module to locate specific byte
/// patterns. Unlike Dalamud's implementation, this class uses the Win32 API to read memory. This is because
/// Anamnesis does not have access to protected memory regions.
/// <para>
/// This class is largely based on Dalamud's SigScanner class, which can be found at:
/// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Game/SigScanner.cs.
/// </para>
/// </remarks>
public sealed unsafe class SignatureScanner
{
	/// <summary>The size of each chunk of memory to scan at a time.</summary>
	private const int ScanChunkSize = 1024; // 1kB

	/// <summary>
	/// Initializes a new instance of the <see cref="SignatureScanner"/> class.
	/// </summary>
	/// <param name="module">The process to be used for scanning.</param>
	public SignatureScanner(ProcessModule module)
	{
		Debug.Assert(module != null, "Process module cannot be null.");
		this.Module = module;

		// Limit the search space to .text section.
		this.SetupSearchSpace(module);

		Log.Information($"Signature Scanner Created: {module.ModuleName}");
		Log.Information($"Module Text base: {this.TextSectionBase}");
		Log.Information($"Module Text size: {this.TextSectionSize}");
		Log.Information($"Module Data base: {this.DataSectionBase}");
		Log.Information($"Module Data size: {this.DataSectionSize}");

		if (this.TextSectionSize <= 0 || this.DataSectionSize <= 0)
		{
			throw new ArgumentException("Process module is invalid");
		}
	}

	/// <summary>Gets the base address of the module the scanner is attached to.</summary>
	public IntPtr SearchBase => this.Module.BaseAddress;

	/// <summary>Gets the base address of the .text section within the module.</summary>
	public IntPtr TextSectionBase => new(this.SearchBase.ToInt64() + this.TextSectionOffset);

	/// <summary>Gets the offset of the .text section within the module.</summary>
	public long TextSectionOffset { get; private set; }

	/// <summary>Gets the size of the .text section within the module.</summary>
	public int TextSectionSize { get; private set; }

	/// <summary>Gets the base address of the .data section within the module.</summary>
	public IntPtr DataSectionBase => new(this.SearchBase.ToInt64() + this.DataSectionOffset);

	/// <summary>Gets the offset of the .data section within the module.</summary>
	public long DataSectionOffset { get; private set; }

	/// <summary>Gets the size of the .data section within the module.</summary>
	public int DataSectionSize { get; private set; }

	/// <summary>Gets the process module the scanner is attached to.</summary>
	public ProcessModule Module { get; }

	/// <summary>
	/// Scans a specified memory region for a given byte signature.
	/// </summary>
	/// <param name="baseAddress">The base address of the memory region to scan.</param>
	/// <param name="size">The size of the memory region to scan.</param>
	/// <param name="signature">The byte signature to search for. The signature needs to follow the IDA format. </param>
	/// <returns>The address where the signature is found, or <see cref="IntPtr.Zero"/> if the process is not alive.</returns>
	/// <exception cref="KeyNotFoundException">Thrown if the signature is not found in the specified memory region.</exception>
	/// <note>
	/// - The signature must be in the format of a string of hexadecimal bytes separated by spaces.
	/// - The signature can contain wildcard characters represented by "??".
	/// - This algorithm uses the Boyer-Moore algorithm for fast searching by employing a bad character shift table.
	/// It also searches in chunks to prevent reading large amounts of memory at once, which can cause a significant
	/// spike in memory usage, leaving the Large Object Heap (LOH) largely fragmented.
	/// </note>
	public static IntPtr Scan(IntPtr baseAddress, int size, string signature)
	{
		if (!MemoryService.IsProcessAlive)
			return IntPtr.Zero;

		var (needle, mask, badCharShift) = ParseSignature(signature);

		Debug.Assert(needle != null && needle.Length > 0, "Parsed needle must not be null or empty.");
		Debug.Assert(mask != null && mask.Length == needle.Length, "Parsed mask must not be null and must match the length of the needle.");
		Debug.Assert(badCharShift != null && badCharShift.Length == 256, "Bad character shift table must have 256 entries.");

		unsafe
		{
			int overlap = needle.Length - 1;

			// Iterate over the memory in chunks
			for (long offset = 0; offset < size;)
			{
				// Calculate the number of bytes to read in the current chunk
				int bytesToRead = Math.Min(ScanChunkSize, size - (int)offset);
				Span<byte> chunkBuffer = new byte[bytesToRead + overlap];

				// Read memory into the chunk buffer, including the overlap
				IntPtr currentAddress = IntPtr.Add(baseAddress, (int)offset);
				MemoryService.Read(currentAddress, chunkBuffer);

				// Scan the chunk buffer for the signature
				for (int chunkOffset = 0; chunkOffset < bytesToRead;)
				{
					// Check if the current slice of the chunk buffer matches the needle
					if (IsMatch(needle, mask, chunkBuffer.Slice(chunkOffset, needle.Length)))
						return IntPtr.Add(currentAddress, chunkOffset);

					// Use the bad character shift table to determine the next offset
					chunkOffset += badCharShift[chunkBuffer[chunkOffset + needle.Length - 1]];
				}

				// Move to the next chunk, excluding the overlap to ensure no matches are missed
				offset += bytesToRead;
			}
		}

		// Throw an exception if the signature is not found
		throw new KeyNotFoundException($"Signature \"{signature}\" not found.");
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
		if (signature == null)
			throw new ArgumentNullException(nameof(signature));

		IntPtr scanRet = Scan(this.TextSectionBase, this.TextSectionSize, signature);

		var startByte = MemoryService.ReadByte(scanRet);
		if (startByte == 0xE8 || startByte == 0xE9)
		{
			scanRet = ReadJmpCallSig(scanRet);
			var rel = scanRet.ToInt64() - this.Module.BaseAddress.ToInt64();
			if (rel < 0 || rel >= this.TextSectionSize)
			{
				throw new KeyNotFoundException(
					$"Signature \"{signature}\" resolved to 0x{rel:X} which is outside the .text section. Possible signature conflicts?");
			}
		}

		return scanRet;
	}

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
	/// Scan for a byte signature in the .data section.
	/// </summary>
	/// <param name="signature">The signature.</param>
	/// <returns>The real offset of the found signature.</returns>
	public IntPtr ScanData(string signature) => Scan(this.DataSectionBase, this.DataSectionSize, signature);

	/// <summary>
	/// Scan for a byte signature in the whole module search area.
	/// </summary>
	/// <param name="signature">The signature.</param>
	/// <returns>The real offset of the found signature.</returns>
	public IntPtr ScanModule(string signature) => Scan(this.SearchBase, this.Module.ModuleMemorySize, signature);

	/// <summary>
	/// Resolve a RVA address.
	/// </summary>
	/// <param name="nextInstAddr">The address of the next instruction.</param>
	/// <param name="relOffset">The relative offset.</param>
	/// <returns>The calculated offset.</returns>
	public IntPtr ResolveRelativeAddress(IntPtr nextInstAddr, int relOffset)
	{
		if (!Environment.Is64BitProcess)
			throw new NotSupportedException("32-bit processes are not supported.");

		return nextInstAddr + relOffset;
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
		for (idx = last; idx > 0 && mask[idx]; --idx)
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
	private static (byte[] needle, bool[] mask, int[] badCharShift) ParseSignature(string signature)
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
				mask[i] = false;
				continue;
			}

			needle[i] = byte.Parse(hexString, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			mask[i] = true;
		}

		return (needle, mask, BuildBadCharTable(needle, mask));
	}

	/// <summary>
	/// Check if the buffer matches the needle.
	/// </summary>
	/// <param name="needle">The byte signature to search for.</param>
	/// <param name="mask">The mask to indicate wildcard bytes.</param>
	/// <param name="buffer">The buffer to search in.</param>
	/// <returns>True if the buffer matches the needle, false otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static unsafe bool IsMatch(byte[] needle, bool[] mask, Span<byte> buffer)
	{
		for (int i = 0; i < needle.Length; i++)
		{
			if (mask[i] && needle[i] != buffer[i])
				return false;
		}

		return true;
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

	/// <summary>
	/// Sets up the search space by identifying the offsets and sizes of the .text and .data sections
	/// within the specified process module.
	/// </summary>
	/// <param name="module">The process module to analyze.</param>
	private void SetupSearchSpace(ProcessModule module)
	{
		IntPtr baseAddress = module.BaseAddress;

		// We don't want to read all of IMAGE_DOS_HEADER or IMAGE_NT_HEADER stuff so we cheat here.
		int ntNewOffset = MemoryService.ReadInt32(baseAddress, 0x3C);
		IntPtr ntHeader = baseAddress + ntNewOffset;

		// IMAGE_NT_HEADER
		IntPtr fileHeader = ntHeader + 4;
		short numSections = MemoryService.ReadInt16(ntHeader, 6);

		// IMAGE_OPTIONAL_HEADER
		IntPtr optionalHeader = fileHeader + 20;

		IntPtr sectionHeader;
		if (Environment.Is64BitProcess) // IMAGE_OPTIONAL_HEADER64
			sectionHeader = optionalHeader + 240;
		else // IMAGE_OPTIONAL_HEADER32
			sectionHeader = optionalHeader + 224;

		// IMAGE_SECTION_HEADER
		IntPtr sectionCursor = sectionHeader;
		for (int i = 0; i < numSections; i++)
		{
			long sectionName = MemoryService.ReadInt64(sectionCursor);

			// .text
			switch (sectionName)
			{
				case 0x747865742E: // .text
					this.TextSectionOffset = MemoryService.ReadInt32(sectionCursor, 12);
					this.TextSectionSize = MemoryService.ReadInt32(sectionCursor, 8);
					break;
				case 0x617461642E: // .data
					this.DataSectionOffset = MemoryService.ReadInt32(sectionCursor, 12);
					this.DataSectionSize = MemoryService.ReadInt32(sectionCursor, 8);
					break;
			}

			sectionCursor += 40;
		}

		Debug.Assert(this.TextSectionSize > 0, "Text section size must be greater than 0.");
		Debug.Assert(this.DataSectionSize > 0, "Data section size must be greater than 0.");
	}

	private unsafe class UnsafeCodeReader : CodeReader
	{
		private readonly int length;
		private readonly byte* address;
		private int pos;
		public UnsafeCodeReader(byte* address, int length)
		{
			this.length = length;
			this.address = address;
		}

		public bool CanReadByte => this.pos < this.length;

		public override int ReadByte()
		{
			if (this.pos >= this.length)
				return -1;

			return MemoryService.ReadByte((nint)(this.address + this.pos++));
		}
	}
}
