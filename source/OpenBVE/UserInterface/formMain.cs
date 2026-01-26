using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using LibRender2.MotionBlurs;
using LibRender2.Text;
using OpenBve.Input;
using OpenBve.UserInterface;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Packages;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenTK.Input;
using ButtonState = OpenTK.Input.ButtonState;
using ContentAlignment = System.Drawing.ContentAlignment;
using Control = System.Windows.Forms.Control;
using Path = OpenBveApi.Path;

namespace OpenBve {
	internal partial class formMain : Form
	{
		private formMain()
		{
			InitializeComponent();
		}

		public sealed override string Text
		{
			get => base.Text;
			set => base.Text = value;
		}
		
		internal static LaunchParameters ShowMainDialog(LaunchParameters initial)
		{
			using (formMain Dialog = new formMain())
			{
				Dialog.Result = initial;
				Dialog.ShowDialog();
				LaunchParameters result = Dialog.Result;
				//Dispose of the worker thread when closing the form
				//If it's still running, it attempts to update a non-existant form and crashes nastily
				Dialog.DisposePreviewRouteThread();
				if (!OpenTK.Configuration.RunningOnMacOS)
				{
					Dialog.trainWatcher.Dispose();
					Dialog.routeWatcher.Dispose();
				}
				return result;
			}
		}

		// members
		private LaunchParameters Result;
		private int[] EncodingCodepages;
		private Image JoystickImage;
		private Image RailDriverImage;
		private Image GamepadImage;
		private Image XboxImage;
		private Image ZukiImage;

		// ====
		// form
		// ====

		// load
		private void formMain_Load(object sender, EventArgs e)
		{
			MinimumSize = Size;
			if (Interface.CurrentOptions.MainMenuWidth == -1 & Interface.CurrentOptions.MainMenuHeight == -1)
			{
				WindowState = FormWindowState.Maximized;
			}
			else if (Interface.CurrentOptions.MainMenuWidth > 0 & Interface.CurrentOptions.MainMenuHeight > 0)
			{
				Size = new Size(Interface.CurrentOptions.MainMenuWidth, Interface.CurrentOptions.MainMenuHeight);
				CenterToScreen();
			}
			labelVersion.Text = @"v" + Application.ProductVersion + Program.VersionSuffix;
			if (IntPtr.Size != 4)
			{
				labelVersion.Text += @" 64-bit";
			}
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// form icon
			try
			{
				string File = Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.ico");
				Icon = new Icon(File);
			}
			catch
			{
				// Ignored
			}
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
				string[] Args = Environment.GetCommandLineArgs();
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
			Image Bve5Icon = LoadImage(MenuFolder, "icon_bve5.png");
			Image TrainIcon = LoadImage(MenuFolder, "icon_train.png");
			Image MSTSIcon = LoadImage(MenuFolder, "icon_msts.png");
			Image KeyboardIcon = LoadImage(MenuFolder, "icon_keyboard.png");
			Image MouseIcon = LoadImage(MenuFolder, "icon_mouse.png");
			Image JoystickIcon = LoadImage(MenuFolder, "icon_joystick.png");
			Image GamepadIcon = LoadImage(MenuFolder, "icon_gamepad.png");
			JoystickImage = LoadImage(MenuFolder, "joystick.png");
			RailDriverImage = LoadImage(MenuFolder, "raildriver2.png");
			GamepadImage = LoadImage(MenuFolder, "gamepad.png");
			XboxImage = LoadImage(MenuFolder, "xbox.png");
			ZukiImage = LoadImage(MenuFolder, "zuki.png");
			Image Logo = LoadImage(MenuFolder, "logo.png");
			if (Logo != null) pictureboxLogo.Image = Logo;
			pictureboxRouteImage.ErrorImage = LoadImage(Program.FileSystem.GetDataFolder("Menu"),"error_route.png");
			pictureboxTrainImage.ErrorImage = LoadImage(Program.FileSystem.GetDataFolder("Menu"), "error_train.png");
			// route selection
			listviewRouteFiles.SmallImageList = new ImageList { TransparentColor = Color.White };
			listViewRoutePackages.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (ParentIcon != null) listviewRouteFiles.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewRouteFiles.SmallImageList.Images.Add("folder", FolderIcon);
			if (CsvRouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("csvroute", CsvRouteIcon);
			if (RwRouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("rwroute", RwRouteIcon);
			if (MechanikRouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("mechanik", MechanikRouteIcon);
			if (Bve5Icon != null) listviewRouteFiles.SmallImageList.Images.Add("bve5", Bve5Icon);
			if (ParentIcon != null) listViewRoutePackages.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listViewRoutePackages.SmallImageList.Images.Add("folder", FolderIcon);
			if (CsvRouteIcon != null) listViewRoutePackages.SmallImageList.Images.Add("csvroute", CsvRouteIcon);
			if (RwRouteIcon != null) listViewRoutePackages.SmallImageList.Images.Add("rwroute", RwRouteIcon);
			if (Bve5Icon != null) listViewRoutePackages.SmallImageList.Images.Add("bve5", Bve5Icon);
			if (MechanikRouteIcon != null) listViewRoutePackages.SmallImageList.Images.Add("mechanik", MechanikRouteIcon);
			if (DiskIcon != null) listviewRouteFiles.SmallImageList.Images.Add("disk", DiskIcon);
			listviewRouteFiles.Columns.Clear();
			listviewRouteFiles.Columns.Add("");
			listViewRoutePackages.Columns.Clear();
			listViewRoutePackages.Columns.Add("");
			listviewRouteRecently.Items.Clear();
			listviewRouteRecently.Columns.Add("");
			listviewRouteRecently.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (CsvRouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("csvroute", CsvRouteIcon);
			if (RwRouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("rwroute", RwRouteIcon);
			if (Bve5Icon != null) listviewRouteRecently.SmallImageList.Images.Add("bve5", Bve5Icon);
			if (MechanikRouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("mechanik", MechanikRouteIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++)
			{
				if (string.IsNullOrEmpty(Interface.CurrentOptions.RecentlyUsedRoutes[i])) continue;
				string routeFileName = Path.GetFileName(Interface.CurrentOptions.RecentlyUsedRoutes[i]);
				string routePath = Path.GetDirectoryName(Interface.CurrentOptions.RecentlyUsedRoutes[i]);
				if (string.IsNullOrEmpty(routeFileName) || string.IsNullOrEmpty(routePath)) continue;
				ListViewItem listItem = listviewRouteRecently.Items.Add(routeFileName);
				string extension = System.IO.Path.GetExtension(Interface.CurrentOptions.RecentlyUsedRoutes[i]).ToLowerInvariant();
				switch (extension)
				{
					case ".dat":
						listItem.ImageKey = @"mechanik";
						break;
					case ".rw":
						listItem.ImageKey = @"rwroute";
						break;
					case ".csv":
						listItem.ImageKey = @"csvroute";
						break;
					case ".txt":
						listItem.ImageKey = @"bve5";
						break;
				}

				listItem.Tag = Interface.CurrentOptions.RecentlyUsedRoutes[i];
				if (textboxRouteFolder.Items.Count == 0 || !textboxRouteFolder.Items.Contains(routePath))
				{
					textboxRouteFolder.Items.Add(routePath);
				}

			}
			listviewRouteRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			currentRoutePackageFolder = Program.FileSystem.RouteInstallationDirectory;
			currentTrainPackageFolder = Program.FileSystem.TrainInstallationDirectory;
			// train selection
			listviewTrainFolders.SmallImageList = new ImageList { TransparentColor = Color.White };
			listViewTrainPackages.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (ParentIcon != null) listviewTrainFolders.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewTrainFolders.SmallImageList.Images.Add("folder", FolderIcon);
			if (TrainIcon != null) listviewTrainFolders.SmallImageList.Images.Add("train", TrainIcon);
			if (MSTSIcon != null) listviewTrainFolders.SmallImageList.Images.Add("msts", MSTSIcon);
			if (DiskIcon != null) listviewTrainFolders.SmallImageList.Images.Add("disk", DiskIcon);
			if (ParentIcon != null) listViewTrainPackages.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listViewTrainPackages.SmallImageList.Images.Add("folder", FolderIcon);
			if (TrainIcon != null) listViewTrainPackages.SmallImageList.Images.Add("train", TrainIcon);
			listviewTrainFolders.Columns.Clear();
			listviewTrainFolders.Columns.Add("");
			listViewTrainPackages.Columns.Clear();
			listViewTrainPackages.Columns.Add("");
			listviewTrainRecently.Columns.Clear();
			listviewTrainRecently.Columns.Add("");
			listviewTrainRecently.SmallImageList = new ImageList { TransparentColor = Color.White };
			if (TrainIcon != null) listviewTrainRecently.SmallImageList.Images.Add("train", TrainIcon);
			if (MSTSIcon != null) listviewTrainRecently.SmallImageList.Images.Add("msts", MSTSIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++)
			{
				if (string.IsNullOrEmpty(Interface.CurrentOptions.RecentlyUsedTrains[i])) continue;
				string trainFileName = Path.GetFileName(Interface.CurrentOptions.RecentlyUsedTrains[i]);
				string trainPath = Path.GetDirectoryName(Interface.CurrentOptions.RecentlyUsedTrains[i]);
				if (!Directory.Exists(trainPath))
				{
					continue;
				}
				ListViewItem Item = listviewTrainRecently.Items.Add(trainFileName);
				Item.ImageKey = trainFileName.EndsWith(".con", StringComparison.InvariantCultureIgnoreCase) ? @"msts" : @"train";
				Item.Tag = Interface.CurrentOptions.RecentlyUsedTrains[i];
				if (textboxTrainFolder.Items.Count == 0 || !textboxTrainFolder.Items.Contains(trainPath))
				{
					textboxTrainFolder.Items.Add(trainPath);
				}
			}
			listviewTrainRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			// text boxes
			if (Interface.CurrentOptions.RouteFolder.Length != 0 && Directory.Exists(Interface.CurrentOptions.RouteFolder))
			{
				textboxRouteFolder.Text = Interface.CurrentOptions.RouteFolder;
			}
			else {
				textboxRouteFolder.Text = Program.FileSystem.InitialRouteFolder;
			}
			if (Interface.CurrentOptions.TrainFolder.Length != 0 && Directory.Exists(Interface.CurrentOptions.TrainFolder))
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
				Array.Sort(EncodingDescriptions, EncodingCodepages, 1, Info.Length);
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
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : Game.CurrentScore.CurrentValue / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * Translations.RatingsCount);
					if (index >= Translations.RatingsCount) index = Translations.RatingsCount - 1;
					labelReviewRouteValue.Text = Game.LogRouteName;
					labelReviewTrainValue.Text = Game.LogTrainName;
					labelReviewDateValue.Text = Game.LogDateTime.ToString("yyyy-MM-dd", Culture);
					labelReviewTimeValue.Text = Game.LogDateTime.ToString("HH:mm:ss", Culture);
					switch (Interface.CurrentOptions.PreviousGameMode)
					{
						case GameMode.Arcade: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","arcade"}); break;
						case GameMode.Normal: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","normal"}); break;
						case GameMode.Expert: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","expert"}); break;
						default: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","unknown"}); break;
					}
					if (Game.CurrentScore.Maximum == 0)
					{
						labelRatingColor.BackColor = Color.Gray;
						labelRatingDescription.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"rating","unknown"});
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
						labelRatingDescription.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"rating", index.ToString(Culture)});
					}
					labelRatingAchievedValue.Text = Game.CurrentScore.CurrentValue.ToString(Culture);
					labelRatingMaximumValue.Text = Game.CurrentScore.Maximum.ToString(Culture);
					labelRatingRatioValue.Text = (100.0 * ratio).ToString("0.00", Culture) + @"%";
				}
			}
			comboboxBlackBoxFormat.Items.Clear();
			comboboxBlackBoxFormat.Items.AddRange(new object[] { "", "" });
			comboboxBlackBoxFormat.SelectedIndex = 1;
			if (Game.BlackBoxEntries.Count == 0)
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
				double a = (trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum) * Interface.CurrentOptions.JoystickAxisThreshold + trackbarJoystickAxisThreshold.Minimum;
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
			try
			{
				SetFont(Controls, Interface.CurrentOptions.Font);
			}
			catch
			{
				// ignore
			}
			
			radiobuttonStart_CheckedChanged(this, EventArgs.Empty); // Mono mucks up the button colors and selections if non-default color and we don't reset them
			string defaultFont = comboBoxFont.Font.Name;
			
			List<FontFamily> fonts = FontFamily.Families.ToList();
			List<string> addedFonts = new List<string>();
			for (int i = fonts.Count - 1; i > 0; i--)
			{
				
				if ((Program.CurrentHost.Platform == HostPlatform.WINE && Fonts.BlockedFonts.Any(f => fonts[i].Name.StartsWith(f))) || (Program.CurrentHost.Platform != HostPlatform.WINE && Fonts.BlockedFonts.Contains(fonts[i].Name)))
				{
					fonts.RemoveAt(i);
					continue;
				}

				if (Program.CurrentHost.Platform == HostPlatform.WINE && fonts[i].Name.StartsWith("Noto") && !fonts[i].Name.EndsWith("Regular"))
				{
					// Dump the bold, italic and stuff from Wine
					fonts.RemoveAt(i);
					continue;
				}

				if (fonts[i].Name.IndexOf("MathJax", 0, StringComparison.OrdinalIgnoreCase) != -1)
				{
					// Cross-platform browser Math fonts, useless for general use
					fonts.RemoveAt(i);
					continue;
				}
				/*
				 * Under Mono, different font weights are returned as a separate font
				 * Only use the first one, otherwise our list becomes absolutely massive
				 *
				 * We have no way to tell these apart (yuck), but the regular weight seems to be returned first normally
				 *
				 * Also avoids duplicates elsewhere, if someone has been installing multiple copies
				 *
				 * BUG: for some reason, the *first* Mono font box entry is glitched. We'll assume that this is another unavoidable oddity at present
				 */
				if (addedFonts.Contains(fonts[i].Name))
				{
					fonts.RemoveAt(i);
					continue;
				}
				addedFonts.Add(fonts[i].Name);
			}
			// Fonts can be returned in a random order (no idea why)- sort, ignoring the period that some seem to add at the front
			fonts.Sort((x, y) => string.Compare(x.Name.TrimStart('.'), y.Name.TrimStart('.'), StringComparison.InvariantCultureIgnoreCase));
			comboBoxFont.DataSource = fonts;
			comboBoxFont.DrawMode = DrawMode.OwnerDrawFixed;
			for (int i = 0; i < comboBoxFont.Items.Count; i++)
			{
				if (fonts[i].Name == defaultFont)
				{
					comboBoxFont.SelectedIndex = i;
					break;
				}
			}

			panelOptionsPage2.Visible = false; // Deliberately hide, as changing font can glitch this into visibility
			comboBoxFont.DrawItem += comboBoxFont_DrawItem;
		}

		private void comboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxFont.Items[comboBoxFont.SelectedIndex] is FontFamily font)
			{
				string oldFont = Interface.CurrentOptions.Font;
				try
				{
					SetFont(Controls, font.Name);
					Interface.CurrentOptions.Font = font.Name;
					Program.Renderer.Fonts = new Fonts(Program.CurrentHost, Program.Renderer, font.Name);
				}
				catch
				{
					// setting the font failed, so roll back
					MessageBox.Show(@"Failed to set font " + font.Name, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}), MessageBoxButtons.OK, MessageBoxIcon.Warning);
					SetFont(Controls, oldFont);
					Interface.CurrentOptions.Font = oldFont;
					Program.Renderer.Fonts = new Fonts(Program.CurrentHost, Program.Renderer, oldFont);
				}
				
			}
			
		}

		private void comboBoxFont_DrawItem(object sender, DrawItemEventArgs e)
		{
			var comboBox = (ComboBox)sender;
			var fontFamily = (FontFamily)comboBox.Items[e.Index];
			var fallback = false;
			Font font;
			try {
				font = new Font(fontFamily, comboBox.Font.SizeInPoints);
			} catch {
				// Fallback to render the font name with current font
				font = new Font(Interface.CurrentOptions.Font, comboBox.Font.SizeInPoints);
				fallback = true;
			}

			e.DrawBackground();
			e.Graphics.DrawString((fallback ? fontFamily.Name : font.Name), font, (fallback ? Brushes.Red : Brushes.Black), e.Bounds.X, e.Bounds.Y);
		}

		public static void SetFont(Control.ControlCollection ctrls, string fontName)
		{
			foreach (Control ctrl in ctrls)
			{
				// recursive
				SetFont(ctrl.Controls, fontName);
				ctrl.Font = new Font(fontName, ctrl.Font.Size);
			}
		}

		/// <summary>This function is called to change the display language of the program</summary>
		private void ApplyLanguage()
		{
			Translations.SetInGameLanguage(Translations.CurrentLanguageCode);
			// Main form title bar
			Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"});
			/*
			 * Localisation for strings in main panel
			 */
			radiobuttonStart.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","start"});
			radiobuttonReview.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","review"});
			radiobuttonControls.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","controls"});
			radiobuttonOptions.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","options"});
			linkHomepage.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","homepage"});
			buttonClose.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","close"});
			radioButtonPackages.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","packages"});
			linkLabelCheckUpdates.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates"});
			linkLabelReportBug.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","reportbug"});
			aboutLabel.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","about"});
			/*
			 * Localisation for strings in the options pane
			 */
			labelOptionsTitle.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","title"});
			//Basic display mode settings
			groupboxDisplayMode.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_mode"});
			radiobuttonWindow.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_mode_window"});
			radiobuttonFullscreen.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_mode_fullscreen"});
			labelVSync.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_vsync"});
			comboboxVSync.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_vsync_off"});
			comboboxVSync.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_vsync_on"});
			labelHUDScale.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","hud_size"});
			labelHUDSmall.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","hud_size_small"});
			labelHUDNormal.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","hud_size_normal"});
			labelHUDLarge.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","hud_size_large"});
			labelFontName.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","font"});
			//Windowed Mode
			groupboxWindow.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_window"});
			labelWindowWidth.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_window_width"});
			labelWindowHeight.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_window_height"});
			//Fullscreen
			groupboxFullscreen.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_fullscreen"});
			labelFullscreenWidth.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_fullscreen_width"});
			labelFullscreenHeight.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_fullscreen_height"});
			labelFullscreenBits.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","display_fullscreen_bits"});
			//Interpolation, AA and AF
			groupboxInterpolation.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation"});
			labelInterpolation.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode"});
			comboboxInterpolation.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearest"});
			comboboxInterpolation.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinear"});
			comboboxInterpolation.Items[2] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_nearestmipmap"});
			comboboxInterpolation.Items[3] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_bilinearmipmap"});
			comboboxInterpolation.Items[4] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_trilinearmipmap"});
			comboboxInterpolation.Items[5] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_mode_anisotropic"});
			labelAnisotropic.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_anisotropic_level"});
			labelAntiAliasing.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_antialiasing_level"});
			labelTransparency.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_transparency"});
			labelTransparencyPerformance.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_transparency_sharp"});
			labelTransparencyQuality.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_interpolation_transparency_smooth"});
			groupboxDistance.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance"});
			//Viewing distance and motion blur
			labelDistance.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_viewingdistance"});
			labelDistanceUnit.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_viewingdistance_meters"});
			labelMotionBlur.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_motionblur"});
			comboboxMotionBlur.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_motionblur_none"});
			comboboxMotionBlur.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_motionblur_low"});
			comboboxMotionBlur.Items[2] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_motionblur_medium"});
			comboboxMotionBlur.Items[3] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_motionblur_high"});
			labelMotionBlur.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","quality_distance_motionblur"});
			//Simulation
			groupboxSimulation.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_simulation"});
			checkboxToppling.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_simulation_toppling"});
			checkboxCollisions.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_simulation_collisions"});
			checkboxDerailments.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_simulation_derailments"});
			checkboxBlackBox.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_simulation_blackbox"});
			checkBoxLoadingSway.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_simulation_loadingsway"});
			//Controls
			groupboxControls.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_controls"});
			checkboxJoysticksUsed.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_controls_joysticks"});
			checkBoxEBAxis.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_controls_ebaxis"});
			labelJoystickAxisThreshold.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_controls_threshold"});
			//Sound
			groupboxSound.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_sound"});
			labelSoundNumber.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","misc_sound_number"});
			//Verbosity
			groupboxVerbosity.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","verbosity"});
			checkboxWarningMessages.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","verbosity_warningmessages"});
			checkboxErrorMessages.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","verbosity_errormessages"});
			checkBoxAccessibility.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","verbosity_accessibilityaids"});
			//Advanced Options
			groupBoxAdvancedOptions.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","advanced"});
			checkBoxLoadInAdvance.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","advanced_load_advance"});
			checkBoxUnloadTextures.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","advanced_unload_textures"});
			checkBoxIsUseNewRenderer.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","advanced_is_use_new_renderer"});
			labelTimeAcceleration.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","advanced_timefactor"});
			labelCursor.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","advanced_cursor"});
			//Other Options
			groupBoxOther.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","other"});
			labelTimeTableDisplayMode.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","other_timetable_mode"});
			comboBoxTimeTableDisplayMode.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","other_timetable_mode_none"});
			comboBoxTimeTableDisplayMode.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","other_timetable_mode_default"});
			comboBoxTimeTableDisplayMode.Items[2] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","other_timetable_mode_autogenerated"});
			comboBoxTimeTableDisplayMode.Items[3] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","other_timetable_mode_prefercustom"});
			//Options Page
			buttonOptionsPrevious.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","page_previous"});
			buttonOptionsNext.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","page_next"});
			checkBoxPanel2Extended.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","panel2_extended"});
			/*
			 * Options Page 2
			 */
			//Package directories
			groupBoxPackageOptions.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","packages"});
			buttonSetRouteDirectory.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","package_choose"});
			buttonTrainInstallationDirectory.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","package_choose"});
			buttonOtherDirectory.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","package_choose"});
			textBoxRouteDirectory.Text = Program.FileSystem.RouteInstallationDirectory;
			textBoxTrainDirectory.Text = Program.FileSystem.TrainInstallationDirectory;
			textBoxOtherDirectory.Text = Program.FileSystem.OtherInstallationDirectory;
			textBoxMSTSTrainsetDirectory.Text = Program.FileSystem.MSTSDirectory;
			labelRouteInstallDirectory.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","package_route_directory"});
			labelTrainInstallDirectory.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","package_train_directory"});
			labelOtherInstallDirectory.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","package_other_directory"});
			labelOtherInstallDirectory.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "options", "package_msts_directory" });
			labelPackageCompression.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","package_compression"});
			//Kiosk Mode
			groupBoxKioskMode.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","kiosk_mode"});
			checkBoxEnableKiosk.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","kiosk_mode_enable"});
			labelKioskTimeout.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","kiosk_mode_timer"});
			labelRailDriverSpeedUnits.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","speedunits"});
			comboBoxRailDriverUnits.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","milesperhour"});
			comboBoxRailDriverUnits.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","kilometersperhour"});
			labelRailDriverCalibration.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","setcalibration"});
			buttonRailDriverCalibration.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","launch"});
			checkBoxTransparencyFix.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","transparencyfix"});
			checkBoxHacks.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","hacks_enable"});
			//Object Parser Options
			groupBoxObjectParser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","object_parser"});
			labelXparser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","xobject_parser"});
			labelObjparser.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","objobject_parser"});
			//Input Device
			groupBoxInputDevice.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin"});
			labelInputDevice.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_warning"});
			listviewInputDevice.Columns[0].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_name"});
			listviewInputDevice.Columns[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_status"});
			listviewInputDevice.Columns[2].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_version"});
			listviewInputDevice.Columns[3].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_provider"});
			listviewInputDevice.Columns[4].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_file_name"});
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
			checkBoxInputDeviceEnable.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_switch"});
			buttonInputDeviceConfig.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","input_device_plugin_config"});
			/*
			 * Localisation for strings in the game start pane
			 */
			labelStartTitle.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","title"});
			labelRoute.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route"});
			groupboxRouteSelection.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_selection"});
			tabpageRouteBrowse.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_browse"});
			tabpageRouteRecently.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_recently"});
			tabPageRoutePackages.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list"});
			groupboxRouteDetails.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_details"});
			tabpageRouteDescription.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_description"});
			tabpageRouteMap.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_map"});
			tabpageRouteGradient.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_gradient"});
			tabpageRouteSettings.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_settings"});
			labelRouteEncoding.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_settings_encoding"});
			comboboxRouteEncoding.Items[0] = @"(UTF-8)";
			labelRouteEncodingPreview.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","route_settings_encoding_preview"});
			labelTrain.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train"});
			groupboxTrainSelection.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_selection"});
			tabpageTrainBrowse.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_browse"});
			tabpageTrainRecently.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_recently"});
			tabpageTrainDefault.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_default"});
			tabPageTrainPackages.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list"});
			checkboxTrainDefault.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_usedefault"});
			groupboxTrainDetails.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_details"});
			tabpageTrainDescription.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_description"});
			tabpageTrainSettings.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_settings"});
			labelTrainEncoding.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_settings_encoding"});
			labelReverseConsist.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_settings_reverseconsist"});
			comboboxTrainEncoding.Items[0] = @"(UTF-8)";
			labelTrainEncodingPreview.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","train_settings_encoding_preview"});
			labelStart.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","start"});
			labelMode.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","start_mode"});
			buttonStart.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"start","start_start"});
			comboboxMode.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","arcade"});
			comboboxMode.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","normal"});
			comboboxMode.Items[2] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","expert"});
			labelCompatibilitySignalSet.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"options","compatibility_signals"});
			/*
			 * Localisation for strings in the game review pane
			 */
			labelReviewTitle.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","title"});
			labelConditions.Text = @" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions"});
			groupboxReviewRoute.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions_route"});
			labelReviewRouteCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions_route_file"});
			groupboxReviewTrain.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions_train"});
			labelReviewTrainCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions_train_folder"});
			groupboxReviewDateTime.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions_datetime"});
			labelReviewDateCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions_datetime_date"});
			labelReviewTimeCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","conditions_datetime_time"});
			labelScore.Text = @" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score"});
			groupboxRating.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_rating"});
			labelRatingModeCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_rating_mode"});
			switch (Interface.CurrentOptions.PreviousGameMode)
			{
				case GameMode.Arcade: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","arcade"}); break;
				case GameMode.Normal: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","normal"}); break;
				case GameMode.Expert: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","expert"}); break;
				default: labelRatingModeValue.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"mode","unkown"}); break;
			}
			{
				double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.CurrentValue / Game.CurrentScore.Maximum;
				if (ratio < 0.0) ratio = 0.0;
				if (ratio > 1.0) ratio = 1.0;
				int index = (int)Math.Floor(ratio * Translations.RatingsCount);
				if (index >= Translations.RatingsCount) index = Translations.RatingsCount - 1;
				labelRatingDescription.Text = Game.CurrentScore.Maximum == 0 ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"rating","unknown"}) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"rating",index.ToString(System.Globalization.CultureInfo.InvariantCulture)});
			}
			labelRatingAchievedCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_rating_achieved"});
			labelRatingMaximumCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_rating_maximum"});
			labelRatingRatioCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_rating_ratio"});
			groupboxScore.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log"});
			listviewScore.Columns[0].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log_list_time"});
			listviewScore.Columns[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log_list_position"});
			listviewScore.Columns[2].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log_list_value"});
			listviewScore.Columns[3].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log_list_cumulative"});
			listviewScore.Columns[4].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log_list_reason"});
			ShowScoreLog(checkboxScorePenalties.Checked);
			checkboxScorePenalties.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log_penalties"});
			buttonScoreExport.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","score_log_export"});
			labelBlackBox.Text = @" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","blackbox"});
			labelBlackBoxFormat.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","blackbox_format"});
			comboboxBlackBoxFormat.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","blackbox_format_csv"});
			comboboxBlackBoxFormat.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","blackbox_format_text"});
			buttonBlackBoxExport.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"review","blackbox_export"});
			/*
			 * Localisation for strings related to controls (Keyboard etc.)
			 */
			for (int i = 0; i < listviewControls.SelectedItems.Count; i++)
			{
				listviewControls.SelectedItems[i].Selected = false;
			}
			labelControlsTitle.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","title"});
			listviewControls.Columns[0].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_command"});
			listviewControls.Columns[1].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_type"});
			listviewControls.Columns[2].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_description"});
			listviewControls.Columns[3].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_assignment"});
			listviewControls.Columns[4].Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","list_option"});
			buttonControlAdd.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","add"});
			buttonControlRemove.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","remove"});
			buttonControlsImport.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","import"});
			buttonControlsExport.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","export"});
			buttonControlReset.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","reset"});
			buttonControlUp.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","up"});
			buttonControlDown.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","down"});
			groupboxControl.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection"});
			labelCommand.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_command"});
			labelCommandOption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_command_option"});
			radiobuttonKeyboard.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard"});
			labelKeyboardKey.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_key"});
			labelKeyboardModifier.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_modifiers"});
			//Load text for SHIFT modifier
			checkboxKeyboardShift.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_modifiers_shift"});
			//Shift CTRL
			checkboxKeyboardCtrl.Location = new Point(checkboxKeyboardShift.Location.X + (checkboxKeyboardShift.Text.Length + 5) * 5, checkboxKeyboardCtrl.Location.Y);
			//Load text for CTRL modifier
			checkboxKeyboardCtrl.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_modifiers_ctrl"});
			//Shift ALT to suit
			checkboxKeyboardAlt.Location = new Point(checkboxKeyboardCtrl.Location.X + (checkboxKeyboardCtrl.Text.Length + 5) * 5, checkboxKeyboardAlt.Location.Y);
			
			checkboxKeyboardAlt.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_modifiers_alt"});
			radiobuttonJoystick.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_joystick"});
			labelJoystickAssignmentCaption.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_joystick_assignment"});
			textboxJoystickGrab.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","selection_keyboard_assignment_grab"});
			groupboxJoysticks.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"controls","attached"});
			{
				listviewControls.Items.Clear();
				comboboxCommand.DataSource = new BindingSource(Translations.CommandInfos, null);
				comboboxCommand.DisplayMember = "Value";
				comboboxCommand.ValueMember = "Key";

				comboboxKeyboardKey.DataSource = new BindingSource(Translations.TranslatedKeys, null);
				comboboxKeyboardKey.DisplayMember = "Value";
				comboboxKeyboardKey.ValueMember = "Key";
				
				ListViewItem[] listItems = new ListViewItem[Interface.CurrentControls.Length];
				for (int i = 0; i < Interface.CurrentControls.Length; i++)
				{
					listItems[i] = new ListViewItem(new[] { "", "", "", "", "" });
					UpdateControlListElement(listItems[i], i, false);
				}
				listviewControls.Items.AddRange(listItems);
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
			/*
			 * Localisation for strings in package management display
			 * 
			 */
			//Navigation buttons
			buttonBack.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_back"});
			buttonCreatePackage.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_create"});
			buttonBack2.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_back"});
			buttonNext.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_next"});
			buttonCancel.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_cancel"});
			buttonProceedAnyway1.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_install"});
			buttonCancel2.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_cancel"});
			buttonCreateProceed.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_next"});
			buttonAbort.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_abort"});
			buttonProceedAnyway.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_ignore"});
			//Main display tab
			labelPackagesTitle.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","title"});
			labelInstalledPackages.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list"});
			labelPackageListType.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_type"});
			buttonInstallPackage.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_button"});
			buttonUninstallPackage.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","uninstall_button"});
			createPackageButton.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_button"});
			comboBoxPackageType.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_route"});
			comboBoxPackageType.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_train"});
			comboBoxPackageType.Items[2] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_other"});
			routeName.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_name"});
			routeVersion.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_version"});
			routeAuthor.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_author"});
			routeWebsite.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_website"});
			//Creation tab 1
			labelPackageCreationHeader.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_header"});
			SaveFileNameButton.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_saveas_button"});
			labelSaveAs.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_saveas_label"});
			labelDependanciesNextStep.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_dependancies_nextstep"});
			newPackageClearSelectionButton.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_clearselection"});
			addPackageItemsButton.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_additems"});
			labelSelectFiles.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_selecteditems"});
			labelNewGUID.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_new_guid"});
			dataGridViewTextBoxColumn21.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_name"});
			dataGridViewTextBoxColumn22.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_version"});
			dataGridViewTextBoxColumn23.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_author"});
			dataGridViewTextBoxColumn24.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_website"});
			//Replace package panel of creation tab
			replacePackageButton.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","replace_select"});
			packageToReplaceLabel.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","replace_choose"});
			//New package panel
			radioButtonQ2Other.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_other"});
			radioButtonQ2Route.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_route"});
			radioButtonQ2Train.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_train"});
			labelPackageType.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_select"});
			labelReplacePackage.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_replace"});
			radioButtonQ1Yes.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_yes"});
			radioButtonQ1No.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_no"});
			//Please wait tab
			labelPleaseWait.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","processing"});
			labelProgressFile.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","unknown_file"});
			//Missing dependancies tab
			/*
			 * NOTE: THIS TAB IS MULTI-FUNCTIONAL, AND MAY BE UPDATED AT RUNTIME
			 * REMEMBER TO RESET AFTERWARDS
			 * 
			 */
			labelMissingDependanciesText1.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_dependancies_unmet"});
			labelMissingDependanciesText2.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","shownlist"});
			labelDependancyErrorHeader.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_dependancies_unmet_header"});
			//Install tab
			/*
			 * NOTE: THIS TAB IS MULTI-FUNCTIONAL, AND THE HEADER MAY BE UPDATED AT RUNTIME
			 * REMEMBER TO RESET AFTERWARDS
			 * 
			 */
			labelPackageName.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_name"});
			labelPackageAuthor.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_author"});
			labelPackageVersion.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_version"});
			labelPackageWebsite.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_website"});
			labelPackageDescription.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_description"});

			//Add dependancies panel
			labelDependanciesHeader.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_dependancies"});
			labelInstalledDependancies.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list"});
			labelSelectedDependencies.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","selected"});
			labelDependancyType.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_type"});
			comboBoxDependancyType.Items[0] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_route"});
			comboBoxDependancyType.Items[1] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_train"});
			comboBoxDependancyType.Items[2] = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","type_other"});
			buttonDepends.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_dependancies_add"});
			buttonReccomends.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_reccommends_add"});
			dataGridViewTextBoxColumn13.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_name"});
			dataGridViewTextBoxColumn14.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_version"});
			dataGridViewTextBoxColumn15.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_author"});
			dataGridViewTextBoxColumn16.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_website"});
			dataGridViewTextBoxColumn1.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_name"});
			dataGridViewTextBoxColumn2.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_minimum"});
			dataGridViewTextBoxColumn3.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_maximum"});
			dataGridViewTextBoxColumn4.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_packagetype"});
			buttonRemove.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_dependancies_remove"});
			labelNoDependencyReminder.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","creation_dependancies_skip_if_none"});
			website.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_website"});
			//Version Error panel
			labelBrokenDependancies.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_dependancies_broken"});
			labelNewVersion.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","version_new"});
			labelCurrentVersion.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","version_current"});
			dataGridViewTextBoxColumn5.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_name"});
			dataGridViewTextBoxColumn6.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_maximum"});
			dataGridViewTextBoxColumn7.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_minimum"});
			dataGridViewTextBoxColumn8.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_author"});
			website.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_website"});
			groupBoxVersionErrorAction.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","error_action"});
			radioButtonOverwrite.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","error_overwrite"});
			radioButtonReplace.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","error_replace"});
			// *** labelVersionError.Text is set dynamically at runtime ***
			labelVersionErrorHeader.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","install_version_error"});
			dataGridViewTextBoxColumn9.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_name"});
			dataGridViewTextBoxColumn10.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_version"});
			dataGridViewTextBoxColumn11.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_author"});
			dataGridViewTextBoxColumn12.HeaderText = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","list_website"});
			//Please Wait panel
			labelPleaseWait.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","processing"});
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
			int newViewingDistance = (int)Math.Round(updownDistance.Value);
			if (newViewingDistance != Interface.CurrentOptions.ViewingDistance)
			{
				Interface.CurrentOptions.ViewingDistance = newViewingDistance;
				Interface.CurrentOptions.QuadTreeLeafSize = Math.Max(50, (int)Math.Ceiling(Interface.CurrentOptions.ViewingDistance / 10.0d) * 10); // quad tree size set to 10% of viewing distance to the nearest 10
			}
			
			
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
			Interface.CurrentOptions.JoystickAxisThreshold = (trackbarJoystickAxisThreshold.Value - (double)trackbarJoystickAxisThreshold.Minimum) / (trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum);
			Interface.CurrentOptions.SoundNumber = (int)Math.Round(updownSoundNumber.Value);
			Interface.CurrentOptions.ShowWarningMessages = checkboxWarningMessages.Checked;
			Interface.CurrentOptions.ShowErrorMessages = checkboxErrorMessages.Checked;
			Interface.CurrentOptions.RouteFolder = textboxRouteFolder.Text;
			Interface.CurrentOptions.TrainFolder = textboxTrainFolder.Text;
			Interface.CurrentOptions.MainMenuWidth = WindowState == FormWindowState.Maximized ? -1 : Size.Width;
			Interface.CurrentOptions.MainMenuHeight = WindowState == FormWindowState.Maximized ? -1 : Size.Height;
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
					if (File.Exists(Interface.CurrentOptions.RecentlyUsedRoutes[i]))
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
					if ((Interface.CurrentOptions.RecentlyUsedTrains[i].EndsWith(".con", StringComparison.InvariantCultureIgnoreCase) && File.Exists(Interface.CurrentOptions.RecentlyUsedTrains[i])) || Directory.Exists(Interface.CurrentOptions.RecentlyUsedTrains[i]))
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
					if (File.Exists(Interface.CurrentOptions.RouteEncodings[i].Value))
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
					if (Directory.Exists(Interface.CurrentOptions.TrainEncodings[i].Value))
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
				List<string> a = new List<string>();
				for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
				{
					if (InputDevicePlugin.AvailablePluginInfos[i].Status != InputDevicePlugin.PluginInfo.PluginStatus.Enable) {
						continue;
					}
					string pluginPath = Path.CombineFile(Program.FileSystem.GetDataFolder("InputDevicePlugins"), InputDevicePlugin.AvailablePluginInfos[i].FileName);
					if (File.Exists(pluginPath))
					{
						a.Add(InputDevicePlugin.AvailablePluginInfos[i].FileName);
					}
				}
				Interface.CurrentOptions.EnabledInputDevicePlugins = a.ToArray();
			}
			// Unload input device plugins if we're closing the program
			if (!Result.Start)
			{
				for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
				{
					InputDevicePlugin.CallPluginUnload(i);
				}
			}
			Program.Sounds.DeInitialize();
			DisposePreviewRouteThread();
			{
				Program.CurrentHost.UnloadPlugins(out _);
			}

			routeWatcher.EnableRaisingEvents = false;
			trainWatcher.EnableRaisingEvents = false;
			if (Program.CurrentHost.Platform != HostPlatform.AppleOSX && Program.CurrentHost.Platform != HostPlatform.FreeBSD)
			{
				// A FileSystemWatcher may crash when disposed as the game is closing (without launching a route) on these platforms
				// This is a Mono issue
				routeWatcher.Dispose();
				trainWatcher.Dispose();
			}
			workerThread.Dispose();
			// finish
#if !DEBUG
			try
			{
#endif
				Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg"));
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
			catch
			{
				// Ignored
			}

			try
			{
				int width = Math.Min((panelOptions.Width - 24) / 2, 420);
				panelOptionsLeft.Width = width;
				panelOptionsRight.Left = panelOptionsLeft.Left + width + 8;
				panelOptionsRight.Width = width;
			}
			catch
			{
				// Ignored
			}

			try
			{
				int width = Math.Min((panelReview.Width - 32) / 3, 360);
				groupboxReviewRoute.Width = width;
				groupboxReviewTrain.Left = groupboxReviewRoute.Left + width + 8;
				groupboxReviewTrain.Width = width;
				groupboxReviewDateTime.Left = groupboxReviewTrain.Left + width + 8;
				groupboxReviewDateTime.Width = width;
			}
			catch
			{
				// Ignored
			}
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
			if (WindowState != FormWindowState.Maximized)
			{
				System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromControl(this);
				if (Width >= 0.95 * s.WorkingArea.Width | Height >= 0.95 * s.WorkingArea.Height)
				{
					WindowState = FormWindowState.Maximized;
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
			pictureboxJoysticks.Visible = false;
			UpdatePanelColor();
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
			pictureboxJoysticks.Visible = false;
			UpdatePanelColor();

			//HACK: Column Header won't appear in Mono without resizing it...
			listviewScore.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
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
			pictureboxJoysticks.Visible = true;
			UpdatePanelColor();
			//HACK: Column Header in list view won't appear in Mono without resizing it...
			listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
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
			pictureboxJoysticks.Visible = false;
			UpdatePanelColor();
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
			pictureboxJoysticks.Visible = false;
			UpdatePanelColor();
			//Load packages & rest panel states
			if (radioButtonPackages.Checked)
			{
				ResetInstallerPanels();
				if (Database.LoadDatabase(Program.FileSystem.PackageDatabaseFolder, currentDatabaseFile, out string[] errorMessage))
				{
					PopulatePackageList(Database.currentDatabase.InstalledRoutes, dataGridViewPackages, true, false, false);
				}
				if (errorMessage.Length != 0)
				{
					MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, errorMessage));
				}
				comboBoxPackageType.SelectedIndex = 0;
			}
		}

		private void UpdatePanelColor() {
			if(panelStart.Visible) {
				panelPanels.BackColor = labelStartTitle.BackColor;
				radiobuttonStart.BackColor = Color.White;
			} else {
				radiobuttonStart.BackColor = Color.LightGray;
			}

			if (panelReview.Visible) {
				panelPanels.BackColor = labelReviewTitle.BackColor;
				radiobuttonReview.BackColor = Color.White;
			} else {
				radiobuttonReview.BackColor = Color.LightGray;
			}

			if (panelControls.Visible) {
				panelPanels.BackColor = labelControlsTitle.BackColor;
				radiobuttonControls.BackColor = Color.White;
			} else {
				radiobuttonControls.BackColor = Color.LightGray;
			}

			if (panelOptions.Visible) {
				panelPanels.BackColor = labelOptionsTitle.BackColor;
				radiobuttonOptions.BackColor = Color.White;
			} else {
				radiobuttonOptions.BackColor = Color.LightGray;
			}

			if (panelPackages.Visible) {
				panelPanels.BackColor = labelPackagesTitle.BackColor;
				radioButtonPackages.BackColor = Color.White;
			} else {
				radioButtonPackages.BackColor = Color.LightGray;
			}
		}

		/// <summary>Launches a web-browser linked to the project homepage</summary>
		private void linkHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				Process.Start("http://openbve-project.net");
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
			previewRouteResultQueue.CompleteAdding();
			if (sender != null)
			{
				//Don't cause an infinite loop
				Close();
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
			if (radiobuttonJoystick.Checked && textboxJoystickGrab.Focused && Tag == null && listviewControls.SelectedIndices.Count == 1)
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
		private Image LoadImage(string imageFolder, string fileName)
		{
			try
			{
				fileName = Path.CombineFile(imageFolder, fileName);
				if (File.Exists(fileName))
				{
					try
					{
						return ImageExtensions.FromFile(fileName);
					}
					catch
					{
						return null;
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
		private void TryLoadImage(PictureBox pictureBox, string imageFile)
		{
			try
			{
				if (!File.Exists(imageFile))
				{
					string menuFolder = Program.FileSystem.GetDataFolder("Menu");
					imageFile = Path.CombineFile(menuFolder, imageFile);
				}
				if (File.Exists(imageFile))
				{
					FileInfo f = new FileInfo(imageFile);
					if (f.Length == 0)
					{
						pictureBox.Image = pictureBox.ErrorImage;
						return;
					}
					try
					{
						pictureBox.Image = ImageExtensions.FromFile(imageFile);
						return;
					}
					catch
					{
						pictureBox.Image = pictureBox.ErrorImage;
						return;
					}
				}
				pictureBox.Image = pictureBox.ErrorImage;
			}
			catch
			{
				pictureBox.Image = pictureBox.ErrorImage;
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

		private void CheckForUpdate()
		{
			string xmlUrl = Interface.CurrentOptions.DailyBuildUpdates ? "https://vps.bvecornwall.co.uk/OpenBVE/Builds/version.xml" : "http://openbve-project.net/version.xml";
			HttpWebRequest hwRequest = (HttpWebRequest)WebRequest.Create(xmlUrl);
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
										if (!Interface.CurrentOptions.DailyBuildUpdates)
										{
											newVersion = new Version(reader.Value);
										}
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
			catch (WebException)
			{
				//The internet connection is broken.....
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates_invalid"}), Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates"}), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			finally
			{
				reader?.Close();
				hwResponse?.Close();
			}
			Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			if (url == null)
			{
				return;
			}
			if (Interface.CurrentOptions.DailyBuildUpdates)
			{
				string question = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates_daily"});
				question = question.Replace("[date]", date);
				if (DialogResult.OK == MessageBox.Show(this, question, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates"}), MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
				{
					Process.Start(url);
				}
			}
			else
			{
				bool newerVersion = curVersion.CompareTo(newVersion) < 0;
				if (newerVersion)
				{
					string question = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates_new"});
					question = question.Replace("[version]", newVersion.ToString());
					question = question.Replace("[date]", date);
					if (DialogResult.OK == MessageBox.Show(this, question, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates"}), MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
					{
						Process.Start(url);
					}
				}
				else
				{
					MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel","updates_old"}));
				}
			}
			
		}

		private void aboutLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (FormAbout f = new FormAbout()) {
				f.ShowDialog();
			}
		}

		private void linkLabelCheckUpdates_Click(object sender, EventArgs e)
		{
			CheckForUpdate();
		}

		private void buttonOptionsPrevious_Click(object sender, EventArgs e)
		{
			if (panelOptionsLeft.Visible)
			{
				panelOptionsLeft.Hide();
				panelOptionsRight.Hide();
				panelOptionsPage2.Show();
				//HACK: Column Header in list view won't appear in Mono without resizing it...
				listviewInputDevice.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
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
			using (FormRaildriverCalibration f = new FormRaildriverCalibration())
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

		private void tabcontrolRouteDetails_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Program.CurrentHost.MonoRuntime) {
				// MONO issue on some systems means that the map may not draw initially, so force redraw
				pictureboxRouteMap.Invalidate();
				// HACK: On some mono systems, the preview would not appear if it's in the base resolution.
				// If so, resize the image height by 1px
				if(pictureboxRouteMap.Image != null) {
					if (pictureboxRouteMap.Image.Size == pictureboxRouteMap.Size) {
						pictureboxRouteMap.Height += 1;
					}
				}

				if (pictureboxRouteGradient.Image != null) {
					if (pictureboxRouteGradient.Image.Size == pictureboxRouteGradient.Size) {
						pictureboxRouteGradient.Height += 1;
					}
				}
			}
		}
		
		private void toolStripExport_Click(object sender, EventArgs e)
		{
			Control sourceControl = ((ContextMenuStrip)((ToolStripItem)sender).Owner).SourceControl;
			FormImageExport exporter = sourceControl == pictureboxRouteMap ? new FormImageExport(true, Result.RouteFile) : new FormImageExport(false, Result.RouteFile);
			
			exporter.ShowDialog();
		}

		private void panelPackageInstall_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}

		private void textboxTrainDescription_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}

		private void buttonMSTSTrainsetDirectory_Click(object sender, EventArgs e)
		{
			using (var folderSelectDialog = new FolderBrowserDialog())
			{
				if (folderSelectDialog.ShowDialog() == DialogResult.OK)
				{
					Program.FileSystem.MSTSDirectory = folderSelectDialog.SelectedPath;
					textBoxMSTSTrainsetDirectory.Text = folderSelectDialog.SelectedPath;
				}
			}
		}
	}
}
