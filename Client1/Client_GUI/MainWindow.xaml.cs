///////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Serve Graphical User Interface               //
// ver 1.0                                                           //
// Language:    C#, Visual Studio 13.0, .Net Framework 4.5           //
// Platform:    Dell Inspiron N5010, Win7 Professional, SP1          //
// Application: Demonstration for CIS 681, Project #4, Fall 2014     //
// Author:      Sunilkumar Lakkad, Syracuse University               //
//              (315) 751-5834, lakkads1@gmail.com                   //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *    WindowMain - Which Provide simple and elegent GUI to user, 
 *    by which user can perform processing task easily
 */
/* Required Files:
 *   Progclient.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 14 Nov 2014
 * - first release
 */

using DependencyAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;

namespace Client_GUI
{

    public partial class WindowMain : Window
    {
        SvcMsg rcvdMsg = new SvcMsg();
        Receiver recvr=new Receiver();
        Sender snd = new Sender();
        ProgClient pc = new ProgClient();
        int messageCounter = 0;

        Thread rcvThrd = null;
        delegate void NewMessage(SvcMsg msg);
        event NewMessage OnNewMessage;

        //----< receive thread processing >------------------------------

        void ThreadProc()
        {
            while (true)
            {               
                rcvdMsg = recvr.GetMessage();

                // call window functions on UI thread
                this.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  OnNewMessage,
                  rcvdMsg);
            }
        }

        //----< called by UI thread when dispatched from rcvThrd >-------

        void OnNewMessageHandler(SvcMsg msg)
        {            
            if (msg.cmd.ToString().Equals("ProjectList"))
            {
                messageCounter++;
                message_listbox.Items.Insert(0, " ");
                message_listbox.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
                message_listbox.Items.Insert(0, messageCounter+". Response for ProjectList From: " + msg.src.ToString());

                listbox1.Items.Clear();
                List<string> projectList = pc.getProjectList(msg.body.ToString());
                if (projectList!=null)
                {
                    foreach (string s in projectList)
                        listbox1.Items.Add(s);
                }
                else
                {
                    dependency_Result.Items.Add("There is Projects Available on Server");
                }
            }
            if (msg.cmd.ToString().Equals("Dependency"))
            {
                messageCounter++;
                message_listbox.Items.Insert(0, " ");
                message_listbox.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
                message_listbox.Items.Insert(0, messageCounter+". Response for Dependency From: " + msg.src.ToString());

                pc.outGoingResultAnalysis(msg.body.ToString());
               // dependency_Result.Text = msg.body.ToString();

            }
            if (msg.cmd.ToString().Equals("DependencyAll"))
            {
                pc.inComingResultAnalysis(msg.body.ToString());
            }
        }

        //----< subscribe to new message events and Intialize Window component >------------------------
        public WindowMain()
        {
            InitializeComponent();
            Title = "Client1";

            IPHostEntry host1;
            string localIP = "?";
            host1 = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host1.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            string address = "http://" + localIP + ":8001/MessageService";
            ServiceHost host = ProgClient.CreateServiceChannel(address);
            host.Open();

            OnNewMessage += new NewMessage(OnNewMessageHandler);

            combobox_1.Items.Add("http://localhost:9001/MessageService");
            combobox_1.Items.Add("http://localhost:9002/MessageService");

            Connect.IsEnabled = false;
            Dependency_Button.IsEnabled = false;

            result.IsEnabled = false;
            message.IsEnabled = false;
        }


        private void Connection_Button_Click(object sender, RoutedEventArgs e)
        {
            string src = combobox_1.SelectedItem.ToString();
            try
            {
                snd.sendRequestForprojectList(src);
                Connect.IsEnabled = false;
                combobox_1.IsEnabled = false;
 
                // create receive thread which calls rcvBlockingQ.deQ() (see ThreadProc above)
                rcvThrd = new Thread(new ThreadStart(this.ThreadProc));
                rcvThrd.IsBackground = true;
                rcvThrd.Start();

                message.IsEnabled = true;
                messageCounter++;
                message_listbox.Items.Insert(0, " ");
                message_listbox.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
                message_listbox.Items.Insert(0, messageCounter+". Request for ProjectList To: " + src);

            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);               
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
        }


        private void listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dependency_Button.IsEnabled = true;
        }


        private void combobox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Connect.IsEnabled = true;
        }


        private void Dependency_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                snd.sendRequestForAnalysis(listbox1.SelectedItem.ToString(), combobox_1.SelectedItem.ToString());

                messageCounter++;
                message_listbox.Items.Insert(0, " ");
                message_listbox.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));
                message_listbox.Items.Insert(0, messageCounter + ". Request for Dependency Analysis To: " + combobox_1.SelectedItem.ToString());

                Connect.IsEnabled = true;
                combobox_1.IsEnabled = true;

                result.IsEnabled = true;
                result.Focus();
                dependency_Result.Items.Clear();
                result1.Content = "Click on Below Button for specific Result: ";
                dependency_Result.Items.Add("Results Came from Server...........\n Which contains In Coming and Out Going Dependency");
            }
            catch 
            {
                Window temp = new Window();
                temp.Content = "First Choose any Project from the available Projects List !!!";
                temp.Height = 70;
                temp.Width = 500;
                temp.Show();
            }
            
        }


        private void Button_Show_Type(object sender, RoutedEventArgs e)
        {
            result1.Content = "Out Going Type Dependency Result: ";
            dependency_Result.Items.Clear();
            List<string> type = pc.getOutTypeDependecyResult();
            if (type != null)
            {
                foreach (string s in type)
                    dependency_Result.Items.Add(s);
            }
            else
            {
                dependency_Result.Items.Add("There is No Results Data");
            }

        }


        private void Button_Show_Package(object sender, RoutedEventArgs e)
        {
            result1.Content = "Out Going Package Dependency Result: ";
            dependency_Result.Items.Clear();

            List<string> type = pc.getOutPackageDependecyResult();
            if (type != null)
            {
                foreach (string s in type)
                    dependency_Result.Items.Add(s);
            }
            else
            {
                dependency_Result.Items.Add("There is No Results Data");
            }
        }


        private void Button_Show_XML(object sender, RoutedEventArgs e)
        {
            Window temp = new Window();
            ScrollViewer viewer = new ScrollViewer();
            viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            StackPanel spanel = new StackPanel();
            spanel.HorizontalAlignment = HorizontalAlignment.Left;
            spanel.VerticalAlignment = VerticalAlignment.Top;

            TextBlock tBlock = new TextBlock();
            tBlock.TextWrapping = TextWrapping.Wrap;
            tBlock.Margin = new Thickness(0, 0, 0, 20);
            tBlock.Text = pc.getOutGoingXML();

            spanel.Children.Add(tBlock);
            viewer.Content = spanel;
            temp.Content = viewer;
            temp.Title = "XML: Out Going Dependency Result";     
            temp.Height = 600;
            temp.Width = 500;
            temp.Show();
        }


        private void in_show_type_Click(object sender, RoutedEventArgs e)
        {
            result1.Content = "In Coming Type Dependency Result: ";
            dependency_Result.Items.Clear();

            List<string> type = pc.getInTypeDependecyResult();
            if (type!=null)
            {
                foreach (string s in type)
                    dependency_Result.Items.Add(s);
            }
            else 
            { 
                dependency_Result.Items.Add("There is No Results Data"); 
            }
        }


        private void in_show_package_Click(object sender, RoutedEventArgs e)
        {
            result1.Content = "In Coming Package Dependency Result: ";
            dependency_Result.Items.Clear();

            List<string> type = pc.getInPackageDependecyResult();
            if (type != null)
            {
                foreach (string s in type)
                    dependency_Result.Items.Add(s);
            }
            else
            {
                dependency_Result.Items.Add("There is No Results Data");
            }
        }


        private void in_show_xml_Click(object sender, RoutedEventArgs e)
        {
            Window temp = new Window();
            ScrollViewer viewer = new ScrollViewer();
            viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            StackPanel spanel = new StackPanel();
            spanel.HorizontalAlignment = HorizontalAlignment.Left;
            spanel.VerticalAlignment = VerticalAlignment.Top;

            TextBlock tBlock = new TextBlock();
            tBlock.TextWrapping = TextWrapping.Wrap;
            tBlock.Margin = new Thickness(0, 0, 0, 20);
            tBlock.Text = pc.getInComingXML();

            spanel.Children.Add(tBlock);
            viewer.Content = spanel;
            temp.Content = viewer;
            temp.Title = "XML: In Coming Dependency Result";     
            temp.Height = 600;
            temp.Width = 500;
            temp.Show();
        }
    }
}
