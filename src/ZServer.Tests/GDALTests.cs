// using System.Threading.Tasks;
// using OSGeo.GDAL;
// using Xunit;
// using ZMap.Source.GDAL;
// using NetTopologySuite.Geometries;
// using System.Linq;
//
// namespace ZServer.Tests
// {
//     public class GDALTests
//     {
//         [Fact]
//         public void ReadTiffMetadata()
//         {
//             Gdal.AllRegister();
//             const string path = "images/world_raster_mod.tif";
//             using var raw = Gdal.OpenShared(path, Access.GA_ReadOnly);
//             using var wrap = new GDALSource(path);
//             const double minx = -179.914180331;
//             const double maxx = 156.513316275;
//             const double miny = -90.208959114;
//             const double maxy = 89.779751571;
//
//             Assert.Equal(raw.GetProjectionRef(), wrap.Metadata.Projection);
//             Assert.Equal(raw.RasterXSize, wrap.Metadata.Width);
//             Assert.Equal(raw.RasterYSize, wrap.Metadata.Height);
//             Assert.Equal(raw.RasterCount, wrap.Metadata.Bands);
//
//             Assert.Equal(minx, wrap.Metadata.Extent.MinX, 5);
//             Assert.Equal(miny, wrap.Metadata.Extent.MinY, 5);
//             Assert.Equal(maxx, wrap.Metadata.Extent.MaxX, 5);
//             Assert.Equal(maxy, wrap.Metadata.Extent.MaxY, 5);
//
//             Assert.Equal(path, wrap.File);
//         }
//
//         [Fact]
//         public async Task ReadTiff()
//         {
//             Gdal.AllRegister();
//             const string path = "images/world_raster_mod.tif";
//             using var wrap = new GDALSource(path);
//             var extent = new Envelope(144.99, 147.35, -41.70, -44.02);
//             var imageBytes = await wrap.GetImageInExtentAsync(extent);
//
//             Assert.Equal(4 * 4, imageBytes.Length);
//
//             Assert.Equal(165, imageBytes[0]);
//             Assert.Equal(170, imageBytes[1]);
//             Assert.Equal(170, imageBytes[2]);
//             Assert.Equal(160, imageBytes[3]);
//
//             Assert.Equal(160, imageBytes[4]);
//             Assert.Equal(158, imageBytes[5]);
//             Assert.Equal(158, imageBytes[6]);
//             Assert.Equal(154, imageBytes[7]);
//
//             Assert.Equal(169, imageBytes[8]);
//             Assert.Equal(164, imageBytes[9]);
//             Assert.Equal(154, imageBytes[10]);
//             Assert.Equal(154, imageBytes[11]);
//
//             Assert.Equal(188, imageBytes[12]);
//             Assert.Equal(168, imageBytes[13]);
//             Assert.Equal(155, imageBytes[14]);
//             Assert.Equal(155, imageBytes[15]);
//
//
//             var imageBytesFull = await wrap.GetImageInExtentAsync(wrap.Metadata.Extent);
//             Assert.Equal(600 * 321, imageBytesFull.Length);
//             Assert.Equal(255, imageBytesFull[0]);
//             Assert.Equal(106, imageBytesFull.Last());
//         }
//
//
//         // private Envelope GetExtent(Dataset dataSet)
//         // {
//         //     if (dataSet == null)
//         //         return null;
//         //
//         //     // no rotation...use default transform
//         //     var geoTransform = new GeoTransform(dataSet);
//         //     if (geoTransform.IsIdentity)
//         //         geoTransform = new GeoTransform(-0.5, dataSet.RasterYSize + 0.5);
//         //
//         //     // image pixels
//         //     double dblW = dataSet.RasterXSize;
//         //     double dblH = dataSet.RasterYSize;
//         //
//         //     double left = geoTransform.EnvelopeLeft(dblW, dblH);
//         //     double right = geoTransform.EnvelopeRight(dblW, dblH);
//         //     double top = geoTransform.EnvelopeTop(dblW, dblH);
//         //     double bottom = geoTransform.EnvelopeBottom(dblW, dblH);
//         //
//         //     // Hack to set number of bands
//         //     if (Bands == 0)
//         //         Bands = dataSet.RasterCount;
//         //
//         //     return new Envelope(left, right, bottom, top);
//         // }
//     }
// }