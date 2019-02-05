using System;
using System.Collections.Generic;
using System.Text;

namespace VkBotFramework.Models
{
    public class PeerContext
    {
	    public PeerContext(Dictionary<string, dynamic> globalVars)
	    {
		    this.GlobalVars = globalVars;
	    }
		public Dictionary<string, dynamic> Vars = new Dictionary<string, dynamic>();
		public Dictionary<string, dynamic> GlobalVars;
    }
}
