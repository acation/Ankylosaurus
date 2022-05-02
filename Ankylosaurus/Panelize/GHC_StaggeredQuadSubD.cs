using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using static Ankylosaurus.Panelize.PanelUtility;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.Panelize
{
    public class GHC_StaggeredQuadSubD : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_StaggeredQuadSubD class.
        /// </summary>
        public GHC_StaggeredQuadSubD()
          : base("Staggered Quad SubD", "StagQSubD",
              "Extract staggered isoparametric subsets of a surface based on U and V domain input",
              "Ankylosaurus", "Panelize")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "U", "U division parameter", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("V", "V", "V division parameter", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Staggered subset of base surfaces(s)", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface iSrf = null;
            int iU = 0;
            int iV = 0;

            DA.GetData(0, ref iSrf);
            DA.GetData(1, ref iU);
            DA.GetData(2, ref iV);

            // U is the staggered value
            // V is uniform

            Surface iSurface = ReparameterizeSurface(iSrf);
            Interval surfU = iSurface.Domain(0);
            Interval surfV = iSurface.Domain(1);

            List<Surface> subSrfV = new List<Surface>();
            List<GH_Surface> subSrfU = new List<GH_Surface>();

            double uStep = surfU.Max / iU;
            double vStep = surfV.Max / iV;
            List<double> numberListU = new List<double>();
            for (int u = 0; u < iU + 1; u++)
                numberListU.Add(uStep * u);
            List<double> numberListV = new List<double>();
            for (int v = 0; v < iV + 1; v++)
                numberListV.Add(vStep * v);

            // First Divide the surface into it's V spacing
            List<Interval> consecDomainsV = ConsecutiveDomains(numberListV);

            for (int i = 0; i < consecDomainsV.Count; i++)
            {
                subSrfV.Add(iSurface.Trim(new Interval(0.0, 1.0), consecDomainsV[i]));
            }

            // Next we need the U staggered spacing divisions
            List<Interval> consecDomainsRegularU = ConsecutiveDomains(numberListU);
            List<double> numberListStaggeredU = AverageNumbersConsecutive(numberListU);
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
                return Ankylosaurus.Properties.Resources.Staggered_SubD;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("073fa331-15f1-4cb3-8a3a-608375e9d909"); }
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