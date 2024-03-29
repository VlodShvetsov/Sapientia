﻿using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections.ByteReader;

namespace Sapientia.Transport.RemoteMessage
{
	public struct RemoteMessageSender : IDisposable
	{
		private RemoteMessageStack _stack;
		private ByteReaderPool.Element _readerContainer;

		public readonly ByteReader Reader
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _readerContainer.Reader;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RemoteMessageSender(RemoteMessageStack stack)
		{
			_stack = stack;
			_readerContainer = _stack.AllocateReader_Interlocked();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Send(ConnectionReference connectionReference)
		{
			_stack.AddMessage_Interlocked(new RemoteMessage(connectionReference, _readerContainer));
			Reset();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SendAndExpand(ConnectionReference connectionReference)
		{
			_stack.AddMessageAndExpand_Interlocked(new RemoteMessage(connectionReference, _readerContainer));
			Reset();
		}

		/// <summary>
		/// Use it for message cancellation. Don't use after sending.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_readerContainer.Dispose();
			Reset();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Reset()
		{
			_stack = default!;
			_readerContainer = default;
		}
	}
}