// project created on 5/11/2007 at 12:26 PM
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