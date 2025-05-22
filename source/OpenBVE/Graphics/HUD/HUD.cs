using Formats.OpenBve;
using Microsoft.Win32;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenTK.Audio.OpenAL;
using SoundManager;
using System;
using System.Globalization;
using Vector2 = OpenBveApi.Math.Vector2;

namespace OpenBve
{
	internal static partial class HUD
	{
		/// <summary>The current HUD elements to render</summary>
		internal static Element[] CurrentHudElements = { };

		/// <summary>Sound source used by the accessibility station adjust tone</summary>
		internal static SoundSource stationAdjustBeepSource;
		/// <summary>Station adjust beep sound used by accessibility</summary>
		internal static SoundBuffer stationAdjustBeep;

		/// <summary>Loads the current HUD</summary>
		internal static void LoadHUD()
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string Folder = Program.FileSystem.GetDataFolder("In-game", Interface.CurrentOptions.UserInterfaceFolder);
			string File = OpenBveApi.Path.CombineFile(Folder, "interface.cfg");
			ConfigFile<HUDSection, HUDKey> cfg = new ConfigFile<HUDSection, HUDKey>(File, Program.CurrentHost);
			Program.CurrentHost.RegisterSound(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("In-game"), "beep.wav"), 50, out var beep);
			stationAdjustBeep = beep as SoundBuffer;
			CurrentHudElements = new Element[16];
			int Length = 0;
			string[] texturePath = null;
			while (cfg.RemainingSubBlocks > 0)
			{
				Block<HUDSection, HUDKey> block = cfg.ReadNextBlock();
				if (block.Key != HUDSection.Element)
				{
					Interface.AddMessage(MessageType.Error, false, "Unknown section encountered in HUD configuration in " + File);
					continue;
				}
				Length++;
				if (Length > CurrentHudElements.Length)
				{
					Array.Resize(ref CurrentHudElements, CurrentHudElements.Length << 1);
				}

				CurrentHudElements[Length - 1] = new Element();
				if (!block.GetEnumValue(HUDKey.Subject, out CurrentHudElements[Length -1].Subject))
				{
					continue;
				}
				block.TryGetVector2(HUDKey.Position, ',', ref CurrentHudElements[Length -1].Position);
				if (block.TryGetVector2(HUDKey.Alignment, ',', ref CurrentHudElements[Length - 1].Alignment))
				{
					CurrentHudElements[Length - 1].Alignment.X = Math.Sign(CurrentHudElements[Length - 1].Alignment.X);
					CurrentHudElements[Length - 1].Alignment.Y = Math.Sign(CurrentHudElements[Length - 1].Alignment.Y);
				}
				if (block.GetPathArray(HUDKey.TopLeft, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].TopLeft.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].TopLeft.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.TopMiddle, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].TopMiddle.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].TopMiddle.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.TopRight, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].TopRight.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].TopRight.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.CenterLeft, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].CenterLeft.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].CenterLeft.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.CenterMiddle, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].CenterMiddle.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].CenterMiddle.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.CenterRight, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].CenterRight.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].CenterRight.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.BottomLeft, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].BottomLeft.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].BottomLeft.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.BottomMiddle, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].BottomMiddle.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].BottomMiddle.OverlayTexture);
				}
				if (block.GetPathArray(HUDKey.BottomRight, ',', Folder, ref texturePath))
				{
					Program.Renderer.TextureManager.RegisterTexture(texturePath[0], out CurrentHudElements[Length - 1].BottomRight.BackgroundTexture);
					Program.Renderer.TextureManager.RegisterTexture(texturePath[1], out CurrentHudElements[Length - 1].BottomRight.OverlayTexture);
				}
				block.TryGetColor32(HUDKey.BackColor, ref CurrentHudElements[Length - 1].BackgroundColor);
				block.TryGetColor32(HUDKey.OverlayColor, ref CurrentHudElements[Length - 1].OverlayColor);
				block.TryGetColor32(HUDKey.TextColor, ref CurrentHudElements[Length - 1].TextColor);
				block.TryGetVector2(HUDKey.TextPosition, ',', ref CurrentHudElements[Length - 1].TextPosition);
				if (block.TryGetVector2(HUDKey.TextAlignment, ',', ref CurrentHudElements[Length - 1].TextAlignment))
				{
					CurrentHudElements[Length - 1].TextAlignment.X = Math.Sign(CurrentHudElements[Length - 1].TextAlignment.X);
					CurrentHudElements[Length - 1].TextAlignment.Y = Math.Sign(CurrentHudElements[Length - 1].TextAlignment.Y);
				}
				if (block.GetValue(HUDKey.TextSize, out int s))
				{
					s += Interface.CurrentOptions.UserInterfaceScaleFactor - 1;
					switch (s)
					{
						case 0:
							CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.VerySmallFont;
							break;
						case 1:
							CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.SmallFont;
							break;
						case 2:
							CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.NormalFont;
							break;
						case 3:
							CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.LargeFont;
							break;
						case 4:
							CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.VeryLargeFont;
							break;
						case 5:
							CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.EvenLargerFont;
							break;
						default:
							CurrentHudElements[Length - 1].Font = Program.Renderer.Fonts.NormalFont;
							break;
					}
				}
				if (block.GetValue(HUDKey.TextShadow, out s))
				{
					CurrentHudElements[Length - 1].TextShadow = s != 0;
				}
				block.TryGetValue(HUDKey.Text, ref CurrentHudElements[Length - 1].Text);
				double[] values = null;
				if (block.GetDoubleArray(HUDKey.Value, ',', ref values))
				{
					if (values.Length >= 1)
					{
						CurrentHudElements[Length - 1].Value1 = values[0];
					}
					if (values.Length >= 2)
					{
						CurrentHudElements[Length - 1].Value2 = values[1];
					}
				}
				block.GetEnumValue(HUDKey.Transition, out CurrentHudElements[Length - 1].Transition);
				block.GetVector2(HUDKey.TransitionVector, ',', out CurrentHudElements[Length - 1].TransitionVector);
			}
			Array.Resize(ref CurrentHudElements, Length);
		}
	}
}
