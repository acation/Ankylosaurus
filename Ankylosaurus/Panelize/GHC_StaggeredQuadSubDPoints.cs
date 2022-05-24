using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using static Ankylosaurus.Panelize.PanelUtility;

namespace Ankylosaurus.Panelize
{
    public class GHC_StaggeredQuadSubDPoints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_StaggeredQuadSubDPoints class.
        /// </summary>
        public GHC_StaggeredQuadSubDPoints()
          : base("StaggeredQuad Subdivide - Points", "PtSDivStag",
              "Extract a staggered isoparametric subset of a surface based on 2 lists of intersecting input points for U and V subdivision",
              "Ankylosaurus", "Panelize")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
            pManager.AddPointParameter("Intersection Points U", "ptU", "Intersection Points in U direction along surface edge", GH_ParamAccess.list);
            pManager.AddPointParameter("Intersection Points V", "ptV", "Intersection Points in V direction along surface edge", GH_ParamAccess.list);
            pManager[1].Optional = true; pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface Panels", "S", "Output dynamic panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
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
            List<double> iNumberListU = GetSrfPointParameter(iSrf, iPtsU, u);
            List<double> iNumberListV = GetSrfPointParameter(iSrf, iPtsV, v);


            // U is the staggered value
            // V is uniform

            iNumberListU.Sort();
            iNumberListV.Sort();

            List<Surface> subSrfV = new List<Surface>();
            List<GH_Surface> subSrfU = new List<GH_Surface>();

            // First Divide the surface into it's V spacing
            List<Interval> consecDomainsV = ConsecutiveDomains(iNumberListV);

            for (int i = 0; i < consecDomainsV.Count; i++)
            {
                subSrfV.Add(iSrf.Trim(new Interval(0.0, 1.0), consecDomainsV[i]));
            }

            // Next we need the U staggered spacing divisions
            List<Interval> consecDomainsRegularU = ConsecutiveDomains(iNumberListU);
            List<double> numberListStaggeredU = AverageNumbersConsecutive(iNumberListU);
            List<Interval> consecDomainsStaggeredU = ConsecutiveDomains(numberListStaggeredU);

            for (int i = 0; i < subSrfV.Count; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < consecDomainsRegularU.Count; j++)
                        subSrfU.Add(new GH_Surface(subSrfV[i].Trim(consecDomainsRegularU[j], new Interval(0.0, 1.0))));
                }
                else
                {
                    for (int j = 0; j < consecDomainsStaggeredU.Count; j++)
                        subSrfU.Add(new GH_Surface(subSrfV[i].Trim(consecDomainsStaggeredU[j], new Interval(0.0, 1.0))));
                }
            }

            DA.SetDataList(0, subSrfU);
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
                return Ankylosaurus.Properties.Resources.Staggered_SubD_Points;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("17523AE1-093B-4682-9F7C-CD68D61118D1"); }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }
    }
}