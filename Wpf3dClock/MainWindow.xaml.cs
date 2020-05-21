using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf3dClock
{
    public class MyList: ObservableCollection<Division>
    {
        public MyList():base()
        {
            const int HOUR_ANGLE = 30;
            const int SECOND_ANGLE = 6;
            for (int i = 0; i < 12; i++)
            {
                double currentHourAngle = HOUR_ANGLE * i;

                Add(new Division { Data = "M195,5 l10,0 l0,20 l-10,0 Z", Color = Brushes.DarkRed, Angle = currentHourAngle });

                for (int j = 1; j < 5; j++)
                {
                   Add(new Division { Data = "M195,5 l10,0 l0,10 l-10,0 Z", Color = Brushes.Black, Angle = currentHourAngle + (SECOND_ANGLE * j) });
                }
            }
        }
    }



    public class Division
    {
        public string Data { get; set; }
        public double Angle { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {      
        private Point previousMousePosition;
        private Quaternion currentCubePosition;


        public MainWindow()
        {
            currentCubePosition = new Quaternion(new Vector3D(0, 1, 0), 0);
            InitializeComponent();       
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime timeNow = DateTime.Now;

            //DoubleAnimation secondAnimation = new DoubleAnimation(timeNow.Second * 6, 360d, new Duration(TimeSpan.FromSeconds(60 - timeNow.Second)));
            //secondAnimation.Completed += SecondAnimation_Completed;
            //(SecondsLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, secondAnimation);

            //DoubleAnimation minuteAnimation = new DoubleAnimation((timeNow.Minute + timeNow.Second / 60) * 6, 360d, new Duration(TimeSpan.FromMinutes(60 - (timeNow.Minute + timeNow.Second / 60))));
            //minuteAnimation.Completed += MinuteAnimation_Completed;
            //(MinutesLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, minuteAnimation);


            //double hourNow = timeNow.Hour <= 12 ? timeNow.Hour : timeNow.Hour - 12;
            //DoubleAnimation HourAnimation = new DoubleAnimation((hourNow + timeNow.Minute / 60) * 30, 360d, new Duration(TimeSpan.FromHours(12 - (hourNow + timeNow.Minute / 60))));
            //HourAnimation.Completed += HourAnimation_Completed;
            //(HoursLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, HourAnimation);



            //Без колбэков
            DoubleAnimation secondAnimation = new DoubleAnimation(timeNow.Second * 6, 360 + timeNow.Second * 6, new Duration(TimeSpan.FromSeconds(60)));
            secondAnimation.RepeatBehavior = RepeatBehavior.Forever;
            (SecondsLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, secondAnimation);

            DoubleAnimation minuteAnimation = new DoubleAnimation((timeNow.Minute + timeNow.Second / 60) * 6, 360 + (timeNow.Minute + timeNow.Second / 60) * 6, new Duration(TimeSpan.FromMinutes(60)));
            minuteAnimation.RepeatBehavior = RepeatBehavior.Forever;
            (MinutesLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, minuteAnimation);


            double hourNow = timeNow.Hour <= 12 ? timeNow.Hour : timeNow.Hour - 12;
            DoubleAnimation HourAnimation = new DoubleAnimation((hourNow + timeNow.Minute / 60) * 30, 360 + (hourNow + timeNow.Minute / 60) * 30, new Duration(TimeSpan.FromHours(12)));
            HourAnimation.RepeatBehavior = RepeatBehavior.Forever;
            (HoursLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, HourAnimation);
        }

        #region animation
        private void HourAnimation_Completed(object sender, EventArgs e)
        {
            DoubleAnimation hourAnimation = new DoubleAnimation(0, 360d, new Duration(TimeSpan.FromHours(60)));
            hourAnimation.RepeatBehavior = RepeatBehavior.Forever;
            (HoursLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, hourAnimation);
        }

        private void MinuteAnimation_Completed(object sender, EventArgs e)
        {
            DoubleAnimation minuteAnimation = new DoubleAnimation(0, 360d, new Duration(TimeSpan.FromMinutes(60)));
            minuteAnimation.RepeatBehavior = RepeatBehavior.Forever;
            (MinutesLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, minuteAnimation);
        }

        private void SecondAnimation_Completed(object sender, EventArgs e)
        {
            DoubleAnimation secondAnimation = new DoubleAnimation(0, 360d, new Duration(TimeSpan.FromSeconds(60)));
            secondAnimation.RepeatBehavior = RepeatBehavior.Forever;
            (SecondsLine.RenderTransform as RotateTransform).BeginAnimation(RotateTransform.AngleProperty, secondAnimation);
        }
        #endregion

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            previousMousePosition = e.GetPosition(Cube);
            //Console.WriteLine($"Previous = {previousMousePosition}");
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point currentPosition = e.GetPosition(Cube);
            Point realDifference = new Point(previousMousePosition.X - currentPosition.X, previousMousePosition.Y - currentPosition.Y);
            Point difference = new Point(Math.Abs(previousMousePosition.X - currentPosition.X), Math.Abs(previousMousePosition.Y - currentPosition.Y));


            var swipe = difference.X > difference.Y ? (Difference: difference.X, Axis: "X") : (Difference: difference.Y, Axis: "Y");

            if (swipe.Difference > 100)
            {
                double angle = swipe.Axis == "X" ? realDifference.X : realDifference.Y;

                if (angle < 0)
                    angle = 270; //поворот влево либо вверх (-90 градусов) реализован поворотом вправо/вниз на 3 грани (270 градусов) во избежания отрицательных значений и ошибки 
                else
                    angle = 90;

                Vector3D axis = swipe.Axis == "X" ? new Vector3D(0, 1, 0) : new Vector3D(1, 0, 0);
                Quaternion newPosition;


                Console.WriteLine($"Current axis {currentCubePosition.Axis}");
                Console.WriteLine($"Current angle {currentCubePosition.Angle}");



                if (currentCubePosition.Axis != axis)
                {                   
                    newPosition = new Quaternion(axis, angle);
                }
                else
                {                  
                        newPosition = new Quaternion(axis, currentCubePosition.Angle + angle);
                }


                if (currentCubePosition.Axis.X == 1 && currentCubePosition.Angle == 90 && newPosition.Axis.X == 1 && newPosition.Angle == 180)
                {
                    Console.WriteLine("bot to back");
                    newPosition = new Quaternion(new Vector3D(0, 1, 0), 180);
                }
                if (currentCubePosition.Axis.X == 1 && currentCubePosition.Angle == 270 && newPosition.Axis.X == 1 && newPosition.Angle == 180)
                {
                    Console.WriteLine("top to back");
                    newPosition = new Quaternion(new Vector3D(0, 1, 0), 180);
                }

                var rotation = new QuaternionAnimation(currentCubePosition, newPosition, new Duration(TimeSpan.FromSeconds(1)));
                currentCubePosition = newPosition;
                CameraRotation.BeginAnimation(QuaternionRotation3D.QuaternionProperty, rotation);
            }
        }

        //private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    // Get the finishing mouse position (when the user has finished dragging)
        //    Point newPosition = e.GetPosition(this.Cube);

        //    Double deltaX = newPosition.X - _originalMousePosition.X;
        //    Double deltaY = newPosition.Y - _originalMousePosition.Y;

        //    // Don't bother doing anything if the user has hardly moved the mouse...
        //    if (Math.Abs(deltaX) <= SystemParameters.MinimumHorizontalDragDistance &&
        //            Math.Abs(deltaY) <= SystemParameters.MinimumVerticalDragDistance)
        //    {
        //        return;
        //    }

        //    // A vector defines a length and direction

        //    Vector vectorY = new Vector(0, deltaY);
        //    Vector vectorX = new Vector(deltaX, 0);

        //    // Add the X and Y vectors together to get the actual vector representing the mouse movement
        //    // (although we are not actually interested in the 'length' part of the vector)
        //    Vector vectorXPlusY = vectorX + vectorY;

        //    // Now get the angle between due east and this vector
        //    Double angleBetween = Vector.AngleBetween(new Vector(newPosition.X, 0), vectorXPlusY);
        //    Direction directionToRotate;

        //    if (angleBetween > -135 && angleBetween < -45)
        //    {
        //        directionToRotate = Direction.North;
        //    }
        //    else if (angleBetween >= -45 && angleBetween < 45)
        //    {
        //        directionToRotate = Direction.East;
        //    }
        //    else if (angleBetween >= 45 && angleBetween < 135)
        //    {
        //        directionToRotate = Direction.South;
        //    }
        //    else
        //    {
        //        directionToRotate = Direction.West;
        //    }

        //    RotateCube(directionToRotate);
        //}


        //private void RotateCube(Direction direction)
        //{
        //    if (!_isRotating)
        //    {
        //        // Let the quaternion animation figure out how to transform between 2 quaternions...
        //        QuaternionAnimation animation = new QuaternionAnimation();

        //        // A quaternion is way of representing a rotation around an axis

        //        // The From quaternion is the one required to display the current cube side based on the original side being the 'front'  
        //        animation.From = _possibleRotationMatrix[_currentCubeRotation.CubeSide][Direction.None].Quaternion;
        //        // The To quaternion is the one required to display the next cube side based on the original side being the 'front'
        //        animation.To = _possibleRotationMatrix[_currentCubeRotation.CubeSide][direction].Quaternion;
        //        animation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 650));
        //        _isRotating = true;

        //        animation.Completed += (o, e) =>
        //        {
        //            _isRotating = false;
        //            //ToggleVisualHitTesting(true);
        //            _currentCubeRotation = _possibleRotationMatrix[_currentCubeRotation.CubeSide][direction];
        //        };
        //        //_isRotating = false;
        //        // Temporarily remove hit testing to make things a but smoother
        //        //ToggleVisualHitTesting(false);

        //        CameraRotation.BeginAnimation(QuaternionRotation3D.QuaternionProperty, animation);
        //    }
        //}
    }
}
