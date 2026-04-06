using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Image = System.Drawing.Image;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FluentControls.IconFonts
{
    /// <summary>
    /// 字体图标Image编辑器
    /// </summary>
    public class IconFontImageEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider == null)
            {
                return value;
            }

            Image result = value as Image;

            using (var dialog = new IconFontImageSelectorDialog(context, value as Image))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedImage = dialog.SelectedImage;

                    if (selectedImage == null)
                    {
                        return null;
                    }

                    // 验证图片有效性
                    try
                    {
                        var testWidth = selectedImage.Width;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[EditValue] Dialog returned INVALID image: {ex.Message}");
                        return value; // 返回原值
                    }

                    // 再次创建独立副本
                    result = CreateFinalCopy(selectedImage);

                    // 验证最终副本
                    try
                    {
                        var testWidth = result.Width;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[EditValue] Final copy is INVALID: {ex.Message}");
                        return value;
                    }
                }
            }

            // Dialog 已经 Dispose, 再次验证结果
            if (result != null && result != value)
            {
                try
                {
                    var testWidth = result.Width;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EditValue] After dialog dispose, result is INVALID: {ex.Message}");
                    return value;
                }
            }

            return result ?? value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is Image image)
            {
                try
                {
                    e.Graphics.DrawImage(image, e.Bounds);
                }
                catch (ArgumentException)
                {
                    using (var brush = new SolidBrush(Color.Red))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }
            }
        }

        /// <summary>
        /// 创建最终的独立图片副本
        /// </summary>
        private Image CreateFinalCopy(Image source)
        {
            if (source == null)
            {
                return null;
            }

            // 通过字节数组完全隔离
            using (var ms = new MemoryStream())
            {
                source.Save(ms, ImageFormat.Png);
                var bytes = ms.ToArray();

                // 创建新的 stream
                var newStream = new MemoryStream(bytes);
                return Image.FromStream(newStream);
            }
        }

    }

    /// <summary>
    /// 字体图标Image选择对话框
    /// </summary>
    public class IconFontImageSelectorDialog : Form
    {
        private ITypeDescriptorContext context;
        private ProjectResourceLocator resourceLocator;
        private string projectPath;

        private TabControl tabControl;
        private Image selectedImage;
        private Image originalImage;

        // 本地文件
        private PictureBox localFilePreview;
        private Label localFileInfo;
        private Button btnBrowse;

        // 项目资源
        private TreeView resourceTreeView;
        private PictureBox resourcePreview;
        private Label resourceInfo;
        private Button btnImportToResource;
        private ComboBox cmbResourceFiles;


        // 字体图标
        private ComboBox fontFamilyCombo;
        private ListBox iconListBox;
        private TextBox iconSearchBox;
        private PictureBox iconPreviewBox;
        private NumericUpDown iconSizeNumeric;
        private Button iconColorButton;
        private NumericUpDown iconRotationNumeric;
        private Label lblIconInfo;
        private Color iconColor = Color.Black;
        private IIconFontProvider currentProvider;
        private Type currentEnumType;

        public Image SelectedImage => selectedImage;

        public IconFontImageSelectorDialog(ITypeDescriptorContext context, Image currentImage)
        {
            this.context = context;

            // 复制当前图像,避免对原图像的引用
            if (currentImage != null)
            {
                this.originalImage = CreateIndependentCopy(currentImage);
                this.selectedImage = CreateIndependentCopy(currentImage);
            }
            else
            {
                this.originalImage = null;
                this.selectedImage = null;
            }

            resourceLocator = new ProjectResourceLocator(context);
            // 获取项目路径
            projectPath = resourceLocator.GetProjectPath();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Size = new Size(900, 650);
            Text = "选择图像资源";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            MaximizeBox = false;

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei", 9f)
            };

            // 三个标签页
            var localTab = new TabPage("本地文件");
            InitializeLocalFileTab(localTab);
            tabControl.TabPages.Add(localTab);

            var resourceTab = new TabPage("项目资源");
            InitializeProjectResourceTab(resourceTab);
            tabControl.TabPages.Add(resourceTab);

            var iconFontTab = new TabPage("字体图标");
            InitializeIconFontTab(iconFontTab);
            tabControl.TabPages.Add(iconFontTab);

            // 按钮面板
            var buttonPanel = CreateButtonPanel();

            Controls.Add(tabControl);
            Controls.Add(buttonPanel);

            this.Load += (s, e) =>
            {

                var sc1 = resourceTab.FindChild<SplitContainer>();
                if (sc1 != null)
                {
                    sc1.SplitterDistance = Math.Max(200, this.Width - 400);
                }

                var sc2 = iconFontTab.FindChild<SplitContainer>();
                if (sc2 != null)
                {
                    sc2.SplitterDistance = Math.Max(100, this.Width - 300);
                }

            };
        }

        /// <summary>
        /// 创建完全独立的图片副本
        /// </summary>
        private static Image CreateIndependentCopy(Image source)
        {
            if (source == null)
            {
                return null;
            }

            try
            {
                // 使用 MemoryStream + PNG 格式确保完全独立
                using (var ms = new MemoryStream())
                {
                    source.Save(ms, ImageFormat.Png);
                    var data = ms.ToArray();

                    // 从字节数组创建新的 MemoryStream
                    var newStream = new MemoryStream(data);
                    return Image.FromStream(newStream);
                }
            }
            catch
            {
                // 备用方案：像素级复制
                try
                {
                    var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(source, 0, 0, source.Width, source.Height);
                    }
                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }
        }

        #region 本地文件标签页

        private void InitializeLocalFileTab(TabPage tab)
        {
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // 顶部：按钮区
            var topPanel = new Panel { Dock = DockStyle.Fill };

            btnBrowse = new Button
            {
                Text = "浏览本地文件...",
                Size = new Size(150, 35),
                Location = new Point(10, 10)
            };
            btnBrowse.Click += BtnBrowse_Click;

            localFileInfo = new Label
            {
                Text = "未选择文件",
                Location = new Point(170, 18),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            topPanel.Controls.AddRange(new Control[] { btnBrowse, localFileInfo });
            mainContainer.Controls.Add(topPanel, 0, 0);

            // 底部：预览区
            var previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            localFilePreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            previewPanel.Controls.Add(localFilePreview);
            mainContainer.Controls.Add(previewPanel, 0, 1);

            // 如果有当前图片, 显示它
            if (originalImage != null)
            {
                localFilePreview.Image = new Bitmap(originalImage);
                localFileInfo.Text = $"当前图像: {originalImage.Width} x {originalImage.Height}";
                localFileInfo.ForeColor = Color.Black;
            }

            tab.Controls.Add(mainContainer);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.ico|所有文件|*.*";
                dialog.Title = "选择图像文件";
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var image = Image.FromFile(dialog.FileName);

                        localFilePreview.Image?.Dispose();
                        localFilePreview.Image = image;

                        selectedImage?.Dispose();
                        selectedImage = new Bitmap(image);

                        var fileInfo = new FileInfo(dialog.FileName);
                        localFileInfo.Text = $"{fileInfo.Name} ({image.Width} x {image.Height}, {fileInfo.Length / 1024} KB)";
                        localFileInfo.ForeColor = Color.Black;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载图像失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion

        #region 项目资源标签页

        private void InitializeProjectResourceTab(TabPage tab)
        {
            // 使用 TableLayoutPanel 作为主容器
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(5)
            };

            // 固定高度工具栏
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            // 自动填充剩余空间
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            #region 顶部工具栏

            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            var lblResourceFile = new Label
            {
                Text = "资源文件:",
                Location = new Point(5, 15),
                AutoSize = true
            };

            cmbResourceFiles = new ComboBox
            {
                Location = new Point(80, 12),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            cmbResourceFiles.SelectedIndexChanged += CmbResourceFiles_SelectedIndexChanged;

            toolbarPanel.Controls.AddRange(new Control[]
            {
                lblResourceFile,
                cmbResourceFiles,
                btnImportToResource
            });

            mainLayout.Controls.Add(toolbarPanel, 0, 0);

            #endregion

            #region 资源列表和预览区

            var contentSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 280,
                SplitterWidth = 2,
                BorderStyle = BorderStyle.FixedSingle
            };

            // === 左侧资源树 ===
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            resourceTreeView = new TreeView
            {
                Location = new Point(5, 30),
                Size = new Size(340, 400),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(5, 30, 5, 5),
                ImageList = CreateResourceImageList(),
                ShowNodeToolTips = true,
                HideSelection = false
            };
            resourceTreeView.AfterSelect += ResourceTreeView_AfterSelect;
            resourceTreeView.DoubleClick += ResourceTreeView_DoubleClick;

            // 再添加 TreeView, 它会填充剩余空间
            leftPanel.Controls.Add(resourceTreeView);

            contentSplitter.Panel1.Controls.Add(leftPanel);

            // 右侧预览区
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var previewGroup = new GroupBox
            {
                Text = "预览",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var previewLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // 图片预览
            previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // 信息区

            resourcePreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            resourceInfo = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8.5f),
                Padding = new Padding(5),
                ForeColor = Color.Gray,
                BackColor = Color.WhiteSmoke
            };

            previewLayout.Controls.Add(resourcePreview, 0, 0);
            previewLayout.Controls.Add(resourceInfo, 0, 1);

            previewGroup.Controls.Add(previewLayout);
            rightPanel.Controls.Add(previewGroup);

            contentSplitter.Panel2.Controls.Add(rightPanel);

            mainLayout.Controls.Add(contentSplitter, 0, 1);

            #endregion

            tab.Controls.Add(mainLayout);

            // 加载资源文件列表
            LoadResourceFiles();
        }

        private ImageList CreateResourceImageList()
        {
            var imageList = new ImageList { ImageSize = new Size(16, 16) };

            // 添加图标
            imageList.Images.Add("folder", SystemIcons.Shield.ToBitmap());
            imageList.Images.Add("image", SystemIcons.Information.ToBitmap());

            return imageList;
        }

        private void LoadResourceFiles()
        {
            cmbResourceFiles.Items.Clear();

            if (string.IsNullOrEmpty(projectPath))
            {
                cmbResourceFiles.Items.Add("(无法定位项目路径 - 点击此处手动选择)");
                cmbResourceFiles.Tag = "manual";
                cmbResourceFiles.SelectedIndex = 0;
                return;
            }

            var resxFiles = FindResxFiles(projectPath);


            foreach (var resxFile in resxFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(resxFile);
                var relativePath = resxFile.Replace(projectPath, "").TrimStart('\\', '/');
                var isPropertiesResources = resxFile.Contains("\\Properties\\") &&
                                           fileName.Equals("Resources", StringComparison.OrdinalIgnoreCase);

                cmbResourceFiles.Items.Add(new ResourceFileInfo
                {
                    BaseName = fileName,
                    DisplayName = isPropertiesResources ? "Properties.Resources ★" : relativePath,
                    ResxFilePath = resxFile,
                    IsPropertiesResources = isPropertiesResources
                });
            }

            if (cmbResourceFiles.Items.Count > 0)
            {
                // Properties.Resources 优先
                var propertiesRes = cmbResourceFiles.Items.Cast<object>()
                    .OfType<ResourceFileInfo>()
                    .FirstOrDefault(r => r.IsPropertiesResources);

                cmbResourceFiles.SelectedItem = propertiesRes ?? cmbResourceFiles.Items[0];
            }
            else
            {
                cmbResourceFiles.Items.Add("(未找到 .resx 文件)");
                cmbResourceFiles.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 查找所有 .resx 文件
        /// </summary>
        public List<string> FindResxFiles(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
            {
                return new List<string>();
            }

            try
            {
                return Directory.GetFiles(projectPath, "*.resx", SearchOption.AllDirectories)
                    .Where(f => !IsExcludedPath(f))
                    .OrderBy(f => f)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"查找 .resx 文件失败: {ex.Message}");
                return new List<string>();
            }
        }


        private bool IsExcludedPath(string path)
        {
            var excludedFolders = new[] { "bin", "obj" };
            var attributes = File.GetAttributes(path);

            if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                return true;
            }

            return excludedFolders.Any(folder =>
                path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Contains(folder, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 从 .resx 文件加载资源
        /// </summary>
        private void LoadResourcesFromResxFile(string resxFilePath)
        {
            resourceTreeView.Nodes.Clear();

            if (!File.Exists(resxFilePath))
            {
                MessageBox.Show($"资源文件不存在: {resxFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var reader = new ResXResourceReader(resxFilePath))
                {
                    reader.BasePath = Path.GetDirectoryName(resxFilePath);
                    reader.UseResXDataNodes = true;

                    var imageNode = new TreeNode("图像资源") { ImageKey = "folder", SelectedImageKey = "folder" };
                    var otherNode = new TreeNode("其他资源") { ImageKey = "folder", SelectedImageKey = "folder" };

                    int imageCount = 0;
                    int otherCount = 0;
                    int errorCount = 0;
                    foreach (DictionaryEntry entry in reader)
                    {
                        var key = entry.Key.ToString();
                        var dataNode = entry.Value as ResXDataNode;

                        if (dataNode == null)
                        {
                            continue;
                        }

                        try
                        {
                            // 获取文件引用信息
                            var fileRef = dataNode.FileRef;

                            if (fileRef != null)
                            {
                                // 构造完整路径
                                var fullPath = Path.Combine(reader.BasePath, fileRef.FileName);
                            }

                            // 尝试获取值
                            object value = null;

                            try
                            {
                                // 方法1: 使用 GetValue
                                value = dataNode.GetValue((ITypeResolutionService)null);
                            }
                            catch (Exception ex)
                            {
                                // 方法2: 如果是文件引用, 手动加载
                                if (fileRef != null)
                                {
                                    value = LoadResourceFromFileRef(reader.BasePath, fileRef);
                                }
                            }

                            if (value != null)
                            {
                                if (value is byte[] bytes)
                                {
                                    var bImage = ToImage(bytes);
                                    var treeNode = new TreeNode(key)
                                    {
                                        ImageKey = "image",
                                        SelectedImageKey = "image",
                                        Tag = new ResourceItemInfo
                                        {
                                            Name = key,
                                            Value = bImage,
                                            ResourceType = ResourceItemType.ResxFile,
                                            ResxFilePath = resxFilePath
                                        },
                                        ToolTipText = $"{bImage.Width} x {bImage.Height}, {bImage.PixelFormat}"
                                    };
                                    imageNode.Nodes.Add(treeNode);
                                    imageCount++;
                                }
                                else if (value is Image image)
                                {
                                    var treeNode = new TreeNode(key)
                                    {
                                        ImageKey = "image",
                                        SelectedImageKey = "image",
                                        Tag = new ResourceItemInfo
                                        {
                                            Name = key,
                                            Value = image,
                                            ResourceType = ResourceItemType.ResxFile,
                                            ResxFilePath = resxFilePath
                                        },
                                        ToolTipText = $"{image.Width} x {image.Height}, {image.PixelFormat}"
                                    };
                                    imageNode.Nodes.Add(treeNode);
                                    imageCount++;
                                }
                                else if (value is Icon icon)
                                {
                                    var bitmap = icon.ToBitmap();
                                    var treeNode = new TreeNode(key)
                                    {
                                        ImageKey = "image",
                                        SelectedImageKey = "image",
                                        Tag = new ResourceItemInfo
                                        {
                                            Name = key,
                                            Value = bitmap,
                                            ResourceType = ResourceItemType.ResxFile,
                                            ResxFilePath = resxFilePath
                                        },
                                        ToolTipText = $"{bitmap.Width} x {bitmap.Height}, Icon"
                                    };
                                    imageNode.Nodes.Add(treeNode);
                                    imageCount++;
                                }
                                else if (value is Bitmap bitmap)
                                {
                                    var treeNode = new TreeNode(key)
                                    {
                                        ImageKey = "image",
                                        SelectedImageKey = "image",
                                        Tag = new ResourceItemInfo
                                        {
                                            Name = key,
                                            Value = bitmap,
                                            ResourceType = ResourceItemType.ResxFile,
                                            ResxFilePath = resxFilePath
                                        },
                                        ToolTipText = $"{bitmap.Width} x {bitmap.Height}, Bitmap"
                                    };
                                    imageNode.Nodes.Add(treeNode);
                                    imageCount++;
                                }
                                else
                                {
                                    var typeName = value.GetType().Name;
                                    otherNode.Nodes.Add(new TreeNode($"{key} ({typeName})")
                                    {
                                        ToolTipText = value.GetType().FullName
                                    });
                                    otherCount++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorNode = new TreeNode($"{key} (加载失败)")
                            {
                                ForeColor = Color.Red,
                                ToolTipText = ex.Message
                            };
                            otherNode.Nodes.Add(errorNode);
                            errorCount++;
                        }
                    }

                    // 添加节点到树
                    if (imageNode.Nodes.Count > 0)
                    {
                        imageNode.Text = $"图像资源 ({imageCount})";
                        resourceTreeView.Nodes.Add(imageNode);
                        imageNode.Expand();
                    }

                    if (otherNode.Nodes.Count > 0)
                    {
                        otherNode.Text = $"其他资源 ({otherCount + errorCount})";
                        resourceTreeView.Nodes.Add(otherNode);
                    }

                    if (imageNode.Nodes.Count == 0 && otherNode.Nodes.Count == 0)
                    {
                        resourceTreeView.Nodes.Add(new TreeNode("(此资源文件中没有资源)"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取资源文件失败:\n{ex.Message}\n\n详细信息请查看输出窗口。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Image ToImage(byte[] data)
        {

            return CreateImageFromBytes(data);
            //if (data == null || data.Length == 0)
            //{
            //    return null;
            //}
            //using (var ms = new MemoryStream(data))
            //{
            //    return Image.FromStream(ms);
            //}
        }

        /// <summary>
        /// 从文件引用加载资源
        /// </summary>
        private object LoadResourceFromFileRef(string basePath, ResXFileRef fileRef)
        {
            try
            {
                // 构造完整路径
                var fullPath = Path.Combine(basePath, fileRef.FileName);

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                // 根据类型加载
                var typeName = fileRef.TypeName;

                if (typeName.Contains("System.Drawing.Bitmap") || typeName.Contains("System.Drawing.Image"))
                {
                    return Image.FromFile(fullPath);
                }
                else if (typeName.Contains("System.Drawing.Icon"))
                {
                    return new Icon(fullPath);
                }
                else if (typeName.Contains("System.String"))
                {
                    return File.ReadAllText(fullPath);
                }
                else if (typeName.Contains("System.Byte[]"))
                {
                    return File.ReadAllBytes(fullPath);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void CmbResourceFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbResourceFiles.SelectedItem is ResourceFileInfo resFileInfo)
            {
                if (!string.IsNullOrEmpty(resFileInfo.ResxFilePath))
                {
                    LoadResourcesFromResxFile(resFileInfo.ResxFilePath);
                }
            }
        }


        private void ResourceTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            resourcePreview.Image?.Dispose();
            resourcePreview.Image = null;
            resourceInfo.Text = "";

            if (e.Node?.Tag is ResourceItemInfo itemInfo && itemInfo.Value is Image image)
            {
                try
                {
                    resourcePreview.Image = new Bitmap(image);
                    resourceInfo.Text = $"名称: {itemInfo.Name}\n" +
                                      $"大小: {image.Width} x {image.Height}\n" +
                                      $"格式: {image.PixelFormat}";

                    selectedImage?.Dispose();
                    selectedImage = new Bitmap(image);
                }
                catch (Exception ex)
                {
                    resourceInfo.Text = $"预览失败: {ex.Message}";
                }
            }
        }

        private void ResourceTreeView_DoubleClick(object sender, EventArgs e)
        {
            if (resourceTreeView.SelectedNode?.Tag is ResourceItemInfo itemInfo && itemInfo.Value is Image image)
            {
                selectedImage?.Dispose();
                selectedImage = new Bitmap(image);

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private class ResourceFileInfo
        {
            public string BaseName { get; set; }
            public string DisplayName { get; set; }
            public string ResxFilePath { get; set; }
            public bool IsCurrentFormResource { get; set; }
            public bool IsPropertiesResources { get; set; }

            public override string ToString() => DisplayName ?? BaseName;
        }

        private enum ResourceItemType
        {
            PropertiesResource,
            ResourceManager,
            ResxFile
        }

        private class ResourceItemInfo
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public ResourceItemType ResourceType { get; set; }
            public string ResxFilePath { get; set; }
        }

        #endregion

        #region 字体图标标签页

        private void InitializeIconFontTab(TabPage tab)
        {
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 500,
                SplitterWidth = 2,
                FixedPanel = FixedPanel.Panel2,
                Padding = new Padding(10)
            };

            // 左侧选择区
            InitializeIconSelectionPanel(splitContainer.Panel1);

            // 右侧预览和设置区
            InitializeIconPreviewPanel(splitContainer.Panel2);

            tab.Controls.Add(splitContainer);
        }

        private void InitializeIconSelectionPanel(SplitterPanel panel)
        {
            var container = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            // 字体族选择
            var lblFontFamily = new Label
            {
                Text = "字体族:",
                Location = new Point(5, 12),
                AutoSize = true
            };

            fontFamilyCombo = new ComboBox
            {
                Location = new Point(70, 9),
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var providers = IconFontManager.Instance.GetAllProviders();
            foreach (var provider in providers)
            {
                fontFamilyCombo.Items.Add(new FontProviderItem
                {
                    Provider = provider,
                    DisplayText = $"{provider.DisplayName} ({provider.FontFamilyName})"
                });
            }

            fontFamilyCombo.SelectedIndexChanged += FontFamilyCombo_SelectedIndexChanged;

            // 搜索框
            var lblSearch = new Label
            {
                Text = "搜索:",
                Location = new Point(5, 47),
                AutoSize = true
            };

            iconSearchBox = new TextBox
            {
                Location = new Point(70, 44),
                Width = 350,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            iconSearchBox.TextChanged += IconSearchBox_TextChanged;

            // 图标列表
            var lblIcons = new Label
            {
                Text = "图标列表:",
                Location = new Point(5, 77),
                AutoSize = true
            };

            iconListBox = new ListBox
            {
                Location = new Point(5, 100),
                Size = new Size(container.Width - 10, container.Height - 105),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 9f),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28
            };
            iconListBox.DrawItem += IconListBox_DrawItem;
            iconListBox.SelectedIndexChanged += IconListBox_SelectedIndexChanged;
            iconListBox.DoubleClick += (s, e) =>
            {
                if (iconListBox.SelectedItem != null)
                {
                    GenerateIconImage();
                }
            };

            container.Controls.AddRange(new Control[]
            {
                lblFontFamily, fontFamilyCombo,
                lblSearch, iconSearchBox,
                lblIcons, iconListBox
            });

            panel.Controls.Add(container);


            if (fontFamilyCombo.Items.Count > 0)
            {
                fontFamilyCombo.SelectedIndex = 0;
            }
        }

        private void InitializeIconPreviewPanel(SplitterPanel panel)
        {
            var container = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            container.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // 预览
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 200)); // 设置
            container.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // 按钮

            // 预览区
            var previewGroup = new GroupBox
            {
                Text = "预览",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            iconPreviewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.White
            };

            previewGroup.Controls.Add(iconPreviewBox);
            container.Controls.Add(previewGroup, 0, 0);

            // 设置区
            var settingsGroup = CreateIconSettingsGroup();
            container.Controls.Add(settingsGroup, 0, 1);

            // 信息区
            lblIconInfo = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 8.5f),
                ForeColor = Color.Gray,
                Padding = new Padding(5, 0, 0, 0)
            };
            container.Controls.Add(lblIconInfo, 0, 2);

            panel.Controls.Add(container);
        }

        private GroupBox CreateIconSettingsGroup()
        {
            var group = new GroupBox
            {
                Text = "图标设置",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblSize = new Label
            {
                Text = "大小:",
                Location = new Point(15, 30),
                AutoSize = true
            };

            iconSizeNumeric = new NumericUpDown
            {
                Location = new Point(100, 27),
                Width = 100,
                Minimum = 16,
                Maximum = 512,
                Value = 32,
                Increment = 8
            };
            iconSizeNumeric.ValueChanged += (s, e) => UpdateIconPreview();

            var lblColor = new Label
            {
                Text = "颜色:",
                Location = new Point(15, 65),
                AutoSize = true
            };

            iconColorButton = new Button
            {
                Location = new Point(100, 62),
                Size = new Size(100, 25),
                BackColor = iconColor,
                FlatStyle = FlatStyle.Flat
            };
            iconColorButton.Click += IconColorButton_Click;

            var lblRotation = new Label
            {
                Text = "旋转:",
                Location = new Point(15, 100),
                AutoSize = true
            };

            iconRotationNumeric = new NumericUpDown
            {
                Location = new Point(100, 97),
                Width = 100,
                Minimum = 0,
                Maximum = 360,
                Value = 0,
                Increment = 15
            };
            iconRotationNumeric.ValueChanged += (s, e) => UpdateIconPreview();

            var btnGenerate = new Button
            {
                Text = "生成图像",
                Location = new Point(15, 140),
                Size = new Size(185, 35),
                Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold)
            };
            btnGenerate.Click += (s, e) => GenerateIconImage();

            group.Controls.AddRange(new Control[]
            {
                lblSize, iconSizeNumeric,
                lblColor, iconColorButton,
                lblRotation, iconRotationNumeric,
                btnGenerate
            });

            return group;
        }

        private void FontFamilyCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fontFamilyCombo.SelectedItem is FontProviderItem item)
            {
                currentProvider = item.Provider;
                currentEnumType = currentProvider.GetIconEnumType();
                LoadIconList();
            }
        }

        private void LoadIconList(string filter = null)
        {
            iconListBox.BeginUpdate();
            iconListBox.Items.Clear();

            if (currentEnumType == null)
            {
                iconListBox.EndUpdate();
                return;
            }

            var enumValues = Enum.GetValues(currentEnumType);

            foreach (Enum enumValue in enumValues)
            {
                var name = enumValue.ToString();
                var description = GetEnumDescription(enumValue);

                if (!string.IsNullOrEmpty(filter) &&
                    name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 &&
                    description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                iconListBox.Items.Add(new IconEnumDisplayItem
                {
                    EnumValue = enumValue,
                    Name = name,
                    Description = description,
                    Unicode = $"U+{(int)(object)enumValue:X4}"
                });
            }

            iconListBox.EndUpdate();
        }

        private string GetEnumDescription(Enum enumValue)
        {
            var field = currentEnumType.GetField(enumValue.ToString());
            var attributes = field?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Length > 0)
            {
                // 解析注释中的描述(如 "Address Book")
                var lines = attributes[0].Description.Split('\n');
                if (lines.Length > 0)
                {
                    return lines[0].Trim();
                }
            }

            return enumValue.ToString();
        }

        private void IconSearchBox_TextChanged(object sender, EventArgs e)
        {
            LoadIconList(iconSearchBox.Text.Trim());
        }

        private void IconListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();

            var item = iconListBox.Items[e.Index] as IconEnumDisplayItem;
            if (item != null)
            {
                // 绘制图标
                try
                {
                    var icon = currentProvider.GetIcon(item.EnumValue, 20f, Color.Black);
                    if (icon != null)
                    {
                        e.Graphics.DrawImage(icon, e.Bounds.Left + 4, e.Bounds.Top + 4, 20, 20);
                        icon.Dispose();
                    }
                }
                catch { }

                // 绘制文本
                using (var brush = new SolidBrush(e.ForeColor))
                {
                    var textRect = new Rectangle(e.Bounds.Left + 30, e.Bounds.Top + 2, e.Bounds.Width - 30, e.Bounds.Height);
                    e.Graphics.DrawString(item.Name, e.Font, brush, textRect);

                    if (!string.IsNullOrEmpty(item.Description) && item.Description != item.Name)
                    {
                        using (var grayBrush = new SolidBrush(Color.Gray))
                        using (var smallFont = new Font(e.Font.FontFamily, 7.5f))
                        {
                            var descRect = new Rectangle(e.Bounds.Left + 30, e.Bounds.Top + 14, e.Bounds.Width - 30, e.Bounds.Height);
                            e.Graphics.DrawString(item.Description, smallFont, grayBrush, descRect);
                        }
                    }
                }
            }

            e.DrawFocusRectangle();
        }

        private void IconListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateIconPreview();
        }

        private void IconColorButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new ColorDialog())
            {
                dialog.Color = iconColor;
                dialog.AllowFullOpen = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    iconColor = dialog.Color;
                    iconColorButton.BackColor = iconColor;
                    UpdateIconPreview();
                }
            }
        }

        private void UpdateIconPreview()
        {
            if (iconListBox.SelectedItem is IconEnumDisplayItem item && currentProvider != null)
            {
                try
                {
                    var previewSize = Math.Min(iconPreviewBox.Width, iconPreviewBox.Height) - 20;
                    Image newPreview = null;

                    using (var icon = currentProvider.GetIcon(
                        item.EnumValue,
                        previewSize,
                        iconColor,
                        (float)iconRotationNumeric.Value))
                    {
                        if (icon != null)
                        {
                            newPreview = CreateIndependentCopy(icon);
                        }
                    }

                    // 安全地更换预览图片
                    var oldImage = iconPreviewBox.Image;
                    iconPreviewBox.Image = newPreview;
                    oldImage?.Dispose();

                    lblIconInfo.Text = $"名称: {item.Name}\n" +
                                     $"Unicode: {item.Unicode}\n" +
                                     $"描述: {item.Description}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"生成预览失败: {ex.Message}");
                }
            }
        }

        private void GenerateIconImage()
        {
            if (iconListBox.SelectedItem is IconEnumDisplayItem item && currentProvider != null)
            {
                try
                {
                    Image newImage = null;

                    // 从 provider 获取图标
                    using (var icon = currentProvider.GetIcon(
                        item.EnumValue,
                        (float)iconSizeNumeric.Value,
                        iconColor,
                        (float)iconRotationNumeric.Value))
                    {
                        if (icon != null)
                        {
                            // 创建独立副本
                            newImage = CreateIndependentCopy(icon);
                        }
                    }

                    if (newImage != null)
                    {
                        // 释放旧图片
                        selectedImage?.Dispose();
                        selectedImage = newImage;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"生成图标失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 从字节数组创建独立图片
        /// </summary>
        private static Image CreateImageFromBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            var stream = new MemoryStream(data);
            var image = Image.FromStream(stream);

            // 创建独立副本后释放原始资源
            var independentCopy = CreateIndependentCopy(image);
            image.Dispose();
            stream.Dispose();

            return independentCopy;
        }

        private class FontProviderItem
        {
            public IIconFontProvider Provider { get; set; }
            public string DisplayText { get; set; }
            public override string ToString() => DisplayText;
        }

        private class IconEnumDisplayItem
        {
            public Enum EnumValue { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Unicode { get; set; }
            public override string ToString() => Name;
        }

        #endregion

        #region 按钮面板

        private Panel CreateButtonPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            var btnOK = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnOK.Location = new Point(panel.Width - 230, 8);
            btnOK.Click += BtnOK_Click;

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnCancel.Location = new Point(panel.Width - 120, 8);

            var btnClear = new Button
            {
                Text = "清空",
                Size = new Size(100, 35),
                Location = new Point(10, 8)
            };
            btnClear.Click += (s, e) =>
            {
                selectedImage?.Dispose();
                selectedImage = null;
                DialogResult = DialogResult.OK;
            };

            panel.Controls.AddRange(new Control[] { btnOK, btnCancel, btnClear });

            AcceptButton = btnOK;
            CancelButton = btnCancel;

            return panel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // 根据当前标签页处理
            switch (tabControl.SelectedIndex)
            {
                case 0: // 本地文件
                        // selectedImage 已经在 BtnBrowse_Click 中设置
                    break;

                case 1: // 项目资源
                        // selectedImage 已经在 ResourceTreeView_AfterSelect 中设置
                    break;

                case 2: // 字体图标
                        // 确保生成最新的图标
                    if (iconListBox.SelectedItem != null)
                    {
                        GenerateIconImage();
                    }
                    break;
            }

            // 验证 selectedImage 是否有效
            if (selectedImage != null)
            {
                try
                {
                    var test = selectedImage.Width; // 简单验证
                    Debug.WriteLine($"[BtnOK_Click] selectedImage is valid: {selectedImage.Width}x{selectedImage.Height}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BtnOK_Click] selectedImage is INVALID: {ex.Message}");
                    MessageBox.Show("图片无效, 请重新选择", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            DialogResult = DialogResult.OK;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 清理预览控件的图片
                if (localFilePreview?.Image != null)
                {
                    var img = localFilePreview.Image;
                    localFilePreview.Image = null;
                    img.Dispose();
                }

                if (resourcePreview?.Image != null)
                {
                    var img = resourcePreview.Image;
                    resourcePreview.Image = null;
                    img.Dispose();
                }

                if (iconPreviewBox?.Image != null)
                {
                    var img = iconPreviewBox.Image;
                    iconPreviewBox.Image = null;
                    img.Dispose();
                }

                // 清理副本
                originalImage?.Dispose();
                originalImage = null;

                if (DialogResult != DialogResult.OK)
                {
                    selectedImage?.Dispose();
                    selectedImage = null;
                }
            }
            base.Dispose(disposing);
        }
    }

}
