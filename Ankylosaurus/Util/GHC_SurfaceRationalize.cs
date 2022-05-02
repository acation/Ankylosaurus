using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.Util
{
	public class GHC_SurfaceRationalize : GH_Component
	{
		
		public GHC_SurfaceRationalize()
		  : base("Surface Rationalize", "SRat",
			  "Converts an isotrim / other doubly curved edge surface into a more 'rationalized' panel with straight edges",
			  "Ankylosaurus", "Util")
		{
		}


		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface Panels", "S", "Input the curved edge surface panels here", GH_ParamAccess.item);
		}


		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface Panels", "S", "The resulting straight-edge panels", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Surface iSurface = null;
			DA.GetData("Surface Panels", ref iSurface);

			GH_Surface rationalSrf = new GH_Surface(iSurface.Rebuild(1, 1, 2, 2));
			DA.SetData("Surface Panels", rationalSrf);
		}


		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Util_SrfRAT;
			}
		}

		
		public override Guid ComponentGuid
		{
			get { return new Guid("f43a5061-9b73-4415-bd99-e476cfb54954"); }
		}
	}
}