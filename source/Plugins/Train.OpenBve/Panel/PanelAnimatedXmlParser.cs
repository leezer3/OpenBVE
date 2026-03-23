using Formats.OpenBve;
using Formats.OpenBve.XML;
using LibRender2;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
	class PanelAnimatedXmlParser
	{

		internal Plugin Plugin;

		internal PanelAnimatedXmlParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		/// <summary>Parses a openBVE panel.animated.xml file</summary>
		/// <param name="PanelFile">The relative path of the panel configuration file from the train</param>
		/// <param name="Train">The train</param>
		/// <param name="Car">The car index to add the panel to</param>
		internal void ParsePanelAnimatedXml(string PanelFile, TrainBase Train, int Car)
		{
			// The current XML file to load
			string FileName = PanelFile;
			if (!File.Exists(FileName))
			{
				FileName = Path.CombineFile(Train.TrainFolder, PanelFile);
			}


			XMLFile<Panel2Sections, Panel2Key> xmlFile = new XMLFile<Panel2Sections, Panel2Key>(FileName, "/openBVE/PanelAnimated", Plugin.CurrentHost);
			while (xmlFile.RemainingSubBlocks > 0)
			{
				Block<Panel2Sections, Panel2Key> subBlock = xmlFile.ReadNextBlock();
				ParsePanelAnimatedNode(subBlock, Train.TrainFolder, Train.Cars[Car].CarSections[CarSectionType.Interior], 0);
			}
			xmlFile.ReportErrors();
		}

		private int currentSectionElement = 0;

		private void ParsePanelAnimatedNode(Block<Panel2Sections, Panel2Key> panelAnimatedBlock, string TrainPath, CarSection CarSection, int GroupIndex)
		{
			
			double invfac = panelAnimatedBlock.RemainingSubBlocks == 0 ? 0.4 : 0.4 / panelAnimatedBlock.RemainingSubBlocks;
			switch(panelAnimatedBlock.Key)
			{
				case Panel2Sections.Group:
				if (GroupIndex == 0)
				{
					if (!panelAnimatedBlock.GetValue(Panel2Key.Number, out int n, NumberRange.NonNegative))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PanelAnimated: The group number is invalid.");
						break;
					}

					if (n + 1 >= CarSection.Groups.Length)
					{
						Array.Resize(ref CarSection.Groups, n + 2);
						CarSection.Groups[n + 1] = new ElementsGroup();
					}

					while (panelAnimatedBlock.RemainingSubBlocks > 0)
					{
						Block<Panel2Sections, Panel2Key> subBlock = panelAnimatedBlock.ReadNextBlock();
						ParsePanelAnimatedNode(subBlock, TrainPath, CarSection, n + 1);
					}
				}
				break;
				case Panel2Sections.Touch:
				if (GroupIndex >= 0)
				{
					Vector3 Position = Vector3.Zero;
					Vector3 Size = Vector3.Zero;
					int JumpScreen = GroupIndex - 1;
					List<int> SoundIndices = new List<int>();
					List<CommandEntry> CommandEntries = new List<CommandEntry>();
					CommandEntry CommandEntry = new CommandEntry();
					Bitmap cursorTexture = null;
					panelAnimatedBlock.TryGetVector3(Panel2Key.Position, ',', ref Position);
					panelAnimatedBlock.TryGetVector3(Panel2Key.Size, ',', ref Size);
					panelAnimatedBlock.TryGetValue(Panel2Key.JumpScreen, ref JumpScreen);
					if (panelAnimatedBlock.GetValue(Panel2Key.SoundIndex, out int soundIndex, NumberRange.NonNegative))
					{
						SoundIndices.Add(soundIndex);
					}

					if (panelAnimatedBlock.GetEnumValue(Panel2Key.Command, out Translations.Command command))
					{
						if (!CommandEntries.Contains(CommandEntry))
						{
							CommandEntries.Add(CommandEntry);
						}

						CommandEntry.Command = command;
					}

					panelAnimatedBlock.TryGetValue(Panel2Key.CommandOption, ref CommandEntry.Option);
					if (panelAnimatedBlock.ReadBlock(Panel2Sections.SoundEntries, out Block<Panel2Sections, Panel2Key> soundEntriesBlock))
					{
						ParseTouchSoundEntryNode(soundEntriesBlock, SoundIndices);
					}

					if (panelAnimatedBlock.ReadBlock(Panel2Sections.CommandEntries,
						    out Block<Panel2Sections, Panel2Key> commandEntriesBlock))
					{
						ParseTouchCommandEntryNode(commandEntriesBlock, CommandEntries);
					}

					if (panelAnimatedBlock.GetPath(Panel2Key.Cursor, TrainPath, out string cursorFile))
					{
						cursorTexture = (Bitmap)Image.FromFile(cursorFile);
					}

					CreateTouchElement(CarSection.Groups[GroupIndex], Position, Size, JumpScreen,
						SoundIndices.ToArray(), CommandEntries.ToArray());
					if (cursorTexture != null)
					{
						CarSection.Groups[GroupIndex]
								.TouchElements[CarSection.Groups[GroupIndex].TouchElements.Length - 1].MouseCursor =
							new MouseCursor(Plugin.Renderer, string.Empty, cursorTexture);
					}
				}

				break;
				case Panel2Sections.Include:
				if (panelAnimatedBlock.GetPath(Panel2Key.FileName, TrainPath, out string includeFile))
				{
					System.Text.Encoding e = TextEncoding.GetSystemEncodingFromFile(includeFile);
					Plugin.CurrentHost.LoadObject(includeFile, e, out var currentObject);
					var a = (AnimatedObjectCollection)currentObject;
					if (a != null)
					{
						for (int i = 0; i < a.Objects.Length; i++)
						{
							Plugin.CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
						}

						CarSection.Groups[GroupIndex].Elements = a.Objects;
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false,
							"PanelAnimated: Include file " + includeFile + " was not a valid AnimatedObject.");
					}
				}

				break;
			}
			panelAnimatedBlock.ReportErrors();
			Plugin.CurrentProgress = Plugin.LastProgress + invfac * currentSectionElement;
			if ((currentSectionElement & 4) == 0)
			{
				System.Threading.Thread.Sleep(1);
				if (Plugin.Cancel) return;
			}

			currentSectionElement++;
		}

		private static void ParseTouchSoundEntryNode(Block<Panel2Sections, Panel2Key> soundEntriesBlock, ICollection<int> indices)
		{
			while (soundEntriesBlock.RemainingSubBlocks > 0)
			{
				Block<Panel2Sections, Panel2Key> soundEntryBlock = soundEntriesBlock.ReadNextBlock();
				if (soundEntryBlock.Key != Panel2Sections.Entry)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PanelAnimated: Invalid entry node " + soundEntryBlock.Key);
					continue;
				}
				if (soundEntryBlock.GetValue(Panel2Key.Index, out int idx, NumberRange.NonNegative))
				{
					indices.Add(idx);
				}
			}
			soundEntriesBlock.ReportErrors();
		}

		private static void ParseTouchCommandEntryNode(Block<Panel2Sections, Panel2Key> commandEntriesBlock, ICollection<CommandEntry> entries)
		{
			while (commandEntriesBlock.RemainingSubBlocks > 0)
			{
				Block<Panel2Sections, Panel2Key> commandEntryBlock = commandEntriesBlock.ReadNextBlock();
				if (commandEntryBlock.Key != Panel2Sections.Entry)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "PanelAnimated: Invalid entry node " + commandEntryBlock.Key);
					continue;
				}

				if (commandEntryBlock.GetEnumValue(Panel2Key.Name, out Translations.Command command))
				{
					CommandEntry entry = new CommandEntry();
					entry.Command = command;
					commandEntriesBlock.GetValue(Panel2Key.Option, out entry.Option);
					entries.Add(entry);
				}
			}
			commandEntriesBlock.ReportErrors();
		}

		private void CreateTouchElement(ElementsGroup Group, Vector3 Position, Vector3 Size, int ScreenIndex, int[] SoundIndices, CommandEntry[] CommandEntries)
		{
			Vertex t0 = new Vertex(Size.X, Size.Y, -Size.Z);
            Vertex t1 = new Vertex(Size.X, -Size.Y, -Size.Z);
            Vertex t2 = new Vertex(-Size.X, -Size.Y, -Size.Z);
            Vertex t3 = new Vertex(-Size.X, Size.Y, -Size.Z);
            Vertex t4 = new Vertex(Size.X, Size.Y, Size.Z);
            Vertex t5 = new Vertex(Size.X, -Size.Y, Size.Z);
            Vertex t6 = new Vertex(-Size.X, -Size.Y, Size.Z);
            Vertex t7 = new Vertex(-Size.X, Size.Y, Size.Z);
			StaticObject Object = new StaticObject(Plugin.CurrentHost);
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3, t4, t5, t6, t7 };
            Object.Mesh.Faces = new[] { new MeshFace(new[] { 0, 1, 2, 3 }), new MeshFace(new[] { 0, 4, 5, 1 }), new MeshFace(new[] { 0, 3, 7, 4 }), new MeshFace(new[] { 6, 5, 4, 7 }), new MeshFace(new[] { 6, 7, 3, 2 }), new MeshFace(new[] { 6, 2, 1, 5 }) };
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = 0;
			Object.Mesh.Materials[0].Color = Color32.White;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = null;
			Object.Mesh.Materials[0].NighttimeTexture = null;
			Object.Dynamic = true;
			if (Group.TouchElements == null)
			{
				Group.TouchElements = new TouchElement[0];
			}
			int n = Group.TouchElements.Length;
			int m = Plugin.CurrentControls.Length;
			Array.Resize(ref Plugin.CurrentControls, m + CommandEntries.Length);
			int[] controlIndicies = new int[CommandEntries.Length];
			for (int i = 0; i < CommandEntries.Length; i++)
			{
				Plugin.CurrentControls[m + i].Command = CommandEntries[i].Command;
				Plugin.CurrentControls[m + i].Method = ControlMethod.Touch;
				Plugin.CurrentControls[m + i].Option = CommandEntries[i].Option;
				controlIndicies[i] = m + i;
			}
			Array.Resize(ref Group.TouchElements, n + 1);
			Group.TouchElements[n] = new TouchElement(new AnimatedObject(Plugin.CurrentHost, Object), ScreenIndex, SoundIndices, controlIndicies);
			Group.TouchElements[n].Element.States[0].Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			Plugin.CurrentHost.CreateDynamicObject(ref Group.TouchElements[n].Element.internalObject);
			
		}
	}
}
