using System;
using System.Collections.Generic;
using VkNet.Model;

namespace VkBotFramework.Models
{
	public class GroupUpdateReceivedEventArgs : EventArgs
	{
		public Dictionary<string, dynamic> GlobalVars;

		public GroupUpdate Update;

		public GroupUpdateReceivedEventArgs(GroupUpdate update, Dictionary<string, dynamic> globalVars)
		{
			this.Update = update;
			this.GlobalVars = globalVars;
		}
	}
}