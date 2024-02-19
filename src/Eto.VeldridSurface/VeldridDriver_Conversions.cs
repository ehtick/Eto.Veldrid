using Eto.Drawing;

namespace TestEtoVeldrid2;

public partial class VeldridDriver
{
	private Point WorldToScreen(float x, float y)
	{
		// int oX = (int)((x - ovpSettings.getCameraX() / (ovpSettings.getZoomFactor() * ovpSettings.getBaseZoom())) + Surface.RenderWidth / 2);

		double oX_2 = (double)Surface.RenderWidth / 2;
		double oX_3 = ovpSettings.getCameraX() / (ovpSettings.getZoomFactor() * ovpSettings.getBaseZoom());
		double oX_4 = x;

		int oXC = (int)(oX_4 - oX_3 + oX_2);

		// int oY = (int)((y - ovpSettings.getCameraY() / (ovpSettings.getZoomFactor() * ovpSettings.getBaseZoom())) + Surface.RenderHeight / 2);

		double oY_2 = (double)Surface.RenderHeight / 2;
		double oY_3 = ovpSettings.getCameraY() / (ovpSettings.getZoomFactor() * ovpSettings.getBaseZoom());
		double oY_4 = y;

		int oYC = (int)(oY_4 - oY_3 + oY_2);

		return new Point(oXC, oYC);
	}

	private Size WorldToScreen(SizeF pt)
	{
		Point pt1 = WorldToScreen(0, 0);
		Point pt2 = WorldToScreen(pt.Width, pt.Height);
		return new Size(pt2.X - pt1.X, pt2.Y - pt1.Y);
	}
}
