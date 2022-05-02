using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.Panelize
{
    public class GHC__DivideSurface_Numeric : GH_Component
    {
        
        public GHC__DivideSurface_Numeric()
          : base("Divide Surface - Numeric", "SDivNums",
              "Generate a grid of {UV} points on a surface based on 2 number lists for U and V divisions",
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
            pManager.AddPointParameter("Points", "P", "Division points", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Normals", "N", "Normal vectors at division points", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Parameters", "uv", "Parameter (uv) coordinates at division points", GH_ParamAccess.tree);
        }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Surface iSrf = null;
            List<double> iUList = new List<double>();
            List<double> iVList = new List<double>();

            DA.GetData(0, ref iSrf);
            DA.GetDataList(1, iUList);
            DA.GetDataList(2, iVList);

            Surface iSurface = iSrf;
            Interval surfU = iSurface.Domain(0);
            Interval surfV = iSurface.Domain(1);

            int iU = iUList.Count;
            int iV = iVList.Count;

            DataTree<GH_Point> srfPts = new DataTree<GH_Point>();
            DataTree<GH_Vector> srfNormals = new DataTree<GH_Vector>();
            DataTree<GH_Vector> srfUVs = new DataTree<GH_Vector>();

            List<double> numberListU = iUList;
            List<double> numberListV = iVList;

            for (int i = 0; i < numberListU.Count; i++)
            {
                for (int j = 0; j < numberListV.Count; j++)
                {
                    double u = numberListU[i];
                    double v = numberListV[j];

                    GH_Point srfPt = new GH_Point(iSurface.PointAt(u, v));
                    srfPts.Add(srfPt, new GH_Path(i));

                    GH_Vector srfNorm = new GH_Vector(iSurface.NormalAt(u, v));
                    srfNormals.Add(srfNorm, new GH_Path(i));

                    GH_Vector srfParam = new GH_Vector(new Vector3d(u, v, 0.0));
                    srfUVs.Add(srfParam, new GH_Path(i));
                }
            }

            DA.SetDataTree(0, srfPts);
            DA.SetDataTree(1, srfNormals);
            DA.SetDataTree(2, srfUVs);

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
                return Ankylosaurus.Properties.Resources.Divide_Srf_Numeric;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1fb8627e-b5c3-4f11-b35f-59edf35b634c"); }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.quarternary;
            }
        }

    }
}