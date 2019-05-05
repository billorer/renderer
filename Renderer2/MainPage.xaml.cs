using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Renderer2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ParallelRenderer paralellRenderer;
        private Renderer renderer;
        private Mesh[] meshes; 
        private Camera camera;

        private DateTime previousDate;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void onPageLoad(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap(1280, 750);
            renderer = new Renderer(bmp);

            // Connect with the image object on xaml
            frontBuffer.Source = bmp;

            camera = new Camera();
            camera.Position = new Vector3(0, 0, 0.10f);
            camera.Target = Vector3.Zero;

            meshes = await Utils.LoadJSONFileAsync(@"Babylon\ember.babylon");
            // Registering to the XAML rendering loop, its an event handler
            CompositionTarget.Rendering += renderingLoop;
        }

        private async void renderingLoop(object sender, object e)
        {
            DateTime currentDate = DateTime.Now;
            double currentFps = 1000.0 / (currentDate - previousDate).TotalMilliseconds;
            previousDate = currentDate;

            renderer.PaintBackBufferBlack();
            foreach (Mesh mesh in meshes)
            {
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y - 0.01f, mesh.Rotation.Z);
            }
            // await device.Render(camera, meshes);
            renderer.Render(camera, meshes);
            renderer.Present();
            fpsTextBox.Text = string.Format("{0:0.000} fps", currentFps);
        }
    }
}