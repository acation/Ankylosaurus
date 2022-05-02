using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace Ankylosaurus.Util
{
	public class GHC_SurfaceSwapDirection : GH_Component
	{
		
		public GHC_SurfaceSwapDirection()
		  : base("Swap Surface Direction", "SwapUV",
			  "Options for swaping the U and V directions of a surface",
			  "Ankylosaurus", "Util")
		{
		}

		
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Input surface", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Flip Surface", "F", "Flip the surface", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Swap Surface Direction", "Sw", "Swap the UV direction of the surface", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Reverse U", "U", "Reverse the U direction", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Reverse V", "V", "Reverse the V direction", GH_ParamAccess.item, false);
		}

		
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddSurfaceParameter("Surface", "S", "Output transposed surface", GH_ParamAccess.item);
		}

		
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Brep iBrep = null;
			bool iFlip = false;
			bool iSwap = false;
			bool iReverseU = false;
			bool iReverseV = false;

			DA.GetData<Brep>(0, ref iBrep);
			DA.GetData(1, ref iFlip);
			DA.GetData(2, ref iSwap);
			DA.GetData(3, ref iReverseU);
			DA.GetData(4, ref iReverseV);
            
			if (iSwap)
			{
				iBrep.Faces[0].Transpose(true);
			}
			if (iReverseU)
			{
				// This Modifies the direction in the u
				// setting to true modifies the original surface directly
				iBrep.Faces[0].Reverse(0, true);
			}
			if (iReverseV)
			{
				iBrep.Faces[0].Reverse(1, true);
			}

			if (iFlip)
			{
                iBrep.Flip();
			}

			GH_Brep outSrf = new GH_Brep(iBrep);

			// GH_Surface outSrf = new GH_Surface(iSrf.Transpose());
			DA.SetData(0, outSrf);

			List<double> nums = new List<double>();
			List<double> dupNums = nums.Distinct().ToList();
		}


		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return Ankylosaurus.Properties.Resources.Util_Set_Surface_Direction;
			}
		}

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("daf0047c-145f-4a2d-9c77-8b00fd946ac4"); }
		}
	}
}