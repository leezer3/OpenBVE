using System;
using LibRender2.Texts;
using OpenBveApi.Interface;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
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
                OpenGlFont font = Fonts.NormalFont;
                for (int i = 0; i < HUD.CurrentHudElements.Length; i++)
                {
                    if (HUD.CurrentHudElements[i].Subject.Equals("ats", StringComparison.OrdinalIgnoreCase))
                    {
                        font = HUD.CurrentHudElements[i].Font;
                        break;
                    }
                }
                System.Drawing.Size size = font.MeasureString(this.Text);
                this.Width = size.Width;
                this.Height = size.Height;
            }
        }

        private class LampCollection
        {
	        internal readonly Lamp[] Lamps;
	        internal readonly float Width;

	        /// <summary>Initialises the ATS lamps for the specified train using one of the default safety systems</summary>
	        internal LampCollection(TrainManager.Train Train)
	        {
		        bool atsSn = (Train.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.AtsSn) != 0;
		        bool atsP = (Train.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.AtsP) != 0;
		        bool atc = (Train.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.Atc) != 0;
		        bool eb = (Train.Specs.DefaultSafetySystems & TrainManager.DefaultSafetySystems.Eb) != 0;
		        Width = 0.0f;
		        Lamps = new Lamp[17];
		        int Count;
		        if (Train.Plugin == null || !Train.Plugin.IsDefault)
		        {
			        Count = 0;
		        }
		        else if (atsP & atc)
		        {
			        Lamps[0] = new Lamp(LampType.Ats);
			        Lamps[1] = new Lamp(LampType.AtsOperation);
			        Lamps[2] = new Lamp(LampType.None);
			        Lamps[3] = new Lamp(LampType.AtsPPower);
			        Lamps[4] = new Lamp(LampType.AtsPPattern);
			        Lamps[5] = new Lamp(LampType.AtsPBrakeOverride);
			        Lamps[6] = new Lamp(LampType.AtsPBrakeOperation);
			        Lamps[7] = new Lamp(LampType.AtsP);
			        Lamps[8] = new Lamp(LampType.AtsPFailure);
			        Lamps[9] = new Lamp(LampType.None);
			        Lamps[10] = new Lamp(LampType.Atc);
			        Lamps[11] = new Lamp(LampType.AtcPower);
			        Lamps[12] = new Lamp(LampType.AtcUse);
			        Lamps[13] = new Lamp(LampType.AtcEmergency);
			        Count = 14;
		        }
		        else if (atsP)
		        {
			        Lamps[0] = new Lamp(LampType.Ats);
			        Lamps[1] = new Lamp(LampType.AtsOperation);
			        Lamps[2] = new Lamp(LampType.None);
			        Lamps[3] = new Lamp(LampType.AtsPPower);
			        Lamps[4] = new Lamp(LampType.AtsPPattern);
			        Lamps[5] = new Lamp(LampType.AtsPBrakeOverride);
			        Lamps[6] = new Lamp(LampType.AtsPBrakeOperation);
			        Lamps[7] = new Lamp(LampType.AtsP);
			        Lamps[8] = new Lamp(LampType.AtsPFailure);
			        Count = 9;
		        }
		        else if (atsSn & atc)
		        {
			        Lamps[0] = new Lamp(LampType.Ats);
			        Lamps[1] = new Lamp(LampType.AtsOperation);
			        Lamps[2] = new Lamp(LampType.None);
			        Lamps[3] = new Lamp(LampType.Atc);
			        Lamps[4] = new Lamp(LampType.AtcPower);
			        Lamps[5] = new Lamp(LampType.AtcUse);
			        Lamps[6] = new Lamp(LampType.AtcEmergency);
			        Count = 7;
		        }
		        else if (atc)
		        {
			        Lamps[0] = new Lamp(LampType.Atc);
			        Lamps[1] = new Lamp(LampType.AtcPower);
			        Lamps[2] = new Lamp(LampType.AtcUse);
			        Lamps[3] = new Lamp(LampType.AtcEmergency);
			        Count = 4;
		        }
		        else if (atsSn)
		        {
			        Lamps[0] = new Lamp(LampType.Ats);
			        Lamps[1] = new Lamp(LampType.AtsOperation);
			        Count = 2;
		        }
		        else
		        {
			        Count = 0;
		        }

		        if (Train.Plugin != null && Train.Plugin.IsDefault)
		        {
			        if (Count != 0 & (eb | Train.Specs.HasConstSpeed))
			        {
				        Lamps[Count] = new Lamp(LampType.None);
				        Count++;
			        }

			        if (eb)
			        {
				        Lamps[Count] = new Lamp(LampType.Eb);
				        Count++;
			        }

			        if (Train.Specs.HasConstSpeed)
			        {
				        Lamps[Count] = new Lamp(LampType.ConstSpeed);
				        Count++;
			        }
		        }

		        Array.Resize<Lamp>(ref Lamps, Count);
		        for (int i = 0; i < Count; i++)
		        {
			        if (Lamps[i].Width > Width)
			        {
				        Width = Lamps[i].Width;
			        }
		        }
	        }
        }

        private LampCollection CurrentLampCollection;

		
    }
}
