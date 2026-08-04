namespace CsvRwRouteParser
{
	internal enum ControlCommands
	{
		/// <summary>The If directive</summary>
		If,
		/// <summary>The Else directive</summary>
		Else,
		/// <summary>The ElseIf directive</summary>
		ElseIf,
		/// <summary>The EndIf directive</summary>
		EndIf,
		/// <summary>Includes another file</summary>
		Include,
		/// <summary>Inserts the specified unicode character</summary>
		Chr,
		ChrUni = Chr,
		/// <summary>Inserts the specified ASCII character</summary>
		ChrAscii,
		/// <summary>Inserts a random number</summary>
		Rnd,
		/// <summary>Inserts the text stored in the matching Sub directive</summary>
		Sub,

	}
}
