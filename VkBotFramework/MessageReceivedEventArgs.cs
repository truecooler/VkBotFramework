using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model;
namespace VkBotFramework
{
	public partial class VkBot
	{
		public class MessageReceivedEventArgs : EventArgs
		{
			public MessageReceivedEventArgs(Message message)
			{
				this.message = message;
			}
			public Message message;
		}
	}
}
