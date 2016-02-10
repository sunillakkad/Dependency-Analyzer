///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 3.1                                                           //
// Language:    C#, Visual Studio 13.0, .Net Framework 4.5           //
// Platform:    Dell Inspiron N5010, Win7 Professional, SP1          //
// Application: Demonstration for CIS 681, Project #2, Fall 2014     //
// Author:      Sunilkumar Lakkad, Syracuse University               //
//              (315) 751-5834, lakkads1@gmail.com                   //
// Source:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectDelegate rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   - DetectInheritance
 *   - DetectAggregation
 *   - DetectComposition
 *   - DetectUsing
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 3.1 : 9 Oct 2014
 * - added new class and mathod for merging Type Table and finding Package 
 *   Dependency
 * - remove rules for finding complexity
 *  ver 3.0 : 9 Oct 2014
 * - added new rules for finding complexity and relationship 
 * - added new repository for output and store relationship data result
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace DependencyAnalyzer
{
    public class Elem  // holds scope information
    {
        public string servername { get; set; }
        public string filename { get; set; }
        public string namespacename { get; set; }
        public string type { get; set; }
        public string name { get; set; }

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", filename)).Append(" => ");
            temp.Append(String.Format("{0,-10}", namespacename)).Append(" : ");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            
            temp.Append("}");
            return temp.ToString();
        }
    }

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

    public class ElemPackage
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
    public class RepositoryForOutput //store data for all the files
    {
        public static List<Elem> storageForOutput_ = new List<Elem>();
        public List<Elem> storageForOutput
        {
            get { return storageForOutput_; }
            set { storageForOutput_=value; }
        }
    }

    public  class RepositoryForRelation //store Relationship output data for all files
    {
         public static List<ElemRelation> storageForRelationship_ = new List<ElemRelation>();
         public List<ElemRelation> storageForRelationship
        {
            get { return storageForRelationship_; }
            set { storageForRelationship_ = value; }
        }
    }

    /////////////////////////////////////////////////////////
    // Use for TYPE TABLE Merging
    public class TypeTableMerging
    {
        public void typeTableMerging(List<Elem> typeFromOtherServer)
        {
            List<Elem> temp3 = RepositoryForOutput.storageForOutput_;
            try
            {
                foreach (Elem e in typeFromOtherServer)
                {
                    if (!(temp3.Exists(x => x.filename == e.filename && x.namespacename == e.namespacename && x.type == e.type && x.name == e.name)))              //notworking
                        RepositoryForOutput.storageForOutput_.Add(e);
                }
            }
            catch { }
        }
    }

    /////////////////////////////////////////////////////////
    // Perform Package Dependency
    public class PackageDependency
    {
        public List<ElemPackage> getPackageDependency()
        {
            List<ElemRelation> relation = RepositoryForRelation.storageForRelationship_;
            List<ElemPackage> package=new List<ElemPackage>();
            Dictionary<string, string> package1 = new Dictionary<string, string>();
            try
            {
                //foreach (ElemRelation e in relation)
                //{
                //if (!e.filenamefrom.Equals(e.filenameto))
                //{
                //    package1.Add(e.filenamefrom,e.filenameto);
                //}

                //}

                foreach (ElemRelation e in relation)
                {
                    ElemPackage ep = new ElemPackage();
                    if (!e.filenamefrom.Equals(e.filenameto))
                    {
                        ep.filenamefrom = e.filenamefrom;
                        ep.filenameto = e.filenameto;
                        package.Add(ep);
                    }
                }

                //foreach (ElemRelation e in relation)
                //{
                //    if (!e.filenamefrom.Equals(e.filenameto))
                //    {
                //        foreach (ElemPackage v in package)
                //        {
                //            if (!v.filenamefrom.Equals(e.filenamefrom) && !v.filenameto.Equals(e.filenameto))
                //            {
                //                ElemPackage ep=new ElemPackage();
                //                ep.filenamefrom=e.filenamefrom;
                //                ep.filenameto=e.filenameto;
                //                package.Add(ep);
                //            }
                //        }
                //    }
                //}
            }
            catch { }
            return package;
        }
    }
    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        static Repository instance;

        public Repository()
        {
            instance = this;
        }
        public static Repository getInstance()
        {
            return instance;
        }
        // provides all actions access to current semiExp
        public CSsemi.CSemiExp semi
        {
            get;
            set;
        }
        // semi gets line count from toker who counts lines
        // while reading from its source
        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount; }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }
        // enables recursively tracking entry and exit from scopes

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }
        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
        }
    }
    /////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope

    public class PushStack : AAction
    {
        Repository repo_;
        public PushStack(Repository repo)
        {
            repo_ = repo;
        }
        private string namespacematch()
        {
            int x = repo_.locations.Count;
            string test = "xyz";
            try
            {
                for (int i = x-1; i >= 0; i--)
                {
                    if (repo_.locations[i].type.Equals("namespace"))
                    {
                        test = repo_.locations[i].name;
                        break;
                    }
                }
                return test;
            }
            catch
            {
                return test;
            }

        }
        public override void doAction(CSsemi.CSemiExp semi, string filename)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.filename = filename;
            if (elem.type.Equals("namespace"))
                elem.namespacename = elem.name;
            else
                elem.namespacename = namespacematch();
            //repo_.stack.push(elem);
            repo_.locations.Add(elem);
            if(!elem.type.Equals("namespace"))
            RepositoryForOutput.storageForOutput_.Add(elem);

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            if (AAction.displayStack)
                repo_.stack.display();

        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // pushes relationship data  info on stack when any action found for Relation

    public class PushStackRelation : AAction
    {
        Repository repo_;
        public PushStackRelation(Repository repo)
        {
            repo_ = repo;
        }
        Elem e = new Elem();
        private string namespacematch()
        {
            int x = repo_.locations.Count;
            string test = "xyz";
            try
            {
                for (int i = x - 1; i >= 0; i--)
                {
                    if (repo_.locations[i].type.Equals("namespace"))
                    {
                        test = repo_.locations[i].name;
                        break;
                    }
                }
                return test;
            }
            catch
            {
                return test;
            }

        }
        private void inheritance(CSsemi.CSemiExp semi, string filename)
        {
            ElemRelation elem = new ElemRelation();
            elem.filenamefrom = filename;
            elem.namespacefrom = namespacematch();
            elem.definedType = semi[0];
            elem.relationType = semi[1];
            elem.fromName = semi[2];
            elem.toName = semi[3];
            elem.filenameto = semi[4];
            elem.namespaceto = semi[5];
            RepositoryForRelation.storageForRelationship_.Add(elem);
        }
        private void aggregation(CSsemi.CSemiExp semi, string filename)
        {
            ElemRelation elem = new ElemRelation();
            elem.filenamefrom = filename;
            elem.namespacefrom = namespacematch();
            elem.relationType = semi[0];
            elem.toName = semi[1];
            elem.filenameto = semi[3];
            elem.namespaceto = semi[4];

            int x = repo_.locations.Count;
            Elem temp = repo_.locations[x - 1];
            elem.fromName = temp.name;
            elem.definedType = temp.type;

            if (!elem.fromName.Equals(elem.toName))
                RepositoryForRelation.storageForRelationship_.Add(elem);
        }
        private void composition(CSsemi.CSemiExp semi, string filename)
        {
            ElemRelation elem = new ElemRelation();
            elem.filenamefrom = filename;
            elem.namespacefrom = namespacematch();
            elem.relationType = semi[0];
            elem.toName = semi[1];
            elem.filenameto = semi[2];
            elem.namespaceto = semi[3];
            
            int x = repo_.locations.Count;
            Elem temp = repo_.locations[x - 1];
            elem.fromName = temp.name;
            elem.definedType = temp.type;

            if (!elem.fromName.Equals(elem.toName))
                RepositoryForRelation.storageForRelationship_.Add(elem);
        }
        private void usingRelation(CSsemi.CSemiExp semi, string filename)
        {
            ElemRelation elem = new ElemRelation();
            elem.filenamefrom = filename;
            elem.namespacefrom = namespacematch();
            elem.relationType = semi[0] + "      ";
            elem.toName = semi[1];
            elem.filenameto = semi[2];
            elem.namespaceto = semi[3];

            int x = repo_.locations.Count;
            Elem temp = repo_.locations[x - 1];
            elem.fromName = temp.name;
            elem.definedType = temp.type;

            if (!elem.fromName.Equals(elem.toName))
                RepositoryForRelation.storageForRelationship_.Add(elem);
        }
        public override void doAction(CSsemi.CSemiExp semi, string filename)
        {
            if (semi[1].Equals("Inheritance")) //Action for Inheritance
            {
                inheritance(semi, filename);
                return;
            }
            if (semi[0].Contains("Aggregation"))
            {
                aggregation(semi, filename);
                return;
            }
            if (semi[0].Contains("Composition"))
            {
                composition(semi, filename);
                return;
            }
            if (semi[0].Contains("Using"))
            {
                usingRelation(semi, filename);
                return;
            }
            if (semi[0].Contains("namespace"))
            {
                Elem ele = new Elem();
                ele.type = semi[0];
                ele.name = semi[1];
                ele.filename = filename;
                repo_.locations.Add(ele);
            }             
            if (semi[0].Contains("class"))
            {
                Elem ele = new Elem();
                ele.type = semi[0];
                ele.name = semi[1];
                ele.filename = filename;
                repo_.locations.Add(ele);
            }
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            if (AAction.displayStack)
                repo_.stack.display();
        }
    }




    /////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(CSsemi.CSemiExp semi, string filename)
        {
            int index = semi.Contains("namespace");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local, filename);
                return true;
            }
            return false;
        }
    }

    /////////////////////////////////////////////////////////
    // rule to detect delegate declarations

    public class DetectDelegate : ARule
    {
        public override bool test(CSsemi.CSemiExp semi, string filename)
        {
            int index = semi.Contains("delegate");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 2]);
                doActions(local, filename);
                return true;
            }
            return false;
        }
    }

    /////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        public override bool test(CSsemi.CSemiExp semi, string filename)
        {
            int indexCL = semi.Contains("class");
            int indexIF = semi.Contains("interface");
            int indexST = semi.Contains("struct");
            int indexEN = semi.Contains("enum");

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEN);
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local, filename);
                return true;
            }
            return false;
        }
    }



    /////////////////////////////////////////////////////////
    // rules to detect Inheritance

    public class DetectInheritance : ARule
    {
        List<Elem> table = RepositoryForOutput.storageForOutput_;
        private void inhertanceTest(CSsemi.CSemiExp semi, int index, string filename)
        {
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            CSsemi.CSemiExp local1 = new CSsemi.CSemiExp();
            local.displayNewLines = false;
            local1.displayNewLines = false;

            if ((semi[index + 2].Equals(":")))
            {
                foreach (Elem check in table)
                {
                    if (check.name.Equals(semi[index + 3]))
                    {
                        local.Add(semi[index]).Add("Inheritance").Add(semi[index + 1]).Add(semi[index + 3]).Add(check.filename).Add(check.namespacename);
                        doActions(local, filename);
                        break;
                    }
                }
            }
            if ((semi[index + 2].Equals(":")) && (semi[index + 4].Equals(",")))
            {
                foreach (Elem check in table)
                {
                    if (check.name.Equals(semi[index + 5]))
                    {
                        local1.Add(semi[index]).Add("Inheritance").Add(semi[index + 1]).Add(semi[index + 5]).Add(check.filename).Add(check.namespacename);
                        doActions(local1, filename);
                        break;
                    }
                }
            }
        }
        public override bool test(CSsemi.CSemiExp semi, string filename)
        {
            int index = semi.Contains("class");
            try
            {
                if (index != -1)
                {
                    foreach (Elem e in table)
                    {
                        if (e.name.Equals(semi[index + 1]))
                        {
                            inhertanceTest(semi, index, filename);
                            return false;
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }
    }


    ///////////////////////////////////////////////////////////
    // rules to detect Aggregation
    public class DetectAggregation : ARule
    {
        private void aggregationTest(CSsemi.CSemiExp semi, int index, string filename, string filenameto, string namespacename)
        {
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            if ((semi[index + 2].Equals(".")))
            {
                local.Add("Aggregation").Add(semi[index + 3]).Add(semi[index - 2]).Add(filenameto).Add(namespacename);
                doActions(local, filename);
            }
            else
            {
                local.Add("Aggregation").Add(semi[index + 1]).Add(semi[index - 2]).Add(filenameto).Add(namespacename);
                doActions(local, filename);
            }
        }

        public override bool test(CSsemi.CSemiExp semi, string filename)
        {
            int index = semi.Contains("new");
            try
            {
                if (index != -1)
                {
                    List<Elem> table = RepositoryForOutput.storageForOutput_;
                    foreach (Elem e in table)
                    {
                        if ((e.name.Equals(semi[index + 1])) && !(e.type.Equals("function")) && !semi[index + 1].Equals("List"))
                        {
                            aggregationTest(semi, index, filename,e.filename, e.namespacename);
                            return true;

                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////
    // rules to detect Composition
    public class DetectComposition : ARule
    {
        public override bool test(CSsemi.CSemiExp semi, string filename)
        {
            List<Elem> table = RepositoryForOutput.storageForOutput_;
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            try
            {
                for (int i = 0; i < semi.count; i++)
                {
                    foreach (Elem e in table)
                    {
                        if ((e.name.Equals(semi[i])) && ((e.type.Equals("struct")) || (e.type.Equals("enum"))) && !(e.type.Equals("function")) && (semi[i + 2].Equals(";")))
                        {
                            local.Add("Composition").Add(semi[i]).Add(e.filename).Add(e.namespacename);
                            doActions(local, filename);
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////
    // rules to detect Using
    public class DetectUsing : ARule
    {
        private static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSsemi.CSemiExp semi, string filename)
        {
            if (semi[semi.count - 1] != "{")
                return false;
            int index = semi.FindFirst("(");

            List<Elem> table = RepositoryForOutput.storageForOutput_;
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            try
            {
                if (index > 0 && !isSpecialToken(semi[index - 1]))
                {
                    for (int i = index - 1; i < semi.count; i++)
                    {
                        foreach (Elem e in table)
                        {
                            if ((e.name.Equals(semi[i])) && !(e.type.Equals("function")) && !(e.type.Equals("namespace")))
                            {
                                local.Add("Using").Add(semi[i]).Add(e.filename).Add(e.namespacename);
                                doActions(local, filename);
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }
    }

    public class BuildCodeAnalyzer
    {
        Repository repo = new Repository();

        public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
        {
            repo.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();
            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // this is default so redundant

            // action used for namespaces, classes, and functions, complexity
            PushStack push = new PushStack(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            // capture delegate
            DetectDelegate detectDG = new DetectDelegate();
            detectDG.add(push);
            parser.add(detectDG);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            // parser configured
            return parser;
        }
    }

    ///////////////////////////////////////////////////////////
    // to add rules and actions for Relationship

    public class BuildCodeAnalyzerRelation
    {
        Repository repo = new Repository();

        public BuildCodeAnalyzerRelation(CSsemi.CSemiExp semi)
        {
            repo.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // this is default so redundant

            // action used for inheritance, composition, aggregation, and using
            PushStackRelation push = new PushStackRelation(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            //capture inheritance data
            DetectInheritance detectIN = new DetectInheritance();
            detectIN.add(push);
            parser.add(detectIN);

            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            //capture composition data
            DetectComposition detectComp = new DetectComposition();
            detectComp.add(push);
            parser.add(detectComp);

            //capture Aggregation data
            DetectAggregation detectagg = new DetectAggregation();
            detectagg.add(push);
            parser.add(detectagg);

            //capture Using data
            DetectUsing detectUS = new DetectUsing();
            detectUS.add(push);
            parser.add(detectUS);

            // parser configured
            return parser;
        }
    }
}

