﻿using Microsoft.Win32; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Track_Maker
{
    /// <summary>
    /// TProj-v2 format
    /// 
    /// Priscilla v442
    /// 
    /// 2020-09-13
    /// 
    /// Version 2.0a     Priscilla v442      Dummy only
    /// Version 2.0b     Priscilla v443      Began functionality
    /// Version 2.0c     Priscilla v445      Based code off of v1 - added metadata and project based
    /// Version 2.1      Priscilla v447      Bug fixes, IsSelected & IsOpen
    /// Version 2.2      Priscilla v453      Importing, BasinName => Name
    /// Version 2.2b     Priscilla v455      Add full importing code   
    /// </summary>
    /// 
    public class XMLv2 : IExportFormat
    {
        public bool AutoStart { get; set; }
        public string Name { get; set; }
        public static int FormatVersionMajor = 2;
        public static int FormatVersionMinor = 2;
        public XMLv2()
        {
            AutoStart = false;
            Name = "Project Format 2.x";
        }

        /// <summary>
        /// Get the name of the XMLv2 class. 
        /// </summary>
        /// <returns></returns>
        public string GetName() => Name;

        public bool Export(Project Project)
        {
            try
            {
                SaveFileDialog SFD = new SaveFileDialog();

                SFD.Title = "Export to project (version 2.x)...";
                SFD.Filter = "Track Maker Project XML files|*.tproj";
                SFD.ShowDialog();

                // user hit cancel
                if (SFD.FileName == "") return true;

                // If it exists, delete it
                if (File.Exists(SFD.FileName))
                {
                    File.Delete(SFD.FileName);
                    FileStream FS = File.Create(SFD.FileName);
                    FS.Close();
                }

                // Export.
                ExportCore(Project, SFD.FileName);

                return true;
            }
            // error checking
            catch (IOException err)
            {
                MessageBox.Show($"An error occurred while writing to XML format. [Error Code: EX1].\n\n{err}");
                Application.Current.Shutdown(-0xE1);
                return false;
            }
            catch (XmlException err)
            {
                MessageBox.Show($"An error occurred while saving the XML file. [Error Code: EX2].\n\n{err}");
                Application.Current.Shutdown(-0xE2);
                return false;
            }
        }

        public bool ExportCore(Project Project, string FileName)
        {
            XmlDocument XDoc = new XmlDocument();
            XmlNode XRoot = XDoc.CreateElement("Project");

            // Version 2.0: Write the metadata
            XmlNode XMetadataNode = XDoc.CreateElement("Metadata"); 
            // Version of the format. 
            XmlNode XFormatVersionMajor = XDoc.CreateElement("FormatVersionMajor");
            XmlNode XFormatVersionMinor = XDoc.CreateElement("FormatVersionMinor");

            XmlNode XTimestamp = XDoc.CreateElement("Timestamp");

            XFormatVersionMajor.InnerText = FormatVersionMajor.ToString();
            XFormatVersionMinor.InnerText = FormatVersionMinor.ToString();

            // ISO 8601 date format
            XTimestamp.InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            XmlNode XBasinsNode = XDoc.CreateElement("Basins"); 

            foreach (Basin Bas in Project.OpenBasins)
            {
                XmlNode XBasinNode = XDoc.CreateElement("Basin");
                XmlNode XBasinNameNode = XDoc.CreateElement("UserTag");
                XmlNode XBasinNameBasin = XDoc.CreateElement("Name");
                XmlNode XBasinIsOpen = XDoc.CreateElement("IsOpen");
                XmlNode XBasinIsSelected = XDoc.CreateElement("IsSelected");
                XmlNode XBasinLayers = XDoc.CreateElement("Layers");
                
                XBasinNameNode.InnerText = Bas.UserTag;
                XBasinNameBasin.InnerText = Bas.Name;
                XBasinIsOpen.InnerText = Bas.IsOpen.ToString();
                XBasinIsSelected.InnerText = Bas.IsSelected.ToString();
                // build a layer

                foreach (Layer Lay in Bas.Layers)
                {
                    XmlNode XLayerNode = XDoc.CreateElement("Layer");
                    XmlNode XLayerNameNode = XDoc.CreateElement("Name");
                    XmlNode XLayerGUIDNode = XDoc.CreateElement("GUID");
                    XmlNode XStormsNode = XDoc.CreateElement("Storms");

                    XLayerNameNode.InnerText = Lay.Name;
                    XLayerGUIDNode.InnerText = Lay.LayerId.ToString(); 

                    // dump the storm info to file
                    foreach (Storm XStorm in Lay.AssociatedStorms)
                    {
                        // create the xml nodes.
                        XmlNode XStormNode = XDoc.CreateElement("Storm");
                        XmlNode XStormFormationDate = XDoc.CreateElement("FormationDate");
                        XmlNode XStormID = XDoc.CreateElement("ID");
                        XmlNode XStormName = XDoc.CreateElement("Name");
                        XmlNode XStormNodeList = XDoc.CreateElement("Nodes");
                        XmlNode XStormNodeListDel = XDoc.CreateElement("DeletedNodes"); // the undone nodes

                        // set the basic info - name etc
                        XStormFormationDate.InnerText = XStorm.FormationDate.ToString();
                        XStormID.InnerText = XStorm.Id.ToString();
                        XStormName.InnerText = XStorm.Name;

                        // populate the node list
                        foreach (Node XNode in XStorm.NodeList)
                        {
                            // add new nodes
                            XmlNode XNodeNode = XDoc.CreateElement("Node");
                            XmlNode XNodeIntensity = XDoc.CreateElement("Intensity");
                            XmlNode XNodePosition = XDoc.CreateElement("Position");
                            XmlNode XNodeType = XDoc.CreateElement("Type");

                            // set the info
                            XNodeIntensity.InnerText = XNode.Intensity.ToString();
                            XNodePosition.InnerText = XNode.Position.ToStringEmerald();
                            XNodeType.InnerText = XNode.NodeType.ToString();

                            // build the node list xml structure
                            XNodeNode.AppendChild(XNodeIntensity);
                            XNodeNode.AppendChild(XNodePosition);
                            XNodeNode.AppendChild(XNodeType);
                            XStormNodeList.AppendChild(XNodeNode);
                        }

                        // Populate the deleted node list xmlinfo for the basin save information.

                        foreach (Node XNode in XStorm.NodeList_Deleted)
                        {
                            // add new nodes
                            XmlNode XNodeNode = XDoc.CreateElement("Node");
                            XmlNode XNodeIntensity = XDoc.CreateElement("Intensity");
                            XmlNode XNodePosition = XDoc.CreateElement("Position");
                            XmlNode XNodeType = XDoc.CreateElement("Type");

                            // set the info
                            XNodeIntensity.InnerText = XNode.Intensity.ToString();
                            XNodePosition.InnerText = XNode.Position.ToStringEmerald();
                            XNodeType.InnerText = XNode.NodeType.ToString();

                            // build the node list xml structure
                            XNodeNode.AppendChild(XNodeIntensity);
                            XNodeNode.AppendChild(XNodePosition);
                            XNodeNode.AppendChild(XNodeType);
                            XStormNodeListDel.AppendChild(XNodeNode);

                        }
                        // build perstorm xml

                        XStormNode.AppendChild(XStormFormationDate);
                        XStormNode.AppendChild(XStormID);
                        XStormNode.AppendChild(XStormName);
                        XStormNode.AppendChild(XStormNodeList);
                        XStormNode.AppendChild(XStormNodeListDel);

                        XStormsNode.AppendChild(XStormNode);

                    }

                    XLayerNode.AppendChild(XLayerNameNode);
                    XLayerNode.AppendChild(XLayerGUIDNode);
                    XLayerNode.AppendChild(XStormsNode);

                    XBasinLayers.AppendChild(XLayerNode); 
                }

                // Build the Basins node

                XBasinNode.AppendChild(XBasinNameNode);
                XBasinNode.AppendChild(XBasinNameBasin);
                XBasinNode.AppendChild(XBasinLayers);

                XBasinsNode.AppendChild(XBasinNode);
            }

            // build metadata

            XMetadataNode.AppendChild(XFormatVersionMajor);
            XMetadataNode.AppendChild(XFormatVersionMinor); 

            XRoot.AppendChild(XMetadataNode);
           
            // build storms
            XRoot.AppendChild(XBasinsNode);

            XDoc.AppendChild(XRoot);

            XDoc.Save(FileName);

            return true;
        }

        /// <summary>
        /// I have to compromise against my old shit code
        /// </summary>
        /// <returns>The imported basin.</returns>
        public Project Import()
        {
            // Implement later

            try
            {
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.Title = "Import from project";
                OFD.Filter = "Track Maker Project files|*.tproj";
                OFD.ShowDialog();

                if (OFD.FileName == null)
                {
                    return null; 
                }
                else
                {
                    XMLImportResult XER = ImportCore(SFD.FileName);

                    if (XER.Successful && !XER.Cancelled)
                    {
                        return XER.Project;
                    }
                    else
                    {
                        return null; 
                    }
                    
                }
            }
            catch (XmlException)
            {
                Error.Throw("Invalid basin!", "One of the basins in this Project2 file is corrupted!", ErrorSeverity.Error, 121);
                return null;
            }
        }

        public XMLImportResult ImportCore(string FileName)
        {
            XMLImportResult XER = new XMLImportResult();
            XER.Successful = false;
            
            XmlDocument XD = new XmlDocument();
            XD.Load(FileName);

            XmlNode XDR = XD.FirstChild;

            Project Proj = new Project();

            if (XDR.HasChildNodes) return XER;

            foreach (XmlNode XDRA in XDR.ChildNodes)
            {
                switch (XDRA.Name)
                {
                    case "Basin":

                        // Create a new basin.
                        Basin Bas = new Basin();

                        if (!XDRA.HasChildNodes)
                        {
                            Error.Throw("Invalid basin!", "One of the basins in this Project2 file is corrupted!", ErrorSeverity.Error, 121);
                            return XER; 
                        }
                        else
                        {
                            XmlNodeList XDRAList = XDRA.ChildNodes;
                            // Iterate through the child nodes of the basin.
                            foreach (XmlNode XDRAC in XDRAList)
                            {
                                switch (XDRAC.Name)
                                {
                                    case "Name": // The name of this basin. Triggers a GlobalState load.
                                        Bas = Proj.GetBasinWithName(XDRAC.Value);
                                        continue;
                                    case "UserTag": // The user-given name of this basin
                                        Bas.UserTag = XDRAC.Value;
                                        continue;
                                    case "IsOpen": // Not sure if I'll use this
                                        Bas.IsOpen = Convert.ToBoolean(XDRAC.Value);
                                        continue;
                                    case "IsSelected": // Not sure if I'll use this
                                        Bas.IsSelected = Convert.ToBoolean(XDRAC.Value);
                                        continue;
                                    case "Layers":
                                        
                                        // Detect if somehow an invalid layer was created
                                        if (!XDRAC.HasChildNodes)
                                        {
                                            Error.Throw("Invalid basin!", "There are no layers!", ErrorSeverity.Error, 122);
                                            return XER;
                                        }
                                        else
                                        {
                                            // Iterate through the layers
                                            XmlNodeList XDRACList = XDRAC.ChildNodes;

                                            foreach (XmlNode XDRACL in XDRACList)
                                            {
                                                switch (XDRACL.Name)
                                                {
                                                    case "Layer":
                                                        if (!XDRACL.HasChildNodes)
                                                        {
                                                            Error.Throw("Invalid basin!", "Empty layer detected!", ErrorSeverity.Error, 123);
                                                            return XER;
                                                        }
                                                        else
                                                        {
                                                            XmlNodeList XDRACLList = XDRACL.ChildNodes;

                                                            // Yeah
                                                            foreach (XmlNode XDRACLL in XDRACLList)
                                                            {
                                                                Layer Lyr = new Layer(); 

                                                                switch (XDRACLL.Name)
                                                                {
                                                                    case "GUID": // GUID of this basin
                                                                        Lyr.LayerId = Guid.Parse(XDRACL.Value);
                                                                        continue;
                                                                    case "Name": // Name of this basin
                                                                        Lyr.Name = XDRACL.Value;
                                                                        continue;
                                                                    case "Storms": // Storms of this basin

                                                                        if (!XDRACLL.HasChildNodes)
                                                                        {
                                                                            continue; // there may be no storms
                                                                        }
                                                                        else
                                                                        {
                                                                            // Find each storm node
                                                                            
                                                                            XmlNodeList XDRACLLSList = XDRACLL.ChildNodes;

                                                                            foreach (XmlNode XDRACLLS in XDRACLLSList)
                                                                            {
                                                                                
                                                                                switch (XDRACLLS.Name)
                                                                                {
                                                                                    case "Storm":
                                                                                        Storm Sto = new Storm();
                                                                                        if (!XDRACLLS.HasChildNodes)
                                                                                        {
                                                                                            Error.Throw("Invalid basin!", "Empty layer detected!", ErrorSeverity.Error, 123);
                                                                                            return XER;
                                                                                        }
                                                                                        else
                                                                                        {

                                                                                            XmlNodeList XDRACLLSSList = XDRACLLS.ChildNodes;

                                                                                            foreach (XmlNode XDRACLLSS in XDRACLLSSList)
                                                                                            {
                                                                                                switch (XDRACLLSS.Name)
                                                                                                {
                                                                                                    case "FormationDate": // The formation date of this system.
                                                                                                        Sto.FormationDate = DateTime.Parse(XDRACLLSS.Value); 
                                                                                                        continue;
                                                                                                    case "ID": // The ID of this system.
                                                                                                        Sto.Id = Convert.ToInt32(XDRACLLSS.Value);
                                                                                                        continue;
                                                                                                    case "Name": // Name of this system.
                                                                                                        Sto.Name = XDRACLLSS.Value;
                                                                                                        continue;
                                                                                                    case "Nodes":
                                                                                                        NodeImportResult NIR = ImportNodes(XDRACLLSS); 

                                                                                                        if (NIR.Successful && !NIR.Empty)
                                                                                                        {
                                                                                                            Sto.NodeList = NIR.Nodes;
                                                                                                        }

                                                                                                        continue;
                                                                                                    case "DeletedNodes":
                                                                                                        NodeImportResult DNIR = ImportNodes(XDRACLLSS);

                                                                                                        if (DNIR.Successful && !DNIR.Empty)
                                                                                                        {
                                                                                                            Sto.NodeList_Deleted = DNIR.Nodes;
                                                                                                        }

                                                                                                        continue;
                                                                                                    
                                                                                                }
                                                                                            }

                                                                                            // Get the storm nodes


                                                                                        }

                                                                                        Lyr.AddStorm(Sto);
                                                                                        continue;
                                                                                }
                                                                                 
                                                                            }

                                                                        }

                                                                        continue;
                                                                }

                                                                if (Lyr.Name == null)
                                                                {
                                                                    Error.Throw("Invalid basin!", "Layer with no name!", ErrorSeverity.Error, 125);
                                                                    return XER;
                                                                }

                                                                Bas.AddLayer(Lyr.Name);
                                                            }

                                                        }

                                                        continue;


                                                }

                                            }
                                        }
                                        
                                        continue;
                                }
                            }
                        }

                        Proj.AddBasin(XDRA.Value);

                        continue; 
                }
            }

            XER.Successful = true;
            XER.Project = Proj; 
            return XER;
        } 

        private protected NodeImportResult ImportNodes(XmlNode XNN)
        {

            NodeImportResult NIR = new NodeImportResult(); 

            if (!XNN.HasChildNodes)
            {
                NIR.Successful = true;
                NIR.Empty = true;
                return NIR; 
            }
            else
            {
                foreach (XmlNode XNNChild in XNN.ChildNodes)
                {
                    Node XNNN = new Node(); 

                    switch (XNNChild.Name)
                    {
                        case "Intensity":
                            XNNN.Intensity = Convert.ToInt32(XNNChild.Value);
                            continue;
                        case "Position":
                            XNNN.Position = Utilities.SplitXY(XNNChild.Value);
                            continue;
                        case "Type":
                            XNNN.NodeType = (StormType)Enum.Parse(typeof(StormType), XNNChild.Value);
                            continue; 
                    }

                    NIR.Nodes.Add(XNNN); 
                }

                NIR.Successful = true;
                NIR.Empty = false;
                return NIR; 
            }



        }
        
        /// <summary>
        /// This will be removed - export formats will not generate previews in v2
        /// </summary>
        /// <param name="Canvas"></param>
        public void GeneratePreview(Canvas Canvas)
        {
            throw new NotImplementedException();
        }
    }
}
