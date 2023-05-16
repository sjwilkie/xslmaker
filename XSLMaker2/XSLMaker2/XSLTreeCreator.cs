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
                // for each tablix, add a tree item named for that tablix.
                XSLOutputTree.Items.Add(treeItem);
                for (int row = 0; row <= tree.Items.Count - 1; row++)
                {
                    // for every row under the tablix in the RDL, find the textbox items underneath them.
                    // make note of their row and column number, and add them to the Fields list for use later.
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
                    // find all columns, add them to the tablixmemberscol list. Find all column groups using the recursive method.
                    if (rowItem.Header.ToString().Contains("tablixcolumnhierarchy"))
                    {
                        GroupsFieldsRecursive((TreeViewItem)rowItem.Items[0],null, 0, tablename, tablixmemberscol, "col");
                    }
                    // find all rows, add them to the tablixmembersrow list. find all row groupings using the recursive method.
                    if (rowItem.Header.ToString().Contains("tablixrowhierarchy"))
                    {
                        GroupsFieldsRecursive((TreeViewItem)rowItem.Items[0], null, 0, tablename, tablixmembersrow, "row");
                    }
                }
                int rownum = 0;
                // still iterating through tablixs, find all rows and columns for the tablix. Use the recursive method to add 
                // TreeViewItems to the XSLOutputTree. Also find all groups and put them in appropriately.
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
            // now that we have the entire tree structure, all we need to do is add the fields to the right places.
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
                        // if the field is a row, col, or group (the things we care about) and has a parent,
                        // check the parent name to see what kind of object we're dealing with. Parent names can
                        // be simple strings if they are col/row fields, or a docker panel that includes
                        // check boxes and a string object if it is a group. Thus, we must retrieve the name in 
                        // different ways depending on what kind it is.
                        string parenttreename = "";
                        if (!parenttreeitem.Header.ToString().Contains("row") & !parenttreeitem.Header.ToString().Contains("col"))
                        {
                            parenttreename = (string)((DockPanel)parenttreeitem.Header).Children[1].ToString();
                        } 
                        else
                        {
                            parenttreename = parenttreeitem.Header.ToString();
                        }
                        // check to see if the parent is the same object as the field parent we're expecting
                        // if it is, then continue.
                            if (parenttreename == field.parent.name)
                            {
                                if (passalong == null)
                                {
                                // 'passalong' is a group of parent/child objects (treeViewItems) that we are remembering and sending 
                                // to the recursive method. This is particularly used for getting groups to parent correctly over the fields.
                                // If it is a group, create a TreeViewItem with a docker and a checkbox/string. Else, create a normal 
                                // TreeViewItem for rows/cols.
                                TreeViewItem Item;
                                if (field.name.Contains("group)"))
                                    Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                                else
                                    Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                                // field is a class I made. tree items stores each TreeViewItem that the col or row is associated with for use later.
                                // This is important because there is only one set of columns in the RDL that must be applied to each row.
                                field.treeitems.Add(Item);
                                // Add the object as a child to the parent.
                                parenttreeitem.Items.Add(Item);
                                returnedItem = Item;
                                }
                                else
                                {
                                // otherwise 'pass along' the passalong parent/child objects and parent them all as child objects.
                                parenttreeitem.Items.Add(passalong);
                                }
                            }
                            else
                            {
                            // if the parent we found is not the one we expect or are looking for, create the parent object we're expecting,
                            // make the current item a child object of the newly created parent, and send it as a 'pass along'.
                            // Still maintaining the object difference between groups and rows/cols
                                if (passalong == null)
                                {
                                    TreeViewItem Item;
                                    if (field.name.Contains("group)"))
                                        Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                                    else
                                        Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                                    field.treeitems.Add(Item);
                                    TreeViewItem ParentItem = TreeItemCheck.AddTreeItemCheckBox(field.parent.name, new FieldRow(field.parent, rowfield));
                                    ParentItem.Items.Add(Item);
                                    ParentGroupRecursive(field.parent, parenttreeitem, ParentItem, Item, rowfield);
                                    returnedItem = Item;
                                }
                                else
                                {
                                // otherwise, if PassAlong does exist, create the parent object, add the passalong as a child, and...
                                // PASS IT ALONG!
                                    TreeViewItem ParentItem = TreeItemCheck.AddTreeItemCheckBox(field.parent.name, new FieldRow(field.parent, rowfield));
                                    TreeViewItem Item = passalong;
                                    ParentItem.Items.Add(Item);
                                    ParentGroupRecursive(field.parent, parenttreeitem, ParentItem, Item, rowfield);
                                }
                            }
                        
                    }
                    else
                    {
                        // if the parent is not a group, do not recursively call this method again. Instead, add the 
                        // new item or passalong as a child object to the parent and be done with it.
                        if (passalong == null)
                        {
                            TreeViewItem Item;
                            if (field.name.Contains("group)"))
                                Item = TreeItemCheck.AddTreeItemCheckBox(field.name, new FieldRow(field, rowfield));
                            else
                                Item = TreeItemCheck.AddTreeItem(field.name, new FieldRow(field, rowfield));
                            field.treeitems.Add(Item);
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
            // iterate over the RDL node child nodes
            if (RDLnode.Items.Count > 0)
            {
                for(int i = 0; i <= RDLnode.Items.Count - 1; i++)
                {
                    int tablixfieldcount = 0;
                    TreeViewItem nodeItem = (TreeViewItem)RDLnode.Items[i];
                    // If the node is a group, add it and then find child objects of the
                    // subsequent child of the RDL Node (the group's parent)
                    // This is important because the group nodes of the RDL have no children. 
                    // Usually child groups have a TeblixMembers subsequent sibling that
                    // contains the actual child objects.
                    if (nodeItem.Header.ToString().Contains("group)"))
                    {
                        numofgroups++;
                        RDLField groupfield = new RDLField(nodeField.parent, numb, numb, tablix, nodeItem.Header.ToString().Substring(0, nodeItem.Header.ToString().LastIndexOf("(")+1)+fieldType+ "_"+ nodeItem.Header.ToString().Substring(nodeItem.Header.ToString().LastIndexOf("(")+1), "Group");
                        groups.Add(groupfield);
                        nodeField.parent = groupfield;
                        if (nodeItem.Items.Count > i+1)
                        GroupsFieldsRecursive((TreeViewItem)nodeItem.Items[i+1], nodeField, numb, tablix, listing, fieldType);
                    }
                    // if it is a column or group, add it to the data structure and record the type (row or col) that it is
                    // Check to see if the col or row already exists. This is important because children of groups will
                    // count the same field multiple times, especially if there are child groups underneath that group.
                        if (nodeItem.Header.ToString() == "tablixmember")
                    {
                        tablixfieldcount++;
                        TreeViewItem parentNode = (TreeViewItem)nodeItem.Parent;
                        numoffields++;
                        int number = numb + numoffields;
                            RDLField rdlfield = new RDLField(nodeField, number, number, tablix, fieldType+"_" + number.ToString(), fieldType);
                        RDLField List = null;
                        // find if field already exists. Add preexisting field if already exists and drop the new iteration.
                        // Else add the new field.
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
                        // check this node's child nodes
                        if (nodeItem.Items.Count > 0)
                            {
                            // if the first child of this node is a group, flip the parenting structure so that
                            // this col/row becomes a child object of that group. This will continue to happen,
                            // causing groups to float to the top and the col/row to float to the bottom.
                                TreeViewItem child = (TreeViewItem)nodeItem.Items[0];
                                if(child.Header.ToString().Contains("group)") & nodeItem.Header.ToString().Contains("group)"))
                                {
                                    RDLField groupfield = new RDLField(rdlfield, number, number, tablix, child.Header.ToString(), "Group");
                                groups.Add(groupfield);
                                    groupfield.parent = rdlfield.parent;
                                rdlfield.parent = groupfield;
                                // continue searching for groups using the group field as the parent object
                                GroupsFieldsRecursive(nodeItem, groupfield, number, tablix, listing, fieldType);
                                }
                                else
                                {
                                // if this child node has children and the first child is not a group, continue searching for groups
                                // using the rdlfield as the parent object
                                    GroupsFieldsRecursive(nodeItem, rdlfield, number, tablix, listing, fieldType);
                                }
                                
                            }
                    }
                        // if the child is a Tablixmembers which has children of tablixmember, continue looking for
                        // groups in children but don't count it in the field number. Also don't add it to our
                        // growing list of fields for use later.
                    if (nodeItem.Header.ToString() == "tablixmembers")
                    {
                        int number = numb;
                        // if the tablixmembers has more than one child, iterate through it normally
                        if (nodeItem.Items.Count > 1)
                        {
                            GroupsFieldsRecursive(nodeItem, nodeField, number, tablix, listing, fieldType);
                        }
                        else
                        {
                            // if it has one child (a tablixmember), jump to that child's children without including it in the 
                            // field count or list.
                            GroupsFieldsRecursive((TreeViewItem)nodeItem.Items[0], nodeField, number, tablix, listing, fieldType);
                        }
                            
                    }
                   
                }
            }
        }

        private void AddFieldsToXSL()
        {
            // This method finds each field and associates them with the correct column and row. It will iterate through 
            // all of the objects to find the right combination. Not optimized at all, but since RDLs generally have only
            // a couple hundred fields/columns, the performance hit shouldn't be too much. We can optimize it later if it's
            // a problem.
            foreach (RDLField field in Fields)
            {
                foreach (RDLField Column in tablixmemberscol)
                {
                    // field numbering starts at 0, col numbering starts at 1. 
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
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
        }

        class RDLField
        {
            // class used for storing all information related to columns, rows, fields and groups.
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
