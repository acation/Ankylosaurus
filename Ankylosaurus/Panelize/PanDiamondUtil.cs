using System;
using System.Collections.Generic;

using Rhino.Geometry;

namespace Ankylosaurus.Panelize
{
    public static class PanDiamondUtil
    {

        // This utility function computes the point index knowing the row and column indices
        private static int GetPtIndex(int u, int v, int vDivision)
        {
            return u * (vDivision + 1) + v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iPts"></param>
        /// <param name="iU"></param>
        /// <param name="iV"></param>
        /// <returns></returns>
        public static Tuple<List<NurbsSurface>, List<NurbsSurface>> DiamondsFromPoints(List<Point3d> iPts, int iU, int iV)
        {
            // RETURNS A TUPLE WITH EMBEDDED LISTS FOR DIAMOND PANELS, AND FOR TRIANGLE PANELS

            // Create the surface panels. For each face, we need to obtain the indices of the four relevant vertices
            List<NurbsSurface> diamondPanels = new List<NurbsSurface>();
            List<NurbsSurface> trianglePanels = new List<NurbsSurface>();

            for (int u = 0; u < iU; u++)
            {
                for (int v = 0; v < iV; v++)
                {

                    //----------------------- THE FOLLOWING IS FOR THE QUAD DIAMOND PANELS ----------------------------

                    // Get every other row, excluding points that only create triangular panels
                    if (u % 2 != 0 && v % 2 == 0
                      // the rest gives middle panels
                      && v > 1 && iV > v && u < iU - 1)
                    {
                        // First point of interest - side middle
                        int v1 = GetPtIndex(u, v, iV);
                        // Bottom Corner Point of panel
                        int v2 = GetPtIndex(u + 1, v - 1, iV);
                        // Other side middle corner point
                        int v3 = GetPtIndex(u + 2, v, iV);
                        // Top corner point
                        int v4 = GetPtIndex(u + 1, v + 1, iV);

                        NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v4]);
                        diamondPanels.Add(panelSrf);
                    }
                    // Get every other row, excluding points that only create triangular panels
                    else if (u % 2 == 0 && v % 2 != 0
                      // the rest gives middle panels
                      && iV > v && u < iU - 1)
                    {
                        // First point of interest - side middle
                        int v1 = GetPtIndex(u, v, iV);
                        // Bottom Corner Point of panel
                        int v2 = GetPtIndex(u + 1, v - 1, iV);
                        // Other side middle corner point
                        int v3 = GetPtIndex(u + 2, v, iV);
                        // Top corner point
                        int v4 = GetPtIndex(u + 1, v + 1, iV);

                        NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v4]);
                        diamondPanels.Add(panelSrf);
                    }


                    //-----------------------  THE FOLLOWING IS FOR THE TRIANGLE PANELS ----------------------------

                    // "left-side" or u = 0 edge triangles
                    if (u == 0 && v % 2 != 0 && v < iV - 1)
                    {
                        // First point of interest - bottom middle
                        int v1 = GetPtIndex(u, v, iV);
                        // side Corner Point of panel
                        int v2 = GetPtIndex(u + 1, v + 1, iV);
                        // Top corner point
                        int v3 = GetPtIndex(u, v + 2, iV);

                        NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                        trianglePanels.Add(panelSrf);
                    }

                    // "bottom" or v = 0 edge triangles
                    if (v == 0 && u % 2 != 0 && u < iU - 1)
                    {
                        // First point of interest - bottom left (or right) side
                        int v1 = GetPtIndex(u, v, iV);
                        // other side Corner Point of panel
                        int v2 = GetPtIndex(u + 2, v, iV);
                        // Top corner point
                        int v3 = GetPtIndex(u + 1, v + 1, iV);

                        NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                        trianglePanels.Add(panelSrf);
                    }



                    // ------- IF V DIVISION IS EVEN ------- 

                    if (iV % 2 == 0)
                    {
                        // "TOP ROW" or v = v division length edge triangles
                        if (v + 1 == iV && u % 2 != 0 && u < iU - 1)
                        {
                            // First point of interest - mid left (or right) side
                            int v1 = GetPtIndex(u, v + 1, iV);
                            // other side Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v, iV);
                            // bottom corner point
                            int v3 = GetPtIndex(u + 2, v + 1, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }

                        // TOP LEFT CORNER
                        if (u == 0 && v + 1 == iV)
                        {
                            // First point of interest - bottom point
                            int v1 = GetPtIndex(u, v, iV);
                            // top Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v + 1, iV);
                            // "left" (or right) middle corner point
                            int v3 = GetPtIndex(u, v + 1, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }

                        // TOP RIGHT CORNER
                        if (u == iU - 1 && v == iV - 1 && iU % 2 == 0)
                        {
                            // First point of interest - bottom point
                            int v1 = GetPtIndex(u, v + 1, iV);
                            // top Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v, iV);
                            // "left" (or right) middle corner point
                            int v3 = GetPtIndex(u + 1, v + 1, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }
                    }

                    // ------- IF V DIVISION IS ODD ------- 

                    if (iV % 2 != 0)
                    {
                        // "TOP ROW" or v = v division length edge triangles
                        if (v + 1 == iV && u % 2 == 0 && u < iU - 1)
                        {
                            // First point of interest - mid left (or right) side
                            int v1 = GetPtIndex(u, v + 1, iV);
                            // other side Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v, iV);
                            // bottom corner point
                            int v3 = GetPtIndex(u + 2, v + 1, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }


                        // TOP RIGHT CORNER
                        if (u == iU - 1 && v == iV - 1 && iU % 2 != 0)
                        {
                            // First point of interest - bottom point
                            int v1 = GetPtIndex(u, v + 1, iV);
                            // top Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v, iV);
                            // "left" (or right) middle corner point
                            int v3 = GetPtIndex(u + 1, v + 1, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }

                    }

                    // ------- IF U DIVISION IS EVEN ------- 

                    if (iU % 2 == 0)
                    {

                        // "RIGHT SIDE" or u = u division length edge triangles
                        if (u == iU - 1 && v % 2 != 0 && v < iV - 1)
                        {
                            // First point of interest - bottom point
                            int v1 = GetPtIndex(u, v + 1, iV);
                            // top Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v, iV);
                            // "left" (or right) middle corner point
                            int v3 = GetPtIndex(u + 1, v + 2, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }

                        // BOTTOM RIGHT CORNER
                        if (u == iU - 1 && v == 0)
                        {
                            // First point of interest - bottom point
                            int v1 = GetPtIndex(u, v, iV);
                            // top Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v, iV);
                            // "left" (or right) middle corner point
                            int v3 = GetPtIndex(u + 1, v + 1, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }

                    }

                    // ------- IF U DIVISION IS ODD ------- 

                    if (iU % 2 != 0)
                    {

                        // "RIGHT SIDE" or u = u division length edge triangles
                        if (u == iU - 1 && v % 2 == 0 && v < iV - 1)
                        {
                            // First point of interest - bottom point
                            int v1 = GetPtIndex(u, v + 1, iV);
                            // top Corner Point of panel
                            int v2 = GetPtIndex(u + 1, v, iV);
                            // "left" (or right) middle corner point
                            int v3 = GetPtIndex(u + 1, v + 2, iV);

                            NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                            trianglePanels.Add(panelSrf);
                        }

                    }

                    // BOTTOM LEFT CORNER
                    if (u == 0 && v == 0)
                    {
                        // First point of interest - bottom point
                        int v1 = GetPtIndex(u, v, iV);
                        // top Corner Point of panel
                        int v2 = GetPtIndex(u + 1, v, iV);
                        // "left" (or right) middle corner point
                        int v3 = GetPtIndex(u, v + 1, iV);

                        NurbsSurface panelSrf = NurbsSurface.CreateFromCorners(iPts[v1], iPts[v2], iPts[v3], iPts[v1]);
                        trianglePanels.Add(panelSrf);
                    }


                }
            }

            return Tuple.Create(diamondPanels, trianglePanels);

        }

    }
}
