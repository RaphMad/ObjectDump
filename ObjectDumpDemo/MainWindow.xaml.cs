using System;
using System.Windows;
using System.Windows.Controls;
using ObjectDump;

namespace ObjectDumpDemo
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="MainWindow"/> class.
      /// </summary>
      public MainWindow()
      {
         InitializeComponent();

         // dump DateTime.Now
         var dump = DateTime.Now.Dump("DateTime.Now");

         var rootItem = new TreeViewItem { Header = dump.ObjectName, DataContext = Tuple.Create(dump, "Root") };
         rootItem.IsVisibleChanged += HandleTreeViewItemVisibleChanged;
         TreeView.Items.Add(rootItem);
      }

      /// <summary>
      /// Handles the TreeView item visible changed.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
      private void HandleTreeViewItemVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         var selectedItem = sender as TreeViewItem;

         // whenever a TreeViewItem becomes visible re-calculate its sub-items
         if (selectedItem != null && selectedItem.IsVisible)
         {
            var dump = selectedItem.DataContext as Tuple<IObjectDump, string>;

            if (dump != null)
            {
               // remove old sub-items
               foreach (TreeViewItem oldSubItem in selectedItem.Items)
               {
                  oldSubItem.IsVisibleChanged -= HandleTreeViewItemVisibleChanged;
               }

               selectedItem.Items.Clear();

               // add new sub-items for public fields, properties and enumerable members
               foreach (IObjectDump subItemDump in dump.Item1.PublicFields)
               {
                  AddDumpWithMemberType(selectedItem, subItemDump, "Public field");
               }

               foreach (IObjectDump subItemDump in dump.Item1.Properties)
               {
                  AddDumpWithMemberType(selectedItem, subItemDump, "Property");
               }

               foreach (IObjectDump subItemDump in dump.Item1.EnumerableMembers)
               {
                  AddDumpWithMemberType(selectedItem, subItemDump, "Enumerable");
               }
            }
         }
      }

      /// <summary>
      /// Adds a dump as a new subitem and associates the given memberType string with it.
      /// </summary>
      /// <param name="parentItem">The parent item.</param>
      /// <param name="dumpToAdd">The object dump to add as a subitem.</param>
      /// <param name="memberType">The text to display in the member type textbox.</param>
      private void AddDumpWithMemberType(TreeViewItem parentItem, IObjectDump dumpToAdd, string memberType)
      {
         var newSubItem = new TreeViewItem { Header = dumpToAdd.ObjectName, DataContext = Tuple.Create(dumpToAdd, memberType) };
         newSubItem.IsVisibleChanged += HandleTreeViewItemVisibleChanged;
         parentItem.Items.Add(newSubItem);
      }

      /// <summary>
      /// Handles the TreeView selected item changed.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The e.</param>
      private void HandleTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
      {
         var selectedItem = TreeView.SelectedItem as TreeViewItem;

         // update or clear info text boxes depending on the currently selected item
         if (selectedItem != null)
         {
            var dump = selectedItem.DataContext as Tuple<IObjectDump, string>;

            if (dump != null)
            {
               TextBoxType.Text = dump.Item1.ObjectType.FullName;
               TextBoxMemberType.Text = dump.Item2;
               TextBoxValue.Text = dump.Item1.ObjectValue;
            }
         }
         else
         {
            TextBoxType.Text = string.Empty;
            TextBoxMemberType.Text = string.Empty;
            TextBoxValue.Text = string.Empty;
         }
      }
   }
}