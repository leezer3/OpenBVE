using System;
using LibRender2.Text;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainManager.SafetySystems;
using TrainManager.Trains;

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
        private readonly struct Lamp
        {
            internal readonly LampType Type;
            internal readonly string Text;
            internal readonly double Width;
            internal readonly double Height;
            internal Lamp(LampType Type)
            {
                this.Type = Type;
                switch (Type)
                {
                    case LampType.None:
                        Text = null;
                        break;
                    case LampType.Ats:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","ats"});
                        break;
                    case LampType.AtsOperation:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atsoperation"});
                        break;
                    case LampType.AtsPPower:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atsppower"});
                        break;
                    case LampType.AtsPPattern:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atsppattern"});
                        break;
                    case LampType.AtsPBrakeOverride:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atspbrakeoverride"});
                        break;
                    case LampType.AtsPBrakeOperation:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atspbrakeoperation"});
                        break;
                    case LampType.AtsP:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atsp"});
                        break;
                    case LampType.AtsPFailure:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atspfailure"});
                        break;
                    case LampType.Atc:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atc"});
                        break;
                    case LampType.AtcPower:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atcpower"});
                        break;
                    case LampType.AtcUse:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atcuse"});
                        break;
                    case LampType.AtcEmergency:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","atcemergency"});
                        break;
                    case LampType.Eb:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","eb"});
                        break;
                    case LampType.ConstSpeed:
                        Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"lamps","constspeed"});
                        break;
                    default:
                        Text = "TEXT";
                        break;
                }
                OpenGlFont font = Program.Renderer.Fonts.NormalFont;
                for (int i = 0; i < HUD.CurrentHudElements.Length; i++)
                {
                    if (HUD.CurrentHudElements[i].Subject == HUDSubject.ATS)
                    {
                        font = HUD.CurrentHudElements[i].Font;
                        break;
                    }
                }
                Vector2 size = font.MeasureString(Text);
				Width = size.X;
                Height = size.Y;
            }
        }

        private class LampCollection
        {
	        internal readonly Lamp[] Lamps;
	        internal readonly double Width;

	        /// <summary>Initialises the ATS lamps for the specified train using one of the default safety systems</summary>
	        internal LampCollection(TrainBase Train)
	        {
		        bool atsSn = (Train.Specs.DefaultSafetySystems & DefaultSafetySystems.AtsSn) != 0;
		        bool atsP = (Train.Specs.DefaultSafetySystems & DefaultSafetySystems.AtsP) != 0;
		        bool atc = (Train.Specs.DefaultSafetySystems & DefaultSafetySystems.Atc) != 0;
		        bool eb = (Train.Specs.DefaultSafetySystems & DefaultSafetySystems.Eb) != 0;
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

		        Array.Resize(ref Lamps, Count);
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
