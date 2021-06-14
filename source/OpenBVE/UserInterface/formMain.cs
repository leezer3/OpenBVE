using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using LibRender2.MotionBlurs;
using OpenBve.Input;
using OpenBve.UserInterface;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Packages;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenTK.Input;
using ButtonState = OpenTK.Input.ButtonState;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace OpenBve {
	internal partial class formMain : Form
	{
		private formMain()
		{
			InitializeComponent();
			this.Text = Translations.GetInterfaceString("program_title");
		}

		public sealed override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		// show main dialog
		internal struct MainDialogResult
		{
			/// <summary>Whether to start the simulation</summary>
			internal bool Start;
			/// <summary>The absolute on-disk path of the route file to start the simulation with</summary>
			internal string RouteFile;
			/// <summary>The last file an error was encountered on (Used for changing character encodings)</summary>
			internal string ErrorFile;
			/// <summary>The text encoding of the selected route file</summary>
			internal System.Text.Encoding RouteEncoding;
			/// <summary>The absolute on-disk path of the train folder to start the simulation with</summary>
			internal string TrainFolder;
			/// <summary>The text encoding of the selected train</summary>
			internal System.Text.Encoding TrainEncoding;
			/// <summary>Whether the consist of the train is to be reversed on start</summary>
			internal bool ReverseConsist;
			internal string InitialStation;
			internal double StartTime;
			internal bool AIDriver;
			internal bool FullScreen;
			internal int Width;
			internal int Height;
		}
		internal static MainDialogResult ShowMainDialog(MainDialogResult initial)
		{
			using (formMain Dialog = new formMain())
			{
				Dialog.Result = initial;
				Dialog.ShowDialog();
				MainDialogResult result = Dialog.Result;
				//Dispose of the worker thread when closing the form
				//If it's still running, it attempts to update a non-existant form and crashes nastily
				Dialog.DisposePreviewRouteThread();
				if (!OpenTK.Configuration.RunningOnMacOS)
				{
					Dialog.trainWatcher.Dispose();
					Dialog.routeWatcher.Dispose();
				}
				Dialog.Dispose();
				return result;
			}
		}

		// members
		private MainDialogResult Result;
		private int[] EncodingCodepages = new int[0];
		private Image JoystickImage = null;
		private Image RailDriverImage = null;
		private Image GamepadImage = null;
		private Image XboxImage = null;

		// ====
		// form
		// ====

		// load
		private void formMain_Load(object sender, EventArgs e)
		{
			this.MinimumSize = this.Size;
			if (Interface.CurrentOptions.MainMenuWidth == -1 & Interface.CurrentOptions.MainMenuHeight == -1)
			{
				this.WindowState = FormWindowState.Maximized;
			}
			else if (Interface.CurrentOptions.MainMenuWidth > 0 & Interface.CurrentOptions.MainMenuHeight > 0)
			{
				this.Size = new Size(Interface.CurrentOptions.MainMenuWidth, Interface.CurrentOptions.MainMenuHeight);
				this.CenterToScreen();
			}
			labelVersion.Text = @"v" + Application.ProductVersion + OpenBve.Program.VersionSuffix;
			if (IntPtr.Size != 4)
			{
				labelVersion.Text += @" 64-bit";
			}
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// form icon
			try
			{
				string File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico");
				this.Icon = new Icon(File);
			}
			catch { }
			radiobuttonStart.Appearance = Appearance.Button;
			radiobuttonStart.AutoSize = false;
			radiobuttonStart.Size = new Size(buttonClose.Width, buttonClose.Height);
			radiobuttonStart.TextAlign = ContentAlignment.MiddleCenter;
			radiobuttonReview.Appearance = Appearance.Button;
			radiobuttonReview.AutoSize = false;
			radiobuttonReview.Size = new Size(buttonClose.Width, buttonClose.Height);
			radiobuttonReview.TextAlign = ContentAlignment.MiddleCenter;
			radiobuttonControls.Appearance = Appearance.Button;
			radiobuttonControls.AutoSize = false;
			radiobuttonControls.Size = new Size(buttonClose.Width, buttonClose.Height);
			radiobuttonControls.TextAlign = ContentAlignment.MiddleCenter;
			radiobuttonOptions.Appearance = Appearance.Button;
			radiobuttonOptions.AutoSize = false;
			radiobuttonOptions.Size = new Size(buttonClose.Width, buttonClose.Height);
			radiobuttonOptions.TextAlign = ContentAlignment.MiddleCenter;
			radioButtonPackages.Appearance = Appearance.Button;
			radioButtonPackages.AutoSize = false;
			radioButtonPackages.Size = new Size(buttonClose.Width, buttonClose.Height);
			radioButtonPackages.TextAlign = ContentAlignment.MiddleCenter;
			// options
			Interface.LoadLogs();
			{
				int Tab = 0;
				string[] Args = System.Environment.GetCommandLineArgs();
				for (int i = 1; i < Args.Length; i++)
				{
					switch (Args[i].ToLowerInvariant())
					{
						case "/newgame": Tab = 0; break;
						case "/review": Tab = 1; break;
						case "/controls": Tab = 2; break;
						case "/options": Tab = 3; break;
						case "/packages": Tab = 4; break;
					}
				}
				switch (Tab)
				{
					case 1: radiobuttonReview.Checked = true; break;
					case 2: radiobuttonControls.Checked = true; break;
					case 3: radiobuttonOptions.Checked = true; break;
					case 4: radioButtonPackages.Checked = true; break;
					default: radiobuttonStart.Checked = true; break;
				}
			}
			// icons and images
			string MenuFolder = Program.FileSystem.GetDataFolder("Menu");
			Image ParentIcon = LoadImage(MenuFolder, "icon_parent.png");
			Image FolderIcon = LoadImage(MenuFolder, "icon_folder.png");
			Image DiskIcon = LoadImage(MenuFolder, "icon_disk.png");
			Image CsvRouteIcon = LoadImage(MenuFolder, "icon_route.png");
			Image RwRouteIcon = LoadImage(MenuFolder, "icon_route_outdatedversion.png");
			Image MechanikRouteIcon = LoadImage(MenuFolder, "icon_mechanik.png");
			Image TrainIcon = LoadImage(MenuFolder, "icon_train.png");
			Image KeyboardIcon = LoadImage(MenuFolder, "icon_keyboard.png");
			Image MouseIcon = LoadImage(MenuFolder, "icon_mouse.png");
			Image JoystickIcon = LoadImage(MenuFolder, "icon_joystick.png");
			Image GamepadIcon = LoadImage(MenuFolder, "icon_gamepad.png");
			JoystickImage = LoadImage(MenuFolder, "joystick.png");
			RailDriverImage = LoadImage(MenuFolder, "raildriver2.png");
			GamepadImage = LoadImage(MenuFolder, "gamepad.png");
			XboxImage = LoadImage(MenuFolder, "xbox.png");
			Image Logo = LoadImage(MenuFolder, "logo.png");
			if (Logo != null) pictureboxLogo.Image = Logo;
			string flagsFolder = Program.FileSystem.GetDataFolder("Flags");
			pictureboxRouteImage.ErrorImage = LoadImage(Program.FileSystem.GetDataFolder("Menu"),"error_route.png");
			pictureboxTrainImage.ErrorImage = LoadImage(Program.FileSystem.GetDataFolder("Menu"), "error_train.png");
			/* 
			 * TODO: Integrate into packages
			 */
	#pragma warning disable 0219
			string[] flags = new string[] { };
			try
			{
				flags = System.IO.Directory.GetFiles(flagsFolder);
			}
			catch (Exception)
			{
			}
	#pragma warning restore 0219
			// route selection
			listviewRouteFiles.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (ParentIcon != null) listviewRouteFiles.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewRouteFiles.SmallImageList.Images.Add("folder", FolderIcon);
			if (CsvRouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("csvroute", CsvRouteIcon);
			if (RwRouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("rwroute", RwRouteIcon);
			if (MechanikRouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("mechanik", MechanikRouteIcon);
			if (DiskIcon != null) listviewRouteFiles.SmallImageList.Images.Add("disk", DiskIcon);
			listviewRouteFiles.Columns.Clear();
			listviewRouteFiles.Columns.Add("");
			listviewRouteRecently.Items.Clear();
			listviewRouteRecently.Columns.Add("");
			listviewRouteRecently.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (CsvRouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("csvroute", CsvRouteIcon);
			if (RwRouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("rwroute", RwRouteIcon);
			if (MechanikRouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("mechanik", MechanikRouteIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++)
			{
				ListViewItem Item = listviewRouteRecently.Items.Add(System.IO.Path.GetFileName(Interface.CurrentOptions.RecentlyUsedRoutes[i]));
				string extension = System.IO.Path.GetExtension(Interface.CurrentOptions.RecentlyUsedRoutes[i]).ToLowerInvariant();
				switch (extension)
				{
					case ".dat":
						Item.ImageKey = @"mechanik";
						break;
					case ".rw":
						Item.ImageKey = @"rwroute";
						break;
					case ".csv":
						Item.ImageKey = @"csvroute";
						break;

				}

				Item.Tag = Interface.CurrentOptions.RecentlyUsedRoutes[i];
				string RoutePath = System.IO.Path.GetDirectoryName(Interface.CurrentOptions.RecentlyUsedRoutes[i]);
				if (textboxRouteFolder.Items.Count == 0 || !textboxRouteFolder.Items.Contains(RoutePath))
				{
					textboxRouteFolder.Items.Add(RoutePath);
				}

			}
			listviewRouteRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			// train selection
			listviewTrainFolders.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (ParentIcon != null) listviewTrainFolders.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewTrainFolders.SmallImageList.Images.Add("folder", FolderIcon);
			if (TrainIcon != null) listviewTrainFolders.SmallImageList.Images.Add("train", TrainIcon);
			if (DiskIcon != null) listviewTrainFolders.SmallImageList.Images.Add("disk", DiskIcon);
			listviewTrainFolders.Columns.Clear();
			listviewTrainFolders.Columns.Add("");
			listviewTrainRecently.Columns.Clear();
			listviewTrainRecently.Columns.Add("");
			listviewTrainRecently.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (TrainIcon != null) listviewTrainRecently.SmallImageList.Images.Add("train", TrainIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++)
			{
				ListViewItem Item = listviewTrainRecently.Items.Add(System.IO.Path.GetFileName(Interface.CurrentOptions.RecentlyUsedTrains[i]));
				Item.ImageKey = @"train";
				Item.Tag = Interface.CurrentOptions.RecentlyUsedTrains[i];
				string TrainPath = System.IO.Path.GetDirectoryName(Interface.CurrentOptions.RecentlyUsedTrains[i]);
				if (textboxTrainFolder.Items.Count == 0 || !textboxTrainFolder.Items.Contains(TrainPath))
				{
					textboxTrainFolder.Items.Add(TrainPath);
				}
			}
			listviewTrainRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			// text boxes
			if (Interface.CurrentOptions.RouteFolder.Length != 0 && System.IO.Directory.Exists(Interface.CurrentOptions.RouteFolder))
			{
				textboxRouteFolder.Text = Interface.CurrentOptions.RouteFolder;
			}
			else {
				textboxRouteFolder.Text = Program.FileSystem.InitialRouteFolder;
			}
			if (Interface.CurrentOptions.TrainFolder.Length != 0 && System.IO.Directory.Exists(Interface.CurrentOptions.TrainFolder))
			{
				textboxTrainFolder.Text = Interface.CurrentOptions.TrainFolder;
			}
			else {
				textboxTrainFolder.Text = Program.FileSystem.InitialTrainFolder;
			}
			// encodings
			{
				System.Text.EncodingInfo[] Info = System.Text.Encoding.GetEncodings();
				EncodingCodepages = new int[Info.Length + 1];
				string[] EncodingDescriptions = new string[Info.Length + 1];
				EncodingCodepages[0] = System.Text.Encoding.UTF8.CodePage;
				EncodingDescriptions[0] = $"{System.Text.Encoding.UTF8.EncodingName} - {System.Text.Encoding.UTF8.CodePage}";
				for (int i = 0; i < Info.Length; i++)
				{
					EncodingCodepages[i + 1] = Info[i].CodePage;
					try
					{
						EncodingDescriptions[i + 1] = Info[i].DisplayName + " - " + Info[i].CodePage.ToString(Culture);
					}
					catch
					{
						EncodingDescriptions[i + 1] = Info[i].Name;
					}
				}
				Array.Sort<string, int>(EncodingDescriptions, EncodingCodepages, 1, Info.Length);
				comboboxRouteEncoding.Items.Clear();
				comboboxTrainEncoding.Items.Clear();
				for (int i = 0; i < Info.Length + 1; i++)
				{
					comboboxRouteEncoding.Items.Add(EncodingDescriptions[i]);
					comboboxTrainEncoding.Items.Add(EncodingDescriptions[i]);
				}
			}
			// modes
			comboboxMode.Items.Clear();
			comboboxMode.Items.AddRange(new object[] { "", "", "" });
			comboboxMode.SelectedIndex = Interface.CurrentOptions.GameMode == GameMode.Arcade ? 0 : Interface.CurrentOptions.GameMode == GameMode.Expert ? 2 : 1;
			// review last game
			{
				if (Game.LogRouteName.Length == 0 | Game.LogTrainName.Length == 0)
				{
					radiobuttonReview.Enabled = false;
				}
				else {
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.CurrentValue / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * (double)Translations.RatingsCount);
					if (index >= Translations.RatingsCount) index = Translations.RatingsCount - 1;
					labelReviewRouteValue.Text = Game.LogRouteName;
					labelReviewTrainValue.Text = Game.LogTrainName;
					labelReviewDateValue.Text = Game.LogDateTime.ToString("yyyy-MM-dd", Culture);
					labelReviewTimeValue.Text = Game.LogDateTime.ToString("HH:mm:ss", Culture);
					switch (Interface.CurrentOptions.PreviousGameMode)
					{
						case GameMode.Arcade: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_arcade"); break;
						case GameMode.Normal: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_normal"); break;
						case GameMode.Expert: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_expert"); break;
						default: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_unknown"); break;
					}
					if (Game.CurrentScore.Maximum == 0)
					{
						labelRatingColor.BackColor = Color.Gray;
						labelRatingDescription.Text = Translations.GetInterfaceString("rating_unknown");
					}
					else {
						Color[] Colors = { Color.PaleVioletRed, Color.IndianRed, Color.Peru, Color.Goldenrod, Color.DarkKhaki, Color.YellowGreen, Color.MediumSeaGreen, Color.MediumAquamarine, Color.SkyBlue, Color.CornflowerBlue };
						if (index >= 0 & index < Colors.Length)
						{
							labelRatingColor.BackColor = Colors[index];
						}
						else {
							labelRatingColor.BackColor = Color.Gray;
						}
						labelRatingDescription.Text = Translations.GetInterfaceString("rating_" + index.ToString(Culture));
					}
					labelRatingAchievedValue.Text = Game.CurrentScore.CurrentValue.ToString(Culture);
					labelRatingMaximumValue.Text = Game.CurrentScore.Maximum.ToString(Culture);
					labelRatingRatioValue.Text = (100.0 * ratio).ToString("0.00", Culture) + @"%";
				}
			}
			comboboxBlackBoxFormat.Items.Clear();
			comboboxBlackBoxFormat.Items.AddRange(new object[] { "", "" });
			comboboxBlackBoxFormat.SelectedIndex = 1;
			if (Game.BlackBoxEntryCount == 0)
			{
				labelBlackBox.Enabled = false;
				labelBlackBoxFormat.Enabled = false;
				comboboxBlackBoxFormat.Enabled = false;
				buttonBlackBoxExport.Enabled = false;
			}
			// controls
			listviewControls.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (KeyboardIcon != null) listviewControls.SmallImageList.Images.Add("keyboard", KeyboardIcon);
			if (MouseIcon != null) listviewControls.SmallImageList.Images.Add("mouse", MouseIcon);
			if (JoystickIcon != null) listviewControls.SmallImageList.Images.Add("joystick", JoystickIcon);
			if (GamepadIcon != null) listviewControls.SmallImageList.Images.Add("gamepad", GamepadIcon);
			// options
			if (Interface.CurrentOptions.FullscreenMode)
			{
				radiobuttonFullscreen.Checked = true;
			}
			else {
				radiobuttonWindow.Checked = true;
			}
			comboboxVSync.Items.Clear();
			comboboxVSync.Items.Add("");
			comboboxVSync.Items.Add("");
			comboboxVSync.SelectedIndex = Interface.CurrentOptions.VerticalSynchronization ? 1 : 0;
			switch (Interface.CurrentOptions.UserInterfaceFolder)
			{
				case "Slim":
					trackBarHUDSize.Value = 0;
					break;
				case "Default":
					trackBarHUDSize.Value = 1;
					break;
				case "Large":
					trackBarHUDSize.Value = 2;
					break;
				default:
					trackBarHUDSize.Value = 1;
					break;
			}
			updownWindowWidth.Value = Interface.CurrentOptions.WindowWidth;
			updownWindowHeight.Value = Interface.CurrentOptions.WindowHeight;
			updownFullscreenWidth.Value = Interface.CurrentOptions.FullscreenWidth;
			updownFullscreenHeight.Value = Interface.CurrentOptions.FullscreenHeight;
			comboboxFullscreenBits.Items.Clear();
			comboboxFullscreenBits.Items.Add("16");
			comboboxFullscreenBits.Items.Add("32");
			comboboxFullscreenBits.SelectedIndex = Interface.CurrentOptions.FullscreenBits == 16 ? 0 : 1;
			comboboxInterpolation.Items.Clear();
			comboboxInterpolation.Items.AddRange(new object[] { "", "", "", "", "", "" });
			if ((int)Interface.CurrentOptions.Interpolation >= 0 & (int)Interface.CurrentOptions.Interpolation < comboboxInterpolation.Items.Count)
			{
				comboboxInterpolation.SelectedIndex = (int)Interface.CurrentOptions.Interpolation;
			}
			else {
				comboboxInterpolation.SelectedIndex = 3;
			}
			comboBoxTimeTableDisplayMode.Items.Clear();
			comboBoxTimeTableDisplayMode.Items.AddRange(new object[] { "", "", "", "" });
			if ((int)Interface.CurrentOptions.TimeTableStyle >= 0 & (int)Interface.CurrentOptions.TimeTableStyle < comboBoxTimeTableDisplayMode.Items.Count)
			{
				comboBoxTimeTableDisplayMode.SelectedIndex = (int)Interface.CurrentOptions.TimeTableStyle;
			}
			else
			{
				comboBoxTimeTableDisplayMode.SelectedIndex = 1;
			}
			if (Interface.CurrentOptions.AnisotropicFilteringMaximum <= 0)
			{
				labelAnisotropic.Enabled = false;
				updownAnisotropic.Enabled = false;
				updownAnisotropic.Minimum = 0;
				updownAnisotropic.Maximum = 0;
			}
			else {
				updownAnisotropic.Minimum = 1;
				updownAnisotropic.Maximum = Interface.CurrentOptions.AnisotropicFilteringMaximum;
				if (Interface.CurrentOptions.AnisotropicFilteringLevel >= updownAnisotropic.Minimum & Interface.CurrentOptions.AnisotropicFilteringLevel <= updownAnisotropic.Maximum)
				{
					updownAnisotropic.Value = Interface.CurrentOptions.AnisotropicFilteringLevel;
				}
				else {
					updownAnisotropic.Value = updownAnisotropic.Minimum;
				}
			}
			updownAntiAliasing.Value = Interface.CurrentOptions.AntiAliasingLevel;
			updownDistance.Value = Interface.CurrentOptions.ViewingDistance;
			comboboxMotionBlur.Items.Clear();
			comboboxMotionBlur.Items.AddRange(new object[] { "", "", "", "" });
			comboboxMotionBlur.SelectedIndex = (int)Interface.CurrentOptions.MotionBlur;
			trackbarTransparency.Value = (int)Interface.CurrentOptions.TransparencyMode;
			updownTimeAccelerationFactor.Value = Interface.CurrentOptions.TimeAccelerationFactor > updownTimeAccelerationFactor.Maximum ? updownTimeAccelerationFactor.Maximum : Interface.CurrentOptions.TimeAccelerationFactor;
			checkboxToppling.Checked = Interface.CurrentOptions.Toppling;
			checkboxCollisions.Checked = Interface.CurrentOptions.Collisions;
			checkboxDerailments.Checked = Interface.CurrentOptions.Derailments;
			checkBoxLoadInAdvance.Checked = Interface.CurrentOptions.LoadInAdvance;
			checkBoxUnloadTextures.Checked = Interface.CurrentOptions.UnloadUnusedTextures;
			checkBoxIsUseNewRenderer.Checked = Interface.CurrentOptions.IsUseNewRenderer;
			checkboxBlackBox.Checked = Interface.CurrentOptions.BlackBox;
			checkBoxLoadingSway.Checked = Interface.CurrentOptions.LoadingSway;
			checkBoxTransparencyFix.Checked = Interface.CurrentOptions.OldTransparencyMode;
			checkBoxHacks.Checked = Interface.CurrentOptions.EnableBveTsHacks;
			checkboxJoysticksUsed.Checked = Interface.CurrentOptions.UseJoysticks;
			checkBoxEBAxis.Checked = Interface.CurrentOptions.AllowAxisEB;
			{
				double a = (double)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum) * Interface.CurrentOptions.JoystickAxisThreshold + (double)trackbarJoystickAxisThreshold.Minimum;
				int b = (int)Math.Round(a);
				if (b < trackbarJoystickAxisThreshold.Minimum) b = trackbarJoystickAxisThreshold.Minimum;
				if (b > trackbarJoystickAxisThreshold.Maximum) b = trackbarJoystickAxisThreshold.Maximum;
				trackbarJoystickAxisThreshold.Value = b;
			}
			updownSoundNumber.Value = Interface.CurrentOptions.SoundNumber;
			checkboxWarningMessages.Checked = Interface.CurrentOptions.ShowWarningMessages;
			checkboxErrorMessages.Checked = Interface.CurrentOptions.ShowErrorMessages;
			comboBoxCompressionFormat.SelectedIndex = (int)Interface.CurrentOptions.packageCompressionType;
			comboBoxRailDriverUnits.SelectedIndex = Interface.CurrentOptions.RailDriverMPH ? 0 : 1;
			checkBoxEnableKiosk.Checked = Interface.CurrentOptions.KioskMode;
			numericUpDownKioskTimeout.Value = (decimal)Interface.CurrentOptions.KioskModeTimer;
			checkBoxAccessibility.Checked = Interface.CurrentOptions.Accessibility;
			ListInputDevicePlugins();
			if (Program.CurrentHost.MonoRuntime)
			{
				//HACK: If we're running on Mono, manually select the tabpage at start. This avoids the 'grey tab' bug
				tabcontrolRouteSelection.SelectedTab = tabpageRouteBrowse;
				tabcontrolTrainSelection.SelectedTab = tabpageTrainBrowse;
			}
			// lists
			ShowScoreLog(checkboxScorePenalties.Checked);
			// result
			Result.Start = false;

			InitializePreviewRouteThread();
			Manipulation.ProgressChanged += OnWorkerProgressChanged;
			Manipulation.ProblemReport += OnWorkerReportsProblem;
			updownTimeAccelerationFactor.ValueChanged += updownTimeAccelerationFactor_ValueChanged;
			comboBoxXparser.SelectedIndex = (int)Interface.CurrentOptions.CurrentXParser;
			comboBoxObjparser.SelectedIndex = (int)Interface.CurrentOptions.CurrentObjParser;
			//Load languages last to ensure that everything is populated
			Translations.CurrentLanguageCode = Interface.CurrentOptions.LanguageCode;
			Translations.ListLanguages(comboboxLanguages);
			Cursors.ListCursors(comboboxCursor);
			checkBoxPanel2Extended.Checked = Interface.CurrentOptions.Panel2ExtendedMode;
			LoadCompatibilitySignalSets();
		}

		

		/// <summary>This function is called to change the display language of the program</summary>
		private void ApplyLanguage()
		{
			Translations.SetInGameLanguage(Translations.CurrentLanguageCode);
			/*
			 * Localisation for strings in main panel
			 */
			radiobuttonStart.Text = Translations.GetInterfaceString("panel_start");
			radiobuttonReview.Text = Translations.GetInterfaceString("panel_review");
			radiobuttonControls.Text = Translations.GetInterfaceString("panel_controls");
			radiobuttonOptions.Text = Translations.GetInterfaceString("panel_options");
			linkHomepage.Text = Translations.GetInterfaceString("panel_homepage");
			buttonClose.Text = Translations.GetInterfaceString("panel_close");
			radioButtonPackages.Text = Translations.GetInterfaceString("panel_packages");
			linkLabelCheckUpdates.Text = Translations.GetInterfaceString("panel_updates");
			linkLabelReportBug.Text = Translations.GetInterfaceString("panel_reportbug");
			aboutLabel.Text = Translations.GetInterfaceString("panel_about");
			/*
			 * Localisation for strings in the options pane
			 */
			labelOptionsTitle.Text = Translations.GetInterfaceString("options_title");
			//Basic display mode settings
			groupboxDisplayMode.Text = Translations.GetInterfaceString("options_display_mode");
			radiobuttonWindow.Text = Translations.GetInterfaceString("options_display_mode_window");
			radiobuttonFullscreen.Text = Translations.GetInterfaceString("options_display_mode_fullscreen");
			labelVSync.Text = Translations.GetInterfaceString("options_display_vsync");
			comboboxVSync.Items[0] = Translations.GetInterfaceString("options_display_vsync_off");
			comboboxVSync.Items[1] = Translations.GetInterfaceString("options_display_vsync_on");
			labelHUDScale.Text = Translations.GetInterfaceString("options_hud_size");
			labelHUDSmall.Text = Translations.GetInterfaceString("options_hud_size_small");
			labelHUDNormal.Text = Translations.GetInterfaceString("options_hud_size_normal");
			labelHUDLarge.Text = Translations.GetInterfaceString("options_hud_size_large");
			//Windowed Mode
			groupboxWindow.Text = Translations.GetInterfaceString("options_display_window");
			labelWindowWidth.Text = Translations.GetInterfaceString("options_display_window_width");
			labelWindowHeight.Text = Translations.GetInterfaceString("options_display_window_height");
			//Fullscreen
			groupboxFullscreen.Text = Translations.GetInterfaceString("options_display_fullscreen");
			labelFullscreenWidth.Text = Translations.GetInterfaceString("options_display_fullscreen_width");
			labelFullscreenHeight.Text = Translations.GetInterfaceString("options_display_fullscreen_height");
			labelFullscreenBits.Text = Translations.GetInterfaceString("options_display_fullscreen_bits");
			//Interpolation, AA and AF
			groupboxInterpolation.Text = Translations.GetInterfaceString("options_quality_interpolation");
			labelInterpolation.Text = Translations.GetInterfaceString("options_quality_interpolation_mode");
			comboboxInterpolation.Items[0] = Translations.GetInterfaceString("options_quality_interpolation_mode_nearest");
			comboboxInterpolation.Items[1] = Translations.GetInterfaceString("options_quality_interpolation_mode_bilinear");
			comboboxInterpolation.Items[2] = Translations.GetInterfaceString("options_quality_interpolation_mode_nearestmipmap");
			comboboxInterpolation.Items[3] = Translations.GetInterfaceString("options_quality_interpolation_mode_bilinearmipmap");
			comboboxInterpolation.Items[4] = Translations.GetInterfaceString("options_quality_interpolation_mode_trilinearmipmap");
			comboboxInterpolation.Items[5] = Translations.GetInterfaceString("options_quality_interpolation_mode_anisotropic");
			labelAnisotropic.Text = Translations.GetInterfaceString("options_quality_interpolation_anisotropic_level");
			labelAntiAliasing.Text = Translations.GetInterfaceString("options_quality_interpolation_antialiasing_level");
			labelTransparency.Text = Translations.GetInterfaceString("options_quality_interpolation_transparency");
			labelTransparencyPerformance.Text = Translations.GetInterfaceString("options_quality_interpolation_transparency_sharp");
			labelTransparencyQuality.Text = Translations.GetInterfaceString("options_quality_interpolation_transparency_smooth");
			groupboxDistance.Text = Translations.GetInterfaceString("options_quality_distance");
			//Viewing distance and motion blur
			labelDistance.Text = Translations.GetInterfaceString("options_quality_distance_viewingdistance");
			labelDistanceUnit.Text = Translations.GetInterfaceString("options_quality_distance_viewingdistance_meters");
			labelMotionBlur.Text = Translations.GetInterfaceString("options_quality_distance_motionblur");
			comboboxMotionBlur.Items[0] = Translations.GetInterfaceString("options_quality_distance_motionblur_none");
			comboboxMotionBlur.Items[1] = Translations.GetInterfaceString("options_quality_distance_motionblur_low");
			comboboxMotionBlur.Items[2] = Translations.GetInterfaceString("options_quality_distance_motionblur_medium");
			comboboxMotionBlur.Items[3] = Translations.GetInterfaceString("options_quality_distance_motionblur_high");
			labelMotionBlur.Text = Translations.GetInterfaceString("options_quality_distance_motionblur");
			//Simulation
			groupboxSimulation.Text = Translations.GetInterfaceString("options_misc_simulation");
			checkboxToppling.Text = Translations.GetInterfaceString("options_misc_simulation_toppling");
			checkboxCollisions.Text = Translations.GetInterfaceString("options_misc_simulation_collisions");
			checkboxDerailments.Text = Translations.GetInterfaceString("options_misc_simulation_derailments");
			checkboxBlackBox.Text = Translations.GetInterfaceString("options_misc_simulation_blackbox");
			checkBoxLoadingSway.Text = Translations.GetInterfaceString("options_misc_simulation_loadingsway");
			//Controls
			groupboxControls.Text = Translations.GetInterfaceString("options_misc_controls");
			checkboxJoysticksUsed.Text = Translations.GetInterfaceString("options_misc_controls_joysticks");
			checkBoxEBAxis.Text = Translations.GetInterfaceString("options_misc_controls_ebaxis");
			labelJoystickAxisThreshold.Text = Translations.GetInterfaceString("options_misc_controls_threshold");
			//Sound
			groupboxSound.Text = Translations.GetInterfaceString("options_misc_sound");
			labelSoundNumber.Text = Translations.GetInterfaceString("options_misc_sound_number");
			//Verbosity
			groupboxVerbosity.Text = Translations.GetInterfaceString("options_verbosity");
			checkboxWarningMessages.Text = Translations.GetInterfaceString("options_verbosity_warningmessages");
			checkboxErrorMessages.Text = Translations.GetInterfaceString("options_verbosity_errormessages");
			//Advanced Options
			groupBoxAdvancedOptions.Text = Translations.GetInterfaceString("options_advanced");
			checkBoxLoadInAdvance.Text = Translations.GetInterfaceString("options_advanced_load_advance");
			checkBoxUnloadTextures.Text = Translations.GetInterfaceString("options_advanced_unload_textures");
			checkBoxIsUseNewRenderer.Text = Translations.GetInterfaceString("options_advanced_is_use_new_renderer");
			labelTimeAcceleration.Text = Translations.GetInterfaceString("options_advanced_timefactor");
			//Other Options
			groupBoxOther.Text = Translations.GetInterfaceString("options_other");
			labelTimeTableDisplayMode.Text = Translations.GetInterfaceString("options_other_timetable_mode");
			comboBoxTimeTableDisplayMode.Items[0] = Translations.GetInterfaceString("options_other_timetable_mode_none");
			comboBoxTimeTableDisplayMode.Items[1] = Translations.GetInterfaceString("options_other_timetable_mode_default");
			comboBoxTimeTableDisplayMode.Items[2] = Translations.GetInterfaceString("options_other_timetable_mode_autogenerated");
			comboBoxTimeTableDisplayMode.Items[3] = Translations.GetInterfaceString("options_other_timetable_mode_prefercustom");
			//Options Page
			buttonOptionsPrevious.Text = Translations.GetInterfaceString("options_page_previous");
			buttonOptionsNext.Text = Translations.GetInterfaceString("options_page_next");
			checkBoxPanel2Extended.Text = Translations.GetInterfaceString("options_panel2_extended");
			labelXparser.Text = Translations.GetInterfaceString("options_xobject_parser");
			labelObjparser.Text = Translations.GetInterfaceString("options_objobject_parser");
			/*
			 * Options Page 2
			 */
			//Package directories
			groupBoxPackageOptions.Text = Translations.GetInterfaceString("panel_packages");
			buttonSetRouteDirectory.Text = Translations.GetInterfaceString("options_package_choose");
			buttonTrainInstallationDirectory.Text = Translations.GetInterfaceString("options_package_choose");
			buttonOtherDirectory.Text = Translations.GetInterfaceString("options_package_choose");
			textBoxRouteDirectory.Text = Program.FileSystem.RouteInstallationDirectory;
			textBoxTrainDirectory.Text = Program.FileSystem.TrainInstallationDirectory;
			textBoxOtherDirectory.Text = Program.FileSystem.OtherInstallationDirectory;
			labelRouteInstallDirectory.Text = Translations.GetInterfaceString("options_package_route_directory");
			labelTrainInstallDirectory.Text = Translations.GetInterfaceString("options_package_train_directory");
			labelOtherInstallDirectory.Text = Translations.GetInterfaceString("options_package_other_directory");
			labelPackageCompression.Text = Translations.GetInterfaceString("options_package_compression");
			groupBoxKioskMode.Text = Translations.GetInterfaceString("options_kiosk_mode");
			checkBoxEnableKiosk.Text = Translations.GetInterfaceString("options_kiosk_mode_enable");
			labelKioskTimeout.Text = Translations.GetInterfaceString("options_kiosk_mode_timer");
			labelRailDriverSpeedUnits.Text = Translations.GetInterfaceString("raildriver_speedunits");
			comboBoxRailDriverUnits.Items[0] = Translations.GetInterfaceString("raildriver_milesperhour");
			comboBoxRailDriverUnits.Items[1] = Translations.GetInterfaceString("raildriver_kilometersperhour");
			labelRailDriverCalibration.Text = Translations.GetInterfaceString("raildriver_setcalibration");
			buttonRailDriverCalibration.Text = Translations.GetInterfaceString("raildriver_launch");
			checkBoxTransparencyFix.Text = Translations.GetInterfaceString("options_transparencyfix");
			checkBoxHacks.Text = Translations.GetInterfaceString("options_hacks_enable");
			groupBoxInputDevice.Text = Translations.GetInterfaceString("options_input_device_plugin");
			labelInputDevice.Text = Translations.GetInterfaceString("options_input_device_plugin_warning");
			listviewInputDevice.Columns[0].Text = Translations.GetInterfaceString("options_input_device_plugin_name");
			listviewInputDevice.Columns[1].Text = Translations.GetInterfaceString("options_input_device_plugin_status");
			listviewInputDevice.Columns[2].Text = Translations.GetInterfaceString("options_input_device_plugin_version");
			listviewInputDevice.Columns[3].Text = Translations.GetInterfaceString("options_input_device_plugin_provider");
			listviewInputDevice.Columns[4].Text = Translations.GetInterfaceString("options_input_device_plugin_file_name");
			{
				listviewInputDevice.Items.Clear();
				ListViewItem[] Items = new ListViewItem[InputDevicePlugin.AvailablePluginInfos.Count];
				for (int i = 0; i < Items.Length; i++)
				{
					Items[i] = new ListViewItem(new[] { "", "", "", "", "" });
					UpdateInputDeviceListViewItem(Items[i], i, false);
				}
				listviewInputDevice.Items.AddRange(Items);
				listviewInputDevice.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
			}
			checkBoxInputDeviceEnable.Text = Translations.GetInterfaceString("options_input_device_plugin_switch");
			buttonInputDeviceConfig.Text = Translations.GetInterfaceString("options_input_device_plugin_config");
			/*
			 * Localisation for strings in the game start pane
			 */
			labelStartTitle.Text = Translations.GetInterfaceString("start_title");
			labelRoute.Text = Translations.GetInterfaceString("start_route");
			groupboxRouteSelection.Text = Translations.GetInterfaceString("start_route_selection");
			tabpageRouteBrowse.Text = Translations.GetInterfaceString("start_route_browse");
			tabpageRouteRecently.Text = Translations.GetInterfaceString("start_route_recently");
			groupboxRouteDetails.Text = Translations.GetInterfaceString("start_route_details");
			tabpageRouteDescription.Text = Translations.GetInterfaceString("start_route_description");
			tabpageRouteMap.Text = Translations.GetInterfaceString("start_route_map");
			tabpageRouteGradient.Text = Translations.GetInterfaceString("start_route_gradient");
			tabpageRouteSettings.Text = Translations.GetInterfaceString("start_route_settings");
			labelRouteEncoding.Text = Translations.GetInterfaceString("start_route_settings_encoding");
			comboboxRouteEncoding.Items[0] = Translations.GetInterfaceString("(UTF-8)");
			labelRouteEncodingPreview.Text = Translations.GetInterfaceString("start_route_settings_encoding_preview");
			labelTrain.Text = Translations.GetInterfaceString("start_train");
			groupboxTrainSelection.Text = Translations.GetInterfaceString("start_train_selection");
			tabpageTrainBrowse.Text = Translations.GetInterfaceString("start_train_browse");
			tabpageTrainRecently.Text = Translations.GetInterfaceString("start_train_recently");
			tabpageTrainDefault.Text = Translations.GetInterfaceString("start_train_default");
			checkboxTrainDefault.Text = Translations.GetInterfaceString("start_train_usedefault");
			groupboxTrainDetails.Text = Translations.GetInterfaceString("start_train_details");
			tabpageTrainDescription.Text = Translations.GetInterfaceString("start_train_description");
			tabpageTrainSettings.Text = Translations.GetInterfaceString("start_train_settings");
			labelTrainEncoding.Text = Translations.GetInterfaceString("start_train_settings_encoding");
			labelReverseConsist.Text = Translations.GetInterfaceString("start_train_settings_reverseconsist");
			comboboxTrainEncoding.Items[0] = Translations.GetInterfaceString("(UTF-8)");
			labelTrainEncodingPreview.Text = Translations.GetInterfaceString("start_train_settings_encoding_preview");
			labelStart.Text = Translations.GetInterfaceString("start_start");
			labelMode.Text = Translations.GetInterfaceString("start_start_mode");
			buttonStart.Text = Translations.GetInterfaceString("start_start_start");
			comboboxMode.Items[0] = Translations.GetInterfaceString("mode_arcade");
			comboboxMode.Items[1] = Translations.GetInterfaceString("mode_normal");
			comboboxMode.Items[2] = Translations.GetInterfaceString("mode_expert");
			labelCompatibilitySignalSet.Text = Translations.GetInterfaceString("options_compatibility_signals");
			/*
			 * Localisation for strings in the game review pane
			 */
			labelReviewTitle.Text = Translations.GetInterfaceString("review_title");
			labelConditions.Text = @" " + Translations.GetInterfaceString("review_conditions");
			groupboxReviewRoute.Text = Translations.GetInterfaceString("review_conditions_route");
			labelReviewRouteCaption.Text = Translations.GetInterfaceString("review_conditions_route_file");
			groupboxReviewTrain.Text = Translations.GetInterfaceString("review_conditions_train");
			labelReviewTrainCaption.Text = Translations.GetInterfaceString("review_conditions_train_folder");
			groupboxReviewDateTime.Text = Translations.GetInterfaceString("review_conditions_datetime");
			labelReviewDateCaption.Text = Translations.GetInterfaceString("review_conditions_datetime_date");
			labelReviewTimeCaption.Text = Translations.GetInterfaceString("review_conditions_datetime_time");
			labelScore.Text = @" " + Translations.GetInterfaceString("review_score");
			groupboxRating.Text = Translations.GetInterfaceString("review_score_rating");
			labelRatingModeCaption.Text = Translations.GetInterfaceString("review_score_rating_mode");
			switch (Interface.CurrentOptions.PreviousGameMode)
			{
				case GameMode.Arcade: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_arcade"); break;
				case GameMode.Normal: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_normal"); break;
				case GameMode.Expert: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_expert"); break;
				default: labelRatingModeValue.Text = Translations.GetInterfaceString("mode_unkown"); break;
			}
			{
				double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.CurrentValue / (double)Game.CurrentScore.Maximum;
				if (ratio < 0.0) ratio = 0.0;
				if (ratio > 1.0) ratio = 1.0;
				int index = (int)Math.Floor(ratio * (double)Translations.RatingsCount);
				if (index >= Translations.RatingsCount) index = Translations.RatingsCount - 1;
				if (Game.CurrentScore.Maximum == 0)
				{
					labelRatingDescription.Text = Translations.GetInterfaceString("rating_unknown");
				}
				else {
					labelRatingDescription.Text = Translations.GetInterfaceString("rating_" + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
				}
			}
			labelRatingAchievedCaption.Text = Translations.GetInterfaceString("review_score_rating_achieved");
			labelRatingMaximumCaption.Text = Translations.GetInterfaceString("review_score_rating_maximum");
			labelRatingRatioCaption.Text = Translations.GetInterfaceString("review_score_rating_ratio");
			groupboxScore.Text = Translations.GetInterfaceString("review_score_log");
			listviewScore.Columns[0].Text = Translations.GetInterfaceString("review_score_log_list_time");
			listviewScore.Columns[1].Text = Translations.GetInterfaceString("review_score_log_list_position");
			listviewScore.Columns[2].Text = Translations.GetInterfaceString("review_score_log_list_value");
			listviewScore.Columns[3].Text = Translations.GetInterfaceString("review_score_log_list_cumulative");
			listviewScore.Columns[4].Text = Translations.GetInterfaceString("review_score_log_list_reason");
			ShowScoreLog(checkboxScorePenalties.Checked);
			checkboxScorePenalties.Text = Translations.GetInterfaceString("review_score_log_penalties");
			buttonScoreExport.Text = Translations.GetInterfaceString("review_score_log_export");
			labelBlackBox.Text = @" " + Translations.GetInterfaceString("review_blackbox");
			labelBlackBoxFormat.Text = Translations.GetInterfaceString("review_blackbox_format");
			comboboxBlackBoxFormat.Items[0] = Translations.GetInterfaceString("review_blackbox_format_csv");
			comboboxBlackBoxFormat.Items[1] = Translations.GetInterfaceString("review_blackbox_format_text");
			buttonBlackBoxExport.Text = Translations.GetInterfaceString("review_blackbox_export");
			/*
			 * Localisation for strings related to controls (Keyboard etc.)
			 */
			for (int i = 0; i < listviewControls.SelectedItems.Count; i++)
			{
				listviewControls.SelectedItems[i].Selected = false;
			}
			labelControlsTitle.Text = Translations.GetInterfaceString("controls_title");
			listviewControls.Columns[0].Text = Translations.GetInterfaceString("controls_list_command");
			listviewControls.Columns[1].Text = Translations.GetInterfaceString("controls_list_type");
			listviewControls.Columns[2].Text = Translations.GetInterfaceString("controls_list_description");
			listviewControls.Columns[3].Text = Translations.GetInterfaceString("controls_list_assignment");
			listviewControls.Columns[4].Text = Translations.GetInterfaceString("controls_list_option");
			buttonControlAdd.Text = Translations.GetInterfaceString("controls_add");
			buttonControlRemove.Text = Translations.GetInterfaceString("controls_remove");
			buttonControlsImport.Text = Translations.GetInterfaceString("controls_import");
			buttonControlsExport.Text = Translations.GetInterfaceString("controls_export");
			buttonControlReset.Text = Translations.GetInterfaceString("controls_reset");
			buttonControlUp.Text = Translations.GetInterfaceString("controls_up");
			buttonControlDown.Text = Translations.GetInterfaceString("controls_down");
			groupboxControl.Text = Translations.GetInterfaceString("controls_selection");
			labelCommand.Text = Translations.GetInterfaceString("controls_selection_command");
			labelCommandOption.Text = Translations.GetInterfaceString("controls_selection_command_option");
			radiobuttonKeyboard.Text = Translations.GetInterfaceString("controls_selection_keyboard");
			labelKeyboardKey.Text = Translations.GetInterfaceString("controls_selection_keyboard_key");
			labelKeyboardModifier.Text = Translations.GetInterfaceString("controls_selection_keyboard_modifiers");
			//Load text for SHIFT modifier
			checkboxKeyboardShift.Text = Translations.GetInterfaceString("controls_selection_keyboard_modifiers_shift");
			//Shift CTRL
			checkboxKeyboardCtrl.Location = new Point(checkboxKeyboardShift.Location.X + (checkboxKeyboardShift.Text.Length + 5) * 5, checkboxKeyboardCtrl.Location.Y);
			//Load text for CTRL modifier
			checkboxKeyboardCtrl.Text = Translations.GetInterfaceString("controls_selection_keyboard_modifiers_ctrl");
			//Shift ALT to suit
			checkboxKeyboardAlt.Location = new Point(checkboxKeyboardCtrl.Location.X + (checkboxKeyboardCtrl.Text.Length + 5) * 5, checkboxKeyboardAlt.Location.Y);
			
			checkboxKeyboardAlt.Text = Translations.GetInterfaceString("controls_selection_keyboard_modifiers_alt");
			radiobuttonJoystick.Text = Translations.GetInterfaceString("controls_selection_joystick");
			labelJoystickAssignmentCaption.Text = Translations.GetInterfaceString("controls_selection_joystick_assignment");
			textboxJoystickGrab.Text = Translations.GetInterfaceString("controls_selection_keyboard_assignment_grab");
			groupboxJoysticks.Text = Translations.GetInterfaceString("controls_attached");
			{
				listviewControls.Items.Clear();
				comboboxCommand.Items.Clear();
				for (int i = 0; i < Translations.CommandInfos.Length; i++)
				{
					comboboxCommand.Items.Add(Translations.CommandInfos[i].Name + " - " + Translations.CommandInfos[i].Description);
				}
				comboboxKeyboardKey.Items.Clear();
				for (int i = 0; i < Translations.TranslatedKeys.Length; i++)
				{
					comboboxKeyboardKey.Items.Add(Translations.TranslatedKeys[i]);
				}

				ListViewItem[] Items = new ListViewItem[Interface.CurrentControls.Length];
				for (int i = 0; i < Interface.CurrentControls.Length; i++)
				{
					Items[i] = new ListViewItem(new[] { "", "", "", "", "" });
					UpdateControlListElement(Items[i], i, false);
				}
				listviewControls.Items.AddRange(Items);
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
			/*
			 * Localisation for strings in package management display
			 * 
			 */
			//Navigation buttons
			buttonBack.Text = Translations.GetInterfaceString("packages_button_back");
			buttonCreatePackage.Text = Translations.GetInterfaceString("packages_button_create");
			buttonBack2.Text = Translations.GetInterfaceString("packages_button_back");
			buttonNext.Text = Translations.GetInterfaceString("packages_button_next");
			buttonCancel.Text = Translations.GetInterfaceString("packages_button_cancel");
			buttonProceedAnyway1.Text = Translations.GetInterfaceString("packages_button_install");
			buttonCancel2.Text = Translations.GetInterfaceString("packages_button_cancel");
			buttonCreateProceed.Text = Translations.GetInterfaceString("packages_button_next");
			buttonAbort.Text = Translations.GetInterfaceString("packages_button_abort");
			buttonProceedAnyway.Text = Translations.GetInterfaceString("packages_button_ignore");
			//Main display tab
			labelPackagesTitle.Text = Translations.GetInterfaceString("packages_title");
			labelInstalledPackages.Text = Translations.GetInterfaceString("packages_list");
			labelPackageListType.Text = Translations.GetInterfaceString("packages_list_type");
			buttonInstallPackage.Text = Translations.GetInterfaceString("packages_install_button");
			buttonUninstallPackage.Text = Translations.GetInterfaceString("packages_uninstall_button");
			createPackageButton.Text = Translations.GetInterfaceString("packages_creation_button");
			comboBoxPackageType.Items[0] = Translations.GetInterfaceString("packages_type_route");
			comboBoxPackageType.Items[1] = Translations.GetInterfaceString("packages_type_train");
			comboBoxPackageType.Items[2] = Translations.GetInterfaceString("packages_type_other");
			routeName.HeaderText = Translations.GetInterfaceString("packages_list_name");
			routeVersion.HeaderText = Translations.GetInterfaceString("packages_list_version");
			routeAuthor.HeaderText = Translations.GetInterfaceString("packages_list_author");
			routeWebsite.HeaderText = Translations.GetInterfaceString("packages_list_website");
			//Creation tab 1
			labelPackageCreationHeader.Text = Translations.GetInterfaceString("packages_creation_header");
			SaveFileNameButton.Text = Translations.GetInterfaceString("packages_creation_saveas_button");
			labelSaveAs.Text = Translations.GetInterfaceString("packages_creation_saveas_label");
			labelDependanciesNextStep.Text = Translations.GetInterfaceString("packages_creation_dependancies_nextstep");
			newPackageClearSelectionButton.Text = Translations.GetInterfaceString("packages_creation_clearselection");
			addPackageItemsButton.Text = Translations.GetInterfaceString("packages_creation_additems");
			labelSelectFiles.Text = Translations.GetInterfaceString("packages_creation_selecteditems");
			labelNewGUID.Text = Translations.GetInterfaceString("packages_creation_new_guid");
			dataGridViewTextBoxColumn21.HeaderText = Translations.GetInterfaceString("packages_list_name");
			dataGridViewTextBoxColumn22.HeaderText = Translations.GetInterfaceString("packages_list_version");
			dataGridViewTextBoxColumn23.HeaderText = Translations.GetInterfaceString("packages_list_author");
			dataGridViewTextBoxColumn24.HeaderText = Translations.GetInterfaceString("packages_list_website");
			//Replace package panel of creation tab
			replacePackageButton.Text = Translations.GetInterfaceString("packages_replace_select");
			packageToReplaceLabel.Text = Translations.GetInterfaceString("packages_replace_choose");
			//New package panel
			radioButtonQ2Other.Text = Translations.GetInterfaceString("packages_type_other");
			radioButtonQ2Route.Text = Translations.GetInterfaceString("packages_type_route");
			radioButtonQ2Train.Text = Translations.GetInterfaceString("packages_type_train");
			labelPackageType.Text = Translations.GetInterfaceString("packages_type_select");
			labelReplacePackage.Text = Translations.GetInterfaceString("packages_creation_replace");
			radioButtonQ1Yes.Text = Translations.GetInterfaceString("packages_creation_yes");
			radioButtonQ1No.Text = Translations.GetInterfaceString("packages_creation_no");
			//Please wait tab
			labelPleaseWait.Text = Translations.GetInterfaceString("packages_processing");
			labelProgressFile.Text = Translations.GetInterfaceString("packages_unknown_file");
			//Missing dependancies tab
			/*
			 * NOTE: THIS TAB IS MULTI-FUNCTIONAL, AND MAY BE UPDATED AT RUNTIME
			 * REMEMBER TO RESET AFTERWARDS
			 * 
			 */
			labelMissingDependanciesText1.Text = Translations.GetInterfaceString("packages_install_dependancies_unmet");
			labelMissingDependanciesText2.Text = Translations.GetInterfaceString("packages_shownlist");
			labelDependancyErrorHeader.Text = Translations.GetInterfaceString("packages_install_dependancies_unmet_header");
			//Install tab
			/*
			 * NOTE: THIS TAB IS MULTI-FUNCTIONAL, AND THE HEADER MAY BE UPDATED AT RUNTIME
			 * REMEMBER TO RESET AFTERWARDS
			 * 
			 */
			labelPackageName.Text = Translations.GetInterfaceString("packages_install_name");
			labelPackageAuthor.Text = Translations.GetInterfaceString("packages_install_author");
			labelPackageVersion.Text = Translations.GetInterfaceString("packages_install_version");
			labelPackageWebsite.Text = Translations.GetInterfaceString("packages_install_website");
			labelPackageDescription.Text = Translations.GetInterfaceString("packages_install_description");

			//Add dependancies panel
			labelDependanciesHeader.Text = Translations.GetInterfaceString("packages_creation_dependancies");
			labelInstalledDependancies.Text = Translations.GetInterfaceString("packages_list");
			labelSelectedDependencies.Text = Translations.GetInterfaceString("packages_selected");
			labelDependancyType.Text = Translations.GetInterfaceString("packages_list_type");
			comboBoxDependancyType.Items[0] = Translations.GetInterfaceString("packages_type_route");
			comboBoxDependancyType.Items[1] = Translations.GetInterfaceString("packages_type_train");
			comboBoxDependancyType.Items[2] = Translations.GetInterfaceString("packages_type_other");
			buttonDepends.Text = Translations.GetInterfaceString("packages_creation_dependancies_add");
			buttonReccomends.Text = Translations.GetInterfaceString("packages_creation_reccommends_add");
			dataGridViewTextBoxColumn13.HeaderText = Translations.GetInterfaceString("packages_list_name");
			dataGridViewTextBoxColumn14.HeaderText = Translations.GetInterfaceString("packages_list_version");
			dataGridViewTextBoxColumn15.HeaderText = Translations.GetInterfaceString("packages_list_author");
			dataGridViewTextBoxColumn16.HeaderText = Translations.GetInterfaceString("packages_list_website");
			dataGridViewTextBoxColumn1.HeaderText = Translations.GetInterfaceString("packages_list_name");
			dataGridViewTextBoxColumn2.HeaderText = Translations.GetInterfaceString("packages_list_minimum");
			dataGridViewTextBoxColumn3.HeaderText = Translations.GetInterfaceString("packages_list_maximum");
			dataGridViewTextBoxColumn4.HeaderText = Translations.GetInterfaceString("packages_list_packagetype");
			buttonRemove.Text = Translations.GetInterfaceString("packages_creation_dependancies_remove");
			website.HeaderText = Translations.GetInterfaceString("packages_list_website");
			//Version Error panel
			labelBrokenDependancies.Text = Translations.GetInterfaceString("packages_install_dependancies_broken");
			labelNewVersion.Text = Translations.GetInterfaceString("packages_version_new");
			labelCurrentVersion.Text = Translations.GetInterfaceString("packages_version_current");
			dataGridViewTextBoxColumn5.HeaderText = Translations.GetInterfaceString("packages_list_name");
			dataGridViewTextBoxColumn6.HeaderText = Translations.GetInterfaceString("packages_list_maximum");
			dataGridViewTextBoxColumn7.HeaderText = Translations.GetInterfaceString("packages_list_minimum");
			dataGridViewTextBoxColumn8.HeaderText = Translations.GetInterfaceString("packages_list_author");
			website.HeaderText = Translations.GetInterfaceString("packages_list_website");
			groupBoxVersionErrorAction.Text = Translations.GetInterfaceString("packages_error_action");
			radioButtonOverwrite.Text = Translations.GetInterfaceString("packages_error_overwrite");
			radioButtonReplace.Text = Translations.GetInterfaceString("packages_error_replace");
			// *** labelVersionError.Text is set dynamically at runtime ***
			labelVersionErrorHeader.Text = Translations.GetInterfaceString("packages_install_version_error");
			dataGridViewTextBoxColumn9.HeaderText = Translations.GetInterfaceString("packages_list_name");
			dataGridViewTextBoxColumn10.HeaderText = Translations.GetInterfaceString("packages_list_version");
			dataGridViewTextBoxColumn11.HeaderText = Translations.GetInterfaceString("packages_list_author");
			dataGridViewTextBoxColumn12.HeaderText = Translations.GetInterfaceString("packages_list_website");
			//Please Wait panel
			labelPleaseWait.Text = Translations.GetInterfaceString("packages_processing");
			//Success panel
			// *** Multi-functional, all labels set at runtime ***
			//Uninstall result panel
			// *** All labels set at runtime ***

			//HACK- WHY IS THIS NEEDED???
			if (panelOptionsPage2.Visible)
			{
				panelOptionsPage2.Hide();
				panelOptionsPage2.Show();
			}
			else
			{
				panelOptionsPage2.Hide();
			}
			

		}

		// form closing
		private void formMain_FormClosing()
		{
			Interface.CurrentOptions.FullscreenMode = radiobuttonFullscreen.Checked;
			Interface.CurrentOptions.VerticalSynchronization = comboboxVSync.SelectedIndex == 1;
			Interface.CurrentOptions.WindowWidth = (int)Math.Round(updownWindowWidth.Value);
			Interface.CurrentOptions.WindowHeight = (int)Math.Round(updownWindowHeight.Value);
			Interface.CurrentOptions.FullscreenWidth = (int)Math.Round(updownFullscreenWidth.Value);
			Interface.CurrentOptions.FullscreenHeight = (int)Math.Round(updownFullscreenHeight.Value);
			Interface.CurrentOptions.FullscreenBits = comboboxFullscreenBits.SelectedIndex == 0 ? 16 : 32;
			Interface.CurrentOptions.Interpolation = (InterpolationMode)comboboxInterpolation.SelectedIndex;
			Interface.CurrentOptions.AnisotropicFilteringLevel = (int)Math.Round(updownAnisotropic.Value);
			Interface.CurrentOptions.AntiAliasingLevel = (int)Math.Round(updownAntiAliasing.Value);
			Interface.CurrentOptions.TransparencyMode = (TransparencyMode)trackbarTransparency.Value;
			Interface.CurrentOptions.ViewingDistance = (int)Math.Round(updownDistance.Value);
			Interface.CurrentOptions.MotionBlur = (MotionBlurMode)comboboxMotionBlur.SelectedIndex;
			Interface.CurrentOptions.Toppling = checkboxToppling.Checked;
			Interface.CurrentOptions.Collisions = checkboxCollisions.Checked;
			Interface.CurrentOptions.Derailments = checkboxDerailments.Checked;
			Interface.CurrentOptions.LoadInAdvance = checkBoxLoadInAdvance.Checked;
			Interface.CurrentOptions.UnloadUnusedTextures = checkBoxUnloadTextures.Checked;
			Interface.CurrentOptions.OldTransparencyMode = checkBoxTransparencyFix.Checked;
			Interface.CurrentOptions.EnableBveTsHacks = checkBoxHacks.Checked;
			Interface.CurrentOptions.IsUseNewRenderer = checkBoxIsUseNewRenderer.Checked;
			Interface.CurrentOptions.GameMode = (GameMode)comboboxMode.SelectedIndex;
			Interface.CurrentOptions.BlackBox = checkboxBlackBox.Checked;
			Interface.CurrentOptions.LoadingSway = checkBoxLoadingSway.Checked;
			Interface.CurrentOptions.UseJoysticks = checkboxJoysticksUsed.Checked;
			Interface.CurrentOptions.AllowAxisEB = checkBoxEBAxis.Checked;
			Interface.CurrentOptions.JoystickAxisThreshold = ((double)trackbarJoystickAxisThreshold.Value - (double)trackbarJoystickAxisThreshold.Minimum) / (double)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum);
			Interface.CurrentOptions.SoundNumber = (int)Math.Round(updownSoundNumber.Value);
			Interface.CurrentOptions.ShowWarningMessages = checkboxWarningMessages.Checked;
			Interface.CurrentOptions.ShowErrorMessages = checkboxErrorMessages.Checked;
			Interface.CurrentOptions.RouteFolder = textboxRouteFolder.Text;
			Interface.CurrentOptions.TrainFolder = textboxTrainFolder.Text;
			Interface.CurrentOptions.MainMenuWidth = this.WindowState == FormWindowState.Maximized ? -1 : this.Size.Width;
			Interface.CurrentOptions.MainMenuHeight = this.WindowState == FormWindowState.Maximized ? -1 : this.Size.Height;
			Interface.CurrentOptions.KioskMode = checkBoxEnableKiosk.Checked;
			Interface.CurrentOptions.KioskModeTimer = (double)numericUpDownKioskTimeout.Value;
			Interface.CurrentOptions.CurrentXParser = (XParsers)comboBoxXparser.SelectedIndex;
			Interface.CurrentOptions.CurrentObjParser = (ObjParsers)comboBoxObjparser.SelectedIndex;
			Interface.CurrentOptions.Panel2ExtendedMode = checkBoxPanel2Extended.Checked;
			Interface.CurrentOptions.Accessibility = checkBoxAccessibility.Checked;
			switch (trackBarHUDSize.Value)
			{
				case 0:
					Interface.CurrentOptions.UserInterfaceFolder = "Slim";
					break;
				case 1:
					Interface.CurrentOptions.UserInterfaceFolder = "Default";
					break;
				case 2:
					Interface.CurrentOptions.UserInterfaceFolder = "Large";
					break;
			}
			if (Result.Start)
			{
				// recently used routes
				if (Interface.CurrentOptions.RecentlyUsedLimit > 0)
				{
					int i; for (i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++)
					{
						if (string.Compare(Result.RouteFile, Interface.CurrentOptions.RecentlyUsedRoutes[i], StringComparison.OrdinalIgnoreCase) == 0)
						{
							break;
						}
					}
					if (i == Interface.CurrentOptions.RecentlyUsedRoutes.Length)
					{
						if (Interface.CurrentOptions.RecentlyUsedRoutes.Length < Interface.CurrentOptions.RecentlyUsedLimit)
						{
							Array.Resize(ref Interface.CurrentOptions.RecentlyUsedRoutes, i + 1);
						}
						else {
							i--;
						}
					}
					for (int j = i; j > 0; j--)
					{
						Interface.CurrentOptions.RecentlyUsedRoutes[j] = Interface.CurrentOptions.RecentlyUsedRoutes[j - 1];
					}
					Interface.CurrentOptions.RecentlyUsedRoutes[0] = Result.RouteFile;
				}
				// recently used trains
				if (Interface.CurrentOptions.RecentlyUsedLimit > 0)
				{
					int i; for (i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++)
					{
						if (string.Compare(Result.TrainFolder, Interface.CurrentOptions.RecentlyUsedTrains[i], StringComparison.OrdinalIgnoreCase) == 0)
						{
							break;
						}
					}
					if (i == Interface.CurrentOptions.RecentlyUsedTrains.Length)
					{
						if (Interface.CurrentOptions.RecentlyUsedTrains.Length < Interface.CurrentOptions.RecentlyUsedLimit)
						{
							Array.Resize(ref Interface.CurrentOptions.RecentlyUsedTrains, i + 1);
						}
						else {
							i--;
						}
					}
					for (int j = i; j > 0; j--)
					{
						Interface.CurrentOptions.RecentlyUsedTrains[j] = Interface.CurrentOptions.RecentlyUsedTrains[j - 1];
					}
					Interface.CurrentOptions.RecentlyUsedTrains[0] = Result.TrainFolder;
				}
			}
			// remove non-existing recently used routes
			{
				int n = 0;
				string[] a = new string[Interface.CurrentOptions.RecentlyUsedRoutes.Length];
				for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++)
				{
					if (System.IO.File.Exists(Interface.CurrentOptions.RecentlyUsedRoutes[i]))
					{
						a[n] = Interface.CurrentOptions.RecentlyUsedRoutes[i];
						n++;
					}
				}
				Array.Resize(ref a, n);
				Interface.CurrentOptions.RecentlyUsedRoutes = a;
			}
			// remove non-existing recently used trains
			{
				int n = 0;
				string[] a = new string[Interface.CurrentOptions.RecentlyUsedTrains.Length];
				for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++)
				{
					if (System.IO.Directory.Exists(Interface.CurrentOptions.RecentlyUsedTrains[i]))
					{
						a[n] = Interface.CurrentOptions.RecentlyUsedTrains[i];
						n++;
					}
				}
				Array.Resize(ref a, n);
				Interface.CurrentOptions.RecentlyUsedTrains = a;
			}
			// remove non-existing route encoding mappings
			{
				int n = 0;
				TextEncoding.EncodingValue[] a = new TextEncoding.EncodingValue[Interface.CurrentOptions.RouteEncodings.Length];
				for (int i = 0; i < Interface.CurrentOptions.RouteEncodings.Length; i++)
				{
					if (System.IO.File.Exists(Interface.CurrentOptions.RouteEncodings[i].Value))
					{
						a[n] = Interface.CurrentOptions.RouteEncodings[i];
						n++;
					}
				}
				Array.Resize(ref a, n);
				Interface.CurrentOptions.RouteEncodings = a;
			}
			// remove non-existing train encoding mappings
			{
				int n = 0;
				TextEncoding.EncodingValue[] a = new TextEncoding.EncodingValue[Interface.CurrentOptions.TrainEncodings.Length];
				for (int i = 0; i < Interface.CurrentOptions.TrainEncodings.Length; i++)
				{
					if (System.IO.Directory.Exists(Interface.CurrentOptions.TrainEncodings[i].Value))
					{
						a[n] = Interface.CurrentOptions.TrainEncodings[i];
						n++;
					}
				}
				Array.Resize(ref a, n);
				Interface.CurrentOptions.TrainEncodings = a;
			}
			// Remember enabled input device plugins
			{
				int n = 0;
				string[] a = new string[InputDevicePlugin.AvailablePluginInfos.Count];
				for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
				{
					InputDevicePlugin.PluginInfo Info = InputDevicePlugin.AvailablePluginInfos[i];
					if (Info.Status != InputDevicePlugin.PluginInfo.PluginStatus.Enable) {
						continue;
					}
					string PluginPath = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("InputDevicePlugins"), Info.FileName);
					if (System.IO.File.Exists(PluginPath))
					{
						a[n] = Info.FileName;
						n++;
					}
				}
				Array.Resize(ref a, n);
				Interface.CurrentOptions.EnableInputDevicePlugins = a;
			}
			// Unload input device plugins if we're closing the program
			if (!Result.Start)
			{
				for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
				{
					InputDevicePlugin.CallPluginUnload(i);
				}
			}
			Program.Sounds.Deinitialize();
			DisposePreviewRouteThread();
			{
				string error;
				Program.CurrentHost.UnloadPlugins(out error);
			}
			if (!OpenTK.Configuration.RunningOnMacOS)
			{
				routeWatcher.Dispose();
				trainWatcher.Dispose();
			}
			workerThread.Dispose();
			// finish
#if !DEBUG
			try
			{
#endif
				Interface.SaveOptions();
				Program.FileSystem.SaveCurrentFileSystemConfiguration();
#if !DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Save options", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
#endif
#if !DEBUG
			try
			{
#endif
				Interface.SaveControls(null, Interface.CurrentControls);
#if !DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Save controls", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
#endif
		}

		// resize
		private void formMain_Resize(object sender, EventArgs e)
		{
			try
			{
				int wt = panelStart.Width;
				int ox = labelStart.Left;
				int wa = (wt - 3 * ox) / 2;
				int wb = (wt - 3 * ox) / 2;
				groupboxRouteSelection.Width = wa;
				groupboxRouteDetails.Left = 2 * ox + wa;
				groupboxRouteDetails.Width = wb;
				groupboxTrainSelection.Width = wa;
				groupboxTrainDetails.Left = 2 * ox + wa;
				groupboxTrainDetails.Width = wb;
				int oy = (labelRoute.Top - labelStartTitleBackground.Height) / 2;
				int ht = (labelStart.Top - labelRoute.Top - 4 * oy) / 2 - labelRoute.Height - oy;
				groupboxRouteSelection.Height = ht;
				groupboxRouteDetails.Height = ht;
				labelTrain.Top = groupboxRouteSelection.Top + groupboxRouteSelection.Height + 2 * oy;
				groupboxTrainSelection.Top = labelTrain.Top + labelTrain.Height + oy;
				groupboxTrainDetails.Top = labelTrain.Top + labelTrain.Height + oy;
				groupboxTrainSelection.Height = ht;
				groupboxTrainDetails.Height = ht;
				tabcontrolRouteSelection.Width = groupboxRouteSelection.Width - 2 * tabcontrolRouteSelection.Left;
				tabcontrolRouteSelection.Height = groupboxRouteSelection.Height - 3 * tabcontrolRouteSelection.Top / 2;
				tabcontrolRouteDetails.Width = groupboxRouteDetails.Width - 2 * tabcontrolRouteDetails.Left;
				tabcontrolRouteDetails.Height = groupboxRouteDetails.Height - 3 * tabcontrolRouteDetails.Top / 2;
				tabcontrolTrainSelection.Width = groupboxTrainSelection.Width - 2 * tabcontrolTrainSelection.Left;
				tabcontrolTrainSelection.Height = groupboxTrainSelection.Height - 3 * tabcontrolTrainSelection.Top / 2;
				tabcontrolTrainDetails.Width = groupboxTrainDetails.Width - 2 * tabcontrolTrainDetails.Left;
				tabcontrolTrainDetails.Height = groupboxTrainDetails.Height - 3 * tabcontrolTrainDetails.Top / 2;
			}
			catch { }
			try
			{
				int width = Math.Min((panelOptions.Width - 24) / 2, 420);
				panelOptionsLeft.Width = width;
				panelOptionsRight.Left = panelOptionsLeft.Left + width + 8;
				panelOptionsRight.Width = width;
			}
			catch { }
			try
			{
				int width = Math.Min((panelReview.Width - 32) / 3, 360);
				groupboxReviewRoute.Width = width;
				groupboxReviewTrain.Left = groupboxReviewRoute.Left + width + 8;
				groupboxReviewTrain.Width = width;
				groupboxReviewDateTime.Left = groupboxReviewTrain.Left + width + 8;
				groupboxReviewDateTime.Width = width;
			}
			catch { }
		}

		// shown
		private void formMain_Shown(object sender, EventArgs e)
		{
			if (radiobuttonStart.Checked)
			{
				listviewRouteFiles.Focus();
			}
			else if (radiobuttonReview.Checked)
			{
				listviewScore.Focus();
			}
			else if (radiobuttonControls.Checked)
			{
				listviewControls.Focus();
			}
			else if (radiobuttonOptions.Checked)
			{
				comboboxLanguages.Focus();
			}
			//TODO: Needs focus changing when packages tab is selected
			formMain_Resize(null, null);
			if (this.WindowState != FormWindowState.Maximized)
			{
				System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromControl(this);
				if ((double)this.Width >= 0.95 * (double)s.WorkingArea.Width | (double)this.Height >= 0.95 * (double)s.WorkingArea.Height)
				{
					this.WindowState = FormWindowState.Maximized;
				}
			}
			radiobuttonStart.Focus();
			// command line arguments
			if (Result.TrainFolder != null)
			{
				if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
				ShowTrain(false);
			}
			if (Result.RouteFile != null)
			{
				ShowRoute(false);
			}
		}



		// ========
		// top page
		// ========

		// page selection
		private void radiobuttonStart_CheckedChanged(object sender, EventArgs e)
		{
			if (workerThread.IsBusy)
			{
				radioButtonPackages.Checked = true;
				//If the worker thread is currently extracting or creating a package, don't allow the user to cancel...
				return;
			}
			panelStart.Visible = true;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelOptions.Visible = false;
			panelPackages.Visible = false;
			panelPanels.BackColor = labelStartTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonHighlight;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radioButtonPackages.BackColor = SystemColors.ButtonFace;
			//Update the route/ train displays in case a package has been installed
			textboxRouteFolder_TextChanged(this, EventArgs.Empty);
			textboxTrainFolder_TextChanged(this, EventArgs.Empty);
			
		}
		private void radiobuttonReview_CheckedChanged(object sender, EventArgs e)
		{
			if (workerThread.IsBusy)
			{
				radioButtonPackages.Checked = true;
				//If the worker thread is currently extracting or creating a package, don't allow the user to cancel...
				return;
			}
			panelReview.Visible = true;
			panelStart.Visible = false;
			panelControls.Visible = false;
			panelOptions.Visible = false;
			panelPackages.Visible = false;
			panelPanels.BackColor = labelReviewTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonHighlight;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radioButtonPackages.BackColor = SystemColors.ButtonFace;
		}
		private void radiobuttonControls_CheckedChanged(object sender, EventArgs e)
		{
			if (workerThread.IsBusy)
			{
				radioButtonPackages.Checked = true;
				//If the worker thread is currently extracting or creating a package, don't allow the user to cancel...
				return;
			}
			panelControls.Visible = true;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelOptions.Visible = false;
			panelPackages.Visible = false;
			panelPanels.BackColor = labelControlsTitle.BackColor;
			pictureboxJoysticks.Visible = true;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonHighlight;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radioButtonPackages.BackColor = SystemColors.ButtonFace;
		}
		private void radiobuttonOptions_CheckedChanged(object sender, EventArgs e)
		{
			if (workerThread.IsBusy)
			{
				radioButtonPackages.Checked = true;
				//If the worker thread is currently extracting or creating a package, don't allow the user to cancel...
				return;
			}
			panelOptions.Visible = true;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelPackages.Visible = false;
			panelPanels.BackColor = labelOptionsTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonHighlight;
			radioButtonPackages.BackColor = SystemColors.ButtonFace;
		}
		private void radioButtonPackages_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonPackages.Checked && workerThread.IsBusy)
			{
				return;
			}
			panelOptions.Visible = false;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelPackages.Visible = true;
			panelPanels.BackColor = labelPackagesTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			radioButtonPackages.BackColor = SystemColors.ButtonHighlight;
			//Load packages & rest panel states
			if (radioButtonPackages.Checked)
			{
				ResetInstallerPanels();
				string errorMessage;
				if (Database.LoadDatabase(currentDatabaseFolder, currentDatabaseFile, out errorMessage))
				{
					PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages, true, false, false);
				}
				if (errorMessage != string.Empty)
				{
					MessageBox.Show(Translations.GetInterfaceString(errorMessage));
				}
				comboBoxPackageType.SelectedIndex = 0;
			}
		}

		/// <summary>Launches a web-browser linked to the project homepage</summary>
		private void linkHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			const string Url = "http://github.com/leezer3/OpenBVE/";
			try
			{
				Process.Start(Url);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private bool currentlyClosing = false;
		private void buttonClose_Click(object sender, EventArgs e)
		{
			currentlyClosing = true;
			if (sender != null)
			{
				//Don't cause an infinite loop
				this.Close();
			}
			//HACK: Call Application.DoEvents() to force the message pump to process all pending messages when the form closes
			//This fixes the main form failing to close on Linux
			formMain_FormClosing();
			Application.DoEvents();
			if (Program.CurrentHost.MonoRuntime && sender != StartGame)
			{
				//On some systems, the process *still* seems to hang around
				//https://github.com/leezer3/OpenBVE/issues/213
				//Altered (again) https://github.com/cefsharp/CefSharp/issues/990#issuecomment-134962152
				Process.GetCurrentProcess().Kill();
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (currentlyClosing)
			{
				return;
			}
			//Call the explicit closing method
			buttonClose_Click(null, e);
		}


		// ======
		// events
		// ======

		// tick

		private JoystickState[] currentJoystickStates;

		private void timerEvents_Tick(object sender, EventArgs e)
		{
			Program.Joysticks.RefreshJoysticks();
			if (currentJoystickStates == null || currentJoystickStates.Length < Program.Joysticks.AttachedJoysticks.Values.Count)
			{
				currentJoystickStates = new JoystickState[Program.Joysticks.AttachedJoysticks.Values.Count];
			}	
			if (radiobuttonJoystick.Checked && textboxJoystickGrab.Focused && this.Tag == null && listviewControls.SelectedIndices.Count == 1)
			{
				int j = listviewControls.SelectedIndices[0];

				for (int k = 0; k < Program.Joysticks.AttachedJoysticks.Count; k++)
				{
					Guid guid = Program.Joysticks.AttachedJoysticks.ElementAt(k).Key;
					Program.Joysticks.AttachedJoysticks[guid].Poll();
					bool railDriver = Program.Joysticks.AttachedJoysticks[guid] is AbstractRailDriver;
					int axes = Program.Joysticks.AttachedJoysticks[guid].AxisCount();
						for (int i = 0; i < axes; i++)
						{
							double a = Program.Joysticks.AttachedJoysticks[guid].GetAxis(i);
							if (a < -0.75)
							{
								if (railDriver)
								{
									if (i == 4)
									{
										//Bail-off lever, starts at negative
										continue;
									}
								Interface.CurrentControls[j].Method = ControlMethod.RailDriver;
								}
								Interface.CurrentControls[j].Device = guid;
								Interface.CurrentControls[j].Component = JoystickComponent.Axis;
								Interface.CurrentControls[j].Element = i;
								Interface.CurrentControls[j].Direction = -1;
								radiobuttonJoystick.Focus();
								UpdateJoystickDetails();
								UpdateControlListElement(listviewControls.Items[j], j, true);
								return;
							}
							if (a > 0.75)
							{
								if (railDriver)
								{
									Interface.CurrentControls[j].Method = ControlMethod.RailDriver;
								}
								Interface.CurrentControls[j].Device = guid;
								Interface.CurrentControls[j].Component = JoystickComponent.Axis;
								Interface.CurrentControls[j].Element = i;
								Interface.CurrentControls[j].Direction = 1;
								radiobuttonJoystick.Focus();
								UpdateJoystickDetails();
								UpdateControlListElement(listviewControls.Items[j], j, true);
								return;
							}
						}
						int buttons = Program.Joysticks.AttachedJoysticks[guid].ButtonCount();
						for (int i = 0; i < buttons; i++)
						{
							if (Program.Joysticks.AttachedJoysticks[guid].GetButton(i) == ButtonState.Pressed)
							{
								if (railDriver)
								{
									Interface.CurrentControls[j].Method = ControlMethod.RailDriver;
								}
								Interface.CurrentControls[j].Device = guid;
								Interface.CurrentControls[j].Component = JoystickComponent.Button;
								Interface.CurrentControls[j].Element = i;
								Interface.CurrentControls[j].Direction = 1;
								radiobuttonJoystick.Focus();
								UpdateJoystickDetails();
								UpdateControlListElement(listviewControls.Items[j], j, true);
								return;
							}
						}
						int hats = Program.Joysticks.AttachedJoysticks[guid].HatCount();
						for (int i = 0; i < hats; i++)
						{
							JoystickHatState hat = Program.Joysticks.AttachedJoysticks[guid].GetHat(i);
							if (hat.Position != HatPosition.Centered)
							{
								Interface.CurrentControls[j].Device = guid;
								Interface.CurrentControls[j].Component = JoystickComponent.Hat;
								Interface.CurrentControls[j].Element = i;
								Interface.CurrentControls[j].Direction = (int)hat.Position;
								radiobuttonJoystick.Focus();
								UpdateJoystickDetails();
								UpdateControlListElement(listviewControls.Items[j], j, true);
								return;
							}
						}
					
					
				}
			}

			pictureboxJoysticks.Invalidate();

		}



		// =========
		// functions
		// =========

		/// <summary>Attempts to load an image into memory using the OpenBVE path resolution API</summary>
		private Image LoadImage(string Folder, string Title)
		{
			try
			{
				string File = OpenBveApi.Path.CombineFile(Folder, Title);
				if (System.IO.File.Exists(File))
				{
					try
					{
						return ImageExtensions.FromFile(File);
					}
					catch
					{
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
	
		/// <summary>Attempts to load an image into a picture box using the OpenBVE path resolution API</summary>
		private void TryLoadImage(PictureBox Box, string File)
		{
			try
			{
				if (!System.IO.File.Exists(File))
				{
					string Folder = Program.FileSystem.GetDataFolder("Menu");
					File = Path.CombineFile(Folder, File);
				}
				if (System.IO.File.Exists(File))
				{
					System.IO.FileInfo f = new System.IO.FileInfo(File);
					if (f.Length == 0)
					{
						Box.Image = Box.ErrorImage;
						return;
					}
					try
					{
						Box.Image = ImageExtensions.FromFile(File);
						return;
					}
					catch
					{
						Box.Image = Box.ErrorImage;
						return;
					}
				}
				Box.Image = Box.ErrorImage;
			}
			catch
			{
				Box.Image = Box.ErrorImage;
			}
		}

		private void checkBoxLoadInAdvance_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxLoadInAdvance.Checked)
			{
				//Load in advance negates unloading textures...
				checkBoxUnloadTextures.Checked = false;
				checkBoxUnloadTextures.Enabled = false;
			}
			else
			{
				checkBoxUnloadTextures.Enabled = true;
			}
		}

		private void checkBoxUnloadTextures_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxUnloadTextures.Checked)
			{
				//If we use display lists, a stale texture reference may remain in the GPU, resulting in untextured faces
				checkBoxLoadInAdvance.Checked = false;
				checkBoxLoadInAdvance.Enabled = false;
			}
			else
			{
				checkBoxLoadInAdvance.Enabled = true;
			}
		}

		private void checkForUpdate()
		{
			string xmlURL = "http://openbve-project.net/version.xml";
			HttpWebRequest hwRequest = (HttpWebRequest)WebRequest.Create(xmlURL);
			hwRequest.Timeout = 5000;
			HttpWebResponse hwResponse = null;
			XmlTextReader reader = null;
			string url = null;
			string date = null;
			Version newVersion = new Version();
			try
			{
				hwResponse = (HttpWebResponse)hwRequest.GetResponse();
				// ReSharper disable once AssignNullToNotNullAttribute
				reader = new XmlTextReader(hwResponse.GetResponseStream());
				reader.MoveToContent();
				string elementName = "";
				if ((reader.NodeType == XmlNodeType.Element) &&
					(reader.Name == "openBVE"))
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element)
							elementName = reader.Name.ToLowerInvariant();
						else
						{
							if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
							{
								switch (elementName)
								{
									case "version":
										newVersion = new Version(reader.Value);
										break;
									case "url":
										url = reader.Value;
										break;
									case "date":
										date = reader.Value;
										break;
								}
							}
						}
					}
				}

			}
			finally
			{
				if (reader != null) reader.Close();
				if (hwResponse != null) hwResponse.Close();
			}
			Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			bool newerVersion = curVersion.CompareTo(newVersion) < 0;
			if (url == null)
			{
				//The internet connection is broken.....
				MessageBox.Show(Translations.GetInterfaceString("panel_updates_invalid"));
				return;
			}
			if (newerVersion)
			{
				string question = Translations.GetInterfaceString("panel_updates_new");
				question = question.Replace("[version]", newVersion.ToString());
				question = question.Replace("[date]", date);
				if (DialogResult.OK == MessageBox.Show(this, question, Translations.GetInterfaceString("panel_updates"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
				{
					Process.Start(url);
				}
			}
			else
			{
				MessageBox.Show(Translations.GetInterfaceString("panel_updates_old"));
			}
		}

		private formAbout AboutDialog;

		private void aboutLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (AboutDialog == null || AboutDialog.Visible == false)
			{
				AboutDialog = new formAbout();
				AboutDialog.Show();
			}
		}

		private void linkLabelCheckUpdates_Click(object sender, EventArgs e)
		{
			checkForUpdate();
		}

		private void buttonOptionsPrevious_Click(object sender, EventArgs e)
		{
			if (panelOptionsLeft.Visible)
			{
				panelOptionsLeft.Hide();
				panelOptionsRight.Hide();
				panelOptionsPage2.Show();
			}
			else
			{
				panelOptionsPage2.Hide();
				panelOptionsLeft.Show();
				panelOptionsRight.Show();
			}
		}

		private void buttonSetRouteDirectory_Click(object sender, EventArgs e)
		{
			using (var folderSelectDialog = new FolderBrowserDialog())
			{
				if (folderSelectDialog.ShowDialog() == DialogResult.OK)
				{
					Program.FileSystem.RouteInstallationDirectory = folderSelectDialog.SelectedPath;
					textBoxRouteDirectory.Text = folderSelectDialog.SelectedPath;
				}
			}
		}

		private void buttonTrainInstallationDirectory_Click(object sender, EventArgs e)
		{
			using (var folderSelectDialog = new FolderBrowserDialog())
			{
				if (folderSelectDialog.ShowDialog() == DialogResult.OK)
				{
					Program.FileSystem.TrainInstallationDirectory = folderSelectDialog.SelectedPath;
					textBoxTrainDirectory.Text = folderSelectDialog.SelectedPath;
				}
			}
		}

		private void buttonOtherDirectory_Click(object sender, EventArgs e)
		{
			using (var folderSelectDialog = new FolderBrowserDialog())
			{
				if (folderSelectDialog.ShowDialog() == DialogResult.OK)
				{
					Program.FileSystem.OtherInstallationDirectory = folderSelectDialog.SelectedPath;
					textBoxOtherDirectory.Text = folderSelectDialog.SelectedPath;
				}
			}
		}

		private void comboBoxCompressionFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (comboBoxCompressionFormat.SelectedIndex)
			{
				case 0:
					Interface.CurrentOptions.packageCompressionType = CompressionType.Zip;
					break;
				case 1:
					Interface.CurrentOptions.packageCompressionType = CompressionType.TarGZ;
					break;
				case 2:
					Interface.CurrentOptions.packageCompressionType = CompressionType.BZ2;
					break;
			}
		}

		private void linkLabelReportBug_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var bugReportForm = new formBugReport();
			bugReportForm.ShowDialog();
		}

		private void comboBoxRailDriverUnits_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (comboBoxRailDriverUnits.SelectedIndex)
			{
				case 0:
					Interface.CurrentOptions.RailDriverMPH = true;
					break;
				case 1:
					Interface.CurrentOptions.RailDriverMPH = false;
					break;
			}
		}

		private void buttonRailDriverCalibration_Click(object sender, EventArgs e)
		{
			using (formRaildriverCalibration f = new formRaildriverCalibration())
			{
				f.ShowDialog();
			}
		}

		private void checkBoxReverseConsist_CheckedChanged(object sender, EventArgs e)
		{
			Result.ReverseConsist = checkBoxReverseConsist.Checked;
		}

		private void comboBoxCompatibilitySignals_SelectedIndexChanged(object sender, EventArgs e)
		{
			Interface.CurrentOptions.CurrentCompatibilitySignalSet = compatibilitySignals[comboBoxCompatibilitySignals.GetItemText(comboBoxCompatibilitySignals.SelectedItem)]; //Cheat by using the name as the dictionary key!
		}
	}
}
