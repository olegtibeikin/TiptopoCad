namespace Tiptopo.Model
{
    public class Measurement
    {
        public string id { get; set; }

        public string name { get; set; }
        public string note { get; set; }
        public bool hasStation { get; set; }
        public bool isImported { get; set; }
        public bool isMeasured { get; set; }
        public PointType type { get; set; }
        public long color { get; set; }
        public Position position { get; set; }
        public string code { get; set; }

        public override string ToString()
        {
            return $"id: {id}, name: {name}, note: {note}, hasStation: {hasStation}, isImported: {isImported}, isMeasured: {isMeasured}, type: {type}, color: {color}, position: {position}, code: {code}";
        }
    }
}
