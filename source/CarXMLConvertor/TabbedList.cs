using System.Collections.Generic;

namespace CarXmlConvertor
{
	internal class TabbedList
	{
		internal TabbedList()
		{
			this.Lines = new List<string>();
			this.Lines.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
			this.Lines.Add("<openBVE xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
			this.Tabs = 0;
		}

		internal List<string> Lines;

		internal void Add(string Line)
		{
			bool found = false;
			bool decreaseTabs = false;
			for (int i = 1; i < Line.Length; i++)
			{
				if (found)
				{
					break;
				}
				switch (Line[i])
				{
					case '<':
						//If we find a second opening symbol in our line, it's a single-line declaration
						found = true;
						this.Tabs++;
						decreaseTabs = true;
						break;
					case '/':
						//We are closing a declaration
						if (Line[i -1] == '<')
						{
							if (this.Tabs > 0)
							{
								//Cannot decrease tabs below zero
								decreaseTabs = true;
							}
							found = true;
						}
						break;
				}
			}
			if (!found && this.Lines.Count != 0)
			{
				//This is either an opening tag, or a node contents contained on a new line
				//In either case, we want to increase our tabs by one
				this.Tabs++;
			}
			this.Lines.Add(new string('\t', Tabs) + Line);
			if (decreaseTabs)
			{
				this.Tabs--;
			}
		}

		internal void AddRange(List<string> newLines)
		{
			foreach (var Line in newLines)
			{
				Add(Line);
			}
		}

		internal int Tabs;
	}
}
