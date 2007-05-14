
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

    public string prepare(int offset){
        string o = "\n";
        string off = new string(' ', offset);
        foreach(string s in text.Trim().Split(new char[]{'\n'})){
            o += off + "/// " + s + "\n";
        }
        return o.TrimEnd(new char[]{'\n'});
    }
}

}
