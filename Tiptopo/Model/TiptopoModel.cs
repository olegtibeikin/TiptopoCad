using System.Collections.Generic;

namespace Tiptopo.Model
{
    public class TiptopoModel
    {
        public List<Line> lines { get; set; }
        public List<Measurement> measurements { get; set; }
        public List<MapText> texts { get; set; }
    }
}
