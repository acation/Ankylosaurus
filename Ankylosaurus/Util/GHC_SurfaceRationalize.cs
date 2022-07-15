using System;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Ankylosaurus.Panelize;

namespace Ankylosaurus.Util
{
	public class GHC_SurfaceRationalize : GH_Component
	{
		
		public GHC_SurfaceRationalize()
		  : base("Surface Rationalize", "SRat",
			  "Converts an isotrim / other doubly curved edge surface into a more rationalized panel with straight edges. There is an option to planarize the panel or not",
			  "Ankylosaurus", "Util")
		{
		}


		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface Panels", "S", "Input the curved edge surface panels here", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Planarize", "P", "Toggle whether or not the panels are planarized. They are planarized by projecting " +
				"them onto an evaluation plane along the surface", GH_ParamAccess.item, false);
			pManager.AddPointParameter("UV Parameter", "UV", "The UV parameter to planarize the Surface. " +
                "Use points with xy values between 0 and 1", GH_ParamAccess.item, new Point3d(0, 0, 0));
		}


		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface Panels", "S", "The resulting straight-edge panels", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Surface iSurface = null;
			bool iPlanar = false;
			Point3d iUV = new Point3d(0, 0, 0);

			DA.GetData("Surface Panels", ref iSurface);
			DA.GetData(1, ref iPlanar);
			DA.GetData(2, ref iUV);

			//Rebuild the surface so it is a reparameterized 1 degree surface
			Surface rebuiltSrf = iSurface.Rebuild(1, 1, 2, 2);
			PanelUtility.ReparameterizeSurface(rebuiltSrf);

			//Get Plane At Surface UV
			Plane UVplane = new Plane();
			rebuiltSrf.FrameAt(iUV.X, iUV.Y, out UVplane);

			//Planarize the Surface if iPlanar is true
			NurbsSurface planarSrf = null;
			if (iPlanar == true) { 


				Point3d corner1 = rebuiltSrf.PointAt(0, 0);
				Point3d corner2 = rebuiltSrf.PointAt(0, 1);
				Point3d corner3 = rebuiltSrf.PointAt(1, 1);
				Point3d corner4 = rebuiltSrf.PointAt(1, 0);

				Transform projection = Transform.PlanarProjection(UVplane);
				corner1.Transform(projection);
				corner2.Transform(projection);
				corner3.Transform(projection);
				corner4.Transform(projection);

				planarSrf = NurbsSurface.CreateFromCorners(corner1, corner2, corner3, corner4);
			}

			// GH_Surface Turns a RhinoCommon Surface into a GH readable surface and the following makes the output planar or not
			GH_Surface rationalSrf = null;
			if (iPlanar == false)
			{
				rationalSrf = new GH_Surface(rebuiltSrf);
			}
			else
			{
				rationalSrf = new GH_Surface(planarSrf);
			}

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