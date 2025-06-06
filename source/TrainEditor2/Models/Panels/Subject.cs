using System;
using System.Globalization;
using Formats.OpenBve;
using OpenBveApi.Interface;
using TrainEditor2.Extensions;
using TrainEditor2.Systems;

namespace TrainEditor2.Models.Panels
{
	internal enum SubjectSuffix
	{
		None,
		D
	}

	internal class Subject : BindableBase, ICloneable
	{
		private Panel2Subject _base;
		private int baseOption;
		private SubjectSuffix suffix;
		private int suffixOption;

		internal Panel2Subject Base
		{
			get => _base;
			set => SetProperty(ref _base, value);
		}

		internal int BaseOption
		{
			get => baseOption;
			set => SetProperty(ref baseOption, value);
		}

		internal SubjectSuffix Suffix
		{
			get => suffix;
			set => SetProperty(ref suffix, value);
		}

		internal int SuffixOption
		{
			get => suffixOption;
			set => SetProperty(ref suffixOption, value);
		}

		internal Subject()
		{
			Base = Panel2Subject.True;
			BaseOption = -1;
			Suffix = SubjectSuffix.None;
			SuffixOption = -1;
		}

		public override string ToString()
		{
			string result = Base.ToString();

			switch (Base)
			{
				case Panel2Subject.ATS:
				case Panel2Subject.DoorL:
				case Panel2Subject.DoorR:
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

		internal static Subject StringToSubject(string value, Panel2Sections errorLocation)
		{
			return StringToSubject(value, errorLocation.ToString());
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

						if (int.TryParse(value.Substring(i + 1), NumberStyles.Integer, culture, out int n))
						{
							result.SuffixOption = n;
							value = value.Substring(0, i);
						}
					}
				}
			}

			// transform subject
			bool ret = Enum.TryParse(value, true, out Panel2Subject _base);
			result.Base = _base;

			if (!ret)
			{
				bool unsupported = true;

				if (value.StartsWith("ats", StringComparison.OrdinalIgnoreCase))
				{
					string a = value.Substring(3);

					if (int.TryParse(a, NumberStyles.Integer, culture, out int n))
					{
						result.Base = Panel2Subject.ATS;
						result.BaseOption = n;
						unsupported = false;
					}
				}
				else if (value.StartsWith("doorl", StringComparison.OrdinalIgnoreCase))
				{
					string a = value.Substring(5);

					if (int.TryParse(a, NumberStyles.Integer, culture, out int n))
					{
						result.Base = Panel2Subject.DoorL;
						result.BaseOption = n;
						unsupported = false;
					}
				}
				else if (value.StartsWith("doorr", StringComparison.OrdinalIgnoreCase))
				{
					string a = value.Substring(5);

					if (int.TryParse(a, NumberStyles.Integer, culture, out int n))
					{
						result.Base = Panel2Subject.DoorR;
						result.BaseOption = n;
						unsupported = false;
					}
				}

				if (unsupported)
				{
					result.Base = Panel2Subject.True;
					Interface.AddMessage(MessageType.Error, false, $"Invalid subject {value} encountered in {errorLocation}");
				}
			}

			return result;
		}
	}
}
