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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GongSolutions.Shell.Interop;

namespace GongSolutions.Shell
{
    public class KnownFolderManager : IEnumerable<KnownFolder>
    {
        public KnownFolderManager()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                m_ComInterface = (IKnownFolderManager)
                    new CoClass.KnownFolderManager();
            }
            else
            {
                m_NameIndex = new Dictionary<string, KnownFolder>();
                m_PathIndex = new Dictionary<string, KnownFolder>();

                AddFolder("Common Desktop", CSIDL.COMMON_DESKTOPDIRECTORY);
                AddFolder("Desktop", CSIDL.DESKTOP);
                AddFolder("Personal", CSIDL.PERSONAL);
                AddFolder("Recent", CSIDL.RECENT);
                AddFolder("MyComputerFolder", CSIDL.DRIVES);
                AddFolder("My Pictures", CSIDL.MYPICTURES);
                AddFolder("ProgramFilesCommon", CSIDL.PROGRAM_FILES_COMMON);
                AddFolder("Windows", CSIDL.WINDOWS);
            }
        }

        public KnownFolder FindNearestParent(ShellItem item)
        {
            if (m_ComInterface != null)
            {
                IKnownFolder iKnownFolder;

                if (item.IsFileSystem)
                {
                    if (m_ComInterface.FindFolderFromPath(item.FileSystemPath,
                            FFFP_MODE.NEARESTPARENTMATCH, out iKnownFolder)
                            == HResult.S_OK)
                    {
                        return CreateFolder(iKnownFolder);
                    }
                }
                else
                {
                    if (m_ComInterface.FindFolderFromIDList(item.Pidl, out iKnownFolder)
                            == HResult.S_OK)
                    {
                        return CreateFolder(iKnownFolder);
                    }
                }
            }
            else
            {
                if (item.IsFileSystem)
                {
                    foreach (KeyValuePair<string, KnownFolder> i in m_PathIndex)
                    {
                        if ((i.Key != string.Empty) &&
                            item.FileSystemPath.StartsWith(i.Key))
                        {
                            return i.Value;
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, KnownFolder> i in m_NameIndex)
                    {
                        if (item == i.Value.CreateShellItem())
                        {
                            return i.Value;
                        }
                    }
                }
            }

            return null;
        }

        public IEnumerator<KnownFolder> GetEnumerator()
        {
            IntPtr buffer;
            uint count;
            KnownFolder[] results;

            if (m_ComInterface != null)
            {
                m_ComInterface.GetFolderIds(out buffer, out count);

                try
                {
                    results = new KnownFolder[count];
                    IntPtr p = buffer;

                    for (uint n = 0; n < count; ++n)
                    {
                        Guid guid = (Guid)Marshal.PtrToStructure(p, typeof(Guid));
                        results[n] = GetFolder(guid);
                        p = (IntPtr)((int)p + Marshal.SizeOf(typeof(Guid)));
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(buffer);
                }

                foreach (KnownFolder f in results)
                {
                    yield return f;
                }
            }
            else
            {
                foreach (KnownFolder f in m_NameIndex.Values)
                {
                    yield return f;
                }
            }
        }

        public KnownFolder GetFolder(Guid guid)
        {
            return CreateFolder(m_ComInterface.GetFolder(guid));
        }

        public KnownFolder GetFolder(string name)
        {
            if (m_ComInterface != null)
            {
                IKnownFolder iKnownFolder;

                if (m_ComInterface.GetFolderByName(name, out iKnownFolder)
                        == HResult.S_OK)
                {
                    return CreateFolder(iKnownFolder);
                }
            }
            else
            {
                return m_NameIndex[name];
            }

            throw new InvalidOperationException("Unknown shell folder: " + name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KnownFolder>)this).GetEnumerator();
        }

        void AddFolder(string name, CSIDL csidl)
        {
            KnownFolder folder = CreateFolder(csidl, name);

            m_NameIndex.Add(folder.Name, folder);

            if (folder.ParsingName != string.Empty)
            {
                m_PathIndex.Add(folder.ParsingName, folder);
            }
        }

        static KnownFolder CreateFolder(CSIDL csidl, string name)
        {
            StringBuilder path = new StringBuilder(512);

            if (Shell32.SHGetFolderPath(IntPtr.Zero, csidl, IntPtr.Zero, 0, path) == HResult.S_OK)
            {
                return new KnownFolder(csidl, name, path.ToString());
            }
            else
            {
                return new KnownFolder(csidl, name, string.Empty);
            }
        }

        static KnownFolder CreateFolder(IKnownFolder iface)
        {
            KNOWNFOLDER_DEFINITION def = iface.GetFolderDefinition();

            try
            {
                return new KnownFolder(iface,
                    Marshal.PtrToStringUni(def.pszName),
                    Marshal.PtrToStringUni(def.pszParsingName));
            }
            finally
            {
                Marshal.FreeCoTaskMem(def.pszName);
                Marshal.FreeCoTaskMem(def.pszDescription);
                Marshal.FreeCoTaskMem(def.pszRelativePath);
                Marshal.FreeCoTaskMem(def.pszParsingName);
                Marshal.FreeCoTaskMem(def.pszTooltip);
                Marshal.FreeCoTaskMem(def.pszLocalizedName);
                Marshal.FreeCoTaskMem(def.pszIcon);
                Marshal.FreeCoTaskMem(def.pszSecurity);
            }
        }

        struct PathIndexEntry
        {
            public PathIndexEntry(string name, CSIDL csidl)
            {
                Name = name;
                Csidl = csidl;
            }

            public string Name;
            public CSIDL Csidl;
        }

        IKnownFolderManager m_ComInterface;
        Dictionary<string, KnownFolder> m_NameIndex;
        Dictionary<string, KnownFolder> m_PathIndex;
    }

    public class KnownFolder
    {
        public KnownFolder(IKnownFolder iface, string name, string parsingName)
        {
            m_ComInterface = iface;
            m_Name = name;
            m_ParsingName = parsingName;
        }

        public KnownFolder(CSIDL csidl, string name, string parsingName)
        {
            m_Csidl = csidl;
            m_Name = name;
            m_ParsingName = parsingName;
        }

        public ShellItem CreateShellItem()
        {
            if (m_ComInterface != null)
            {
                return new ShellItem(m_ComInterface.GetShellItem(0,
                    typeof(IShellItem).GUID));
            }
            else
            {
                return new ShellItem((Environment.SpecialFolder)m_Csidl);
            }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public string ParsingName
        {
            get { return m_ParsingName; }
        }

        IKnownFolder m_ComInterface;
        CSIDL m_Csidl;
        string m_Name;
        string m_ParsingName;
    }
}
