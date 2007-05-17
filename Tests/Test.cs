//

using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Parser.AST;

namespace Tests
{
	
	
	[TestFixture()]
	public class Test
	{
		
		[Test()]
		public void TestCase()
		{
		    string text = File.ReadAllText("../../ClassForParsing.cs");
		    Assert.IsTrue(text != "");
            IParser p = ParserFactory.CreateParser("../../ClassForParsing.cs");
            p.Parse();
            Console.WriteLine("testing the parser");
		    Assert.IsTrue(p.Errors.count == 0);
            List<KeyValuePair<INode, DocAddin.CommentHolder>> nodes = new List<KeyValuePair<INode,  DocAddin.CommentHolder>>();
            Console.WriteLine("collecting the nodes");
            foreach(INode n1 in p.CompilationUnit.Children) {
                DocAddin.Docer.collectNodes(nodes, n1, p);
            }
            Assert.IsTrue(nodes.Count != 0);
            foreach(KeyValuePair<INode, DocAddin.CommentHolder> pair in nodes){
                Console.WriteLine("Node {0}\nOld Comment:{1}\nAuto Comment:{2}\n", pair.Key, pair.Value, DocAddin.Docer.generateComment(pair.Key));
            }
            KeyValuePair<INode, DocAddin.CommentHolder> mp = DocAddin.Docer.findNodeByPos(nodes, text, 174);
            Assert.IsNotNull(mp.Key);
            Console.WriteLine("found Node {0}\nOld Comment:{1}\n", mp.Key, mp.Value);
		}
	}
}
