﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TrackMaker.Core
{

    /// <summary>
    /// 2020-01-25      Iris v692
    /// 
    /// StormGraph (Implements interface IGraph)
    /// 
    /// Defines a class used for graphing individual storms.
    /// </summary>
    public class StormGraph : IGraph
    {
        public GraphSettings Settings { get; set; }

        public List<GraphLine> Lines { get; set; }

        public bool Plot()
        {
            throw new NotImplementedException();
        }

        public GraphLine GetLineWithName(string KeyName)
        {
            foreach (GraphLine Ln in Lines)
            {
                if (Ln.Settings.KeyName == KeyName) return Ln;

            }

            return null;
        }

        public GraphLine AddLine(string KeyName, int StrokeThickness, Color Colour)
        {

            if (StrokeThickness < 0 || StrokeThickness > 100)
            {
                Error.Throw("Warning!", "Warning: invalid StrokeThickness - must be within the range [0,100]!", ErrorSeverity.Error, 411);
                return null;
            }

            GraphLine Line = new GraphLine();
            Line.Settings.Colour = Colour;
            Line.Settings.KeyName = KeyName;
            Line.Settings.StrokeThickness = StrokeThickness;
            Lines.Add(Line);

            return Line;
        }

        public void DeleteLineWithId(int Id)
        {
            if (Id < 0 || Id > Lines.Count - 1)
            {
                Error.Throw("Error!", "Attempted to delete invalid GraphLine!", ErrorSeverity.Error, 412);
                return;
            }
            else
            {
                Lines.RemoveAt(Id);
            }
        }

        public void DeleteLineWithName(string Name)
        {
            foreach (GraphLine Line in Lines)
            {
                if (Line.Settings.KeyName == Name)
                {
                    Lines.Remove(Line);
                }
            }
        }
    }
}
