﻿using ImageToGCode.Engine.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ImageToGCode.Engine.GCodeGeneration.ImageProcessor
{
    class IdleStroke : Stroke
    {
        //closestPoint - ближайшая точка прямой 
        //pointForDirection - любая точка на линии разгона
        public IdleStroke(Vector destination)
            : base(destination, 0)
        {

        }
    }
}
