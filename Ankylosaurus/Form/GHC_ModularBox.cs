using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Ankylosaurus.Form
{
    public class GHC_ModularBox : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GHC_ModularBox()
          : base("Modular Box", "ModBox",
              "A modular box that is constrained to specific XYZ dimensions without having to think about the math ¯\\_(ツ)_/¯",
              "Ankylosaurus", "Form")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Base Plane", "P", "The base plane of the box", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntervalParameter("Domain X", "Dx", "Domain for X direction", GH_ParamAccess.item, new Interval(-5, 5));
            pManager.AddIntervalParameter("Domain Y", "Dy", "Domain for Y direction", GH_ParamAccess.item, new Interval(-5, 5));
            pManager.AddIntervalParameter("Domain Z", "Dz", "Domain for Z direction", GH_ParamAccess.item, new Interval(-5, 5));
            pManager.AddNumberParameter("Constraint X", "Cx", "Modular distance constraint for X direction", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Constraint Y", "Cy", "Modular distance constraint for Y direction", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Constraint Z", "Cz", "Modular distance constraint for Z direction", GH_ParamAccess.item, 0.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Box", "B", "The resulting modular box", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.Unset;
            Interval ix = new Interval();
            Interval iy = new Interval();
            Interval iz = new Interval();
            double cx = 0, cy = 0, cz = 0;

            if (!DA.GetData(0, ref plane)) return;
            if (!DA.GetData(1, ref ix)) return;
            if (!DA.GetData(2, ref iy)) return;
            if (!DA.GetData(3, ref iz)) return;
            if (!DA.GetData(4, ref cx)) return;
            if (!DA.GetData(5, ref cy)) return;
            if (!DA.GetData(6, ref cz)) return;

            if (cx > ix.Length / 2 || cy > iy.Length / 2 || cz > iz.Length / 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Hey doofus, the module length is larger than the domain (if you really give a heck). " +
                    "The box lengths will start to equal the respective constraint size ¯\\_(ツ)_/¯");
            }

            // Adjust intervals to fit modular constraints
            ix = ConstrainInterval(ix, cx);
            iy = ConstrainInterval(iy, cy);
            iz = ConstrainInterval(iz, cz);

            // Create the box with the adjusted intervals
            GH_Box box = new GH_Box(new Box(plane, ix, iy, iz));

            // Output the box
            DA.SetData(0, box);
        }

        private Interval ConstrainInterval(Interval interval, double constraint)
        {
            if (constraint <= 0) return interval;

            double start = interval.Min;
            double end = interval.Max;

            // Calculate the new length as a multiple of the constraint
            double length = end - start;
            double modularLength = Math.Round(length / constraint) * constraint;

            // Center the interval around the original midpoint
            double midpoint = (start + end) / 2;
            double halfLength = modularLength / 2;

            return new Interval(midpoint - halfLength, midpoint + halfLength);
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
                return Ankylosaurus.Properties.Resources.Form_ModularBox;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("13D7951C-C47A-4D52-B249-8BCE491EF4B0"); }
        }
    }
}