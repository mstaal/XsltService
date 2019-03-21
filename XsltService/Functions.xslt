<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:func="urn:oio:ebst:diadem:functions"
                exclude-result-prefixes="msxsl xsl func">

  <msxsl:script language="C#" implements-prefix="func">
    <msxsl:assembly name="XsltService" />
    <msxsl:using namespace="XsltService" />
    public bool IsDateTime(XPathNodeIterator iterator)
    {
    return XsltFunctions.IsDateTime(iterator);
    }

    public bool Conflicts(XPathNodeIterator matrikel, XPathNodeIterator features, string bufferWidth)
    {
    return XsltFunctions.Conflicts(matrikel, features, bufferWidth);
    }

    public double Intersection(XPathNodeIterator matrikel, XPathNodeIterator features)
    {
    return XsltFunctions.Intersection(matrikel, features);
    }

    public string BoundingBox(XPathNodeIterator gml, string maximumBufferWidth)
    {
    return XsltFunctions.BoundingBox(gml, maximumBufferWidth);
    }

    public string PaddedBoundingBoxFromGml(XPathNodeIterator gml, string padding, string minSize)
    {
    return XsltFunctions.PaddedBoundingBox(gml, padding, minSize);
    }

    public string PaddedBoundingBox(string boundingBox, string padding, string minSize)
    {
    return XsltFunctions.PaddedBoundingBox(boundingBox, padding, minSize);
    }

    public string SplitString(string text, string delimeter, int index)
    {
    return XsltFunctions.SplitString(text, delimeter, index);
    }

    public string ConvertYear(XPathNodeIterator iterator)
    {
    return XsltFunctions.ConvertYear(iterator);
    }

    public string ConvertDate(XPathNodeIterator iterator)
    {
    return XsltFunctions.ConvertDate(iterator);
    }

    public string ConvertUtcDate(XPathNodeIterator iterator)
    {
    return XsltFunctions.ConvertUtcDate(iterator);
    }

    public string ConvertDateTime(XPathNodeIterator iterator)
    {
    return XsltFunctions.ConvertDateTime(iterator);
    }

    public string FormatDateTime(XPathNodeIterator iterator, string format)
    {
    return XsltFunctions.FormatDateTime(iterator, format);
    }

    public string FormatDouble(XPathNodeIterator iterator, string format)
    {
    return XsltFunctions.FormatDouble(iterator, format);
    }

    public bool DoesNodesContainSameValue(XPathNodeIterator iterator)
    {
    return XsltFunctions.DoesNodesContainSameValue(iterator);
    }

    public static string ToLowercase(string value)
    {
    return XsltFunctions.ToLowercase(value);
    }

    public static bool CompareString(string first, string second)
    {
    return XsltFunctions.CompareString(first, second);
    }

    public static string ConvertUtcToLocalTime(string utcTime)
    {
    return XsltFunctions.ConvertUtcToLocalTime(utcTime);
    }

    public static bool DebugIt(object obj)
    {
    return XsltFunctions.DebugIt(obj);
    }

    public static bool HasXDaysPassedSince(string date, int days)
    {
    return XsltFunctions.HasXDaysPassedSince(date, days);
    }

    public string EncodeUrl(XPathNodeIterator iterator)
    {
    return XsltFunctions.EncodeUrl(iterator);
    }
  </msxsl:script>

</xsl:stylesheet>