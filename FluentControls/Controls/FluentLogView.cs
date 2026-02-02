using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentControls.Logging;
using Infrastructure;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel.Design;
using FluentControls.Themes;
using System.Windows.Forms.Design;

namespace FluentControls.Controls
{
    [DefaultProperty("MaxDisplayCount")]
    [Designer(typeof(FluentLogViewDesigner))]
    public class FluentLogView : FluentContainerBase
    {
        #region 字段

        private FluentListView listView;
        private ToolStrip toolStrip;
        private ContextMenuStrip contextMenu;

        // 工具栏按钮
        private ToolStripButton btnClear;
        private ToolStripButton btnExport;
        private ToolStripSeparator separator1;
        private ToolStripDropDownButton btnLevel;
        private ToolStripMenuItem menuTrace;
        private ToolStripMenuItem menuDebug;
        private ToolStripMenuItem menuInfo;
        private ToolStripMenuItem menuWarn;
        private ToolStripMenuItem menuError;
        private ToolStripMenuItem menuFatal;
        private ToolStripSeparator separator2;
        private ToolStripLabel labelMaxCount;
        private ToolStripTextBox txtMaxCount;
        private ToolStripSeparator separator3;
        private ToolStripLabel labelSearch;
        private ToolStripTextBox txtSearch;
        private ToolStripButton btnSearch;
        private ToolStripSeparator separator4;
        private ToolStripDropDownButton btnGroup;
        private ToolStripMenuItem menuAllGroups;

        // 右键菜单
        private ToolStripMenuItem menuCopy;
        private ToolStripMenuItem menuCopyMessage;
        private ToolStripMenuItem menuCopyAll;
        private ToolStripSeparator menuSeparator1;
        private ToolStripMenuItem menuClearLogs;
        private ToolStripMenuItem menuExport;

        // 数据
        private CircularBuffer<ILogEvent> logBuffer;
        private ConcurrentQueue<ILogEvent> pendingLogs;
        private List<ILogEvent> filteredLogs;
        private HashSet<string> logGroups;

        // 配置
        private int maxDisplayCount = 1000;
        private int maxStoreCount = 10000;
        private LogLevel currentLevel = LogLevel.Info | LogLevel.Warn | LogLevel.Error | LogLevel.Fatal;
        private LogColorScheme colorScheme = LogColorScheme.Default;
        private string currentGroup = "";
        private string searchKeyword = "";
        private string timestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private bool showToolStrip = true;
        private bool autoSelectColorScheme = true; // 是否自动选择颜色方案

        // 线程
        private Thread processingThread;
        private volatile bool isRunning = true;
        private AutoResetEvent logEvent = new AutoResetEvent(false);

        #endregion

        #region 构造函数

        public FluentLogView()
        {
            logBuffer = new CircularBuffer<ILogEvent>(maxStoreCount);
            pendingLogs = new ConcurrentQueue<ILogEvent>();
            filteredLogs = new List<ILogEvent>();
            logGroups = new HashSet<string>();

            InitializeComponents();

            // 延迟启动处理线程，确保控件完全初始化
            this.HandleCreated += (s, e) =>
            {
                if (processingThread == null || !processingThread.IsAlive)
                {
                    InitializeProcessingThread();
                }
            };
            UpdateLevelButtons();

            // 应用初始颜色方案
            ApplyThemeColorScheme();
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            // 创建ListView
            listView = new FluentListView
            {
                Dock = DockStyle.Fill,
                View = FluentListViewMode.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = true,
                ShowColumnHeaders = true,
                BackColor = Color.White,
                AllowColumnResize = true,
                UseTheme = this.UseTheme,
                ThemeName = this.ThemeName
            };

            // 添加列
            listView.Columns.Add(new FluentListViewColumn { Text = "时间", Width = 150 });
            listView.Columns.Add(new FluentListViewColumn { Text = "级别", Width = 80 });
            listView.Columns.Add(new FluentListViewColumn { Text = "消息", Width = 400 });
            listView.Columns.Add(new FluentListViewColumn { Text = "来源", Width = 150 });
            listView.Columns.Add(new FluentListViewColumn { Text = "分组", Width = 100 });

            // 创建工具栏
            InitializeToolStrip();

            // 创建右键菜单
            InitializeContextMenu();

            // 添加控件
            Controls.Add(listView);
            Controls.Add(toolStrip);

            listView.ContextMenuStrip = contextMenu;

            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeToolStrip()
        {
            toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden
            };

            // 清空按钮
            btnClear = new ToolStripButton
            {
                Text = "清空",
                ToolTipText = "清空所有日志"
            };
            btnClear.Click += BtnClear_Click;

            // 导出按钮
            btnExport = new ToolStripButton
            {
                Text = "导出",
                ToolTipText = "导出日志到文件"
            };
            btnExport.Click += BtnExport_Click;

            separator1 = new ToolStripSeparator();

            // 日志级别下拉菜单
            btnLevel = new ToolStripDropDownButton
            {
                Text = "级别",
                ToolTipText = "选择要显示的日志级别"
            };

            menuTrace = new ToolStripMenuItem { Text = "Trace", CheckOnClick = true };
            menuDebug = new ToolStripMenuItem { Text = "Debug", CheckOnClick = true };
            menuInfo = new ToolStripMenuItem { Text = "Info", CheckOnClick = true, Checked = true };
            menuWarn = new ToolStripMenuItem { Text = "Warn", CheckOnClick = true, Checked = true };
            menuError = new ToolStripMenuItem { Text = "Error", CheckOnClick = true, Checked = true };
            menuFatal = new ToolStripMenuItem { Text = "Fatal", CheckOnClick = true, Checked = true };

            menuTrace.CheckedChanged += LevelMenu_CheckedChanged;
            menuDebug.CheckedChanged += LevelMenu_CheckedChanged;
            menuInfo.CheckedChanged += LevelMenu_CheckedChanged;
            menuWarn.CheckedChanged += LevelMenu_CheckedChanged;
            menuError.CheckedChanged += LevelMenu_CheckedChanged;
            menuFatal.CheckedChanged += LevelMenu_CheckedChanged;

            btnLevel.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuTrace, menuDebug, menuInfo, menuWarn, menuError, menuFatal
            });

            separator2 = new ToolStripSeparator();

            // 最大显示数量
            labelMaxCount = new ToolStripLabel { Text = "最大显示:" };
            txtMaxCount = new ToolStripTextBox
            {
                Width = 60,
                Text = maxDisplayCount.ToString()
            };
            txtMaxCount.TextChanged += TxtMaxCount_TextChanged;

            separator3 = new ToolStripSeparator();

            // 搜索
            labelSearch = new ToolStripLabel { Text = "搜索:" };
            txtSearch = new ToolStripTextBox { Width = 150 };
            txtSearch.KeyDown += TxtSearch_KeyDown;

            btnSearch = new ToolStripButton
            {
                Text = "Go",
                ToolTipText = "搜索日志"
            };
            btnSearch.Click += BtnSearch_Click;

            separator4 = new ToolStripSeparator();

            // 分组下拉菜单
            btnGroup = new ToolStripDropDownButton
            {
                Text = "分组",
                ToolTipText = "选择日志分组"
            };

            menuAllGroups = new ToolStripMenuItem
            {
                Text = "所有分组",
                Checked = true
            };
            menuAllGroups.Click += GroupMenu_Click;

            btnGroup.DropDownItems.Add(menuAllGroups);

            // 添加所有工具栏项
            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                btnClear, btnExport, separator1,
                btnLevel, separator2,
                labelMaxCount, txtMaxCount, separator3,
                labelSearch, txtSearch, btnSearch, separator4,
                btnGroup
            });
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            menuCopy = new ToolStripMenuItem { Text = "复制选中行" };
            menuCopy.Click += MenuCopy_Click;

            menuCopyMessage = new ToolStripMenuItem { Text = "复制消息" };
            menuCopyMessage.Click += MenuCopyMessage_Click;

            menuCopyAll = new ToolStripMenuItem { Text = "复制所有" };
            menuCopyAll.Click += MenuCopyAll_Click;

            menuSeparator1 = new ToolStripSeparator();

            menuClearLogs = new ToolStripMenuItem { Text = "清空日志" };
            menuClearLogs.Click += BtnClear_Click;

            menuExport = new ToolStripMenuItem { Text = "导出日志..." };
            menuExport.Click += BtnExport_Click;

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                menuCopy, menuCopyMessage, menuCopyAll,
                menuSeparator1,
                menuClearLogs, menuExport
            });
        }

        private void InitializeProcessingThread()
        {
            processingThread = new Thread(ProcessLogs)
            {
                IsBackground = true,
                Name = "FluentLogView_ProcessingThread"
            };
            processingThread.Start();
        }

        #endregion

        #region 属性

        [Category("LogView")]
        [Description("最大显示日志数量")]
        [DefaultValue(1000)]
        public int MaxDisplayCount
        {
            get => maxDisplayCount;
            set
            {
                if (maxDisplayCount != value && value > 0)
                {
                    maxDisplayCount = value;
                    if (txtMaxCount != null && !txtMaxCount.Text.Equals(value.ToString()))
                    {
                        txtMaxCount.Text = value.ToString();
                    }
                    RefreshDisplay();
                }
            }
        }

        [Category("LogView")]
        [Description("最大存储日志数量")]
        [DefaultValue(10000)]
        public int MaxStoreCount
        {
            get => maxStoreCount;
            set
            {
                if (maxStoreCount != value && value > 0)
                {
                    maxStoreCount = value;
                    logBuffer = new CircularBuffer<ILogEvent>(value);
                }
            }
        }

        [Category("LogView")]
        [Description("当前显示的日志级别")]
        [DefaultValue(LogLevel.Info | LogLevel.Warn | LogLevel.Error | LogLevel.Fatal)]
        public LogLevel CurrentLevel
        {
            get => currentLevel;
            set
            {
                if (currentLevel != value)
                {
                    currentLevel = value;
                    UpdateLevelButtons();
                    RefreshDisplay();
                }
            }
        }

        [Category("LogView")]
        [Description("时间戳格式")]
        [DefaultValue("yyyy-MM-dd HH:mm:ss.fff")]
        public string TimestampFormat
        {
            get => timestampFormat;
            set
            {
                if (timestampFormat != value)
                {
                    timestampFormat = value ?? "yyyy-MM-dd HH:mm:ss.fff";
                    RefreshDisplay();
                }
            }
        }

        [Category("LogView")]
        [Description("是否显示工具栏")]
        [DefaultValue(true)]
        public bool ShowToolStrip
        {
            get => showToolStrip;
            set
            {
                if (showToolStrip != value)
                {
                    showToolStrip = value;
                    toolStrip.Visible = value;
                }
            }
        }

        [Category("LogView")]
        [Description("当前选中的分组")]
        [DefaultValue("")]
        public string CurrentGroup
        {
            get => currentGroup;
            set
            {
                if (currentGroup != value)
                {
                    currentGroup = value ?? "";
                    RefreshDisplay();
                }
            }
        }

        [Category("LogView")]
        [Description("日志颜色方案")]
        [Browsable(false)]
        public LogColorScheme ColorScheme
        {
            get => colorScheme;
            set
            {
                if (colorScheme != value)
                {
                    colorScheme = value ?? LogColorScheme.Default;
                    autoSelectColorScheme = false;
                    RefreshDisplay();
                }
            }
        }

        [Category("LogView")]
        [Description("是否根据主题自动选择颜色方案")]
        [DefaultValue(true)]
        public bool AutoSelectColorScheme
        {
            get => autoSelectColorScheme;
            set
            {
                if (autoSelectColorScheme != value)
                {
                    autoSelectColorScheme = value;
                    if (value)
                    {
                        ApplyThemeColorScheme();
                    }
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加日志(支持任意类型的日志对象)
        /// </summary>
        public void AddLog(object logEventObj)
        {
            if (logEventObj == null)
            {
                return;
            }

            var log = LogEventFactory.CreateLogEvent(logEventObj);
            if (log != null)
            {
                pendingLogs.Enqueue(log);

                // 确保字段名正确
                this.logEvent?.Set();
            }

        }

        /// <summary>
        /// 添加日志(使用接口)
        /// </summary>
        public void AddLog(ILogEvent logEventObj)
        {
            if (logEventObj != null)
            {
                pendingLogs.Enqueue(logEventObj);
                this.logEvent?.Set();
            }
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        public void Clear()
        {
            logBuffer.Clear();
            filteredLogs.Clear();

            if (InvokeRequired)
            {
                Invoke(new Action(() => listView.Items.Clear()));
            }
            else
            {
                listView.Items.Clear();
            }
        }

        /// <summary>
        /// 导出日志到文件
        /// </summary>
        public void ExportLogs(string filePath)
        {
            try
            {
                var logs = filteredLogs.ToArray();
                var sb = new StringBuilder();

                foreach (var log in logs)
                {
                    sb.AppendLine($"[{log.Timestamp.ToString(timestampFormat)}] [{log.Level}] {log.Message}");
                    if (!string.IsNullOrEmpty(log.Source))
                    {
                        sb.AppendLine($"  Source: {log.Source}");
                    }
                    if (log.Exception != null)
                    {
                        sb.AppendLine($"  Exception: {log.Exception}");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出日志失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddContextMenu(ToolStripMenuItem menuItem)
        {
            contextMenu.Items.Add(menuItem);
        }

        public void AddContextMenu(string text, Action action)
        {
            var menuItem = new ToolStripMenuItem { Text = text };
            menuItem.Click += (s, e) => action?.Invoke();

            contextMenu.Items.Add(menuItem);
        }

        #endregion

        #region 私有方法

        private void ProcessLogs()
        {
            while (isRunning)
            {
                try
                {
                    // 等待新日志
                    logEvent.WaitOne(100);

                    // 处理待处理的日志
                    var batch = new List<ILogEvent>();
                    while (pendingLogs.TryDequeue(out var log) && batch.Count < 100)
                    {
                        batch.Add(log);

                        // 更新分组列表
                        if (!string.IsNullOrEmpty(log.Group))
                        {
                            lock (logGroups)
                            {
                                if (logGroups.Add(log.Group))
                                {
                                    UpdateGroupMenu();
                                }
                            }
                        }
                    }

                    if (batch.Count > 0)
                    {
                        // 添加到缓冲区
                        foreach (var log in batch)
                        {
                            logBuffer.Add(log);
                        }

                        // 更新显示
                        RefreshDisplay();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ProcessLogs error: {ex.Message}");
                }
            }
        }

        private void RefreshDisplay()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(RefreshDisplay));
                }
                catch
                {
                    // 控件可能已释放，忽略
                }
                return;
            }

            // 确保ListView和控件句柄已创建
            if (listView == null || !listView.IsHandleCreated || !IsHandleCreated)
            {
                return;
            }

            try
            {
                listView.BeginUpdate();
                listView.Items.Clear();

                // 过滤日志
                var allLogs = logBuffer.ToArray();
                filteredLogs = FilterLogs(allLogs).ToList();

                // 限制显示数量
                var displayLogs = filteredLogs.Take(maxDisplayCount).ToArray();

                // 添加到ListView
                foreach (var log in displayLogs)
                {
                    var item = CreateListViewItem(log);
                    listView.Items.Add(item);
                }

                listView.EndUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshDisplay error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private IEnumerable<ILogEvent> FilterLogs(ILogEvent[] logs)
        {
            if (logs == null || logs.Length == 0)
            {
                return Enumerable.Empty<ILogEvent>();
            }

            var result = logs.AsEnumerable();

            // 级别过滤
            result = result.Where(log => currentLevel.HasFlag(log.Level));

            var afterLevelFilter = result.ToList();
            result = afterLevelFilter;

            // 分组过滤
            if (!string.IsNullOrEmpty(currentGroup))
            {
                result = result.Where(log => log.Group == currentGroup);
            }

            // 搜索过滤
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                var keyword = searchKeyword.ToLower();
                result = result.Where(log =>
                    log.Message?.ToLower().Contains(keyword) == true ||
                    log.Source?.ToLower().Contains(keyword) == true);
            }

            // 按时间倒序
            return result.OrderByDescending(log => log.Timestamp);
        }

        private FluentListViewItem CreateListViewItem(ILogEvent log)
        {
            var item = new FluentListViewItem(new[]
            {
                log.Timestamp.ToString(timestampFormat),
                log.Level.ToString(),
                log.Message ?? "",
                log.Source ?? "",
                log.Group ?? ""
            });

            item.Tag = log;

            // 应用颜色 - 确保颜色不为空
            var colors = colorScheme.GetColors(log.Level);

            // 设置前景色
            item.ForeColor = colors.ForeColor;

            // 设置背景色(确保不是透明色)
            if (colors.BackColor != Color.Transparent && colors.BackColor != Color.Empty)
            {
                item.BackColor = colors.BackColor;
            }
            else
            {
                // 如果配色方案返回透明色，使用ListView的背景色
                item.BackColor = listView?.BackColor ?? SystemColors.Window;
            }

            return item;
        }


        private void UpdateLevelButtons()
        {
            if (menuTrace != null)
            {
                menuTrace.Checked = currentLevel.HasFlag(LogLevel.Trace);
                menuDebug.Checked = currentLevel.HasFlag(LogLevel.Debug);
                menuInfo.Checked = currentLevel.HasFlag(LogLevel.Info);
                menuWarn.Checked = currentLevel.HasFlag(LogLevel.Warn);
                menuError.Checked = currentLevel.HasFlag(LogLevel.Error);
                menuFatal.Checked = currentLevel.HasFlag(LogLevel.Fatal);
            }
        }

        private void UpdateGroupMenu()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateGroupMenu));
                return;
            }

            btnGroup.DropDownItems.Clear();
            btnGroup.DropDownItems.Add(menuAllGroups);

            lock (logGroups)
            {
                foreach (var group in logGroups.OrderBy(g => g))
                {
                    var menuItem = new ToolStripMenuItem
                    {
                        Text = group,
                        Tag = group
                    };
                    menuItem.Click += GroupMenu_Click;
                    btnGroup.DropDownItems.Add(menuItem);
                }
            }
        }

        #endregion

        #region 事件处理

        private void BtnClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "日志文件(*.log)|*.log|文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
                dialog.DefaultExt = "log";
                dialog.FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.log";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ExportLogs(dialog.FileName);
                    MessageBox.Show("日志导出成功！", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void LevelMenu_CheckedChanged(object sender, EventArgs e)
        {
            var level = LogLevel.None;

            if (menuTrace.Checked)
            {
                level |= LogLevel.Trace;
            }

            if (menuDebug.Checked)
            {
                level |= LogLevel.Debug;
            }

            if (menuInfo.Checked)
            {
                level |= LogLevel.Info;
            }

            if (menuWarn.Checked)
            {
                level |= LogLevel.Warn;
            }

            if (menuError.Checked)
            {
                level |= LogLevel.Error;
            }

            if (menuFatal.Checked)
            {
                level |= LogLevel.Fatal;
            }

            CurrentLevel = level;
        }

        private void TxtMaxCount_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtMaxCount.Text, out int value) && value > 0)
            {
                MaxDisplayCount = value;
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnSearch_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            searchKeyword = txtSearch.Text?.Trim() ?? "";
            RefreshDisplay();
        }

        private void GroupMenu_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null)
            {
                return;
            }

            // 取消所有其他菜单项的选中
            foreach (ToolStripMenuItem item in btnGroup.DropDownItems)
            {
                item.Checked = false;
            }

            menuItem.Checked = true;

            if (menuItem == menuAllGroups)
            {
                CurrentGroup = "";
            }
            else
            {
                CurrentGroup = menuItem.Tag as string ?? "";
            }
        }

        private void MenuCopy_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var item in listView.SelectedItems)
            {
                var log = item.Tag as ILogEvent;
                if (log != null)
                {
                    sb.AppendLine($"[{log.Timestamp.ToString(timestampFormat)}] [{log.Level}] {log.Message}");
                }
            }

            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void MenuCopyMessage_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var item in listView.SelectedItems)
            {
                var log = item.Tag as ILogEvent;
                if (log != null)
                {
                    sb.AppendLine(log.Message);
                }
            }

            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void MenuCopyAll_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var log in filteredLogs)
            {
                sb.AppendLine($"[{log.Timestamp.ToString(timestampFormat)}] [{log.Level}] {log.Message}");
            }

            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        #endregion

        #region 主题支持

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();

            if (listView != null)
            {
                listView.UseTheme = UseTheme;
                listView.ThemeName = ThemeName;
            }

            // 自动应用主题颜色方案
            if (autoSelectColorScheme)
            {
                ApplyThemeColorScheme();
            }
        }

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                // 应用主题背景色到ListView
                var bgColor = GetThemeColor(c => c.Surface, SystemColors.Window);
                var textColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);

                BackColor = bgColor;

                if (listView != null)
                {
                    // 强制设置ListView的背景色
                    listView.BackColor = bgColor;
                    listView.ForeColor = textColor;

                    // 确保立即刷新
                    listView.Invalidate();
                }

                // 应用主题到工具栏
                if (toolStrip != null)
                {
                    var toolBgColor = GetThemeColor(c => c.BackgroundSecondary, SystemColors.Control);
                    var toolFgColor = GetThemeColor(c => c.TextPrimary, SystemColors.ControlText);

                    toolStrip.BackColor = toolBgColor;
                    toolStrip.ForeColor = toolFgColor;

                    // 应用到所有工具栏项
                    foreach (ToolStripItem item in toolStrip.Items)
                    {
                        item.BackColor = toolBgColor;
                        item.ForeColor = toolFgColor;
                    }
                }

                // 应用主题到右键菜单
                if (contextMenu != null)
                {
                    contextMenu.BackColor = bgColor;
                    contextMenu.ForeColor = textColor;
                }

                // 自动选择颜色方案
                if (autoSelectColorScheme)
                {
                    ApplyThemeColorScheme();
                }
            }
            else
            {
                // 不使用主题时，设置默认背景色
                BackColor = SystemColors.Window;

                if (listView != null)
                {
                    listView.BackColor = SystemColors.Window;
                    listView.ForeColor = SystemColors.ControlText;
                }
            }

            Invalidate();
        }

        /// <summary>
        /// 根据当前主题应用合适的颜色方案
        /// </summary>
        private void ApplyThemeColorScheme()
        {
            if (!autoSelectColorScheme)
            {
                return;
            }

            if (UseTheme && Theme != null)
            {
                // 根据主题类型选择颜色方案
                colorScheme = LogColorScheme.GetRecommendedScheme(Theme.Type);
            }
            else if (!string.IsNullOrEmpty(ThemeName))
            {
                // 根据主题名称选择颜色方案
                colorScheme = LogColorScheme.GetRecommendedScheme(ThemeName);
            }
            else
            {
                // 默认使用浅色方案
                colorScheme = LogColorScheme.Default;
            }

            RefreshDisplay();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                isRunning = false;
                logEvent?.Set();

                if (processingThread != null && processingThread.IsAlive)
                {
                    processingThread.Join(1000);
                }

                logEvent?.Dispose();
                contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {

        }

        protected override void DrawContent(Graphics g)
        {

        }

        protected override void DrawBorder(Graphics g)
        {

        }

        #endregion
    }


    #region 枚举和辅助类

    /// <summary>
    /// 日志级别颜色配置
    /// </summary>
    public class LogLevelColors
    {
        public LogLevelColors(Color foreColor, Color backColor)
        {
            ForeColor = foreColor;
            BackColor = backColor;
        }

        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
    }

    /// <summary>
    /// 日志颜色方案
    /// </summary>
    public class LogColorScheme
    {
        public LogLevelColors Trace { get; set; }
        public LogLevelColors Debug { get; set; }
        public LogLevelColors Info { get; set; }
        public LogLevelColors Warn { get; set; }
        public LogLevelColors Error { get; set; }
        public LogLevelColors Fatal { get; set; }

        /// <summary>
        /// 默认配色方案
        /// </summary>
        public static LogColorScheme Default => new LogColorScheme
        {
            Trace = new LogLevelColors(Color.FromArgb(128, 128, 128), Color.White),
            Debug = new LogLevelColors(Color.FromArgb(75, 75, 150), Color.FromArgb(240, 240, 255)),
            Info = new LogLevelColors(Color.FromArgb(50, 50, 50), Color.White),
            Warn = new LogLevelColors(Color.FromArgb(180, 100, 0), Color.FromArgb(255, 250, 220)),
            Error = new LogLevelColors(Color.FromArgb(180, 0, 0), Color.FromArgb(255, 240, 240)),
            Fatal = new LogLevelColors(Color.White, Color.FromArgb(180, 0, 0))
        };

        /// <summary>
        /// 深色配色方案(深色背景)
        /// </summary>
        public static LogColorScheme Dark => new LogColorScheme
        {
            Trace = new LogLevelColors(Color.FromArgb(160, 160, 160), Color.FromArgb(40, 40, 40)),
            Debug = new LogLevelColors(Color.FromArgb(150, 180, 220), Color.FromArgb(30, 40, 55)),
            Info = new LogLevelColors(Color.FromArgb(220, 220, 220), Color.FromArgb(45, 45, 45)),
            Warn = new LogLevelColors(Color.FromArgb(255, 200, 100), Color.FromArgb(60, 50, 30)),
            Error = new LogLevelColors(Color.FromArgb(255, 150, 150), Color.FromArgb(70, 30, 30)),
            Fatal = new LogLevelColors(Color.FromArgb(255, 255, 100), Color.FromArgb(120, 20, 20))
        };

        /// <summary>
        /// 高对比度配色方案
        /// </summary>
        public static LogColorScheme HighContrast => new LogColorScheme
        {
            Trace = new LogLevelColors(Color.Black, Color.FromArgb(220, 220, 220)),
            Debug = new LogLevelColors(Color.Black, Color.FromArgb(173, 216, 230)),
            Info = new LogLevelColors(Color.Black, Color.White),
            Warn = new LogLevelColors(Color.Black, Color.FromArgb(255, 255, 0)),
            Error = new LogLevelColors(Color.White, Color.FromArgb(220, 0, 0)),
            Fatal = new LogLevelColors(Color.Yellow, Color.FromArgb(139, 0, 0))
        };

        /// <summary>
        /// 柔和配色方案
        /// </summary>
        public static LogColorScheme Soft => new LogColorScheme
        {
            Trace = new LogLevelColors(Color.FromArgb(120, 120, 120), Color.FromArgb(250, 250, 250)),
            Debug = new LogLevelColors(Color.FromArgb(80, 100, 140), Color.FromArgb(245, 248, 255)),
            Info = new LogLevelColors(Color.FromArgb(60, 60, 60), Color.FromArgb(255, 255, 255)),
            Warn = new LogLevelColors(Color.FromArgb(160, 100, 30), Color.FromArgb(255, 252, 235)),
            Error = new LogLevelColors(Color.FromArgb(160, 50, 50), Color.FromArgb(255, 245, 245)),
            Fatal = new LogLevelColors(Color.White, Color.FromArgb(160, 50, 50))
        };

        /// <summary>
        /// 彩色配色方案
        /// </summary>
        public static LogColorScheme Colorful => new LogColorScheme
        {
            Trace = new LogLevelColors(Color.Gray, Color.White),
            Debug = new LogLevelColors(Color.FromArgb(0, 100, 200), Color.FromArgb(230, 240, 255)),
            Info = new LogLevelColors(Color.FromArgb(0, 120, 0), Color.FromArgb(240, 255, 240)),
            Warn = new LogLevelColors(Color.FromArgb(200, 120, 0), Color.FromArgb(255, 248, 220)),
            Error = new LogLevelColors(Color.White, Color.FromArgb(220, 80, 80)),
            Fatal = new LogLevelColors(Color.White, Color.FromArgb(180, 0, 0))
        };

        /// <summary>
        /// 根据主题类型获取推荐的配色方案
        /// </summary>
        public static LogColorScheme GetRecommendedScheme(ThemeType themeType)
        {
            switch (themeType)
            {
                case ThemeType.Light:
                    return Default;
                case ThemeType.Dark:
                    return Dark;
                case ThemeType.HighContrast:
                    return HighContrast;
                default:
                    return Default;
            }
        }

        /// <summary>
        /// 根据主题名称获取推荐的配色方案
        /// </summary>
        public static LogColorScheme GetRecommendedScheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
            {
                return Default;
            }

            var lowerName = themeName.ToLower();

            if (lowerName.Contains("dark") || lowerName.Contains("暗"))
            {
                return Dark;
            }

            if (lowerName.Contains("contrast") || lowerName.Contains("对比"))
            {
                return HighContrast;
            }

            if (lowerName.Contains("light") || lowerName.Contains("亮"))
            {
                return Default;
            }

            // 默认返回浅色方案
            return Default;
        }

        public LogLevelColors GetColors(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return Trace;
                case LogLevel.Debug:
                    return Debug;
                case LogLevel.Info:
                    return Info;
                case LogLevel.Warn:
                    return Warn;
                case LogLevel.Error:
                    return Error;
                case LogLevel.Fatal:
                    return Fatal;
                default:
                    return Info;
            }
        }
    }

    #endregion

    #region 设计时支持

    public class FluentLogViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentLogViewActionList(Component));
                }
                return actionLists;
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                return new DesignerVerbCollection(new[]
                {
                    new DesignerVerb("清空日志", (s, e) => ClearLogs()),
                    new DesignerVerb("添加测试日志", (s, e) => AddTestLogs()),
                    new DesignerVerb("切换工具栏", (s, e) => ToggleToolStrip())
                });
            }
        }

        private void ClearLogs()
        {
            if (Control is FluentLogView logView)
            {
                logView.Clear();
            }
        }

        private void AddTestLogs()
        {
            if (Control is FluentLogView logView)
            {
                logView.AddLog(new DefaultLogEvent(LogLevel.Trace, "这是一条 Trace 级别的测试日志", "TestSource"));
                logView.AddLog(new DefaultLogEvent(LogLevel.Debug, "这是一条 Debug 级别的测试日志", "TestSource"));
                logView.AddLog(new DefaultLogEvent(LogLevel.Info, "这是一条 Info 级别的测试日志", "TestSource"));
                logView.AddLog(new DefaultLogEvent(LogLevel.Warn, "这是一条 Warning 级别的测试日志", "TestSource"));
                logView.AddLog(new DefaultLogEvent(LogLevel.Error, "这是一条 Error 级别的测试日志", "TestSource"));
                logView.AddLog(new DefaultLogEvent(LogLevel.Fatal, "这是一条 Fatal 级别的测试日志", "TestSource"));
            }
        }

        private void ToggleToolStrip()
        {
            if (Control is FluentLogView logView)
            {
                logView.ShowToolStrip = !logView.ShowToolStrip;
                RaiseComponentChanged(TypeDescriptor.GetProperties(logView)["ShowToolStrip"], null, null);
            }
        }

        private void RaiseComponentChanged(PropertyDescriptor property, object oldValue, object newValue)
        {
            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            changeService?.OnComponentChanged(Control, property, oldValue, newValue);
        }
    }

    public class FluentLogViewActionList : DesignerActionList
    {
        private FluentLogView logView;

        public FluentLogViewActionList(IComponent component) : base(component)
        {
            logView = component as FluentLogView;
        }

        public bool ShowToolStrip
        {
            get => logView.ShowToolStrip;
            set => SetProperty("ShowToolStrip", value);
        }

        public int MaxDisplayCount
        {
            get => logView.MaxDisplayCount;
            set => SetProperty("MaxDisplayCount", value);
        }

        public int MaxStoreCount
        {
            get => logView.MaxStoreCount;
            set => SetProperty("MaxStoreCount", value);
        }

        public LogLevel CurrentLevel
        {
            get => logView.CurrentLevel;
            set => SetProperty("CurrentLevel", value);
        }

        public string TimestampFormat
        {
            get => logView.TimestampFormat;
            set => SetProperty("TimestampFormat", value);
        }

        public bool AutoSelectColorScheme
        {
            get => logView.AutoSelectColorScheme;
            set => SetProperty("AutoSelectColorScheme", value);
        }

        public void ClearLogs()
        {
            logView.Clear();
        }

        public void AddTestLogs()
        {
            logView.AddLog(new DefaultLogEvent(LogLevel.Trace, "Trace 测试日志", "Test"));
            logView.AddLog(new DefaultLogEvent(LogLevel.Debug, "Debug 测试日志", "Test"));
            logView.AddLog(new DefaultLogEvent(LogLevel.Info, "Info 测试日志", "Test"));
            logView.AddLog(new DefaultLogEvent(LogLevel.Warn, "Warning 测试日志", "Test"));
            logView.AddLog(new DefaultLogEvent(LogLevel.Error, "Error 测试日志", "Test"));
            logView.AddLog(new DefaultLogEvent(LogLevel.Fatal, "Fatal 测试日志", "Test"));
        }

        public void SetColorScheme_Default()
        {
            logView.AutoSelectColorScheme = false;
            logView.ColorScheme = LogColorScheme.Default;
        }

        public void SetColorScheme_Dark()
        {
            logView.AutoSelectColorScheme = false;
            logView.ColorScheme = LogColorScheme.Dark;
        }

        public void SetColorScheme_HighContrast()
        {
            logView.AutoSelectColorScheme = false;
            logView.ColorScheme = LogColorScheme.HighContrast;
        }

        public void SetColorScheme_Soft()
        {
            logView.AutoSelectColorScheme = false;
            logView.ColorScheme = LogColorScheme.Soft;
        }

        public void SetColorScheme_Colorful()
        {
            logView.AutoSelectColorScheme = false;
            logView.ColorScheme = LogColorScheme.Colorful;
        }

        public void SetColorScheme_Auto()
        {
            logView.AutoSelectColorScheme = true;
        }

        private void SetProperty(string propertyName, object value)
        {
            var property = TypeDescriptor.GetProperties(logView)[propertyName];
            if (property != null)
            {
                property.SetValue(logView, value);
                RaiseComponentChanged(property);
            }
        }

        private void RaiseComponentChanged(PropertyDescriptor property)
        {
            var changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            changeService?.OnComponentChanged(logView, property, null, null);
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 快速操作
            items.Add(new DesignerActionHeaderItem("快速操作"));
            items.Add(new DesignerActionMethodItem(this, "AddTestLogs", "添加测试日志", "快速操作", true));
            items.Add(new DesignerActionMethodItem(this, "ClearLogs", "清空日志", "快速操作", false));

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowToolStrip", "显示工具栏", "外观"));
            items.Add(new DesignerActionPropertyItem("TimestampFormat", "时间格式:", "外观"));

            // 颜色方案
            items.Add(new DesignerActionHeaderItem("颜色方案"));
            items.Add(new DesignerActionPropertyItem("AutoSelectColorScheme", "自动选择方案", "颜色方案"));
            items.Add(new DesignerActionMethodItem(this, "SetColorScheme_Auto", "→ 自动(跟随主题)", "颜色方案", false));
            items.Add(new DesignerActionMethodItem(this, "SetColorScheme_Default", "→ 默认(浅色)", "颜色方案", false));
            items.Add(new DesignerActionMethodItem(this, "SetColorScheme_Dark", "→ 深色", "颜色方案", false));
            items.Add(new DesignerActionMethodItem(this, "SetColorScheme_HighContrast", "→ 高对比度", "颜色方案", false));
            items.Add(new DesignerActionMethodItem(this, "SetColorScheme_Soft", "→ 柔和", "颜色方案", false));
            items.Add(new DesignerActionMethodItem(this, "SetColorScheme_Colorful", "→ 彩色", "颜色方案", false));

            // 行为
            items.Add(new DesignerActionHeaderItem("行为"));
            items.Add(new DesignerActionPropertyItem("MaxDisplayCount", "最大显示数:", "行为"));
            items.Add(new DesignerActionPropertyItem("MaxStoreCount", "最大存储数:", "行为"));
            items.Add(new DesignerActionPropertyItem("CurrentLevel", "日志级别:", "行为"));

            return items;
        }
    }

    #endregion
}
