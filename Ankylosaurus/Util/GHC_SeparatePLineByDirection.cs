using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Ankylosaurus.Util
{
	public class GHC_SeparatePLineByDirection : GH_Component
	{
		
		public GHC_SeparatePLineByDirection()
		  : base("Separate Segments By Direction", "SepPLine",
			  "Separate the segments of a polyline according to an input vector that delineates the curve direction.",
			  "Ankylosaurus", "Util")
		{
		}

		
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddCurveParameter("Polyline", "P", "Input polyline", GH_ParamAccess.item);
			pManager.AddVectorParameter("Vector", "V", "Direction vector to separate segments from", GH_ParamAccess.item, Vector3d.ZAxis);
			pManager.AddNumberParameter("Tolerance", "T", "The tolerance to test segments against vector.", GH_ParamAccess.item, 0.02);
		}

		
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddCurveParameter("Curve A", "A", "Segment curves along the input vector", GH_ParamAccess.list);
			pManager.AddCurveParameter("Curve B", "B", "Segment curves not along the input vector", GH_ParamAccess.list);
		}

		
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Curve iCrv = null;
			Vector3d iVec = new Vector3d();
			double iTol = 0;

			DA.GetData(0, ref iCrv);
			DA.GetData(1, ref iVec);
			DA.GetData(2, ref iTol);
			
			Curve[] segments = iCrv.DuplicateSegments();
			
			List<GH_Curve> crvsDirectionA = new List<GH_Curve>();
			List<GH_Curve> crvsDirectionB = new List<GH_Curve>();
			
			double topAngleMax = Math.PI + iTol; double topAngleMin = Math.PI - iTol;
			double lowAngleMax = 0 + iTol; double lowAngleMin = 0 - iTol;

			for (int i = 0; i < segments.Length; i++)
			{
				Point3d startPt = segments[i].PointAtStart;
				Point3d endPt = segments[i].PointAtEnd;
				Vector3d segVec = new Vector3d(endPt) - new Vector3d(startPt);

				double vecAngle = Vector3d.VectorAngle(segVec, iVec);

				if (topAngleMax <= vecAngle || vecAngle >= topAngleMin
				  || lowAngleMax >= vecAngle || vecAngle <= lowAngleMin)
				{
					crvsDirectionA.Add(new GH_Curve(segments[i]));
				}
				else
					crvsDirectionB.Add(new GH_Curve(segments[i]));
			}

			DA.SetDataList(0, crvsDirectionA);
			DA.SetDataList(1, crvsDirectionB);

		}

		
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Util_Seperate_Segments_by_Direction;
			}
		}

		
		public override Guid ComponentGuid
		{
			get { return new Guid("35c9bab4-9572-496b-ba55-533b67eaa5ab"); }
		}
	}
}