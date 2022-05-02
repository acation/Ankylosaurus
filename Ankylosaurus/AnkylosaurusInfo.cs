using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Ankylosaurus
{
	public class AnkylosaurusInfo : GH_AssemblyInfo
	{
		public override string Name
		{
			get
			{
				return "Ankylosaurus";
			}
		}
		public override Bitmap Icon
		{
			get
			{
				//Return a 24x24 pixel bitmap to represent this GHA library.
				return Ankylosaurus.Properties.Resources.Ankylosaurus_Logo_Small;
			}
		}
		public override string Description
		{
			get
			{
				//Return a short string describing the purpose of this GHA library.
				return "";
			}
		}
		public override Guid Id
		{
			get
			{
				return new Guid("bc23f523-6e0d-4fa2-a9ed-4b9fd22b72a0");
			}
		}

		public override string AuthorName
		{
			get
			{
				//Return a string identifying you or your company.
				return "";
			}
		}
		public override string AuthorContact
		{
			get
			{
				//Return a string representing your preferred contact details.
				return "";
			}
		}
	}
}
