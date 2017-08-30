using AlwaysAligned;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElasticTabstopsConverterTest
{
    
    
    /// <summary>
    ///This is a test class for ElasticTabstopsConverterTest and is intended
    ///to contain all ElasticTabstopsConverterTest Unit Tests
    ///</summary>
	[TestClass()]
	public class ElasticTabstopsConverterTest
	{
		private const string ET_TEXT_1 = @"
abc

	def
	ghi

		jkl
		mno

			pqr
			stu

vwx
";

		private const string SPACE_TEXT_1 = @"
abc

        def
        ghi

                jkl
                mno

                        pqr
                        stu

vwx
";

		private const string ET_TEXT_2 = @"aaa

	abc
	def
		ghi
		jkl

	mno
	pqr

		stu
		vwx
";

		private const string SPACE_TEXT_2 = @"aaa

        abc
        def
                ghi
                jkl

        mno
        pqr

                stu
                vwx
";

		private const string ET_TEXT_3 = @"
	abc
	def

	ghi
x	jkl

	mno
xxxxxxxxx	pqr
";

		private const string SPACE_TEXT_3 = @"
        abc
        def

        ghi
x       jkl

                mno
xxxxxxxxx       pqr
";

		private List<Line> SPACE_TEXT_3_POSITIONS_CONTENTS = new List<Line>
		{
			new Line(),
			new Line(new SortedDictionary<int, Cell> {{8, new Cell(SPACE_TEXT_3.IndexOf("abc"), "abc".Length)}}),
			new Line(new SortedDictionary<int, Cell> {{8, new Cell(SPACE_TEXT_3.IndexOf("def"), "def".Length)}}),
			new Line(),
			new Line(new SortedDictionary<int, Cell> {{8, new Cell(SPACE_TEXT_3.IndexOf("ghi"), "ghi".Length)}}),
			new Line(new SortedDictionary<int, Cell> {{0, new Cell(SPACE_TEXT_3.IndexOf("x"), "x".Length)}, {8, new Cell(SPACE_TEXT_3.IndexOf("jkl"), "jkl".Length)}}),
			new Line(),
			new Line(new SortedDictionary<int, Cell> {{16, new Cell(SPACE_TEXT_3.IndexOf("mno"), "mno".Length)}}),
			new Line(new SortedDictionary<int, Cell> {{0, new Cell(SPACE_TEXT_3.IndexOf("xxxxxxxxx"), "xxxxxxxxx".Length)}, {16, new Cell(SPACE_TEXT_3.IndexOf("pqr"), "pqr".Length)}}),
			new Line(),
		};

		private const string ET_TEXT_4A = @"
	abc
	def	ghi
	jkl	mno
	pqr
";

		private const string SPACE_TEXT_4A = @"
        abc
        def     ghi
        jkl     mno
        pqr
";

		private const string ET_TEXT_4B = @"
	abc
	def	ghi
	jkl	mno
	pqr";

		private const string SPACE_TEXT_4B = @"
        abc
        def     ghi
        jkl     mno
        pqr";

		private const string ET_TEXT_5 = @"
#			commented out
		return something";

		private const string SPACE_TEXT_5_IN = @"
#                commented out
                return something";

		private const string SPACE_TEXT_5_OUT = @"
#                       commented out
                return something";

		private const string ET_TEXT_6 = @"// eeeeeeee.cpp : Defines the entry point for the console application.
//

#include ""stdafx.h""


int _tmain(int argc, _TCHAR* argv[])
{
	return 0;
	kkkkkkkkkkkkkk	kkkkkkkk
	llllllllllllllllllllll	llllllllllll

	aa	bb	cc
	a	b	c
}

";

		private const string SPACE_TEXT_6 = @"// eeeeeeee.cpp : Defines the entry point for the console application.
//

#include ""stdafx.h""


int _tmain(int argc, _TCHAR* argv[])
{
                return 0;
                kkkkkkkkkkkkkk                  kkkkkkkk
                llllllllllllllllllllll          llllllllllll

                aa              bb              cc
                a               b               c
}

";

		private const string ET_TEXT_7 = @"	Hallo
	Pupallo
	Gugu	gaga
	hhghga	hghghhghg
	adsdasdasdasda	ghghghgghghg
";

		private const string SPACE_TEXT_7_IN = @"        Hallo             
        Pupallo
        Gugu    gaga
        hhghga  hghghhghg
        adsdasdasdasda  ghghghgghghg                
";

		private const string SPACE_TEXT_7_OUT = @"        Hallo
        Pupallo
        Gugu            gaga
        hhghga          hghghhghg
        adsdasdasdasda  ghghghgghghg
";

		private const string ET_TEXT_8 = @"	push
		(
		@{$self->{struct}},
			{
			source	=> $source,
			filename	=> $filename,
			pathname	=> $pathname,
			lang	=> $lang,
			level	=> $level,
			back	=> $back,
			url	=> $url,
			modified	=> $modified,
			id	=> Digest::MD5::md5_hex($url),
			file	=> $file,
			}
		);
	}
";

		private const string TAB_TEXT_8_IN = @"	push
		(
		@{$self->{struct}},
			{
			source		=> $source,
			filename	=> $filename,
			pathname 	=> $pathname,
			lang		=> $lang,
			level		=> $level,
			back		=> $back,
			url			=> $url,
			modified	=> $modified,
			id			=> Digest::MD5::md5_hex($url),
			file		=> $file,
			}
		);
	}
";

		private const string SPACE_TEXT_8_OUT = @"    push
        (
        @{$self->{struct}},
            {
            source      => $source,
            filename    => $filename,
            pathname    => $pathname,
            lang        => $lang,
            level       => $level,
            back        => $back,
            url         => $url,
            modified    => $modified,
            id          => Digest::MD5::md5_hex($url),
            file        => $file,
            }
        );
    }
";

		private const string SPACE_TEXT_9 = @"
/* Hopefully this Java program should demonstrate how elastic tabstops work.               a*/
/* Try inserting and deleting different parts of the text and watch as the tabstops move.  b*/
/* If you like this, please ask the writers of your text editor to implement it.           c*/
";

		private List<Line> SPACE_TEXT_9_POSITIONS_CONTENTS = new List<Line>
		{
			new Line(),
			new Line(new SortedDictionary<int, Cell> {{0, new Cell(SPACE_TEXT_9.IndexOf("/* Ho"), "/* Hopefully this Java program should demonstrate how elastic tabstops work.".Length)}, {91, new Cell(SPACE_TEXT_9.IndexOf("a*/"), "a*/".Length)}}),
			new Line(new SortedDictionary<int, Cell> {{0, new Cell(SPACE_TEXT_9.IndexOf("/* Tr"), "/* Try inserting and deleting different parts of the text and watch as the tabstops move.".Length)}, {91, new Cell(SPACE_TEXT_9.IndexOf("b*/"), "b*/".Length)}}),
			new Line(new SortedDictionary<int, Cell> {{0, new Cell(SPACE_TEXT_9.IndexOf("/* If"), "/* If you like this, please ask the writers of your text editor to implement it.".Length)}, {91, new Cell(SPACE_TEXT_9.IndexOf("c*/"), "c*/".Length)}}),
			new Line(),
		};

		private const string ET_FORMATTED_CODE = @"
/* Hopefully this Java program should demonstrate how elastic tabstops work.	*/
/* Try inserting and deleting different parts of the text and watch as the tabstops move.	*/
/* If you like this, please ask the writers of your text editor to implement it.	*/

#include <stdio.h>

struct ipc_perm
{
	key_t	key;
	ushort	uid;	/* owner euid and egid	*/
	ushort	gid;	/* group id	*/
	ushort	cuid;	/* creator euid and egid	*/
	cell-missing		/* for test purposes	*/
	ushort	mode;	/* access modes	*/
	ushort	seq;	/* sequence number	*/
};

int someDemoCode(	int fred,
	int wilma)
{
	x();	/* try making	*/
	showTextGreeting();	/* this comment	*/
	doSomethingComplicated();	/* a bit longer	*/
	for (i = start; i < end; ++i)
	{
		if (isPrime(i))
		{
			++numPrimes;
		}
	}
	return numPrimes;
}

---- and now for something completely different: a table ----

Title	Author	Publisher	Year
Generation X	Douglas Coupland	Abacus	1995
Informagic	Jean-Pierre Petit	John Murray Ltd	1982
The Cyberiad	Stanislaw Lem	Harcourt Publishers Ltd	1985
The Selfish Gene	Richard Dawkins	Oxford University Press	2006
";

		private const string SPACE_FORMATTED_CODE_MIN_2 = @"
/* Hopefully this Java program should demonstrate how elastic tabstops work.               */
/* Try inserting and deleting different parts of the text and watch as the tabstops move.  */
/* If you like this, please ask the writers of your text editor to implement it.           */

#include <stdio.h>

struct ipc_perm
{
    key_t         key;
    ushort        uid;   /* owner euid and egid    */
    ushort        gid;   /* group id               */
    ushort        cuid;  /* creator euid and egid  */
    cell-missing         /* for test purposes      */
    ushort        mode;  /* access modes           */
    ushort        seq;   /* sequence number        */
};

int someDemoCode(  int fred,
                   int wilma)
{
    x();                       /* try making    */
    showTextGreeting();        /* this comment  */
    doSomethingComplicated();  /* a bit longer  */
    for (i = start; i < end; ++i)
    {
        if (isPrime(i))
        {
            ++numPrimes;
        }
    }
    return numPrimes;
}

---- and now for something completely different: a table ----

Title             Author             Publisher                Year
Generation X      Douglas Coupland   Abacus                   1995
Informagic        Jean-Pierre Petit  John Murray Ltd          1982
The Cyberiad      Stanislaw Lem      Harcourt Publishers Ltd  1985
The Selfish Gene  Richard Dawkins    Oxford University Press  2006
";

		private const string SPACE_FORMATTED_CODE_MOD_8 = @"
/* Hopefully this Java program should demonstrate how elastic tabstops work.                    */
/* Try inserting and deleting different parts of the text and watch as the tabstops move.       */
/* If you like this, please ask the writers of your text editor to implement it.                */

#include <stdio.h>

struct ipc_perm
{
        key_t           key;
        ushort          uid;    /* owner euid and egid          */
        ushort          gid;    /* group id                     */
        ushort          cuid;   /* creator euid and egid        */
        cell-missing            /* for test purposes            */
        ushort          mode;   /* access modes                 */
        ushort          seq;    /* sequence number              */
};

int someDemoCode(       int fred,
                        int wilma)
{
        x();                            /* try making           */
        showTextGreeting();             /* this comment         */
        doSomethingComplicated();       /* a bit longer         */
        for (i = start; i < end; ++i)
        {
                if (isPrime(i))
                {
                        ++numPrimes;
                }
        }
        return numPrimes;
}

---- and now for something completely different: a table ----

Title                   Author                  Publisher                       Year
Generation X            Douglas Coupland        Abacus                          1995
Informagic              Jean-Pierre Petit       John Murray Ltd                 1982
The Cyberiad            Stanislaw Lem           Harcourt Publishers Ltd         1985
The Selfish Gene        Richard Dawkins         Oxford University Press         2006
";

		private const string NO_STRING = "NO_STRING"; // ugh

		// this changed from dicts like: {'et_text': ET_TEXT_8, 'space_text': TAB_TEXT_8_IN, 'space_text_out': SPACE_TEXT_8_OUT, 'tab_size': 4}
		// to tuples like: (ET_TEXT_8, TAB_TEXT_8_IN, SPACE_TEXT_8_OUT, 4)
		private List<Tuple<string, string, string, int>> TEST_STRINGS_LIST = new List<Tuple<string, string, string, int>>
		{
			Tuple.Create(ET_FORMATTED_CODE, SPACE_FORMATTED_CODE_MOD_8, NO_STRING, 8),
			Tuple.Create(ET_TEXT_1, SPACE_TEXT_1, NO_STRING, 8),
			Tuple.Create(ET_TEXT_2, SPACE_TEXT_2, NO_STRING, 8),
			Tuple.Create(ET_TEXT_3, SPACE_TEXT_3, NO_STRING, 8),
			Tuple.Create(ET_TEXT_4A, SPACE_TEXT_4A, NO_STRING, 8),
			Tuple.Create(ET_TEXT_4B, SPACE_TEXT_4B, NO_STRING, 8),
			Tuple.Create(ET_TEXT_5, SPACE_TEXT_5_IN, SPACE_TEXT_5_OUT, 8),
			Tuple.Create(ET_TEXT_6, SPACE_TEXT_6, NO_STRING, 16),
			Tuple.Create(ET_TEXT_7, SPACE_TEXT_7_IN, SPACE_TEXT_7_OUT, 8),
			Tuple.Create(ET_TEXT_8, TAB_TEXT_8_IN, SPACE_TEXT_8_OUT, 4),
		};

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///A test for ToElasticTabstops
		///</summary>
		[TestMethod()]
		public void ToElasticTabstopsTest()
		{
			foreach (Tuple<string, string, string, int> testStrings in TEST_STRINGS_LIST)
			{
				string string1 = testStrings.Item1;
				string string2 = ElasticTabstopsConverter.ToElasticTabstops(testStrings.Item2, testStrings.Item4);

				Assert.AreEqual(string1, string2);
			}
		}

		/// <summary>
		///A test for ToSpaces
		///</summary>
		[TestMethod()]
		public void ToSpacesTest()
		{
			foreach (Tuple<string, string, string, int> testStrings in TEST_STRINGS_LIST)
			{
				string string1;
				if (testStrings.Item3 == NO_STRING)
				{
					string1 = testStrings.Item2;
				}
				else
				{
					string1 = testStrings.Item3;
				}
				string string2 = ElasticTabstopsConverter.ToSpaces(testStrings.Item1, testStrings.Item4);
				Assert.AreEqual(string1, string2);
			}
		}

		/// <summary>
		///A test for CellExists
		///</summary>
		[TestMethod()]
		public void CellExistsTest()
		{
			List<List<int>> listOfLists = new List<List<int>>
			{
				new List<int> {},
				new List<int> {1},
				new List<int> {2, 3},
				new List<int> {4, 5, 6}
			};
			Assert.IsFalse(ElasticTabstopsConverter.CellExists(listOfLists, 0, 0));
			Assert.IsFalse(ElasticTabstopsConverter.CellExists(listOfLists, 1, 1));
			Assert.IsFalse(ElasticTabstopsConverter.CellExists(listOfLists, 2, 2));
			Assert.IsFalse(ElasticTabstopsConverter.CellExists(listOfLists, 3, 3));
			Assert.IsFalse(ElasticTabstopsConverter.CellExists(listOfLists, 4, 0));
			Assert.IsTrue(ElasticTabstopsConverter.CellExists(listOfLists, 1, 0));
			Assert.IsTrue(ElasticTabstopsConverter.CellExists(listOfLists, 2, 1));
			Assert.IsTrue(ElasticTabstopsConverter.CellExists(listOfLists, 3, 2));
		}

		string PositionsToString(List<Line> lines)
		{
			return String.Join("\n", from line in lines select String.Join("\t", from cell in line.Cells select cell.Value.ToString() + "|" + cell.Key));
		}

		/// <summary>
		///A test for GetPositionsContents
		///</summary>
		[TestMethod()]
		public void GetPositionsContentsTest()
		{
			string string1 = PositionsToString(SPACE_TEXT_3_POSITIONS_CONTENTS);
			string string2 = PositionsToString(ElasticTabstopsConverter.GetLines(SPACE_TEXT_3, 8));
			Assert.AreEqual(string1, string2);

			string1 = PositionsToString(SPACE_TEXT_9_POSITIONS_CONTENTS);
			string2 = PositionsToString(ElasticTabstopsConverter.GetLines(SPACE_TEXT_9, 8));
			Assert.AreEqual(string1, string2);
		}
	}
}
