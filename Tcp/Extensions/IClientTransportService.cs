﻿using System.Net;
using System.Threading.Tasks;

namespace Sapientia.Tcp.Extensions
{
	public interface IClientTransportService : ITransportService
	{
		public void Connect(EndPoint remoteEndPoint, int customId = -1);
		public Task ConnectAsync(EndPoint remoteEndPoint, int customId = -1);
	}
}