﻿using Eto.Forms;
using Eto.Mac.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using System;
using Veldrid;
using Veldrid.OpenGL;

#if MONOMAC
using MonoMac.AppKit;
#elif XAMMAC2
using AppKit;
#endif

namespace PlaceholderName
{
	public class MacVeldridView : NSView, IMacControl
	{
		public override bool AcceptsFirstMouse(NSEvent theEvent)
		{
			return CanFocus;
		}

		public override bool AcceptsFirstResponder()
		{
			return CanFocus;
		}

		public bool CanFocus { get; set; } = true;

		public WeakReference WeakHandler { get; set; }

		GraphicsMode Mode = new GraphicsMode(new ColorFormat(32), 8, 8);

		public IWindowInfo WindowInfo { get; set; }
		public GraphicsContext Context { get; private set; }

		public event EventHandler OpenGLContextCreated;

		public void CreateOpenGLContext()
		{
			WindowInfo = Utilities.CreateMacOSWindowInfo(Window.Handle, Handle);

			Context = new GraphicsContext(Mode, WindowInfo, 3, 3, GraphicsContextFlags.ForwardCompatible);
		}

		public void MakeCurrent(IntPtr context)
		{
			Context.MakeCurrent(WindowInfo);
		}

		public void UpdateContext()
		{
			WindowInfo = Utilities.CreateMacOSWindowInfo(Window.Handle, Handle);

			Context?.Update(WindowInfo);
		}

		public override void DidChangeBackingProperties()
		{
			base.DidChangeBackingProperties();

			if (Context == null)
			{
				CreateOpenGLContext();

				Context.MakeCurrent(WindowInfo);

				OpenGLContextCreated?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				UpdateContext();
			}
		}
	}

	public class MacVeldridSurfaceHandler : MacView<MacVeldridView, VeldridSurface, VeldridSurface.ICallback>, VeldridSurface.IHandler
	{
		public new VeldridSurface.ICallback Callback => (VeldridSurface.ICallback)base.Callback;
		public new VeldridSurface Widget => (VeldridSurface)base.Widget;

		public override NSView ContainerControl => Control;

		public override bool Enabled { get; set; }

		public MacVeldridSurfaceHandler()
		{
			Control = new MacVeldridView();

			Control.OpenGLContextCreated += Control_OpenGLContextCreated;
		}

		private void Control_OpenGLContextCreated(object sender, EventArgs e)
		{
			Callback.InitializeOpenGL(Widget);
		}

		public void InitializeOpenGL()
		{
			var platformInfo = new OpenGLPlatformInfo(
				VeldridGL.GetGLContextHandle(),
				VeldridGL.GetProcAddress,
				Control.MakeCurrent,
				VeldridGL.GetCurrentContext,
				VeldridGL.ClearCurrentContext,
				VeldridGL.DeleteContext,
				VeldridGL.SwapBuffers,
				VeldridGL.SetVSync,
				VeldridGL.SetSwapchainFramebuffer,
				(w, h) => Control.UpdateContext());

			Widget.GraphicsDevice = GraphicsDevice.CreateOpenGL(
				Widget.GraphicsDeviceOptions,
				platformInfo,
				(uint)Widget.Width,
				(uint)Widget.Height);

			Widget.Swapchain = Widget.GraphicsDevice.MainSwapchain;

			Callback.OnVeldridInitialized(Widget, EventArgs.Empty);
		}

		public void InitializeOtherApi()
		{
			// To embed Veldrid in an Eto control, all these platform-specific
			// versions of InitializeOtherApi use the technique outlined here:
			//
			//   https://github.com/mellinoe/veldrid/issues/155
			//
			var source = SwapchainSource.CreateNSView(Control.Handle);

			Widget.Swapchain = Widget.GraphicsDevice.ResourceFactory.CreateSwapchain(
				new SwapchainDescription(
					source,
					(uint)Widget.Width,
					(uint)Widget.Height,
					PixelFormat.R32_Float,
					false));

			Callback.OnVeldridInitialized(Widget, EventArgs.Empty);
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

			var platform = new Eto.Mac.Platform();
			platform.Add<VeldridSurface.IHandler>(() => new MacVeldridSurfaceHandler());

			new Application(platform).Run(new MainForm(backend));
		}
	}
}
