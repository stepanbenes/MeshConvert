using System;

namespace MeshConvert
{
    class EncodingParameters
    {
        public string DataType { get; set; }
        public int OriginalLength { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string DefaultValue { get; set; }
    }
}
