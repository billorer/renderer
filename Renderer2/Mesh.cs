using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Renderer2
{
    public struct Face
    {
        public int A;
        public int B;
        public int C;
    }

    public class Mesh
    {
        public string Name { get; set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3[] Vertexes { get; private set; }
        public Mesh(string name, int verticesCount, int facesCount)
        {
            Vertexes = new Vector3[verticesCount];
            Faces = new Face[facesCount];
            Name = name;
        }
    }
}
