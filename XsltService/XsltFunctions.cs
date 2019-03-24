using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;


namespace XsltService
{
    /// <summary>
    /// C# containing methods visible in Functions.xslt for use in transformations.
    /// </summary>
    public class XsltFunctions
    {
        private const string YearPattern = "yyyy";

        private const string ShortDatePattern = "yyyy-MM-dd";

        private const string ShortUtcDatePattern = ShortDatePattern + "K";

        private const string LongDatePattern = "yyyy-MM-ddTHH:mm:ss";

        private const string LongUtcDatePattern = LongDatePattern + "K";

        private static readonly IList<string> CustomFormats = new List<string>
            {
                "yyyyMMdd",
                "yyyy-MM-dd-HH.mm.ss.f",
                "yyyy-MM-dd-HH.mm.ss.ff",
                "yyyy-MM-dd-HH.mm.ss.fff",
                "yyyy-MM-dd-HH.mm.ss.ffff",
                "yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "dd.MM.yyyy"
            };

        private static List<string> XPathToStrings(XPathNodeIterator iterator)
        {
            return iterator.OfType<XPathNavigator>().Select(n => n.OuterXml).ToList();
        }

        private static List<XElement> XPathToElements(XPathNodeIterator iterator)
        {
            var xmlStrings = XPathToStrings(iterator);
            var xmlElements = new List<XElement>();
            xmlStrings.ForEach(el => xmlElements.Add(XElement.Parse(el)));

            return xmlElements;
        }

        public static XElement XPathToElement(XPathNodeIterator iterator)
        {
            var xmlElements = XPathToElements(iterator);   

            return xmlElements.FirstOrDefault();
        }

        /// <summary>
        /// Calculates conlicts.
        /// </summary>
        /// <param name="matrikel">The Geometry of the matrikel</param>
        /// <param name="features">The Geometry of the features</param>
        /// <param name="bufferWidth">The Geometry of the features</param>
        /// <returns>True if conflicts where found</returns>
        /// <exception cref="XsltException">If <paramref name="matrikel"/> or <paramref name="features"/> are badly formatted.</exception>
        public static bool Conflicts(XPathNodeIterator cadastral, XPathNodeIterator features, string bufferWidth)
        {
            if (!double.TryParse(bufferWidth, out var bufferWidthParsed))
            {
                throw new ArgumentException("BufferWidth is null or not a double.");
            }

            IGeometry cadastralGeometry = CadastralIterToElement(cadastral);
            IGeometry cadastralGeometryBuffered = (!cadastralGeometry.Buffer(bufferWidthParsed).IsEmpty ? cadastralGeometry.Buffer(bufferWidthParsed) : cadastralGeometry) as Geometry;
            //using original geometry if buffered is empty
            IGeometry intersection = GetIntersectingGeometry(cadastralGeometryBuffered, features);
            return intersection.IsValid || !intersection.IsEmpty;
        }

        /// <summary>
        /// Calculates intersection between a matrikel and a feature and returns, the 
        /// </summary>
        /// <param name="matrikel">The Geometry of the matrikel</param>
        /// <param name="features">The Geometry of the features</param>
        /// <returns>True if conflicts where found</returns>
        /// <exception cref="XsltException">If <paramref name="matrikel"/> or <paramref name="features"/> are badly formatted.</exception>
        public static double Intersection(XPathNodeIterator cadastralIter, XPathNodeIterator features)
        {
            IGeometry cadastralGeometry = CadastralIterToElement(cadastralIter);
            IGeometry feature = GetIntersectingGeometry(cadastralGeometry, features);
            return feature != null ? GeometryHelper.Intersection(cadastralGeometry, feature) : 0;
        }

        /// <summary>
        /// Finds the boundingbox for <paramref name="gml"/>.
        /// </summary>
        /// <param name="gml">The Geometry</param>
        /// <param name="maximumBufferWidth">The Geometry</param>
        /// <returns>Bounding box in a format suitable for WFS service</returns>
        /// <exception cref="XsltException">If <paramref name="gml"/> is badly formatted.</exception>
        public static string BoundingBox(XPathNodeIterator iterator, string maxBufferWidth)
        {

            if (maxBufferWidth == null || !double.TryParse(maxBufferWidth, out var tryParse))
            {
                throw new ArgumentException("maximumbufferWidth is null or not a double." + maxBufferWidth);
            }
            // Convert to element and then to string, to ensure correct format.
            XElement element = XPathToElement(iterator);
            string stringGeometry = ToSimpleGml3(element);

            // Get Polygon geometry and return the matching boundingBox
            IGeometry geometry = GeometryHelper.GetGeometryFromGml(stringGeometry);
            return GeometryHelper.GetBoundingBoxFromGeometry(geometry);
        }

        public static String PaddedBoundingBoxFromGml(XPathNodeIterator iterator, String padding, String minSize)
        {
            XElement xml = XPathToElement(iterator);
            IGeometry geometry = GeometryHelper.GetGeometryFromGml(ToSimpleGml3(xml));

            return GeometryHelper.GetPaddedBoundingBox(geometry, double.Parse(padding), double.Parse(minSize));

        }

        private static IGeometry CadastralIterToElement(XPathNodeIterator cadastral)
        {
            XElement element = XPathToElement(cadastral).Elements().FirstOrDefault();
            return ElementToGeometry(element);
        }

        private static IGeometry ElementToGeometry(XElement element)
        {
            String simpleGml = ToSimpleGml3(element);
            return GeometryHelper.GetGeometryFromGml(simpleGml);
        }

        private static IGeometry GetIntersectingGeometry(IGeometry geometry, XPathNodeIterator features)
        {
            if (!geometry.IsValid)
            {
                throw new ArgumentException("Geometry is not valid");
            }

            List<XElement> featureList = XPathToElements(features).Select(it => it.Nodes().FirstOrDefault() as XElement).ToList();

            foreach (var element in featureList)
            {
                IGeometry feature = ElementToGeometry(element);
                if (!feature.IsValid)
                {
                    throw new ArgumentException("Geometry is not valid");
                }

                IGeometry intersection = geometry.Intersects(feature) ? feature : null;
                if (intersection != null)
                {
                    return feature;
                }
            }

            return null;
        }


        /// <summary>
        /// Finds the padded bounding box for the geometry defined in <paramref name="gml"/>.
        /// The bounding box of the geometry will be padded by the number of meters defined by 
        /// <paramref name="padding"/> and expanded to a square. The <paramref name="minSize"/>
        /// parameter defines the minimum side length of the square in meters.
        /// </summary>
        /// <param name="gml"></param>
        /// <param name="padding"></param>
        /// <param name="minSize"></param>
        /// <returns>String representation of the bounding box in the format [x],[y],[size].</returns>
        public static string PaddedBoundingBox(XPathNodeIterator gml, string padding, string minSize)
        {
            var boundingBox = BoundingBox(gml, "0");

            return PaddedBoundingBox(boundingBox, padding, minSize);
        }

        /// <summary>
        /// Finds the padded bounding box for a bounding box defined in <paramref name="gml"/>.
        /// The bounding box of the geometry will be padded by the number of meters defined by 
        /// <paramref name="padding"/> and expanded to a square. The <paramref name="minSize"/>
        /// parameter defines the minimum side length of the square in meters.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="padding"></param>
        /// <param name="minSize"></param>
        /// <returns>String representation of the bounding box in the format [x],[y],[size].</returns>
        public static string PaddedBoundingBox(string boundingBox, string padding, string minSize)
        {
            var corners = boundingBox.Split(',').Select(corner => double.Parse(corner, CultureInfo.InvariantCulture)).ToList();

            var minX = corners[0]; var minY = corners[1]; var maxX = corners[2]; var maxY = corners[3];

            var width = maxX - minX; var height = maxY - minY;

            var bufferedWidth = Math.Max(double.Parse(minSize), width + double.Parse(padding) * 2);
            var bufferedHeight = Math.Max(double.Parse(minSize), height + double.Parse(padding) * 2);

            var east = minX - (bufferedWidth - width) / 2;
            var north = minY - (bufferedHeight - height) / 2;

            if (bufferedWidth < bufferedHeight)
                east -= (bufferedHeight - bufferedWidth) / 2;
            else
                north -= (bufferedWidth - bufferedHeight) / 2;

            return String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", east, north, Math.Max(bufferedWidth, bufferedHeight));
        }

        /// <summary>
        /// Split a string into parts using the specified delimeter and return the part at
        /// the given index.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="delimeter"></param>
        /// <param name="index"></param>
        /// <returns>The part of the string at the given index.</returns>
        public static string SplitString(string text, string delimeter, int index)
        {
            return text.Split(delimeter[0]).ToList()[index];
        }

        /// <summary>
        /// Returns true if the two strings are equal. Case of the strings are ignored.
        /// </summary>
        /// <param name="first">First string to compare.</param>
        /// <param name="second">Second string to compare.</param>
        /// <returns>True if the strings are equal.</returns>
        public static bool CompareString(string first, string second)
        {
            return String.Compare(first, second, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Returns true if the first node is a valid date time. If <paramref name="iterator"/> contains zero elements, then false is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <returns>True if the first node is a date time.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static bool IsDateTime(XPathNodeIterator iterator)
        {
            var value = GetTrimmed(iterator);
            return IsDateTime(value);
        }

        /// <summary>
        /// Returns true if value is a date time.
        /// </summary>
        /// <param name="value">The value to inspect.</param>
        /// <returns>True if <paramref name="value"/> is a date time.</returns>
        public static bool IsDateTime(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return CanParseUsingDefault(value) || CanParseCustom(value);
        }

        /// <summary>
        /// Converts the first node in <paramref name="iterator"/> to a string in the format yyyy. If <paramref name="iterator"/> contains zero elements, then <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <returns>The normalzied date time.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static string ConvertYear(XPathNodeIterator iterator)
        {
            var value = GetTrimmed(iterator);
            return ConvertYear(value);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a string in the format yyyy.
        /// </summary>
        /// <param name="value">The date time to convert.</param>
        /// <returns>The normalzied date time.</returns>
        public static string ConvertYear(string value)
        {
            return Convert(value, YearPattern, YearPattern);
        }

        /// <summary>
        /// Converts the first node in <paramref name="iterator"/> to a string in the format yyyy-MM-dd(K). If <paramref name="iterator"/> contains zero elements, then <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <returns>The normalzied date time.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static string ConvertDate(XPathNodeIterator iterator)
        {
            var value = GetTrimmed(iterator);
            return ConvertDate(value);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a string in the format yyyy-MM-dd(K).
        /// </summary>
        /// <param name="value">The date time to convert.</param>
        /// <returns>The normalzied date time.</returns>
        public static string ConvertDate(string value)
        {
            return Convert(value, ShortDatePattern, ShortUtcDatePattern);
        }

        /// <summary>
        /// Converts the first node in <paramref name="iterator"/> to a string in the format yyyy-MM-dd(K). If <paramref name="iterator"/> contains zero elements, then <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <returns>The normalzied date time.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static string ConvertUtcDate(XPathNodeIterator iterator)
        {
            var value = GetTrimmed(iterator);
            DateTime dateTime;
            var parseOperation = DateTime.TryParse(value, out dateTime);

            if (parseOperation)
            {
                var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                var localTime = utcDateTime.ToLocalTime();
                return localTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            throw new ArgumentException(string.Format("Following utcTime is not a valid value: {0}", dateTime));
        }

        /// <summary>
        /// Converts the first node in <paramref name="iterator"/> to a string in the format yyyy-MM-ddTHH-mm-ss(K). If <paramref name="iterator"/> contains zero elements, then <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <returns>The normalzied date time.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static string ConvertDateTime(XPathNodeIterator iterator)
        {
            var value = GetTrimmed(iterator);
            return ConvertDateTime(value);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a string in the format yyyy-MM-ddTHH-mm-ss(K).
        /// </summary>
        /// <param name="value">The date time to convert.</param>
        /// <returns>The normalzied date time.</returns>
        public static string ConvertDateTime(string value)
        {
            return Convert(value, LongDatePattern, LongUtcDatePattern);
        }

        /// <summary>
        /// Formats the first node in <paramref name="iterator"/> to a string in the format defined by <paramref name="format"/>. 
        /// If <paramref name="iterator"/> contains zero elements, then <see cref="string.Empty"/> is returned.
        /// If <paramref name="format"/> is an invalid format, then the value of the node is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <param name="format">The format, can only contain the letters 'd', 'm', 'å', dot, whitespace and '-'.</param>
        /// <returns>The formated date time.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static string FormatDateTime(XPathNodeIterator iterator, string format)
        {
            var value = GetTrimmed(iterator);
            return FormatDateTime(value, format);
        }

        /// <summary>
        /// Formats <paramref name="value"/> to a string in the format defined by <paramref name="format"/>. 
        /// If <paramref name="format"/> is an invalid format, then value is returned.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="format">The format, can only contain the letters 'd', 'm', 'å', dot, whitespace and '-'.</param>
        /// <returns>The formated date time.</returns>
        public static string FormatDateTime(string value, string format)
        {
            const string ValidFormatPattern = "[-/dmå. ]+";

            return Format(value, format, ValidFormatPattern, ConvertDateTime, SafeParseDateTime, DateTimeToString);
        }

        /// <summary>
        /// Formats the first node in <paramref name="iterator"/> to a string in the format defined by <paramref name="format"/>. 
        /// If <paramref name="iterator"/> contains zero elements, then <see cref="string.Empty"/> is returned.
        /// If <paramref name="format"/> is an invalid format, then the value of the node is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <param name="format">The format, can only contain the letters dot, comma and '#'.</param>
        /// <returns>The formated double.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static string FormatDouble(XPathNodeIterator iterator, string format)
        {
            var value = GetTrimmed(iterator);
            return FormatDouble(value, format);
        }

        /// <summary>
        /// Formats <paramref name="value"/> to a string in the format defined by <paramref name="format"/>. 
        /// If <paramref name="format"/> is an invalid format, then value is returned.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="format">The format, can only contain the letters dot, comma and '#'.</param>
        /// <returns>The formated double.</returns>
        public static string FormatDouble(string value, string format)
        {
            const string ValidFormatPattern = "[#.,0]+";

            return Format(
                value,
                format,
                ValidFormatPattern,
                val => val,
                SafeParseDouble,
                DoubleToString);
        }

        /// <summary>
        /// Compares the value in all the nodes and check if the value is the same in every node.
        /// </summary>
        /// <param name="iterator">Node iterator.</param>
        /// <returns>True if all node values are the same.</returns>
        public static bool DoesNodesContainSameValue(XPathNodeIterator iterator)
        {
            if (iterator == null)
            {
                return false;
            }

            string initialValue;

            if (iterator.MoveNext())
            {
                initialValue = iterator.Current.Value;
            }
            else
            {
                return true;
            }

            var result = true;
            while (iterator.MoveNext() && result)
            {
                result &= iterator.Current.Value == initialValue;
            }

            return result;
        }

        /// <summary>
        /// Returns true if x numbers of days has passed since the provided date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public static bool HasXDaysPassedSince(string date, int days)
        {
            var deadline = DateTime.Parse(date).AddDays(days);
            return (deadline < DateTime.Now);
        }

        /// <summary>
        /// Convers a string to lower case
        /// </summary>
        /// <param name="value">The string to be converted</param>
        /// <returns>The original string in lower case</returns>
        public static string ToLowercase(string value)
        {
            return value.ToLower(CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Converts the first node in <paramref name="iterator"/> to a string in the format yyyy-MM-dd(K). If <paramref name="iterator"/> contains zero elements, then <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <param name="iterator">The node iterator.</param>
        /// <returns>The normalzied date time.</returns>
        /// <exception cref="XsltException">If <paramref name="iterator"/> contains multiple elements.</exception>
        public static string EncodeUrl(XPathNodeIterator iterator)
        {
            if (iterator == null)
            {
                return string.Empty;
            }

            var value = GetTrimmed(iterator);
            return EncodeUrl(value);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a encoded url.
        /// </summary>
        /// <param name="value">The url to convert.</param>
        /// <returns>The encoded url.</returns>
        private static string EncodeUrl(string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        private static string Format<T>(string value, string format, string regexToValidateFormat,
                                        Func<string, string> normalize, Func<string, T?> parse,
                                        Func<T, string, string> toString)
            where T : struct
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(format) ||
                !Regex.IsMatch(format, regexToValidateFormat, RegexOptions.IgnoreCase))
            {
                return value;
            }

            var normalizedValue = normalize(value);
            if (string.IsNullOrEmpty(normalizedValue))
            {
                return value;
            }

            var tmp = parse(normalizedValue);
            return tmp == null ? value : toString(tmp.Value, format);
        }

        private static string Convert(string value, string datePattern, string utcDatePattern)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            string result;
            if (CanParseUsingDefault(value))
            {
                result = NormalizeDateTime(value, datePattern, utcDatePattern);
            }
            else if (CanParseCustom(value))
            {
                result = NormalizeDateTimeCustomParse(value, datePattern);
            }
            else
            {
                result = value;
            }

            return result;
        }

        private static bool IsUtc(string value)
        {
            const string UtcPattern = "[^ \t]Z$";
            return Regex.IsMatch(value, UtcPattern);
        }

        private static string NormalizeDateTimeCustomParse(string value, string localTimePattern)
        {
            var dt = DateTime.ParseExact(value, CustomFormats.ToArray(), CultureInfo.InvariantCulture,
                                         DateTimeStyles.AssumeLocal);
            return dt.ToString(localTimePattern, CultureInfo.InvariantCulture);
        }

        private static string NormalizeDateTime(string value, string localTimePattern, string utcTimePattern)
        {
            var formatingPattern = IsUtc(value) ? utcTimePattern : localTimePattern;
            var adjustToUniversal = IsUtc(value) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;

            var time = DateTime.Parse(value, CultureInfo.InvariantCulture,
                                      DateTimeStyles.AssumeLocal | adjustToUniversal);
            string result = time.ToString(formatingPattern, CultureInfo.InvariantCulture);
            return result;
        }

        private static bool CanParseUsingDefault(string value)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, 
                                     DateTimeStyles.AssumeLocal, out var notUsed);
        }

        private static bool CanParseCustom(string value)
        {
            return DateTime.TryParseExact(value, CustomFormats.ToArray(), CultureInfo.InvariantCulture,
                                          DateTimeStyles.AssumeLocal, out var notUsed);
        }

        private static string GetTrimmed(XPathNodeIterator iterator)
        {
            if (iterator.Count > 1)
            {
                throw new XsltException("Cannot convert a node-set which contains more than one node");
            }

            var xpathExpressions = XPathToStrings(iterator);

            var value = xpathExpressions.FirstOrDefault()?.Trim();

            return value ?? string.Empty;
        }

        private static double? SafeParseDouble(string val)
        {
            return double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                       ? result
                       : (double?) null;
        }

        private static string DoubleToString(double v, string format)
        {
            if (double.IsNaN(v))
            {
                return string.Empty;
            }

            var tmpFormat = format.Replace(".", "<dot>").Replace(",", "<comma>");
            format = tmpFormat.Replace("<dot>", NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator).Replace(
                "<comma>", NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator);

            return v.ToString(string.Format(CultureInfo.InvariantCulture, "{0};-{0};0", format),
                              CultureInfo.CurrentCulture);
        }

        private static DateTime? SafeParseDateTime(string val)
        {
            return DateTime.TryParse(val, CultureInfo.InvariantCulture,
                                     DateTimeStyles.AssumeLocal, out var result)
                       ? result
                       : (DateTime?)null;
        }

        private static string DateTimeToString(DateTime v, string format)
        {
            return v.ToString(format.Replace('å', 'y').Replace('m', 'M').Replace("/", "\\/"), CultureInfo.CurrentCulture);
        }

        /// <summary>
        ///  Removes attributes to make it parsable by the SqlGeometry constructor
        /// </summary>
        /// <param name="gml">The GML which needs to have attributes removed.</param>
        /// <returns></returns>
        private static string ToSimpleGml3(string gml)
        {
            /* 1. Remove attributes */
            var result = new StringBuilder();
            var currentInputPosition = 0;
            var startOfAttributes = StartOfAttributes(gml, 0);
            var endOfAttributes = gml.IndexOf('>', startOfAttributes);

            while (startOfAttributes != -1)
            {
                result.Append(gml, currentInputPosition, startOfAttributes - currentInputPosition);
                currentInputPosition = endOfAttributes;
                startOfAttributes = StartOfAttributes(gml, endOfAttributes);
                if (startOfAttributes == -1)
                {
                    break;
                }

                endOfAttributes = gml.IndexOf('>', startOfAttributes);
            }

            result.Append(gml, currentInputPosition, gml.Length - currentInputPosition);

            /* 2. Add gml namespace and prefixes */
            var right = result.ToString().IndexOf('>');

            result.Insert(right, @" xmlns:gml=""http://www.opengis.net/gml""");

            EnsureGmlPrefix(result, "Polygon");
            EnsureGmlPrefix(result, "posList");
            EnsureGmlPrefix(result, "exterior");
            EnsureGmlPrefix(result, "interior");
            EnsureGmlPrefix(result, "LinearRing");

            return result.ToString();
        }

        private static string ToSimpleGml3(XElement xml)
        {
            /* 1. Remove attributes */
            XElement xmlElement = new XElement(xml);
            xmlElement.DescendantsAndSelf().ToList().ForEach(it => it.RemoveAttributes());

            /* 2. Add gml namespace and prefixes */
            XNamespace gml = "http://www.opengis.net/gml";
            xmlElement.SetAttributeValue(XNamespace.Xmlns + "gml", "http://www.opengis.net/gml");

            return xmlElement.ToString();
        }

        private static int StartOfAttributes(string s, int left)
        {
            while (true)
            {
                left = s.IndexOf('<', left);
                if (left == -1)
                {
                    return -1;
                }

                var right = s.IndexOf('>', left);
                if (right == -1)
                {
                    // No more elements in XML
                    return -1;
                }

                var space = s.IndexOfAny(new[] { ' ', '\t' }, left, right - left);
                /*Math.Min(gml.IndexOf(' ', left), gml.IndexOf('\t', left));*/
                if (space != -1)
                {
                    // Found left of attributes
                    return space;
                }

                left = right;
            }
        }

        public static string ConvertUtcToLocalTime(string utcTime)
        {
            DateTime dateTime;
            var parseOperation = DateTime.TryParse(utcTime, out dateTime);
            if (parseOperation)
            {
                return dateTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            throw new ArgumentException(string.Format("Following utcTime is not a valid value: {0}", utcTime));
        }

        private static void EnsureGmlPrefix(StringBuilder result, string s)
        {
            result.Replace("<" + s, @"<gml:" + s);
            result.Replace("</" + s, @"</gml:" + s);
        }

        /// <summary>
        /// Function to 'debug' xslt. Call this function with a node-set, and see what the nodes in the set contains 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object DebugIt(object obj)
        {
            if (obj is string s)
            {
                Console.Write(s);
                return s;
            }

            if (obj == null)
            {
                return obj;
            }

            /*XPathNodeIterator iterator*/
            var nodes = obj as XPathNodeIterator;
            var copy = nodes?.Clone();
            var texts = XPathToStrings(nodes);

            Console.WriteLine("Responses are: \n");
            foreach(var entry in texts)
            {
                Console.WriteLine(entry);
            }
            return copy;
        }
    }
}