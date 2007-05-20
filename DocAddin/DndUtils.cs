
using System;
using System.Text.RegularExpressions;
using Gtk;

namespace DocAddin
{
	
	public static class DndUtils
	{
		// Enums
		// Enums :: Drag-and-Drop TargetType
		public enum TargetType {
			UriList,
			Uri,
			Plain
		};

		// Drag-and-Drop Targets
		public static readonly TargetEntry TargetUriList =
			new TargetEntry
			  ("text/uri-list", 0,
			   (uint) TargetType.UriList);

		public static readonly TargetEntry TargetNetscapeUrl =
			new TargetEntry
			  ("_NETSCAPE_URL", 0,
			   (uint) TargetType.Uri);

		public static readonly TargetEntry TargetPlain =
			new TargetEntry
			  ("text/plain", 0,
			   (uint) TargetType.Plain);

		// Methods
		// Methods :: Public
		// Methods :: Public :: SelectionDataToString
		/// <summary>
		///	Converts <see cref="Gtk.SelectionData" /> to a
		/// 	<see cref="String" />.
		/// </summary>
		/// <remarks>
		///	Data in <see cref="Gtk.SelectionData" /> is held as an
		/// 	array of <see cref="Byte">bytes</see>. This function
		///	just calls <see cref="System.Text.Encoding.UTF8.GetString" />
		///	on that array.
		/// </remarks>
		/// <param name="data">
		///	A <see cref="Gtk.SelectionData" /> object.
		/// </param>
		public static string SelectionDataToString (Gtk.SelectionData data)
		{
			return System.Text.Encoding.UTF8.GetString (data.Data);
		}

		// Methods :: Public :: SplitSelectionData
		/// <summary>
		///	Split <see cref="Gtk.SelectionData" /> data into an
		/// 	array of <see cref="String">strings</see>.
		/// </summary>
		/// <remarks>
		///	Data is separated by "\r\n" pairs.
		/// </remarks>
		/// <param name="data">
		///	A <see cref="Gtk.SelectionData" /> object.
		/// </param>
		public static string [] SplitSelectionData (Gtk.SelectionData data)
		{
			string s = SelectionDataToString (data);
			return SplitSelectionData (s);
		}

		/// <summary>
		///	Split <see cref="Gtk.SelectionData" /> data into an
		///	array of <see cref="String">strings</see>.
		/// </summary>
		/// <remarks>
		///	Data is separated by "\r\n" pairs.
		/// </remarks>
		/// <param name="data">
		///	A <see cref="String" />.
		/// </param>
		public static string [] SplitSelectionData (string data)
		{
			return Regex.Split (data, Environment.NewLine);
		}
	}
}
