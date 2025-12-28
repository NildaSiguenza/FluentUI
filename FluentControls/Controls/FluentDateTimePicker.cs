using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("Value")]
    public class FluentDateTimePicker : FluentControlBase
    {
        // 显示模式
        private DateTimePickerMode mode = DateTimePickerMode.DateTime;

        // 值相关
        private DateTime currentValue = DateTime.Now;
        private DateTime minimum = new DateTime(1753, 1, 1, 0, 0, 0);
        private DateTime maximum = DateTime.MaxValue;
        private bool includeMilliseconds = false;

        // 格式化相关
        private string customFormat = "";
        private string displayFormat = "";

        // 输入模式
        private DateTimeInputMode inputMode = DateTimeInputMode.Both;
        private TextBox textBox;
        private Button dropDownButton;
        private bool isDroppedDown = false;

        // 弹出面板
        private DateTimePickerPanel pickerPanel;
        private ToolStripDropDown dropDown;
        private ToolStripControlHost dropDownHost;

        // 倒计时模式
        private bool countdownMode = false;
        private TimeSpan countdownInitial = TimeSpan.FromMinutes(5);  // 倒计时初始值
        private TimeSpan countdownRemaining = TimeSpan.FromMinutes(5); // 剩余时间
        private TimeSpan countdownBase = TimeSpan.Zero;  // 倒计时基准值
        private DateTime countdownStartTime;  // 倒计时开始时间
        private Timer countdownTimer;
        private bool isCountdownRunning = false;  // 是否正在运行
        private bool isCountdownPaused = false;  // 是否暂停

        // 外观
        private int dropDownButtonWidth = 20;
        private bool showDropDownButton = true;
        private Color dropDownButtonColor;
        private Color dropDownButtonHoverColor;

        // 状态
        private bool isTextBoxFocused = false;
        private bool isButtonHovered = false;
        private bool isReadOnly = false;

        #region 构造函数

        public FluentDateTimePicker()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.Selectable, true);

            Size = new Size(200, 30);

            InitializeComponents();
            InitializeCountdownTimer();
            UpdateDisplayFormat();
            UpdateTextBoxValue();
        }

        private void InitializeComponents()
        {
            // 创建文本框
            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Location = new Point(4, 7),
                Width = Width - dropDownButtonWidth - 8
            };

            textBox.TextChanged += TextBox_TextChanged;
            textBox.KeyDown += TextBox_KeyDown;
            textBox.GotFocus += (s, e) =>
            {
                isTextBoxFocused = true;
                Invalidate();
            };
            textBox.LostFocus += (s, e) =>
            {
                isTextBoxFocused = false; ValidateTextInput();
                Invalidate();
            };

            Controls.Add(textBox);

            // 创建下拉按钮(虚拟按钮, 通过绘制实现)
            dropDownButton = new Button
            {
                Visible = false
            };

            // 创建选择面板
            pickerPanel = new DateTimePickerPanel(this);
            pickerPanel.ValueChanged += (s, e) =>
            {
                if (countdownMode)
                {
                    // 倒计时模式下, 从面板获取的时间转换为TimeSpan
                    var selectedTime = pickerPanel.SelectedDateTime;
                    var newDuration = new TimeSpan(
                        0,  // days
                        selectedTime.Hour,
                        selectedTime.Minute,
                        selectedTime.Second,
                        includeMilliseconds ? selectedTime.Millisecond : 0);

                    countdownInitial = newDuration;
                    if (!isCountdownRunning)
                    {
                        countdownRemaining = newDuration;
                    }
                    UpdateTextBoxValue();
                    OnValueChanged();  // 触发值变化事件
                }
                else
                {
                    SetValue(pickerPanel.SelectedDateTime, true);
                }
                CloseDropDown();
            };
        }

        private void InitializeCountdownTimer()
        {
            countdownTimer = new Timer
            {
                Interval = 100 // 100ms更新一次
            };
            countdownTimer.Tick += CountdownTimer_Tick;
        }

        #endregion

        #region 属性

        [Category("Data")]
        [Description("当前选中的日期时间值")]
        [RefreshProperties(RefreshProperties.All)]
        public DateTime Value
        {
            get
            {
                if (countdownMode)
                {
                    // 倒计时模式下, 将TimeSpan转换为DateTime返回(用于设计器)
                    return DateTime.Today.Add(countdownInitial);
                }
                return currentValue;
            }
            set
            {
                if (countdownMode)
                {
                    // 倒计时模式下, 从DateTime提取时间部分作为倒计时初始值
                    TimeSpan timeSpan = value.TimeOfDay;

                    // 如果时间为0, 保持默认的5分钟
                    if (timeSpan == TimeSpan.Zero && DesignMode)
                    {
                        timeSpan = TimeSpan.FromMinutes(5);
                    }

                    countdownInitial = timeSpan;
                    countdownRemaining = timeSpan;
                    UpdateTextBoxValue();

                    if (!DesignMode)
                    {
                        OnValueChanged();
                    }
                }
                else
                {
                    SetValue(value, true);
                }
            }
        }


        [Category("Data")]
        [Description("最小日期时间值")]
        public DateTime Minimum
        {
            get => minimum;
            set
            {
                if (minimum != value)
                {
                    minimum = value;
                    if (currentValue < minimum)
                    {
                        Value = minimum;
                    }
                    pickerPanel?.UpdateConstraints(minimum, maximum);
                }
            }
        }

        [Category("Data")]
        [Description("最大日期时间值")]
        public DateTime Maximum
        {
            get => maximum;
            set
            {
                if (maximum != value)
                {
                    maximum = value;
                    if (currentValue > maximum)
                    {
                        Value = maximum;
                    }
                    pickerPanel?.UpdateConstraints(minimum, maximum);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(DateTimePickerMode.DateTime)]
        [Description("选择器模式")]
        public DateTimePickerMode Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;
                    UpdateDisplayFormat();
                    UpdateTextBoxValue();
                    pickerPanel?.SetMode(mode, includeMilliseconds);
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("时间模式下是否包含毫秒")]
        public bool IncludeMilliseconds
        {
            get => includeMilliseconds;
            set
            {
                if (includeMilliseconds != value)
                {
                    includeMilliseconds = value;
                    UpdateDisplayFormat();
                    UpdateTextBoxValue();
                    pickerPanel?.SetMode(mode, includeMilliseconds);
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("自定义日期时间格式字符串")]
        public string CustomFormat
        {
            get => customFormat;
            set
            {
                if (customFormat != value)
                {
                    customFormat = value ?? "";
                    UpdateDisplayFormat();
                    UpdateTextBoxValue();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(DateTimeInputMode.Both)]
        [Description("输入模式")]
        public DateTimeInputMode InputMode
        {
            get => inputMode;
            set
            {
                if (inputMode != value)
                {
                    inputMode = value;
                    UpdateInputMode();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("是否只读")]
        public bool ReadOnly
        {
            get => isReadOnly;
            set
            {
                if (isReadOnly != value)
                {
                    isReadOnly = value;
                    textBox.ReadOnly = value;
                    Invalidate();
                }
            }
        }

        [Category("Countdown")]
        [DefaultValue(false)]
        [Description("是否启用倒计时模式")]
        public bool CountdownMode
        {
            get => countdownMode;
            set
            {
                if (countdownMode != value)
                {
                    countdownMode = value;

                    if (value)
                    {
                        // 启用倒计时模式
                        // 从当前Value提取时间部分作为倒计时初始值
                        if (currentValue.TimeOfDay != TimeSpan.Zero)
                        {
                            countdownInitial = currentValue.TimeOfDay;
                        }
                        else if (countdownInitial == TimeSpan.Zero)
                        {
                            countdownInitial = TimeSpan.FromMinutes(5);
                        }
                        countdownRemaining = countdownInitial;

                        // 强制切换到Time模式
                        if (mode != DateTimePickerMode.Time)
                        {
                            mode = DateTimePickerMode.Time;
                            UpdateDisplayFormat();
                        }

                        // 停止任何正在进行的倒计时
                        StopCountdown();
                    }
                    else
                    {
                        // 关闭倒计时模式
                        StopCountdown();
                        // 恢复正常时间显示
                        currentValue = DateTime.Now;
                    }

                    UpdateTextBoxValue();

                    // 通知面板更新
                    if (pickerPanel != null)
                    {
                        pickerPanel.SetCountdownMode(value);
                    }

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置倒计时初始值
        /// </summary>
        [Browsable(false)]
        public TimeSpan CountdownInitialValue
        {
            get => countdownInitial;
            set
            {
                if (value != countdownInitial && value >= TimeSpan.Zero)
                {
                    countdownInitial = value;
                    if (countdownMode && !isCountdownRunning)
                    {
                        countdownRemaining = value;
                        UpdateTextBoxValue();
                    }
                }
            }
        }

        /// <summary>
        /// 倒计时是否正在运行
        /// </summary>
        [Browsable(false)]
        public bool IsCountdownRunning => isCountdownRunning;

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("是否显示下拉按钮")]
        public bool ShowDropDownButton
        {
            get => showDropDownButton;
            set
            {
                if (showDropDownButton != value)
                {
                    showDropDownButton = value;
                    UpdateTextBoxWidth();
                    Invalidate();
                }
            }
        }

        #endregion

        #region 事件

        public event EventHandler ValueChanged;
        public event EventHandler CountdownCompleted;
        public event EventHandler<CountdownTickEventArgs> CountdownTick;

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCountdownCompleted()
        {
            CountdownCompleted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCountdownTick(TimeSpan remaining)
        {
            CountdownTick?.Invoke(this, new CountdownTickEventArgs(remaining));
        }

        #endregion

        #region 值管理

        private void SetValue(DateTime value, bool raiseEvent)
        {
            // 确保值在范围内
            if (value < minimum)
            {
                value = minimum;
            }

            if (value > maximum)
            {
                value = maximum;
            }

            // 根据模式调整值
            switch (mode)
            {
                case DateTimePickerMode.Date:
                    value = value.Date;
                    break;
                case DateTimePickerMode.Time:
                    if (!includeMilliseconds)
                    {
                        value = new DateTime(
                            currentValue.Year, currentValue.Month, currentValue.Day,
                            value.Hour, value.Minute, value.Second);
                    }
                    else
                    {
                        value = new DateTime(
                            currentValue.Year, currentValue.Month, currentValue.Day,
                            value.Hour, value.Minute, value.Second, value.Millisecond);
                    }
                    break;
            }

            if (currentValue != value)
            {
                currentValue = value;
                UpdateTextBoxValue();

                if (raiseEvent)
                {
                    OnValueChanged();
                }
            }
        }

        private void UpdateDisplayFormat()
        {
            if (!string.IsNullOrEmpty(customFormat))
            {
                displayFormat = customFormat;
            }
            else
            {
                switch (mode)
                {
                    case DateTimePickerMode.Date:
                        displayFormat = "yyyy-MM-dd";
                        break;
                    case DateTimePickerMode.Time:
                        displayFormat = includeMilliseconds ? "HH:mm:ss.fff" : "HH:mm:ss";
                        break;
                    case DateTimePickerMode.DateTime:
                        displayFormat = includeMilliseconds ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss";
                        break;
                }
            }
        }

        private void UpdateTextBoxValue()
        {
            if (textBox == null)
            {
                return;
            }

            if (countdownMode)
            {
                // 倒计时模式下显示时间(可能是剩余时间或初始设置值)
                TimeSpan displayTime = isCountdownRunning ? countdownRemaining : countdownInitial;
                textBox.Text = FormatTimeSpan(displayTime);
            }
            else
            {
                textBox.Text = currentValue.ToString(displayFormat);
            }
        }


        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalDays >= 1)
            {
                return $"{(int)ts.TotalDays}d {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
            }
            else if (ts.TotalHours >= 1)
            {
                return $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}";
            }
            else
            {
                return $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            }
        }

        #endregion

        #region 输入处理

        private void UpdateInputMode()
        {
            switch (inputMode)
            {
                case DateTimeInputMode.TextOnly:
                    textBox.ReadOnly = false;
                    showDropDownButton = false;
                    break;
                case DateTimeInputMode.DropDownOnly:
                    textBox.ReadOnly = true;
                    showDropDownButton = true;
                    break;
                case DateTimeInputMode.Both:
                    textBox.ReadOnly = false;
                    showDropDownButton = true;
                    break;
            }

            UpdateTextBoxWidth();
            Invalidate();
        }

        private void UpdateTextBoxWidth()
        {
            if (textBox != null)
            {
                textBox.Width = showDropDownButton
                    ? Width - dropDownButtonWidth - 8
                    : Width - 8;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (countdownMode)
            {
                return;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ValidateTextInput();
            }
            else if (e.KeyCode == Keys.Down && showDropDownButton)
            {
                ShowDropDown();
            }
        }

        private void ValidateTextInput()
        {
            if (textBox.ReadOnly)
            {
                return;
            }

            if (countdownMode)
            {
                // 倒计时模式下, 解析输入的时间作为倒计时初始值
                TimeSpan newDuration = ParseTimeSpanFromText(textBox.Text);

                if (newDuration != TimeSpan.Zero && newDuration != countdownInitial)
                {
                    countdownInitial = newDuration;

                    // 如果没有运行, 更新显示的剩余时间
                    if (!isCountdownRunning)
                    {
                        countdownRemaining = countdownInitial;
                    }

                    UpdateTextBoxValue();
                    OnValueChanged();  // 触发值变化事件
                }
                else if (newDuration == TimeSpan.Zero)
                {
                    // 恢复原值
                    UpdateTextBoxValue();
                }
            }
            else
            {
                // 正常模式
                if (DateTime.TryParseExact(textBox.Text, displayFormat,
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
                {
                    SetValue(result, true);
                }
                else
                {
                    // 恢复原值
                    UpdateTextBoxValue();
                }
            }
        }

        /// <summary>
        /// 从文本解析TimeSpan
        /// </summary>
        private TimeSpan ParseTimeSpanFromText(string text)
        {
            try
            {
                string[] parts = text.Split(':', '.');

                if (parts.Length >= 2)
                {
                    int hours = 0, minutes = 0, seconds = 0, milliseconds = 0;

                    if (parts.Length == 2)
                    {
                        // mm:ss
                        int.TryParse(parts[0], out minutes);
                        int.TryParse(parts[1], out seconds);
                    }
                    else if (parts.Length == 3)
                    {
                        if (text.Contains("."))
                        {
                            // mm:ss.fff
                            int.TryParse(parts[0], out minutes);
                            int.TryParse(parts[1], out seconds);
                            int.TryParse(parts[2], out milliseconds);
                        }
                        else
                        {
                            // hh:mm:ss
                            int.TryParse(parts[0], out hours);
                            int.TryParse(parts[1], out minutes);
                            int.TryParse(parts[2], out seconds);
                        }
                    }
                    else if (parts.Length == 4)
                    {
                        // hh:mm:ss.fff
                        int.TryParse(parts[0], out hours);
                        int.TryParse(parts[1], out minutes);
                        int.TryParse(parts[2], out seconds);
                        int.TryParse(parts[3], out milliseconds);
                    }

                    return new TimeSpan(0, hours, minutes, seconds, milliseconds);
                }
            }
            catch { }

            return TimeSpan.Zero;
        }

        #endregion

        #region 下拉面板

        private void ShowDropDown()
        {
            if (isDroppedDown || !showDropDownButton || isReadOnly)
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

            // 设置面板的倒计时模式
            pickerPanel.SetCountdownMode(countdownMode);

            if (countdownMode)
            {
                // 倒计时模式下, 将TimeSpan转换为DateTime显示
                var baseDate = DateTime.Today;
                var displayDateTime = baseDate.Add(isCountdownRunning ? countdownRemaining : countdownInitial);
                pickerPanel.SetValue(displayDateTime);
                pickerPanel.SetMode(DateTimePickerMode.Time, includeMilliseconds);
            }
            else
            {
                pickerPanel.SetValue(currentValue);
                pickerPanel.SetMode(mode, includeMilliseconds);
                pickerPanel.UpdateConstraints(minimum, maximum);
            }

            // 计算面板大小
            Size panelSize = pickerPanel.GetPreferredSize();
            dropDownHost.Size = panelSize;
            dropDown.Size = new Size(panelSize.Width + 2, panelSize.Height + 2);

            // 显示下拉面板
            dropDown.Show(this, new Point(0, Height));
            isDroppedDown = true;

            dropDown.Closed += (s, e) =>
            {
                isDroppedDown = false;
                Invalidate();
            };

            Invalidate();
        }


        private void CloseDropDown()
        {
            if (dropDown != null && isDroppedDown)
            {
                dropDown.Close();
                isDroppedDown = false;
                Invalidate();
            }
        }

        #endregion

        #region 倒计时功能

        /// <summary>
        /// 开始倒计时(使用当前设置的初始值)
        /// </summary>
        public void StartCountdown()
        {
            if (!countdownMode)
            {
                CountdownMode = true;
            }

            // 从控件获取当前显示的时间作为倒计时初始值
            countdownInitial = GetTimeSpanFromDisplay();
            countdownRemaining = countdownInitial;
            countdownBase = countdownInitial;  // 设置基准值
            countdownStartTime = DateTime.Now;
            isCountdownRunning = true;
            isCountdownPaused = false;

            countdownTimer.Start();
        }

        /// <summary>
        /// 开始倒计时(指定时长)
        /// </summary>
        public void StartCountdown(TimeSpan duration)
        {
            if (!countdownMode)
            {
                CountdownMode = true;
            }

            countdownInitial = duration;
            countdownRemaining = duration;
            countdownBase = duration;  // 设置基准值
            countdownStartTime = DateTime.Now;
            isCountdownRunning = true;
            isCountdownPaused = false;

            UpdateTextBoxValue();
            countdownTimer.Start();
        }

        /// <summary>
        /// 暂停倒计时
        /// </summary>
        public void PauseCountdown()
        {
            if (isCountdownRunning && !isCountdownPaused)
            {
                countdownTimer.Stop();
                isCountdownPaused = true;
                // 保存当前剩余时间作为新的基准值
                countdownBase = countdownRemaining;
            }
        }

        /// <summary>
        /// 恢复倒计时
        /// </summary>
        public void ResumeCountdown()
        {
            if (isCountdownPaused && isCountdownRunning)
            {
                isCountdownPaused = false;
                countdownStartTime = DateTime.Now;
                countdownTimer.Start();
            }
        }

        /// <summary>
        /// 重置倒计时
        /// </summary>
        public void ResetCountdown()
        {
            StopCountdown();
            // 从控件获取当前显示的时间作为新的初始值
            countdownInitial = GetTimeSpanFromDisplay();
            countdownRemaining = countdownInitial;
            countdownBase = countdownInitial;
            UpdateTextBoxValue();
        }

        /// <summary>
        /// 停止倒计时
        /// </summary>
        public void StopCountdown()
        {
            countdownTimer.Stop();
            isCountdownRunning = false;
            isCountdownPaused = false;
            countdownBase = TimeSpan.Zero;
            // 保持显示当前剩余时间
            UpdateTextBoxValue();
        }

        /// <summary>
        /// 从显示控件获取TimeSpan值
        /// </summary>
        private TimeSpan GetTimeSpanFromDisplay()
        {
            try
            {
                // 尝试从文本框解析时间
                string text = textBox.Text;

                // 尝试不同的格式
                string[] parts = text.Split(':', '.');

                if (parts.Length >= 2)
                {
                    int hours = 0, minutes = 0, seconds = 0, milliseconds = 0;

                    if (parts.Length == 2)
                    {
                        // mm:ss 格式
                        int.TryParse(parts[0], out minutes);
                        int.TryParse(parts[1], out seconds);
                    }
                    else if (parts.Length == 3)
                    {
                        // hh:mm:ss 或 mm:ss.fff 格式
                        if (text.Contains("."))
                        {
                            // mm:ss.fff
                            int.TryParse(parts[0], out minutes);
                            int.TryParse(parts[1], out seconds);
                            int.TryParse(parts[2], out milliseconds);
                        }
                        else
                        {
                            // hh:mm:ss
                            int.TryParse(parts[0], out hours);
                            int.TryParse(parts[1], out minutes);
                            int.TryParse(parts[2], out seconds);
                        }
                    }
                    else if (parts.Length == 4)
                    {
                        // hh:mm:ss.fff 格式
                        int.TryParse(parts[0], out hours);
                        int.TryParse(parts[1], out minutes);
                        int.TryParse(parts[2], out seconds);
                        int.TryParse(parts[3], out milliseconds);
                    }

                    return new TimeSpan(0, hours, minutes, seconds, milliseconds);
                }
            }
            catch
            {
                // 解析失败, 返回当前剩余时间
            }

            return countdownRemaining;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (isCountdownRunning && !isCountdownPaused)
            {
                // 计算从最近一次开始/恢复到现在经过的时间
                var elapsed = DateTime.Now - countdownStartTime;

                // 从基准值减去经过的时间
                countdownRemaining = countdownBase - elapsed;

                if (countdownRemaining <= TimeSpan.Zero)
                {
                    countdownRemaining = TimeSpan.Zero;
                    countdownTimer.Stop();
                    countdownBase = TimeSpan.Zero;
                    UpdateTextBoxValue();
                    isCountdownRunning = false;

                    OnCountdownCompleted();
                }
                else
                {
                    UpdateTextBoxValue();
                    OnCountdownTick(countdownRemaining);
                }
            }
        }

        #endregion

        #region 鼠标事件

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && GetDropDownButtonRect().Contains(e.Location))
            {
                if (isDroppedDown)
                {
                    CloseDropDown();
                }
                else
                {
                    ShowDropDown();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool wasButtonHovered = isButtonHovered;
            isButtonHovered = GetDropDownButtonRect().Contains(e.Location);

            if (wasButtonHovered != isButtonHovered)
            {
                Invalidate();
            }

            Cursor = isButtonHovered ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (isButtonHovered)
            {
                isButtonHovered = false;
                Invalidate();
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
                bgColor = GetThemeColor(c => c.Surface, bgColor);
            }

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, rect);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 绘制下拉按钮
            if (showDropDownButton)
            {
                DrawDropDownButton(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color borderColor = Color.Gray;

            if (UseTheme && Theme != null)
            {
                if (isTextBoxFocused)
                {
                    borderColor = GetThemeColor(c => c.Primary, Color.Blue);
                }
                else if (State == ControlState.Hover)
                {
                    borderColor = GetThemeColor(c => c.BorderFocused, Color.DarkGray);
                }
                else
                {
                    borderColor = GetThemeColor(c => c.Border, Color.Gray);
                }
            }

            using (var pen = new Pen(borderColor, isTextBoxFocused ? 2 : 1))
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

        private void DrawDropDownButton(Graphics g)
        {
            var buttonRect = GetDropDownButtonRect();

            // 绘制按钮背景
            Color buttonBgColor = BackColor;

            if (isButtonHovered && !isDroppedDown)
            {
                buttonBgColor = UseTheme && Theme != null
                    ? GetThemeColor(c => c.SurfaceHover, Color.LightGray)
                    : Color.LightGray;

                using (var brush = new SolidBrush(buttonBgColor))
                {
                    g.FillRectangle(brush, buttonRect);
                }
            }

            // 绘制分隔线
            using (var pen = new Pen(GetThemeColor(c => c.Border, Color.Gray)))
            {
                g.DrawLine(pen, buttonRect.Left, 4, buttonRect.Left, Height - 4);
            }

            // 绘制下拉箭头
            DrawDropDownArrow(g, buttonRect);
        }

        private void DrawDropDownArrow(Graphics g, Rectangle buttonRect)
        {
            Color arrowColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.TextPrimary, Color.Black)
                : Color.Black;

            if (isButtonHovered || isDroppedDown)
            {
                arrowColor = UseTheme && Theme != null
                    ? GetThemeColor(c => c.Primary, Color.Blue)
                    : Color.Blue;
            }

            int centerX = buttonRect.X + buttonRect.Width / 2;
            int centerY = buttonRect.Y + buttonRect.Height / 2;

            Point[] arrow = new Point[]
            {
                new Point(centerX - 4, centerY - 2),
                new Point(centerX + 4, centerY - 2),
                new Point(centerX, centerY + 2)
            };

            using (var brush = new SolidBrush(arrowColor))
            {
                g.FillPolygon(brush, arrow);
            }
        }

        private Rectangle GetDropDownButtonRect()
        {
            if (!showDropDownButton)
            {
                return Rectangle.Empty;
            }

            return new Rectangle(
                Width - dropDownButtonWidth - 1,
                1,
                dropDownButtonWidth,
                Height - 2);
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

            if (textBox != null)
            {
                textBox.BackColor = BackColor;
                textBox.ForeColor = ForeColor;
                textBox.Font = GetThemeFont(t => t.Body, Font);
            }
        }

        #endregion

        #region 布局

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateTextBoxWidth();

            if (textBox != null)
            {
                textBox.Top = (Height - textBox.Height) / 2;
            }
        }


        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                countdownTimer?.Dispose();
                dropDown?.Dispose();
                pickerPanel?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 日期时间选择面板

    [ToolboxItem(false)]
    public class DateTimePickerPanel : UserControl
    {
        private FluentDateTimePicker parentPicker;
        private DateTimePickerMode mode;
        private bool includeMilliseconds;
        private DateTime selectedDateTime;
        private DateTime minimum;
        private DateTime maximum;
        private bool isCountdownMode = false;

        // 控件
        private MonthCalendar calendar;
        private NumericUpDown hourUpDown;
        private NumericUpDown minuteUpDown;
        private NumericUpDown secondUpDown;
        private NumericUpDown millisecondUpDown;
        private Label colonLabel1;
        private Label colonLabel2;
        private Label colonLabel3;
        private Button okButton;
        private Button nowButton;

        public event EventHandler ValueChanged;

        public DateTime SelectedDateTime => selectedDateTime;

        public DateTimePickerPanel(FluentDateTimePicker parent)
        {
            parentPicker = parent;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = Color.White;
            BorderStyle = BorderStyle.None;

            // 日历控件
            calendar = new MonthCalendar
            {
                MaxSelectionCount = 1,
                ShowToday = true,
                ShowTodayCircle = true
            };
            calendar.DateSelected += (s, e) =>
            {
                UpdateSelectedDateTime();
                if (mode == DateTimePickerMode.Date && !isCountdownMode)
                {
                    OnValueChanged();
                }
            };

            // 时间控件
            hourUpDown = CreateNumericUpDown(0, 23, 2);
            minuteUpDown = CreateNumericUpDown(0, 59, 2);
            secondUpDown = CreateNumericUpDown(0, 59, 2);
            millisecondUpDown = CreateNumericUpDown(0, 999, 3);

            colonLabel1 = new Label { Text = ":", AutoSize = true };
            colonLabel2 = new Label { Text = ":", AutoSize = true };
            colonLabel3 = new Label { Text = ".", AutoSize = true };

            // OK按钮
            okButton = new Button
            {
                Text = "确认",
                Size = new Size(60, 25),
                FlatStyle = FlatStyle.Flat
            };
            okButton.Click += (s, e) =>
            {
                UpdateSelectedDateTime();
                OnValueChanged();
            };

            // Now按钮
            nowButton = new Button
            {
                Text = "当前时刻",
                Size = new Size(100, 25),
                FlatStyle = FlatStyle.Flat
            };
            nowButton.Click += (s, e) =>
            {
                SetValue(DateTime.Now);
                OnValueChanged();
            };

            Controls.AddRange(new Control[]
            {
                calendar,
                hourUpDown, colonLabel1, minuteUpDown, colonLabel2, secondUpDown,
                colonLabel3, millisecondUpDown,
                okButton, nowButton
            });
        }

        /// <summary>
        /// 设置倒计时模式
        /// </summary>
        public void SetCountdownMode(bool countdown)
        {
            isCountdownMode = countdown;
            UpdateLayout();
        }

        private NumericUpDown CreateNumericUpDown(int min, int max, int digits)
        {
            var upDown = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Size = new Size(digits * 15 + 20, 24),
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Microsoft YaHei", 9f)
            };

            upDown.ValueChanged += (s, e) => UpdateSelectedDateTime();

            // 垂直居中
            if (upDown.Controls[1] is TextBox textBox)
            {
                textBox.TextAlign = HorizontalAlignment.Center;
                var margin = (upDown.Height - textBox.Font.Height) / 2;
                textBox.Margin = new Padding(0, margin, 0, 0);
            }

            return upDown;
        }

        public void SetMode(DateTimePickerMode newMode, bool withMilliseconds)
        {
            mode = newMode;
            includeMilliseconds = withMilliseconds;
            UpdateLayout();
        }

        public void SetValue(DateTime value)
        {
            selectedDateTime = value;

            calendar.SetDate(value.Date);
            hourUpDown.Value = value.Hour;
            minuteUpDown.Value = value.Minute;
            secondUpDown.Value = value.Second;
            millisecondUpDown.Value = value.Millisecond;
        }

        public void UpdateConstraints(DateTime min, DateTime max)
        {
            minimum = min;
            maximum = max;

            if (calendar != null)
            {
                calendar.MinDate = min.Date;
                calendar.MaxDate = max.Date;
            }
        }

        private void UpdateSelectedDateTime()
        {
            var date = calendar.SelectionStart.Date;

            selectedDateTime = new DateTime(
                date.Year, date.Month, date.Day,
                (int)hourUpDown.Value,
                (int)minuteUpDown.Value,
                (int)secondUpDown.Value,
                includeMilliseconds ? (int)millisecondUpDown.Value : 0);

            // 确保在范围内
            if (selectedDateTime < minimum)
            {
                selectedDateTime = minimum;
            }

            if (selectedDateTime > maximum)
            {
                selectedDateTime = maximum;
            }
        }

        private void UpdateLayout()
        {
            SuspendLayout();

            // 隐藏所有控件
            foreach (Control ctrl in Controls)
            {
                ctrl.Visible = false;
            }

            int yPos = 5;

            // 倒计时模式下不显示日历
            if (!isCountdownMode && (mode == DateTimePickerMode.Date || mode == DateTimePickerMode.DateTime))
            {
                calendar.Location = new Point(5, yPos);
                calendar.Visible = true;
                yPos = calendar.Bottom + 5;
            }

            // 显示时间控件(倒计时模式或时间模式)
            if (isCountdownMode || mode == DateTimePickerMode.Time || mode == DateTimePickerMode.DateTime)
            {
                int xPos = 10;
                int labelYOffset = 4;

                // 倒计时模式下, 增加顶部间距
                if (isCountdownMode)
                {
                    yPos = 10;
                }

                hourUpDown.Location = new Point(xPos, yPos);
                hourUpDown.Visible = true;
                xPos = hourUpDown.Right + 2;

                colonLabel1.Location = new Point(xPos, yPos + labelYOffset);
                colonLabel1.Visible = true;
                xPos = colonLabel1.Right + 2;

                minuteUpDown.Location = new Point(xPos, yPos);
                minuteUpDown.Visible = true;
                xPos = minuteUpDown.Right + 2;

                colonLabel2.Location = new Point(xPos, yPos + labelYOffset);
                colonLabel2.Visible = true;
                xPos = colonLabel2.Right + 2;

                secondUpDown.Location = new Point(xPos, yPos);
                secondUpDown.Visible = true;
                xPos = secondUpDown.Right + 2;

                if (includeMilliseconds)
                {
                    colonLabel3.Location = new Point(xPos, yPos + labelYOffset);
                    colonLabel3.Visible = true;
                    xPos = colonLabel3.Right + 2;

                    millisecondUpDown.Location = new Point(xPos, yPos);
                    millisecondUpDown.Visible = true;
                }

                yPos += 30;
            }

            // 按钮布局
            if (isCountdownMode)
            {
                // 倒计时模式：只显示OK按钮, 靠右显示
                okButton.Location = new Point(GetPreferredSize().Width - okButton.Width - 10, yPos); // new Point((GetPreferredSize().Width - okButton.Width) / 2, yPos);
                okButton.Visible = true;
                nowButton.Visible = false;
            }
            else
            {
                // 正常模式
                nowButton.Location = new Point(10, yPos);
                nowButton.Visible = true;

                okButton.Location = new Point(nowButton.Right + 5, yPos);
                okButton.Visible = (mode == DateTimePickerMode.Time || mode == DateTimePickerMode.DateTime);
            }

            ResumeLayout();
        }

        public Size GetPreferredSize()
        {
            if (isCountdownMode)
            {
                // 倒计时模式：只有时间选择器和OK按钮
                return new Size(includeMilliseconds ? 280 : 220, 75);
            }

            switch (mode)
            {
                case DateTimePickerMode.Date:
                    return new Size(240, 190);
                case DateTimePickerMode.Time:
                    return new Size(includeMilliseconds ? 280 : 220, 68);
                case DateTimePickerMode.DateTime:
                    return new Size(includeMilliseconds ? 280 : 240, 252);
                default:
                    return new Size(240, 190);
            }
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region 枚举和事件参数

    /// <summary>
    /// 日期时间选择器模式
    /// </summary>
    public enum DateTimePickerMode
    {
        /// <summary>
        /// 仅日期
        /// </summary>
        Date,

        /// <summary>
        /// 仅时间
        /// </summary>
        Time,

        /// <summary>
        /// 日期和时间
        /// </summary>
        DateTime
    }

    /// <summary>
    /// 输入模式
    /// </summary>
    public enum DateTimeInputMode
    {
        /// <summary>
        /// 仅文本输入
        /// </summary>
        TextOnly,

        /// <summary>
        /// 仅下拉选择
        /// </summary>
        DropDownOnly,

        /// <summary>
        /// 文本输入和下拉选择
        /// </summary>
        Both
    }

    /// <summary>
    /// 倒计时Tick事件参数
    /// </summary>
    public class CountdownTickEventArgs : EventArgs
    {
        public TimeSpan RemainingTime { get; }

        public CountdownTickEventArgs(TimeSpan remaining)
        {
            RemainingTime = remaining;
        }
    }

    #endregion
}
