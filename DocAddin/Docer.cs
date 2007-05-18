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
namespace DocAddin
{
	
	
	public class Docer
	{
	
	    public static string generateComment(INode node) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<summary> </summary>\n");
            if (node is MethodDeclaration) {

                MethodDeclaration m = node as MethodDeclaration;
                foreach(ParameterDeclarationExpression param in m.Parameters) {
                    sb.AppendFormat("<param name=\"{0}\"></param>\n", param.ParameterName);
                }
                sb.AppendFormat("<returns>{0}</returns>\n",m.TypeReference.Type);
            }
            return sb.ToString();
        }

        public static System.Drawing.Point getStartPosition(INode node) {

            if(node is MethodDeclaration)
                return ((MethodDeclaration)node).StartLocation;

            if(node is TypeDeclaration)
                return ((TypeDeclaration)node).StartLocation;

            if(node is NamespaceDeclaration)
                return ((NamespaceDeclaration)node).StartLocation;

            return System.Drawing.Point.Empty;
        }


        public static System.Drawing.Point getEndPosition(INode node) {
            if(node is MethodDeclaration)
                return ((MethodDeclaration)node).EndLocation;

            if(node is TypeDeclaration)
                return ((TypeDeclaration)node).EndLocation;

            if(node is NamespaceDeclaration)
                return ((NamespaceDeclaration)node).EndLocation;

            return System.Drawing.Point.Empty;
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
        public static KeyValuePair<int, int> findNodeStart(INode node, string text){
        int line = 0, s=-1, e=-1;
            for(int i=0;i<text.Length ;i++) {            
                if(text[i] == '\n')
                    line++;
               if (line == getStartPosition(node).Y){
               Console.WriteLine("found item at line {0} type {1}", line , node.GetType());
                   for(int j=i-1;j>0;j--){
                       if(text[j] == '\n'){
                       Console.WriteLine("found item start at " +j);
                           s = j+1;
                           e = i;                       
                           break;
                           }
                   }
               }
               if(s!=-1 && e!= -1) break;
            }
            return new KeyValuePair<int, int>(s,e);
        }

        public static KeyValuePair<INode, CommentHolder> findNodeByPos(List<KeyValuePair<INode, CommentHolder>> nodes, string text, int pos){
            int i=0,line=0,prevLineStart=0;
            for(i=0;i<pos;i++) {            
                if(text[i] == '\n'){
                    line++;
                    prevLineStart = i+1;
                    Console.WriteLine("i am on line " + line);
                }
                foreach(KeyValuePair<INode, CommentHolder> p in nodes){
                    KeyValuePair<int, int> ns = findNodeStart(p.Key, text);
                    if (ns.Key != -1 && pos > ns.Key && pos < ns.Value){
                    Console.WriteLine("foudn match " + p);
                            return p;                            
                            }
                }
             }
                
            return new KeyValuePair<INode, CommentHolder> (null, null);            
        }

        public static CommentHolder findComment(INode node, IParser parser) {
            List<string> b = new List<string>();
            int endPos = getStartPosition(node).Y;
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
            string res = "";

            foreach(string s in b) {
                res += s + "\n";
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

        public static string replaceLines(string orig, INode node, CommentHolder comment) {
            int i=0,line=0,fromPos=-1,endPos=-1;
            int offset = 0, funcPosEnd = -1;
            if(comment.text != ""){
            for(i=0;i<orig.Length;i++) {
                if(orig[i] == '\n') line++;

                if (line == comment.lineStart-2 && fromPos == -1) {
                    fromPos = i;
                }

                if (line == comment.lineStop-1 && endPos == -1) {
                    endPos = i;
                }

                if (line == getStartPosition(node).Y) {
                    funcPosEnd = i;
                }
            }

            int startCount = -1;
            if(funcPosEnd != -1) {

                for(int j=funcPosEnd-1;j>0;j--) {
                    if (orig[j] == '\n') {
                        startCount = j;
                        break;
                    }
                }

                if (startCount != -1) {
                    int k=startCount+1,z=0;

                    for(;k<funcPosEnd;k++,z++) {
                        if(orig[k] != ' ' && orig[k] != '\t')
                            break;
                    }

                    offset = z;
                }

            }

            if (fromPos != -1 && endPos != -1) {
                return orig.Substring(0, fromPos) + comment.prepare(offset) + orig.Substring(endPos);
            }else if (startCount != -1){
                return orig.Substring(0, startCount) + comment.prepare(offset) + orig.Substring(startCount);
            }
            }
            return orig;
        }

		
	}
}
