using System.Collections.Generic;
using VkBotFramework.Models;

namespace VkBotFramework.Abstractions
{
	public interface IPeerContextManager
	{
		Dictionary<long, PeerContext> Peers { get; }

		Dictionary<string, dynamic> GlobalVars { get; }
	}
}