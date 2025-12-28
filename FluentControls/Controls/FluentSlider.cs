using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Themes;
using System.Windows.Forms.Design;

namespace FluentControls.Controls
{
    [DefaultEvent("ValueChanged")]
    [Designer(typeof(FluentSliderDesigner))]
    [ToolboxItem(true)]
    public class FluentSlider : FluentControlBase
    {
        private double minimum = 0;
        private double maximum = 100;
        private double value = 0;
        private double rangeStart = 0;
        private double rangeEnd = 50;
        private double smallChange = 10;

        private SliderShape thumbShape = SliderShape.Circle;
        private SliderOrientation orientation = SliderOrientation.Horizontal;
        private SliderValueType valueType = SliderValueType.Continuous;

        private bool isRangeMode = false;
        private bool showTickMarks = false;
        private bool showTickText = false;
        private bool showValueTooltip = true;

        private TextBox associatedTextBox;
        private ToolTip valueTooltip;

        private List<SliderMark> marks;

        // 滑块尺寸
        private int thumbSize = 20;
        private int trackHeight = 6;

        // 拖动状态
        private bool isDraggingThumb = false;
        private bool isDraggingRangeStart = false;
        private bool isDraggingRangeEnd = false;
        private Point lastMousePosition;

        // 悬停状态
        private bool isThumbHovered = false;
        private bool isRangeStartHovered = false;
        private bool isRangeEndHovered = false;

        // 布局缓存
        private Rectangle trackRect;
        private Rectangle thumbRect;
        private Rectangle rangeStartRect;
        private Rectangle rangeEndRect;

        public FluentSlider()
        {
            marks = new List<SliderMark>();

            valueTooltip = new ToolTip
            {
                ShowAlways = true,
                InitialDelay = 0,
                ReshowDelay = 0
            };

            Size = new Size(200, 50);

            // 订阅鼠标移动事件以更新tooltip
            MouseMove += OnSliderMouseMove;
        }

        #region 属性

        /// <summary>
        /// 滑块形状
        /// </summary>
        [Category("Slider")]
        [DefaultValue(SliderShape.Circle)]
        [Description("滑块的形状")]
        public SliderShape ThumbShape
        {
            get => thumbShape;
            set
            {
                if (thumbShape != value)
                {
                    thumbShape = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 滑块方向
        /// </summary>
        [Category("Slider")]
        [DefaultValue(SliderOrientation.Horizontal)]
        [Description("滑块的方向")]
        public SliderOrientation Orientation
        {
            get => orientation;
            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 值类型
        /// </summary>
        [Category("Slider")]
        [DefaultValue(SliderValueType.Continuous)]
        [Description("滑块值类型：连续或离散")]
        public SliderValueType ValueType
        {
            get => valueType;
            set
            {
                if (valueType != value)
                {
                    valueType = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 最小值
        /// </summary>
        [Category("Slider")]
        [DefaultValue(0.0)]
        [Description("滑块最小值")]
        public double Minimum
        {
            get => minimum;
            set
            {
                if (minimum != value)
                {
                    minimum = value;
                    if (this.value < minimum)
                    {
                        Value = minimum;
                    }

                    if (rangeStart < minimum)
                    {
                        RangeStart = minimum;
                    }

                    if (rangeEnd < minimum)
                    {
                        RangeEnd = minimum;
                    }

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 最大值
        /// </summary>
        [Category("Slider")]
        [DefaultValue(100.0)]
        [Description("滑块最大值")]
        public double Maximum
        {
            get => maximum;
            set
            {
                if (maximum != value)
                {
                    maximum = value;
                    if (this.value > maximum)
                    {
                        Value = maximum;
                    }

                    if (rangeStart > maximum)
                    {
                        RangeStart = maximum;
                    }

                    if (rangeEnd > maximum)
                    {
                        RangeEnd = maximum;
                    }

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 当前值
        /// </summary>
        [Category("Slider")]
        [DefaultValue(0.0)]
        [Description("滑块当前值")]
        public double Value
        {
            get => value;
            set
            {
                var newValue = ConstrainValue(value);
                if (this.value != newValue)
                {
                    this.value = newValue;
                    UpdateAssociatedTextBox();
                    UpdateLayout();
                    Invalidate();
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 步长（离散模式）
        /// </summary>
        [Category("Slider")]
        [DefaultValue(10.0)]
        [Description("离散模式下的步长")]
        public double SmallChange
        {
            get => smallChange;
            set
            {
                if (smallChange != value && value > 0)
                {
                    smallChange = value;
                    if (valueType == SliderValueType.Discrete)
                    {
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 是否启用范围模式
        /// </summary>
        [Category("Slider")]
        [DefaultValue(false)]
        [Description("是否启用范围选择模式")]
        public bool IsRangeMode
        {
            get => isRangeMode;
            set
            {
                if (isRangeMode != value)
                {
                    isRangeMode = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 范围起始值
        /// </summary>
        [Category("Slider")]
        [DefaultValue(0.0)]
        [Description("范围模式的起始值")]
        public double RangeStart
        {
            get => rangeStart;
            set
            {
                var newValue = ConstrainValue(value);
                if (newValue > rangeEnd)
                {
                    newValue = rangeEnd;
                }

                if (rangeStart != newValue)
                {
                    rangeStart = newValue;
                    UpdateLayout();
                    Invalidate();
                    OnRangeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 范围结束值
        /// </summary>
        [Category("Slider")]
        [DefaultValue(50.0)]
        [Description("范围模式的结束值")]
        public double RangeEnd
        {
            get => rangeEnd;
            set
            {
                var newValue = ConstrainValue(value);
                if (newValue < rangeStart)
                {
                    newValue = rangeStart;
                }

                if (rangeEnd != newValue)
                {
                    rangeEnd = newValue;
                    UpdateLayout();
                    Invalidate();
                    OnRangeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 是否显示刻度标记
        /// </summary>
        [Category("Slider")]
        [DefaultValue(false)]
        [Description("是否显示刻度标记")]
        public bool ShowTickMarks
        {
            get => showTickMarks;
            set
            {
                if (showTickMarks != value)
                {
                    showTickMarks = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示刻度文本
        /// </summary>
        [Category("Slider")]
        [DefaultValue(false)]
        [Description("是否显示刻度文本")]
        public bool ShowTickText
        {
            get => showTickText;
            set
            {
                if (showTickText != value)
                {
                    showTickText = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示值提示
        /// </summary>
        [Category("Slider")]
        [DefaultValue(true)]
        [Description("是否显示鼠标悬停时的值提示")]
        public bool ShowValueTooltip
        {
            get => showValueTooltip;
            set => showValueTooltip = value;
        }

        /// <summary>
        /// 滑块大小
        /// </summary>
        [Category("Slider")]
        [DefaultValue(20)]
        [Description("滑块的大小")]
        public int ThumbSize
        {
            get => thumbSize;
            set
            {
                if (thumbSize != value && value > 0)
                {
                    thumbSize = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 轨道高度
        /// </summary>
        [Category("Slider")]
        [DefaultValue(6)]
        [Description("轨道的高度/宽度")]
        public int TrackHeight
        {
            get => trackHeight;
            set
            {
                if (trackHeight != value && value > 0)
                {
                    trackHeight = value;
                    UpdateLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 关联的文本框
        /// </summary>
        [Category("Slider")]
        [DefaultValue(null)]
        [Description("关联的文本框, 用于显示和输入值")]
        public TextBox AssociatedTextBox
        {
            get => associatedTextBox;
            set
            {
                if (associatedTextBox != value)
                {
                    // 取消旧文本框的订阅
                    if (associatedTextBox != null)
                    {
                        associatedTextBox.TextChanged -= OnAssociatedTextBoxTextChanged;
                        associatedTextBox.KeyPress -= OnAssociatedTextBoxKeyPress;
                    }

                    associatedTextBox = value;

                    // 订阅新文本框的事件
                    if (associatedTextBox != null)
                    {
                        associatedTextBox.TextChanged += OnAssociatedTextBoxTextChanged;
                        associatedTextBox.KeyPress += OnAssociatedTextBoxKeyPress;
                        UpdateAssociatedTextBox();
                    }
                }
            }
        }

        /// <summary>
        /// 标记集合
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<SliderMark> Marks => marks;

        #endregion

        #region 事件

        /// <summary>
        /// 值改变事件
        /// </summary>
        [Category("Slider")]
        [Description("当滑块值改变时触发")]
        public event EventHandler ValueChanged;

        /// <summary>
        /// 范围改变事件
        /// </summary>
        [Category("Slider")]
        [Description("当范围值改变时触发")]
        public event EventHandler RangeChanged;

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected virtual void OnRangeChanged(EventArgs e)
        {
            RangeChanged?.Invoke(this, e);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加标记
        /// </summary>
        public void AddMark(double value, string text = null, Color? color = null, Font font = null)
        {
            var mark = new SliderMark
            {
                Value = value,
                Text = text ?? value.ToString("F1"),
                Color = color ?? Color.Red,
                Font = font ?? Font
            };

            marks.Add(mark);
            Invalidate();
        }

        /// <summary>
        /// 添加标记
        /// </summary>
        public void AddMark(SliderMark mark)
        {
            if (mark != null)
            {
                marks.Add(mark);
                Invalidate();
            }
        }

        /// <summary>
        /// 移除指定值的标记
        /// </summary>
        public void RemoveMark(double value)
        {
            marks.RemoveAll(m => Math.Abs(m.Value - value) < 0.001);
            Invalidate();
        }

        /// <summary>
        /// 清除所有标记
        /// </summary>
        public void ClearMarks()
        {
            marks.Clear();
            Invalidate();
        }

        #endregion

        #region 重写方法

        protected override void OnBoundsChanged()
        {
            base.OnBoundsChanged();
            UpdateLayout();
        }

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                BackColor = GetThemeColor(c => c.Background, SystemColors.Control);
                ForeColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);
            }
        }

        #endregion

        #region 布局计算

        private void UpdateLayout()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            int padding = thumbSize / 2;

            if (orientation == SliderOrientation.Horizontal)
            {
                // 水平方向
                int trackWidth = Width - padding * 2;
                int trackY = (Height - trackHeight) / 2;
                trackRect = new Rectangle(padding, trackY, trackWidth, trackHeight);

                // 计算滑块位置
                if (!isRangeMode)
                {
                    int thumbX = GetPositionFromValue(value);
                    thumbRect = new Rectangle(thumbX - thumbSize / 2, Height / 2 - thumbSize / 2,
                        thumbSize, thumbSize);
                }
                else
                {
                    int startX = GetPositionFromValue(rangeStart);
                    int endX = GetPositionFromValue(rangeEnd);

                    rangeStartRect = new Rectangle(startX - thumbSize / 2, Height / 2 - thumbSize / 2,
                        thumbSize, thumbSize);
                    rangeEndRect = new Rectangle(endX - thumbSize / 2, Height / 2 - thumbSize / 2,
                        thumbSize, thumbSize);
                }
            }
            else
            {
                // 垂直方向
                int trackHeight = Height - padding * 2;
                int trackX = (Width - this.trackHeight) / 2;
                trackRect = new Rectangle(trackX, padding, this.trackHeight, trackHeight);

                // 计算滑块位置
                if (!isRangeMode)
                {
                    int thumbY = GetPositionFromValue(value);
                    thumbRect = new Rectangle(Width / 2 - thumbSize / 2, thumbY - thumbSize / 2,
                        thumbSize, thumbSize);
                }
                else
                {
                    int startY = GetPositionFromValue(rangeStart);
                    int endY = GetPositionFromValue(rangeEnd);

                    rangeStartRect = new Rectangle(Width / 2 - thumbSize / 2, startY - thumbSize / 2,
                        thumbSize, thumbSize);
                    rangeEndRect = new Rectangle(Width / 2 - thumbSize / 2, endY - thumbSize / 2,
                        thumbSize, thumbSize);
                }
            }
        }

        private int GetPositionFromValue(double val)
        {
            if (Maximum <= Minimum)
            {
                return 0;
            }

            double ratio = (val - Minimum) / (Maximum - Minimum);
            int padding = thumbSize / 2;

            if (orientation == SliderOrientation.Horizontal)
            {
                int trackWidth = Width - padding * 2;
                return padding + (int)(trackWidth * ratio);
            }
            else
            {
                int trackHeight = Height - padding * 2;
                // 垂直方向从下到上
                return Height - padding - (int)(trackHeight * ratio);
            }
        }

        private double GetValueFromPosition(int position)
        {
            int padding = thumbSize / 2;
            double ratio;

            if (orientation == SliderOrientation.Horizontal)
            {
                int trackWidth = Width - padding * 2;
                ratio = (double)(position - padding) / trackWidth;
            }
            else
            {
                int trackHeight = Height - padding * 2;
                // 垂直方向从下到上
                ratio = (double)(Height - padding - position) / trackHeight;
            }

            ratio = Math.Max(0, Math.Min(1, ratio));
            return Minimum + ratio * (Maximum - Minimum);
        }

        private double ConstrainValue(double val)
        {
            val = Math.Max(Minimum, Math.Min(Maximum, val));

            if (valueType == SliderValueType.Discrete && smallChange > 0)
            {
                // 对齐到最近的步长
                val = Math.Round((val - Minimum) / smallChange) * smallChange + Minimum;
            }

            return val;
        }

        #endregion

        #region 鼠标处理

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (isRangeMode)
            {
                // 范围模式：检查点击的是哪个滑块
                if (rangeEndRect.Contains(e.Location))
                {
                    isDraggingRangeEnd = true;
                    lastMousePosition = e.Location;
                }
                else if (rangeStartRect.Contains(e.Location))
                {
                    isDraggingRangeStart = true;
                    lastMousePosition = e.Location;
                }
                else if (trackRect.Contains(e.Location))
                {
                    // 点击轨道, 移动最近的滑块
                    double clickValue = GetValueFromPosition(
                        orientation == SliderOrientation.Horizontal ? e.X : e.Y);

                    double distToStart = Math.Abs(clickValue - rangeStart);
                    double distToEnd = Math.Abs(clickValue - rangeEnd);

                    if (distToStart < distToEnd)
                    {
                        RangeStart = clickValue;
                        isDraggingRangeStart = true;
                    }
                    else
                    {
                        RangeEnd = clickValue;
                        isDraggingRangeEnd = true;
                    }
                    lastMousePosition = e.Location;
                }
            }
            else
            {
                // 单值模式
                if (thumbRect.Contains(e.Location))
                {
                    isDraggingThumb = true;
                    lastMousePosition = e.Location;
                }
                else if (trackRect.Contains(e.Location))
                {
                    // 点击轨道, 直接跳转
                    Value = GetValueFromPosition(
                        orientation == SliderOrientation.Horizontal ? e.X : e.Y);
                    isDraggingThumb = true;
                    lastMousePosition = e.Location;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // 更新悬停状态
            bool needsRedraw = false;

            if (!isRangeMode)
            {
                bool oldHovered = isThumbHovered;
                isThumbHovered = thumbRect.Contains(e.Location);
                needsRedraw = oldHovered != isThumbHovered;
            }
            else
            {
                bool oldStartHovered = isRangeStartHovered;
                bool oldEndHovered = isRangeEndHovered;

                isRangeStartHovered = rangeStartRect.Contains(e.Location);
                isRangeEndHovered = rangeEndRect.Contains(e.Location);

                needsRedraw = oldStartHovered != isRangeStartHovered ||
                             oldEndHovered != isRangeEndHovered;
            }

            // 处理拖动
            if (isDraggingThumb)
            {
                Value = GetValueFromPosition(
                    orientation == SliderOrientation.Horizontal ? e.X : e.Y);
                needsRedraw = true;
            }
            else if (isDraggingRangeStart)
            {
                RangeStart = GetValueFromPosition(
                    orientation == SliderOrientation.Horizontal ? e.X : e.Y);
                needsRedraw = true;
            }
            else if (isDraggingRangeEnd)
            {
                RangeEnd = GetValueFromPosition(
                    orientation == SliderOrientation.Horizontal ? e.X : e.Y);
                needsRedraw = true;
            }

            if (needsRedraw)
            {
                Invalidate();
            }

            lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            isDraggingThumb = false;
            isDraggingRangeStart = false;
            isDraggingRangeEnd = false;
        }

        private void OnSliderMouseMove(object sender, MouseEventArgs e)
        {
            if (!showValueTooltip)
            {
                return;
            }

            // 显示当前鼠标位置对应的值
            if (trackRect.Contains(e.Location))
            {
                double hoverValue = GetValueFromPosition(orientation == SliderOrientation.Horizontal ? e.X : e.Y);

                string tooltipText;
                if (isRangeMode)
                {
                    tooltipText = $"范围值: [{rangeStart:F1} - {rangeEnd:F1}]\nHover: {hoverValue:F2}";
                }
                else
                {
                    tooltipText = $"值: {value:F2}\nHover: {hoverValue:F2}";
                }

                valueTooltip.Show(tooltipText, this, e.X + 10, e.Y - 20, 2000);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            isThumbHovered = false;
            isRangeStartHovered = false;
            isRangeEndHovered = false;
            valueTooltip.Hide(this);

            Invalidate();
        }

        #endregion

        #region 键盘处理

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            double step = valueType == SliderValueType.Discrete ? smallChange :
                         (Maximum - Minimum) / 100;

            bool handled = true;

            if (orientation == SliderOrientation.Horizontal)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        Value -= step;
                        break;
                    case Keys.Right:
                        Value += step;
                        break;
                    case Keys.Home:
                        Value = Minimum;
                        break;
                    case Keys.End:
                        Value = Maximum;
                        break;
                    default:
                        handled = false;
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Down:
                        Value -= step;
                        break;
                    case Keys.Up:
                        Value += step;
                        break;
                    case Keys.Home:
                        Value = Minimum;
                        break;
                    case Keys.End:
                        Value = Maximum;
                        break;
                    default:
                        handled = false;
                        break;
                }
            }

            e.Handled = handled;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        #endregion

        #region 关联文本框处理

        private void UpdateAssociatedTextBox()
        {
            if (associatedTextBox != null && !associatedTextBox.Focused)
            {
                associatedTextBox.Text = value.ToString("F2");
            }
        }

        private void OnAssociatedTextBoxTextChanged(object sender, EventArgs e)
        {
            if (associatedTextBox == null || !associatedTextBox.Focused)
            {
                return;
            }

            if (double.TryParse(associatedTextBox.Text, out double newValue))
            {
                Value = newValue;
            }
        }

        private void OnAssociatedTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许数字、小数点、负号和控制字符
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != '.' && e.KeyChar != '-')
            {
                e.Handled = true;
            }

            // 只允许一个小数点
            if (e.KeyChar == '.' && associatedTextBox.Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            // 负号只能在开头
            if (e.KeyChar == '-' && associatedTextBox.SelectionStart != 0)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            // 绘制控件背景
            using (var brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制轨道
            DrawTrack(g);

            // 绘制刻度
            if (showTickMarks && valueType == SliderValueType.Discrete)
            {
                DrawTickMarks(g);
            }

            // 绘制自定义标记
            DrawCustomMarks(g);

            // 绘制滑块
            if (isRangeMode)
            {
                DrawRangeSelection(g);
                DrawThumb(g, rangeStartRect, isRangeStartHovered || isDraggingRangeStart);
                DrawThumb(g, rangeEndRect, isRangeEndHovered || isDraggingRangeEnd);
            }
            else
            {
                DrawThumb(g, thumbRect, isThumbHovered || isDraggingThumb);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            // 如果需要, 可以绘制外边框
        }

        private void DrawTrack(Graphics g)
        {
            Color trackColor = GetThemeColor(c => c.Border, Color.LightGray);
            Color activeColor = GetThemeColor(c => c.Primary, SystemColors.Highlight);

            int cornerRadius = trackHeight / 2;

            // 绘制背景轨道
            using (var path = GetRoundedRectangle(trackRect, cornerRadius))
            using (var brush = new SolidBrush(trackColor))
            {
                g.FillPath(brush, path);
            }

            // 绘制活动部分
            Rectangle activeRect = trackRect;

            if (isRangeMode)
            {
                // 范围模式：绘制两个滑块之间的部分
                int startPos = GetPositionFromValue(rangeStart);
                int endPos = GetPositionFromValue(rangeEnd);

                if (orientation == SliderOrientation.Horizontal)
                {
                    int left = Math.Min(startPos, endPos);
                    int right = Math.Max(startPos, endPos);
                    int width = Math.Max(1, right - left); // 确保宽度至少为1
                    activeRect = new Rectangle(left, trackRect.Y, width, trackRect.Height);
                }
                else
                {
                    int top = Math.Min(startPos, endPos);
                    int bottom = Math.Max(startPos, endPos);
                    int height = Math.Max(1, bottom - top); // 确保高度至少为1
                    activeRect = new Rectangle(trackRect.X, top, trackRect.Width, height);
                }
            }
            else
            {
                // 单值模式：从起点到当前值
                int valuePos = GetPositionFromValue(value);

                if (orientation == SliderOrientation.Horizontal)
                {
                    int width = Math.Max(1, valuePos - trackRect.Left);
                    activeRect = new Rectangle(trackRect.Left, trackRect.Y, width, trackRect.Height);
                }
                else
                {
                    int height = Math.Max(1, trackRect.Bottom - valuePos);
                    activeRect = new Rectangle(trackRect.X, valuePos, trackRect.Width, height);
                }
            }

            // 验证activeRect是否有效
            if (activeRect.Width > 0 && activeRect.Height > 0)
            {
                using (var path = GetRoundedRectangle(activeRect, cornerRadius))
                using (var brush = new SolidBrush(activeColor))
                {
                    g.FillPath(brush, path);
                }
            }
        }


        private void DrawThumb(Graphics g, Rectangle rect, bool isHighlight)
        {
            Color thumbColor = GetThemeColor(c => c.Primary, SystemColors.Highlight);
            Color thumbBorder = GetThemeColor(c => c.PrimaryDark, SystemColors.HotTrack);

            if (isHighlight)
            {
                thumbColor = GetThemeColor(c => c.PrimaryLight, Color.LightSkyBlue);
            }

            if (!Enabled)
            {
                thumbColor = GetThemeColor(c => c.TextDisabled, SystemColors.GrayText);
                thumbBorder = thumbColor;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (thumbShape == SliderShape.Circle)
            {
                // 绘制阴影
                if (ShadowLevel > 0 || isHighlight)
                {
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(30, Color.Black)))
                    {
                        var shadowRect = rect;
                        shadowRect.Offset(2, 2);
                        g.FillEllipse(shadowBrush, shadowRect);
                    }
                }

                // 绘制滑块
                using (var brush = new SolidBrush(thumbColor))
                using (var pen = new Pen(thumbBorder, 2))
                {
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(pen, rect);
                }

                // 高亮效果
                if (isHighlight)
                {
                    var highlightRect = new Rectangle(
                        rect.X + rect.Width / 4,
                        rect.Y + rect.Height / 4,
                        rect.Width / 2,
                        rect.Height / 2);

                    using (var brush = new SolidBrush(Color.FromArgb(100, Color.White)))
                    {
                        g.FillEllipse(brush, highlightRect);
                    }
                }
            }
            else
            {
                int cornerRadius = GetThemeValue(e => e.CornerRadiusSmall, 4);

                // 绘制阴影
                if (ShadowLevel > 0 || isHighlight)
                {
                    var shadowRect = rect;
                    shadowRect.Offset(2, 2);
                    using (var path = GetRoundedRectangle(shadowRect, cornerRadius))
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(30, Color.Black)))
                    {
                        g.FillPath(shadowBrush, path);
                    }
                }

                // 绘制滑块
                using (var path = GetRoundedRectangle(rect, cornerRadius))
                using (var brush = new SolidBrush(thumbColor))
                using (var pen = new Pen(thumbBorder, 2))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(pen, path);
                }

                // 高亮效果
                if (isHighlight)
                {
                    var highlightRect = new Rectangle(
                        rect.X + rect.Width / 4,
                        rect.Y + rect.Height / 4,
                        rect.Width / 2,
                        rect.Height / 2);

                    using (var path = GetRoundedRectangle(highlightRect, cornerRadius / 2))
                    using (var brush = new SolidBrush(Color.FromArgb(100, Color.White)))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        private void DrawRangeSelection(Graphics g)
        {
            // 在范围模式下, 高亮选中的范围已在DrawTrack中完成
        }

        private void DrawTickMarks(Graphics g)
        {
            if (Maximum <= Minimum || smallChange <= 0)
            {
                return;
            }

            Color tickColor = GetThemeColor(c => c.TextSecondary, SystemColors.GrayText);
            Font tickFont = GetThemeFont(t => t.Caption, Font);

            using (var pen = new Pen(tickColor, 1))
            using (var brush = new SolidBrush(tickColor))
            {
                double currentValue = Minimum;
                while (currentValue <= Maximum)
                {
                    int position = GetPositionFromValue(currentValue);

                    if (orientation == SliderOrientation.Horizontal)
                    {
                        // 绘制刻度线
                        int tickY = trackRect.Bottom + 2;
                        g.DrawLine(pen, position, tickY, position, tickY + 5);

                        // 绘制文本
                        if (showTickText)
                        {
                            string text = currentValue.ToString("F0");
                            var textSize = g.MeasureString(text, tickFont);
                            g.DrawString(text, tickFont, brush,
                                position - textSize.Width / 2,
                                tickY + 7);
                        }
                    }
                    else
                    {
                        // 绘制刻度线
                        int tickX = trackRect.Right + 2;
                        g.DrawLine(pen, tickX, position, tickX + 5, position);

                        // 绘制文本
                        if (showTickText)
                        {
                            string text = currentValue.ToString("F0");
                            var textSize = g.MeasureString(text, tickFont);
                            g.DrawString(text, tickFont, brush,
                                tickX + 7,
                                position - textSize.Height / 2);
                        }
                    }

                    currentValue += smallChange;
                }
            }
        }

        private void DrawCustomMarks(Graphics g)
        {
            foreach (var mark in marks)
            {
                if (mark.Value < Minimum || mark.Value > Maximum)
                {
                    continue;
                }

                int position = GetPositionFromValue(mark.Value);

                // 绘制标记点
                int markSize = mark.Size;
                Rectangle markRect;

                if (orientation == SliderOrientation.Horizontal)
                {
                    markRect = new Rectangle(
                        position - markSize / 2,
                        trackRect.Y - markSize - 2,
                        markSize, markSize);
                }
                else
                {
                    markRect = new Rectangle(
                        trackRect.X - markSize - 2,
                        position - markSize / 2,
                        markSize, markSize);
                }

                using (var brush = new SolidBrush(mark.Color))
                {
                    g.FillEllipse(brush, markRect);
                }

                // 绘制文本
                if (mark.ShowText && !string.IsNullOrEmpty(mark.Text))
                {
                    using (var brush = new SolidBrush(mark.TextColor))
                    {
                        var textSize = g.MeasureString(mark.Text, mark.Font);

                        if (orientation == SliderOrientation.Horizontal)
                        {
                            g.DrawString(mark.Text, mark.Font, brush,
                                position - textSize.Width / 2,
                                markRect.Y - textSize.Height - 2);
                        }
                        else
                        {
                            g.DrawString(mark.Text, mark.Font, brush,
                                markRect.X - textSize.Width - 2,
                                position - textSize.Height / 2);
                        }
                    }
                }
            }
        }

        private int GetThemeValue(Func<IElevation, int> selector, int defaultValue)
        {
            if (UseTheme && Theme?.Elevation != null)
            {
                try
                {
                    return selector(Theme.Elevation);
                }
                catch
                {
                    // 如果获取失败, 返回默认值
                }
            }
            return defaultValue;
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                valueTooltip?.Dispose();

                if (associatedTextBox != null)
                {
                    associatedTextBox.TextChanged -= OnAssociatedTextBoxTextChanged;
                    associatedTextBox.KeyPress -= OnAssociatedTextBoxKeyPress;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }


    #region 枚举和辅助类

    /// <summary>
    /// 滑块形状
    /// </summary>
    public enum SliderShape
    {
        Circle,
        Square
    }

    /// <summary>
    /// 滑块方向
    /// </summary>
    public enum SliderOrientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// 滑块值类型
    /// </summary>
    public enum SliderValueType
    {
        /// <summary>
        /// 连续值
        /// </summary>
        Continuous,

        /// <summary>
        /// 离散值
        /// </summary>
        Discrete
    }

    /// <summary>
    /// 标记
    /// </summary>
    public class SliderMark
    {
        public SliderMark()
        {
            Color = Color.Red;
            Font = SystemFonts.DefaultFont;
            TextColor = Color.Black;
        }

        /// <summary>
        /// 标记的值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 标记文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 标记颜色
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 文本字体
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// 文本颜色
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// 标记大小
        /// </summary>
        public int Size { get; set; } = 8;

        /// <summary>
        /// 是否显示文本
        /// </summary>
        public bool ShowText { get; set; } = true;
    }

    #endregion

    #region 设计器

    public class FluentSliderDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentSliderActionList(Component));
                }
                return actionLists;
            }
        }

        public override SelectionRules SelectionRules
        {
            get
            {
                FluentSlider slider = Control as FluentSlider;
                if (slider != null && slider.Orientation == SliderOrientation.Vertical)
                {
                    // 垂直滑块限制水平调整
                    return SelectionRules.Visible | SelectionRules.Moveable |
                           SelectionRules.TopSizeable | SelectionRules.BottomSizeable;
                }
                else
                {
                    // 水平滑块限制垂直调整
                    return SelectionRules.Visible | SelectionRules.Moveable |
                           SelectionRules.LeftSizeable | SelectionRules.RightSizeable;
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 可以在这里修改属性的显示方式
            string[] shadowProps = new string[] { "Text", "BackgroundImage", "BackgroundImageLayout" };

            foreach (string prop in shadowProps)
            {
                if (properties.Contains(prop))
                {
                    PropertyDescriptor descriptor = (PropertyDescriptor)properties[prop];
                    properties[prop] = TypeDescriptor.CreateProperty(
                        typeof(FluentSliderDesigner),
                        descriptor,
                        new BrowsableAttribute(false));
                }
            }
        }

        // 设计时绘制辅助线
        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);

            FluentSlider slider = Control as FluentSlider;
            if (slider == null) return;

            // 绘制设计时的辅助信息
            using (Pen pen = new Pen(Color.FromArgb(100, SystemColors.Highlight)))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                Rectangle bounds = Control.ClientRectangle;
                bounds.Inflate(-1, -1);
                pe.Graphics.DrawRectangle(pen, bounds);
            }
        }
    }

    /// <summary>
    /// FluentSlider的智能标签操作列表
    /// </summary>
    public class FluentSliderActionList : DesignerActionList
    {
        private FluentSlider slider;
        private DesignerActionUIService designerService;

        public FluentSliderActionList(IComponent component) : base(component)
        {
            slider = component as FluentSlider;
            designerService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        #region 智能标签属性

        [Category("外观")]
        [Description("滑块的方向")]
        public SliderOrientation Orientation
        {
            get => slider.Orientation;
            set
            {
                SetProperty("Orientation", value);

                // 自动调整尺寸
                if (value == SliderOrientation.Vertical)
                {
                    if (slider.Width > slider.Height)
                    {
                        var temp = slider.Width;
                        slider.Width = Math.Max(50, slider.Height / 4);
                        slider.Height = temp;
                    }
                }
                else
                {
                    if (slider.Height > slider.Width)
                    {
                        var temp = slider.Height;
                        slider.Height = Math.Max(50, slider.Width / 4);
                        slider.Width = temp;
                    }
                }
            }
        }

        [Category("外观")]
        [Description("滑块的形状")]
        public SliderShape ThumbShape
        {
            get => slider.ThumbShape;
            set => SetProperty("ThumbShape", value);
        }

        [Category("行为")]
        [Description("值类型")]
        public SliderValueType ValueType
        {
            get => slider.ValueType;
            set => SetProperty("ValueType", value);
        }

        [Category("行为")]
        [Description("是否启用范围模式")]
        public bool IsRangeMode
        {
            get => slider.IsRangeMode;
            set => SetProperty("IsRangeMode", value);
        }

        [Category("外观")]
        [Description("是否显示刻度")]
        public bool ShowTickMarks
        {
            get => slider.ShowTickMarks;
            set => SetProperty("ShowTickMarks", value);
        }

        [Category("外观")]
        [Description("是否显示刻度文本")]
        public bool ShowTickText
        {
            get => slider.ShowTickText;
            set => SetProperty("ShowTickText", value);
        }

        #endregion

        #region 智能标签方法

        [Description("设置为连续值模式")]
        public void SetContinuousMode()
        {
            SetProperty("ValueType", SliderValueType.Continuous);
            SetProperty("ShowTickMarks", false);
            SetProperty("ShowTickText", false);
        }

        [Description("设置为离散值模式")]
        public void SetDiscreteMode()
        {
            SetProperty("ValueType", SliderValueType.Discrete);
            SetProperty("SmallChange", 10.0);
            SetProperty("ShowTickMarks", true);
        }

        [Description("添加示例标记")]
        public void AddSampleMarks()
        {
            slider.ClearMarks();

            double range = slider.Maximum - slider.Minimum;
            slider.AddMark(slider.Minimum + range * 0.25, "低", Color.Green);
            slider.AddMark(slider.Minimum + range * 0.5, "中", Color.Orange);
            slider.AddMark(slider.Minimum + range * 0.75, "高", Color.Red);

            slider.Invalidate();
        }

        [Description("清除所有标记")]
        public void ClearAllMarks()
        {
            slider.ClearMarks();
        }

        #endregion

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 添加分组标题和属性
            items.Add(new DesignerActionHeaderItem("外观设置"));
            items.Add(new DesignerActionPropertyItem("Orientation", "方向", "外观设置"));
            items.Add(new DesignerActionPropertyItem("ThumbShape", "滑块形状", "外观设置"));
            items.Add(new DesignerActionPropertyItem("ShowTickMarks", "显示刻度", "外观设置"));
            items.Add(new DesignerActionPropertyItem("ShowTickText", "显示刻度文本", "外观设置"));

            items.Add(new DesignerActionHeaderItem("行为设置"));
            items.Add(new DesignerActionPropertyItem("ValueType", "值类型", "行为设置"));
            items.Add(new DesignerActionPropertyItem("IsRangeMode", "范围模式", "行为设置"));

            items.Add(new DesignerActionHeaderItem("快速操作"));
            items.Add(new DesignerActionMethodItem(this, "SetContinuousMode", "设置连续值模式", "快速操作"));
            items.Add(new DesignerActionMethodItem(this, "SetDiscreteMode", "设置离散值模式", "快速操作"));

            items.Add(new DesignerActionHeaderItem("标记管理"));
            items.Add(new DesignerActionMethodItem(this, "AddSampleMarks", "添加示例标记", "标记管理"));
            items.Add(new DesignerActionMethodItem(this, "ClearAllMarks", "清除所有标记", "标记管理"));

            return items;
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(slider)[propertyName];
            if (property != null)
            {
                property.SetValue(slider, value);
            }
            designerService?.Refresh(Component);
        }
    }

    /// <summary>
    /// SliderMark集合编辑器
    /// </summary>
    public class SliderMarkCollectionEditor : CollectionEditor
    {
        public SliderMarkCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(SliderMark);
        }

        protected override object CreateInstance(Type itemType)
        {
            return new SliderMark
            {
                Value = 50,
                Text = "标记",
                Color = Color.Red,
                Font = SystemFonts.DefaultFont,
                TextColor = Color.Black,
                Size = 8,
                ShowText = true
            };
        }
    }

    #endregion
}
