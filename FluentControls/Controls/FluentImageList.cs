using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    /// <summary>
    /// Fluent 图片列表组件
    /// </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(ImageList))]
    [DefaultProperty("Images")]
    [Designer(typeof(FluentImageListDesigner))]
    public class FluentImageList : Component
    {
        private FluentImageCollection images;
        private ColorDepth colorDepth = ColorDepth.Depth32Bit;
        private bool preserveOriginalQuality = true;

        public event EventHandler ImagesChanged;

        public FluentImageList()
        {
            images = new FluentImageCollection(this);
        }

        public FluentImageList(IContainer container) : this()
        {
            container?.Add(this);
        }

        #region 属性

        /// <summary>
        /// 图片集合
        /// </summary>
        [Category("Data")]
        [Description("图片集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(FluentImageCollectionEditor), typeof(UITypeEditor))]
        public FluentImageCollection Images => images;

        /// <summary>
        /// 颜色深度
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(ColorDepth.Depth32Bit)]
        [Description("图片颜色深度")]
        public ColorDepth ColorDepth
        {
            get => colorDepth;
            set => colorDepth = value;
        }

        /// <summary>
        /// 是否保持原始质量
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("是否保持图片原始质量（不进行压缩和缩放）")]
        public bool PreserveOriginalQuality
        {
            get => preserveOriginalQuality;
            set => preserveOriginalQuality = value;
        }

        #endregion

        #region 事件

        internal void OnImageAdded(FluentImageListItem item)
        {
            OnImagesChanged();
        }

        internal void OnImageRemoved(FluentImageListItem item)
        {
            OnImagesChanged();
        }

        internal void OnImagesCleared()
        {
            OnImagesChanged();
        }

        protected virtual void OnImagesChanged()
        {
            ImagesChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 从文件加载图片
        /// </summary>
        public int AddImageFromFile(string filePath, string key = null, string description = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("图片文件不存在", filePath);
            }

            try
            {
                // 使用高质量方式加载图片
                Image image;

                if (preserveOriginalQuality)
                {
                    // 原始质量加载
                    image = Image.FromFile(filePath);
                }
                else
                {
                    // 可以在这里添加压缩逻辑
                    image = Image.FromFile(filePath);
                }

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string imageKey = key ?? fileName;

                return images.Add(image, imageKey, fileName, description);
            }
            catch (Exception ex)
            {
                throw new Exception($"加载图片失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量从文件加载
        /// </summary>
        public void AddImagesFromFiles(string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    AddImageFromFile(filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载图片失败 {filePath}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 从文件夹加载所有图片
        /// </summary>
        public void AddImagesFromDirectory(string directoryPath, string searchPattern = "*.*", bool recursive = false)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException("目录不存在: " + directoryPath);
            }

            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".ico" };

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directoryPath, searchPattern, searchOption)
                .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToArray();

            AddImagesFromFiles(files);
        }

        /// <summary>
        /// 保存图片到文件
        /// </summary>
        public void SaveImage(int index, string filePath, System.Drawing.Imaging.ImageFormat format = null)
        {
            if (index < 0 || index >= images.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var item = images.GetItem(index);
            if (item.Image == null)
            {
                throw new InvalidOperationException("图片为空");
            }

            format = format ?? System.Drawing.Imaging.ImageFormat.Png;
            item.Image.Save(filePath, format);
        }

        /// <summary>
        /// 导出所有图片
        /// </summary>
        public void ExportAllImages(string directoryPath, System.Drawing.Imaging.ImageFormat format = null)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            format = format ?? System.Drawing.Imaging.ImageFormat.Png;
            string extension = GetExtensionFromFormat(format);

            for (int i = 0; i < images.Count; i++)
            {
                var item = images.GetItem(i);
                if (item.Image != null)
                {
                    string fileName = $"{item.Key}{extension}";
                    string filePath = Path.Combine(directoryPath, fileName);
                    item.Image.Save(filePath, format);
                }
            }
        }

        private string GetExtensionFromFormat(System.Drawing.Imaging.ImageFormat format)
        {
            if (format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
            {
                return ".jpg";
            }

            if (format.Equals(System.Drawing.Imaging.ImageFormat.Png))
            {
                return ".png";
            }

            if (format.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
            {
                return ".bmp";
            }

            if (format.Equals(System.Drawing.Imaging.ImageFormat.Gif))
            {
                return ".gif";
            }

            if (format.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
            {
                return ".tiff";
            }

            return ".png";
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                images?.Clear();
            }
            base.Dispose(disposing);
        }
    }

    #region 图片列表项

    /// <summary>
    /// Fluent 图片列表项
    /// </summary>
    public class FluentImageListItem
    {
        public FluentImageListItem()
        {
        }

        public FluentImageListItem(Image image, string key, string name = null, string description = null)
        {
            Image = image;
            Key = key;
            Name = name ?? key;
            Description = description ?? "";
        }

        /// <summary>
        /// 图片
        /// </summary>
        public Image Image { get; set; }

        /// <summary>
        /// 键名
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 标签（用于分类）
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 自定义数据
        /// </summary>
        public object CustomData { get; set; }


        public FluentImageListItem Clone()
        {
            return new FluentImageListItem
            {
                Image = Image?.Clone() as Image,
                Key = Key,
                Name = Name,
                Description = Description,
                Tag = Tag,
                CustomData = CustomData
            };
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? Name : Key;
        }
    }

    /// <summary>
    /// Fluent 图片列表集合
    /// </summary>
    public class FluentImageCollection : IList, ICollection, IEnumerable
    {
        private readonly List<FluentImageListItem> items = new List<FluentImageListItem>();
        private readonly FluentImageList owner;

        internal FluentImageCollection(FluentImageList owner)
        {
            this.owner = owner;
        }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public bool IsFixedSize => false;

        public FluentImageListItem this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        object IList.this[int index]
        {
            get => items[index];
            set => items[index] = (FluentImageListItem)value;
        }

        /// <summary>
        /// 通过键获取图片
        /// </summary>
        public Image this[string key]
        {
            get
            {
                var item = items.FirstOrDefault(i => i.Key == key);
                return item?.Image;
            }
        }

        /// <summary>
        /// 键集合
        /// </summary>
        public IEnumerable<string> Keys => items.Select(i => i.Key);

        public int Add(Image image)
        {
            return Add(image, $"Image{items.Count}");
        }

        public int Add(Image image, string key)
        {
            return Add(image, key, key);
        }

        public int Add(Image image, string key, string name, string description = null)
        {
            var item = new FluentImageListItem(image, key, name, description);
            items.Add(item);
            owner.OnImageAdded(item);
            return items.Count - 1;
        }

        public void AddRange(Image[] images)
        {
            foreach (var image in images)
            {
                Add(image);
            }
        }

        public int Add(object value)
        {
            if (value is Image image)
            {
                return Add(image);
            }
            else if (value is FluentImageListItem item)
            {
                items.Add(item);
                owner.OnImageAdded(item);
                return items.Count - 1;
            }
            throw new ArgumentException("值必须是 Image 或 FluentImageListItem 类型");
        }

        public void Clear()
        {
            // 释放所有图片资源
            foreach (var item in items)
            {
                item.Image?.Dispose();
            }
            items.Clear();
            owner.OnImagesCleared();
        }

        public bool Contains(object value)
        {
            if (value is string key)
            {
                return items.Any(i => i.Key == key);
            }
            return items.Contains(value);
        }

        public int IndexOf(object value)
        {
            if (value is string key)
            {
                return items.FindIndex(i => i.Key == key);
            }
            return items.IndexOf(value as FluentImageListItem);
        }

        public void Insert(int index, object value)
        {
            if (value is FluentImageListItem item)
            {
                items.Insert(index, item);
                owner.OnImageAdded(item);
            }
        }

        public void Remove(object value)
        {
            if (value is FluentImageListItem item)
            {
                items.Remove(item);
                item.Image?.Dispose();
                owner.OnImageRemoved(item);
            }
            else if (value is string key)
            {
                RemoveByKey(key);
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                var item = items[index];
                items.RemoveAt(index);
                item.Image?.Dispose();
                owner.OnImageRemoved(item);
            }
        }

        public void RemoveByKey(string key)
        {
            var item = items.FirstOrDefault(i => i.Key == key);
            if (item != null)
            {
                Remove(item);
            }
        }

        public void CopyTo(Array array, int index)
        {
            items.CopyTo((FluentImageListItem[])array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// 获取所有图片
        /// </summary>
        public IEnumerable<Image> GetImages()
        {
            return items.Select(i => i.Image);
        }

        /// <summary>
        /// 获取图片项
        /// </summary>
        public FluentImageListItem GetItem(int index)
        {
            return items[index];
        }

        /// <summary>
        /// 通过键获取图片项
        /// </summary>
        public FluentImageListItem GetItemByKey(string key)
        {
            return items.FirstOrDefault(i => i.Key == key);
        }
    }

    #endregion

    #region 设计器支持

    public class FluentImageListDesigner : ComponentDesigner
    {
        public override DesignerVerbCollection Verbs
        {
            get
            {
                return new DesignerVerbCollection(new[]
                {
                    new DesignerVerb("添加图片...", OnAddImages),
                    new DesignerVerb("从文件夹导入...", OnImportFromFolder),
                    new DesignerVerb("清空图片", OnClearImages)
                });
            }
        }

        private void OnAddImages(object sender, EventArgs e)
        {
            var imageList = Component as FluentImageList;
            if (imageList == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|所有文件|*.*";
                dialog.Multiselect = true;
                dialog.Title = "选择图片";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        imageList.AddImagesFromFiles(dialog.FileNames);

                        var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host != null)
                        {
                            var transaction = host.CreateTransaction("添加图片");
                            try
                            {
                                RaiseComponentChanged(
                                    TypeDescriptor.GetProperties(imageList)["Images"],
                                    null, null);
                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Cancel();
                            }
                        }

                        MessageBox.Show($"成功添加 {dialog.FileNames.Length} 张图片", "完成",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"添加图片失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnImportFromFolder(object sender, EventArgs e)
        {
            var imageList = Component as FluentImageList;
            if (imageList == null)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择包含图片的文件夹";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        imageList.AddImagesFromDirectory(dialog.SelectedPath);

                        var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host != null)
                        {
                            var transaction = host.CreateTransaction("从文件夹导入");
                            try
                            {
                                RaiseComponentChanged(
                                    TypeDescriptor.GetProperties(imageList)["Images"],
                                    null, null);
                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Cancel();
                            }
                        }

                        MessageBox.Show($"成功导入 {imageList.Images.Count} 张图片", "完成",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败: {ex.Message}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnClearImages(object sender, EventArgs e)
        {
            var imageList = Component as FluentImageList;
            if (imageList == null)
            {
                return;
            }

            var result = MessageBox.Show("确定要清空所有图片吗？", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                imageList.Images.Clear();

                var host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                {
                    var transaction = host.CreateTransaction("清空图片");
                    try
                    {
                        RaiseComponentChanged(
                            TypeDescriptor.GetProperties(imageList)["Images"],
                            null, null);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Cancel();
                    }
                }
            }
        }
    }

    public class FluentImageCollectionEditor : CollectionEditor
    {
        public FluentImageCollectionEditor(Type type) : base(type)
        {
        }

        protected override string GetDisplayText(object value)
        {
            if (value is FluentImageListItem item)
            {
                return item.ToString();
            }
            return base.GetDisplayText(value);
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(FluentImageListItem);
        }
    }

    #endregion
}
