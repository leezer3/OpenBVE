using System;
using OpenBveApi.Interface;
using Prism.Mvvm;

namespace TrainEditor2.Models.Panels
{
	internal class TouchElement : BindableBase, ICloneable
	{
		private double locationX;
		private double locationY;
		private double sizeX;
		private double sizeY;
		private int jumpScreen;
		private int soundIndex;
		private Translations.CommandInfo commandInfo;
		private int commandOption;

		internal double LocationX
		{
			get
			{
				return locationX;
			}
			set
			{
				SetProperty(ref locationX, value);
			}
		}

		internal double LocationY
		{
			get
			{
				return locationY;
			}
			set
			{
				SetProperty(ref locationY, value);
			}
		}

		internal double SizeX
		{
			get
			{
				return sizeX;
			}
			set
			{
				SetProperty(ref sizeX, value);
			}
		}

		internal double SizeY
		{
			get
			{
				return sizeY;
			}
			set
			{
				SetProperty(ref sizeY, value);
			}
		}

		internal int JumpScreen
		{
			get
			{
				return jumpScreen;
			}
			set
			{
				SetProperty(ref jumpScreen, value);
			}
		}

		internal int SoundIndex
		{
			get
			{
				return soundIndex;
			}
			set
			{
				SetProperty(ref soundIndex, value);
			}
		}

		internal Translations.CommandInfo CommandInfo
		{
			get
			{
				return commandInfo;
			}
			set
			{
				SetProperty(ref commandInfo, value);
			}
		}

		internal int CommandOption
		{
			get
			{
				return commandOption;
			}
			set
			{
				SetProperty(ref commandOption, value);
			}
		}

		internal TouchElement(Screen screen)
		{
			LocationX = 0.0;
			LocationY = 0.0;
			SizeX = 0.0;
			SizeY = 0.0;
			JumpScreen = screen.Number;
			SoundIndex = -1;
			CommandInfo = Translations.CommandInfos.TryGetInfo(Translations.Command.None);
			CommandOption = 0;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
