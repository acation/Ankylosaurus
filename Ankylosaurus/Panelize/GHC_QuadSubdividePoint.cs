using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using static Ankylosaurus.Panelize.PanelUtility;

namespace Ankylosaurus.Panelize
{
	public class GHC_QuadSubdividePoint : GH_Component
	{
		
		public GHC_QuadSubdividePoint()
		  : base("Quad Subdivide - Points", "PtSDivQ",
			  "Extract an isoparametric subset of a surface based on 2 lists of intersecting input points for U and V subdivision",
			  "Ankylosaurus", "Panelize")
		{
		}

		
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
			pManager.AddPointParameter("Intersection Points U", "ptU", "Intersection Points in U direction along surface edge", GH_ParamAccess.list);
			pManager.AddPointParameter("Intersection Points V", "ptV", "Intersection Points in V direction along surface edge", GH_ParamAccess.list);
			pManager[1].Optional = true; pManager[2].Optional = true;
		}

		
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface Panels", "S", "Output dynamic panels", GH_ParamAccess.list);
		}

		
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Surface iSrf = null;
			List<Point3d> iPtsU = new List<Point3d>();
			List<Point3d> iPtsV = new List<Point3d>();

			DA.GetData(0, ref iSrf);
			DA.GetDataList(1, iPtsU);
			DA.GetDataList(2, iPtsV);

			ReparameterizeSurface(iSrf);

			string u = "u"; string v = "v";
			List<double> paramU = GetSrfPointParameter(iSrf, iPtsU, u);
			List<double> paramV = GetSrfPointParameter(iSrf, iPtsV, v);

			List<Interval> consecDomainsU = ConsecutiveDomains(paramU);
			List<Interval> consecDomainsV = ConsecutiveDomains(paramV);

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
				subSrfDynamic.Add(new GH_Surface(iSrf.Trim(crossRefU[i], crossRefV[i])));
			}

			DA.SetDataList("Surface Panels", subSrfDynamic);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Quad_SubD_Points_V2;
			}
		}

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("e39a8a18-fd60-4ff4-8e8b-a24029643575"); }
		}
	}
}