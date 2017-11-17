using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace wyDay.Controls
{

#if NET_2_0
    /// <summary>Represents the AutomaticUpdater control.</summary>
    [Designer(typeof(AutomaticUpdaterDesigner))]
#else
    /// <summary>Represents the AutomaticUpdater control.</summary>
    [Designer("wyDay.Controls.AutomaticUpdaterDesigner, AutomaticUpdater.Design")]
#endif
    [ToolboxBitmapAttribute(typeof(AutomaticUpdater), "update-notify.png")]
    public class AutomaticUpdater : ContainerControl, ISupportInitialize
    {
        AutomaticUpdaterBackend auBackend = new AutomaticUpdaterBackend(true);

        Form ownerForm;

        readonly AnimationControl ani = new AnimationControl();

        Rectangle textRect;

        readonly Timer tmrCollapse = new Timer { Interval = 3000 };
        readonly Timer tmrAniExpandCollapse = new Timer { Interval = 30 };

        int m_WaitBeforeCheckSecs = 10;
        readonly Timer tmrWaitBeforeCheck = new Timer { Interval = 10000 };

        int m_DaysBetweenChecks = 12;

        FailArgs failArgs;

        ContextMenu contextMenu;
        MenuType CurrMenuType = MenuType.Nothing;
        bool isMenuVisible;

        // changes
        bool ShowButtonUpdateNow;

        string currentActionText;


        // menu items
        ToolStripItem toolStripItem;
        MenuItem menuItem;

        static readonly Bitmap BmpInfo = new Bitmap(typeof (AutomaticUpdater), "info.png");
        static readonly Bitmap BmpWorking = new Bitmap(typeof (AutomaticUpdater), "update-working.png");
        static readonly Bitmap BmpNotify = new Bitmap(typeof (AutomaticUpdater), "update-notify.png");
        static readonly Bitmap BmpSuccess = new Bitmap(typeof (AutomaticUpdater), "tick.png");
        static readonly Bitmap BmpFailed = new Bitmap(typeof (AutomaticUpdater), "cross.png");

        SolidBrush backBrush;

        #region Events

        /// <summary>Event is raised before the update checking begins.</summary>
        [Description("Event is raised before the update checking begins."),
        Category("Updater")]
        public event BeforeHandler BeforeChecking;

        /// <summary>Event is raised before the downloading of the update begins.</summary>
        [Description("Event is raised before the downloading of the update begins."),
        Category("Updater")]
        public event BeforeHandler BeforeDownloading;

        /// <summary>Event is raised before the installation of the update begins.</summary>
        [Description("Event is raised before the installation of the update begins."),
        Category("Updater")]
        public event BeforeHandler BeforeInstalling;

        /// <summary>Event is raised when checking or updating is cancelled.</summary>
        [Description("Event is raised when checking or updating is cancelled."),
        Category("Updater")]
        public event EventHandler Cancelled;

        /// <summary>Event is raised when the checking for updates fails.</summary>
        [Description("Event is raised when the checking for updates fails."),
        Category("Updater")]
        public event FailHandler CheckingFailed;

        /// <summary>Event is raised after you or your user invoked InstallNow(). You should close your app as quickly as possible (because wyUpdate will be waiting).</summary>
        [Description("Event is raised after you or your user invoked InstallNow(). You should close your app as quickly as possible (because wyUpdate will be waiting)."),
        Category("Updater")]
        public event EventHandler CloseAppNow;

        /// <summary>Event is raised when an update can't be installed and the closing is aborted.</summary>
        [Description("Event is raised when an update can't be installed and the closing is aborted."),
        Category("Updater")]
        public event EventHandler ClosingAborted;

        /// <summary>Event is raised when the update fails to download or extract.</summary>
        [Description("Event is raised when the update fails to download or extract."),
        Category("Updater")]
        public event FailHandler DownloadingOrExtractingFailed;

        /// <summary>Event is raised when the current update step progress changes.</summary>
        [Description("Event is raised when the current update step progress changes."),
        Category("Updater")]
        public event UpdateProgressChanged ProgressChanged;

        /// <summary>Event is raised when the update is ready to be installed.</summary>
        [Description("Event is raised when the update is ready to be installed."),
        Category("Updater")]
        public event EventHandler ReadyToBeInstalled;

        /// <summary>Event is raised when a new update is found.</summary>
        [Description("Event is raised when a new update is found."),
        Category("Updater")]
        public event EventHandler UpdateAvailable;

        /// <summary>Event is raised when an update fails to install.</summary>
        [Description("Event is raised when an update fails to install."),
        Category("Updater")]
        public event FailHandler UpdateFailed;

        /// <summary>Event is raised when an update installs successfully.</summary>
        [Description("Event is raised when an update installs successfully."),
        Category("Updater")]
        public event SuccessHandler UpdateSuccessful;

        /// <summary>Event is raised when the latest version is already installed.</summary>
        [Description("Event is raised when the latest version is already installed."),
        Category("Updater")]
        public event SuccessHandler UpToDate;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TextChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets whether to slide the AutomaticUpdater out when you hover over it and in when you hover away.
        /// </summary>
        [Description("Whether to slide the AutomaticUpdater out when you hover over it and in when you hover away."),
        DefaultValue(true),
        Category("Updater")]
        public bool Animate { get; set; }

        /// <summary>
        /// Gets or sets the arguments to pass to your app when it's being restarted after an update.
        /// </summary>
        [Description("The arguments to pass to your app when it's being restarted after an update."),
        DefaultValue(null),
        Category("Updater")]
        public string Arguments
        {
            get { return auBackend.Arguments; }
            set { auBackend.Arguments = value; }
        }

        /// <summary>
        /// Gets the changes for the new update.
        /// </summary>
        [Browsable(false)]
        public string Changes { get { return auBackend.Changes; } }

        /// <summary>
        /// Gets if this AutomaticUpdater has hidden this form and preparing to install an update.
        /// </summary>
        [Browsable(false)]
        public bool ClosingForInstall { get { return auBackend.ClosingForInstall; } }

        /// <summary>
        /// Gets or sets the number of days to wait before automatically re-checking for updates.
        /// </summary>
        [Description("The number of days to wait before automatically re-checking for updates."),
        DefaultValue(12),
        Category("Updater")]
        public int DaysBetweenChecks
        {
            get { return m_DaysBetweenChecks; }
            set { m_DaysBetweenChecks = value; }
        }

        /// <summary>
        /// Gets the GUID (Globally Unique ID) of the automatic updater. It is recommended you set this value (especially if there is more than one exe for your product).
        /// </summary>
        /// <exception cref="System.Exception">Thrown when trying to set the GUID at runtime.</exception>
        [Description("The GUID (Globally Unique ID) of the automatic updater. It is recommended you set this value (especially if there is more than one exe for your product)."),
        Category("Updater"),
        DefaultValue(null),
        EditorAttribute(typeof(GUIDEditor), typeof(UITypeEditor)),
        EditorBrowsable(EditorBrowsableState.Never)]
        public string GUID
        {
            get { return auBackend.GUID; }
            set { auBackend.GUID = value; }
        }

        bool ShouldSerializeGUID()
        {
            return !string.IsNullOrEmpty(auBackend.GUID);
        }

        /// <summary>
        /// Gets or sets whether the AutomaticUpdater control should stay hidden even when the user should be notified. (Not recommended).
        /// </summary>
        [Description("Keeps the AutomaticUpdater control hidden even when the user should be notified. (Not recommended)"),
        DefaultValue(false),
        Category("Updater")]
        public bool KeepHidden { get; set; }

        /// <summary>
        /// Gets the date the updates were last checked for.
        /// </summary>
        [Browsable(false)]
        public DateTime LastCheckDate
        {
            get { return auBackend.LastCheckDate; }
        }

        /// <summary>
        /// Gets and sets the MenuItem that will be used to check for updates.
        /// </summary>
        [Description("The MenuItem that will be used to check for updates."),
        Category("Updater"),
        DefaultValue(null)]
        public MenuItem MenuItem
        {
            get
            {
                return menuItem;
            }
            set
            {
                if (menuItem != null)
                    menuItem.Click -= menuItem_Click;

                menuItem = value;

                if (menuItem != null)
                    menuItem.Click += menuItem_Click;
            }
        }

        /// <summary>
        /// Gets and sets the ToolStripItem that will be used to check for updates.
        /// </summary>
        [Description("The ToolStripItem that will be used to check for updates."),
        Category("Updater"),
        DefaultValue(null)]
        public ToolStripItem ToolStripItem
        {
            get
            {
                return toolStripItem;
            }
            set
            {
                if (toolStripItem != null)
                    toolStripItem.Click -= menuItem_Click;

                toolStripItem = value;

                if (toolStripItem != null)
                    toolStripItem.Click += menuItem_Click;
            }
        }

        AUTranslation translation = new AUTranslation();

        /// <summary>
        /// The translation for the english strings in the AutomaticUpdater control.
        /// </summary>
        /// <exception cref="ArgumentNullException">The translation cannot be null.</exception>
        [Browsable(false)]
        public AUTranslation Translation
        {
            get { return translation; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "No translation present.");

                translation = value;
            }
        }

        bool ShouldSerializeTranslation() { return false; }

        /// <summary>
        /// Gets the update step the AutomaticUpdater is currently on.
        /// </summary>
        [Browsable(false)]
        public UpdateStepOn UpdateStepOn { get { return auBackend.UpdateStepOn; } }

        /// <summary>
        /// Gets or sets how much this AutomaticUpdater control should do without user interaction.
        /// </summary>
        [Description("How much this AutomaticUpdater control should do without user interaction."),
        DefaultValue(UpdateType.Automatic),
        Category("Updater")]
        public UpdateType UpdateType
        {
            get { return auBackend.UpdateType; }
            set { auBackend.UpdateType = value; }
        }

        /// <summary>
        /// Gets the version of the new update.
        /// </summary>
        [Browsable(false)]
        public string Version { get { return auBackend.Version; } }

        /// <summary>
        /// Gets or sets the seconds to wait after the form is loaded before checking for updates.
        /// </summary>
        [Description("Seconds to wait after the form is loaded before checking for updates."),
        DefaultValue(10),
        Category("Updater")]
        public int WaitBeforeCheckSecs
        {
            get { return m_WaitBeforeCheckSecs; }
            set
            {
                m_WaitBeforeCheckSecs = value;

                tmrWaitBeforeCheck.Interval = m_WaitBeforeCheckSecs * 1000;
            }
        }

        /// <summary>
        /// Gets or sets the arguments to pass to wyUpdate when it is started to check for updates.
        /// </summary>
        [Description("Arguments to pass to wyUpdate when it is started to check for updates."),
        Category("Updater")]
        public string wyUpdateCommandline
        {
            get { return auBackend.wyUpdateCommandline; }
            set { auBackend.wyUpdateCommandline = value; }
        }

        /// <summary>
        /// Gets or sets the relative path to the wyUpdate (e.g. wyUpdate.exe  or  SubDir\\wyUpdate.exe)
        /// </summary>
        [Description("The relative path to the wyUpdate (e.g. wyUpdate.exe  or  SubDir\\wyUpdate.exe)"), 
        DefaultValue("wyUpdate.exe"),
        Category("Updater")]
        public string wyUpdateLocation
        {
            get { return auBackend.wyUpdateLocation; }
            set { auBackend.wyUpdateLocation = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;

                RefreshTextRect();

                Invalidate();
            }
        }

        [Browsable(false)]
        public Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (DesignMode && Animate)
                Size = new Size(16, 16);
            else
                Height = Math.Max(16, Font.Height);

            base.OnSizeChanged(e);
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Not already disposed ?
            if (disposing)
            {
                // Not already disposed ?
                if (auBackend != null)
                {
                    auBackend.Dispose(); // Dispose it
                    auBackend = null; // Its now inaccessible
                }

                if (backBrush != null)
                {
                    backBrush.Dispose(); // Dispose it
                    backBrush = null; // Its now inaccessible
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        //Methods and such

        /// <summary>
        /// Represents an AutomaticUpdater control.
        /// </summary>
        public AutomaticUpdater()
        {
            Animate = true;

            // This turns on double buffering of all custom GDI+ drawing
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.OptimizedDoubleBuffer 
                | ControlStyles.ResizeRedraw 
                | ControlStyles.UserPaint
                | ControlStyles.FixedHeight
                | ControlStyles.FixedWidth, true);

            ani.Size = new Size(16, 16);
            ani.Location = new Point(0, 0);

            ani.Rows = 5;
            ani.Columns = 10;
            Controls.Add(ani);


            ani.MouseEnter += ani_MouseEnter;
            ani.MouseLeave += ani_MouseLeave;
            ani.MouseDown += ani_MouseDown;

            if (DesignMode)
                ani.Visible = false;

            // events for the timers
            tmrCollapse.Tick += tmrCollapse_Tick;
            tmrAniExpandCollapse.Tick += tmrAniExpandCollapse_Tick;
            tmrWaitBeforeCheck.Tick += tmrWaitBeforeCheck_Tick;

            Size = new Size(16, 16);

            backBrush = new SolidBrush(BackColor);

            // use all events from the AutomaticUpdater backend
            auBackend.BeforeChecking += auBackend_BeforeChecking;
            auBackend.BeforeDownloading += auBackend_BeforeDownloading;
            auBackend.BeforeExtracting += auBackend_BeforeExtracting;
            auBackend.BeforeInstalling += auBackend_BeforeInstalling;
            auBackend.Cancelled += auBackend_Cancelled;
            auBackend.ReadyToBeInstalled += auBackend_ReadyToBeInstalled;
            auBackend.UpdateAvailable += auBackend_UpdateAvailable;
            auBackend.UpdateSuccessful += auBackend_UpdateSuccessful;
            auBackend.UpToDate += auBackend_UpToDate;

            auBackend.ProgressChanged += auBackend_ProgressChanged;

            auBackend.CheckingFailed += auBackend_CheckingFailed;
            auBackend.DownloadingFailed += auBackend_DownloadingFailed;
            auBackend.ExtractingFailed += auBackend_ExtractingFailed;
            auBackend.UpdateFailed += auBackend_UpdateFailed;

            auBackend.ClosingAborted += auBackend_ClosingAborted;
            auBackend.UpdateStepMismatch += auBackend_UpdateStepMismatch;

            auBackend.CloseAppNow += auBackend_CloseAppNow;
        }

        /// <summary>Represents an AutomaticUpdater control.</summary>
        /// <param name="parentControl">The owner form of the AutomaticUpdater.</param>
        public AutomaticUpdater(Form parentControl)
            : this()
        {
            ownerForm = parentControl;
            ownerForm.Load += ownerForm_Load;
        }

        void auBackend_CloseAppNow(object sender, EventArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new EventHandler(auBackend_CloseAppNow), new object[] { null, e });
                return;
            }

            // close this application so it can be updated
            if (CloseAppNow != null)
                CloseAppNow(this, EventArgs.Empty);
            else
                Application.Exit();
        }

        void auBackend_UpToDate(object sender, SuccessArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new SuccessHandler(auBackend_UpToDate), new object[] { null, e });
                return;
            }

            Text = translation.AlreadyUpToDate;

            if (Visible)
                UpdateStepSuccessful(MenuType.AlreadyUpToDate);

            SetMenuText(translation.CheckForUpdatesMenu);

            if (UpToDate != null)
                UpToDate(this, e);
        }

        void auBackend_UpdateSuccessful(object sender, SuccessArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new SuccessHandler(auBackend_UpdateSuccessful), new object[] { null, e });
                return;
            }

            // show the control
            Visible = !KeepHidden;

            Text = translation.SuccessfullyUpdated.Replace("%version%", Version);
            UpdateStepSuccessful(MenuType.UpdateSuccessful);

            if (UpdateSuccessful != null)
                UpdateSuccessful(this, e);
        }

        void auBackend_UpdateFailed(object sender, FailArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new FailHandler(auBackend_UpdateFailed), new object[] { null, e });
                return;
            }

            // show the control
            Visible = !KeepHidden;

            failArgs = e;

            // show failed Text & icon
            Text = translation.UpdateFailed;
            CreateMenu(MenuType.Error);
            AnimateImage(BmpFailed, true);

            if (UpdateFailed != null)
                UpdateFailed(this, e);
        }

        void auBackend_UpdateAvailable(object sender, EventArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new EventHandler(auBackend_UpdateAvailable), new object[] { null, e });
                return;
            }

            CreateMenu(MenuType.DownloadAndChanges);

            SetUpdateStepOn(UpdateStepOn.UpdateAvailable);

            if (!KeepHidden)
                Show();

            // temporarily disable the collapse timer
            tmrCollapse.Enabled = false;

            // animate this open
            if (Animate)
                BeginAniOpen();

            AnimateImage(BmpNotify, true);

            SetMenuText(translation.DownloadUpdateMenu);

            if (UpdateAvailable != null)
                UpdateAvailable(this, e);
        }

        void auBackend_ReadyToBeInstalled(object sender, EventArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new EventHandler(auBackend_ReadyToBeInstalled), new object[] { null, e });
                return;
            }

            CreateMenu(MenuType.InstallAndChanges);

            // UpdateDownloaded or UpdateReadyToInstall
            SetUpdateStepOn(auBackend.UpdateStepOn);

            if (!KeepHidden)
                Show();

            // temporarily disable the collapse timer
            tmrCollapse.Enabled = false;

            // animate this open
            if (Animate)
                BeginAniOpen();

            AnimateImage(BmpInfo, true);

            SetMenuText(translation.InstallUpdateMenu);


            if (ReadyToBeInstalled != null)
                ReadyToBeInstalled(this, e);
        }

        void auBackend_Cancelled(object sender, EventArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new EventHandler(auBackend_Cancelled), new object[] { null, e });
                return;
            }

            // stop animation & hide
            ani.StopAnimation();
            Visible = false;

            SetMenuText(translation.CheckForUpdatesMenu);

            if (Cancelled != null)
                Cancelled(this, e);
        }

        void auBackend_BeforeChecking(object sender, BeforeArgs e)
        {
            // disable any scheduled checking
            tmrWaitBeforeCheck.Enabled = false;

            SetMenuText(translation.CancelCheckingMenu);

            if (BeforeChecking != null)
                BeforeChecking(this, e);

            if (e.Cancel)
            {
                // close wyUpdate
                auBackend.Cancel();
                return;
            }

            // show the working animation
            SetUpdateStepOn(UpdateStepOn.Checking);
            UpdateProcessing(false);

            // setup the context menu
            CreateMenu(MenuType.CheckingMenu);
        }

        void auBackend_BeforeDownloading(object sender, BeforeArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new BeforeHandler(auBackend_BeforeDownloading), new object[] { null, e });
                return;
            }

            if (BeforeDownloading != null)
                BeforeDownloading(this, e);

            if (e.Cancel)
                return;

            SetMenuText(translation.CancelUpdatingMenu);

            // if the control is hidden show it now (so the user can cancel the downloading if they want)
            // show the 'working' animation
            SetUpdateStepOn(UpdateStepOn.DownloadingUpdate);
            UpdateProcessing(true);

            CreateMenu(MenuType.CancelDownloading);
        }

        void auBackend_BeforeExtracting(object sender, BeforeArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new BeforeHandler(auBackend_BeforeExtracting), new object[] { null, e });
                return;
            }

            SetUpdateStepOn(UpdateStepOn.ExtractingUpdate);

            CreateMenu(MenuType.CancelExtracting);
        }

        void auBackend_BeforeInstalling(object sender, BeforeArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                if (ownerForm.IsHandleCreated)
                    ownerForm.Invoke(new BeforeHandler(auBackend_BeforeInstalling), new object[] { null, e });

                return;
            }

            if (BeforeInstalling != null)
                BeforeInstalling(this, e);
        }

        void auBackend_CheckingFailed(object sender, FailArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new FailHandler(auBackend_CheckingFailed), new object[] { null, e });
                return;
            }

            UpdateStepFailed(e);

            Text = translation.FailedToCheck;

            if (CheckingFailed != null)
                CheckingFailed(this, e);
        }

        void auBackend_DownloadingFailed(object sender, FailArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new FailHandler(auBackend_DownloadingFailed), new object[] { null, e });
                return;
            }

            UpdateStepFailed(e);

            Text = translation.FailedToDownload;

            if (DownloadingOrExtractingFailed != null)
                DownloadingOrExtractingFailed(this, e);
        }

        void auBackend_ExtractingFailed(object sender, FailArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new FailHandler(auBackend_ExtractingFailed), new object[] { null, e });
                return;
            }

            UpdateStepFailed(e);

            Text = translation.FailedToExtract;

            if (DownloadingOrExtractingFailed != null)
                DownloadingOrExtractingFailed(this, e);
        }

        void UpdateStepFailed(FailArgs e)
        {
            if (e.wyUpdatePrematureExit)
            {
                e.ErrorTitle = translation.PrematureExitTitle;

                // use the general "premature exit" message only when there's no other message present
                if (e.ErrorMessage == null || e.ErrorMessage == AUTranslation.C_PrematureExitMessage)
                    e.ErrorMessage = translation.PrematureExitMessage;
            }

            failArgs = e;

            //only show the error if this is visible
            if (Visible)
            {
                CreateMenu(MenuType.Error);
                AnimateImage(BmpFailed, true);
            }

            SetMenuText(translation.CheckForUpdatesMenu);
        }

        void auBackend_ProgressChanged(object sender, int progress)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new UpdateProgressChanged(auBackend_ProgressChanged), new object[] { null, progress });
                return;
            }

            // update progress status (only for greater than 0%)
            if (progress > 0)
                Text = currentActionText + ", " + progress + "%";

            // call the progress changed event
            if (ProgressChanged != null)
                ProgressChanged(this, progress);
        }

        void auBackend_ClosingAborted(object sender, EventArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                if (ownerForm.IsHandleCreated)
                    ownerForm.Invoke(new EventHandler(auBackend_ClosingAborted), new object[] { null, e });
                return;
            }

            // we need to show the form (it was hidden in ISupport() )
            if (auBackend.ClosingForInstall)
            {
                ownerForm.ShowInTaskbar = true;
                ownerForm.WindowState = FormWindowState.Normal;
            }

            if (ClosingAborted != null)
                ClosingAborted(this, EventArgs.Empty);
        }

        void auBackend_UpdateStepMismatch(object sender, EventArgs e)
        {
            // call this function from ownerForm's thread context
            if (sender != null)
            {
                ownerForm.Invoke(new EventHandler(auBackend_UpdateStepMismatch), new object[] { null, e });
                return;
            }

            SetUpdateStepOn(auBackend.UpdateStepOn);

            // set the working progress animation
            if (auBackend.UpdateStepOn == UpdateStepOn.Checking
                || auBackend.UpdateStepOn == UpdateStepOn.DownloadingUpdate
                || auBackend.UpdateStepOn == UpdateStepOn.ExtractingUpdate)
            {
                UpdateProcessing(false);
            }
        }

        /// <summary>
        /// The owner form of the AutomaticUpdater.
        /// </summary>
        [Browsable(false)]
        public Form ContainerForm
        {
            get { return ownerForm; }
            set
            {
                if (ownerForm != null)
                    ownerForm.Load -= ownerForm_Load;

                ownerForm = value;
                ownerForm.Load += ownerForm_Load;
            }
        }

        public override ISite Site
        {
            set
            {
                // Runs at design time, ensures designer initializes ContainerControl
                base.Site = value;
                if (value == null) return;
                IDesignerHost service = value.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service == null) return;
                IComponent rootComponent = service.RootComponent;
                ContainerForm = rootComponent as Form;
            }
        }
        

        bool insideChildControl;
        bool insideSelf;

        void ani_MouseLeave(object sender, EventArgs e)
        {
            insideChildControl = false;

            if (Animate && !insideSelf && isFullExpanded && !isMenuVisible)
                tmrCollapse.Enabled = true;
        }

        void ani_MouseEnter(object sender, EventArgs e)
        {
            insideChildControl = true;

            if (Animate)
            {
                tmrCollapse.Enabled = false;

                if (!isFullExpanded)
                    BeginAniOpen();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            insideSelf = true;

            if (Animate)
            {
                tmrCollapse.Enabled = false;

                if (!isFullExpanded)
                    BeginAniOpen();
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            insideSelf = false;

            if (Animate && !insideChildControl && isFullExpanded && !isMenuVisible)
                tmrCollapse.Enabled = true;

            base.OnMouseLeave(e);
        }

        void tmrCollapse_Tick(object sender, EventArgs e)
        {
            // begin collapse animation
            BeginAniClose();

            // disable this timer
            tmrCollapse.Stop();
        }

        bool skipNextMenuShow;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            ani_MouseDown(null, null);
            
            base.OnMouseDown(e);
        }

        void ani_MouseDown(object sender, MouseEventArgs e)
        {
            if (!(insideChildControl || insideSelf))
                //skip next menu show when "reclicking" while the menu is still visible
                skipNextMenuShow = true;

            if (skipNextMenuShow)
                skipNextMenuShow = false;
            else
                ShowContextMenu();
        }

        void ShowContextMenu()
        {
            if (contextMenu != null)
            {
                isMenuVisible = true;

                if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
                    contextMenu.Show(this, new Point(Width, Height), LeftRightAlignment.Left);
                else
                    contextMenu.Show(this, new Point(0, Height), LeftRightAlignment.Right);
            }
        }


        #region Animation

        // calculate the expanded width based on the changing text
        int expandedWidth = 16;
        const int collapsedWidth = 16;

        const int maxTimeTicks = 30;
        int totalTimeTicks;
        int startSize, sizeChange;

        bool isFullExpanded;

        void tmrAniExpandCollapse_Tick(object sender, EventArgs e)
        {
            if (startSize - Width + sizeChange == 0)
            {
                isFullExpanded = Width != collapsedWidth;

                //enable the collapse timer
                if (isFullExpanded && !insideChildControl && !insideSelf && !isMenuVisible)
                    tmrCollapse.Enabled = true;

                tmrAniExpandCollapse.Stop();
            }
            else
            {
                totalTimeTicks++;

                int DeltaAnileft;

                if (totalTimeTicks == maxTimeTicks)
                    DeltaAnileft = startSize + sizeChange - Width;
                else
                    DeltaAnileft = (int)(sizeChange * (-Math.Pow(2, (float)(-10 * totalTimeTicks) / maxTimeTicks) + 1) + startSize) - Width;

                Width += DeltaAnileft;

                if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
                    Left -= DeltaAnileft;
            }
        }


        void BeginAniClose()
        {
            // totalDist = destX - startX
            sizeChange = collapsedWidth - Width;

            // bail out if no tabs need to be moved
            if (sizeChange == 0)
                return;

            // set the start position
            startSize = Width;

            // begin the scrolling animation
            totalTimeTicks = 0;

            // begin the closing animation
            isFullExpanded = false;
            tmrAniExpandCollapse.Start();
        }

        void BeginAniOpen()
        {
            // totalDist = destX - startX
            sizeChange = expandedWidth - Width;

            // bail out if no tabs need to be moved
            if (sizeChange == 0)
            {
                tmrCollapse.Enabled = true;

                return;
            }
                
            // set the start position
            startSize = Width;

            // begin the scrolling animation
            totalTimeTicks = 0;


            // begin the opening animation
            tmrAniExpandCollapse.Start();
        }

        #endregion Animation

        void menuItem_Click(object sender, EventArgs e)
        {
            switch (UpdateStepOn)
            {
                case UpdateStepOn.Checking:
                case UpdateStepOn.DownloadingUpdate:
                case UpdateStepOn.ExtractingUpdate:

                    Cancel();
                    break;

                case UpdateStepOn.UpdateReadyToInstall:
                case UpdateStepOn.UpdateAvailable:
                case UpdateStepOn.UpdateDownloaded:

                    InstallNow();
                    break;

                default:

                    ForceCheckForUpdate(false, true);
                    break;
            }
        }

        void SetMenuText(string text)
        {
            if (menuItem != null)
                menuItem.Text = text;
            else if (toolStripItem != null)
                toolStripItem.Text = text;
        }


        void InstallNow_Click(object sender, EventArgs e)
        {
            InstallNow();
        }

        /// <summary>Proceed with the download and installation of pending updates.</summary>
        public void InstallNow()
        {
            auBackend.InstallNow();
        }

        void CancelUpdate_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        void Hide_Click(object sender, EventArgs e)
        {
            Hide();
        }

        /// <summary>
        /// Cancel the checking, downloading, or extracting currently in progress.
        /// </summary>
        public void Cancel()
        {
            auBackend.Cancel();
        }

        void ViewChanges_Click(object sender, EventArgs e)
        {
            using (frmChanges changeForm = new frmChanges(auBackend.Version, auBackend.RawChanges, auBackend.AreChangesRTF, ShowButtonUpdateNow, translation))
            {
                changeForm.ShowDialog(ownerForm);

                if (changeForm.UpdateNow)
                    InstallNow();
            }
        }

        void ViewError_Click(object sender, EventArgs e)
        {
            using (frmError errorForm = new frmError(failArgs, translation))
            {
                errorForm.ShowDialog(ownerForm);

                if (errorForm.TryAgainLater)
                    TryAgainLater_Click(this, EventArgs.Empty);
            }
        }

        void TryAgainLater_Click(object sender, EventArgs e)
        {
            // we'll check on next start of this app,
            // just hide for now.
            Hide();
        }

        void TryAgainNow_Click(object sender, EventArgs e)
        {
            // check for updates (if we're actually further along, wyUpdate will set us straight)
            ForceCheckForUpdate(true);
        }


        void CreateMenu(MenuType NewMenuType)
        {
            // if the context menu is visible
            if (isMenuVisible)
            {
                // hide the context menu
                SendKeys.Send("{ESC}");
            }


            // destroy previous menu type (by removing events associated with it)
            // unregister the events to existing menu items
            switch (CurrMenuType)
            {
                case MenuType.CancelDownloading:
                case MenuType.CancelExtracting:
                case MenuType.CheckingMenu:
                    contextMenu.MenuItems[0].Click -= CancelUpdate_Click;
                    break;

                case MenuType.InstallAndChanges:
                case MenuType.DownloadAndChanges:
                    contextMenu.MenuItems[0].Click -= InstallNow_Click;
                    contextMenu.MenuItems[1].Click -= ViewChanges_Click;
                    break;

                case MenuType.Error:
                    contextMenu.MenuItems[0].Click -= TryAgainLater_Click;
                    contextMenu.MenuItems[1].Click -= TryAgainNow_Click;
                    contextMenu.MenuItems[3].Click -= ViewError_Click;
                    break;

                case MenuType.UpdateSuccessful:
                    contextMenu.MenuItems[0].Click -= Hide_Click;
                    contextMenu.MenuItems[1].Click -= ViewChanges_Click;
                    break;

                case MenuType.AlreadyUpToDate:
                    contextMenu.MenuItems[0].Click -= Hide_Click;
                    break;
            }


            // create new menu type & add new events

            switch (NewMenuType)
            {
                case MenuType.Nothing:

                    contextMenu = null;

                    break;
                case MenuType.CheckingMenu:
                    contextMenu = new ContextMenu(new[] { new MenuItem(translation.StopChecking, CancelUpdate_Click) });
                    break;
                case MenuType.CancelDownloading:
                    contextMenu = new ContextMenu(new[] { new MenuItem(translation.StopDownloading, CancelUpdate_Click) });
                    break;
                case MenuType .CancelExtracting:
                    contextMenu = new ContextMenu(new[] { new MenuItem(translation.StopExtracting, CancelUpdate_Click) });
                    break;
                case MenuType.InstallAndChanges:
                case MenuType.DownloadAndChanges:

                    contextMenu = new ContextMenu(new[]
                                { 
                                    new MenuItem(NewMenuType == MenuType.InstallAndChanges ? translation.InstallUpdateMenu : translation.DownloadUpdateMenu, InstallNow_Click), 
                                    new MenuItem(translation.ViewChangesMenu.Replace("%version%", Version), ViewChanges_Click)
                                });
                    contextMenu.MenuItems[0].DefaultItem = true;
                    ShowButtonUpdateNow = true;

                    break;
                case MenuType.Error:

                    contextMenu = new ContextMenu(new[]
                                { 
                                    new MenuItem(translation.TryAgainLater, TryAgainLater_Click), 
                                    new MenuItem(translation.TryAgainNow, TryAgainNow_Click),
                                    new MenuItem("-"),
                                    new MenuItem(translation.ViewError, ViewError_Click)
                                });
                    contextMenu.MenuItems[0].DefaultItem = true;
                    
                    break;
                case MenuType.UpdateSuccessful:

                    contextMenu = new ContextMenu(new[]
                                {
                                    new MenuItem(translation.HideMenu, Hide_Click),
                                    new MenuItem(translation.ViewChangesMenu.Replace("%version%", Version), ViewChanges_Click)
                                });

                    ShowButtonUpdateNow = false;

                    break;

                case MenuType.AlreadyUpToDate:

                    contextMenu = new ContextMenu(new[]
                                {
                                    new MenuItem(translation.HideMenu, Hide_Click)
                                });
                    break;
            }

            CurrMenuType = NewMenuType;
        }


        void tmrWaitBeforeCheck_Tick(object sender, EventArgs e)
        {
            Visible = false;
            auBackend.ForceCheckForUpdate(false);
        }

        /// <summary>
        /// Check for updates forcefully -- returns true if the updating has begun. Use the "CheckingFailed", "UpdateAvailable", or "UpToDate" events for the result.
        /// </summary>
        /// <param name="recheck">Recheck with the servers regardless of cached updates, etc.</param>
        /// <returns>Returns true if checking has begun, false otherwise.</returns>
        public bool ForceCheckForUpdate(bool recheck)
        {
            return ForceCheckForUpdate(recheck, false);
        }

        bool ForceCheckForUpdate(bool recheck, bool fromUI)
        {
            // if not already checking for updates then begin checking.
            if (UpdateStepOn == UpdateStepOn.Nothing || (recheck && UpdateStepOn == UpdateStepOn.UpdateAvailable))
            {
                if (recheck || fromUI)
                    Visible = !KeepHidden;

                return auBackend.ForceCheckForUpdate(recheck);
            }

            return false;
        }

        /// <summary>
        /// Check for updates forcefully -- returns true if the updating has begun. Use the "CheckingFailed", "UpdateAvailable", or "UpToDate" events for the result.
        /// </summary>
        /// <returns>Returns true if checking has begun, false otherwise.</returns>
        public bool ForceCheckForUpdate()
        {
            return ForceCheckForUpdate(false, false);
        }

        void RefreshTextRect()
        {
            textRect = new Rectangle(new Point(24, 1), TextRenderer.MeasureText(Text, Font, Size, TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix));

            expandedWidth = textRect.Width + textRect.X;

            if (isFullExpanded && Width != expandedWidth)
            {
                // reposition an resize the control
                if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
                    Left -= expandedWidth - Width;

                Width += expandedWidth - Width;
            }
                // if expanding
            else if (tmrAniExpandCollapse.Enabled && sizeChange > 0)
            {
                // re-start the expansion with the new size
                BeginAniOpen();
            }

            // 8  = 16 / 2
            textRect.Y = 8 - textRect.Height / 2 - 1;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            RefreshTextRect();

            Height = Math.Max(16, Font.Height);

            base.OnFontChanged(e);
        }

        void UpdateStepSuccessful(MenuType menuType)
        {
            // create the "hide" menu
            CreateMenu(menuType);

            if (Animate)
            {
                // temporarily diable the collapse timer
                tmrCollapse.Enabled = false;

                // animate this open
                BeginAniOpen();
            }

            AnimateImage(BmpSuccess, true);
        }

        void UpdateProcessing(bool forceShow)
        {
            if (forceShow && !KeepHidden)
                Show();

            if (Animate)
            {
                // temporarily diable the collapse timer
                tmrCollapse.Enabled = false;

                // animate this open
                BeginAniOpen();
            }

            AnimateImage(BmpWorking, false);
        }

        void AnimateImage(Image img, bool staticImg)
        {
            ani.StopAnimation();
            ani.StaticImage = staticImg;
            ani.AnimationInterval = 25;
            ani.BaseImage = img;
            ani.StartAnimation();
        }

        void SetUpdateStepOn(UpdateStepOn uso)
        {
            switch(uso)
            {
                case UpdateStepOn.Checking:
                    Text = currentActionText = translation.Checking;
                    break;

                case UpdateStepOn.DownloadingUpdate:
                    Text = currentActionText = translation.Downloading;
                    break;

                case UpdateStepOn.ExtractingUpdate:
                    Text = currentActionText = translation.Extracting;
                    break;

                case UpdateStepOn.UpdateAvailable:
                    Text = translation.UpdateAvailable;
                    break;

                case UpdateStepOn.UpdateDownloaded:
                    Text = translation.UpdateAvailable;
                    break;

                case UpdateStepOn.UpdateReadyToInstall:
                    Text = translation.InstallOnNextStart;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = InterpolationMode.Low;

            e.Graphics.FillRectangle(backBrush, ClientRectangle);

            //Add the text
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor, TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            backBrush = new SolidBrush(BackColor);
        }

        protected override void WndProc(ref Message m)
        {
            //0x0212 == WM_EXITMENULOOP
            if (m.Msg == 0x212)
            {
                //this message is only sent when a ContextMenu is closed (not a ContextMenuStrip)
                isMenuVisible = false;

                //begin collapsing the update helper
                if (Animate && (isFullExpanded || tmrAniExpandCollapse.Enabled))
                    tmrCollapse.Enabled = true;
            }

            base.WndProc(ref m);
        }

        void ISupportInitialize.BeginInit() { }

        void ISupportInitialize.EndInit()
        {
            if (DesignMode)
                return;

            auBackend.Initialize();

            if (auBackend.ClosingForInstall)
            {
                // hide self if there's an update pending
                ownerForm.ShowInTaskbar = false;
                ownerForm.WindowState = FormWindowState.Minimized;
            }
        }

        void ownerForm_Load(object sender, EventArgs e)
        {
            // if we've already recieved messages from wyUpdate
            // then process them now.
            auBackend.FlushResponses();

            SetMenuText(translation.CheckForUpdatesMenu);

            // if we want to kill ouself, then don't bother checking for updates
            if (auBackend.ClosingForInstall)
                return;

            auBackend.AppLoaded();

            if (UpdateStepOn != UpdateStepOn.Nothing)
            {
                // show the updater control
                if (!KeepHidden)
                    Show();
            }
            else if (auBackend.UpdateType != UpdateType.DoNothing) // UpdateStepOn == UpdateStepOn.Nothing
            {
                // see if enough days have elapsed since last check.
                TimeSpan span = DateTime.Now.Subtract(auBackend.LastCheckDate);

                if (span.Days >= m_DaysBetweenChecks)
                {
                    tmrWaitBeforeCheck.Enabled = true;
                }
            }
        }
    }
}