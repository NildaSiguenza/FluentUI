using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
	[DefaultEvent("ColorChanged")]
	[DefaultProperty("SelectedColor")]
	public class FluentColorPicker : FluentControlBase
	{
		private Color selectedColor = Color.Blue;
		private Color originalColor = Color.Blue;

		// 外观
		private ColorBlockShape blockShape = ColorBlockShape.Square;
		private ColorTextFormat textFormat = ColorTextFormat.Hex;
		private int blockSize = 24;
		private int spacing = 8;
		private bool showColorText = true;
		private bool showAlpha = true;

        // 边框相关
        private bool showBorder = true;
        private int borderSize = 1;
        private Color borderColor = Color.Gray;
        private bool useBorderThemeColor = true; 

        // 文本显示
        private Label colorTextLabel;
		private Rectangle colorBlockRect;
		private Rectangle textRect;

		// 下拉面板
		private ColorPickerPanel pickerPanel;
		private ToolStripDropDown dropDown;
		private ToolStripControlHost dropDownHost;
		private bool isDroppedDown = false;

		// 交互状态
		private bool isHovering = false;
		private bool isPressed = false;

		public event EventHandler ColorChanged;
		public event EventHandler<ColorChangingEventArgs> ColorChanging;

		#region 构造函数

		public FluentColorPicker()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
					ControlStyles.UserPaint |
					ControlStyles.ResizeRedraw |
					ControlStyles.OptimizedDoubleBuffer |
					ControlStyles.Selectable, true);

			Size = new Size(150, 32);
			Cursor = Cursors.Hand;

			InitializeComponents();
			UpdateLayout();
		}

		private void InitializeComponents()
		{
			// 创建颜色文本标签(虚拟, 用于测量)
			colorTextLabel = new Label
			{
				AutoSize = true,
				Visible = false
			};

			// 创建颜色选择面板
			pickerPanel = new ColorPickerPanel();
			pickerPanel.ShowAlpha = showAlpha;
			pickerPanel.ColorChanged += (s, e) =>
			{
				SetColor(pickerPanel.SelectedColor, false);
			};
			pickerPanel.ColorConfirmed += (s, e) =>
			{
				SetColor(pickerPanel.SelectedColor, true);
				CloseDropDown();
			};
		}

		#endregion

		#region 属性

		[Category("Color")]
		[Description("选中的颜色")]
		public Color SelectedColor
		{
			get => selectedColor;
			set => SetColor(value, true);
		}

		[Category("Appearance")]
		[DefaultValue(ColorBlockShape.Square)]
		[Description("颜色块形状")]
		public ColorBlockShape BlockShape
		{
			get => blockShape;
			set
			{
				if (blockShape != value)
				{
					blockShape = value;
					UpdateLayout();
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(ColorTextFormat.Hex)]
		[Description("颜色文本格式")]
		public ColorTextFormat TextFormat
		{
			get => textFormat;
			set
			{
				if (textFormat != value)
				{
					textFormat = value;
					UpdateLayout();
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(24)]
		[Description("颜色块大小")]
		public int BlockSize
		{
			get => blockSize;
			set
			{
				if (blockSize != value && value > 0)
				{
					blockSize = value;
					UpdateLayout();
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("是否显示颜色文本")]
		public bool ShowColorText
		{
			get => showColorText;
			set
			{
				if (showColorText != value)
				{
					showColorText = value;
					UpdateLayout();
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(true)]
		[Description("是否显示Alpha通道")]
		public bool ShowAlpha
		{
			get => showAlpha;
			set
			{
				if (showAlpha != value)
				{
					showAlpha = value;
					if (pickerPanel != null)
					{
						pickerPanel.ShowAlpha = value;
					}
					UpdateLayout();
					Invalidate();
				}
			}
		}

        [Category("Border")]
        [DefaultValue(true)]
        [Description("是否显示边框")]
        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                if (showBorder != value)
                {
                    showBorder = value;
                    Invalidate();
                }
            }
        }

        [Category("Border")]
        [DefaultValue(1)]
        [Description("边框粗细")]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                if (borderSize != value && value > 0 && value <= 5)
                {
                    borderSize = value;
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Border")]
        [DefaultValue(typeof(Color), "Gray")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    useBorderThemeColor = false;
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Border")]
        [DefaultValue(true)]
        [Description("是否使用主题边框颜色")]
        public bool UseBorderThemeColor
        {
            get => useBorderThemeColor;
            set
            {
                if (useBorderThemeColor != value)
                {
                    useBorderThemeColor = value;
                    if (showBorder)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [Category("Appearance")]
		[DefaultValue(8)]
		[Description("颜色块与文本的间距")]
		public int Spacing
		{
			get => spacing;
			set
			{
				if (spacing != value && value >= 0)
				{
					spacing = value;
					UpdateLayout();
					Invalidate();
				}
			}
		}

		#endregion

		#region 事件

		protected virtual void OnColorChanged()
		{
			ColorChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnColorChanging(ColorChangingEventArgs e)
		{
			ColorChanging?.Invoke(this, e);
		}

		#endregion

		#region 颜色管理

		private void SetColor(Color color, bool raiseEvent)
		{
			if (selectedColor != color)
			{
				if (raiseEvent)
				{
					var args = new ColorChangingEventArgs(selectedColor, color);
					OnColorChanging(args);
					if (args.Cancel)
					{
						return;
					}
				}

				selectedColor = color;
				UpdateLayout();
				Invalidate();

				if (raiseEvent)
				{
					OnColorChanged();
				}
			}
		}

		private string GetColorText()
		{
			switch (textFormat)
			{
				case ColorTextFormat.Hex:
					if (showAlpha && selectedColor.A < 255)
					{
						return $"#{selectedColor.A:X2}{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
					}
					else
					{
						return $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
					}

				case ColorTextFormat.RGB:
					if (showAlpha && selectedColor.A < 255)
					{
						return $"RGBA({selectedColor.R}, {selectedColor.G}, {selectedColor.B}, {selectedColor.A})";
					}
					else
					{
						return $"RGB({selectedColor.R}, {selectedColor.G}, {selectedColor.B})";
					}

				case ColorTextFormat.ARGB:
					if (showAlpha)
					{
						return $"A:{selectedColor.A} R:{selectedColor.R} G:{selectedColor.G} B:{selectedColor.B}";
					}
					else
					{
						return $"R:{selectedColor.R} G:{selectedColor.G} B:{selectedColor.B}";
					}

				default:
					return selectedColor.Name;
			}
		}

		#endregion

		#region 布局

		private void UpdateLayout()
		{
			// 计算颜色块位置
			int yPos = (Height - blockSize) / 2;
			colorBlockRect = new Rectangle(8, yPos, blockSize, blockSize);

			// 计算文本位置
			if (showColorText)
			{
				string colorText = GetColorText();
				Size textSize = TextRenderer.MeasureText(colorText, Font);

				textRect = new Rectangle(
					colorBlockRect.Right + spacing,
					(Height - textSize.Height) / 2,
					Width - colorBlockRect.Right - spacing - 8,
					textSize.Height);
			}
			else
			{
				textRect = Rectangle.Empty;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			UpdateLayout();
		}

		#endregion

		#region 鼠标事件

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			isHovering = true;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			isHovering = false;
			Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				isPressed = true;
				ShowDropDown();
				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			isPressed = false;
			Invalidate();
		}

		#endregion

		#region 下拉面板

		private void ShowDropDown()
		{
			if (isDroppedDown)
			{
				return;
			}

			if (dropDown == null)
			{
				dropDown = new ToolStripDropDown();
				dropDown.AutoSize = false;
				dropDown.Margin = Padding.Empty;
				dropDown.Padding = Padding.Empty;

				dropDownHost = new ToolStripControlHost(pickerPanel);
				dropDownHost.Margin = Padding.Empty;
				dropDownHost.Padding = Padding.Empty;
				dropDownHost.AutoSize = false;

				dropDown.Items.Add(dropDownHost);
			}

			// 保存原始颜色
			originalColor = selectedColor;
			pickerPanel.SetColor(selectedColor);

			// 设置面板大小
			Size panelSize = pickerPanel.GetPreferredSize();
			dropDownHost.Size = panelSize;
			dropDown.Size = new Size(panelSize.Width + 2, panelSize.Height + 2);

			// 显示下拉面板
			dropDown.Show(this, new Point(0, Height));
			isDroppedDown = true;

			dropDown.Closed += (s, e) =>
			{
				isDroppedDown = false;
				isPressed = false;
				Invalidate();
			};
		}

		private void CloseDropDown()
		{
			if (dropDown != null && isDroppedDown)
			{
				dropDown.Close();
			}
		}

		#endregion

		#region 绘制

		protected override void DrawBackground(Graphics g)
		{
			var rect = ClientRectangle;
			Color bgColor = BackColor;

			if (UseTheme && Theme != null)
			{
				if (isPressed || isDroppedDown)
				{
					bgColor = GetThemeColor(c => c.SurfacePressed, bgColor);
				}
				else if (isHovering)
				{
					bgColor = GetThemeColor(c => c.SurfaceHover, bgColor);
				}
				else
				{
					bgColor = GetThemeColor(c => c.Surface, bgColor);
				}
			}

			using (var brush = new SolidBrush(bgColor))
			{
				if (UseTheme && Theme?.Elevation?.CornerRadius > 0)
				{
					using (var path = GetRoundedRectangle(rect, Theme.Elevation.CornerRadius))
					{
						g.FillPath(brush, path);
					}
				}
				else
				{
					g.FillRectangle(brush, rect);
				}
			}
		}

		protected override void DrawContent(Graphics g)
		{
			// 绘制颜色块
			DrawColorBlock(g);

			// 绘制文本
			if (showColorText && !textRect.IsEmpty)
			{
				DrawColorText(g);
			}

			// 绘制下拉箭头
			DrawDropDownArrow(g);
		}

		private void DrawColorBlock(Graphics g)
		{
			// 先绘制棋盘格背景(用于显示透明度)
			DrawCheckerboard(g, colorBlockRect);

			// 绘制颜色
			using (var brush = new SolidBrush(selectedColor))
			{
				switch (blockShape)
				{
					case ColorBlockShape.Circle:
						g.FillEllipse(brush, colorBlockRect);
						break;

					case ColorBlockShape.Square:
						g.FillRectangle(brush, colorBlockRect);
						break;

					case ColorBlockShape.RoundedSquare:
						using (var path = GetRoundedRectangle(colorBlockRect, 4))
						{
							g.FillPath(brush, path);
						}
						break;
				}
			}
		}

		private void DrawCheckerboard(Graphics g, Rectangle rect)
		{
			const int checkSize = 4;
			using (var lightBrush = new SolidBrush(Color.White))
			using (var darkBrush = new SolidBrush(Color.LightGray))
			{
				// 先填充白色背景
				g.FillRectangle(lightBrush, rect);

				// 绘制灰色方块
				for (int y = rect.Top; y < rect.Bottom; y += checkSize)
				{
					for (int x = rect.Left; x < rect.Right; x += checkSize)
					{
						if ((x - rect.Left) / checkSize % 2 == (y - rect.Top) / checkSize % 2)
						{
							var checkRect = new Rectangle(x, y,
								Math.Min(checkSize, rect.Right - x),
								Math.Min(checkSize, rect.Bottom - y));
							g.FillRectangle(darkBrush, checkRect);
						}
					}
				}
			}

			// 裁剪到形状
			var oldClip = g.Clip;
			switch (blockShape)
			{
				case ColorBlockShape.Circle:
					using (var path = new GraphicsPath())
					{
						path.AddEllipse(rect);
						g.SetClip(path, CombineMode.Intersect);
					}
					break;

				case ColorBlockShape.RoundedSquare:
					using (var path = GetRoundedRectangle(rect, 4))
					{
						g.SetClip(path, CombineMode.Intersect);
					}
					break;
			}
			g.Clip = oldClip;
		}

		private void DrawColorText(Graphics g)
		{
			string text = GetColorText();
			Color textColor = ForeColor;

			if (UseTheme && Theme != null)
			{
				textColor = GetThemeColor(c => c.TextPrimary, textColor);
			}

			TextRenderer.DrawText(g, text, Font, textRect, textColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
		}

		private void DrawDropDownArrow(Graphics g)
		{
			int arrowSize = 5;
			int arrowX = Width - arrowSize - 10;
			int arrowY = Height / 2;

			Color arrowColor = ForeColor;
			if (UseTheme && Theme != null)
			{
				arrowColor = GetThemeColor(c => c.TextSecondary, arrowColor);
			}

			Point[] arrow = new Point[]
			{
				new Point(arrowX - arrowSize, arrowY - 2),
				new Point(arrowX + arrowSize, arrowY - 2),
				new Point(arrowX, arrowY + 3)
			};

			using (var brush = new SolidBrush(arrowColor))
			{
				g.FillPolygon(brush, arrow);
			}
		}

		protected override void DrawBorder(Graphics g)
		{
            if (!showBorder)
            {
                return;
            }

            // 调整矩形以适应不同的边框粗细
            int offset = borderSize / 2;
            var rect = new Rectangle(offset, offset, Width - borderSize, Height - borderSize);

            Color actualBorderColor = borderColor;

            // 如果使用主题颜色
            if (useBorderThemeColor && UseTheme && Theme != null)
            {
                if (isDroppedDown)
                {
                    actualBorderColor = GetThemeColor(c => c.Primary, Color.Blue);
                }
                else if (isHovering)
                {
                    actualBorderColor = GetThemeColor(c => c.BorderFocused, Color.DarkGray);
                }
                else
                {
                    actualBorderColor = GetThemeColor(c => c.Border, Color.Gray);
                }
            }

            // 下拉时可以使用更粗的边框
            int actualBorderSize = (isDroppedDown && useBorderThemeColor) ? Math.Min(borderSize + 1, 3) : borderSize;

            using (var pen = new Pen(actualBorderColor, actualBorderSize))
            {
                if (UseTheme && Theme?.Elevation?.CornerRadius > 0)
                {
                    using (var path = GetRoundedRectangle(rect, Theme.Elevation.CornerRadius))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        public void ResetBorderColor()
        {
            borderColor = Color.Gray;
            useBorderThemeColor = true;
            if (showBorder)
            {
                Invalidate();
            }
        }

        #endregion

        #region 主题

        protected override void OnThemeChanged()
		{
			base.OnThemeChanged();
		}

		protected override void ApplyThemeStyles()
		{
			if (Theme == null)
			{
				return;
			}

			BackColor = GetThemeColor(c => c.Surface, BackColor);
			ForeColor = GetThemeColor(c => c.TextPrimary, ForeColor);
			Font = GetThemeFont(t => t.Body, Font);
		}

		#endregion

		#region 清理

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				dropDown?.Dispose();
				pickerPanel?.Dispose();
				colorTextLabel?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}

	#region 颜色选择面板

	internal class ColorPickerPanel : UserControl
	{
		private Color selectedColor = Color.Blue;
		private bool showAlpha = true;

		// 滑块控件
		private TrackBar alphaSlider;
		private TrackBar redSlider;
		private TrackBar greenSlider;
		private TrackBar blueSlider;

		// 数值输入框
		private NumericUpDown alphaUpDown;
		private NumericUpDown redUpDown;
		private NumericUpDown greenUpDown;
		private NumericUpDown blueUpDown;

		// 标签
		private Label alphaLabel;
		private Label redLabel;
		private Label greenLabel;
		private Label blueLabel;

		// 预览区域
		private Panel previewPanel;
		private Panel originalColorPanel;
		private Panel newColorPanel;

		// 十六进制输入
		private TextBox hexTextBox;
		private Label hexLabel;

		// 按钮
		private Button okButton;
		private Button cancelButton;

		// 预设颜色
		private Panel presetColorsPanel;
		private Color[] presetColors = new Color[]
		{
			Color.Red, Color.Green, Color.Blue, Color.Yellow,
			Color.Cyan, Color.Magenta, Color.Orange, Color.Purple,
			Color.Pink, Color.Brown, Color.Gray, Color.Black,
			Color.White, Color.LightGray, Color.DarkGray, Color.Transparent
		};

		public event EventHandler ColorChanged;
		public event EventHandler ColorConfirmed;

		public Color SelectedColor
		{
			get => selectedColor;
			set => SetColor(value);
		}

		public bool ShowAlpha
		{
			get => showAlpha;
			set
			{
				showAlpha = value;
				UpdateAlphaVisibility();
			}
		}

		public ColorPickerPanel()
		{
			InitializeComponents();
			UpdateFromColor();
		}

		private void InitializeComponents()
		{
			BackColor = Color.White;
			BorderStyle = BorderStyle.None;
			Size = new Size(320, 400);

			int yPos = 10;

			// 预览区域
			var previewLabel = new Label
			{
				Text = "颜色预览",
				Location = new Point(10, yPos),
				AutoSize = true
			};
			Controls.Add(previewLabel);
			yPos += 25;

			previewPanel = new Panel
			{
				Location = new Point(10, yPos),
				Size = new Size(300, 40),
				BorderStyle = BorderStyle.FixedSingle
			};

			originalColorPanel = new Panel
			{
				Location = new Point(0, 0),
				Size = new Size(150, 40),
				BackColor = selectedColor
			};

			newColorPanel = new Panel
			{
				Location = new Point(150, 0),
				Size = new Size(150, 40),
				BackColor = selectedColor
			};

			previewPanel.Controls.Add(originalColorPanel);
			previewPanel.Controls.Add(newColorPanel);
			Controls.Add(previewPanel);
			yPos += 50;

			// Alpha通道
			alphaLabel = new Label
			{
				Text = "A:",
				Location = new Point(10, yPos + 3),
				Size = new Size(20, 20)
			};

			alphaSlider = new TrackBar
			{
				Location = new Point(35, yPos),
				Size = new Size(200, 30),
				Minimum = 0,
				Maximum = 255,
				Value = 255,
				TickFrequency = 16
			};
			alphaSlider.ValueChanged += (s, e) => OnSliderChanged();

			alphaUpDown = new NumericUpDown
			{
				Location = new Point(240, yPos),
				TextAlign = HorizontalAlignment.Center,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("Microsoft YaHei", 9f),
				Size = new Size(60, 22),
				Minimum = 0,
				Maximum = 255,
				Value = 255
			};
			alphaUpDown.ValueChanged += (s, e) => OnUpDownChanged();

			Controls.AddRange(new Control[] { alphaLabel, alphaSlider, alphaUpDown });
			yPos += 35;

			// Red通道
			redLabel = new Label
			{
				Text = "R:",
				Location = new Point(10, yPos + 3),
				Size = new Size(20, 20)
			};

			redSlider = new TrackBar
			{
				Location = new Point(35, yPos),
				Size = new Size(200, 30),
				Minimum = 0,
				Maximum = 255,
				Value = 0,
				TickFrequency = 16
			};
			redSlider.ValueChanged += (s, e) => OnSliderChanged();

			redUpDown = new NumericUpDown
			{
				Location = new Point(240, yPos),
				TextAlign = HorizontalAlignment.Center,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("Microsoft YaHei", 9f),
				Size = new Size(60, 22),
				Minimum = 0,
				Maximum = 255,
				Value = 0
			};
			redUpDown.ValueChanged += (s, e) => OnUpDownChanged();

			Controls.AddRange(new Control[] { redLabel, redSlider, redUpDown });
			yPos += 35;

			// Green通道
			greenLabel = new Label
			{
				Text = "G:",
				Location = new Point(10, yPos + 3),
				Size = new Size(20, 20)
			};

			greenSlider = new TrackBar
			{
				Location = new Point(35, yPos),
				Size = new Size(200, 30),
				Minimum = 0,
				Maximum = 255,
				Value = 0,
				TickFrequency = 16
			};
			greenSlider.ValueChanged += (s, e) => OnSliderChanged();

			greenUpDown = new NumericUpDown
			{
				Location = new Point(240, yPos),
				TextAlign = HorizontalAlignment.Center,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("Microsoft YaHei", 9f),
				Size = new Size(60, 22),
				Minimum = 0,
				Maximum = 255,
				Value = 0
			};
			greenUpDown.ValueChanged += (s, e) => OnUpDownChanged();

			Controls.AddRange(new Control[] { greenLabel, greenSlider, greenUpDown });
			yPos += 35;

			// Blue通道
			blueLabel = new Label
			{
				Text = "B:",
				Location = new Point(10, yPos + 3),
				Size = new Size(20, 20)
			};

			blueSlider = new TrackBar
			{
				Location = new Point(35, yPos),
				Size = new Size(200, 30),
				Minimum = 0,
				Maximum = 255,
				Value = 0,
				TickFrequency = 16
			};
			blueSlider.ValueChanged += (s, e) => OnSliderChanged();

			blueUpDown = new NumericUpDown
			{
				Location = new Point(240, yPos),
				TextAlign = HorizontalAlignment.Center,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("Microsoft YaHei", 9f),
				Size = new Size(60, 22),
				Minimum = 0,
				Maximum = 255,
				Value = 0
			};
			blueUpDown.ValueChanged += (s, e) => OnUpDownChanged();

			Controls.AddRange(new Control[] { blueLabel, blueSlider, blueUpDown });
			yPos += 40;

			// 十六进制输入
			hexLabel = new Label
			{
				Text = "Hex:",
				Location = new Point(10, yPos + 5),
				AutoSize = true
			};

			hexTextBox = new TextBox
			{
				Location = new Point(45, yPos + 1),
				Font = new Font("Microsoft YaHei", 8.75f),
				Size = new Size(100, 22),
				Text = "#000000"
			};
			hexTextBox.TextChanged += (s, e) => OnHexChanged();

			Controls.AddRange(new Control[] { hexLabel, hexTextBox });
			yPos += 35;

			// 预设颜色
			var presetLabel = new Label
			{
				Text = "预设颜色",
				Location = new Point(10, yPos),
				AutoSize = true
			};
			Controls.Add(presetLabel);
			yPos += 25;

			presetColorsPanel = new Panel
			{
				Location = new Point(10, yPos),
				Size = new Size(300, 70),
				BorderStyle = BorderStyle.None
			};

			int colorIndex = 0;
			for (int row = 0; row < 2; row++)
			{
				for (int col = 0; col < 8; col++)
				{
					if (colorIndex >= presetColors.Length)
					{
						break;
					}

					var colorPanel = new Panel
					{
						Location = new Point(col * 37, row * 30),
						Size = new Size(24, 24),
						BackColor = presetColors[colorIndex],
						BorderStyle = BorderStyle.FixedSingle,
						Cursor = Cursors.Hand,
						Tag = presetColors[colorIndex]
					};

					int index = colorIndex;
					colorPanel.Click += (s, e) =>
					{
						SetColor((Color)colorPanel.Tag);
						UpdateFromColor();
					};

					presetColorsPanel.Controls.Add(colorPanel);
					colorIndex++;
				}
			}

			Controls.Add(presetColorsPanel);
			yPos += 70;

			// 按钮
			okButton = new Button
			{
				Text = "确认",
				Location = new Point(150, yPos),
				Size = new Size(75, 25),
				FlatStyle = FlatStyle.Flat
			};
			okButton.Click += (s, e) => OnColorConfirmed();

			cancelButton = new Button
			{
				Text = "取消",
				Location = new Point(235, yPos),
				Size = new Size(75, 25),
				FlatStyle = FlatStyle.Flat
			};
			cancelButton.Click += (s, e) => Parent?.Hide();

			Controls.AddRange(new Control[] { okButton, cancelButton });

			UpdateAlphaVisibility();
		}

		private void UpdateAlphaVisibility()
		{
			bool visible = showAlpha;
			alphaLabel.Visible = visible;
			alphaSlider.Visible = visible;
			alphaUpDown.Visible = visible;

			// 调整其他控件位置
			if (!visible)
			{
				int yOffset = 35;
				AdjustControlPosition(redLabel, redSlider, redUpDown, -yOffset);
				AdjustControlPosition(greenLabel, greenSlider, greenUpDown, -yOffset);
				AdjustControlPosition(blueLabel, blueSlider, blueUpDown, -yOffset);
			}
		}

		private void AdjustControlPosition(Label label, TrackBar slider, NumericUpDown upDown, int yOffset)
		{
			label.Top += yOffset;
			slider.Top += yOffset;
			upDown.Top += yOffset;
		}

		private bool updating = false;

		private void OnSliderChanged()
		{
			if (updating)
			{
				return;
			}

			updating = true;
			alphaUpDown.Value = alphaSlider.Value;
			redUpDown.Value = redSlider.Value;
			greenUpDown.Value = greenSlider.Value;
			blueUpDown.Value = blueSlider.Value;

			UpdateColor();
			updating = false;
		}

		private void OnUpDownChanged()
		{
			if (updating)
			{
				return;
			}

			updating = true;
			alphaSlider.Value = (int)alphaUpDown.Value;
			redSlider.Value = (int)redUpDown.Value;
			greenSlider.Value = (int)greenUpDown.Value;
			blueSlider.Value = (int)blueUpDown.Value;

			UpdateColor();
			updating = false;
		}

		private void OnHexChanged()
		{
			if (updating)
			{
				return;
			}

			try
			{
				string hex = hexTextBox.Text.TrimStart('#');
				if (hex.Length == 6)
				{
					int rgb = Convert.ToInt32(hex, 16);
					Color color = Color.FromArgb(
						(rgb >> 16) & 0xFF,
						(rgb >> 8) & 0xFF,
						rgb & 0xFF);

					if (showAlpha)
					{
						color = Color.FromArgb((int)alphaUpDown.Value, color);
					}

					SetColor(color);
					UpdateFromColor();
				}
				else if (hex.Length == 8 && showAlpha)
				{
					uint argb = Convert.ToUInt32(hex, 16);
					Color color = Color.FromArgb((int)argb);
					SetColor(color);
					UpdateFromColor();
				}
			}
			catch { }
		}

		private void UpdateColor()
		{
			selectedColor = Color.FromArgb(
				showAlpha ? (int)alphaUpDown.Value : 255,
				(int)redUpDown.Value,
				(int)greenUpDown.Value,
				(int)blueUpDown.Value);

			newColorPanel.BackColor = selectedColor;
			UpdateHexText();
			OnColorChanged();
		}

		private void UpdateFromColor()
		{
			updating = true;

			alphaSlider.Value = selectedColor.A;
			alphaUpDown.Value = selectedColor.A;
			redSlider.Value = selectedColor.R;
			redUpDown.Value = selectedColor.R;
			greenSlider.Value = selectedColor.G;
			greenUpDown.Value = selectedColor.G;
			blueSlider.Value = selectedColor.B;
			blueUpDown.Value = selectedColor.B;

			newColorPanel.BackColor = selectedColor;
			UpdateHexText();

			updating = false;
		}

		private void UpdateHexText()
		{
			if (showAlpha && selectedColor.A < 255)
			{
				hexTextBox.Text = $"#{selectedColor.A:X2}{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
			}
			else
			{
				hexTextBox.Text = $"#{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";
			}
		}

		public void SetColor(Color color)
		{
			selectedColor = color;
			originalColorPanel.BackColor = color;
			UpdateFromColor();
		}

		public Size GetPreferredSize()
		{
			return showAlpha ? new Size(320, 395) : new Size(320, 360);
		}

		protected virtual void OnColorChanged()
		{
			ColorChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnColorConfirmed()
		{
			ColorConfirmed?.Invoke(this, EventArgs.Empty);
		}
	}

	#endregion

	#region 枚举和辅助类

	/// <summary>
	/// 颜色块形状
	/// </summary>
	public enum ColorBlockShape
	{
		/// <summary>
		/// 正方形
		/// </summary>
		Square,

		/// <summary>
		/// 圆形
		/// </summary>
		Circle,

		/// <summary>
		/// 圆角正方形
		/// </summary>
		RoundedSquare
	}

	/// <summary>
	/// 颜色文本格式
	/// </summary>
	public enum ColorTextFormat
	{
		/// <summary>
		/// 十六进制格式 (#RRGGBB)
		/// </summary>
		Hex,

		/// <summary>
		/// RGB格式 (RGB(R, G, B))
		/// </summary>
		RGB,

		/// <summary>
		/// ARGB格式 (A:255 R:255 G:255 B:255)
		/// </summary>
		ARGB,

		/// <summary>
		/// 颜色名称
		/// </summary>
		Name
	}

	/// <summary>
	/// 颜色变化事件参数
	/// </summary>
	public class ColorChangingEventArgs : EventArgs
	{
		public Color OldColor { get; }
		public Color NewColor { get; }
		public bool Cancel { get; set; }

		public ColorChangingEventArgs(Color oldColor, Color newColor)
		{
			OldColor = oldColor;
			NewColor = newColor;
			Cancel = false;
		}
	}

	#endregion
}
