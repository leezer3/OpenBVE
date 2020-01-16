using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.ViewModels.Others;

namespace TrainEditor2.Extensions
{
	internal static class Binders
	{
		internal static IDisposable BindToButton(this ReactiveCommand command, Button button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton(this IEnumerable<ReactiveCommand> commands, Button button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			ReactiveCommand[] commandsArray = commands.ToArray();

			commandsArray
				.Select(x => x.CanExecuteChangedAsObservable())
				.Merge()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = commandsArray.Select(x => x.CanExecute()).Any(x => x))
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ =>
				{
					foreach (ReactiveCommand command in commandsArray.Where(x => x.CanExecute()).ToArray())
					{
						command.Execute();
					}
				})
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton(this ReactiveCommand command, ToolStripMenuItem button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
				h => (s, e) => h(e),
				h => button.Click += h,
				h => button.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton<T>(this ReactiveCommand<T> command, T value, ToolStripMenuItem button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute(value))
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton(this ReactiveCommand command, ToolStripButton button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToButton<T>(this ReactiveCommand<T> command, T value, ToolStripButton button)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => button.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => button.Click += h,
					h => button.Click -= h
				)
				.Subscribe(_ => command.Execute(value))
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToCheckBox(this ReactiveCommand command, CheckBox checkBox)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			command
				.CanExecuteChangedAsObservable()
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
				.Subscribe(_ => checkBox.Enabled = command.CanExecute())
				.AddTo(disposable);

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => checkBox.Click += h,
					h => checkBox.Click -= h
				)
				.Subscribe(_ => command.Execute())
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToErrorProvider<T>(this ReactiveProperty<T> property, ErrorProvider errorProvider, Control control)
		{
			return property.ObserveErrorChanged.Subscribe(x => errorProvider.SetError(control, x?.OfType<string>().FirstOrDefault(y => true)));
		}

		private class DictionaryDisposable<T> : Dictionary<T, IDisposable>, IDisposable
		{
			public void Dispose()
			{
				foreach (IDisposable value in Values)
				{
					value.Dispose();
				}
			}
		}

		internal static IDisposable BindToTreeView(TreeView treeView, ReadOnlyReactiveCollection<TreeViewItemViewModel> items, ReactiveProperty<TreeViewItemViewModel> selectedItem)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			BindToTreeNodeCollection(treeView, items, treeView.Nodes).AddTo(disposable);

			bool sourceUpdating = false;
			bool targetUpdating = false;

			Observable.FromEvent<TreeViewEventHandler, TreeViewEventArgs>(
					h => (s, e) => h(e),
					h => treeView.AfterSelect += h,
					h => treeView.AfterSelect -= h
				)
				.ToUnit()
				.Subscribe(_ =>
				{
					if (sourceUpdating)
					{
						return;
					}

					targetUpdating = true;

					try
					{
						selectedItem.Value = treeView.SelectedNode?.Tag as TreeViewItemViewModel;
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
						selectedItem.Value = null;
					}

					targetUpdating = false;
				})
				.AddTo(disposable);

			selectedItem
				.Subscribe(x =>
				{
					if (targetUpdating)
					{
						return;
					}

					sourceUpdating = true;

					try
					{
						treeView.SelectedNode = treeView.Nodes.OfType<TreeNode>().Select(y => SearchTreeNode(x, y)).FirstOrDefault(y => y != null);
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
						treeView.SelectedNode = null;
					}

					sourceUpdating = false;
				})
				.AddTo(disposable);

			return disposable;
		}

		private static IDisposable BindToTreeNodeCollection(TreeView treeView, ReadOnlyReactiveCollection<TreeViewItemViewModel> items, TreeNodeCollection collection)
		{
			CompositeDisposable disposable = new CompositeDisposable();
			DictionaryDisposable<TreeViewItemViewModel> disposables = new DictionaryDisposable<TreeViewItemViewModel>().AddTo(disposable);

			TreeNode selectedNode = null;

			collection.Clear();
			disposables.AddRange(
				items.ToDictionary(
					x => x,
					x =>
					{
						TreeNode node = new TreeNode { Tag = x };
						collection.Add(node);

						if (!node.IsExpanded)
						{
							treeView.ExpandAll();
						}

						return BindToTreeNode(treeView, x, node);
					})
			);

			items
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Add)
				.Subscribe(x =>
				{
					TreeViewItemViewModel item = (TreeViewItemViewModel)x.NewItems[0];
					TreeNode node = new TreeNode { Tag = item };
					collection.Insert(x.NewStartingIndex, node);
					disposables.Add(item, BindToTreeNode(treeView, item, node));

					if (!node.IsExpanded)
					{
						treeView.ExpandAll();
					}

					if (selectedNode != null)
					{
						if (item == selectedNode.Tag)
						{
							treeView.SelectedNode = node;
							selectedNode = null;
						}
					}
				})
				.AddTo(disposable);

			items
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Move)
				.Subscribe(x =>
				{
					TreeNode node = collection[x.OldStartingIndex];
					collection.Remove(node);
					collection.Insert(x.NewStartingIndex, node);
				})
				.AddTo(disposable);

			items
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Remove)
				.Subscribe(x =>
				{
					TreeViewItemViewModel item = (TreeViewItemViewModel)x.OldItems[0];
					selectedNode = collection[x.OldStartingIndex] == treeView.SelectedNode ? collection[x.OldStartingIndex] : null;
					collection.RemoveAt(x.OldStartingIndex);
					disposables[item].Dispose();
					disposables.Remove(item);
				})
				.AddTo(disposable);

			items
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Replace)
				.Subscribe(x =>
				{
					TreeViewItemViewModel oldItem = (TreeViewItemViewModel)x.OldItems[0];
					collection.RemoveAt(x.OldStartingIndex);
					disposables[oldItem].Dispose();
					disposables.Remove(oldItem);

					TreeViewItemViewModel newItem = (TreeViewItemViewModel)x.NewItems[0];
					TreeNode node = new TreeNode { Tag = newItem };
					collection.Insert(x.NewStartingIndex, node);
					disposables.Add(newItem, BindToTreeNode(treeView, newItem, node));
				})
				.AddTo(disposable);

			items
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Reset)
				.Subscribe(_ =>
				{
					collection.Clear();
					disposables.Dispose();
					disposables.Clear();
				})
				.AddTo(disposable);

			return disposable;
		}

		private static IDisposable BindToTreeNode(TreeView treeView, TreeViewItemViewModel item, TreeNode node)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			BindToTreeNodeCollection(treeView, item.Children, node.Nodes).AddTo(disposable);

			bool sourceUpdating = false;
			bool targetUpdating = false;

			Observable.FromEvent<TreeViewEventHandler, TreeViewEventArgs>(
					h => (s, e) => h(e),
					h => treeView.AfterCheck += h,
					h => treeView.AfterCheck -= h
				)
				.ToUnit()
				.Subscribe(_ =>
				{
					if (sourceUpdating)
					{
						return;
					}

					targetUpdating = true;

					try
					{
						item.Checked.Value = node.Checked;
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
						item.Checked.Value = false;
					}

					targetUpdating = false;
				})
				.AddTo(disposable);

			item.Title
				.Subscribe(x =>
				{
					if (targetUpdating)
					{
						return;
					}

					sourceUpdating = true;

					try
					{
						node.Text = x;
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
						node.Text = string.Empty;
					}

					sourceUpdating = false;
				})
				.AddTo(disposable);

			item.Checked
				.Subscribe(x =>
				{
					if (targetUpdating)
					{
						return;
					}

					sourceUpdating = true;

					try
					{
						node.Checked = x;
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
						node.Checked = false;
					}

					sourceUpdating = false;
				})
				.AddTo(disposable);

			bool parentUpdating = false;
			bool childrenUpdating = false;

			item.Checked
				.Subscribe(x =>
				{
					if (parentUpdating)
					{
						return;
					}

					childrenUpdating = true;

					foreach (TreeViewItemViewModel child in item.Children)
					{
						child.Checked.Value = x;
					}

					childrenUpdating = false;
				})
				.AddTo(disposable);

			item.Children
				.ObserveElementObservableProperty(x => x.Checked)
				.Subscribe(x =>
				{
					if (childrenUpdating)
					{
						return;
					}

					parentUpdating = true;

					item.Checked.Value = x.Value && item.Children.All(y => y.Checked.Value);

					parentUpdating = false;
				})
				.AddTo(disposable);

			return disposable;
		}

		private static TreeNode SearchTreeNode(TreeViewItemViewModel item, TreeNode node)
		{
			return node.Tag == item ? node : node.Nodes.OfType<TreeNode>().Select(x => SearchTreeNode(item, x)).FirstOrDefault(x => x != null);
		}

		internal static IDisposable BindToListView(ListView listView, ReadOnlyReactiveCollection<ListViewColumnHeaderViewModel> columns, ReadOnlyReactiveCollection<ListViewItemViewModel> items, ReactiveProperty<ListViewItemViewModel> selectedItem)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			BindToColumnHeaderCollection(listView, columns, listView.Columns).AddTo(disposable);
			BindToListViewItemCollection(listView, items, listView.Items).AddTo(disposable);

			bool sourceUpdating = false;
			bool targetUpdating = false;

			Observable.FromEvent<EventHandler, EventArgs>(
					h => (s, e) => h(e),
					h => listView.SelectedIndexChanged += h,
					h => listView.SelectedIndexChanged -= h
				)
				.ToUnit()
				.Subscribe(_ =>
				{
					if (sourceUpdating)
					{
						return;
					}

					targetUpdating = true;

					try
					{
						if (listView.SelectedItems.Count == 1)
						{
							selectedItem.Value = listView.SelectedItems[0].Tag as ListViewItemViewModel;
						}
						else
						{
							selectedItem.Value = null;
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
						selectedItem.Value = null;
					}

					targetUpdating = false;
				})
				.AddTo(disposable);

			selectedItem
				.Subscribe(x =>
				{
					if (targetUpdating)
					{
						return;
					}

					sourceUpdating = true;

					try
					{
						foreach (ListViewItem view in listView.Items.OfType<ListViewItem>().Where(y => y.Selected))
						{
							view.Selected = false;
						}

						ListViewItem selectedView = listView.Items.OfType<ListViewItem>().FirstOrDefault(y => y.Tag == x);

						if (selectedView != null)
						{
							selectedView.Selected = true;
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);

						foreach (ListViewItem view in listView.Items.OfType<ListViewItem>().Where(y => y.Selected))
						{
							view.Selected = false;
						}
					}

					sourceUpdating = false;
				})
				.AddTo(disposable);

			return disposable;
		}

		private static IDisposable BindToColumnHeaderCollection(ListView listView, ReadOnlyReactiveCollection<ListViewColumnHeaderViewModel> viewModels, ListView.ColumnHeaderCollection views)
		{
			CompositeDisposable disposable = new CompositeDisposable();
			DictionaryDisposable<ListViewColumnHeaderViewModel> disposables = new DictionaryDisposable<ListViewColumnHeaderViewModel>().AddTo(disposable);

			views.Clear();
			disposables.AddRange(
				viewModels.ToDictionary(
					x => x,
					x =>
					{
						ColumnHeader view = new ColumnHeader { Tag = x };
						views.Add(view);
						int index = views.Count - 1;

						return x.Text.Subscribe(y =>
						{
							view.Text = y;
							listView.AutoResizeColumn(index, ColumnHeaderAutoResizeStyle.HeaderSize);
						});
					})
			);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Add)
				.Subscribe(x =>
				{
					ListViewColumnHeaderViewModel viewModel = (ListViewColumnHeaderViewModel)x.NewItems[0];
					ColumnHeader view = new ColumnHeader { Tag = viewModel };
					views.Insert(x.NewStartingIndex, view);
					int index = views.Count - 1;

					disposables.Add(
						viewModel,
						viewModel.Text.Subscribe(y =>
						{
							view.Text = y;
							listView.AutoResizeColumn(index, ColumnHeaderAutoResizeStyle.HeaderSize);
						})
					);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Move)
				.Subscribe(x =>
				{
					ColumnHeader view = views[x.OldStartingIndex];
					views.Remove(view);
					views.Insert(x.NewStartingIndex, view);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Remove)
				.Subscribe(x =>
				{
					ListViewColumnHeaderViewModel viewModel = (ListViewColumnHeaderViewModel)x.OldItems[0];
					views.RemoveAt(x.OldStartingIndex);
					disposables[viewModel].Dispose();
					disposables.Remove(viewModel);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Reset)
				.Subscribe(_ =>
				{
					views.Clear();
					disposables.Dispose();
					disposables.Clear();
				})
				.AddTo(disposable);

			return disposable;
		}

		internal static IDisposable BindToListViewItemCollection(ListView listView, ReadOnlyReactiveCollection<ListViewItemViewModel> viewModels, ListView.ListViewItemCollection views)
		{
			CompositeDisposable disposable = new CompositeDisposable();
			DictionaryDisposable<ListViewItemViewModel> disposables = new DictionaryDisposable<ListViewItemViewModel>().AddTo(disposable);

			ListViewItem selectedItem = null;

			views.Clear();
			disposables.AddRange(
				viewModels.ToDictionary(
					x => x,
					x =>
					{
						ListViewItem view = new ListViewItem { Tag = x };
						views.Add(view);

						return BindToListViewItem(listView, x, view);
					})
			);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Add)
				.Subscribe(x =>
				{
					ListViewItemViewModel viewModel = (ListViewItemViewModel)x.NewItems[0];
					ListViewItem view = new ListViewItem { Tag = viewModel };
					views.Insert(x.NewStartingIndex, view);
					disposables.Add(viewModel, BindToListViewItem(listView, viewModel, view));

					if (selectedItem != null)
					{
						if (viewModel == selectedItem.Tag)
						{
							foreach (ListViewItem unselectedItem in listView.Items.OfType<ListViewItem>().Where(y => y.Selected))
							{
								unselectedItem.Selected = false;
							}

							view.Selected = true;
							selectedItem = null;
						}
					}
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Move)
				.Subscribe(x =>
				{
					ListViewItem view = views[x.OldStartingIndex];
					views.Remove(view);
					views.Insert(x.NewStartingIndex, view);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Remove)
				.Subscribe(x =>
				{
					ListViewItemViewModel viewModel = (ListViewItemViewModel)x.OldItems[0];
					selectedItem = views[x.OldStartingIndex].Selected ? views[x.OldStartingIndex] : null;
					views.RemoveAt(x.OldStartingIndex);
					disposables[viewModel].Dispose();
					disposables.Remove(viewModel);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Reset)
				.Subscribe(_ =>
				{
					views.Clear();
					disposables.Dispose();
					disposables.Clear();
				})
				.AddTo(disposable);

			return disposable;
		}

		private static IDisposable BindToListViewItem(ListView listView, ListViewItemViewModel viewModel, ListViewItem view)
		{
			CompositeDisposable disposable = new CompositeDisposable();

			viewModel.ImageIndex.Subscribe(x => view.ImageIndex = x).AddTo(disposable);

			BindToListViewSubItemCollection(listView, viewModel.SubItems, view.SubItems).AddTo(disposable);

			return disposable;
		}

		private static IDisposable BindToListViewSubItemCollection(ListView listView, ReadOnlyReactiveCollection<ListViewSubItemViewModel> viewModels, ListViewItem.ListViewSubItemCollection views)
		{
			CompositeDisposable disposable = new CompositeDisposable();
			DictionaryDisposable<ListViewSubItemViewModel> disposables = new DictionaryDisposable<ListViewSubItemViewModel>().AddTo(disposable);

			views.Clear();
			disposables.AddRange(
				viewModels.ToDictionary(
					x => x,
					x =>
					{
						ListViewItem.ListViewSubItem view = new ListViewItem.ListViewSubItem { Tag = x };
						views.Add(view);
						int index = views.Count - 1;

						return x.Text.Subscribe(y =>
						{
							view.Text = y;
							listView.AutoResizeColumn(index, ColumnHeaderAutoResizeStyle.HeaderSize);
						});
					})
			);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Add)
				.Subscribe(x =>
				{
					ListViewSubItemViewModel viewModel = (ListViewSubItemViewModel)x.NewItems[0];
					ListViewItem.ListViewSubItem view = new ListViewItem.ListViewSubItem { Tag = viewModel };
					views.Insert(x.NewStartingIndex, view);
					int index = views.Count - 1;

					disposables.Add(
						viewModel,
						viewModel.Text.Subscribe(y =>
						{
							view.Text = y;
							listView.AutoResizeColumn(index, ColumnHeaderAutoResizeStyle.HeaderSize);
						})
					);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Move)
				.Subscribe(x =>
				{
					ListViewItem.ListViewSubItem view = views[x.OldStartingIndex];
					views.Remove(view);
					views.Insert(x.NewStartingIndex, view);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Remove)
				.Subscribe(x =>
				{
					ListViewSubItemViewModel viewModel = (ListViewSubItemViewModel)x.OldItems[0];
					views.RemoveAt(x.OldStartingIndex);
					disposables[viewModel].Dispose();
					disposables.Remove(viewModel);
				})
				.AddTo(disposable);

			viewModels
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Reset)
				.Subscribe(_ =>
				{
					views.Clear();
					disposables.Dispose();
					disposables.Clear();
				})
				.AddTo(disposable);

			return disposable;
		}
	}
}
