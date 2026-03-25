using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentControls.Controls;
using FluentControls.Themes;
using System.Windows.Forms;
using FluentControls.IconFonts;
using FluentControls;

namespace FluentControls.WinformDemo
{
    public class DemoForm : FluentForm
    {
        #region 成员变量

        // 布局
        private FluentLayoutContainer layoutContainer;

        // 左侧导航
        private FluentNavigationView navigation;

        // 中央区域
        private Panel centerPanel;
        private Label titleLabel;
        private Label descriptionLabel;
        private Panel previewContainer;

        // 右侧属性编辑
        private FluentPropertyGrid propertyGrid;

        // 实时预览
        private Timer applyDebounceTimer;
        private bool isApplying = false;
        private bool hasPendingChanges = false; // 是否有待应用的属性变更

        // 当前状态
        private Control currentPreviewControl;
        private ControlDemoConfigBase currentConfig;
        private string currentControlKey;

        // 控件注册表
        private Dictionary<string, ControlDemoEntry> registry;

        // 图标颜色
        private Color iconColor => ThemeManager.CurrentTheme?.Colors?.TextPrimary
                                    ?? Color.FromArgb(60, 60, 60);

        #endregion

        #region 构造函数

        public DemoForm()
        {
            registry = new Dictionary<string, ControlDemoEntry>();

            SetupForm();
            RegisterAllControls();       // 注册所有控件
            BuildLayoutContainer();      // 三栏布局
            BuildNavigationView();       // 左侧导航
            BuildCenterPanel();          // 中央预览
            BuildPropertyGridPanel();    // 右侧属性
            SetupLivePreviewTimer();     // 实时预览

            // 默认选中第一个
            if (registry.Count > 0)
            {
                SwitchToControl(registry.Keys.First());
            }
        }

        #endregion

        #region 初始化

        private void SetupForm()
        {
            Text = "Fluent 控件库演示";
            Size = new Size(1200, 720);
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(243, 243, 243);
            Font = new Font("Segoe UI", 9f);
            DoubleBuffered = true;
        }

        /// <summary>
        /// 构建三栏布局
        /// </summary>
        private void BuildLayoutContainer()
        {
            int propertyGridPanelWidth = 380;

            layoutContainer = new FluentLayoutContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                LayoutMode = FluentLayoutMode.Horizontal,
                AllowSplitterDrag = true,
                EnableSplitterAnimation = false,
                HorizontalSplitterPixelDistance = 220,
                SecondHorizontalSplitterPixelDistance = this.Width - propertyGridPanelWidth,
                MinimumSize = new Size(200, 160),
                UseTheme = true
            };

            Controls.Add(layoutContainer);
        }

        /// <summary>
        /// 构建左侧导航
        /// </summary>
        private void BuildNavigationView()
        {
            navigation = new FluentNavigationView
            {
                Dock = DockStyle.Fill,
                ShowNavigationArea = true,
                BackColor = Color.White,
                ItemHeight = 36,
                ExpandedWidth = 218,
                NavigationTitle = "控件列表",
                UseTheme = true
            };

            navigation.ExpandedChanged += (s, e) =>
            {
                if (navigation.IsExpanded)
                {
                    layoutContainer.HorizontalSplitterPixelDistance = 220;
                }
                else
                {
                    layoutContainer.HorizontalSplitterPixelDistance = 36;
                }
            };

            navigation.MenuItems.Clear();
            navigation.FooterItems.Clear();

            // 创建所有分类和菜单项
            BuildBasicControlsCategory();
            BuildContainerControlsCategory();
            BuildListControlsCategory();
            BuildDataControlsCategory();
            //BuildLayoutControlsCategory();
            BuildToolbarControlsCategory();
            BuildFormControlsCategory();
            BuildCompositeControlsCategory();
            BuildComponentsCategory();

            layoutContainer.Panel1.Controls.Add(navigation);
        }

        #endregion

        #region 控件构造

        /// <summary>
        /// 快捷创建图标
        /// </summary>
        private System.Drawing.Image Nav(FluentSystemIconsResizableIconChar c, int size = 24)
        {
            try
            {
                return IconFontManager.Instance.GetIcon(IconFontProviderType.FluentSystemIconsResizable, c, size, iconColor);
            }
            catch { return null; }
        }

        /// <summary>
        /// 创建菜单项并关联点击事件
        /// </summary>
        private NavigationMenuItem MI(string controlKey, Image icon = null)
        {
            var item = new NavigationMenuItem(controlKey, icon);
            item.Tag = controlKey;

            // 关联点击事件
            item.Click += (s, e) => SwitchToControl(controlKey);

            return item;
        }

        /// <summary>
        ///  基本控件
        /// </summary>
        private void BuildBasicControlsCategory()
        {
            var cat = new NavigationCategory("基本控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentApps20Regular, 28);

            cat.Items.Add(MI("FluentButton",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentCursor20Regular)));
            cat.Items.Add(MI("FluentTextBox",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentTextbox20Regular)));
            cat.Items.Add(MI("FluentLabel",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentTag20Regular)));
            cat.Items.Add(MI("FluentCheckBox",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentCheckboxChecked20Regular)));
            cat.Items.Add(MI("FluentRadio",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentRadioButton20Regular)));
            cat.Items.Add(MI("FluentComboBox",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentOptions20Regular)));
            cat.Items.Add(MI("FluentSplitButton",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentSplitHint20Regular)));
            cat.Items.Add(MI("FluentSlider",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentOptions20Regular)));
            cat.Items.Add(MI("FluentProgress",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentArrowSync20Regular)));
            cat.Items.Add(MI("FluentDateTimePicker",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentCalendar20Regular)));
            cat.Items.Add(MI("FluentColorPicker",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentColor20Regular)));
            cat.Items.Add(MI("FluentColorComboBox",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentColor20Regular)));
            cat.Items.Add(MI("FluentDropdown",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentChevronDown20Regular)));
            cat.Items.Add(MI("FluentMessage",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentChat20Regular)));
            cat.Items.Add(MI("FluentShape",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentCircle20Regular)));
            //cat.Items.Add(MI("FluentFontIcon",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentTextFont20Regular)));
            //cat.Items.Add(MI("FluentMasker",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentSquare20Regular)));
            //cat.Items.Add(MI("FluentProgressMasker",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentSquare20Regular)));

            navigation.MenuItems.Add(cat);
        }

        /// <summary>
        ///  容器控件
        /// </summary>
        private void BuildContainerControlsCategory()
        {
            var cat = new NavigationCategory("容器控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentGroup20Regular, 28);

            cat.Items.Add(MI("FluentPanel",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentSquare20Regular)));
            cat.Items.Add(MI("FluentTabControl",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentTab20Regular)));
            cat.Items.Add(MI("FluentRepeater",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentCopy20Regular)));

            navigation.MenuItems.Add(cat);
        }

        /// <summary>
        ///  列表控件
        /// </summary>
        private void BuildListControlsCategory()
        {
            var cat = new NavigationCategory("列表控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentList20Regular, 28);

            cat.Items.Add(MI("FluentCheckBoxList",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentCheckboxChecked20Regular)));
            cat.Items.Add(MI("FluentRadioList",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentRadioButton20Regular)));
            cat.Items.Add(MI("FluentListBox",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentList20Regular)));
            cat.Items.Add(MI("FluentTreeView",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentTextBulletListTree20Regular)));
            cat.Items.Add(MI("FluentNavigationView",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentNavigation20Regular)));

            navigation.MenuItems.Add(cat);
        }

        /// <summary>
        ///  数据控件
        /// </summary>
        private void BuildDataControlsCategory()
        {
            var cat = new NavigationCategory("数据控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentTable20Regular, 28);

            cat.Items.Add(MI("FluentListView",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentList20Regular)));
            cat.Items.Add(MI("FluentDataGridView",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentTable20Regular)));

            navigation.MenuItems.Add(cat);
        }

        /// <summary>
        ///  布局控件
        /// </summary>
        private void BuildLayoutControlsCategory()
        {
            var cat = new NavigationCategory("布局控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentBoardSplit20Regular, 28);

            cat.Items.Add(MI("FluentLayoutContainer",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentBoardSplit20Regular)));

            navigation.MenuItems.Add(cat);
        }

        /// <summary>
        ///  工具栏控件
        /// </summary>
        private void BuildToolbarControlsCategory()
        {
            var cat = new NavigationCategory("工具栏控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentRibbon20Regular, 28);

            cat.Items.Add(MI("FluentToolStrip",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentWrench20Regular)));
            cat.Items.Add(MI("FluentMenuStrip",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentNavigation20Regular)));
            cat.Items.Add(MI("FluentStatusStrip",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentInfo20Regular)));

            navigation.MenuItems.Add(cat);
        }

        /// <summary>
        ///  窗体控件
        /// </summary>
        private void BuildFormControlsCategory()
        {
            var cat = new NavigationCategory("窗体控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentWindow20Regular, 28);

            //cat.Items.Add(MI("FluentForm",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentWindow20Regular)));
            cat.Items.Add(MI("FluentDialog",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentChat20Regular)));

            navigation.MenuItems.Add(cat);
        }

        /// <summary>
        ///  组合控件 (10)
        /// </summary>
        private void BuildCompositeControlsCategory()
        {
            var cat = new NavigationCategory("组合控件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentPuzzlePiece20Regular, 28);

            cat.Items.Add(MI("FluentCard",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentContactCard20Regular)));
            //cat.Items.Add(MI("FluentPagination",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentPageFit20Regular)));
            //cat.Items.Add(MI("FluentFileSelect",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentFolder20Regular)));
            //cat.Items.Add(MI("FluentImageViewer",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentImage20Regular)));
            //cat.Items.Add(MI("FluentLogView",
            //    Nav(FluentSystemIconsResizableIconChar.IconIcFluentDocument20Regular)));

            navigation.MenuItems.Add(cat);
        }

        private void BuildComponentsCategory()
        {
            var cat = new NavigationCategory("组件");
            cat.Icon = Nav(FluentSystemIconsResizableIconChar.IconIcFluentPuzzlePiece20Regular, 28);

            cat.Items.Add(MI("FluentTooltip",
                Nav(FluentSystemIconsResizableIconChar.IconIcFluentTooltipQuote20Regular)));
        }

        #endregion

        #region 中央预览区

        private void BuildCenterPanel()
        {
            centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            // 标题
            titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 32, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "选择左侧控件开始预览"
            };
            titleLabel.DoubleClick += (s, e) => ForceApply();

            // 描述
            descriptionLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(120, 120, 120),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = ""
            };

            // 分隔线
            var separator = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(230, 230, 232),
                Margin = new Padding(0, 4, 0, 4)
            };

            // 预览容器
            previewContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 248, 252),
                BorderStyle = BorderStyle.FixedSingle
            };
            //previewContainer.Click += (s, e) => ForceApply();
            previewContainer.Resize += (s, e) => CenterPreviewControl();

            centerPanel.Controls.Add(previewContainer);
            centerPanel.Controls.Add(separator);
            centerPanel.Controls.Add(descriptionLabel);
            centerPanel.Controls.Add(titleLabel);

            // 添加到布局容器中央面板
            layoutContainer.Panel2.Controls.Add(centerPanel);
        }

        #endregion

        #region 右侧属性面板

        private void BuildPropertyGridPanel()
        {
            propertyGrid = new FluentPropertyGrid
            {
                Dock = DockStyle.Fill,
                ShowActionButtons = false,
                GroupByCategory = true,
                UseTheme = true
            };

            layoutContainer.Panel3.Controls.Add(propertyGrid);
        }

        #endregion

        #region 实时预览

        private void SetupLivePreviewTimer()
        {
            applyDebounceTimer = new Timer
            {
                Interval = 400,
                Enabled = false
            };
            applyDebounceTimer.Tick += OnApplyDebounceTimerTick;
            propertyGrid.PropertyValueEdited += OnPropertyGridValueEdited;
        }

        /// <summary>
        /// PropertyGrid 编辑器值变更时触发
        /// </summary>
        private void OnPropertyGridValueEdited(object sender, EventArgs e)
        {
            if (isApplying)
            {
                return;
            }

            hasPendingChanges = true;

            // 重启定时器
            applyDebounceTimer.Stop();
            applyDebounceTimer.Start();
        }

        /// <summary>
        /// 定时器事件
        /// </summary>
        private void OnApplyDebounceTimerTick(object sender, EventArgs e)
        {
            applyDebounceTimer.Stop();

            if (!hasPendingChanges || isApplying ||
                currentConfig == null || currentPreviewControl == null)
            {
                return;
            }

            try
            {
                isApplying = true;
                hasPendingChanges = false;

                if (currentConfig is MessageDemoConfig msgCfg)
                {
                    propertyGrid.SaveDirtyChanges();
                    RebuildMessagePreview(msgCfg);
                    propertyGrid.ClearDirtyFlags();
                    CenterPreviewControl();
                    return;
                }

                // 从控件读取当前状态到config 
                currentConfig.ReadFrom(currentPreviewControl);

                // 将 PropertyGrid 中被编辑过的属性覆盖写入config
                propertyGrid.SaveDirtyChanges();

                // 将合并后的 config 应用到控件
                currentConfig.ApplyTo(currentPreviewControl);

                // 从控件回读
                currentConfig.ReadFrom(currentPreviewControl);

                // 刷新 PropertyGrid 编辑器显示
                propertyGrid.RefreshValues();

                // 清除脏标记
                propertyGrid.ClearDirtyFlags();

                // 重新居中
                CenterPreviewControl();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"实时预览更新失败: {ex.Message}");
            }
            finally
            {
                isApplying = false;
            }
        }

        private void RebuildMessagePreview(MessageDemoConfig cfg)
        {
            // 比较关键属性是否变化
            bool needRebuild = false;

            if (currentPreviewControl is FluentMessage oldMsg)
            {
                var oldOpts = oldMsg.Options;
                needRebuild = oldOpts.Title != cfg.Title
                           || oldOpts.Content != cfg.Content
                           || oldOpts.Type != cfg.MessageType
                           || oldOpts.ShowIcon != cfg.ShowIcon
                           || oldOpts.Closable != cfg.Closable
                           || oldOpts.Width != cfg.Width;

                // 可以直接更新的属性
                if (!needRebuild)
                {
                    oldMsg.ShowBorder = cfg.ShowBorder;
                    oldMsg.BorderWidth = cfg.BorderWidth;
                    oldMsg.BorderColor = cfg.BorderColor;
                    return;
                }
            }
            else
            {
                needRebuild = true;
            }

            if (needRebuild)
            {
                previewContainer.Controls.Clear();
                var newMsg = cfg.CreateMessage();
                currentPreviewControl = newMsg;
                int delay = cfg.Duration > 0 ? cfg.Duration : 2000;

                Task.Delay(delay).ContinueWith(t =>
                {
                    previewContainer.Controls.Add(newMsg);
                });
            }
        }

        /// <summary>
        /// 将预览控件居中显示
        /// </summary>
        private void CenterPreviewControl()
        {
            if (currentPreviewControl == null || previewContainer == null)
            {
                return;
            }

            var ctrl = currentPreviewControl;
            var area = previewContainer.ClientSize;

            ctrl.Location = new Point(
                Math.Max(10, (area.Width - ctrl.Width) / 2),
                Math.Max(10, (area.Height - ctrl.Height) / 2)
            );
        }

        /// <summary>
        /// 强制应用当前 PropertyGrid 的所有值
        /// </summary>
        private void ForceApply()
        {
            if (isApplying || currentConfig == null || currentPreviewControl == null)
            {
                return;
            }

            try
            {
                isApplying = true;

                propertyGrid.SaveChanges();
                currentConfig.ApplyTo(currentPreviewControl);
                currentConfig.ReadFrom(currentPreviewControl);
                propertyGrid.RefreshValues();
                propertyGrid.ClearDirtyFlags();

                CenterPreviewControl();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"强制应用失败: {ex.Message}");
            }
            finally
            {
                isApplying = false;
            }
        }

        #endregion

        #region 控件切换

        /// <summary>
        /// 切换到指定控件的演示
        /// </summary>
        private void SwitchToControl(string controlKey)
        {
            if (string.IsNullOrEmpty(controlKey) || controlKey == currentControlKey)
            {
                return;
            }

            if (!registry.TryGetValue(controlKey, out var entry))
            {
                return;
            }

            // 停止定时器
            applyDebounceTimer.Stop();
            hasPendingChanges = false;

            try
            {
                previewContainer.Controls.Clear();

                titleLabel.Text = entry.DisplayName;
                descriptionLabel.Text = entry.Description;

                currentPreviewControl = entry.CreateControl();

                if (currentPreviewControl != null)
                {
                    previewContainer.Controls.Add(currentPreviewControl);

                    currentConfig = entry.CreateConfig(currentPreviewControl);

                    propertyGrid.SelectedObject = currentConfig;

                    // 清除脏标记
                    propertyGrid.ClearDirtyFlags();

                    CenterPreviewControl();
                }

                currentControlKey = controlKey;
            }
            catch (Exception ex)
            {
                titleLabel.Text = $"{controlKey} — 加载失败";
                descriptionLabel.Text = ex.Message;

                var errorLabel = new Label
                {
                    Text = $"无法创建 {controlKey} 预览\n\n{ex.Message}",
                    AutoSize = true,
                    ForeColor = Color.FromArgb(200, 60, 60),
                    Font = new Font("Segoe UI", 10f)
                };
                previewContainer.Controls.Add(errorLabel);
                CenterPreviewControl();
            }
        }

        #endregion

        #region 控件注册

        private void RegisterAllControls()
        {
            // 基本控件

            Register("FluentButton", "FluentButton", "支持光影效果和动效的按钮控件", "基本控件",
                () => new FluentButton
                {
                    Text = "Fluent Button",
                    Size = new Size(160, 42),
                    ButtonStyle = ButtonStyle.Primary,
                    CornerRadius = 6,
                    UseTheme = true
                },
                ctrl => ButtonDemoConfig.FromControl((FluentButton)ctrl));

            Register("FluentTextBox", "FluentTextBox", "支持输入类型控制及前后缀的文本输入框", "基本控件",
                 () => new FluentTextBox
                 {
                     Text = "",
                     Placeholder = "请输入内容...",
                     Size = new Size(240, 32),
                     ShowBorder = true,
                     UseTheme = true
                 },
                 ctrl => TextBoxDemoConfig.FromControl((FluentTextBox)ctrl));

            Register("FluentLabel", "FluentLabel", "支持多种显示模式的标签控件", "基本控件",
                () => new FluentLabel
                {
                    Text = "Fluent Label 示例文本",
                    Size = new Size(220, 44),
                    CornerRadius = 0,
                    ShowBorder = false,
                    AutoSize = false,
                    BackColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    UseTheme = true
                },
                ctrl => LabelDemoConfig.FromControl((FluentLabel)ctrl));

            Register("FluentCheckBox", "FluentCheckBox", "支持自定义样式的复选框控件", "基本控件",
                () => new FluentCheckBox
                {
                    Text = "复选框选项",
                    Checked = true,
                    Size = new Size(160, 32),
                    UseTheme = true
                },
                ctrl => CheckBoxDemoConfig.FromControl((FluentCheckBox)ctrl));

            Register("FluentRadio", "FluentRadio", "支持组管理的单选按钮控件", "基本控件",
                () => new FluentRadio
                {
                    Text = "单选按钮",
                    Checked = true,
                    Size = new Size(160, 32),
                    UseTheme = true
                },
                ctrl => RadioDemoConfig.FromControl((FluentRadio)ctrl));

            Register("FluentComboBox", "FluentComboBox", "支持下拉选择和搜索功能的组合框控件", "基本控件",
                () =>
                {
                    var cb = new FluentComboBox
                    {
                        Size = new Size(200, 32),
                        UseTheme = true
                    };
                    cb.Items.AddRange(new object[] { "选项 A", "选项 B", "选项 C", "选项 D" });
                    cb.SelectedIndex = 0;
                    return cb;
                },
                ctrl => ComboBoxDemoConfig.FromControl((FluentComboBox)ctrl));

            Register("FluentSplitButton", "FluentSplitButton", "支持分割操作的按钮控件", "基本控件",
                () =>
                {
                    var fsb = new FluentSplitButton
                    {
                        Text = "分割按钮",
                        Size = new Size(150, 36),
                        ItemPosition = FluentSplitButtonItemPosition.Center,
                        DropDownOrientation = FluentSplitButtonOrientation.Vertical,
                        ShowDropDownBorder = false,
                        ItemSpacing = 3,
                        UseTheme = true
                    };
                    fsb.Items.Add(new FluentSplitButtonItem("选项列表", FluentSplitButtonItemType.Label));
                    for (int i = 0; i < 3; i++)
                    {
                        var item = new FluentSplitButtonItem($"选项{i + 1}", FluentSplitButtonItemType.Button);
                        fsb.Items.Add(item);
                    }
                    return fsb;
                },
                ctrl => SplitButtonDemoConfig.FromControl((FluentSplitButton)ctrl));

            Register("FluentSlider", "FluentSlider", "支持滑动选择和动效的滑块控件", "基本控件",
                () => new FluentSlider
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 50,
                    Size = new Size(260, 40),
                    UseTheme = true
                },
                ctrl => SliderDemoConfig.FromControl((FluentSlider)ctrl));

            Register("FluentProgress", "FluentProgress", "支持多种样式的进度条控件", "基本控件",
                () => new FluentProgress
                {
                    Progress = 65,
                    Size = new Size(260, 24),
                    UseTheme = true
                },
                ctrl => ProgressDemoConfig.FromControl((FluentProgress)ctrl));

            Register("FluentDateTimePicker", "FluentDateTimePicker", "支持日期和时间选择的控件", "基本控件",
                () => new FluentDateTimePicker
                {
                    Value = DateTime.Now,
                    Size = new Size(220, 32),
                    UseTheme = true
                },
                ctrl => DateTimePickerDemoConfig.FromControl((FluentDateTimePicker)ctrl));

            Register("FluentColorPicker", "FluentColorPicker", "支持颜色选择和预设的颜色选择器控件", "基本控件",
                () => new FluentColorPicker
                {
                    SelectedColor = Color.CornflowerBlue,
                    Size = new Size(200, 32),
                    UseTheme = true
                },
                ctrl => ColorPickerDemoConfig.FromControl((FluentColorPicker)ctrl));

            Register("FluentColorComboBox", "FluentColorComboBox", "支持颜色选择的组合框控件", "基本控件",
                () => new FluentColorComboBox
                {
                    Size = new Size(200, 32),
                    UseTheme = true
                },
                ctrl => ColorComboBoxDemoConfig.FromControl((FluentColorComboBox)ctrl));


            Register("FluentDropdown", "FluentDropdown", "支持分组及状态勾选的下拉菜单控件", "基本控件",
                () =>
                {
                    var fdd = new FluentDropdown
                    {
                        Text = "下拉菜单",
                        Size = new Size(160, 36),
                        ItemHeight = 30,
                        ItemSpacing = 3,
                        UseTheme = true
                    };
                    for (int i = 0; i < 6; i++)
                    {
                        fdd.Items.Add(new DropdownItem($"选项 {i + 1}"));
                    }
                    return fdd;
                },
                ctrl => DropdownDemoConfig.FromControl((FluentDropdown)ctrl));

            Register("FluentMessage", "FluentMessage", "支持多种消息类型显示的消息控件", "基本控件",
                () =>
                {
                    var opts = new MessageOptions
                    {
                        Title = "操作成功",
                        Content = "数据已保存到数据库。",
                        Type = MessageType.Success,
                        ShowIcon = true,
                        Closable = true,
                        Duration = 0  // 预览模式不自动关闭
                    };
                    return new FluentMessage(opts) { ShowBorder = true };
                },
                ctrl => MessageDemoConfig.FromControl((FluentMessage)ctrl));

            Register("FluentShape", "FluentShape", "支持多种形状绘制的图形控件", "基本控件",
                () => new FluentShape
                {
                    ShapeType = ShapeType.Star,
                    FillColor = Color.FromArgb(0, 120, 212),
                    Size = new Size(120, 120),
                    UseTheme = true
                },
                ctrl => ShapeDemoConfig.FromControl((FluentShape)ctrl));

            Register("FluentFontIcon", "FluentFontIcon", "支持字体图标显示的图标控件", "基本控件",
                () => new FluentFontIcon
                {
                    Size = new Size(48, 48)
                });

            Register("FluentMasker", "FluentMasker", "支持遮罩效果的控件", "基本控件",
                () => new FluentMasker
                {
                    Size = new Size(200, 120)
                });

            Register("FluentProgressMasker", "FluentProgressMasker", "支持进度遮罩效果的控件", "基本控件",
                () => new FluentProgressMasker
                {
                    Size = new Size(200, 120)
                });

            // 容器控件

            Register("FluentPanel", "FluentPanel", "支持多种布局方式的面板控件", "容器控件",
                () =>
                {
                    var panel = new FluentPanel
                    {
                        Size = new Size(300, 200),
                        UseTheme = true
                    };
                    panel.Controls.Add(new Label
                    {
                        Text = "FluentPanel 容器内容",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                    return panel;
                },
                ctrl => PanelDemoConfig.FromControl((FluentPanel)ctrl));

            Register("FluentTabControl", "FluentTabControl", "支持多标签页切换的选项卡控件", "容器控件",
                () =>
                {
                    var tab = new FluentTabControl
                    {
                        Size = new Size(400, 260),
                        UseTheme = true
                    };
                    for (int i = 1; i <= 3; i++)
                    {
                        var page = new FluentTabPage { Text = $"标签页 {i}" };
                        page.Controls.Add(new Label
                        {
                            Text = $"这是第 {i} 个标签页的内容",
                            Dock = DockStyle.Fill,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Segoe UI", 11f)
                        });
                        tab.TabPages.Add(page);
                    }
                    if (tab.TabCount > 0)
                    {
                        tab.SelectedIndex = 0;
                    }

                    return tab;
                },
                ctrl => TabControlDemoConfig.FromControl((FluentTabControl)ctrl));

            Register("FluentRepeater", "FluentRepeater", "支持模板显示的重复控件", "容器控件",
                () =>
                {
                    var repeater = new FluentRepeater
                    {
                        LayoutMode = RepeaterLayoutMode.Horizontal,
                        ItemDefaultSize = new Size(120, 80),
                        ItemPadding = new Padding(10),
                        BackColor = Color.White,
                        MaxItemCount = 20
                    };

                    // 使用控件类型
                    repeater.SetItemFactory(() =>
                    {
                        var btn = new FluentButton
                        {
                            Text = $"按钮 {repeater.ItemCount + 1}",
                            ButtonStyle = ButtonStyle.Primary,
                            CornerRadius = 0,
                            UseTheme = true
                        };
                        return btn;
                    });

                    // 事件处理
                    repeater.ItemAdded += (s, e) => FluentMessageManager.Instance.Info($"Repeater: 添加了第 {e.Index + 1} 个项目");
                    repeater.ItemRemoved += (s, e) => FluentMessageManager.Instance.Info($"Repeater: 移除了第 {e.Index + 1} 个项目");

                    return repeater;
                },
                ctrl => RepeaterDemoConfig.FromControl((FluentRepeater)ctrl));

            // 列表控件

            Register("FluentCheckBoxList", "FluentCheckBoxList", "支持多选列表的复选框列表控件", "列表控件",
                () =>
                {
                    var list = new FluentCheckBoxList
                    {
                        Size = new Size(240, 240),
                        UseTheme = true
                    };
                    foreach (var item in new[] { "苹果", "香蕉", "橙子", "葡萄", "草莓" })
                    {
                        list.Items.Add(new FluentCheckBoxItem { Text = item });
                    }

                    return list;
                },
                ctrl => CheckBoxListDemoConfig.FromControl((FluentCheckBoxList)ctrl));


            Register("FluentRadioList", "FluentRadioList", "支持单选列表的单选按钮列表控件", "列表控件",
                () =>
                {
                    var list = new FluentRadioList
                    {
                        Size = new Size(240, 240),
                        UseTheme = true
                    };
                    foreach (var item in new[] { "小号", "中号", "大号", "超大号" })
                    {
                        list.Items.Add(new FluentRadioItem { Text = item });
                    }

                    if (list.Items.Count > 0)
                    {
                        list.SelectedIndex = 0;
                    }

                    return list;
                },
                ctrl => RadioListDemoConfig.FromControl((FluentRadioList)ctrl));

            Register("FluentListBox", "FluentListBox", "支持多选和自定义样式的列表框控件", "列表控件",
                () =>
                {
                    var lb = new FluentListBox
                    {
                        Size = new Size(240, 200),
                        UseTheme = true
                    };
                    foreach (var city in new[] { "北京", "上海", "广州", "深圳", "杭州", "成都", "武汉", "南京" })
                    {
                        lb.Items.Add(new FluentListItem(city));
                    }

                    return lb;
                },
                ctrl => ListBoxDemoConfig.FromControl((FluentListBox)ctrl));


            Register("FluentTreeView", "FluentTreeView", "支持层级结构显示的树视图控件", "列表控件",
                () =>
                {
                    var tree = new FluentTreeView
                    {
                        Size = new Size(240, 240),
                        UseTheme = true
                    };
                    var root = tree.Nodes.Add("根节点");
                    root.Nodes.Add("子节点 A");
                    var b = root.Nodes.Add("子节点 B");
                    b.Nodes.Add("孙节点 B-1");
                    b.Nodes.Add("孙节点 B-2");
                    root.Nodes.Add("子节点 C");
                    root.Expand();
                    b.Expand();
                    return tree;
                },
                ctrl => TreeViewDemoConfig.FromControl((FluentTreeView)ctrl));

            Register("FluentNavigationView", "FluentNavigationView", "支持导航菜单显示的控件", "列表控件",
                () =>
                {
                    var nav = new FluentNavigationView
                    {
                        Size = new Size(200, 280),
                        ShowNavigationArea = true,
                        ExpandedWidth = 200,
                        UseTheme = true
                    };
                    var demoCat = new NavigationCategory("示例分类");
                    demoCat.Items.Add(new NavigationMenuItem("菜单项 1", null));
                    demoCat.Items.Add(new NavigationMenuItem("菜单项 2", null));
                    demoCat.Items.Add(new NavigationMenuItem("菜单项 3", null));
                    nav.MenuItems.Add(demoCat);
                    return nav;
                });

            // 数据控件

            Register("FluentListView", "FluentListView", "支持多列显示和排序的列表视图控件", "数据控件",
                 () =>
                 {
                     var lv = new FluentListView
                     {
                         Size = new Size(480, 300),
                         View = FluentListViewMode.Details,
                         UseTheme = true
                     };
                     lv.Columns.Add(new FluentListViewColumn("名称", 140));
                     lv.Columns.Add(new FluentListViewColumn("类型", 80));
                     lv.Columns.Add(new FluentListViewColumn("大小", 80));
                     lv.Columns.Add(new FluentListViewColumn("修改日期", 120));
                     lv.Items.Add(new FluentListViewItem(new[] { "报告.docx", "Word", "256 KB", "2024-01-15" }));
                     lv.Items.Add(new FluentListViewItem(new[] { "数据.xlsx", "Excel", "128 KB", "2024-02-20" }));
                     lv.Items.Add(new FluentListViewItem(new[] { "照片.png", "PNG", "1.2 MB", "2024-03-10" }));
                     lv.Items.Add(new FluentListViewItem(new[] { "备份.zip", "ZIP", "45 MB", "2024-04-05" }));
                     lv.Items.Add(new FluentListViewItem(new[] { "笔记.txt", "Text", "12 KB", "2024-05-18" }));
                     return lv;
                 },
                 ctrl => ListViewDemoConfig.FromControl((FluentListView)ctrl));

            Register("FluentDataGridView", "FluentDataGridView", "支持数据绑定和编辑的数据网格视图控件", "数据控件",
                () =>
                {
                    var dgv = new FluentDataGridView
                    {
                        Size = new Size(480, 260),
                        UseTheme = true
                    };
                    dgv.Columns.Add(new FluentDataGridViewTextBoxColumn("Name", "姓名") { Width = 100 });
                    dgv.Columns.Add(new FluentDataGridViewTextBoxColumn("Age", "年龄") { Width = 60 });
                    dgv.Columns.Add(new FluentDataGridViewTextBoxColumn("City", "城市") { Width = 100 });
                    dgv.Columns.Add(new FluentDataGridViewTextBoxColumn("Email", "邮箱") { Width = 160 });
                    dgv.DataSource = new[]
                    {
                        new { Name = "张三", Age = 28, City = "北京", Email = "zhangsan@test.com" },
                        new { Name = "李四", Age = 32, City = "上海", Email = "lisi@test.com" },
                        new { Name = "王五", Age = 25, City = "广州", Email = "wangwu@test.com" },
                        new { Name = "赵六", Age = 30, City = "深圳", Email = "zhaoliu@test.com" },
                        new { Name = "孙七", Age = 27, City = "杭州", Email = "sunqi@test.com" },
                    };
                    return dgv;
                },
                ctrl => DataGridViewDemoConfig.FromControl((FluentDataGridView)ctrl));

            // 布局控件

            RegisterSpecial("FluentLayoutContainer", "FluentLayoutContainer",
                "支持多种布局方式的布局容器控件", "布局控件",
                "布局容器控件需在完整窗体中使用, 点击按钮查看示例");

            RegisterSpecial("FluentDockablePanel", "FluentDockablePanel",
                "支持停靠和浮动的面板控件", "布局控件",
                "停靠面板需在完整窗体中使用, 点击按钮查看示例");

            // 工具栏控件

            Register("FluentToolStrip", "FluentToolStrip", "支持自定义样式和行为的工具栏控件", "工具栏控件",
                () =>
                {
                    var strip = new FluentToolStrip
                    {
                        Dock = DockStyle.Top,
                        UseTheme = true
                    };
                    strip.UseTheme = true;
                    strip.Items.Add(new FluentToolStripLabel { Text = "工具栏" });
                    strip.Items.Add(new FluentToolStripButton { Text = "新建" });
                    strip.Items.Add(new FluentToolStripSeparator());
                    strip.Items.Add(new FluentToolStripCheckBox { Text = "复选框" });
                    return strip;
                },
                ctrl => ToolStripDemoConfig.FromControl((FluentToolStrip)ctrl));

            Register("FluentMenuStrip", "FluentMenuStrip", "支持多级菜单显示的菜单栏控件", "工具栏控件",
                () =>
                {
                    var ms = new FluentMenuStrip
                    {
                        Size = new Size(360, 28),
                        UseTheme = true
                    };
                    ms.Items.Add("文件");
                    ms.Items.Add("编辑");
                    ms.Items.Add("视图");
                    ms.Items.Add("帮助");
                    return ms;
                },
                ctrl => MenuStripDemoConfig.FromControl((FluentMenuStrip)ctrl));

            Register("FluentStatusStrip", "FluentStatusStrip", "支持状态显示的状态栏控件", "工具栏控件",
                () =>
                {
                    var ss = new FluentStatusStrip
                    {
                        Size = new Size(400, 26),
                        UseTheme = true
                    };
                    ss.Items.Add(new FluentToolStripLabel { Text = "就绪" });
                    ss.Items.Add(new FluentToolStripLabel { Text = "行: 1  列: 1", Alignment = FluentToolStripItemAlignment.Right });
                    return ss;
                },
                ctrl => StatusStripDemoConfig.FromControl((FluentStatusStrip)ctrl));

            // 窗体控件

            RegisterFormDemo("FluentForm", "FluentForm", "支持自定义样式和行为的窗体控件", "窗体控件",
                () => { var f = new FluentForm { Text = "FluentForm 示例", Size = new Size(480, 320) }; f.Show(); });

            RegisterFormDemo("FluentDialog", "FluentDialog", "支持多种显示样式的对话框控件", "窗体控件",
                () => FluentDialog.Show("这是一个 FluentDialog 示例", "对话框演示", DialogType.Information));

            // 组合控件

            Register("FluentCard", "FluentCard", "支持卡片式显示的组合控件", "组合控件",
                () => new FluentCard
                {
                    Title = "卡片标题",
                    Subtitle = "这是一段副标题描述文字",
                    Size = new Size(280, 160),
                    UseTheme = true
                },
                ctrl => CardDemoConfig.FromControl((FluentCard)ctrl));

            Register("FluentPagination", "FluentPagination", "支持分页显示和导航的分页控件", "组合控件",
                () => new FluentPagination
                {
                    CurrentPage = 1,
                    Size = new Size(360, 40),
                    UseTheme = true
                });

            Register("FluentFileSelect", "FluentFileSelect", "支持文件选择的组合控件", "组合控件",
                () => new FluentFileSelect
                {
                    Size = new Size(320, 40),
                    UseTheme = true
                },
                ctrl => FileSelectDemoConfig.FromControl((FluentFileSelect)ctrl));

            Register("FluentImageViewer", "FluentImageViewer", "支持图像查看和基本编辑的图像查看器控件", "组合控件",
                () => new FluentImageViewer
                {
                    Size = new Size(320, 240),
                    UseTheme = true
                });

            Register("FluentLogView", "FluentLogView", "支持日志显示和过滤的日志视图控件", "组合控件",
                () => new FluentLogView
                {
                    Size = new Size(400, 240),
                    UseTheme = true
                });

            // 组件
            Register("FluentTooltip", "FluentTooltip", "Fluent风格的工具提示组件", "组件",
                () =>
                {
                    var btn = new FluentButton
                    {
                        Text = "悬停查看 Tooltip 效果",
                        Size = new Size(220, 44),
                        ButtonStyle = ButtonStyle.Secondary,
                        CornerRadius = 0,
                        UseTheme = true
                    };
                    var tooltip = new FluentTooltip();
                    tooltip.SetTooltipTitle(btn, "提示标题");
                    tooltip.SetTooltipText(btn, "这是一段工具提示内容, 用于演示FluentTooltip的效果。");
                    btn.Tag = tooltip;
                    return btn;
                },
                ctrl => TooltipDemoConfig.FromControl(ctrl));
        }

        #endregion

        #region 注册辅助

        /// <summary>
        /// 注册普通控件
        /// </summary>
        private void Register(string key, string displayName, string description, string category, Func<Control> createControl,
            Func<Control, ControlDemoConfigBase> createConfig = null)
        {
            registry[key] = new ControlDemoEntry
            {
                Key = key,
                DisplayName = displayName,
                Description = description,
                Category = category,
                CreateControl = createControl,
                CreateConfig = createConfig ?? FallbackConfig
            };
        }

        /// <summary>
        /// 注册窗体类控件
        /// </summary>
        private void RegisterFormDemo(string key, string displayName, string description, string category, Action openAction)
        {
            registry[key] = new ControlDemoEntry
            {
                Key = key,
                DisplayName = displayName,
                Description = description,
                Category = category,
                IsSpecialDemo = true,
                CreateControl = () =>
                {
                    var panel = new Panel
                    {
                        Size = new Size(300, 120),
                        BackColor = Color.Transparent
                    };

                    var infoLabel = new Label
                    {
                        Text = $"窗体类控件无法在此处预览\n请点击下方按钮打开示例窗体",
                        AutoSize = false,
                        Dock = DockStyle.Top,
                        Height = 60,
                        ForeColor = Color.FromArgb(100, 100, 100),
                        Font = new Font("Segoe UI", 9.5f),
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    var openBtn = new FluentButton
                    {
                        Text = $"打开 {displayName}",
                        Size = new Size(200, 40),
                        Location = new Point(50, 68),
                        ButtonStyle = ButtonStyle.Primary,
                        CornerRadius = 6,
                        UseTheme = true
                    };
                    openBtn.Click += (s, e) =>
                    {
                        try { openAction?.Invoke(); }
                        catch (Exception ex)
                        {
                            FluentDialog.Show($"打开失败: {ex.Message}", "错误", DialogType.Error);
                        }
                    };

                    panel.Controls.Add(openBtn);
                    panel.Controls.Add(infoLabel);
                    return panel;
                },
                CreateConfig = ctrl =>
                {
                    var cfg = new GenericDemoConfig();
                    return cfg;
                }
            };
        }

        /// <summary>
        /// 注册需要特殊展示的控件
        /// </summary>
        private void RegisterSpecial(string key, string displayName, string description, string category, string hint)
        {
            RegisterFormDemo(key, displayName, description, category, () =>
            {
                FluentDialog.Show(hint, displayName, DialogType.Information);
            });
        }

        private ControlDemoConfigBase FallbackConfig(Control ctrl)
        {
            var cfg = new GenericDemoConfig();
            cfg.ReadFrom(ctrl);
            return cfg;
        }

        #endregion

        #region 释放

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                applyDebounceTimer?.Stop();
                applyDebounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion


    }

}
