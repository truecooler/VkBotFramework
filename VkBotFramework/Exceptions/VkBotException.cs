using System;
using System.Collections.Generic;
using System.Text;

namespace VkBotFramework.Exceptions
{
    class VkBotException : Exception
	{
		public VkBotException()
		{
		}

		public VkBotException(string message)
			: base(message)
		{
		}

		public VkBotException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
