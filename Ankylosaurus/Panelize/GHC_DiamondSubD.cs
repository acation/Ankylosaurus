using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using static Ankylosaurus.Panelize.PanelUtility;
using static Ankylosaurus.Panelize.PanDiamondUtil;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.DiamondPanels
{
    public class GHC_DiamondSubD : GH_Component
    {
        
        public GHC_DiamondSubD()
          : base("Diamond Subdivide", "Diamonds",
              "Make a diamond grid on a surface",
              "Ankylosaurus", "Panelize")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "U", "U division parameter", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("V", "V", "V division parameter", GH_ParamAccess.item, 10);
        }

        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Diamonds", "D", "Diamond panels", GH_ParamAccess.list);
            pManager.AddBrepParameter("Triangles", "T", "Triangular edge panels", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Surface iSrf = null;
            int iU = 0;
            int iV = 0;

            DA.GetData(0, ref iSrf);
            DA.GetData(1, ref iU);
            DA.GetData(2, ref iV);

            Surface iSurface = ReparameterizeSurface(iSrf);
            Interval surfU = iSurface.Domain(0);
            Interval surfV = iSurface.Domain(1);

            List<Point3d> srfPts = new List<Point3d>();

            double uStep = surfU.Max / iU;
            double vStep = surfV.Max / iV;
            

            // Create vertices for the panels
            for (int i = 0; i < iU + 1; i++)
                for (int j = 0; j < iV + 1; j++)
                {
                    double u = i * uStep;
                    double v = j * vStep;
                    srfPts.Add(iSurface.PointAt(u, v));
                }

            // Make the panels, get diamonds and triangles in a Tuple
            Tuple<List<NurbsSurface>, List<NurbsSurface>> allPanels = DiamondsFromPoints(srfPts, iU, iV);

            DA.SetDataList(0, allPanels.Item1);
            DA.SetDataList(1, allPanels.Item2);

            /*List<GH_Brep> diamondPanels = new List<GH_Brep>();
            List<GH_Brep> trianglePanels = new List<GH_Brep>();

            // allPanels.Item1 are diamond panels && allPanels.Item2 are triangle panels
            foreach (var diamond in allPanels.Item1)
                diamondPanels.Add(new GH_Brep(diamond));
            foreach (var triangle in allPanels.Item2)
                trianglePanels.Add(new GH_Brep(triangle));

            DA.SetDataList(0, diamondPanels);
            DA.SetDataList(1, trianglePanels);*/

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
                return Ankylosaurus.Properties.Resources.Diamond_SubD;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("adf918e2-3963-403f-b756-aad5407605d3"); }
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