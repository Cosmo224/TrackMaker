﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection; 
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    class Update
    {
        // Check for updates.
        static void Main(string[] args)
        {
            WebClient NetConnection = new WebClient();

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                string VersionString = NetConnection.DownloadString(@"https://trackmaker-update.medicanecentre.org/LatestVersion.txt");

                string[] VersionComponents = VersionString.Split('.');

                // 2 = build
                double CurBuildNumber = Convert.ToDouble(VersionComponents[2]);

                FileVersionInfo FVI = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

                if (CurBuildNumber > FVI.FileBuildPart)
                {
                    if (MessageBox.Show($"An update is available.\n\nCurrent Version: {FVI.ProductVersion}.\nNew Version: {VersionString}.\n\nDo you wish to update?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        NetConnection.DownloadFile($"https://trackmaker-update.medicanecentre.org/TrackMaker-{VersionString}.zip", "new.zip");
                        
                        if (Directory.Exists(@".\update"))
                        {
                            foreach (string FileName in Directory.EnumerateFiles($@".\update"))
                            {
                                File.Delete(FileName); 
                            }

                            foreach (string DirName in Directory.EnumerateDirectories($@"{Directory.GetCurrentDirectory()}\update"))
                            {
                                string[] TempArray = DirName.Split('\\');
                                string DirNameTemp = TempArray[TempArray.Length - 1];

                                foreach (string FileName in Directory.EnumerateFiles($@".\update\{DirNameTemp}"))
                                {
                                    // Don't delete ourselves!

                                    File.Delete(FileName);
                                }
 
                            }
                        }

                        // Extract and update.
                        ZipFile.ExtractToDirectory($"new.zip", @".\update");

                        Updater.Update(); 
                    }
                    else
                    {
                        Application.Current.Shutdown(0); // User rejected the update.
                    }
                }
            }
            catch (ArgumentException err)
            {
                MessageBox.Show($"An error occurred while updating.\n\n{err}", "Error 2", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(2);
            }
            catch (WebException err)
            {
                MessageBox.Show($"An error occurred while updating.\n\n{err}", "Error 1", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1); 
            }
            finally
            {
                NetConnection.Dispose();
            }
        }

    }

    public static class Updater
    {
        public static void Update()
        {

            // Kill all instances of the track maker..
            foreach (Process Proc in Process.GetProcessesByName("Track Maker"))
            {
                Proc.Kill();
            }


            foreach (string DirName in Directory.EnumerateDirectories(Directory.GetCurrentDirectory()))
            {
                string[] TempArray = DirName.Split('\\');
                string DirNameTemp = TempArray[TempArray.Length - 1];

                // Delete all track maker files. 
                foreach (string FileName in Directory.EnumerateFiles($@"{Directory.GetCurrentDirectory()}\{DirNameTemp}"))
                {
                    // Don't delete ourselves!
                    if (!FileName.Contains("Update")
                        && !FileName.Contains(".ico")
                        && !FileName.Contains(".txt")
                        && !FileName.Contains(".docx")
                        && !FileName.Contains(".cs")
                        && !FileName.Contains(".xaml")
                        && !FileName.Contains(".odt")
                        && !FileName.Contains(".cmd")
                        )
                    {
                        File.Delete(FileName);
                    }
                }
            }


            // Set our current dir
            Directory.SetCurrentDirectory(@".\update");

            // Copy the new files. 

            foreach (string FileName in Directory.EnumerateFiles($@"{Directory.GetCurrentDirectory()}"))
            {
                // Don't copy over ourselves. The batch file does this.

                string[] TempArray0 = FileName.Split('\\');
                string FileNameTemp = TempArray0[TempArray0.Length - 1];

                if (!FileName.Contains("Update"))
                {
                    File.Copy(FileName, $@"..\{FileNameTemp}");
                }
            }

            foreach (string DirName in Directory.EnumerateDirectories(Directory.GetCurrentDirectory()))
            {
                string[] TempArray = DirName.Split('\\');
                string DirNameTemp = TempArray[TempArray.Length - 1];

                foreach (string FileName in Directory.EnumerateFiles($@"{Directory.GetCurrentDirectory()}\{DirNameTemp}"))
                {
                    // Don't copy over ourselves. The batch file does this.

                    string[] TempArray2 = FileName.Split('\\');
                    string FileNameTemp = TempArray2[TempArray2.Length - 1];

                    if (!FileName.Contains("Update"))
                    {
                        File.Copy(FileName, $@"..\{DirNameTemp}\{FileNameTemp}");
                    }
                }
            }

            Directory.SetCurrentDirectory(@"..");

            //

            File.Delete("new.zip");
            // Completion batchfile that updates this updater.
            Process.Start(@"..\UpdateComplete.cmd");
        }
    }

}
