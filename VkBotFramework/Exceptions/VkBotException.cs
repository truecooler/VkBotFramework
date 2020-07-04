using System;

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