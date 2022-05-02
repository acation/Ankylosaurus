using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;

namespace Ankylosaurus.Util
{
    public static class SrfDirectionUtility
    {
        public static Point3d getSrfCenterPoint(Surface srf)
        {
            Point3d centPt = new Point3d();
            Vector3d[] srfUV;
            srf.Evaluate(srf.Domain(0).Mid, srf.Domain(1).Mid, 1, out centPt, out srfUV);
            return centPt;
        }

        public static Vector3d getSrfU(Surface srf)
        {
            Point3d centPt = new Point3d();
            Vector3d[] srfUV;
            srf.Evaluate(srf.Domain(0).Mid, srf.Domain(1).Mid, 1, out centPt, out srfUV);
            Vector3d srfU = srfUV[0];
            return srfU;
        }

        public static Vector3d getSrfV(Surface srf)
        {
            Point3d centPt = new Point3d();
            Vector3d[] srfUV;
            srf.Evaluate(srf.Domain(0).Mid, srf.Domain(1).Mid, 1, out centPt, out srfUV);
            Vector3d srfV = srfUV[1];
            return srfV;
        }

        public static Vector3d getSrfNormAtCenter(Surface srf)
        {
            Vector3d srfN = srf.NormalAt(srf.Domain(0).Mid, srf.Domain(1).Mid);
            return srfN;
        }
    }
}
