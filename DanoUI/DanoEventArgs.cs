﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DanoUI
{
    // Hmm
    public enum DanoUIComponent { StartPage, NewProject, NewSeason, ProjectMain, Export2, Settings, AddNewStorm, AddTrackPoint, BasinSwitcher, CategoryManager, SeasonManager, PackInstaller, AnimationEditor }
    public class DanoEventArgs : EventArgs
    {
        public List<object> DanoParameters { get; set; }
        public bool LaunchUIComponentAfterDone { get; set; }
        public DanoUIComponent LaunchAfterDone { get; set; }

    }
}