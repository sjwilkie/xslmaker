using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Win32;
using System.Data;

namespace XSLMaker2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        XmlDataDocument xmlDoc = new XmlDataDocument();
        DataSet data = new DataSet();
        ManageRDL RDLData;
        XSLTreeCreator XSLData;
        SQLTreeCheckbox treeItemCheckbox;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            var openFile = new OpenFileDialog();
            openFile.DefaultExt = "rdl";
            openFile.Filter = "RDL File|*.rdl";
            openFile.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            openFile.ShowDialog();

            try
            {
                if (openFile.FileName.Length > 0)
                {
                    xmlDoc = new XmlDataDocument();
                    data = xmlDoc.DataSet;
                    RDLOutputTree.Items.Clear();
                    XSLOutputTree.Items.Clear();
                    // Add code here to create the schema of the DataSet to view the data.  

                    xmlDoc.Load(openFile.FileName);

                    treeItemCheckbox = new SQLTreeCheckbox(XSLOutputTree, ElementOptionsGrid, ElementOptionRemove, ElementOptionMoveUp, ElementOptionMoveDown);

                    RDLData = new ManageRDL(RDLOutputTree, XSLOutputTree, xmlDoc, treeItemCheckbox);

                    RDLData.PopulateData();

                    XSLData = new XSLTreeCreator(RDLOutputTree, XSLOutputTree, treeItemCheckbox);

                    XSLData.populateXSLTree();
                }
            }
            catch (Exception ex) {
                string messageBoxText = "You have encountered an error: " + ex.ToString();
                string caption = "Error";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBoxResult result;

                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            }
        }



    }

}
