﻿using Eto.Forms;
using Eto.Gl;
using Eto.Gl.WPF_WFControl;
using OpenTK;
using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace PlaceholderName
{
	public class PuppetWPFWFGLSurfaceHandler : WPFWFGLSurfaceHandler
	{
		public override void AttachEvent(string id)
		{
			switch (id)
			{
				// Prevent the base surface handler class from attaching its own
				// internal event handler to these events; said handler calls
				// MakeCurrent, uses GL.Viewport, and swaps buffers. That's
				// undesirable here, so just attach the appropriate callback.
				case GLSurface.ShownEvent:
					break;
				case GLSurface.GLDrawEvent:
					WinFormsControl.Paint += (sender, e) => Callback.OnDraw(Widget, EventArgs.Empty);
					break;
				case GLSurface.SizeChangedEvent:
					WinFormsControl.SizeChanged += (sender, e) => Callback.OnSizeChanged(Widget, EventArgs.Empty);
					break;
				default:
					base.AttachEvent(id);
					break;
			}
		}
	}

	public class WpfVeldridSurfaceHandler : VeldridSurfaceHandler
	{
		protected override void InitializeOtherApi()
		{
			base.InitializeOtherApi();

			var dummy = new WpfVeldridHost();
			dummy.Loaded += (sender, e) =>
			{
				// To embed Veldrid in an Eto control, all these platform-specific
				// overrides of InitializeOtherApi use the technique outlined here:
				//
				//   https://github.com/mellinoe/veldrid/issues/155
				//
				var source = SwapchainSource.CreateWin32(
					dummy.Hwnd, Marshal.GetHINSTANCE(typeof(VeldridSurface).Module));
				Widget.Swapchain = Widget.GraphicsDevice.ResourceFactory.CreateSwapchain(
					new SwapchainDescription(
						source,
						(uint)Widget.Width,
						(uint)Widget.Height,
						PixelFormat.R32_Float,
						false));

				Callback.OnVeldridInitialized(Widget, EventArgs.Empty);
			};
			dummy.WMPaint += (sender, e) => Callback.OnDraw(Widget, e);
			dummy.WMSize += (sender, e) => Callback.OnResize(Widget, e);

			RenderTarget = WpfHelpers.ToEto(dummy);
		}
	}

	public static class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			GraphicsBackend backend = VeldridSurface.PreferredBackend;

			if (backend == GraphicsBackend.OpenGL)
			{
				Toolkit.Init(new ToolkitOptions { Backend = PlatformBackend.PreferNative });
			}

			var platform = new Eto.Wpf.Platform();
			platform.Add<GLSurface.IHandler>(() => new PuppetWPFWFGLSurfaceHandler());
			platform.Add<VeldridSurface.IHandler>(() => new WpfVeldridSurfaceHandler());

			new Application(platform).Run(new MainForm(backend));
		}
	}
}
