using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace XsltService.Test
{
   [TestFixture]
    public class XsltFunctionsTest
    {
        [Test]
        [TestCase(@"<?xml version=""1.0"" encoding=""utf-8""?>
<QueryResult>
  <Output>
    <gml:FeatureCollection xsi:schemaLocation=""http://schemas.kms.dk/wfs http://schemas.kms.dk/wfs/mat/2011/11/16/wfs110_gmlsfpl1_kms_matrikel.xsd http://www.opengis.net/gml http://schemas.opengis.net/gml/3.1.1/base/gml.xsd http://www.opengis.net/wfs http://schemas.opengis.net/wfs/1.1.0/wfs.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:kms=""http://schemas.kms.dk/wfs"" xmlns:gml=""http://www.opengis.net/gml"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:smil20=""http://www.w3.org/2001/SMIL20/"" xmlns:smil20lang=""http://www.w3.org/2001/SMIL20/Language"" xmlns:wfs=""http://www.opengis.net/wfs"" xmlns:ogc=""http://www.opengis.net/ogc"" xmlns:ows=""http://www.opengis.net/ows"">
  <gml:featureMember>
    <kms:Jordstykke gml:id=""IDD71D3CEA403023CDE04400144FAD6027"">
      <kms:featureID>322929</kms:featureID>
      <kms:featureType>Jordstykke, matrikuleret</kms:featureType>
      <kms:featureCode>9034</kms:featureCode>
      <kms:datasetIdentifier>2013-09-18T15:23:56</kms:datasetIdentifier>
      <kms:dataQualitySpecification>vejl. for matr. arb.</kms:dataQualitySpecification>
      <kms:dataQualityStatement>konverteret til LDS2</kms:dataQualityStatement>
      <kms:dataQualityDescription>ukendt</kms:dataQualityDescription>
      <kms:dataQualityProcessor>ukendt</kms:dataQualityProcessor>
      <kms:dataQualityResponsibleParty>Kort- og Matrikelstyrelsen</kms:dataQualityResponsibleParty>
      <kms:timeOfPublication>1979-10-24T00:00:00</kms:timeOfPublication>
      <kms:esr_Ejendomsnummer>7510221746</kms:esr_Ejendomsnummer>
      <kms:harSamletFastEjendom>
        <kms:SFESamletFastEjendom>
          <kms:sfe_SagsID>7805483</kms:sfe_SagsID>
          <kms:sfe_Journalnummer>U1979/05273</kms:sfe_Journalnummer>
          <kms:sfe_Registreringsdato>1979-10-24</kms:sfe_Registreringsdato>
          <kms:sfe_Ejendomsnummer>4230388</kms:sfe_Ejendomsnummer>
        </kms:SFESamletFastEjendom>
      </kms:harSamletFastEjendom>
      <kms:kms_SagsID>7805483</kms:kms_SagsID>
      <kms:kms_Journalnummer>U1979/05273</kms:kms_Journalnummer>
      <kms:registreringsdato>1979-10-24</kms:registreringsdato>
      <kms:ejerlavsnavn>Vejlby By, Ellevang</kms:ejerlavsnavn>
      <kms:landsejerlavskode>1021151</kms:landsejerlavskode>
      <kms:matrikelnummer>26ez</kms:matrikelnummer>
      <kms:faelleslod>nej</kms:faelleslod>
      <kms:registreretAreal>28357</kms:registreretAreal>
      <kms:arealBeregn>o</kms:arealBeregn>
      <kms:vejAreal>0</kms:vejAreal>
      <kms:vejArealBeregn>b</kms:vejArealBeregn>
      <kms:vandArealBeregn>ukendt</kms:vandArealBeregn>
      <kms:regionskode>1082</kms:regionskode>
      <kms:regionsnavn>Region Midtjylland</kms:regionsnavn>
      <kms:kommunekode>0751</kms:kommunekode>
      <kms:kommunenavn>Århus Kommune</kms:kommunenavn>
      <kms:sognekode>9088</kms:sognekode>
      <kms:sognenavn>Ellevang</kms:sognenavn>
      <kms:retskredskode>1165</kms:retskredskode>
      <kms:retskredsnavn>Århus retskreds</kms:retskredsnavn>
      <kms:geometri>
        <gml:Polygon srsName=""urn:ogc:def:crs:EPSG::25832"">
          <gml:exterior>
            <gml:LinearRing>
              <gml:posList srsDimension=""2"" count=""11"">575494.081 6229485.745 575461.155 6229327.71 575477.623 6229325.037 575478.273 6229328.36 575573.284 6229308.212 575565.043 6229266.511 575574.746 6229259.649 575613.95 6229252.176 575618.961 6229275.824 575656.227 6229451.725 575494.081 6229485.745</gml:posList>
            </gml:LinearRing>
          </gml:exterior>
        </gml:Polygon>
      </kms:geometri>
    </kms:Jordstykke>
  </gml:featureMember>
</gml:FeatureCollection>
  </Output>
</QueryResult>", "575461.155,6229252.176,575656.227,6229485.745")]
        public void BoundingBoxTest(string input, string expected)
        {
            using (var sr = new StringReader(input))
            {
                var inputDocument = XDocument.Load(sr);
                var navigator = inputDocument.CreateNavigator();
                var manager = new XmlNamespaceManager(navigator.NameTable);
                manager.AddNamespace("gml", "http://www.opengis.net/gml");

                var polygon = navigator.Select("//gml:Polygon", manager);
                var actual = XsltFunctions.BoundingBox(polygon, "0");
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [Test]
        [TestCase(@"<?xml version=""1.0"" encoding=""utf-8""?>
<QueryResult>
  <Output>
    <gml:FeatureCollection xsi:schemaLocation=""http://schemas.kms.dk/wfs http://schemas.kms.dk/wfs/mat/2011/11/16/wfs110_gmlsfpl1_kms_matrikel.xsd http://www.opengis.net/gml http://schemas.opengis.net/gml/3.1.1/base/gml.xsd http://www.opengis.net/wfs http://schemas.opengis.net/wfs/1.1.0/wfs.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:kms=""http://schemas.kms.dk/wfs"" xmlns:gml=""http://www.opengis.net/gml"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:smil20=""http://www.w3.org/2001/SMIL20/"" xmlns:smil20lang=""http://www.w3.org/2001/SMIL20/Language"" xmlns:wfs=""http://www.opengis.net/wfs"" xmlns:ogc=""http://www.opengis.net/ogc"" xmlns:ows=""http://www.opengis.net/ows"">
  <gml:featureMember>
    <kms:Jordstykke gml:id=""IDD71D3CEA403023CDE04400144FAD6027"">
      <kms:featureID>322929</kms:featureID>
      <kms:featureType>Jordstykke, matrikuleret</kms:featureType>
      <kms:featureCode>9034</kms:featureCode>
      <kms:datasetIdentifier>2013-09-18T15:23:56</kms:datasetIdentifier>
      <kms:dataQualitySpecification>vejl. for matr. arb.</kms:dataQualitySpecification>
      <kms:dataQualityStatement>konverteret til LDS2</kms:dataQualityStatement>
      <kms:dataQualityDescription>ukendt</kms:dataQualityDescription>
      <kms:dataQualityProcessor>ukendt</kms:dataQualityProcessor>
      <kms:dataQualityResponsibleParty>Kort- og Matrikelstyrelsen</kms:dataQualityResponsibleParty>
      <kms:timeOfPublication>1979-10-24T00:00:00</kms:timeOfPublication>
      <kms:esr_Ejendomsnummer>7510221746</kms:esr_Ejendomsnummer>
      <kms:harSamletFastEjendom>
        <kms:SFESamletFastEjendom>
          <kms:sfe_SagsID>7805483</kms:sfe_SagsID>
          <kms:sfe_Journalnummer>U1979/05273</kms:sfe_Journalnummer>
          <kms:sfe_Registreringsdato>1979-10-24</kms:sfe_Registreringsdato>
          <kms:sfe_Ejendomsnummer>4230388</kms:sfe_Ejendomsnummer>
        </kms:SFESamletFastEjendom>
      </kms:harSamletFastEjendom>
      <kms:kms_SagsID>7805483</kms:kms_SagsID>
      <kms:kms_Journalnummer>U1979/05273</kms:kms_Journalnummer>
      <kms:registreringsdato>1979-10-24</kms:registreringsdato>
      <kms:ejerlavsnavn>Vejlby By, Ellevang</kms:ejerlavsnavn>
      <kms:landsejerlavskode>1021151</kms:landsejerlavskode>
      <kms:matrikelnummer>26ez</kms:matrikelnummer>
      <kms:faelleslod>nej</kms:faelleslod>
      <kms:registreretAreal>28357</kms:registreretAreal>
      <kms:arealBeregn>o</kms:arealBeregn>
      <kms:vejAreal>0</kms:vejAreal>
      <kms:vejArealBeregn>b</kms:vejArealBeregn>
      <kms:vandArealBeregn>ukendt</kms:vandArealBeregn>
      <kms:regionskode>1082</kms:regionskode>
      <kms:regionsnavn>Region Midtjylland</kms:regionsnavn>
      <kms:kommunekode>0751</kms:kommunekode>
      <kms:kommunenavn>Århus Kommune</kms:kommunenavn>
      <kms:sognekode>9088</kms:sognekode>
      <kms:sognenavn>Ellevang</kms:sognenavn>
      <kms:retskredskode>1165</kms:retskredskode>
      <kms:retskredsnavn>Århus retskreds</kms:retskredsnavn>
      <kms:geometri>
        <gml:Polygon srsName=""urn:ogc:def:crs:EPSG::25832"">
          <gml:exterior>
            <gml:LinearRing>
              <gml:posList srsDimension=""2"" count=""11"">575494.081 6229485.745 575461.155 6229327.71 575477.623 6229325.037 575478.273 6229328.36 575573.284 6229308.212 575565.043 6229266.511 575574.746 6229259.649 575613.95 6229252.176 575618.961 6229275.824 575656.227 6229451.725 575494.081 6229485.745</gml:posList>
            </gml:LinearRing>
          </gml:exterior>
        </gml:Polygon>
      </kms:geometri>
    </kms:Jordstykke>
  </gml:featureMember>
</gml:FeatureCollection>
  </Output>
</QueryResult>", "100", "300", "575341.9065,6229152.176,433.569000000134")]
        public void PaddedBoundingBoxFromGeometry(string input, string padding, string minSize, string expected)
        {
            using (var sr = new StringReader(input))
            {
                var inputDocument = XDocument.Load(sr);
                var navigator = inputDocument.CreateNavigator();
                var manager = new XmlNamespaceManager(navigator.NameTable);
                manager.AddNamespace("gml", "http://www.opengis.net/gml");

                var polygon = navigator.Select("//gml:Polygon", manager);
                var result = XsltFunctions.PaddedBoundingBox(polygon, padding, minSize);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        [TestCase("1000.000,1000.000,1300.000,1300.000", "100", "300", "900,900,500")]
        [TestCase("1000.000,1000.000,1200.000,1200.000", "100", "300", "900,900,400")]
        [TestCase("1000.500,1000.000,1100.000,1100.000", "100", "300", "900.25,900,300")]
        [TestCase("1000.000,1000.000,1050.000,1050.000", "100", "300", "875,875,300")]
        [TestCase("1000.000,1000.000,1050.000,1300.000", "100", "300", "775,900,500")]
        [TestCase("1000.000,1000.000,1300.000,1050.000", "100", "300", "900,775,500")]
        [TestCase("1000.000,1000.000,1500.000,1500.000", "100", "300", "900,900,700")]
        public void PaddedBoundingBoxFromBoundingBox(string boundingBox, string padding, string minSize, string expected)
        {
            var result = XsltFunctions.PaddedBoundingBox(boundingBox, padding, minSize);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase(true, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Intermediate>
  <Input>
    <QueryInput xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
      <Subject>
        <Content>
          <CadastralDistrictIdentifier>960453</CadastralDistrictIdentifier>
          <LandParcelIdentifier>5e</LandParcelIdentifier>
          <MunicipalityCode>0706</MunicipalityCode>
          <MunicipalRealPropertyIdentifier>3107</MunicipalRealPropertyIdentifier>
          <FetchPrivate>false</FetchPrivate>
          <ReportElementKey>11111111-0000-0000-0000-000000000000</ReportElementKey>
          <RealProperty xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:oio:ebst:diadem:property"">
            <ParentMunicipalRealPropertyIdentifier>0</ParentMunicipalRealPropertyIdentifier>
            <MunicipalityCode>0706</MunicipalityCode>
            <MunicipalityName>Syddjurs Kommune</MunicipalityName>
            <AccessAddress>
              <Easting>592377.19</Easting>
              <Northing>6222393.33</Northing>
            </AccessAddress>
            <Lots>
              <Lot>
                <CadastralDistrictCode>960453</CadastralDistrictCode>
                <LandParcelIdentifier>5e</LandParcelIdentifier>
                <CadastralDistrictName>Fejrup By, Helgenæs</CadastralDistrictName>
                <LandParcelRegistrationAreaMeasure>40938</LandParcelRegistrationAreaMeasure>
                <AccessAddresses>
                  <AccessAddress>
                    <Easting>592377.19</Easting>
                    <Northing>6222393.33</Northing>
                  </AccessAddress>
                </AccessAddresses>
                <Buildings>
                  <Building>
                    <BuildingIdentifier>1 </BuildingIdentifier>
                    <BuildingReference>7818f028-efbe-46d1-be2a-146d2bbbad7f</BuildingReference>
                  </Building>
                  <Building>
                    <BuildingIdentifier>2 </BuildingIdentifier>
                    <BuildingReference>f409d133-1eb9-49fc-823d-dc31533775bd</BuildingReference>
                  </Building>
                </Buildings>
              </Lot>
            </Lots>
          </RealProperty>
          <ReportElementUniqueIdentifier>22222222-0000-0000-0000-000000000000</ReportElementUniqueIdentifier>
        </Content>
      </Subject>
      <QueryResultStep QueryResultStepIdentifier=""0"">
        <eks:matrikel xmlns:eks=""ekstern"">
          <eks:bbox>592099.181,6222311.913,592516.483,6222504.976</eks:bbox>
          <eks:matrikelPolygon>
            <gml:Polygon xmlns:gml=""http://www.opengis.net/gml"">
                  <gml:exterior>
                    <gml:LinearRing>
                      <gml:posList>592237.658 6222346.858 592355.328 6222338.367 592470.896 6222370.397 592464.749 6222389.332 592516.483 6222402.349 592514.379 6222409.132 592501.728 6222449.944 592491.982 6222481.41 592484.683 6222504.976 592101.933 6222391.749 592099.181 6222390.948 592111.749 6222332.746 592115.594 6222311.913 592237.658 6222346.858</gml:posList>
                    </gml:LinearRing>
                  </gml:exterior>
                </gml:Polygon>
         </eks:matrikelPolygon>
        </eks:matrikel>
      </QueryResultStep>
    </QueryInput>
  </Input>
  <Output>
    <wfs:FeatureCollection xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:wfs=""http://www.opengis.net/wfs"" xmlns:gml=""http://www.opengis.net/gml"" xmlns:dmp=""http://arealinformation.miljoeportal.dk/gis/services/distribution/MapServer/WFSServer"" xsi:schemaLocation=""http://arealinformation.miljoeportal.dk/gis/services/distribution/MapServer/WFSServer http://arealinformation.miljoeportal.dk/gis/services/distribution/MapServer/WFSServer?request=DescribeFeatureType%26version=1.1.0%26typename=NATURPERLER http://www.opengis.net/wfs http://schemas.opengis.net/wfs/1.1.0/wfs.xsd"">
      <gml:boundedBy>
        <gml:Envelope srsName=""urn:ogc:def:crs:EPSG:6.9:25832"">
          <gml:lowerCorner>445000.3339999998 6052657.0489000008</gml:lowerCorner>
          <gml:upperCorner>889604.06300000008 6380733.1898999996</gml:upperCorner>
        </gml:Envelope>
      </gml:boundedBy>
      <gml:featureMember>
        <dmp:NATURPERLER gml:id=""F26__1327"">
          <dmp:OBJECTID>1327</dmp:OBJECTID>
          <dmp:DB_IDENT>0</dmp:DB_IDENT>
          <dmp:ADM_KODE>5100</dmp:ADM_KODE>
          <dmp:ADM_TEKST>Staten</dmp:ADM_TEKST>
          <dmp:DATAREF/>
          <dmp:OBJEKTKODE>8230</dmp:OBJEKTKODE>
          <dmp:OBJEKTTKST>Naturperler</dmp:OBJEKTTKST>
          <dmp:SIGNATUR>0</dmp:SIGNATUR>
          <dmp:STATUS>Gældende</dmp:STATUS>
          <dmp:STATUSKODE>1</dmp:STATUSKODE>
          <dmp:OFFENTLIG>1</dmp:OFFENTLIG>
          <dmp:BEMERKNING/>
          <dmp:TEMAKODE1>0</dmp:TEMAKODE1>
          <dmp:TEMATEKST1/>
          <dmp:TEMAKODE2>0</dmp:TEMAKODE2>
          <dmp:TEMATEKST2/>
          <dmp:SHAPE>
            <gml:MultiSurface>
              <gml:surfaceMember>
                <gml:Polygon>
                  <gml:exterior>
                    <gml:LinearRing>
                      <gml:posList> 592279.68099999987 6222826.1019000001 592279.68099999987 6222850.2568999995 592267.87200000044 6222865.2138999999 592270.47800000012 6222888.6088999994 592276.97599999979 6222888.7389000002 592276.2089999998 6222896.4868999999 592275.12899999972 6222911.1039000005 592273.71899999958 6222920.3128999993 592297.73900000006 6222954.1959000006 592302.54600000009 6222957.7149 592336.67599999998 6222982.6599000003 592319.78799999971 6223017.0628999993 592307.03199999966 6223032.5599000007 592299.99799999967 6223039.1579 592296.17200000025 6223034.6289000008 592290.30900000036 6223029.1298999991 592286.8870000001 6223025.9209000003 592289.12999999989 6223018.6629000008 592289.12999999989 6223007.2749000005 592280.48900000006 6222998.6269000005 592278.51800000016 6222989.9879000001 592271.45100000035 6222970.3419000003 592260.0549999997 6222953.8358999994 592254.5549999997 6222929.0909000002 592249.94600000046 6222911.0239000004 592243.22599999979 6222907.2248999998 592235.47499999963 6222896.2869000006 592232.26699999999 6222890.8889000006 592228.62200000044 6222879.5809000004 592216.83899999969 6222847.7568999995 592202.30200000014 6222822.6218999997 592192.87700000033 6222807.6849000007 592182.26400000043 6222795.8979000002 592175.98099999968 6222781.3708999995 592175.98099999968 6222762.1149000004 592166.55599999987 6222743.2588999998 592162.62299999967 6222725.1819000002 592156.72699999996 6222710.6448999997 592155.54800000042 6222695.3278999999 592145.72699999996 6222678.8219000008 592126.08499999996 6222663.8948999997 592098.98099999968 6222644.6489000004 592081.29399999976 6222636.7898999993 592080.50999999978 6222632.0709000006 592084.04800000042 6222626.1819000002 592094.65199999977 6222612.0349000003 592096.62299999967 6222596.7179000005 592085.22699999996 6222569.9938999992 592085.22699999996 6222560.9659000002 592084.2620000001 6222555.5269000009 592093.86899999995 6222546.8189000003 592102.50999999978 6222533.8509 592108.40600000042 6222518.9239000008 592104.48099999968 6222510.6758999992 592114.30200000014 6222492.9899000004 592120.18900000025 6222482.7818999998 592122.16000000015 6222469.8138999995 592124.90600000042 6222454.4968999997 592131.18900000025 6222438.3808999993 592134.33899999969 6222431.3118999992 592132.76400000043 6222416.3848999999 592135.50999999978 6222408.5269000009 592135.12299999967 6222392.8099000007 592139.83899999969 6222388.4909000006 592149.66000000015 6222384.5618999992 592151.06199999992 6222379.4129000008 592156.36400000006 6222372.5638999995 592160 6222358.4068999998 592158.74700000044 6222353.5078999996 592157.55999999959 6222345.5699000005 592162.71300000045 6222340.4109000005 592164.98099999968 6222338.5109000001 592170.6540000001 6222324.9339000005 592175.01599999983 6222313.0259000007 592181.36500000022 6222303.5078999996 592184.14400000032 6222292.7898999993 592184.23500000034 6222284.8319000006 592186.13100000005 6222272.5438999999 592179.52699999977 6222259.9469000008 592219.26300000027 6222272.0349000003 592213.12000000011 6222286.0418999996 592212.20500000007 6222290.6908999998 592209.14599999972 6222306.2879000008 592212.96300000045 6222321.7338999994 592213.86199999973 6222325.5638999995 592215.94799999986 6222327.5528999995 592217.47400000039 6222331.6218999997 592219.28799999971 6222334.6318999995 592221.84399999958 6222338.8109000009 592241.02400000021 6222352.3179000001 592261.44900000002 6222351.2879000008 592309.1679999996 6222348.6888999995 592321.22300000023 6222357.0968999993 592324.12600000016 6222377.9028999992 592326.62399999984 6222379.3128999993 592334.93599999975 6222378.6129000001 592333.86400000006 6222392.9398999996 592333.05599999987 6222414.9848999996 592329.32100000046 6222416.8549000006 592328.21600000001 6222420.5839000009 592311.65799999982 6222423.1039000005 592307.1799999997 6222426.5329 592285.19699999969 6222444.2588999998 592287.34900000039 6222450.9579000007 592283.7620000001 6222450.7478999998 592277.60300000012 6222451.0679000001 592273.62000000011 6222456.0669 592271.63300000038 6222458.9958999995 592270.03299999982 6222462.7859000005 592268.68099999987 6222465.8549000006 592267.25399999972 6222469.0438999999 592265.29200000037 6222473.2138999999 592263.73300000001 6222476.4728999995 592260.13800000027 6222484.3808999993 592258.39800000004 6222494.0388999991 592256.9709999999 6222500.9879000001 592255.92399999965 6222505.6668999996 592254.89400000032 6222509.6359000001 592242.66500000004 6222586.5709000006 592241.85699999984 6222593.6188999992 592241.25499999989 6222597.9278999995 592240.63599999994 6222602.2568999995 592239.95999999996 6222606.5658999998 592239.25899999961 6222610.8659000006 592238.62399999984 6222614.3048999999 592236.49700000044 6222618.1739000008 592235.87899999972 6222634.2708999999 592204.94000000041 6222628.7619000003 592188.07699999958 6222633.7808999997 592179.41899999976 6222653.3769000005 592186.25499999989 6222666.1338999998 592204.03299999982 6222675.7118999995 592222.25600000005 6222686.6399000008 592236.84300000034 6222689.3798999991 592249.14599999972 6222684.8209000006 592260.92899999954 6222674.7529000007 592264.18699999992 6222679.3519000001 592272.39099999983 6222713.0748999994 592261.4570000004 6222719.9129000008 592247.97499999963 6222732.6609000005 592244.59399999958 6222739.0489000008 592244.13300000038 6222751.3568999991 592252.33700000029 6222760.0149000008 592267.37799999956 6222763.2049000002 592260.28600000031 6222774.3818999995 592249.56699999981 6222792.8488999996 592247.78600000031 6222795.8278999999 592248.23900000006 6222799.2169000003 592253.70600000024 6222805.1359000001 592260.08800000045 6222796.4779000003 592266.92399999965 6222801.9469000008 592279.68099999987 6222826.1019000001</gml:posList>
                    </gml:LinearRing>
                  </gml:exterior>
                </gml:Polygon>
              </gml:surfaceMember>
            </gml:MultiSurface>
          </dmp:SHAPE>
          <dmp:SHAPE.STArea___>73386.405876580044</dmp:SHAPE.STArea___>
          <dmp:SHAPE.STLength__>2103.1509900507122</dmp:SHAPE.STLength__>
        </dmp:NATURPERLER>
      </gml:featureMember>
    </wfs:FeatureCollection>
  </Output>
</Intermediate>")]
        public void ConflictTest(bool expected, string features)
        {
            XPathNodeIterator matrikelIt;
            XPathNodeIterator featuresIt;
            using (var sr = new StringReader(features))
            {
                var matrikelDocument = XDocument.Load(sr);
                var navigator = matrikelDocument.CreateNavigator();
                var manager = new XmlNamespaceManager(navigator.NameTable);
                manager.AddNamespace("eks", "ekstern");
                manager.AddNamespace("dmp", "http://arealinformation.miljoeportal.dk/gis/services/distribution/MapServer/WFSServer");

                matrikelIt = navigator.Select("//eks:matrikelPolygon", manager);

                featuresIt = navigator.Select("//Output//dmp:SHAPE", manager);

                var actual = XsltFunctions.Conflicts(matrikelIt, featuresIt, "0");
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [TestCase("02-01-2001", "2001-02-01")]
        [TestCase("20061231", "2006-12-31")]
        [TestCase("2007-04-16-13.48.10.1", "2007-04-16")]
        [TestCase("2007-04-16-13.48.10.14", "2007-04-16")]
        [TestCase("2007-04-16-13.48.10.140", "2007-04-16")]
        [TestCase("2007-04-16-13.48.10.1400", "2007-04-16")]
        [TestCase("02-01-2001Z", "2001-02-01Z")]
        [TestCase("2011-08-19T13:33:34.76+02:00", "2011-08-19")]
        [TestCase("2011-10-26T10:14:51.478+02:00", "2011-10-26")]
        [TestCase("02-01-2001 00:00:00", "2001-02-01")]
        [TestCase("02-01-2001 00:00:00Z", "2001-02-01Z")]
        [TestCase("2011-06-02T16:03:57.6230000+02:00", "2011-06-02")]
        [TestCase("2001", "2001-01-01")]
        [TestCase("21.05.2012", "2012-05-21")]
        [TestCase("09/20/2012 13:21:51", "2012-09-20")]
        public void ConvertDateTest(string input, string expected)
        {
            var actual = XsltFunctions.ConvertDate(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertUnsupportedTest()
        {
            const string input = "01/25/41 1-1-2011";

            Assert.That(XsltFunctions.IsDateTime(input), Is.False);

            var actual = XsltFunctions.ConvertDate(input);
            Assert.That(actual, Is.EqualTo(input));

            actual = XsltFunctions.ConvertDateTime(input);
            Assert.That(actual, Is.EqualTo(input));

            actual = XsltFunctions.ConvertYear(input);
            Assert.That(actual, Is.EqualTo(input));
        }

        [TestCase("02-01-2001", "2001-02-01T00:00:00")]
        [TestCase("02-01-2001Z", "2001-02-01T00:00:00Z")]
        [TestCase("20061231", "2006-12-31T00:00:00")]
        [TestCase("2007-04-16-13.48.10.1", "2007-04-16T13:48:10")]
        [TestCase("2007-04-16-13.48.10.14", "2007-04-16T13:48:10")]
        [TestCase("2007-04-16-13.48.10.140", "2007-04-16T13:48:10")]
        [TestCase("2007-04-16-13.48.10.1400", "2007-04-16T13:48:10")]
        [TestCase("2011-08-19T13:33:34.76+02:00", "2011-08-19T13:33:34")]
        [TestCase("2011-10-26T10:14:51.478+02:00", "2011-10-26T10:14:51")]
        [TestCase("02-01-2001 00:00:00", "2001-02-01T00:00:00")]
        [TestCase("02-01-2001 00:00:00Z", "2001-02-01T00:00:00Z")]
        [TestCase("02-01-2001 13:41:23", "2001-02-01T13:41:23")]
        [TestCase("02-01-2001 13:41:23Z", "2001-02-01T13:41:23Z")]
        [TestCase("2011-06-02T16:03:57.6230000+02:00", "2011-06-02T16:03:57")]
        [TestCase("2001", "2001-01-01T00:00:00")]
        [TestCase("21.05.2012", "2012-05-21T00:00:00")]
        [TestCase("09/20/2012 13:21:51", "2012-09-20T13:21:51")]
        public void ConvertDateTimeTest(string input, string expected)
        {
            var actual = XsltFunctions.ConvertDateTime(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("02-01-2001", "2001")]
        [TestCase("02-01-2001Z", "2001")]
        [TestCase("20061231", "2006")]
        [TestCase("2007-04-16-13.48.10.1", "2007")]
        [TestCase("2007-04-16-13.48.10.14", "2007")]
        [TestCase("2007-04-16-13.48.10.140", "2007")]
        [TestCase("2007-04-16-13.48.10.1400", "2007")]
        [TestCase("2011-08-19T13:33:34.76+02:00", "2011")]
        [TestCase("2011-10-26T10:14:51.478+02:00", "2011")]
        [TestCase("02-01-2001 00:00:00", "2001")]
        [TestCase("02-01-2001 00:00:00Z", "2001")]
        [TestCase("02-01-2001 13:41:23", "2001")]
        [TestCase("02-01-2001 13:41:23Z", "2001")]
        [TestCase("2011-06-02T16:03:57.6230000+02:00", "2011")]
        [TestCase("2001", "2001")]
        [TestCase("21.05.2012", "2012")]
        [TestCase("09/20/2012 00:00:00", "2012")]
        public void ConvertYearTest(string input, string expected)
        {
            var actual = XsltFunctions.ConvertYear(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("02-01-2001")]
        [TestCase("2011-06-02T16:03:57.6230000+02:00")]
        [TestCase("02-01-2001Z")]
        [TestCase("20061231")]
        [TestCase("2007-04-16-13.48.10.1")]
        [TestCase("2007-04-16-13.48.10.14")]
        [TestCase("2007-04-16-13.48.10.140")]
        [TestCase("2007-04-16-13.48.10.1400")]
        [TestCase("2011-08-19T13:33:34.76+02:00")]
        [TestCase("2011-10-26T10:14:51.478+02:00")]
        [TestCase("02-01-2001 00:00:00")]
        [TestCase("02-01-2001 00:00:00Z")]
        [TestCase("02-01-2001 13:41:23")]
        [TestCase("02-01-2001 13:41:23Z")]
        [TestCase("2001")]
        [TestCase("21.05.2012")]
        [TestCase("09/20/2012 00:00:00")]
        public void IsDateTimeTest(string input)
        {
            var result = XsltFunctions.IsDateTime(input);
            Assert.That(result, Is.True, input + " was not a date time.");
        }

        [TestCase("02-01-2001", "01-02-2001", "1. februar 2001", "1/2 2001")]
        [TestCase("2011-06-02T16:03:57.6230000+02:00", "02-06-2011", "2. juni 2011", "2/6 2011")]
        [TestCase("02-01-2001Z", "01-02-2001", "1. februar 2001", "1/2 2001")]
        [TestCase("20061231", "31-12-2006", "31. december 2006", "31/12 2006")]
        [TestCase("2007-04-16-13.48.10.1", "16-04-2007", "16. april 2007", "16/4 2007")]
        [TestCase("2007-04-16-13.48.10.14", "16-04-2007", "16. april 2007", "16/4 2007")]
        [TestCase("2007-04-16-13.48.10.140", "16-04-2007", "16. april 2007", "16/4 2007")]
        [TestCase("2007-04-16-13.48.10.1400", "16-04-2007", "16. april 2007", "16/4 2007")]
        [TestCase("2011-08-19T13:33:34.76+02:00", "19-08-2011", "19. august 2011", "19/8 2011")]
        [TestCase("2011-10-26T10:14:51.478+02:00", "26-10-2011", "26. oktober 2011", "26/10 2011")]
        [TestCase("02-01-2001 00:00:00", "01-02-2001", "1. februar 2001", "1/2 2001")]
        [TestCase("02-01-2001 00:00:00Z", "01-02-2001", "1. februar 2001", "1/2 2001")]
        [TestCase("02-01-2001 13:41:23", "01-02-2001", "1. februar 2001", "1/2 2001")]
        [TestCase("02-01-2001 13:41:23Z", "01-02-2001", "1. februar 2001", "1/2 2001")]
        [TestCase("2001", "01-01-2001", "1. januar 2001", "1/1 2001")]
        [TestCase("21.05.2012", "21-05-2012", "21. maj 2012", "21/5 2012")]
        [TestCase("09/20/2012 00:00:00", "20-09-2012", "20. september 2012", "20/9 2012")]
        [SetCulture("da-DK")]
        public void FormatDateTimeTest(string input, string expected1, string expected2, string expected3)
        {
            var result = XsltFunctions.FormatDateTime(input, "dd-mm-åååå");
            Assert.That(result, Is.EqualTo(expected1));

            result = XsltFunctions.FormatDateTime(input, "d. mmmm åååå");
            Assert.That(result, Is.EqualTo(expected2));

            result = XsltFunctions.FormatDateTime(input, "d/m åååå");
            Assert.That(result, Is.EqualTo(expected3));
        }

        [TestCase("123.45", "123", "123,45", "123,45", "123,45")]
        [TestCase("-123.45", "-123", "-123,45", "-123,45", "-123,45")]
        [TestCase("0", "0", "0", "0", "0")]
        [TestCase("123456.789", "123.457", "123.456,79", "123456,79", "123.456,79")]
        [TestCase("1234567.89", "1.234.568", "1.234.567,89", "1234567,89", "1.234.567,89")]
        [TestCase("123456789.0", "123.456.789", "123.456.789", "123456789", "123.456.789,00")]
        [TestCase("123456789.1", "123.456.789", "123.456.789,1", "123456789,1", "123.456.789,10")]
        public void FormatDoubleTest(string input, string expected1, string expected2, string expected3, string expected4)
        {
            var actual = XsltFunctions.FormatDouble(input, "###.###");
            Assert.That(actual, Is.EqualTo(expected1));

            actual = XsltFunctions.FormatDouble(input, "###.###,##");
            Assert.That(actual, Is.EqualTo(expected2));

            actual = XsltFunctions.FormatDouble(input, "###,##");
            Assert.That(actual, Is.EqualTo(expected3));

            actual = XsltFunctions.FormatDouble(input, "###.###,00");
            Assert.That(actual, Is.EqualTo(expected4));
        }

        [Test]
        public void FormatDoubleNanTest()
        {
            var actual = XsltFunctions.FormatDouble("NaN", "###.###,##");
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        [Test]
        [Combinatorial]
        public void DoesNodesContainSameValueFalseTest([Values(1, 2, 3)]int first, [Values(1, 2, 3)]int second, [Values(1, 2, 3)]int third)
        {
            var result = first == second && first == third;
            RunDoesNodesContainSameValue(new[] { first, second, third }, result);
        }

        [Test]
        public void DoesNodesContainSameValueOneNodeTest()
        {
            RunDoesNodesContainSameValue(new[] { 1 }, true);
        }
        
        [Test]
        public void DoesNodesContainSameValueZeroNodeTest()
        {
            RunDoesNodesContainSameValue(new int[] { }, true);
        }

        [Test]
        public void DoesNodesContainSameValueNullTest()
        {
            var result = XsltFunctions.DoesNodesContainSameValue(null);

            Assert.That(result, Is.EqualTo(false));
        }

        [TestCase("E7D7A866-3D8C-4463-BB96-13C3C883FCC1", "E7D7A866-3D8C-4463-BB96-13C3C883FCC1", true)]
        [TestCase("E7D7A866-3D8C-4463-BB96-13C3C883FCC1", "e7D7a866-3d8c-4463-bb96-13c3c883fcc1", true)]
        [TestCase("E7D7A866-3D8C-4463-BB96-13C3C883FCC1", "e7D7a866-3d8c-4463-bb96-13c3c883fcc2", false)]
        public void CompareStringTest(string first, string second, bool expectedResult)
        {
            var result = XsltFunctions.CompareString(first, second);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private void RunDoesNodesContainSameValue(IEnumerable<int> nodeValues, bool expectedResult)
        {
            var nodes = CreateXPathNodeIterator(CreateXmlStringForCompareTest(nodeValues), "/Root/Node");

            var result = XsltFunctions.DoesNodesContainSameValue(nodes);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private XPathNodeIterator CreateXPathNodeIterator(string xmlString, string selectElement)
        {
            var document = CreateXpathDocument(xmlString);
            var navigator = document.CreateNavigator();

            var nodes = navigator.Select(selectElement);

            return nodes;
        }

        private XPathDocument CreateXpathDocument(string xmlString)
        {
            using (var stream = new StringReader(xmlString))
            {
               return new XPathDocument(stream);
            }
        }

        private string CreateXmlStringForCompareTest(IEnumerable<int> values)
        {
            var xml = new XElement("Root");

            foreach (var value in values)
            {
               xml.Add(new XElement("Node", value));
            }
                
            return xml.ToString();
        }
    }
}
