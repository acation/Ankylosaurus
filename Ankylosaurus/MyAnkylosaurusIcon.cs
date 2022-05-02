using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ankylosaurus
{
	public class MyAnkylosaurusIcon : Grasshopper.Kernel.GH_AssemblyPriority
	{
		public override Grasshopper.Kernel.GH_LoadingInstruction PriorityLoad()
		{
			Grasshopper.Instances.ComponentServer.AddCategoryIcon("Ankylosaurus", Ankylosaurus.Properties.Resources.Ankylosaurus_Logo_Small);
			Grasshopper.Instances.ComponentServer.AddCategoryShortName("Ankylosaurus", "Ankill");
			Grasshopper.Instances.ComponentServer.AddCategorySymbolName("Ankylosaurus", 'A');

			return Grasshopper.Kernel.GH_LoadingInstruction.Proceed;
		}
	}
}
