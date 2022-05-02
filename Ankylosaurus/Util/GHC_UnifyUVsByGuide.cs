using System;

using System.Collections.Generic;


using Rhino.Geometry;


using Grasshopper.Kernel;

using System.Linq;


namespace Ankylosaurus.Util
{
    public class GHC_UnifyUVsByGuide : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GHC_UnifyUVsByGuide()
          : base("Unify UVs by Guide Srf", "UnifyByGuide",
              "Unifies the UV directions of a list of brep faces / surfaces based on a guide surface. If it does not seem" +
                " like the component is working, then increase the tolerance. It may need to be >1.0 to work.",
              "Ankylosaurus", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Breps", "B", "Input breps or surfaces. The output will be faces of the breps if the input is joined.", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Guide Surface", "S", "Guide surface used to modify the U and V parameters of the input breps", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "T", "The vector angle tolerance in radians", GH_ParamAccess.item, 0.10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "The UV corrected output surfaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> iBreps = new List<Brep>();
            Surface iTestSrfs = null;
            double iTol = 0.0; 

            DA.GetDataList(0, iBreps);
            DA.GetData(1, ref iTestSrfs);
            DA.GetData(2, ref iTol);

            // Variable Setup
            List<Point3d> centerPts = new List<Point3d>();
            List<Vector3d[]> surfaceUVs = new List<Vector3d[]>();
            List<Vector3d> srfUs = new List<Vector3d>();
            List<Vector3d> srfVs = new List<Vector3d>();
            List<Surface> surfaces = new List<Surface>();

            List<Vector3d> testSrfUs = new List<Vector3d>();
            List<Vector3d> testSrfVs = new List<Vector3d>();

            // Setup Angle thresholds with tolerance to compare surfaces to
            double angle180Max = Math.PI + iTol; double angle180Min = Math.PI - iTol;
            double angle0Max = 0 + iTol; double angle0Min = 0 - iTol;


            // Loop for the Joined Brep list
            for (int i = 0; i < iBreps.Count; i++)
            {
                // Loop for all surfaces in the joined Brep
                for (int j = 0; j < iBreps[i].Faces.Count; j++)
                {
                    // Extract the messed up surface from the brep face
                    Brep brepSrf = iBreps[i].Faces.ExtractFace(j);
                    Surface srf = brepSrf.Surfaces.First();

                    // Get the Center point and UV vectors from messed up Surface
                    Point3d centPt = new Point3d();
                    Vector3d[] srfUV;
                    srf.Evaluate(srf.Domain(0).Mid, srf.Domain(1).Mid, 1, out centPt, out srfUV);
                    Vector3d srfU = srfUV[0];
                    Vector3d srfV = srfUV[1];

                    //Evaluate the Test Surface for comparison
                    double testUParam;
                    double testVParam;
                    Vector3d[] testUV;
                    Point3d testPt = new Point3d();
                    iTestSrfs.ClosestPoint(centPt, out testUParam, out testVParam);
                    iTestSrfs.Evaluate(testUParam, testVParam, 1, out testPt, out testUV);
                    //Get the comparison Vectors:
                    Vector3d testU = testUV[0];
                    Vector3d testV = testUV[1];

                    // Compare the vectors for transpose operation
                    // If they are not 0 or 180 degrees, the surface UVs need to be swapped
                    //    so they match test surface directionality
                    double vectorAngleT = Vector3d.VectorAngle(srfU, testU); // the transpose angle
                    if ((angle0Max <= vectorAngleT || vectorAngleT <= angle0Min) &&
                      (angle180Max <= vectorAngleT || vectorAngleT <= angle180Min))
                    {
                        // setting transpose to true modifies the srf directly
                        srf.Transpose(true);
                    }

                    // Get the new UV Vectors from the transposed surfaces
                    Point3d dumbPt = new Point3d();
                    Vector3d[] newSrfUV;
                    srf.Evaluate(srf.Domain(0).Mid, srf.Domain(1).Mid, 1, out dumbPt, out newSrfUV);
                    Vector3d newSrfU = newSrfUV[0];
                    Vector3d newSrfV = newSrfUV[1];

                    // Test to make sure U and V directions are correct
                    // If they are not within the 0 deg tolerance, they are reversed
                    double vecAngleU = Vector3d.VectorAngle(newSrfU, testU);
                    double vecAngleV = Vector3d.VectorAngle(newSrfV, testV);

                    if (angle0Max <= vecAngleU || vecAngleU <= angle0Min)
                    {
                        srf.Reverse(0, true);
                    }
                    if (angle0Max <= vecAngleV || vecAngleV <= angle0Min)
                    {
                        srf.Reverse(1, true);
                    }


                    // Set the Lists for outputting
                   /* centerPts.Add(centPt);
                    surfaceUVs.Add(srfUV);
                    srfUs.Add(newSrfU);
                    srfVs.Add(newSrfV);
                    testSrfUs.Add(testU);
                    testSrfVs.Add(testV);*/
                    surfaces.Add(srf);
                }
            }

            DA.SetDataList(0, surfaces);
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
                return Ankylosaurus.Properties.Resources.Util_UnifySrf_Dir_by_Guide;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("13B3B820-119E-4BF6-AC72-AC239322FB85"); }
        }
    }
}