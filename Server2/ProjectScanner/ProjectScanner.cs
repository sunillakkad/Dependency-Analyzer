///////////////////////////////////////////////////////////////////////
// ProjectScanner.cs - Scan Projects available in local Directoey    //
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
 *   ProjectScanner - it will scan the project available in the local 
 *   directory 
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

namespace DependencyAnalyzer
{
    /////////////////////////////////////////////////////////
    // Perform Project Scanning

    public class ProjectScanner
    {
        public List<string> getProjectList(string path)
        {
            string[] dir=Directory.GetDirectories(Path.GetFullPath(path));
            List<string> dir1 = new List<string>();
            foreach (string d in dir)
                dir1.Add(d);
            return dir1;
        }
#if(TEST_PROJECTSCANNER)
        static void Main(string[] args)
        {
            ProjectScanner ps=new ProjectScanner();
            List<string> dire = ps.getProjectList("../../../TESTFOLDER");
            foreach (string s in dire)
                Console.Write("\n{0}",s);
        }
#endif
    }
}
