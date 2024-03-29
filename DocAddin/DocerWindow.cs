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



        /// test1
        /// test2
        /// test3
        public bool SetFile(string f) {
            file = f;

            IParser p = ParserFactory.CreateParser(file);
            p.Parse();

            if(p.Errors.count > 0) return false;

            foreach(INode n1 in p.CompilationUnit.Children) {
                Docer.collectNodes(nodes, n1, p);
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
                (cell as Gtk.CellRendererPixbuf).Pixbuf = Docer.getNodePic(item.Key);
            }
        }

        private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {

            object o = model.GetValue (iter, 0);

            if(o != null) {
                KeyValuePair<INode, CommentHolder> item = (KeyValuePair<INode, CommentHolder>) o;
                (cell as Gtk.CellRendererText).Text = Docer.getNodeName(item.Key);
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


        public string SaveComments() {
            StreamReader sr = File.OpenText (file);
            string Text = sr.ReadToEnd ();
            sr.Close ();
            File.WriteAllText(file+".docbak", Text);
            foreach(KeyValuePair<INode, CommentHolder> p in nodes) {
                    Text = Docer.replaceComment(Text, p.Key, p.Value);
            }

            return Text;
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




        protected virtual void OnTagviewRowActivated(object o, Gtk.RowActivatedArgs args) {
            try {
                KeyValuePair<string, string> item = (KeyValuePair<string, string>)itemByPath(tagstore, args.Path);
                sourceview.Buffer.InsertAtCursor("<"+item.Key+">"+"</"+item.Key+">");
            } catch{}

        }

protected virtual void OnTagviewDragBegin(object o, Gtk.DragBeginArgs args) {}

  

        protected virtual void OnBtnAutoClicked(object sender, System.EventArgs e)
        {
            if(funcview.Selection.GetSelectedRows().Length>0) {
                try {
                    KeyValuePair<INode, CommentHolder> item = (KeyValuePair<INode, CommentHolder>)itemByPath(store, funcview.Selection.GetSelectedRows()[0]);
                    if (item.Value.text != String.Empty) {
                        MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo , "This will erase your old comment. Are you sure?", null);
                        if ((int)m.Run() == (int)ResponseType.No) return;
                    }

                    sourceview.Buffer.Text = Docer.generateComment(item.Key);

                } catch{}
            }
        }

        protected virtual void OnBtnValidateClicked(object sender, System.EventArgs e)
        {
            // TODO: check this
            MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "Validation passed!", null);
            m.Run();
        }
        TooltipWindow tooltip_window = new TooltipWindow();
        [GLib.ConnectBefore]
        protected virtual void OnTagviewMotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args)
        {
        TreePath path = null;
        tagview.GetPathAtPos((int)args.Event.X, (int)args.Event.Y, out path); 
        if (path != null){
           Requisition size = tooltip_window.SizeRequest();
           tooltip_window.Move((int)(args.Event.XRoot - size.Width/2),
                (int)(args.Event.YRoot  + 12));
            KeyValuePair<string, string> item = (KeyValuePair<string, string>)itemByPath(tagstore, path);
            tooltip_window.SetLabel( item.Value );
            tooltip_window.Show();
        }
        }
        [GLib.ConnectBefore]
        protected virtual void OnTagviewLeaveNotifyEvent(object o, Gtk.LeaveNotifyEventArgs args)
        {
        tooltip_window.Hide();
        }

}

}
