///////////////////////////////////////////////////////////////////////////
// ServiceLibrary.cs - Interfaces for ServiceContract                    //
// ver 1.1                                                               //
// Language:    C#, Visual Studio 13.0, .Net Framework 4.5               //
// Platform:    Dell Inspiron N5010, Win7 Professional, SP1              //
// Application: Demonstration for CIS 681, Project #2, Fall 2014         //
// Author:      Sunilkumar Lakkad, Syracuse University                   //
//              (315) 751-5834, lakkads1@gmail.com                       //
// Source:      Jim Fawcett, CST 4-187, Syracuse University              //
//              (315) 443-3948, jfawcett@twcny.rr.com                    //
///////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following classes:
 *   IMessageService   - interface for OperationContract
 *   SvcMsg - Class for ServiceContract
 */
/*
 * Note:
 * This package does not have a test stub as it contains only interfaces
 * and abstract classes.
 *
 *  * ver 1.1 : 14 Nov 2014
 * - Add one MessageServer function in interface for server to server 
 *   communication 
 * ver 1.0 : 5 Nov 2014
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Web;

namespace DependencyAnalyzer
{
    [DataContract(Namespace = "DependencyAnalyzer")]
    public class SvcMsg
    {
        public enum Command { ProjectList, Dependency, DependencyAll, Request, Response };
        [DataMember]
        public Command cmd;
        [DataMember]
        public Uri src;
        [DataMember]
        public Uri dst;
        [DataMember]
        public string body;

        public void ShowMessage()
        {
            Console.Write("\n  Received Message:");
            Console.Write("\n    src = {0}\n    dst = {1}", src.ToString(), dst.ToString());
            Console.Write("\n    cmd = {0}", cmd.ToString());
            Console.Write("\n    body: {0}\n", body);
        }
    }
    [ServiceContract(Namespace = "DependencyAnalyzer")]
    public interface IMessageService
    {
        [OperationContract]
        void PostMessage(SvcMsg msg);
        [OperationContract]
        void ServerMessage(SvcMsg msg);
    }

}
