﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI;
using System.Diagnostics;

namespace Renderer2
{
    internal class ParallelRenderer
    {
        private byte[] backBuffer;
        private byte[] backBuffer1;
        private byte[] backBuffer2;
        private byte[] backBuffer3;
        private byte[] backBuffer4;
        private WriteableBitmap bitmap;

        private readonly int bitmapWidth;
        private readonly int bitmapHeight;
        //private object[] lockBuffer;

        public ParallelRenderer(WriteableBitmap bitmap)
        {
            this.bitmap = bitmap;
            bitmapWidth = bitmap.PixelWidth;
            bitmapHeight = bitmap.PixelHeight;

            // we allocate the size for the buffer, the 4 comes from the 4 color values (RGBA (alpha))
            //backBuffer1 = new byte[bitmapWidth * bitmapHeight * 4];
            //backBuffer2 = new byte[bitmapWidth * bitmapHeight * 4];
            //backBuffer3 = new byte[bitmapWidth * bitmapHeight * 4];
            //backBuffer4 = new byte[bitmapWidth * bitmapHeight * 4];
            backBuffer = new byte[bitmapWidth * bitmapHeight * 4];

            //lockBuffer = new object[bitmapWidth * bitmapHeight];
            //for (int index = 0; index < lockBuffer.Length; index++)
            //{
            //    lockBuffer[index] = new object();
            //}
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
        public void PlacePixelinBitmap(int x, int y, Color color, byte[] backBuffer)
        {
            //  calculate the index, where we should place the pixel, the pixel gonna have a color
            int index = (x + y * bitmapWidth);
            int index4 = index * 4;
            //lock (lockBuffer[index])
            //{
                backBuffer[index4] = color.B;
                backBuffer[index4 + 1] = color.G;
                backBuffer[index4 + 2] = color.R;
                backBuffer[index4 + 3] = color.A;
            //}
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
            float x = point.X * bitmapWidth + bitmapWidth / 2.0f;
            float y = -point.Y * bitmapHeight + bitmapHeight / 2.0f;
            return (new Vector2(x, y));
        }

        /// <summary>
        /// It connects two vertexes, it is called recursively
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        //public void ConnectVertexes(Vector2 p1, Vector2 p2)
        //{
        //    double distance = (p1 - p2).Length();

        //    // The distance is less than a pixel, no need to connect the vertexes
        //    if (distance < 2)
        //        return;

        //    Vector2 middleP = p1 + (p2 - p1) / 2;
        //    // Check if the vertex will be visible in the camera, then put the pixel in bitmap
        //    if (middleP.X >= 0 && middleP.Y >= 0 && middleP.X < bitmapWidth && middleP.Y < bitmapHeight)
        //        PlacePixelinBitmap((int)middleP.X, (int)middleP.Y, Colors.Red);

        //    ConnectVertexes(p1, middleP);
        //    ConnectVertexes(middleP, p2);
        //}

        /// <summary>
        /// Implemented according to the Bresenham algorithm
        /// It draws a line between two points
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void ConnectVertexesBresenham(Vector2 from, Vector2 to, byte[] currentBuffer)
        {
            int fromX = (int)from.X;
            int fromY = (int)from.Y;

            int toX = (int)to.X;
            int toY = (int)to.Y;

            int deltaX = Math.Abs(toX - fromX);
            int sx = (fromX < toX) ? 1 : -1;

            int deltaY = Math.Abs(toY - fromY);
            int sy = (fromY < toY) ? 1 : -1;

            int error = deltaX - deltaY;

            while (true)
            {
                // Check if the vertex will be visible in the camera, then put the pixel in bitmap
                if (fromX >= 0 && fromY >= 0 && fromX < bitmapWidth && fromY < bitmapHeight)
                    PlacePixelinBitmap(fromX, fromY, Colors.Red, currentBuffer);
                if ((fromX == toX) && (fromY == toY)) break;
                int error2 = 2 * error;
                if (error2 > -deltaY) { error -= deltaY; fromX += sx; }
                if (error2 < deltaX) { error += deltaX; fromY += sy; }
            }
        }

        /// <summary>
        /// Params -> the number of parameters can be 0 or infinity
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="meshes"></param>
        public async Task Render(Camera camera, params Mesh[] meshes)
        {
            // The camera will look at a direction horizontally (Vector3.UnitY) -> cameraMatrix
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY);
            // Debug.WriteLine(viewMatrix);
            Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspective(15, 15, 1, 2);
            // Debug.WriteLine(viewMatrix); 

            List<Task> tasks = new List<Task>();
            foreach (Mesh mesh in meshes)
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateScale(mesh.Scaling) * Matrix4x4.CreateFromYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix4x4.CreateTranslation(mesh.Position);
                // Matrix4x4 worldMatrix = Matrix4x4.CreateWorld(mesh.Position, new Vector3(1, 0, 1), new Vector3(0, 1, 0));
                Matrix4x4 transformMatrix = worldMatrix * viewMatrix * projectionMatrix;
                foreach(Face face in mesh.Faces)
                {
                    tasks.Add(new Task(() =>
                    {
                        Vector3 vertexA = mesh.Vertexes[face.A];
                        Vector3 vertexB = mesh.Vertexes[face.B];
                        Vector3 vertexC = mesh.Vertexes[face.C];

                        Vector2 pixelA = ConvertVec3toVec2(vertexA, transformMatrix);
                        Vector2 pixelB = ConvertVec3toVec2(vertexB, transformMatrix);
                        Vector2 pixelC = ConvertVec3toVec2(vertexC, transformMatrix);

                        ConnectVertexesBresenham(pixelB, pixelC, backBuffer);
                        ConnectVertexesBresenham(pixelC, pixelA, backBuffer);
                        ConnectVertexesBresenham(pixelA, pixelB, backBuffer);
                    }));
                }

                foreach (Task t in tasks)
                    t.Start();
                await Task.WhenAll(tasks.ToArray());
            }
        }
    }
}
