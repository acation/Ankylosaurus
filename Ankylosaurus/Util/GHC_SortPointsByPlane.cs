using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;

namespace Ankylosaurus.Util
{
    public class GHC_SortPointsByPlane : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_SortPointsByPlane class.
        /// </summary>
        public GHC_SortPointsByPlane()
          : base("Sort Points By Plane", "Pt-PlnSort",
              "Sort points 2 dimensionally on an input plane. This method uses angular sorting by finding the polar array from the center of the plane. " +
                "The start angle defines where the sorting begins (angle relative to Plane's X-axis, and it sorts counterclockwise.",
              "Ankylosaurus", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "The plane to sort points on", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddPointParameter("Points", "Pts", "The points to sort using angular polar array method", GH_ParamAccess.list);
            pManager.AddAngleParameter("Angle", "A", "The start angle relative to the input plane's X-axis. The points will be sorted counterclockwise from this angle.",
                GH_ParamAccess.item, RhinoMath.ToRadians(180));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Sorted Points", "Pts", "The sorted points", GH_ParamAccess.list);
        }

        // Make a variable for degrees
        private bool _useDegrees = false;

        // This is needed to test for radians or degrees
        protected override void BeforeSolveInstance()
        {
            _useDegrees = false;
            Param_Number angleParameter = Params.Input[2] as Param_Number;
            if (angleParameter != null)
                _useDegrees = angleParameter.UseDegrees;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane iPlane = Plane.Unset;
            List<Point3d> iPoints = new List<Point3d>();
            double iAngle = 0.0;

            DA.GetData(0, ref iPlane);
            DA.GetDataList(1, iPoints);
            DA.GetData(2, ref iAngle);


            // Initialize a list to store tuples of points and angles
            List<Tuple<Point3d, double>> pointAngles = new List<Tuple<Point3d, double>>();

            double startAngleRad = iAngle;

            // Convert the start angle from degrees to radians if param is set to degrees
            if (_useDegrees) 
                startAngleRad = RhinoMath.ToRadians(iAngle);

            // Loop through each point to calculate the polar angle
            foreach (Point3d point in iPoints)
            {
                // Project the point onto the plane
                Point3d projectedPoint = iPlane.ClosestPoint(point);

                // Translate the point relative to the plane origin
                Vector3d vec = projectedPoint - iPlane.Origin;

                // Find the angle between this vector and the X-axis of the plane
                double angle = Vector3d.VectorAngle(iPlane.XAxis, vec, iPlane);

                // Adjust the angle by subtracting the start angle
                angle -= startAngleRad;

                // Normalize the angle to the range [0, 2*PI)
                if (angle < 0) angle += 2 * Math.PI;

                // Store the point and its calculated angle
                pointAngles.Add(new Tuple<Point3d, double>(point, angle));
            }

            // Sort the points by their angles
            pointAngles.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            // Extract the sorted points into a list
            List<GH_Point> sortedPoints = new List<GH_Point>();
            foreach (var tuple in pointAngles)
            {
                sortedPoints.Add(new GH_Point(tuple.Item1));
            }

            // Output the sorted points
            DA.SetDataList(0, sortedPoints);
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
                return Ankylosaurus.Properties.Resources.Util_SortPointsByPlane;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1DDB012F-22CA-45AA-AEDD-9AE1615703B5"); }
        }
    }
}