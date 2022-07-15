using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;

namespace Ankylosaurus.Util
{
    public class GHC_TrimPanels : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHC_TrimPanels class.
        /// </summary>
        public GHC_TrimPanels()
          : base("Trim Panels", "PTrim",
              "This component is useful if you are panelizing a trimmed surface, and would like for the panels to be trimmed as well. " +
                "Keep in mind that the tolerance is comparing the distance of the panel center to the original surface and may need to be adjusted",
              "Ankylosaurus", "Util")
        {
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

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //GH_Brep ghTrimSrf = null;
            Brep iTrimSrf = null;
            //GH_Brep ghPanel = null;
            double iTol = 0;
            Brep iPanel = null;


            DA.GetData(0, ref iTrimSrf);
            DA.GetData(1, ref iPanel);
            DA.GetData(2, ref iTol);
            //GH_Convert.ToBrep(DA.GetData(0, ref ghTrimSrf), ref iTrimSrf, GH_Conversion.Primary);
            //GH_Convert.ToBrep(DA.GetData(1, ref ghPanel), ref iPanel, GH_Conversion.Primary);
            //GH_Brep iPanel = new GH_Brep(iPanels);
            //if (iPanel.IsValid == true) {
            //AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "This is valid");
            //}

            // BEGIN SPLIT OPERATION - SPLIT PANELS BY ORIGINAL SURFACE EDGES
            var trimCrvs = iTrimSrf.DuplicateNakedEdgeCurves(true, false);
            Brep splitBrep = iPanel.Faces[0].Split(trimCrvs, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            // Create an empty list to store our new trimmed panels
            List<GH_Brep> trimmedPanels = new List<GH_Brep>();
            List<double> centroidDists = new List<double>();


            for (int i = 0; i < splitBrep.Faces.Count; i++)
            {
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
            }

            DA.SetDataList(0, trimmedPanels);

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
            get { return new Guid("63DCE3E2-6E78-49C7-A306-E5126757AB13"); }
        }
    }
}