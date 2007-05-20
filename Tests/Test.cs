//
// Copyright (c) Eli Yukelzon - reflog@gmail.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
