using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    [ToolboxItem(true)]
    [DefaultProperty("Progress")]
    [DefaultEvent("ProgressChanged")]
    public class FluentProgress : FluentControlBase
    {
        private ProgressMode mode = ProgressMode.Determinate;
        private ProgressStyle style = ProgressStyle.Linear;
        private double progress = 0;
        private double minimum = 0;
        private double maximum = 100;

        // 任务管理
        private IProgressTask singleTask;
        private readonly ProgressTaskCollection tasks;
        private bool autoUpdateFromTasks = true;

        // 循环模式动画
        private Timer indeterminateTimer;
        private float indeterminatePosition = 0;
        private int indeterminateSpeed = 20; // 像素/秒

        // 文本显示
        private bool showProgressText = true;
        private bool showPercentage = true;
        private string customText = "";
        private string prefixText = "";
        private string suffixText = "";
        private Font progressFont;
        private Color progressTextColor = Color.Empty;
        private ProgressTextPosition textPosition = ProgressTextPosition.Center;
        private bool showTaskInfo = false;

        // 颜色配置
        private Color progressBarColor = Color.Empty;
        private Color progressBackColor = Color.Empty;
        private bool useGradient = true;

        // 分段模式
        private int segmentCount = 10;
        private int segmentSpacing = 2;

        // 圆形模式
        private int circularThickness = 8;
        private int circularStartAngle = -90;


        public FluentProgress()
        {
            tasks = new ProgressTaskCollection();
            tasks.CollectionChanged += OnTasksCollectionChanged;
            tasks.ProgressChanged += OnTasksProgressChanged;

            Size = new Size(300, 24);

            InitializeIndeterminateTimer();
            progressFont = new Font(Font.FontFamily, Font.Size, FontStyle.Regular);
        }


        #region 属性

        [Category("Progress")]
        [Description("进度条模式")]
        [DefaultValue(ProgressMode.Determinate)]
        public ProgressMode Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;
                    UpdateIndeterminateTimer();
                    Invalidate();
                }
            }
        }

        [Category("Progress")]
        [Description("进度条样式")]
        [DefaultValue(ProgressStyle.Linear)]
        public ProgressStyle Style
        {
            get => style;
            set
            {
                if (style != value)
                {
                    style = value;
                    Invalidate();
                }
            }
        }

        [Category("Progress")]
        [Description("当前进度值")]
        [DefaultValue(0.0)]
        public double Progress
        {
            get => progress;
            set
            {
                var newValue = Math.Max(minimum, Math.Min(maximum, value));
                if (Math.Abs(progress - newValue) > 0.001)
                {
                    progress = newValue;
                    OnProgressChanged();
                    Invalidate();
                }
            }
        }

        [Category("Progress")]
        [Description("进度最小值")]
        [DefaultValue(0.0)]
        public double Minimum
        {
            get => minimum;
            set
            {
                if (minimum != value)
                {
                    minimum = value;
                    if (progress < minimum)
                    {
                        Progress = minimum;
                    }

                    Invalidate();
                }
            }
        }

        [Category("Progress")]
        [Description("进度最大值")]
        [DefaultValue(100.0)]
        public double Maximum
        {
            get => maximum;
            set
            {
                if (maximum != value && value > minimum)
                {
                    maximum = value;
                    if (progress > maximum)
                    {
                        Progress = maximum;
                    }

                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IProgressTask SingleTask
        {
            get => singleTask;
            set
            {
                if (singleTask != value)
                {
                    if (singleTask != null)
                    {
                        singleTask.ProgressChanged -= OnSingleTaskProgressChanged;
                    }

                    singleTask = value;

                    if (singleTask != null)
                    {
                        singleTask.ProgressChanged += OnSingleTaskProgressChanged;
                        UpdateProgressFromTasks();
                    }
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ProgressTaskCollection Tasks => tasks;

        [Category("Progress")]
        [Description("是否自动从挂接的任务更新进度")]
        [DefaultValue(true)]
        public bool AutoUpdateFromTasks
        {
            get => autoUpdateFromTasks;
            set
            {
                if (autoUpdateFromTasks != value)
                {
                    autoUpdateFromTasks = value;
                    if (value)
                    {
                        UpdateProgressFromTasks();
                    }
                }
            }
        }

        [Category("Text")]
        [Description("是否显示进度文本")]
        [DefaultValue(true)]
        public bool ShowProgressText
        {
            get => showProgressText;
            set
            {
                if (showProgressText != value)
                {
                    showProgressText = value;
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [Description("是否显示百分比")]
        [DefaultValue(true)]
        public bool ShowPercentage
        {
            get => showPercentage;
            set
            {
                if (showPercentage != value)
                {
                    showPercentage = value;
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [Description("自定义显示文本")]
        [DefaultValue("")]
        public string CustomText
        {
            get => customText;
            set
            {
                if (customText != value)
                {
                    customText = value ?? "";
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [Description("进度文本前缀")]
        [DefaultValue("")]
        public string PrefixText
        {
            get => prefixText;
            set
            {
                if (prefixText != value)
                {
                    prefixText = value ?? "";
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [Description("进度文本后缀")]
        [DefaultValue("")]
        public string SuffixText
        {
            get => suffixText;
            set
            {
                if (suffixText != value)
                {
                    suffixText = value ?? "";
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [Description("进度文本字体")]
        public Font ProgressFont
        {
            get => progressFont;
            set
            {
                if (progressFont != value)
                {
                    progressFont = value ?? Font;
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [Description("进度文本颜色(空表示自动)")]
        public Color ProgressTextColor
        {
            get => progressTextColor;
            set
            {
                if (progressTextColor != value)
                {
                    progressTextColor = value;
                    Invalidate();
                }
            }
        }

        private bool ShouldSerializeProgressTextColor()
        {
            return progressTextColor != Color.Empty;
        }

        private void ResetProgressTextColor()
        {
            ProgressTextColor = Color.Empty;
        }

        [Category("Progress")]
        [Description("进度文本显示位置")]
        [DefaultValue(ProgressTextPosition.Center)]
        public ProgressTextPosition TextPosition
        {
            get => textPosition;
            set
            {
                if (textPosition != value)
                {
                    textPosition = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 是否显示任务信息
        /// </summary>
        [Category("Progress")]
        [Description("是否显示当前任务信息")]
        [DefaultValue(false)]
        public bool ShowTaskInfo
        {
            get => showTaskInfo;
            set
            {
                if (showTaskInfo != value)
                {
                    showTaskInfo = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 进度条颜色
        /// </summary>
        [Category("Progress")]
        [Description("进度条颜色(空表示使用主题色)")]
        public Color ProgressBarColor
        {
            get => progressBarColor;
            set
            {
                if (progressBarColor != value)
                {
                    progressBarColor = value;
                    Invalidate();
                }
            }
        }

        private bool ShouldSerializeProgressBarColor()
        {
            return progressBarColor != Color.Empty;
        }

        private void ResetProgressBarColor()
        {
            ProgressBarColor = Color.Empty;
        }

        /// <summary>
        /// 进度条背景色
        /// </summary>
        [Category("Progress")]
        [Description("进度条背景色(空表示使用主题色)")]
        public Color ProgressBackColor
        {
            get => progressBackColor;
            set
            {
                if (progressBackColor != value)
                {
                    progressBackColor = value;
                    Invalidate();
                }
            }
        }

        private bool ShouldSerializeProgressBackColor()
        {
            return progressBackColor != Color.Empty;
        }

        private void ResetProgressBackColor()
        {
            ProgressBackColor = Color.Empty;
        }

        /// <summary>
        /// 是否使用渐变
        /// </summary>
        [Category("Colors")]
        [Description("是否使用渐变效果")]
        [DefaultValue(true)]
        public bool UseGradient
        {
            get => useGradient;
            set
            {
                if (useGradient != value)
                {
                    useGradient = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 分段数量(Segmented样式)
        /// </summary>
        [Category("Style")]
        [Description("分段进度条的分段数量")]
        [DefaultValue(10)]
        public int SegmentCount
        {
            get => segmentCount;
            set
            {
                if (segmentCount != value && value > 0)
                {
                    segmentCount = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 分段间距(Segmented样式)
        /// </summary>
        [Category("Style")]
        [Description("分段进度条的分段间距")]
        [DefaultValue(2)]
        public int SegmentSpacing
        {
            get => segmentSpacing;
            set
            {
                if (segmentSpacing != value && value >= 0)
                {
                    segmentSpacing = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 圆形进度条厚度
        /// </summary>
        [Category("Style")]
        [Description("圆形进度条的线条厚度")]
        [DefaultValue(8)]
        public int CircularThickness
        {
            get => circularThickness;
            set
            {
                if (circularThickness != value && value > 0)
                {
                    circularThickness = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 圆形进度条起始角度
        /// </summary>
        [Category("Style")]
        [Description("圆形进度条的起始角度")]
        [DefaultValue(-90)]
        public int CircularStartAngle
        {
            get => circularStartAngle;
            set
            {
                if (circularStartAngle != value)
                {
                    circularStartAngle = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 循环动画速度
        /// </summary>
        [Category("Animation")]
        [Description("循环模式下的动画速度(像素/秒)")]
        [DefaultValue(20)]
        public int IndeterminateSpeed
        {
            get => indeterminateSpeed;
            set
            {
                if (indeterminateSpeed != value && value > 0)
                {
                    indeterminateSpeed = value;
                }
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 进度变更事件
        /// </summary>
        [Category("Progress")]
        [Description("进度值变更时触发")]
        public event EventHandler ProgressChanged;

        protected virtual void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置进度(带动画)
        /// </summary>
        public void SetProgress(double value, bool animated = false)
        {
            if (animated && EnableAnimation)
            {
                // TODO: 实现动画过渡
                Progress = value;
            }
            else
            {
                Progress = value;
            }
        }

        /// <summary>
        /// 增加进度
        /// </summary>
        public void Increment(double value)
        {
            Progress += value;
        }

        /// <summary>
        /// 重置进度
        /// </summary>
        public void Reset()
        {
            Progress = minimum;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public void AddTask(IProgressTask task)
        {
            if (task != null)
            {
                tasks.Add(task);
            }
        }

        /// <summary>
        /// 添加任务(指定名称和权重)
        /// </summary>
        public IProgressTask AddTask(string name, double weight = 1.0)
        {
            var task = new ProgressTask(name, weight);
            tasks.Add(task);
            return task;
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        public void RemoveTask(IProgressTask task)
        {
            if (task != null)
            {
                tasks.Remove(task);
            }
        }

        /// <summary>
        /// 清除所有任务
        /// </summary>
        public void ClearTasks()
        {
            SingleTask = null;
            tasks.Clear();
        }

        /// <summary>
        /// 标准化任务权重
        /// </summary>
        public void NormalizeTaskWeights()
        {
            tasks.NormalizeWeights();
        }

        /// <summary>
        /// 设置任务权重
        /// </summary>
        public void SetTaskWeights(params double[] weights)
        {
            tasks.SetWeights(weights);
        }

        /// <summary>
        /// 获取当前进度百分比
        /// </summary>
        public double GetPercentage()
        {
            return maximum <= minimum ? 0 : (progress - minimum) / (maximum - minimum) * 100;
        }

        #endregion

        #region 私有方法

        private void InitializeIndeterminateTimer()
        {
            indeterminateTimer = new Timer();
            indeterminateTimer.Interval = 16; // ~60 FPS
            indeterminateTimer.Tick += OnIndeterminateTick;
        }

        private void UpdateIndeterminateTimer()
        {
            if (mode == ProgressMode.Indeterminate)
            {
                indeterminatePosition = 0;
                indeterminateTimer?.Start();
            }
            else
            {
                indeterminateTimer?.Stop();
            }
        }

        private void OnIndeterminateTick(object sender, EventArgs e)
        {
            indeterminatePosition += (indeterminateSpeed * 16 / 1000f) / Width;
            if (indeterminatePosition > 1.0f)
            {
                indeterminatePosition = 0;
            }
            Invalidate();
        }

        private void OnSingleTaskProgressChanged(object sender, EventArgs e)
        {
            UpdateProgressFromTasks();
        }

        private void OnTasksCollectionChanged(object sender, EventArgs e)
        {
            UpdateProgressFromTasks();
        }

        private void OnTasksProgressChanged(object sender, EventArgs e)
        {
            UpdateProgressFromTasks();
        }

        private void UpdateProgressFromTasks()
        {
            if (!autoUpdateFromTasks)
            {
                return;
            }

            if (singleTask != null)
            {
                // 单任务模式
                Progress = singleTask.Progress;
            }
            else if (tasks.Count > 0)
            {
                // 多任务模式
                Progress = tasks.GetTotalProgress();
            }
        }

        private Color GetActualProgressBarColor()
        {
            return progressBarColor != Color.Empty ? progressBarColor : GetThemeColor(c => c.Primary, SystemColors.Highlight);
        }

        private Color GetActualProgressBackColor()
        {
            return progressBackColor != Color.Empty ? progressBackColor : GetThemeColor(c => c.BackgroundSecondary, SystemColors.ControlLight);
        }

        private Color GetActualTextColor()
        {
            return progressTextColor != Color.Empty ? progressTextColor : GetThemeColor(c => c.TextPrimary, ForeColor);
        }

        private string GetProgressText()
        {
            if (!string.IsNullOrEmpty(customText))
            {
                return customText;
            }

            string text = "";

            // 前缀
            if (!string.IsNullOrEmpty(prefixText))
            {
                text += prefixText + " ";
            }

            // 百分比或数值
            if (showPercentage)
            {
                text += $"{GetPercentage():F1}%";
            }
            else
            {
                text += $"{progress:F1}/{maximum:F1}";
            }

            // 后缀
            if (!string.IsNullOrEmpty(suffixText))
            {
                text += " " + suffixText;
            }

            // 任务信息
            if (showTaskInfo)
            {
                if (singleTask != null)
                {
                    text += $" - {singleTask.Name}";
                }
                else if (tasks.Count > 0)
                {
                    var completedTasks = tasks.Count(t => t.Progress >= 100);
                    text += $" ({completedTasks}/{tasks.Count} tasks)";
                }
            }

            return text;
        }

        #endregion

        #region 绘制方法

        protected override void DrawBackground(Graphics g)
        {
            var backColor = GetActualProgressBackColor();
            int cornerRadius = UseTheme && Theme?.Elevation != null
                ? Theme.Elevation.CornerRadiusSmall
                : 4;

            switch (style)
            {
                case ProgressStyle.Linear:
                case ProgressStyle.Segmented:
                    using (var brush = new SolidBrush(backColor))
                    using (var path = GetRoundedRectangle(ClientRectangle, cornerRadius))
                    {
                        g.FillPath(brush, path);
                    }
                    break;

                case ProgressStyle.Circular:
                    using (var brush = new SolidBrush(BackColor))
                    {
                        g.FillEllipse(brush, ClientRectangle);
                    }
                    DrawCircularBackground(g, backColor);
                    break;
            }
        }

        protected override void DrawContent(Graphics g)
        {
            switch (style)
            {
                case ProgressStyle.Linear:
                    if (mode == ProgressMode.Indeterminate)
                    {
                        DrawLinearIndeterminate(g);
                    }
                    else
                    {
                        DrawLinearDeterminate(g);
                    }

                    break;

                case ProgressStyle.Segmented:
                    if (mode == ProgressMode.Indeterminate)
                    {
                        DrawSegmentedIndeterminate(g);
                    }
                    else
                    {
                        DrawSegmentedDeterminate(g);
                    }

                    break;

                case ProgressStyle.Circular:
                    if (mode == ProgressMode.Indeterminate)
                    {
                        DrawCircularIndeterminate(g);
                    }
                    else
                    {
                        DrawCircularDeterminate(g);
                    }

                    break;
            }

            if (showProgressText && mode == ProgressMode.Determinate)
            {
                DrawProgressText(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            // 线性和分段样式不需要边框
            if (style == ProgressStyle.Linear || style == ProgressStyle.Segmented)
            {
                return;
            }

            // 圆形样式可以添加边框
            if (style == ProgressStyle.Circular)
            {
                var borderColor = GetThemeColor(c => c.Border, SystemColors.ControlDark);
                using (var pen = new Pen(borderColor, 1))
                {
                    var rect = ClientRectangle;
                    rect.Inflate(-1, -1);
                    g.DrawEllipse(pen, rect);
                }
            }
        }

        #region 线性样式绘制

        private void DrawLinearDeterminate(Graphics g)
        {
            double percentage = GetPercentage();
            int progressWidth = (int)(Width * percentage / 100.0);

            if (progressWidth <= 0)
            {
                return;
            }

            var progressRect = new Rectangle(0, 0, progressWidth, Height);
            var progressColor = GetActualProgressBarColor();
            int cornerRadius = UseTheme && Theme?.Elevation != null
                ? Theme.Elevation.CornerRadiusSmall
                : 4;

            if (useGradient)
            {
                using (var brush = new LinearGradientBrush(
                    progressRect,
                    AdjustBrightness(progressColor, 0.2f),
                    progressColor,
                    LinearGradientMode.Vertical))
                using (var path = GetRoundedRectangle(progressRect, cornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
            else
            {
                using (var brush = new SolidBrush(progressColor))
                using (var path = GetRoundedRectangle(progressRect, cornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawLinearIndeterminate(Graphics g)
        {
            int blockWidth = Width / 3;
            float totalWidth = Width + blockWidth;
            int x = (int)(indeterminatePosition * totalWidth) - blockWidth;

            if (x >= Width)
            {
                return;
            }

            int drawX = Math.Max(0, x);
            int drawWidth = Math.Min(blockWidth, Width - x);

            if (drawWidth <= 0)
            {
                return;
            }

            var progressColor = GetActualProgressBarColor();
            int cornerRadius = UseTheme && Theme?.Elevation != null
                ? Theme.Elevation.CornerRadiusSmall
                : 4;

            var progressRect = new Rectangle(drawX, 0, drawWidth, Height);

            using (var brush = new LinearGradientBrush(
                new Rectangle(x, 0, blockWidth, Height),
                Color.FromArgb(50, progressColor),
                progressColor,
                LinearGradientMode.Horizontal))
            using (var path = GetRoundedRectangle(progressRect, cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        #endregion

        #region 分段样式绘制

        private void DrawSegmentedDeterminate(Graphics g)
        {
            double percentage = GetPercentage();
            int completedSegments = (int)Math.Ceiling(segmentCount * percentage / 100.0);

            var progressColor = GetActualProgressBarColor();
            float segmentWidth = (Width - (segmentCount - 1) * segmentSpacing) / (float)segmentCount;
            int cornerRadius = UseTheme && Theme?.Elevation != null
                ? Theme.Elevation.CornerRadiusSmall
                : 2;

            for (int i = 0; i < segmentCount; i++)
            {
                float x = i * (segmentWidth + segmentSpacing);
                var segmentRect = new RectangleF(x, 0, segmentWidth, Height);

                Color segmentColor;
                if (i < completedSegments)
                {
                    segmentColor = progressColor;
                }
                else
                {
                    segmentColor = Color.FromArgb(50, progressColor);
                }

                using (var brush = new SolidBrush(segmentColor))
                using (var path = GetRoundedRectangle(
                    Rectangle.Round(segmentRect), cornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawSegmentedIndeterminate(Graphics g)
        {
            var progressColor = GetActualProgressBarColor();
            float segmentWidth = (Width - (segmentCount - 1) * segmentSpacing) / (float)segmentCount;
            int cornerRadius = UseTheme && Theme?.Elevation != null
                ? Theme.Elevation.CornerRadiusSmall
                : 2;

            int activeSegment = (int)(indeterminatePosition * segmentCount);

            for (int i = 0; i < segmentCount; i++)
            {
                float x = i * (segmentWidth + segmentSpacing);
                var segmentRect = new RectangleF(x, 0, segmentWidth, Height);

                // 计算当前分段的亮度
                int distance = Math.Abs(i - activeSegment);
                float alpha = distance == 0 ? 1.0f : (distance == 1 ? 0.5f : 0.2f);

                using (var brush = new SolidBrush(
                    Color.FromArgb((int)(255 * alpha), progressColor)))
                using (var path = GetRoundedRectangle(
                    Rectangle.Round(segmentRect), cornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        #endregion

        #region 圆形样式绘制

        private void DrawCircularBackground(Graphics g, Color backColor)
        {
            var rect = GetCircularRect();
            using (var pen = new Pen(backColor, circularThickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, rect, 0, 360);
            }
        }

        private void DrawCircularDeterminate(Graphics g)
        {
            double percentage = GetPercentage();
            float sweepAngle = (float)(360 * percentage / 100.0);

            if (sweepAngle <= 0)
            {
                return;
            }

            var rect = GetCircularRect();
            var progressColor = GetActualProgressBarColor();

            using (var pen = new Pen(progressColor, circularThickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, rect, circularStartAngle, sweepAngle);
            }
        }

        private void DrawCircularIndeterminate(Graphics g)
        {
            float sweepAngle = 90;
            float startAngle = circularStartAngle + (indeterminatePosition * 360);

            var rect = GetCircularRect();
            var progressColor = GetActualProgressBarColor();

            using (var pen = new Pen(progressColor, circularThickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, rect, startAngle, sweepAngle);
            }
        }

        private Rectangle GetCircularRect()
        {
            int size = Math.Min(Width, Height) - circularThickness;
            int x = (Width - size) / 2;
            int y = (Height - size) / 2;
            return new Rectangle(x, y, size, size);
        }

        #endregion

        #region 文本绘制

        private void DrawProgressText(Graphics g)
        {
            string text = GetProgressText();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var textColor = GetActualTextColor();
            var textSize = g.MeasureString(text, progressFont);

            PointF location;
            switch (textPosition)
            {
                case ProgressTextPosition.Left:
                    location = new PointF(
                        4,
                        (Height - textSize.Height) / 2);
                    break;

                case ProgressTextPosition.Right:
                    location = new PointF(
                        Width - textSize.Width - 4,
                        (Height - textSize.Height) / 2);
                    break;

                case ProgressTextPosition.Center:
                default:
                    location = new PointF(
                        (Width - textSize.Width) / 2,
                        (Height - textSize.Height) / 2);
                    break;
            }

            // 绘制文本阴影(提高可读性)
            using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
            {
                g.DrawString(text, progressFont, shadowBrush,
                    location.X + 1, location.Y + 1);
            }

            // 绘制文本
            using (var textBrush = new SolidBrush(textColor))
            {
                g.DrawString(text, progressFont, textBrush, location);
            }
        }

        #endregion

        #endregion

        #region 主题支持

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                // 应用主题字体
                if (progressFont == null || progressFont == Font)
                {
                    progressFont = GetThemeFont(t => t.Body, Font);
                }

                Invalidate();
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                indeterminateTimer?.Dispose();
                progressFont?.Dispose();

                if (singleTask != null)
                {
                    singleTask.ProgressChanged -= OnSingleTaskProgressChanged;
                }

                tasks.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion
    }


#region 枚举和辅助类

/// <summary>
/// 进度条模式
/// </summary>
public enum ProgressMode
    {
        /// <summary>
        /// 循环模式(不确定进度)
        /// </summary>
        Indeterminate,

        /// <summary>
        /// 进度模式(确定进度)
        /// </summary>
        Determinate
    }

    /// <summary>
    /// 进度条样式
    /// </summary>
    public enum ProgressStyle
    {
        /// <summary>
        /// 线性进度条
        /// </summary>
        Linear,

        /// <summary>
        /// 圆形进度条
        /// </summary>
        Circular,

        /// <summary>
        /// 分段进度条
        /// </summary>
        Segmented
    }

    /// <summary>
    /// 文本显示位置
    /// </summary>
    public enum ProgressTextPosition
    {
        Center,
        Left,
        Right
    }

    /// <summary>
    /// 进度任务接口
    /// </summary>
    public interface IProgressTask
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 任务进度 (0-100)
        /// </summary>
        double Progress { get; set; }

        /// <summary>
        /// 任务权重
        /// </summary>
        double Weight { get; set; }

        /// <summary>
        /// 进度变更事件
        /// </summary>
        event EventHandler ProgressChanged;

        /// <summary>
        /// 权重变更事件
        /// </summary>
        event EventHandler WeightChanged;
    }

    /// <summary>
    /// 进度任务实现
    /// </summary>
    public class ProgressTask : IProgressTask
    {
        private string name = "";
        private double progress = 0;
        private double weight = 1.0;

        // 事件
        public event EventHandler ProgressChanged;
        public event EventHandler WeightChanged;

        public ProgressTask() { }

        public ProgressTask(string name, double weight = 1.0)
        {
            this.name = name;
            this.weight = Math.Max(0, weight);
        }

        public string Name
        {
            get => name;
            set => name = value ?? "";
        }

        public double Progress
        {
            get => progress;
            set
            {
                var newValue = Math.Max(0, Math.Min(100, value));
                if (Math.Abs(progress - newValue) > 0.001)
                {
                    progress = newValue;
                    OnProgressChanged();
                }
            }
        }

        public double Weight
        {
            get => weight;
            set
            {
                var newValue = Math.Max(0, value);
                if (Math.Abs(weight - newValue) > 0.001)
                {
                    weight = newValue;
                    OnWeightChanged();
                }
            }
        }


        protected virtual void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnWeightChanged()
        {
            WeightChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return $"{Name} ({Progress:F1}%, Weight: {Weight:F2})";
        }
    }

    /// <summary>
    /// 进度任务集合
    /// </summary>
    public class ProgressTaskCollection : Collection<IProgressTask>
    {
        public event EventHandler CollectionChanged;
        public event EventHandler ProgressChanged;

        protected override void InsertItem(int index, IProgressTask item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            base.InsertItem(index, item);
            item.ProgressChanged += OnTaskProgressChanged;
            item.WeightChanged += OnTaskWeightChanged;
            OnCollectionChanged();
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            item.ProgressChanged -= OnTaskProgressChanged;
            item.WeightChanged -= OnTaskWeightChanged;
            base.RemoveItem(index);
            OnCollectionChanged();
        }

        protected override void ClearItems()
        {
            foreach (var task in this)
            {
                task.ProgressChanged -= OnTaskProgressChanged;
                task.WeightChanged -= OnTaskWeightChanged;
            }
            base.ClearItems();
            OnCollectionChanged();
        }

        protected override void SetItem(int index, IProgressTask item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var oldItem = this[index];
            oldItem.ProgressChanged -= OnTaskProgressChanged;
            oldItem.WeightChanged -= OnTaskWeightChanged;

            base.SetItem(index, item);
            item.ProgressChanged += OnTaskProgressChanged;
            item.WeightChanged += OnTaskWeightChanged;
            OnCollectionChanged();
        }

        private void OnTaskProgressChanged(object sender, EventArgs e)
        {
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTaskWeightChanged(object sender, EventArgs e)
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 获取总进度(根据权重计算)
        /// </summary>
        public double GetTotalProgress()
        {
            if (Count == 0)
            {
                return 0;
            }

            double totalWeight = this.Sum(t => t.Weight);
            if (totalWeight <= 0)
            {
                return 0;
            }

            double weightedProgress = this.Sum(t => t.Progress * t.Weight);
            return weightedProgress / totalWeight;
        }

        /// <summary>
        /// 标准化权重(平均分配)
        /// </summary>
        public void NormalizeWeights()
        {
            if (Count == 0)
            {
                return;
            }

            foreach (var task in this)
            {
                task.Weight = 1.0;
            }
        }

        /// <summary>
        /// 按指定权重分配
        /// </summary>
        public void SetWeights(params double[] weights)
        {
            if (weights == null || weights.Length == 0)
            {
                NormalizeWeights();
                return;
            }

            int count = Math.Min(Count, weights.Length);
            for (int i = 0; i < count; i++)
            {
                this[i].Weight = Math.Max(0, weights[i]);
            }
        }

        /// <summary>
        /// 添加任务并自动标准化权重
        /// </summary>
        public void AddWithNormalize(IProgressTask task)
        {
            Add(task);
            NormalizeWeights();
        }
    }

    #endregion
}
