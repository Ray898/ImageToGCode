﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageToGCode.Engine.GCodeGeneration.VectorProcessor
{
    class VPathGroup : INotifyPropertyChanged
    {
        private int _Feed = 400;
        private int _Power = 40;
        private bool _Engrave = true;
        public Color PathColor { get; private set; }
        public System.Windows.Media.Brush Brush 
        { 
            get 
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(PathColor.A, PathColor.R, PathColor.G, PathColor.B));
            } 
        }
        public int Power
        {
            get
            {
                return _Power;
            }
            set
            {
                if (_Power == value)
                    return;
                _Power = value;
                RaisePropertyChanged("Power");
            }
        }
        public int Feed
        {
            get
            {
                return _Feed;
            }
            set
            {
                if (_Feed == value)
                    return;
                _Feed = value;
                RaisePropertyChanged("Feed");
            }
        }
        public bool Engrave
        {
            get { return _Engrave; }
            set
            {
                if (_Engrave == value)
                    return;
                _Engrave = value;
                RaisePropertyChanged("Engrave");
            }
        }
        public List<GraphicsPath> PathList { get; private set; }

        public VPathGroup(Color pathColor) 
        {
            PathColor = pathColor;
            PathList = new List<GraphicsPath>();
        }

        public VPathGroup(Color pathColor, IEnumerable<GraphicsPath> paths) : this(pathColor)
        {
            PathList.AddRange(paths);
        }

        #region IPropertyChanged
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        private void OnPropertyChanged(string propertyName)
        {
            if (SynchronizationContext.Current != _synchronizationContext)
                RaisePropertyChanged(propertyName);
            else
                _synchronizationContext.Post(RaisePropertyChanged, propertyName);
        }
        private void RaisePropertyChanged(object param)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs((string)param));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}