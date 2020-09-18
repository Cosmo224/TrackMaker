﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; 

/// <summary>
/// starfrost's Hurricane Track Maker Export Platform Interface
/// 
/// This allows us to write classes easily and quickly for exporting to any format we want. 
/// 
/// Version 1.3.0
/// September 18, 2020
/// 
/// (Priscilla - v454)
/// </summary>
/// 
namespace Track_Maker
{
    public interface IExportFormat
    {
        bool AutoStart { get; set; } // Does it auto-start?
        string Name { get; set; } // The name of the file format to export to.
        string GetName(); // Returns the name of this ExportFormat. 
        Project Import(); // Import from this file format.
        bool Export(Project Project); // Export from this file format. 
        bool ExportCore(Project Project, string FileName); // Does the actual exporting. 
        void GeneratePreview(Canvas ImportCanvas); // Generate a preview (for import only; the export preview is handled by the Track Maker). 
    }
}
