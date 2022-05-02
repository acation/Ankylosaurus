using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;

namespace Ankylosaurus.Panelize
{
	public static class PanelUtility
	{

        // This utility function computes the point index knowing the row and column indices
        public static int GetPtIndex(int u, int v, int vDivision)
        {
            return u * (vDivision + 1) + v;
        }

		//  if you do a tripple slash you can create a summary that tells you information about the methods
		// it must be right above a method

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sList"></param>
		/// <param name="U"></param>
		/// <param name="V"></param>
		/// <returns></returns>
		public static DataTree<GH_Surface> DivideSurfaceList(List<Surface> sList, int U, int V)
		{

			DataTree<GH_Surface> subSurf = new DataTree<GH_Surface>();

			for (int surfIndex = 0; surfIndex < sList.Count; surfIndex++)
			{
				Surface surf = sList.ElementAt(surfIndex);

				Interval surfU = surf.Domain(0);
				Interval surfV = surf.Domain(1);
				
				double uStep = surfU.Max / U;
				double vStep = surfV.Max / V;

				for (int u = 0; u < U; u++)
				{
					double umin = u * uStep;
					double umax = (u + 1) * uStep;
					Interval nU = new Interval(umin, umax);
					for (int v = 0; v < V; v++)
					{
						double vmin = v * vStep;
						double vmax = (v + 1) * vStep;
						Interval nV = new Interval(vmin, vmax);

						GH_Surface tempSubSrf = new GH_Surface(surf.Trim(nU, nV)); 
						subSurf.Add(tempSubSrf, new GH_Path(surfIndex));
					}
				}
			}
			return subSurf;
		}


		public static List<Interval> ConsecutiveDomains(List<Double> numbers)
		{

			List<Interval> consecutiveDomains = new List<Interval>();

			for (int i = 0; i < numbers.Count - 1; i++)
			{
				Interval tempDom = new Interval(numbers[i], numbers[i + 1]);
				consecutiveDomains.Add(tempDom);
			}
			return consecutiveDomains;
		}


		public static Surface ReparameterizeSurface(Surface srf)
		{
			srf.SetDomain(0, new Interval(0.0, 1.0));
			srf.SetDomain(1, new Interval(0.0, 1.0));
			
			return srf;
		}


		public static List<Point3d> GetSrfCrvIntersectionPts(Surface srf, List<Curve> crvs)
		{
			Point3d[] pointsToIntersect = new Point3d[1];
			List<Point3d> intersectPoints = new List<Point3d>();
			Curve[] overlapCrv = new Curve[1];

			for (int i = 0; i < crvs.Count; i++)
			{
				Rhino.Geometry.Intersect.Intersection.CurveBrep(crvs[i], srf.ToBrep(), 0.1, out overlapCrv, out pointsToIntersect);

				if (pointsToIntersect.Length == 0 && overlapCrv.Length > 0)
					intersectPoints.Add(crvs[i].PointAt(0.0));

				else
				{
					for (int j = 0; j < pointsToIntersect.Length; j++)
					{
						intersectPoints.Add(pointsToIntersect[j]);
					}
				}
			}
			return intersectPoints;
		}


		public static List<double> GetSrfPointParameterAlongEdge(Surface srf, List<Point3d> intersectPoints)
		{
			List<double> srfParameters = new List<double>();
			srfParameters.Add(0.0);

			for (int i = 0; i < intersectPoints.Count; i++)
			{
				double u;
				double v;
				srf.ClosestPoint(intersectPoints[i], out u, out v);
				if (u == 0.0)
					srfParameters.Add(v);
				else if (v == 0.0)
					srfParameters.Add(u);
			}

			srfParameters.Add(1.0);
			srfParameters.Sort();
			List<double> noDupSrfParams = new HashSet<double>(new List<double>(RoundNumbersList(srfParameters))).ToList();
			return noDupSrfParams;
		}


		public static List<double> GetSrfPointParameter(Surface srf, List<Point3d> intersectPoints, string srfDirection)
		{
			/// <summary>
			/// derp derp derp
			/// </summary>
			List<double> srfParametersU = new List<double>();
			List<double> srfParametersV = new List<double>();
			srfParametersU.Add(0.0);
			srfParametersV.Add(0.0);

			double u;
			double v;

			for (int i = 0; i < intersectPoints.Count; i++)
			{
				srf.ClosestPoint(intersectPoints[i], out u, out v);
                // The following if statement makes sure the points are touching the Surface
                // Added Monday 03/26/2018
                double dist = srf.PointAt(u, v).DistanceTo(intersectPoints[i]);
                if (dist < 0.1)
                {
                    srfParametersU.Add(u);
                    srfParametersV.Add(v);
                }
			}

			if (srfDirection == "u" || srfDirection == "U")
			{
				srfParametersU.Add(1.0);
				srfParametersU.Sort();
				// The numbers must be rounded or there can be duplicates to to floating point error in values in Grasshopper
				List<double> noDupSrfParamsU = new HashSet<double>(new List<double>(RoundNumbersList(srfParametersU))).ToList();
				if (noDupSrfParamsU.Count <= 2)
				{
					srfParametersV.Add(1.0);
					srfParametersV.Sort();
					List<double> noDupSrfParamsV = new HashSet<double>(new List<double>(RoundNumbersList(srfParametersV))).ToList();
					return noDupSrfParamsV;
					//AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "You seem to have a mismatch between your desired input geometry and the surface UV direction");
				}
				else
					return noDupSrfParamsU;
			}

			else if (srfDirection == "v" || srfDirection == "V")
			{
				srfParametersV.Add(1.0);
				srfParametersV.Sort();
				List<double> noDupSrfParamsV = new HashSet<double>(new List<double>(RoundNumbersList(srfParametersV))).ToList();
				if (noDupSrfParamsV.Count <= 2)
				{
					srfParametersU.Add(1.0);
					srfParametersU.Sort();
					List<double> noDupSrfParamsU = new HashSet<double>(new List<double>(RoundNumbersList(srfParametersU))).ToList();

					return noDupSrfParamsU;
				}
				else
					return noDupSrfParamsV;
			}

			else
				return null;

		}


		public static List<double> RoundNumbersList(List<double> numbers)
		{
			List<double> newNums = new List<double>();
			for (int i = 0; i < numbers.Count; i++)
			{
				newNums.Add(Math.Round(numbers[i], 10));
			}
			return newNums;
		}

        public static List<double> AverageNumbersConsecutive(List<double> nums)
        {
            List<double> numbers = new List<double>();

            numbers.Add(nums[0]);

            for (int i = 0; i < nums.Count - 1; i++)
            {
                double average = (nums[i] + nums[i + 1]) * 0.5;
                numbers.Add(average);
            }

            numbers.Add(nums[nums.Count - 1]);
            return numbers;
        }
    }
}
