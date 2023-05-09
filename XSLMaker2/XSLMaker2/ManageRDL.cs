using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace XSLMaker2
{
    class ManageRDL
    {
        TreeView RDLOutputTree;
        XSLTree XSLOutputTree;
        SQLTreeCheckbox TreeItemCheck;
        XmlDataDocument RDLData;

        public ManageRDL(TreeView rDLOutputTree, XSLTree xSLOutputTree, XmlDataDocument rDLData, SQLTreeCheckbox treeItemCheck)
        {
            RDLOutputTree = rDLOutputTree;
            XSLOutputTree = xSLOutputTree;
            TreeItemCheck = treeItemCheck;
            RDLData = rDLData;
        }

        public void PopulateData()
        {
            try
            {
                RDLOutputTree.Items.Clear();
                XSLOutputTree.Items.Clear();
                TreeViewItem rootNode = TreeItemCheck.AddTreeItem("Root");
                RDLOutputTree.Items.Add(rootNode);
                nestedChildNodesRDL(RDLData.ChildNodes[1], rootNode, "tablerow", 0);
            }
            catch (Exception e) {
                string messageBoxText = "You have encountered an error: " + e.Message;
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            }
        }


        private void nestedChildNodesRDL(XmlNode node, TreeViewItem selectedParentNode, string grouptype, int groupnumber)
        {
            TreeViewItem parentNode = selectedParentNode;

            string groupType = grouptype;
            int groupnum = groupnumber;
            if (node.Attributes != null & node.Name.ToLower() == "tablix" | node.Name.ToLower() == "textbox" | node.Name.ToLower() == "group")
            {
                if (node.Attributes["Name"] != null)
                {
                    if (node.Attributes["Name"].Value != null)
                    {
                        if (node.Attributes["Name"].Value != "")
                        {
                            if (node.ChildNodes.Count > 0)
                            {
                                string ItemName = node.Attributes["Name"].Value + "_(" + node.Name.ToLower() + ")";
                                TreeViewItem TreeNode = TreeItemCheck.AddTreeItem(ItemName);
                                selectedParentNode.Items.Add(TreeNode);
                                foreach (XmlNode child in node.ChildNodes)
                                {
                                    groupnum++;
                                    nestedChildNodesRDL(child, TreeNode, groupType, groupnum);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if(node.Name.ToLower() == "tablixrow" | node.Name.ToLower() == "tablixcolumnhierarchy" | node.Name.ToLower() == "tablixrowhierarchy" | node.Name.ToLower() == "tablixmembers" | node.Name.ToLower() == "tablixmember")
                {
                    string ItemName = node.Name.ToLower();
                    TreeViewItem TreeNode = TreeItemCheck.AddTreeItem(ItemName);
                    selectedParentNode.Items.Add(TreeNode);
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        groupnum++;
                        nestedChildNodesRDL(child, TreeNode, groupType, groupnum);
                    }
                }
                else
                {
                    if (node.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            nestedChildNodesRDL(child, parentNode, groupType, groupnum);
                        }
                    }
                        
                }
                
            }
        }
        

        

    }
    public static class SOExtension
    {
        public static IEnumerable<TreeViewItem> FlattenTree(this TreeViewItem tv)
        {
            return FlattenTree(tv.Items);
        }

        public static IEnumerable<TreeViewItem> FlattenTree(this ItemCollection coll)
        {
            return coll.Cast<TreeViewItem>()
                        .Concat(coll.Cast<TreeViewItem>()
                                    .SelectMany(x => FlattenTree(x.Items)));
        }

    }

}
