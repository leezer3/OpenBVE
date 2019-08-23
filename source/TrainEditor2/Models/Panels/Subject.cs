using System;
using System.Globalization;
using OpenBveApi.Interface;
using Prism.Mvvm;
using TrainEditor2.Systems;

namespace TrainEditor2.Models.Panels
{
	internal enum SubjectBase
	{
		Acc,
		Atc,
		Ats,
		Bc,
		LocoBrakeCylinder,
		Bp,
		LocoBrakePipe,
		Brake,
		LocoBrake,
		Csc,
		Door,
		DoorL,
		DoorR,
		DoorButtonL,
		DoorButtonR,
		Er,
		Hour,
		Kmph,
		Min,
		Motor,
		Mph,
		Mr,
		Ms,
		Power,
		Rev,
		Sap,
		Sec,
		True,
		Klaxon,
		PrimaryKlaxon,
		SecondaryKlaxon,
		MusicKlaxon
	}

	internal enum SubjectSuffix
	{
		None,
		D
	}

	internal class Subject : BindableBase, ICloneable
	{
		private SubjectBase _base;
		private int baseOption;
		private SubjectSuffix suffix;
		private int suffixOption;

		internal SubjectBase Base
		{
			get
			{
				return _base;
			}
			set
			{
				SetProperty(ref _base, value);
			}
		}

		internal int BaseOption
		{
			get
			{
				return baseOption;
			}
			set
			{
				SetProperty(ref baseOption, value);
			}
		}

		internal SubjectSuffix Suffix
		{
			get
			{
				return suffix;
			}
			set
			{
				SetProperty(ref suffix, value);
			}
		}

		internal int SuffixOption
		{
			get
			{
				return suffixOption;
			}
			set
			{
				SetProperty(ref suffixOption, value);
			}
		}

		internal Subject()
		{
			Base = SubjectBase.True;
			BaseOption = -1;
			Suffix = SubjectSuffix.None;
			SuffixOption = -1;
		}

		public override string ToString()
		{
			string result = Base.ToString();

			switch (Base)
			{
				case SubjectBase.Ats:
				case SubjectBase.DoorL:
				case SubjectBase.DoorR:
					if (BaseOption >= 0)
					{
						result += BaseOption;
					}
					break;
			}

			if (Suffix != SubjectSuffix.None)
			{
				result += Suffix.ToString();

				if (SuffixOption >= 0)
				{
					result += SuffixOption;
				}
			}

			return result;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		internal static Subject StringToSubject(string value, string errorLocation)
		{
			Subject result = new Subject();

			CultureInfo culture = CultureInfo.InvariantCulture;

			// detect d# suffix
			{
				int i;

				for (i = value.Length - 1; i >= 0; i--)
				{
					int a = char.ConvertToUtf32(value, i);

					if (a < 48 | a > 57)
					{
						break;
					}
				}

				if (i >= 0 & i < value.Length - 1)
				{
					if (value[i] == 'd' | value[i] == 'D')
					{
						result.Suffix = SubjectSuffix.D;
						int n;

						if (int.TryParse(value.Substring(i + 1), NumberStyles.Integer, culture, out n))
						{
							result.SuffixOption = n;
							value = value.Substring(0, i);
						}
					}
				}
			}

			// transform subject
			SubjectBase _base;
			bool ret = Enum.TryParse(value, true, out _base);
			result.Base = _base;

			if (!ret)
			{
				bool unsupported = true;

				if (value.StartsWith("ats", StringComparison.OrdinalIgnoreCase))
				{
					string a = value.Substring(3);
					int n;

					if (int.TryParse(a, NumberStyles.Integer, culture, out n))
					{
						result.Base = SubjectBase.Ats;
						result.BaseOption = n;
						unsupported = false;
					}
				}
				else if (value.StartsWith("doorl", StringComparison.OrdinalIgnoreCase))
				{
					string a = value.Substring(5);
					int n;

					if (int.TryParse(a, NumberStyles.Integer, culture, out n))
					{
						result.Base = SubjectBase.DoorL;
						result.BaseOption = n;
						unsupported = false;
					}
				}
				else if (value.StartsWith("doorr", StringComparison.OrdinalIgnoreCase))
				{
					string a = value.Substring(5);
					int n;

					if (int.TryParse(a, NumberStyles.Integer, culture, out n))
					{
						result.Base = SubjectBase.DoorR;
						result.BaseOption = n;
						unsupported = false;
					}
				}

				if (unsupported)
				{
					result.Base = SubjectBase.True;
					Interface.AddMessage(MessageType.Error, false, $"Invalid subject {value} encountered in {errorLocation}");
				}
			}

			return result;
		}
	}
}
