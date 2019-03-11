﻿using Eto.Forms;
using Eto.Gl;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Veldrid;

namespace Eto.VeldridSurface
{
	public static class VeldridGL
	{
		public static Dictionary<IntPtr, GLSurface> Contexts = new Dictionary<IntPtr, GLSurface>();

		public static List<GLSurface> Surfaces = new List<GLSurface>();

		public static IntPtr GetGLContextHandle()
		{
			return GetCurrentContext();
		}

		public static IntPtr GetProcAddress(string name)
		{
			Type type = typeof(OpenTK.Platform.Utilities);

			MethodInfo createGetAddress = type.GetMethod("CreateGetAddress", BindingFlags.NonPublic | BindingFlags.Static);
			var getAddress = (GraphicsContext.GetAddressDelegate)createGetAddress.Invoke(null, Array.Empty<string>());

			return getAddress.Invoke(name);
		}

		public static void MakeCurrent(IntPtr context)
		{
			Type type = typeof(GraphicsContext);

			var available = (Dictionary<ContextHandle, IGraphicsContext>)type
				.GetField("available_contexts", BindingFlags.NonPublic | BindingFlags.Static)
				.GetValue(null);

			bool found = false;
			foreach (KeyValuePair<ContextHandle, IGraphicsContext> pair in available)
			{
				foreach (GLSurface s in Surfaces)
				{
					if (pair.Key.Handle == context)
					{
						if (!Contexts.ContainsKey(context))
						{
							Contexts.Add(context, s);
						}
						Contexts[context].MakeCurrent();

						found = true;
					}

					if (found)
					{
						break;
					}
				}

				if (found)
				{
					break;
				}
			}
		}

		public static IntPtr GetCurrentContext()
		{
			return GraphicsContext.CurrentContextHandle.Handle;
		}

		public static void ClearCurrentContext()
		{
			GraphicsContext.CurrentContext.MakeCurrent(null);
		}

		public static void DeleteContext(IntPtr context)
		{
			// Do nothing! With this Eto.Gl-based approach, Veldrid should never
			// need to destroy an OpenGL context on its own; let the GLSurface
			// take care of context deletion upon its own disposal.
		}

		public static void SwapBuffers()
		{
			GraphicsContext.CurrentContext.SwapBuffers();
		}

		public static void SetVSync(bool on)
		{
			GraphicsContext.CurrentContext.SwapInterval = on ? 1 : 0;
		}

		// It's perfectly acceptable to create an instance of OpenGLPlatformInfo
		// without providing these last two methods, if indeed you don't need
		// them. They're stubbed out here only to serve as a reminder that they
		// can be customized should the occasion call for it.

		public static void SetSwapchainFramebuffer()
		{
		}

		public static void ResizeSwapchain(uint width, uint height)
		{
		}
	}

	/// <summary>
	/// A simple control that allows drawing with Veldrid.
	/// </summary>
	public class VeldridSurface : Panel
	{
		private VeldridDriver _driver;
		public VeldridDriver Driver
		{
			get { return _driver; }
			set
			{
				_driver = value;

				_driver.Surface = this;
			}
		}

		public static GraphicsBackend PreferredBackend
		{
			get
			{
				GraphicsBackend backend;
				if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal))
				{
					backend = GraphicsBackend.Metal;
				}
				else if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan))
				{
					backend = GraphicsBackend.Vulkan;
				}
				else if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11))
				{
					backend = GraphicsBackend.Direct3D11;
				}
				else
				{
					backend = GraphicsBackend.OpenGL;
				}

				return backend;
			}
		}

		public GraphicsDevice GraphicsDevice { get; set; }
		public Swapchain Swapchain { get; set; }

		public static string VeldridSurfaceInitializedEvent = "VeldridSurface.Initialized";

		public event EventHandler<EventArgs> VeldridSurfaceInitialized
		{
			add { Properties.AddHandlerEvent(VeldridSurfaceInitializedEvent, value); }
			remove { Properties.RemoveEvent(VeldridSurfaceInitializedEvent, value); }
		}

		private new Control Content
		{
			get { return base.Content; }
			set { base.Content = value; }
		}

		public VeldridSurface(Action<VeldridSurface, GraphicsBackend, Action> initOther) :
			this(initOther, PreferredBackend)
		{
		}
		public VeldridSurface(Action<VeldridSurface, GraphicsBackend, Action> initOther, GraphicsBackend backend)
		{
			Driver = new VeldridDriver();

			if (backend == GraphicsBackend.OpenGL)
			{
				var mode = new GraphicsMode();
				int major = 3;
				int minor = EtoEnvironment.Platform.IsMac ? 2 : 0;
				GraphicsContextFlags flags = GraphicsContextFlags.ForwardCompatible;

				var surface = new GLSurface(mode, major, minor, flags);
				surface.GLInitalized += (sender, e) =>
				{
					InitGL();
					OnVeldridInitialized(EventArgs.Empty);
				};

				Content = surface;
			}
			else
			{
				// TODO: Add this all the time, but make a separate set of bools that
				// check for OpenGL. On GLInitalized (sic), set some new boolean called,
				// I don't know, "GLReady", to true. Make it nullable also. Have LoadComplete
				// set one called "LoadComplete", or something, and on setting either of
				// those new bools, call a function that checks for both LoadComplete being
				// true, and GLReady being either null, or non-null and true. Raise the
				// VeldridInitialized event if those conditions are met. Hopefully that'll
				// take care of OpenGL in WPF.
				LoadComplete += (sender, e) =>
				{
					initOther.Invoke(this, backend, Driver.Draw);
					OnVeldridInitialized(EventArgs.Empty);
				};
			}
		}

		private void InitGL()
		{
			VeldridGL.Surfaces.Add(Content as GLSurface);

			var platformInfo = new Veldrid.OpenGL.OpenGLPlatformInfo(
				VeldridGL.GetGLContextHandle(),
				VeldridGL.GetProcAddress,
				VeldridGL.MakeCurrent,
				VeldridGL.GetCurrentContext,
				VeldridGL.ClearCurrentContext,
				VeldridGL.DeleteContext,
				VeldridGL.SwapBuffers,
				VeldridGL.SetVSync,
				VeldridGL.SetSwapchainFramebuffer,
				VeldridGL.ResizeSwapchain);

			GraphicsDevice = GraphicsDevice.CreateOpenGL(
				new GraphicsDeviceOptions(),
				platformInfo,
				640,
				480);

			Swapchain = GraphicsDevice.MainSwapchain;
		}

		protected virtual void OnVeldridInitialized(EventArgs e)
		{
			Properties.TriggerEvent(VeldridSurfaceInitializedEvent, this, e);
		}
	}
}
