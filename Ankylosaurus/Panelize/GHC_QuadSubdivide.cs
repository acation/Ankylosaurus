using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Ankylosaurus.Panelize
{
	public class GHC_QuadSubdivide : GH_Component
	{
		
		public GHC_QuadSubdivide()
		  : base("Quad Subdivide", "SubDivQ",
			  "Extract an isoparametric subset of a surface based on U and V domain input",
			  "Ankylosaurus", "Panelize")
		{
		}

		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
			pManager.AddIntegerParameter("U", "U", "U division parameter", GH_ParamAccess.item, 10);
			pManager.AddIntegerParameter("V", "V", "V division parameter", GH_ParamAccess.item, 10);
		}

		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Subset of base surfaces(s)", GH_ParamAccess.list);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Surface iSrf = null;
			int iU = 0;
			int iV = 0;

			DA.GetData("Surface", ref iSrf);
			DA.GetData("U", ref iU);
			DA.GetData("V", ref iV);

			Surface iSurface = PanelUtility.ReparameterizeSurface(iSrf);
			Interval surfU = iSurface.Domain(0);
			Interval surfV = iSurface.Domain(1);

			double uStep = surfU.Max / iU;
			double vStep = surfV.Max / iV;
			List<double> numberListU = new List<double>();
			for (int u = 0; u < iU + 1; u++)
				numberListU.Add(uStep * u);
			List<double> numberListV = new List<double>();
			for (int v = 0; v < iV + 1; v++)
				numberListV.Add(vStep * v);

			List<Interval> consecDomainsU = PanelUtility.ConsecutiveDomains(numberListU);
			List<Interval> consecDomainsV = PanelUtility.ConsecutiveDomains(numberListV);

			List<Interval> crossRefU = new List<Interval>();
			List<Interval> crossRefV = new List<Interval>();

			for (int i = 0; i < consecDomainsV.Count; i++)
			{
				for (int j = 0; j < consecDomainsU.Count; j++)
				{
					crossRefU.Add(consecDomainsU[j]);
					crossRefV.Add(consecDomainsV[i]);
				}
			}

			List<GH_Surface> subSrf = new List<GH_Surface>();

			for (int i = 0; i < crossRefU.Count; i++)
			{
				subSrf.Add(new GH_Surface(iSurface.Trim(crossRefU[i], crossRefV[i])));
			}

			/* It is unnecessary to reparam the surface I think?
             * 
            for (int i = 0; i < iSurface.Count; i++)
            {
                PanelUtility.ReparameterizeSurface(iSurface[i]);
            }
            */

			// RETURNS A GH_SURFACE DATA TREE
			// DataTree<GH_Surface> subSurfTree = PanelUtility.DivideSurfaceList(iSurface, iU, iV); 

			DA.SetDataList(0, subSrf);
		}

		
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Quad_SubD;
			}
		}

		
		public override Guid ComponentGuid
		{
			get { return new Guid("cabc5478-3622-45c5-9649-c938592c1f87"); }
		}
	}
}