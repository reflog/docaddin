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
using System.IO;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui.Content;
using Gtk;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Parser.AST;
using System.Collections.Generic;

namespace MonoDevelop.DocAddIn {
    public enum Commands
    {
        OpenDocer,
        AutoDocer
    }

public class AutoDocerHandler : CommandHandler {
        protected override void Update(CommandInfo info) {
            info.Enabled = IdeApp.Workbench.ActiveDocument != null;
        }

        protected override void Run() {
            IParser p = ParserFactory.CreateParser(IdeApp.Workbench.ActiveDocument.FileName);
            p.Parse();

            if(p.Errors.count > 0) {
                MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "cannot parse the file!", null);
                m.Run();
            } else {
                List<KeyValuePair<INode, DocAddin.CommentHolder>> nodes = new List<KeyValuePair<INode,  DocAddin.CommentHolder>>();

                foreach(INode n1 in p.CompilationUnit.Children) {
                    DocAddin.Docer.collectNodes(nodes, n1, p);
                }
                KeyValuePair<INode, DocAddin.CommentHolder> item = DocAddin.Docer.findNodeByPos(nodes, IdeApp.Workbench.ActiveDocument.TextEditor.Text, IdeApp.Workbench.ActiveDocument.TextEditor.CursorPosition);
                if(item.Key != null) {
                    if (item.Value.text != String.Empty) {
                        MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo , "This will erase your old comment. Are you sure?", null);
                        if ((int)m.Run() == (int)ResponseType.No) return;
                    }
                   string s = DocAddin.Docer.replaceComment(IdeApp.Workbench.ActiveDocument.TextEditor.Text, item.Key, item.Value);
                   if (s != IdeApp.Workbench.ActiveDocument.TextEditor.Text){
                    IdeApp.Workbench.ActiveDocument.TextEditor.DeleteText(0, IdeApp.Workbench.ActiveDocument.TextEditor.Text.Length);
                    IdeApp.Workbench.ActiveDocument.TextEditor.InsertText(0, s);
                    }
                }
            }
        }
    }

public class OpenDocerHandler : CommandHandler {
        protected override void Run() {
            DocAddin.DocerWindow w = new DocAddin.DocerWindow();
            if(!w.SetFile(IdeApp.Workbench.ActiveDocument.FileName)) {
                MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "cannot parse the file!", null);
                m.Run();
            } else {
                if(w.Run() == (int)ResponseType.Ok) {
                    string text = w.SaveComments();
                    IdeApp.Workbench.ActiveDocument.TextEditor.DeleteText(0, IdeApp.Workbench.ActiveDocument.TextEditor.Text.Length);
                    IdeApp.Workbench.ActiveDocument.TextEditor.InsertText(0, text);
                    IdeApp.Workbench.ActiveDocument.IsDirty = true;
                }
                w.Hide();
            }
        }

        protected override void Update(CommandInfo info) {
            info.Enabled = IdeApp.Workbench.ActiveDocument != null;
        }


    }


}