
using System;

namespace DocAddin
{
	
	
public class CommentHolder {
    public string text;
    public int lineStart, lineStop;
    public CommentHolder(string s, int i1, int i2){

        text = s;
        lineStart = i1;
        lineStop = i2;
    }
    public override string ToString(){
        return " text: " + text + " from : " + lineStart + " to : " + lineStop;
    }

    public string prepare(string off){
        string o = Environment.NewLine;
        foreach(string s in text.Trim().Split(Environment.NewLine.ToCharArray())){
            o += off + "/// " + s + Environment.NewLine;
        }
        return o.TrimEnd(Environment.NewLine.ToCharArray());
    }
}

}
