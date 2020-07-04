using System;

namespace VkBotFramework.Exceptions
{
	class GroupNotResolvedException : VkBotException
	{
		public GroupNotResolvedException()
		{
		}

		public GroupNotResolvedException(string message)
			: base(message)
		{
		}

		public GroupNotResolvedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}