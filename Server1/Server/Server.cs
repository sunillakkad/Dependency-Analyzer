///////////////////////////////////////////////////////////////////////////
// Server.cs - Perform Message sending and Receiving                     //
// ver 1.0                                                               //
// Language:    C#, Visual Studio 13.0, .Net Framework 4.5               //
// Platform:    Dell Inspiron N5010, Win7 Professional, SP1              //
// Application: Demonstration for CIS 681, Project #4, Fall 2014         //
// Author:      Sunilkumar Lakkad, Syracuse University                   //
//              (315) 751-5834, lakkads1@gmail.com                       //
///////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following classes:
 *   Receiver   - Impelement ImessegeService and perform safe receiveing
 *   Sender - Send Appropiate request and response to client or server
 *   Server - which open endpoint for this service and start thread for 
 *   receiving process
 */
/* Required Files:
 *   BlockingQueue.cs, Dispatcher.cs, ServerLibrary.cs
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

namespace DependencyAnalyzer
{
    /////////////////////////////////////////////////////////////
    // Receiver hosts Communication service used by other Peers

    [ServiceBehavior(Namespace = "DependencyAnalyzer")]
    public class Receiver : IMessageService
    {
        static BlockingQueue<SvcMsg> rcvBlockingQ = null;
        static BlockingQueue<SvcMsg> rcvBlockingQServer = null;

        static Receiver()
        {
            if (rcvBlockingQ == null)
                rcvBlockingQ = new BlockingQueue<SvcMsg>();
            if (rcvBlockingQServer == null)
                rcvBlockingQServer = new BlockingQueue<SvcMsg>();
        }


        // ------< Implement service method to receive messages from other Client >-------
        public void PostMessage(SvcMsg msg)
        {
            rcvBlockingQ.enQ(msg);
        }


        // ------< Implement service method to receive messages from other Server >-------
        public void ServerMessage(SvcMsg msg)
        {
            rcvBlockingQServer.enQ(msg);
        }


        // ----< This will often block on empty queue, untill msg comes to this peer >------
        public static SvcMsg GetMessage()
        {
            return rcvBlockingQ.deQ();
        }

        public static SvcMsg GetMessageServer()
        {
            return rcvBlockingQServer.deQ();
        }

        public static void ThreadProcedure()
        {
            while (true)
            {
                 SvcMsg msg = GetMessage();
                 Dispatcher.clientProcessing(msg);
            }
        }

        public static void ThreadProcedureServer()
        {
            while (true)
            {
                SvcMsg msg = GetMessageServer();
                Dispatcher.serverProcessing(msg);              
            }
        }
    }

    /////////////////////////////////////////////////////////
    // Perform sending Operation to clients and servers

    public class Sender
    {
        public static void sendingClient(SvcMsg msg)
        {
            try
            {
                IMessageService proxy = CreateClientChannel(msg.dst.ToString());
                proxy.PostMessage(msg);
            }
            catch (Exception ex)
            {
                Console.Write("\n\n" + ex.Message);
            }
        }
        public static void sendingServer(SvcMsg msg)
        {
            try
            {
                IMessageService proxy = CreateClientChannel(msg.dst.ToString());
                proxy.ServerMessage(msg);
            }
            catch (Exception ex)
            {
                Console.Write("\n\n" + ex.Message);
            }
        }


        // ----------< Create proxy to another Peer's Communicator >------------ 
         public static IMessageService CreateClientChannel(string url)
         {
             BasicHttpBinding binding = new BasicHttpBinding();
             EndpointAddress address = new EndpointAddress(url);
             ChannelFactory<IMessageService> factory =
               new ChannelFactory<IMessageService>(binding, address);
             return factory.CreateChannel();
         }
    }
    class Server
    {
        // ----------< Create ServiceHost for Communication service >-----------
        static ServiceHost CreateServiceChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri baseAddress = new Uri(url);
            Type service = typeof(Receiver);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IMessageService), binding, baseAddress);
            return host;
        }
        
        static void Main(string[] args)
        {
            Console.Title = "Server1";           
            Server server = new Server();
            Intermediator im = new Intermediator();
            im.projectScanning(args[0]);
            im.intialTypeTable(args[0]);
           
            try
            {
                ServiceHost host = CreateServiceChannel("http://localhost:9001/MessageService");
                host.Open();

                Thread rcvThrd = new Thread(Receiver.ThreadProcedure);
                rcvThrd.IsBackground = true;
                rcvThrd.Start();

                Thread rcvThrdServer = new Thread(Receiver.ThreadProcedureServer);
                rcvThrdServer.IsBackground = true;
                rcvThrdServer.Start();

                Console.Write("\n  press key to terminate service\n");
                Console.ReadKey();
                Console.Write("\n");
                host.Close();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n" + ex.Message);
               
            }          
        }
    }
}
