using System.Windows.Forms;

namespace OpenBve
{
    internal static partial class Game
    {
        internal enum MenuTag { None, Caption, Back, JumpToStation, ExitToMainMenu, Quit, Control }
        internal abstract class MenuEntry
        {
            internal string Text;
            internal double Highlight;
            internal double Alpha;
        }
        internal class MenuCaption : MenuEntry
        {
            internal MenuCaption(string Text)
            {
                this.Text = Text;
                this.Highlight = 0.0;
                this.Alpha = 0.0;
            }
        }
        internal class MenuCommand : MenuEntry
        {
            internal MenuTag Tag;
            internal int Data;
            internal MenuCommand(string Text, MenuTag Tag, int Data)
            {
                this.Text = Text;
                this.Highlight = 0.0;
                this.Alpha = 0.0;
                this.Tag = Tag;
                this.Data = Data;
            }
        }
        internal class MenuSubmenu : MenuEntry
        {
            internal MenuEntry[] Entries;
            internal MenuSubmenu(string Text, MenuEntry[] Entries)
            {
                this.Text = Text;
                this.Highlight = 0.0;
                this.Alpha = 0.0;
                this.Entries = Entries;
            }
        }
        internal static MenuEntry[] CurrentMenu = { };
        internal static int[] CurrentMenuSelection = { -1 };
        internal static double[] CurrentMenuOffsets = { double.NegativeInfinity };
        /// <summary>Creates a new menu</summary>
        /// <param name="QuitOnly">Whether this menu is a full menu, or a quit menu only</param>
        internal static void CreateMenu(bool QuitOnly)
        {
            if (QuitOnly)
            {
                // quit menu only
                CurrentMenu = new MenuEntry[3];
                CurrentMenu[0] = new MenuCaption(Interface.GetInterfaceString("menu_quit_question"));
                CurrentMenu[1] = new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0);
                CurrentMenu[2] = new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0);
                CurrentMenuSelection = new int[] { 1 };
                CurrentMenuOffsets = new double[] { double.NegativeInfinity };
                CurrentMenu[1].Highlight = 1.0;
            }
            else
            {
                // full menu
                int n = 0;
                for (int i = 0; i < Stations.Length; i++)
                {
                    if (PlayerStopsAtStation(i) & Stations[i].Stops.Length > 0)
                    {
                        n++;
                    }
                }
                MenuEntry[] a = new MenuEntry[n];
                n = 0;
                for (int i = 0; i < Stations.Length; i++)
                {
                    if (PlayerStopsAtStation(i) & Stations[i].Stops.Length > 0)
                    {
                        a[n] = new MenuCommand(Stations[i].Name, MenuTag.JumpToStation, i);
                        n++;
                    }
                }

                MenuEntry[] b = new MenuEntry[Interface.CurrentControls.Length];
                for (int i = 0; i < Interface.CurrentControls.Length; i++)
                {
                    b[i] = new MenuCommand(Interface.CurrentControls[i].Command.ToString(), MenuTag.Control, i);
                }
                if (n != 0)
                {
                    CurrentMenu = new MenuEntry[5];
                    CurrentMenu[0] = new MenuCommand(Interface.GetInterfaceString("menu_resume"), MenuTag.Back, 0);
                    CurrentMenu[1] = new MenuSubmenu(Interface.GetInterfaceString("menu_jump"), a);
                    CurrentMenu[2] = new MenuSubmenu(Interface.GetInterfaceString("menu_exit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_exit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_yes"), MenuTag.ExitToMainMenu, 0)
					                                 });
                    CurrentMenu[3] = new MenuSubmenu(Interface.GetInterfaceString("menu_quit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_quit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0)
					                                 });
                    CurrentMenu[4] = new MenuSubmenu("Customise Controls", b);
                }
                else
                {
                    CurrentMenu = new MenuEntry[4];
                    CurrentMenu[0] = new MenuCommand(Interface.GetInterfaceString("menu_resume"), MenuTag.Back, 0);
                    CurrentMenu[1] = new MenuSubmenu(Interface.GetInterfaceString("menu_exit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_exit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_yes"), MenuTag.ExitToMainMenu, 0)
					                                 });
                    CurrentMenu[2] = new MenuSubmenu(Interface.GetInterfaceString("menu_quit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_quit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0)
					                                 });
                    CurrentMenu[3] = new MenuSubmenu("Customise Controls", b);
                }
                CurrentMenuSelection = new int[] { 0 };
                CurrentMenuOffsets = new double[] { double.NegativeInfinity };
                CurrentMenu[0].Highlight = 1.0;
            }
        }
    }
}
