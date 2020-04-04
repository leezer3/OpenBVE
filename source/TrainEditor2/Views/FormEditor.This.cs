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
		private IDisposable BindToThis(ThisViewModel _this)
		{
			CompositeDisposable thisDisposable = new CompositeDisposable();

			_this.Resolution
				.BindTo(
					textBoxThisResolution,
					x => x.Text,
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

			_this.Resolution
				.BindToErrorProvider(errorProvider, textBoxThisResolution)
				.AddTo(thisDisposable);

			_this.Left
				.BindTo(
					textBoxThisLeft,
					x => x.Text,
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

			_this.Left
				.BindToErrorProvider(errorProvider, textBoxThisLeft)
				.AddTo(thisDisposable);

			_this.Right
				.BindTo(
					textBoxThisRight,
					x => x.Text,
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

			_this.Right
				.BindToErrorProvider(errorProvider, textBoxThisRight)
				.AddTo(thisDisposable);

			_this.Top
				.BindTo(
					textBoxThisTop,
					x => x.Text,
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

			_this.Top
				.BindToErrorProvider(errorProvider, textBoxThisTop)
				.AddTo(thisDisposable);

			_this.Bottom
				.BindTo(
					textBoxThisBottom,
					x => x.Text,
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

			_this.Bottom
				.BindToErrorProvider(errorProvider, textBoxThisBottom)
				.AddTo(thisDisposable);

			_this.DaytimeImage
				.BindTo(
					textBoxThisDaytimeImage,
					x => x.Text,
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

			_this.NighttimeImage
				.BindTo(
					textBoxThisNighttimeImage,
					x => x.Text,
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

			_this.TransparentColor
				.BindTo(
					textBoxThisTransparentColor,
					x => x.Text,
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

			_this.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxThisTransparentColor)
				.AddTo(thisDisposable);

			_this.CenterX
				.BindTo(
					textBoxThisCenterX,
					x => x.Text,
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

			_this.CenterX
				.BindToErrorProvider(errorProvider, textBoxThisCenterX)
				.AddTo(thisDisposable);

			_this.CenterY
				.BindTo(
					textBoxThisCenterY,
					x => x.Text,
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

			_this.CenterY
				.BindToErrorProvider(errorProvider, textBoxThisCenterY)
				.AddTo(thisDisposable);

			_this.OriginX
				.BindTo(
					textBoxThisOriginX,
					x => x.Text,
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

			_this.OriginX
				.BindToErrorProvider(errorProvider, textBoxThisOriginX)
				.AddTo(thisDisposable);

			_this.OriginY
				.BindTo(
					textBoxThisOriginY,
					x => x.Text,
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

			_this.OriginY
				.BindToErrorProvider(errorProvider, textBoxThisOriginY)
				.AddTo(thisDisposable);

			return thisDisposable;
		}
	}
}
