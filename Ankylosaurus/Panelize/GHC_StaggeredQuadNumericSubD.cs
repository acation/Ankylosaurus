using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using static Ankylosaurus.Panelize.PanelUtility;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.Panelize
{
    public class GHC_StaggeredQuadNumericSubD : GH_Component
    {

        public GHC_StaggeredQuadNumericSubD()
          : base("Staggered Quad - Numeric", "NumStagSubD",
              "Extract staggered isoparametric subsets of a surface dynamically based on U and V value list input",
              "Ankylosaurus", "Panelize")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
            pManager.AddNumberParameter("Numbers U", "nU", "A dynamic list of U division parameters", GH_ParamAccess.list);
            pManager.AddNumberParameter("Numbers V", "nV", "A dynamic list of V division parameters", GH_ParamAccess.list);
        }


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
            List<double> iNumberListU = new List<double>();
            List<double> iNumberListV = new List<double>();

            DA.GetData(0, ref iSrf);
            DA.GetDataList(1, iNumberListU);
            DA.GetDataList(2, iNumberListV);

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
                return Ankylosaurus.Properties.Resources.Staggered_SubD_Numeric;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6cb1a842-4284-493b-8b41-2be228004e9f"); }
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