using System.Collections.Generic;

namespace ZServer.Interfaces
{
    public class ModeState
    {
        public string Text { get; set; }
        public string Code { get; set; }
        public bool IsValid { get; set; } = true;
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public int SRID { get; set; }
        public List<(string ResourceGroup, string Layer)> Layers { get; set; }

        public ModeState(string text, string code)
        {
            Text = text;
            Code = code;
            IsValid = false;
        }

        public ModeState()
        {
        }
    }
}