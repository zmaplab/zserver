using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZMap.Infrastructure;
using ZMap.Ogc.Wmts;

namespace ZServer.Tests;

[Collection("WebApplication collection")]
public class WmtsTests(WebApplicationFactoryFixture fixture)
{
    [Fact]
    public void LoadCapabilities()
    {
        var capabilities = Capabilities.LoadFromXml(CapabilitiesXml);
        Assert.Equal(2, capabilities.Contents.TileMatrixSet.TileMatrices.Length);
    }

    [Fact(DisplayName = "Get cd tile")]
    public async Task GetCdTile()
    {
        var service = fixture.Instance.Services.GetRequiredService<StoreRefreshService>();
        await service.StartAsync(default);
        var httpClient = fixture.Instance.CreateClient();
// TileMatrix=16&TileCol=54894&TileRow=10944
        var result =
            await httpClient.GetAsync(
                "WMTS?layer=cd&tileMatrixSet=EPSG:4326&tileMatrix=16&tileRow=10793&tileCol=51699");
        var image = await result.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("images/cd.png", image);
        var hash1 = CryptographyUtility.ComputeHash(image);
        var bytes = await File.ReadAllBytesAsync("images/cd.png");
        var hash2 = CryptographyUtility.ComputeHash(bytes);
        Assert.Equal(hash2, hash1);
    }

    [Fact(DisplayName = "Get tianditu c tile")]
    public async Task GetRemoteWmts1Tile()
    {
        var service = fixture.Instance.Services.GetRequiredService<StoreRefreshService>();
        await service.StartAsync(default);
        var httpClient = fixture.Instance.CreateClient();
// TileMatrix=16&TileCol=54894&TileRow=10944
        var result =
            await httpClient.GetAsync(
                "WMTS?layer=tianditu_c&tileMatrixSet=EPSG:4326&tileMatrix=16&tileRow=10944&tileCol=54894");
        var image = await result.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("images/tianditu_c.png", image);
        var hash1 = CryptographyUtility.ComputeHash(image);
        var bytes = await File.ReadAllBytesAsync("images/tianditu_c.png");
        var hash2 = CryptographyUtility.ComputeHash(bytes);
        Assert.Equal(hash2, hash1);
    }

    [Fact(DisplayName = "Get tianditu w tile")]
    public async Task GetRemoteWmts2Tile()
    {
        var service = fixture.Instance.Services.GetRequiredService<StoreRefreshService>();
        await service.StartAsync(default);
        var httpClient = fixture.Instance.CreateClient();

        // 后端要请求 TILEMATRIX=16&TILEROW=27063&TILECOL=54894
        var result =
            await httpClient.GetAsync(
                "WMTS?layer=tianditu_w&tileMatrixSet=EPSG:4326&tileMatrix=16&tileRow=10944&tileCol=54894");
        var image = await result.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("images/tianditu_w.png", image);
        var hash1 = CryptographyUtility.ComputeHash(image);
        var bytes = await File.ReadAllBytesAsync("images/tianditu_w.png");
        var hash2 = CryptographyUtility.ComputeHash(bytes);
        Assert.Equal(hash2, hash1);
    }

    private const string CapabilitiesXml = """
                                           <?xml version="1.0" encoding="UTF-8"?>
                                           <Capabilities
                                               xsi:schemaLocation="http://www.opengis.net/wmts/1.0 http://schemas.opengis.net/wmts/1.0.0/wmtsGetCapabilities_response.xsd"
                                               version="1.0.0" xmlns="http://www.opengis.net/wmts/1.0"
                                               xmlns:ows="http://www.opengis.net/ows/1.1" xmlns:gml="http://www.opengis.net/gml"
                                               xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xlink="http://www.w3.org/1999/xlink">
                                               <ows:ServiceIdentification>
                                                   <ows:Title>
                                                       在线地图服务
                                                   </ows:Title>
                                                   <ows:Abstract>
                                                       基于OGC标准的地图服务
                                                   </ows:Abstract>
                                                   <ows:Keywords>
                                                       <ows:Keyword>
                                                           OGC
                                                       </ows:Keyword>
                                                   </ows:Keywords>
                                                   <ows:ServiceType codeSpace="wmts" />
                                                   <ows:ServiceTypeVersion>
                                                       1.0.0
                                                   </ows:ServiceTypeVersion>
                                                   <ows:Fees>
                                                       none
                                                   </ows:Fees>
                                                   <ows:AccessConstraints>
                                                       none
                                                   </ows:AccessConstraints>
                                               </ows:ServiceIdentification>
                                               <ows:ServiceProvider>
                                                   <ows:ProviderName>
                                                       国家基础地理信息中心
                                                   </ows:ProviderName>
                                                   <ows:ProviderSite>
                                                       http://www.tianditu.gov.cn
                                                   </ows:ProviderSite>
                                                   <ows:ServiceContact>
                                                       <ows:IndividualName>
                                                           Mr Wang
                                                       </ows:IndividualName>
                                                       <ows:PositionName>
                                                           Software Engineer
                                                       </ows:PositionName>
                                                       <ows:ContactInfo>
                                                           <ows:Phone>
                                                               <ows:Voice>
                                                                   010-63881203
                                                               </ows:Voice>
                                                               <ows:Facsimile>
                                                                   010-63881203
                                                               </ows:Facsimile>
                                                           </ows:Phone>
                                                           <ows:Address>
                                                               <ows:DeliveryPoint>
                                                                   北京市海淀区莲花池西路28号
                                                               </ows:DeliveryPoint>
                                                               <ows:City>
                                                                   北京市
                                                               </ows:City>
                                                               <ows:AdministrativeArea>
                                                                   北京市
                                                               </ows:AdministrativeArea>
                                                               <ows:Country>
                                                                   中国
                                                               </ows:Country>
                                                               <ows:PostalCode>
                                                                   100830
                                                               </ows:PostalCode>
                                                               <ows:ElectronicMailAddress>
                                                                   tdt@ngcc.cn
                                                               </ows:ElectronicMailAddress>
                                                           </ows:Address>
                                                           <ows:OnlineResource xlink:type="simple" xlink:href="http://www.tianditu.gov.cn" />
                                                       </ows:ContactInfo>
                                                   </ows:ServiceContact>
                                               </ows:ServiceProvider>
                                               <ows:OperationsMetadata>
                                                   <ows:Operation name="GetCapabilities">
                                                       <ows:DCP>
                                                           <ows:HTTP>
                                                               <ows:Get xlink:href="http://t0.tianditu.gov.cn/vec_w/wmts?">
                                                                   <ows:Constraint name="GetEncoding">
                                                                       <ows:AllowedValues>
                                                                           <ows:Value>
                                                                               KVP
                                                                           </ows:Value>
                                                                       </ows:AllowedValues>
                                                                   </ows:Constraint>
                                                               </ows:Get>
                                                           </ows:HTTP>
                                                       </ows:DCP>
                                                   </ows:Operation>
                                                   <ows:Operation name="GetTile">
                                                       <ows:DCP>
                                                           <ows:HTTP>
                                                               <ows:Get xlink:href="http://t0.tianditu.gov.cn/vec_w/wmts?">
                                                                   <ows:Constraint name="GetEncoding">
                                                                       <ows:AllowedValues>
                                                                           <ows:Value>
                                                                               KVP
                                                                           </ows:Value>
                                                                       </ows:AllowedValues>
                                                                   </ows:Constraint>
                                                               </ows:Get>
                                                           </ows:HTTP>
                                                       </ows:DCP>
                                                   </ows:Operation>
                                               </ows:OperationsMetadata>
                                               <Contents>
                                                   <Layer>
                                                       <ows:Title>
                                                           vec
                                                       </ows:Title>
                                                       <ows:Abstract>
                                                           vec
                                                       </ows:Abstract>
                                                       <ows:Identifier>
                                                           vec
                                                       </ows:Identifier>
                                                       <ows:WGS84BoundingBox>
                                                           <ows:LowerCorner>
                                                               -20037508.3427892 -20037508.3427892
                                                           </ows:LowerCorner>
                                                           <ows:UpperCorner>
                                                               20037508.3427892 20037508.3427892
                                                           </ows:UpperCorner>
                                                       </ows:WGS84BoundingBox>
                                                       <ows:BoundingBox>
                                                           <ows:LowerCorner>
                                                               -20037508.3427892 -20037508.3427892
                                                           </ows:LowerCorner>
                                                           <ows:UpperCorner>
                                                               20037508.3427892 20037508.3427892
                                                           </ows:UpperCorner>
                                                       </ows:BoundingBox>
                                                       <Style>
                                                           <ows:Identifier>
                                                               default
                                                           </ows:Identifier>
                                                       </Style>
                                                       <Format>
                                                           tiles
                                                       </Format>
                                                       <TileMatrixSetLink>
                                                           <TileMatrixSet>
                                                               w
                                                           </TileMatrixSet>
                                                       </TileMatrixSetLink>
                                                   </Layer>
                                                   <TileMatrixSet>
                                                       <ows:Identifier>
                                                           w
                                                       </ows:Identifier>
                                                       <ows:SupportedCRS>
                                                           urn:ogc:def:crs:EPSG::900913
                                                       </ows:SupportedCRS>
                                                       <TileMatrix>
                                                           <ows:Identifier>
                                                               1
                                                           </ows:Identifier>
                                                           <ScaleDenominator>
                                                               2.958293554545656E8
                                                           </ScaleDenominator>
                                                           <TopLeftCorner>
                                                               20037508.3427892 -20037508.3427892
                                                           </TopLeftCorner>
                                                           <TileWidth>
                                                               256
                                                           </TileWidth>
                                                           <TileHeight>
                                                               256
                                                           </TileHeight>
                                                           <MatrixWidth>
                                                               2
                                                           </MatrixWidth>
                                                           <MatrixHeight>
                                                               2
                                                           </MatrixHeight>
                                                       </TileMatrix>
                                                       <TileMatrix>
                                                           <ows:Identifier>
                                                               2
                                                           </ows:Identifier>
                                                           <ScaleDenominator>
                                                               1.479146777272828E8
                                                           </ScaleDenominator>
                                                           <TopLeftCorner>
                                                               20037508.3427892 -20037508.3427892
                                                           </TopLeftCorner>
                                                           <TileWidth>
                                                               256
                                                           </TileWidth>
                                                           <TileHeight>
                                                               256
                                                           </TileHeight>
                                                           <MatrixWidth>
                                                               4
                                                           </MatrixWidth>
                                                           <MatrixHeight>
                                                               4
                                                           </MatrixHeight>
                                                       </TileMatrix>        
                                                   </TileMatrixSet>
                                               </Contents>
                                           </Capabilities>
                                           """;
}