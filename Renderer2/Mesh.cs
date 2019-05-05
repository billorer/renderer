using System.Numerics;

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
        public Vector3 Scaling { get; set; }
        public Vector3[] Vertexes { get; private set; }
        public Mesh(string name, int vertexCounter, int faceCounter)
        {
            Vertexes = new Vector3[vertexCounter];
            Faces = new Face[faceCounter];
            Name = name;
        }
    }
}
