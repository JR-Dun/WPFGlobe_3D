using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF3DGlobe
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 三维 模型.
        private Model3DGroup MainModel3Dgroup = new Model3DGroup();
        // 照相机.
        private PerspectiveCamera TheCamera;

        // 照相机默认角度.
        private double CameraPhi = Math.PI / 6.0;       // 30 degrees （度）
        private double CameraTheta = Math.PI / 6.0;     // 30 degrees （度）
        private double CameraR = 3.0;

        // 点击上、下方向键，来改变CameraPhi
        private const double CameraDPhi = 0.1;
        // 点击左、右方向键，来改变CameraTheta.
        private const double CameraDTheta = 0.1;
        // 点击+、-键，来改变CameraR.
        private const double CameraDR = 0.1;

        // mouseDown标识
        private bool IsMouseDown;
        // 上一个mouseMove的点
        private Point MouseMovePoint;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化照相机的视野位置
            TheCamera = new PerspectiveCamera();
            //照相机的水平的视野，以度为单位。 默认值为 45。
            TheCamera.FieldOfView = 60; 
            MainViewport3D.Camera = TheCamera;
            PositionCamera();

            // 初始化光源。
            DefineLights();

            // 初始化地球仪对象。
            DefineModel();

            // ModelVisual3D 提供了 Visual3D 呈现的 Model3D 对象
            ModelVisual3D model_visual = new ModelVisual3D();
            // 将MainModel3Dgroup 添加到 ModelVisual3D
            model_visual.Content = MainModel3Dgroup;

            // 添加到 Viewport3D 视窗显示。
            MainViewport3D.Children.Add(model_visual);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    CameraPhi += CameraDPhi;
                    if (CameraPhi > Math.PI / 2.0) CameraPhi = Math.PI / 2.0;
                    break;
                case Key.Down:
                    CameraPhi -= CameraDPhi;
                    if (CameraPhi < -Math.PI / 2.0) CameraPhi = -Math.PI / 2.0;
                    break;
                case Key.Left:
                    CameraTheta += CameraDTheta;
                    break;
                case Key.Right:
                    CameraTheta -= CameraDTheta;
                    break;
                case Key.Add:
                case Key.OemPlus:
                    CameraR -= CameraDR;
                    if (CameraR < CameraDR) CameraR = CameraDR;
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    CameraR += CameraDR;
                    break;
            }

            // 更新照相机位置.
            PositionCamera();
        }

        #region main method
        /// <summary>
        /// 初始化光源
        /// </summary>
        private void DefineLights()
        {
            //光适用 light 于对象统一，而不考虑其形状的对象。
            AmbientLight ambient_light = new AmbientLight(Colors.Gray);
            //沿 Vector3D 指定的方向投射其效果的光对象。
            DirectionalLight directional_light = new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));

            MainModel3Dgroup.Children.Add(ambient_light);
            MainModel3Dgroup.Children.Add(directional_light);
        }

        /// <summary>
        /// 初始化3D对象
        /// </summary>
        private void DefineModel()
        {
            Model3DGroup globe_model = new Model3DGroup();
            MainModel3Dgroup.Children.Add(globe_model);

            ImageBrush globe_brush = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/globe.png")));
            Material globe_material = new DiffuseMaterial(globe_brush);
            MeshGeometry3D globe_mesh = null;
            MakeSphere(globe_model, ref globe_mesh, globe_material, 1, 0, 0, 0, 20, 30);
        }

        /// <summary>
        /// 创建一个球体
        /// </summary>
        /// <param name="model_group"></param>
        /// <param name="sphere_mesh"></param>
        /// <param name="sphere_material"></param>
        /// <param name="radius"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="cz"></param>
        /// <param name="num_phi"></param>
        /// <param name="num_theta"></param>
        private void MakeSphere(Model3DGroup model_group, ref MeshGeometry3D sphere_mesh, Material sphere_material,
            double radius, double cx, double cy, double cz, int num_phi, int num_theta)
        {
            // Make the mesh if we must.
            if (sphere_mesh == null)
            {
                sphere_mesh = new MeshGeometry3D();
                GeometryModel3D new_model = new GeometryModel3D(sphere_mesh, sphere_material);
                model_group.Children.Add(new_model);
            }

            double dphi = Math.PI / num_phi;
            double dtheta = 2 * Math.PI / num_theta;

            // Remember the first point.
            int pt0 = sphere_mesh.Positions.Count;

            // Make the points.
            double phi1 = Math.PI / 2;
            for (int p = 0; p <= num_phi; p++)
            {
                double r1 = radius * Math.Cos(phi1);
                double y1 = radius * Math.Sin(phi1);

                double theta = 0;
                for (int t = 0; t <= num_theta; t++)
                {
                    sphere_mesh.Positions.Add(new Point3D(
                        cx + r1 * Math.Cos(theta), cy + y1, cz + -r1 * Math.Sin(theta)));
                    sphere_mesh.TextureCoordinates.Add(new Point(
                        (double)t / num_theta, (double)p / num_phi));
                    theta += dtheta;
                }
                phi1 -= dphi;
            }

            // Make the triangles.
            int i1, i2, i3, i4;
            for (int p = 0; p <= num_phi - 1; p++)
            {
                i1 = p * (num_theta + 1);
                i2 = i1 + (num_theta + 1);
                for (int t = 0; t <= num_theta - 1; t++)
                {
                    i3 = i1 + 1;
                    i4 = i2 + 1;
                    sphere_mesh.TriangleIndices.Add(pt0 + i1);
                    sphere_mesh.TriangleIndices.Add(pt0 + i2);
                    sphere_mesh.TriangleIndices.Add(pt0 + i4);

                    sphere_mesh.TriangleIndices.Add(pt0 + i1);
                    sphere_mesh.TriangleIndices.Add(pt0 + i4);
                    sphere_mesh.TriangleIndices.Add(pt0 + i3);
                    i1 += 1;
                    i2 += 1;
                }
            }
        }

        /// <summary>
        /// 照相机位置
        /// </summary>
        private void PositionCamera()
        {
            // 笛卡尔坐标 计算照相机位置
            double y = CameraR * Math.Sin(CameraPhi);
            double hyp = CameraR * Math.Cos(CameraPhi);
            double x = hyp * Math.Cos(CameraTheta);
            double z = hyp * Math.Sin(CameraTheta);
            TheCamera.Position = new Point3D(x, y, z);

            // 获取或设置 Vector3D 定义在世界坐标查找照相机方向
            TheCamera.LookDirection = new Vector3D(-x, -y, -z);

            // 获取或设置 Vector3D 定义照相机的向上方向
            TheCamera.UpDirection = new Vector3D(0, 1, 0);

            //Console.WriteLine("Camera.Position: (" + x + ", " + y + ", " + z + ")");
        }
        #endregion

        #region 鼠标拖动
        private void MainViewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                MouseMovePoint = e.GetPosition(MainViewport3D);
                IsMouseDown = true;
            }
        }

        private void MainViewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown && e.LeftButton == MouseButtonState.Pressed)
            {
                Point point = e.GetPosition(MainViewport3D);

                double x = point.X - MouseMovePoint.X;
                double y = point.Y - MouseMovePoint.Y;

                if (Math.Abs(x) > Math.Abs(y))
                {
                    CameraTheta += (x / MainViewport3D.ActualWidth * 3);
                }
                else
                {
                    CameraPhi += (y / MainViewport3D.ActualHeight * 3);
                    if (y > 0)
                    {
                        if (CameraPhi > Math.PI / 2.0) CameraPhi = Math.PI / 2.0;
                    }
                    else
                    {
                        if (CameraPhi < -Math.PI / 2.0) CameraPhi = -Math.PI / 2.0;
                    }
                }

                // 更新照相机位置.
                PositionCamera();

                MouseMovePoint = point;
            }
        }

        private void MainViewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;
        }
        #endregion

        #region 点击事件
        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            CameraPhi += CameraDPhi;
            if (CameraPhi > Math.PI / 2.0) CameraPhi = Math.PI / 2.0;

            // 更新照相机位置.
            PositionCamera();
        }

        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            CameraTheta += CameraDTheta;

            // 更新照相机位置.
            PositionCamera();
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            CameraPhi -= CameraDPhi;
            if (CameraPhi < -Math.PI / 2.0) CameraPhi = -Math.PI / 2.0;

            // 更新照相机位置.
            PositionCamera();
        }

        private void buttonRight_Click(object sender, RoutedEventArgs e)
        {
            CameraTheta -= CameraDTheta;

            // 更新照相机位置.
            PositionCamera();
        }
        #endregion

    }
}
