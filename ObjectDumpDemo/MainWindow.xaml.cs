namespace ObjectDumpDemo
{
   using System;
   using System.Linq;
   using System.Windows;
   using System.Windows.Controls;
   using ObjectDump;

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="MainWindow" /> class.
      /// </summary>
      public MainWindow()
      {
         InitializeComponent();
         InitializeDisplayedInformation();
      }

      /// <summary>
      /// Initializes the displayed information.
      /// </summary>
      private void InitializeDisplayedInformation()
      {
         // dump DateTime.Now, also create dumps of properties and enumerable members
         var dumpToDisplay = DateTime.Now.Dump("DateTime.Now", dumpProperties: true, dumpEnumerableMembers: true);

         var rootItem = new TreeViewItem {Header = dumpToDisplay.ObjectName, DataContext = dumpToDisplay};
         rootItem.IsVisibleChanged += HandleTreeViewItemVisibleChanged;
         TreeView.Items.Add(rootItem);
      }

      /// <summary>
      /// Handles the TreeView item visible changed.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
      private void HandleTreeViewItemVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         var selectedItem = sender as TreeViewItem;

         // whenever a TreeViewItem becomes visible re-calculate its sub-items
         if (selectedItem != null && selectedItem.IsVisible)
         {
            var dump = selectedItem.DataContext as IObjectDump;

            if (dump != null)
            {
               // remove old sub-items
               foreach (TreeViewItem oldSubItem in selectedItem.Items)
               {
                  oldSubItem.IsVisibleChanged -= HandleTreeViewItemVisibleChanged;
               }

               selectedItem.Items.Clear();

               // add new sub-items for public fields, properties and enumerable members
               foreach (IObjectDump subItemDump in dump.PublicFields.Union(dump.Properties).Union(dump.EnumerableMembers))
               {
                  var newSubItem = new TreeViewItem {Header = subItemDump.ObjectName, DataContext = subItemDump};
                  newSubItem.IsVisibleChanged += HandleTreeViewItemVisibleChanged;
                  selectedItem.Items.Add(newSubItem);
               }
            }
         }
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
            var dump = selectedItem.DataContext as IObjectDump;

            if (dump != null)
            {
               TextBoxDeclaredType.Text = dump.DeclaredType.FullName;
               TextBoxActualType.Text = dump.ActualType.FullName;
               TextBoxValue.Text = dump.ObjectValue;
            }
         }
         else
         {
            TextBoxDeclaredType.Text = string.Empty;
            TextBoxActualType.Text = string.Empty;
            TextBoxValue.Text = string.Empty;
         }
      }
   }
}