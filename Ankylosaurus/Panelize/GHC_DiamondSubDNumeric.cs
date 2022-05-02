using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using static Ankylosaurus.Panelize.PanelUtility;
using static Ankylosaurus.Panelize.PanDiamondUtil;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.Panelize
{
    public class GHC_DiamondSubDNumeric : GH_Component
    {
        
        public GHC_DiamondSubDNumeric()
          : base("Diamond Subdivide - Numeric", "DiaNum",
              "Make a diamond grid on a surface with 2 lists of numeric input values between 0 and 1",
              "Ankylosaurus", "Panelize")
        {
        }

        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Base surface", GH_ParamAccess.item);
            pManager.AddNumberParameter("Numbers U", "nU", "A dynamic list of U division parameters between 0 and 1", GH_ParamAccess.list);
            pManager.AddNumberParameter("Numbers V", "nV", "A dynamic list of V division parameters between 0 and 1", GH_ParamAccess.list);
        }

        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Diamonds", "D", "Diamond panels", GH_ParamAccess.list);
            pManager.AddBrepParameter("Triangles", "T", "Triangular edge panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface iSrf = null;
            List<double> iUList = new List<double>();
            List<double> iVList = new List<double>();

            DA.GetData(0, ref iSrf);
            DA.GetDataList(1, iUList);
            DA.GetDataList(2, iVList);

            Surface iSurface = ReparameterizeSurface(iSrf);
            Interval surfU = iSurface.Domain(0);
            Interval surfV = iSurface.Domain(1);

            // Create vertices for the panels
            int iU = iUList.Count - 1;
            int iV = iVList.Count - 1;

            List<Point3d> srfPoints = new List<Point3d>();

            List<double> numberListU = iUList;
            List<double> numberListV = iVList;

            for (int i = 0; i < numberListU.Count; i++)
            {
                for (int j = 0; j < numberListV.Count; j++)
                {
                    Point3d srfPt = iSurface.PointAt(numberListU[i], numberListV[j]);
                    srfPoints.Add(srfPt);
                }
            }

            // Make the panels, get diamonds and triangles in a Tuple
            Tuple<List<Brep>, List<Brep>> allPanels = DiamondsFromPoints(srfPoints, iU, iV);

            List<GH_Brep> diamondPanels = new List<GH_Brep>();
            List<GH_Brep> trianglePanels = new List<GH_Brep>();

            // allPanels.Item1 are diamond panels && allPanels.Item2 are triangle panels
            foreach (var diamond in allPanels.Item1)
                diamondPanels.Add(new GH_Brep(diamond));
            foreach (var triangle in allPanels.Item2)
                trianglePanels.Add(new GH_Brep(triangle));

            DA.SetDataList(0, diamondPanels);
            DA.SetDataList(1, trianglePanels);
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
                return Ankylosaurus.Properties.Resources.Diamond_SubD_Numeric;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f112b7fd-f978-42ec-993a-54d0582d42e2"); }
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