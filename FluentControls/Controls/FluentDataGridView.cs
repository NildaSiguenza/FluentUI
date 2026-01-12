using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using FluentControls.Themes;

namespace FluentControls.Controls
{
    [Designer(typeof(FluentDataGridViewDesigner))]
    [DefaultEvent("CellClick")]
    public class FluentDataGridView : FluentContainerBase
    {
        #region 字段

        private DataGridViewColumnCollection columns;
        private DataGridViewRowCollection rows;
        private VScrollBar vScrollBar;
        private HScrollBar hScrollBar;

        // 布局相关
        private int rowHeaderWidth = 42;
        private int columnHeaderHeight = 36;
        private int firstVisibleRowIndex = 0;
        private int firstVisibleColumnIndex = 0;

        // 交替色
        private bool alternatingRowColors = true;
        private Color alternatingRowColor = Color.FromArgb(245, 245, 245);

        // 选择相关
        private DataGridViewCellAddress currentCell = new DataGridViewCellAddress(-1, -1);
        private List<FluentDataGridViewRow> selectedRows = new List<FluentDataGridViewRow>();
        private List<DataGridViewCellAddress> selectedCells = new List<DataGridViewCellAddress>();

        // 编辑相关
        private Control currentEditingControl;
        private DataGridViewCellAddress editingCellAddress = new DataGridViewCellAddress(-1, -1);

        // 拖拽调整列宽
        private bool isResizingColumn = false;
        private int resizingColumnIndex = -1;
        private int resizeStartX = 0;
        private int resizeStartWidth = 0;

        // 排序相关
        private FluentDataGridViewColumn sortedColumn;
        private SortOrder currentSortOrder = SortOrder.None;

        // 悬停相关
        private DataGridViewCellAddress hoveredCell = new DataGridViewCellAddress(-1, -1);

        // 数据绑定
        private object dataSource;

        // 记录多选
        private DataGridViewCellAddress rangeSelectionAnchor = new DataGridViewCellAddress(-1, -1); // 记录范围选择的起点

        // 标记管理
        private Dictionary<object, RowMark> rowMarksByDataItem = new Dictionary<object, RowMark>(); // 按数据项
        private List<RowStyleFilter> styleFilters = new List<RowStyleFilter>();

        // 分页相关
        private bool showRowNumbers = true; // 是否显示行号
        private int pageSize = 0; // 0表示不分页
        private int currentPage = 1;
        private int startRowNumber = 1; // 起始行号
        private FluentPagination pagination;
        private bool enablePagination = false;
        private PaginationPosition paginationPosition = PaginationPosition.Bottom;
        private int paginationHeight = 45;
        private List<object> fullDataList; // 完整数据列表

        // 右键菜单
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem menuItemCopy;
        private ToolStripMenuItem menuItemPaste;
        private ToolStripMenuItem menuItemExportCsv;
        private ToolStripSeparator menuSeparator;

        // 事件
        public event EventHandler<DataGridViewCellEventArgs> CellClick;
        public event EventHandler<DataGridViewCellEventArgs> CellDoubleClick;
        public event EventHandler<DataGridViewCellValueEventArgs> CellValueChanged;
        public event EventHandler<DataGridViewSortEventArgs> ColumnHeaderClick;
        public event EventHandler<DataGridViewSortEventArgs> Sorting;
        public event EventHandler SelectionChanged;
        public event EventHandler DataSourceChanged;
        public event EventHandler PageChanged;

        #endregion

        #region 构造函数

        public FluentDataGridView()
        {
            columns = new DataGridViewColumnCollection(this);
            rows = new DataGridViewRowCollection(this);

            InitializeScrollBars();

            // 初始化右键菜单
            InitializeContextMenu();

            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }

        private void InitializeScrollBars()
        {
            // 垂直滚动条
            vScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false
            };
            vScrollBar.Scroll += VScrollBar_Scroll;

            // 水平滚动条
            hScrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                Visible = false
            };
            hScrollBar.Scroll += HScrollBar_Scroll;

            Controls.Add(vScrollBar);
            Controls.Add(hScrollBar);
        }

        #endregion

        #region 属性

        /// <summary>
        /// 列集合
        /// </summary>
        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentDataGridViewColumnCollectionEditor), typeof(UITypeEditor))]
        [Description("列集合")]
        public DataGridViewColumnCollection Columns => columns;

        /// <summary>
        /// 行集合
        /// </summary>
        [Category("Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataGridViewRowCollection Rows => rows;

        /// <summary>
        /// 数据源
        /// </summary>
        [Category("Data")]
        [DefaultValue(null)]
        public object DataSource
        {
            get => dataSource;
            set
            {
                if (dataSource != value)
                {
                    dataSource = value;
                    BindData();
                }
            }
        }

        /// <summary>
        /// 是否显示行头
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowRowHeader { get; set; } = true;

        /// <summary>
        /// 行头宽度
        /// </summary>
        [Category("Layout")]
        [DefaultValue(42)]
        public int RowHeaderWidth
        {
            get => rowHeaderWidth;
            set
            {
                if (rowHeaderWidth != value && value >= 20)
                {
                    rowHeaderWidth = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 列头高度
        /// </summary>
        [Category("Layout")]
        [DefaultValue(36)]
        public int ColumnHeaderHeight
        {
            get => columnHeaderHeight;
            set
            {
                if (columnHeaderHeight != value && value >= 20)
                {
                    columnHeaderHeight = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 默认行高
        /// </summary>
        [Category("Layout")]
        [DefaultValue(32)]
        public int DefaultRowHeight { get; set; } = 32;

        /// <summary>
        /// 是否启用交替色
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool AlternatingRowColors
        {

            get => alternatingRowColors;
            set
            {
                if (alternatingRowColors != value)
                {
                    alternatingRowColors = value;
                    RefreshAlternatingColors();
                }
            }
        }

        /// <summary>
        /// 交替行颜色
        /// </summary>
        [Category("Appearance")]
        public Color AlternatingRowColor
        {
            get => alternatingRowColor;
            set
            {
                if (alternatingRowColor != value)
                {
                    alternatingRowColor = value;
                    RefreshAlternatingColors();
                }
            }

        }

        /// <summary>
        /// 选择模式
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(DataGridViewSelectionMode.FullRowSelect)]
        public DataGridViewSelectionMode SelectionMode { get; set; } = DataGridViewSelectionMode.FullRowSelect;

        /// <summary>
        /// 编辑模式
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(DataGridViewEditMode.EditOnDoubleClick)]
        public DataGridViewEditMode EditMode { get; set; } = DataGridViewEditMode.EditOnDoubleClick;

        /// <summary>
        /// 是否只读
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// 是否允许用户添加行
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool AllowUserToAddRows { get; set; } = false;

        /// <summary>
        /// 是否允许用户删除行
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool AllowUserToDeleteRows { get; set; } = false;

        /// <summary>
        /// 是否允许用户调整列宽
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool AllowUserToResizeColumns { get; set; } = true;

        /// <summary>
        /// 网格线颜色
        /// </summary>
        [Category("Appearance")]
        public Color GridColor { get; set; } = Color.FromArgb(229, 229, 229);

        /// <summary>
        /// 是否显示网格线
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowGridLines { get; set; } = true;

        /// <summary>
        /// 是否在行头显示行号
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowRowNumbers
        {
            get => showRowNumbers;
            set
            {
                if (showRowNumbers != value)
                {
                    showRowNumbers = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 起始行号(用于跨页显示连续行号)
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(1)]
        public int StartRowNumber
        {
            get => startRowNumber;
            set
            {
                if (startRowNumber != value && value >= 0)
                {
                    startRowNumber = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 当前单元格
        /// </summary>
        [Browsable(false)]
        public DataGridViewCellAddress CurrentCell => currentCell;

        /// <summary>
        /// 选中的行
        /// </summary>
        [Browsable(false)]
        public IReadOnlyList<FluentDataGridViewRow> SelectedRows => selectedRows.AsReadOnly();

        /// <summary>
        /// 是否启用分页
        /// </summary>
        [Category("Pagination")]
        [DefaultValue(false)]
        [Description("启用或禁用分页功能")]
        public bool EnablePagination
        {
            get => enablePagination;
            set
            {
                if (enablePagination != value)
                {
                    enablePagination = value;

                    if (value)
                    {
                        CreatePagination();
                    }
                    else
                    {
                        RemovePagination();
                    }

                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 分页位置
        /// </summary>
        [Category("Pagination")]
        [DefaultValue(PaginationPosition.Bottom)]
        [Description("分页控件的显示位置")]
        public PaginationPosition PaginationPosition
        {
            get => paginationPosition;
            set
            {
                if (paginationPosition != value)
                {
                    paginationPosition = value;
                    if (enablePagination && pagination != null)
                    {
                        PerformLayout();
                    }
                }
            }
        }

        /// <summary>
        /// 分页控件高度
        /// </summary>
        [Category("Pagination")]
        [DefaultValue(45)]
        [Description("分页控件的高度")]
        public int PaginationHeight
        {
            get => paginationHeight;
            set
            {
                if (paginationHeight != value && value > 0)
                {
                    paginationHeight = value;
                    if (enablePagination && pagination != null)
                    {
                        pagination.Height = value;
                        PerformLayout();
                    }
                }
            }
        }

        /// <summary>
        /// 获取分页控件(只读)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FluentPagination Pagination => pagination;

        /// <summary>
        /// 每页大小(覆盖原有属性)
        /// </summary>
        [Category("Pagination")]
        [DefaultValue(0)]
        [Description("每页显示的行数(0表示不分页)")]
        public int PageSize
        {
            get => enablePagination && pagination != null ? pagination.PageSize : pageSize;
            set
            {
                if (enablePagination && pagination != null)
                {
                    pagination.PageSize = value;
                }
                else
                {
                    pageSize = value;
                }
            }
        }

        /// <summary>
        /// 是否启用默认右键菜单
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否启用默认右键菜单")]
        public bool EnableDefaultContextMenu { get; set; } = true;

        /// <summary>
        /// 是否显示复制菜单项
        /// </summary>
        [Category("Context Menu")]
        [DefaultValue(true)]
        [Description("是否在右键菜单中显示复制选项")]
        public bool ShowCopyMenuItem { get; set; } = true;

        /// <summary>
        /// 是否显示粘贴菜单项
        /// </summary>
        [Category("Context Menu")]
        [DefaultValue(true)]
        [Description("是否在右键菜单中显示粘贴选项")]
        public bool ShowPasteMenuItem { get; set; } = true;

        /// <summary>
        /// 是否显示导出CSV菜单项
        /// </summary>
        [Category("Context Menu")]
        [DefaultValue(true)]
        [Description("是否在右键菜单中显示导出CSV选项")]
        public bool ShowExportCsvMenuItem { get; set; } = true;

        /// <summary>
        /// 获取右键菜单(可用于添加自定义菜单项)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ContextMenuStrip GridContextMenu
        {
            get
            {
                if (contextMenu == null)
                {
                    InitializeContextMenu();
                }
                return contextMenu;
            }
        }

        /// <summary>
        /// 当前页(只读)
        /// </summary>
        [Browsable(false)]
        public int CurrentPage => enablePagination && pagination != null ? pagination.CurrentPage : currentPage;

        /// <summary>
        /// 总页数(只读)
        /// </summary>
        [Browsable(false)]
        public int TotalPages => enablePagination && pagination != null ? pagination.TotalPages : 1;

        /// <summary>
        /// 获取选中的单元格集合
        /// </summary>
        [Browsable(false)]
        public IReadOnlyList<DataGridViewCellAddress> SelectedCells => selectedCells.AsReadOnly();

        #endregion

        #region 数据绑定

        private void BindData()
        {
            rows.Clear();
            fullDataList = null;

            if (dataSource == null)
            {
                if (enablePagination && pagination != null)
                {
                    pagination.DataSource = null;
                }
                OnDataSourceChanged(EventArgs.Empty);
                return;
            }

            // 转换数据源为列表
            if (dataSource is IEnumerable enumerable)
            {
                fullDataList = enumerable.Cast<object>().ToList();
            }
            else
            {
                fullDataList = new List<object> { dataSource };
            }

            if (enablePagination && pagination != null)
            {
                // 启用分页时，设置分页控件的数据源
                pagination.DataSource = fullDataList;
                pagination.Refresh();
                LoadPageData();
            }
            else
            {
                // 不启用分页时，加载所有数据
                BindDataInternal(fullDataList, false);
            }

            OnDataSourceChanged(EventArgs.Empty);
            PerformLayout();
            Invalidate();
        }

        private void BindDataInternal(IList data, bool isPaginated)
        {
            rows.Clear();

            if (data == null)
            {
                return;
            }

            int rowNumberStart = isPaginated && pagination != null
                ? ((pagination.CurrentPage - 1) * pagination.PageSize + 1)
                : this.startRowNumber;

            int rowIndex = 0;
            foreach (var item in data)
            {
                var row = new FluentDataGridViewRow
                {
                    DataBoundItem = item,
                    Height = DefaultRowHeight,
                    Index = rowIndex  // 设置索引
                };
                rows.Add(row);
                BindRowData(row, item);
                rowIndex++;
            }

            // 更新起始行号
            if (isPaginated)
            {
                this.startRowNumber = rowNumberStart;
            }
        }

        private void LoadAllData()
        {
            if (fullDataList == null)
            {
                return;
            }

            foreach (var item in fullDataList)
            {
                var row = new FluentDataGridViewRow
                {
                    DataBoundItem = item,
                    Height = DefaultRowHeight
                };
                rows.Add(row);
                BindRowData(row, item);
            }
        }

        private void UpdatePageData()
        {
            rows.Clear();

            if (fullDataList == null || pageSize <= 0)
            {
                return;
            }

            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, fullDataList.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var item = fullDataList[i];
                var row = new FluentDataGridViewRow
                {
                    DataBoundItem = item,
                    Height = DefaultRowHeight
                };
                rows.Add(row);
                BindRowData(row, item);
            }

            OnPageChanged(EventArgs.Empty);
            PerformLayout();
            Invalidate();
        }

        private void BindRowData(FluentDataGridViewRow row, object dataItem)
        {
            // 先创建单元格并绑定数据
            foreach (var cell in row.Cells)
            {
                var column = cell.OwningColumn;
                if (column != null)
                {
                    cell.Value = column.GetCellValue(dataItem);
                }
            }

            // 恢复行标记(只使用数据项字典)
            if (dataItem != null && rowMarksByDataItem.ContainsKey(dataItem))
            {
                row.Mark = rowMarksByDataItem[dataItem];
            }
            else
            {
                row.Mark = null; // 确保清除之前可能存在的标记
            }

            // 应用交替色
            if (AlternatingRowColors && row.Index % 2 == 1)
            {
                if (row.DefaultCellStyle == null)
                {
                    row.DefaultCellStyle = new DataGridViewCellStyle();
                }
                row.DefaultCellStyle.BackColor = AlternatingRowColor;

                foreach (var cell in row.Cells)
                {
                    if (cell.Style == null)
                    {
                        cell.Style = new DataGridViewCellStyle();
                    }
                    cell.Style.BackColor = AlternatingRowColor;
                }
            }

            // 应用样式过滤器
            ApplyStyleFiltersToRow(row);
        }


        /// <summary>
        /// 获取当前行的全局索引
        /// </summary>
        private int GetGlobalRowIndex(int localRowIndex)
        {
            if (enablePagination && pagination != null)
            {
                return (pagination.CurrentPage - 1) * pagination.PageSize + localRowIndex;
            }
            return localRowIndex;
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        public void RefreshData()
        {
            BindData();
        }

        #endregion

        #region 右键菜单

        /// <summary>
        /// 初始化右键菜单
        /// </summary>
        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            // 复制
            menuItemCopy = new ToolStripMenuItem
            {
                Text = "复制",
                ShortcutKeys = Keys.Control | Keys.C,
                Image = null // 可以添加图标
            };
            menuItemCopy.Click += MenuItemCopy_Click;

            // 粘贴
            menuItemPaste = new ToolStripMenuItem
            {
                Text = "粘贴",
                ShortcutKeys = Keys.Control | Keys.V,
                Image = null // 可以添加图标
            };
            menuItemPaste.Click += MenuItemPaste_Click;

            // 分隔线
            menuSeparator = new ToolStripSeparator();

            // 导出CSV
            menuItemExportCsv = new ToolStripMenuItem
            {
                Text = "导出为 CSV...",
                Image = null // 可以添加图标
            };
            menuItemExportCsv.Click += MenuItemExportCsv_Click;

            // 添加到菜单
            UpdateContextMenuItems();

            // 菜单打开前更新项的可用性
            contextMenu.Opening += ContextMenu_Opening;

            this.ContextMenuStrip = contextMenu;
        }

        /// <summary>
        /// 更新右键菜单项的显示
        /// </summary>
        private void UpdateContextMenuItems()
        {
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Items.Clear();

            if (ShowCopyMenuItem && menuItemCopy != null)
            {
                contextMenu.Items.Add(menuItemCopy);
            }

            if (ShowPasteMenuItem && menuItemPaste != null)
            {
                contextMenu.Items.Add(menuItemPaste);
            }

            // 如果有复制或粘贴项，并且要显示导出CSV，添加分隔线
            if ((ShowCopyMenuItem || ShowPasteMenuItem) && ShowExportCsvMenuItem)
            {
                contextMenu.Items.Add(menuSeparator);
            }

            if (ShowExportCsvMenuItem && menuItemExportCsv != null)
            {
                contextMenu.Items.Add(menuItemExportCsv);
            }
        }

        /// <summary>
        /// 菜单打开时更新项的可用性
        /// </summary>
        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!EnableDefaultContextMenu)
            {
                e.Cancel = true;
                return;
            }

            // 更新复制项
            if (menuItemCopy != null)
            {
                menuItemCopy.Enabled = selectedCells.Count > 0 || selectedRows.Count > 0;
            }

            // 更新粘贴项
            if (menuItemPaste != null)
            {
                menuItemPaste.Enabled = !ReadOnly && Clipboard.ContainsText();
            }

            // 更新导出CSV项
            if (menuItemExportCsv != null)
            {
                menuItemExportCsv.Enabled = rows.Count > 0;
            }
        }

        /// <summary>
        /// 复制选中的单元格
        /// </summary>
        private void MenuItemCopy_Click(object sender, EventArgs e)
        {
            CopySelectedCells();
        }

        /// <summary>
        /// 粘贴到选中的单元格
        /// </summary>
        private void MenuItemPaste_Click(object sender, EventArgs e)
        {
            PasteToSelectedCells();
        }

        /// <summary>
        /// 导出为CSV
        /// </summary>
        private void MenuItemExportCsv_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        /// <summary>
        /// 复制选中的单元格到剪贴板
        /// </summary>
        public void CopySelectedCells()
        {
            if (selectedCells.Count == 0 && selectedRows.Count == 0)
            {
                return;
            }

            try
            {
                var sb = new StringBuilder();

                if (SelectionMode == DataGridViewSelectionMode.FullRowSelect && selectedRows.Count > 0)
                {
                    // 复制整行
                    var sortedRows = selectedRows.OrderBy(r => r.Index).ToList();

                    foreach (var row in sortedRows)
                    {
                        var values = new List<string>();
                        for (int c = 0; c < columns.Count; c++)
                        {
                            if (columns[c].Visible)
                            {
                                var cell = row.Cells[c];
                                values.Add(cell.FormattedValue ?? string.Empty);
                            }
                        }
                        sb.AppendLine(string.Join("\t", values));
                    }
                }
                else if (selectedCells.Count > 0)
                {
                    // 复制选中的单元格(按行列排序)
                    var sortedCells = selectedCells
                        .OrderBy(c => c.RowIndex)
                        .ThenBy(c => c.ColumnIndex)
                        .ToList();

                    int lastRow = -1;
                    var rowValues = new List<string>();

                    foreach (var cellAddr in sortedCells)
                    {
                        if (cellAddr.RowIndex != lastRow && lastRow >= 0)
                        {
                            sb.AppendLine(string.Join("\t", rowValues));
                            rowValues.Clear();
                        }

                        var cell = rows[cellAddr.RowIndex].Cells[cellAddr.ColumnIndex];
                        rowValues.Add(cell.FormattedValue ?? string.Empty);
                        lastRow = cellAddr.RowIndex;
                    }

                    if (rowValues.Count > 0)
                    {
                        sb.AppendLine(string.Join("\t", rowValues));
                    }
                }

                if (sb.Length > 0)
                {
                    Clipboard.SetText(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 从剪贴板粘贴到选中的单元格
        /// </summary>
        public void PasteToSelectedCells()
        {
            if (ReadOnly || !Clipboard.ContainsText())
            {
                return;
            }

            if (currentCell.ColumnIndex < 0 || currentCell.RowIndex < 0)
            {
                return;
            }

            try
            {
                string clipboardText = Clipboard.GetText();
                var lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                int startRow = currentCell.RowIndex;
                int startCol = currentCell.ColumnIndex;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrEmpty(lines[i]))
                    {
                        continue;
                    }

                    int targetRow = startRow + i;
                    if (targetRow >= rows.Count)
                    {
                        break;
                    }

                    var values = lines[i].Split('\t');

                    for (int j = 0; j < values.Length; j++)
                    {
                        int targetCol = startCol + j;
                        if (targetCol >= columns.Count)
                        {
                            break;
                        }

                        var column = columns[targetCol];
                        var cell = rows[targetRow].Cells[targetCol];

                        if (!column.ReadOnly && !cell.ReadOnly)
                        {
                            cell.Value = values[j];

                            // 更新数据源
                            if (rows[targetRow].DataBoundItem != null)
                            {
                                column.SetCellValue(rows[targetRow].DataBoundItem, values[j]);
                            }
                        }
                    }
                }

                Invalidate();
                OnCellValueChanged(new DataGridViewCellValueEventArgs(
                    currentCell.ColumnIndex, currentCell.RowIndex, null, null));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"粘贴失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 导出为CSV文件
        /// </summary>
        public void ExportToCsv()
        {
            if (rows.Count == 0)
            {
                MessageBox.Show("没有数据可导出", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                    dialog.DefaultExt = "csv";
                    dialog.FileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportToCsv(dialog.FileName);
                        MessageBox.Show("导出成功！", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 导出为CSV文件(指定文件名)
        /// </summary>
        /// <param name="fileName">文件路径</param>
        public void ExportToCsv(string fileName)
        {
            using (var writer = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
            {
                // 写入列头
                var headers = new List<string>();
                foreach (var column in columns)
                {
                    if (column.Visible)
                    {
                        var header = !string.IsNullOrEmpty(column.HeaderText)
                            ? column.HeaderText
                            : column.Name;
                        headers.Add(EscapeCsvValue(header));
                    }
                }
                writer.WriteLine(string.Join(",", headers));

                // 写入数据行(使用完整数据列表)
                var dataList = fullDataList ?? rows.Select(r => r.DataBoundItem).ToList();

                foreach (var dataItem in dataList)
                {
                    if (dataItem == null)
                    {
                        continue;
                    }

                    var values = new List<string>();
                    foreach (var column in columns)
                    {
                        if (column.Visible)
                        {
                            var value = column.GetCellValue(dataItem);
                            values.Add(EscapeCsvValue(value?.ToString() ?? string.Empty));
                        }
                    }
                    writer.WriteLine(string.Join(",", values));
                }
            }
        }

        /// <summary>
        /// 转义CSV值
        /// </summary>
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            // 如果包含逗号、引号或换行符，需要用引号包裹
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                // 将引号替换为两个引号
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }

        /// <summary>
        /// 添加自定义右键菜单项
        /// </summary>
        /// <param name="menuItem">菜单项</param>
        /// <param name="insertIndex">插入位置，-1表示添加到末尾</param>
        public void AddContextMenuItem(ToolStripItem menuItem, int insertIndex = -1)
        {
            if (contextMenu == null)
            {
                InitializeContextMenu();
            }

            if (insertIndex < 0 || insertIndex > contextMenu.Items.Count)
            {
                contextMenu.Items.Add(menuItem);
            }
            else
            {
                contextMenu.Items.Insert(insertIndex, menuItem);
            }
        }

        /// <summary>
        /// 移除右键菜单项
        /// </summary>
        public void RemoveContextMenuItem(ToolStripItem menuItem)
        {
            if (contextMenu != null && menuItem != null)
            {
                contextMenu.Items.Remove(menuItem);
            }
        }

        /// <summary>
        /// 移除默认菜单项
        /// </summary>
        public void RemoveDefaultMenuItem(string menuItemName)
        {
            if (contextMenu == null)
            {
                return;
            }

            switch (menuItemName.ToLower())
            {
                case "copy":
                    ShowCopyMenuItem = false;
                    break;
                case "paste":
                    ShowPasteMenuItem = false;
                    break;
                case "exportcsv":
                    ShowExportCsvMenuItem = false;
                    break;
            }

            UpdateContextMenuItems();
        }


        #endregion

        //#region 数据筛选

        ///// <summary>
        ///// 显示列筛选
        ///// </summary>
        //public void ShowColumnFilter(FluentDataGridViewColumn column)
        //{
        //    if (column == null || !enableFiltering)
        //    {
        //        return;
        //    }

        //    // 检查列类型，决定使用文本筛选还是数值筛选
        //    bool isNumeric = IsNumericColumn(column);

        //    if (isNumeric)
        //    {
        //        ShowNumericFilter(column);
        //    }
        //    else
        //    {
        //        ShowTextFilter(column);
        //    }
        //}

        //private bool IsNumericColumn(FluentDataGridViewColumn column)
        //{
        //    // 判断列是否为数值类型
        //    if (column is FluentDataGridViewProgressColumn)
        //    {
        //        return true;
        //    }

        //    // 检查数据类型
        //    if (fullDataList != null && fullDataList.Count > 0 && !string.IsNullOrEmpty(column.DataPropertyName))
        //    {
        //        try
        //        {
        //            var firstItem = fullDataList[0];
        //            var property = firstItem.GetType().GetProperty(column.DataPropertyName);
        //            if (property != null)
        //            {
        //                var type = property.PropertyType;
        //                return type == typeof(int) || type == typeof(long) ||
        //                       type == typeof(float) || type == typeof(double) ||
        //                       type == typeof(decimal) || type == typeof(short) ||
        //                       type == typeof(byte);
        //            }
        //        }
        //        catch { }
        //    }

        //    return false;
        //}

        //private void ShowTextFilter(FluentDataGridViewColumn column)
        //{
        //    // 创建或获取筛选文本框
        //    if (!filterTextBoxes.ContainsKey(column))
        //    {
        //        var textBox = new ColumnFilterTextBox
        //        {
        //            Column = column
        //        };
        //        textBox.KeyDown += FilterTextBox_KeyDown;
        //        textBox.Leave += FilterTextBox_Leave;
        //        filterTextBoxes[column] = textBox;
        //        Controls.Add(textBox);
        //    }

        //    var filterBox = filterTextBoxes[column];

        //    // 计算位置
        //    var columnBounds = GetColumnHeaderBounds(column);
        //    if (columnBounds != Rectangle.Empty)
        //    {
        //        filterBox.Location = new Point(columnBounds.X + 2, columnBounds.Bottom + 2);
        //        filterBox.Width = columnBounds.Width - 4;
        //        filterBox.Visible = true;
        //        filterBox.BringToFront();
        //        filterBox.Focus();
        //    }
        //}

        //private void ShowNumericFilter(FluentDataGridViewColumn column)
        //{
        //    using (var dialog = new NumericFilterDialog(column.HeaderText ?? column.Name))
        //    {
        //        if (dialog.ShowDialog(this) == DialogResult.OK)
        //        {
        //            var filter = new ColumnFilter
        //            {
        //                Column = column,
        //                Operator = dialog.SelectedOperator,
        //                Value = dialog.Value1,
        //                Value2 = dialog.Value2,
        //                IsNumeric = true
        //            };

        //            ApplyFilter(filter);
        //        }
        //    }
        //}

        //private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        var textBox = sender as ColumnFilterTextBox;
        //        if (textBox != null)
        //        {
        //            ApplyTextFilter(textBox);
        //            e.Handled = true;
        //            e.SuppressKeyPress = true;
        //        }
        //    }
        //    else if (e.KeyCode == Keys.Escape)
        //    {
        //        var textBox = sender as ColumnFilterTextBox;
        //        if (textBox != null)
        //        {
        //            textBox.Visible = false;
        //            Focus();
        //        }
        //    }
        //}

        //private void FilterTextBox_Leave(object sender, EventArgs e)
        //{
        //    var textBox = sender as ColumnFilterTextBox;
        //    if (textBox != null)
        //    {
        //        ApplyTextFilter(textBox);
        //        textBox.Visible = false;
        //    }
        //}

        //private void ApplyTextFilter(ColumnFilterTextBox textBox)
        //{
        //    if (string.IsNullOrWhiteSpace(textBox.Text))
        //    {
        //        // 清除该列的筛选
        //        filterManager.RemoveFilter(textBox.Column);
        //    }
        //    else
        //    {
        //        // 应用筛选
        //        var filter = new ColumnFilter
        //        {
        //            Column = textBox.Column,
        //            Operator = FilterOperator.Contains,
        //            Value = textBox.Text.Trim(),
        //            IsNumeric = false
        //        };

        //        filterManager.AddFilter(filter);
        //    }

        //    ApplyAllFilters();
        //}

        //private void ApplyFilter(ColumnFilter filter)
        //{
        //    filterManager.AddFilter(filter);
        //    ApplyAllFilters();
        //}

        //private void ApplyAllFilters()
        //{
        //    if (fullDataList == null)
        //    {
        //        return;
        //    }

        //    List<object> filteredData;

        //    if (filterManager.HasFilters)
        //    {
        //        // 应用筛选
        //        filteredData = new List<object>();

        //        foreach (var dataItem in fullDataList)
        //        {
        //            // 创建临时行用于筛选
        //            var tempRow = new FluentDataGridViewRow
        //            {
        //                DataBoundItem = dataItem
        //            };
        //            tempRow.CreateCells(this);

        //            // 绑定数据到临时行
        //            foreach (var cell in tempRow.Cells)
        //            {
        //                var column = cell.OwningColumn;
        //                if (column != null)
        //                {
        //                    cell.Value = column.GetCellValue(dataItem);
        //                }
        //            }

        //            // 检查是否匹配筛选条件
        //            if (filterManager.MatchRow(tempRow))
        //            {
        //                filteredData.Add(dataItem);
        //            }

        //            // 清理临时行
        //            tempRow.Dispose();
        //        }
        //    }
        //    else
        //    {
        //        // 无筛选，使用全部数据
        //        filteredData = new List<object>(fullDataList);
        //    }

        //    // 应用排序(如果有)
        //    if (sortedColumn != null && currentSortOrder != SortOrder.None)
        //    {
        //        var propertyName = sortedColumn.DataPropertyName;
        //        if (!string.IsNullOrEmpty(propertyName))
        //        {
        //            if (currentSortOrder == SortOrder.Ascending)
        //            {
        //                filteredData = filteredData.OrderBy(item =>
        //                {
        //                    var prop = item?.GetType().GetProperty(propertyName);
        //                    return prop?.GetValue(item);
        //                }).ToList();
        //            }
        //            else
        //            {
        //                filteredData = filteredData.OrderByDescending(item =>
        //                {
        //                    var prop = item?.GetType().GetProperty(propertyName);
        //                    return prop?.GetValue(item);
        //                }).ToList();
        //            }
        //        }
        //    }

        //    // 更新显示
        //    if (enablePagination && pagination != null)
        //    {
        //        pagination.DataSource = filteredData;
        //        LoadPageData();
        //    }
        //    else
        //    {
        //        BindDataInternal(filteredData, false);
        //    }

        //    Invalidate();
        //}

        ///// <summary>
        ///// 清除所有筛选
        ///// </summary>
        //public void ClearAllFilters()
        //{
        //    filterManager.ClearFilters();

        //    foreach (var textBox in filterTextBoxes.Values)
        //    {
        //        textBox.Text = "";
        //        textBox.Visible = false;
        //    }

        //    ApplyAllFilters();
        //}

        ///// <summary>
        ///// 清除指定列的筛选
        ///// </summary>
        //public void ClearColumnFilter(FluentDataGridViewColumn column)
        //{
        //    filterManager.RemoveFilter(column);

        //    if (filterTextBoxes.ContainsKey(column))
        //    {
        //        filterTextBoxes[column].Text = "";
        //        filterTextBoxes[column].Visible = false;
        //    }

        //    ApplyAllFilters();
        //}

        //private Rectangle GetColumnHeaderBounds(FluentDataGridViewColumn column)
        //{
        //    int x = ShowRowHeader ? rowHeaderWidth : 0;
        //    x -= hScrollBar.Value;

        //    int y = 0;
        //    if (enablePagination && paginationPosition == PaginationPosition.Top && pagination != null)
        //    {
        //        y = paginationHeight;
        //    }

        //    foreach (var col in columns)
        //    {
        //        if (!col.Visible)
        //        {
        //            continue;
        //        }

        //        if (col == column)
        //        {
        //            return new Rectangle(x, y, col.Width, columnHeaderHeight);
        //        }

        //        x += col.Width;
        //    }

        //    return Rectangle.Empty;
        //}

        //#endregion

        #region 标记管理

        /// <summary>
        /// 为指定行设置标记
        /// </summary>
        public void SetRowMark(int globalRowIndex, RowMark mark)
        {
            // 获取对应的数据项
            object dataItem = null;

            if (fullDataList != null && globalRowIndex >= 0 && globalRowIndex < fullDataList.Count)
            {
                dataItem = fullDataList[globalRowIndex];
            }
            else if (globalRowIndex >= 0 && globalRowIndex < rows.Count)
            {
                dataItem = rows[globalRowIndex].DataBoundItem;
            }

            if (dataItem == null)
            {
                return;
            }

            // 保存到数据项字典
            rowMarksByDataItem[dataItem] = mark;

            // 刷新当前显示的行
            RefreshRowMarkDisplay(dataItem, mark);
        }

        /// <summary>
        /// 为数据项设置标记
        /// </summary>
        public void SetRowMarkByDataItem(object dataItem, RowMark mark)
        {
            if (dataItem == null)
            {
                return;
            }

            rowMarksByDataItem[dataItem] = mark;
            RefreshRowMarkDisplay(dataItem, mark);
        }

        /// <summary>
        /// 刷新指定数据项的标记显示
        /// </summary>
        private void RefreshRowMarkDisplay(object dataItem, RowMark mark)
        {
            // 在当前显示的行中查找并更新
            foreach (var row in rows)
            {
                if (row.DataBoundItem == dataItem)
                {
                    row.Mark = mark;
                    InvalidateRow(row);
                    break;
                }
            }
        }

        /// <summary>
        /// 为指定全局行号集合设置标记(支持跨页)
        /// </summary>
        public void SetRowMarks(IEnumerable<int> globalRowIndices, RowMark mark)
        {
            foreach (var index in globalRowIndices)
            {
                SetRowMark(index, mark);
            }
        }

        /// <summary>
        /// 为匹配条件的行设置标记(支持全数据源筛选)
        /// </summary>
        public void SetRowMarksByFilter(Func<object, int, bool> predicate, RowMark mark)
        {
            if (fullDataList == null)
            {
                // 如果没有完整数据列表，只筛选当前显示的行
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i].DataBoundItem != null && predicate(rows[i].DataBoundItem, i))
                    {
                        rowMarksByDataItem[rows[i].DataBoundItem] = mark;
                        rows[i].Mark = mark;
                        InvalidateRow(rows[i]);
                    }
                }
            }
            else
            {
                // 有完整数据列表，遍历所有数据
                for (int i = 0; i < fullDataList.Count; i++)
                {
                    var dataItem = fullDataList[i];
                    if (predicate(dataItem, i))
                    {
                        // 保存标记
                        rowMarksByDataItem[dataItem] = mark;

                        // 如果该数据项在当前显示的行中，立即应用
                        foreach (var row in rows)
                        {
                            if (row.DataBoundItem == dataItem)
                            {
                                row.Mark = mark;
                                InvalidateRow(row);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清除指定全局行号的标记
        /// </summary>
        public void ClearRowMark(int globalRowIndex)
        {
            object dataItem = null;

            if (fullDataList != null && globalRowIndex >= 0 && globalRowIndex < fullDataList.Count)
            {
                dataItem = fullDataList[globalRowIndex];
            }
            else if (globalRowIndex >= 0 && globalRowIndex < rows.Count)
            {
                dataItem = rows[globalRowIndex].DataBoundItem;
            }

            if (dataItem != null)
            {
                ClearRowMarkByDataItem(dataItem);
            }
        }

        /// <summary>
        /// 清除指定数据项的标记
        /// </summary>
        public void ClearRowMarkByDataItem(object dataItem)
        {
            if (dataItem == null)
            {
                return;
            }

            rowMarksByDataItem.Remove(dataItem);

            // 刷新显示
            foreach (var row in rows)
            {
                if (row.DataBoundItem == dataItem)
                {
                    row.Mark = null;
                    InvalidateRow(row);
                    break;
                }
            }
        }

        /// <summary>
        /// 清除所有标记
        /// </summary>
        public void ClearAllMarks()
        {
            foreach (var row in rows)
            {
                row.ClearMark();
            }
            rowMarksByDataItem.Clear();
            Invalidate();
        }

        #endregion

        #region 批量样式更新

        /// <summary>
        /// 为指定行应用样式
        /// </summary>
        public void ApplyRowStyle(int rowIndex, DataGridViewCellStyle style)
        {
            if (rowIndex >= 0 && rowIndex < rows.Count)
            {
                rows[rowIndex].ApplyStyle(style);
            }
        }

        /// <summary>
        /// 为指定行集合应用样式
        /// </summary>
        public void ApplyRowStyles(IEnumerable<int> rowIndices, DataGridViewCellStyle style)
        {
            foreach (var index in rowIndices)
            {
                ApplyRowStyle(index, style);
            }
            Invalidate();
        }

        /// <summary>
        /// 为匹配条件的行应用样式
        /// </summary>
        public void ApplyRowStylesByFilter(Func<FluentDataGridViewRow, bool> predicate, DataGridViewCellStyle style)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (predicate(rows[i]))
                {
                    ApplyRowStyle(i, style);
                }
            }
            Invalidate();
        }

        /// <summary>
        /// 添加样式过滤器
        /// </summary>
        public void AddStyleFilter(RowStyleFilter filter)
        {
            if (filter != null)
            {
                styleFilters.Add(filter);
                styleFilters = styleFilters.OrderByDescending(f => f.Priority).ToList();

                // 重新应用所有样式过滤器
                foreach (var row in rows)
                {
                    ApplyStyleFiltersToRow(row);
                }
                Invalidate();
            }
        }

        /// <summary>
        /// 移除样式过滤器
        /// </summary>
        public void RemoveStyleFilter(RowStyleFilter filter)
        {
            styleFilters.Remove(filter);

            // 重新应用所有样式过滤器
            foreach (var row in rows)
            {
                ApplyStyleFiltersToRow(row);
            }
            Invalidate();
        }

        /// <summary>
        /// 清除所有样式过滤器
        /// </summary>
        public void ClearStyleFilters()
        {
            styleFilters.Clear();
            Invalidate();
        }

        private void ApplyStyleFiltersToRow(FluentDataGridViewRow row)
        {
            foreach (var filter in styleFilters)
            {
                if (filter.Predicate(row))
                {
                    row.ApplyStyle(filter.Style);
                    break; // 只应用优先级最高的匹配过滤器
                }
            }
        }

        #endregion

        #region 排序

        /// <summary>
        /// 对指定列进行排序
        /// </summary>
        public void Sort(FluentDataGridViewColumn column, SortOrder order)
        {
            if (column == null || column.SortMode == DataGridViewColumnSortMode.NotSortable)
            {
                return;
            }

            var args = new DataGridViewSortEventArgs(column, order);
            OnSorting(args);

            if (args.Cancel)
            {
                return;
            }

            // 清除之前的排序标记
            if (sortedColumn != null && sortedColumn != column)
            {
                sortedColumn.SortOrder = SortOrder.None;
            }

            sortedColumn = column;
            currentSortOrder = order;
            column.SortOrder = order;

            if (fullDataList == null || fullDataList.Count == 0)
            {
                return;
            }

            // 执行排序
            var propertyName = column.DataPropertyName;
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            try
            {
                if (order == SortOrder.Ascending)
                {
                    fullDataList = fullDataList.OrderBy(item =>
                    {
                        var prop = item?.GetType().GetProperty(propertyName);
                        return prop?.GetValue(item);
                    }).ToList();
                }
                else if (order == SortOrder.Descending)
                {
                    fullDataList = fullDataList.OrderByDescending(item =>
                    {
                        var prop = item?.GetType().GetProperty(propertyName);
                        return prop?.GetValue(item);
                    }).ToList();
                }

                // 重新绑定数据
                if (enablePagination && pagination != null)
                {
                    pagination.DataSource = fullDataList;
                    pagination.Refresh();
                    LoadPageData();
                }
                else
                {
                    BindDataInternal(fullDataList, false);
                }

                // 恢复行标记(排序后重新应用)
                RestoreRowMarks();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sort error: {ex.Message}");
            }

            Invalidate();
        }

        private void RestoreRowMarks()
        {
            foreach (var row in rows)
            {
                if (row.DataBoundItem != null && rowMarksByDataItem.ContainsKey(row.DataBoundItem))
                {
                    row.Mark = rowMarksByDataItem[row.DataBoundItem];
                }
                else
                {
                    row.Mark = null;
                }
            }
        }

        /// <summary>
        /// 切换列的排序顺序
        /// </summary>
        private void ToggleSort(FluentDataGridViewColumn column)
        {
            if (column == null || column.SortMode == DataGridViewColumnSortMode.NotSortable)
            {
                return;
            }

            SortOrder newOrder;
            if (sortedColumn == column)
            {
                // 同一列：Ascending -> Descending -> None -> Ascending
                switch (currentSortOrder)
                {
                    case SortOrder.None:
                        newOrder = SortOrder.Ascending;
                        break;
                    case SortOrder.Ascending:
                        newOrder = SortOrder.Descending;
                        break;
                    case SortOrder.Descending:
                        newOrder = SortOrder.None;
                        break;
                    default:
                        newOrder = SortOrder.Ascending;
                        break;
                }
            }
            else
            {
                // 新列：从升序开始
                newOrder = SortOrder.Ascending;

                // 清除之前列的排序状态
                if (sortedColumn != null)
                {
                    sortedColumn.SortOrder = SortOrder.None;
                }
            }

            if (newOrder == SortOrder.None)
            {
                // 清除排序，恢复原始数据
                column.SortOrder = SortOrder.None;
                sortedColumn = null;
                currentSortOrder = SortOrder.None;
                BindData();
            }
            else
            {
                Sort(column, newOrder);
            }
        }

        #endregion

        #region 选择

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearSelection()
        {
            // 添加空检查
            if (rows == null || rows.Count == 0)
            {
                selectedRows.Clear();
                selectedCells.Clear();
                OnSelectionChanged(EventArgs.Empty);
                return;
            }

            // 清除所有选中的行，并重置其 Selected 状态
            foreach (var row in selectedRows.ToList())
            {
                if (row != null)
                {
                    row.Selected = false;

                    // 同时清除该行所有单元格的选中状态
                    foreach (var cell in row.Cells)
                    {
                        if (cell != null)
                        {
                            cell.Selected = false;
                        }
                    }

                    // 刷新该行
                    InvalidateRow(row);
                }
            }
            selectedRows.Clear();

            // 清除所有选中的单元格
            foreach (var cellAddress in selectedCells.ToList())
            {
                if (cellAddress.RowIndex >= 0 && cellAddress.RowIndex < rows.Count &&
                    cellAddress.ColumnIndex >= 0 && cellAddress.ColumnIndex < columns.Count)
                {
                    var cell = rows[cellAddress.RowIndex].Cells[cellAddress.ColumnIndex];
                    if (cell != null)
                    {
                        cell.Selected = false;

                        // 刷新该单元格
                        InvalidateCell(cellAddress.ColumnIndex, cellAddress.RowIndex);
                    }
                }
            }
            selectedCells.Clear();

            OnSelectionChanged(EventArgs.Empty);

            // 强制重绘以确保视觉更新
            Invalidate();
            Update();
        }

        /// <summary>
        /// 选择行
        /// </summary>
        public void SelectRow(int rowIndex, bool clearPrevious = true)
        {
            if (rowIndex < 0 || rowIndex >= rows.Count)
            {
                return;
            }

            if (clearPrevious)
            {
                ClearSelection();
            }

            var row = rows[rowIndex];
            if (!row.Selected)
            {
                row.Selected = true;
                if (!selectedRows.Contains(row))
                {
                    selectedRows.Add(row);
                }
                OnSelectionChanged(EventArgs.Empty);
                InvalidateRow(row);
            }

            // 更新当前单元格
            if (columns.Count > 0)
            {
                currentCell = new DataGridViewCellAddress(0, rowIndex);
            }
        }

        /// <summary>
        /// 选择单元格
        /// </summary>
        public void SelectCell(int columnIndex, int rowIndex, bool clearPrevious = true)
        {
            if (columnIndex < 0 || columnIndex >= columns.Count || rowIndex < 0 || rowIndex >= rows.Count)
            {
                return;
            }

            if (clearPrevious)
            {
                ClearSelection();
            }

            var address = new DataGridViewCellAddress(columnIndex, rowIndex);
            if (!selectedCells.Contains(address))
            {
                selectedCells.Add(address);
                var cell = rows[rowIndex].Cells[columnIndex];
                cell.Selected = true;
                OnSelectionChanged(EventArgs.Empty);
                InvalidateCell(columnIndex, rowIndex);
            }

            currentCell = address;
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 开始编辑单元格
        /// </summary>
        private bool BeginEdit(int columnIndex, int rowIndex)
        {
            if (ReadOnly || columnIndex < 0 || rowIndex < 0 ||
                columnIndex >= columns.Count || rowIndex >= rows.Count)
            {
                return false;
            }

            var column = columns[columnIndex];
            var cell = rows[rowIndex].Cells[columnIndex];

            if (column.ReadOnly || cell.ReadOnly)
            {
                return false;
            }

            // 结束之前的编辑
            EndEdit();

            // 开始新的编辑
            if (cell.BeginEdit())
            {
                currentEditingControl = cell.IsInEditMode ? GetEditingControl(cell) : null;
                if (currentEditingControl != null)
                {
                    editingCellAddress = new DataGridViewCellAddress(columnIndex, rowIndex);

                    // 计算编辑控件的位置
                    var cellBounds = GetCellBounds(columnIndex, rowIndex);
                    currentEditingControl.Bounds = cellBounds;
                    currentEditingControl.Visible = true;
                    currentEditingControl.Focus();

                    Controls.Add(currentEditingControl);
                    return true;
                }
            }

            return false;
        }

        private Control GetEditingControl(FluentDataGridViewCell cell)
        {
            // 这个方法需要访问cell的私有editingControl字段
            // 实际实现中，需要在Cell类中提供GetEditingControl方法
            var field = typeof(FluentDataGridViewCell).GetField("editingControl",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(cell) as Control;
        }

        /// <summary>
        /// 结束编辑
        /// </summary>
        private bool EndEdit()
        {
            if (editingCellAddress.ColumnIndex < 0 || editingCellAddress.RowIndex < 0)
            {
                return false;
            }

            try
            {
                var cell = rows[editingCellAddress.RowIndex].Cells[editingCellAddress.ColumnIndex];
                if (cell.EndEdit())
                {
                    // 更新数据源
                    if (rows[editingCellAddress.RowIndex].DataBoundItem != null)
                    {
                        var column = columns[editingCellAddress.ColumnIndex];
                        column.SetCellValue(rows[editingCellAddress.RowIndex].DataBoundItem, cell.Value);
                    }

                    return true;
                }
            }
            finally
            {
                if (currentEditingControl != null)
                {
                    Controls.Remove(currentEditingControl);
                    currentEditingControl.Dispose();
                    currentEditingControl = null;
                }
                editingCellAddress = new DataGridViewCellAddress(-1, -1);
            }

            return false;
        }

        /// <summary>
        /// 取消编辑
        /// </summary>
        private void CancelEdit()
        {
            if (editingCellAddress.ColumnIndex >= 0 && editingCellAddress.RowIndex >= 0)
            {
                var cell = rows[editingCellAddress.RowIndex].Cells[editingCellAddress.ColumnIndex];
                cell.CancelEdit();

                if (currentEditingControl != null)
                {
                    Controls.Remove(currentEditingControl);
                    currentEditingControl.Dispose();
                    currentEditingControl = null;
                }

                editingCellAddress = new DataGridViewCellAddress(-1, -1);
            }
        }

        #endregion

        #region 布局计算

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            // 先布局分页控件
            if (enablePagination && pagination != null)
            {
                LayoutPagination();
            }

            // 再更新主布局
            UpdateLayout();
        }

        private void LayoutPagination()
        {
            if (pagination == null || !enablePagination)
            {
                return;
            }

            int paginationY;
            int paginationWidth = Width;

            // 如果有垂直滚动条，宽度要减去滚动条宽度
            if (vScrollBar != null && vScrollBar.Visible)
            {
                paginationWidth -= vScrollBar.Width;
            }

            switch (paginationPosition)
            {
                case PaginationPosition.Top:
                    // 分页在顶部：紧贴控件顶部
                    paginationY = 0;
                    pagination.Location = new Point(0, paginationY);
                    pagination.Size = new Size(paginationWidth, paginationHeight);

                    // 确保分页控件在最上层
                    pagination.BringToFront();
                    break;

                case PaginationPosition.Bottom:
                default:
                    // 分页在底部：考虑水平滚动条
                    paginationY = Height - paginationHeight;
                    if (hScrollBar != null && hScrollBar.Visible)
                    {
                        paginationY -= hScrollBar.Height;
                    }

                    pagination.Location = new Point(0, paginationY);
                    pagination.Size = new Size(paginationWidth, paginationHeight);

                    // 确保分页控件在最上层
                    pagination.BringToFront();
                    break;
            }

            // 确保分页控件可见
            pagination.Visible = true;
        }

        private Rectangle GetContentArea()
        {
            int top = columnHeaderHeight;
            int height = ClientSize.Height - columnHeaderHeight;

            // 如果启用分页，调整区域
            if (enablePagination && pagination != null)
            {
                if (paginationPosition == PaginationPosition.Top)
                {
                    // 分页在顶部：列头下移，内容区域减少
                    top += paginationHeight;
                    height -= paginationHeight;
                }
                else
                {
                    // 分页在底部：内容区域高度减少
                    height -= paginationHeight;
                }
            }

            int left = ShowRowHeader ? rowHeaderWidth : 0;
            int width = ClientSize.Width - left;

            if (vScrollBar.Visible)
            {
                width -= vScrollBar.Width;
            }

            if (hScrollBar.Visible)
            {
                height -= hScrollBar.Height;
            }

            return new Rectangle(left, top, Math.Max(0, width), Math.Max(0, height));
        }

        private void UpdateLayout()
        {
            if (columns.Count == 0)
            {
                vScrollBar.Visible = false;
                hScrollBar.Visible = false;
                return;
            }

            // 计算内容区域
            var contentArea = GetContentArea();

            // 计算列宽
            CalculateColumnWidths(contentArea.Width);

            // 更新滚动条
            UpdateScrollBars(contentArea);
        }

        private void CalculateColumnWidths(int availableWidth)
        {
            // 计算固定宽度列的总宽度
            int fixedWidth = 0;
            int autoSizeCount = 0;
            int fillCount = 0;
            float totalPercentage = 0;

            foreach (var column in columns)
            {
                if (!column.Visible)
                {
                    continue;
                }

                switch (column.WidthMode)
                {
                    case DataGridViewColumnWidthMode.Fixed:
                        fixedWidth += column.Width;
                        break;
                    case DataGridViewColumnWidthMode.Percentage:
                        totalPercentage += column.WidthPercentage;
                        break;
                    case DataGridViewColumnWidthMode.AutoSize:
                        autoSizeCount++;
                        break;
                    case DataGridViewColumnWidthMode.Fill:
                        fillCount++;
                        break;
                }
            }

            // 计算百分比宽度
            int percentageWidth = 0;
            if (totalPercentage > 0)
            {
                percentageWidth = (int)(availableWidth * totalPercentage / 100f);
            }

            // 计算自动大小列的宽度
            int autoSizeWidth = 0;
            if (autoSizeCount > 0)
            {
                foreach (var column in columns)
                {
                    if (column.Visible && column.WidthMode == DataGridViewColumnWidthMode.AutoSize)
                    {
                        column.Width = CalculateAutoWidth(column);
                        autoSizeWidth += column.Width;
                    }
                }
            }

            // 计算填充列的宽度
            int remainingWidth = availableWidth - fixedWidth - percentageWidth - autoSizeWidth;
            if (fillCount > 0 && remainingWidth > 0)
            {
                int fillWidth = remainingWidth / fillCount;
                foreach (var column in columns)
                {
                    if (column.Visible && column.WidthMode == DataGridViewColumnWidthMode.Fill)
                    {
                        column.Width = Math.Max(column.MinimumWidth, fillWidth);
                    }
                }
            }

            // 应用百分比宽度
            foreach (var column in columns)
            {
                if (column.Visible && column.WidthMode == DataGridViewColumnWidthMode.Percentage)
                {
                    column.Width = Math.Max(column.MinimumWidth,
                        (int)(availableWidth * column.WidthPercentage / 100f));
                }
            }
        }

        private int CalculateAutoWidth(FluentDataGridViewColumn column)
        {
            int maxWidth = column.MinimumWidth;

            // 计算列头宽度
            using (var g = CreateGraphics())
            {
                var headerSize = g.MeasureString(column.HeaderText ?? "", Font);
                maxWidth = Math.Max(maxWidth, (int)headerSize.Width + 40); // 40 = padding + sort indicator
            }

            // 计算单元格内容宽度(采样前100行)
            int sampleCount = Math.Min(100, rows.Count);
            using (var g = CreateGraphics())
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    var cell = rows[i].Cells[column.Index];
                    var text = cell.FormattedValue;
                    if (!string.IsNullOrEmpty(text))
                    {
                        var size = g.MeasureString(text, Font);
                        maxWidth = Math.Max(maxWidth, (int)size.Width + 20); // 20 = padding
                    }
                }
            }

            return maxWidth;
        }

        private void UpdateScrollBars(Rectangle contentArea)
        {
            // 计算总内容高度
            int totalHeight = 0;
            foreach (var row in rows)
            {
                if (row.Visible)
                {
                    totalHeight += row.Height;
                }
            }

            // 计算总内容宽度
            int totalWidth = 0;
            foreach (var column in columns)
            {
                if (column.Visible)
                {
                    totalWidth += column.Width;
                }
            }

            // 更新垂直滚动条
            bool needVScroll = totalHeight > contentArea.Height;
            if (needVScroll)
            {
                vScrollBar.Minimum = 0;
                // 最大值应该是总高度减去可见高度，而不是总高度减1
                vScrollBar.Maximum = totalHeight - contentArea.Height + vScrollBar.LargeChange - 1;
                vScrollBar.LargeChange = contentArea.Height;
                vScrollBar.SmallChange = DefaultRowHeight;

                // 确保当前值在有效范围内
                if (vScrollBar.Value > vScrollBar.Maximum - vScrollBar.LargeChange + 1)
                {
                    vScrollBar.Value = Math.Max(0, vScrollBar.Maximum - vScrollBar.LargeChange + 1);
                }

                // 设置滚动条位置和大小
                int scrollBarY = contentArea.Top;
                int scrollBarHeight = contentArea.Height;

                vScrollBar.Location = new Point(Width - vScrollBar.Width, scrollBarY);
                vScrollBar.Height = scrollBarHeight;
                vScrollBar.Visible = true;
            }
            else
            {
                vScrollBar.Visible = false;
                firstVisibleRowIndex = 0;
            }

            // 更新水平滚动条
            bool needHScroll = totalWidth > contentArea.Width;
            if (needHScroll)
            {
                hScrollBar.Minimum = 0;
                // 同样修正最大值计算
                hScrollBar.Maximum = totalWidth - contentArea.Width + hScrollBar.LargeChange - 1;
                hScrollBar.LargeChange = contentArea.Width;
                hScrollBar.SmallChange = 50;

                // 确保当前值在有效范围内
                if (hScrollBar.Value > hScrollBar.Maximum - hScrollBar.LargeChange + 1)
                {
                    hScrollBar.Value = Math.Max(0, hScrollBar.Maximum - hScrollBar.LargeChange + 1);
                }

                // 设置滚动条位置和大小
                int scrollBarY = contentArea.Bottom;
                int scrollBarWidth = contentArea.Width + (ShowRowHeader ? rowHeaderWidth : 0);

                hScrollBar.Location = new Point(0, scrollBarY);
                hScrollBar.Width = scrollBarWidth;
                hScrollBar.Visible = true;
            }
            else
            {
                hScrollBar.Visible = false;
                firstVisibleColumnIndex = 0;
            }
        }

        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateFirstVisibleRow();
            Invalidate();
            Update();
        }

        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            var backColor = GetThemeColor(c => c.Background, SystemColors.Window);
            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (columns.Count == 0)
            {
                return;
            }

            // 保存原始裁剪区域
            var oldClip = g.Clip;

            try
            {
                // 1. 先绘制列头(不裁剪)
                g.ResetClip();
                DrawColumnHeaders(g);

                // 2. 计算内容区域
                var contentArea = GetContentArea();

                // 3. 绘制行头(不裁剪，因为行头在左侧固定区域)
                g.ResetClip();
                if (ShowRowHeader)
                {
                    DrawRowHeaders(g, contentArea);
                }

                // 4. 设置内容区域裁剪(只裁剪数据单元格区域)
                g.SetClip(contentArea);

                // 5. 绘制数据行
                DrawRows(g, contentArea);

                // 6. 绘制网格线
                if (ShowGridLines)
                {
                    DrawGridLines(g, contentArea);
                }
            }
            finally
            {
                // 恢复原始裁剪
                g.Clip = oldClip;
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            var borderColor = GetThemeColor(c => c.Border, Color.FromArgb(204, 204, 204));
            using (var pen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void DrawColumnHeaders(Graphics g)
        {
            int x = ShowRowHeader ? rowHeaderWidth : 0;
            x -= hScrollBar.Value;
            int y = 0;

            // 如果分页在顶部，调整Y坐标
            if (enablePagination && paginationPosition == PaginationPosition.Top && pagination != null)
            {
                y = paginationHeight;
            }

            var headerBackColor = GetThemeColor(c => c.BackgroundSecondary, Color.FromArgb(250, 250, 250));
            var headerForeColor = GetThemeColor(c => c.TextPrimary, Color.Black);
            var borderColor = GetThemeColor(c => c.Border, Color.FromArgb(229, 229, 229));

            // 绘制整个列头背景
            var headerBounds = new Rectangle(0, y, Width, columnHeaderHeight);
            using (var brush = new SolidBrush(headerBackColor))
            {
                g.FillRectangle(brush, headerBounds);
            }

            // 绘制行头区域(如果显示)
            if (ShowRowHeader)
            {
                var rowHeaderRect = new Rectangle(0, y, rowHeaderWidth, columnHeaderHeight);
                using (var brush = new SolidBrush(headerBackColor))
                {
                    g.FillRectangle(brush, rowHeaderRect);
                }
                using (var pen = new Pen(borderColor))
                {
                    g.DrawRectangle(pen, rowHeaderRect.X, rowHeaderRect.Y,
                        rowHeaderRect.Width - 1, rowHeaderRect.Height - 1);
                }
            }

            // 绘制各列头
            foreach (var column in columns)
            {
                if (!column.Visible)
                {
                    continue;
                }

                var headerRect = new Rectangle(x, y, column.Width, columnHeaderHeight);

                // 检查是否在可见区域内
                if (headerRect.Right < 0 || headerRect.X > Width)
                {
                    x += column.Width;
                    continue;
                }

                // 绘制列头背景(悬停效果)
                if (IsMouseOverColumnHeader(column.Index))
                {
                    using (var brush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                    {
                        g.FillRectangle(brush, headerRect);
                    }
                }

                // 绘制文本
                var textRect = new Rectangle(
                    headerRect.X + 8,
                    headerRect.Y,
                    headerRect.Width - 16,
                    headerRect.Height);

                using (var format = new StringFormat
                {
                    Alignment = GetStringAlignment(column.HeaderTextAlign),
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                })
                using (var brush = new SolidBrush(headerForeColor))
                {
                    var headerText = !string.IsNullOrEmpty(column.HeaderText)
                        ? column.HeaderText
                        : column.Name;
                    g.DrawString(headerText, Font, brush, textRect, format);
                }

                // 绘制排序指示器
                if (column.SortOrder != SortOrder.None)
                {
                    DrawSortIndicator(g, headerRect, column.SortOrder);
                }

                // 绘制列分隔线
                using (var pen = new Pen(borderColor))
                {
                    g.DrawLine(pen, headerRect.Right - 1, headerRect.Top,
                        headerRect.Right - 1, headerRect.Bottom - 1);
                }

                x += column.Width;
            }

            // 绘制列头底部边框
            using (var pen = new Pen(borderColor, 2))
            {
                int borderY = y + columnHeaderHeight - 1;
                g.DrawLine(pen, 0, borderY, Width, borderY);
            }
        }

        private void DrawSortIndicator(Graphics g, Rectangle headerRect, SortOrder sortOrder)
        {
            if (sortOrder == SortOrder.None)
            {
                return;
            }

            var arrowRect = new Rectangle(
                headerRect.Right - 20,
                headerRect.Y + (headerRect.Height - 8) / 2,
                8, 8);

            var points = sortOrder == SortOrder.Ascending
                ? new[]
                {
                    new Point(arrowRect.X, arrowRect.Bottom),
                    new Point(arrowRect.Right, arrowRect.Bottom),
                    new Point(arrowRect.X + arrowRect.Width / 2, arrowRect.Top)
                }
                : new[]
                {
                    new Point(arrowRect.X, arrowRect.Top),
                    new Point(arrowRect.Right, arrowRect.Top),
                    new Point(arrowRect.X + arrowRect.Width / 2, arrowRect.Bottom)
                };

            var color = GetThemeColor(c => c.Primary, Color.FromArgb(0, 120, 215));
            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }
        }

        private void DrawRowHeaders(Graphics g, Rectangle contentArea)
        {
            if (!ShowRowHeader)
            {
                return;
            }

            var headerBackColor = GetThemeColor(c => c.BackgroundSecondary, Color.FromArgb(250, 250, 250));
            var headerForeColor = GetThemeColor(c => c.TextSecondary, Color.FromArgb(96, 96, 96));
            var borderColor = GetThemeColor(c => c.Border, Color.FromArgb(229, 229, 229));

            // 保存原始裁剪区域
            var oldClip = g.Clip;

            try
            {
                // 设置行头的裁剪区域：从内容区域顶部开始，不包括列头
                var rowHeaderClipRect = new Rectangle(0, contentArea.Top, rowHeaderWidth, contentArea.Height);
                g.SetClip(rowHeaderClipRect);

                int scrollOffset = vScrollBar?.Value ?? 0;

                int accumulatedHeight = 0;
                for (int i = 0; i < firstVisibleRowIndex && i < rows.Count; i++)
                {
                    if (rows[i].Visible)
                    {
                        accumulatedHeight += rows[i].Height;
                    }
                }

                int y = contentArea.Top - (scrollOffset - accumulatedHeight);

                // 先绘制整个行头列的背景
                using (var brush = new SolidBrush(headerBackColor))
                {
                    g.FillRectangle(brush, rowHeaderClipRect);
                }

                // 然后绘制每一行的行头
                for (int i = firstVisibleRowIndex; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (!row.Visible)
                    {
                        continue;
                    }

                    if (y + row.Height < contentArea.Top)
                    {
                        y += row.Height;
                        continue;
                    }

                    if (y > contentArea.Bottom)
                    {
                        break;
                    }

                    int rowNumber = CalculateRowNumber(i);

                    var headerRect = new Rectangle(0, y, rowHeaderWidth, row.Height);

                    // 确保不绘制到列头区域
                    if (headerRect.Top < contentArea.Top)
                    {
                        int clipHeight = contentArea.Top - headerRect.Top;
                        headerRect = new Rectangle(
                            headerRect.X,
                            contentArea.Top,
                            headerRect.Width,
                            headerRect.Height - clipHeight);
                    }

                    if (row.Selected)
                    {
                        var selectedBackColor = GetThemeColor(c => c.Primary, Color.FromArgb(0, 120, 215));
                        using (var brush = new SolidBrush(selectedBackColor))
                        {
                            g.FillRectangle(brush, headerRect);
                        }
                    }

                    bool hasMark = row.Mark != null && row.Mark.MarkType != RowMarkType.None
                                  && row.Mark.Position == RowMarkPosition.RowHeader;
                    bool showNumber = ShowRowNumbers;

                    if (hasMark && showNumber)
                    {
                        DrawRowHeaderWithMarkAndNumber(g, headerRect, row, rowNumber,
                            row.Selected ? Color.White : headerForeColor);
                    }
                    else if (hasMark)
                    {
                        DrawRowHeaderMark(g, headerRect, row.Mark);
                    }
                    else if (showNumber)
                    {
                        DrawRowHeaderNumber(g, headerRect, rowNumber,
                            row.Selected ? Color.White : headerForeColor);
                    }

                    using (var pen = new Pen(borderColor, 1))
                    {
                        g.DrawLine(pen, headerRect.Right - 1, headerRect.Top,
                            headerRect.Right - 1, headerRect.Bottom - 1);
                        g.DrawLine(pen, headerRect.Left, headerRect.Bottom - 1,
                            headerRect.Right - 1, headerRect.Bottom - 1);
                    }

                    y += row.Height;
                }

                // 绘制行头右侧的主边框
                using (var pen = new Pen(borderColor, 2))
                {
                    g.DrawLine(pen, rowHeaderWidth - 1, contentArea.Top, rowHeaderWidth - 1, contentArea.Bottom);
                }
            }
            finally
            {
                // 恢复原始裁剪区域
                g.Clip = oldClip;
            }
        }

        /// <summary>
        /// 根据行索引计算实际行号的方法
        /// </summary>
        private int CalculateRowNumber(int rowIndex)
        {
            if (!ShowRowNumbers)
            {
                return 0;
            }

            if (enablePagination && pagination != null)
            {
                // 分页模式：当前页起始行号 + 行索引
                return ((pagination.CurrentPage - 1) * pagination.PageSize + 1) + rowIndex;
            }

            // 非分页模式：起始行号 + 行索引
            return startRowNumber + rowIndex;
        }


        private int CalculatePageStartRowNumber()
        {
            if (!ShowRowNumbers)
            {
                return 0;
            }

            if (enablePagination && pagination != null)
            {
                return ((pagination.CurrentPage - 1) * pagination.PageSize + 1);
            }

            return startRowNumber;
        }

        private void DrawRowHeaderWithMarkAndNumber(Graphics g, Rectangle headerRect, FluentDataGridViewRow row, int rowNumber, Color foreColor)
        {
            // 计算布局
            int markSize = 14; // 减小标记大小
            int markPadding = 2;

            // 行号区域(占大部分空间)
            var numberRect = new Rectangle(
                headerRect.X,
                headerRect.Y,
                headerRect.Width - markSize - markPadding * 2,
                headerRect.Height);

            // 标记区域(右侧小图标)
            var markRect = new Rectangle(
                headerRect.Right - markSize - markPadding,
                headerRect.Y + (headerRect.Height - markSize) / 2,
                markSize,
                markSize);

            // 绘制行号
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            using (var brush = new SolidBrush(foreColor))
            using (var font = new Font(Font.FontFamily, Font.Size))
            {
                g.DrawString(rowNumber.ToString(), font, brush, numberRect, format);
            }

            // 绘制标记
            if (row.Mark != null)
            {
                row.Mark.Draw(g, markRect);
            }
        }

        private void DrawRowHeaderMark(Graphics g, Rectangle headerRect, RowMark mark)
        {
            if (mark == null || mark.MarkType == RowMarkType.None)
            {
                return;
            }

            // 居中显示标记
            var markSize = Math.Min(20, Math.Min(headerRect.Width - 4, headerRect.Height - 4));
            var markRect = new Rectangle(
                headerRect.X + (headerRect.Width - markSize) / 2,
                headerRect.Y + (headerRect.Height - markSize) / 2,
                markSize, markSize);

            mark.Draw(g, markRect);
        }

        private void DrawRowHeaderNumber(Graphics g, Rectangle headerRect, int rowNumber, Color foreColor)
        {
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            using (var brush = new SolidBrush(foreColor))
            using (var font = new Font(Font.FontFamily, Math.Max(7, Font.Size - 1)))
            {
                g.DrawString(rowNumber.ToString(), font, brush, headerRect, format);
            }
        }

        private void DrawRows(Graphics g, Rectangle contentArea)
        {
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            int scrollOffset = vScrollBar?.Value ?? 0;

            int accumulatedHeight = 0;
            for (int i = 0; i < firstVisibleRowIndex && i < rows.Count; i++)
            {
                if (rows[i] != null && rows[i].Visible)
                {
                    accumulatedHeight += rows[i].Height;
                }
            }

            int y = contentArea.Top - (scrollOffset - accumulatedHeight);

            for (int i = firstVisibleRowIndex; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row == null || !row.Visible)
                {
                    continue;
                }

                if (y + row.Height < contentArea.Top)
                {
                    y += row.Height;
                    continue;
                }

                if (y > contentArea.Bottom)
                {
                    break;
                }

                DrawRow(g, row, y, contentArea);
                y += row.Height;
            }
        }

        private void DrawRow(Graphics g, FluentDataGridViewRow row, int y, Rectangle contentArea)
        {
            int x = ShowRowHeader ? rowHeaderWidth : 0;
            x -= hScrollBar.Value;

            for (int colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                var column = columns[colIndex];
                if (!column.Visible)
                {
                    continue;
                }

                var cell = row.Cells[colIndex];
                var cellBounds = new Rectangle(x, y, column.Width, row.Height);

                // 确定单元格状态
                var cellState = DataGridViewCellState.None;

                if (cell.Selected || row.Selected)
                {
                    cellState |= DataGridViewCellState.Selected;
                }

                if (hoveredCell.ColumnIndex == colIndex && hoveredCell.RowIndex == row.Index)
                {
                    cellState |= DataGridViewCellState.Hovered;
                }

                if (cell.IsInEditMode)
                {
                    cellState |= DataGridViewCellState.Editing;
                }

                if (cell.ReadOnly || column.ReadOnly)
                {
                    cellState |= DataGridViewCellState.ReadOnly;
                }

                // 绘制单元格
                cell.Paint(g, cellBounds, cellState);

                x += column.Width;
            }
        }

        private void DrawGridLines(Graphics g, Rectangle contentArea)
        {
            using (var pen = new Pen(GridColor))
            {
                // 绘制水平网格线
                int y = columnHeaderHeight;
                for (int i = firstVisibleRowIndex; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (!row.Visible)
                    {
                        continue;
                    }

                    if (y >= contentArea.Bottom)
                    {
                        break;
                    }

                    y += row.Height;
                    g.DrawLine(pen, contentArea.Left, y - 1, contentArea.Right, y - 1);
                }

                // 绘制垂直网格线
                int x = ShowRowHeader ? rowHeaderWidth : 0;
                x -= hScrollBar.Value;

                foreach (var column in columns)
                {
                    if (!column.Visible)
                    {
                        continue;
                    }

                    x += column.Width;
                    g.DrawLine(pen, x - 1, columnHeaderHeight, x - 1, contentArea.Bottom);
                }
            }
        }

        private StringAlignment GetStringAlignment(HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    return StringAlignment.Near;
                case HorizontalAlignment.Center:
                    return StringAlignment.Center;
                case HorizontalAlignment.Right:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Near;
            }
        }

        #endregion

        #region 鼠标交互

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // 处理列宽调整
            if (isResizingColumn)
            {
                HandleColumnResize(e.X);
                return;
            }

            // 更新鼠标指针(列宽调整)
            if (AllowUserToResizeColumns)
            {
                var resizeColumn = GetColumnResizeIndex(e.Location);
                Cursor = resizeColumn >= 0 ? Cursors.VSplit : Cursors.Default;
            }

            // 更新悬停单元格
            var cell = GetCellAtPoint(e.Location);

            // 验证单元格地址的有效性
            bool isValidCell = cell.ColumnIndex >= 0 && cell.ColumnIndex < columns.Count &&
                               cell.RowIndex >= 0 && cell.RowIndex < rows.Count;

            if (!isValidCell)
            {
                cell = new DataGridViewCellAddress(-1, -1);
            }

            if (cell != hoveredCell)
            {
                var oldHovered = hoveredCell;
                hoveredCell = cell;

                // 刷新受影响的单元格
                if (oldHovered.ColumnIndex >= 0 && oldHovered.RowIndex >= 0 &&
                    oldHovered.ColumnIndex < columns.Count && oldHovered.RowIndex < rows.Count)
                {
                    InvalidateCell(oldHovered.ColumnIndex, oldHovered.RowIndex);
                }

                if (hoveredCell.ColumnIndex >= 0 && hoveredCell.RowIndex >= 0)
                {
                    InvalidateCell(hoveredCell.ColumnIndex, hoveredCell.RowIndex);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // 确保控件获得焦点以接收键盘事件
            if (!Focused && CanFocus)
            {
                Focus();
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // 检查是否点击列头
            if (e.Y < columnHeaderHeight)
            {
                var columnIndex = GetColumnIndexAtX(e.X);
                if (columnIndex >= 0)
                {
                    // 检查是否点击调整大小区域
                    if (AllowUserToResizeColumns)
                    {
                        var resizeIndex = GetColumnResizeIndex(e.Location);
                        if (resizeIndex >= 0)
                        {
                            StartColumnResize(resizeIndex, e.X);
                            return;
                        }
                    }

                    // 处理列头点击(排序)
                    var column = columns[columnIndex];
                    OnColumnHeaderClick(new DataGridViewSortEventArgs(column, column.SortOrder));

                    if (column.SortMode == DataGridViewColumnSortMode.Automatic)
                    {
                        ToggleSort(column);
                    }
                    return;
                }
            }

            // 检查是否点击单元格
            var cell = GetCellAtPoint(e.Location);
            if (cell.ColumnIndex >= 0 && cell.RowIndex >= 0)
            {
                HandleCellClick(cell, e);
            }

            Focus();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (isResizingColumn)
            {
                EndColumnResize();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            // 自动获取焦点以接收滚轮事件
            if (!Focused && CanFocus)
            {
                Focus();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // 列头筛选
            //bool columnFilterState = false;
            //if (enableFiltering)
            //{
            //    int headerY = 0;
            //    if (enablePagination && paginationPosition == PaginationPosition.Top && pagination != null)
            //    {
            //        headerY = paginationHeight;
            //    }

            //    if (e.Y >= headerY && e.Y < headerY + columnHeaderHeight)
            //    {
            //        columnFilterState = true;

            //        var columnIndex = GetColumnIndexAtX(e.X);
            //        if (columnIndex >= 0 && columnIndex < columns.Count)
            //        {
            //            var column = columns[columnIndex];
            //            ShowColumnFilter(column);
            //        }
            //    }
            //}

            var cell = GetCellAtPoint(e.Location);
            if (cell.ColumnIndex >= 0 && cell.RowIndex >= 0)
            {
                OnCellDoubleClick(new DataGridViewCellEventArgs(cell.ColumnIndex, cell.RowIndex));

                // 处理编辑
                if (EditMode == DataGridViewEditMode.EditOnDoubleClick)
                {
                    BeginEdit(cell.ColumnIndex, cell.RowIndex);
                }
            }

        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hoveredCell.ColumnIndex >= 0 && hoveredCell.RowIndex >= 0)
            {
                InvalidateCell(hoveredCell.ColumnIndex, hoveredCell.RowIndex);
                hoveredCell = new DataGridViewCellAddress(-1, -1);
            }
        }

        private void HandleCellClick(DataGridViewCellAddress cell, MouseEventArgs e)
        {
            OnCellClick(new DataGridViewCellEventArgs(cell.ColumnIndex, cell.RowIndex));

            // 检查是否点击按钮列
            if (columns[cell.ColumnIndex] is FluentDataGridViewButtonColumn buttonColumn)
            {
                buttonColumn.OnButtonClick(new DataGridViewCellEventArgs(cell.ColumnIndex, cell.RowIndex,
                    rows[cell.RowIndex].Cells[cell.ColumnIndex]));
                return;
            }

            // 检查修饰键
            bool isMultiSelect = ModifierKeys.HasFlag(Keys.Control);
            bool isRangeSelect = ModifierKeys.HasFlag(Keys.Shift);

            // 处理选择
            switch (SelectionMode)
            {
                case DataGridViewSelectionMode.CellSelect:
                    if (isRangeSelect && rangeSelectionAnchor.ColumnIndex >= 0 && rangeSelectionAnchor.RowIndex >= 0)
                    {
                        // Shift+点击：范围选择
                        SelectCellRange(rangeSelectionAnchor, cell, !isMultiSelect);
                    }
                    else if (isMultiSelect)
                    {
                        // Ctrl+点击：切换单元格选中状态
                        ToggleCellSelection(cell.ColumnIndex, cell.RowIndex);
                        rangeSelectionAnchor = cell; // 更新锚点
                    }
                    else
                    {
                        // 普通点击：清除其他选择，只选中当前单元格
                        SelectCell(cell.ColumnIndex, cell.RowIndex, clearPrevious: true);
                        rangeSelectionAnchor = cell; // 设置新锚点
                    }
                    break;

                case DataGridViewSelectionMode.FullRowSelect:
                    if (isRangeSelect && rangeSelectionAnchor.RowIndex >= 0)
                    {
                        // Shift+点击：范围选择行
                        SelectRowRange(rangeSelectionAnchor.RowIndex, cell.RowIndex, !isMultiSelect);
                    }
                    else if (isMultiSelect)
                    {
                        // Ctrl+点击：切换行选中状态
                        ToggleRowSelection(cell.RowIndex);
                        rangeSelectionAnchor = new DataGridViewCellAddress(0, cell.RowIndex);
                    }
                    else
                    {
                        // 普通点击：清除其他选择，只选中当前行
                        SelectRow(cell.RowIndex, clearPrevious: true);
                        rangeSelectionAnchor = new DataGridViewCellAddress(0, cell.RowIndex);
                    }
                    break;

                case DataGridViewSelectionMode.FullColumnSelect:
                    // TODO: 实现列选择
                    break;
            }

            // 处理编辑
            if (EditMode == DataGridViewEditMode.EditOnEnter && !ReadOnly && !isMultiSelect && !isRangeSelect)
            {
                BeginEdit(cell.ColumnIndex, cell.RowIndex);
            }
        }

        /// <summary>
        /// 选择单元格范围
        /// </summary>
        private void SelectCellRange(DataGridViewCellAddress start, DataGridViewCellAddress end, bool clearPrevious)
        {
            if (clearPrevious)
            {
                ClearSelection();
            }

            int minRow = Math.Min(start.RowIndex, end.RowIndex);
            int maxRow = Math.Max(start.RowIndex, end.RowIndex);
            int minCol = Math.Min(start.ColumnIndex, end.ColumnIndex);
            int maxCol = Math.Max(start.ColumnIndex, end.ColumnIndex);

            for (int r = minRow; r <= maxRow && r < rows.Count; r++)
            {
                if (!rows[r].Visible)
                {
                    continue;
                }

                for (int c = minCol; c <= maxCol && c < columns.Count; c++)
                {
                    if (!columns[c].Visible)
                    {
                        continue;
                    }

                    var cell = rows[r].Cells[c];
                    var address = new DataGridViewCellAddress(c, r);

                    if (!cell.Selected)
                    {
                        cell.Selected = true;
                        if (!selectedCells.Contains(address))
                        {
                            selectedCells.Add(address);
                        }
                        InvalidateCell(c, r);
                    }
                }
            }

            currentCell = end;
            OnSelectionChanged(EventArgs.Empty);
        }

        /// <summary>
        /// 选择行范围
        /// </summary>
        private void SelectRowRange(int startRowIndex, int endRowIndex, bool clearPrevious)
        {
            if (clearPrevious)
            {
                ClearSelection();
            }

            int minRow = Math.Min(startRowIndex, endRowIndex);
            int maxRow = Math.Max(startRowIndex, endRowIndex);

            for (int r = minRow; r <= maxRow && r < rows.Count; r++)
            {
                var row = rows[r];
                if (!row.Visible)
                {
                    continue;
                }

                if (!row.Selected)
                {
                    row.Selected = true;
                    if (!selectedRows.Contains(row))
                    {
                        selectedRows.Add(row);
                    }
                    InvalidateRow(row);
                }
            }

            if (columns.Count > 0)
            {
                currentCell = new DataGridViewCellAddress(0, endRowIndex);
            }

            OnSelectionChanged(EventArgs.Empty);
        }

        /// <summary>
        /// 切换单元格的选中状态(用于 Ctrl+点击多选)
        /// </summary>
        private void ToggleCellSelection(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || columnIndex >= columns.Count ||
                rowIndex < 0 || rowIndex >= rows.Count)
            {
                return;
            }

            var cell = rows[rowIndex].Cells[columnIndex];
            var address = new DataGridViewCellAddress(columnIndex, rowIndex);

            if (cell.Selected)
            {
                // 取消选中
                cell.Selected = false;
                selectedCells.Remove(address);
            }
            else
            {
                // 选中
                cell.Selected = true;
                if (!selectedCells.Contains(address))
                {
                    selectedCells.Add(address);
                }
            }

            currentCell = address;
            OnSelectionChanged(EventArgs.Empty);
            InvalidateCell(columnIndex, rowIndex);
        }

        /// <summary>
        /// 切换行的选中状态(用于 Ctrl+点击多选)
        /// </summary>
        private void ToggleRowSelection(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= rows.Count)
            {
                return;
            }

            var row = rows[rowIndex];

            if (row.Selected)
            {
                // 取消选中
                row.Selected = false;
                selectedRows.Remove(row);
            }
            else
            {
                // 选中
                row.Selected = true;
                if (!selectedRows.Contains(row))
                {
                    selectedRows.Add(row);
                }
            }

            // 更新当前单元格
            if (columns.Count > 0)
            {
                currentCell = new DataGridViewCellAddress(0, rowIndex);
            }

            OnSelectionChanged(EventArgs.Empty);
            InvalidateRow(row);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            // 如果有垂直滚动条且可见
            if (vScrollBar != null && vScrollBar.Visible)
            {
                // 计算滚动量
                // 每次滚动3行的高度
                int scrollAmount = DefaultRowHeight * 3;

                // 根据滚轮方向计算新的滚动位置
                int newValue = vScrollBar.Value;

                if (e.Delta > 0)
                {
                    // 向上滚动
                    newValue = Math.Max(vScrollBar.Minimum, vScrollBar.Value - scrollAmount);
                }
                else if (e.Delta < 0)
                {
                    // 向下滚动
                    int maxScroll = vScrollBar.Maximum - vScrollBar.LargeChange + 1;
                    newValue = Math.Min(maxScroll, vScrollBar.Value + scrollAmount);
                }

                // 应用新的滚动位置
                if (newValue != vScrollBar.Value)
                {
                    vScrollBar.Value = newValue;

                    // 更新第一个可见行
                    UpdateFirstVisibleRow();

                    // 刷新显示
                    Invalidate();
                }
            }
        }

        private void UpdateFirstVisibleRow()
        {
            if (!vScrollBar.Visible)
            {
                firstVisibleRowIndex = 0;
                return;
            }

            int y = 0;
            int scrollValue = vScrollBar.Value;

            firstVisibleRowIndex = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                if (!rows[i].Visible)
                {
                    continue;
                }

                if (y + rows[i].Height > scrollValue)
                {
                    firstVisibleRowIndex = i;
                    break;
                }
                y += rows[i].Height;
            }

            Invalidate();
            Update();
        }

        #endregion

        #region 列宽调整

        private void StartColumnResize(int columnIndex, int mouseX)
        {
            isResizingColumn = true;
            resizingColumnIndex = columnIndex;
            resizeStartX = mouseX;
            resizeStartWidth = columns[columnIndex].Width;
        }

        private void HandleColumnResize(int mouseX)
        {
            if (!isResizingColumn || resizingColumnIndex < 0)
            {
                return;
            }

            var column = columns[resizingColumnIndex];
            int delta = mouseX - resizeStartX;
            int newWidth = Math.Max(column.MinimumWidth, resizeStartWidth + delta);

            if (newWidth != column.Width)
            {
                column.Width = newWidth;
                PerformLayout();
                Invalidate();
            }
        }

        private void EndColumnResize()
        {
            isResizingColumn = false;
            resizingColumnIndex = -1;
        }

        private int GetColumnResizeIndex(Point location)
        {
            if (location.Y > columnHeaderHeight)
            {
                return -1;
            }

            int x = ShowRowHeader ? rowHeaderWidth : 0;
            x -= hScrollBar.Value;
            const int resizeMargin = 4;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (!column.Visible || !column.Resizable)
                {
                    x += column.Width;
                    continue;
                }

                // 检查是否在列的右边缘
                if (Math.Abs(location.X - (x + column.Width)) <= resizeMargin)
                {
                    return i;
                }

                x += column.Width;
            }

            return -1;
        }

        #endregion

        #region 键盘交互

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // 复制
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedCells();
                e.Handled = true;
                return;
            }

            // 粘贴
            if (e.Control && e.KeyCode == Keys.V && !ReadOnly)
            {
                PasteToSelectedCells();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);

            // 检查是否是多选模式
            bool isMultiSelect = e.Control;

            // 如果正在编辑，某些键由编辑控件处理
            if (editingCellAddress.ColumnIndex >= 0 && editingCellAddress.RowIndex >= 0)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        EndEdit();
                        MoveCurrentCell(0, 1);
                        e.Handled = true;
                        return;

                    case Keys.Escape:
                        CancelEdit();
                        e.Handled = true;
                        return;

                    case Keys.Tab:
                        if (e.Shift)
                        {
                            EndEdit();
                            MoveCurrentCell(-1, 0);
                        }
                        else
                        {
                            EndEdit();
                            MoveCurrentCell(1, 0);
                        }
                        e.Handled = true;
                        return;
                }
            }

            // 方向键导航
            switch (e.KeyCode)
            {
                case Keys.Home:
                    if (e.Control)
                    {
                        if (rows.Count > 0 && columns.Count > 0)
                        {
                            if (SelectionMode == DataGridViewSelectionMode.FullRowSelect)
                            {
                                SelectRow(0, !isMultiSelect);
                                EnsureRowVisible(0);
                            }
                            else
                            {
                                SelectCell(0, 0, !isMultiSelect);
                                EnsureCellVisible(0, 0);
                            }
                        }
                    }
                    else
                    {
                        if (currentCell.RowIndex >= 0 && columns.Count > 0)
                        {
                            SelectCell(0, currentCell.RowIndex, !isMultiSelect);
                            EnsureCellVisible(0, currentCell.RowIndex);
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.End:
                    if (e.Control)
                    {
                        if (rows.Count > 0 && columns.Count > 0)
                        {
                            if (SelectionMode == DataGridViewSelectionMode.FullRowSelect)
                            {
                                SelectRow(rows.Count - 1, !isMultiSelect);
                                EnsureRowVisible(rows.Count - 1);
                            }
                            else
                            {
                                SelectCell(columns.Count - 1, rows.Count - 1, !isMultiSelect);
                                EnsureCellVisible(columns.Count - 1, rows.Count - 1);
                            }
                        }
                    }
                    else
                    {
                        if (currentCell.RowIndex >= 0 && columns.Count > 0)
                        {
                            SelectCell(columns.Count - 1, currentCell.RowIndex, !isMultiSelect);
                            EnsureCellVisible(columns.Count - 1, currentCell.RowIndex);
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.Enter:
                    if (currentCell.ColumnIndex >= 0 && currentCell.RowIndex >= 0)
                    {
                        if (EditMode == DataGridViewEditMode.EditOnEnter)
                        {
                            BeginEdit(currentCell.ColumnIndex, currentCell.RowIndex);
                        }
                    }
                    e.Handled = true;
                    break;

                case Keys.F2:
                    if (EditMode == DataGridViewEditMode.EditOnF2 &&
                        currentCell.ColumnIndex >= 0 && currentCell.RowIndex >= 0)
                    {
                        BeginEdit(currentCell.ColumnIndex, currentCell.RowIndex);
                        e.Handled = true;
                    }
                    break;

                case Keys.Delete:
                    if (AllowUserToDeleteRows && selectedRows.Count > 0)
                    {
                        DeleteSelectedRows();
                        e.Handled = true;
                    }
                    break;

                case Keys.A:
                    // Ctrl+A: 全选
                    if (e.Control)
                    {
                        SelectAll();
                        e.Handled = true;
                    }
                    break;

                case Keys.Space:
                    // 空格键：切换选择状态
                    if (currentCell.RowIndex >= 0)
                    {
                        if (SelectionMode == DataGridViewSelectionMode.FullRowSelect)
                        {
                            ToggleRowSelection(currentCell.RowIndex);
                        }
                        else if (currentCell.ColumnIndex >= 0)
                        {
                            ToggleCellSelection(currentCell.ColumnIndex, currentCell.RowIndex);
                        }
                        e.Handled = true;
                    }
                    break;

                default:
                    // 其他按键触发编辑
                    if (EditMode == DataGridViewEditMode.EditOnKeystroke &&
                        !ReadOnly &&
                        editingCellAddress.ColumnIndex < 0 &&
                        currentCell.ColumnIndex >= 0 && currentCell.RowIndex >= 0 &&
                        !e.Control && !e.Alt &&
                        (char.IsLetterOrDigit((char)e.KeyCode) || e.KeyCode == Keys.Space))
                    {
                        BeginEdit(currentCell.ColumnIndex, currentCell.RowIndex);
                    }
                    break;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (vScrollBar != null && vScrollBar.Visible)
            {
                switch (keyData)
                {
                    case Keys.Up:
                        if (SelectionMode == DataGridViewSelectionMode.FullRowSelect)
                        {
                            MoveCurrentRow(-1);
                        }
                        else
                        {
                            MoveCurrentCell(0, -1);
                        }
                        break;

                    case Keys.Down:
                        if (SelectionMode == DataGridViewSelectionMode.FullRowSelect)
                        {
                            MoveCurrentRow(1);
                        }
                        else
                        {
                            MoveCurrentCell(0, 1);
                        }
                        break;

                    case Keys.Left:
                        MoveCurrentCell(-1, 0);
                        break;

                    case Keys.Right:
                        MoveCurrentCell(1, 0);
                        break;

                    case Keys.PageUp:
                        ScrollPage(true);
                        return true;

                    case Keys.PageDown:
                        ScrollPage(false);
                        return true;

                    case Keys.Home:
                        if (vScrollBar.Value != vScrollBar.Minimum)
                        {
                            vScrollBar.Value = vScrollBar.Minimum;
                            UpdateFirstVisibleRow();
                            Invalidate();
                        }
                        return true;

                    case Keys.End:
                        int maxScroll = vScrollBar.Maximum - vScrollBar.LargeChange + 1;
                        if (vScrollBar.Value != maxScroll)
                        {
                            vScrollBar.Value = maxScroll;
                            UpdateFirstVisibleRow();
                            Invalidate();
                        }
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// 移动当前行
        /// </summary>
        private void MoveCurrentRow(int delta, bool keepSelection = false)
        {
            if (rows.Count == 0)
            {
                return;
            }

            int currentRowIndex = -1;

            if (selectedRows.Count > 0)
            {
                currentRowIndex = selectedRows[selectedRows.Count - 1].Index;
            }
            else if (currentCell.RowIndex >= 0)
            {
                currentRowIndex = currentCell.RowIndex;
            }
            else
            {
                currentRowIndex = 0;
            }

            int newRowIndex = currentRowIndex + delta;

            while (newRowIndex >= 0 && newRowIndex < rows.Count && !rows[newRowIndex].Visible)
            {
                newRowIndex += delta;
            }

            if (newRowIndex >= 0 && newRowIndex < rows.Count)
            {
                SelectRow(newRowIndex, clearPrevious: !keepSelection);
                EnsureRowVisible(newRowIndex);

                if (currentCell.ColumnIndex >= 0)
                {
                    currentCell = new DataGridViewCellAddress(currentCell.ColumnIndex, newRowIndex);
                }
                else if (columns.Count > 0)
                {
                    currentCell = new DataGridViewCellAddress(0, newRowIndex);
                }
            }
        }

        /// <summary>
        /// 移动当前单元格
        /// </summary>
        private void MoveCurrentCell(int deltaX, int deltaY, bool keepSelection = false)
        {
            if (currentCell.ColumnIndex < 0 || currentCell.RowIndex < 0)
            {
                if (rows.Count > 0 && columns.Count > 0)
                {
                    SelectCell(0, 0, clearPrevious: true);
                    EnsureCellVisible(0, 0);
                }
                return;
            }

            if (editingCellAddress.ColumnIndex >= 0)
            {
                EndEdit();
            }

            int newCol = currentCell.ColumnIndex + deltaX;
            int newRow = currentCell.RowIndex + deltaY;

            while (newCol >= 0 && newCol < columns.Count && !columns[newCol].Visible)
            {
                newCol += (deltaX > 0 ? 1 : -1);
            }

            while (newRow >= 0 && newRow < rows.Count && !rows[newRow].Visible)
            {
                newRow += (deltaY > 0 ? 1 : -1);
            }

            if (newCol < 0 && newRow > 0)
            {
                newRow--;
                newCol = columns.Count - 1;
                while (newCol >= 0 && !columns[newCol].Visible)
                {
                    newCol--;
                }
            }
            else if (newCol >= columns.Count && newRow < rows.Count - 1)
            {
                newRow++;
                newCol = 0;
                while (newCol < columns.Count && !columns[newCol].Visible)
                {
                    newCol++;
                }
            }

            newCol = Math.Max(0, Math.Min(columns.Count - 1, newCol));
            newRow = Math.Max(0, Math.Min(rows.Count - 1, newRow));

            if (newCol >= 0 && newCol < columns.Count && newRow >= 0 && newRow < rows.Count)
            {
                if (SelectionMode == DataGridViewSelectionMode.FullRowSelect)
                {
                    SelectRow(newRow, clearPrevious: !keepSelection);
                    EnsureRowVisible(newRow);
                    currentCell = new DataGridViewCellAddress(newCol, newRow);
                }
                else
                {
                    SelectCell(newCol, newRow, clearPrevious: !keepSelection);
                    EnsureCellVisible(newCol, newRow);
                }
            }
        }

        /// <summary>
        /// 全选
        /// </summary>
        public void SelectAll()
        {
            if (SelectionMode == DataGridViewSelectionMode.FullRowSelect)
            {
                // 选择所有行
                ClearSelection();
                foreach (var row in rows)
                {
                    if (row.Visible)
                    {
                        row.Selected = true;
                        if (!selectedRows.Contains(row))
                        {
                            selectedRows.Add(row);
                        }
                    }
                }
            }
            else if (SelectionMode == DataGridViewSelectionMode.CellSelect)
            {
                // 选择所有单元格
                ClearSelection();
                for (int r = 0; r < rows.Count; r++)
                {
                    if (!rows[r].Visible)
                    {
                        continue;
                    }

                    for (int c = 0; c < columns.Count; c++)
                    {
                        if (!columns[c].Visible)
                        {
                            continue;
                        }

                        var cell = rows[r].Cells[c];
                        cell.Selected = true;
                        var address = new DataGridViewCellAddress(c, r);
                        if (!selectedCells.Contains(address))
                        {
                            selectedCells.Add(address);
                        }
                    }
                }
            }

            OnSelectionChanged(EventArgs.Empty);
            Invalidate();
        }

        private void EnsureRowVisible(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= rows.Count)
            {
                return;
            }

            if (!vScrollBar.Visible)
            {
                return;
            }

            // 计算该行的位置
            int y = 0;
            for (int i = 0; i < rowIndex; i++)
            {
                if (rows[i].Visible)
                {
                    y += rows[i].Height;
                }
            }

            var contentArea = GetContentArea();
            int rowHeight = rows[rowIndex].Height;
            int scrollValue = vScrollBar.Value;

            // 如果行在可见区域上方
            if (y < scrollValue)
            {
                vScrollBar.Value = y;
                UpdateFirstVisibleRow();
                Invalidate();
            }
            // 如果行在可见区域下方
            else if (y + rowHeight > scrollValue + contentArea.Height)
            {
                int maxScroll = vScrollBar.Maximum - vScrollBar.LargeChange + 1;
                vScrollBar.Value = Math.Min(maxScroll, y + rowHeight - contentArea.Height);
                UpdateFirstVisibleRow();
                Invalidate();
            }
        }

        private void EnsureCellVisible(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || columnIndex >= columns.Count || rowIndex < 0 || rowIndex >= rows.Count)
            {
                return;
            }

            // 确保行可见(垂直滚动)
            EnsureRowVisible(rowIndex);

            // 确保列可见(水平滚动)
            if (hScrollBar.Visible)
            {
                int x = 0;
                for (int i = 0; i < columnIndex; i++)
                {
                    if (columns[i].Visible)
                    {
                        x += columns[i].Width;
                    }
                }

                var contentArea = GetContentArea();
                int columnWidth = columns[columnIndex].Width;
                int scrollValue = hScrollBar.Value;

                // 如果列在可见区域左侧
                if (x < scrollValue)
                {
                    hScrollBar.Value = x;
                    Invalidate();
                }
                // 如果列在可见区域右侧
                else if (x + columnWidth > scrollValue + contentArea.Width)
                {
                    int maxScroll = hScrollBar.Maximum - hScrollBar.LargeChange + 1;
                    hScrollBar.Value = Math.Min(maxScroll, x + columnWidth - contentArea.Width);
                    Invalidate();
                }
            }
        }

        private void DeleteSelectedRows()
        {
            if (selectedRows.Count == 0)
            {
                return;
            }

            var rowsToDelete = selectedRows.ToList();
            foreach (var row in rowsToDelete)
            {
                rows.Remove(row);
            }

            ClearSelection();
            Invalidate();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // 处理Tab键在单元格间移动
            if (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift))
            {
                if (currentCell.ColumnIndex >= 0 && currentCell.RowIndex >= 0)
                {
                    int delta = (keyData & Keys.Shift) == Keys.Shift ? -1 : 1;
                    MoveCurrentCell(delta, 0);
                    return true;
                }
            }

            return base.ProcessDialogKey(keyData);
        }

        private void ScrollPage(bool up)
        {
            if (vScrollBar == null || !vScrollBar.Visible)
            {
                return;
            }

            int scrollAmount = vScrollBar.LargeChange;
            int newValue;

            if (up)
            {
                newValue = Math.Max(vScrollBar.Minimum, vScrollBar.Value - scrollAmount);
            }
            else
            {
                int maxScroll = vScrollBar.Maximum - vScrollBar.LargeChange + 1;
                newValue = Math.Min(maxScroll, vScrollBar.Value + scrollAmount);
            }

            if (newValue != vScrollBar.Value)
            {
                vScrollBar.Value = newValue;
                UpdateFirstVisibleRow();
                Invalidate();
            }
        }

        #endregion

        #region 分页方法

        private void CreatePagination()
        {
            if (pagination != null)
            {
                return;
            }

            pagination = new FluentPagination
            {
                Height = paginationHeight,
                Dock = DockStyle.None
            };

            // 继承主题
            if (UseTheme)
            {
                pagination.UseTheme = true;
                pagination.ThemeName = ThemeName;
            }

            // 绑定事件
            pagination.PageChanged += Pagination_PageChanged;

            Controls.Add(pagination);
            pagination.BringToFront();
        }

        private void RemovePagination()
        {
            if (pagination != null)
            {
                pagination.PageChanged -= Pagination_PageChanged;
                Controls.Remove(pagination);
                pagination.Dispose();
                pagination = null;

                // 恢复显示所有数据
                if (fullDataList != null)
                {
                    BindDataInternal(fullDataList, false);
                }
            }
        }

        private void Pagination_PageChanged(object sender, PageChangedEventArgs e)
        {
            if (fullDataList != null)
            {
                LoadPageData();
            }

            // 重新计算起始行号
            if (pagination != null)
            {
                startRowNumber = ((pagination.CurrentPage - 1) * pagination.PageSize + 1);
            }

            OnPageChanged(EventArgs.Empty);

            // 强制刷新显示
            Invalidate();
            Update();
        }

        private void LoadPageData()
        {
            if (pagination == null || fullDataList == null)
            {
                return;
            }

            var pageData = pagination.GetCurrentPageData();
            BindDataInternal(pageData, true);

            // 恢复行标记
            RestoreRowMarks();

            // 确保界面更新
            Invalidate();
            Update();
        }

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        public void FirstPage()
        {
            if (enablePagination && pagination != null)
            {
                pagination.GoToFirstPage();
            }
        }

        /// <summary>
        /// 上一页
        /// </summary>
        public void PreviousPage()
        {
            if (enablePagination && pagination != null)
            {
                pagination.GoToPreviousPage();
            }
        }

        /// <summary>
        /// 下一页
        /// </summary>
        public void NextPage()
        {
            if (enablePagination && pagination != null)
            {
                pagination.GoToNextPage();
            }
        }

        /// <summary>
        /// 末页
        /// </summary>
        public void LastPage()
        {
            if (enablePagination && pagination != null)
            {
                pagination.GoToLastPage();
            }
        }

        /// <summary>
        /// 配置分页控件
        /// </summary>
        public void ConfigurePagination(Action<FluentPagination> configure)
        {
            if (!enablePagination)
            {
                EnablePagination = true;
            }

            configure?.Invoke(pagination);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 刷新交替色
        /// </summary>
        private void RefreshAlternatingColors()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                if (AlternatingRowColors && i % 2 == 1)
                {
                    if (row.DefaultCellStyle == null)
                    {
                        row.DefaultCellStyle = new DataGridViewCellStyle();
                    }
                    row.DefaultCellStyle.BackColor = AlternatingRowColor;

                    foreach (var cell in row.Cells)
                    {
                        if (cell.Style == null)
                        {
                            cell.Style = new DataGridViewCellStyle();
                        }
                        cell.Style.BackColor = AlternatingRowColor;
                    }
                }
                else
                {
                    // 清除交替色
                    if (row.DefaultCellStyle != null)
                    {
                        row.DefaultCellStyle.BackColor = null;
                    }

                    foreach (var cell in row.Cells)
                    {
                        if (cell.Style != null)
                        {
                            cell.Style.BackColor = null;
                        }
                    }
                }
            }

            Invalidate();
        }

        private DataGridViewCellAddress GetCellAtPoint(Point location)
        {
            var contentArea = GetContentArea();

            // 检查是否在列头
            if (location.Y < contentArea.Top)
            {
                return new DataGridViewCellAddress(-1, -1);
            }

            // 检查是否在行头
            if (ShowRowHeader && location.X < rowHeaderWidth)
            {
                return new DataGridViewCellAddress(-1, -1);
            }

            int columnIndex = GetColumnIndexAtX(location.X);
            int rowIndex = GetRowIndexAtY(location.Y);

            return new DataGridViewCellAddress(columnIndex, rowIndex);
        }

        private int GetColumnIndexAtX(int x)
        {
            // 考虑行头
            int startX = ShowRowHeader ? rowHeaderWidth : 0;

            // 如果点击在行头内，返回-1
            if (x < startX)
            {
                return -1;
            }

            // 考虑水平滚动
            int scrollOffset = hScrollBar?.Value ?? 0;
            int currentX = startX - scrollOffset;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (!column.Visible)
                {
                    continue;
                }

                if (x >= currentX && x < currentX + column.Width)
                {
                    return i;
                }

                currentX += column.Width;
            }

            return -1;
        }

        private int GetRowIndexAtY(int y)
        {
            var contentArea = GetContentArea();

            // 检查是否在内容区域内
            if (y < contentArea.Top || y >= contentArea.Bottom)
            {
                return -1;
            }

            // 计算滚动偏移
            int scrollOffset = vScrollBar?.Value ?? 0;

            // 计算从第一个可见行开始的累积高度
            int accumulatedHeight = 0;
            for (int i = 0; i < firstVisibleRowIndex && i < rows.Count; i++)
            {
                if (rows[i].Visible)
                {
                    accumulatedHeight += rows[i].Height;
                }
            }

            // 调整起始Y位置
            int currentY = contentArea.Top - (scrollOffset - accumulatedHeight);

            // 从第一个可见行开始查找
            for (int i = firstVisibleRowIndex; i < rows.Count; i++)
            {
                var row = rows[i];
                if (!row.Visible)
                {
                    continue;
                }

                // 检查点击位置是否在当前行范围内
                if (y >= currentY && y < currentY + row.Height)
                {
                    return i;
                }

                currentY += row.Height;

                // 如果已经超出可见区域，停止查找
                if (currentY > contentArea.Bottom)
                {
                    break;
                }
            }

            return -1;
        }

        private Rectangle GetCellBounds(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || rowIndex < 0 ||
                columnIndex >= columns.Count || rowIndex >= rows.Count)
            {
                return Rectangle.Empty;
            }

            var contentArea = GetContentArea();

            // 计算X坐标
            int x = ShowRowHeader ? rowHeaderWidth : 0;
            int scrollOffsetH = hScrollBar?.Value ?? 0;
            x -= scrollOffsetH;

            for (int i = 0; i < columnIndex; i++)
            {
                if (columns[i].Visible)
                {
                    x += columns[i].Width;
                }
            }

            // 计算Y坐标
            int scrollOffsetV = vScrollBar?.Value ?? 0;

            // 计算从第一个可见行开始的累积高度
            int accumulatedHeight = 0;
            for (int i = 0; i < firstVisibleRowIndex && i < rows.Count; i++)
            {
                if (rows[i].Visible)
                {
                    accumulatedHeight += rows[i].Height;
                }
            }

            int y = contentArea.Top - (scrollOffsetV - accumulatedHeight);

            for (int i = firstVisibleRowIndex; i < rowIndex; i++)
            {
                if (rows[i].Visible)
                {
                    y += rows[i].Height;
                }
            }

            return new Rectangle(x, y, columns[columnIndex].Width, rows[rowIndex].Height);
        }

        private bool IsMouseOverColumnHeader(int columnIndex)
        {
            var mousePos = PointToClient(MousePosition);
            if (mousePos.Y >= columnHeaderHeight)
            {
                return false;
            }

            int x = ShowRowHeader ? rowHeaderWidth : 0;
            x -= hScrollBar.Value;

            for (int i = 0; i < columns.Count; i++)
            {
                if (!columns[i].Visible)
                {
                    continue;
                }

                if (i == columnIndex)
                {
                    return mousePos.X >= x && mousePos.X < x + columns[i].Width;
                }

                x += columns[i].Width;
            }

            return false;
        }

        /// <summary>
        /// 刷新指定单元格
        /// </summary>
        public void InvalidateCell(int columnIndex, int rowIndex)
        {
            var bounds = GetCellBounds(columnIndex, rowIndex);
            if (!bounds.IsEmpty)
            {
                Invalidate(bounds);
            }
        }

        /// <summary>
        /// 刷新指定单元格
        /// </summary>
        public void InvalidateCell(FluentDataGridViewCell cell)
        {
            if (cell != null)
            {
                InvalidateCell(cell.ColumnIndex, cell.RowIndex);
            }
        }

        /// <summary>
        /// 刷新指定行
        /// </summary>
        public void InvalidateRow(FluentDataGridViewRow row)
        {
            if (row != null && row.Index >= 0)
            {
                Invalidate();
                Update();
            }
        }

        /// <summary>
        /// 刷新指定列
        /// </summary>
        public void InvalidateColumn(FluentDataGridViewColumn column)
        {
            if (column != null && column.Index >= 0)
            {
                Invalidate();
            }
        }

        #endregion

        #region 事件处理

        internal void OnCellValueChanged(DataGridViewCellValueEventArgs e)
        {
            CellValueChanged?.Invoke(this, e);
        }

        protected virtual void OnCellClick(DataGridViewCellEventArgs e)
        {
            CellClick?.Invoke(this, e);
        }

        protected virtual void OnCellDoubleClick(DataGridViewCellEventArgs e)
        {
            CellDoubleClick?.Invoke(this, e);
        }

        protected virtual void OnColumnHeaderClick(DataGridViewSortEventArgs e)
        {
            ColumnHeaderClick?.Invoke(this, e);
        }

        protected virtual void OnSorting(DataGridViewSortEventArgs e)
        {
            Sorting?.Invoke(this, e);
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            DataSourceChanged?.Invoke(this, e);
        }

        protected virtual void OnPageChanged(EventArgs e)
        {
            PageChanged?.Invoke(this, e);
        }

        internal void OnColumnAdded(FluentDataGridViewColumn column)
        {
            // 设置列的容器
            if (column.Site == null && this.Site != null)
            {
                IDesignerHost host = this.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null && host.Container != null)
                {
                    // 列已经是Component，会自动添加到容器
                }
            }

            // 为现有行添加新单元格
            foreach (var row in rows)
            {
                var cell = column.CreateCell();
                cell.OwningRow = row;
                cell.OwningColumn = column;
                row.Cells.Add(cell);
            }

            PerformLayout();
            Invalidate();
        }

        internal void OnColumnRemoved(FluentDataGridViewColumn column)
        {
            // 从所有行中移除对应的单元格
            foreach (var row in rows)
            {
                if (column.Index < row.Cells.Count)
                {
                    row.Cells[column.Index].Dispose();
                    row.Cells.RemoveAt(column.Index);
                }
            }

            PerformLayout();
            Invalidate();
        }

        internal void OnColumnsCleared()
        {
            rows.Clear();
            PerformLayout();
            Invalidate();
        }

        internal void OnColumnChanged()
        {
            PerformLayout();
            Invalidate();
        }

        internal void OnColumnWidthChanged(FluentDataGridViewColumn column)
        {
            PerformLayout();
            Invalidate();
        }

        internal void OnRowAdded(FluentDataGridViewRow row)
        {
            PerformLayout();
            Invalidate();
        }

        internal void OnRowRemoved(FluentDataGridViewRow row)
        {
            PerformLayout();
            Invalidate();
        }

        internal void OnRowsCleared()
        {
            ClearSelection();
            PerformLayout();
            Invalidate();
        }

        internal void OnRowChanged()
        {
            Invalidate();
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EndEdit();

                rowMarksByDataItem?.Clear();

                foreach (var row in rows)
                {
                    row?.Dispose();
                }

                foreach (var column in columns)
                {
                    column?.Dispose();
                }

                vScrollBar?.Dispose();
                hScrollBar?.Dispose();

                if (pagination != null)
                {
                    pagination.PageChanged -= Pagination_PageChanged;
                    pagination.Dispose();
                }

                if (contextMenu != null)
                {
                    contextMenu.Opening -= ContextMenu_Opening;
                    if (menuItemCopy != null)
                    {
                        menuItemCopy.Click -= MenuItemCopy_Click;
                    }

                    if (menuItemPaste != null)
                    {
                        menuItemPaste.Click -= MenuItemPaste_Click;
                    }

                    if (menuItemExportCsv != null)
                    {
                        menuItemExportCsv.Click -= MenuItemExportCsv_Click;
                    }

                    contextMenu.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    #region 数据表格相关基类

    #region 单元格

    /// <summary>
    /// Fluent数据网格单元格
    /// </summary>
    public abstract class FluentDataGridViewCell : IDisposable
    {
        private object value;
        private bool isSelected;
        private bool isEditing;
        private Control editingControl;

        #region 属性

        /// <summary>
        /// 所属行
        /// </summary>
        public FluentDataGridViewRow OwningRow { get; internal set; }

        /// <summary>
        /// 所属列
        /// </summary>
        public FluentDataGridViewColumn OwningColumn { get; internal set; }

        /// <summary>
        /// 所属DataGridView
        /// </summary>
        public FluentDataGridView DataGridView => OwningRow?.DataGridView;

        /// <summary>
        /// 列索引
        /// </summary>
        public int ColumnIndex => OwningColumn?.Index ?? -1;

        /// <summary>
        /// 行索引
        /// </summary>
        public int RowIndex => OwningRow?.Index ?? -1;

        /// <summary>
        /// 单元格值
        /// </summary>
        public virtual object Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    var oldValue = this.value;
                    this.value = value;
                    OnValueChanged(oldValue, value);
                }
            }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Selected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    DataGridView?.InvalidateCell(this);
                }
            }
        }

        /// <summary>
        /// 是否正在编辑
        /// </summary>
        public bool IsInEditMode
        {
            get => isEditing;
            internal set => isEditing = value;
        }

        /// <summary>
        /// 单元格样式
        /// </summary>
        public DataGridViewCellStyle Style { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        public virtual bool ReadOnly { get; set; }

        /// <summary>
        /// 格式化的值
        /// </summary>
        public virtual string FormattedValue
        {
            get
            {
                if (Value == null)
                {
                    return string.Empty;
                }

                if (!string.IsNullOrEmpty(Style?.Format))
                {
                    try
                    {
                        return string.Format(Style.Format, Value);
                    }
                    catch
                    {
                        return Value.ToString();
                    }
                }

                return Value.ToString();
            }
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 绘制单元格
        /// </summary>
        public abstract void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state);

        /// <summary>
        /// 初始化编辑控件
        /// </summary>
        public abstract Control CreateEditingControl();

        /// <summary>
        /// 准备编辑控件
        /// </summary>
        public abstract void PrepareEditingControl(Control editingControl);

        /// <summary>
        /// 从编辑控件获取值
        /// </summary>
        public abstract object GetEditingControlValue(Control editingControl);

        /// <summary>
        /// 克隆单元格
        /// </summary>
        public abstract FluentDataGridViewCell Clone();

        #endregion

        #region 虚方法

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            DataGridView?.OnCellValueChanged(new DataGridViewCellValueEventArgs(
                ColumnIndex, RowIndex, oldValue, newValue));
        }

        /// <summary>
        /// 进入编辑模式
        /// </summary>
        public virtual bool BeginEdit()
        {
            if (ReadOnly || IsInEditMode)
            {
                return false;
            }

            editingControl = CreateEditingControl();
            if (editingControl == null)
            {
                return false;
            }

            PrepareEditingControl(editingControl);
            IsInEditMode = true;
            return true;
        }

        /// <summary>
        /// 结束编辑模式
        /// </summary>
        public virtual bool EndEdit()
        {
            if (!IsInEditMode || editingControl == null)
            {
                return false;
            }

            try
            {
                Value = GetEditingControlValue(editingControl);
                return true;
            }
            finally
            {
                IsInEditMode = false;
                editingControl?.Dispose();
                editingControl = null;
            }
        }

        /// <summary>
        /// 取消编辑
        /// </summary>
        public virtual void CancelEdit()
        {
            if (IsInEditMode)
            {
                IsInEditMode = false;
                editingControl?.Dispose();
                editingControl = null;
            }
        }

        #endregion

        /// <summary>
        /// 获取有效的单元格样式(考虑继承)
        /// </summary>
        public DataGridViewCellStyle GetEffectiveStyle()
        {
            var effectiveStyle = new DataGridViewCellStyle();

            // 1. 首先应用列的默认样式
            if (OwningColumn?.DefaultCellStyle != null)
            {
                MergeStyle(effectiveStyle, OwningColumn.DefaultCellStyle);
            }

            // 2. 然后应用行的默认样式
            if (OwningRow?.DefaultCellStyle != null)
            {
                MergeStyle(effectiveStyle, OwningRow.DefaultCellStyle);
            }

            // 3. 最后应用单元格自己的样式
            if (Style != null)
            {
                MergeStyle(effectiveStyle, Style);
            }

            return effectiveStyle;
        }

        private void MergeStyle(DataGridViewCellStyle target, DataGridViewCellStyle source)
        {
            if (source.BackColor.HasValue && !target.BackColor.HasValue)
            {
                target.BackColor = source.BackColor;
            }

            if (source.ForeColor.HasValue && !target.ForeColor.HasValue)
            {
                target.ForeColor = source.ForeColor;
            }

            if (source.SelectionBackColor.HasValue && !target.SelectionBackColor.HasValue)
            {
                target.SelectionBackColor = source.SelectionBackColor;
            }

            if (source.SelectionForeColor.HasValue && !target.SelectionForeColor.HasValue)
            {
                target.SelectionForeColor = source.SelectionForeColor;
            }

            if (source.Font != null && target.Font == null)
            {
                target.Font = source.Font;
            }

            // 其他属性根据需要合并
            if (target.Alignment == StringAlignment.Near && source.Alignment != StringAlignment.Near)
            {
                target.Alignment = source.Alignment;
            }

            if (target.LineAlignment == StringAlignment.Near && source.LineAlignment != StringAlignment.Near)
            {
                target.LineAlignment = source.LineAlignment;
            }
        }

        public void Dispose()
        {
            editingControl?.Dispose();
            editingControl = null;
        }
    }

    /// <summary>
    /// 单元格状态
    /// </summary>
    [Flags]
    public enum DataGridViewCellState
    {
        None = 0,
        Selected = 1,
        Hovered = 2,
        Editing = 4,
        ReadOnly = 8
    }

    /// <summary>
    /// 单元格样式
    /// </summary>
    public class DataGridViewCellStyle
    {
        public Color? BackColor { get; set; }
        public Color? ForeColor { get; set; }
        public Color? SelectionBackColor { get; set; }
        public Color? SelectionForeColor { get; set; }
        public Font Font { get; set; }
        public StringAlignment Alignment { get; set; } = StringAlignment.Near;
        public StringAlignment LineAlignment { get; set; } = StringAlignment.Center;
        public string Format { get; set; }
        public Padding Padding { get; set; } = new Padding(4);

        public DataGridViewCellStyle Clone()
        {
            return new DataGridViewCellStyle
            {
                BackColor = BackColor,
                ForeColor = ForeColor,
                SelectionBackColor = SelectionBackColor,
                SelectionForeColor = SelectionForeColor,
                Font = Font,
                Alignment = Alignment,
                LineAlignment = LineAlignment,
                Format = Format,
                Padding = Padding
            };
        }
    }

    #endregion

    #region 列

    /// <summary>
    /// Fluent数据网格列
    /// </summary>
    [ToolboxItem(false)]
    public abstract class FluentDataGridViewColumn : Component
    {
        private int width = 100;
        private float widthPercentage = 0;
        private DataGridViewColumnWidthMode widthMode = DataGridViewColumnWidthMode.Fixed;
        private bool visible = true;
        private SortOrder sortOrder = SortOrder.None;

        #region 属性

        /// <summary>
        /// 所属DataGridView
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FluentDataGridView DataGridView { get; internal set; }

        /// <summary>
        /// 列索引
        /// </summary>
        [Browsable(false)]
        public int Index { get; internal set; } = -1;

        /// <summary>
        /// 列名
        /// </summary>
        [Category("设计")]
        [Description("列的名称")]
        public string Name
        {
            get => Site?.Name ?? base.GetType().Name;
            set
            {
                if (Site != null)
                {
                    Site.Name = value;
                }
            }
        }

        /// <summary>
        /// 数据属性名
        /// </summary>
        [Category("数据")]
        [Description("绑定到数据源的属性名")]
        [DefaultValue("")]
        public string DataPropertyName { get; set; }

        /// <summary>
        /// 列头文本
        /// </summary>
        [Category("外观")]
        [Description("列头显示的文本")]
        [DefaultValue("")]
        public string HeaderText { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        [Category("布局")]
        [DefaultValue(100)]
        [Description("列的宽度")]
        public int Width
        {
            get => width;
            set
            {
                if (width != value && value >= MinimumWidth)
                {
                    width = value;
                    DataGridView?.OnColumnWidthChanged(this);
                }
            }
        }

        /// <summary>
        /// 宽度百分比(当WidthMode为Percentage时使用)
        /// </summary>
        [Category("布局")]
        [DefaultValue(0f)]
        [Description("宽度百分比(当宽度模式为百分比时有效)")]
        public float WidthPercentage
        {
            get => widthPercentage;
            set
            {
                value = Math.Max(0, Math.Min(100, value));
                if (widthPercentage != value)
                {
                    widthPercentage = value;
                    if (WidthMode == DataGridViewColumnWidthMode.Percentage)
                    {
                        DataGridView?.OnColumnWidthChanged(this);
                    }
                }
            }
        }

        /// <summary>
        /// 最小宽度
        /// </summary>
        [Category("布局")]
        [DefaultValue(20)]
        [Description("列的最小宽度")]
        public int MinimumWidth { get; set; } = 20;

        /// <summary>
        /// 宽度模式
        /// </summary>
        [Category("布局")]
        [DefaultValue(DataGridViewColumnWidthMode.Fixed)]
        [Description("列宽度的调整模式")]
        public DataGridViewColumnWidthMode WidthMode
        {
            get => widthMode;
            set
            {
                if (widthMode != value)
                {
                    widthMode = value;
                    DataGridView?.OnColumnWidthChanged(this);
                }
            }
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        [Category("外观")]
        [DefaultValue(true)]
        [Description("列是否可见")]
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;
                    DataGridView?.PerformLayout();
                    DataGridView?.Invalidate();
                }
            }
        }

        /// <summary>
        /// 对齐方式
        /// </summary>
        [Category("外观")]
        [DefaultValue(HorizontalAlignment.Left)]
        [Description("列头文本对齐方式")]
        public HorizontalAlignment HeaderTextAlign { get; set; } = HorizontalAlignment.Left;

        /// <summary>
        /// 单元格对齐方式
        /// </summary>
        [Category("外观")]
        [DefaultValue(HorizontalAlignment.Left)]
        [Description("单元格内容对齐方式")]
        public HorizontalAlignment CellAlignment { get; set; } = HorizontalAlignment.Left;

        /// <summary>
        /// 排序模式
        /// </summary>
        [Category("行为")]
        [DefaultValue(DataGridViewColumnSortMode.Automatic)]
        [Description("列的排序模式")]
        public DataGridViewColumnSortMode SortMode { get; set; } = DataGridViewColumnSortMode.Automatic;

        /// <summary>
        /// 当前排序顺序
        /// </summary>
        [Browsable(false)]
        public SortOrder SortOrder
        {
            get => sortOrder;
            internal set
            {
                if (sortOrder != value)
                {
                    sortOrder = value;
                    DataGridView?.InvalidateColumn(this);
                }
            }
        }

        /// <summary>
        /// 是否可调整大小
        /// </summary>
        [Category("行为")]
        [DefaultValue(true)]
        [Description("用户是否可以调整列宽")]
        public bool Resizable { get; set; } = true;

        /// <summary>
        /// 是否只读
        /// </summary>
        [Category("行为")]
        [DefaultValue(false)]
        [Description("列是否只读")]
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// 是否显示过滤器
        /// </summary>
        [Category("行为")]
        [DefaultValue(false)]
        [Description("是否显示列过滤器")]
        public bool ShowFilter { get; set; } = false;

        /// <summary>
        /// 默认单元格样式
        /// </summary>
        [Category("外观")]
        [Description("列的默认单元格样式")]
        public DataGridViewCellStyle DefaultCellStyle { get; set; }

        /// <summary>
        /// 列头样式
        /// </summary>
        [Category("外观")]
        [Description("列头的样式")]
        public DataGridViewCellStyle HeaderStyle { get; set; }

        #endregion

        protected FluentDataGridViewColumn()
        {
            DefaultCellStyle = new DataGridViewCellStyle();
            HeaderStyle = new DataGridViewCellStyle();
        }

        #region 抽象方法

        /// <summary>
        /// 创建单元格
        /// </summary>
        public abstract FluentDataGridViewCell CreateCell();

        /// <summary>
        /// 克隆列
        /// </summary>
        public abstract FluentDataGridViewColumn Clone();

        #endregion

        #region 虚方法

        /// <summary>
        /// 获取单元格的值
        /// </summary>
        public virtual object GetCellValue(object dataSource)
        {
            if (dataSource == null || string.IsNullOrEmpty(DataPropertyName))
            {
                return null;
            }

            try
            {
                var property = dataSource.GetType().GetProperty(DataPropertyName);
                return property?.GetValue(dataSource);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 设置单元格的值
        /// </summary>
        public virtual void SetCellValue(object dataSource, object value)
        {
            if (dataSource == null || string.IsNullOrEmpty(DataPropertyName))
            {
                return;
            }

            try
            {
                var property = dataSource.GetType().GetProperty(DataPropertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(dataSource, value);
                }
            }
            catch
            {
                // 忽略设置失败
            }
        }

        public override string ToString()
        {
            var name = !string.IsNullOrEmpty(Name) ? Name : GetType().Name;
            var header = !string.IsNullOrEmpty(HeaderText) ? $" ({HeaderText})" : "";
            return name + header;
        }

        #endregion

        #region Component重写

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 清理资源
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #endregion

    #region 行

    /// <summary>
    /// Fluent数据网格行
    /// </summary>
    public class FluentDataGridViewRow : IDisposable
    {
        private List<FluentDataGridViewCell> cells;
        private int height = 32;
        private bool isSelected;
        private bool visible = true;
        private RowMark mark;
        private Dictionary<string, object> tags; // 用于存储自定义数据

        #region 属性

        /// <summary>
        /// 所属DataGridView
        /// </summary>
        public FluentDataGridView DataGridView { get; internal set; }

        /// <summary>
        /// 行索引
        /// </summary>
        public int Index { get; internal set; } = -1;

        /// <summary>
        /// 单元格集合
        /// </summary>
        public List<FluentDataGridViewCell> Cells => cells;

        /// <summary>
        /// 行高
        /// </summary>
        public int Height
        {
            get => height;
            set
            {
                if (height != value && value > 0)
                {
                    height = value;
                    DataGridView?.PerformLayout();
                }
            }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Selected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;

                    // 更新所有单元格的选中状态
                    foreach (var cell in cells)
                    {
                        cell.Selected = value;
                    }

                    DataGridView?.InvalidateRow(this);
                }
            }
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;
                    DataGridView?.PerformLayout();
                }
            }
        }

        /// <summary>
        /// 数据源对象
        /// </summary>
        public object DataBoundItem { get; internal set; }

        /// <summary>
        /// 行样式
        /// </summary>
        public DataGridViewCellStyle DefaultCellStyle { get; set; }

        /// <summary>
        /// 行标记
        /// </summary>
        public RowMark Mark
        {
            get => mark;
            set
            {
                if (mark != value)
                {
                    mark = value;
                    DataGridView?.InvalidateRow(this);
                }
            }
        }

        /// <summary>
        /// 是否有标记
        /// </summary>
        public bool HasMark => mark != null && mark.MarkType != RowMarkType.None;

        /// <summary>
        /// 自定义标签集合
        /// </summary>
        public Dictionary<string, object> Tags
        {
            get
            {
                if (tags == null)
                {
                    tags = new Dictionary<string, object>();
                }

                return tags;
            }
        }

        #endregion

        public FluentDataGridViewRow()
        {
            cells = new List<FluentDataGridViewCell>();
            DefaultCellStyle = new DataGridViewCellStyle();
        }

        /// <summary>
        /// 创建单元格
        /// </summary>
        internal void CreateCells(FluentDataGridView dataGridView)
        {
            cells.Clear();

            foreach (var column in dataGridView.Columns)
            {
                if (column != null)
                {
                    var cell = column.CreateCell();
                    cell.OwningRow = this;
                    cell.OwningColumn = column;

                    // 应用列的默认样式
                    if (column.DefaultCellStyle != null)
                    {
                        cell.Style = column.DefaultCellStyle.Clone();
                    }

                    cells.Add(cell);
                }
            }
        }

        /// <summary>
        /// 设置标记
        /// </summary>
        public void SetMark(RowMarkType markType, RowMarkPosition position = RowMarkPosition.RowHeader, int targetColumnIndex = -1)
        {
            Mark = new RowMark(markType, position)
            {
                TargetColumnIndex = targetColumnIndex
            };
        }

        /// <summary>
        /// 清除标记
        /// </summary>
        public void ClearMark()
        {
            Mark = null;
        }

        /// <summary>
        /// 应用样式
        /// </summary>
        public void ApplyStyle(DataGridViewCellStyle style)
        {
            if (style == null)
            {
                return;
            }

            if (style.BackColor.HasValue)
            {
                DefaultCellStyle.BackColor = style.BackColor;
            }

            if (style.ForeColor.HasValue)
            {
                DefaultCellStyle.ForeColor = style.ForeColor;
            }

            if (style.Font != null)
            {
                DefaultCellStyle.Font = style.Font;
            }

            if (style.SelectionBackColor.HasValue)
            {
                DefaultCellStyle.SelectionBackColor = style.SelectionBackColor;
            }

            if (style.SelectionForeColor.HasValue)
            {
                DefaultCellStyle.SelectionForeColor = style.SelectionForeColor;
            }

            // 应用到所有单元格
            foreach (var cell in cells)
            {
                if (cell.Style == null)
                {
                    cell.Style = new DataGridViewCellStyle();
                }

                if (style.BackColor.HasValue)
                {
                    cell.Style.BackColor = style.BackColor;
                }

                if (style.ForeColor.HasValue)
                {
                    cell.Style.ForeColor = style.ForeColor;
                }

                if (style.Font != null)
                {
                    cell.Style.Font = style.Font;
                }
            }

            DataGridView?.InvalidateRow(this);
        }

        public void Dispose()
        {
            foreach (var cell in cells)
            {
                cell?.Dispose();
            }
            cells.Clear();
            tags?.Clear();
        }
    }

    #endregion

    #region 行标记

    /// <summary>
    /// 行标记类型
    /// </summary>
    public enum RowMarkType
    {
        None,       // 无标记
        New,        // 新增数据
        Important,  // 重要数据
        Error,      // 错误数据
        Warning,    // 警告数据
        Question,   // 可疑数据
        Success,    // 成功/完成
        Custom      // 自定义
    }

    /// <summary>
    /// 行标记位置
    /// </summary>
    public enum RowMarkPosition
    {
        RowHeader,      // 行头
        CellTopLeft     // 单元格左上角
    }

    /// <summary>
    /// 行标记
    /// </summary>
    public class RowMark
    {

        public RowMark(RowMarkType markType, RowMarkPosition position = RowMarkPosition.RowHeader)
        {
            MarkType = markType;
            Position = position;

            // 设置默认颜色
            switch (markType)
            {
                case RowMarkType.New:
                    MarkColor = Color.FromArgb(0, 120, 215); // 蓝色
                    BackColor = Color.FromArgb(230, 243, 255);
                    break;
                case RowMarkType.Important:
                    MarkColor = Color.FromArgb(255, 140, 0); // 橙色
                    BackColor = Color.FromArgb(255, 244, 229);
                    break;
                case RowMarkType.Error:
                    MarkColor = Color.FromArgb(232, 17, 35); // 红色
                    BackColor = Color.FromArgb(253, 231, 233);
                    break;
                case RowMarkType.Warning:
                    MarkColor = Color.FromArgb(255, 185, 0); // 黄色
                    BackColor = Color.FromArgb(255, 251, 230);
                    break;
                case RowMarkType.Question:
                    MarkColor = Color.FromArgb(138, 43, 226); // 紫色
                    BackColor = Color.FromArgb(243, 233, 253);
                    break;
                case RowMarkType.Success:
                    MarkColor = Color.FromArgb(16, 124, 16); // 绿色
                    BackColor = Color.FromArgb(223, 246, 221);
                    break;
                default:
                    MarkColor = Color.Gray;
                    BackColor = Color.Transparent;
                    break;
            }
        }

        /// <summary>
        /// 标记类型
        /// </summary>
        public RowMarkType MarkType { get; set; }

        /// <summary>
        /// 标记位置
        /// </summary>
        public RowMarkPosition Position { get; set; }

        /// <summary>
        /// 目标列索引(当Position为CellTopLeft时有效)
        /// </summary>
        public int TargetColumnIndex { get; set; } = -1;

        /// <summary>
        /// 自定义图标
        /// </summary>
        public Image CustomIcon { get; set; }

        /// <summary>
        /// 自定义文本
        /// </summary>
        public string CustomText { get; set; }

        /// <summary>
        /// 标记颜色
        /// </summary>
        public Color MarkColor { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color BackColor { get; set; }

        /// <summary>
        /// 提示文本
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// 标记大小
        /// </summary>
        public Size MarkSize { get; set; } = new Size(16, 16);


        /// <summary>
        /// 获取标记文本
        /// </summary>
        public string GetMarkText()
        {
            if (!string.IsNullOrEmpty(CustomText))
            {
                return CustomText;
            }

            switch (MarkType)
            {
                case RowMarkType.New:
                    return "N";
                case RowMarkType.Important:
                    return "i";
                case RowMarkType.Error:
                    return "!";
                case RowMarkType.Warning:
                    return "⚠";
                case RowMarkType.Question:
                    return "?";
                case RowMarkType.Success:
                    return "✓";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 绘制标记
        /// </summary>
        public void Draw(Graphics g, Rectangle bounds)
        {
            if (MarkType == RowMarkType.None)
            {
                return;
            }

            // 如果有自定义图标
            if (CustomIcon != null)
            {
                var iconRect = new Rectangle(
                    bounds.X + (bounds.Width - MarkSize.Width) / 2,
                    bounds.Y + (bounds.Height - MarkSize.Height) / 2,
                    MarkSize.Width,
                    MarkSize.Height);
                g.DrawImage(CustomIcon, iconRect);
                return;
            }

            // 绘制圆形背景
            var markRect = new Rectangle(
                bounds.X + (bounds.Width - MarkSize.Width) / 2,
                bounds.Y + (bounds.Height - MarkSize.Height) / 2,
                MarkSize.Width,
                MarkSize.Height);

            using (var brush = new SolidBrush(MarkColor))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillEllipse(brush, markRect);
            }

            // 绘制文本
            var text = GetMarkText();
            if (!string.IsNullOrEmpty(text))
            {
                using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(text, font, brush, markRect, format);
                }
            }
        }
    }

    /// <summary>
    /// 行样式过滤器
    /// </summary>
    public class RowStyleFilter
    {
        public RowStyleFilter(Func<FluentDataGridViewRow, bool> predicate, DataGridViewCellStyle style, int priority = 0)
        {
            Predicate = predicate;
            Style = style;
            Priority = priority;
        }

        /// <summary>
        /// 过滤条件
        /// </summary>
        public Func<FluentDataGridViewRow, bool> Predicate { get; set; }

        /// <summary>
        /// 要应用的样式
        /// </summary>
        public DataGridViewCellStyle Style { get; set; }

        /// <summary>
        /// 优先级(数值越大优先级越高)
        /// </summary>
        public int Priority { get; set; }

    }

    #endregion

    #endregion

    #region 数据列实现

    #region FluentDataGridViewTextBoxColumn

    /// <summary>
    /// FluentTextBox单元格
    /// </summary>
    public class FluentDataGridViewTextBoxCell : FluentDataGridViewCell
    {
        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // 使用有效样式
            var style = GetEffectiveStyle();

            var backColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);

            var foreColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionForeColor ?? Color.White)
                : (style.ForeColor ?? Color.Black);

            // 绘制背景
            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 绘制悬停效果
            if (state.HasFlag(DataGridViewCellState.Hovered) && !state.HasFlag(DataGridViewCellState.Selected))
            {
                using (var brush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // 绘制标记(如果在单元格左上角)
            if (OwningRow?.Mark != null &&
                OwningRow.Mark.Position == RowMarkPosition.CellTopLeft &&
                OwningRow.Mark.TargetColumnIndex == ColumnIndex)
            {
                var markBounds = new Rectangle(bounds.X + 2, bounds.Y + 2, 16, 16);
                OwningRow.Mark.Draw(g, markBounds);
            }

            // 绘制文本
            var text = FormattedValue;
            if (!string.IsNullOrEmpty(text))
            {
                var padding = style.Padding;
                var textBounds = new Rectangle(
                    bounds.X + padding.Left,
                    bounds.Y + padding.Top,
                    bounds.Width - padding.Horizontal,
                    bounds.Height - padding.Vertical);

                var font = style.Font ?? OwningColumn?.DataGridView?.Font ?? SystemFonts.DefaultFont;

                using (var brush = new SolidBrush(foreColor))
                using (var format = new StringFormat
                {
                    Alignment = GetStringAlignment(OwningColumn?.CellAlignment ?? HorizontalAlignment.Left),
                    LineAlignment = style.LineAlignment,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                })
                {
                    g.DrawString(text, font, brush, textBounds, format);
                }
            }
        }

        public override Control CreateEditingControl()
        {
            var textBox = new FluentTextBox
            {
                CustomBorderStyle = BorderStyle.None,
                BorderSize = 0,
                Margin = new Padding(0)
            };

            // 继承DataGridView的主题
            if (DataGridView != null && DataGridView.UseTheme)
            {
                textBox.UseTheme = true;
                textBox.ThemeName = DataGridView.ThemeName;
            }

            return textBox;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
            if (editingControl is FluentTextBox textBox)
            {
                textBox.Text = FormattedValue;
                textBox.SelectAll();
            }
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            if (editingControl is FluentTextBox textBox)
            {
                return textBox.Text;
            }
            return null;
        }

        public override FluentDataGridViewCell Clone()
        {
            var cell = new FluentDataGridViewTextBoxCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = this.ReadOnly
            };
            return cell;
        }

        private StringAlignment GetStringAlignment(HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    return StringAlignment.Near;
                case HorizontalAlignment.Center:
                    return StringAlignment.Center;
                case HorizontalAlignment.Right:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Near;
            }
        }
    }

    /// <summary>
    /// FluentTextBox列
    /// </summary>
    public class FluentDataGridViewTextBoxColumn : FluentDataGridViewColumn
    {
        /// <summary>
        /// 最大长度
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(32767)]
        public int MaxLength { get; set; } = 32767;

        public override FluentDataGridViewCell CreateCell()
        {
            return new FluentDataGridViewTextBoxCell();
        }

        public override FluentDataGridViewColumn Clone()
        {
            var column = new FluentDataGridViewTextBoxColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = this.ReadOnly,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                MaxLength = this.MaxLength,
                HeaderTextAlign = this.HeaderTextAlign,
                CellAlignment = this.CellAlignment,
                Resizable = this.Resizable,
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
            return column;
        }
    }

    #endregion

    #region FluentDataGridViewLabelColumn

    /// <summary>
    /// FluentLabel单元格
    /// </summary>
    public class FluentDataGridViewLabelCell : FluentDataGridViewCell
    {
        private LabelShape shape = LabelShape.Rectangle;
        private bool showBorder = false;
        private Color borderColor = Color.Gray;

        public LabelShape Shape
        {
            get => shape;
            set => shape = value;
        }

        public bool ShowBorder
        {
            get => showBorder;
            set => showBorder = value;
        }

        public Color BorderColor
        {
            get => borderColor;
            set => borderColor = value;
        }

        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var style = GetEffectiveStyle();
            var backColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);
            var foreColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionForeColor ?? Color.White)
                : (style.ForeColor ?? Color.Black);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 添加一些内边距
            var labelBounds = new Rectangle(
                bounds.X + 4,
                bounds.Y + 2,
                bounds.Width - 8,
                bounds.Height - 4);

            // 绘制标签背景
            DrawLabelBackground(g, labelBounds, backColor);

            // 绘制标签边框
            if (showBorder)
            {
                DrawLabelBorder(g, labelBounds, borderColor);
            }

            // 绘制文本
            var text = FormattedValue;
            if (!string.IsNullOrEmpty(text))
            {
                var textBounds = new Rectangle(
                    labelBounds.X + 4,
                    labelBounds.Y,
                    labelBounds.Width - 8,
                    labelBounds.Height);

                using (var brush = new SolidBrush(foreColor))
                using (var format = new StringFormat
                {
                    Alignment = style.Alignment,
                    LineAlignment = style.LineAlignment,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    var font = style.Font ?? OwningColumn?.DataGridView?.Font ?? SystemFonts.DefaultFont;
                    g.DrawString(text, font, brush, textBounds, format);
                }
            }
        }

        private void DrawLabelBackground(Graphics g, Rectangle bounds, Color color)
        {
            using (var brush = new SolidBrush(color))
            {
                switch (shape)
                {
                    case LabelShape.Rectangle:
                        g.FillRectangle(brush, bounds);
                        break;

                    case LabelShape.RoundedRectangle:
                        using (var path = GetRoundedPath(bounds, 4))
                        {
                            g.FillPath(brush, path);
                        }
                        break;

                    case LabelShape.Ellipse:
                        g.FillEllipse(brush, bounds);
                        break;
                }
            }
        }

        private void DrawLabelBorder(Graphics g, Rectangle bounds, Color color)
        {
            using (var pen = new Pen(color, 1))
            {
                switch (shape)
                {
                    case LabelShape.Rectangle:
                        g.DrawRectangle(pen, bounds);
                        break;

                    case LabelShape.RoundedRectangle:
                        using (var path = GetRoundedPath(bounds, 4))
                        {
                            g.DrawPath(pen, path);
                        }
                        break;

                    case LabelShape.Ellipse:
                        g.DrawEllipse(pen, bounds);
                        break;
                }
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }

        public override Control CreateEditingControl()
        {
            // Label列通常是只读的
            return null;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
            // Label列通常是只读的
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            return Value;
        }

        public override FluentDataGridViewCell Clone()
        {
            return new FluentDataGridViewLabelCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = true, // Label通常是只读的
                Shape = this.Shape,
                ShowBorder = this.ShowBorder,
                BorderColor = this.BorderColor
            };
        }
    }

    /// <summary>
    /// FluentLabel列
    /// </summary>
    public class FluentDataGridViewLabelColumn : FluentDataGridViewColumn
    {
        [Category("Appearance")]
        [DefaultValue(LabelShape.Rectangle)]
        public LabelShape LabelShape { get; set; } = LabelShape.Rectangle;

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowLabelBorder { get; set; } = false;

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color LabelBorderColor { get; set; } = Color.Gray;

        public FluentDataGridViewLabelColumn()
        {
            ReadOnly = true; // Label列默认只读
        }

        public override FluentDataGridViewCell CreateCell()
        {
            return new FluentDataGridViewLabelCell
            {
                Shape = this.LabelShape,
                ShowBorder = this.ShowLabelBorder,
                BorderColor = this.LabelBorderColor,
                ReadOnly = true
            };
        }

        public override FluentDataGridViewColumn Clone()
        {
            var column = new FluentDataGridViewLabelColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = true,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                LabelShape = this.LabelShape,
                ShowLabelBorder = this.ShowLabelBorder,
                LabelBorderColor = this.LabelBorderColor,
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
            return column;
        }
    }

    #endregion

    #region FluentDataGridViewButtonColumn

    /// <summary>
    /// FluentButton单元格
    /// </summary>
    public class FluentDataGridViewButtonCell : FluentDataGridViewCell
    {
        private string buttonText = "...";
        private ButtonStyle buttonStyle = ButtonStyle.Secondary;

        public string ButtonText
        {
            get => buttonText;
            set => buttonText = value ?? "...";
        }

        public ButtonStyle ButtonStyle
        {
            get => buttonStyle;
            set => buttonStyle = value;
        }

        public override object Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                // 如果值是字符串，也更新按钮文本
                if (value is string text)
                {
                    buttonText = text;
                }
            }
        }

        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var style = GetEffectiveStyle();
            var backColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);

            // 绘制单元格背景
            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 计算按钮位置(在单元格中居中，留出一些边距)
            var padding = 4;
            var buttonBounds = new Rectangle(
                bounds.X + padding,
                bounds.Y + padding,
                bounds.Width - padding * 2,
                bounds.Height - padding * 2);

            // 绘制按钮
            DrawButton(g, buttonBounds, state);
        }

        private void DrawButton(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            // 确定按钮颜色
            Color btnBackColor, btnForeColor;

            if (DataGridView?.UseTheme == true && DataGridView.Theme != null)
            {
                switch (buttonStyle)
                {
                    case ButtonStyle.Primary:
                        btnBackColor = DataGridView.Theme.Colors.Primary;
                        btnForeColor = DataGridView.Theme.Colors.TextOnPrimary;
                        break;
                    case ButtonStyle.Secondary:
                        btnBackColor = DataGridView.Theme.Colors.Surface;
                        btnForeColor = DataGridView.Theme.Colors.Primary;
                        break;
                    case ButtonStyle.Danger:
                        btnBackColor = DataGridView.Theme.Colors.Error;
                        btnForeColor = DataGridView.Theme.Colors.TextOnPrimary;
                        break;
                    case ButtonStyle.Success:
                        btnBackColor = DataGridView.Theme.Colors.Success;
                        btnForeColor = DataGridView.Theme.Colors.TextOnPrimary;
                        break;
                    default:
                        btnBackColor = Color.FromArgb(240, 240, 240);
                        btnForeColor = Color.Black;
                        break;
                }
            }
            else
            {
                btnBackColor = SystemColors.Control;
                btnForeColor = SystemColors.ControlText;
            }

            // 悬停效果
            if (state.HasFlag(DataGridViewCellState.Hovered))
            {
                btnBackColor = ControlPaint.Light(btnBackColor, 0.1f);
            }

            // 按下效果
            if (state.HasFlag(DataGridViewCellState.Selected))
            {
                btnBackColor = ControlPaint.Dark(btnBackColor, 0.1f);
            }

            // 绘制按钮背景
            var cornerRadius = 4;
            using (var path = GetRoundedRectPath(bounds, cornerRadius))
            using (var brush = new SolidBrush(btnBackColor))
            {
                g.FillPath(brush, path);
            }

            // 绘制按钮边框
            if (buttonStyle == ButtonStyle.Secondary)
            {
                var borderColor = DataGridView?.UseTheme == true && DataGridView.Theme != null
                    ? DataGridView.Theme.Colors.Border
                    : SystemColors.ControlDark;

                using (var path = GetRoundedRectPath(bounds, cornerRadius))
                using (var pen = new Pen(borderColor, 1))
                {
                    g.DrawPath(pen, path);
                }
            }

            // 绘制按钮文本
            using (var brush = new SolidBrush(btnForeColor))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                var font = DataGridView?.Font ?? SystemFonts.DefaultFont;
                g.DrawString(buttonText, font, brush, bounds, format);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // 左上
            path.AddArc(arc, 180, 90);
            // 右上
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            // 右下
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // 左下
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public override Control CreateEditingControl()
        {
            // 按钮单元格通常不进入编辑模式，而是触发点击事件
            return null;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
            // 按钮单元格不需要编辑控件
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            return Value;
        }

        public override FluentDataGridViewCell Clone()
        {
            return new FluentDataGridViewButtonCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = this.ReadOnly,
                ButtonText = this.ButtonText,
                ButtonStyle = this.ButtonStyle
            };
        }
    }

    /// <summary>
    /// FluentButton列
    /// </summary>
    public class FluentDataGridViewButtonColumn : FluentDataGridViewColumn
    {
        [Category("Appearance")]
        [DefaultValue("...")]
        public string ButtonText { get; set; } = "...";

        [Category("Appearance")]
        [DefaultValue(ButtonStyle.Secondary)]
        public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Secondary;

        /// <summary>
        /// 按钮点击事件
        /// </summary>
        public event EventHandler<DataGridViewCellEventArgs> ButtonClick;

        public override FluentDataGridViewCell CreateCell()
        {
            return new FluentDataGridViewButtonCell
            {
                ButtonText = this.ButtonText,
                ButtonStyle = this.ButtonStyle
            };
        }

        public override FluentDataGridViewColumn Clone()
        {
            var column = new FluentDataGridViewButtonColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = this.ReadOnly,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                ButtonText = this.ButtonText,
                ButtonStyle = this.ButtonStyle,
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
            return column;
        }

        internal void OnButtonClick(DataGridViewCellEventArgs e)
        {
            ButtonClick?.Invoke(this, e);
        }
    }

    #endregion

    #region FluentDataGridViewComboBoxColumn

    /// <summary>
    /// FluentComboBox单元格
    /// </summary>
    public class FluentDataGridViewComboBoxCell : FluentDataGridViewCell
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public object DataSource { get; set; }

        /// <summary>
        /// 显示成员
        /// </summary>
        public string DisplayMember { get; set; }

        /// <summary>
        /// 值成员
        /// </summary>
        public string ValueMember { get; set; }

        /// <summary>
        /// 选项列表
        /// </summary>
        public List<object> Items { get; set; } = new List<object>();

        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // 使用有效样式
            var style = GetEffectiveStyle();

            var backColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);

            var foreColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionForeColor ?? Color.White)
                : (style.ForeColor ?? Color.Black);

            // 绘制背景
            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 绘制悬停效果
            if (state.HasFlag(DataGridViewCellState.Hovered) && !state.HasFlag(DataGridViewCellState.Selected))
            {
                using (var brush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    g.FillRectangle(brush, bounds);
                }
            }

            // 绘制文本
            var text = GetDisplayText();
            if (!string.IsNullOrEmpty(text))
            {
                var padding = style.Padding;
                var textBounds = new Rectangle(
                    bounds.X + padding.Left,
                    bounds.Y + padding.Top,
                    bounds.Width - padding.Horizontal - 20, // 留出下拉箭头的空间
                    bounds.Height - padding.Vertical);

                var font = style.Font ?? OwningColumn?.DataGridView?.Font ?? SystemFonts.DefaultFont;

                using (var brush = new SolidBrush(foreColor))
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    g.DrawString(text, font, brush, textBounds, format);
                }
            }

            // 绘制下拉箭头
            var arrowRect = new Rectangle(
                bounds.Right - 18,
                bounds.Y + (bounds.Height - 8) / 2,
                8, 4);
            DrawDropDownArrow(g, arrowRect, foreColor);
        }

        private void DrawDropDownArrow(Graphics g, Rectangle bounds, Color color)
        {
            var points = new[]
            {
            new Point(bounds.X, bounds.Y),
            new Point(bounds.Right, bounds.Y),
            new Point(bounds.X + bounds.Width / 2, bounds.Bottom)
        };

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }
        }

        private string GetDisplayText()
        {
            if (Value == null)
            {
                return string.Empty;
            }

            // 如果设置了DisplayMember，尝试获取显示文本
            if (!string.IsNullOrEmpty(DisplayMember))
            {
                try
                {
                    var property = Value.GetType().GetProperty(DisplayMember);
                    if (property != null)
                    {
                        return property.GetValue(Value)?.ToString() ?? string.Empty;
                    }
                }
                catch { }
            }

            return Value.ToString();
        }

        public override Control CreateEditingControl()
        {
            var comboBox = new FluentComboBox
            {
                OnlySelection = true,
                SelectionStyle = ComboBoxSelectionStyle.Single
            };

            // 继承DataGridView的主题
            if (DataGridView != null && DataGridView.UseTheme)
            {
                comboBox.UseTheme = true;
                comboBox.ThemeName = DataGridView.ThemeName;
            }

            // 设置数据源
            if (DataSource != null)
            {
                comboBox.DataSource = DataSource as IList;
                comboBox.DisplayMember = DisplayMember;
                comboBox.ValueMember = ValueMember;
            }
            else if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    comboBox.Items.Add(item);
                }
            }

            return comboBox;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
            if (editingControl is FluentComboBox comboBox)
            {
                if (!string.IsNullOrEmpty(ValueMember))
                {
                    comboBox.SelectedValue = Value;
                }
                else
                {
                    comboBox.SelectedItem = Value;
                }
            }
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            if (editingControl is FluentComboBox comboBox)
            {
                return !string.IsNullOrEmpty(ValueMember)
                    ? comboBox.SelectedValue
                    : comboBox.SelectedItem;
            }
            return null;
        }

        public override FluentDataGridViewCell Clone()
        {
            var cell = new FluentDataGridViewComboBoxCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = this.ReadOnly,
                DataSource = this.DataSource,
                DisplayMember = this.DisplayMember,
                ValueMember = this.ValueMember,
                Items = new List<object>(this.Items)
            };
            return cell;
        }
    }

    /// <summary>
    /// FluentComboBox列
    /// </summary>
    public class FluentDataGridViewComboBoxColumn : FluentDataGridViewColumn
    {
        [Category("Data")]
        public object DataSource { get; set; }

        [Category("Data")]
        public string DisplayMember { get; set; }

        [Category("Data")]
        public string ValueMember { get; set; }

        [Category("Data")]
        public List<object> Items { get; set; } = new List<object>();

        public override FluentDataGridViewCell CreateCell()
        {
            return new FluentDataGridViewComboBoxCell
            {
                DataSource = this.DataSource,
                DisplayMember = this.DisplayMember,
                ValueMember = this.ValueMember,
                Items = new List<object>(this.Items)
            };
        }

        public override FluentDataGridViewColumn Clone()
        {
            var column = new FluentDataGridViewComboBoxColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = this.ReadOnly,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                DataSource = this.DataSource,
                DisplayMember = this.DisplayMember,
                ValueMember = this.ValueMember,
                Items = new List<object>(this.Items),
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
            return column;
        }
    }

    #endregion

    #region FluentDataGridViewImageColumn

    /// <summary>
    /// FluentImage单元格
    /// </summary>
    public class FluentDataGridViewImageCell : FluentDataGridViewCell
    {
        private ImageLayout imageLayout = ImageLayout.Zoom;

        public ImageLayout ImageLayout
        {
            get => imageLayout;
            set => imageLayout = value;
        }

        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var style = GetEffectiveStyle();
            var backColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);

            // 绘制背景
            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 获取图像
            Image image = null;
            if (Value is Image img)
            {
                image = img;
            }
            else if (Value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    if (File.Exists(path))
                    {
                        image = Image.FromFile(path);
                    }
                }
                catch
                {
                    // 加载失败，绘制占位符
                }
            }
            else if (Value is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    using (var ms = new System.IO.MemoryStream(bytes))
                    {
                        image = Image.FromStream(ms);
                    }
                }
                catch
                {
                    // 加载失败
                }
            }

            if (image != null)
            {
                var padding = style.Padding;
                var imageBounds = new Rectangle(
                    bounds.X + padding.Left,
                    bounds.Y + padding.Top,
                    bounds.Width - padding.Horizontal,
                    bounds.Height - padding.Vertical);

                DrawImage(g, image, imageBounds);
            }
            else
            {
                // 绘制占位符
                DrawPlaceholder(g, bounds);
            }
        }

        private void DrawImage(Graphics g, Image image, Rectangle bounds)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle destRect;

            switch (imageLayout)
            {
                case ImageLayout.None:
                    // 原始大小，左上角对齐
                    destRect = new Rectangle(bounds.Location, image.Size);
                    break;

                case ImageLayout.Tile:
                    // 平铺
                    using (var brush = new TextureBrush(image))
                    {
                        g.FillRectangle(brush, bounds);
                    }
                    return;

                case ImageLayout.Center:
                    // 居中显示原始大小
                    destRect = new Rectangle(
                        bounds.X + (bounds.Width - image.Width) / 2,
                        bounds.Y + (bounds.Height - image.Height) / 2,
                        image.Width,
                        image.Height);
                    break;

                case ImageLayout.Stretch:
                    // 拉伸填充
                    destRect = bounds;
                    break;

                case ImageLayout.Zoom:
                default:
                    // 按比例缩放
                    float imageRatio = (float)image.Width / image.Height;
                    float boundsRatio = (float)bounds.Width / bounds.Height;

                    int width, height;
                    if (imageRatio > boundsRatio)
                    {
                        // 图像更宽，以宽度为基准
                        width = bounds.Width;
                        height = (int)(bounds.Width / imageRatio);
                    }
                    else
                    {
                        // 图像更高，以高度为基准
                        height = bounds.Height;
                        width = (int)(bounds.Height * imageRatio);
                    }

                    destRect = new Rectangle(
                        bounds.X + (bounds.Width - width) / 2,
                        bounds.Y + (bounds.Height - height) / 2,
                        width,
                        height);
                    break;
            }

            // 确保不超出边界
            destRect.Intersect(bounds);

            if (destRect.Width > 0 && destRect.Height > 0)
            {
                g.DrawImage(image, destRect);
            }
        }

        private void DrawPlaceholder(Graphics g, Rectangle bounds)
        {
            // 绘制一个简单的占位图标
            var iconSize = Math.Min(bounds.Width, bounds.Height) / 2;
            var iconRect = new Rectangle(
                bounds.X + (bounds.Width - iconSize) / 2,
                bounds.Y + (bounds.Height - iconSize) / 2,
                iconSize,
                iconSize);

            using (var pen = new Pen(Color.Gray, 2))
            {
                // 绘制图片图标轮廓
                g.DrawRectangle(pen, iconRect);

                // 绘制山和太阳
                var sunSize = iconSize / 4;
                g.DrawEllipse(pen,
                    iconRect.X + iconSize / 4,
                    iconRect.Y + iconSize / 4,
                    sunSize, sunSize);

                // 绘制山
                var points = new[]
                {
                new Point(iconRect.X, iconRect.Bottom),
                new Point(iconRect.X + iconSize / 3, iconRect.Y + iconSize / 2),
                new Point(iconRect.X + iconSize * 2 / 3, iconRect.Y + iconSize * 2 / 3),
                new Point(iconRect.Right, iconRect.Bottom)
            };
                g.DrawLines(pen, points);
            }
        }

        public override Control CreateEditingControl()
        {
            // 图像列通常不直接编辑
            return null;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            return Value;
        }

        public override FluentDataGridViewCell Clone()
        {
            return new FluentDataGridViewImageCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = this.ReadOnly,
                ImageLayout = this.ImageLayout
            };
        }
    }

    /// <summary>
    /// FluentImage列
    /// </summary>
    public class FluentDataGridViewImageColumn : FluentDataGridViewColumn
    {
        [Category("Appearance")]
        [DefaultValue(ImageLayout.Zoom)]
        public ImageLayout ImageLayout { get; set; } = ImageLayout.Zoom;

        /// <summary>
        /// 默认图像(当单元格值为空时显示)
        /// </summary>
        [Category("Appearance")]
        public Image DefaultImage { get; set; }

        public override FluentDataGridViewCell CreateCell()
        {
            var cell = new FluentDataGridViewImageCell
            {
                ImageLayout = this.ImageLayout
            };

            if (DefaultImage != null)
            {
                cell.Value = DefaultImage;
            }

            return cell;
        }

        public override FluentDataGridViewColumn Clone()
        {
            var column = new FluentDataGridViewImageColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = this.ReadOnly,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                ImageLayout = this.ImageLayout,
                DefaultImage = this.DefaultImage,
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
            return column;
        }
    }

    #endregion

    #region FluentDataGridViewCheckBoxColumn

    /// <summary>
    /// FluentCheckBox单元格
    /// </summary>
    public class FluentDataGridViewCheckBoxCell : FluentDataGridViewCell
    {
        private bool isChecked;

        public override object Value
        {
            get => isChecked;
            set
            {
                if (value is bool boolValue)
                {
                    if (isChecked != boolValue)
                    {
                        var oldValue = isChecked;
                        isChecked = boolValue;
                        OnValueChanged(oldValue, isChecked);
                    }
                }
            }
        }

        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // 使用有效样式
            var style = GetEffectiveStyle();

            var backColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);

            // 绘制背景
            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 计算复选框位置(居中)
            int checkBoxSize = Math.Min(16, Math.Min(bounds.Width - 8, bounds.Height - 8));
            var checkBoxRect = new Rectangle(
                bounds.X + (bounds.Width - checkBoxSize) / 2,
                bounds.Y + (bounds.Height - checkBoxSize) / 2,
                checkBoxSize,
                checkBoxSize);

            // 绘制复选框
            DrawCheckBox(g, checkBoxRect, isChecked, state);
        }

        private void DrawCheckBox(Graphics g, Rectangle bounds, bool isChecked, DataGridViewCellState state)
        {
            var borderColor = state.HasFlag(DataGridViewCellState.Hovered)
                ? Color.FromArgb(0, 120, 215)
                : Color.FromArgb(153, 153, 153);

            var fillColor = isChecked
                ? Color.FromArgb(0, 120, 215)
                : Color.White;

            // 绘制边框
            using (var pen = new Pen(borderColor, 1))
            using (var brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, bounds);
                g.DrawRectangle(pen, bounds);
            }

            // 绘制勾选标记
            if (isChecked)
            {
                using (var pen = new Pen(Color.White, 2))
                {
                    var points = new[]
                    {
                    new Point(bounds.X + 3, bounds.Y + bounds.Height / 2),
                    new Point(bounds.X + bounds.Width / 2 - 1, bounds.Bottom - 4),
                    new Point(bounds.Right - 3, bounds.Y + 3)
                };
                    g.DrawLines(pen, points);
                }
            }
        }

        public override Control CreateEditingControl()
        {
            var checkBox = new FluentCheckBox
            {
                BackColor = Color.Transparent,
                AutoSize = false
            };

            if (DataGridView != null && DataGridView.UseTheme)
            {
                checkBox.UseTheme = true;
                checkBox.ThemeName = DataGridView.ThemeName;
            }

            return checkBox;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
            if (editingControl is FluentCheckBox checkBox)
            {
                checkBox.Checked = isChecked;
            }
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            if (editingControl is FluentCheckBox checkBox)
            {
                return checkBox.Checked;
            }
            return false;
        }

        public override FluentDataGridViewCell Clone()
        {
            return new FluentDataGridViewCheckBoxCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = this.ReadOnly
            };
        }
    }

    /// <summary>
    /// FluentCheckBox列
    /// </summary>
    public class FluentDataGridViewCheckBoxColumn : FluentDataGridViewColumn
    {
        public override FluentDataGridViewCell CreateCell()
        {
            return new FluentDataGridViewCheckBoxCell();
        }

        public override FluentDataGridViewColumn Clone()
        {
            return new FluentDataGridViewCheckBoxColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = this.ReadOnly,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
        }
    }

    #endregion

    #region FluentDataGridViewProgressColumn

    /// <summary>
    /// FluentProgress单元格
    /// </summary>
    public class FluentDataGridViewProgressCell : FluentDataGridViewCell
    {
        private ProgressStyle progressStyle = ProgressStyle.Linear;
        private double minimum = 0;
        private double maximum = 100;
        private bool showPercentage = true;
        private Color progressBarColor = Color.Empty;
        private Color progressBackColor = Color.Empty;

        public ProgressStyle ProgressStyle
        {
            get => progressStyle;
            set => progressStyle = value;
        }

        public double Minimum
        {
            get => minimum;
            set => minimum = value;
        }

        public double Maximum
        {
            get => maximum;
            set => maximum = value > minimum ? value : minimum + 1;
        }

        public bool ShowPercentage
        {
            get => showPercentage;
            set => showPercentage = value;
        }

        public Color ProgressBarColor
        {
            get => progressBarColor;
            set => progressBarColor = value;
        }

        public Color ProgressBackColor
        {
            get => progressBackColor;
            set => progressBackColor = value;
        }

        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var style = GetEffectiveStyle();
            var cellBackColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);

            // 绘制单元格背景
            using (var brush = new SolidBrush(cellBackColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 获取进度值
            double progress = 0;
            if (Value != null)
            {
                if (Value is double d)
                {
                    progress = d;
                }
                else if (Value is float f)
                {
                    progress = f;
                }
                else if (Value is int i)
                {
                    progress = i;
                }
                else if (Value is long l)
                {
                    progress = l;
                }
                else if (double.TryParse(Value.ToString(), out double parsed))
                {
                    progress = parsed;
                }
            }

            // 限制进度范围
            progress = Math.Max(minimum, Math.Min(maximum, progress));

            // 计算进度条位置(留出一些边距)
            var padding = 4;
            var progressBounds = new Rectangle(
                bounds.X + padding,
                bounds.Y + padding,
                bounds.Width - padding * 2,
                bounds.Height - padding * 2);

            // 绘制进度条
            DrawProgress(g, progressBounds, progress);
        }

        private void DrawProgress(Graphics g, Rectangle bounds, double progress)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 获取颜色
            Color barColor = progressBarColor;
            Color backColor = progressBackColor;

            if (barColor == Color.Empty)
            {
                barColor = DataGridView?.UseTheme == true && DataGridView.Theme != null
                    ? DataGridView.Theme.Colors.Primary
                    : Color.FromArgb(0, 120, 215);
            }

            if (backColor == Color.Empty)
            {
                backColor = DataGridView?.UseTheme == true && DataGridView.Theme != null
                    ? DataGridView.Theme.Colors.BackgroundSecondary
                    : Color.FromArgb(240, 240, 240);
            }

            switch (progressStyle)
            {
                case ProgressStyle.Linear:
                    DrawLinearProgress(g, bounds, progress, barColor, backColor);
                    break;
                case ProgressStyle.Circular:
                    DrawCircularProgress(g, bounds, progress, barColor, backColor);
                    break;
                case ProgressStyle.Segmented:
                    DrawSegmentedProgress(g, bounds, progress, barColor, backColor);
                    break;
            }

            // 绘制百分比文本
            if (showPercentage && progressStyle != ProgressStyle.Circular)
            {
                DrawPercentageText(g, bounds, progress);
            }
        }

        private void DrawLinearProgress(Graphics g, Rectangle bounds, double progress, Color barColor, Color backColor)
        {
            int cornerRadius = 3;

            // 绘制背景
            using (var path = GetRoundedRectPath(bounds, cornerRadius))
            using (var brush = new SolidBrush(backColor))
            {
                g.FillPath(brush, path);
            }

            // 计算进度宽度
            double percentage = (progress - minimum) / (maximum - minimum);
            int progressWidth = (int)(bounds.Width * percentage);

            if (progressWidth > 0)
            {
                var progressRect = new Rectangle(bounds.X, bounds.Y, progressWidth, bounds.Height);

                using (var path = GetRoundedRectPath(progressRect, cornerRadius))
                using (var brush = new LinearGradientBrush(
                    progressRect,
                    AdjustBrightness(barColor, 0.2f),
                    barColor,
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawCircularProgress(Graphics g, Rectangle bounds, double progress, Color barColor, Color backColor)
        {
            // 计算圆形区域
            int size = Math.Min(bounds.Width, bounds.Height);
            int x = bounds.X + (bounds.Width - size) / 2;
            int y = bounds.Y + (bounds.Height - size) / 2;
            var circleRect = new Rectangle(x + 2, y + 2, size - 4, size - 4);

            int thickness = Math.Max(2, size / 8);

            // 绘制背景圆环
            using (var pen = new Pen(backColor, thickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, circleRect, 0, 360);
            }

            // 绘制进度圆环
            double percentage = (progress - minimum) / (maximum - minimum);
            float sweepAngle = (float)(360 * percentage);

            if (sweepAngle > 0)
            {
                using (var pen = new Pen(barColor, thickness))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, circleRect, -90, sweepAngle);
                }
            }

            // 在圆心绘制百分比
            if (showPercentage)
            {
                string text = $"{(int)(percentage * 100)}%";
                var font = DataGridView?.Font ?? SystemFonts.DefaultFont;
                using (var smallFont = new Font(font.FontFamily, font.Size * 0.7f))
                using (var brush = new SolidBrush(barColor))
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(text, smallFont, brush, circleRect, format);
                }
            }
        }

        private void DrawSegmentedProgress(Graphics g, Rectangle bounds, double progress, Color barColor, Color backColor)
        {
            int segmentCount = 8;
            int segmentSpacing = 2;
            double percentage = (progress - minimum) / (maximum - minimum);
            int completedSegments = (int)Math.Ceiling(segmentCount * percentage);

            float segmentWidth = (bounds.Width - (segmentCount - 1) * segmentSpacing) / (float)segmentCount;
            int cornerRadius = 2;

            for (int i = 0; i < segmentCount; i++)
            {
                float x = bounds.X + i * (segmentWidth + segmentSpacing);
                var segmentRect = new RectangleF(x, bounds.Y, segmentWidth, bounds.Height);

                Color color = i < completedSegments ? barColor : backColor;

                using (var brush = new SolidBrush(color))
                using (var path = GetRoundedRectPath(Rectangle.Round(segmentRect), cornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawPercentageText(Graphics g, Rectangle bounds, double progress)
        {
            double percentage = (progress - minimum) / (maximum - minimum) * 100;
            string text = $"{percentage:F0}%";

            var font = DataGridView?.Font ?? SystemFonts.DefaultFont;
            Color textColor = Color.White; // 使用白色以确保在深色背景上可见

            // 计算文本是否在进度条内
            var textSize = g.MeasureString(text, font);
            double progressPercentage = (progress - minimum) / (maximum - minimum);
            int progressWidth = (int)(bounds.Width * progressPercentage);

            // 如果文本在进度条外，使用深色
            if (textSize.Width / 2 > progressWidth)
            {
                textColor = DataGridView?.ForeColor ?? Color.Black;
            }

            using (var brush = new SolidBrush(textColor))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString(text, font, brush, bounds, format);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0 || rect.Width < radius * 2 || rect.Height < radius * 2)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }

        private Color AdjustBrightness(Color color, float factor)
        {
            factor = Math.Max(-1, Math.Min(1, factor));

            int r = color.R;
            int g = color.G;
            int b = color.B;

            if (factor > 0)
            {
                r = r + (int)((255 - r) * factor);
                g = g + (int)((255 - g) * factor);
                b = b + (int)((255 - b) * factor);
            }
            else
            {
                r = r + (int)(r * factor);
                g = g + (int)(g * factor);
                b = b + (int)(b * factor);
            }

            return Color.FromArgb(color.A, r, g, b);
        }

        public override Control CreateEditingControl()
        {
            var progress = new FluentProgress
            {
                Mode = ProgressMode.Determinate,
                Style = this.ProgressStyle,
                Minimum = this.Minimum,
                Maximum = this.Maximum,
                ShowProgressText = this.ShowPercentage,
                ShowPercentage = true
            };

            if (DataGridView != null && DataGridView.UseTheme)
            {
                progress.UseTheme = true;
                progress.ThemeName = DataGridView.ThemeName;
            }

            if (this.ProgressBarColor != Color.Empty)
            {
                progress.ProgressBarColor = this.ProgressBarColor;
            }

            if (this.ProgressBackColor != Color.Empty)
            {
                progress.ProgressBackColor = this.ProgressBackColor;
            }

            return progress;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
            if (editingControl is FluentProgress progress)
            {
                double value = 0;
                if (Value is double d)
                {
                    value = d;
                }
                else if (Value is float f)
                {
                    value = f;
                }
                else if (Value is int i)
                {
                    value = i;
                }
                else if (Value is long l)
                {
                    value = l;
                }
                else if (double.TryParse(Value?.ToString(), out double parsed))
                {
                    value = parsed;
                }

                progress.Progress = value;
            }
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            if (editingControl is FluentProgress progress)
            {
                return progress.Progress;
            }
            return 0;
        }

        public override FluentDataGridViewCell Clone()
        {
            return new FluentDataGridViewProgressCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = this.ReadOnly,
                ProgressStyle = this.ProgressStyle,
                Minimum = this.Minimum,
                Maximum = this.Maximum,
                ShowPercentage = this.ShowPercentage,
                ProgressBarColor = this.ProgressBarColor,
                ProgressBackColor = this.ProgressBackColor
            };
        }
    }

    /// <summary>
    /// FluentProgress列
    /// </summary>
    public class FluentDataGridViewProgressColumn : FluentDataGridViewColumn
    {
        [Category("Progress")]
        [DefaultValue(ProgressStyle.Linear)]
        public ProgressStyle ProgressStyle { get; set; } = ProgressStyle.Linear;

        [Category("Progress")]
        [DefaultValue(0.0)]
        public double Minimum { get; set; } = 0;

        [Category("Progress")]
        [DefaultValue(100.0)]
        public double Maximum { get; set; } = 100;

        [Category("Progress")]
        [DefaultValue(true)]
        public bool ShowPercentage { get; set; } = true;

        [Category("Progress")]
        public Color ProgressBarColor { get; set; } = Color.Empty;

        [Category("Progress")]
        public Color ProgressBackColor { get; set; } = Color.Empty;

        public FluentDataGridViewProgressColumn()
        {
            // 进度列通常只读，但也可以允许编辑
            ReadOnly = false;
        }

        public override FluentDataGridViewCell CreateCell()
        {
            return new FluentDataGridViewProgressCell
            {
                ProgressStyle = this.ProgressStyle,
                Minimum = this.Minimum,
                Maximum = this.Maximum,
                ShowPercentage = this.ShowPercentage,
                ProgressBarColor = this.ProgressBarColor,
                ProgressBackColor = this.ProgressBackColor
            };
        }

        public override FluentDataGridViewColumn Clone()
        {
            var column = new FluentDataGridViewProgressColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = this.ReadOnly,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                ProgressStyle = this.ProgressStyle,
                Minimum = this.Minimum,
                Maximum = this.Maximum,
                ShowPercentage = this.ShowPercentage,
                ProgressBarColor = this.ProgressBarColor,
                ProgressBackColor = this.ProgressBackColor,
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
            return column;
        }
    }

    #endregion

    #region FluentDataGridViewColorComboBoxColumn

    /// <summary>
    /// FluentColorComboBox单元格
    /// </summary>
    public class FluentDataGridViewColorComboBoxCell : FluentDataGridViewCell
    {
        private ColorBlockShape blockShape = ColorBlockShape.Square;
        private ColorTextFormat textFormat = ColorTextFormat.Hex;
        private bool showColorText = true;
        private List<ColorItem> colorItems = new List<ColorItem>();

        public ColorBlockShape BlockShape
        {
            get => blockShape;
            set => blockShape = value;
        }

        public ColorTextFormat TextFormat
        {
            get => textFormat;
            set => textFormat = value;
        }

        public bool ShowColorText
        {
            get => showColorText;
            set => showColorText = value;
        }

        public List<ColorItem> ColorItems
        {
            get => colorItems;
            set => colorItems = value ?? new List<ColorItem>();
        }

        public override void Paint(Graphics g, Rectangle bounds, DataGridViewCellState state)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var style = GetEffectiveStyle();
            var backColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionBackColor ?? Color.FromArgb(0, 120, 215))
                : (style.BackColor ?? Color.White);
            var foreColor = state.HasFlag(DataGridViewCellState.Selected)
                ? (style.SelectionForeColor ?? Color.White)
                : (style.ForeColor ?? Color.Black);

            // 绘制背景
            using (var brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, bounds);
            }

            // 获取颜色值
            Color color = Color.Empty;
            if (Value is Color c)
            {
                color = c;
            }
            else if (Value != null && !string.IsNullOrEmpty(Value.ToString()))
            {
                try
                {
                    // 尝试从字符串解析颜色
                    var converter = new ColorConverter();
                    color = (Color)converter.ConvertFromString(Value.ToString());
                }
                catch
                {
                    color = Color.Gray;
                }
            }

            if (color == Color.Empty)
            {
                color = Color.Transparent;
            }

            // 绘制颜色块和文本
            var padding = 4;
            int blockSize = Math.Min(bounds.Height - padding * 2, 18);
            var blockRect = new Rectangle(
                bounds.X + padding,
                bounds.Y + (bounds.Height - blockSize) / 2,
                blockSize,
                blockSize);

            // 绘制颜色块
            DrawColorBlock(g, blockRect, color);

            // 绘制文本
            if (showColorText)
            {
                var textRect = new Rectangle(
                    blockRect.Right + 6,
                    bounds.Y,
                    bounds.Width - blockRect.Right - 6 - padding - 20, // 留出下拉箭头空间
                    bounds.Height);

                string text = GetColorText(color);
                using (var brush = new SolidBrush(foreColor))
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    var font = style.Font ?? OwningColumn?.DataGridView?.Font ?? SystemFonts.DefaultFont;
                    g.DrawString(text, font, brush, textRect, format);
                }
            }

            // 绘制下拉箭头
            DrawDropDownArrow(g, bounds, foreColor);
        }

        private void DrawColorBlock(Graphics g, Rectangle bounds, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 先绘制棋盘格背景(用于显示透明色)
            DrawCheckerboard(g, bounds);

            // 绘制颜色
            using (var brush = new SolidBrush(color))
            {
                switch (blockShape)
                {
                    case ColorBlockShape.Circle:
                        g.FillEllipse(brush, bounds);
                        break;
                    case ColorBlockShape.Square:
                        g.FillRectangle(brush, bounds);
                        break;
                    case ColorBlockShape.RoundedSquare:
                        using (var path = GetRoundedRectPath(bounds, 3))
                        {
                            g.FillPath(brush, path);
                        }
                        break;
                }
            }

            // 绘制边框
            using (var pen = new Pen(Color.FromArgb(150, Color.Gray), 1))
            {
                switch (blockShape)
                {
                    case ColorBlockShape.Circle:
                        g.DrawEllipse(pen, bounds);
                        break;
                    case ColorBlockShape.Square:
                        g.DrawRectangle(pen, bounds);
                        break;
                    case ColorBlockShape.RoundedSquare:
                        using (var path = GetRoundedRectPath(bounds, 3))
                        {
                            g.DrawPath(pen, path);
                        }
                        break;
                }
            }
        }

        private void DrawCheckerboard(Graphics g, Rectangle rect)
        {
            const int checkSize = 4;
            using (var lightBrush = new SolidBrush(Color.White))
            using (var darkBrush = new SolidBrush(Color.LightGray))
            {
                g.FillRectangle(lightBrush, rect);

                for (int y = rect.Top; y < rect.Bottom; y += checkSize)
                {
                    for (int x = rect.Left; x < rect.Right; x += checkSize)
                    {
                        if ((x - rect.Left) / checkSize % 2 == (y - rect.Top) / checkSize % 2)
                        {
                            var checkRect = new Rectangle(x, y,
                                Math.Min(checkSize, rect.Right - x),
                                Math.Min(checkSize, rect.Bottom - y));
                            g.FillRectangle(darkBrush, checkRect);
                        }
                    }
                }
            }
        }

        private void DrawDropDownArrow(Graphics g, Rectangle bounds, Color color)
        {
            int arrowSize = 4;
            int arrowX = bounds.Right - arrowSize - 8;
            int arrowY = bounds.Y + bounds.Height / 2;

            Point[] arrow = new Point[]
            {
            new Point(arrowX - arrowSize, arrowY - 2),
            new Point(arrowX + arrowSize, arrowY - 2),
            new Point(arrowX, arrowY + 2)
            };

            using (var brush = new SolidBrush(Color.FromArgb(150, color)))
            {
                g.FillPolygon(brush, arrow);
            }
        }

        private string GetColorText(Color color)
        {
            // 查找命名颜色
            var item = colorItems.FirstOrDefault(i => i.Color.ToArgb() == color.ToArgb());
            if (item != null && !string.IsNullOrEmpty(item.Name))
            {
                return item.Name;
            }

            // 根据格式显示
            switch (textFormat)
            {
                case ColorTextFormat.Hex:
                    return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                case ColorTextFormat.RGB:
                    return $"RGB({color.R}, {color.G}, {color.B})";
                case ColorTextFormat.ARGB:
                    return $"R:{color.R} G:{color.G} B:{color.B}";
                default:
                    return color.Name;
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }

        public override Control CreateEditingControl()
        {
            var colorComboBox = new FluentColorComboBox
            {
                BlockShape = this.BlockShape,
                TextFormat = this.TextFormat,
                ShowColorText = this.ShowColorText,
                ShowBorder = false,
                BorderSize = 0
            };

            // 继承DataGridView的主题
            if (DataGridView != null && DataGridView.UseTheme)
            {
                colorComboBox.UseTheme = true;
                colorComboBox.ThemeName = DataGridView.ThemeName;
            }

            // 设置颜色列表
            if (colorItems != null && colorItems.Count > 0)
            {
                colorComboBox.ClearColors();
                colorComboBox.AddColors(colorItems.ToArray());
            }

            return colorComboBox;
        }

        public override void PrepareEditingControl(Control editingControl)
        {
            if (editingControl is FluentColorComboBox colorComboBox)
            {
                if (Value is Color color)
                {
                    colorComboBox.SelectedColor = color;
                }
            }
        }

        public override object GetEditingControlValue(Control editingControl)
        {
            if (editingControl is FluentColorComboBox colorComboBox)
            {
                return colorComboBox.SelectedColor;
            }
            return Color.Empty;
        }

        public override FluentDataGridViewCell Clone()
        {
            return new FluentDataGridViewColorComboBoxCell
            {
                Value = this.Value,
                Style = this.Style?.Clone(),
                ReadOnly = this.ReadOnly,
                BlockShape = this.BlockShape,
                TextFormat = this.TextFormat,
                ShowColorText = this.ShowColorText,
                ColorItems = new List<ColorItem>(this.ColorItems)
            };
        }
    }

    /// <summary>
    /// FluentColorComboBox列
    /// </summary>
    public class FluentDataGridViewColorComboBoxColumn : FluentDataGridViewColumn
    {
        [Category("Appearance")]
        [DefaultValue(ColorBlockShape.Square)]
        public ColorBlockShape BlockShape { get; set; } = ColorBlockShape.Square;

        [Category("Appearance")]
        [DefaultValue(ColorTextFormat.Hex)]
        public ColorTextFormat TextFormat { get; set; } = ColorTextFormat.Hex;

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShowColorText { get; set; } = true;

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<ColorItem> ColorItems { get; set; } = new List<ColorItem>();

        public FluentDataGridViewColorComboBoxColumn()
        {
            // 初始化默认颜色
            InitializeDefaultColors();
        }

        private void InitializeDefaultColors()
        {
            ColorItems.Add(new ColorItem(Color.FromArgb(0, 120, 212), "蓝色"));
            ColorItems.Add(new ColorItem(Color.FromArgb(32, 32, 32), "黑色"));
            ColorItems.Add(new ColorItem(Color.FromArgb(76, 175, 80), "绿色"));
            ColorItems.Add(new ColorItem(Color.FromArgb(255, 193, 7), "黄色"));
            ColorItems.Add(new ColorItem(Color.FromArgb(244, 67, 54), "红色"));
            ColorItems.Add(new ColorItem(Color.FromArgb(156, 39, 176), "紫色"));
            ColorItems.Add(new ColorItem(Color.FromArgb(255, 87, 34), "橙色"));
            ColorItems.Add(new ColorItem(Color.FromArgb(96, 125, 139), "灰色"));
        }

        /// <summary>
        /// 添加颜色
        /// </summary>
        public void AddColor(Color color, string name = null)
        {
            ColorItems.Add(new ColorItem(color, name));
        }

        /// <summary>
        /// 清除所有颜色
        /// </summary>
        public void ClearColors()
        {
            ColorItems.Clear();
        }

        public override FluentDataGridViewCell CreateCell()
        {
            return new FluentDataGridViewColorComboBoxCell
            {
                BlockShape = this.BlockShape,
                TextFormat = this.TextFormat,
                ShowColorText = this.ShowColorText,
                ColorItems = new List<ColorItem>(this.ColorItems)
            };
        }

        public override FluentDataGridViewColumn Clone()
        {
            var column = new FluentDataGridViewColorComboBoxColumn
            {
                Name = this.Name,
                DataPropertyName = this.DataPropertyName,
                HeaderText = this.HeaderText,
                Width = this.Width,
                WidthPercentage = this.WidthPercentage,
                WidthMode = this.WidthMode,
                Visible = this.Visible,
                ReadOnly = this.ReadOnly,
                SortMode = this.SortMode,
                ShowFilter = this.ShowFilter,
                BlockShape = this.BlockShape,
                TextFormat = this.TextFormat,
                ShowColorText = this.ShowColorText,
                ColorItems = new List<ColorItem>(this.ColorItems),
                DefaultCellStyle = this.DefaultCellStyle?.Clone(),
                HeaderStyle = this.HeaderStyle?.Clone()
            };
            return column;
        }
    }

    #endregion

    #endregion

    #region 集合

    /// <summary>
    /// 列集合
    /// </summary>
    public class DataGridViewColumnCollection : IList<FluentDataGridViewColumn>
    {
        private readonly List<FluentDataGridViewColumn> columns = new List<FluentDataGridViewColumn>();
        private readonly FluentDataGridView owner;

        public DataGridViewColumnCollection(FluentDataGridView owner)
        {
            this.owner = owner;
        }

        public FluentDataGridViewColumn this[int index]
        {
            get => columns[index];
            set
            {
                if (columns[index] != value)
                {
                    var oldColumn = columns[index];
                    oldColumn.DataGridView = null;
                    columns[index] = value;
                    value.DataGridView = owner;
                    value.Index = index;
                    owner.OnColumnChanged();
                }
            }
        }

        public FluentDataGridViewColumn this[string name]
        {
            get => columns.FirstOrDefault(c => c.Name == name);
        }

        public int Count => columns.Count;
        public bool IsReadOnly => false;

        public void Add(FluentDataGridViewColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.DataGridView = owner;
            column.Index = columns.Count;
            columns.Add(column);
            owner.OnColumnAdded(column);
        }

        public void Insert(int index, FluentDataGridViewColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            column.DataGridView = owner;
            columns.Insert(index, column);

            // 更新索引
            for (int i = index; i < columns.Count; i++)
            {
                columns[i].Index = i;
            }

            owner.OnColumnAdded(column);
        }

        public bool Remove(FluentDataGridViewColumn column)
        {
            if (column == null)
            {
                return false;
            }

            var index = columns.IndexOf(column);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var column = columns[index];
            column.DataGridView = null;
            columns.RemoveAt(index);

            // 更新索引
            for (int i = index; i < columns.Count; i++)
            {
                columns[i].Index = i;
            }

            owner.OnColumnRemoved(column);
        }

        public void Clear()
        {
            foreach (var column in columns)
            {
                column.DataGridView = null;
            }
            columns.Clear();
            owner.OnColumnsCleared();
        }

        public bool Contains(FluentDataGridViewColumn column) => columns.Contains(column);
        public int IndexOf(FluentDataGridViewColumn column) => columns.IndexOf(column);
        public void CopyTo(FluentDataGridViewColumn[] array, int arrayIndex) => columns.CopyTo(array, arrayIndex);
        public IEnumerator<FluentDataGridViewColumn> GetEnumerator() => columns.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// 行集合
    /// </summary>
    public class DataGridViewRowCollection : IList<FluentDataGridViewRow>
    {
        private readonly List<FluentDataGridViewRow> rows = new List<FluentDataGridViewRow>();
        private readonly FluentDataGridView owner;

        public DataGridViewRowCollection(FluentDataGridView owner)
        {
            this.owner = owner;
        }

        public FluentDataGridViewRow this[int index]
        {
            get => rows[index];
            set
            {
                if (rows[index] != value)
                {
                    var oldRow = rows[index];
                    oldRow.DataGridView = null;
                    rows[index] = value;
                    value.DataGridView = owner;
                    value.Index = index;
                    owner.OnRowChanged();
                }
            }
        }

        public int Count => rows.Count;
        public bool IsReadOnly => false;

        public void Add(FluentDataGridViewRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            row.DataGridView = owner;
            row.Index = rows.Count;
            row.CreateCells(owner);
            rows.Add(row);
            owner.OnRowAdded(row);
        }

        public FluentDataGridViewRow AddNew()
        {
            var row = new FluentDataGridViewRow();
            Add(row);
            return row;
        }

        public void Insert(int index, FluentDataGridViewRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            row.DataGridView = owner;
            row.CreateCells(owner);
            rows.Insert(index, row);

            // 更新索引
            for (int i = index; i < rows.Count; i++)
            {
                rows[i].Index = i;
            }

            owner.OnRowAdded(row);
        }

        public bool Remove(FluentDataGridViewRow row)
        {
            if (row == null)
            {
                return false;
            }

            var index = rows.IndexOf(row);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            var row = rows[index];
            row.DataGridView = null;
            row.Dispose();
            rows.RemoveAt(index);

            // 更新索引
            for (int i = index; i < rows.Count; i++)
            {
                rows[i].Index = i;
            }

            owner.OnRowRemoved(row);
        }

        public void Clear()
        {
            foreach (var row in rows)
            {
                row.DataGridView = null;
                row.Dispose();
            }
            rows.Clear();
            owner.OnRowsCleared();
        }

        public bool Contains(FluentDataGridViewRow row) => rows.Contains(row);
        public int IndexOf(FluentDataGridViewRow row) => rows.IndexOf(row);
        public void CopyTo(FluentDataGridViewRow[] array, int arrayIndex) => rows.CopyTo(array, arrayIndex);
        public IEnumerator<FluentDataGridViewRow> GetEnumerator() => rows.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    #endregion

    #region 数据筛选功能

    ///// <summary>
    ///// 列筛选条件
    ///// </summary>
    //public class ColumnFilter
    //{
    //    public FluentDataGridViewColumn Column { get; set; }
    //    public FilterOperator Operator { get; set; }
    //    public object Value { get; set; }
    //    public object Value2 { get; set; } // 用于Between操作
    //    public bool IsNumeric { get; set; }

    //    public bool Match(object cellValue)
    //    {
    //        if (cellValue == null)
    //        {
    //            return false;
    //        }

    //        try
    //        {
    //            if (IsNumeric)
    //            {
    //                return MatchNumeric(cellValue);
    //            }
    //            else
    //            {
    //                return MatchText(cellValue);
    //            }
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    private bool MatchText(object cellValue)
    //    {
    //        string cellText = cellValue?.ToString() ?? "";
    //        string filterText = Value?.ToString() ?? "";

    //        switch (Operator)
    //        {
    //            case FilterOperator.Contains:
    //                return cellText.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
    //            case FilterOperator.Equals:
    //                return cellText.Equals(filterText, StringComparison.OrdinalIgnoreCase);
    //            case FilterOperator.NotEquals:
    //                return !cellText.Equals(filterText, StringComparison.OrdinalIgnoreCase);
    //            case FilterOperator.StartsWith:
    //                return cellText.StartsWith(filterText, StringComparison.OrdinalIgnoreCase);
    //            case FilterOperator.EndsWith:
    //                return cellText.EndsWith(filterText, StringComparison.OrdinalIgnoreCase);
    //            default:
    //                return true;
    //        }
    //    }

    //    private bool MatchNumeric(object cellValue)
    //    {
    //        if (!TryConvertToDouble(cellValue, out double cellNumber))
    //        {
    //            return false;
    //        }

    //        if (!TryConvertToDouble(Value, out double filterNumber))
    //        {
    //            return false;
    //        }

    //        switch (Operator)
    //        {
    //            case FilterOperator.Equals:
    //                return Math.Abs(cellNumber - filterNumber) < 0.0001;
    //            case FilterOperator.NotEquals:
    //                return Math.Abs(cellNumber - filterNumber) >= 0.0001;
    //            case FilterOperator.GreaterThan:
    //                return cellNumber > filterNumber;
    //            case FilterOperator.LessThan:
    //                return cellNumber < filterNumber;
    //            case FilterOperator.GreaterThanOrEqual:
    //                return cellNumber >= filterNumber;
    //            case FilterOperator.LessThanOrEqual:
    //                return cellNumber <= filterNumber;
    //            case FilterOperator.Between:
    //                if (TryConvertToDouble(Value2, out double filterNumber2))
    //                {
    //                    double min = Math.Min(filterNumber, filterNumber2);
    //                    double max = Math.Max(filterNumber, filterNumber2);
    //                    return cellNumber >= min && cellNumber <= max;
    //                }
    //                return false;
    //            default:
    //                return true;
    //        }
    //    }

    //    private bool TryConvertToDouble(object value, out double result)
    //    {
    //        result = 0;
    //        if (value == null)
    //        {
    //            return false;
    //        }

    //        if (value is double d)
    //        {
    //            result = d;
    //            return true;
    //        }

    //        return double.TryParse(value.ToString(), out result);
    //    }
    //}

    ///// <summary>
    ///// 筛选管理器
    ///// </summary>
    //public class FilterManager
    //{
    //    private Dictionary<FluentDataGridViewColumn, ColumnFilter> filters =
    //        new Dictionary<FluentDataGridViewColumn, ColumnFilter>();

    //    public void AddFilter(ColumnFilter filter)
    //    {
    //        if (filter != null && filter.Column != null)
    //        {
    //            filters[filter.Column] = filter;
    //        }
    //    }

    //    public void RemoveFilter(FluentDataGridViewColumn column)
    //    {
    //        filters.Remove(column);
    //    }

    //    public void ClearFilters()
    //    {
    //        filters.Clear();
    //    }

    //    public bool HasFilters => filters.Count > 0;

    //    public bool MatchRow(FluentDataGridViewRow row)
    //    {
    //        // 所有筛选条件都必须匹配
    //        foreach (var filter in filters.Values)
    //        {
    //            var cell = row.Cells.FirstOrDefault(c => c.OwningColumn == filter.Column);
    //            if (cell == null || !filter.Match(cell.Value))
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }

    //    public IEnumerable<ColumnFilter> GetFilters()
    //    {
    //        return filters.Values;
    //    }

    //    public ColumnFilter GetFilter(FluentDataGridViewColumn column)
    //    {
    //        filters.TryGetValue(column, out var filter);
    //        return filter;
    //    }
    //}

    ///// <summary>
    ///// 列筛选输入框
    ///// </summary>
    //internal class ColumnFilterTextBox : TextBox
    //{
    //    public ColumnFilterTextBox()
    //    {
    //        BorderStyle = BorderStyle.FixedSingle;
    //        Font = new Font("Microsoft YaHei", 8f);
    //        Height = 20;
    //    }

    //    public FluentDataGridViewColumn Column { get; set; }
    //}

    ///// <summary>
    ///// 数值筛选对话框
    ///// </summary>
    //internal class NumericFilterDialog : Form
    //{
    //    private ComboBox cmbOperator;
    //    private TextBox txtValue1;
    //    private TextBox txtValue2;
    //    private Label lblValue2;
    //    private Button btnOK;
    //    private Button btnCancel;

    //    public NumericFilterDialog(string columnName)
    //    {
    //        InitializeComponents(columnName);
    //    }

    //    public FilterOperator SelectedOperator { get; private set; }
    //    public double Value1 { get; private set; }
    //    public double Value2 { get; private set; }


    //    private void InitializeComponents(string columnName)
    //    {
    //        Text = $"筛选 - {columnName}";
    //        Size = new Size(350, 200);
    //        StartPosition = FormStartPosition.CenterParent;
    //        FormBorderStyle = FormBorderStyle.FixedDialog;
    //        MaximizeBox = false;
    //        MinimizeBox = false;

    //        var lblOperator = new Label
    //        {
    //            Text = "条件:",
    //            Location = new Point(20, 20),
    //            AutoSize = true
    //        };

    //        cmbOperator = new ComboBox
    //        {
    //            Location = new Point(80, 17),
    //            Width = 240,
    //            DropDownStyle = ComboBoxStyle.DropDownList
    //        };
    //        cmbOperator.Items.AddRange(new object[]
    //        {
    //            "等于",
    //            "不等于",
    //            "大于",
    //            "小于",
    //            "大于等于",
    //            "小于等于",
    //            "范围"
    //        });
    //        cmbOperator.SelectedIndex = 0;
    //        cmbOperator.SelectedIndexChanged += (s, e) => UpdateValueFields();

    //        var lblValue1 = new Label
    //        {
    //            Text = "值:",
    //            Location = new Point(20, 60),
    //            AutoSize = true
    //        };

    //        txtValue1 = new TextBox
    //        {
    //            Location = new Point(80, 57),
    //            Width = 240
    //        };

    //        lblValue2 = new Label
    //        {
    //            Text = "到:",
    //            Location = new Point(20, 95),
    //            AutoSize = true,
    //            Visible = false
    //        };

    //        txtValue2 = new TextBox
    //        {
    //            Location = new Point(80, 92),
    //            Width = 240,
    //            Visible = false
    //        };

    //        btnOK = new Button
    //        {
    //            Text = "确定",
    //            DialogResult = DialogResult.OK,
    //            Location = new Point(160, 130),
    //            Size = new Size(75, 25)
    //        };
    //        btnOK.Click += BtnOK_Click;

    //        btnCancel = new Button
    //        {
    //            Text = "取消",
    //            DialogResult = DialogResult.Cancel,
    //            Location = new Point(245, 130),
    //            Size = new Size(75, 25)
    //        };

    //        Controls.AddRange(new Control[]
    //        {
    //        lblOperator, cmbOperator,
    //        lblValue1, txtValue1,
    //        lblValue2, txtValue2,
    //        btnOK, btnCancel
    //        });

    //        AcceptButton = btnOK;
    //        CancelButton = btnCancel;
    //    }

    //    private void UpdateValueFields()
    //    {
    //        bool isBetween = cmbOperator.SelectedIndex == 6; // "范围"
    //        lblValue2.Visible = isBetween;
    //        txtValue2.Visible = isBetween;
    //    }

    //    private void BtnOK_Click(object sender, EventArgs e)
    //    {
    //        if (!double.TryParse(txtValue1.Text, out double val1))
    //        {
    //            MessageBox.Show("请输入有效的数值", "错误",
    //                MessageBoxButtons.OK, MessageBoxIcon.Error);
    //            DialogResult = DialogResult.None;
    //            return;
    //        }

    //        Value1 = val1;

    //        switch (cmbOperator.SelectedIndex)
    //        {
    //            case 0: SelectedOperator = FilterOperator.Equals; break;
    //            case 1: SelectedOperator = FilterOperator.NotEquals; break;
    //            case 2: SelectedOperator = FilterOperator.GreaterThan; break;
    //            case 3: SelectedOperator = FilterOperator.LessThan; break;
    //            case 4: SelectedOperator = FilterOperator.GreaterThanOrEqual; break;
    //            case 5: SelectedOperator = FilterOperator.LessThanOrEqual; break;
    //            case 6:
    //                SelectedOperator = FilterOperator.Between;
    //                if (!double.TryParse(txtValue2.Text, out double val2))
    //                {
    //                    MessageBox.Show("请输入有效的第二个数值", "错误",
    //                        MessageBoxButtons.OK, MessageBoxIcon.Error);
    //                    DialogResult = DialogResult.None;
    //                    return;
    //                }
    //                Value2 = val2;
    //                break;
    //        }
    //    }
    //}

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 列宽模式
    /// </summary>
    public enum DataGridViewColumnWidthMode
    {
        Fixed,          // 固定宽度
        Percentage,     // 比例宽度
        AutoSize,       // 自适应内容
        Fill            // 填充剩余空间
    }

    /// <summary>
    /// 列排序模式
    /// </summary>
    public enum DataGridViewColumnSortMode
    {
        NotSortable,    // 不可排序
        Automatic,      // 自动排序
        Programmatic    // 基于编程排序
    }

    /// <summary>
    /// 排序方向
    /// </summary>
    public enum SortOrder
    {
        None,
        Ascending,
        Descending
    }

    /// <summary>
    /// 单元格编辑模式
    /// </summary>
    public enum DataGridViewEditMode
    {
        EditOnEnter,        // 点击进入编辑
        EditOnDoubleClick,  // 双击进入编辑
        EditOnKeystroke,    // 按键进入编辑
        EditOnF2            // 按F2进入编辑
    }

    /// <summary>
    /// 选择模式
    /// </summary>
    public enum DataGridViewSelectionMode
    {
        CellSelect,         // 单元格选择
        FullRowSelect,      // 整行选择
        FullColumnSelect    // 整列选择
    }

    /// <summary>
    /// 分页位置枚举
    /// </summary>
    public enum PaginationPosition
    {
        Top,
        Bottom
    }

    /// <summary>
    /// 筛选操作符
    /// </summary>
    public enum FilterOperator
    {
        Contains,           // 包含(文本)
        Equals,            // 等于
        NotEquals,         // 不等于
        GreaterThan,       // 大于
        LessThan,          // 小于
        GreaterThanOrEqual,// 大于等于
        LessThanOrEqual,   // 小于等于
        Between,           // 范围
        StartsWith,        // 开始于
        EndsWith          // 结束于
    }



    /// <summary>
    /// 单元格位置
    /// </summary>
    public struct DataGridViewCellAddress
    {
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }

        public DataGridViewCellAddress(int columnIndex, int rowIndex)
        {
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is DataGridViewCellAddress other)
            {
                return ColumnIndex == other.ColumnIndex && RowIndex == other.RowIndex;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ColumnIndex.GetHashCode() ^ RowIndex.GetHashCode();
        }

        public static bool operator ==(DataGridViewCellAddress left, DataGridViewCellAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DataGridViewCellAddress left, DataGridViewCellAddress right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// 排序事件参数
    /// </summary>
    public class DataGridViewSortEventArgs : EventArgs
    {
        public FluentDataGridViewColumn Column { get; set; }
        public SortOrder SortOrder { get; set; }
        public bool Cancel { get; set; }

        public DataGridViewSortEventArgs(FluentDataGridViewColumn column, SortOrder sortOrder)
        {
            Column = column;
            SortOrder = sortOrder;
            Cancel = false;
        }
    }

    /// <summary>
    /// 单元格事件参数
    /// </summary>
    public class DataGridViewCellEventArgs : EventArgs
    {
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
        public FluentDataGridViewCell Cell { get; set; }

        public DataGridViewCellEventArgs(int columnIndex, int rowIndex, FluentDataGridViewCell cell = null)
        {
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            Cell = cell;
        }
    }

    /// <summary>
    /// 单元格值改变事件参数
    /// </summary>
    public class DataGridViewCellValueEventArgs : DataGridViewCellEventArgs
    {
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public DataGridViewCellValueEventArgs(int columnIndex, int rowIndex, object oldValue, object newValue)
            : base(columnIndex, rowIndex)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    #endregion

    #region 设计器支持

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class FluentDataGridViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private ISelectionService selectionService;
        private IDesignerHost designerHost;
        private IServiceProvider serviceProvider;
        private ContextMenuStrip contextMenu;

        public FluentDataGridView DataGridView => (FluentDataGridView)Control;

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            if (component is FluentDataGridView grid)
            {
                // 获取设计时服务
                serviceProvider = component.Site;
                selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));

                // 初始化上下文菜单
                InitializeContextMenu();
            }
        }

        #region 智能标记

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (actionLists == null)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new FluentDataGridViewActionList(this));
                }
                return actionLists;
            }
        }

        #endregion

        #region 上下文菜单

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            // 添加列菜单
            var addColumnMenu = new ToolStripMenuItem("添加列");
            addColumnMenu.DropDownItems.Add("文本列", null, (s, e) => AddColumn(typeof(FluentDataGridViewTextBoxColumn)));
            addColumnMenu.DropDownItems.Add("下拉列", null, (s, e) => AddColumn(typeof(FluentDataGridViewComboBoxColumn)));
            addColumnMenu.DropDownItems.Add("复选框列", null, (s, e) => AddColumn(typeof(FluentDataGridViewCheckBoxColumn)));
            addColumnMenu.DropDownItems.Add("按钮列", null, (s, e) => AddColumn(typeof(FluentDataGridViewButtonColumn)));
            addColumnMenu.DropDownItems.Add("标签列", null, (s, e) => AddColumn(typeof(FluentDataGridViewLabelColumn)));
            addColumnMenu.DropDownItems.Add("图片列", null, (s, e) => AddColumn(typeof(FluentDataGridViewImageColumn)));
            addColumnMenu.DropDownItems.Add("颜色选择列", null, (s, e) => AddColumn(typeof(FluentDataGridViewColorComboBoxColumn)));
            addColumnMenu.DropDownItems.Add("进度条列", null, (s, e) => AddColumn(typeof(FluentDataGridViewProgressColumn)));

            contextMenu.Items.Add(addColumnMenu);
            contextMenu.Items.Add(new ToolStripSeparator());

            // 编辑列
            contextMenu.Items.Add("编辑列...", null, (s, e) => ShowColumnsEditor());
            contextMenu.Items.Add(new ToolStripSeparator());

            // 常用属性
            contextMenu.Items.Add("启用分页", null, (s, e) => TogglePagination());
            contextMenu.Items.Add("显示行号", null, (s, e) => ToggleRowNumbers());
            contextMenu.Items.Add("交替行颜色", null, (s, e) => ToggleAlternatingColors());
        }

        #endregion

        #region 列管理

        internal void AddColumn(Type columnType)
        {
            if (designerHost == null)
            {
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = designerHost.CreateTransaction($"Add {columnType.Name}");

                // 创建新列
                var column = (FluentDataGridViewColumn)designerHost.CreateComponent(columnType);

                // 设置默认属性
                SetDefaultProperties(column);

                // 添加到集合
                DataGridView.Columns.Add(column);

                // 选中新列
                if (selectionService != null)
                {
                    selectionService.SetSelectedComponents(new object[] { column });
                }

                // 刷新设计器
                DataGridView.Invalidate();

                transaction.Commit();
            }
            catch
            {
                transaction?.Cancel();
                throw;
            }
        }

        private void SetDefaultProperties(FluentDataGridViewColumn column)
        {
            var columnCount = DataGridView.Columns.Count;
            column.Name = $"Column{columnCount}";
            column.HeaderText = $"列{columnCount}";

            if (column is FluentDataGridViewTextBoxColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewComboBoxColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewCheckBoxColumn)
            {
                column.Width = 80;
                column.HeaderTextAlign = HorizontalAlignment.Center;
            }
            else if (column is FluentDataGridViewButtonColumn)
            {
                column.Width = 100;
            }
            else if (column is FluentDataGridViewProgressColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewColorComboBoxColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewImageColumn)
            {
                column.Width = 100;
            }
        }

        internal void ShowColumnsEditor()
        {
            ShowCustomColumnsEditor();
        }

        private void ShowCustomColumnsEditor()
        {
            if (Component.Site == null)
            {
                return;
            }

            IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
            DesignerTransaction transaction = null;

            try
            {
                transaction = host?.CreateTransaction("Edit DataGridView Columns");

                // 创建列集合的副本
                var columnsCopy = new List<FluentDataGridViewColumn>();
                foreach (var column in DataGridView.Columns)
                {
                    columnsCopy.Add(column);
                }

                using (var editorForm = new FluentDataGridViewColumnCollectionEditorForm(
                    DataGridView, columnsCopy, Component.Site))
                {
                    if (editorForm.ShowDialog() == DialogResult.OK)
                    {
                        // 应用更改
                        ApplyColumnChanges(editorForm.GetResultColumns());
                        transaction?.Commit();
                    }
                    else
                    {
                        transaction?.Cancel();
                    }
                }
            }
            catch (Exception ex)
            {
                transaction?.Cancel();
                MessageBox.Show($"编辑列时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyColumnChanges(List<FluentDataGridViewColumn> newColumns)
        {
            IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (host == null)
            {
                return;
            }

            IComponentChangeService changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));

            // 移除不在新列表中的列
            var columnsToRemove = new List<FluentDataGridViewColumn>();
            foreach (var column in DataGridView.Columns)
            {
                if (!newColumns.Contains(column))
                {
                    columnsToRemove.Add(column);
                }
            }

            // 先从集合中移除
            foreach (var column in columnsToRemove)
            {
                DataGridView.Columns.Remove(column);

                // 销毁组件(现在column是Component了)
                try
                {
                    if (column.Site != null)
                    {
                        host.DestroyComponent(column);
                    }
                    else
                    {
                        column.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"销毁列失败: {ex.Message}");
                }
            }

            // 重新排序列
            DataGridView.Columns.Clear();
            foreach (var column in newColumns)
            {
                DataGridView.Columns.Add(column);
            }

            // 通知更改
            try
            {
                PropertyDescriptor columnsProperty = TypeDescriptor.GetProperties(DataGridView)["Columns"];
                changeService?.OnComponentChanged(DataGridView, columnsProperty, null, DataGridView.Columns);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"通知更改失败: {ex.Message}");
            }

            // 刷新设计器
            DataGridView.Refresh();
        }

        internal void ShowColumnsEditorDialog()
        {
            var columnsCopy = new List<FluentDataGridViewColumn>();
            foreach (var column in DataGridView.Columns)
            {
                columnsCopy.Add(column);
            }

            using (var editorForm = new FluentDataGridViewColumnCollectionEditorForm(
                DataGridView, columnsCopy, Component.Site))
            {
                if (editorForm.ShowDialog() == DialogResult.OK)
                {
                    if (designerHost != null)
                    {
                        using (var transaction = designerHost.CreateTransaction("Edit Columns"))
                        {
                            PropertyDescriptor columnsProperty =
                                TypeDescriptor.GetProperties(DataGridView)["Columns"];
                            columnsProperty?.SetValue(DataGridView, DataGridView.Columns);
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        private void TogglePagination()
        {
            if (designerHost == null)
            {
                return;
            }

            using (var transaction = designerHost.CreateTransaction("Toggle Pagination"))
            {
                PropertyDescriptor property = TypeDescriptor.GetProperties(DataGridView)["EnablePagination"];
                property?.SetValue(DataGridView, !DataGridView.EnablePagination);
                transaction.Commit();
            }
        }

        private void ToggleRowNumbers()
        {
            if (designerHost == null)
            {
                return;
            }

            using (var transaction = designerHost.CreateTransaction("Toggle Row Numbers"))
            {
                PropertyDescriptor property = TypeDescriptor.GetProperties(DataGridView)["ShowRowNumbers"];
                property?.SetValue(DataGridView, !DataGridView.ShowRowNumbers);
                transaction.Commit();
            }
        }

        private void ToggleAlternatingColors()
        {
            if (designerHost == null)
            {
                return;
            }

            using (var transaction = designerHost.CreateTransaction("Toggle Alternating Colors"))
            {
                PropertyDescriptor property = TypeDescriptor.GetProperties(DataGridView)["AlternatingRowColors"];
                property?.SetValue(DataGridView, !DataGridView.AlternatingRowColors);
                transaction.Commit();
            }
        }

        #endregion

        #region 属性过滤

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // 为 Columns 属性绑定自定义编辑器
            PropertyDescriptor columnsProp = (PropertyDescriptor)properties["Columns"];
            if (columnsProp != null)
            {
                properties["Columns"] = TypeDescriptor.CreateProperty(
                    typeof(FluentDataGridView),
                    columnsProp,
                    new EditorAttribute(typeof(FluentDataGridViewColumnCollectionEditor),
                        typeof(UITypeEditor)));
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);

            // 设置默认属性
            if (Component is FluentDataGridView grid)
            {
                grid.Dock = DockStyle.Fill;
                grid.ShowRowNumbers = true;
            }
        }

        #endregion

        #region 选择规则

        public override SelectionRules SelectionRules
        {
            get
            {
                return SelectionRules.Visible | SelectionRules.Moveable |
                       SelectionRules.AllSizeable;
            }
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (contextMenu != null)
                {
                    contextMenu.Dispose();
                    contextMenu = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    public class FluentDataGridViewActionList : DesignerActionList
    {
        private FluentDataGridViewDesigner designer;
        private FluentDataGridView dataGridView;

        public FluentDataGridViewActionList(FluentDataGridViewDesigner designer)
            : base(designer.Component)
        {
            this.designer = designer;
            this.dataGridView = designer.DataGridView;
        }

        #region 属性

        [Category("数据")]
        [Description("数据源")]
        public object DataSource
        {
            get => dataGridView.DataSource;
            set => SetProperty("DataSource", value);
        }

        [Category("分页")]
        [Description("是否启用分页")]
        public bool EnablePagination
        {
            get => dataGridView.EnablePagination;
            set => SetProperty("EnablePagination", value);
        }

        [Category("分页")]
        [Description("每页大小")]
        public int PageSize
        {
            get => dataGridView.PageSize;
            set => SetProperty("PageSize", value);
        }

        [Category("外观")]
        [Description("是否显示行号")]
        public bool ShowRowNumbers
        {
            get => dataGridView.ShowRowNumbers;
            set => SetProperty("ShowRowNumbers", value);
        }

        [Category("外观")]
        [Description("是否启用交替行颜色")]
        public bool AlternatingRowColors
        {
            get => dataGridView.AlternatingRowColors;
            set => SetProperty("AlternatingRowColors", value);
        }

        [Category("外观")]
        [Description("是否显示网格线")]
        public bool ShowGridLines
        {
            get => dataGridView.ShowGridLines;
            set => SetProperty("ShowGridLines", value);
        }

        #endregion

        #region 方法

        public void AddTextBoxColumn()
        {
            designer.AddColumn(typeof(FluentDataGridViewTextBoxColumn));
        }

        public void AddComboBoxColumn()
        {
            designer.AddColumn(typeof(FluentDataGridViewComboBoxColumn));
        }

        public void AddCheckBoxColumn()
        {
            designer.AddColumn(typeof(FluentDataGridViewCheckBoxColumn));
        }

        public void AddButtonColumn()
        {
            designer.AddColumn(typeof(FluentDataGridViewButtonColumn));
        }

        public void AddProgressColumn()
        {
            designer.AddColumn(typeof(FluentDataGridViewProgressColumn));
        }

        public void AddColorColumn()
        {
            designer.AddColumn(typeof(FluentDataGridViewColorComboBoxColumn));
        }

        public void EditColumns()
        {
            designer.ShowColumnsEditorDialog();
        }

        private void SetProperty(string propertyName, object value)
        {
            PropertyDescriptor property = TypeDescriptor.GetProperties(dataGridView)[propertyName];
            property?.SetValue(dataGridView, value);
        }

        #endregion

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();

            // 数据
            items.Add(new DesignerActionHeaderItem("数据"));
            items.Add(new DesignerActionPropertyItem("DataSource", "数据源", "数据"));

            // 列
            items.Add(new DesignerActionHeaderItem("列"));
            items.Add(new DesignerActionMethodItem(this, "AddTextBoxColumn", "添加文本列", "列"));
            items.Add(new DesignerActionMethodItem(this, "AddComboBoxColumn", "添加下拉列", "列"));
            items.Add(new DesignerActionMethodItem(this, "AddCheckBoxColumn", "添加复选框列", "列"));
            items.Add(new DesignerActionMethodItem(this, "AddButtonColumn", "添加按钮列", "列"));
            items.Add(new DesignerActionMethodItem(this, "AddProgressColumn", "添加进度条列", "列"));
            items.Add(new DesignerActionMethodItem(this, "AddColorColumn", "添加颜色列", "列"));
            items.Add(new DesignerActionMethodItem(this, "EditColumns", "编辑列...", "列", true));

            // 分页
            items.Add(new DesignerActionHeaderItem("分页"));
            items.Add(new DesignerActionPropertyItem("EnablePagination", "启用分页", "分页"));
            items.Add(new DesignerActionPropertyItem("PageSize", "每页大小", "分页"));

            // 外观
            items.Add(new DesignerActionHeaderItem("外观"));
            items.Add(new DesignerActionPropertyItem("ShowRowNumbers", "显示行号", "外观"));
            items.Add(new DesignerActionPropertyItem("AlternatingRowColors", "交替行颜色", "外观"));
            items.Add(new DesignerActionPropertyItem("ShowGridLines", "显示网格线", "外观"));

            return items;
        }
    }


    public class FluentDataGridViewColumnCollectionEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context,
            IServiceProvider provider, object value)
        {
            if (context != null && context.Instance != null && provider != null)
            {
                var grid = context.Instance as FluentDataGridView;
                if (grid != null)
                {
                    var columnsCopy = new List<FluentDataGridViewColumn>();
                    foreach (var column in grid.Columns)
                    {
                        columnsCopy.Add(column);
                    }

                    using (var editorForm = new FluentDataGridViewColumnCollectionEditorForm(
                        grid, columnsCopy, grid.Site))
                    {
                        if (editorForm.ShowDialog() == DialogResult.OK)
                        {
                            return grid.Columns;
                        }
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }

    public class FluentDataGridViewColumnCollectionEditorForm : Form
    {
        private DataGridViewColumnCollection collection;
        private IServiceProvider serviceProvider;
        private ListBox columnsListBox;
        private PropertyGrid propertyGrid;
        private Button addButton;
        private Button removeButton;
        private Button moveUpButton;
        private Button moveDownButton;
        private Button okButton;
        private Button cancelButton;
        private ComboBox columnTypeComboBox;

        private FluentDataGridView dataGridView;
        private List<FluentDataGridViewColumn> workingColumns;
        private List<FluentDataGridViewColumn> columnsToDestroy;

        public FluentDataGridViewColumnCollectionEditorForm(FluentDataGridView dataGridView,
            List<FluentDataGridViewColumn> columns, IServiceProvider serviceProvider)
        {
            this.dataGridView = dataGridView;
            this.collection = dataGridView.Columns;
            this.workingColumns = new List<FluentDataGridViewColumn>(columns);
            this.columnsToDestroy = new List<FluentDataGridViewColumn>();
            this.serviceProvider = serviceProvider;
            InitializeComponents();
            LoadColumns();
        }

        private void InitializeComponents()
        {
            Text = "FluentDataGridView 列集合编辑器";
            Size = new Size(720, 480);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 400);

            // 创建主TableLayoutPanel
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(5)
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));

            // 左侧面板
            var leftPanel = CreateLeftPanel();
            // 右侧面板
            var rightPanel = CreateRightPanel();
            // 底部按钮
            var dialogButtonPanel = CreateDialogButtonPanel();

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);
            mainLayout.SetColumnSpan(dialogButtonPanel, 2);
            mainLayout.Controls.Add(dialogButtonPanel, 0, 1);

            Controls.Add(mainLayout);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            UpdateButtonStates();
        }

        private Panel CreateLeftPanel()
        {
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));

            var columnsLabel = new Label
            {
                Text = "列:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            columnsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 22,
                IntegralHeight = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            columnsListBox.DrawItem += OnDrawItem;
            columnsListBox.SelectedIndexChanged += OnSelectedColumnChanged;

            var buttonPanel = CreateButtonPanel();

            leftLayout.Controls.Add(columnsLabel, 0, 0);
            leftLayout.Controls.Add(columnsListBox, 0, 1);
            leftLayout.Controls.Add(buttonPanel, 0, 2);

            leftPanel.Controls.Add(leftLayout);
            return leftPanel;
        }

        private Panel CreateButtonPanel()
        {
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            var topButtonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 32,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            columnTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Margin = new Padding(0, 2, 5, 0)
            };
            columnTypeComboBox.Items.AddRange(new string[]
            {
                "TextBox",
                "ComboBox",
                "CheckBox",
                "Button",
                "Label",
                "Image",
                "ColorComboBox",
                "Progress"
            });
            columnTypeComboBox.SelectedIndex = 0;

            addButton = new Button
            {
                Text = "添加",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 0, 0)
            };
            addButton.Click += OnAddColumn;

            topButtonFlow.Controls.Add(columnTypeComboBox);
            topButtonFlow.Controls.Add(addButton);

            var bottomButtonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            removeButton = new Button
            {
                Text = "移除",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 5, 0)
            };
            removeButton.Click += OnRemoveColumn;

            moveUpButton = new Button
            {
                Text = "上移 ↑",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 5, 0)
            };
            moveUpButton.Click += OnMoveUp;

            moveDownButton = new Button
            {
                Text = "下移 ↓",
                Width = 70,
                Height = 24,
                Margin = new Padding(0, 2, 0, 0)
            };
            moveDownButton.Click += OnMoveDown;

            bottomButtonFlow.Controls.Add(removeButton);
            bottomButtonFlow.Controls.Add(moveUpButton);
            bottomButtonFlow.Controls.Add(moveDownButton);

            buttonPanel.Controls.Add(bottomButtonFlow);
            buttonPanel.Controls.Add(topButtonFlow);

            return buttonPanel;
        }

        private Panel CreateRightPanel()
        {
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var propertiesLabel = new Label
            {
                Text = "属性:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                PropertySort = PropertySort.Categorized,
                ToolbarVisible = true
            };

            rightLayout.Controls.Add(propertiesLabel, 0, 0);
            rightLayout.Controls.Add(propertyGrid, 0, 1);

            rightPanel.Controls.Add(rightLayout);
            return rightPanel;
        }

        private Panel CreateDialogButtonPanel()
        {
            var dialogButtonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 5, 5)
            };

            okButton = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Size = new Size(75, 25),
                Anchor = AnchorStyles.Right
            };

            cancelButton = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Size = new Size(75, 25),
                Anchor = AnchorStyles.Right
            };

            var dialogButtonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            dialogButtonFlow.Controls.Add(cancelButton);
            dialogButtonFlow.Controls.Add(okButton);

            dialogButtonPanel.Controls.Add(dialogButtonFlow);
            return dialogButtonPanel;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                ApplyChanges();
            }

            // 清理资源
            if (propertyGrid != null)
            {
                propertyGrid.SelectedObject = null;
            }

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (propertyGrid != null)
                {
                    propertyGrid.SelectedObject = null;
                }

                columnsToDestroy?.Clear();
                workingColumns?.Clear();
            }
            base.Dispose(disposing);
        }

        private void OnDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            var column = columnsListBox.Items[e.Index] as FluentDataGridViewColumn;
            if (column == null)
            {
                return;
            }

            e.DrawBackground();

            string displayText = "";
            string typeText = "";
            Color typeColor = Color.Gray;

            // 获取列类型标识
            if (column is FluentDataGridViewTextBoxColumn)
            {
                typeText = "[Text]";
                typeColor = Color.Blue;
            }
            else if (column is FluentDataGridViewComboBoxColumn)
            {
                typeText = "[Combo]";
                typeColor = Color.Green;
            }
            else if (column is FluentDataGridViewCheckBoxColumn)
            {
                typeText = "[Check]";
                typeColor = Color.Purple;
            }
            else if (column is FluentDataGridViewButtonColumn)
            {
                typeText = "[Button]";
                typeColor = Color.DarkOrange;
            }
            else if (column is FluentDataGridViewLabelColumn)
            {
                typeText = "[Label]";
                typeColor = Color.DarkGreen;
            }
            else if (column is FluentDataGridViewImageColumn)
            {
                typeText = "[Image]";
                typeColor = Color.Brown;
            }
            else if (column is FluentDataGridViewColorComboBoxColumn)
            {
                typeText = "[Color]";
                typeColor = Color.Crimson;
            }
            else if (column is FluentDataGridViewProgressColumn)
            {
                typeText = "[Progress]";
                typeColor = Color.Teal;
            }

            displayText = column.Name;
            if (!string.IsNullOrEmpty(column.HeaderText) && column.HeaderText != column.Name)
            {
                displayText += $" - \"{column.HeaderText}\"";
            }

            // 添加数据属性显示
            if (!string.IsNullOrEmpty(column.DataPropertyName))
            {
                displayText += $" ({column.DataPropertyName})";
            }

            // 绘制类型标识
            var typeRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 3, 70, e.Bounds.Height);
            using (var typeBrush = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ?
                SystemColors.HighlightText : typeColor))
            {
                e.Graphics.DrawString(typeText, e.Font, typeBrush, typeRect);
            }

            // 绘制列信息
            var textRect = new Rectangle(e.Bounds.X + 75, e.Bounds.Y + 3,
                e.Bounds.Width - 75, e.Bounds.Height);
            using (var textBrush = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ?
                SystemColors.HighlightText : SystemColors.ControlText))
            {
                var format = new StringFormat
                {
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                e.Graphics.DrawString(displayText, e.Font, textBrush, textRect, format);
            }

            e.DrawFocusRectangle();
        }

        private void LoadColumns()
        {
            columnsListBox.Items.Clear();
            foreach (var column in collection)
            {
                columnsListBox.Items.Add(column);
            }

            if (columnsListBox.Items.Count > 0)
            {
                columnsListBox.SelectedIndex = 0;
            }

            UpdateButtonStates();
        }

        public List<FluentDataGridViewColumn> GetResultColumns()
        {
            return new List<FluentDataGridViewColumn>(workingColumns);
        }

        private void OnSelectedColumnChanged(object sender, EventArgs e)
        {
            if (columnsListBox.SelectedItem is FluentDataGridViewColumn column)
            {
                propertyGrid.SelectedObject = column;
            }
            else
            {
                propertyGrid.SelectedObject = null;
            }

            UpdateButtonStates();
        }

        private void OnAddColumn(object sender, EventArgs e)
        {
            Type columnType = GetSelectedColumnType();
            if (columnType == null)
            {
                return;
            }

            IDesignerHost host = serviceProvider?.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host != null)
            {
                DesignerTransaction transaction = host.CreateTransaction($"Add {columnType.Name}");
                try
                {
                    string uniqueName = GetUniqueNameFromHost(host, columnType);
                    var column = (FluentDataGridViewColumn)host.CreateComponent(columnType, uniqueName);

                    SetDefaultProperties(column);

                    workingColumns?.Add(column);
                    collection?.Add(column);
                    columnsListBox.Items.Add(column);
                    columnsListBox.SelectedItem = column;

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction?.Cancel();
                    MessageBox.Show($"添加列失败: {ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    var column = (FluentDataGridViewColumn)Activator.CreateInstance(columnType);
                    column.Name = GetSimpleUniqueName(columnType);
                    SetDefaultProperties(column);

                    collection?.Add(column);
                    columnsListBox.Items.Add(column);
                    columnsListBox.SelectedItem = column;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"添加列失败: {ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetUniqueNameFromHost(IDesignerHost host, Type columnType)
        {
            if (host == null)
            {
                return GetSimpleUniqueName(columnType);
            }

            string baseName = GetBaseName(columnType);
            string name;
            int index = 1;

            do
            {
                name = $"{baseName}{index}";
                index++;

                bool existsInHost = host.Container.Components[name] != null;
                bool existsInCollection = collection.Cast<FluentDataGridViewColumn>()
                    .Any(col => string.Equals(col.Name, name, StringComparison.OrdinalIgnoreCase));

                if (!existsInHost && !existsInCollection)
                {
                    break;
                }
            }
            while (index < 1000);

            return name;
        }

        private string GetSimpleUniqueName(Type columnType)
        {
            string baseName = GetBaseName(columnType);
            int index = 1;
            string name;

            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (FluentDataGridViewColumn column in collection)
            {
                if (!string.IsNullOrEmpty(column.Name))
                {
                    existingNames.Add(column.Name);
                }
            }

            do
            {
                name = $"{baseName}{index}";
                index++;
            }
            while (existingNames.Contains(name) && index < 1000);

            return name;
        }

        private string GetBaseName(Type columnType)
        {
            if (columnType == typeof(FluentDataGridViewTextBoxColumn))
            {
                return "columnText";
            }
            else if (columnType == typeof(FluentDataGridViewComboBoxColumn))
            {
                return "columnCombo";
            }
            else if (columnType == typeof(FluentDataGridViewCheckBoxColumn))
            {
                return "columnCheck";
            }
            else if (columnType == typeof(FluentDataGridViewButtonColumn))
            {
                return "columnButton";
            }
            else if (columnType == typeof(FluentDataGridViewLabelColumn))
            {
                return "columnLabel";
            }
            else if (columnType == typeof(FluentDataGridViewImageColumn))
            {
                return "columnImage";
            }
            else if (columnType == typeof(FluentDataGridViewColorComboBoxColumn))
            {
                return "columnColor";
            }
            else if (columnType == typeof(FluentDataGridViewProgressColumn))
            {
                return "columnProgress";
            }
            else
            {
                return "column";
            }
        }

        private void OnRemoveColumn(object sender, EventArgs e)
        {
            if (columnsListBox.SelectedItem is FluentDataGridViewColumn column)
            {
                int index = columnsListBox.SelectedIndex;

                workingColumns?.Remove(column);
                columnsListBox.Items.Remove(column);

                if (!columnsToDestroy.Contains(column))
                {
                    columnsToDestroy.Add(column);
                }

                if (columnsListBox.Items.Count > 0)
                {
                    columnsListBox.SelectedIndex = Math.Min(index, columnsListBox.Items.Count - 1);
                }

                UpdateButtonStates();
            }
        }

        private void OnMoveUp(object sender, EventArgs e)
        {
            int index = columnsListBox.SelectedIndex;
            if (index > 0)
            {
                var column = collection[index];
                collection.RemoveAt(index);
                collection.Insert(index - 1, column);
                LoadColumns();
                columnsListBox.SelectedIndex = index - 1;
            }
        }

        private void OnMoveDown(object sender, EventArgs e)
        {
            int index = columnsListBox.SelectedIndex;
            if (index >= 0 && index < columnsListBox.Items.Count - 1)
            {
                var column = collection[index];
                collection.RemoveAt(index);
                collection.Insert(index + 1, column);
                LoadColumns();
                columnsListBox.SelectedIndex = index + 1;
            }
        }

        private void UpdateButtonStates()
        {
            int index = columnsListBox.SelectedIndex;
            removeButton.Enabled = index >= 0;
            moveUpButton.Enabled = index > 0;
            moveDownButton.Enabled = index >= 0 && index < columnsListBox.Items.Count - 1;
        }

        public void ApplyChanges()
        {
            IDesignerHost host = serviceProvider?.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                // 如果没有设计器主机，则直接应用更改
                ApplyChangesWithoutHost();
                return;
            }

            IComponentChangeService changeService =
                serviceProvider?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;

            try
            {
                // 通知开始更改
                changeService?.OnComponentChanging(dataGridView,
                    TypeDescriptor.GetProperties(dataGridView)["Columns"]);

                // 销毁标记为删除的组件
                foreach (var column in columnsToDestroy)
                {
                    try
                    {
                        // 现在column是Component，可以安全地调用DestroyComponent
                        if (column.Site != null)
                        {
                            host.DestroyComponent(column);
                        }
                        else
                        {
                            column.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"销毁列组件失败: {ex.Message}");
                    }
                }

                // 清空原始集合
                dataGridView.Columns.Clear();

                // 添加工作列表中的列
                foreach (var column in workingColumns)
                {
                    dataGridView.Columns.Add(column);
                }

                // 通知更改完成
                changeService?.OnComponentChanged(dataGridView,
                    TypeDescriptor.GetProperties(dataGridView)["Columns"], null, dataGridView.Columns);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用更改时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyChangesWithoutHost()
        {
            // 在没有设计器主机的情况下
            foreach (var column in columnsToDestroy)
            {
                try
                {
                    column.Dispose();
                }
                catch { }
            }

            dataGridView.Columns.Clear();
            foreach (var column in workingColumns)
            {
                dataGridView.Columns.Add(column);
            }
        }


        private Type GetSelectedColumnType()
        {
            switch (columnTypeComboBox.SelectedItem?.ToString())
            {
                case "TextBox": return typeof(FluentDataGridViewTextBoxColumn);
                case "ComboBox": return typeof(FluentDataGridViewComboBoxColumn);
                case "CheckBox": return typeof(FluentDataGridViewCheckBoxColumn);
                case "Button": return typeof(FluentDataGridViewButtonColumn);
                case "Label": return typeof(FluentDataGridViewLabelColumn);
                case "Image": return typeof(FluentDataGridViewImageColumn);
                case "ColorComboBox": return typeof(FluentDataGridViewColorComboBoxColumn);
                case "Progress": return typeof(FluentDataGridViewProgressColumn);
                default: return null;
            }
        }

        private void SetDefaultProperties(FluentDataGridViewColumn column)
        {
            var columnCount = dataGridView.Columns.Count;
            if (string.IsNullOrEmpty(column.HeaderText))
            {
                column.HeaderText = $"列{columnCount + 1}";
            }

            if (column is FluentDataGridViewTextBoxColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewComboBoxColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewCheckBoxColumn)
            {
                column.Width = 80;
                column.HeaderTextAlign = HorizontalAlignment.Center;
                column.CellAlignment = HorizontalAlignment.Center;
            }
            else if (column is FluentDataGridViewButtonColumn btn)
            {
                column.Width = 100;
                btn.ButtonText = "...";
            }
            else if (column is FluentDataGridViewLabelColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewImageColumn)
            {
                column.Width = 100;
            }
            else if (column is FluentDataGridViewColorComboBoxColumn)
            {
                column.Width = 150;
            }
            else if (column is FluentDataGridViewProgressColumn prog)
            {
                column.Width = 150;
                prog.ShowPercentage = true;
            }
        }
    }

    #endregion
}
