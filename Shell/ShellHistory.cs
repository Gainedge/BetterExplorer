// GongSolutions.Shell - A Windows Shell library for .Net.
// Copyright (C) 2007-2009 Steven J. Kirk
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either 
// version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free 
// Software Foundation, Inc., 51 Franklin Street, Fifth Floor,  
// Boston, MA 2110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.Text;
using GongSolutions.Shell.Interop;

namespace GongSolutions.Shell
{
    /// <summary>
    /// Holds a <see cref="ShellView"/>'s navigation history.
    /// </summary>
    public class ShellHistory
    {

        /// <summary>
        /// Clears the shell history.
        /// </summary>
        public void Clear()
        {
            ShellItem current = null;

            if (m_History.Count > 0)
            {
                current = Current;
            }

            m_History.Clear();

            if (current != null)
            {
                Add(current);
            }
        }

        /// <summary>
        /// Gets the list of folders in the <see cref="ShellView"/>'s
        /// <b>Back</b> history.
        /// </summary>
        public ShellItem[] HistoryBack
        {
            get
            {
                return m_History.GetRange(0, m_Current).ToArray();
            }
        }

        /// <summary>
        /// Gets the list of folders in the <see cref="ShellView"/>'s
        /// <b>Forward</b> history.
        /// </summary>
        public ShellItem[] HistoryForward
        {
            get
            {
                if (CanNavigateForward)
                {
                    return m_History.GetRange(m_Current + 1,
                        m_History.Count - (m_Current + 1)).ToArray();
                }
                else
                {
                    return new ShellItem[0];
                }
            }
        }

        internal ShellHistory()
        {
            m_History = new List<ShellItem>();
        }

        internal void Add(ShellItem folder)
        {
            while (m_Current < m_History.Count - 1)
            {
                m_History.RemoveAt(m_Current + 1);
            }

            m_History.Add(folder);
            m_Current = m_History.Count - 1;
        }

        internal ShellItem MoveBack()
        {
            if (m_Current == 0)
            {
                throw new InvalidOperationException("Cannot navigate back");
            }
            return m_History[--m_Current];
        }

        internal void MoveBack(ShellItem folder)
        {
            int index = m_History.IndexOf(folder);

            if ((index == -1) || (index >= m_Current))
            {
                throw new Exception(
                    "The requested folder could not be located in the " +
                    "'back' shell history");
            }

            m_Current = index;
        }

        internal ShellItem MoveForward()
        {
            if (m_Current == m_History.Count - 1)
            {
                throw new InvalidOperationException("Cannot navigate forward");
            }
            return m_History[++m_Current];
        }

        internal void MoveForward(ShellItem folder)
        {
            int index = m_History.IndexOf(folder, m_Current + 1);

            if (index == -1)
            {
                throw new Exception(
                    "The requested folder could not be located in the " +
                    "'forward' shell history");
            }

            m_Current = index;
        }

        internal bool CanNavigateBack
        {
            get { return m_Current > 0; }
        }

        internal bool CanNavigateForward
        {
            get { return m_Current < m_History.Count - 1; }
        }

        internal ShellItem Current
        {
            get { return m_History[m_Current]; }
        }

        List<ShellItem> m_History;
        int m_Current;
    }
}
