using System;
using VkNet.Model;

namespace VkBotFramework.Models
{
	public class MessageReceivedEventArgs : EventArgs
	{
		public Message Message;
		public PeerContext PeerContext;

		public MessageReceivedEventArgs(Message message, PeerContext peerContext)
		{
			this.Message = message;
			this.PeerContext = peerContext;
		}
	}
}