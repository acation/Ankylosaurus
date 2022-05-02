using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;

using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using static Ankylosaurus.Util.SrfDirectionUtility;

namespace Ankylosaurus.Util
{
	public class GHC_UnifySrfByDirection : GH_Component
	{
		
		public GHC_UnifySrfByDirection()
		  : base("Unify Surface by Direction", "SrfUnifyDir",
			  "Unifies the UV directions of a list of surfaces based on a desired vector.",
			  "Ankylosaurus", "Util")
		{
		}

		
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Input surfaces to unify direction", GH_ParamAccess.item);
			pManager.AddVectorParameter("Vector", "V", "The desired vector to unify surface directions towards. The default is the Z-axis", GH_ParamAccess.item, Vector3d.ZAxis);
			pManager.AddAngleParameter("Tolerance Angle", "T", "The tolerance angle to test the surface direction against", GH_ParamAccess.item, 0.1);
		}

		
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Output surfaces with unified direction", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Surface iSrf = null;
			Vector3d iVector = Vector3d.ZAxis;
			double iTolerance = 0;

			DA.GetData(0, ref iSrf);
			DA.GetData(1, ref iVector);
			DA.GetData(2, ref iTolerance);

			Point3d cPt = getSrfCenterPoint(iSrf);
			Vector3d srfU = getSrfU(iSrf);
			Vector3d srfV = getSrfV(iSrf);
			Vector3d srfN = getSrfNormAtCenter(iSrf);


			//Set Angle variables - 90 & 180

			double angle90max = (0.5 * Math.PI) + iTolerance;
			double angle90min = (0.5 * Math.PI) - iTolerance;
			double angle180max = Math.PI + iTolerance;
			double angle180min = Math.PI - iTolerance;

			//initialize start angle comparing the V vector to the input Vector direction

			double startAngle = Vector3d.VectorAngle(srfV, iVector);

			//Set comparison boolean testing if the V vector is 90 degrees of the direction vector

			if (startAngle < angle90max & startAngle > angle90min)
			{
				iSrf.Transpose(true);
			}

			//Set comparison boolean testing if the V vector is 180 degrees of the direction vector

			Vector3d newV = getSrfV(iSrf);
			double newAngle = Vector3d.VectorAngle(newV, iVector);
			if (newAngle <= angle180max & newAngle >= angle180min)
			{
				iSrf.Reverse(1, true);
			}

			Vector3d finalV = getSrfV(iSrf);
			Vector3d finalU = getSrfU(iSrf);

			DA.SetData(0, iSrf);

			// AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "This sh*t doesn't even work right now, try it or don't bother, whatever");

			/*List<GH_Surface> newSrf = new List<GH_Surface>();
			
			for (int i = 0; i < iSrf.Count; i++)
			{
				Plane pln = new Plane();
				
				Interval uDom = iSrf[i].Domain(0);
				Interval vDom = iSrf[i].Domain(1);
				
				iSrf[i].FrameAt(uDom.Max / 2, vDom.Max / 2, out pln);

				Vector3d srfVec = pln.YAxis;
				Double angle = Vector3d.VectorAngle(iVector, srfVec);
				if (angle > iTolerance)
				{
					newSrf.Add(new GH_Surface(iSrf[i].Transpose()));
				}
				else
					newSrf.Add(new GH_Surface(iSrf[i]));
			}

			DA.SetDataList(0, newSrf);*/

		}

		
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Util_UnifySrf_Dir_by_Vector;
			}
		}

		
		public override Guid ComponentGuid
		{
			get { return new Guid("0e255edd-7922-4ef6-8dc6-012f397b9bc6"); }
		}
	}
}