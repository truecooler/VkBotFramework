using System.Collections.Generic;

namespace VkBotFramework.Models
{
	public class PeerContext
	{
		public Dictionary<string, dynamic> GlobalVars;
		public Dictionary<string, dynamic> Vars = new Dictionary<string, dynamic>();

		public PeerContext(Dictionary<string, dynamic> globalVars)
		{
			this.GlobalVars = globalVars;
		}
	}
}