using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using FluentControls.Themes;
using System.Drawing.Drawing2D;

namespace FluentControls.Controls
{
    public class FluentCheckBox : FluentControlBase
    {
        private bool isChecked;
        private CheckBoxStyle checkBoxStyle;
        private int checkBoxSize = 18;
        private ContentAlignment checkAlign;
        private int spacing;
        private float switchAnimationProgress;
        private Timer switchAnimationTimer;

        // 独立点击模式
        private bool independentClickMode = false;
        private Rectangle currentCheckBoxRect;
        private Rectangle currentTextRect;

        public event EventHandler TextClicked;
        public event EventHandler CheckedChanged;

        public FluentCheckBox()
        {
            isChecked = false;
            checkBoxStyle = CheckBoxStyle.Standard;
            checkAlign = ContentAlignment.MiddleLeft;
            spacing = 8;
            switchAnimationProgress = 0f;

            Size = new Size(120, 32);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            ForeColor = SystemColors.ControlText;
            Font = SystemFonts.DefaultFont;

            EnableRippleEffect = true;
            ShadowLevel = 0;
        }

        #region 属性

        /// <summary>
        /// 选中状态
        /// </summary>
        [Category("Fluent")]
        [Description("获取或设置复选框是否被选中")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnCheckedChanged(EventArgs.Empty);

                    if (EnableAnimation && checkBoxStyle == CheckBoxStyle.Switch)
                    {
                        AnimateSwitchToggle();
                    }
                    else
                    {
                        switchAnimationProgress = isChecked ? 1f : 0f;
                        Invalidate();
                    }
                }
            }
        }

        [Category("Fluent")]
        [Description("获取或设置复选框是否被选中")]
        [DefaultValue(18)]
        public int CheckBoxSize
        {
            get { return checkBoxSize; }
            set
            {
                checkBoxSize = Math.Min(40, Math.Max(12, checkBoxSize));
                Invalidate();
            }
        }

        /// <summary>
        /// 复选框样式
        /// </summary>
        [Category("Fluent")]
        [Description("复选框的显示样式")]
        [DefaultValue(CheckBoxStyle.Standard)]
        public CheckBoxStyle CheckBoxStyle
        {
            get { return checkBoxStyle; }
            set
            {
                if (checkBoxStyle != value)
                {
                    checkBoxStyle = value;
                    switchAnimationProgress = isChecked ? 1f : 0f;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 复选框位置
        /// </summary>
        [Category("Fluent")]
        [Description("复选框相对于文本的位置")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get { return checkAlign; }
            set
            {
                if (checkAlign != value)
                {
                    checkAlign = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 复选框与文本间距
        /// </summary>
        [Category("Fluent")]
        [Description("复选框与文本之间的间距")]
        [DefaultValue(8)]
        public int Spacing
        {
            get { return spacing; }
            set
            {
                if (spacing != value && value >= 0)
                {
                    spacing = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 独立点击模式
        /// </summary>
        [Category("Fluent")]
        [Description("启用独立点击模式")]
        [DefaultValue(false)]
        public bool IndependentClickMode
        {
            get { return independentClickMode; }
            set
            {
                if (independentClickMode != value)
                {
                    independentClickMode = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region 事件

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            if (CheckedChanged != null)
            {
                CheckedChanged.Invoke(this, e);
            }
        }

        /// <summary>
        /// 触发文本点击事件
        /// </summary>
        protected virtual void OnTextClicked(EventArgs e)
        {
            TextClicked?.Invoke(this, e);
        }

        #endregion

        #region 重写方法

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
            Font = Theme.Typography.Body;
            ForeColor = Theme.Colors.TextPrimary;
        }

        protected override void InitializeDefaultStyles()
        {
            base.InitializeDefaultStyles();
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
        }

        protected override void ApplyThemeStyles()
        {

            if (Theme == null)
            {
                return;
            }

            BackColor = Theme.Colors.Surface;
            ForeColor = Theme.Colors.TextPrimary;
        }


        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
            {
                if (independentClickMode)
                {
                    // 独立点击模式：根据点击位置决定行为
                    if (IsPointInCheckBox(e.Location))
                    {
                        // 点击复选框区域 - 切换选中状态
                        Checked = !Checked;
                    }
                    else if (IsPointInText(e.Location))
                    {
                        // 点击文本区域 - 触发文本点击事件
                        OnTextClicked(EventArgs.Empty);
                    }
                }
                else
                {
                    // 普通模式：点击任意位置都切换状态
                    Checked = !Checked;
                }
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            // 始终绘制背景, 确保设计时和运行时一致
            Color bgColor = BackColor;

            // 如果背景色是透明的, 并且父控件存在, 使用父控件背景色
            if (bgColor == Color.Transparent && Parent != null)
            {
                bgColor = Parent.BackColor;
            }

            // 如果还是透明, 使用主题背景色
            if (bgColor == Color.Transparent)
            {
                bgColor = Theme.Colors.Background;
            }

            using (SolidBrush brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (checkBoxStyle == CheckBoxStyle.Switch)
            {
                DrawSwitch(g);
            }
            else
            {
                DrawStandard(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {

        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            // 当父控件背景色改变时重绘
            if (BackColor == Color.Transparent)
            {
                Invalidate();
            }
        }

        #endregion

        #region 绘制方法

        /// <summary>
        /// 绘制标准复选框
        /// </summary>
        private void DrawStandard(Graphics g)
        {
            // 计算复选框和文本位置
            bool checkOnLeft = IsCheckOnLeft(checkAlign);

            // 计算并保存复选框和文本位置
            if (checkOnLeft)
            {
                currentCheckBoxRect = new Rectangle(0, (Height - checkBoxSize) / 2, checkBoxSize, checkBoxSize);
                currentTextRect = new Rectangle(checkBoxSize + spacing, 0, Width - checkBoxSize - spacing, Height);
            }
            else
            {
                currentTextRect = new Rectangle(0, 0, Width - checkBoxSize - spacing, Height);
                currentCheckBoxRect = new Rectangle(Width - checkBoxSize, (Height - checkBoxSize) / 2, checkBoxSize, checkBoxSize);
            }

            // 获取颜色
            Color boxColor = GetBoxBackColor();
            Color borderColor = GetBoxBorderColor();

            // 绘制复选框背景
            using (SolidBrush brush = new SolidBrush(boxColor))
            using (Pen pen = new Pen(borderColor, 2f))
            {
                GraphicsPath boxPath = GetRoundedRectangle(currentCheckBoxRect, Theme.Elevation.CornerRadiusSmall);
                g.FillPath(brush, boxPath);
                g.DrawPath(pen, boxPath);
            }

            // 绘制勾选标记
            if (isChecked)
            {
                DrawCheckMark(g, currentCheckBoxRect);
            }

            // 绘制文本
            if (!string.IsNullOrEmpty(Text))
            {
                Color textColor = Enabled ? ForeColor : Theme.Colors.TextDisabled;
                TextFormatFlags flags = TextFormatFlags.VerticalCenter;
                flags |= checkOnLeft ? TextFormatFlags.Left : TextFormatFlags.Right;

                TextRenderer.DrawText(g, Text, Font, currentTextRect, textColor, flags);
            }
        }

        /// <summary>
        /// 绘制开关样式
        /// </summary>
        private void DrawSwitch(Graphics g)
        {
            int switchWidth = 44;
            int switchHeight = 24;
            int thumbSize = 18;

            // 开关位置(保存为复选框区域)
            currentCheckBoxRect = new Rectangle(
                (Width - switchWidth) / 2,
                (Height - switchHeight) / 2,
                switchWidth,
                switchHeight);

            // 文本区域
            currentTextRect = new Rectangle(0, 0, Width, Height);

            // 绘制开关轨道
            Color trackColor = GetSwitchTrackColor();

            using (SolidBrush brush = new SolidBrush(trackColor))
            using (Pen pen = new Pen(Theme.Colors.Border, 1.5f))
            {
                GraphicsPath trackPath = GetRoundedRectangle(currentCheckBoxRect, switchHeight / 2);
                g.FillPath(brush, trackPath);

                if (!isChecked || State == ControlState.Normal)
                {
                    g.DrawPath(pen, trackPath);
                }
            }

            // 计算滑块位置
            int thumbPadding = 3;
            float thumbX = currentCheckBoxRect.X + thumbPadding +
                          (switchAnimationProgress * (currentCheckBoxRect.Width - thumbSize - thumbPadding * 2));
            int thumbY = currentCheckBoxRect.Y + (currentCheckBoxRect.Height - thumbSize) / 2;

            // 绘制滑块阴影
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                RectangleF shadowRect = new RectangleF(thumbX + 1, thumbY + 2, thumbSize, thumbSize);
                g.FillEllipse(shadowBrush, shadowRect);
            }

            // 绘制滑块
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                RectangleF thumbRect = new RectangleF(thumbX, thumbY, thumbSize, thumbSize);
                g.FillEllipse(brush, thumbRect);
            }
        }

        /// <summary>
        /// 绘制勾选标记
        /// </summary>
        private void DrawCheckMark(Graphics g, Rectangle rect)
        {
            using (Pen pen = new Pen(Theme.Colors.TextOnPrimary, 2.5f))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                // 勾选标记路径
                Point[] checkPoints = new Point[]
                {
                    new Point(rect.X + rect.Width / 4, rect.Y + rect.Height / 2),
                    new Point(rect.X + rect.Width / 2 - 1, rect.Y + rect.Height * 2 / 3),
                    new Point(rect.X + rect.Width * 3 / 4, rect.Y + rect.Height / 3)
                };

                g.DrawLines(pen, checkPoints);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 判断点是否在复选框区域内
        /// </summary>
        private bool IsPointInCheckBox(Point point)
        {
            // 稍微扩大点击区域以提升用户体验
            Rectangle expandedRect = currentCheckBoxRect;
            int expandSize = 4;
            expandedRect.Inflate(expandSize, expandSize);
            return expandedRect.Contains(point);
        }

        /// <summary>
        /// 判断点是否在文本区域内
        /// </summary>
        private bool IsPointInText(Point point)
        {
            if (checkBoxStyle == CheckBoxStyle.Switch)
            {
                // Switch 样式下, 非开关区域都是文本区域
                return !IsPointInCheckBox(point) && currentTextRect.Contains(point);
            }
            return currentTextRect.Contains(point);
        }

        /// <summary>
        /// 获取复选框背景色
        /// </summary>
        private Color GetBoxBackColor()
        {
            if (!Enabled)
            {
                return Theme.Colors.Surface;
            }

            if (isChecked)
            {
                switch (State)
                {
                    case ControlState.Pressed:
                        return Theme.Colors.AccentDark;
                    case ControlState.Hover:
                        return Theme.Colors.AccentLight;
                    default:
                        return Theme.Colors.Accent;
                }
            }
            else
            {
                switch (State)
                {
                    case ControlState.Pressed:
                        return Theme.Colors.SurfacePressed;
                    case ControlState.Hover:
                        return Theme.Colors.SurfaceHover;
                    default:
                        return Theme.Colors.Surface;
                }
            }
        }

        /// <summary>
        /// 获取复选框边框色
        /// </summary>
        private Color GetBoxBorderColor()
        {
            if (!Enabled)
            {
                return Theme.Colors.BorderLight;
            }

            if (isChecked)
            {
                return Theme.Colors.Accent;
            }

            if (State == ControlState.Hover || State == ControlState.Focused)
            {
                return Theme.Colors.BorderFocused;
            }

            return Theme.Colors.Border;
        }

        /// <summary>
        /// 获取开关轨道颜色
        /// </summary>
        private Color GetSwitchTrackColor()
        {
            if (!Enabled)
            {
                return Theme.Colors.Surface;
            }

            if (isChecked)
            {
                switch (State)
                {
                    case ControlState.Pressed:
                        return Theme.Colors.AccentDark;
                    case ControlState.Hover:
                        return Theme.Colors.AccentLight;
                    default:
                        return Theme.Colors.Accent;
                }
            }
            else
            {
                switch (State)
                {
                    case ControlState.Pressed:
                        return Theme.Colors.SurfacePressed;
                    case ControlState.Hover:
                        return Theme.Colors.SurfaceHover;
                    default:
                        return Theme.Colors.Surface;
                }
            }
        }

        /// <summary>
        /// 判断复选框是否在左侧
        /// </summary>
        private bool IsCheckOnLeft(ContentAlignment align)
        {
            return align == ContentAlignment.TopLeft ||
                   align == ContentAlignment.MiddleLeft ||
                   align == ContentAlignment.BottomLeft;
        }

        /// <summary>
        /// 开关切换动画
        /// </summary>
        private void AnimateSwitchToggle()
        {
            float targetProgress = isChecked ? 1f : 0f;
            float startProgress = switchAnimationProgress;

            int duration = Theme.Animation.FastDuration;
            int steps = duration / 16;
            int currentStep = 0;

            if (switchAnimationTimer != null)
            {
                switchAnimationTimer.Stop();
                switchAnimationTimer.Dispose();
            }

            switchAnimationTimer = new Timer();
            switchAnimationTimer.Interval = 16;
            switchAnimationTimer.Tick += (s, e) =>
            {
                currentStep++;
                float progress = Math.Min(1f, (float)currentStep / steps);
                float easedProgress = (float)Theme.Animation.DefaultEasing(progress);

                switchAnimationProgress = startProgress + (targetProgress - startProgress) * easedProgress;
                Invalidate();

                if (currentStep >= steps)
                {
                    switchAnimationProgress = targetProgress;
                    switchAnimationTimer.Stop();
                    switchAnimationTimer.Dispose();
                    switchAnimationTimer = null;
                    Invalidate();
                }
            };
            switchAnimationTimer.Start();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (switchAnimationTimer != null)
                {
                    switchAnimationTimer.Stop();
                    switchAnimationTimer.Dispose();
                    switchAnimationTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    /// <summary>
    /// 复选框样式
    /// </summary>
    public enum CheckBoxStyle
    {
        /// <summary>
        /// 标准复选框
        /// </summary>
        Standard,

        /// <summary>
        /// 开关样式
        /// </summary>
        Switch
    }
}
