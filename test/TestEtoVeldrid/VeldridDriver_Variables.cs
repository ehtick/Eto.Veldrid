using Eto.Drawing;
using Eto.Forms;
using Eto.Veldrid;
using System.Numerics;
using Veldrid;

namespace TestEtoVeldrid;

public partial class VeldridDriver
{
	// Core Veldrid stuff. This shouldn't need to be messed with
	private VeldridSurface _surface;

	public VeldridSurface Surface
	{
		get
		{
			return _surface;
		}
		private set
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

	private float axisZ = 0;
	private float gridZ = 0;

	private DeviceBuffer GridVertexBuffer;
	private DeviceBuffer GridIndexBuffer;
	private DeviceBuffer AxesVertexBuffer;
	private DeviceBuffer AxesIndexBuffer;

	private DeviceBuffer VertexBuffer { get; set; }
	private DeviceBuffer IndexBuffer { get; set; }
	private DeviceBuffer ModelBuffer { get; set; }

	private DeviceBuffer ViewBuffer;

	private DeviceBuffer LinesVertexBuffer;
	private DeviceBuffer PointsVertexBuffer;
	private DeviceBuffer PolysVertexBuffer;
	private DeviceBuffer TessVertexBuffer;

	private Pipeline LinePipeline;
	private Pipeline LinesPipeline;
	private Pipeline FilledPipeline;

	private Matrix4x4 ModelMatrix { get; set; } = Matrix4x4.Identity;
	private Matrix4x4 ViewMatrix;
	private ResourceSet ModelMatrixSet { get; set; }

	private ResourceSet ViewMatrixSet;

	private CommandList CommandList { get; set; }

	private Shader VertexShader { get; set; }
	private Shader FragmentShader { get; set; }

	private bool Ready = false;
	private PointF savedLocation;

	private const float pointWidth = 0.50f;
	private bool hasFocus;

	// Use for drag handling.
	private bool dragging { get; set; }
	private float x_orig;
	private float y_orig;


	private ContextMenu menu;
}
