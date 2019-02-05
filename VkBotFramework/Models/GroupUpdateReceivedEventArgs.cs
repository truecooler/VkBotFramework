using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.GroupUpdate;

namespace VkBotFramework.Models
{

	public class GroupUpdateReceivedEventArgs : EventArgs
	{
		public GroupUpdateReceivedEventArgs(GroupUpdate update, Dictionary<string, dynamic> globalVars)
		{
			this.Update = update;
			this.GlobalVars = globalVars;
		}

		public GroupUpdate Update;
		public Dictionary<string, dynamic> GlobalVars;
	}
}
