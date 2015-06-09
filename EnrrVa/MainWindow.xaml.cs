using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EnrrVa.Common;
using System.Data.SqlClient;
using System.Xml;
using System.IO;

namespace Vssc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }

        private void btnGenerateXML_Click(object sender, RoutedEventArgs e)
        {
            txtRtb.Selection.Text = "";
            
            using (var cmd = new SqlCommand())
            {
                String tablename = "";

                DataConnection.serverName = "";

                if (cboServerName.Text == "Local")
                {
                    DataConnection.serverName = "WINDOWS-K0BCQM6";
                }
                else
                {
                    DataConnection.serverName = cboServerName.Text;
                }

                DataConnection.dbName = cboDbName.Text;

                tablename = cboTable.Text;

                if (tablename != "")
                {
                    string xdoc =MakeXml.ToXml(MakeXml.xGen(tablename));
                    txtRtb.Selection.Text = xdoc;
                }
                else
                {
                    int unicode = 58;
                    char character = (char)unicode;
                    string tx = character.ToString();
                    MessageBox.Show("No table selected" + tx, "No Table");
                }


            }

        }

        private void btoListner_Click(object sender, RoutedEventArgs e)
        {
            TcpDiscovery.tcpListen();
        }
 

        private void btnWriteFile_Click(object sender, RoutedEventArgs e)
        {
            String tablename = "";

            DataConnection.serverName = "";

            if (cboServerName.Text == "Local")
            {
                DataConnection.serverName = "WINDOWS-K0BCQM6";
            }
            else
            {
                DataConnection.serverName = cboServerName.Text;
            }

            DataConnection.dbName = cboDbName.Text;

            tablename = cboTable.Text;

            if (tablename != "")
            {
                string xdoc =MakeXml.ToXml(MakeXml.xGen(tablename));
                using (StreamWriter outfile = new StreamWriter(@"C:\dev\1622.2\output.xml"))
                {
                    outfile.Write(xdoc);
                }
            }
            else
            {
                int unicode = 58;
                char character = (char)unicode;
                string tx = character.ToString();
                MessageBox.Show("No table selected" + tx, "No Table");
            }
        }

    }
}