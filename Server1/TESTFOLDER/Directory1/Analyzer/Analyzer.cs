///////////////////////////////////////////////////////////////////////
// Analyzer.cs - Control whole Parsing Process                       //
// ver 1.1                                                           //
// Language:    C#, Visual Studio 13.0, .Net Framework 4.5           //
// Platform:    Dell Inspiron N5010, Win7 Professional, SP1          //
// Application: Demonstration for CIS 681, Project #4, Fall 2014     //
// Author:      Sunilkumar Lakkad, Syracuse University               //
//              (315) 751-5834, lakkads1@gmail.com                   //
// Source:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   Analyzer  - contains two methods doAnalysis & doRelationAnalysis for 
 *   different kind of analysis
 */
/* Required Files:
 *   IRulesAndActions.cs, RulesAndActions.cs, Parser.cs, Semi.cs, Toker.cs
 *   ScopeStack.cs  
 *   
 * Maintenance History:
 * --------------------
 * ver 1.1 : 14 Nov 2014
 * - Add two new mathod for more functionality
 * ver 1.0 : 9 Oct 2014
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
    public class Analyzer
    {
        static List<Elem> typetable= null;
        static List<ElemRelation> relationshipTable = null;

        //------< to find types defiend, functions and Complexity and size of functions >-----
        public static void doAnalysis(string[] files)
        {
            string filename = null;
            foreach (object file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    filename = file.ToString();
                    while (semi.getSemi())
                        parser.parse(semi, filename);
                    // filename store with types and function to identify which types belongs to which file.

                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                semi.close();
               typetable= RepositoryForOutput.storageForOutput_;
                                    
            }
        }

        public static List<Elem> getTypeTable()
        {
            return typetable;
        }

        // ------------< to find Relationship between types defiend >--------------
        public static void doRelationAnalysis(string[] files)
        {
            string filename = null;
            foreach (object file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }

                BuildCodeAnalyzerRelation builder = new BuildCodeAnalyzerRelation(semi);
                Parser parser = builder.build();

                try
                {
                    filename = file.ToString();
                    while (semi.getSemi())
                        parser.parse(semi, filename);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                semi.close();
                relationshipTable = RepositoryForRelation.storageForRelationship_;
            }
        }
        public static List<ElemRelation> getRelationshipTable()
        {
            return relationshipTable;
        }
        private static string[] getFiles(string path, List<string> patterns)
        {
            List<string> files = new List<string>();
            foreach (string pattern in patterns)
            {
                string[] newFiles = Directory.GetFiles(path, pattern);
                for (int i = 0; i < newFiles.Length; ++i)
                    newFiles[i] = Path.GetFullPath(newFiles[i]);

                files.AddRange(newFiles);
            }
            return files.ToArray();
        }
        private static void displayFileHeading(string filename)
        {
            Console.Write("\n\n Processing Files: {0}", filename);
            Console.Write("\n ------------------------------------------\n");
        }
        public static void displayTypeDefined(string[] files)
        {
            Console.Write("\n \tTypes Defined and Function of the File Set\n");
            Console.Write("==============================================================================");
            List<Elem> table = RepositoryForOutput.storageForOutput_;
            displayFileHeading(files[0]);
            int i = 0;

            foreach (Elem e in table)
            {
            check:
                if (!e.filename.Equals(files[i]))
                {
                    i++;
                    displayFileHeading(files[i]);
                    goto check;
                }
                Console.Write("\n {0,10} ===> {1,20}", e.type, e.name);
            }

            Console.WriteLine();
        }
        public static void displayRelation(string[] files)
        {
            Console.Write("\n\n\n \t\t\tOutput with a Relationship Data \n");
            Console.Write("==============================================================================");

            List<ElemRelation> table = RepositoryForRelation.storageForRelationship_;

            displayFileHeading(files[0]);
            int i = 0;

            foreach (ElemRelation e in table)
            {
            check:
                if (!e.filenamefrom.Equals(files[i]))
                {
                    i++;
                    displayFileHeading(files[i]); ;
                    goto check;
                }
                Console.Write("\n {0,5}, {1,25}, {2,25}", e.relationType, e.fromName, e.toName);
            }

            for (int j = i + 1; j <= files.Length - 1; j++)
            {
                displayFileHeading(files[j]);
            }
            Console.WriteLine();
        }
#if(TEST_ANALYZER)
        static void Main(string[] args)
        {
            Console.Write("\n Testing Analyzer Package");
            Console.Write("\n ========================\n");

            string path = "../../";
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
            string[] files = Analyzer.getFiles(path, patterns);
            doAnalysis(files);
            List<Elem> demo = getTypeTable();
            displayTypeDefined(files);
            doRelationAnalysis(files);
            List<ElemRelation> demo1 = getRelationshipTable();
            displayRelation(files);
        }
#endif
    }
}
