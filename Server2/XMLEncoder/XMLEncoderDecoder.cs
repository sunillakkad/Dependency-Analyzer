///////////////////////////////////////////////////////////////////////
// XMLEncoderDecoder.cs - perfrom Encoding and Decoding from/to XML  //
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
 *   XMLEncoderDecoder - 
 *   Encoding: XML to String
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DependencyAnalyzer
{
    public class XMLEncoderDecoder
    {
        // ----------< Convert Project List in to XML and then String >------------ 
        public string projectXML(List<string> projects)
        {
            if (projects.Count == 0)
                return "there is No Projects available on the Server side";
            XDocument xml = new XDocument();
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XElement root = new XElement("PROJECTLIST");
            xml.Add(root);
            foreach(string s in projects)
            {
                XElement file = new XElement("ProjectName", s);
                root.Add(file);
            }
            xml.Save("ProjectList");
            return xml.ToString();
        }

        // ----------< Convert Type Table List in to XML and then String >------------ 
        public string typeTableXML(List<Elem> typetable)
        {
            if (typetable.Count == 0)
                return "No TypeTable";
            XDocument xml = new XDocument();
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XElement root = new XElement("TYPETABLE");
            xml.Add(root);
            foreach (Elem s in typetable)
            {
                XElement file = new XElement("Filename", s.filename);
                root.Add(file);
                XElement nspace = new XElement("NamespaceName", s.namespacename);
                root.Add(nspace);
                XElement type = new XElement("Type", s.type);
                root.Add(type);
                XElement tname = new XElement("TypeName", s.name);
                root.Add(tname);
            }          
            return xml.ToString();
            
        }

        // ----------< Convert Dependency Result in to XML and then String >------------ 

        public string dependencyResultXML(List<ElemRelation> relation, List<ElemPackage> package)
        {
            XDocument xml = new XDocument();
            xml.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            XElement root = new XElement("Dependency");
            xml.Add(root);
            XElement  type= new XElement("Type");
            root.Add(type);
            foreach (ElemRelation s in relation)
            {
                XElement filef = new XElement("FilenameFrom", s.filenamefrom);
                type.Add(filef);
                XElement nspacef = new XElement("NamespaceFrom", s.namespacefrom);
                type.Add(nspacef);
                XElement dtype = new XElement("DefinedType", s.definedType);
                type.Add(dtype);
                XElement fname = new XElement("FromName", s.fromName);
                type.Add(fname);
                XElement realtion = new XElement("Realtion", s.relationType);
                type.Add(realtion);
                XElement filet = new XElement("FilenameTo", s.filenameto);
                type.Add(filet);
                XElement nspacet = new XElement("NamespaceTo", s.namespaceto);
                type.Add(nspacet);
                XElement tname = new XElement("ToName", s.toName);
                type.Add(tname);
            }
            XElement pack = new XElement("Package");
            root.Add(pack);
            foreach (ElemPackage ep in package)
            {
                XElement packageFrom = new XElement("PackageFrom", ep.filenamefrom);
                pack.Add(packageFrom);
                XElement packageTo = new XElement("PackageTo", ep.filenameto);
                pack.Add(packageTo);
            }
            xml.Save("DependencyResult");
            return xml.ToString();
        }

        // ----------< Convert given String to XML and then Type Table List >------------ 
        public List<Elem> typeTableDecoding(string typeTableXML)
        {
            XDocument doc = XDocument.Parse(typeTableXML);
            var q = from x in
                        doc.Elements("TYPETABLE")
                    .Elements()
                    select x;

            List<Elem> typeTable = new List<Elem>();
            Elem el = null;
            int t = 0;
            foreach (var elem in q)
            {
                if (t == 0)
                {
                    el = new Elem();
                }
                if (elem.Name.ToString().Equals("Filename"))
                {
                    el.filename = elem.Value;
                    t++;
                }
                if (elem.Name.ToString().Equals("NamespaceName"))
                {
                    el.namespacename = elem.Value;
                    t++;
                }
                if (elem.Name.ToString().Equals("Type"))
                {
                    el.type = elem.Value;
                    t++;
                }
                if (elem.Name.ToString().Equals("TypeName"))
                {
                    el.name = elem.Value;
                    t++;
                }
                if (t == 4)
                {
                    typeTable.Add(el);
                    t = 0;
                }
            }
            return typeTable;
        }

        private static void repositoryForTestUP()
        {
            Elem e = new Elem();
            e.type = "class";
            e.name = "sunil";
            e.filename = "file1";
            e.namespacename = "xyz";
            RepositoryForOutput.storageForOutput_.Add(e);
            Elem e1 = new Elem();
            e1.type = "class";
            e1.name = "sunil1";
            e1.filename = "file2";
            e1.namespacename = "xyz";
            RepositoryForOutput.storageForOutput_.Add(e1);

            ElemRelation er = new ElemRelation();
            er.relationType = "Aggregation";
            er.fromName = "Display";
            er.toName = "Xyz";
            RepositoryForRelation.storageForRelationship_.Add(er);
            ElemRelation er1 = new ElemRelation();
            er1.relationType = "Inheritance";
            er1.fromName = "Display";
            er1.toName = "Test";
            RepositoryForRelation.storageForRelationship_.Add(er1);
        }
#if(TEST_XMLENCODERDECODER)
        static void Main(string[] args)
        {
            string[] dir = Directory.GetDirectories(Path.GetFullPath("../../../TESTFOLDER"));
            List<string> dir1 = new List<string>();
            foreach (string d in dir)
                dir1.Add(d);
            XMLEncoderDecoder xed=new XMLEncoderDecoder();
            Console.Write("ProjectList:\n{0}", xed.projectXML(dir1));

            repositoryForTestUP();
            Console.Write("\nType Table XML:\n{0}", xed.typeTableXML(RepositoryForOutput.storageForOutput_));

            List<ElemPackage> package = new List<ElemPackage>();
            ElemPackage ep = new ElemPackage();
            ep.filenamefrom = "file1";
            ep.filenameto = "file2";
            ElemPackage ep1 = new ElemPackage();
            ep1.filenamefrom = "file3";
            ep1.filenameto = "file4";
            package.Add(ep);
            package.Add(ep1);
            Console.Write("\nDependency Result XML:\n{0}", xed.dependencyResultXML(RepositoryForRelation.storageForRelationship_,package));

            List<Elem> type = xed.typeTableDecoding(xed.typeTableXML(RepositoryForOutput.storageForOutput_));
            foreach (Elem e in type)
                Console.Write("\n {0} {1} {2} {3}", e.filename, e.namespacename, e.type, e.name);
        }
#endif
    }
}
