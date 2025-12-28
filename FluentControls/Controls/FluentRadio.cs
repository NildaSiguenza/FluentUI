using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    public class FluentRadio : FluentControlBase
    {
        private bool isChecked;
        private ContentAlignment radioAlign;
        private int spacing;
        private string groupName;
        private float checkAnimationProgress;
        private Timer checkAnimationTimer;

        public event EventHandler CheckedChanged;

        public FluentRadio()
        {
            isChecked = false;
            radioAlign = ContentAlignment.MiddleLeft;
            spacing = 8;
            groupName = "default";
            checkAnimationProgress = 0f;

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
        [Description("获取或设置单选框是否被选中")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;

                    if (isChecked)
                    {
                        UncheckOthersInGroup();
                    }

                    OnCheckedChanged(EventArgs.Empty);

                    if (EnableAnimation)
                    {
                        AnimateCheck();
                    }
                    else
                    {
                        checkAnimationProgress = isChecked ? 1f : 0f;
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 单选框位置
        /// </summary>
        [Category("Fluent")]
        [Description("单选框相对于文本的位置")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment RadioAlign
        {
            get { return radioAlign; }
            set
            {
                if (radioAlign != value)
                {
                    radioAlign = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 单选框与文本间距
        /// </summary>
        [Category("Fluent")]
        [Description("单选框与文本之间的间距")]
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
        /// 分组名称
        /// </summary>
        [Category("Fluent")]
        [Description("单选框所属的分组名称")]
        [DefaultValue("default")]
        public string GroupName
        {
            get { return groupName; }
            set
            {
                if (groupName != value)
                {
                    groupName = value ?? "default";
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
                Checked = true;
            }
        }

        protected override void DrawBackground(Graphics g)
        {
            // 单选框使用透明或设定的背景
            if (BackColor != Color.Transparent)
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            DrawRadio(g);
        }

        protected override void DrawBorder(Graphics g)
        {
            // 单选框自身处理边框
        }

        #endregion

        #region 绘制方法

        /// <summary>
        /// 绘制单选框
        /// </summary>
        private void DrawRadio(Graphics g)
        {
            int radioSize = 20;

            // 计算单选框和文本位置
            Rectangle radioRect;
            Rectangle textRect;

            bool radioOnLeft = IsRadioOnLeft(radioAlign);

            if (radioOnLeft)
            {
                radioRect = new Rectangle(0, (Height - radioSize) / 2, radioSize, radioSize);
                textRect = new Rectangle(radioSize + spacing, 0, Width - radioSize - spacing, Height);
            }
            else
            {
                textRect = new Rectangle(0, 0, Width - radioSize - spacing, Height);
                radioRect = new Rectangle(Width - radioSize, (Height - radioSize) / 2, radioSize, radioSize);
            }

            // 获取颜色
            Color borderColor = GetRadioBorderColor();
            Color backColor = GetRadioBackColor();

            // 绘制单选框外圈
            using (SolidBrush brush = new SolidBrush(backColor))
            using (Pen pen = new Pen(borderColor, 2f))
            {
                g.FillEllipse(brush, radioRect);
                g.DrawEllipse(pen, radioRect);
            }

            // 绘制选中状态的内圈
            if (checkAnimationProgress > 0)
            {
                DrawRadioInner(g, radioRect);
            }

            // 绘制文本
            if (!string.IsNullOrEmpty(Text))
            {
                Color textColor = Enabled ? ForeColor : Theme.Colors.TextDisabled;
                TextFormatFlags flags = TextFormatFlags.VerticalCenter;
                flags |= radioOnLeft ? TextFormatFlags.Left : TextFormatFlags.Right;

                TextRenderer.DrawText(g, Text, Font, textRect, textColor, flags);
            }
        }

        /// <summary>
        /// 绘制单选框内圈
        /// </summary>
        private void DrawRadioInner(Graphics g, Rectangle rect)
        {
            int maxInnerSize = 10;
            int innerSize = (int)(maxInnerSize * checkAnimationProgress);
            int innerPadding = (rect.Width - innerSize) / 2;

            Rectangle innerRect = new Rectangle(
                rect.X + innerPadding,
                rect.Y + innerPadding,
                innerSize,
                innerSize);

            Color innerColor = isChecked ? Theme.Colors.Accent : Theme.Colors.Border;

            using (SolidBrush brush = new SolidBrush(innerColor))
            {
                g.FillEllipse(brush, innerRect);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取单选框背景色
        /// </summary>
        private Color GetRadioBackColor()
        {
            if (!Enabled)
            {
                return Theme.Colors.Surface;
            }

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

        /// <summary>
        /// 获取单选框边框色
        /// </summary>
        private Color GetRadioBorderColor()
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
        /// 判断单选框是否在左侧
        /// </summary>
        private bool IsRadioOnLeft(ContentAlignment align)
        {
            return align == ContentAlignment.TopLeft ||
                   align == ContentAlignment.MiddleLeft ||
                   align == ContentAlignment.BottomLeft;
        }

        /// <summary>
        /// 取消同组其他单选框的选中状态
        /// </summary>
        private void UncheckOthersInGroup()
        {
            if (Parent == null)
            {
                return;
            }

            foreach (Control control in Parent.Controls)
            {
                FluentRadio radio = control as FluentRadio;
                if (radio != null && radio != this && radio.GroupName == this.GroupName)
                {
                    radio.isChecked = false;
                    radio.checkAnimationProgress = 0f;
                    radio.Invalidate();
                }
            }
        }

        /// <summary>
        /// 选中动画
        /// </summary>
        private void AnimateCheck()
        {
            float targetProgress = isChecked ? 1f : 0f;
            float startProgress = checkAnimationProgress;

            int duration = Theme.Animation.FastDuration;
            int steps = duration / 16;
            int currentStep = 0;

            if (checkAnimationTimer != null)
            {
                checkAnimationTimer.Stop();
                checkAnimationTimer.Dispose();
            }

            checkAnimationTimer = new Timer();
            checkAnimationTimer.Interval = 16;
            checkAnimationTimer.Tick += (s, e) =>
            {
                currentStep++;
                float progress = Math.Min(1f, (float)currentStep / steps);
                float easedProgress = (float)Theme.Animation.DefaultEasing(progress);

                checkAnimationProgress = startProgress + (targetProgress - startProgress) * easedProgress;
                Invalidate();

                if (currentStep >= steps)
                {
                    checkAnimationProgress = targetProgress;
                    checkAnimationTimer.Stop();
                    checkAnimationTimer.Dispose();
                    checkAnimationTimer = null;
                    Invalidate();
                }
            };
            checkAnimationTimer.Start();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (checkAnimationTimer != null)
                {
                    checkAnimationTimer.Stop();
                    checkAnimationTimer.Dispose();
                    checkAnimationTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
