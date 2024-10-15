using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GrasshopperAsyncComponent;
using Rhino;
using Rhino.Geometry;

namespace Ankylosaurus.Form
{
    public class Brep_Cell : GH_AsyncComponent
    {
        /// <summary>
        /// Initializes a new instance of the Brep_Cell class.
        /// </summary>
        public Brep_Cell()
          : base("Brep Cell", "Cell",
              "Takes any closed Brep, scales it, and lofts the faces of the outer brep to the inner brep.\n" +
                "\nComponent uses Async adapted from: " +
                "https://github.com/specklesystems/GrasshopperAsyncComponent/blob/main/LICENSE",
              "Ankylosaurus", "Form")
        {
            // NEED TO PROVIDE THE WORKER CLASS HERE OR IT WILL NOT WORK!!!!!!
            BaseWorker = new CellWorker();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Input closed brep.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Brep Scale", "Bs", "Scales the original Brep", GH_ParamAccess.item, 0.35);
            pManager.AddNumberParameter("Face Scale", "Fs", "Scales the faces of the original Brep", GH_ParamAccess.item, 0.2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep Cell", "B", "The output brep", GH_ParamAccess.item);
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
            get { return new Guid("75F4A7DF-D4EF-4290-9509-A9FC3F47535F"); }
        }

        // ADDS AN OVERRIDE TO "CANCEL" The operation if it is too slow - copy this into any async component
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, "Cancel", (s, e) =>
            {
                RequestCancellation();
            });
        }

        // The solver class - create a workinstance that runs on a different thread
        // This is necessary for aSync
        private class CellWorker : WorkerInstance
        {
            // Needs a constructor
            public CellWorker() : base(null) { }

            //NEED ACCESSIBLE GLOBAL VARIABLES / INPUTS AND OUTPUTS
            // GLOBAL INPUTS
            Brep iBrep = null;
            double iFormScale = 0.0;
            double iFaceScale = 0.0;
            // GLOBAL OUTPUTS
            GH_Brep oBrepCell = null;

            // Get Data defines the inputs and outputs
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                // INPUTS
                Brep _iBrep = null;
                double _iFormScale = 0.0;
                double _iFaceScale = 0.0;

                DA.GetData(0, ref _iBrep);
                DA.GetData(1, ref _iFormScale);
                DA.GetData(2, ref _iFaceScale);

                // Set the input data to global variables
                iBrep = _iBrep;
                iFormScale = _iFormScale;
                iFaceScale = _iFaceScale;
            }

            // The following is the replacement for Solve Instance - Do the Work Dummy
            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // 👉 Checking for cancellation!
                if (CancellationToken.IsCancellationRequested) { return; }

                // COMPUTING LOGIC BELOW HERE
                AreaMassProperties brepAreaProp = AreaMassProperties.Compute(iBrep);
                // Maybe should figure out a faster method for finding centers
                Point3d brepCenterPt = brepAreaProp.Centroid;
                Transform xScaleBrep = Transform.Scale(brepCenterPt, iFormScale);

                // The transform operation is a type Bool - so will need to copy the brep
                Brep scaledBrep = iBrep.DuplicateBrep();
                scaledBrep.Transform(xScaleBrep);

                List<Brep> loftedBreps = new List<Brep>();
                List<Brep> scaledFaces = new List<Brep>();

                for (int i = 0; i < iBrep.Faces.Count; i++)
                {
                    // 👉 Checking for cancellation!
                    if (CancellationToken.IsCancellationRequested) { return; }

                    BrepFace iBrepFace = iBrep.Faces[i];
                    // This is the correct funtion to turn face into BREP --- NOT ToBrep() which
                    // only returns an untrimmed surface representation of a brep
                    Brep newBrepFace = iBrepFace.DuplicateFace(true);
                    BrepFace scaleBrepFace = scaledBrep.Faces[i];
                    Brep endFace = scaleBrepFace.DuplicateFace(true);

                    AreaMassProperties faceArea = AreaMassProperties.Compute(newBrepFace);
                    Point3d facePt = faceArea.Centroid;
                    Transform xScaleFace = Transform.Scale(facePt, iFaceScale);
                    //BrepFace newFace = iBrepFace.DuplicateFace();

                    newBrepFace.Transform(xScaleFace);
                    scaledFaces.Add(newBrepFace);
                    // it is incorrect to turn the face into brep like this:
                    // Brep scaledFace = iBrepFace.ToBrep();

                    Curve[] scaledFaceCrvs = newBrepFace.DuplicateEdgeCurves(true);
                    //Brep endFace = scaleBrepFace.ToBrep();
                    Curve[] endFaceCrvs = endFace.DuplicateEdgeCurves(true);


                    for (int j = 0; j < endFaceCrvs.Length; j++)
                    {
                        List<Curve> loftCrvs = new List<Curve>();

                        Point3d pt1 = scaledFaceCrvs[j].PointAtStart;
                        Point3d pt2 = scaledFaceCrvs[j].PointAtEnd;
                        Point3d pt3 = endFaceCrvs[j].PointAtEnd;
                        Point3d pt4 = endFaceCrvs[j].PointAtStart;

                        Brep loftedFace = Brep.CreateFromCornerPoints(pt1, pt2, pt3, pt4,
                            RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                        loftedBreps.Add(loftedFace);
                    }
                    // This reports the progress of the 4 loop - should be in loop calculation
                    ReportProgress(Id, i / (double)iBrep.Faces.Count);
                }

                Brep[] joinedBreps = Brep.JoinBreps(loftedBreps, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                oBrepCell = new GH_Brep(joinedBreps[0]);

                // IMPORTANT NEED TO CALL DONE TO LET IT KNOW
                Done();
            }



            public override void SetData(IGH_DataAccess DA)
            {
                // 👉 Checking for cancellation!
                if (CancellationToken.IsCancellationRequested) { return; }
                DA.SetData(0, oBrepCell);
            }

            // NEED THIS - JUST CALL A NEW INSTANCE OF THIS CLASS
            public override WorkerInstance Duplicate() => new CellWorker();
        }
    }
}