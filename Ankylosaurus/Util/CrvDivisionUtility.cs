using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ankylosaurus.Util
{
    public static class CrvDivisionUtility
    {
        // Divide Curve Equidistant
        public static Point3d[] DivideCurveByTargetDistance(Curve iCrv, double iTargetDist, int iSteps, double iTol)
        {
            double dist = iTargetDist;

            Point3d[] startPts = iCrv.DivideEquidistant(dist);
            
            Point3d lastPt = startPts[startPts.Length - 1];
            Point3d crvEnd = iCrv.PointAtEnd;
            double leftover = crvEnd.DistanceTo(lastPt);

            double cutoff = iTol;

            double inc = leftover / (startPts.Length + 2);

            Point3d[] pts = null;
            //List<GH_Point> ghPts = new List<GH_Point>();

            for (int i = 0; i < iSteps; i++)
            {
                dist += inc;
                pts = iCrv.DivideEquidistant(dist);
                double endDist = crvEnd.DistanceTo(pts[pts.Length - 1]);


                if (endDist < cutoff)
                {
                    break;
                }

                inc = endDist / (pts.Length + 2);
            }

            return pts;
        }




    }
}
