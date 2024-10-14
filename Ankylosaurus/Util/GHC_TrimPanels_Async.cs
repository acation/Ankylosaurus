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
                "Keep in mind that the tolerance is comparing the distance of the panel center to the original surface and may need to be adjusted. " +
                "The trimming calculation itself uses the document tolerance.",
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
            pManager.AddBrepParameter("Panels", "P", "These are the panels you wish to trim. The panels should ideally be derived from the trimming surface.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "t", "The distance tolerance for comparing the panels", GH_ParamAccess.item, 0.1);
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
                return null;
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
            double iTol = 0;
            Brep iPanel = null;
            // GLOBAL OUTPUTS
            List<GH_Brep> trimmedPanels = new List<GH_Brep>();

            // Get Data defines the inputs and outputs
            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                // INPUTS
                Brep _iTrimSrf = null;
                double _iTol = 0;
                Brep _iPanel = null;

                DA.GetData(0, ref _iTrimSrf);
                DA.GetData(1, ref _iPanel);
                DA.GetData(2, ref _iTol);

                // Set the input data to global variables
                iTrimSrf = _iTrimSrf; 
                iTol = _iTol; 
                iPanel = _iPanel;
            }

            // The following is the replacement for Solve Instance - Do the Work Dummy
            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // 👉 Checking for cancellation!
                if (CancellationToken.IsCancellationRequested) { return; }

                // BEGIN SPLIT OPERATION - SPLIT PANELS BY ORIGINAL SURFACE EDGES
                var trimCrvs = iTrimSrf.DuplicateNakedEdgeCurves(true, false);
                Brep splitBrep = iPanel.Faces[0].Split(trimCrvs, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                // Create an empty list to store our new trimmed panels
                // List<GH_Brep> trimmedPanels = new List<GH_Brep>();
                List<double> centroidDists = new List<double>();

                for (int i = 0; i < splitBrep.Faces.Count; i++)
                {
                    // 👉 Checking for cancellation!
                    if (CancellationToken.IsCancellationRequested) { return; }

                    // We first need to compare if the panel center is touching the trim surface
                    //var newSrf = splitBrep.Faces[i].ToBrep();
                    var newSrf = splitBrep.Faces[i].DuplicateFace(false);
                    //Get split Surface Centroid using AreaMassProperties
                    AreaMassProperties newSrfAreaProperties = AreaMassProperties.Compute(splitBrep.Faces[i]);
                    //For the centroid, we need to actually pull the Area to the surface
                    //using closest point, otherwise the point is not actually touching the surface geometry
                    Point3d newSrfCentroid = newSrfAreaProperties.Centroid;
                    Point3d newSrfCenter = newSrf.ClosestPoint(newSrfCentroid);
                    // Pull the center point to the trimming surface
                    Point3d comparePt = iTrimSrf.ClosestPoint(newSrfCenter);

                    //Get the comparison distance for the center points
                    double compareCenterDistance = newSrfCenter.DistanceTo(comparePt);
                    centroidDists.Add(compareCenterDistance);

                    // We also need to get the vertices of the split surfaces to make sure they all touch the original trimmed surface
                    List<double> verticeDistance = new List<double>();
                    Point3d[] vertices = newSrf.DuplicateVertices();

                    // 👉 Checking for cancellation!
                    if (CancellationToken.IsCancellationRequested) { return; }

                    foreach (var vert in vertices)
                    {
                        Point3d vertCompare = iTrimSrf.ClosestPoint(vert);
                        double dist = vertCompare.DistanceTo(vert);
                        verticeDistance.Add(dist);
                    }
                    //The average (or compare) vertex distance is how we compare the vertices to the trim surface
                    double compareVertexDist = System.Linq.Enumerable.Average(verticeDistance);
                    //averageDistances.Add(compareVertexDist);

                    //NOW WE SEPARATE THE PANELS
                    if (compareCenterDistance < iTol && compareVertexDist < iTol)
                    {
                        GH_Brep ghNewSrf = new GH_Brep(newSrf);
                        trimmedPanels.Add(ghNewSrf);
                    }

                    // This reports the progress of the 4 loop
                    ReportProgress(Id, i / (double)splitBrep.Faces.Count);
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