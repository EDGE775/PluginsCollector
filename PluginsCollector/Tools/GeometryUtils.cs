﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PluginsCollector.Tools
{
    public static class GeometryUtils
    {
        public static XYZ[] GetMaxMinHeightPoints(List<Solid> solids)
        {
            XYZ maxZpoint = new XYZ(0, 0, -9999999);
            XYZ minZpount = new XYZ(0, 0, 9999999);

            List<Edge> edges = new List<Edge>();
            foreach (Solid s in solids)
            {
                foreach (Edge e in s.Edges)
                {
                    edges.Add(e);
                }
            }

            foreach (Edge e in edges)
            {
                Curve c = e.AsCurve();
                XYZ p1 = c.GetEndPoint(0);
                if (p1.Z > maxZpoint.Z) maxZpoint = p1;
                if (p1.Z < minZpount.Z) minZpount = p1;

                XYZ p2 = c.GetEndPoint(1);
                if (p2.Z > maxZpoint.Z) maxZpoint = p2;
                if (p2.Z < minZpount.Z) minZpount = p2;
            }
            XYZ[] result = new XYZ[] { maxZpoint, minZpount };
            return result;
        }


        public static List<Solid> GetSolidsFromElement(Element elem)
        {
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geoElem = elem.get_Geometry(opt);

            List<Solid> solids = GetSolidsFromElement(geoElem);
            return solids;
        }

        public static List<Solid> GetSolidsFromElement(GeometryElement geoElem)
        {
            List<Solid> solids = new List<Solid>();

            foreach (GeometryObject geoObj in geoElem)
            {
                if (geoObj is Solid)
                {
                    Solid solid = geoObj as Solid;
                    if (solid == null) continue;
                    if (solid.Volume == 0) continue;
                    solids.Add(solid);
                    continue;
                }
                if (geoObj is GeometryInstance)
                {
                    GeometryInstance geomIns = geoObj as GeometryInstance;
                    if (geomIns == null) continue;
                    GeometryElement instGeoElement = geomIns.GetInstanceGeometry();
                    List<Solid> solids2 = GetSolidsFromElement(instGeoElement);
                    solids.AddRange(solids2);
                }
            }
            return solids;
        }

    }
}
