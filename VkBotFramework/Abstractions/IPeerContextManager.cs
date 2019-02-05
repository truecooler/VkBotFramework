using System;
using System.Collections.Generic;
using System.Text;
using VkBotFramework.Models;

namespace VkBotFramework.Abstractions
{
    public interface IPeerContextManager
    {
	    Dictionary<long, PeerContext> Peers { get; }

		Dictionary<string, dynamic> GlobalPeerContext { get; }

	}
}
