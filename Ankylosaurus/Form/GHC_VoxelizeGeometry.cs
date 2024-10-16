using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Ankylosaurus.Form
{
    public class GHC_VoxelizeGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_VoxelizeGeometry class.
        /// </summary>
        public GHC_VoxelizeGeometry()
          : base("Voxelize Geometry", "Voxels",
              "Create a voxel grid on a closed input mesh. The input plane changes the coordinate space of the voxels. Outputs voxels as Boxes",
              "Ankylosaurus", "Form")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "The base plane for the voxel system", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddMeshParameter("Mesh", "M", "The closed mesh to be voxelized", GH_ParamAccess.item);
            pManager.AddNumberParameter("X", "X", "The size of the voxel in the X dimension", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Y", "Y", "The size of the voxel in the Y dimension", GH_ParamAccess.item, 1.5);
            pManager.AddNumberParameter("Z", "Z", "The size of the voxel in the Z dimension", GH_ParamAccess.item, 2.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Voxel Boxes", "B", "The ouput voxels as boxes", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Voxel Planes", "P", "The planes at the center of each voxel", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Establish inputs
            Plane iBasePlane = new Plane();
            Mesh iMesh = new Mesh();
            double iX = 0.0;
            double iY = 0.0;
            double iZ = 0.0;

            DA.GetData("Plane", ref iBasePlane);
            DA.GetData("Mesh", ref iMesh);
            DA.GetData("X", ref iX);
            DA.GetData("Y", ref iY);
            DA.GetData("Z", ref iZ);

            // LOGIC MFer

            //Initialize the output lists
            List<GH_Box> voxelBoxes = new List<GH_Box>();
            // List<GH_Point> centers = new List<GH_Point>();
            List<GH_Plane> voxelPlanes = new List<GH_Plane>();

            if (iMesh.IsClosed == false)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh needs to be closed doofus");
                return;
            }

            // Calculate the bounding box of the mesh in the iBasePlane orientation
            BoundingBox bbox = iMesh.GetBoundingBox(iBasePlane);

            // Calculate grid points based on bounding box and plane orientation
            for (double x = bbox.Min.X; x <= bbox.Max.X; x += iX)
            {
                for (double y = bbox.Min.Y; y <= bbox.Max.Y; y += iY)
                {
                    for (double z = bbox.Min.Z; z <= bbox.Max.Z; z += iZ)
                    {
                        // Calculate the voxel center point in iBasePlane's coordinate system
                        Point3d voxelCenter = iBasePlane.PointAt(x + iX / 2.0, y + iY / 2.0, z + iZ / 2.0);

                        //Create the voxel output plane
                        Plane voxelPlane = new Plane(voxelCenter, iBasePlane.XAxis, iBasePlane.YAxis);

                        // Create the voxel box aligned with iBasePlane
                        Box voxelBox = new Box(iBasePlane,
                            new Interval(x, x + iX),
                            new Interval(y, y + iY),
                            new Interval(z, z + iZ));

                        // Check if the center of the voxel is inside the mesh
                        if (iMesh.IsPointInside(voxelCenter, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true))
                        {
                            // Add the voxel box and its center point to the output lists
                            voxelBoxes.Add(new GH_Box(voxelBox));
                            //centers.Add(new GH_Point(voxelCenter));
                            voxelPlanes.Add(new GH_Plane(voxelPlane));
                        }
                    }
                }
            }

            // Assign outputs - Just going to output planes so it speeds up calc slightly
            DA.SetDataList("Voxel Boxes", voxelBoxes);
            DA.SetDataList("Voxel Planes", voxelPlanes);
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
            get { return new Guid("E5B69E9A-86DD-48AF-9F36-9A8E3F03DFD2"); }
        }
    }
}