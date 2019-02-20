using System;
using Eto.Forms;
using Eto.Drawing;

namespace Eto.VeldridSurface
{
	public partial class VeldridForm : Form
	{
		void InitializeComponent()
		{
			Title = "Veldrid in Eto";
			ClientSize = new Size(400, 350);
			Padding = 10;

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);

			var drawCommand = new Command { MenuText = "Draw" };
			drawCommand.Executed += (sender, e) =>
			{
                VeldridDriver.SetUpVeldrid();
                VeldridDriver.Draw();
			};

			Menu = new MenuBar
			{
				Items =
				{
					new ButtonMenuItem { Text = "&File", Items = { drawCommand } }
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};
		}
	}
}