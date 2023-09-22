using System.IO;

namespace ZMap.Ogc;

public record MapResult(Stream Image, string Code, string Message);