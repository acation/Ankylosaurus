using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using static Ankylosaurus.Panelize.PanelUtility;

namespace Ankylosaurus.Panelize
{
    public class GHC_QuadsFromPoints : GH_Component
    {
        
        public GHC_QuadsFromPoints()
          : base("Quad From Points", "QuadFromPts",
              "Generate quad panels from a custom grid of points",
              "Ankylosaurus", "Panelize")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Input points to turn to diamond panels", GH_ParamAccess.list);
            pManager.AddIntegerParameter("U", "U", "U division parameter", GH_ParamAccess.item);
            pManager.AddIntegerParameter("V", "V", "V division parameter", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "S", "Quad panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Point3d> srfPts = new List<Point3d>();
            int iU = 0;
            int iV = 0;

            DA.GetDataList(0, srfPts);
            DA.GetData(1, ref iU);
            DA.GetData(2, ref iV);

            List<GH_Brep> srfPanels = new List<GH_Brep>();

            // Create the surface panels. For each face, we need to obtain the indices of the four relevant vertices
            for (int u = 0; u < iU; u++)
                for (int v = 0; v < iV; v++)
                {
                    int v1 = GetPtIndex(u, v, iV);
                    int v2 = GetPtIndex(u + 1, v, iV);
                    int v3 = GetPtIndex(u + 1, v + 1, iV);
                    int v4 = GetPtIndex(u, v + 1, iV);

                    Brep panel = Brep.CreateFromCornerPoints(srfPts[v1], srfPts[v2], srfPts[v3], srfPts[v4], 0.001);
                    srfPanels.Add(new GH_Brep(panel));
                }

            DA.SetDataList(0, srfPanels);

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
                return Ankylosaurus.Properties.Resources.Quad_FROM_Pts;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4820b8ac-f4e5-4141-bf37-706056a04555"); }
        }
    }
}