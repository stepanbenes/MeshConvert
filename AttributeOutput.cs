using System;

namespace MeshConvert
{
    class AttributeOutput
    {
        public Guid LayerId { get; set; }
        public int Index { get; set; }
        public int MeshIndex { get; set; }
        public string FieldName { get; set; }
        public string Location { get; set; }
        public CompressionParameters Compression { get; set; }
        public EncodingParameters Encoding { get; set; }
        public string Data { get; set; }
    }
}
