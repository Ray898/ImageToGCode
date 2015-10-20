﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageToGCode.Engine
{
    class ImageByLinesPresenter
    {
        private readonly List<Line> _Lines;
        private readonly Image _Image;
        public List<Line> Lines { get { return _Lines; } }

        public ImageByLinesPresenter(Image image)
        {
            _Image = image;
            _Lines = new List<Line>();
        }

        public void Present(Vector normalVector, double lineResolution, double pointResolution)
        {
            Lines.Clear();

            Vector vectorIncrement = normalVector.Normalize() * lineResolution;


            Vector startingPoint;
            Vector endPoint;
            Vector currentVector;
            if(vectorIncrement.X < 0)
            {
                startingPoint = new Vector(0, _Image.Height - 1);
                endPoint = new Vector(_Image.Width - 1, 0);
                vectorIncrement = vectorIncrement.Reverse();
                currentVector = Line.GetIntersection(new Line(vectorIncrement, startingPoint, _Image), new Line(new Vector(-vectorIncrement.Y, vectorIncrement.X), new Vector(0,0), _Image));
            }
            else
            {
                startingPoint = new Vector(0, 0);
                endPoint = new Vector(_Image.Width - 1, _Image.Height - 1);
                currentVector = vectorIncrement;
            }

            Lines.Add(new Line(vectorIncrement, startingPoint, _Image)); //узнать, что с цветом НЕ ТАК!

            var BorderLine = new Line(vectorIncrement, endPoint, _Image);
            var IncrementLine = new Line(new Vector(-vectorIncrement.Y, vectorIncrement.X), new Vector(0, 0), _Image);
            var Intersection = Line.GetIntersection(BorderLine, IncrementLine);


            while (currentVector.X <= Intersection.X && (currentVector.Y * Intersection.Y < 0 || Math.Abs(currentVector.Y) <= Math.Abs(Intersection.Y)))
            {
                Lines.Add(new Line(currentVector, _Image));
                currentVector += vectorIncrement;
            }

            foreach (var item in Lines)
            {
                item.GeneratePixels(pointResolution);
            }

        }
    }
}
