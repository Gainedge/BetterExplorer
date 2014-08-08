// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Windows7.DesktopIntegration.Interop;
using VistaBridgeInterop = Microsoft.SDK.Samples.VistaBridge.Interop;

namespace Windows7.DesktopIntegration
{
    /// <summary>
    /// Provides services to manage taskbar jump lists, including
    /// custom destinations and custom tasks.
    /// </summary>
    /// <remarks>
    /// This class mostly borrows the Windows Shell's concepts where
    /// jump lists are concerned including:
    /// <b>Application destinations</b> - Destinations added to the application's
    /// recent and frequent categories by the shell or by the application.
    /// <b>Custom destinations</b> - Destinations added to the application's
    /// jump list in other categories by the application.
    /// <b>Tasks</b> - Tasks added to the application's jump list.
    /// <b>The methods of this class are not thread-safe.</b>
    /// </remarks>
    public sealed class JumpListManager : IDisposable
    {
        #region Members

        string _appId;
        JumpListDestinations _destinations;
        JumpListTasks _tasks;

        ICustomDestinationList _customDestinationList;
        uint _maxSlotsInList;

        ApplicationDestinationType _enabledAutoDestinationType = ApplicationDestinationType.Recent;

        EventHandler _displaySettingsChangeHandler;

        #endregion

        /// <summary>
        /// Initializes a new instance of the jump list manager
        /// with the specified application id.
        /// </summary>
        /// <param name="appId">The application id.</param>
        public JumpListManager(string appId)
        {
            _appId = appId;
            _destinations = new JumpListDestinations();
            _tasks = new JumpListTasks();

            _customDestinationList = (ICustomDestinationList)new CDestinationList();
            if (String.IsNullOrEmpty(_appId))
            {
                _appId = Windows7Taskbar.GetCurrentProcessAppId();
            }
            if (!String.IsNullOrEmpty(_appId))
            {
                _customDestinationList.SetAppID(_appId);
            }

            _displaySettingsChangeHandler = delegate
            {
                RefreshMaxSlots();
            };
            SystemEvents.DisplaySettingsChanged +=
                _displaySettingsChangeHandler;
        }
        /// <summary>
        /// Initializes a new instance of the jump list manager
        /// with the specified window handle.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        public JumpListManager(IntPtr hwnd)
            : this(Windows7Taskbar.GetWindowAppId(hwnd))
        {
        }

        /// <summary>
        /// Adds a task to the application's jump list.
        /// </summary>
        /// <param name="task">An object implementing <see cref="IJumpListTask"/>,
        /// such as <see cref="ShellLink"/>.</param>
        public void AddUserTask(IJumpListTask task)
        {
            _tasks.AddTask(task);
        }

        /// <summary>
        /// Retrieves the tasks currently present in the application's
        /// jump list.  If the tasks are modified through the use of this
        /// property, the <see cref="Refresh"/> method must be called to
        /// repopulate the application's jump list.
        /// </summary>
        public IEnumerable<IJumpListTask> Tasks
        {
            get { return _tasks.Tasks; }
        }

        /// <summary>
        /// Deletes the specified task from the application's jump list.
        /// </summary>
        /// <param name="task">The task to delete.</param>
        public void DeleteTask(IJumpListTask task)
        {
            _tasks.DeleteTask(task);
        }

        /// <summary>
        /// Deletes all the tasks from the application's jump list.
        /// </summary>
        public void ClearTasks()
        {
            _tasks.Clear();
        }

        /// <summary>
        /// Adds a custom destination to the application's jump list.
        /// </summary>
        /// <param name="destination">An object implementing
        /// <see cref="IJumpListDestination"/> such as <see cref="ShellLink"/>
        /// or <see cref="ShellItem"/>.</param>
        public void AddCustomDestination(IJumpListDestination destination)
        {
            _destinations.AddDestination(destination);
        }

        /// <summary>
        /// Retrieves all custom destination categories in this application's
        /// jump list.
        /// </summary>
        public IEnumerable<string> CustomDestinationCategories
        {
            get { return _destinations.Categories; }
        }
        /// <summary>
        /// Retrieves the custom destinations belonging to the specified
        /// category from this application's jump list.  If destinations are
        /// modified through the use of this property, the
        /// <see cref="Refresh"/> method must be called to repopulate the
        /// application's jump list.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>A collection of destinations of type <see cref="IJumpListDestination"/>.</returns>
        public IEnumerable<IJumpListDestination> GetCustomDestinationsByCategory(
            string category)
        {
            return _destinations.GetDestinationsByCategory(category);
        }

        /// <summary>
        /// Deletes the specified custom destination from the application's
        /// jump list.
        /// </summary>
        /// <param name="destination">The destination to delete.</param>
        public void DeleteCustomDestination(IJumpListDestination destination)
        {
            _destinations.DeleteDestination(destination);
        }

        /// <summary>
        /// Adds the specified destination to the application's
        /// recent destinations list.
        /// </summary>
        /// <remarks>
        /// If the recent and frequent categories are disabled,
        /// this method has no visual effect on the jump list
        /// in the recent and frequent categories respectively.
        /// </remarks>
        /// <param name="destination">The path to the destination.</param>
        public void AddToRecent(string destination)
        {
            UnsafeNativeMethods.SHAddToRecentDocs(destination);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Adds the specified destination to the application's
        /// recent destinations list.
        /// </summary>
        /// <remarks>
        /// If the recent and frequent categories are disabled,
        /// this method has no visual effect on the jump list
        /// in the recent and frequent categories respectively.
        /// </remarks>
        /// <param name="destination">An object implementing
        /// <see cref="IJumpListDestination"/> such as <see cref="ShellLink"/>
        /// or <see cref="ShellItem"/>.</param>
        public void AddToRecent(IJumpListDestination destination)
        {
            if (destination is ShellItem)
            {
                SHARDAPPIDINFO info = new SHARDAPPIDINFO
                {
                    psi = (VistaBridgeInterop.IShellItem)destination.GetShellRepresentation(),
                    pszAppID = _appId
                };
                UnsafeNativeMethods.SHAddToRecentDocs(ref info);
            }
            else if (destination is ShellLink)
            {
                //TODO: This doesn't actually work
                SHARDAPPIDINFOLINK info = new SHARDAPPIDINFOLINK
                {
                    psl = (IShellLinkW)destination.GetShellRepresentation(),
                    pszAppID = _appId
                };
                UnsafeNativeMethods.SHAddToRecentDocs(ref info);
            }
            Thread.Sleep(200);
        }

        /// <summary>
        /// The currently enabled automatic application destination type.
        /// The supported values are the values of the
        /// <see cref="ApplicationDestinationType"/> enumeration.
        /// Only of the values can be set at any given time.
        /// </summary>
        public ApplicationDestinationType EnabledAutoDestinationType
        {
            get
            {
                return _enabledAutoDestinationType;
            }
            set
            {
                if (_enabledAutoDestinationType == value)
                    return;

                _enabledAutoDestinationType = value;
            }
        }

        /// <summary>
        /// Removes all destinations from the application's jump list.
        /// </summary>
        public void ClearAllDestinations()
        {
            ClearApplicationDestinations();
            ClearCustomDestinations();
        }

        /// <summary>
        /// Removes all application destinations (such as frequent and recent)
        /// from the application's jump list.
        /// </summary>
        public void ClearApplicationDestinations()
        {
            IApplicationDestinations destinations = (IApplicationDestinations)
                new CApplicationDestinations();
            if (!String.IsNullOrEmpty(_appId))
            {
                destinations.SetAppID(_appId);
            }
            try
            {
                //This does not remove pinned items.
                destinations.RemoveAllDestinations();
            }
            catch (System.IO.FileNotFoundException)
            {//There are no destinations.  That's cool.
            }
        }

        /// <summary>
        /// Retrieves all application destinations belonging to the specified
        /// application destination type.
        /// </summary>
        /// <param name="type">The application destination type.</param>
        /// <returns>A copy of the application destinations belonging to
        /// the specified type; modifying the returned objects has no effect
        /// on the application's destination list.</returns>
        public IEnumerable<IJumpListDestination> GetApplicationDestinations(
            ApplicationDestinationType type)
        {
            if (type == ApplicationDestinationType.NONE)
                throw new ArgumentException("type can't be NONE");

            IApplicationDocumentLists destinations = (IApplicationDocumentLists)
                new CApplicationDocumentLists();
            Guid iidObjectArray = typeof(IObjectArray).GUID;
            object obj;
            destinations.GetList((APPDOCLISTTYPE)type, 100,
                ref iidObjectArray, out obj);

            List<IJumpListDestination> returnValue = new List<IJumpListDestination>();

            Guid iidShellItem = typeof(VistaBridgeInterop.IShellItem).GUID;
            Guid iidShellLink = typeof(IShellLinkW).GUID;
            IObjectArray array = (IObjectArray)obj;
            uint count;
            array.GetCount(out count);
            for (uint i = 0; i < count; ++i)
            {
                try
                {
                    array.GetAt(i, ref iidShellItem, out obj);
                }
                catch (Exception)//Wrong type
                {
                }
                if (obj == null)
                {
                    array.GetAt(i, ref iidShellLink, out obj);
                    //This shouldn't fail since if it's not IShellItem
                    //then it must be IShellLink.

                    IShellLinkW link = (IShellLinkW)obj;
                    ShellLink wrapper = new ShellLink();

                    StringBuilder sb = new StringBuilder(256);
                    link.GetPath(sb, sb.Capacity, IntPtr.Zero, 2);
                    wrapper.Path = sb.ToString();

                    link.GetArguments(sb, sb.Capacity);
                    wrapper.Arguments = sb.ToString();

                    int iconId;
                    link.GetIconLocation(sb, sb.Capacity, out iconId);
                    wrapper.IconIndex = iconId;
                    wrapper.IconLocation = sb.ToString();

                    uint showCmd;
                    link.GetShowCmd(out showCmd);
                    wrapper.ShowCommand = (WindowShowCommand)showCmd;

                    link.GetWorkingDirectory(sb, sb.Capacity);
                    wrapper.WorkingDirectory = sb.ToString();

                    returnValue.Add(wrapper);
                }
                else //It's an IShellItem.
                {
                    VistaBridgeInterop.IShellItem item = (VistaBridgeInterop.IShellItem)obj;
                    ShellItem wrapper = new ShellItem();

                    string path;
                    item.GetDisplayName(
                        VistaBridgeInterop.SafeNativeMethods.SIGDN.SIGDN_FILESYSPATH,
                        out path);
                    wrapper.Path = path;

                    //Title and Category are irrelevant here, because it's
                    //an IShellItem.  The user might want to see them, but he's
                    //free to go to the IShellItem and look at its property store.

                    returnValue.Add(wrapper);
                }
            }
            
            return returnValue;
        }
        /// <summary>
        /// Deletes the specified application destination from the application's
        /// jump list.
        /// </summary>
        /// <param name="destination">The application destination.</param>
        public void DeleteApplicationDestination(IJumpListDestination destination)
        {
            IApplicationDestinations destinations = (IApplicationDestinations)
                new CApplicationDestinations();
            if (!String.IsNullOrEmpty(_appId))
            {
                destinations.SetAppID(_appId);
            }

            destinations.RemoveDestination(destination.GetShellRepresentation());
        }

        /// <summary>
        /// Deletes all custom destinations from the application's jump list.
        /// </summary>
        public void ClearCustomDestinations()
        {
            try
            {
                _customDestinationList.DeleteList(_appId);
            }
            catch (FileNotFoundException)
            {//Means the list is empty, that's cool.
            }
            
            _destinations.Clear();
        }

        /// <summary>
        /// Repopulates the application's jump list.
        /// Use this method after all current changes to 
        /// the application's jump list have been introduced,
        /// and you want the list to be refreshed.
        /// </summary>
        /// <returns><b>true</b> if the list was refreshed; <b>false</b>
        /// if the operation was cancelled.  The operation might have
        /// been cancelled if the <see cref="UserRemovedItems"/> event
        /// handler instructed us to cancel the operation.</returns>
        /// <remarks>
        /// If the user removed items from the jump list between the
        /// last refresh operation and this one, then the
        /// <see cref="UserRemovedItems"/> event will be invoked.
        /// If the event handler for this event instructed us to cancel
        /// the operation, then the current transaction is aborted,
        /// no items are added, and this method returns <b>false</b>.
        /// Check the return value to determine whether the jump list
        /// needs to be changed and the operation attempted again.
        /// </remarks>
        public bool Refresh()
        {
            if (!BeginList())
                return false;   //Operation was cancelled

            _tasks.RefreshTasks(_customDestinationList);
            _destinations.RefreshDestinations(_customDestinationList);

            switch (EnabledAutoDestinationType)
            {
                case ApplicationDestinationType.Frequent:
                    _customDestinationList.AppendKnownCategory(KNOWNDESTCATEGORY.KDC_FREQUENT);
                    break;
                case ApplicationDestinationType.Recent:
                    _customDestinationList.AppendKnownCategory(KNOWNDESTCATEGORY.KDC_RECENT);
                    break;
            }

            CommitList();
            return true;
        }

        /// <summary>
        /// Returns the maximum number of items to be placed
        /// in the application's jump list.  This number depends
        /// on factors such as the display resolution or monitor
        /// change - do not assume it is always constant.
        /// </summary>
        public uint MaximumSlotsInList
        {
            get
            {
                if (_maxSlotsInList == 0)
                {
                    RefreshMaxSlots();
                }
                return _maxSlotsInList;
            }
        }

        /// <summary>
        /// Cleans the resources associated with this jump list.
        /// </summary>
        public void Dispose()
        {
            SystemEvents.DisplaySettingsChanged -=
                _displaySettingsChangeHandler;
            if (_customDestinationList != null)
                Marshal.ReleaseComObject(_customDestinationList);
        }

        /// <summary>
        /// Register to this event to receive notifications when custom
        /// destinations are being removed from your jump list by the user.
        /// If you do not register to this event, you will not be able
        /// to refresh the list.  Additionally, if you attempt to add
        /// items to the list which have been previously removed by the user,
        /// the next refresh will fail to add your category.
        /// </summary>
        public event EventHandler<UserRemovedItemsEventArgs> UserRemovedItems;

        #region Implementation

        private void RefreshMaxSlots()
        {
            object obj;
            _customDestinationList.BeginList(
                out _maxSlotsInList,
                ref SafeNativeMethods.IID_IObjectArray,
                out obj);
            _customDestinationList.AbortList();
        }

        private bool BeginList()
        {
            if (UserRemovedItems == null)
            {
                throw new InvalidOperationException(
                    "You must register for the JumpListManager.UserRemovedItems event before adding any items");
            }

            object obj;
            _customDestinationList.BeginList(
                out _maxSlotsInList,
                ref SafeNativeMethods.IID_IObjectArray,
                out obj);

            IObjectArray removedItems = (IObjectArray)obj;
            uint count;
            removedItems.GetCount(out count);
            if (count == 0)
                return true;

            string[] removedItemsArr = new string[count];
            for (uint i = 0; i < count; ++i)
            {
                object item;
                removedItems.GetAt(i, ref SafeNativeMethods.IID_IUnknown, out item);

                try
                {
                    IShellLinkW shellLink = (IShellLinkW)item;
                    if (shellLink != null)
                    {
                        StringBuilder sb = new StringBuilder(256);
                        shellLink.GetPath(sb, sb.Capacity, IntPtr.Zero, 2);
                        removedItemsArr[i] = sb.ToString();
                    }
                    continue;
                }
                catch (InvalidCastException)//It's not a shell link
                {
                }
                try
                {
                    VistaBridgeInterop.IShellItem shellItem = (VistaBridgeInterop.IShellItem)item;
                    if (shellItem != null)
                    {
                        string path;
                        shellItem.GetDisplayName(
                            VistaBridgeInterop.SafeNativeMethods.SIGDN.SIGDN_FILESYSPATH,
                            out path);
                        removedItemsArr[i] = path;
                    }
                }
                catch (InvalidCastException)
                {
                    //It's neither a shell link nor a shell item.
                    //This is impossible.
                    Debug.Assert(false,
                        "List of removed items contains something that is neither a shell item nor a shell link");
                }
            }

            UserRemovedItemsEventArgs args = new UserRemovedItemsEventArgs(removedItemsArr);
            UserRemovedItems(this, args);
            if (args.CancelCurrentOperation)
            {
                _customDestinationList.AbortList();
            }
            return !args.CancelCurrentOperation;
        }

        private void CommitList()
        {
            _customDestinationList.CommitList();
        }

        #endregion
    }
    
    /// <summary>
    /// The application destination type.
    /// </summary>
    public enum ApplicationDestinationType
    {
        /// <summary>
        /// No application destination type is selected.
        /// </summary>
        NONE = -1,
        /// <summary>
        /// Destinations used recently.
        /// </summary>
        Recent = 0,
        /// <summary>
        /// Destinations used frequently.
        /// </summary>
        Frequent
    }

    /// <summary>
    /// The event arguments for the event that occurs
    /// when the user removes items from the application's
    /// jump list.
    /// </summary>
    public class UserRemovedItemsEventArgs : EventArgs
    {
        readonly string[] _removedItems;

        internal UserRemovedItemsEventArgs(
            string[] removedItems)
        {
            _removedItems = removedItems;
        }

        /// <summary>
        /// The collection of removed items.  Each item
        /// is the path.
        /// </summary>
        public string[] RemovedItems
        {
            get
            {
                return _removedItems;
            }
        }

        /// <summary>
        /// Set to <b>true</b> if the current operation
        /// should be cancelled.  Should be used by the application
        /// if because of the items the user has removed
        /// there is no real work to do with the jump list.
        /// </summary>
        public bool CancelCurrentOperation { get; set; }
    }
}