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
    class XSLTreeCreator
    {

        List<string> tables = new List<string>();
        List<RDLField> Fields = new List<RDLField>();
        List<RDLField> tablixmemberscol = new List<RDLField>();
        List<RDLField> tablixmembersrow = new List<RDLField>();
        List<RDLField> groups = new List<RDLField>();
        SQLTreeCheckbox TreeItemCheck;
        TreeView RDLOutputTree;
        XSLTree XSLOutputTree;

        public XSLTreeCreator(TreeView rDLOutputTree, XSLTree xSLOutputTree, SQLTreeCheckbox treeItemCheck)
        {
            RDLOutputTree = rDLOutputTree;
            XSLOutputTree = xSLOutputTree;
            TreeItemCheck = treeItemCheck;
        }

        public void populateXSLTree()
        {


            TreeViewItem rootnode = (TreeViewItem)RDLOutputTree.Items.GetItemAt(0);
            TreeViewItem[] treetablixes = RDLOutputTree.Items.FlattenTree().Where(r => r.Header.ToString().Contains("(tablix)")).ToArray();
            foreach (TreeViewItem tree in treetablixes)
            {
                string tablename = tree.Header.ToString();
                tables.Add(tablename);
                int rowsnum = 0;
                int colsnum = 0;
                TreeViewItem treeItem = TreeItemCheck.AddTreeItemCheckBox(tree.Header.ToString(), tree.Header.ToString());
                XSLOutputTree.Items.Add(treeItem);
                for (int row = 0; row <= tree.Items.Count - 1; row++)
                {
                    TreeViewItem rowItem = (TreeViewItem)tree.Items[row];
                    if (rowItem.Header.ToString().Contains("tablixrow"))
                    {
                        rowsnum++;
                        for (int col = 0; col <= rowItem.Items.Count - 1; col++)
                        {
                            TreeViewItem colItem = (TreeViewItem)rowItem.Items[col];
                            if (colItem.Header.ToString().Contains("(textbox)"))
                            {
                                colsnum++;
                                Fields.Add(new RDLField(row, col, tablename, colItem.Header.ToString(), "Field"));
                            }
                        }
                    }
                    if (rowItem.Header.ToString().Contains("tablixcolumnhierarchy"))
                    {
                        GroupsFieldsRecursive((TreeViewItem)rowItem.Items[0],null, 0, tablename, tablixmemberscol, "col");
                    }
                    if (rowItem.Header.ToString().Contains("tablixrowhierarchy"))
                    {
                        GroupsFieldsRecursive((TreeViewItem)rowItem.Items[0], null, 0, tablename, tablixmembersrow, "row");
                    }
                }
                int rownum = 0;
                foreach(RDLField row in tablixmembersrow)
                {
                    rownum++;
                    if(row.tablix == tablename)
                    {
                        TreeViewItem rowitem = ParentGroupRecursive(row, treeItem, null, null, null);
                        foreach (RDLField col in tablixmemberscol)
                        {
                            if (col.tablix == tablename)
                            {
                                ParentGroupRecursive(col, rowitem, null, null, row);
                            }

                        }
                    }
                    
                }
                


            }

            AddFieldsToXSL();

        }

        private TreeViewItem ParentGroupRecursive(RDLField field, TreeViewItem parenttreeitem, TreeViewItem passalong, TreeViewItem firstField, RDLField rowfield)
        {
            TreeViewItem returnedItem = firstField;
            if(field.parent != null)
            {
                if(field.name.Contains("row") | field.name.Contains("group)") | field.name.Contains("col"))
                {
                    if (field.parent.name.Contains("group)"))
                    {
                        string parenttreename = "";
                        if (!parenttreeitem.Header.ToString().Contains("row") & !parenttreeitem.Header.ToString().Contains("col"))
                        {
                            parenttreename = (string)((DockPanel)parenttreeitem.Header).Children[1].ToString();
                        } 
                        else
                        {
                            parenttreename = parenttreeitem.Header.ToString();
                        }
                            if (parenttreename == field.parent.name)
                            {
                                if (passalong == null)
                                {
                                    //if(field.treeitem == null )
                                    //{
                                    TreeViewItem Item;
                                    if (field.name.Contains("group)"))
                                        Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                                    else
                                        Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                                    field.treeitems.Add(Item);
                                    //}
                                    parenttreeitem.Items.Add(Item);
                                    returnedItem = Item;
                                }
                                else
                                {
                                    parenttreeitem.Items.Add(passalong);
                                }
                            }
                            else
                            {
                                if (passalong == null)
                                {
                                    //if (field.treeitem == null)
                                    //{
                                    TreeViewItem Item;
                                    if (field.name.Contains("group)"))
                                        Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                                    else
                                        Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                                    field.treeitems.Add(Item);
                                    //}
                                    TreeViewItem ParentItem = TreeItemCheck.AddTreeItemCheckBox(field.parent.name, new FieldRow(field.parent, rowfield));
                                    //if(field.treeitem.Parent == null)
                                    ParentItem.Items.Add(Item);
                                    ParentGroupRecursive(field.parent, parenttreeitem, ParentItem, Item, rowfield);
                                    returnedItem = Item;
                                }
                                else
                                {
                                    TreeViewItem ParentItem = TreeItemCheck.AddTreeItemCheckBox(field.parent.name, new FieldRow(field.parent, rowfield));
                                    TreeViewItem Item = passalong;
                                    ParentItem.Items.Add(Item);
                                    ParentGroupRecursive(field.parent, parenttreeitem, ParentItem, Item, rowfield);
                                }
                            }
                        
                    }
                    else
                    {
                        if (passalong == null)
                        {
                            //if (field.treeitem == null)
                            //{
                            TreeViewItem Item;
                            if (field.name.Contains("group)"))
                                Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                            else
                                Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                            field.treeitems.Add(Item);
                            //}
                            parenttreeitem.Items.Add(Item);
                            returnedItem = Item;
                        }
                        else
                        {
                            parenttreeitem.Items.Add(passalong);
                        }
                    }
                }
                else
                {
                    if (passalong == null)
                    {
                        //if (field.treeitem == null)
                        //{
                        TreeViewItem Item;
                        if (field.name.Contains("group)"))
                            Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                        else
                            Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                        field.treeitems.Add(Item);
                        //}
                        parenttreeitem.Items.Add(Item);
                        returnedItem = Item;
                    }
                    else
                    {
                        parenttreeitem.Items.Add(passalong);
                    }
                }
            }
            else
            {
                if (passalong == null)
                {
                    TreeViewItem Item;
                    if (field.name.Contains("group)"))
                        Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                    else
                        Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                    field.treeitems.Add(Item);
                    //}
                    //if (field.treeitem.Parent == null)
                        parenttreeitem.Items.Add(Item);
                    returnedItem = Item;
                }
                else
                {
                    parenttreeitem.Items.Add(passalong);
                }
            }
            return returnedItem;
            
        }

        private void GroupsFieldsRecursive(TreeViewItem RDLnode, RDLField nodeField, int field, string tablix, List<RDLField> listing, string fieldType)
        {
            int numb = field;
            int numoffields = 0;
            int numofgroups = 0;
            if (RDLnode.Items.Count > 0)
            {
                for(int i = 0; i <= RDLnode.Items.Count - 1; i++)
                {
                    int tablixfieldcount = 0;
                    TreeViewItem nodeItem = (TreeViewItem)RDLnode.Items[i];
                    if (nodeItem.Header.ToString().Contains("group)"))
                    {
                        numofgroups++;
                        RDLField groupfield = new RDLField(nodeField.parent, numb, numb, tablix, nodeItem.Header.ToString().Substring(0, nodeItem.Header.ToString().LastIndexOf("(")+1)+fieldType+ "_"+ nodeItem.Header.ToString().Substring(nodeItem.Header.ToString().LastIndexOf("(")+1), "Group");
                        groups.Add(groupfield);
                        nodeField.parent = groupfield;
                        if (nodeItem.Items.Count > i+1)
                        GroupsFieldsRecursive((TreeViewItem)nodeItem.Items[i+1], nodeField, numb, tablix, listing, fieldType);
                    }
                        if (nodeItem.Header.ToString() == "tablixmember")
                    {
                        tablixfieldcount++;
                        TreeViewItem parentNode = (TreeViewItem)nodeItem.Parent;
                        numoffields++;
                        int number = numb + numoffields;
                            RDLField rdlfield = new RDLField(nodeField, number, number, tablix, fieldType+"_" + number.ToString(), fieldType);
                        RDLField List = null;
                        foreach (RDLField fld in listing)
                        {
                            if (fld.name == rdlfield.name)
                            {
                                List = fld;
                            }
                        }
                        if (List != null)
                        {
                            rdlfield = List;
                        }
                        listing.Add(rdlfield);
                        //TreeViewItem treeitem = TreeItemCheck.AddTreeItemCheckBox(rdlfield.name, rdlfield);
                        //rdlfield.treeitem = treeitem;

                        if (nodeItem.Items.Count > 0)
                            {
                                TreeViewItem child = (TreeViewItem)nodeItem.Items[0];
                                if(child.Header.ToString().Contains("group)") & nodeItem.Header.ToString().Contains("group)"))
                                {
                                    RDLField groupfield = new RDLField(rdlfield, number, number, tablix, child.Header.ToString(), "Group");
                                groups.Add(groupfield);
                                    groupfield.parent = rdlfield.parent;
                                rdlfield.parent = groupfield;
                                GroupsFieldsRecursive(nodeItem, groupfield, number, tablix, listing, fieldType);
                                }
                                else if (child.Header.ToString().Contains("group)") & nodeItem.Header.ToString().Contains("group)"))
                            {
                                RDLField groupfield = new RDLField(rdlfield, number, number, tablix, child.Header.ToString(), "Group");
                                groups.Add(groupfield);
                                GroupsFieldsRecursive(nodeItem, groupfield, number, tablix, listing, fieldType);
                            }
                                else
                                {
                                    GroupsFieldsRecursive(nodeItem, rdlfield, number, tablix, listing, fieldType);
                                }
                                
                            }
                    }
                    if (nodeItem.Header.ToString() == "tablixmembers")
                    {
                        int number = numb;
                        if (nodeItem.Items.Count > 1)
                        {
                            GroupsFieldsRecursive(nodeItem, nodeField, number, tablix, listing, fieldType);
                        }
                        else
                        {
                            GroupsFieldsRecursive((TreeViewItem)nodeItem.Items[0], nodeField, number, tablix, listing, fieldType);
                        }
                            
                    }
                   
                }
            }
        }

        private RDLField FindLastTypeRecursive(RDLField node, string type)
        {
            RDLField finalNode = null;
            if (node.parent != null)
            {
                    if (!node.parent.name.Contains(type))
                    {
                        finalNode = FindLastTypeRecursive(node.parent, type);
                    }
                    else
                {
                    finalNode = node.parent;
                }
            }
            if (finalNode == null)
            {
                if (node.name.Contains(type))
                {
                    finalNode = node;
                }
            }
            return finalNode;
        }

        private void AddFieldsToXSL()
        {

            foreach (RDLField field in Fields)
            {
                foreach (RDLField Column in tablixmemberscol)
                {
                    if (field.column + 1 == Column.column & field.tablix == Column.tablix)
                    {
                        if (Column.treeitems.Count > 0)
                        {
                            foreach (TreeViewItem col in Column.treeitems)
                            {

                                FieldRow colfield = (FieldRow)col.Tag;
                                if (colfield.Rowfield.row == field.row + 1 & col.Items.Count == 0)
                                {
                                    col.Items.Add(TreeItemCheck.AddTreeItemCheckBox(field.name, field));
                                }
                            }
                        }
                    }
                }
            }
            
        }

        class RDLField
        {
            public RDLField parent;
            public List<object> children;
            public int row;
            public int column;
            public string tablix;
            public string name;
            public List<TreeViewItem> treeitems = new List<TreeViewItem>();
            public string type = "None";

            public RDLField(int row, int column, string tablix, string fieldname, string Type)
            {
                this.children = new List<object>();
                this.parent = null;
                this.row = row;
                this.column = column;
                this.tablix = tablix;
                this.name = fieldname;
            }

            public RDLField(RDLField parent, int row, int column, string tablix, string name, string Type)
            {
                this.parent = parent;
                this.row = row;
                this.column = column;
                this.tablix = tablix;
                this.name = name;
                this.children = new List<object>();
            }
        }

        class FieldRow
        {
            public RDLField Field;
            public RDLField Rowfield;

            public FieldRow(RDLField field, RDLField rowfield)
            {
                Field = field;
                Rowfield = rowfield;
            }
        }

    }
}
