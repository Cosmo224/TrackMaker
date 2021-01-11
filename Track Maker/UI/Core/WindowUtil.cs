﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Track_Maker
{
    public enum Direction { Smaller, Larger }
    public partial class MainWindow : Window
    {
        // Todo: Handle the Priscilla Sidebar
        internal void SetFullscreen()
        {
            switch (Setting.WindowStyle)
            {
                // MOVE THIS CODE
                case WndStyle.Windowed: // if it's false, turn it on
                    CurrentProject.SelectedBasin.RecalculateNodePositions(true, new Point(Width, Height));
                    Setting.WindowStyle = WndStyle.Fullscreen;
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.None;
                    MainMenu.Width = SystemParameters.PrimaryScreenWidth;
                    HurricaneBasin.Width = SystemParameters.PrimaryScreenWidth;
                    HurricaneBasin.Height = SystemParameters.PrimaryScreenHeight - MainMenu.Height; // MOVE THIS CODE 
                    PriscillaSidebar.Margin = new Thickness(SystemParameters.PrimaryScreenWidth - 181, 0, 0, 0); 
                    return;
                case WndStyle.Fullscreen: // if it's true, turn it off
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    MainMenu.Width = Width;
                    HurricaneBasin.Width = Width;
                    HurricaneBasin.Height = Height - MainMenu.Height; // MOVE THIS CODE
                    PriscillaSidebar.Margin = new Thickness(Width - 191, 0, 0, 0);
                    CurrentProject.SelectedBasin.RecalculateNodePositions(false, new Point(Width, Height));
                    Setting.WindowStyle = WndStyle.Windowed;
                    return;
            }
        }
    }
}
