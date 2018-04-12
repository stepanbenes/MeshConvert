using System;

namespace MeshConvert
{
    class MeshOutput
    {
        public Guid LayerId { get; set; }
        public int Index { get; set; }
        public int NumberOfPoints { get; set; }
        public int NumberOfCells { get; set; }
        public int NumberOfEdges { get; set; }
        public float[] Center { get; set; }
        public float Radius { get; set; }
        public string PointCoordinates { get; set; }
        public string CellConnectivity { get; set; }
        public string CellTypes { get; set; }
    }
}
