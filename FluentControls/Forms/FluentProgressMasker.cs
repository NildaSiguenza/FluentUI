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
    /// <summary>
    /// 带进度显示的遮罩面板
    /// </summary>
    public class FluentProgressMasker : FluentMasker
    {
        #region 字段

        private double progress = 0;
        private string statusMessage = "";
        private bool showProgress = true;
        private bool showPercentage = true;
        private ProgressDisplayStyle progressStyle = ProgressDisplayStyle.Bar;

        private Color progressBackColor = Color.FromArgb(60, 60, 60);
        private Color progressForeColor = Color.FromArgb(0, 120, 212);
        private int progressBarHeight = 6;
        private int progressBarWidth = 200;

        private bool showCancelButton = false;
        private Button cancelButton;

        #endregion

        #region 事件

        /// <summary>
        /// 取消请求事件
        /// </summary>
        public event EventHandler CancelRequested;

        #endregion

        #region 构造函数

        public FluentProgressMasker() : base()
        {
            InitializeCancelButton();
        }

        public FluentProgressMasker(Control target) : base(target)
        {
            InitializeCancelButton();
        }

        private void InitializeCancelButton()
        {
            cancelButton = new Button
            {
                Text = "取消",
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 9F),
                Cursor = Cursors.Hand,
                Visible = false,
                TabStop = false
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(130, 130, 130);
            cancelButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 80, 80);
            cancelButton.Click += CancelButton_Click;

            this.Controls.Add(cancelButton);
        }

        #endregion

        #region 属性

        /// <summary>
        /// 当前进度 (0-1)
        /// </summary>
        [Category("Progress")]
        [Description("当前进度值 (0.0 - 1.0)")]
        [DefaultValue(0.0)]
        public double Progress
        {
            get => progress;
            set
            {
                progress = Math.Max(0, Math.Min(1, value));
                SafeInvalidate();
            }
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        [Category("Progress")]
        [Description("状态消息文本")]
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value ?? "";
                SafeInvalidate();
            }
        }

        /// <summary>
        /// 是否显示进度
        /// </summary>
        [Category("Progress")]
        [DefaultValue(true)]
        public bool ShowProgress
        {
            get => showProgress;
            set
            {
                showProgress = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 是否显示百分比
        /// </summary>
        [Category("Progress")]
        [DefaultValue(true)]
        public bool ShowPercentage
        {
            get => showPercentage;
            set
            {
                showPercentage = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 进度显示样式
        /// </summary>
        [Category("Progress")]
        [DefaultValue(ProgressDisplayStyle.Bar)]
        public ProgressDisplayStyle ProgressStyle
        {
            get => progressStyle;
            set
            {
                progressStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 进度条背景色
        /// </summary>
        [Category("Progress")]
        public Color ProgressBackColor
        {
            get => progressBackColor;
            set
            {
                progressBackColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 进度条前景色
        /// </summary>
        [Category("Progress")]
        public Color ProgressForeColor
        {
            get => progressForeColor;
            set
            {
                progressForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 进度条高度
        /// </summary>
        [Category("Progress")]
        [DefaultValue(6)]
        public int ProgressBarHeight
        {
            get => progressBarHeight;
            set
            {
                progressBarHeight = Math.Max(2, value);
                Invalidate();
            }
        }

        /// <summary>
        /// 进度条宽度
        /// </summary>
        [Category("Progress")]
        [DefaultValue(200)]
        public int ProgressBarWidth
        {
            get => progressBarWidth;
            set
            {
                progressBarWidth = Math.Max(50, value);
                Invalidate();
            }
        }

        /// <summary>
        /// 是否显示取消按钮
        /// </summary>
        [Category("Progress")]
        [DefaultValue(false)]
        public bool ShowCancelButton
        {
            get => showCancelButton;
            set
            {
                showCancelButton = value;
                UpdateCancelButtonVisibility();
            }
        }

        #endregion

        #region 公共方法


        /// <summary>
        /// 更新进度
        /// </summary>
        public void UpdateProgress(double progress, string message = null)
        {
            this.progress = Math.Max(0, Math.Min(1, progress));
            if (message != null)
            {
                this.statusMessage = message;
            }
            SafeInvalidate();
        }

        /// <summary>
        /// 重置进度
        /// </summary>
        public void ResetProgress()
        {
            progress = 0;
            statusMessage = "";
            Invalidate();
        }

        private void SafeInvalidate()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(Invalidate));
            }
            else
            {
                Invalidate();
            }
        }

        #endregion

        #region 重写

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdateCancelButtonPosition();
            UpdateCancelButtonVisibility();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateCancelButtonPosition();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!IsShowing || !showProgress)
            {
                return;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int centerX = Width / 2;
            int centerY = Height / 2;
            int progressY = centerY + AnimationSize.Height / 2 + ContentSpacing + 20;

            switch (progressStyle)
            {
                case ProgressDisplayStyle.Bar:
                    DrawProgressBar(g, centerX, progressY);
                    break;
                case ProgressDisplayStyle.Circle:
                    DrawProgressCircle(g, centerX, progressY);
                    break;
                case ProgressDisplayStyle.Text:
                    DrawProgressText(g, centerX, progressY);
                    break;
            }

            // 绘制状态消息
            if (!string.IsNullOrEmpty(statusMessage))
            {
                int messageY = progressY + GetProgressAreaHeight() + 15;
                using (var brush = new SolidBrush(TextColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(statusMessage, TextFont, brush, centerX, messageY, sf);
                }
            }
        }

        private int GetProgressAreaHeight()
        {
            switch (progressStyle)
            {
                case ProgressDisplayStyle.Bar:
                    return progressBarHeight + (showPercentage ? 25 : 0);
                case ProgressDisplayStyle.Circle:
                    return 50;
                case ProgressDisplayStyle.Text:
                    return 30;
                default:
                    return 30;
            }
        }

        #endregion

        #region 绘制进度

        private void DrawProgressBar(Graphics g, int centerX, int y)
        {
            int barX = centerX - progressBarWidth / 2;

            // 背景
            using (var bgBrush = new SolidBrush(progressBackColor))
            {
                var bgRect = new Rectangle(barX, y, progressBarWidth, progressBarHeight);
                using (var path = CreateRoundedRectangle(bgRect, progressBarHeight / 2))
                {
                    g.FillPath(bgBrush, path);
                }
            }

            // 进度
            int pw = (int)(progressBarWidth * progress);
            if (pw > 0)
            {
                using (var fgBrush = new SolidBrush(progressForeColor))
                {
                    var fgRect = new Rectangle(barX, y, Math.Max(progressBarHeight, pw), progressBarHeight);
                    using (var path = CreateRoundedRectangle(fgRect, progressBarHeight / 2))
                    {
                        g.FillPath(fgBrush, path);
                    }
                }
            }

            // 百分比
            if (showPercentage)
            {
                string percentText = $"{(int)(progress * 100)}%";
                using (var brush = new SolidBrush(TextColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(percentText, TextFont, brush, centerX, y + progressBarHeight + 8, sf);
                }
            }
        }

        private void DrawProgressCircle(Graphics g, int centerX, int centerY)
        {
            int radius = 25;
            int thickness = 4;

            // 背景
            using (var pen = new Pen(progressBackColor, thickness))
            {
                g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            // 进度
            if (progress > 0)
            {
                using (var pen = new Pen(progressForeColor, thickness))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, centerX - radius, centerY - radius, radius * 2, radius * 2,
                        -90, (float)(progress * 360));
                }
            }

            // 百分比
            if (showPercentage)
            {
                string percentText = $"{(int)(progress * 100)}%";
                using (var brush = new SolidBrush(TextColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(percentText, TextFont, brush, centerX, centerY, sf);
                }
            }
        }

        private void DrawProgressText(Graphics g, int centerX, int y)
        {
            if (!showPercentage)
            {
                return;
            }

            string text = $"{(int)(progress * 100)}%";
            using (var brush = new SolidBrush(TextColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            using (var font = new Font(TextFont.FontFamily, 18, FontStyle.Bold))
            {
                g.DrawString(text, font, brush, centerX, y, sf);
            }
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter > rect.Height)
            {
                diameter = rect.Height;
            }

            if (diameter > rect.Width)
            {
                diameter = rect.Width;
            }

            radius = diameter / 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion

        #region 取消按钮

        private void UpdateCancelButtonPosition()
        {
            if (cancelButton == null)
            {
                return;
            }

            cancelButton.Location = new Point(
                (Width - cancelButton.Width) / 2,
                Height - cancelButton.Height - 50
            );
        }

        private void UpdateCancelButtonVisibility()
        {
            if (cancelButton != null)
            {
                cancelButton.Visible = showCancelButton && IsShowing;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 释放资源

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cancelButton?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    /// <summary>
    /// 进度显示样式
    /// </summary>
    public enum ProgressDisplayStyle
    {
        Bar,        // 进度条
        Circle,     // 圆形进度
        Text        // 仅文本
    }
}
