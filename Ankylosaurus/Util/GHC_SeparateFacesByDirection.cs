using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;


namespace Ankylosaurus.Util
{
	public class GHC_SeparateFacesByDirection : GH_Component
	{
		
		public GHC_SeparateFacesByDirection()
		  : base("Separate Faces By Direction", "BrepFaces",
			  "Separates Brep faces based on an input vector",
			  "Ankylosaurus", "Util")
		{
		}

		
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddBrepParameter("Brep", "B", "The Brep to separate faces from", GH_ParamAccess.item);
			pManager.AddVectorParameter("Direction Vector", "V", "The vector to separate the faces against. The default is the Z", GH_ParamAccess.item, Vector3d.ZAxis);
			pManager.AddNumberParameter("Tolerance", "T", "The tolerance to test against the vector for determining direction", GH_ParamAccess.item, 0.1);
		}

		
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddBrepParameter("Top Faces", "T", "The faces at the top, or facing the direction of the direction vector.", GH_ParamAccess.list);
			pManager.AddBrepParameter("Bottom Faces", "B", "The faces at the bottom, or facing away from the direction vector.", GH_ParamAccess.list);
			pManager.AddBrepParameter("Side Faces", "S", "The faces on the sides, or not facing the direction vector.", GH_ParamAccess.list);
		}

		
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Brep iBrep = new Brep();
			Vector3d iVec = new Vector3d(0, 0, 0);
			double iTol = 0.0;
            
			DA.GetData(0, ref iBrep);
			DA.GetData(1, ref iVec);
			DA.GetData(2, ref iTol);

			List<GH_Brep> topFaces = new List<GH_Brep>();
			List<GH_Brep> bottomFaces = new List<GH_Brep>();
			List<GH_Brep> sideFaces = new List<GH_Brep>();

			double botSrfAngleMax = Math.PI + iTol; double botSrfAngleMin = Math.PI - iTol;
			double topSrfAngleMax = 0 + iTol; double topSrfAngleMin = 0 - iTol;

			for (int i = 0; i < iBrep.Faces.Count; i++)
			{
				// Get a face as a trimed brep and then turn to a surface to get normals:
				Brep face = iBrep.Faces.ExtractFace(i);
				// Faces.Flip(true) gives the correct surface direction
				face.Faces.Flip(true);
				BrepFace srfFace = face.Faces.Add(0); 

				// Get the normals at the face centers
				Vector3d srfNormal = srfFace.NormalAt(srfFace.Domain(0).Max / 2, srfFace.Domain(1).Max / 2);

				// Get Vector Angle
				double vecAngle = Vector3d.VectorAngle(srfNormal, iVec);

				// Compare the normal vectors to input vector
				// Okay this is dumb
				// But you must re extract the face for it to not flip the normal
				if (topSrfAngleMax >= vecAngle || vecAngle <= topSrfAngleMin)
					topFaces.Add(new GH_Brep(face.Faces.ExtractFace(0)));
				else if (botSrfAngleMax <= vecAngle || vecAngle >= botSrfAngleMin)
					bottomFaces.Add(new GH_Brep(face.Faces.ExtractFace(0)));
				else
					sideFaces.Add(new GH_Brep(face.Faces.ExtractFace(0)));

			}

			DA.SetDataList(0, topFaces);
			DA.SetDataList(1, bottomFaces);
			DA.SetDataList(2, sideFaces);

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
				return Ankylosaurus.Properties.Resources.Util_Seperate_Faces_by_Direction;
			}
		}

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("027f8b74-64c1-438d-beeb-66922f27c032"); }
		}
	}
}