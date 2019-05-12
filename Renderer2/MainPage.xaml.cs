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
        private int framesRendered;
        private int currentFPS;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void OnPageLoad(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap(1280, 750);
            paralellRenderer = new ParallelRenderer(bmp);
            //renderer = new Renderer(bmp);
            // Connect with the image object on xaml
            frontBuffer.Source = bmp;

            camera = new Camera();
            camera.Position = new Vector3(0, 0, 0.10f);
            camera.Target = Vector3.Zero;

            meshes = await Utils.LoadJSONFileAsync(@"Babylon\ember.babylon");
            // Registering to the XAML rendering loop, its an event handler
            CompositionTarget.Rendering += OnRenderingLoop;
        }

        private async void OnRenderingLoop(object sender, object e)
        {
            framesRendered++;
            if ((DateTime.Now - previousDate).TotalSeconds >= 1)
            {
                currentFPS = framesRendered;
                framesRendered = 0;
                previousDate = DateTime.Now;
            }

            paralellRenderer.PaintBackBufferBlack();
            foreach (Mesh mesh in meshes)
            {
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y - 0.01f, mesh.Rotation.Z);
            }
            await paralellRenderer.Render(camera, meshes);
            paralellRenderer.Present();
            fpsTextBox.Text = string.Format("{0:0.000} fps", currentFPS);
        }
    }
}