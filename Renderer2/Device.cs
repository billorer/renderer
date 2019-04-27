using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using System.Diagnostics;

namespace Renderer2
{
    internal class Device
    {
        private byte[] backBuffer;
        private WriteableBitmap bitmap;

        public Device(WriteableBitmap bitmap)
        {
            this.bitmap = bitmap;
            // we allocate the size for the buffer, the 4 comes from the 4 color values (RGBA (alpha))
            backBuffer = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        }

        /// <summary>
        /// It goes through the backBuffer array and paints it to black (pixel by pixel)
        /// </summary>
        public void PaintBackBufferBlack()
        {
            for (int index = 0; index < backBuffer.Length; index += 4)
            {
                // black color (0 0 0 255)
                backBuffer[index] = 0;
                backBuffer[index + 1] = 0;
                backBuffer[index + 2] = 0;
                backBuffer[index + 3] = 255;
            }
        }

        /// <summary>
        /// It converts bitmap to a writeable one
        /// </summary>
        public void Present()
        {
            // copy the image to the WriteableBitmap's pixel backBuffer 
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(backBuffer, 0, backBuffer.Length);
            }
            // request a redraw of the entire bitmap
            bitmap.Invalidate();
        }

        /// <summary>
        /// Places a pixel on the given x,y coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void PlacePixelinBitmap(int x, int y, Color color)
        {
            //  calculate the index, where we should place the pixel, the pixel gonna have a color
            int index = (x + y * bitmap.PixelWidth) * 4;

            backBuffer[index] = color.B;
            backBuffer[index + 1] = color.G;
            backBuffer[index + 2] = color.R;
            backBuffer[index + 3] = color.A;
        }

        /// <summary>
        /// With the help of the tranformation matrix we can create the 2d representation of a 3d point (vertex)
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="transMat"></param>
        /// <returns></returns>
        private Vector2 ConvertVec3toVec2(Vector3 coord, Matrix4x4 transMat)
        {
            Vector3 point = Vector3.TransformNormal(coord, transMat);
            // we divide it by two so we place it to the center of the screen -> img.width / 2 : img.height / 2
            float x = point.X * bitmap.PixelWidth + bitmap.PixelWidth / 2.0f;
            float y = -point.Y * bitmap.PixelHeight + bitmap.PixelHeight / 2.0f;
            return (new Vector2(x, y));
        }

        /// <summary>
        /// It invites the PlacePixelinBitmap
        /// </summary>
        /// <param name="point"></param>
        public void DrawVertex(Vector2 point)
        {
            // Check if the vertex will be visible in the camera
            if (point.X >= 0 && point.Y >= 0 && point.X < bitmap.PixelWidth && point.Y < bitmap.PixelHeight)
            {
                PlacePixelinBitmap((int)point.X, (int)point.Y, Colors.Red);
            }
        }

        /// <summary>
        /// It connects two vertexes, it is called recursively
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void ConnectVertexes(Vector2 p1, Vector2 p2)
        {
            double distance = (p1 - p2).Length();

            // The distance is less than a pixel, no need to connect the vertexes
            if (distance < 2)
                return;

            Vector2 middleP = p1 + (p2 - p1) / 2;
            DrawVertex(middleP);

            ConnectVertexes(p1, middleP);
            ConnectVertexes(middleP, p2);
        }


        public void ConnectVertexesBresenham(Vector2 point0, Vector2 point1)
        {
            int x0 = (int)point0.X;
            int y0 = (int)point0.Y;
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawVertex(new Vector2(x0, y0));

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>
        /// Params -> the number of parameters can be 0 or infinity
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="meshes"></param>
        public void Render(Camera camera, params Mesh[] meshes)
        {
            // The camera will look at a direction horizontally (Vector3.UnitY) -> cameraMatrix
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY);
            // Debug.WriteLine(viewMatrix);
            Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspective(25, 25, 1, 2);
            // Debug.WriteLine(viewMatrix);
            foreach (Mesh mesh in meshes)
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateFromYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix4x4.CreateTranslation(mesh.Position);
                Matrix4x4 transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                // drawing triangles
                foreach (Face face in mesh.Faces)
                {
                    Vector3 vertexA = mesh.Vertexes[face.A];
                    Vector3 vertexB = mesh.Vertexes[face.B];
                    Vector3 vertexC = mesh.Vertexes[face.C];

                    Vector2 pixelA = ConvertVec3toVec2(vertexA, transformMatrix);
                    Vector2 pixelB = ConvertVec3toVec2(vertexB, transformMatrix);
                    Vector2 pixelC = ConvertVec3toVec2(vertexC, transformMatrix);

                    ConnectVertexesBresenham(pixelA, pixelB);
                    ConnectVertexesBresenham(pixelB, pixelC);
                    ConnectVertexesBresenham(pixelC, pixelA);
                }

            }
        }
    }
}