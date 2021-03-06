﻿using TrackMaker.Util.StringUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Windows.Media;

namespace TrackMaker.Core
{
    public partial class CategoryManager
    {
        public void InitCategories()
        {
            try
            {
                // load categorysystems.xml
                XmlDocument XDoc = new XmlDocument();

                XDoc.Load(@"Data\CategorySystems.xml");

                // Check the root node

                XmlNode XParent = XDoc.FirstChild;

                while (XParent.Name != "CategorySystems")
                {
                    if (XParent.NextSibling == null)
                    {
                        // the last node - no valid root node
                        Error.Throw("Error!!", $"CategorySystems.xml is empty or corrupted. The Track Maker cannot start.", ErrorSeverity.FatalError, 193);
                    }

                    XParent = XParent.NextSibling;
                }

                // check that there are actually category systems installed.
                if (!XParent.HasChildNodes)
                {
                    Error.Throw("Error!!", $"No category systems are installed. The Track Maker cannot start.", ErrorSeverity.FatalError, 194);
                }

                XmlNodeList XChildren = XParent.ChildNodes;

                // loop through all the child nodes of categorymanager

                foreach (XmlNode XChild in XChildren)
                {
                    // GET THE KIDS
                    switch (XChild.Name)
                    {
                        case "CategorySystem":
                            CategorySystem CatSystem = new CategorySystem();

                            // can't load an empty category system
                            if (!XParent.HasChildNodes)
                            {
                                // don't throw a fatal error and continue anyways 
                                continue; 
                            }

                            // can't load a non-named category system
                            if (XChild.Attributes.Count == 0)
                            {
                                continue;
                            }

                            XmlAttributeCollection XNieces = XChild.Attributes;

                            foreach (XmlAttribute XNiece in XNieces)
                            {
                                // name shit
                                switch (XNiece.Name)
                                {
                                    case "name":
                                    case "Name": // the name of the category system. 
                                        CatSystem.Name = XNiece.Value;
                                        continue;
                                }
                            }

                            // get the actual category information
                            XmlNodeList XGrandchildren = XChild.ChildNodes;

                            // check the category information

                            foreach (XmlNode XGrandchild in XGrandchildren)
                            {
                                switch (XGrandchild.Name)
                                {
                                    case "Category": // a category in the category system
                                        Category Category = new Category();
                                        
                                        if (XGrandchild.Attributes.Count == 0)
                                        {
                                            // can't load an empty category!
                                            continue;
                                        }

                                        XmlAttributeCollection XGrandnieces = XGrandchild.Attributes;

                                        foreach (XmlAttribute XGrandniece in XGrandnieces)
                                        {
                                            switch (XGrandniece.Name)
                                            {
                                                case "bounds":
                                                case "Bounds": // category bounds
                                                    Point fx = XGrandniece.Value.SplitXY();
                                                    Category.LowerBound = (int)fx.X;
                                                    Category.HigherBound = (int)fx.Y;
                                                    continue;
                                                case "color":
                                                case "colors":
                                                case "colours":
                                                case "Color":
                                                case "Colors":
                                                case "Colour":
                                                case "Colours": // category colour
                                                    // Load the colour using our string extension method
                                                    Category.Color = XGrandniece.Value.SplitRGB();
                                                    continue;
                                                case "name": // the category name
                                                case "Name":
                                                    Category.Name = XGrandniece.Value;
                                                    continue;
                                                case "Abbreviation":
                                                case "Acronym": // BasinSignifier, BasinInitials, BasinAbbreviation...
                                                case "BasinCode":
                                                    Category.Abbreviation = XGrandniece.Value; 
                                                    continue;


                                            }
                                        }

                                        Logging.Log($"Successfully loaded category with name {Category.Name}");
                                        CatSystem.Categories.Add(Category); // yeah
                                        continue;
                                }
                            }

                            // add
                            Logging.Log($"Successfully loaded category system with name {CatSystem.Name}");
                            CategorySystems.Add(CatSystem);
                            continue;
                    }
                }
            }
            catch (XmlException err)
            {
                Error.Throw("Error!!", $"CategorySystems.xml is malformed. The Track Maker cannot start.\n\nExtended Error Information:\n{err}", ErrorSeverity.FatalError, 196);
            }
        }
    }
}
