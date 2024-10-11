using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Ankylosaurus.Util
{
    public class GHC_CrvDivideTargetDistance : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_CrvDivideTargetDistance class.
        /// </summary>
        public GHC_CrvDivideTargetDistance()
          : base("Divide Crv by Target Distance", "CrvTargDist",
              "Iteratively divides a curve by equal distance until all points are as close as possible to the target distance. This means that if you draw a polyline through" +
                "the points, all segemnts would be of equal length that approximates the target distance.",
              "Ankylosaurus", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "The input curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Target Distance", "D", "The target distance to divide the curve into equal parts", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "The output points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve iCrv = null;
            double iTargetDist = 0.0;
            int iSteps = 100;

            DA.GetData("Curve", ref iCrv);
            DA.GetData("Target Distance", ref iTargetDist);

            Point3d[] pts = CrvDivisionUtility.DivideCurveByTargetDistance(iCrv, iTargetDist, iSteps);
            List<GH_Point> ghPts = new List<GH_Point>();

            foreach (Point3d pt in pts)
            {
                ghPts.Add(new GH_Point(pt));
            }

            DA.SetDataList("Points", ghPts);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0236B6B2-03BE-4CF7-B8A5-97074F6F9792"); }
        }
    }
}