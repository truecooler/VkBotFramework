using System;
using System.Collections.Generic;
using System.Text;
using VkBotFramework.Abstractions;
using VkBotFramework.Models;

namespace VkBotFramework
{
    class PeerContextManager : IPeerContextManager
    {
	    public Dictionary<long, PeerContext> Peers { get; private set; } = new Dictionary<long, PeerContext>();
	    public Dictionary<string, dynamic> GlobalVars { get; private set; } = new Dictionary<string, dynamic>();

	}
}
