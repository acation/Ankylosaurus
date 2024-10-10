using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Ankylosaurus.Util
{
    public class GHC_SeparateCurves_Lines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_SeparateCurves_Lines class.
        /// </summary>
        public GHC_SeparateCurves_Lines()
          : base("Separate Curves & Lines", "CrvsVsLns",
              "Use a tolerance to separate Curves from Lines. Curves with a a degree of curvature lower than the linearity tolerance will be output as a line.",
              "Ankylosaurus", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Input curves", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "t", "The tolerance for testing linearity", GH_ParamAccess.item, 0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Output curves - curvature is greater than linearity tolerance", GH_ParamAccess.item);
            pManager.AddLineParameter("Line", "L", "Output lines - curvature is less than linearity tolerance", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Boolean", "B", "The boolean for whether the output is linear: aka a curve (false) or a line (true)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve iCrv = null;
            double iTol = 0.0;
            GH_Curve curve = null;
            GH_Line line = null;
            GH_Boolean testBool = null;

            DA.GetData("Curve", ref iCrv);
            DA.GetData("Tolerance", ref iTol);

            //Test the curve for linearity - create a line if it is within tolerance
            if (iCrv.IsLinear(iTol))
            {
                Point3d startPt = iCrv.PointAtStart;
                Point3d endPt = iCrv.PointAtEnd;

                line = new GH_Line(new Line(startPt, endPt));
                testBool = new GH_Boolean(true);
            }
            else
            {
                curve = new GH_Curve(iCrv);
                testBool = new GH_Boolean(false);
            }

            DA.SetData("Curve", curve);
            DA.SetData("Line", line);
            DA.SetData("Boolean", testBool);

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
            get { return new Guid("CF954962-32F1-4BD1-9B66-AF96C80454E8"); }
        }
    }
}