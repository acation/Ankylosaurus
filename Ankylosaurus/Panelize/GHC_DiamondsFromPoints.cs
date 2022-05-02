using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using static Ankylosaurus.Panelize.PanelUtility;
using static Ankylosaurus.Panelize.PanDiamondUtil;
using Grasshopper.Kernel.Types;


namespace Ankylosaurus.Panelize
{
    public class GHC_DiamondsFromPoints : GH_Component
    {
        
        public GHC_DiamondsFromPoints()
          : base("Diamonds From Points", "DiaFromPts",
              "Generate diamond panels from a custom grid of points",
              "Ankylosaurus", "Panelize")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Input points to turn to diamond panels", GH_ParamAccess.list);
            pManager.AddIntegerParameter("U", "U", "U division parameter", GH_ParamAccess.item);
            pManager.AddIntegerParameter("V", "V", "V division parameter", GH_ParamAccess.item);
        }

        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Diamonds", "D", "Diamond panels", GH_ParamAccess.list);
            pManager.AddBrepParameter("Triangles", "T", "Triangular edge panels", GH_ParamAccess.list);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Point3d> srfPts = new List<Point3d>();
            int iU = 0;
            int iV = 0;

            DA.GetDataList(0, srfPts);
            DA.GetData(1, ref iU);
            DA.GetData(2, ref iV);

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
                return Ankylosaurus.Properties.Resources.Diamond_FROM_Pts;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ef763de3-f7d1-4185-b5f2-ae522fdcb70d"); }
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