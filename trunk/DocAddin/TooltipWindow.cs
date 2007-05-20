//

using System;
using Gtk;

namespace DocAddin
{
	
	
	public class TooltipWindow: Window
	{
	    Label label = new Label(String.Empty);
		public TooltipWindow():base(WindowType.Popup)
		{
		      Name = "gtk-tooltips";
		      Resizable = false;
		      BorderWidth = 4;
              AppPaintable = true;

        label.LineWrap = true;        
        label.SetAlignment((float)0.5, (float)0.5);
        label.UseMarkup = true;
        label.Show();
        Add(label);
		}
		
		public void SetLabel(string s){
		label.Text = s;
		}
	}
}
