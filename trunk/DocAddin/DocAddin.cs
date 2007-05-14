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
namespace MonoDevelop.DocAddIn
{
	public enum Commands
	{
		OpenDocer
	}

	public class OpenDocerHandler : CommandHandler {
        protected override void Run()
		{
            DocAddin.DocerWindow w = new DocAddin.DocerWindow();
            if(!w.SetFile(IdeApp.Workbench.ActiveDocument.FileName)){            
               MessageDialog m = new MessageDialog(IdeApp.Workbench.RootWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "cannot parse the file!", null);
               m.Run();
            }else{
                if(w.Run() == (int)ResponseType.Ok){
                    w.SaveComments();
                    IdeApp.Workbench.ActiveDocument.IsDirty = true;            
                }
                w.Hide();
            }
		}

		protected override void Update(CommandInfo info)
		{		
			info.Enabled = IdeApp.Workbench.ActiveDocument != null;
		}
		
			
	}
	}