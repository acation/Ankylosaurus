using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.Panelize
{
	public class GHC_QuadSubdivideNumeric : GH_Component
	{
		
		public GHC_QuadSubdivideNumeric()
		  : base("Quad Subdivide - Numeric", "NumSDivQ",
			  "Extract an isoparametric subset of a surface dynamically, based on numeric lists of U and V domain input parameter values. "
				+ "The length of each incoming list will correspond to the number of divisions.",
			  "Ankylosaurus", "Panelize")
		{
		}


		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
			pManager.AddNumberParameter("Numbers U", "nU", "A dynamic list of U division parameters", GH_ParamAccess.list);
			pManager.AddNumberParameter("Numbers V", "nV", "A dynamic list of V division parameters", GH_ParamAccess.list);
			pManager[1].Optional = true; pManager[2].Optional = true;
		}


		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface Panels", "S", "Output dynamic panels", GH_ParamAccess.list);
		}


		protected override void SolveInstance(IGH_DataAccess DA)
		{
			List<double> iNumberList1 = new List<double>();
			List<double> iNumberList2 = new List<double>();
			Surface iSurface = null;

			DA.GetDataList("Numbers U", iNumberList1);
			DA.GetDataList("Numbers V", iNumberList2);
			DA.GetData("Surface", ref iSurface);

			iNumberList1.Sort();
			iNumberList2.Sort();

			List<Interval> consecDomainsU = PanelUtility.ConsecutiveDomains(iNumberList1);
			List<Interval> consecDomainsV = PanelUtility.ConsecutiveDomains(iNumberList2);

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

			List<GH_Surface> subSrfDynamic = new List<GH_Surface>();

			for (int i = 0; i < crossRefU.Count; i++)
			{
				subSrfDynamic.Add(new GH_Surface(iSurface.Trim(crossRefU[i], crossRefV[i])));
			}

			DA.SetDataList("Surface Panels", subSrfDynamic);
		}


		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Quad_SubD_Numeric;
			}
		}

		
		public override Guid ComponentGuid
		{
			get { return new Guid("bf5be944-b6a0-45e7-bf47-b76f3a181a69"); }
		}
	}
}