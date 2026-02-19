using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    public class FluentFileSelect : FluentContainerBase
    {
        #region 字段

        private Label labelText;
        private FluentRepeater fileListRepeater;
        private FluentButton selectButton;

        private string labelTextValue = "选择文件：";
        private bool showLabel = true;
        private FileSelectLayout selectLayout = FileSelectLayout.SingleLine;
        private string filter = "所有文件|*.*";
        private bool multiSelect = true;
        private int maxFileCount = int.MaxValue;
        private Size fileItemSize = new Size(200, 28);
        private bool showFileSize = true;
        private Padding listBoxMargin = new Padding(0);
        private Padding listBoxSpacing = new Padding(4);

        private Font buttonFont;
        private bool autoSizeFileItems = true; // 默认启用自适应

        private HashSet<string> filePathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private List<FileItemInfo> fileItems = new List<FileItemInfo>();

        #endregion

        #region 构造函数

        public FluentFileSelect()
        {
            buttonFont = new Font("Microsoft YaHei UI", 9F);
            InitializeComponents();
            PerformLayout();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(400, 36);
            this.Padding = new Padding(6);

            // 创建标签 - 设置AutoSize为true
            labelText = new Label
            {
                Text = labelTextValue,
                AutoSize = true, // 自动调整大小
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
            this.Controls.Add(labelText);

            // 创建文件列表
            fileListRepeater = new FluentRepeater
            {
                BorderWidth = 1,
                BorderColor = Color.FromArgb(200, 200, 200),
                Padding = new Padding(4),
                AutoScroll = true,
                LayoutMode = RepeaterLayoutMode.Auto,
                ItemDefaultSize = fileItemSize,
                DeleteIconSize = 20,
                ItemPadding = new Padding(2),
                ShowAddButton = false,
                AutoSizeItems = false
            };
            fileListRepeater.SetItemFactory<FileItemInfo>(CreateFileItemControl);
            fileListRepeater.SetItemSizeCalculator(CalculateFileItemSize);
            fileListRepeater.ItemRemoved += FileListRepeater_ItemRemoved;
            this.Controls.Add(fileListRepeater);

            // 创建选择按钮
            selectButton = new FluentButton
            {
                Text = "选择文件",
                Size = new Size(100, 30),
                ButtonStyle = ButtonStyle.Primary,
                CornerRadius = 0,
                Font = buttonFont
            };
            selectButton.Click += SelectButton_Click;
            this.Controls.Add(selectButton);

            UpdateLayout();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 标签文本
        /// </summary>
        [Category("Fluent")]
        [Description("标签显示的文本")]
        [DefaultValue("选择文件：")]
        public string LabelText
        {
            get => labelTextValue;
            set
            {
                if (labelTextValue != value)
                {
                    labelTextValue = value;
                    labelText.Text = value;
                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// 是否显示标签
        /// </summary>
        [Category("Fluent")]
        [Description("是否显示标签文本")]
        [DefaultValue(true)]
        public bool ShowLabel
        {
            get => showLabel;
            set
            {
                if (showLabel != value)
                {
                    showLabel = value;
                    labelText.Visible = value;
                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// 布局模式
        /// </summary>
        [Category("Fluent")]
        [Description("控件的布局模式")]
        [DefaultValue(FileSelectLayout.SingleLine)]
        public FileSelectLayout SelectLayout
        {
            get => selectLayout;
            set
            {
                if (selectLayout != value)
                {
                    selectLayout = value;

                    switch (value)
                    {
                        case FileSelectLayout.SingleLine:
                            fileListRepeater.AutoScroll = false;
                            fileListRepeater.AutoSizeItems = false;
                            break;
                        case FileSelectLayout.MultiLine:
                            fileListRepeater.AutoScroll = true;
                            fileListRepeater.AutoSizeItems = true;
                            break;
                    }

                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// 文件过滤器
        /// </summary>
        [Category("Fluent")]
        [Description("文件选择对话框的过滤器")]
        [DefaultValue("所有文件|*.*")]
        public string Filter
        {
            get => filter;
            set => filter = value ?? "所有文件|*.*";
        }

        /// <summary>
        /// 是否允许多选
        /// </summary>
        [Category("Fluent")]
        [Description("是否允许在对话框中多选文件")]
        [DefaultValue(true)]
        public bool MultiSelect
        {
            get => multiSelect;
            set => multiSelect = value;
        }

        /// <summary>
        /// 最大文件数量
        /// </summary>
        [Category("Fluent")]
        [Description("允许选择的最大文件数量")]
        [DefaultValue(int.MaxValue)]
        public int MaxFileCount
        {
            get => maxFileCount;
            set
            {
                maxFileCount = Math.Max(1, value);
                UpdateButtonState();
            }
        }

        /// <summary>
        /// 文件项大小（当AutoSizeFileItems为false时使用）
        /// </summary>
        [Category("Fluent")]
        [Description("文件列表中每个文件项的大小（自适应模式下仅高度生效）")]
        public Size FileItemSize
        {
            get => fileItemSize;
            set
            {
                if (fileItemSize != value)
                {
                    fileItemSize = value;
                    fileListRepeater.ItemDefaultSize = value;
                }
                RefreshFileItems();
            }
        }

        /// <summary>
        /// 是否显示文件大小
        /// </summary>
        [Category("Fluent")]
        [Description("是否在文件项中显示文件大小")]
        [DefaultValue(true)]
        public bool ShowFileSize
        {
            get => showFileSize;
            set
            {
                if (showFileSize != value)
                {
                    showFileSize = value;
                    RefreshFileItems();
                }
            }
        }

        /// <summary>
        /// 文件列表框边距
        /// </summary>
        [Category("Fluent")]
        [Description("文件列表框与控件边界的间距")]
        public Padding ListBoxMargin
        {
            get => listBoxMargin;
            set
            {
                if (listBoxMargin != value)
                {
                    listBoxMargin = value;
                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// 是否自适应文件项大小
        /// </summary>
        [Category("Fluent")]
        [Description("是否根据文件名长度自动调整文件项宽度")]
        [DefaultValue(true)]
        public bool AutoSizeFileItems
        {
            get => autoSizeFileItems;
            set
            {
                if (autoSizeFileItems != value)
                {
                    autoSizeFileItems = value;
                    fileListRepeater.AutoSizeItems = value;
                }
            }
        }

        /// <summary>
        /// 选择按钮文本
        /// </summary>
        [Category("Fluent")]
        [Description("选择按钮显示的文本")]
        [DefaultValue("选择文件")]
        public string SelectButtonText
        {
            get => selectButton.Text;
            set => selectButton.Text = value;
        }

        /// <summary>
        /// 选择按钮大小
        /// </summary>
        [Category("Fluent")]
        [Description("选择按钮的大小")]
        public Size SelectButtonSize
        {
            get => selectButton.Size;
            set
            {
                selectButton.Size = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 选择按钮字体
        /// </summary>
        [Category("Fluent")]
        [Description("选择按钮的字体")]
        public Font SelectButtonFont
        {
            get => buttonFont;
            set
            {
                if (buttonFont != value)
                {
                    buttonFont = value;
                    selectButton.Font = value;
                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// 文件列表框布局模式
        /// </summary>
        [Category("Fluent")]
        [Description("文件列表框的布局模式")]
        [DefaultValue(RepeaterLayoutMode.Vertical)]
        public RepeaterLayoutMode FileListLayoutMode
        {
            get => fileListRepeater.LayoutMode;
            set => fileListRepeater.LayoutMode = value;
        }

        [Category("Fluent")]
        [Description("文件列表框间距")]
        public Padding ListBoxSpacing
        {
            get => listBoxSpacing;
            set
            {
                listBoxSpacing = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 当前文件数量
        /// </summary>
        [Browsable(false)]
        public int FileCount => fileItems.Count;

        /// <summary>
        /// 是否已达到最大文件数
        /// </summary>
        [Browsable(false)]
        public bool IsFull => fileItems.Count >= maxFileCount;

        /// <summary>
        /// 获取所有文件路径
        /// </summary>
        [Browsable(false)]
        public string[] FilePaths => fileItems.Select(f => f.FilePath).ToArray();

        /// <summary>
        /// 获取所有文件信息
        /// </summary>
        [Browsable(false)]
        public FileItemInfo[] FileInfos => fileItems.ToArray();

        #endregion

        #region 事件

        /// <summary>
        /// 文件添加事件
        /// </summary>
        [Category("Fluent")]
        [Description("文件被添加时触发")]
        public event EventHandler<FileEventArgs> FileAdded;

        /// <summary>
        /// 文件移除事件
        /// </summary>
        [Category("Fluent")]
        [Description("文件被移除时触发")]
        public event EventHandler<FileEventArgs> FileRemoved;

        /// <summary>
        /// 文件列表变化事件
        /// </summary>
        [Category("Fluent")]
        [Description("文件列表发生变化时触发")]
        public event EventHandler FilesChanged;

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加文件
        /// </summary>
        public bool AddFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            if (!File.Exists(filePath))
            {
                FluentMessageManager.Instance.Error($"文件不存在：{filePath}");
                return false;
            }

            if (IsFull)
            {
                FluentMessageManager.Instance.Warning($"已达到最大文件数量限制：{maxFileCount}");
                return false;
            }

            // 检查是否已存在
            if (filePathSet.Contains(filePath))
            {
                FluentMessageManager.Instance.Warning($"文件已存在：{Path.GetFileName(filePath)}");
                return false;
            }

            // 添加文件
            var fileInfo = new FileItemInfo(filePath);
            fileItems.Add(fileInfo);
            filePathSet.Add(filePath);

            // 在Repeater中添加控件，直接传入fileInfo
            var control = fileListRepeater.AddItem(fileInfo);

            UpdateButtonState();
            OnFileAdded(new FileEventArgs(fileInfo));
            OnFilesChanged();

            return true;
        }

        /// <summary>
        /// 添加多个文件
        /// </summary>
        public void AddFiles(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
            {
                return;
            }

            using (CreateUpdateScope())
            {
                foreach (var path in filePaths)
                {
                    AddFile(path);
                }
            }
        }

        /// <summary>
        /// 移除文件
        /// </summary>
        public bool RemoveFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var fileInfo = fileItems.FirstOrDefault(f =>
                f.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

            if (fileInfo == null)
            {
                return false;
            }

            int index = fileItems.IndexOf(fileInfo);
            return RemoveFileAt(index);
        }

        /// <summary>
        /// 移除指定索引的文件
        /// </summary>
        public bool RemoveFileAt(int index)
        {
            if (index < 0 || index >= fileItems.Count)
            {
                return false;
            }

            var fileInfo = fileItems[index];
            fileItems.RemoveAt(index);
            filePathSet.Remove(fileInfo.FilePath);

            fileListRepeater.RemoveItemAt(index);

            UpdateButtonState();
            OnFileRemoved(new FileEventArgs(fileInfo));
            OnFilesChanged();

            return true;
        }

        /// <summary>
        /// 清除所有文件
        /// </summary>
        public void ClearFiles()
        {
            if (fileItems.Count == 0)
            {
                return;
            }

            using (CreateUpdateScope())
            {
                fileItems.Clear();
                filePathSet.Clear();
                fileListRepeater.ClearItems();

                UpdateButtonState();
                OnFilesChanged();
            }
        }

        /// <summary>
        /// 打开文件选择对话框
        /// </summary>
        public void ShowFileDialog()
        {
            SelectButton_Click(selectButton, EventArgs.Empty);
        }

        #endregion

        #region 布局

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (labelText == null || fileListRepeater == null || selectButton == null)
            {
                return;
            }

            SuspendLayout();

            switch (selectLayout)
            {
                case FileSelectLayout.SingleLine:
                    LayoutSingleLine();
                    break;
                case FileSelectLayout.MultiLine:
                    LayoutMultiLine();
                    break;
            }

            ResumeLayout(true);
        }

        private void LayoutSingleLine()
        {
            int padding = ListBoxSpacing.Top;
            int spacing = ListBoxSpacing.Left;

            int currentX = spacing;
            int currentY = padding;
            int availableHeight = this.ClientSize.Height - padding * 2;

            // 标签
            if (showLabel)
            {
                labelText.MaximumSize = new Size(0, 0); // 不限制，让其自动调整
                labelText.AutoSize = true;

                var labelSize = labelText.PreferredSize;
                labelText.Bounds = new Rectangle(
                    currentX,
                    currentY + (availableHeight - labelSize.Height) / 2,
                    labelSize.Width,
                    labelSize.Height
                );
                labelText.Visible = true;
                currentX += labelSize.Width + spacing;
            }
            else
            {
                labelText.Visible = false;
            }

            // 选择按钮（在右侧）
            int buttonWidth = selectButton.Width;
            int buttonHeight = selectButton.Height;
            selectButton.Bounds = new Rectangle(
                this.ClientSize.Width - ListBoxSpacing.Horizontal - buttonWidth,
                currentY + (availableHeight - buttonHeight) / 2,
                buttonWidth,
                buttonHeight
            );

            // 文件列表
            int repeaterX = currentX + listBoxMargin.Left;
            int repeaterY = currentY + listBoxMargin.Top;
            int repeaterWidth = selectButton.Left - repeaterX - listBoxMargin.Right - listBoxSpacing.Horizontal;
            int repeaterHeight = availableHeight - listBoxMargin.Top - listBoxMargin.Bottom - listBoxSpacing.Vertical;

            fileListRepeater.Bounds = new Rectangle(
                repeaterX,
                repeaterY,
                Math.Max(100, repeaterWidth),
                Math.Max(fileItemSize.Height + 4, repeaterHeight)
            );
            fileListRepeater.Padding = new Padding(2);
            ListBoxMargin = new Padding(listBoxMargin.Left, ListBoxSpacing.Top * (-1) + 1, listBoxMargin.Right, listBoxMargin.Bottom);
        }

        private void LayoutMultiLine()
        {
            int padding = ListBoxSpacing.Top;
            int spacing = ListBoxSpacing.Left;

            int currentX = padding;
            int currentY = padding;

            // 计算第一行高度（标签和按钮中的较大者）
            int buttonHeight = selectButton.Height;
            int labelHeight = 0;

            if (showLabel)
            {
                labelText.MaximumSize = new Size(0, 0);
                labelText.AutoSize = true;
                labelHeight = labelText.PreferredSize.Height;
            }

            int firstRowHeight = Math.Max(buttonHeight, labelHeight);

            // 第一行：标签和按钮
            if (showLabel)
            {
                var labelSize = labelText.PreferredSize;
                labelText.Bounds = new Rectangle(
                    currentX,
                    currentY + (firstRowHeight - labelSize.Height) / 2,
                    labelSize.Width,
                    labelSize.Height
                );
                labelText.Visible = true;
            }
            else
            {
                labelText.Visible = false;
            }

            // 按钮在右侧
            int buttonWidth = selectButton.Width;
            selectButton.Bounds = new Rectangle(
                this.ClientSize.Width - ListBoxSpacing.Horizontal - buttonWidth,
                currentY + (firstRowHeight - buttonHeight) / 2,
                buttonWidth,
                buttonHeight
            );

            // 第二行：文件列表 - 应用ListBoxMargin
            currentY += firstRowHeight + spacing;
            int repeaterX = padding + listBoxMargin.Left;
            int repeaterY = currentY + listBoxMargin.Top;
            int repeaterWidth = this.ClientSize.Width - padding * 2 - listBoxMargin.Left - listBoxMargin.Right;
            int repeaterHeight = this.ClientSize.Height - repeaterY - padding - listBoxMargin.Bottom;

            fileListRepeater.Bounds = new Rectangle(
                repeaterX,
                repeaterY,
                Math.Max(100, repeaterWidth),
                Math.Max(fileItemSize.Height + 4, repeaterHeight)
            );
            fileListRepeater.Padding = new Padding(6);
        }

        #endregion

        #region 文件项控件

        private Control CreateFileItemControl(FileItemInfo fileInfo)
        {
            var panel = new Panel
            {
                Size = fileItemSize,
                BackColor = Color.White,
                Padding = new Padding(2, 1, 2, 1),
                AutoSize = false
            };

            // 单行显示：文件名 + 文件大小
            var contentLabel = new Label
            {
                Name = "contentLabel",
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 8.5F),
                ForeColor = Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                UseMnemonic = false
            };

            if (fileInfo != null)
            {
                if (showFileSize)
                {
                    contentLabel.Text = $"{fileInfo.FileName}  ({fileInfo.FileSizeText})";
                }
                else
                {
                    contentLabel.Text = fileInfo.FileName;
                }
            }

            panel.Controls.Add(contentLabel);

            // 设置工具提示
            if (fileInfo != null)
            {
                var toolTip = new ToolTip
                {
                    AutoPopDelay = 5000,
                    InitialDelay = 500,
                    ReshowDelay = 100
                };
                toolTip.SetToolTip(panel, fileInfo.FilePath);
                toolTip.SetToolTip(contentLabel, fileInfo.FilePath);
            }

            // 计算合适的大小
            if (fileInfo != null && autoSizeFileItems)
            {
                using (var g = contentLabel.CreateGraphics())
                {
                    var textSize = g.MeasureString(contentLabel.Text, contentLabel.Font);

                    // 计算宽度：文本宽度 + 内边距 + 删除按钮空间 + 额外余量
                    int width = (int)Math.Ceiling(textSize.Width) +
                               panel.Padding.Left + panel.Padding.Right +
                               fileListRepeater.DeleteIconSize + 8 + 20;

                    panel.Size = new Size(width, fileItemSize.Height);
                }
            }
            else
            {
                panel.Size = fileItemSize;
            }

            return panel;
        }

        //private void UpdateFileItemControl(Control control, FileItemInfo fileInfo)
        //{
        //    if (control is Panel panel)
        //    {
        //        var contentLabel = panel.Controls["contentLabel"] as Label;

        //        if (contentLabel != null)
        //        {
        //            // 组合显示：文件名 + 文件大小
        //            if (showFileSize)
        //            {
        //                contentLabel.Text = $"{fileInfo.FileName}  ({fileInfo.FileSizeText})";
        //            }
        //            else
        //            {
        //                contentLabel.Text = fileInfo.FileName;
        //            }
        //        }

        //        // 设置工具提示显示完整路径
        //        var toolTip = new ToolTip
        //        {
        //            AutoPopDelay = 5000,
        //            InitialDelay = 500,
        //            ReshowDelay = 100
        //        };
        //        toolTip.SetToolTip(panel, fileInfo.FilePath);

        //        if (contentLabel != null)
        //        {
        //            toolTip.SetToolTip(contentLabel, fileInfo.FilePath);
        //        }
        //    }
        //}

        /// <summary>
        /// 计算文件项的适当大小
        /// </summary>
        private Size CalculateFileItemSize(Control control)
        {
            if (!autoSizeFileItems)
            {
                return fileItemSize;
            }

            if (control is Panel panel)
            {
                var contentLabel = panel.Controls["contentLabel"] as Label;
                if (contentLabel != null && !string.IsNullOrEmpty(contentLabel.Text))
                {
                    using (var g = contentLabel.CreateGraphics())
                    {
                        var textSize = g.MeasureString(contentLabel.Text, contentLabel.Font);

                        // 计算宽度：文本宽度 + 内边距 + 删除按钮空间 + 额外余量
                        int width = (int)Math.Ceiling(textSize.Width) +
                                   panel.Padding.Left + panel.Padding.Right +
                                   fileListRepeater.DeleteIconSize + 8 + 20;

                        // 返回已经在创建时设置的大小，或重新计算
                        return new Size(width, fileItemSize.Height);
                    }
                }
            }

            return control.Size;
        }

        private void RefreshFileItems()
        {
            // 清空并重新添加所有项
            fileListRepeater.ClearItems();

            foreach (var fileInfo in fileItems)
            {
                fileListRepeater.AddItem(fileInfo);
            }
        }

        #endregion

        #region 事件处理

        private void SelectButton_Click(object sender, EventArgs e)
        {
            if (IsFull)
            {
                FluentMessageManager.Instance.Warning($"已达到最大文件数量限制：{maxFileCount}");
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = filter;
                dialog.Multiselect = multiSelect;
                dialog.Title = "选择文件";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.FileNames.Length > 0)
                    {
                        if(SelectLayout == FileSelectLayout.SingleLine)
                        {
                            fileListRepeater.ClearItems();
                        }
                        AddFiles(dialog.FileNames);
                    }
                }
            }
        }

        private void FileListRepeater_ItemRemoved(object sender, RepeaterItemEventArgs e)
        {
            // 通过控件找到对应的文件信息
            if (e.Index >= 0 && e.Index < fileItems.Count)
            {
                var fileInfo = fileItems[e.Index];
                fileItems.RemoveAt(e.Index);
                filePathSet.Remove(fileInfo.FilePath);

                UpdateButtonState();
                OnFileRemoved(new FileEventArgs(fileInfo));
                OnFilesChanged();
            }
        }

        private void UpdateButtonState()
        {
            selectButton.Enabled = !IsFull;
        }

        #endregion

        #region 事件触发

        protected virtual void OnFileAdded(FileEventArgs e)
        {
            FileAdded?.Invoke(this, e);
        }

        protected virtual void OnFileRemoved(FileEventArgs e)
        {
            FileRemoved?.Invoke(this, e);
        }

        protected virtual void OnFilesChanged()
        {
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 抽象方法实现

        protected override void DrawBackground(Graphics g)
        {
            // 使用透明背景或指定颜色
            using (var brush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 内容由子控件绘制
        }

        protected override void DrawBorder(Graphics g)
        {
            // 可选：绘制控件边框
        }

        #endregion

        #region 主题支持

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                // 应用主题到子控件
                labelText.Font = Theme.Typography.Body;
                labelText.ForeColor = Theme.Colors.TextPrimary;

                selectButton.UseTheme = true;
                selectButton.Theme = Theme;

                fileListRepeater.UseTheme = true;
                fileListRepeater.Theme = Theme;
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fileItems.Clear();
                filePathSet.Clear();

                buttonFont?.Dispose();
                labelText?.Dispose();
                fileListRepeater?.Dispose();
                selectButton?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region 枚举和辅助类

    /// <summary>
    /// 文件选择布局模式
    /// </summary>
    public enum FileSelectLayout
    {
        SingleLine,     // 单行布局
        MultiLine       // 多行布局
    }

    /// <summary>
    /// 文件项信息
    /// </summary>
    public class FileItemInfo
    {
        public FileItemInfo(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);

            try
            {
                var fileInfo = new FileInfo(filePath);
                FileSize = fileInfo.Length;
                FileSizeText = FormatFileSize(FileSize);
                CreationTime = fileInfo.CreationTime;
                LastModifiedTime = fileInfo.LastWriteTime;
            }
            catch
            {
                FileSize = 0;
                FileSizeText = "未知";
                CreationTime = DateTime.MinValue;
                LastModifiedTime = DateTime.MinValue;
            }
        }

        /// <summary>
        /// 文件完整路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// 文件大小文本
        /// </summary>
        public string FileSizeText { get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModifiedTime { get; }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public override string ToString()
        {
            return $"{FileName} ({FileSizeText})";
        }
    }

    /// <summary>
    /// 文件事件参数
    /// </summary>
    public class FileEventArgs : EventArgs
    {
        public FileEventArgs(FileItemInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        /// <summary>
        /// 文件信息
        /// </summary>
        public FileItemInfo FileInfo { get; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath => FileInfo.FilePath;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName => FileInfo.FileName;
    }

    #endregion
}
