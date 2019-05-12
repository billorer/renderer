using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Renderer2
{
    static class Utils
    {
        public static async Task<Mesh[]> LoadJSONFileAsync(string fileName)
        {
            List<Mesh> meshes = new List<Mesh>();
            dynamic jsonObject = JsonConvert.DeserializeObject(File.ReadAllText(fileName));
            for (int meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var meshVertexes = jsonObject.meshes[meshIndex].vertices;
                var meshFaces = jsonObject.meshes[meshIndex].indices;
                int uvCount = (int)jsonObject.meshes[meshIndex].uvCount.Value;

                int vertexJumper = 1;
                // how much we should jump on the vertexes later on in the meshVertexes array, based on the uvCount value
                if (uvCount == 0)
                    vertexJumper = 6;
                else if (uvCount == 1)
                    vertexJumper = 8;
                else if (uvCount == 2)
                    vertexJumper = 10;

                meshes.Add(CreateMeshFromData(meshVertexes, meshFaces, vertexJumper, jsonObject.meshes[meshIndex].name.Value, jsonObject.meshes[meshIndex]));
            }
            return meshes.ToArray();
        }

        private static Mesh CreateMeshFromData(dynamic meshVertexes, dynamic meshFaces, int vertexJumper, string meshName, dynamic meshParameters)
        {
            // we do not need all of the vertexes since babylon gives an extra detail based on the uvCount
            int vertexCounter = meshVertexes.Count / vertexJumper;
            int faceCounter = meshFaces.Count / 3; // triangles

            Mesh mesh = new Mesh(meshName, vertexCounter, faceCounter);

            mesh.Vertexes = GetMeshVertexes(vertexCounter, vertexJumper, meshVertexes);
            mesh.Faces = GetMeshFaces(faceCounter, meshFaces);

            var position = meshParameters.position;
            var rotation = meshParameters.rotation;
            var scaling = meshParameters.scaling;
            mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
            mesh.Rotation = new Vector3((float)rotation[0].Value, (float)rotation[1].Value, (float)rotation[2].Value);
            mesh.Scaling = new Vector3((float)scaling[0].Value, (float)scaling[1].Value, (float)scaling[2].Value);
            return mesh;
        }

        private static Vector3[] GetMeshVertexes(int vertexCounter, int vertexJumper, dynamic meshVertexes)
        {
            Vector3[] vertexes = new Vector3[vertexCounter];
            for (int index = 0; index < vertexCounter; index++)
            {
                float x = (float)meshVertexes[index * vertexJumper].Value;
                float y = (float)meshVertexes[index * vertexJumper + 1].Value;
                float z = (float)meshVertexes[index * vertexJumper + 2].Value;
                vertexes[index] = new Vector3(x, y, z);
            }
            return vertexes;
        }

        private static Face[] GetMeshFaces(int faceCounter, dynamic meshFaces)
        {
            Face[] faces = new Face[faceCounter];
            for (int index = 0; index < faceCounter; index++)
            {
                int A = (int)meshFaces[index * 3].Value;
                int B = (int)meshFaces[index * 3 + 1].Value;
                int C = (int)meshFaces[index * 3 + 2].Value;
                faces[index] = new Face { A = A, B = B, C = C };
            }
            return faces;
        }
    }
}
