﻿using Eto.Drawing;
using Eto.Forms;
using Eto.Veldrid;
using Veldrid;
using PixelFormat = Veldrid.PixelFormat;

namespace TestEtoVeldrid
{
	public partial class MainForm : Form
	{
		VeldridSurface Surface;

		VeldridDriver Driver;

		private OVPSettings ovpSettings;

		private bool _veldridReady = false;
		public bool VeldridReady
		{
			get { return _veldridReady; }
			private set
			{
				_veldridReady = value;

				SetUpVeldrid();
			}
		}

		private bool _formReady = false;
		public bool FormReady
		{
			get { return _formReady; }
			set
			{
				_formReady = value;

				SetUpVeldrid();
			}
		}

		public MainForm() : this(VeldridSurface.PreferredBackend)
		{
		}
		public MainForm(GraphicsBackend backend)
		{
			InitializeComponent();

			Shown += (sender, e) => FormReady = true;

			// A depth buffer isn't strictly necessary for this project, which uses
			// only 2D vertex coordinates, but it's helpful to create one for the
			// sake of demonstration.
			//
			// The "improved" resource binding model changes how resource slots are
			// assigned in the Metal backend, allowing it to work like the others,
			// so the numbers used in calls to CommandList.SetGraphicsResourceSet
			// will make more sense to developers used to e.g. OpenGL or Direct3D.
			var options = new GraphicsDeviceOptions(
				false,
				PixelFormat.R32_Float,
				false,
				ResourceBindingModel.Improved);

			Surface = new VeldridSurface(backend, options);
			Surface.Size = new Eto.Drawing.Size(200, 200);

			TableLayout tl = new();
			tl.Rows.Add(new());
			TextBox tb = new();
			tl.Rows[^1].Cells.Add(new() {Control = tb});

			tl.Rows.Add(new() {ScaleHeight = true});
			tl.Rows[^1].Cells.Add(new() {Control = Surface});

			Content = tl;

			ovpSettings = new OVPSettings();
			addPolys();

			Driver = new VeldridDriver(ref ovpSettings, ref Surface);

			Surface.VeldridInitialized += (sender, e) =>
			{
				Driver.SetUpVeldrid();

				VeldridReady = true;
			};
		}

		private void addPolys()
		{
			ovpSettings.clear();

			float r = 0.0f;

			PointF[] testPoly = new PointF[6];
			testPoly[0] = new PointF(2.0f + r, 2.0f + r);
			testPoly[1] = new PointF(15.0f + r, 12.0f + r);
			testPoly[2] = new PointF(8.0f + r, 24.0f + r);
			testPoly[3] = new PointF(8.0f + r, 15.0f + r);
			testPoly[4] = new PointF(3.0f + r, 2.0f + r);
			testPoly[5] = new PointF(2.0f + r, 2.0f + r);


			PointF[] testPoly2 = new PointF[6];
			testPoly2[0] = new PointF(12.0f + r, 2.0f + r);
			testPoly2[1] = new PointF(25.0f + r, 12.0f + r);
			testPoly2[2] = new PointF(18.0f + r, 24.0f + r);
			testPoly2[3] = new PointF(18.0f + r, 15.0f + r);
			testPoly2[4] = new PointF(13.0f + r, 2.0f + r);
			testPoly2[5] = new PointF(12.0f + r, 2.0f + r);

			ovpSettings.addPolygon(testPoly2, Color.FromArgb(0, 255, 255), 1.0f, true, 1);

			ovpSettings.addPolygon(testPoly, Color.FromArgb(255, 0, 0), 1.0f, true, 2);

			r = -30.0f;

			PointF[] testPolyF1 = new PointF[6];
			testPolyF1[0] = new PointF(2.0f + r, 2.0f + r);
			testPolyF1[1] = new PointF(15.0f + r, 12.0f + r);
			testPolyF1[2] = new PointF(8.0f + r, 24.0f + r);
			testPolyF1[3] = new PointF(8.0f + r, 15.0f + r);
			testPolyF1[4] = new PointF(3.0f + r, 2.0f + r);
			testPolyF1[5] = new PointF(2.0f + r, 2.0f + r);

			PointF[] testPolyF2 = new PointF[6];
			testPolyF2[0] = new PointF(12.0f + r, 2.0f + r);
			testPolyF2[1] = new PointF(25.0f + r, 12.0f + r);
			testPolyF2[2] = new PointF(18.0f + r, 24.0f + r);
			testPolyF2[3] = new PointF(18.0f + r, 15.0f + r);
			testPolyF2[4] = new PointF(13.0f + r, 2.0f + r);
			testPolyF2[5] = new PointF(12.0f + r, 2.0f + r);


			ovpSettings.addPolygon(testPolyF2, Color.FromArgb(0, 255, 255), 0.5f, false, 3);

			ovpSettings.addPolygon(testPolyF1, Color.FromArgb(255, 0, 0), 1.0f, false, 4);

			r = 30.0f;

			PointF[] testPolyBG1 = new PointF[6];
			testPolyBG1[0] = new PointF(2.0f + r, 2.0f + r);
			testPolyBG1[1] = new PointF(15.0f + r, 12.0f + r);
			testPolyBG1[2] = new PointF(8.0f + r, 24.0f + r);
			testPolyBG1[3] = new PointF(8.0f + r, 15.0f + r);
			testPolyBG1[4] = new PointF(3.0f + r, 2.0f + r);
			testPolyBG1[5] = new PointF(2.0f + r, 2.0f + r);

			PointF[] testPolyBG2 = new PointF[6];
			testPolyBG2[0] = new PointF(12.0f + r, 2.0f + r);
			testPolyBG2[1] = new PointF(25.0f + r, 12.0f + r);
			testPolyBG2[2] = new PointF(18.0f + r, 24.0f + r);
			testPolyBG2[3] = new PointF(18.0f + r, 15.0f + r);
			testPolyBG2[4] = new PointF(13.0f + r, 2.0f + r);
			testPolyBG2[5] = new PointF(12.0f + r, 2.0f + r);


			ovpSettings.addBGPolygon(testPolyBG2, Color.FromArgb(0, 255, 255), 1.0f, 3);

			ovpSettings.addBGPolygon(testPolyBG1, Color.FromArgb(255, 0, 0), 1.0f, 4);
		}


	private ContextMenu vp_menu;

	private void createVPContextMenu()
	{
		// Single viewport now mandates regeneration of the context menu each time, to allow for entry screening.
		vp_menu = new ContextMenu();

		int itemIndex = 0;
		vp_menu.Items.Add(new ButtonMenuItem { Text = "Reset" });
		vp_menu.Items[itemIndex].Click += delegate
		{
			Driver.reset();
			updateViewport();
		};
		itemIndex++;

		ButtonMenuItem VPMenuDisplayOptionsMenu = vp_menu.Items.GetSubmenu("Display Options");
		itemIndex++;
		int displayOptionsSubItemIndex = 0;
		VPMenuDisplayOptionsMenu.Items.Add(new ButtonMenuItem { Text = "Toggle AA" });
		VPMenuDisplayOptionsMenu.Items[displayOptionsSubItemIndex].Click += delegate
		{
			ovpSettings.aA(!ovpSettings.aA());
			updateViewport();
		};
		displayOptionsSubItemIndex++;
		VPMenuDisplayOptionsMenu.Items.Add(new ButtonMenuItem { Text = "Toggle Fill" });
		VPMenuDisplayOptionsMenu.Items[displayOptionsSubItemIndex].Click += delegate
		{
			ovpSettings.drawFilled(!ovpSettings.drawFilled());
			updateViewport();
		};
		displayOptionsSubItemIndex++;
		VPMenuDisplayOptionsMenu.Items.Add(new ButtonMenuItem { Text = "Toggle Points" });
		VPMenuDisplayOptionsMenu.Items[displayOptionsSubItemIndex].Click += delegate
		{
			ovpSettings.drawPoints(!ovpSettings.drawPoints());
			updateViewport();
		};
		displayOptionsSubItemIndex++;

		{
			if (Driver.ovpSettings.isLocked())
			{
				vp_menu.Items.Add(new ButtonMenuItem { Text = "Thaw" });
			}
			else
			{
				vp_menu.Items.Add(new ButtonMenuItem { Text = "Freeze" });
			}
			vp_menu.Items[itemIndex].Click += delegate
			{
				Driver.freeze_thaw();
				updateViewport();
			};
			itemIndex++;
			vp_menu.Items.AddSeparator();
			itemIndex++;
			vp_menu.Items.Add(new ButtonMenuItem { Text = "Save bookmark" });
			vp_menu.Items[itemIndex].Click += delegate
			{
				Driver.saveLocation();
			};
			itemIndex++;
			vp_menu.Items.Add(new ButtonMenuItem { Text = "Load bookmark" });
			vp_menu.Items[itemIndex].Click += delegate
			{
				Driver.loadLocation();
			};
			vp_menu.Items[itemIndex].Enabled = Driver.savedLocation_valid switch
			{
				false => false,
				_ => vp_menu.Items[itemIndex].Enabled
			};
			itemIndex++;
		}
		vp_menu.Items.AddSeparator();
		itemIndex++;
		vp_menu.Items.Add(new ButtonMenuItem { Text = "Zoom Extents" });
		vp_menu.Items[itemIndex].Click += delegate
		{
			Driver.zoomExtents(0);
		};
		itemIndex++;
		vp_menu.Items.AddSeparator();
		itemIndex++;
		vp_menu.Items.Add(new ButtonMenuItem { Text = "Zoom In" });
		vp_menu.Items[itemIndex].Click += delegate
		{
			Driver.zoomIn(-1);
			updateViewport();
		};
		itemIndex++;

		ButtonMenuItem VPMenuZoomInMenu = vp_menu.Items.GetSubmenu("Fast Zoom In");
		itemIndex++;
		int zoomInSubItemIndex = 0;
		VPMenuZoomInMenu.Items.Add(new ButtonMenuItem { Text = "Zoom In (x5)" });
		VPMenuZoomInMenu.Items[zoomInSubItemIndex].Click += delegate
		{
			Driver.zoomIn(-50);
			updateViewport();
		};
		zoomInSubItemIndex++;
		VPMenuZoomInMenu.Items.Add(new ButtonMenuItem { Text = "Zoom In (x10)" });
		VPMenuZoomInMenu.Items[zoomInSubItemIndex].Click += delegate
		{
			Driver.zoomIn(-100);
			updateViewport();
		};
		zoomInSubItemIndex++;
		VPMenuZoomInMenu.Items.Add(new ButtonMenuItem { Text = "Zoom In (x50)" });
		VPMenuZoomInMenu.Items[zoomInSubItemIndex].Click += delegate
		{
			Driver.zoomIn(-500);
			updateViewport();
		};
		zoomInSubItemIndex++;
		VPMenuZoomInMenu.Items.Add(new ButtonMenuItem { Text = "Zoom In (x100)" });
		VPMenuZoomInMenu.Items[zoomInSubItemIndex].Click += delegate
		{
			Driver.zoomIn(-1000);
			updateViewport();
		};
		zoomInSubItemIndex++;

		vp_menu.Items.AddSeparator();
		itemIndex++;

		vp_menu.Items.Add(new ButtonMenuItem { Text = "Zoom Out" });
		vp_menu.Items[itemIndex].Click += delegate
		{
			Driver.zoomOut(-1);
			updateViewport();
		};
		itemIndex++;

		ButtonMenuItem VPMenuZoomOutMenu = vp_menu.Items.GetSubmenu("Fast Zoom Out");
		itemIndex++;
		int zoomOutSubItemIndex = 0;
		VPMenuZoomOutMenu.Items.Add(new ButtonMenuItem { Text = "Zoom Out (x5)" });
		VPMenuZoomOutMenu.Items[zoomOutSubItemIndex].Click += delegate
		{
			Driver.zoomOut(-50);
			updateViewport();
		};
		zoomOutSubItemIndex++;
		VPMenuZoomOutMenu.Items.Add(new ButtonMenuItem { Text = "Zoom Out (x10)" });
		VPMenuZoomOutMenu.Items[zoomOutSubItemIndex].Click += delegate
		{
			Driver.zoomOut(-100);
			updateViewport();
		};
		zoomOutSubItemIndex++;
		VPMenuZoomOutMenu.Items.Add(new ButtonMenuItem { Text = "Zoom Out (x50)" });
		VPMenuZoomOutMenu.Items[zoomOutSubItemIndex].Click += delegate
		{
			Driver.zoomOut(-500);
			updateViewport();
		};
		zoomOutSubItemIndex++;
		VPMenuZoomOutMenu.Items.Add(new ButtonMenuItem { Text = "Zoom Out (x100)" });
		VPMenuZoomOutMenu.Items[zoomOutSubItemIndex].Click += delegate
		{
			Driver.zoomOut(-1000);
			updateViewport();
		};
		zoomOutSubItemIndex++;

		Driver.setContextMenu(ref vp_menu);
	}

	private void updateViewport()
	{
		Application.Instance.Invoke(() =>
		{
			Monitor.Enter(ovpSettings);
			// Force a geometry re-add here, just to test the fill system. Wouldn't be necessary in real world cases.
			addPolys();

			try
			{
				createVPContextMenu();
				Driver.updateViewport();
			}
			catch (Exception)
			{
			}
			finally
			{
				Monitor.Exit(ovpSettings);
			}
		});
	}

	private void SetUpVeldrid()
		{
			if (!(FormReady && VeldridReady))
			{
				return;
			}

			Title = $"Veldrid backend: {Surface.Backend.ToString()}";

			createVPContextMenu();

			Driver.Clock.Start();
		}
	}
}
