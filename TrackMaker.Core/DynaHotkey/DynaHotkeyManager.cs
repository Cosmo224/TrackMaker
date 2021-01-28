﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TrackMaker.Core
{
    public class DynaHotkeyManager
    {
        public List<DynaHotkey> Hotkeys { get; set; }

        public DynaHotkeyManager()
        {
            Hotkeys = new List<DynaHotkey>();
        }


        public void AddNewHotkey(string KeyName, List<Key> Keys = null)
        {
            DynaHotkey DK = new DynaHotkey(KeyName, Keys);
            Hotkeys.Add(DK);

        }

        /// <summary>
        /// For single-key DynaHotkeys, check for duplication.  
        /// </summary>
        /// <param name="Key"></param>
        public bool CheckIfKeyIsDuplicated(Key Key)
        {
            foreach (DynaHotkey Hotkey in Hotkeys)
            {
                if (Hotkey.Keys.Count == 1)
                {
                    if (Hotkey.Keys.Contains(Key)) return true; 
                } 
            }

            return false;
        }

        public void ClearAllHotkeys() => Hotkeys.Clear();
        public void DeleteHotkey(DynaHotkey DH) => Hotkeys.Remove(DH);

        public bool DeleteHotkeyWithName(string KeyName)
        {
            foreach (DynaHotkey Hotkey in Hotkeys)
            {
                if (Hotkey.Name == KeyName)
                {
                    Hotkeys.Remove(Hotkey);
                    return true; 
                }

            }

            return false;
        }

        public DynaHotkey GetHotkeyWithName(string KeyName)
        {
            foreach (DynaHotkey Hotkey in Hotkeys)
            {
                if (Hotkey.Name == KeyName) return Hotkey;
            }

            return null;
        }

        
    }
}