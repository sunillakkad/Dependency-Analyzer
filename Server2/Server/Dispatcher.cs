///////////////////////////////////////////////////////////////////////
// Dispatcher.cs - Handle Processing Request from Server             //
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
 *   Dispatcher  - Handle the processing request from the server, and 
 *   perform appropiate task and send result back to the server 
 */
/* Required Files:
 *   Intermediator.cs, Server.cs, ServerLibrary.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 14 Nov 2014
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyAnalyzer
{

    class Dispatcher
    {

        static string typeFromOtherServer=null;
        static bool check = false;
        static string src = "http://localhost:9002/MessageService";
        static string dst = "http://localhost:9001/MessageService";

        // ----------------< Proceesed client Request >---------------- 

        public static void clientProcessing(SvcMsg msg)
        {
            Intermediator im = new Intermediator();
            im.clearRepository();

            if (msg.cmd.ToString().Equals("Dependency"))
            {
                Console.Write("\n This is the new Request from Client for Dependency............\n\n");
                msg.ShowMessage();
                dependencyProcessing(msg);               
            }

            if (msg.cmd.ToString().Equals("DependencyAll"))
            {
                Console.Write("\n This is the slients Request from Client for All Package Dependency ............\n\n");
                msg.ShowMessage();
                allDependencyProcessing(msg);
            }

            if (msg.cmd.ToString().Equals("ProjectList"))
            {
                Console.Write("\n This is the new Request from Client for ProjectList............\n\n");
                msg.ShowMessage();                
                projectListProcessing(msg);
            }
        }

        static void projectListProcessing(SvcMsg msg)
        {
            Intermediator im = new Intermediator();
            msg.body = im.getprojectList();
            clientConnect(msg);
        }

        static void dependencyProcessing(SvcMsg msg)
        {
            string temp = "";
            bool connect = false;
            DateTime timeout = DateTime.Now;           
            timeout.AddSeconds(5);
            try
            {
                serverConnect();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n" + ex.Message);
            }
            finally
            {               
                while (true)
                {
                    if (check == true)
                    {
                        temp = typeFromOtherServer;
                        connect = true;
                        break;
                    }
                    if (DateTime.Compare(timeout,DateTime.Now) < 0 )
                    {
                        connect = false;
                        break;
                    }
                }
                Intermediator im = new Intermediator();
                if (connect)
                {
                    im.typeTableMerging(typeFromOtherServer);
                }
                im.dependencyAnalysis(msg.body.ToString());
                msg.body = im.getDependencyResult();               
                clientConnect(msg);
            }
        }

        static void allDependencyProcessing(SvcMsg msg)
        {
            string temp = "";
            bool connect = false;
            DateTime timeout = DateTime.Now;
            timeout.AddSeconds(5);
            try
            {
                serverConnect();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n" + ex.Message);
            }
            finally
            {
                while (true)
                {
                    if (check == true)
                    {
                        temp = typeFromOtherServer;
                        connect = true;
                        break;
                    }
                    if (DateTime.Compare(timeout, DateTime.Now) < 0)
                    {
                        connect = false;
                        break;
                    }
                }
                Intermediator im = new Intermediator();
                if (connect)
                {
                    im.typeTableMerging(typeFromOtherServer);
                }
                im.dependencyAnalysisAll(msg.body.ToString());
                msg.body = im.getDependencyAllResult();             
                clientConnect(msg); 
            }                 
        }

        static void responeFromServerTesting(string body)
        {
            check = true;
            typeFromOtherServer = body;
        }

        // ----------------< Proceesed Server Request >---------------- 
        public static void serverProcessing(SvcMsg msg)
        {
            if (msg.cmd.ToString().Equals("Response"))
            {
                Console.Write("\nThis is the Response from  Server............\n\n");
                msg.ShowMessage();

                responeFromServerTesting(msg.body.ToString());
            }
            if (msg.cmd.ToString().Equals("Request"))
            {
                Console.Write("\nThis is the Request from  Server for TypeTable............\n\n");
                msg.ShowMessage();

                Intermediator im = new Intermediator();
                string typeTable = im.getTypeTable();

                SvcMsg msg1 = new SvcMsg();
                msg1.src = new Uri(src);
                msg1.dst = new Uri(dst);
                msg1.cmd = SvcMsg.Command.Response;
                msg1.body = typeTable;

                Sender.sendingServer(msg1);
            }
        }
        public static void serverConnect()
        {
                Console.Write("\nEntering in to server to server communication: \n");
                SvcMsg msg1 = new SvcMsg();
                msg1.src = new Uri(src);
                msg1.dst = new Uri(dst);
                msg1.cmd = SvcMsg.Command.Request;

                Sender.sendingServer(msg1);
        }
        public static void clientConnect(SvcMsg msg)
        {
            SvcMsg msg1 = new SvcMsg();
            msg1.src = new Uri(msg.dst.ToString());
            msg1.dst = new Uri(msg.src.ToString());
            msg1.cmd = msg.cmd;
            msg1.body = msg.body;

            Sender.sendingClient(msg1);        
        }

#if(TEST_Dispatcher)
        static void Main(string[] args)
        {
            string src = "http://localhost:9001/MessageService";
            string dst = "http://localhost:9001/MessageService";
            SvcMsg msg = new SvcMsg();
            msg.src = new Uri(src);
            msg.dst = new Uri(dst);
            msg.cmd = SvcMsg.Command.ProjectList;
            msg.body ="";

            clientProcessing(msg);
            SvcMsg msg1 = new SvcMsg();
            msg1.src = new Uri(src);
            msg1.dst = new Uri(dst);
            msg1.cmd = SvcMsg.Command.Request;
            msg1.body = "";
            serverProcessing(msg);
        }
#endif
    }
}
