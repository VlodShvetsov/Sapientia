﻿using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Sapientia.Serializers;
using Sapientia.Transport;
using Sapientia.Transport.RemoteMessage;

namespace Sapientia.Tcp
{
	public class Connection_Tcp
	{
		public enum ConnectionState : byte
		{
			WaitingForStart,
			Working,
			Closing,
			Disconnecting,
		}

		// Reset:
		private ConnectionState _state;
		private ConnectionReference _connectionReference = ConnectionReference.EMPTY;

		public ConnectionState State
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _state;
		}

		public ConnectionReference ConnectionReference
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _connectionReference;
		}

		private Socket _socket = default!;

		private RemoteMessageSender _receiveMessageSender;
		private int _countToReceive = 0;

		private int _messagesCount = 0;
		private readonly int _maxMessagesCount;

		public Connection_Tcp(int maxMessagesCount)
		{
			_maxMessagesCount = maxMessagesCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Start(Socket socket, ConnectionReference reference)
		{
			_socket = socket;
			_connectionReference = reference;

			_state = ConnectionState.Working;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Send(RemoteMessage remoteMessage)
		{
			var sendData = remoteMessage.Reader.Serialize();
			_socket.Send(sendData);
		}

		internal void Receive(in RemoteMessageStack receiveMessageStack)
		{
			if (_messagesCount == _maxMessagesCount)
				return;

			while (_socket.Available > 0)
			{
				if (_countToReceive == 0)
				{
					if (_state == ConnectionState.Closing)
					{
						_state = ConnectionState.Disconnecting;
						return;
					}

					_countToReceive = _socket.Receive<int>();

					if (_countToReceive > receiveMessageStack.messageDataCapacity | _countToReceive <= 0)
					{
						throw new Exception($"The count to receive is out of range. Count To Receive: {_countToReceive}, Range: (0, {receiveMessageStack.messageDataCapacity}].");
					}

					using (receiveMessageStack.GetBusyScope())
					{
						_receiveMessageSender = receiveMessageStack.GetSender();
					}
				}

				var reader = _receiveMessageSender.Reader;

				if (_socket.Available >= _countToReceive)
				{
					var receivedCount = _socket.Receive(reader.SliceFreeSpace(_countToReceive));
					reader.ReserveSpace(receivedCount);

					_receiveMessageSender.Send(_connectionReference);

					_countToReceive = 0;

					if (++_messagesCount == _maxMessagesCount)
					{
						// Server_OLD overload
						if (_state == ConnectionState.Closing)
						{
							_state = ConnectionState.Disconnecting;
						}

						return;
					}
				}
				else
				{
					var receivedCount = _socket.Receive(reader.SliceFreeSpace(_socket.Available));
					reader.ReserveSpace(receivedCount);

					_countToReceive -= receivedCount;
					return;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void OnReadMessage()
		{
			_messagesCount--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Close()
		{
			_state = ConnectionState.Closing;
		}

		internal void Disconnect()
		{
			_receiveMessageSender.Dispose();
			_socket?.Close();

			Reset();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Reset()
		{
			_connectionReference = ConnectionReference.EMPTY;
			_socket = default!;
			_receiveMessageSender = default;
			_countToReceive = 0;
			_messagesCount = 0;

			_state = ConnectionState.WaitingForStart;
		}
	}
}