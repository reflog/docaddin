using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui.Content;
using Gtk;
using GtkSourceView;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Parser.AST;
namespace DocAddin {


    public class Docer {

        public static string generateComment(INode node) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<summary> </summary>"+Environment.NewLine);
            if (node is MethodDeclaration) {

                MethodDeclaration m = node as MethodDeclaration;
                foreach(ParameterDeclarationExpression param in m.Parameters) {
                    sb.AppendFormat("<param name=\"{0}\"></param>"+Environment.NewLine, param.ParameterName);
                }
                sb.AppendFormat("<returns>{0}</returns>"+Environment.NewLine,m.TypeReference.Type);
            }
            return sb.ToString();
        }




        public static Gdk.Pixbuf getNodePic(INode node) {
            if(node is MethodDeclaration)
                return Gdk.Pixbuf.LoadFromResource("Icons.16x16.Method");

            if(node is TypeDeclaration)
                return Gdk.Pixbuf.LoadFromResource("Icons.16x16.Class");

            if(node is NamespaceDeclaration)
                return Gdk.Pixbuf.LoadFromResource("Icons.16x16.NameSpace");

            return null;
        }

        public static string getNodeName(INode node) {

            if(node is MethodDeclaration)
                return ((MethodDeclaration)node).Name;

            if(node is TypeDeclaration)
                return ((TypeDeclaration)node).Name;

            if(node is NamespaceDeclaration)
                return ((NamespaceDeclaration)node).Name;

            return "wtf?";
        }
        static private int linesCharCount(string []lines, int stop){
            int offset = 0;
            if (stop > lines.Length || stop < 0) return -1;
            for(int i=0;i<stop;i++){
                offset += lines[i].Length + Environment.NewLine.Length;
            }
            return offset;
        }
        
        public static KeyValuePair<int, int> findNodeStart(INode node, string [] text ) {
        Console.WriteLine("counting chars till line "+ node.StartLocation.Y);
            int offset = linesCharCount(text, node.StartLocation.Y);
            offset += node.StartLocation.X;
            Console.WriteLine("adding X {0} result {1}", node.StartLocation.X , offset);
            Console.WriteLine("adding length of {0} which is {1} end result {2}", text[node.StartLocation.Y-1], text[node.StartLocation.Y-1].Length,offset+text[node.StartLocation.Y-1].Length-1);
            return new KeyValuePair<int, int>(offset,offset+text[node.StartLocation.Y-1].Length-1);
        }
        
        public static KeyValuePair<int, int> findNodeStart(INode node, string text) {
            string [] lines = text.Split(Environment.NewLine.ToCharArray());
            return findNodeStart(node, lines);
        }

        public static KeyValuePair<INode, CommentHolder> findNodeByPos(List<KeyValuePair<INode, CommentHolder>> nodes, string text, int pos) {
                    
                foreach(KeyValuePair<INode, CommentHolder> p in nodes) {
                    KeyValuePair<int, int> ns = findNodeStart(p.Key, text);
                    Console.WriteLine("checking if line start {0} end {1} is ok for {2}", ns.Key, ns.Value, pos);
                    if (ns.Key != -1 && pos > ns.Key && pos < ns.Value) {
                        return p;
                    }
                }

            return new KeyValuePair<INode, CommentHolder> (null, null);
        }

        public static CommentHolder findComment(INode node, IParser parser) {
            List<string> b = new List<string>();
            int endPos = node.StartLocation.Y;
            int lastPos = endPos;
            int from = -1, to = endPos;

            while (true) {
                Comment c = (Comment) parser.Lexer.SpecialTracker.CurrentSpecials.Find(
                                delegate(ISpecial it) {
                                    return (it is ICSharpCode.NRefactory.Parser.Comment) && ( it.EndPosition.Y == lastPos ) && ( ((ICSharpCode.NRefactory.Parser.Comment)it).CommentType == ICSharpCode.NRefactory.Parser.CommentType.Documentation);
                                }

                            );

                if (c != null) {
                    b.Add(c.CommentText);
                    from =c.StartPosition.Y;
                    lastPos --;

                } else {
                    break;
                }
            }

            b.Reverse();
            string res = String.Empty;

            foreach(string s in b) {
                res += s + Environment.NewLine;
            }

            return new CommentHolder(res, from, to);
        }


        public static void collectNodes(List<KeyValuePair<INode, CommentHolder>> nodes, INode n, IParser parser) {
            if(
                (n is MethodDeclaration)||
                (n is TypeDeclaration)||
                (n is NamespaceDeclaration)) {
                CommentHolder c = findComment(n, parser);
                nodes.Add(new KeyValuePair<INode, CommentHolder>(n, c));
            }

            foreach(INode n1 in n.Children) {
                collectNodes(nodes, n1, parser);
            }
        }

        private static string funcOffset(string line){
        int o=0;
        string r=String.Empty;
        for(o=0;o<line.Length;o++)
            if(Char.IsWhiteSpace(line[o])) r += line[o];
            else 
                break;
          return r;
        }


        public static string insertComment(string[] lines, INode node, CommentHolder comment, string offset) {
            if(comment.text != String.Empty) {
                    string p1 = String.Join(Environment.NewLine, lines, 0, node.StartLocation.Y-1);
                    string p2 = String.Join(Environment.NewLine, lines, node.StartLocation.Y-1, lines.Length-node.StartLocation.Y);
                    return p1 + comment.prepare(offset) + Environment.NewLine + p2;
                    }
            return String.Join(Environment.NewLine, lines);
        }
        
        public static string insertComment(string orig, INode node, CommentHolder comment,string offset) {
            string [] lines = orig.Split(Environment.NewLine.ToCharArray());
            return insertComment(lines, node, comment, offset);        
        }
        
        public static string replaceComment(string orig, INode node, CommentHolder comment) {
            if(comment.text != String.Empty) {

            string [] lines = orig.Split(Environment.NewLine.ToCharArray());
            string offset = funcOffset(lines[node.StartLocation.Y-1]);
            
                if (comment.lineStart != -1) {
                    string p1 = String.Join(Environment.NewLine, lines, 0, comment.lineStart-2);
                    string p2 = String.Join(Environment.NewLine, lines, comment.lineStop-1, lines.Length-comment.lineStop-1);
                    return p1 + comment.prepare(offset) + Environment.NewLine + p2;
                } else  {                
                    return insertComment(lines, node, comment, offset);
                }
            }
            return orig;
        }


    }
}
