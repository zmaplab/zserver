namespace ZMap.SLD
{
    public interface ISymbolizer
    {
        string Geometry { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        Description Description { get; set; }

        abstract object Accept(IStyleVisitor visitor, object extraData);
    }
}