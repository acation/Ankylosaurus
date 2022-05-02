using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using static Ankylosaurus.Panelize.PanelUtility;

namespace Ankylosaurus.Panelize
{
	public class GHC_QuadSubdivideCurves : GH_Component
	{
		
		
		public GHC_QuadSubdivideCurves()
		  : base("Quad Subdivide - Curves", "CrvSDivQ",
			  "Extract an isoparametric subset of a surface based on 2 lists of intersecting input curves for U and V subdivision",
			  "Ankylosaurus", "Panelize")
		{
		}

		
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
			pManager.AddCurveParameter("Intersection Curves U", "cU", "Intersection Curves in U direction", GH_ParamAccess.list);
			pManager.AddCurveParameter("Intersection Curves V", "cV", "Intersection Curves in V direction", GH_ParamAccess.list);
			pManager[1].Optional = true; pManager[2].Optional = true;
		}

		
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface Panels", "S", "Output dynamic panels", GH_ParamAccess.list);
		}

		
		protected override void SolveInstance(IGH_DataAccess DA)
		{

			Surface iSrf = null;
			List<Curve> iCrvsU = new List<Curve>();
			List<Curve> iCrvsV = new List<Curve>();

			DA.GetData("Surface", ref iSrf);
			DA.GetDataList("Intersection Curves U", iCrvsU);
			DA.GetDataList("Intersection Curves V", iCrvsV);

			ReparameterizeSurface(iSrf);

			List<Point3d> srfPtsU = GetSrfCrvIntersectionPts(iSrf, iCrvsU);
			List<Point3d> srfPtsV = GetSrfCrvIntersectionPts(iSrf, iCrvsV);

			string u = "u"; string v = "v";
			List<double> paramU = GetSrfPointParameter(iSrf, srfPtsU, u);
			List<double> paramV = GetSrfPointParameter(iSrf, srfPtsV, v);

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

		
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Quad_SubD_Curves;
			}
		}

		
		public override Guid ComponentGuid
		{
			get { return new Guid("43269471-9d7b-4b9b-874d-5e7f4653622b"); }
		}
	}
}