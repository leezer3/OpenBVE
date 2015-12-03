using System;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
    internal static partial class MainLoop
    {
        /// <summary>The ProcessControls function should be called once a frame, and updates the simulation accordingly</summary>
        /// <param name="TimeElapsed">The time elapsed in ms since the last call to this function</param>
        internal static void ProcessControls(double TimeElapsed)
        {
            //If we are currently blocking key repeat events from firing, return
            if (BlockKeyRepeat) return;
            switch (Game.CurrentInterface)
            {
                case Game.InterfaceType.Pause:
                    // pause
                    for (int i = 0; i < Interface.CurrentControls.Length; i++)
                    {
                        if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital)
                        {
                            if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
                            {
                                Interface.CurrentControls[i].DigitalState =
                                    Interface.DigitalControlState.PressedAcknowledged;
                                switch (Interface.CurrentControls[i].Command)
                                {
                                    case Interface.Command.MiscPause:
                                        Game.CurrentInterface = Game.InterfaceType.Normal;
                                        break;
                                    case Interface.Command.MenuActivate:
                                        Game.CreateMenu(false);
                                        Game.CurrentInterface = Game.InterfaceType.Menu;
                                        break;
                                    case Interface.Command.MiscQuit:
                                        Game.CreateMenu(true);
                                        Game.CurrentInterface = Game.InterfaceType.Menu;
                                        break;
                                    case Interface.Command.MiscFullscreen:
                                        Screen.ToggleFullscreen();
                                        break;
                                    case Interface.Command.MiscMute:
                                        Sounds.GlobalMute = !Sounds.GlobalMute;
                                        Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case Game.InterfaceType.Menu:
                    // menu
                    for (int i = 0; i < Interface.CurrentControls.Length; i++)
                    {
                        if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital)
                        {
                            if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
                            {
                                Interface.CurrentControls[i].DigitalState =
                                    Interface.DigitalControlState.PressedAcknowledged;
                                switch (Interface.CurrentControls[i].Command)
                                {
                                    case Interface.Command.MenuUp:
                                    {
                                        // up
                                        Game.MenuEntry[] a = Game.CurrentMenu;
                                        int j = Game.CurrentMenuSelection.Length - 1;
                                        int k = 0;
                                        while (k < j)
                                        {
                                            Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
                                            if (b == null) break;
                                            a = b.Entries;
                                            k++;
                                        }
                                        if (Game.CurrentMenuSelection[j] > 0 &&
                                            !(a[Game.CurrentMenuSelection[j] - 1] is Game.MenuCaption))
                                        {
                                            Game.CurrentMenuSelection[j]--;
                                        }
                                    }
                                        break;
                                    case Interface.Command.MenuDown:
                                    {
                                        // down
                                        Game.MenuEntry[] a = Game.CurrentMenu;
                                        int j = Game.CurrentMenuSelection.Length - 1;
                                        int k = 0;
                                        while (k < j)
                                        {
                                            Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
                                            if (b == null) break;
                                            a = b.Entries;
                                            k++;
                                        }
                                        if (Game.CurrentMenuSelection[j] < a.Length - 1)
                                        {
                                            Game.CurrentMenuSelection[j]++;
                                        }
                                    }
                                        break;
                                    case Interface.Command.MenuEnter:
                                    {
                                        // enter
                                        Game.MenuEntry[] a = Game.CurrentMenu;
                                        int j = Game.CurrentMenuSelection.Length - 1;
                                        {
                                            int k = 0;
                                            while (k < j)
                                            {
                                                Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
                                                if (b == null) break;
                                                a = b.Entries;
                                                k++;
                                            }
                                        }
                                        if (a[Game.CurrentMenuSelection[j]] is Game.MenuCommand)
                                        {
                                            // command
                                            Game.MenuCommand b = (Game.MenuCommand) a[Game.CurrentMenuSelection[j]];
                                            switch (b.Tag)
                                            {
                                                case Game.MenuTag.Back:
                                                    // back
                                                    if (Game.CurrentMenuSelection.Length <= 1)
                                                    {
                                                        Game.CurrentInterface = Game.InterfaceType.Normal;
                                                    }
                                                    else
                                                    {
                                                        Array.Resize<int>(ref Game.CurrentMenuSelection,
                                                            Game.CurrentMenuSelection.Length - 1);
                                                        Array.Resize<double>(ref Game.CurrentMenuOffsets,
                                                            Game.CurrentMenuOffsets.Length - 1);
                                                    }
                                                    break;
                                                case Game.MenuTag.JumpToStation:
                                                    // jump to station
                                                    TrainManager.JumpTrain(TrainManager.PlayerTrain, b.Data);
                                                    break;
                                                case Game.MenuTag.ExitToMainMenu:
                                                    Program.RestartArguments = Interface.CurrentOptions.GameMode ==
                                                                               Interface.GameMode.Arcade
                                                        ? "/review"
                                                        : "";
                                                    Quit = true;
                                                    break;
                                                case Game.MenuTag.Quit:
                                                    // quit
                                                    Quit = true;
                                                    break;
                                            }
                                        }
                                        else if (a[Game.CurrentMenuSelection[j]] is Game.MenuSubmenu)
                                        {
                                            // menu
                                            Game.MenuSubmenu b = (Game.MenuSubmenu) a[Game.CurrentMenuSelection[j]];
                                            int n = Game.CurrentMenuSelection.Length;
                                            Array.Resize<int>(ref Game.CurrentMenuSelection, n + 1);
                                            Array.Resize<double>(ref Game.CurrentMenuOffsets, n + 1);
                                            /* Select the first non-caption entry. */
                                            int selection;
                                            for (selection = 0; selection < b.Entries.Length; selection++)
                                            {
                                                if (!(b.Entries[selection] is Game.MenuCaption)) break;
                                            }
                                            /* Select the next station if this menu has stations in it. */
                                            int station = TrainManager.PlayerTrain.LastStation;
                                            if (TrainManager.PlayerTrain.Station == -1 |
                                                TrainManager.PlayerTrain.StationState !=
                                                TrainManager.TrainStopState.Pending)
                                            {
                                                for (int k = station + 1; k < Game.Stations.Length; k++)
                                                {
                                                    if (Game.StopsAtStation(k, TrainManager.PlayerTrain))
                                                    {
                                                        station = k;
                                                        break;
                                                    }
                                                }
                                            }
                                            for (int k = selection + 1; k < b.Entries.Length; k++)
                                            {
                                                Game.MenuCommand c = b.Entries[k] as Game.MenuCommand;
                                                if (c != null && c.Tag == Game.MenuTag.JumpToStation)
                                                {
                                                    if (c.Data <= station)
                                                    {
                                                        selection = k;
                                                    }
                                                }
                                            }
                                            Game.CurrentMenuSelection[n] = selection < b.Entries.Length ? selection : 0;
                                            Game.CurrentMenuOffsets[n] = double.NegativeInfinity;
                                            a = b.Entries;
                                            for (int h = 0; h < a.Length; h++)
                                            {
                                                a[h].Highlight = h == 0 ? 1.0 : 0.0;
                                                a[h].Alpha = 0.0;
                                            }
                                        }
                                    }
                                        break;
                                    case Interface.Command.MenuBack:
                                        // back
                                        if (Game.CurrentMenuSelection.Length <= 1)
                                        {
                                            //Game.CurrentInterface = Game.InterfaceType.Normal;
                                        }
                                        else
                                        {
                                            Array.Resize<int>(ref Game.CurrentMenuSelection,
                                                Game.CurrentMenuSelection.Length - 1);
                                            Array.Resize<double>(ref Game.CurrentMenuOffsets,
                                                Game.CurrentMenuOffsets.Length - 1);
                                        }
                                        break;
                                    case Interface.Command.MiscFullscreen:
                                        // fullscreen
                                        Screen.ToggleFullscreen();
                                        break;
                                    case Interface.Command.MiscMute:
                                        // mute
                                        Sounds.GlobalMute = !Sounds.GlobalMute;
                                        Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
                                        break;

                                }
                            }
                        }
                    }
                    break;
                case Game.InterfaceType.Normal:
                    // normal
                    for (int i = 0; i < Interface.CurrentControls.Length; i++)
                    {
                        if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogHalf |
                            Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogFull)
                        {
                            // analog control
                            if (Interface.CurrentControls[i].AnalogState != 0.0)
                            {
                                switch (Interface.CurrentControls[i].Command)
                                {
                                    case Interface.Command.PowerHalfAxis:
                                    case Interface.Command.PowerFullAxis:
                                        // power half/full-axis
                                        if (!TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            double a = Interface.CurrentControls[i].AnalogState;
                                            if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis)
                                            {
                                                a = 0.5*(a + 1.0);
                                            }
                                            a *= (double) TrainManager.PlayerTrain.Specs.MaximumPowerNotch;
                                            int p = (int) Math.Round(a);
                                            TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, 0, true);
                                        }
                                        break;
                                    case Interface.Command.BrakeHalfAxis:
                                    case Interface.Command.BrakeFullAxis:
                                        // brake half/full-axis
                                        if (!TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int d = TrainManager.PlayerTrain.DriverCar;
                                            if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType ==
                                                TrainManager.CarBrakeType.AutomaticAirBrake)
                                            {
                                                double a = Interface.CurrentControls[i].AnalogState;
                                                if (Interface.CurrentControls[i].Command ==
                                                    Interface.Command.BrakeFullAxis)
                                                {
                                                    a = 0.5*(a + 1.0);
                                                }
                                                int b = (int) Math.Round(3.0*a);
                                                switch (b)
                                                {
                                                    case 0:
                                                        TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver =
                                                            false;
                                                        TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                            TrainManager.AirBrakeHandleState.Release);
                                                        break;
                                                    case 1:
                                                        TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver =
                                                            false;
                                                        TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                            TrainManager.AirBrakeHandleState.Lap);
                                                        break;
                                                    case 2:
                                                        TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver =
                                                            false;
                                                        TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                            TrainManager.AirBrakeHandleState.Service);
                                                        break;
                                                    case 3:
                                                        TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                if (TrainManager.PlayerTrain.Specs.HasHoldBrake)
                                                {
                                                    double a = Interface.CurrentControls[i].AnalogState;
                                                    if (Interface.CurrentControls[i].Command ==
                                                        Interface.Command.BrakeFullAxis)
                                                    {
                                                        a = 0.5*(a + 1.0);
                                                    }
                                                    a *= (double) TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 2;
                                                    int b = (int) Math.Round(a);
                                                    bool q = b == 1;
                                                    if (b > 0) b--;
                                                    if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
                                                    {
                                                        TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                                        TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b,
                                                            false);
                                                    }
                                                    else
                                                    {
                                                        TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
                                                    }
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
                                                }
                                                else
                                                {
                                                    double a = Interface.CurrentControls[i].AnalogState;
                                                    if (Interface.CurrentControls[i].Command ==
                                                        Interface.Command.BrakeFullAxis)
                                                    {
                                                        a = 0.5*(a + 1.0);
                                                    }
                                                    a *= (double) TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 1;
                                                    int b = (int) Math.Round(a);
                                                    if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
                                                    {
                                                        TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                                        TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b,
                                                            false);
                                                    }
                                                    else
                                                    {
                                                        TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case Interface.Command.SingleFullAxis:
                                        // single full axis
                                        if (TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            if (TrainManager.PlayerTrain.Specs.HasHoldBrake)
                                            {
                                                double a = Interface.CurrentControls[i].AnalogState;
                                                int p =
                                                    (int)
                                                        Math.Round(a*
                                                                   (double)
                                                                       TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
                                                int b =
                                                    (int)
                                                        Math.Round(-a*
                                                                   (double)
                                                                       TrainManager.PlayerTrain.Specs.MaximumBrakeNotch +
                                                                   2);
                                                if (p < 0) p = 0;
                                                if (b < 0) b = 0;
                                                bool q = b == 1;
                                                if (b > 0) b--;
                                                if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
                                                {
                                                    TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
                                                }
                                                else
                                                {
                                                    TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
                                                }
                                                TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
                                            }
                                            else
                                            {
                                                double a = Interface.CurrentControls[i].AnalogState;
                                                int p =
                                                    (int)
                                                        Math.Round(a*
                                                                   (double)
                                                                       TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
                                                int b =
                                                    (int)
                                                        Math.Round(-a*
                                                                   ((double)
                                                                       TrainManager.PlayerTrain.Specs.MaximumBrakeNotch +
                                                                    1));
                                                if (p < 0) p = 0;
                                                if (b < 0) b = 0;
                                                if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
                                                {
                                                    TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
                                                }
                                                else
                                                {
                                                    TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
                                                }
                                            }
                                        }
                                        break;
                                    case Interface.Command.ReverserFullAxis:
                                        // reverser full axis
                                    {
                                        double a = Interface.CurrentControls[i].AnalogState;
                                        int r = (int) Math.Round(a);
                                        TrainManager.ApplyReverser(TrainManager.PlayerTrain, r, false);
                                    }
                                        break;
                                    case Interface.Command.CameraMoveForward:
                                        // camera move forward
                                        if (World.CameraMode == World.CameraViewMode.Interior |
                                            World.CameraMode == World.CameraViewMode.InteriorLookAhead |
                                            World.CameraMode == World.CameraViewMode.Exterior)
                                        {
                                            double s = World.CameraMode == World.CameraViewMode.Interior |
                                                       World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                                ? World.CameraInteriorTopSpeed
                                                : World.CameraExteriorTopSpeed;
                                            World.CameraAlignmentDirection.Position.Z = s*
                                                                                        Interface.CurrentControls[i]
                                                                                            .AnalogState;
                                        }
                                        else
                                        {
                                            World.CameraAlignmentDirection.TrackPosition = World.CameraExteriorTopSpeed*
                                                                                           Interface.CurrentControls[i]
                                                                                               .AnalogState;
                                        }
                                        break;
                                    case Interface.Command.CameraMoveBackward:
                                        // camera move backward
                                        if (World.CameraMode == World.CameraViewMode.Interior |
                                            World.CameraMode == World.CameraViewMode.InteriorLookAhead |
                                            World.CameraMode == World.CameraViewMode.Exterior)
                                        {
                                            double s = World.CameraMode == World.CameraViewMode.Interior |
                                                       World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                                ? World.CameraInteriorTopSpeed
                                                : World.CameraExteriorTopSpeed;
                                            World.CameraAlignmentDirection.Position.Z = -s*
                                                                                        Interface.CurrentControls[i]
                                                                                            .AnalogState;
                                        }
                                        else
                                        {
                                            World.CameraAlignmentDirection.TrackPosition =
                                                -World.CameraExteriorTopSpeed*Interface.CurrentControls[i].AnalogState;
                                        }
                                        break;
                                    case Interface.Command.CameraMoveLeft:
                                        // camera move left
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopSpeed
                                            : World.CameraExteriorTopSpeed;
                                        World.CameraAlignmentDirection.Position.X = -s*
                                                                                    Interface.CurrentControls[i]
                                                                                        .AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraMoveRight:
                                        // camera move right
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopSpeed
                                            : World.CameraExteriorTopSpeed;
                                        World.CameraAlignmentDirection.Position.X = s*
                                                                                    Interface.CurrentControls[i]
                                                                                        .AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraMoveUp:
                                        // camera move up
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopSpeed
                                            : World.CameraExteriorTopSpeed;
                                        World.CameraAlignmentDirection.Position.Y = s*
                                                                                    Interface.CurrentControls[i]
                                                                                        .AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraMoveDown:
                                        // camera move down
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopSpeed
                                            : World.CameraExteriorTopSpeed;
                                        World.CameraAlignmentDirection.Position.Y = -s*
                                                                                    Interface.CurrentControls[i]
                                                                                        .AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraRotateLeft:
                                        // camera rotate left
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopAngularSpeed
                                            : World.CameraExteriorTopAngularSpeed;
                                        World.CameraAlignmentDirection.Yaw = -s*Interface.CurrentControls[i].AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraRotateRight:
                                        // camera rotate right
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopAngularSpeed
                                            : World.CameraExteriorTopAngularSpeed;
                                        World.CameraAlignmentDirection.Yaw = s*Interface.CurrentControls[i].AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraRotateUp:
                                        // camera rotate up
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopAngularSpeed
                                            : World.CameraExteriorTopAngularSpeed;
                                        World.CameraAlignmentDirection.Pitch = s*
                                                                               Interface.CurrentControls[i].AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraRotateDown:
                                        // camera rotate down
                                    {
                                        double s = World.CameraMode == World.CameraViewMode.Interior |
                                                   World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                            ? World.CameraInteriorTopAngularSpeed
                                            : World.CameraExteriorTopAngularSpeed;
                                        World.CameraAlignmentDirection.Pitch = -s*
                                                                               Interface.CurrentControls[i].AnalogState;
                                    }
                                        break;
                                    case Interface.Command.CameraRotateCCW:
                                        // camera rotate ccw
                                        if ((World.CameraMode != World.CameraViewMode.Interior &
                                             World.CameraMode != World.CameraViewMode.InteriorLookAhead) |
                                            World.CameraRestriction != World.CameraRestrictionMode.On)
                                        {
                                            double s = World.CameraMode == World.CameraViewMode.Interior |
                                                       World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                                ? World.CameraInteriorTopAngularSpeed
                                                : World.CameraExteriorTopAngularSpeed;
                                            World.CameraAlignmentDirection.Roll = -s*
                                                                                  Interface.CurrentControls[i]
                                                                                      .AnalogState;
                                        }
                                        break;
                                    case Interface.Command.CameraRotateCW:
                                        // camera rotate cw
                                        if ((World.CameraMode != World.CameraViewMode.Interior &
                                             World.CameraMode != World.CameraViewMode.InteriorLookAhead) |
                                            World.CameraRestriction != World.CameraRestrictionMode.On)
                                        {
                                            double s = World.CameraMode == World.CameraViewMode.Interior |
                                                       World.CameraMode == World.CameraViewMode.InteriorLookAhead
                                                ? World.CameraInteriorTopAngularSpeed
                                                : World.CameraExteriorTopAngularSpeed;
                                            World.CameraAlignmentDirection.Roll = s*
                                                                                  Interface.CurrentControls[i]
                                                                                      .AnalogState;
                                        }
                                        break;
                                    case Interface.Command.CameraZoomIn:
                                        // camera zoom in
                                        if (TimeElapsed > 0.0)
                                        {
                                            World.CameraAlignmentDirection.Zoom = -World.CameraZoomTopSpeed*
                                                                                  Interface.CurrentControls[i]
                                                                                      .AnalogState;
                                        }
                                        break;
                                    case Interface.Command.CameraZoomOut:
                                        // camera zoom out
                                        if (TimeElapsed > 0.0)
                                        {
                                            World.CameraAlignmentDirection.Zoom = World.CameraZoomTopSpeed*
                                                                                  Interface.CurrentControls[i]
                                                                                      .AnalogState;
                                        }
                                        break;
                                    case Interface.Command.TimetableUp:
                                        // timetable up
                                        if (TimeElapsed > 0.0)
                                        {
                                            const double scrollSpeed = 250.0;
                                            if (Timetable.CurrentTimetable == Timetable.TimetableState.Default)
                                            {
                                                Timetable.DefaultTimetablePosition += scrollSpeed*
                                                                                      Interface.CurrentControls[i]
                                                                                          .AnalogState*TimeElapsed;
                                                if (Timetable.DefaultTimetablePosition > 0.0)
                                                    Timetable.DefaultTimetablePosition = 0.0;
                                            }
                                            else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom)
                                            {
                                                Timetable.CustomTimetablePosition += scrollSpeed*
                                                                                     Interface.CurrentControls[i]
                                                                                         .AnalogState*TimeElapsed;
                                                if (Timetable.CustomTimetablePosition > 0.0)
                                                    Timetable.CustomTimetablePosition = 0.0;
                                            }
                                        }
                                        break;
                                    case Interface.Command.TimetableDown:
                                        // timetable down
                                        if (TimeElapsed > 0.0)
                                        {
                                            const double scrollSpeed = 250.0;
                                            if (Timetable.CurrentTimetable == Timetable.TimetableState.Default)
                                            {
                                                Timetable.DefaultTimetablePosition -= scrollSpeed*
                                                                                      Interface.CurrentControls[i]
                                                                                          .AnalogState*TimeElapsed;
                                                double max;
                                                if (Timetable.DefaultTimetableTexture != null)
                                                {
                                                    Textures.LoadTexture(Timetable.DefaultTimetableTexture,
                                                        Textures.OpenGlTextureWrapMode.ClampClamp);
                                                    max =
                                                        Math.Min(
                                                            Screen.Height - Timetable.DefaultTimetableTexture.Height,
                                                            0.0);
                                                }
                                                else
                                                {
                                                    max = 0.0;
                                                }
                                                if (Timetable.DefaultTimetablePosition < max)
                                                    Timetable.DefaultTimetablePosition = max;
                                            }
                                            else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom)
                                            {
                                                Timetable.CustomTimetablePosition -= scrollSpeed*
                                                                                     Interface.CurrentControls[i]
                                                                                         .AnalogState*TimeElapsed;
                                                Textures.Texture texture =
                                                    Timetable.CurrentCustomTimetableDaytimeTexture;
                                                if (texture == null)
                                                {
                                                    texture = Timetable.CurrentCustomTimetableNighttimeTexture;
                                                }
                                                double max;
                                                if (texture != null)
                                                {
                                                    Textures.LoadTexture(texture,
                                                        Textures.OpenGlTextureWrapMode.ClampClamp);
                                                    max = Math.Min(Screen.Height - texture.Height, 0.0);
                                                }
                                                else
                                                {
                                                    max = 0.0;
                                                }
                                                if (Timetable.CustomTimetablePosition < max)
                                                    Timetable.CustomTimetablePosition = max;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        else if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital)
                        {
                            // digital control
                            if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
                            {
                                // pressed
                                Interface.CurrentControls[i].DigitalState =
                                    Interface.DigitalControlState.PressedAcknowledged;
                                switch (Interface.CurrentControls[i].Command)
                                {
                                    case Interface.Command.MiscQuit:
                                        // quit
                                        Game.CreateMenu(true);
                                        Game.CurrentInterface = Game.InterfaceType.Menu;
                                        break;
                                    case Interface.Command.CameraInterior:
                                        // camera: interior
                                        SaveCameraSettings();
                                        bool lookahead = false;
                                        if (World.CameraMode != World.CameraViewMode.InteriorLookAhead &
                                            World.CameraRestriction == World.CameraRestrictionMode.NotAvailable)
                                        {
                                            Game.AddMessage(
                                                Interface.GetInterfaceString("notification_interior_lookahead"),
                                                Game.MessageDependency.None, Interface.GameMode.Expert,
                                                Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                            lookahead = true;
                                        }
                                        else
                                        {
                                            Game.AddMessage(Interface.GetInterfaceString("notification_interior"),
                                                Game.MessageDependency.None, Interface.GameMode.Expert,
                                                Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                        }
                                        World.CameraMode = World.CameraViewMode.Interior;
                                        RestoreCameraSettings();
                                        for (int j = 0; j <= TrainManager.PlayerTrain.DriverCar; j++)
                                        {
                                            if (TrainManager.PlayerTrain.Cars[j].CarSections.Length != 0)
                                            {
                                                TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
                                            }
                                            else
                                            {
                                                TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
                                            }
                                        }
                                        for (int j = TrainManager.PlayerTrain.DriverCar + 1;
                                            j < TrainManager.PlayerTrain.Cars.Length;
                                            j++)
                                        {
                                            TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
                                        }
                                        World.CameraAlignmentDirection = new World.CameraAlignment();
                                        World.CameraAlignmentSpeed = new World.CameraAlignment();
                                        UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
                                        World.UpdateAbsoluteCamera(TimeElapsed);
                                        World.UpdateViewingDistances();
                                        if (World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
                                        {
                                            if (!World.PerformCameraRestrictionTest())
                                            {
                                                World.InitializeCameraRestriction();
                                            }
                                        }
                                        if (lookahead)
                                        {
                                            World.CameraMode = World.CameraViewMode.InteriorLookAhead;
                                        }
                                        break;
                                    case Interface.Command.CameraExterior:
                                        // camera: exterior
                                        Game.AddMessage(Interface.GetInterfaceString("notification_exterior"),
                                            Game.MessageDependency.None, Interface.GameMode.Expert,
                                            Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                        SaveCameraSettings();
                                        World.CameraMode = World.CameraViewMode.Exterior;
                                        RestoreCameraSettings();
                                        if (TrainManager.PlayerTrain.Cars.Length >= 1 &&
                                            TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar]
                                                .CarSections.Length >= 2)
                                        {
                                            TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                TrainManager.PlayerTrain.DriverCar, 1);
                                        }
                                        else
                                        {
                                            TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                TrainManager.PlayerTrain.DriverCar, -1);
                                        }
                                        for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
                                        {
                                            if (j != TrainManager.PlayerTrain.DriverCar)
                                            {
                                                if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1)
                                                {
                                                    TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
                                                }
                                                else
                                                {
                                                    TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
                                                }
                                            }
                                        }
                                        World.CameraAlignmentDirection = new World.CameraAlignment();
                                        World.CameraAlignmentSpeed = new World.CameraAlignment();
                                        UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
                                        World.UpdateAbsoluteCamera(TimeElapsed);
                                        World.UpdateViewingDistances();
                                        break;
                                    case Interface.Command.CameraTrack:
                                    case Interface.Command.CameraFlyBy:
                                        // camera: track / fly-by
                                    {
                                        SaveCameraSettings();
                                        if (Interface.CurrentControls[i].Command == Interface.Command.CameraTrack)
                                        {
                                            World.CameraMode = World.CameraViewMode.Track;
                                            Game.AddMessage(Interface.GetInterfaceString("notification_track"),
                                                Game.MessageDependency.None, Interface.GameMode.Expert,
                                                Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                        }
                                        else
                                        {
                                            if (World.CameraMode == World.CameraViewMode.FlyBy)
                                            {
                                                World.CameraMode = World.CameraViewMode.FlyByZooming;
                                                Game.AddMessage(
                                                    Interface.GetInterfaceString("notification_flybyzooming"),
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                            }
                                            else
                                            {
                                                World.CameraMode = World.CameraViewMode.FlyBy;
                                                Game.AddMessage(
                                                    Interface.GetInterfaceString("notification_flybynormal"),
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                            }
                                        }
                                        RestoreCameraSettings();
                                        if (TrainManager.PlayerTrain.Cars.Length >= 1 &&
                                            TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar]
                                                .CarSections.Length >= 2)
                                        {
                                            TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                TrainManager.PlayerTrain.DriverCar, 1);
                                        }
                                        else
                                        {
                                            TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                TrainManager.PlayerTrain.DriverCar, -1);
                                        }
                                        for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
                                        {
                                            if (j != TrainManager.PlayerTrain.DriverCar)
                                            {
                                                if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1)
                                                {
                                                    TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
                                                }
                                                else
                                                {
                                                    TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
                                                }
                                            }
                                        }
                                        World.CameraAlignmentDirection = new World.CameraAlignment();
                                        World.CameraAlignmentSpeed = new World.CameraAlignment();
                                        UpdateViewport(ViewPortChangeMode.NoChange);
                                        World.UpdateAbsoluteCamera(TimeElapsed);
                                        World.UpdateViewingDistances();
                                    }
                                        break;
                                    case Interface.Command.CameraPreviousPOI:
                                        // camera: previous poi
                                        if (Game.ApplyPointOfInterest(-1, true))
                                        {
                                            if (World.CameraMode != World.CameraViewMode.Track &
                                                World.CameraMode != World.CameraViewMode.FlyBy &
                                                World.CameraMode != World.CameraViewMode.FlyByZooming)
                                            {
                                                World.CameraMode = World.CameraViewMode.Track;
                                                Game.AddMessage(Interface.GetInterfaceString("notification_track"),
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                            }
                                            double z = World.CameraCurrentAlignment.Position.Z;
                                            World.CameraCurrentAlignment.Position =
                                                new OpenBveApi.Math.Vector3(World.CameraCurrentAlignment.Position.X,
                                                    World.CameraCurrentAlignment.Position.Y, 0.0);
                                            World.CameraCurrentAlignment.Zoom = 0.0;
                                            World.CameraAlignmentDirection = new World.CameraAlignment();
                                            World.CameraAlignmentSpeed = new World.CameraAlignment();
                                            if (TrainManager.PlayerTrain.Cars.Length >= 1 &&
                                                TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar]
                                                    .CarSections.Length >= 2)
                                            {
                                                TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                    TrainManager.PlayerTrain.DriverCar, 1);
                                            }
                                            else
                                            {
                                                TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                    TrainManager.PlayerTrain.DriverCar, -1);
                                            }
                                            for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
                                            {
                                                if (j != TrainManager.PlayerTrain.DriverCar)
                                                {
                                                    if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1)
                                                    {
                                                        TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
                                                    }
                                                    else
                                                    {
                                                        TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
                                                    }
                                                }
                                            }
                                            TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower,
                                                World.CameraTrackFollower.TrackPosition + z, true, false);
                                            World.CameraCurrentAlignment.TrackPosition =
                                                World.CameraTrackFollower.TrackPosition;
                                            World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
                                            UpdateViewport(ViewPortChangeMode.NoChange);
                                            World.UpdateAbsoluteCamera(TimeElapsed);
                                            World.UpdateViewingDistances();
                                        }
                                        break;
                                    case Interface.Command.CameraNextPOI:
                                        // camera: next poi
                                        if (Game.ApplyPointOfInterest(1, true))
                                        {
                                            if (World.CameraMode != World.CameraViewMode.Track &
                                                World.CameraMode != World.CameraViewMode.FlyBy &
                                                World.CameraMode != World.CameraViewMode.FlyByZooming)
                                            {
                                                World.CameraMode = World.CameraViewMode.Track;
                                                Game.AddMessage(Interface.GetInterfaceString("notification_track"),
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                            }
                                            double z = World.CameraCurrentAlignment.Position.Z;
                                            World.CameraCurrentAlignment.Position =
                                                new OpenBveApi.Math.Vector3(World.CameraCurrentAlignment.Position.X,
                                                    World.CameraCurrentAlignment.Position.Y, 0.0);
                                            World.CameraCurrentAlignment.Zoom = 0.0;
                                            World.CameraAlignmentDirection = new World.CameraAlignment();
                                            World.CameraAlignmentSpeed = new World.CameraAlignment();
                                            if (TrainManager.PlayerTrain.Cars.Length >= 1 &&
                                                TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar]
                                                    .CarSections.Length >= 2)
                                            {
                                                TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                    TrainManager.PlayerTrain.DriverCar, 1);
                                            }
                                            else
                                            {
                                                TrainManager.ChangeCarSection(TrainManager.PlayerTrain,
                                                    TrainManager.PlayerTrain.DriverCar, -1);
                                            }
                                            for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
                                            {
                                                if (j != TrainManager.PlayerTrain.DriverCar)
                                                {
                                                    if (TrainManager.PlayerTrain.Cars[j].CarSections.Length >= 1)
                                                    {
                                                        TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
                                                    }
                                                    else
                                                    {
                                                        TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
                                                    }
                                                }
                                            }
                                            TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower,
                                                World.CameraTrackFollower.TrackPosition + z, true, false);
                                            World.CameraCurrentAlignment.TrackPosition =
                                                World.CameraTrackFollower.TrackPosition;
                                            World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
                                            UpdateViewport(ViewPortChangeMode.NoChange);
                                            World.UpdateAbsoluteCamera(TimeElapsed);
                                            World.UpdateViewingDistances();
                                        }
                                        break;
                                    case Interface.Command.CameraReset:
                                        // camera: reset
                                        if (World.CameraMode == World.CameraViewMode.Interior |
                                            World.CameraMode == World.CameraViewMode.InteriorLookAhead)
                                        {
                                            World.CameraCurrentAlignment.Position = new OpenBveApi.Math.Vector3(0.0, 0.0,
                                                0.0);
                                        }
                                        World.CameraCurrentAlignment.Yaw = 0.0;
                                        World.CameraCurrentAlignment.Pitch = 0.0;
                                        World.CameraCurrentAlignment.Roll = 0.0;
                                        if (World.CameraMode == World.CameraViewMode.Track)
                                        {
                                            TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower,
                                                TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition, true,
                                                false);
                                        }
                                        else if (World.CameraMode == World.CameraViewMode.FlyBy |
                                                 World.CameraMode == World.CameraViewMode.FlyByZooming)
                                        {
                                            if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed >= 0.0)
                                            {
                                                double d = 30.0 +
                                                           4.0*TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
                                                TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower,
                                                    TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower
                                                        .TrackPosition + d, true, false);
                                            }
                                            else
                                            {
                                                double d = 30.0 -
                                                           4.0*TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
                                                TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower,
                                                    TrainManager.PlayerTrain.Cars[
                                                        TrainManager.PlayerTrain.Cars.Length - 1].RearAxle.Follower
                                                        .TrackPosition - d, true, false);
                                            }
                                        }
                                        World.CameraCurrentAlignment.TrackPosition =
                                            World.CameraTrackFollower.TrackPosition;
                                        World.CameraCurrentAlignment.Zoom = 0.0;
                                        World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
                                        World.CameraAlignmentDirection = new World.CameraAlignment();
                                        World.CameraAlignmentSpeed = new World.CameraAlignment();
                                        UpdateViewport(ViewPortChangeMode.NoChange);
                                        World.UpdateAbsoluteCamera(TimeElapsed);
                                        World.UpdateViewingDistances();
                                        if ((World.CameraMode == World.CameraViewMode.Interior |
                                             World.CameraMode == World.CameraViewMode.InteriorLookAhead) &
                                            World.CameraRestriction == World.CameraRestrictionMode.On)
                                        {
                                            if (!World.PerformCameraRestrictionTest())
                                            {
                                                World.InitializeCameraRestriction();
                                            }
                                        }
                                        break;
                                    case Interface.Command.CameraRestriction:
                                        // camera: restriction
                                        if (World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
                                        {
                                            if (World.CameraRestriction == World.CameraRestrictionMode.Off)
                                            {
                                                World.CameraRestriction = World.CameraRestrictionMode.On;
                                            }
                                            else
                                            {
                                                World.CameraRestriction = World.CameraRestrictionMode.Off;
                                            }
                                            World.InitializeCameraRestriction();
                                            if (World.CameraRestriction == World.CameraRestrictionMode.Off)
                                            {
                                                Game.AddMessage(
                                                    Interface.GetInterfaceString("notification_camerarestriction_off"),
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                            }
                                            else
                                            {
                                                Game.AddMessage(
                                                    Interface.GetInterfaceString("notification_camerarestriction_on"),
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
                                            }
                                        }
                                        break;
                                    case Interface.Command.SinglePower:
                                        // single power
                                        if (TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
                                            if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver)
                                            {
                                                TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                            }
                                            else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake)
                                            {
                                                TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
                                                TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
                                            }
                                            else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                            {
                                                TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
                                            }
                                            else if (b > 0)
                                            {
                                                TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
                                            }
                                            else
                                            {
                                                int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
                                                if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch)
                                                {
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
                                                }
                                            }
                                        }
                                        break;
                                    case Interface.Command.SingleNeutral:
                                        // single neutral
                                        if (TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
                                            if (p > 0)
                                            {
                                                TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
                                            }
                                            else
                                            {
                                                int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
                                                if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver)
                                                {
                                                    TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                                }
                                                else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake)
                                                {
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
                                                }
                                                else if (b > 0)
                                                {
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
                                                }
                                            }
                                        }
                                        break;
                                    case Interface.Command.SingleBrake:
                                        // single brake
                                        if (TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
                                            if (p > 0)
                                            {
                                                TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
                                            }
                                            else
                                            {
                                                int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
                                                if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 &
                                                    !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
                                                }
                                                else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
                                                {
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
                                                }
                                            }
                                        }
                                        break;
                                    case Interface.Command.SingleEmergency:
                                        // single emergency
                                        if (TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
                                        }
                                        break;
                                    case Interface.Command.PowerIncrease:
                                        // power increase
                                        if (!TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
                                            if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch)
                                            {
                                                TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
                                            }
                                        }
                                        break;
                                    case Interface.Command.PowerDecrease:
                                        // power decrease
                                        if (!TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
                                            if (p > 0)
                                            {
                                                TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
                                            }
                                        }
                                        break;
                                    case Interface.Command.BrakeIncrease:
                                        // brake increase
                                        if (!TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int d = TrainManager.PlayerTrain.DriverCar;
                                            if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType ==
                                                TrainManager.CarBrakeType.AutomaticAirBrake)
                                            {
                                                if (TrainManager.PlayerTrain.Specs.HasHoldBrake &
                                                    TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver ==
                                                    TrainManager.AirBrakeHandleState.Release &
                                                    !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                        TrainManager.AirBrakeHandleState.Lap);
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver ==
                                                         TrainManager.AirBrakeHandleState.Lap)
                                                {
                                                    TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                        TrainManager.AirBrakeHandleState.Service);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver ==
                                                         TrainManager.AirBrakeHandleState.Release)
                                                {
                                                    TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                        TrainManager.AirBrakeHandleState.Lap);
                                                }
                                            }
                                            else
                                            {
                                                int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
                                                if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 &
                                                    !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
                                                }
                                                else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
                                                {
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
                                                }
                                            }
                                        }
                                        break;
                                    case Interface.Command.BrakeDecrease:
                                        // brake decrease
                                        if (!TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            int d = TrainManager.PlayerTrain.DriverCar;
                                            if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType ==
                                                TrainManager.CarBrakeType.AutomaticAirBrake)
                                            {
                                                if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver)
                                                {
                                                    TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.HasHoldBrake &
                                                         TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver ==
                                                         TrainManager.AirBrakeHandleState.Lap &
                                                         !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                        TrainManager.AirBrakeHandleState.Release);
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver ==
                                                         TrainManager.AirBrakeHandleState.Lap)
                                                {
                                                    TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
                                                        TrainManager.AirBrakeHandleState.Release);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver ==
                                                         TrainManager.AirBrakeHandleState.Service)
                                                {
                                                    TrainManager.ApplyAirBrakeHandle(
                                                        TrainManager.PlayerTrain,
                                                        TrainManager.AirBrakeHandleState.Lap);
                                                }
                                            }
                                            else
                                            {
                                                int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
                                                if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver)
                                                {
                                                    TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
                                                }
                                                else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake)
                                                {
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
                                                }
                                                else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
                                                {
                                                    TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
                                                }
                                                else if (b > 0)
                                                {
                                                    TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
                                                }
                                            }
                                        }
                                        break;
                                    case Interface.Command.BrakeEmergency:
                                        // brake emergency
                                        if (!TrainManager.PlayerTrain.Specs.SingleHandle)
                                        {
                                            TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
                                        }
                                        break;
                                    case Interface.Command.DeviceConstSpeed:
                                        // const speed
                                        if (TrainManager.PlayerTrain.Specs.HasConstSpeed)
                                        {
                                            TrainManager.PlayerTrain.Specs.CurrentConstSpeed =
                                                !TrainManager.PlayerTrain.Specs.CurrentConstSpeed;
                                        }
                                        break;
                                    case Interface.Command.ReverserForward:
                                        // reverser forward
                                        if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver < 1)
                                        {
                                            TrainManager.ApplyReverser(TrainManager.PlayerTrain, 1, true);
                                        }
                                        break;
                                    case Interface.Command.ReverserBackward:
                                        // reverser backward
                                        if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver > -1)
                                        {
                                            TrainManager.ApplyReverser(TrainManager.PlayerTrain, -1, true);
                                        }
                                        break;
                                    case Interface.Command.HornPrimary:
                                    case Interface.Command.HornSecondary:
                                    case Interface.Command.HornMusic:
                                        // horn
                                    {
                                        int j = Interface.CurrentControls[i].Command == Interface.Command.HornPrimary
                                            ? 0
                                            : Interface.CurrentControls[i].Command == Interface.Command.HornSecondary
                                                ? 1
                                                : 2;
                                        int d = TrainManager.PlayerTrain.DriverCar;
                                        if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns.Length > j)
                                        {
                                            Sounds.SoundBuffer buffer =
                                                TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Buffer;
                                            if (buffer != null)
                                            {
                                                OpenBveApi.Math.Vector3 pos =
                                                    TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Position;
                                                if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Loop)
                                                {
                                                    if (
                                                        Sounds.IsPlaying(
                                                            TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound
                                                                .Source))
                                                    {
                                                        Sounds.StopSound(
                                                            TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound
                                                                .Source);
                                                    }
                                                    else
                                                    {
                                                        TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Source =
                                                            Sounds.PlaySound(buffer, 1.0, 1.0, pos,
                                                                TrainManager.PlayerTrain,
                                                                TrainManager.PlayerTrain.DriverCar, true);
                                                    }
                                                }
                                                else
                                                {
                                                    TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Source =
                                                        Sounds.PlaySound(buffer, 1.0, 1.0, pos, TrainManager.PlayerTrain,
                                                            TrainManager.PlayerTrain.DriverCar, false);
                                                }
                                                if (TrainManager.PlayerTrain.Plugin != null)
                                                {
                                                    TrainManager.PlayerTrain.Plugin.HornBlow(j == 0
                                                        ? OpenBveApi.Runtime.HornTypes.Primary
                                                        : j == 1
                                                            ? OpenBveApi.Runtime.HornTypes.Secondary
                                                            : OpenBveApi.Runtime.HornTypes.Music);
                                                }
                                            }
                                        }
                                    }
                                        break;
                                    case Interface.Command.DoorsLeft:
                                        // doors: left
                                        if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false) &
                                             TrainManager.TrainDoorState.Opened) == 0)
                                        {
                                            if (TrainManager.PlayerTrain.Specs.DoorOpenMode !=
                                                TrainManager.DoorMode.Automatic)
                                            {
                                                TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, true, false);
                                            }
                                        }
                                        else
                                        {
                                            if (TrainManager.PlayerTrain.Specs.DoorCloseMode !=
                                                TrainManager.DoorMode.Automatic)
                                            {
                                                TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, true, false);
                                            }
                                        }
                                        break;
                                    case Interface.Command.DoorsRight:
                                        // doors: right
                                        if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true) &
                                             TrainManager.TrainDoorState.Opened) == 0)
                                        {
                                            if (TrainManager.PlayerTrain.Specs.DoorOpenMode !=
                                                TrainManager.DoorMode.Automatic)
                                            {
                                                TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, false, true);
                                            }
                                        }
                                        else
                                        {
                                            if (TrainManager.PlayerTrain.Specs.DoorCloseMode !=
                                                TrainManager.DoorMode.Automatic)
                                            {
                                                TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, false, true);
                                            }
                                        }
                                        break;
                                    case Interface.Command.SecurityS:
                                    case Interface.Command.SecurityA1:
                                    case Interface.Command.SecurityA2:
                                    case Interface.Command.SecurityB1:
                                    case Interface.Command.SecurityB2:
                                    case Interface.Command.SecurityC1:
                                    case Interface.Command.SecurityC2:
                                    case Interface.Command.SecurityD:
                                    case Interface.Command.SecurityE:
                                    case Interface.Command.SecurityF:
                                    case Interface.Command.SecurityG:
                                    case Interface.Command.SecurityH:
                                    case Interface.Command.SecurityI:
                                    case Interface.Command.SecurityJ:
                                    case Interface.Command.SecurityK:
                                    case Interface.Command.SecurityL:
                                    case Interface.Command.SecurityM:
                                    case Interface.Command.SecurityN:
                                    case Interface.Command.SecurityO:
                                    case Interface.Command.SecurityP:
                                    case Interface.Command.WiperSpeedUp:
                                    case Interface.Command.WiperSpeedDown:
                                    case Interface.Command.FillFuel:
                                    case Interface.Command.LiveSteamInjector:
                                    case Interface.Command.ExhaustSteamInjector:
                                    case Interface.Command.IncreaseCutoff:
                                    case Interface.Command.DecreaseCutoff:
                                    case Interface.Command.Blowers:
                                    case Interface.Command.EngineStart:
                                    case Interface.Command.EngineStop:
                                    case Interface.Command.GearUp:
                                    case Interface.Command.RaisePantograph:
                                    case Interface.Command.LowerPantograph:
                                    case Interface.Command.MainBreaker:
                                        if (TrainManager.PlayerTrain.Plugin != null)
                                        {
                                            TrainManager.PlayerTrain.Plugin.KeyDown(
                                                Interface.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
                                        }
                                        break;



                                    case Interface.Command.TimetableToggle:
                                        // option: timetable
                                        if (Timetable.CustomTimetableAvailable)
                                        {
                                            switch (Timetable.CurrentTimetable)
                                            {
                                                case Timetable.TimetableState.Custom:
                                                    Timetable.CurrentTimetable = Timetable.TimetableState.Default;
                                                    break;
                                                case Timetable.TimetableState.Default:
                                                    Timetable.CurrentTimetable = Timetable.TimetableState.None;
                                                    break;
                                                default:
                                                    Timetable.CurrentTimetable = Timetable.TimetableState.Custom;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (Timetable.CurrentTimetable)
                                            {
                                                case Timetable.TimetableState.Default:
                                                    Timetable.CurrentTimetable = Timetable.TimetableState.None;
                                                    break;
                                                default:
                                                    Timetable.CurrentTimetable = Timetable.TimetableState.Default;
                                                    break;
                                            }
                                        }
                                        break;
                                    case Interface.Command.DebugWireframe:
                                        // option: wireframe
                                        Renderer.OptionWireframe = !Renderer.OptionWireframe;
                                        if (Renderer.OptionWireframe)
                                        {
                                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                                        }
                                        else
                                        {
                                            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                                        }
                                        Renderer.StaticOpaqueForceUpdate = true;
                                        break;
                                    case Interface.Command.DebugNormals:
                                        // option: normals
                                        Renderer.OptionNormals = !Renderer.OptionNormals;
                                        Renderer.StaticOpaqueForceUpdate = true;
                                        break;
                                    case Interface.Command.MiscAI:
                                        // option: AI
                                        if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
                                        {
                                            Game.AddMessage(
                                                Interface.GetInterfaceString("notification_notavailableexpert"),
                                                Game.MessageDependency.None, Interface.GameMode.Expert,
                                                Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
                                        }
                                        else
                                        {
                                            if (TrainManager.PlayerTrain.AI == null)
                                            {
                                                TrainManager.PlayerTrain.AI =
                                                    new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain);
                                                if (TrainManager.PlayerTrain.Plugin != null &&
                                                    !TrainManager.PlayerTrain.Plugin.SupportsAI)
                                                {
                                                    Game.AddMessage(
                                                        Interface.GetInterfaceString("notification_aiunable"),
                                                        Game.MessageDependency.None, Interface.GameMode.Expert,
                                                        Game.MessageColor.Blue, Game.SecondsSinceMidnight + 10.0);
                                                }
                                            }
                                            else
                                            {
                                                TrainManager.PlayerTrain.AI = null;
                                            }
                                        }
                                        break;
                                    case Interface.Command.MiscInterfaceMode:
                                        // option: debug
                                        switch (Renderer.CurrentOutputMode)
                                        {
                                            case Renderer.OutputMode.Default:
                                                Renderer.CurrentOutputMode = Interface.CurrentOptions.GameMode ==
                                                                             Interface.GameMode.Expert
                                                    ? Renderer.OutputMode.None
                                                    : Renderer.OutputMode.Debug;
                                                break;
                                            case Renderer.OutputMode.Debug:
                                                Renderer.CurrentOutputMode = Renderer.OutputMode.None;
                                                break;
                                            default:
                                                Renderer.CurrentOutputMode = Renderer.OutputMode.Default;
                                                break;
                                        }
                                        break;
                                    case Interface.Command.MiscBackfaceCulling:
                                        // option: backface culling
                                        Renderer.OptionBackfaceCulling = !Renderer.OptionBackfaceCulling;
                                        Renderer.StaticOpaqueForceUpdate = true;
                                        Game.AddMessage(
                                            Interface.GetInterfaceString(Renderer.OptionBackfaceCulling
                                                ? "notification_backfaceculling_on"
                                                : "notification_backfaceculling_off"), Game.MessageDependency.None,
                                            Interface.GameMode.Expert, Game.MessageColor.Blue,
                                            Game.SecondsSinceMidnight + 2.0);
                                        break;
                                    case Interface.Command.MiscCPUMode:
                                        // option: limit frame rate
                                        LimitFramerate = !LimitFramerate;
                                        Game.AddMessage(
                                            Interface.GetInterfaceString(LimitFramerate
                                                ? "notification_cpu_low"
                                                : "notification_cpu_normal"), Game.MessageDependency.None,
                                            Interface.GameMode.Expert, Game.MessageColor.Blue,
                                            Game.SecondsSinceMidnight + 2.0);
                                        break;
                                    case Interface.Command.DebugBrakeSystems:
                                        // option: brake systems
                                        if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
                                        {
                                            Game.AddMessage(
                                                Interface.GetInterfaceString("notification_notavailableexpert"),
                                                Game.MessageDependency.None, Interface.GameMode.Expert,
                                                Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
                                        }
                                        else
                                        {
                                            Renderer.OptionBrakeSystems = !Renderer.OptionBrakeSystems;
                                        }
                                        break;
                                    case Interface.Command.MenuActivate:
                                        // menu
                                        Game.CreateMenu(false);
                                        Game.CurrentInterface = Game.InterfaceType.Menu;
                                        break;
                                    case Interface.Command.MiscPause:
                                        // pause
                                        Game.CurrentInterface = Game.InterfaceType.Pause;
                                        break;
                                    case Interface.Command.MiscClock:
                                        // clock
                                        Renderer.OptionClock = !Renderer.OptionClock;
                                        break;
                                    case Interface.Command.MiscTimeFactor:
                                        // time factor
                                        if (!PluginManager.Plugin.DisableTimeAcceleration)
                                        {
                                            if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
                                            {
                                                Game.AddMessage(
                                                    Interface.GetInterfaceString("notification_notavailableexpert"),
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue,
                                                    Game.SecondsSinceMidnight + 5.0);
                                            }
                                            else
                                            {
                                                TimeFactor = TimeFactor == 1
                                                    ? Interface.CurrentOptions.TimeAccelerationFactor
                                                    : 1;
                                                Game.AddMessage(
                                                    TimeFactor.ToString(
                                                        System.Globalization.CultureInfo.InvariantCulture) + "x",
                                                    Game.MessageDependency.None, Interface.GameMode.Expert,
                                                    Game.MessageColor.Blue,
                                                    Game.SecondsSinceMidnight + 5.0*(double) TimeFactor);
                                            }
                                        }
                                        break;
                                    case Interface.Command.MiscSpeed:
                                        // speed
                                        if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
                                        {
                                            Game.AddMessage(
                                                Interface.GetInterfaceString("notification_notavailableexpert"),
                                                Game.MessageDependency.None, Interface.GameMode.Expert,
                                                Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
                                        }
                                        else
                                        {
                                            Renderer.OptionSpeed++;
                                            if ((int) Renderer.OptionSpeed >= 3) Renderer.OptionSpeed = 0;
                                        }
                                        break;
                                    case Interface.Command.MiscFps:
                                        // fps
                                        Renderer.OptionFrameRates = !Renderer.OptionFrameRates;
                                        break;
                                    case Interface.Command.MiscFullscreen:
                                        // toggle fullscreen
                                        Screen.ToggleFullscreen();
                                        break;
                                    case Interface.Command.MiscMute:
                                        // mute
                                        Sounds.GlobalMute = !Sounds.GlobalMute;
                                        Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
                                        break;
                                    case Interface.Command.RouteInformation:
                                        if (RouteInformation == null)
                                        {
                                            RouteInformation = new formRouteInformation();
                                        }
                                        if (!RouteInformation.Visible)
                                        {
                                            RouteInformation.Show();
                                        }
                                        else
                                        {
                                            RouteInformation.Hide();
                                        }
                                        break;
                                }
                            }
                            else if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released)
                            {
                                // released
                                Interface.CurrentControls[i].DigitalState =
                                    Interface.DigitalControlState.ReleasedAcknowledged;
                                switch (Interface.CurrentControls[i].Command)
                                {
                                    case Interface.Command.SecurityS:
                                    case Interface.Command.SecurityA1:
                                    case Interface.Command.SecurityA2:
                                    case Interface.Command.SecurityB1:
                                    case Interface.Command.SecurityB2:
                                    case Interface.Command.SecurityC1:
                                    case Interface.Command.SecurityC2:
                                    case Interface.Command.SecurityD:
                                    case Interface.Command.SecurityE:
                                    case Interface.Command.SecurityF:
                                    case Interface.Command.SecurityG:
                                    case Interface.Command.SecurityH:
                                    case Interface.Command.SecurityI:
                                    case Interface.Command.SecurityJ:
                                    case Interface.Command.SecurityK:
                                    case Interface.Command.SecurityL:
                                    case Interface.Command.SecurityM:
                                    case Interface.Command.SecurityN:
                                    case Interface.Command.SecurityO:
                                    case Interface.Command.SecurityP:
                                    case Interface.Command.WiperSpeedUp:
                                    case Interface.Command.WiperSpeedDown:
                                    case Interface.Command.FillFuel:
                                    case Interface.Command.LiveSteamInjector:
                                    case Interface.Command.ExhaustSteamInjector:
                                    case Interface.Command.IncreaseCutoff:
                                    case Interface.Command.DecreaseCutoff:
                                    case Interface.Command.Blowers:
                                    case Interface.Command.EngineStart:
                                    case Interface.Command.EngineStop:
                                    case Interface.Command.GearUp:
                                    case Interface.Command.RaisePantograph:
                                    case Interface.Command.LowerPantograph:
                                    case Interface.Command.MainBreaker:
                                        if (TrainManager.PlayerTrain.Plugin != null)
                                        {
                                            TrainManager.PlayerTrain.Plugin.KeyUp(
                                                Interface.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    break;
            }
        }

    }
}
