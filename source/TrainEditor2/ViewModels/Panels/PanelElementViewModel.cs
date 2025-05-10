﻿using System.Globalization;
using OpenBveApi.Math;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal abstract class PanelElementViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> LocationX
		{
			get;
		}

		internal ReactiveProperty<string> LocationY
		{
			get;
		}

		internal ReactiveProperty<int> Layer
		{
			get;
		}

		internal PanelElementViewModel(PanelElement element)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			LocationX = element
				.ToReactivePropertyAsSynchronized(
					x => x.Location,
					x => x.X.ToString(culture),
					x => new Vector2(double.Parse(x, NumberStyles.Float, culture), element.Location.Y),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryParse(x, NumberRange.Any, out double result, out string message);

					return message;
				})
				.AddTo(disposable);

			LocationY = element
				.ToReactivePropertyAsSynchronized(
					x => x.Location,
					x => x.Y.ToString(culture),
					x => new Vector2(element.Location.X, double.Parse(x, NumberStyles.Float, culture)),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryParse(x, NumberRange.Any, out double result, out string message);

					return message;
				})
				.AddTo(disposable);

			Layer = element
				.ToReactivePropertyAsSynchronized(x => x.Layer)
				.AddTo(disposable);
		}
	}
}
