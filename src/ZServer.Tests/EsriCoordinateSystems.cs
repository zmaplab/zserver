using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OfficeOpenXml;
using Xunit;

namespace ZServer.Tests;

public class EsriCoordinateSystems
{
    [Fact]
    public void ConvertToCsv()
    {
        var path = "ESRI_CS.xlsx";
        if (!File.Exists(path))
        {
            return;
        }

        var list = new List<(int Srid, string Name)>();

        var package = new ExcelPackage(new FileInfo(path));
        var ws = package.Workbook.Worksheets[0];

        for (var i = 2; i < 20000; ++i)
        {
            var name = ws.Cells[i, 1].Value?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                break;
            }


            name = name.Trim();
            name = name.Replace("\n", "");
            name = name.Replace(" ", "_");
            name = name.Replace("\"", "");
            name = name.Replace("__", "_");

            var srid = ws.Cells[i, 2].Value?.ToString();

            try
            {
                list.Add((int.Parse(srid), name));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        var sb = new StringBuilder("NAME, WKID");
        sb.AppendLine();
        foreach (var tuple in list)
        {
            sb.Append(tuple.Name).Append(',').AppendLine(tuple.Srid.ToString());
        }

        File.WriteAllText("ESRI_CS.csv", sb.ToString());
    }
}