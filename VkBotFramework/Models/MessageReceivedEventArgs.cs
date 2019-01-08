using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model;

namespace VkBotFramework.Models
{

	public class MessageReceivedEventArgs : EventArgs
	{
		public MessageReceivedEventArgs(Message message)
		{
			this.Message = message;
		}

		public Message Message;
	}

}
