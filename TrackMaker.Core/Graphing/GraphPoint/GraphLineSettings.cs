﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media; 

namespace TrackMaker.Core
{
    /// <summary>
    /// 2020-01-26               Iris v694
    /// 
    /// Settings for graph lines.
    /// </summary>
    public class GraphLineSettings
    {

        /// <summary>
        /// The colour to be used by this line.
        /// </summary>
        public Color Colour { get; set; }

        /// <summary>
        /// Name used in the graph key for this line
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// The thickness of the line.
        /// </summary>
        public int StrokeThickness { get; set; }
        
        /// <summary>
        /// Is this line visible? If not, simply draw each point
        /// </summary>
        public bool Visible { get; set; } 
    }
}
