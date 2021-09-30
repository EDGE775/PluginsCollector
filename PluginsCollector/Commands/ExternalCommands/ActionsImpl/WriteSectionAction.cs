using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB; //для работы с элементами модели Revit
using Autodesk.Revit.UI; //для работы с элементами интерфейса
using PluginsCollector.Tools;
using static KPLN_Loader.Output.Output;

namespace PluginsCollector.Commands.ExternalCommands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class WriteSectionAction : ParamAction
    {
        private string gridSectionParam;
        private string sectionParam;
        private List<Element> elems = new List<Element>();
        private Document doc;
        private bool deleteSolids;

        public WriteSectionAction(Document doc, List<Element> elems, string gridSectionParam, string sectionParam, bool deleteSolids)
        {
            this.doc = doc;
            this.elems = elems;
            this.gridSectionParam = gridSectionParam;
            this.sectionParam = sectionParam;
            this.deleteSolids = deleteSolids;
        }

        public string name()
        {
            return "Заполнение секции";
        }

        public bool execute()
        {
            Print(string.Format("Изначальное количество элементов: {0}", elems.Count), KPLN_Loader.Preferences.MessageType.Warning);
            double[] minMaxCoords = getMinMaxZCoordOfModel(elems);
            List<Grid> grids = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfClass(typeof(Grid)).Cast<Grid>().ToList();
            Dictionary<string, HashSet<Grid>> sectionsGrids = new Dictionary<string, HashSet<Grid>>();
            foreach (Grid grid in grids)
            {
                Parameter param = grid.LookupParameter(gridSectionParam);
                if (param == null) continue;
                string valueOfParam = param.AsString();
                if (valueOfParam == null || valueOfParam.Length == 0) continue;
                foreach (string item in valueOfParam.Split('-'))
                {
                    if (sectionsGrids.ContainsKey(item))
                    {
                        sectionsGrids[item].Add(grid);
                        continue;
                    }
                    sectionsGrids.Add(item, new HashSet<Grid>() { grid });
                }
            }
            if (sectionsGrids.Keys.Count == 0)
            {
                Print(string.Format("Для заполнения номера секции в элементах, необходимо заполнить параметр: {0} в осях! Значение указывается через \"-\" для осей, относящихся к нескольким секциям.", gridSectionParam), KPLN_Loader.Preferences.MessageType.Warning);
                return false;
            }

            List<Element> notIntersectedElems = elems;
            Dictionary<Solid, string> solids = new Dictionary<Solid, string>();
            Dictionary<Element, List<string>> duplicates = new Dictionary<Element, List<string>>(new ElementComparer());
            List<DirectShape> directShapes = new List<DirectShape>();
            foreach (string sg in sectionsGrids.Keys)
            {
                List<Grid> gridsOfSect = sectionsGrids[sg].ToList();
                if (sectionsGrids[sg].Count < 4)
                {
                    Print(string.Format("Количество осей с номером секции: {0} меньше 4. Проверьте оси!", sg), KPLN_Loader.Preferences.MessageType.Warning);
                    return false;
                }
                List<XYZ> pointsOfGridsIntersect = getPointsOfGridsIntersection(gridsOfSect);
                pointsOfGridsIntersect.Sort(new ClockwiseComparer(getCenterPointOfPoints(pointsOfGridsIntersect)));
                Solid solid;
                DirectShape directShape = createSolidsInModel(minMaxCoords, pointsOfGridsIntersect, out solid);
                solids.Add(solid, sg);
                directShapes.Add(directShape);
                List<Element> intersectedElements = new FilteredElementCollector(doc, elems.Select(x => x.Id).ToList())
                    .WhereElementIsNotElementType()
                    .WherePasses(new ElementIntersectsSolidFilter(solid))
                    .ToElements()
                    .ToList();
                notIntersectedElems = notIntersectedElems.Except(intersectedElements, new ElementComparer()).ToList();
                foreach (Element item in intersectedElements)
                {
                    if (item == null) continue;
                    Parameter parameter = item.LookupParameter(sectionParam);
                    if (parameter != null && !parameter.IsReadOnly)
                    {
                        parameter.Set(sg);
                        if (duplicates.ContainsKey(item))
                        {
                            duplicates[item].Add(sg);
                        }
                        else
                        {
                            duplicates.Add(item, new List<string>() { sg });
                        }
                    }
                }
            }
            Print(string.Format("Количество необработанных элементов после 1-ого этапа: {0}", notIntersectedElems.Count), KPLN_Loader.Preferences.MessageType.Warning);
            int counter = 0;
            foreach (Element elem in notIntersectedElems)
            {
                if (elem == null) continue;
                XYZ elemPointCenter = null;
                try
                {
                    List<Solid> solidsFromElem = GeometryUtils.GetSolidsFromElement(elem);
                    int solidsCount = solidsFromElem.Count;
                    elemPointCenter = solidsCount == 1 ? solidsFromElem.First().ComputeCentroid() : solidsFromElem[solidsCount / 2].ComputeCentroid();
                }
                catch (Exception)
                {
                    try
                    {
                        elemPointCenter = solidBoundingBox(elem.get_BoundingBox(null)).ComputeCentroid();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (elemPointCenter == null) continue;
                List<Solid> solidsList = solids.Keys.ToList();

                Solid solid = null;

                //Расстояние от центра элемента до центроида солида
                solidsList.Sort((x, y) => (int)(x.ComputeCentroid().DistanceTo(elemPointCenter) - y.ComputeCentroid().DistanceTo(elemPointCenter)));
                solid = solidsList.First();

                //try
                //{
                //    //Расстояние от центра элемента до ближайшей плоскости солида
                //    Dictionary<Solid, FaceArray> solidsFaces = new Dictionary<Solid, FaceArray>(new SolidComparer());
                //    Dictionary<Solid, List<double>> solidDistances = new Dictionary<Solid, List<double>>(new SolidComparer());
                //    foreach (Solid s in solids.Keys)
                //    {
                //        FaceArray faces = s.Faces;
                //        solidsFaces.Add(s, faces);
                //        foreach (Face face in faces)
                //        {
                //            PlanarFace pf = face as PlanarFace;
                //            if (pf == null) continue;
                //            Plane plane1 = Plane.CreateByNormalAndOrigin(pf.FaceNormal, pf.Origin);
                //            UV uv1;
                //            double d1;
                //            plane1.Project(elemPointCenter, out uv1, out d1);
                //            //XYZ projectPt1 = plane1.Origin + uv1.U * plane1.XVec + uv1.V * plane1.YVec;
                //            if (!solidDistances.ContainsKey(s))
                //            {
                //                solidDistances.Add(s, new List<double>());
                //            }
                //            solidDistances[s].Add(d1);
                //        }
                //        solidDistances[s].Sort();
                //    }
                //    List<KeyValuePair<Solid, List<double>>> list = solidDistances.ToList();
                //    list.Sort((x, y) => (int)(x.Value.First() - y.Value.First()));
                //    solid = list.First().Key;
                //}
                //catch (Exception)
                //{
                //    //Расстояние от центра элемента до центроида солида
                //    Print(string.Format("Зашёл элемент: {0}", elem.Name), KPLN_Loader.Preferences.MessageType.Warning);
                //    solidsList.Sort((x, y) => (int)(x.ComputeCentroid().DistanceTo(elemPointCenter) - y.ComputeCentroid().DistanceTo(elemPointCenter)));
                //    solid = solidsList.First();
                //}

                if (solid == null) continue;
                Parameter parameter = elem.LookupParameter(sectionParam);
                if (parameter != null && !parameter.IsReadOnly)
                {
                    if (parameter.Set(solids[solid]))
                    {
                        counter++;
                    }
                }
            }
            Print(string.Format("Количество дубликатов: {0}", duplicates.Keys.Count), KPLN_Loader.Preferences.MessageType.Warning);
            duplicates = duplicates.Where(x => x.Value.Count > 1).ToDictionary(x => x.Key, x => x.Value);
            Print(string.Format("Количество необработанных элементов после 2-ого этапа: {0}", notIntersectedElems.Count - counter), KPLN_Loader.Preferences.MessageType.Warning);
            Print(string.Format("Количество дубликатов: {0}", duplicates.Keys.Count), KPLN_Loader.Preferences.MessageType.Warning);
            foreach (KeyValuePair<Element, List<string>> item in duplicates)
            {
                if (item.Key == null) continue;
                XYZ elemPointCenter = null;
                try
                {
                    List<Solid> solidsFromElem = GeometryUtils.GetSolidsFromElement(item.Key);
                    elemPointCenter = solidsFromElem.First().ComputeCentroid();
                }
                catch (Exception)
                {
                    try
                    {
                        elemPointCenter = solidBoundingBox(item.Key.get_BoundingBox(null)).ComputeCentroid();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (elemPointCenter == null) continue;
                List<Solid> solidsList = solids.Where(x => item.Value.Contains(x.Value)).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
                solidsList.Sort((x, y) => (int)(x.ComputeCentroid().DistanceTo(elemPointCenter) - y.ComputeCentroid().DistanceTo(elemPointCenter)));
                Solid solid = solidsList.First();
                if (solid == null) continue;
                Parameter parameter = item.Key.LookupParameter(sectionParam);
                if (parameter != null && !parameter.IsReadOnly)
                {
                    if (parameter.Set(solids[solid]))
                    {
                        counter++;
                    }
                }
            }
            if (deleteSolids)
            {
                foreach (DirectShape ds in directShapes)
                {
                    doc.Delete(ds.Id);
                }
            }
            return true;
        }

        private DirectShape createSolidsInModel(double[] minMaxCoords, List<XYZ> pointsOfGridsIntersect, out Solid solid)
        {
            List<XYZ> pointsOfGridsIntersectDwn = new List<XYZ>();
            List<XYZ> pointsOfGridsIntersectUp = new List<XYZ>();
            foreach (XYZ point in pointsOfGridsIntersect)
            {
                XYZ newPointDwn = new XYZ(point.X, point.Y, minMaxCoords[0]);
                pointsOfGridsIntersectDwn.Add(newPointDwn);
                XYZ newPointUp = new XYZ(point.X, point.Y, minMaxCoords[1]);
                pointsOfGridsIntersectUp.Add(newPointUp);
            }
            List<Curve> curvesListDwn = getCurvesListFromPoints(pointsOfGridsIntersectDwn);
            List<Curve> curvesListUp = getCurvesListFromPoints(pointsOfGridsIntersectUp);
            CurveLoop curveLoopDwn = CurveLoop.Create(curvesListDwn);
            CurveLoop curveLoopUp = CurveLoop.Create(curvesListUp);
            try
            {
                solid = GeometryCreationUtilities.CreateLoftGeometry(new CurveLoop[] { curveLoopDwn, curveLoopUp },
                    new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId));
                DirectShape directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                directShape.AppendShape(new GeometryObject[] { solid });
                return directShape;
            }
            catch (Exception e)
            {
                PrintError(e);
                solid = null;
                return null;
            }
        }

        private List<XYZ> getPointsOfGridsIntersection(List<Grid> grids)
        {
            List<XYZ> pointsOfGridsIntersect = new List<XYZ>();
            foreach (Grid grid1 in grids)
            {
                if (grid1 == null) continue;
                Curve curve1 = grid1.Curve;
                foreach (Grid grid2 in grids)
                {
                    if (grid2 == null) continue;
                    if (grid1.Id == grid2.Id) continue;
                    Curve curve2 = grid2.Curve;
                    IntersectionResultArray intersectionResultArray = new IntersectionResultArray();
                    try
                    {
                        curve1.Intersect(curve2, out intersectionResultArray);
                    }
                    catch (Exception e)
                    {
                        TaskDialog.Show("Отладка", e.ToString());
                    }

                    if (intersectionResultArray == null || intersectionResultArray.IsEmpty) continue;

                    foreach (IntersectionResult intersection in intersectionResultArray)
                    {
                        XYZ point = intersection.XYZPoint;
                        if (!isListContainsPoint(pointsOfGridsIntersect, point))
                        {
                            pointsOfGridsIntersect.Add(point);
                        }
                    }
                }
            }
            return pointsOfGridsIntersect;
        }

        private XYZ getCenterPointOfPoints(List<XYZ> pointsOfGridsIntersect)
        {
            double totalX = 0, totalY = 0, totalZ = 0;
            foreach (XYZ xyz in pointsOfGridsIntersect)
            {
                totalX += xyz.X;
                totalY += xyz.Y;
                totalZ += xyz.Z;
            }
            double centerX = totalX / pointsOfGridsIntersect.Count;
            double centerY = totalY / pointsOfGridsIntersect.Count;
            double centerZ = totalZ / pointsOfGridsIntersect.Count;
            return new XYZ(centerX, centerY, centerZ);
        }

        private static List<Curve> getCurvesListFromPoints(List<XYZ> pointsOfGridsIntersect)
        {
            List<Curve> curvesList = new List<Curve>();
            for (int i = 0; i < pointsOfGridsIntersect.Count; i++)
            {
                if (i == pointsOfGridsIntersect.Count - 1)
                {
                    curvesList.Add(Line.CreateBound(pointsOfGridsIntersect[i], pointsOfGridsIntersect[0]));
                    continue;
                }
                curvesList.Add(Line.CreateBound(pointsOfGridsIntersect[i], pointsOfGridsIntersect[i + 1]));
            }
            return curvesList;
        }

        private bool isListContainsPoint(List<XYZ> pointsList, XYZ point)
        {
            foreach (XYZ curpoint in pointsList)
            {
                if (curpoint.IsAlmostEqualTo(point))
                {
                    return true;
                }
            }
            return false;
        }

        private double[] getMinMaxZCoordOfModel(List<Element> elems)
        {
            List<BoundingBoxXYZ> elemsBox = new List<BoundingBoxXYZ>();
            foreach (Element item in elems)
            {
                BoundingBoxXYZ itemBox = item.get_BoundingBox(null);
                if (itemBox == null) continue;
                elemsBox.Add(itemBox);
            }
            double maxPointOfModel = elemsBox.Select(x => x.Max.Z).Max();
            double minPointOfModel = elemsBox.Select(x => x.Min.Z).Min();
            return new double[] { minPointOfModel, maxPointOfModel };
        }

        public static Solid solidBoundingBox(BoundingBoxXYZ bbox)
        {
            // corners in BBox coords
            XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);
            //edges in BBox coords
            Line edge0 = Line.CreateBound(pt0, pt1);
            Line edge1 = Line.CreateBound(pt1, pt2);
            Line edge2 = Line.CreateBound(pt2, pt3);
            Line edge3 = Line.CreateBound(pt3, pt0);
            //create loop, still in BBox coords
            List<Curve> edges = new List<Curve>();
            edges.Add(edge0);
            edges.Add(edge1);
            edges.Add(edge2);
            edges.Add(edge3);
            Double height = bbox.Max.Z - bbox.Min.Z;
            CurveLoop baseLoop = CurveLoop.Create(edges);
            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(baseLoop);
            Solid preTransformBox = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);

            Solid transformBox = SolidUtils.CreateTransformed(preTransformBox, bbox.Transform);

            return transformBox;
        }

    }

    public class ClockwiseComparer : IComparer<XYZ>
    {
        private XYZ CenterPoint;

        public ClockwiseComparer(XYZ centerPoint)
        {
            CenterPoint = centerPoint;
        }

        public int Compare(XYZ pointA, XYZ pointB)
        {
            if (pointA.X - CenterPoint.X >= 0 && pointB.X - CenterPoint.X < 0)
                return 1;
            if (pointA.X - CenterPoint.X < 0 && pointB.X - CenterPoint.X >= 0)
                return -1;

            if (pointA.X - CenterPoint.X == 0 && pointB.X - CenterPoint.X == 0)
            {
                if (pointA.Y - CenterPoint.Y >= 0 || pointB.Y - CenterPoint.Y >= 0)
                    if (pointA.Y > pointB.Y)
                        return 1;
                    else return -1;
                if (pointB.Y > pointA.Y)
                    return 1;
                else return -1;
            }

            // compute the cross product of vectors (CenterPoint -> a) x (CenterPoint -> b)
            double det = (pointA.X - CenterPoint.X) * (pointB.Y - CenterPoint.Y) -
                             (pointB.X - CenterPoint.X) * (pointA.Y - CenterPoint.Y);
            if (det < 0)
                return 1;
            if (det > 0)
                return -1;

            // points a and b are on the same line from the CenterPoint
            // check which point is closer to the CenterPoint
            double d1 = (pointA.X - CenterPoint.X) * (pointA.X - CenterPoint.X) +
                            (pointA.Y - CenterPoint.Y) * (pointA.Y - CenterPoint.Y);
            double d2 = (pointB.X - CenterPoint.X) * (pointB.X - CenterPoint.X) +
                            (pointB.Y - CenterPoint.Y) * (pointB.Y - CenterPoint.Y);
            if (d1 > d2)
                return 1;
            else return -1;
        }
    }

    public class ElementComparer : IEqualityComparer<Element>
    {
        public bool Equals(Element x, Element y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Element obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public class SolidComparer : IEqualityComparer<Solid>
    {
        public bool Equals(Solid x, Solid y)
        {
            return x == y;
        }

        public int GetHashCode(Solid obj)
        {
            return obj.GetHashCode();
        }
    }

}
