using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using static Ankylosaurus.Panelize.PanelUtility;
using static Ankylosaurus.Panelize.PanDiamondUtil;

namespace Ankylosaurus.Panelize
{
    public class GHC_DiamondSubDPoints : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_DiamondSubDPoints class.
        /// </summary>
        public GHC_DiamondSubDPoints()
          : base("Diamond Subdivide - Points", "DiaPts",
              "Make a diamond grid on a surface with 2 lists of intersection points",
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
            pManager.AddBrepParameter("Triangles", "T", "Triangular edge panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get the intersection UV values from the points
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

            iNumberListU.Sort();
            iNumberListV.Sort();

            // Process the Diamond Panels
            // Create vertices for the panels
            int iU = iNumberListU.Count - 1;
            int iV = iNumberListV.Count - 1;

            List<Point3d> srfPoints = new List<Point3d>();

            List<double> numberListU = iNumberListU;
            List<double> numberListV = iNumberListV;

            for (int i = 0; i < numberListU.Count; i++)
            {
                for (int j = 0; j < numberListV.Count; j++)
                {
                    Point3d srfPt = iSrf.PointAt(numberListU[i], numberListV[j]);
                    srfPoints.Add(srfPt);
                }
            }

            // Make the panels, get diamonds and triangles in a Tuple
            Tuple<List<NurbsSurface>, List<NurbsSurface>> allPanels = DiamondsFromPoints(srfPoints, iU, iV);

            DA.SetDataList(0, allPanels.Item1);
            DA.SetDataList(1, allPanels.Item2);

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
                return Ankylosaurus.Properties.Resources.Diamond_SubD_Points;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("08CFA9A5-9ADB-452C-AB2C-88B2CE49D815"); }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }
    }
}