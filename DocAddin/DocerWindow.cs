
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


public partial class DocerWindow : Gtk.Dialog {
        private static Gtk.TargetEntry [] target_table = new Gtk.TargetEntry [] {
                    DndUtils.TargetPlain,
                };
        Dictionary<string, string> tags = new Dictionary<string, string>();

        void FillTags() {
            tags["c"]="a way to indicate that text within a description should be marked as code";
            tags["code"]="a way to indicate multiple lines as code";
            tags["example"]="lets you specify an example of how to use a method or other library member";
            tags["exception"]="lets you document an exception class";
            tags["include"]="lets you refer to comments in another file, using XPath syntax, that describe the types and members in your source code.";
            tags["list"]="Used to insert a list into the documentation file";
            tags["para"]="Used to insert a paragraph into the documentation file";
            tags["param"]="Describes a parameter";
            tags["paramref"]="gives you a way to indicate that a word is a parameter";
            tags["permission"]="lets you document access permissions";
            tags["remarks"]="where you can specify overview information about the type";
            tags["returns"]="describe the return value of a method";
            tags["see"]="lets you specify a link";
            tags["seealso"]="lets you specify the text that you might want to appear in a See Also section";
            tags["summary"]="used for a general description";
            tags["value"]="lets you describe a property";
        }

        private string file;


        private System.Drawing.Point getStartPosition(INode node) {

            if(node is MethodDeclaration)
                return ((MethodDeclaration)node).StartLocation;

            if(node is TypeDeclaration)
                return ((TypeDeclaration)node).StartLocation;

            if(node is NamespaceDeclaration)
                return ((NamespaceDeclaration)node).StartLocation;

            return System.Drawing.Point.Empty;
        }

        private Gdk.Pixbuf getNodePic(INode node) {
            if(node is MethodDeclaration)
                return Gdk.Pixbuf.LoadFromResource("Icons.16x16.Method");

            if(node is TypeDeclaration)
                return Gdk.Pixbuf.LoadFromResource("Icons.16x16.Class");

            if(node is NamespaceDeclaration)
                return Gdk.Pixbuf.LoadFromResource("Icons.16x16.NameSpace");

            return null;

            ;

        }

        private string getNodeName(INode node) {

            if(node is MethodDeclaration)
                return ((MethodDeclaration)node).Name;

            if(node is TypeDeclaration)
                return ((TypeDeclaration)node).Name;

            if(node is NamespaceDeclaration)
                return ((NamespaceDeclaration)node).Name;

            return "wtf?";
        }


        private CommentHolder findComment(INode node, IParser parser) {
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


        private void collectNodes(INode n, IParser parser) {
            if(
                (n is MethodDeclaration)||
                (n is TypeDeclaration)||
                (n is NamespaceDeclaration)) {
                CommentHolder c = findComment(n, parser);
                nodes.Add(new KeyValuePair<INode, CommentHolder>(n, c));
            }

            foreach(INode n1 in n.Children) {
                collectNodes(n1, parser);
            }
        }

        /// test1
        /// test2
        /// test3
        public bool SetFile(string f) {
            file = f;

            IParser p = ParserFactory.CreateParser(file);
            p.Parse();

            if(p.Errors.count > 0) return false;

            foreach(INode n1 in p.CompilationUnit.Children) {
                collectNodes(n1, p);
            }

            BuildUI();
            return true;
        }

        ListStore store = new ListStore(typeof(KeyValuePair<INode, CommentHolder>));
        ListStore tagstore = new ListStore(typeof(KeyValuePair<string, string>));
        List<KeyValuePair<INode, CommentHolder>> nodes = new List<KeyValuePair<INode, CommentHolder>>();

        private void BuildUI() {
            KeyValuePair<INode, CommentHolder> [] ar = nodes.ToArray();
            foreach(KeyValuePair<INode, CommentHolder> zzz in ar) {
                store.AppendValues(new KeyValuePair<INode, CommentHolder>[]{zzz});
            }

            funcview.Model = store;

            tagview.Model = tagstore;


            Gtk.TreeViewColumn icoColumn = new Gtk.TreeViewColumn ();
            icoColumn.Title = "Icon";

            Gtk.CellRendererPixbuf icoCell = new Gtk.CellRendererPixbuf();
            icoColumn.PackStart (icoCell, true);
            funcview.AppendColumn (icoColumn);
            icoColumn.SetCellDataFunc (icoCell, new Gtk.TreeCellDataFunc (RenderIcon));

            Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn ();
            nameColumn.Title = "Name";
            nameColumn.Expand = false;
            nameColumn.MaxWidth = 200;

            Gtk.CellRendererText nameCell = new Gtk.CellRendererText ();
            nameCell.Height = 20;
            nameColumn.PackStart (nameCell, true);
            funcview.AppendColumn (nameColumn);
            nameColumn.SetCellDataFunc (nameCell, new Gtk.TreeCellDataFunc (RenderName));


            SourceLanguagesManager manager = new SourceLanguagesManager ();
            SourceLanguage lang = manager.GetLanguageFromMimeType ("text/xml");

            SourceBuffer buffer = new SourceBuffer (lang);
            buffer.Highlight = true;
            sourceview = new SourceView(buffer);
            buffer.Changed  += new EventHandler(BufferChanged);
            scrolledwindow2.Add(sourceview);
            this.Child.ShowAll();
            FillTags();

            foreach(string k in tags.Keys) {
                tagstore.AppendValues(new KeyValuePair<string,string>[]{new KeyValuePair<string,string>(k,tags[k])});
            }

            Gtk.TreeViewColumn tagColumn = new Gtk.TreeViewColumn ();
            tagColumn.Title = "Name";
            tagColumn.Expand = false;
            tagColumn.MaxWidth = 200;

            Gtk.CellRendererText tagCell = new Gtk.CellRendererText ();
            tagCell.Height = 20;
            tagColumn.PackStart (tagCell, true);
            tagview.AppendColumn (tagColumn);
            tagColumn.SetCellDataFunc (tagCell, new Gtk.TreeCellDataFunc (RenderTagName));

        }

        private void RenderIcon (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {

            object o = model.GetValue (iter, 0);

            if(o != null) {
                KeyValuePair<INode, CommentHolder> item = (KeyValuePair<INode, CommentHolder>) o;
                (cell as Gtk.CellRendererPixbuf).Pixbuf = getNodePic(item.Key);
            }
        }

        private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {

            object o = model.GetValue (iter, 0);

            if(o != null) {
                KeyValuePair<INode, CommentHolder> item = (KeyValuePair<INode, CommentHolder>) o;
                (cell as Gtk.CellRendererText).Text = getNodeName(item.Key);
            }
        }


        private void RenderTagName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {

            object o = model.GetValue (iter, 0);

            if(o != null) {
                KeyValuePair<string,string> item = (KeyValuePair<string, string>) o;
                (cell as Gtk.CellRendererText).Text = item.Key;
            }
        }

        SourceView sourceview;
        public DocerWindow() {
            this.Build();
        }

        object itemByPath(ListStore s, TreePath path) {
            TreeIter iter = TreeIter.Zero;
            s.GetIter(out iter, path);

            if (!s.IterIsValid(iter)) throw new Exception("kuku");

            GLib.Value val = GLib.Value.Empty;

            s.GetValue(iter, 0, ref val);

            return val.Val;
        }

        string replaceLines(string orig, INode node, CommentHolder comment) {
            int i=0,line=0,fromPos=-1,endPos=-1;
            int offset = 0, funcPosEnd = -1;

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

            if(funcPosEnd != -1) {
                int startCount = -1;

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
            }

            return orig;
        }

        public void SaveComments() {
            StreamReader sr = File.OpenText (file);
            string Text = sr.ReadToEnd ();

            sr.Close ();

            foreach(KeyValuePair<INode, CommentHolder> p in nodes) {
                if(p.Value.lineStart != -1) {
                    Text = replaceLines(Text, p.Key, p.Value);
                }
            }

            File.WriteAllText(file+".tmp", Text);
        }

        void BufferChanged(object sender, EventArgs args) {
            if(funcview.Selection.GetSelectedRows().Length > 0) {
                try {
                    KeyValuePair<INode, CommentHolder> item = (KeyValuePair<INode, CommentHolder>)itemByPath(store, funcview.Selection.GetSelectedRows()[0] );
                    item.Value.text = sourceview.Buffer.Text;

                } catch{}
            }
    }

    /*
    test
    */
    protected virtual void OnTreeview1RowActivated(object o, Gtk.RowActivatedArgs args) {
            try {
                KeyValuePair<INode, CommentHolder> item = (KeyValuePair<INode, CommentHolder>)itemByPath(store, args.Path);
                sourceview.Buffer.Text = item.Value.text;

            } catch{}
        }

    private string generateComment(INode node) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<summary> </summary>\n");
            if (node is MethodDeclaration) {

                MethodDeclaration m = node as MethodDeclaration;
                foreach(ParameterDeclarationExpression param in m.Parameters) {
                    sb.AppendFormat("<param name=\"{0}\"></param>", param.ParameterName);
                }
                sb.AppendFormat("<returns>{0}</returns>\n",m.TypeReference.Type);
            }
            return sb.ToString();
        }



        protected virtual void OnTagviewRowActivated(object o, Gtk.RowActivatedArgs args) {
            try {
                KeyValuePair<string, string> item = (KeyValuePair<string, string>)itemByPath(tagstore, args.Path);
                sourceview.Buffer.InsertAtCursor("<"+item.Key+">"+"</"+item.Key+">");
            } catch{}

        }

protected virtual void OnTagviewDragBegin(object o, Gtk.DragBeginArgs args) {}

 [GLib.ConnectBefore]
        protected virtual void OnTagviewButtonPressEvent(object o, Gtk.ButtonPressEventArgs args) {
            if(tagview.Selection.GetSelectedRows().Length>0) {
                try {
                    KeyValuePair<string, string> item = (KeyValuePair<string, string>)itemByPath(tagstore, tagview.Selection.GetSelectedRows()[0]);
                    tagdescr.Buffer.Text = item.Value;

                } catch{}
            }
    }

        protected virtual void OnBtnAutoClicked(object sender, System.EventArgs e)
        {
        Console.WriteLine("auto act. sel:"+funcview.Selection.GetSelectedRows().Length);
            if(funcview.Selection.GetSelectedRows().Length>0) {
                try {
                    KeyValuePair<INode, CommentHolder> item = (KeyValuePair<INode, CommentHolder>)itemByPath(store, funcview.Selection.GetSelectedRows()[0]);
                    if (item.Value.text != "") {
                        MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo , "This will erase your old comment. Are you sure?", null);
                        if ((int)m.Run() == (int)ResponseType.No) return;
                    }

                    sourceview.Buffer.Text = generateComment(item.Key);

                } catch{}
            }
        }

        protected virtual void OnBtnValidateClicked(object sender, System.EventArgs e)
        {
            // TODO: check this
            MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "Validation passed!", null);
            m.Run();
        }

}

}
