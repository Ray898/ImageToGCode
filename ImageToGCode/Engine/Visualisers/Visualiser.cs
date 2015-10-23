﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageToGCode.Engine.GCodeGeneration;

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
            if (Data is LinesVisualiser)
                VisualiseLines(dc);
            if (Data is StrokesFromImageLinesGenerator)
                VisualiseStrokes(dc);

        }

        private void VisualiseLines(DrawingContext dc)
        {
            if (Data == null || !(Data is LinesVisualiser) || ((LinesVisualiser)Data).Lines.Count == 0)
                return;

            var data = (LinesVisualiser)Data;
            foreach (var vline in data.Lines)
            {
                Point start = new Point(vline.V1.X * Magnification, this.ActualHeight - vline.V1.Y * Magnification);
                Point end = new Point(vline.V2.X * Magnification, this.ActualHeight - vline.V2.Y * Magnification);
                byte gcIntence = (byte)(255 - 255 * vline.Intensity);

                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb((byte)gcIntence, (byte)gcIntence, (byte)gcIntence)), 1.0), start, end);
            }            
        }
        private void VisualiseStrokes(DrawingContext dc)
        {
            if (Data == null || !(Data is StrokesFromImageLinesGenerator) || ((StrokesFromImageLinesGenerator)Data).Strokes.Count == 0)
                return;

            var data = (StrokesFromImageLinesGenerator)Data;

            FreeMotionStroke firstStroke = data.Strokes[0];

            for (int i = 1; i < data.Strokes.Count; i++)
            {
                FreeMotionStroke curStroke = data.Strokes[i];
                Point start = new Point((firstStroke.DestinationPoint.X + data.IdleDistance) * Magnification,
                    this.ActualHeight - (firstStroke.DestinationPoint.Y + data.IdleDistance) * Magnification);

                Point end = new Point((curStroke.DestinationPoint.X + data.IdleDistance) * Magnification,
                    this.ActualHeight - (curStroke.DestinationPoint.Y + data.IdleDistance) * Magnification);

                dc.DrawLine(new Pen(new SolidColorBrush(StrokeToColor(curStroke)), 1.0), start, end);

                firstStroke = curStroke;
            }
        }

        private Color StrokeToColor(FreeMotionStroke stroke)
        {
            if (stroke is IdleStroke)
                return Colors.BlueViolet;
            if (stroke is Stroke)
            {
                byte gcIntence = (byte)(255 - 255 * ((Stroke)stroke).Intensity);
                return Color.FromRgb((byte)gcIntence, (byte)gcIntence, (byte)gcIntence);
            }
            
            return Colors.HotPink;
        }
    }
}