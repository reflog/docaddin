//

using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Parser.AST;

namespace Tests {


    [TestFixture()]
    public class Test {
        string text;
        IParser p;
        [SetUp()]
        public void Setup() {
            text = File.ReadAllText("../../ClassForParsing.cs");
            Assert.IsTrue(text != "");
            p = ParserFactory.CreateParser("../../ClassForParsing.cs");
            p.Parse();
        }


        [Test()]
        public void TestParser() {
            Console.WriteLine("testing the parser");
            Assert.IsTrue(p.Errors.count == 0);
        }
        List<KeyValuePair<INode, DocAddin.CommentHolder>> nodes;

        private void collectNodes() {
            nodes = new List<KeyValuePair<INode,  DocAddin.CommentHolder>>();
            Console.WriteLine("collecting the nodes");
            foreach(INode n1 in p.CompilationUnit.Children) {
                DocAddin.Docer.collectNodes(nodes, n1, p);
            }
        }

        [Test()]
        public void TestCollection() {
            collectNodes();
            Assert.IsTrue(nodes.Count != 3);
            foreach(KeyValuePair<INode, DocAddin.CommentHolder> pair in nodes) {
                Console.WriteLine("Node {0}\nOld Comment:{1}\nAuto Comment:{2}\n", pair.Key, pair.Value, DocAddin.Docer.generateComment(pair.Key));
            }
            Assert.AreEqual(nodes[2].Value.text, " <summary> test </summary>\n");
        }

        [Test()]
        public void TestAuto() {
            collectNodes();
            KeyValuePair<INode, DocAddin.CommentHolder> mp = DocAddin.Docer.findNodeByPos(nodes, text, 220);
            Assert.AreEqual(mp.Value.lineStart, -1);
            Assert.IsNotNull(mp.Key);
            Console.WriteLine("found Node {0}\nOld Comment:{1}\nAuto Comment:{2}\n", mp.Key, mp.Value, DocAddin.Docer.generateComment(mp.Key));
        }
        
        [Test()]
        public void TestReplace() {
            collectNodes();
            nodes[2].Value.text = " kuku! ";
            Console.WriteLine("orig text: \n{0}\nnew text:\n{1}\n",text, DocAddin.Docer.replaceComment(text, nodes[2].Key, nodes[2].Value));
        }

        [Test()]
        public void TestInsert() {
            collectNodes();
            nodes[3].Value.text = " kuku22!\nkukun ";
            Console.WriteLine("node {2}\n\n orig text: \n{0}\nnew text:\n{1}\n",text, DocAddin.Docer.replaceComment(text, nodes[2].Key, nodes[3].Value),nodes[3].Key);
        }

    }
}
