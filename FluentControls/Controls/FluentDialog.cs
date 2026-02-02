using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentControls.Animation;
using FluentControls.Themes;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent对话框基类
    /// </summary>
    public class FluentDialog : FluentForm
    {
        private DialogType dialogType;
        private DialogButtons dialogButtons;
        private Panel buttonPanel;
        private Panel contentPanel;
        private TextBox inputTextBox;
        private Label messageLabel;
        private PictureBox iconPictureBox;
        private List<FluentButton> buttons;
        private DwmWindowCorner cornerStyle = DwmWindowCorner.Default;

        #region 构造函数

        public FluentDialog() : this(DialogType.Information)
        {
        }

        public FluentDialog(DialogType type, DialogButtons? buttons = null, Size? size = null)
        {
            InitializeDialog();
            DialogType = type;

            if (size.HasValue)
            {
                this.Size = size.Value;
            }

            // 设置按钮组
            if (buttons.HasValue)
            {
                DialogButtons = buttons.Value;
            }
            else
            {
                DialogButtons = GetDefaultButtons(type);
            }
        }

        private void InitializeDialog()
        {
            // 设置对话框特性
            base.CanResize = false;
            CornerRadius = 0;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(300, 150);
            Size = new Size(300, 180);

            // 隐藏最大化和最小化按钮
            TitleBar.ShowMaximizeButton = false;
            TitleBar.ShowMinimizeButton = false;

            // 初始化内容面板
            InitializeContentPanel();

            // 初始化按钮面板
            InitializeButtonPanel();

            buttons = new List<FluentButton>();
        }

        private void InitializeContentPanel()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = (DialogType == DialogType.Custom) ? true : false
            };

            // 图标
            iconPictureBox = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(20, 36),  //20 +(80-48)/2
                SizeMode = PictureBoxSizeMode.Zoom
            };
            contentPanel.Controls.Add(iconPictureBox);

            // 消息标签
            messageLabel = new Label
            {
                Location = new Point(80, 20),
                AutoSize = false,
                Size = new Size(230, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            contentPanel.Controls.Add(messageLabel);

            // 输入框(仅输入对话框显示)
            inputTextBox = new TextBox
            {
                Location = new Point(80, 100),
                Size = new Size(230, 25),
                Visible = false
            };
            contentPanel.Controls.Add(inputTextBox);

            Controls.Add(contentPanel);
        }

        private void InitializeButtonPanel()
        {
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5)
            };

            Controls.Add(buttonPanel);
        }

        #endregion

        #region 属性

        [Category("Dialog")]
        [Description("对话框类型")]
        public DialogType DialogType
        {
            get => dialogType;
            set
            {
                dialogType = value;
                UpdateDialogAppearance();
            }
        }

        [Category("Dialog")]
        [Description("对话框按钮")]
        public DialogButtons DialogButtons
        {
            get => dialogButtons;
            set
            {
                dialogButtons = value;
                CreateButtons();
            }
        }

        [Category("Dialog")]
        [Description("消息文本")]
        public string Message
        {
            get => messageLabel?.Text;
            set
            {
                if (messageLabel != null)
                {
                    messageLabel.Text = value;
                }
            }
        }

        [Category("Dialog")]
        [Description("输入框默认文本")]
        public string InputText
        {
            get => inputTextBox?.Text;
            set
            {
                if (inputTextBox != null)
                {
                    inputTextBox.Text = value;
                }
            }
        }

        [Category("Dialog")]
        [Description("对话框圆角样式")]
        public DwmWindowCorner CornerStyle
        {
            get => cornerStyle;
            set
            {
                cornerStyle = value;
                this.SetWindowCorner(value);
                this.Invalidate();
            }
        }


        [Browsable(false)]
        public new bool CanResize => false;

        [Browsable(false)]
        public new bool ShowInTaskbar => false;

        #endregion

        #region 重写方法

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle &= ~0x00040000; // 去掉 WS_EX_APPWINDOW
                cp.ExStyle |= 0x00000080;  // 增加 WS_EX_TOOLWINDOW
                return cp;
            }
        }

        #endregion

        #region 辅助方法

        public void AddCustomControl(Control control)
        {
            if (dialogType == DialogType.Custom)
            {
                contentPanel.Controls.Add(control);
            }
        }

        private void UpdateDialogAppearance()
        {
            switch (dialogType)
            {
                case DialogType.Information:
                    iconPictureBox.Image = GetDialogIcon("ℹ", Theme.Colors.Info);
                    TitleBar.Title = "信息";
                    Size = new Size(320, 180);
                    break;

                case DialogType.Warning:
                    iconPictureBox.Image = GetDialogIcon("⚠", Theme.Colors.Warning);
                    TitleBar.Title = "警告";
                    Size = new Size(320, 180);
                    break;

                case DialogType.Error:
                    iconPictureBox.Image = GetDialogIcon("✕", Theme.Colors.Error);
                    TitleBar.Title = "错误";
                    Size = new Size(320, 180);
                    break;

                case DialogType.Question:
                    iconPictureBox.Image = GetDialogIcon("?", Theme.Colors.Primary);
                    TitleBar.Title = "确认";
                    Size = new Size(320, 180);
                    break;

                case DialogType.Input:
                    iconPictureBox.Image = GetDialogIcon("✎", Theme.Colors.Primary);
                    inputTextBox.Visible = true;
                    TitleBar.Title = "输入";
                    Size = new Size(350, 220);
                    break;

                case DialogType.Custom:
                    iconPictureBox.Visible = false;
                    messageLabel.Visible = false;
                    inputTextBox.Visible = false;
                    contentPanel.Controls.Remove(iconPictureBox);
                    contentPanel.Controls.Remove(messageLabel);
                    contentPanel.Controls.Remove(inputTextBox);

                    // 调整内容面板填充
                    contentPanel.Padding = new Padding(8);
                    break;
            }
        }

        private Image GetDialogIcon(string symbol, Color color)
        {
            var bitmap = new Bitmap(48, 48);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                using (var brush = new SolidBrush(color))
                using (var font = new Font("Segoe UI", 24, FontStyle.Regular))
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(symbol, font, brush, new RectangleF(0, 0, 48, 48), format);
                }
            }
            return bitmap;
        }

        private DialogButtons GetDefaultButtons(DialogType type)
        {
            switch (type)
            {
                case DialogType.Information:
                case DialogType.Warning:
                case DialogType.Error:
                    return DialogButtons.OK;

                case DialogType.Question:
                    return DialogButtons.YesNo;

                case DialogType.Input:
                    return DialogButtons.OKCancel;
                case DialogType.Custom:
                    return DialogButtons.OK;
                default:
                    return DialogButtons.OK;
            }
        }

        private void CreateButtons()
        {
            buttonPanel.Controls.Clear();
            buttons.Clear();

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            switch (dialogButtons)
            {
                case DialogButtons.OK:
                    AddButton(flowPanel, "确定", DialogResult.OK, true);
                    break;

                case DialogButtons.OKCancel:
                    AddButton(flowPanel, "取消", DialogResult.Cancel, false);
                    AddButton(flowPanel, "确定", DialogResult.OK, true);
                    break;

                case DialogButtons.YesNo:
                    AddButton(flowPanel, "否", DialogResult.No, false);
                    AddButton(flowPanel, "是", DialogResult.Yes, true);
                    break;

                case DialogButtons.YesNoCancel:
                    AddButton(flowPanel, "取消", DialogResult.Cancel, false);
                    AddButton(flowPanel, "否", DialogResult.No, false);
                    AddButton(flowPanel, "是", DialogResult.Yes, true);
                    break;

                case DialogButtons.RetryCancel:
                    AddButton(flowPanel, "取消", DialogResult.Cancel, false);
                    AddButton(flowPanel, "重试", DialogResult.Retry, true);
                    break;

                case DialogButtons.AbortRetryIgnore:
                    AddButton(flowPanel, "忽略", DialogResult.Ignore, false);
                    AddButton(flowPanel, "重试", DialogResult.Retry, false);
                    AddButton(flowPanel, "中止", DialogResult.Abort, true);
                    break;
            }

            buttonPanel.Controls.Add(flowPanel);
        }

        private void AddButton(FlowLayoutPanel panel, string text, DialogResult result, bool isPrimary)
        {
            var button = new FluentButton
            {
                Text = text,
                Size = new Size(100, 30),
                UseTheme = true,
                ThemeName = ThemeManager.CurrentTheme?.Name ?? nameof(LightTheme),
                ButtonStyle = isPrimary ? ButtonStyle.Primary : ButtonStyle.Secondary,
                DialogResult = result,
                Margin = new Padding(5, 0, 5, 0),
                CornerRadius = 2
            };

            button.Click += (s, e) =>
            {
                if (dialogType == DialogType.Input && result == DialogResult.OK)
                {
                    // 验证输入
                    if (string.IsNullOrWhiteSpace(inputTextBox.Text))
                    {
                        AnimationManager.AnimateShake(inputTextBox, 5, 300);
                        return;
                    }
                }

                this.DialogResult = result;
                this.Close();
            };

            buttons.Add(button);
            panel.Controls.Add(button);

            if (isPrimary)
            {
                this.AcceptButton = button;
            }

            if (result == DialogResult.Cancel || result == DialogResult.No)
            {
                this.CancelButton = button;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // 显示动画
            this.Opacity = 0;
            AnimationManager.AnimateFade(this, 1.0, 200);

            if (dialogType == DialogType.Input)
            {
                inputTextBox.Focus();
                inputTextBox.SelectAll();
            }
        }

        #endregion

        #region 静态方法

        public static DialogResult Show(string message, string title = "信息", DialogType type = DialogType.Information)
        {
            using (var dialog = new FluentDialog(type))
            {
                dialog.TitleBar.Title = title;
                dialog.Message = message;
                return dialog.ShowDialog();
            }
        }

        public static DialogResult Show(IWin32Window owner, string message, string title = "信息", DialogType type = DialogType.Information)
        {
            using (var dialog = new FluentDialog(type))
            {
                dialog.TitleBar.Title = title;
                dialog.Message = message;
                return dialog.ShowDialog(owner);
            }
        }

        public static string InputBox(string prompt, string title = "输入", string defaultValue = "")
        {
            using (var dialog = new FluentDialog(DialogType.Input))
            {
                dialog.TitleBar.Title = title;
                dialog.Message = prompt;
                dialog.InputText = defaultValue;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.InputText;
                }

                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// 对话框类型
    /// </summary>
    public enum DialogType
    {
        Information,
        Warning,
        Error,
        Question,
        Input,
        Custom
    }

    /// <summary>
    /// 对话框按钮
    /// </summary>
    public enum DialogButtons
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel,
        RetryCancel,
        AbortRetryIgnore
    }
}
