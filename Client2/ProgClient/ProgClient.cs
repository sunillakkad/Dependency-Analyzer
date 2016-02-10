///////////////////////////////////////////////////////////////////////////
// ProgClient.cs - Perform Message sending and Receiving                 //
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
 *   ProgClient - Which serve various request of the GUI
 */
/* Required Files:
 *   BlockingQueue.cs, ServerLibrary.cs, XMLDecoder.cs
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
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

namespace DependencyAnalyzer
{
    /////////////////////////////////////////////////////////////
    // Receiver hosts Communication service used by other Peers

    [ServiceBehavior(Namespace = "DependencyAnalyzer")]
    public class Receiver : IMessageService
    {
       static BlockingQueue<SvcMsg> rcvBlockingQ = null;
       SvcMsg check = new SvcMsg();

        static Receiver()
        {
            if (rcvBlockingQ == null)
                rcvBlockingQ = new BlockingQueue<SvcMsg>();
        }


        // Implement service method to receive messages from other Peers
        public void PostMessage(SvcMsg msg)
        {
            msg.ShowMessage();
            rcvBlockingQ.enQ(msg);
        }
        public void ServerMessage(SvcMsg msg)
        {        }

        // Implement service method to extract messages from other Peers.
        // This will often block on empty queue, so user should provide
        // read thread.
        public SvcMsg GetMessage()
        {
            return rcvBlockingQ.deQ();
        }
    }

    ///////////////////////////////////////////////////
    // Perform Sending Request to other peers

    public class Sender
    {
        string src = "http://localhost:8002/MessageService";
        static IMessageService CreateClientChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IMessageService> factory =
              new ChannelFactory<IMessageService>(binding, address);
            return factory.CreateChannel();
        }


        // send request to appropiate server for projectList
        public void sendRequestForprojectList(string dst)
        {
            IMessageService proxy = CreateClientChannel(dst);

            SvcMsg msg = new SvcMsg();
            msg.cmd = SvcMsg.Command.ProjectList;
            msg.src = new Uri(src);
            msg.dst = new Uri(dst);
            msg.body = "";
            proxy.PostMessage(msg);

        }


        // send request to appropiate server for Dependency
        public void sendRequestForAnalysis(string directory, string destination)
        {
            string dst1 = "";
            string dst2 = "";
            if (destination.Equals("http://localhost:9001/MessageService"))
            {
                 dst1 = "http://localhost:9001/MessageService";
                 dst2 = "http://localhost:9002/MessageService";
            }
            else
            {
                 dst2 = "http://localhost:9001/MessageService";
                 dst1 = "http://localhost:9002/MessageService";
            }

            IMessageService proxy = CreateClientChannel(dst1);
            SvcMsg msg = new SvcMsg();
            msg.cmd = SvcMsg.Command.Dependency;
            msg.src = new Uri(src);
            msg.dst = new Uri(dst1);
            msg.body = directory;
            proxy.PostMessage(msg);
            try
            {
                IMessageService proxy1 = CreateClientChannel(dst2);
                SvcMsg msg1 = new SvcMsg();
                msg1.cmd = SvcMsg.Command.DependencyAll;
                msg1.src = new Uri(src);
                msg1.dst = new Uri(dst2);
                msg1.body = directory;
                proxy1.PostMessage(msg1);
            }
            catch { }
        }       
    }


    public class ProgClient
    {
        List<string> outGoingTypeDependency = null;
        List<string> outGoingPackageDependency = null;
        List<string> inComingTypeDependency = null;
        List<string> inComingPackageDependency = null;
        string outGoingXML = null;
        string inComingXML = null;


        //  Create ServiceHost for Communication service
        public static ServiceHost CreateServiceChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri baseAddress = new Uri(url);
            Type service = typeof(Receiver);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IMessageService), binding, baseAddress);
            return host;
        }


        public List<string> getProjectList(string project)
        {
            XMLDecoder xd=new XMLDecoder();
            return xd.projectListResult(project);
        }


        // perform various task to retrive the result of Out Going 
        // Dependency in appropiate Data structure
        public void outGoingResultAnalysis(string result)
        {
            outGoingXML=result;
            XMLDecoder xd=new XMLDecoder();
            List<ElemRelation> tResult = xd.typeDependencyResult(result);
            List<ElemPackage> dResult = xd.packageDependencyResult(result);

            outDictinaryToString(dResult);
            outListToString(tResult);

        }


        public void outDictinaryToString(List<ElemPackage> dResult)
        {
            StringBuilder sb = new StringBuilder();
            List<string> store = new List<string>();
            sb.Append("Out Going Package Dependency\n====================================================\n\n");
            sb.Append("Filename From\t\t\t\t\t\tDEPENDS ON ==>\t Filename To\n");
            sb.Append("-------------\t\t\t\t\t\t--------------\t -----------\n");
            foreach (ElemPackage ep in dResult)
            {
                sb.Append("\n" + ep.filenamefrom + "   DEPENDS ON ==>\t");
                sb.Append(ep.filenameto);
                store.Add(sb.ToString());
                sb.Clear();
            }
            outGoingPackageDependency = store;
        }


        public List<string> getOutPackageDependecyResult()
        {
            return outGoingPackageDependency;
        }


        public void outListToString(List<ElemRelation> tResult)
        {
            StringBuilder sb = new StringBuilder();
            List<string> store = new List<string>();
            sb.Append("Out Going Type Dependency\n=====================================================================\n\n");
            sb.Append("Filename From\t\t\t\t\t\tNamespace From\tType\tType_Name From\t  :  Realtionship  ==>  Type_Name To\tNamespace To\tFilename To\n");
            sb.Append("-------------\t\t\t\t\t\t--------------\t----\t--------------\t  :  ------------  ==>  ------------\t------------\t------------\n");
            foreach (ElemRelation e in tResult)
            {

                sb.Append("\n" + e.filenamefrom + "\t");
                sb.Append(e.namespacefrom + "\t");
                sb.Append(e.definedType + "\t");
                sb.Append(e.fromName + "\t");
                sb.Append("  :  " + e.relationType + "  ==>  ");
                sb.Append(e.toName + "\t");
                sb.Append(e.namespaceto + "\t");
                sb.Append(e.filenameto);
                store.Add(sb.ToString());
                sb.Clear();
            }
            outGoingTypeDependency = store;
        }


        public List<string> getOutTypeDependecyResult()
        {
            return outGoingTypeDependency;
        }


        public string getOutGoingXML()
        {
            return outGoingXML;
        }


        // perform various task to retrive the result of In coming 
        // Dependency in appropiate Data structure
        public void inComingResultAnalysis(string result)
        {
            inComingXML = result;
            XMLDecoder xd = new XMLDecoder();
            List<ElemRelation> tResult = xd.typeDependencyResult(result);

            List<ElemPackage> dResult = xd.packageDependencyResult(result);

            inDictinaryToString(dResult);
            inListToString(tResult);

        }


        public void inDictinaryToString(List<ElemPackage> dResult)
        {
            StringBuilder sb = new StringBuilder();
            List<string> store = new List<string>();
            sb.Append("In Coming Package Dependency ()\n============================================\n\n");
            sb.Append("Filename From\t\t\t\t\t\tDEPENDS ON ==>\t Filename To\n");
            sb.Append("-------------\t\t\t\t\t\t--------------\t -----------\n");
            foreach (ElemPackage ep in dResult)
            {
                sb.Append("\n" + ep.filenamefrom + "   DEPENDS ON ==>\t");
                sb.Append(ep.filenameto);
                store.Add(sb.ToString());
                sb.Clear();
            }
            inComingPackageDependency = store;
        }


        public List<string> getInPackageDependecyResult()
        {
            return inComingPackageDependency;
        }


        public void inListToString(List<ElemRelation> tResult)
        {
            StringBuilder sb = new StringBuilder();
            List<string> store = new List<string>();
            sb.Append("In Coming Type Dependency\n=====================================================================\n\n");
            sb.Append("Filename From\t\t\t\t\t\tNamespace From\tType\tType_Name From\t  :  Realtionship  ==>  Type_Name To\tNamespace To\tFilename To\n");
            sb.Append("-------------\t\t\t\t\t\t--------------\t----\t--------------\t  :  ------------  ==>  ------------\t------------\t------------\n");
            foreach (ElemRelation e in tResult)
            {

                sb.Append("\n" + e.filenamefrom + "\t");
                sb.Append(e.namespacefrom + "\t");
                sb.Append(e.definedType + "\t");
                sb.Append(e.fromName + "\t");
                sb.Append("  :  " + e.relationType + "  ==>  ");
                sb.Append(e.toName + "\t");
                sb.Append(e.namespaceto + "\t");
                sb.Append(e.filenameto);
                store.Add(sb.ToString());
                sb.Clear();
            }
            inComingTypeDependency = store;
        }


        public List<string> getInTypeDependecyResult()
        {
            return inComingTypeDependency;
        }


        public string getInComingXML()
        {
            return inComingXML;
        }

#if(TEST_PROGCLIENT)
        static void Main(string[] args)
        {
            Console.Title = "Client1";
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
            string address = "http://" + localIP + ":8002/MessageService";
            Console.Write("Local: {0}\n", localIP);
            Console.Write("Starting Message Service on Client");

            ServiceHost host = CreateServiceChannel(address);
            host.Open();

            string dst1 = "http://localhost:9001/MessageService";
            Sender snd = new Sender();
            
            snd.sendRequestForprojectList(dst1);
            snd.sendRequestForAnalysis("D:\\SMA\\Project4\\Server1\\TESTFOLDER\\Directory1",dst1);  
         
            Console.Write("\n  press key to terminate service");
            Console.ReadKey();
            Console.Write("\n");
        }
#endif
    }
}
