using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SharpDX;

namespace Renderer2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device device;
        private Mesh cubeMesh; 
        private Camera camera;  

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void onPageLoad(object sender, RoutedEventArgs e)
        {
            // Choose the back buffer resolution here
            WriteableBitmap bmp = new WriteableBitmap(1280, 750);
            device = new Device(bmp);

            // Our Image XAML control
            frontBuffer.Source = bmp;

            camera = new Camera();
            cubeMesh = new Mesh("Cube", 8, 12);

            cubeMesh.Vertexes[0] = new Vector3(-1, 1, 1);
            cubeMesh.Vertexes[1] = new Vector3(1, 1, 1);
            cubeMesh.Vertexes[2] = new Vector3(-1, -1, 1);
            cubeMesh.Vertexes[3] = new Vector3(-1, -1, -1);
            cubeMesh.Vertexes[4] = new Vector3(-1, 1, -1);
            cubeMesh.Vertexes[5] = new Vector3(1, 1, -1);
            cubeMesh.Vertexes[6] = new Vector3(1, -1, 1);
            cubeMesh.Vertexes[7] = new Vector3(1, -1, -1);

            cubeMesh.Faces[0] = new Face { A = 0, B = 1, C = 2 };
            cubeMesh.Faces[1] = new Face { A = 1, B = 2, C = 3 };
            cubeMesh.Faces[2] = new Face { A = 1, B = 3, C = 6 };
            cubeMesh.Faces[3] = new Face { A = 1, B = 5, C = 6 };
            cubeMesh.Faces[4] = new Face { A = 0, B = 1, C = 4 };
            cubeMesh.Faces[5] = new Face { A = 1, B = 4, C = 5 };

            cubeMesh.Faces[6] = new Face { A = 2, B = 3, C = 7 };
            cubeMesh.Faces[7] = new Face { A = 3, B = 6, C = 7 };
            cubeMesh.Faces[8] = new Face { A = 0, B = 2, C = 7 };
            cubeMesh.Faces[9] = new Face { A = 0, B = 4, C = 7 };
            cubeMesh.Faces[10] = new Face { A = 4, B = 5, C = 6 };
            cubeMesh.Faces[11] = new Face { A = 4, B = 6, C = 7 };

            camera.Position = new Vector3(0, 0, 10.0f);
            camera.Target = Vector3.Zero;

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += renderingLoop;
        }

        private void renderingLoop(object sender, object e)
        {
            device.PaintBackBufferBlack();

            // rotating slightly the cube during each frame rendered
            cubeMesh.Rotation = new Vector3(cubeMesh.Rotation.X + 0.01f, cubeMesh.Rotation.Y + 0.01f, cubeMesh.Rotation.Z);

            // Doing the various matrix operations
            device.Render(camera, cubeMesh);
            // Flushing the back buffer into the front buffer
            device.Present();
        }
    }
}