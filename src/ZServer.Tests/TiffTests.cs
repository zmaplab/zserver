// using System;
// using System.Drawing;
// using System.IO;
// using BitMiracle.LibTiff.Classic;
// using SkiaSharp;
// using Xunit;
// using TiffTag = BitMiracle.LibTiff.Classic.TiffTag;
//
// namespace ZServer.Tests
// {
//     public class TaggedImageHeader
//     {
//         public int BitsPerPixel { get; private set; }
//         public int Components { get; private set; }
//         public string Compression { get; private set; }
//         public int ImageHeight { get; private set; }
//         public int ImageWidth { get; private set; }
//         public string PlanarConfig { get; private set; }
//         public int TileHeight { get; private set; }
//         public int TileWidth { get; private set; }
//
//         internal static TaggedImageHeader Read(Tiff tiff)
//         {
//             var imageWidth = tiff.GetField(TiffTag.IMAGEWIDTH);
//             var imageHeight = tiff.GetField(TiffTag.IMAGELENGTH);
//             var bitsPerPixel = tiff.GetField(TiffTag.BITSPERSAMPLE);
//             var components = tiff.GetField(TiffTag.SAMPLESPERPIXEL);
//             var tileWidth = tiff.GetField(TiffTag.TILEWIDTH);
//             var tileHeight = tiff.GetField(TiffTag.TILELENGTH);
//             var compression = tiff.GetField(TiffTag.COMPRESSION);
//             var planarConfig = tiff.GetField(TiffTag.PLANARCONFIG);
//
//             return new TaggedImageHeader
//             {
//                 ImageWidth = imageWidth?[0].ToInt() ?? 0,
//                 ImageHeight = imageHeight?[0].ToInt() ?? 0,
//                 BitsPerPixel = bitsPerPixel?[0].ToInt() ?? 0,
//                 Components = components?[0].ToInt() ?? 0,
//                 TileWidth = tileWidth?[0].ToInt() ?? 0,
//                 TileHeight = tileHeight?[0].ToInt() ?? 0,
//                 Compression = compression?[0].ToString() ?? "",
//                 PlanarConfig = planarConfig?[0].ToString() ?? ""
//             };
//         }
//     }
//
//     public class TiffTests
//     {
//         [Fact]
//         public void ReadTiff()
//         {
//             if (File.Exists("/Users/lewis/Downloads/yx/yx.tif"))
//             {
//                 var bitmap = OpenTiff();
//
//                 File.WriteAllBytes("test.bmp", bitmap.Bytes);
//             }
//         }
//
//         public static SKBitmap OpenTiff()
//         {
//            private void DrawGraphics(Graphics g, Extent envelope, Rectangle window)
//         {
//             // Gets the scaling factor for converting from geographic to pixel coordinates
//             double dx = window.Width / envelope.Width;
//             double dy = window.Height / envelope.Height;
//
//             double[] a = Bounds.AffineCoefficients;
//
//             // calculate inverse
//             double p = 1 / ((a[1] * a[5]) - (a[2] * a[4]));
//             double[] aInv = new double[4];
//             aInv[0] = a[5] * p;
//             aInv[1] = -a[2] * p;
//             aInv[2] = -a[4] * p;
//             aInv[3] = a[1] * p;
//
//             // estimate rectangle coordinates
//             double tlx = ((envelope.MinX - a[0]) * aInv[0]) + ((envelope.MaxY - a[3]) * aInv[1]);
//             double tly = ((envelope.MinX - a[0]) * aInv[2]) + ((envelope.MaxY - a[3]) * aInv[3]);
//             double trx = ((envelope.MaxX - a[0]) * aInv[0]) + ((envelope.MaxY - a[3]) * aInv[1]);
//             double trY = ((envelope.MaxX - a[0]) * aInv[2]) + ((envelope.MaxY - a[3]) * aInv[3]);
//             double blx = ((envelope.MinX - a[0]) * aInv[0]) + ((envelope.MinY - a[3]) * aInv[1]);
//             double bly = ((envelope.MinX - a[0]) * aInv[2]) + ((envelope.MinY - a[3]) * aInv[3]);
//             double brx = ((envelope.MaxX - a[0]) * aInv[0]) + ((envelope.MinY - a[3]) * aInv[1]);
//             double bry = ((envelope.MaxX - a[0]) * aInv[2]) + ((envelope.MinY - a[3]) * aInv[3]);
//
//             // get absolute maximum and minimum coordinates to make a rectangle on projected coordinates
//             // that overlaps all the visible area.
//             double tLx = Math.Min(Math.Min(Math.Min(tlx, trx), blx), brx);
//             double tLy = Math.Min(Math.Min(Math.Min(tly, trY), bly), bry);
//             double bRx = Math.Max(Math.Max(Math.Max(tlx, trx), blx), brx);
//             double bRy = Math.Max(Math.Max(Math.Max(tly, trY), bly), bry);
//
//             // limit it to the available image
//             // todo: why we compare NumColumns\Rows and X,Y coordinates??
//             if (tLx > Bounds.NumColumns) tLx = Bounds.NumColumns;
//             if (tLy > Bounds.NumRows) tLy = Bounds.NumRows;
//             if (bRx > Bounds.NumColumns) bRx = Bounds.NumColumns;
//             if (bRy > Bounds.NumRows) bRy = Bounds.NumRows;
//
//             if (tLx < 0) tLx = 0;
//             if (tLy < 0) tLy = 0;
//             if (bRx < 0) bRx = 0;
//             if (bRy < 0) bRy = 0;
//
//             // gets the affine scaling factors.
//             float m11 = Convert.ToSingle(a[1] * dx);
//             float m22 = Convert.ToSingle(a[5] * -dy);
//             float m21 = Convert.ToSingle(a[2] * dx);
//             float m12 = Convert.ToSingle(a[4] * -dy);
//             double l = a[0] - (.5 * (a[1] + a[2])); // Left of top left pixel
//             double t = a[3] - (.5 * (a[4] + a[5])); // top of top left pixel
//             float xShift = (float)((l - envelope.MinX) * dx);
//             float yShift = (float)((envelope.MaxY - t) * dy);
//             g.PixelOffsetMode = PixelOffsetMode.Half;
//
//             if (m11 > 1 || m22 > 1)
//             {
//                 // out of pyramids
//                 g.InterpolationMode = InterpolationMode.NearestNeighbor;
//                 _overview = -1; // don't use overviews when zooming behind the max res.
//             }
//             else
//             {
//                 // estimate the pyramids that we need.
//                 // when using unreferenced images m11 or m22 can be negative resulting on inf logarithm.
//                 // so the Math.abs
//                 _overview = (int)Math.Min(Math.Log(Math.Abs(1 / m11), 2), Math.Log(Math.Abs(1 / m22), 2));
//
//                 // limit it to the available pyramids
//                 _overview = Math.Min(_overview, _red.GetOverviewCount() - 1);
//
//                 // additional test but probably not needed
//                 if (_overview < 0)
//                 {
//                     _overview = -1;
//                 }
//             }
//
//             var overviewPow = Math.Pow(2, _overview + 1);
//
//             // witdh and height of the image
//             var w = (bRx - tLx) / overviewPow;
//             var h = (bRy - tLy) / overviewPow;
//
//             using (var matrix = new Matrix(m11 * (float)overviewPow, m12 * (float)overviewPow, m21 * (float)overviewPow, m22 * (float)overviewPow, xShift, yShift))
//             {
//                 g.Transform = matrix;
//             }
//
//             int blockXsize, blockYsize;
//
//             // get the optimal block size to request gdal.
//             // if the image is stored line by line then ask for a 100px stripe.
//             if (_overview >= 0 && _red.GetOverviewCount() > 0)
//             {
//                 using (var overview = _red.GetOverview(_overview))
//                 {
//                     overview.GetBlockSize(out blockXsize, out blockYsize);
//                     if (blockYsize == 1)
//                     {
//                         blockYsize = Math.Min(100, overview.YSize);
//                     }
//                 }
//             }
//             else
//             {
//                 _red.GetBlockSize(out blockXsize, out blockYsize);
//                 if (blockYsize == 1)
//                 {
//                     blockYsize = Math.Min(100, _red.YSize);
//                 }
//             }
//
//             int nbX, nbY;
//
//             // limit the block size to the viewable image.
//             if (w < blockXsize)
//             {
//                 blockXsize = (int)w;
//                 nbX = 1;
//             }
//             else
//             {
//                 nbX = (int)(w / blockXsize) + 1;
//             }
//
//             if (h < blockYsize)
//             {
//                 blockYsize = (int)h;
//                 nbY = 1;
//             }
//             else
//             {
//                 nbY = (int)(h / blockYsize) + 1;
//             }
//
//             for (var i = 0; i < nbX; i++)
//             {
//                 for (var j = 0; j < nbY; j++)
//                 {
//                     // The +1 is to remove the white stripes artifacts
//                     using (var bitmap = ReadBlock((int)(tLx / overviewPow) + (i * blockXsize), (int)(tLy / overviewPow) + (j * blockYsize), blockXsize + 1, blockYsize + 1))
//                     {
//                         g.DrawImage(bitmap, (int)(tLx / overviewPow) + (i * blockXsize), (int)(tLy / overviewPow) + (j * blockYsize));
//                     }
//                 }
//             }
//         }
//          
//
//         private void GetBytes()
//         {
//             var tif = Tiff.Open("/Users/lewis/Downloads/yx/yx.tif", "r");
//             int bitsPerSample = (int)tif.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt() / 8;
//             int numBytes = bitsPerSample / 8;
//             int numTiles = tif.NumberOfTiles();
//             int tileWidth = tif.GetField(TiffTag.TILEWIDTH)[0].ToInt();
//             int tileHeight = tif.GetField(TiffTag.TILELENGTH)[0].ToInt();
//             int imageWidth = tif.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
//             int imageHeight = tif.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
//             int stride = numBytes * imageHeight;
//             int bufferSize = tileWidth * tileHeight * numBytes * numTiles;
//             int bytesSavedPerTile = tileWidth * tileHeight * numBytes; //this is the real size of the decompressed bytes
//             byte[] bufferTiff = new byte[bufferSize];
//
//             FieldValue[] value = tif.GetField(TiffTag.TILEWIDTH);
//             int tilewidth = value[0].ToInt();
//
//             value = tif.GetField(TiffTag.TILELENGTH);
//             int tileHeigth = value[0].ToInt();
//
//             int matrixSide =
//                 (int)Math.Sqrt(numTiles); // this works for a square image (for example a tiles organized tiff image)
//             int bytesWidth = matrixSide * tilewidth;
//             int bytesHeigth = matrixSide * tileHeigth;
//
//             int offset = 0;
//
//             for (int j = 0; j < numTiles; j++)
//             {
//                 offset += tif.ReadEncodedTile(j, bufferTiff, offset,
//                     bytesSavedPerTile); //Here was the mistake. Now it works!
//             }
//
//             double[,] aux = new double[bytesHeigth,
//                 bytesWidth]; //Double for a 64 bps tiff image. This matrix will save the alldata, including the transparency (the "blank zone" I was talking before)
//
//             var terrainElevation =
//                 new double[imageWidth,
//                     imageWidth]; // Double for a 64 bps tiff image. This matrix will save only the elevation values, without transparency
//
//             int ptr = 0;
//             int m = 0;
//             int n = -1;
//             int contNumTile = 1;
//             int contBytesPerTile = 0;
//             int i = 0;
//             int tileHeigthReference = tileHeigth;
//             int tileWidthReference = tileWidth;
//             int row = 1;
//             int col = 1;
//
//             byte[] bytesHeigthMeters = new byte[numBytes]; // Buffer to save each one elevation value to parse
//
//             while (i < bufferTiff.Length && contNumTile < numTiles + 1)
//             {
//                 for (contBytesPerTile = 0; contBytesPerTile < bytesSavedPerTile; contBytesPerTile++)
//                 {
//                     bytesHeigthMeters[ptr] = bufferTiff[i];
//                     ptr++;
//                     if (ptr % numBytes == 0 && ptr != 0)
//                     {
//                         ptr = 0;
//                         n++;
//
//                         if (n == tileHeigthReference)
//                         {
//                             n = tileHeigthReference - tileHeigth;
//                             m++;
//                             if (m == tileWidthReference)
//                             {
//                                 m = tileWidthReference - tileWidth;
//                             }
//                         }
//
//                         double heigthMeters = BitConverter.ToDouble(bytesHeigthMeters, 0);
//
//                         if (n < bytesWidth)
//                         {
//                             aux[m, n] = heigthMeters;
//                         }
//                         else
//                         {
//                             n = -1;
//                         }
//                     }
//
//                     i++;
//                 }
//
//                 if (i % tilewidth == 0)
//                 {
//                     col++;
//                     if (col == matrixSide + 1)
//                     {
//                         col = 1;
//                     }
//                 }
//
//                 if (contNumTile % matrixSide == 0)
//                 {
//                     row++;
//                     n = -1;
//                     if (row == matrixSide + 1)
//                     {
//                         row = 1;
//                     }
//                 }
//
//                 contNumTile++;
//                 tileHeigthReference = tileHeight * (col);
//                 tileWidthReference = tileWidth * (row);
//
//                 m = tileWidth * (row - 1);
//             }
//
//             for (int x = 0; x < imageHeight; x++)
//             {
//                 for (int y = 0; y < imageWidth; y++)
//                 {
//                     terrainElevation[x, y] =
//                         aux
//                             [x, y]; // Final result. Each position of matrix has saved each pixel terrain elevation of the map
//                 }
//             }
//         }
//
//         private static void TiffToBitmap()
//         {
//             var tiff = Tiff.Open("/Users/lewis/Downloads/yx/yx.tif", "r");
//             int imageWidth = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
//             int imageHeight = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
//             int bytesPerSample = (int)tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt() / 8;
//             SampleFormat format = (SampleFormat)tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToInt();
//
//             //Array to return
//             float[,] decoded = new float[imageHeight, imageWidth];
//
//             //Get decode function (I only want a float array)
//             // Func<byte[], int, float> decode = GetConversionFunction(format, bytesPerSample);
//             // if (decode == null)
//             // {
//             //     throw new ArgumentException("Unsupported TIFF format:" + format);
//             // }
//
//             if (tiff.IsTiled())
//             {
//                 //tile dimensions in pixels - the image dimensions MAY NOT be a multiple of these dimensions
//                 int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
//                 int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
//
//                 //tile matrix size
//                 int numTiles = tiff.NumberOfTiles();
//                 int tileMatrixWidth = (int)Math.Ceiling(imageWidth / (float)tileWidth);
//                 int tileMatrixHeight = (int)Math.Ceiling(imageHeight / (float)tileHeight);
//
//                 //tile dimensions in bytes
//                 int tileBytesWidth = tileWidth * bytesPerSample;
//                 int tileBytesHeight = tileHeight * bytesPerSample;
//
//                 //tile buffer
//                 int tileBufferSize = tiff.TileSize();
//                 byte[] tileBuffer = new byte[tileBufferSize];
//
//                 int imageHeightMinus1 = imageHeight - 1;
//
//                 for (int tileIndex = 0; tileIndex < numTiles; tileIndex++)
//                 {
//                     int tileX = tileIndex / tileMatrixWidth;
//                     int tileY = tileIndex % tileMatrixHeight;
//
//                     tiff.ReadTile(tileBuffer, 0, tileX * tileWidth, tileY * tileHeight, 0, 0);
//
//                     int xImageOffset = tileX * tileWidth;
//                     int yImageOffset = tileY * tileHeight;
//
//                     for (int col = 0; col < tileWidth && xImageOffset + col < imageWidth; col++)
//                     {
//                         for (int row = 0; row < tileHeight && yImageOffset + row < imageHeight; row++)
//                         {
//                             // decoded[imageHeightMinus1 - (yImageOffset + row), xImageOffset + col] =
//                             //     decode(tileBuffer, row * tileBytesWidth + col * bytesPerSample);
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }