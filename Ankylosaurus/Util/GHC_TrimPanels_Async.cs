using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using GrasshopperAsyncComponent;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using Rhino.UI;
using Rhino;

namespace Ankylosaurus.Util
{
    public class GHC_TrimPanels_Async : GH_AsyncComponent
    {
        /// <summary>
        /// Initializes a new instance of the GHC_TrimPanels_Async class.
        /// </summary>
        public GHC_TrimPanels_Async()
          : base("Trim Panels", "PTrim",
              "This component is useful if you are panelizing a trimmed surface, and would like for the panels to be trimmed as well. " +
                "Keep in mind that the tolerance is comparing the distance of the split panel's center to the original surface and may need to be adjusted. " +
                "The trimming calculation itself uses the document tolerance. The way that inclusion is determined is by offsetting the trim surface and testing" +
                "for point inclusion within the thickened brep. This is what the inclusion distance is for.\n" +
                "\nComponent uses Async adapted from: " +
                "https://github.com/specklesystems/GrasshopperAsyncComponent/blob/main/LICENSE",
              "Ankylosaurus", "Util")
        {
            // NEED TO PROVIDE THE WORKER CLASS HERE OR IT WILL NOT WORK!!!!!!
            BaseWorker = new TrimWorker();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Trim Surface", "T", "This is the original trimmed surface you wish to compare the panels to.", GH_ParamAccess.item);
            pManager.AddBrepParameter("Panels", "P", "These are the panels you wish to trim. The panels should ideally be derived from the trimming surface.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Inclusion Offset", "O", "This distance offsets the trim surface so that it can test that the split panels are within the surface bounds", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Tolerance", "t", "The distance tolerance for comparing the panels to original surface", GH_ParamAccess.item, 0.01);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Trimmed Panels", "P", "The trimmed panels", GH_ParamAccess.list);
        }

        // Use the async solver - everything else is the same - But deleted solve instance.
        // ASYNC IS BELOW!!!!!!!!!

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Ankylosaurus.Properties.Resources.Util_TrimPanels;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D9707D01-CF94-4EE8-9698-EE1F163F5881"); }
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
        private class TrimWorker : WorkerInstance
        {
            // Needs a constructor
            public TrimWorker() : base(null) { }

            //NEED ACCESSIBLE GLOBAL VARIABLES / INPUTS AND OUTPUTS
            // GLOBAL INPUTS
            Brep iTrimSrf = null;
            double iInclusionDist = 0.0;
            double iTol = 0;
            List<Brep> iPanels = new List<Brep>();
            // GLOBAL OUTPUTS
            List<GH_Brep> trimmedPanels = new List<GH_Brep>();

            // Get Data defines the inputs and outputs
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                // INPUTS
                Brep _iTrimSrf = null;
                double _iInclusionDist = 0.0;
                double _iTol = 0.0;
                List<Brep> _iPanel = new List<Brep>();

                DA.GetData(0, ref _iTrimSrf);
                DA.GetDataList(1, _iPanel);
                DA.GetData(2, ref _iInclusionDist);
                DA.GetData(3, ref _iTol);

                // Set the input data to global variables
                iTrimSrf = _iTrimSrf;
                iInclusionDist = _iInclusionDist;
                iTol = _iTol;
                iPanels = _iPanel;
            }

            // The following is the replacement for Solve Instance - Do the Work Dummy
            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // 👉 Checking for cancellation!
                if (CancellationToken.IsCancellationRequested) { return; }

                // COMPUTING LOGIC BELOW HERE
                // CREATE A THICC SOLID TO CALCULATE INNER POINT INCLUSION
                // Initialize useless 'out' walls to run command
                Brep[] offsetBrepBlends = new Brep[0]; Brep[] brepWalls = new Brep[0];

                // Offset first as a surface, then as a solid for both sides
                Brep[] offsetBrep1 = Brep.CreateOffsetBrep(iTrimSrf, iInclusionDist, false, true, 
                    RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out offsetBrepBlends, out brepWalls);
                Brep[] cutterBoiz = Brep.CreateOffsetBrep(offsetBrep1[0], -(iInclusionDist * 2), true, true, 
                    RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out offsetBrepBlends, out brepWalls);
                Brep cutterBoi = cutterBoiz[0];

                // BEGIN SPLIT OPERATION - SPLIT PANELS BY ORIGINAL SURFACE EDGES
                var trimCrvs = iTrimSrf.DuplicateNakedEdgeCurves(true, false);
                //List<Brep> splitPanels = new List<Brep>();
                Double count = 0;

                foreach (Brep panel in iPanels)
                {
                    // 👉 Checking for cancellation!
                    if (CancellationToken.IsCancellationRequested) { return; }

                    // SPLITTING OPERATION ON INDIVIDUAL PANELS
                    // The curve splitting does not always succeed so perhaps we should revisit with BREP splitting...
                    Brep splitBrep = panel.Faces[0].Split(trimCrvs, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    //Brep[] splitBrep = panel.Split(cutterBoi[0], iTol);
                    for (int i = 0; i < splitBrep.Faces.Count; i++)
                    {
                        Brep newSrf = splitBrep.Faces[i].DuplicateFace(false);
                        AreaMassProperties computeAreas = AreaMassProperties.Compute(newSrf);
                        Point3d centerPt = computeAreas.Centroid;
                        Point3d newCenterPt = newSrf.ClosestPoint(centerPt);

                        bool isPtInside = cutterBoi.IsPointInside(newCenterPt, iTol, true);
                        if (isPtInside)
                        {
                            Brep insideSrf = splitBrep.Faces[i].DuplicateFace(false);
                            trimmedPanels.Add(new GH_Brep(insideSrf));
                        }
                    }
                    // This reports the progress of the 4 loop - should be in loop calculation
                    ReportProgress(Id, count / (double)iPanels.Count);
                    count++;
                }

                // IMPORTANT NEED TO CALL DONE TO LET IT KNOW
                Done();
            }



            public override void SetData(IGH_DataAccess DA)
            {
                // 👉 Checking for cancellation!
                if (CancellationToken.IsCancellationRequested) { return; }
                DA.SetDataList(0, trimmedPanels);
            }

            // NEED THIS - JUST CALL A NEW INSTANCE OF THIS CLASS
            public override WorkerInstance Duplicate() => new TrimWorker();
        }

    }
}
