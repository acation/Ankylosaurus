//using System;
//using System.Collections.Generic;

//using Grasshopper.Kernel;
//using Rhino.Geometry;

//namespace Ankylosaurus.Util
//{
//    public class GHC_QuadEdges : GH_Component
//    {
        
//        public GHC_QuadEdges()
//          : base("Quad Edges", "QEdges",
//              "Separates the edges of quad surfaces based on a direction vector. The direction vector is crossed with the Surface normal to determine left and right",
//              "Ankylosaurus", "Util")
//        {
//        }

        
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddSurfaceParameter("Surface", "S", "Input surface", GH_ParamAccess.item);
//            pManager.AddVectorParameter("Vector", "V", "Direction vector to separate segments from", GH_ParamAccess.item, Vector3d.ZAxis);
//            pManager.AddNumberParameter("Tolerance", "T", "The tolerance to test segments against vector.", GH_ParamAccess.item, 0.02);
//        }

        
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddCurveParameter("Top Edge", "T", "Curve that is perpindicular to input vector - the top edge of the surface", GH_ParamAccess.list);
//            pManager.AddCurveParameter("Bottom Edge", "B", "Curve that is perpindicular to the reverse of the input vector - the bottom edge of the surface", GH_ParamAccess.list);
//            pManager.AddCurveParameter("Right Edge", "R", "Curve that is the right edge of surface", GH_ParamAccess.list);
//            pManager.AddCurveParameter("Left Edge", "L", "Curve that is the left edge of surface", GH_ParamAccess.list);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            Brep iBrep = null;
//            Vector3d iVec = Vector3d.ZAxis;
//            double iTol = 0;

//            DA.GetData<Brep>(0, ref iBrep);
//            DA.GetData(1, ref iVec);
//            DA.GetData(2, ref iTol);

//            Curve[] segments = iBrep.DuplicateEdgeCurves();

//            if (segments.Length != 4)
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "There's a surface in here that isn't a quad, sucker!!!");
//            }

//            double srfU = iBrep.Faces[0].Domain(0).Max / 2;
//            double srfV = iBrep.Faces[0].Domain(1).Max / 2;
//            Point3d srfMidPt = iBrep.Faces[0].PointAt(srfU, srfV);
//            Vector3d srfNormal = iBrep.Faces[0].NormalAt(srfU, srfV);

//            Vector3d rightHandVector = Vector3d.CrossProduct(iVec, srfNormal);

//            List<Curve> crvsTop = new List<Curve>();
//            List<Curve> crvsBottom = new List<Curve>();
//            List<Curve> crvsRight = new List<Curve>();
//            List<Curve> crvsLeft = new List<Curve>();

//            double rightAngleMax = (Math.PI * 0.5) + iTol; double rightAngleMin = (Math.PI * 0.5) - iTol;
//            double lowAngleMax = 0 + iTol; double lowAngleMin = 0 - iTol;

//            for (int i = 0; i < segments.Length; i++)
//            {
//                Point3d startPt = segments[i].PointAtStart;
//                Point3d endPt = segments[i].PointAtEnd;
//                Vector3d segVec = new Vector3d(endPt) - new Vector3d(startPt);

//                Point3d crvMidPt = segments[i].PointAtNormalizedLength(0.5);
//                Vector3d centerVector = new Vector3d(crvMidPt) - new Vector3d(srfMidPt);

//                // These Angles calculate for the top and bottom segments
//                double segmentAngle = Vector3d.VectorAngle(segVec, iVec);
//                double perpAngle = Vector3d.VectorAngle(centerVector, iVec);
//                // These Angles calculate for the left and right segments
//                double sideAngle = Vector3d.VectorAngle(segVec, rightHandVector);
//                double sidePerpAngle = Vector3d.VectorAngle(centerVector, rightHandVector);

//                if (rightAngleMax >= segmentAngle && segmentAngle >= rightAngleMin)
//                {
//                    if (lowAngleMax >= perpAngle && perpAngle >= lowAngleMin)
//                        crvsTop.Add(segments[i]);
//                    else
//                        crvsBottom.Add(segments[i]);
//                }

//                if (rightAngleMax >= sideAngle && sideAngle >= rightAngleMin)
//                {
//                    if (lowAngleMax >= sidePerpAngle && sidePerpAngle >= lowAngleMin)
//                        crvsRight.Add(segments[i]);
//                    else
//                        crvsLeft.Add(segments[i]);
//                }

//            }

//            DA.SetDataList(0, crvsTop);
//            DA.SetDataList(1, crvsBottom);
//            DA.SetDataList(2, crvsRight);
//            DA.SetDataList(3, crvsLeft);
            
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon
//        {
//            get
//            {
//                //You can add image files to your project resources and access them like this:
//                // return Resources.IconForThisComponent;
//                return null;
//            }
//        }

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid
//        {
//            get { return new Guid("d3b2a9ef-1ffc-423e-9ede-4fd82acb1461"); }
//        }
//    }
//}