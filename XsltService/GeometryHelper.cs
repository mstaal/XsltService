using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            //gml = replaceObsoleteGmlTag(gml)

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

    }
}
