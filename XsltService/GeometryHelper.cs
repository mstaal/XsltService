using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GML2;

namespace XsltService
{
    public static class GeometryHelper
    {
        public static double Intersection(IGeometry matrikelGeometri, IGeometry featureGeometry)
        {
            double intersection = matrikelGeometri.Intersection(featureGeometry).Area;
            var percent = intersection / matrikelGeometri.Area * 100.0;
            return Math.Round(percent);
        }

        public static string GetBoundingBoxFromGeometry(IGeometry geometry)
        {
            Envelope envelope = geometry.EnvelopeInternal;
            return $"{envelope.MinX},{envelope.MinY},{envelope.MaxX},{envelope.MaxY}";
        }

        public static string GetPaddedBoundingBox(IGeometry geometry, double padding, double minSize)
        {
            string boundedbox = GetBoundingBoxFromGeometry(geometry);
            return GetPaddedBoundedBoxFromBoundedBox(boundedbox, padding, minSize);
        }

        public static string GetPaddedBoundedBoxFromBoundedBox(string boundingbox, double padding, double minSize)
        {
            List<double> corners = boundingbox.Split(',').Select( it => double.Parse(it) ).ToList();

            double minX = corners[0];
            double maxX = corners[2];
            double width = maxX - minX;
            double bufferedWidth = Math.Max(minSize, width + padding * 2);

            double minY = corners[1];
            double maxY = corners[3];
            double height = maxY - minY;
            double bufferedHeight = Math.Max(minSize, height + padding * 2);

            double east = minX - (bufferedWidth - width) / 2;
            double north = minY - (bufferedHeight - height) / 2;

            if (bufferedWidth < bufferedHeight)
                east -= (bufferedHeight - bufferedWidth) / 2;
            else
                north -= (bufferedWidth - bufferedHeight) / 2;

            return $"{east},{north},{Math.Max(bufferedWidth, bufferedHeight)}";
        }

        public static IGeometry GetGeometryFromGml(string gml)
        {
            gml = ReplaceObsoleteGmlTag(gml);

            try
            {
                IGeometry geometry = new GMLReader().Read(gml);
                return geometry;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Input not instance of geometry", ex);
            }
        }

        private static string ReplaceObsoleteGmlTag(string gml)
        {
            if (!gml.Contains("coordinates"))
            {
                return gml;
            }
            XElement gmlElement = XElement.Parse(gml);
            List<XElement> coordinatesList = gmlElement.Descendants().ToList().FindAll(it => it.Name == "coordinates");
            if (coordinatesList.Count > 0)
            {
                XElement coordinates = coordinatesList[0];
                if (IsMoreThanOnePoint(coordinates.Value))
                {
                    coordinates.Name = "posList";
                }
                else
                {
                    coordinates.Name = "pos";
                }
            }

            return gmlElement.ToString().Trim().Replace(",", " ");
        }

        private static bool IsMoreThanOnePoint(string coordinates)
        {
            List<string> spaceList = coordinates.Split("\\s+").ToList();
            List<string> commaList = coordinates.Split(",").ToList();
            return spaceList.Count > 1 && (commaList.Count - 1 == spaceList.Count);
        }

    }
}
