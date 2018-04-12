using System;

namespace MeshConvert
{
    class Mesh
    {
        public int NNodes { get; set; }
        public int NElem { get; set; }
        public float[][] Nodes { get; set; }
        public int[][] Elements { get; set; }
        public int[][] ElemMat { get; set; }

        public Boundary Boundary { get; set; }
    }
}
