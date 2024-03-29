﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.Collections.ByteReader
{
	public unsafe struct ByteReader : IDisposable
	{
		private static readonly int READ_PTR_SIZE = sizeof(byte*);
		private static readonly int COUNT_SIZE = sizeof(Int32);
		private static readonly int HEADER_SIZE = READ_PTR_SIZE + COUNT_SIZE;

		private readonly byte* _bufferPtr;
		private readonly byte** _readPtrPtr;
		private readonly int* _countPtr;

		public readonly int dataCapacity;

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => *_countPtr;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set => *_countPtr = value;
		}

		public int DequeuedCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (int)(ReadPtr - HEADER_SIZE - _bufferPtr);
		}

		private byte* ReadPtr
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => *_readPtrPtr;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => *_readPtrPtr = value;
		}

		public ByteReader(int capacity) : this(Marshal.AllocHGlobal(capacity), capacity)
		{
		}

		public ByteReader(IntPtr ptr, int capacity) : this((byte*)ptr.ToPointer(), capacity)
		{
		}

		public ByteReader(byte* ptr, int capacity)
		{
			Debug.Assert(capacity > HEADER_SIZE);

			_bufferPtr = ptr;
			_readPtrPtr = (byte**)ptr;
			_countPtr = (int*)(ptr + READ_PTR_SIZE);
			dataCapacity = capacity - HEADER_SIZE;

			ReadPtr = ptr + HEADER_SIZE;
			Count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReserveSpace(int count)
		{
			Count += count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(byte[] data, int count)
		{
			new ReadOnlySpan<byte>(data).CopyTo(new Span<byte>(ReadPtr + Count, count));

			Count += count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(ReadOnlySpan<byte> data)
		{
			var span = new Span<byte>(ReadPtr + Count, data.Length);
			data.CopyTo(span);

			Count += data.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<byte> SliceFreeSpace(int offset, int count)
		{
			var span = new Span<byte>(ReadPtr + offset, count);

			return span;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<byte> SliceFreeSpace(int count)
		{
			var span = new Span<byte>(ReadPtr, count);

			return span;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<byte> AllocateData(int count)
		{
			var span = new Span<byte>(ReadPtr + Count, count);
			Count += count;

			return span;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> PopData(int count)
		{
			var span = new ReadOnlySpan<byte>(ReadPtr + Count - count, count);

			Count -= count;

			return span;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> DequeueData(int count)
		{
			var span = new ReadOnlySpan<byte>(ReadPtr, count);

			ReadPtr += count;
			Count -= count;

			return span;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			ReadPtr = _bufferPtr + HEADER_SIZE;
			Count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			Marshal.FreeHGlobal((IntPtr)_bufferPtr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> Serialize()
		{
			return new ReadOnlySpan<byte>(_bufferPtr + READ_PTR_SIZE, COUNT_SIZE + Count);
		}
	}
}