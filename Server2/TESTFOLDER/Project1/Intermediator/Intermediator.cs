///////////////////////////////////////////////////////////////////////
// Intermediator.cs - Communicator between Server system and         //
//                    Analyzing System                               //
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
 *   Intermediator  - contains various methods, works as intermediator 
 *   for Server Communication system and Analyzing System 
 */
/* Required Files:
 *   Analyzer.cs, Filemanager.cs, IRulesAndActions.cs, RulesAndActions.cs, 
 *   Parser.cs, Semi.cs, Toker.cs, ScopeStack.cs, ProjectScanner.cs, 
 *   XMLEncoderDecoder.cs
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
using System.IO;

namespace DependencyAnalyzer
{
    public class Intermediator
    {
        static string directory = null;
        static string typeTable = null;
        string dependencyResult = null;
        string dependencyAllResult = null;


        // ----------------< perform Initial Project scanning >---------------- 
        public void projectScanning(string path)
        {
            ProjectScanner ps = new ProjectScanner();
            List<string> dir = ps.getProjectList(path);

            XMLEncoderDecoder xe = new XMLEncoderDecoder();
            directory = xe.projectXML(dir);
        }

        public string getprojectList()
        {
            return directory;
        }


        // ----------------< Build Intial Type Table on Server Start up >---------------- 
        public void intialTypeTable(string path)
        {

            List<string> pattern = new List<string>();
            pattern.Add("*.cs");

            FileManager fm = new FileManager();
            string[] files = fm.getFiles(path, pattern, true).ToArray();
            if (files.Length != 0)
            {
                try
                {
                    Analyzer.doAnalysis(files);

                    XMLEncoderDecoder xe = new XMLEncoderDecoder();
                    typeTable = xe.typeTableXML(Analyzer.getTypeTable());
                }
                catch { }
            }
            else
            { return; }
        }

        public string getTypeTable()
        {
            return typeTable;
        }


        // ---< perform Type Table Merging whenever server get type table from other server >---
        public void typeTableMerging(string typeTableXML)
        {
            try
            {
                XMLEncoderDecoder xed = new XMLEncoderDecoder();
                List<Elem> typeFromOtherServer = xed.typeTableDecoding(typeTableXML);

                TypeTableMerging ttm = new TypeTableMerging();
                ttm.typeTableMerging(typeFromOtherServer);
            }
            catch { }
        }


        // ----------------< perform Out Going dependency using Complete TypeTable >---------------- 
        public void dependencyAnalysis(string directoryName)
        {
            List<string> pattern = new List<string>();
            pattern.Add("*.cs");

            FileManager fm = new FileManager();
            string[] files = fm.getFiles(directoryName, pattern, true).ToArray();

            if (files.Length != 0)
            {
                try
                {
                    Analyzer.doRelationAnalysis(files);
                    List<ElemRelation> relationship = Analyzer.getRelationshipTable();

                    PackageDependency pd = new PackageDependency();                
                    List<ElemPackage> package = pd.getPackageDependency();

                    RemoveDuplicate rd = new RemoveDuplicate();
                    List<ElemRelation> relationship1 = rd.removeDuplicate(relationship);
                    List<ElemPackage> package1 = rd.removePDuplicate(package);

                    XMLEncoderDecoder xe = new XMLEncoderDecoder();
                    dependencyResult = xe.dependencyResultXML(relationship1, package1);

                    Console.Write("\n\n {0}", dependencyResult);
                }
                catch { }
            }
            else
            { return; }
        }


        // ----------------< perform In Coming dependency using Complete TypeTable >---------------- 
        public void dependencyAnalysisAll(string directoryName)
        {

            List<string> pattern = new List<string>();
            pattern.Add("*.cs");

            FileManager fm = new FileManager();
            string[] files = fm.getFiles("../../../TESTFOLDER", pattern, true).ToArray();

            if (files.Length != 0)
            {
                try
                {
                    Analyzer.doRelationAnalysis(files);
                    List<ElemRelation> relationship = Analyzer.getRelationshipTable();

                    PackageDependency pd = new PackageDependency();
                    List<ElemPackage> package = pd.getPackageDependency();

                    RemoveDuplicate rd = new RemoveDuplicate();
                    List<ElemRelation> relationship1 = rd.removeDuplicate(relationship);
                    List<ElemPackage> package1 = rd.removePDuplicate(package);

                    List<ElemRelation> relationshipTo = new List<ElemRelation>();
                    foreach (ElemRelation er in relationship1)
                    {
                        if (Path.GetDirectoryName(er.filenameto).Equals(directoryName))
                            relationshipTo.Add(er);
                    }

                    List<ElemPackage> packageTo = new List<ElemPackage>();
                    foreach (ElemPackage ep in package)
                    {
                        if (Path.GetDirectoryName(ep.filenameto).Equals(directoryName))
                        {
                            ElemPackage e = new ElemPackage();
                            e.filenamefrom = ep.filenamefrom;
                            e.filenameto = ep.filenameto;
                            packageTo.Add(e);
                        }
                    }

                    XMLEncoderDecoder xe = new XMLEncoderDecoder();
                    dependencyAllResult = xe.dependencyResultXML(relationshipTo, packageTo);
                }
                catch { }
            }
            else
            { return; }
        }

        public string getDependencyResult()
        {
            return dependencyResult;
        }

        public string getDependencyAllResult()
        {
            return dependencyAllResult;
        }
        public void clearRepository()
        {
            RepositoryForRelation.storageForRelationship_.Clear();
        }

#if(TEST_INTERMEDIATOR)
        static void Main(string[] args)
        {
            Intermediator im = new Intermediator();
            im.projectScanning("../../../TESTFOLDER");
            string dire = im.getprojectList();
            Console.Write("\n\nProjectList:\n{0}", dire);

            im.intialTypeTable("../../../TESTFOLDER");
            string typeT = im.getTypeTable();
            Console.Write("\n\nOwn TypeTable:\n{0}", typeT);

            im.typeTableMerging(typeT);

            string direName = "../../../TESTFOLDER/Directory1";
            im.dependencyAnalysis(direName);

            string direName1 = "../../../TESTFOLDER/Directory2";
            im.dependencyAnalysisAll(direName1);

            string depenResult = im.getDependencyResult();
            Console.Write("\n\nDependency Result:\n{0}", depenResult);

            string depenAllResult = im.getDependencyAllResult();
            Console.Write("\n\nInComing Dependency Result:\n{0}", depenAllResult);
        }
#endif
    }
    public class DistinctItemComparer : IEqualityComparer<ElemRelation>
    {
        public bool Equals(ElemRelation x, ElemRelation y)
        {
            return x.filenamefrom == y.filenamefrom &&
                x.namespacefrom == y.namespacefrom &&
                x.definedType == y.definedType &&
                x.fromName == y.fromName &&
                x.relationType == y.relationType &&
                x.filenameto == y.filenameto &&
                x.namespaceto == y.namespaceto &&
                x.toName == y.toName;
        }
        public int GetHashCode(ElemRelation obj)
        {
            return obj.filenamefrom.GetHashCode() ^
                obj.namespacefrom.GetHashCode() ^
                obj.definedType.GetHashCode() ^
                obj.fromName.GetHashCode() ^
                obj.relationType.GetHashCode() ^
                obj.filenameto.GetHashCode() ^
                obj.namespaceto.GetHashCode() ^
                obj.toName.GetHashCode();
        }
    }

    public class DistinctItemComparerP : IEqualityComparer<ElemPackage>
    {
        public bool Equals(ElemPackage x, ElemPackage y)
        {
            return x.filenamefrom == y.filenamefrom &&
                x.filenameto == y.filenameto;               
        }
        public int GetHashCode(ElemPackage obj)
        {
            return obj.filenamefrom.GetHashCode() ^
                obj.filenameto.GetHashCode();
        }
    }
    /////////////////////////////////////////////////////////
    // Use to Remove Duplicate from List
    public class RemoveDuplicate
    {
        public List<ElemRelation> removeDuplicate(List<ElemRelation> relationship)
        {
            var distinctItems = relationship.Distinct(new DistinctItemComparer());
            return distinctItems.ToList();
        }

        public List<ElemPackage> removePDuplicate(List<ElemPackage> relationship)
        {
            var distinctItems = relationship.Distinct(new DistinctItemComparerP());
            return distinctItems.ToList();
        }
    }
}
