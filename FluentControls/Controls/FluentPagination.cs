using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    [DefaultEvent("PageChanged")]
    [DefaultProperty("DataSource")]
    public class FluentPagination : FluentControlBase
    {
        #region 字段

        private IList dataSource;
        private int currentPage = 1;
        private int pageSize = 10;
        private int totalRecords = 0;
        private int totalPages = 0;

        // UI 控件
        private FluentButton btnFirst;
        private FluentButton btnPrevious;
        private FluentButton btnNext;
        private FluentButton btnLast;
        private FluentButton btnGo;
        private TextBox txtPageNumber;
        private Label lblPageInfo;
        private FluentComboBox cmbPageSize;  // 页面大小选择器
        private Label lblPageSize;

        // 布局
        private int buttonWidth = 70;
        private int buttonHeight = 28;
        private int spacing = 8;
        private int pageInputWidth = 60;
        private int pageSizeWidth = 80;

        // 文本设置
        private string firstPageText = "首页";
        private string previousPageText = "上一页";
        private string nextPageText = "下一页";
        private string lastPageText = "末页";
        private string goText = "跳转";
        private string pageInfoFormat = "第 {0}/{1} 页，共 {2} 条";
        private string pageSizeText = "每页";
        private bool showFirstLastButtons = true;
        private bool showPreviousNextButtons = true;
        private bool showPageInfo = true;
        private bool showPageJump = true;
        private bool showPageSizeSelector = true;

        // 页面大小选项
        private List<int> pageSizeOptions = new List<int> { 20, 50, 100 };

        // 外部控件绑定
        private object boundControl;

        #endregion

        #region 构造函数

        public FluentPagination()
        {
            InitializeComponents();
            Size = new Size(600, 40);
            UpdateUI();
        }

        private void InitializeComponents()
        {
            // 首页按钮
            btnFirst = new FluentButton
            {
                Text = firstPageText,
                Size = new Size(buttonWidth, buttonHeight)
            };
            btnFirst.Click += (s, e) => GoToFirstPage();

            // 上一页按钮
            btnPrevious = new FluentButton
            {
                Text = previousPageText,
                Size = new Size(buttonWidth, buttonHeight)
            };
            btnPrevious.Click += (s, e) => GoToPreviousPage();

            // 下一页按钮
            btnNext = new FluentButton
            {
                Text = nextPageText,
                Size = new Size(buttonWidth, buttonHeight)
            };
            btnNext.Click += (s, e) => GoToNextPage();

            // 末页按钮
            btnLast = new FluentButton
            {
                Text = lastPageText,
                Size = new Size(buttonWidth, buttonHeight)
            };
            btnLast.Click += (s, e) => GoToLastPage();

            // 页码输入框
            txtPageNumber = new TextBox
            {
                Width = pageInputWidth,
                Height = buttonHeight,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Microsoft YaHei", 9f)
            };
            txtPageNumber.KeyPress += OnPageNumberKeyPress;
            txtPageNumber.KeyDown += OnPageNumberKeyDown;

            // 跳转按钮
            btnGo = new FluentButton
            {
                Text = goText,
                Size = new Size(buttonWidth, buttonHeight)
            };
            btnGo.Click += (s, e) => GoToInputPage();

            // 页面信息标签
            lblPageInfo = new Label
            {
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei", 9f)
            };

            // 页面大小标签
            lblPageSize = new Label
            {
                Text = pageSizeText,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei", 9f)
            };

            // 页面大小选择器
            cmbPageSize = new FluentComboBox
            {
                Width = pageSizeWidth,
                Height = buttonHeight,
                OnlySelection = true,  // 只允许选择
                DropDownItemCount = 6
            };

            // 初始化页面大小选项
            InitializePageSizeOptions();

            cmbPageSize.SelectedIndexChanged += OnPageSizeChanged;

            Controls.AddRange(new Control[]
            {
                btnFirst, btnPrevious, txtPageNumber, btnGo,
                btnNext, btnLast, lblPageInfo,
                lblPageSize, cmbPageSize  // 添加新控件
            });
        }

        /// <summary>
        /// 初始化页面大小选项
        /// </summary>
        private void InitializePageSizeOptions()
        {
            cmbPageSize.Items.Clear();

            foreach (var size in pageSizeOptions)
            {
                cmbPageSize.Items.Add(size);
            }

            // 设置当前选中项
            var currentIndex = pageSizeOptions.IndexOf(pageSize);
            if (currentIndex >= 0)
            {
                cmbPageSize.SelectedIndex = currentIndex;
            }
            else if (pageSizeOptions.Count > 0)
            {
                cmbPageSize.SelectedIndex = 0;
            }
        }

        #endregion

        #region 属性

        /// <summary>
        /// 数据源
        /// </summary>
        [Category("数据")]
        [Description("分页数据源")]
        [DefaultValue(null)]
        public IList DataSource
        {
            get => dataSource;
            set
            {
                if (dataSource != value)
                {
                    dataSource = value;
                    CalculateTotalPages();

                    // 根据数据源状态控制页面大小选择器
                    UpdatePageSizeSelectorState();

                    GoToFirstPage();
                }
            }
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        [Category("分页")]
        [Description("当前页码")]
        [DefaultValue(1)]
        public int CurrentPage
        {
            get => currentPage;
            set
            {
                if (value < 1 || value > totalPages)
                {
                    return;
                }

                if (currentPage != value)
                {
                    currentPage = value;
                    OnPageChanged();
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// 每页大小
        /// </summary>
        [Category("分页")]
        [Description("每页显示的记录数")]
        [DefaultValue(10)]
        public int PageSize
        {
            get => pageSize;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                if (pageSize != value)
                {
                    pageSize = value;
                    CalculateTotalPages();

                    // 确保当前页在有效范围内
                    if (currentPage > totalPages)
                    {
                        currentPage = Math.Max(1, totalPages);
                    }

                    OnPageChanged();
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// 总记录数
        /// </summary>
        [Browsable(false)]
        public int TotalRecords => totalRecords;

        /// <summary>
        /// 总页数
        /// </summary>
        [Browsable(false)]
        public int TotalPages => totalPages;

        /// <summary>
        /// 绑定的控件
        /// </summary>
        [Category("数据")]
        [Description("绑定的数据显示控件(如DataGridView)")]
        [DefaultValue(null)]
        public object BoundControl
        {
            get => boundControl;
            set
            {
                boundControl = value;
                if (value != null)
                {
                    UpdateBoundControl();
                }
            }
        }

        /// <summary>
        /// 首页按钮文本
        /// </summary>
        [Category("文本")]
        [Description("首页按钮的显示文本")]
        [DefaultValue("首页")]
        public string FirstPageText
        {
            get => firstPageText;
            set
            {
                firstPageText = value;
                if (btnFirst != null)
                {
                    btnFirst.Text = value;
                }
            }
        }

        /// <summary>
        /// 上一页按钮文本
        /// </summary>
        [Category("文本")]
        [Description("上一页按钮的显示文本")]
        [DefaultValue("上一页")]
        public string PreviousPageText
        {
            get => previousPageText;
            set
            {
                previousPageText = value;
                if (btnPrevious != null)
                {
                    btnPrevious.Text = value;
                }
            }
        }

        /// <summary>
        /// 下一页按钮文本
        /// </summary>
        [Category("文本")]
        [Description("下一页按钮的显示文本")]
        [DefaultValue("下一页")]
        public string NextPageText
        {
            get => nextPageText;
            set
            {
                nextPageText = value;
                if (btnNext != null)
                {
                    btnNext.Text = value;
                }
            }
        }

        /// <summary>
        /// 末页按钮文本
        /// </summary>
        [Category("文本")]
        [Description("末页按钮的显示文本")]
        [DefaultValue("末页")]
        public string LastPageText
        {
            get => lastPageText;
            set
            {
                lastPageText = value;
                if (btnLast != null)
                {
                    btnLast.Text = value;
                }
            }
        }

        /// <summary>
        /// 跳转按钮文本
        /// </summary>
        [Category("文本")]
        [Description("跳转按钮的显示文本")]
        [DefaultValue("跳转")]
        public string GoText
        {
            get => goText;
            set
            {
                goText = value;
                if (btnGo != null)
                {
                    btnGo.Text = value;
                }
            }
        }

        /// <summary>
        /// 页面信息格式
        /// </summary>
        [Category("文本")]
        [Description("页面信息的显示格式。{0}=当前页,{1}=总页数,{2}=总记录数")]
        [DefaultValue("第 {0}/{1} 页，共 {2} 条")]
        public string PageInfoFormat
        {
            get => pageInfoFormat;
            set
            {
                pageInfoFormat = value;
                UpdatePageInfo();
            }
        }

        /// <summary>
        /// 是否显示首页/末页按钮
        /// </summary>
        [Category("外观")]
        [Description("是否显示首页和末页按钮")]
        [DefaultValue(true)]
        public bool ShowFirstLastButtons
        {
            get => showFirstLastButtons;
            set
            {
                if (showFirstLastButtons != value)
                {
                    showFirstLastButtons = value;
                    btnFirst.Visible = value;
                    btnLast.Visible = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// 是否显示上一页/下一页按钮
        /// </summary>
        [Category("外观")]
        [Description("是否显示上一页和下一页按钮")]
        [DefaultValue(true)]
        public bool ShowPreviousNextButtons
        {
            get => showPreviousNextButtons;
            set
            {
                if (showPreviousNextButtons != value)
                {
                    showPreviousNextButtons = value;
                    btnPrevious.Visible = value;
                    btnNext.Visible = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// 是否显示页面信息
        /// </summary>
        [Category("外观")]
        [Description("是否显示页面信息标签")]
        [DefaultValue(true)]
        public bool ShowPageInfo
        {
            get => showPageInfo;
            set
            {
                if (showPageInfo != value)
                {
                    showPageInfo = value;
                    lblPageInfo.Visible = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// 是否显示页码跳转
        /// </summary>
        [Category("外观")]
        [Description("是否显示页码输入和跳转按钮")]
        [DefaultValue(true)]
        public bool ShowPageJump
        {
            get => showPageJump;
            set
            {
                if (showPageJump != value)
                {
                    showPageJump = value;
                    txtPageNumber.Visible = value;
                    btnGo.Visible = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// 是否显示页面大小选择器
        /// </summary>
        [Category("外观")]
        [Description("是否显示每页记录数选择器")]
        [DefaultValue(true)]
        public bool ShowPageSizeSelector
        {
            get => showPageSizeSelector;
            set
            {
                if (showPageSizeSelector != value)
                {
                    showPageSizeSelector = value;
                    lblPageSize.Visible = value;
                    cmbPageSize.Visible = value;
                    PerformLayout();
                }
            }
        }

        /// <summary>
        /// 页面大小标签文本
        /// </summary>
        [Category("文本")]
        [Description("页面大小标签的显示文本")]
        [DefaultValue("每页")]
        public string PageSizeText
        {
            get => pageSizeText;
            set
            {
                pageSizeText = value;
                if (lblPageSize != null)
                {
                    lblPageSize.Text = value;
                }
            }
        }

        /// <summary>
        /// 页面大小选项列表
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<int> PageSizeOptions => pageSizeOptions.AsReadOnly();

        #endregion

        #region 事件

        /// <summary>
        /// 页码改变事件
        /// </summary>
        [Category("分页")]
        [Description("页码改变时触发")]
        public event EventHandler<PageChangedEventArgs> PageChanged;

        /// <summary>
        /// 页码改变前事件
        /// </summary>
        [Category("分页")]
        [Description("页码改变前触发，可以取消")]
        public event EventHandler<PageChangingEventArgs> PageChanging;

        protected virtual void OnPageChanged()
        {
            var args = new PageChangedEventArgs(currentPage, totalPages, pageSize);
            PageChanged?.Invoke(this, args);

            UpdateBoundControl();
        }

        protected virtual bool OnPageChanging(int newPage)
        {
            var args = new PageChangingEventArgs(currentPage, newPage, totalPages);
            PageChanging?.Invoke(this, args);
            return !args.Cancel;
        }

        /// <summary>
        /// 页面大小改变事件处理
        /// </summary>
        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            if (cmbPageSize.SelectedItem == null)
            {
                return;
            }

            try
            {
                int newPageSize = Convert.ToInt32(cmbPageSize.SelectedItem);

                if (newPageSize != pageSize)
                {
                    // 计算新页码，尽量保持当前显示的第一条记录位置
                    int firstRecordIndex = (currentPage - 1) * pageSize;

                    // 更新页面大小
                    pageSize = newPageSize;

                    // 重新计算总页数
                    CalculateTotalPages();

                    // 计算新的当前页(保持第一条记录可见)
                    currentPage = (firstRecordIndex / pageSize) + 1;

                    // 确保在有效范围内
                    if (currentPage < 1)
                    {
                        currentPage = 1;
                    }
                    else if (currentPage > totalPages)
                    {
                        currentPage = totalPages;
                    }

                    // 触发页面改变事件
                    OnPageChanged();
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"页面大小设置失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 跳转到首页
        /// </summary>
        public void GoToFirstPage()
        {
            GoToPage(1);
        }

        /// <summary>
        /// 跳转到末页
        /// </summary>
        public void GoToLastPage()
        {
            GoToPage(totalPages);
        }

        /// <summary>
        /// 跳转到上一页
        /// </summary>
        public void GoToPreviousPage()
        {
            GoToPage(currentPage - 1);
        }

        /// <summary>
        /// 跳转到下一页
        /// </summary>
        public void GoToNextPage()
        {
            GoToPage(currentPage + 1);
        }

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        public void GoToPage(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > totalPages)
            {
                MessageBox.Show($"页码必须在 1 到 {totalPages} 之间", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (OnPageChanging(pageNumber))
            {
                CurrentPage = pageNumber;
            }
        }

        /// <summary>
        /// 获取当前页的数据
        /// </summary>
        public IList GetCurrentPageData()
        {
            if (dataSource == null || totalRecords == 0)
            {
                return new ArrayList();
            }

            var startIndex = (currentPage - 1) * pageSize;
            var endIndex = Math.Min(startIndex + pageSize, totalRecords);

            var pageData = new ArrayList();
            for (int i = startIndex; i < endIndex; i++)
            {
                pageData.Add(dataSource[i]);
            }

            return pageData;
        }

        /// <summary>
        /// 获取当前页的数据(泛型版本)
        /// </summary>
        public List<T> GetCurrentPageData<T>()
        {
            if (dataSource == null || totalRecords == 0)
            {
                return new List<T>();
            }

            var startIndex = (currentPage - 1) * pageSize;
            var endIndex = Math.Min(startIndex + pageSize, totalRecords);

            var pageData = new List<T>();
            for (int i = startIndex; i < endIndex; i++)
            {
                if (dataSource[i] is T item)
                {
                    pageData.Add(item);
                }
            }

            return pageData;
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        public override void Refresh()
        {
            CalculateTotalPages();

            // 确保当前页有效
            if (currentPage > totalPages && totalPages > 0)
            {
                currentPage = Math.Max(1, totalPages);
            }
            else if (currentPage < 1)
            {
                currentPage = 1;
            }

            UpdateUI();

            if (dataSource != null && dataSource.Count > 0)
            {
                OnPageChanged();
            }
        }

        /// <summary>
        /// 设置页面大小选项
        /// </summary>
        /// <param name="options">可选的页面大小列表</param>
        public void SetPageSizeOptions(params int[] options)
        {
            if (options == null || options.Length == 0)
            {
                throw new ArgumentException("页面大小选项不能为空", nameof(options));
            }

            // 验证所有选项都是正数
            foreach (var option in options)
            {
                if (option <= 0)
                {
                    throw new ArgumentException("页面大小必须大于0", nameof(options));
                }
            }

            pageSizeOptions.Clear();
            pageSizeOptions.AddRange(options.OrderBy(x => x)); // 按升序排列

            InitializePageSizeOptions();

            // 如果当前页面大小不在新选项中，使用第一个选项
            if (!pageSizeOptions.Contains(pageSize))
            {
                PageSize = pageSizeOptions[0];
            }
        }

        /// <summary>
        /// 添加页面大小选项
        /// </summary>
        public void AddPageSizeOption(int option)
        {
            if (option <= 0)
            {
                throw new ArgumentException("页面大小必须大于0", nameof(option));
            }

            if (!pageSizeOptions.Contains(option))
            {
                pageSizeOptions.Add(option);
                pageSizeOptions.Sort(); // 保持升序
                InitializePageSizeOptions();
            }
        }

        /// <summary>
        /// 移除页面大小选项
        /// </summary>
        public void RemovePageSizeOption(int option)
        {
            if (pageSizeOptions.Count <= 1)
            {
                throw new InvalidOperationException("至少需要保留一个页面大小选项");
            }

            if (pageSizeOptions.Contains(option))
            {
                pageSizeOptions.Remove(option);
                InitializePageSizeOptions();

                // 如果移除的是当前选中的选项，切换到第一个选项
                if (pageSize == option)
                {
                    PageSize = pageSizeOptions[0];
                }
            }
        }

        /// <summary>
        /// 重置页面大小选项为默认值
        /// </summary>
        public void ResetPageSizeOptions()
        {
            SetPageSizeOptions(10, 20, 50, 100);
        }

        #endregion

        #region 私有方法

        private void CalculateTotalPages()
        {
            totalRecords = dataSource?.Count ?? 0;
            totalPages = totalRecords > 0
                ? (int)Math.Ceiling((double)totalRecords / pageSize)
                : 0;

            if (totalPages == 0)
            {
                totalPages = 1; // 至少显示1页
            }
        }

        private void UpdateUI()
        {
            // 更新按钮状态
            btnFirst.Enabled = currentPage > 1;
            btnPrevious.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < totalPages;
            btnLast.Enabled = currentPage < totalPages;
            btnGo.Enabled = totalPages > 0;
            txtPageNumber.Enabled = totalPages > 0;

            // 更新页面大小选择器状态
            UpdatePageSizeSelectorState();

            // 更新页码输入框
            txtPageNumber.Text = currentPage.ToString();

            // 更新页面信息
            UpdatePageInfo();

            // 重新布局
            PerformLayout();
        }

        private void UpdatePageInfo()
        {
            if (lblPageInfo != null && showPageInfo)
            {
                try
                {
                    lblPageInfo.Text = string.Format(pageInfoFormat,
                        currentPage, totalPages, totalRecords);
                }
                catch
                {
                    lblPageInfo.Text = $"第 {currentPage}/{totalPages} 页，共 {totalRecords} 条";
                }
            }
        }

        private void UpdateBoundControl()
        {
            if (boundControl == null)
            {
                return;
            }

            try
            {
                // 支持 DataGridView
                if (boundControl is DataGridView grid)
                {
                    grid.DataSource = GetCurrentPageData();
                }
                // 支持 ListBox
                else if (boundControl is ListBox listBox)
                {
                    listBox.DataSource = GetCurrentPageData();
                }
                // 支持 ComboBox
                else if (boundControl is ComboBox comboBox)
                {
                    comboBox.DataSource = GetCurrentPageData();
                }
                // 可以扩展支持其他控件
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新绑定控件失败: {ex.Message}");
            }
        }

        private void GoToInputPage()
        {
            if (string.IsNullOrWhiteSpace(txtPageNumber.Text))
            {
                MessageBox.Show("请输入页码", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtPageNumber.Text, out int pageNumber))
            {
                MessageBox.Show("页码格式不正确", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPageNumber.Text = currentPage.ToString();
                return;
            }

            GoToPage(pageNumber);
        }

        /// <summary>
        /// 更新页面大小选择器的启用状态
        /// </summary>
        private void UpdatePageSizeSelectorState()
        {
            bool hasData = dataSource != null && dataSource.Count > 0;

            if (cmbPageSize != null)
            {
                cmbPageSize.Enabled = hasData;
            }

            if (lblPageSize != null)
            {
                lblPageSize.Enabled = hasData;
            }
        }

        #endregion

        #region 事件处理

        private void OnPageNumberKeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入数字和控制键
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void OnPageNumberKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GoToInputPage();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        #endregion

        #region 布局

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            LayoutControls();
        }

        private void LayoutControls()
        {
            int x = spacing;
            int y = (Height - buttonHeight) / 2;

            // 首页按钮
            if (showFirstLastButtons && btnFirst.Visible)
            {
                btnFirst.Location = new Point(x, y);
                x += btnFirst.Width + spacing;
            }

            // 上一页按钮
            if (showPreviousNextButtons && btnPrevious.Visible)
            {
                btnPrevious.Location = new Point(x, y);
                x += btnPrevious.Width + spacing;
            }

            // 页码输入和跳转
            if (showPageJump)
            {
                txtPageNumber.Location = new Point(x, y + (buttonHeight - txtPageNumber.Height) / 2);
                x += txtPageNumber.Width + spacing / 2;

                btnGo.Location = new Point(x, y);
                x += btnGo.Width + spacing;
            }

            // 下一页按钮
            if (showPreviousNextButtons && btnNext.Visible)
            {
                btnNext.Location = new Point(x, y);
                x += btnNext.Width + spacing;
            }

            // 末页按钮
            if (showFirstLastButtons && btnLast.Visible)
            {
                btnLast.Location = new Point(x, y);
                x += btnLast.Width + spacing * 2;
            }

            // 页面大小选择器
            if (showPageSizeSelector && cmbPageSize.Visible)
            {
                lblPageSize.Location = new Point(x,
                    y + (buttonHeight - lblPageSize.Height) / 2);
                x += lblPageSize.Width + spacing / 2;

                cmbPageSize.Location = new Point(x, y);
                x += cmbPageSize.Width + spacing * 2;
            }

            // 页面信息(最后显示)
            if (showPageInfo && lblPageInfo.Visible)
            {
                lblPageInfo.Location = new Point(x,
                    y + (buttonHeight - lblPageInfo.Height) / 2);
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            // 使用主题背景色或透明
            var bgColor = UseTheme && Theme != null
                ? GetThemeColor(c => c.Surface, BackColor)
                : BackColor;

            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            // 内容由子控件绘制
        }

        protected override void DrawBorder(Graphics g)
        {
            // 分页控件通常不需要边框
        }

        #endregion

        #region 主题支持

        protected override void OnThemeChanged()
        {
            base.OnThemeChanged();
            ApplyThemeToChildControls();
        }

        protected override void ApplyThemeStyles()
        {
            if (!UseTheme || Theme == null)
            {
                return;
            }

            BackColor = GetThemeColor(c => c.Surface, BackColor);
            ForeColor = GetThemeColor(c => c.TextPrimary, ForeColor);

            // 应用主题到子控件
            ApplyThemeToChildControls();
        }

        private void ApplyThemeToChildControls()
        {
            if (!UseTheme || Theme == null)
            {
                return;
            }

            // 应用主题到按钮
            foreach (var btn in new[] { btnFirst, btnPrevious, btnNext, btnLast, btnGo })
            {
                if (btn != null)
                {
                    btn.InheritThemeFrom(this);
                }
            }

            // 应用主题到下拉框
            if (cmbPageSize != null)
            {
                cmbPageSize.InheritThemeFrom(this);
            }

            // 应用主题到标签
            foreach (var lbl in new[] { lblPageInfo, lblPageSize })
            {
                if (lbl != null)
                {
                    lbl.ForeColor = GetThemeColor(c => c.TextPrimary, ForeColor);
                    lbl.Font = GetThemeFont(t => t.Body, Font);
                }
            }

            // 应用主题到输入框
            if (txtPageNumber != null)
            {
                txtPageNumber.BackColor = GetThemeColor(c => c.Surface, Color.White);
                txtPageNumber.ForeColor = GetThemeColor(c => c.TextPrimary, Color.Black);
                txtPageNumber.Font = GetThemeFont(t => t.Body, Font);
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 清理控件
                btnFirst?.Dispose();
                btnPrevious?.Dispose();
                btnNext?.Dispose();
                btnLast?.Dispose();
                btnGo?.Dispose();
                txtPageNumber?.Dispose();
                lblPageInfo?.Dispose();
                cmbPageSize?.Dispose();
                lblPageSize?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }


    #region 枚举及辅助类

    /// <summary>
    /// 分页导航按钮类型
    /// </summary>
    public enum NavigationButtonType
    {
        First,
        Previous,
        Next,
        Last
    }

    /// <summary>
    /// 页码改变事件参数
    /// </summary>
    public class PageChangedEventArgs : EventArgs
    {
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }

        public PageChangedEventArgs(int currentPage, int totalPages, int pageSize)
        {
            CurrentPage = currentPage;
            TotalPages = totalPages;
            PageSize = pageSize;
        }
    }

    /// <summary>
    /// 页码改变前事件参数
    /// </summary>
    public class PageChangingEventArgs : EventArgs
    {
        public int OldPage { get; }
        public int NewPage { get; }
        public int TotalPages { get; }
        public bool Cancel { get; set; }

        public PageChangingEventArgs(int oldPage, int newPage, int totalPages)
        {
            OldPage = oldPage;
            NewPage = newPage;
            TotalPages = totalPages;
            Cancel = false;
        }
    }

    #endregion
}
