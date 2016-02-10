///////////////////////////////////////////////////////////////////////
// XMLDecoder.cs - perfrom Decoding from XML                         //
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
 *   XMLDecoder - 
 *   Decoding: string to XML and then any particualar Data structure
 */
/*    
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
using System.Xml.Linq;

namespace DependencyAnalyzer
{
    public class ElemRelation  // holds data for Relationship
    {
        public string filenamefrom { get; set; }
        public string namespacefrom { get; set; }
        public string definedType { get; set; } //define which types of defined type like class, struct,..
        public string relationType { get; set; } // Inheritance, aggregation, Composition
        public string fromName { get; set; } //
        public string filenameto { get; set; }
        public string namespaceto { get; set; }
        public string toName { get; set; }
        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", filenamefrom));
            temp.Append(String.Format("{0,-10}", namespacefrom));
            temp.Append(String.Format("{0,-10}", definedType));
            temp.Append(String.Format("{0,-10}", relationType));
            temp.Append(String.Format("{0,-5}", fromName));  // line of scope start
            temp.Append(String.Format("{0,-10}", filenameto));
            temp.Append(String.Format("{0,-10}", namespaceto));
            temp.Append(String.Format("{0,-5}", toName));  // line of scope end
            temp.Append("}");
            return temp.ToString();
        }
    }

    public class ElemPackage // Holds the Data for Package Dependency
    {
        public string filenamefrom { get; set; }
        public string filenameto { get; set; }

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", filenamefrom));
            temp.Append(String.Format("{0,-10}", filenameto));
            temp.Append("}");
            return temp.ToString();
        }

    }
    public class XMLDecoder
    {
        // --------< Decode Project List from the XML string >---------
        public List<string> projectListResult(string project)
        {
            XDocument doc = XDocument.Parse(project);
            var q = from x in
                        doc.Descendants()
                    where (x.Name == "ProjectName")
                    select x;
            List<string> list = new List<string>();
            foreach (var elem in q)
            {
                list.Add(elem.Value);
            }
            return list;
        }


        // --------< Decode Type Dependency Result from the XML string >---------
        public List<ElemRelation> typeDependencyResult(string result)
        {
            XDocument doc = XDocument.Parse(result.ToString());
            var q = from x in
                        doc.Elements("Dependency")
                        .Elements("Type")
                    .Elements()
                    select x;
            List<ElemRelation> relationTable = new List<ElemRelation>();
            ElemRelation el = null;
            int t = 0;
            foreach (var elem in q)
            {
                if (t == 0)
                    el = new ElemRelation();
                if (elem.Name.ToString().Equals("FilenameFrom")){
                    el.filenamefrom = elem.Value;
                    t++;}
                if (elem.Name.ToString().Equals("NamespaceFrom")){
                    el.namespacefrom = elem.Value;
                    t++;}
                if (elem.Name.ToString().Equals("DefinedType")){
                    el.definedType = elem.Value;
                    t++;}
                if (elem.Name.ToString().Equals("FromName")){
                    el.fromName = elem.Value;
                    t++;}
                if (elem.Name.ToString().Equals("Realtion")){
                    el.relationType = elem.Value;
                    t++;}
                if (elem.Name.ToString().Equals("FilenameTo")){
                    el.filenameto = elem.Value;
                    t++;}
                if (elem.Name.ToString().Equals("NamespaceTo")){
                    el.namespaceto = elem.Value;
                    t++;}
                if (elem.Name.ToString().Equals("ToName")){
                    el.toName = elem.Value;
                    t++;}
                if (t == 8){
                    relationTable.Add(el);
                    t = 0;}
            }
            return relationTable;
        }

        // --------< Decode Package Dependency Result from the XML string >---------

        public List<ElemPackage> packageDependencyResult(string result)
        {
            XDocument doc = XDocument.Parse(result.ToString());
            var qp = from x in
                         doc.Elements("Dependency")
                         .Elements("Package")
                     .Elements()
                     select x;
           List<ElemPackage> packageTest = new List<ElemPackage>();
           ElemPackage el = null;
           int t = 0;
           foreach (var elem in qp)
           {
               if (t == 0)
               {
                   el = new ElemPackage();
               }
               if (elem.Name.ToString().Equals("PackageFrom"))
               {
                   el.filenamefrom = elem.Value;
                   t++;
               }
               if (elem.Name.ToString().Equals("PackageTo"))
               {
                   el.filenameto = elem.Value;
                   t++;
               }              
               if (t == 2)
               {
                   packageTest.Add(el);
                   t = 0;
               }
           }
            return packageTest;
        }


#if(TEST_XMLDECODER)
        static void Main(string[] args)
        {
            XMLDecoder xd=new XMLDecoder();
            XDocument xml = XDocument.Load("ProjectList");

            Console.Write("ProjectList:\n");
            List<string> project = xd.projectListResult(xml.ToString());
            foreach (string s in project)
                Console.Write("{0}\n",s);

            XDocument xml1 = XDocument.Load("DependencyResult");
            List<ElemRelation> relation = xd.typeDependencyResult(xml1.ToString());
            Console.Write("\n\n Type Dependency Result:\n");
            foreach (ElemRelation s in relation)
                Console.Write("{0} = {1} > {2}\n", s.fromName, s.relationType, s.toName);

            List<ElemPackage> package = xd.packageDependencyResult(xml1.ToString());
            Console.Write("\n\nPackage Dependency Result:\n");

            foreach (ElemPackage s in package)
                Console.Write("{0} ==>{1}\n", s.filenamefrom, s.filenameto);
        }
#endif
    }
}
