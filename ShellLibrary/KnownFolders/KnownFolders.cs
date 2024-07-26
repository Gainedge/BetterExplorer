//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace BExplorer.Shell {

  /// <summary>
  /// Defines properties for known folders that identify the path of standard known folders.
  /// </summary>
  public static class KnownFolders {

    /*
/// <summary>
/// Gets a strongly-typed read-only collection of all the registered known folders.
/// </summary>
public static ICollection<IKnownFolder> All {
  get {
    return GetAllFolders();
  }
}
*/

    /*
private static ReadOnlyCollection<IKnownFolder> GetAllFolders() {
  // Should this method be thread-safe?? (It'll take a while to get a list of all the
  // known folders, create the managed wrapper and return the read-only collection.

  IList<IKnownFolder> foldersList = new List<IKnownFolder>();
  uint count;
  IntPtr folders = IntPtr.Zero;

  try {
    KnownFolderManagerClass knownFolderManager = new KnownFolderManagerClass();
    knownFolderManager.GetFolderIds(out folders, out count);

    if (count > 0 && folders != IntPtr.Zero) {
      // Loop through all the KnownFolderID elements
      for (int i = 0; i < count; i++) {
        // Read the current pointer
        IntPtr current = new IntPtr(folders.ToInt64() + (Marshal.SizeOf(typeof(Guid)) * i));

        // Convert to Guid
        Guid knownFolderID = (Guid)Marshal.PtrToStructure(current, typeof(Guid));

        IKnownFolder kf = KnownFolderHelper.FromKnownFolderIdInternal(knownFolderID);

        // Add to our collection if it's not null (some folders might not exist on
        // the system or we could have an exception that resulted in the null return
        // from above method call
        if (kf != null) { foldersList.Add(kf); }
      }
    }
  }
  finally {
    if (folders != IntPtr.Zero) { Marshal.FreeCoTaskMem(folders); }
  }

  return new ReadOnlyCollection<IKnownFolder>(foldersList);
}
*/

    /*
/// <summary>
/// Gets a strongly-typed read-only collection of all the registered known folders.
/// </summary>
public static ReadOnlyCollection<IKnownFolder> All {
  get {
    // Should this method be thread-safe?? (It'll take a while to get a list of all the
    // known folders, create the managed wrapper and return the read-only collection.

    IList<IKnownFolder> foldersList = new List<IKnownFolder>();
    uint count;
    IntPtr folders = IntPtr.Zero;

    try {
      KnownFolderManagerClass knownFolderManager = new KnownFolderManagerClass();
      knownFolderManager.GetFolderIds(out folders, out count);

      if (count > 0 && folders != IntPtr.Zero) {
        // Loop through all the KnownFolderID elements
        for (int i = 0; i < count; i++) {
          // Read the current pointer
          IntPtr current = new IntPtr(folders.ToInt64() + (Marshal.SizeOf(typeof(Guid)) * i));

          // Convert to Guid
          Guid knownFolderID = (Guid)Marshal.PtrToStructure(current, typeof(Guid));

          IKnownFolder kf = KnownFolderHelper.FromKnownFolderIdInternal(knownFolderID);

          // Add to our collection if it's not null (some folders might not exist on
          // the system or we could have an exception that resulted in the null return
          // from above method call
          if (kf != null) { foldersList.Add(kf); }
        }
      }
    }
    finally {
      if (folders != IntPtr.Zero) { Marshal.FreeCoTaskMem(folders); }
    }

    return new ReadOnlyCollection<IKnownFolder>(foldersList);
  }
}
*/



    private static IKnownFolder GetKnownFolder(Guid guid) => KnownFolderHelper.FromKnownFolderId(guid);

    #region Default Known Folders

    /// <summary> Gets the metadata for the <b>Computer</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Computer => GetKnownFolder(Folder_GUIs.Computer);

    /// <summary> Gets the metadata for the <b>Conflict</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Conflict => GetKnownFolder(Folder_GUIs.Conflict);


    /// <summary> Gets the metadata for the <b>ControlPanel</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ControlPanel => GetKnownFolder(Folder_GUIs.ControlPanel);

    /// <summary> Gets the metadata for the <b>Desktop</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Desktop => GetKnownFolder(Folder_GUIs.Desktop);

    /// <summary> Gets the metadata for the <b>Internet</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Internet => GetKnownFolder(Folder_GUIs.Internet);

    /// <summary> Gets the metadata for the <b>Network</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Network => GetKnownFolder(Folder_GUIs.Network);

    /// <summary> Gets the metadata for the <b>Printers</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Printers => GetKnownFolder(Folder_GUIs.Printers);

    /// <summary> Gets the metadata for the <b>SyncManager</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SyncManager => GetKnownFolder(Folder_GUIs.SyncManager);

    /// <summary> Gets the metadata for the <b>Connections</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Connections => GetKnownFolder(Folder_GUIs.Connections);

    /// <summary> Gets the metadata for the <b>SyncSetup</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SyncSetup => GetKnownFolder(Folder_GUIs.SyncSetup);

    /// <summary> Gets the metadata for the <b>SyncResults</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SyncResults => GetKnownFolder(Folder_GUIs.SyncResults);

    /// <summary> Gets the metadata for the <b>RecycleBin</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder RecycleBin => GetKnownFolder(Folder_GUIs.RecycleBin);

    /// <summary> Gets the metadata for the <b>Fonts</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Fonts => GetKnownFolder(Folder_GUIs.Fonts);

    /// <summary> Gets the metadata for the <b>Startup</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Startup => GetKnownFolder(Folder_GUIs.Startup);

    /// <summary> Gets the metadata for the <b>Programs</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Programs => GetKnownFolder(Folder_GUIs.Programs);

    /// <summary> Gets the metadata for the per-user <b>StartMenu</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder StartMenu => GetKnownFolder(Folder_GUIs.StartMenu);

    /// <summary> Gets the metadata for the per-user <b>Recent</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Recent => GetKnownFolder(Folder_GUIs.Recent);

    public static IKnownFolder RecentPlaces => GetKnownFolder(Folder_GUIs.RecentPlaces);

    /// <summary> Gets the metadata for the per-user <b>SendTo</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SendTo => GetKnownFolder(Folder_GUIs.SendTo);

    /// <summary> Gets the metadata for the per-user <b>Documents</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Documents => GetKnownFolder(Folder_GUIs.Documents);

    /// <summary> Gets the metadata for the per-user <b>Favorites</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Favorites => GetKnownFolder(Folder_GUIs.Favorites);

    /// <summary> Gets the metadata for the <b>NetHood</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder NetHood => GetKnownFolder(Folder_GUIs.NetHood);

    /// <summary> Gets the metadata for the <b>PrintHood</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PrintHood => GetKnownFolder(Folder_GUIs.PrintHood);

    /// <summary> Gets the metadata for the <b>Templates</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Templates => GetKnownFolder(Folder_GUIs.Templates);

    /// <summary> Gets the metadata for the <b>CommonStartup</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder CommonStartup => GetKnownFolder(Folder_GUIs.CommonStartup);

    /// <summary> Gets the metadata for the <b>CommonPrograms</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder CommonPrograms => GetKnownFolder(Folder_GUIs.CommonPrograms);

    /// <summary> Gets the metadata for the <b>CommonStartMenu</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder CommonStartMenu => GetKnownFolder(Folder_GUIs.CommonStartMenu);

    /// <summary> Gets the metadata for the <b>PublicDesktop</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PublicDesktop => GetKnownFolder(Folder_GUIs.PublicDesktop);

    /// <summary> Gets the metadata for the <b>ProgramData</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ProgramData => GetKnownFolder(Folder_GUIs.ProgramData);

    /// <summary> Gets the metadata for the <b>CommonTemplates</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder CommonTemplates => GetKnownFolder(Folder_GUIs.CommonTemplates);

    /// <summary> Gets the metadata for the <b>PublicDocuments</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PublicDocuments => GetKnownFolder(Folder_GUIs.PublicDocuments);

    /// <summary> Gets the metadata for the <b>RoamingAppData</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder RoamingAppData => GetKnownFolder(Folder_GUIs.RoamingAppData);

    /// <summary> Gets the metadata for the per-user <b>LocalAppData</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder LocalAppData => GetKnownFolder(Folder_GUIs.LocalAppData);

    /// <summary> Gets the metadata for the <b>LocalAppDataLow</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder LocalAppDataLow => GetKnownFolder(Folder_GUIs.LocalAppDataLow);

    /// <summary> Gets the metadata for the <b>InternetCache</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder InternetCache => GetKnownFolder(Folder_GUIs.InternetCache);

    /// <summary> Gets the metadata for the <b>Cookies</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Cookies => GetKnownFolder(Folder_GUIs.Cookies);

    /// <summary> Gets the metadata for the <b>History</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder History => GetKnownFolder(Folder_GUIs.History);

    /// <summary> Gets the metadata for the <b>System</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder System => GetKnownFolder(Folder_GUIs.System);

    /// <summary> Gets the metadata for the <b>SystemX86</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SystemX86 => GetKnownFolder(Folder_GUIs.SystemX86);

    /// <summary> Gets the metadata for the <b>Windows</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Windows => GetKnownFolder(Folder_GUIs.Windows);

    /// <summary> Gets the metadata for the <b>Profile</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Profile => GetKnownFolder(Folder_GUIs.Profile);

    /// <summary> Gets the metadata for the per-user <b>Pictures</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Pictures => GetKnownFolder(Folder_GUIs.Pictures);

    /// <summary> Gets the metadata for the <b>ProgramFilesX86</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ProgramFilesX86 => GetKnownFolder(Folder_GUIs.ProgramFilesX86);

    /// <summary> Gets the metadata for the <b>ProgramFilesCommonX86</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ProgramFilesCommonX86 => GetKnownFolder(Folder_GUIs.ProgramFilesCommonX86);

    /// <summary> Gets the metadata for the <b>ProgramsFilesX64</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ProgramFilesX64 => GetKnownFolder(Folder_GUIs.ProgramFilesX64);

    /// <summary> Gets the metadata for the <b> ProgramFilesCommonX64</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ProgramFilesCommonX64 => GetKnownFolder(Folder_GUIs.ProgramFilesCommonX64);

    /// <summary> Gets the metadata for the <b>ProgramFiles</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ProgramFiles => GetKnownFolder(Folder_GUIs.ProgramFiles);

    /// <summary> Gets the metadata for the <b>ProgramFilesCommon</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ProgramFilesCommon => GetKnownFolder(Folder_GUIs.ProgramFilesCommon);

    /// <summary> Gets the metadata for the <b>AdminTools</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder AdminTools => GetKnownFolder(Folder_GUIs.AdminTools);

    /// <summary> Gets the metadata for the <b>CommonAdminTools</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder CommonAdminTools => GetKnownFolder(Folder_GUIs.CommonAdminTools);

    /// <summary> Gets the metadata for the per-user <b>Music</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Music => GetKnownFolder(Folder_GUIs.Music);

    /// <summary> Gets the metadata for the <b>Videos</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Videos => GetKnownFolder(Folder_GUIs.Videos);

    /// <summary> Gets the metadata for the <b>PublicPictures</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PublicPictures => GetKnownFolder(Folder_GUIs.PublicPictures);

    /// <summary> Gets the metadata for the <b>PublicMusic</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PublicMusic => GetKnownFolder(Folder_GUIs.PublicMusic);

    /// <summary> Gets the metadata for the <b>PublicVideos</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PublicVideos => GetKnownFolder(Folder_GUIs.PublicVideos);

    /// <summary> Gets the metadata for the <b>ResourceDir</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ResourceDir => GetKnownFolder(Folder_GUIs.ResourceDir);

    /// <summary> Gets the metadata for the <b>LocalizedResourcesDir</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder LocalizedResourcesDir => GetKnownFolder(Folder_GUIs.LocalizedResourcesDir);

    /// <summary> Gets the metadata for the <b>CommonOEMLinks</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder CommonOemLinks => GetKnownFolder(Folder_GUIs.CommonOEMLinks);

    /// <summary> Gets the metadata for the <b>CDBurning</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder CDBurning => GetKnownFolder(Folder_GUIs.CDBurning);

    /// <summary> Gets the metadata for the <b>UserProfiles</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder UserProfiles => GetKnownFolder(Folder_GUIs.UserProfiles);

    /// <summary> Gets the metadata for the <b>Playlists</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Playlists => GetKnownFolder(Folder_GUIs.Playlists);

    /// <summary> Gets the metadata for the <b>SamplePlaylists</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SamplePlaylists => GetKnownFolder(Folder_GUIs.SamplePlaylists);

    /// <summary> Gets the metadata for the <b>SampleMusic</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SampleMusic => GetKnownFolder(Folder_GUIs.SampleMusic);

    /// <summary> Gets the metadata for the <b>SamplePictures</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SamplePictures => GetKnownFolder(Folder_GUIs.SamplePictures);

    /// <summary> Gets the metadata for the <b>SampleVideos</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SampleVideos => GetKnownFolder(Folder_GUIs.SampleVideos);

    /// <summary> Gets the metadata for the <b>PhotoAlbums</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PhotoAlbums => GetKnownFolder(Folder_GUIs.PhotoAlbums);

    /// <summary> Gets the metadata for the <b>Public</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Public => GetKnownFolder(Folder_GUIs.Public);

    /// <summary> Gets the metadata for the <b>ChangeRemovePrograms</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder ChangeRemovePrograms => GetKnownFolder(Folder_GUIs.ChangeRemovePrograms);

    /// <summary> Gets the metadata for the <b>AppUpdates</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder AppUpdates => GetKnownFolder(Folder_GUIs.AppUpdates);

    /// <summary> Gets the metadata for the <b>AddNewPrograms</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder AddNewPrograms => GetKnownFolder(Folder_GUIs.AddNewPrograms);

    /// <summary> Gets the metadata for the per-user <b>Downloads</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Downloads => GetKnownFolder(Folder_GUIs.Downloads);

    /// <summary> Gets the metadata for the <b>PublicDownloads</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PublicDownloads => GetKnownFolder(Folder_GUIs.PublicDownloads);

    /// <summary> Gets the metadata for the per-user <b>SavedSearches</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SavedSearches => GetKnownFolder(Folder_GUIs.SavedSearches);

    /// <summary> Gets the metadata for the per-user <b>QuickLaunch</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder QuickLaunch => GetKnownFolder(Folder_GUIs.QuickLaunch);

    /// <summary> Gets the metadata for the <b>Contacts</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Contacts => GetKnownFolder(Folder_GUIs.Contacts);

    /// <summary> Gets the metadata for the <b>SidebarParts</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SidebarParts => GetKnownFolder(Folder_GUIs.SidebarParts);

    /// <summary> Gets the metadata for the <b>SidebarDefaultParts</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SidebarDefaultParts => GetKnownFolder(Folder_GUIs.SidebarDefaultParts);

    /// <summary> Gets the metadata for the <b>TreeProperties</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder TreeProperties => GetKnownFolder(Folder_GUIs.TreeProperties);

    /// <summary> Gets the metadata for the <b>PublicGameTasks</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder PublicGameTasks => GetKnownFolder(Folder_GUIs.PublicGameTasks);

    /// <summary> Gets the metadata for the <b>GameTasks</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder GameTasks => GetKnownFolder(Folder_GUIs.GameTasks);

    /// <summary> Gets the metadata for the per-user <b>SavedGames</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SavedGames => GetKnownFolder(Folder_GUIs.SavedGames);

    /// <summary> Gets the metadata for the <b>Games</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Games => GetKnownFolder(Folder_GUIs.Games);

    /// <summary> Gets the metadata for the <b>RecordedTV</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    /// <remarks> This folder is not used. </remarks>
    public static IKnownFolder RecordedTV => GetKnownFolder(Folder_GUIs.RecordedTV);

    /// <summary> Gets the metadata for the <b>SearchMapi</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SearchMapi => GetKnownFolder(Folder_GUIs.SearchMapi);

    /// <summary> Gets the metadata for the <b>SearchCsc</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SearchCsc => GetKnownFolder(Folder_GUIs.SearchCsc);

    /// <summary> Gets the metadata for the per-user <b>Links</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder Links => GetKnownFolder(Folder_GUIs.Links);

    /// <summary> Gets the metadata for the <b>UsersFiles</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder UsersFiles => GetKnownFolder(Folder_GUIs.UsersFiles);

    public static IKnownFolder OneDrive => GetKnownFolder(Folder_GUIs.OneDrive);

    /// <summary> Gets the metadata for the <b>SearchHome</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder SearchHome => GetKnownFolder(Folder_GUIs.SearchHome);

    /// <summary> Gets the metadata for the <b>OriginalImages</b> folder. </summary>
    /// <value> An <see cref="IKnownFolder" /> object. </value>
    public static IKnownFolder OriginalImages => GetKnownFolder(Folder_GUIs.OriginalImages);

    /// <summary> Gets the metadata for the <b>UserProgramFiles</b> folder. </summary>
    public static IKnownFolder UserProgramFiles => GetKnownFolder(Folder_GUIs.UserProgramFiles);

    /// <summary> Gets the metadata for the <b>UserProgramFilesCommon</b> folder. </summary>
    public static IKnownFolder UserProgramFilesCommon => GetKnownFolder(Folder_GUIs.UserProgramFilesCommon);

    /// <summary> Gets the metadata for the <b>Ringtones</b> folder. </summary>
    public static IKnownFolder Ringtones => GetKnownFolder(Folder_GUIs.Ringtones);

    /// <summary> Gets the metadata for the <b>PublicRingtones</b> folder. </summary>
    public static IKnownFolder PublicRingtones => GetKnownFolder(Folder_GUIs.PublicRingtones);

    /// <summary> Gets the metadata for the <b>UsersLibraries</b> folder. </summary>
    public static IKnownFolder UsersLibraries => GetKnownFolder(Folder_GUIs.UsersLibraries);

    /// <summary> Gets the metadata for the <b>DocumentsLibrary</b> folder. </summary>
    public static IKnownFolder DocumentsLibrary => GetKnownFolder(Folder_GUIs.DocumentsLibrary);

    /// <summary> Gets the metadata for the <b>MusicLibrary</b> folder. </summary>
    public static IKnownFolder MusicLibrary => GetKnownFolder(Folder_GUIs.MusicLibrary);


    /// <summary> Gets the metadata for the <b>PicturesLibrary</b> folder. </summary>
    public static IKnownFolder PicturesLibrary => GetKnownFolder(Folder_GUIs.PicturesLibrary);

    /// <summary> Gets the metadata for the <b>VideosLibrary</b> folder. </summary>
    public static IKnownFolder VideosLibrary => GetKnownFolder(Folder_GUIs.VideosLibrary);

    /// <summary> Gets the metadata for the <b>RecordedTVLibrary</b> folder. </summary>
    public static IKnownFolder RecordedTVLibrary => GetKnownFolder(Folder_GUIs.RecordedTVLibrary);

    /// <summary> Gets the metadata for the <b>OtherUsers</b> folder. </summary>
    public static IKnownFolder OtherUsers => GetKnownFolder(Folder_GUIs.OtherUsers);

    /// <summary> Gets the metadata for the <b>DeviceMetadataStore</b> folder. </summary>
    public static IKnownFolder DeviceMetadataStore => GetKnownFolder(Folder_GUIs.DeviceMetadataStore);


    /// <summary> Gets the metadata for the <b>Libraries</b> folder. </summary>
    public static IKnownFolder Libraries => GetKnownFolder(Folder_GUIs.Libraries);

    /// <summary>
    ///Gets the metadata for the <b>UserPinned</b> folder.
    /// </summary>
    public static IKnownFolder UserPinned => GetKnownFolder(Folder_GUIs.UserPinned);


    /// <summary> Gets the metadata for the <b>ImplicitAppShortcuts</b> folder. </summary>
    public static IKnownFolder ImplicitAppShortcuts => GetKnownFolder(Folder_GUIs.ImplicitAppShortcuts);

    #endregion Default Known Folders
  }
}