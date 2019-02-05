using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model;

namespace VkBotFramework.Models
{

	public class MessageReceivedEventArgs : EventArgs
	{
		public MessageReceivedEventArgs(Message message,PeerContext peerContext)
		{
			this.Message = message;
			this.PeerContext = peerContext;
		}

		public Message Message;
		public PeerContext PeerContext;
	}

}
