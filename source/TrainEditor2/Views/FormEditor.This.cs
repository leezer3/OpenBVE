using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToThis(ThisViewModel y)
		{
			CompositeDisposable thisDisposable = new CompositeDisposable();

			y.Resolution
				.BindTo(
					textBoxThisResolution,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisResolution.TextChanged += h,
							h => textBoxThisResolution.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.Resolution
				.BindToErrorProvider(errorProvider, textBoxThisResolution)
				.AddTo(thisDisposable);

			y.Left
				.BindTo(
					textBoxThisLeft,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisLeft.TextChanged += h,
							h => textBoxThisLeft.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.Left
				.BindToErrorProvider(errorProvider, textBoxThisLeft)
				.AddTo(thisDisposable);

			y.Right
				.BindTo(
					textBoxThisRight,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisRight.TextChanged += h,
							h => textBoxThisRight.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.Right
				.BindToErrorProvider(errorProvider, textBoxThisRight)
				.AddTo(thisDisposable);

			y.Top
				.BindTo(
					textBoxThisTop,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisTop.TextChanged += h,
							h => textBoxThisTop.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.Top
				.BindToErrorProvider(errorProvider, textBoxThisTop)
				.AddTo(thisDisposable);

			y.Bottom
				.BindTo(
					textBoxThisBottom,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisBottom.TextChanged += h,
							h => textBoxThisBottom.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.Bottom
				.BindToErrorProvider(errorProvider, textBoxThisBottom)
				.AddTo(thisDisposable);

			y.DaytimeImage
				.BindTo(
					textBoxThisDaytimeImage,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisDaytimeImage.TextChanged += h,
							h => textBoxThisDaytimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.NighttimeImage
				.BindTo(
					textBoxThisNighttimeImage,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisNighttimeImage.TextChanged += h,
							h => textBoxThisNighttimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.TransparentColor
				.BindTo(
					textBoxThisTransparentColor,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisTransparentColor.TextChanged += h,
							h => textBoxThisTransparentColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxThisTransparentColor)
				.AddTo(thisDisposable);

			y.CenterX
				.BindTo(
					textBoxThisCenterX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisCenterX.TextChanged += h,
							h => textBoxThisCenterX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.CenterX
				.BindToErrorProvider(errorProvider, textBoxThisCenterX)
				.AddTo(thisDisposable);

			y.CenterY
				.BindTo(
					textBoxThisCenterY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisCenterY.TextChanged += h,
							h => textBoxThisCenterY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.CenterY
				.BindToErrorProvider(errorProvider, textBoxThisCenterY)
				.AddTo(thisDisposable);

			y.OriginX
				.BindTo(
					textBoxThisOriginX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisOriginX.TextChanged += h,
							h => textBoxThisOriginX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.OriginX
				.BindToErrorProvider(errorProvider, textBoxThisOriginX)
				.AddTo(thisDisposable);

			y.OriginY
				.BindTo(
					textBoxThisOriginY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxThisOriginY.TextChanged += h,
							h => textBoxThisOriginY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(thisDisposable);

			y.OriginY
				.BindToErrorProvider(errorProvider, textBoxThisOriginY)
				.AddTo(thisDisposable);

			return thisDisposable;
		}
	}
}
