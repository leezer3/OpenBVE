using System;
using OpenBveApi.Interface;

namespace OpenBve
{
    internal static partial class Renderer
    {

        /* --------------------------------------------------------------
		 * This file contains the drawing routines for the ATS Panel Lamps
		 * -------------------------------------------------------------- */

        private enum LampType
        {
            None,
            Ats, AtsOperation,
            AtsPPower, AtsPPattern, AtsPBrakeOverride, AtsPBrakeOperation, AtsP, AtsPFailure,
            Atc, AtcPower, AtcUse, AtcEmergency,
            Eb, ConstSpeed
        }
        private struct Lamp
        {
            internal readonly LampType Type;
            internal readonly string Text;
            internal readonly float Width;
            internal readonly float Height;
            internal Lamp(LampType Type)
            {
                this.Type = Type;
                switch (Type)
                {
                    case LampType.None:
                        this.Text = null;
                        break;
                    case LampType.Ats:
                        this.Text = Translations.GetInterfaceString("lamps_ats");
                        break;
                    case LampType.AtsOperation:
                        this.Text = Translations.GetInterfaceString("lamps_atsoperation");
                        break;
                    case LampType.AtsPPower:
                        this.Text = Translations.GetInterfaceString("lamps_atsppower");
                        break;
                    case LampType.AtsPPattern:
                        this.Text = Translations.GetInterfaceString("lamps_atsppattern");
                        break;
                    case LampType.AtsPBrakeOverride:
                        this.Text = Translations.GetInterfaceString("lamps_atspbrakeoverride");
                        break;
                    case LampType.AtsPBrakeOperation:
                        this.Text = Translations.GetInterfaceString("lamps_atspbrakeoperation");
                        break;
                    case LampType.AtsP:
                        this.Text = Translations.GetInterfaceString("lamps_atsp");
                        break;
                    case LampType.AtsPFailure:
                        this.Text = Translations.GetInterfaceString("lamps_atspfailure");
                        break;
                    case LampType.Atc:
                        this.Text = Translations.GetInterfaceString("lamps_atc");
                        break;
                    case LampType.AtcPower:
                        this.Text = Translations.GetInterfaceString("lamps_atcpower");
                        break;
                    case LampType.AtcUse:
                        this.Text = Translations.GetInterfaceString("lamps_atcuse");
                        break;
                    case LampType.AtcEmergency:
                        this.Text = Translations.GetInterfaceString("lamps_atcemergency");
                        break;
                    case LampType.Eb:
                        this.Text = Translations.GetInterfaceString("lamps_eb");
                        break;
                    case LampType.ConstSpeed:
                        this.Text = Translations.GetInterfaceString("lamps_constspeed");
                        break;
                    default:
                        this.Text = "TEXT";
                        break;
                }
                Fonts.OpenGlFont font = Fonts.NormalFont;
                for (int i = 0; i < HUD.CurrentHudElements.Length; i++)
                {
                    if (HUD.CurrentHudElements[i].Subject.Equals("ats", StringComparison.OrdinalIgnoreCase))
                    {
                        font = HUD.CurrentHudElements[i].Font;
                        break;
                    }
                }
                System.Drawing.Size size = MeasureString(font, this.Text);
                this.Width = size.Width;
                this.Height = size.Height;
            }
        }
        private struct LampCollection
        {
            internal Lamp[] Lamps;
            internal float Width;
        }
        private static LampCollection CurrentLampCollection;

		/// <summary>Initialises the ATS lamps for train using one of the default safety systems</summary>
        private static void InitializeLamps()
        {
            bool atsSn = (TrainManager.PlayerTrain.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.AtsSn) != 0;
            bool atsP = (TrainManager.PlayerTrain.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.AtsP) != 0;
            bool atc = (TrainManager.PlayerTrain.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.Atc) != 0;
            bool eb = (TrainManager.PlayerTrain.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.Eb) != 0;
            CurrentLampCollection.Width = 0.0f;
            CurrentLampCollection.Lamps = new Lamp[17];
            int Count;
            if (TrainManager.PlayerTrain.Plugin == null || !TrainManager.PlayerTrain.Plugin.IsDefault)
            {
                Count = 0;
            }
            else if (atsP & atc)
            {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
                CurrentLampCollection.Lamps[2] = new Lamp(LampType.None);
                CurrentLampCollection.Lamps[3] = new Lamp(LampType.AtsPPower);
                CurrentLampCollection.Lamps[4] = new Lamp(LampType.AtsPPattern);
                CurrentLampCollection.Lamps[5] = new Lamp(LampType.AtsPBrakeOverride);
                CurrentLampCollection.Lamps[6] = new Lamp(LampType.AtsPBrakeOperation);
                CurrentLampCollection.Lamps[7] = new Lamp(LampType.AtsP);
                CurrentLampCollection.Lamps[8] = new Lamp(LampType.AtsPFailure);
                CurrentLampCollection.Lamps[9] = new Lamp(LampType.None);
                CurrentLampCollection.Lamps[10] = new Lamp(LampType.Atc);
                CurrentLampCollection.Lamps[11] = new Lamp(LampType.AtcPower);
                CurrentLampCollection.Lamps[12] = new Lamp(LampType.AtcUse);
                CurrentLampCollection.Lamps[13] = new Lamp(LampType.AtcEmergency);
                Count = 14;
            }
            else if (atsP)
            {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
                CurrentLampCollection.Lamps[2] = new Lamp(LampType.None);
                CurrentLampCollection.Lamps[3] = new Lamp(LampType.AtsPPower);
                CurrentLampCollection.Lamps[4] = new Lamp(LampType.AtsPPattern);
                CurrentLampCollection.Lamps[5] = new Lamp(LampType.AtsPBrakeOverride);
                CurrentLampCollection.Lamps[6] = new Lamp(LampType.AtsPBrakeOperation);
                CurrentLampCollection.Lamps[7] = new Lamp(LampType.AtsP);
                CurrentLampCollection.Lamps[8] = new Lamp(LampType.AtsPFailure);
                Count = 9;
            }
            else if (atsSn & atc)
            {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
                CurrentLampCollection.Lamps[2] = new Lamp(LampType.None);
                CurrentLampCollection.Lamps[3] = new Lamp(LampType.Atc);
                CurrentLampCollection.Lamps[4] = new Lamp(LampType.AtcPower);
                CurrentLampCollection.Lamps[5] = new Lamp(LampType.AtcUse);
                CurrentLampCollection.Lamps[6] = new Lamp(LampType.AtcEmergency);
                Count = 7;
            }
            else if (atc)
            {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Atc);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtcPower);
                CurrentLampCollection.Lamps[2] = new Lamp(LampType.AtcUse);
                CurrentLampCollection.Lamps[3] = new Lamp(LampType.AtcEmergency);
                Count = 4;
            }
            else if (atsSn)
            {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
                Count = 2;
            }
            else
            {
                Count = 0;
            }
            if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin.IsDefault)
            {
                if (Count != 0 & (eb | TrainManager.PlayerTrain.Specs.HasConstSpeed))
                {
                    CurrentLampCollection.Lamps[Count] = new Lamp(LampType.None);
                    Count++;
                }
                if (eb)
                {
                    CurrentLampCollection.Lamps[Count] = new Lamp(LampType.Eb);
                    Count++;
                }
                if (TrainManager.PlayerTrain.Specs.HasConstSpeed)
                {
                    CurrentLampCollection.Lamps[Count] = new Lamp(LampType.ConstSpeed);
                    Count++;
                }
            }
            Array.Resize<Lamp>(ref CurrentLampCollection.Lamps, Count);
            for (int i = 0; i < Count; i++)
            {
                if (CurrentLampCollection.Lamps[i].Width > CurrentLampCollection.Width)
                {
                    CurrentLampCollection.Width = CurrentLampCollection.Lamps[i].Width;
                }
            }
        }
    }
}
