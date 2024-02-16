using Eto.Forms;
using Eto.Veldrid;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;
using VeldridEto;

namespace TestEtoVeldrid2
{
	public struct VertexPositionColor
	{
		public static uint SizeInBytes = (uint)Marshal.SizeOf(typeof(VertexPositionColor));

		public Vector3 Position;
		public RgbaFloat Color;

		public VertexPositionColor(Vector3 position, RgbaFloat color)
		{
			Position = position;
			Color = color;
		}
	}

	/// <summary>
	/// A class that controls rendering to a VeldridSurface.
	/// </summary>
	/// <remarks>
	/// VeldridSurface is only a basic control that lets you render to the screen
	/// using Veldrid. How exactly to do that is up to you; this driver class is
	/// only one possible approach, and in all likelihood not the most efficient.
	/// </remarks>
	public class VeldridDriver
	{
		public OVPSettings ovpSettings;

		private VeldridSurface _surface;
		public VeldridSurface Surface
		{
			get { return _surface; }
			set
			{
				_surface = value;

				Surface.Draw += (sender, e) => Draw();
			}
		}

		private uint[] polyFirst;
		private uint[] polyVertexCount;
		private uint[] tessFirst;
		private uint[] tessVertexCount;
		private uint[] lineFirst;
		private uint[] lineVertexCount;
		private uint[] pointsFirst;
		private uint[] gridIndices;
		private uint[] axesIndices;

		private float axisZ;
		private float gridZ;

		private DeviceBuffer GridVertexBuffer;
		private DeviceBuffer GridIndexBuffer;
		private DeviceBuffer AxesVertexBuffer;
		private DeviceBuffer AxesIndexBuffer;

		private DeviceBuffer LinesVertexBuffer;
		private DeviceBuffer PointsVertexBuffer;
		private DeviceBuffer PolysVertexBuffer;
		private DeviceBuffer TessVertexBuffer;

		private Pipeline PointsPipeline;
		private Pipeline LinePipeline;
		private Pipeline LinesPipeline;
		private Pipeline FilledPipeline;

		public UITimer Clock { get; } = new UITimer();

		public CommandList CommandList { get; private set; }
		public DeviceBuffer VertexBuffer { get; private set; }

		public DeviceBuffer IndexBuffer { get; private set; }
		public Shader VertexShader { get; private set; }
		public Shader FragmentShader { get; private set; }
		public Pipeline Pipeline { get; private set; }

		public Matrix4x4 ModelMatrix { get; private set; } = Matrix4x4.Identity;
		public DeviceBuffer ModelBuffer { get; private set; }
		public ResourceSet ModelMatrixSet { get; private set; }

		private Matrix4x4 ViewMatrix;
		private DeviceBuffer ViewBuffer;
		private ResourceSet ViewMatrixSet;

		public bool Animate { get; set; } = true;

		private int _direction = 1;
		public bool Clockwise
		{
			get { return _direction == 1 ? true : false; }
			set { _direction = value ? 1 : -1; }
		}

		public int Speed { get; set; } = 1;

		private bool Ready = false;

		public VeldridDriver(ref OVPSettings settings, ref VeldridSurface surface)
		{
			ovpSettings = settings;
			Surface = surface;
			Clock.Interval = 1.0f / 60.0f;
			Clock.Elapsed += Clock_Elapsed;
		}

		private void Clock_Elapsed(object sender, EventArgs e) => Surface.Invalidate();

		private DateTime CurrentTime;
		private DateTime PreviousTime = DateTime.Now;

		private void drawAxes()
		{
			if (!ovpSettings.drawAxes())
			{
				return;
			}

			float zoom = ovpSettings.getBaseZoom() * ovpSettings.getZoomFactor();
			VertexPositionColor[] axesArray = new VertexPositionColor[4];
			axesArray[0] = new VertexPositionColor(new Vector3(0.0f, ovpSettings.getCameraY() + Surface.RenderHeight * zoom, axisZ), new RgbaFloat(ovpSettings.axisColor.R, ovpSettings.axisColor.G, ovpSettings.axisColor.B, 1.0f));
			axesArray[1] = new VertexPositionColor(new Vector3(0.0f, ovpSettings.getCameraY() - Surface.RenderHeight * zoom, axisZ), new RgbaFloat(ovpSettings.axisColor.R, ovpSettings.axisColor.G, ovpSettings.axisColor.B, 1.0f));
			axesArray[2] = new VertexPositionColor(new Vector3(ovpSettings.getCameraX() + Surface.RenderWidth * zoom, 0.0f, axisZ), new RgbaFloat(ovpSettings.axisColor.R, ovpSettings.axisColor.G, ovpSettings.axisColor.B, 1.0f));
			axesArray[3] = new VertexPositionColor(new Vector3(ovpSettings.getCameraX() - Surface.RenderWidth * zoom, 0.0f, axisZ), new RgbaFloat(ovpSettings.axisColor.R, ovpSettings.axisColor.G, ovpSettings.axisColor.B, 1.0f));

			axesIndices = new uint[4] { 0, 1, 2, 3 };

			updateBuffer(ref AxesVertexBuffer, axesArray, VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer);
			updateBuffer(ref AxesIndexBuffer, axesIndices, sizeof(uint), BufferUsage.IndexBuffer);
		}

		public void Draw()
		{
			if (!Ready)
			{
				return;
			}

			CommandList.Begin();

			ModelMatrix *= Matrix4x4.CreateFromAxisAngle(
				new Vector3(0, 0, 1), 0);
			CommandList.UpdateBuffer(ModelBuffer, 0, ModelMatrix);

			float zoom = ovpSettings.getZoomFactor() * ovpSettings.getBaseZoom();

			float left = ovpSettings.getCameraX() - (float)Surface.RenderWidth / 2 * zoom;
			float right = ovpSettings.getCameraX() + (float)Surface.RenderWidth / 2 * zoom;
			float bottom = ovpSettings.getCameraY() + (float)Surface.RenderHeight / 2 * zoom;
			float top = ovpSettings.getCameraY() - (float)Surface.RenderHeight / 2 * zoom;

			ViewMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.0f, 1.0f);
			CommandList.UpdateBuffer(ViewBuffer, 0, ViewMatrix);

			CommandList.SetFramebuffer(Surface.Swapchain.Framebuffer);

			// These commands differ from the stock Veldrid "Getting Started"
			// tutorial in two ways. First, the viewport is cleared to pink
			// instead of black so as to more easily distinguish between errors
			// in creating a graphics context and errors drawing vertices within
			// said context. Second, this project creates its swapchain with a
			// depth buffer, and that buffer needs to be reset at the start of
			// each frame.

			RgbaFloat bgColor = new(ovpSettings.backColor.R, ovpSettings.backColor.G, ovpSettings.backColor.B, 1.0f);

			CommandList.ClearColorTarget(0, bgColor);
			CommandList.ClearDepthStencil(1.0f);

			if (GridVertexBuffer != null)
			{
				lock (GridVertexBuffer)
				{
					try
					{
						CommandList.SetVertexBuffer(0, GridVertexBuffer);
						CommandList.SetIndexBuffer(GridIndexBuffer, IndexFormat.UInt32);
						CommandList.SetPipeline(LinePipeline);
						CommandList.SetGraphicsResourceSet(0, ViewMatrixSet);
						CommandList.SetGraphicsResourceSet(1, ModelMatrixSet);

						CommandList.DrawIndexed(
							indexCount: (uint)gridIndices.Length,
							instanceCount: 1,
							indexStart: 0,
							vertexOffset: 0,
							instanceStart: 0);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Ex: " + ex);
					}
				}
			}

			drawAxes();
			if (AxesVertexBuffer != null)
			{
				lock (AxesVertexBuffer)
				{
					try
					{
						CommandList.SetVertexBuffer(0, AxesVertexBuffer);
						CommandList.SetIndexBuffer(AxesIndexBuffer, IndexFormat.UInt32);
						CommandList.SetPipeline(LinePipeline);
						CommandList.SetGraphicsResourceSet(0, ViewMatrixSet);
						CommandList.SetGraphicsResourceSet(1, ModelMatrixSet);

						CommandList.DrawIndexed(
							indexCount: (uint)axesIndices.Length,
							instanceCount: 1,
							indexStart: 0,
							vertexOffset: 0,
							instanceStart: 0);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Ex: " + ex);
					}
				}
			}

			CommandList.End();

			try
			{
				lock (CommandList)
				{
					Surface.GraphicsDevice.SubmitCommands(CommandList);
				}

				Surface.GraphicsDevice.SwapBuffers(Surface.Swapchain);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Ex: " + ex);
			}

		}

		public void SetUpVeldrid()
		{
			CreateResources();

			Ready = true;
		}

		private void CreateResources()
		{
			// Veldrid.SPIRV is an additional library that complements Veldrid
			// by simplifying the development of cross-backend shaders, and is
			// currently the recommended approach to doing so:
			//
			//   https://veldrid.dev/articles/portable-shaders.html
			//
			// If you decide against using it, you can try out Veldrid developer
			// mellinoe's other project, ShaderGen, or drive yourself crazy by
			// writing and maintaining custom shader code for each platform.
			byte[] vertexShaderSpirvBytes = LoadSpirvBytes(ShaderStages.Vertex);
			byte[] fragmentShaderSpirvBytes = LoadSpirvBytes(ShaderStages.Fragment);

			var options = new CrossCompileOptions();
			switch (Surface.GraphicsDevice.BackendType)
			{
				// InvertVertexOutputY and FixClipSpaceZ address two major
				// differences between Veldrid's various graphics APIs, as
				// discussed here:
				//
				//   https://veldrid.dev/articles/backend-differences.html
				//
				// Note that the only reason those options are useful in this
				// example project is that the vertices being drawn are stored
				// the way Vulkan stores vertex data. The options will therefore
				// properly convert from the Vulkan style to whatever's used by
				// the destination backend. If you store vertices in a different
				// coordinate system, these may not do anything for you, and
				// you'll need to handle the difference in your shader code.
				case GraphicsBackend.Metal:
					options.InvertVertexOutputY = true;
					break;
				case GraphicsBackend.Direct3D11:
					options.InvertVertexOutputY = true;
					break;
				case GraphicsBackend.OpenGL:
					options.FixClipSpaceZ = true;
					options.InvertVertexOutputY = true;
					break;
				default:
					break;
			}

			ResourceFactory factory = Surface.GraphicsDevice.ResourceFactory;

			ResourceLayout viewMatrixLayout = factory.CreateResourceLayout(
				new ResourceLayoutDescription(
					new ResourceLayoutElementDescription(
						"ViewMatrix",
						ResourceKind.UniformBuffer,
						ShaderStages.Vertex)));

			ViewBuffer = factory.CreateBuffer(
				new BufferDescription(64, BufferUsage.UniformBuffer));

			ViewMatrixSet = factory.CreateResourceSet(new ResourceSetDescription(
				viewMatrixLayout, ViewBuffer));

			var vertex = new ShaderDescription(ShaderStages.Vertex, vertexShaderSpirvBytes, "main", true);
			var fragment = new ShaderDescription(ShaderStages.Fragment, fragmentShaderSpirvBytes, "main", true);
			Shader[] shaders = factory.CreateFromSpirv(vertex, fragment, options);

			ResourceLayout modelMatrixLayout = factory.CreateResourceLayout(
				new ResourceLayoutDescription(
					new ResourceLayoutElementDescription(
						"ModelMatrix",
						ResourceKind.UniformBuffer,
						ShaderStages.Vertex)));

			ModelBuffer = factory.CreateBuffer(
				new BufferDescription(64, BufferUsage.UniformBuffer));

			ModelMatrixSet = factory.CreateResourceSet(new ResourceSetDescription(
				modelMatrixLayout, ModelBuffer));

			VertexPositionColor[] quadVertices =
			{
				new VertexPositionColor(new Vector3(new Vector2(-.75f, -.75f), 0), RgbaFloat.Red),
				new VertexPositionColor(new Vector3(new Vector2(.75f, -.75f), 0), RgbaFloat.Green),
				new VertexPositionColor(new Vector3(new Vector2(-.75f, .75f), 0), RgbaFloat.Blue),
				new VertexPositionColor(new Vector3(new Vector2(.75f, .75f), 0), RgbaFloat.Yellow)
			};

			ushort[] quadIndices = { 0, 1, 2, 3 };

			VertexBuffer = factory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
			IndexBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

			Surface.GraphicsDevice.UpdateBuffer(VertexBuffer, 0, quadVertices);
			Surface.GraphicsDevice.UpdateBuffer(IndexBuffer, 0, quadIndices);

			// Veldrid.SPIRV, when cross-compiling to HLSL, will always produce
			// TEXCOORD semantics; VertexElementSemantic.TextureCoordinate thus
			// becomes necessary to let D3D11 work alongside Vulkan and OpenGL.
			//
			//   https://github.com/mellinoe/veldrid/issues/121
			//
			var vertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
				new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

			Pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription
			{
				BlendState = BlendStateDescription.SingleOverrideBlend,
				DepthStencilState = new DepthStencilStateDescription(
					depthTestEnabled: true,
					depthWriteEnabled: true,
					comparisonKind: ComparisonKind.LessEqual),
				RasterizerState = new RasterizerStateDescription(
					cullMode: FaceCullMode.Back,
					fillMode: PolygonFillMode.Solid,
					frontFace: FrontFace.Clockwise,
					depthClipEnabled: true,
					scissorTestEnabled: false),
				PrimitiveTopology = PrimitiveTopology.TriangleStrip,
				ResourceLayouts = new[] { modelMatrixLayout },
				ShaderSet = new ShaderSetDescription(
					vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
					shaders: shaders),
				Outputs = Surface.Swapchain.Framebuffer.OutputDescription
			});


			LinePipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription
			{
				BlendState = BlendStateDescription.SingleOverrideBlend,
				DepthStencilState = new DepthStencilStateDescription(
					depthTestEnabled: true,
					depthWriteEnabled: true,
					comparisonKind: ComparisonKind.LessEqual),
				RasterizerState = new RasterizerStateDescription(
					cullMode: FaceCullMode.Back,
					fillMode: PolygonFillMode.Solid,
					frontFace: FrontFace.Clockwise,
					depthClipEnabled: true,
					scissorTestEnabled: false),
				PrimitiveTopology = PrimitiveTopology.LineList,
				ResourceLayouts = new[] { viewMatrixLayout, modelMatrixLayout },
				ShaderSet = new ShaderSetDescription(
					vertexLayouts: new[] { vertexLayout },
					shaders: shaders),
				Outputs = Surface.Swapchain.Framebuffer.OutputDescription
			});

			CommandList = factory.CreateCommandList();
		}

		public void updateBuffer<T>(ref DeviceBuffer buffer, T[] data, uint elementSize, BufferUsage usage)
			where T : unmanaged
		{
			switch (data.Length)
			{
				case > 0:
				{
					buffer?.Dispose();

					ResourceFactory factory = Surface.GraphicsDevice.ResourceFactory;

					buffer = factory.CreateBuffer(new BufferDescription(elementSize * (uint)data.Length, usage));

					Surface.GraphicsDevice.UpdateBuffer(buffer, 0, data);
					break;
				}
			}
		}

		private byte[] LoadSpirvBytes(ShaderStages stage)
		{
			string name = $"VertexColor-{stage.ToString().ToLowerInvariant()}.450.glsl";
			string full = $"Eto.VeldridSurface.shaders.{name}";

			// Precompiled SPIR-V bytecode can speed up program start by saving
			// the need to load text files and compile them before converting
			// the result to the final backend shader format. If they're not
			// available, though, the plain .glsl files will do just fine. Look
			// up glslangValidator to learn how to compile SPIR-V binary files.

			using (var stream = GetType().Assembly.GetManifestResourceStream(full))
			using (var reader = new BinaryReader(stream))
			{
				return reader.ReadBytes((int)stream.Length);
			}
		}
	}
}
