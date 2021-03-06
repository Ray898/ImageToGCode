﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageToGCode.Engine.GCodeGeneration;
using ImageToGCode.Engine.GCodeGeneration.ImageProcessor;

namespace ImageToGCode.Engine.Visualisers
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImageToGCode.Engine.Visualisers"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImageToGCode.Engine.Visualisers;assembly=ImageToGCode.Engine.Visualisers"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:Visualiser/>
    ///
    /// </summary>
    class Visualiser : Canvas
    {
        public int MinIntensity
        {
            get { return (int)GetValue(MinIntensityProperty); }
            set { SetValue(MinIntensityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinIntensity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinIntensityProperty =
            DependencyProperty.Register("MinIntensity", typeof(int), typeof(Visualiser), new UIPropertyMetadata(0));
        public int MaxIntensity
        {
            get { return (int)GetValue(MaxIntensityProperty); }
            set { SetValue(MaxIntensityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxIntensity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxIntensityProperty =
            DependencyProperty.Register("MaxIntensity", typeof(int), typeof(Visualiser), new UIPropertyMetadata(0));



        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }


        public double Magnification
        {
            get { return (double)GetValue(MagnificationProperty); }
            set { SetValue(MagnificationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Magnification.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MagnificationProperty =
            DependencyProperty.Register("Magnification", typeof(double), typeof(Visualiser),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, null, null));



        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(Visualiser),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null, null));

        static Visualiser()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Visualiser), new FrameworkPropertyMetadata(typeof(Visualiser)));
        }
        public Visualiser()
        {

        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
        protected override void OnRender(DrawingContext dc)
        {
            if (Data is IEnumerable<BaseGCode>)
                VisualiseGCode(dc);
            if (Data is VectorProcessorViewModel)
                VisualiseVector(dc);
        }
        private void VisualiseVector(DrawingContext dc)
        {
            if (Data == null || !(Data is VectorProcessorViewModel) || ((VectorProcessorViewModel)Data).PathGroups.Count == 0)
                return;
            var data = (VectorProcessorViewModel)Data;

            foreach (var vPathGrp in data.PathGroups)
            {
                if (!vPathGrp.Engrave)
                    continue;

                foreach (var pth in vPathGrp.PathList)
                {
                    var currentPathData = pth.PathData;

                    if (currentPathData.Points.Length == 0)
                        continue;

                    System.Drawing.PointF? prevPoint = null;
                    System.Drawing.PointF? startPoint = null;

                    for (int i = 0; i < currentPathData.Points.Length; i++)
                    {
                        var curPthType = currentPathData.Types[i];
                        var curPoint = currentPathData.Points[i];
                        //Find dirst point in path
                        if (Geometry.PathTypeHelper.IsSet(curPthType, System.Drawing.Drawing2D.PathPointType.Start))
                            startPoint = prevPoint = curPoint;
                        //Draw line on path points
                        else if (Geometry.PathTypeHelper.IsSet(curPthType, System.Drawing.Drawing2D.PathPointType.Line) ||
                            Geometry.PathTypeHelper.IsSet(curPthType, System.Drawing.Drawing2D.PathPointType.Bezier))
                        {
                            if (prevPoint.HasValue)
                            {
                                Point start = PointFToPoint(prevPoint.Value);
                                Point end = PointFToPoint(curPoint);
                                dc.DrawLine(new Pen(vPathGrp.Brush, 1.0), start, end);
                            }
                            prevPoint = curPoint;

                            //ClosePath
                            if (Geometry.PathTypeHelper.IsSet(curPthType, System.Drawing.Drawing2D.PathPointType.CloseSubpath))
                            {
                                Point start = PointFToPoint(curPoint);
                                Point end = PointFToPoint(startPoint.Value);
                                dc.DrawLine(new Pen(vPathGrp.Brush, 1.0), start, end);
                            }
                        }
                    }
                }
            }
            DrawArrows(dc);
        }
        private void VisualiseGCode(DrawingContext dc)
        {
            if (Data == null || !(Data is IEnumerable<BaseGCode>) || ((IEnumerable<BaseGCode>)Data).Count() == 0)
                return;

            var data = (IList<BaseGCode>)Data;

            BaseMotion firstMotion = null;
            foreach (BaseGCode item in data)
            {
                if (!(item is BaseMotion))
                    continue;

                var curMotion = (BaseMotion)item;

                if (firstMotion != null)
                {

                    Point start = VectorToPoint(firstMotion.Position);
                    Point end = VectorToPoint(curMotion.Position);

                    dc.DrawLine(new Pen(new SolidColorBrush(GCodeToColor(curMotion)), 1.0), start, end);

                    if (curMotion is RapidMotion)
                    {
                        var dir = (curMotion.Position - firstMotion.Position).Normalize();
                        var v1 = dir.Rotate(15.0 / 180.0 * Math.PI) * 5;
                        var v2 = dir.Rotate(-15.0 / 180.0 * Math.PI) * 5;
                        v1 = curMotion.Position - v1;
                        v2 = curMotion.Position - v2;
                        dc.DrawLine(new Pen(new SolidColorBrush(Colors.Magenta), 1.0), end, VectorToPoint(v1));
                        dc.DrawLine(new Pen(new SolidColorBrush(Colors.Magenta), 1.0), end, VectorToPoint(v2));
                    }
                }

                firstMotion = curMotion;
            }

            DrawArrows(dc);
        }

        private void DrawArrows(DrawingContext dc)
        {
            Pen arrowPen = new Pen(Brushes.DarkGray, 1);
            dc.DrawLine(arrowPen, CoordToPoint(0, 0), CoordToPoint(0, 100));
            dc.DrawLine(arrowPen, CoordToPoint(2, 95), CoordToPoint(0, 100));
            dc.DrawLine(arrowPen, CoordToPoint(-2, 95), CoordToPoint(0, 100));
            dc.DrawLine(arrowPen, CoordToPoint(0, 0), CoordToPoint(100, 0));
            dc.DrawLine(arrowPen, CoordToPoint(95, 2), CoordToPoint(100, 0));
            dc.DrawLine(arrowPen, CoordToPoint(95, -2), CoordToPoint(100, 0));

            dc.DrawEllipse(Brushes.Red, new Pen(Brushes.DarkGray, 1), CoordToPoint(0, 0), 3, 3);
        }

        private Color GCodeToColor(BaseMotion motion)
        {
            if (motion is CoordinatMotion)
            {
                var cm = (CoordinatMotion)motion;
                return Color.FromRgb(cm.Color.R, cm.Color.G, cm.Color.B);
            }

            if (motion is RapidMotion)
                return Colors.HotPink;

            return Colors.HotPink;
        }
        private Point VectorToPoint(Geometry.Vector v)
        {
            return new Point(v.X * Magnification, ActualHeight - v.Y * Magnification);
        }
        private Point CoordToPoint(double x, double y)
        {
            return new Point(x * Magnification, ActualHeight - y * Magnification);
        }
        private Point PointFToPoint(System.Drawing.PointF pt)
        {
            return new Point(pt.X * Magnification, ActualHeight - pt.Y * Magnification);
        }
    }
}
