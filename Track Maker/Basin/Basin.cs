﻿using Starfrost.UL5.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace Track_Maker
{
    // Custom hemispheres? (3.x)
#if DANO
    public enum BasinType { Track, Animation };

    public enum Hemisphere { North, South }; 
#endif


    public class Basin
    {
        public string Abbreviation { get; set; } // the official abbreviation of this basin
        public BitmapImage Image { get; set; } // the basin image name
        public string ImagePath { get; set; } // the basin image path. we only load what we need.
        public string Name { get; set; } // the name of the basin
        public Coordinate CoordsLower { get; set; } // The lower point of the coords of this basin
        public Coordinate CoordsHigher { get; set; } // The highest point of the coords of this basin
        // Storms removed for Priscilla 462 
        
        // New for Dano M1 and Priscilla
        public Point FocusPoint { get; set; } // The focus point of the camera inside this basin.
        public int ZoomLevel { get; set; } // The zoom level of the camera inside this basin.
        public int Year { get; set; } // The year in which the season took place.
#if DANO
        public Hemisphere SeasonHemisphere { get; set; } // The hemisphere
        
        public BasinType SeasonType { get; set; } // Type of this season (Dano/3.0 only)
#endif 
        public Guid SeasonID { get; set; } // Unique ID of this season

        // New for Priscilla.
        public List<Layer> Layers { get; set; } // new: list of layers
        public Layer CurrentLayer { get; set; } // set selected layer
        public bool IsOpen { get; set; } // is this basin open?
        public bool IsSelected { get; set; } // is this basin selected?
        public string UserTag { get; set; } // User-given season name (Priscilla v445)

        public Basin()
        {
            Layers = new List<Layer>();
            SeasonID = Guid.NewGuid(); 
        }

        
        // Basin API for Priscilla
        public Coordinate FromNodePositionToCoordinate(Point NodePosition)
        {
            Coordinate Coord = new Coordinate();

            //Convert (coords lower/higher are alwa
            double X1 = CoordsLower.Coordinates.X;
            double Y1 = CoordsLower.Coordinates.Y;
            double X2 = CoordsHigher.Coordinates.X;
            double Y2 = CoordsHigher.Coordinates.Y;

            // Convert to negative

            foreach (CardinalDirection Cardir in CoordsLower.Directions)
            {
                switch (Cardir)
                {
                    case CardinalDirection.W:
                        X1 = -X1;
                        continue; 
                    case CardinalDirection.S:
                        Y1 = -Y1;
                        continue;
                }
            }

            foreach (CardinalDirection Cardir in CoordsHigher.Directions)
            {
                switch (Cardir)
                {
                    case CardinalDirection.W:
                        X2 = -X2;
                        continue;
                    case CardinalDirection.S:
                        Y2 = -Y2;
                        continue;
                }
            }

            // convert to 1. Get rid of this mainwindow shit asap in dano.
            MainWindow MnWindow = (MainWindow)Application.Current.MainWindow; 
            double _ = NodePosition.X / MnWindow.Width;
            double _2 = NodePosition.Y / MnWindow.Height;

            double FinalX = (X1 + (X2 - X1)) * _;
            double FinalY = (Y1 + (Y2 - Y1)) * _2;

            string _s = FinalX.ToString();
            string _s2 = FinalY.ToString();

            string[] _s3 = _s.Split('.');
            string[] _s5 = _s2.Split('.');

            if (_s3.Length != 0)
            {
                // Truncate to the first decimal point if there are decimal points
                string _s4 = _s3[1].Substring(0, 3 - _s3[0].Length);

                // Concanectate
                _s4 = $"{_s3[0]}{_s4}";

                // Convert back
                FinalX = Convert.ToDouble(_s4);
            }

            if (_s5.Length != 0)
            {
                // Truncate to the first decimal point if there are decimal points
                string _s6 = _s5[1].Substring(0, 1);

                // Concanectate
                _s6 = $"{_s5[0]}{_s6}";

                // Convert back
                FinalY = Convert.ToDouble(_s6);
            }

            Coord.Directions = new List<CardinalDirection>();

            if (FinalX < 0)
            {
                FinalX = -FinalX;
                Coord.Directions.Add(CardinalDirection.W);
            }
            else
            {
                Coord.Directions.Add(CardinalDirection.E);
            }

            if (FinalY < 0)
            {
                FinalY = -FinalY;
                Coord.Directions.Add(CardinalDirection.S);
            }
            else
            {
                Coord.Directions.Add(CardinalDirection.N);
            }

            Coord.Coordinates = new Point(FinalX, FinalY);

            return Coord; 
        }

        public Point FromCoordinateToNodePosition(Coordinate Coord)
        {

            Debug.Assert(Coord.Directions.Count == 2); 

            double LowX = CoordsLower.Coordinates.X;
            double LowY = CoordsLower.Coordinates.Y;
            double HighX = CoordsHigher.Coordinates.X;
            double HighY = CoordsHigher.Coordinates.Y;

            // convert to absolute coordinate
            foreach (CardinalDirection CD in CoordsLower.Directions)
            {
                switch (CD)
                {
                    case CardinalDirection.W:
                        LowX = -LowX;
                        continue;
                    case CardinalDirection.S:
                        LowY = -LowY;
                        continue; 
                }
            }

            // convert to absolute coordinate
            foreach (CardinalDirection CD in CoordsHigher.Directions)
            {
                switch (CD)
                {
                    case CardinalDirection.W:
                        HighX = -HighX;
                        continue;
                    case CardinalDirection.S:
                        HighY = -HighY;
                        continue;
                }
            }

            // get the multiplier from the coordshigher.

            double PreFinalX = Coord.Coordinates.X / (HighX - LowX);
            double PreFinalY = Coord.Coordinates.Y / (HighY - LowY);
            
            // TEMP
            MainWindow MnWindow = (MainWindow)Application.Current.MainWindow;
            Point FinalPos = new Point(MnWindow.Width * PreFinalX, MnWindow.Height * PreFinalY);

            return FinalPos;
        }

        /// <summary>
        /// Adds a Storm.
        /// </summary>
        /// <returns></returns>
        public bool AddStorm(string Name, DateTime DateTime)
        {
            try
            {
                Storm Storm = new Storm
                {
                    Id = GetFlatListOfStorms().Count, // this actually makes sense.
                    Name = Name
                };

                Logging.Log($"Adding Storm with id {Storm.Id} and name {Storm.Name}");

                if (Storm.Name == "")
                {
                    Error.Throw("You must add a name!", "Track Maker", ErrorSeverity.Warning, 2);
                    Storm = null;
                    return false;
                }

                // Check the date. [2020-05-13] - convert from nullable datetime to datetime?

                if (DateTime == null)
                {
                    Error.Throw("You must add a date and time!", "Error", ErrorSeverity.Warning, 1);
                    return false;
                }

                Storm.FormationDate = DateTime; // WHY IS THIS NULLABLE I MEAN ITS HELPFUL BUT YES

                // Just starting from what we already had here.

                List<Storm> FlatList = GetFlatListOfStorms();

                if (FlatList.Count != 0)
                {
                    if (Storm.FormationDate < FlatList[0].FormationDate)
                    {
                        MessageBox.Show("You can't have a Storm start earlier than the season!", "Error I1", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

#if DANO
                StormTypeManager ST2Manager = GlobalState.GetST2Manager(); 
#else
                MainWindow Pre3MainWindow = (MainWindow)Application.Current.MainWindow;
                StormTypeManager ST2Manager = Pre3MainWindow.ST2Manager;
#endif

                //Storm.StormType = ST2Manager.GetStormTypeWithName(Type); 


                //Logging.Log($"Set storm type to {Storm.StormType}");

                Logging.Log("Initializing node list...");
                Storm.NodeList = new List<Node>(); // initalize the mode list
                Logging.Log("Adding Storm to basin Storm list...");
                CurrentLayer.AssociatedStorms.Add(Storm);
                Logging.Log("Setting current Storm...");
                CurrentLayer.CurrentStorm = Storm; 
                Logging.Log("Done! Closing...");

                return true;
            }
            catch (FormatException)
            {
                Error.Throw("You must enter a valid date and time!", "Error", ErrorSeverity.Warning, 3);
                return false;
            }
        }

        /// <summary>
        /// ATCF import helper function overload for AddStorm if you've already created a storm object in advance
        /// </summary>
        /// <param name="Sto"></param>
        public void AddStorm(Storm Sto) => CurrentLayer.AssociatedStorms.Add(Sto);

        public bool RenameStormWithName(string OldName, string NewName)
        {
            // todo: experiment with reducing code complexity
            List<Storm> LS = new List<Storm>();

            foreach (Layer Lyr in Layers)
            {
                foreach (Storm Sto in Lyr.AssociatedStorms)
                {
                    if (Sto.Name == OldName)
                    {
                        Sto.Name = NewName;
                        return true; 
                    }
                }
            }

            return false; 
        }

        /// <summary>
        /// Remove the storm 
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool RemoveStormWithName(string Name)
        {
            foreach (Layer Lyr in Layers)
            {
                foreach (Storm Sto in Lyr.AssociatedStorms)
                {
                    if (Sto.Name == Name)
                    {
                        Lyr.AssociatedStorms.Remove(Sto);
                        return true; 
                    }
                    
                }
            }

            return false; 

        }
        
        public bool SelectStormWithName(string StName)
        {
            foreach (Layer Lyr in Layers)
            {
                foreach (Storm Sto in Lyr.AssociatedStorms)
                {
                    // select the current layer
                    if (!(CurrentLayer == Lyr))
                    {
                        CurrentLayer = Lyr;
                    }

                    if (Sto.Name == StName) CurrentLayer.CurrentStorm = Sto;
                    return true; 

                }
            }

            return false;
        }

        public void AddLayer(string Name)
        {
            Layer Layer = new Layer();
            Layer.Name = Name;
            Layers.Add(Layer);
#if PRISCILLA // V2.0 only (hack)
            // TEMP
            MainWindow MnWindow = (MainWindow)Application.Current.MainWindow;
            MnWindow.Layers.AddLayer(Name); 
#endif
        }

        public void SelectLayer(string Name)
        {
            foreach (Layer Lyr in Layers)
            {
                if (Lyr.Name == Name) CurrentLayer = Lyr;
            }
        }

        public void DeleteLayer(string Name)
        {
            foreach (Layer Layer in Layers)
            {
                if (Layer.Name == Name)
                {
                    Layers.Remove(Layer);
                    break;
                }
            }

            return;
        }

        /// <summary>
        /// Load the image with path ImagePath and set it as the basin image.
        /// </summary>
        /// <param name="ImagePath"></param>
        public void LoadImage(string ImgPath)
        {
            ImagePath = ImgPath;
            Image = new BitmapImage(); 
            Image.BeginInit();
            Image.UriSource = new Uri(ImagePath, UriKind.RelativeOrAbsolute); // hopefully valid...hopefully.
            Image.EndInit();
        }

        /// <summary>
        /// When leaving or entering fullscreen mode, recalculate the position of each node so it doesn't end up in the wrong place.
        /// 
        /// Straight MainWindow => Basin port for Priscilla 441, so not the best
        /// </summary>
        public void RecalculateNodePositions(bool IsFullscreen, Point CurWindowSize)
        {
            foreach (Storm StormtoRecalc in GetFlatListOfStorms())
            {
                foreach (Node NodetoRecalc in StormtoRecalc.NodeList)
                {
                    switch (IsFullscreen)
                    {
                        case true:
                            NodetoRecalc.Position = new Point(NodetoRecalc.Position.X / (SystemParameters.PrimaryScreenWidth / CurWindowSize.X), NodetoRecalc.Position.Y / (SystemParameters.PrimaryScreenHeight / CurWindowSize.Y));
                            continue;
                        case false:
                            NodetoRecalc.Position = new Point(NodetoRecalc.Position.X * (SystemParameters.PrimaryScreenWidth / CurWindowSize.X), NodetoRecalc.Position.Y * (SystemParameters.PrimaryScreenHeight / CurWindowSize.Y));
                            continue;

                    }
                }
            }
        }

        // Priscilla 441: Anti-Mainwindow refactoring: Port to Basin class
        public void RecalculateNodePositions(Direction RecalcDir, Point CurWindowSize, Point RecalcRes)
        {
            foreach (Storm StormtoRecalc in GetFlatListOfStorms())
            {
                foreach (Node NodetoRecalc in StormtoRecalc.NodeList)
                {
                    switch (RecalcDir)
                    {
                        case Direction.Smaller:
                            // get it smaller
                            NodetoRecalc.Position = new Point(NodetoRecalc.Position.X / (RecalcRes.X / CurWindowSize.X), NodetoRecalc.Position.Y / (RecalcRes.Y / CurWindowSize.Y));
                            continue;
                        case Direction.Larger:
                            // get it larger
                            NodetoRecalc.Position = new Point(NodetoRecalc.Position.X * (RecalcRes.X / CurWindowSize.X), NodetoRecalc.Position.Y * (RecalcRes.Y / CurWindowSize.Y));
                            continue;

                    }
                }
            }

            return;
        }

        public List<string> GetLayerNames()
        {
            List<string> LayerNames = new List<string>();

            foreach (Layer Lyr in Layers)
            {
                LayerNames.Add(Lyr.Name);
            }

            return LayerNames; 
        }

        /// <summary>
        /// Get a 'flat' (layerless) storm list. Reduces code complexity.[2020-09-25] 
        /// </summary>
        /// <returns></returns>
        public List<Storm> GetFlatListOfStorms()
        {
            List<Storm> LS = new List<Storm>();

            foreach (Layer Lyr in Layers)
            {
                foreach (Storm Sto in Lyr.AssociatedStorms)
                {
                    LS.Add(Sto); 
                }
            }

            return LS; 
        }

        /// <summary>
        /// Get a flat list of storm names
        /// </summary>
        /// <returns></returns>
        public List<string> GetFlatListOfStormNames()
        {
            List<string> StormNames = new List<string>();

            foreach (Layer Lyr in Layers)
            {
                foreach (Storm Sto in Lyr.AssociatedStorms)
                {
                    StormNames.Add(Sto.Name);
                }
            }

            return StormNames; 
        }

        /// <summary>
        /// Get a layer-independent storm with the name Name. 
        /// </summary>
        /// <returns></returns>
        public Storm GetFlatStormWithName(string Name)
        {
            List<Storm> XS = GetFlatListOfStorms();

            foreach (Storm XStorm in XS)
            {
                if (XStorm.Name == Name)
                {
                    return XStorm; 
                }
            }

            return null;

        }
        
        /// <summary>
        /// Before v516 this was utterly fucking retarded.
        /// </summary>
        /// <returns></returns>
        public void ClearBasin()
        {
            foreach (Layer Lyr in Layers)
            {
                Lyr.AssociatedStorms.Clear();
            }

            Layers.Clear();

            return;
        }


        /// <summary>
        /// Internal support for layer ordering
        /// 
        /// Basically just a merge sort IIRC.
        /// </summary>
        /// <returns></returns>
        public List<Layer> BuildListOfZOrderedLayers()
        {
            // Layer

            return Layers.OrderBy(Layers => Layers.ZIndex).ToList();  

        }

        /// <summary>
        /// Checks if a specific layer is in front of another one.
        /// </summary>
        /// <param name="L1"></param>
        /// <param name="L2"></param>
        /// <returns></returns>
        public bool IsInFrontOfLayer(Layer L1, Layer L2)
        {
            return ((L1.ZIndex - L2.ZIndex) < 0); // lower z-index = in front
        }

        /// <summary>
        /// Checks if a specific layer is in front of another one.
        /// </summary>
        /// <param name="L1"></param>
        /// <param name="L2"></param>
        /// <returns></returns>
        public bool IsBehindLayer(Layer L1, Layer L2)
        {
            return ((L1.ZIndex - L2.ZIndex) > 0); // higher z-index = behind
        }

        /// <summary>
        /// Removes the layer with name LayerName.
        /// </summary>
        /// <param name="LayerName">The layer name to be removed</param>
        /// <returns>TRUE if a layer was found, FALSE if an invalid layer name was supplied</returns>
        public bool RemoveLayerWithName(string LayerName)
        {
            foreach (Layer Lyr in Layers)
            {
                if (Lyr.Name == LayerName)
                {
                    Layers.Remove(Lyr);
                    return true;
                }
            }

            return false; 
        }
    }
}