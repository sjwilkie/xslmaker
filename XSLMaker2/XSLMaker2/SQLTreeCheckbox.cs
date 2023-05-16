using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace XSLMaker2
{
    class SQLTreeCheckbox
    {
        XSLTree tree;
        Grid ElementOptions;
        Button remove;
        Button moveUp;
        Button moveDown;


        public SQLTreeCheckbox(XSLTree tree, Grid elopt)
        {
            this.tree = tree;
            this.ElementOptions = elopt;

        }

        public SQLTreeCheckbox(XSLTree tree, Grid elementOptions, Button remove, Button moveUp, Button moveDown)
        {
            this.tree = tree;
            ElementOptions = elementOptions;
            this.remove = remove;
            this.moveUp = moveUp;
            this.moveDown = moveDown;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

        }

        public TreeViewItem AddTreeItemCheckBox(string name, object item)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            DockPanel dp = new DockPanel();
            CheckBox cb = new CheckBox();
            cb.IsEnabled = false;
            TextBlock tb = new TextBlock();
            tb.Text = name;
            dp.Children.Add(cb);
            dp.Children.Add(tb);
            treeViewItem.Header = dp;
            treeViewItem.IsExpanded = true;
            treeViewItem.Selected += new RoutedEventHandler(Item_Selected);
            treeViewItem.Tag = item;
            return treeViewItem;
        }

        public TreeViewItem AddTreeItem(string name)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = name;
            item.IsExpanded = true;
            return item;
        }

        public TreeViewItem AddTreeItem(string name, object tag)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = name;
            item.IsExpanded = true;
            item.Tag = tag;
            return item;
        }

        void Item_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTVI = ((TreeViewItem)sender);

            CheckBox cb = (CheckBox)((DockPanel)selectedTVI.Header).Children[0];
            if (cb.IsChecked == false & selectedTVI == tree.SelectedItem)
            {
                recursiveCheckboxToggle(selectedTVI, true);
            }
            else if (cb.IsChecked == true & selectedTVI == tree.SelectedItem)
            {
                recursiveCheckboxToggle(selectedTVI, false);
                
            }

            if (tree.CheckboxSelectedItems.Count == 0)
            {
                foreach (UIElement child in ElementOptions.Children)
                {
                    child.Visibility = Visibility.Hidden;
                }
            }
            else if (tree.CheckboxSelectedItems.Count == 1)
            {
                foreach (UIElement child in ElementOptions.Children)
                {
                    child.Visibility = Visibility.Visible;
                }
            }
            else if (tree.CheckboxSelectedItems.Count > 1)
            {
                foreach (UIElement child in ElementOptions.Children)
                {
                    child.Visibility = Visibility.Hidden;
                }
                remove.Visibility = Visibility.Visible;
            }

        }


        private void recursiveCheckboxToggle(TreeViewItem item, bool toggle)
        {
            if(item.Header.ToString().Contains("row") | item.Header.ToString().Contains("col"))
            {
                if(item.Items.Count > 0)
                {
                    foreach(TreeViewItem child in item.Items)
                    {
                        recursiveCheckboxToggle(child, toggle);
                    }
                }
            }
            else
            {
                CheckBox cb = (CheckBox)((DockPanel)item.Header).Children[0];
                string name = ((TextBlock)((DockPanel)item.Header).Children[1]).Text;
                if (toggle == true)
                {
                    
                    if (!tree.CheckboxSelectedItems.Contains(item))
                    {
                        cb.IsChecked = true;
                        tree.CheckboxSelectedItems.Add(item);
                        Console.WriteLine("Added " + name);
                    }
                    
                }
                else
                {
                    cb.IsChecked = false;
                    tree.CheckboxSelectedItems.Remove(item);
                    Console.WriteLine("Removed " + name);
                }
                if (item.Items.Count > 0)
                {
                    foreach (TreeViewItem child in item.Items)
                    {
                        recursiveCheckboxToggle(child, toggle);
                    }
                }
            }
        }

    }
}
