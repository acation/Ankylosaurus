using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Ankylosaurus.Form
{
    public class GHC_ModularRandomBox : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_ConstrainedRandomBox class.
        /// </summary>
        public GHC_ModularRandomBox()
          : base("Modular Random Box", "RndBox",
              "Create random boxes from input planes that constrain the dimensions to specific XYZ modular units",
              "Ankylosaurus", "Form")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Planes", "P", "List of center planes for the boxes", GH_ParamAccess.list, Plane.WorldYZ);
            pManager.AddNumberParameter("Max X", "X", "Maximum target size for X direction", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Max Y", "Y", "Maximum target size for Y direction", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Max Z", "Z", "Maximum target size for Z direction", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Constraint X", "Cx", "Modular constraint for X direction", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Constraint Y", "Cy", "Modular constraint for Y direction", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Constraint Z", "Cz", "Modular constraint for Z direction", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Seed", "S", "Random seed for generating consistent results", GH_ParamAccess.item, 666);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Boxes", "B", "The resulting list of random modular boxes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Plane> planes = new List<Plane>();
            double maxX = 0, maxY = 0, maxZ = 0;
            double cx = 0, cy = 0, cz = 0;
            int seed = 0;

            if (!DA.GetDataList(0, planes)) return;
            if (!DA.GetData(1, ref maxX)) return;
            if (!DA.GetData(2, ref maxY)) return;
            if (!DA.GetData(3, ref maxZ)) return;
            if (!DA.GetData(4, ref cx)) return;
            if (!DA.GetData(5, ref cy)) return;
            if (!DA.GetData(6, ref cz)) return;
            if (!DA.GetData(7, ref seed)) return;

            Random random = new Random(seed);
            List<GH_Box> boxes = new List<GH_Box>();

            foreach (var plane in planes)
            {
                // Generate random lengths within specified max values for each axis
                // By adding the constraint we ensure that the box is never smaller than the module, aka 0 length
                double targetX = (random.NextDouble() * (maxX - cx)) + cx;
                double targetY = (random.NextDouble() * (maxY - cy)) + cy;
                double targetZ = (random.NextDouble() * (maxZ - cz)) + cz;

                // Adjust to nearest modular length
                double modularX = Math.Round(targetX / cx) * cx;
                double modularY = Math.Round(targetY / cy) * cy;
                double modularZ = Math.Round(targetZ / cz) * cz;

                // Define intervals centered on origin (0, 0, 0) then move to the plane
                Interval intervalX = new Interval(-modularX / 2, modularX / 2);
                Interval intervalY = new Interval(-modularY / 2, modularY / 2);
                Interval intervalZ = new Interval(-modularZ / 2, modularZ / 2);

                // Create the box and transform to the plane
                Box box = new Box(plane, intervalX, intervalY, intervalZ);
                boxes.Add(new GH_Box(box));
            }

            DA.SetDataList(0, boxes);
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
                return Ankylosaurus.Properties.Resources.Form_RandomModularBox;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("50F85C00-BFEC-4F3F-BC91-FC3EE03829F1"); }
        }
    }
}