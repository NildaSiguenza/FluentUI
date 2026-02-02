using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace FluentControls.Controls
{
    [ToolboxBitmap(typeof(FluentShape))]
    public class FluentShape : FluentControlBase
    {
        private ShapeType shapeType = ShapeType.Rectangle;
        private IShapeRenderer shapeRenderer;
        private ShapeStyle shapeStyle;
        private CompositeShape compositeShape;
        private bool useCompositeMode = false;
        private ShapeTextItemCollection textItems;

        public FluentShape()
        {
            InitializeShapeStyle();
            UpdateShapeRenderer();
            compositeShape = new CompositeShape();
            textItems = new ShapeTextItemCollection();
            textItems.CollectionChanged += (s, e) => Invalidate();

            // 设置默认大小
            Size = new Size(100, 100);
        }

        #region 属性

        [Category("Shape")]
        [DefaultValue(ShapeType.Rectangle)]
        [Description("形状类型")]
        public ShapeType ShapeType
        {
            get => shapeType;
            set
            {
                if (shapeType != value)
                {
                    shapeType = value;
                    UpdateShapeRenderer();
                    Invalidate();
                }
            }
        }

        [Category("Shape")]
        [Description("是否使用组合模式")]
        [DefaultValue(false)]
        public bool UseCompositeMode
        {
            get => useCompositeMode;
            set
            {
                if (useCompositeMode != value)
                {
                    useCompositeMode = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CompositeShape CompositeShape
        {
            get => compositeShape;
            set
            {
                compositeShape = value ?? new CompositeShape();
                Invalidate();
            }
        }

        /// <summary>
        /// 文本项集合
        /// </summary>
        [Category("Shape")]
        [Description("形状上的文本项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(ShapeTextCollectionEditor), typeof(UITypeEditor))]
        public ShapeTextItemCollection TextItems => textItems;

        [Category("Appearance")]
        [Description("填充颜色")]
        public Color FillColor
        {
            get => shapeStyle.FillColor;
            set
            {
                shapeStyle.FillColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => shapeStyle.BorderColor;
            set
            {
                shapeStyle.BorderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("边框宽度")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => shapeStyle.BorderWidth;
            set
            {
                shapeStyle.BorderWidth = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("不透明度")]
        [DefaultValue(1.0f)]
        public float Opacity
        {
            get => shapeStyle.Opacity;
            set
            {
                shapeStyle.Opacity = Math.Max(0, Math.Min(1, value));
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("边框样式")]
        [DefaultValue(DashStyle.Solid)]
        public DashStyle BorderStyle
        {
            get => shapeStyle.BorderStyle;
            set
            {
                shapeStyle.BorderStyle = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("使用渐变填充")]
        [DefaultValue(false)]
        public bool UseGradient
        {
            get => shapeStyle.UseGradient;
            set
            {
                shapeStyle.UseGradient = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("渐变起始颜色")]
        public Color GradientStartColor
        {
            get => shapeStyle.GradientStartColor;
            set
            {
                shapeStyle.GradientStartColor = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("渐变结束颜色")]
        public Color GradientEndColor
        {
            get => shapeStyle.GradientEndColor;
            set
            {
                shapeStyle.GradientEndColor = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("渐变模式")]
        [DefaultValue(LinearGradientMode.Vertical)]
        public LinearGradientMode GradientMode
        {
            get => shapeStyle.GradientMode;
            set
            {
                shapeStyle.GradientMode = value;
                Invalidate();
            }
        }

        [Category("Line")]
        [Description("线段显示模式")]
        [DefaultValue(LineMode.DiagonalDown)]
        public LineMode LineMode { get; set; } = LineMode.DiagonalDown;

        [Category("Line")]
        [Description("线段边距")]
        [DefaultValue(5)]
        public int LineMargin { get; set; } = 5;

        [Category("Polygon")]
        [Description("多边形边数")]
        [DefaultValue(6)]
        public int PolygonSides { get; set; } = 6;

        [Category("Polygon")]
        [Description("多边形起始角度")]
        [DefaultValue(-90f)]
        public float PolygonStartAngle { get; set; } = -90f;

        [Category("Star")]
        [Description("星形角数")]
        [DefaultValue(5)]
        public int StarPoints { get; set; } = 5;

        [Category("Star")]
        [Description("星形内半径比例")]
        [DefaultValue(0.3f)]
        public float StarInnerRadiusRatio { get; set; } = 0.4f;

        [Category("Star")]
        [Description("星形起始角度")]
        [DefaultValue(-18f)]
        public float StarStartAngle { get; set; } = -18f;

        [Category("RoundedRect")]
        [Description("圆角半径")]
        [DefaultValue(8)]
        public int CornerRadius { get; set; } = 8;

        [Category("Trapezoid")]
        [Description("梯形顶边宽度比例")]
        [DefaultValue(0.6f)]
        public float TrapezoidTopWidthRatio { get; set; } = 0.6f;

        [Category("Arrow")]
        [Description("箭头方向")]
        [DefaultValue(ArrowDirection.Right)]
        public ArrowDirection ArrowDirection { get; set; } = ArrowDirection.Right;

        [Category("Arrow")]
        [Description("箭头头部宽度比例(0-1)")]
        [DefaultValue(0.4f)]
        public float ArrowHeadWidthRatio { get; set; } = 0.4f;

        [Category("Arrow")]
        [Description("箭头尾部宽度比例(0-1)")]
        [DefaultValue(0.6f)]
        public float ArrowTailWidthRatio { get; set; } = 0.6f;

        [Category("Arrow")]
        [Description("箭头尾部缺口深度比例(0-1)")]
        [DefaultValue(0.2f)]
        public float TailNotchDepthRatio { get; set; } = 0.2f;

        /// <summary>
        /// 顶点集合
        /// </summary>
        [Category("Vertices")]
        [Description("形状顶点集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(VertexCollectionEditor), typeof(UITypeEditor))]
        public VertexCollection Vertices
        {
            get
            {
                if (shapeRenderer is TriangleRenderer triangle)
                {
                    return triangle.CustomVertices;
                }
                else if (shapeRenderer is TrapezoidRenderer trapezoid)
                {
                    return trapezoid.CustomVertices;
                }
                else if (shapeRenderer is PolygonRenderer polygon)
                {
                    return polygon.CustomVertices;
                }

                return null;
            }
        }

        [Category("3D")]
        [Description("顶面颜色")]
        public Color TopFaceColor
        {
            get => shapeStyle.TopFaceColor;
            set
            {
                shapeStyle.TopFaceColor = value;
                Invalidate();
            }
        }

        [Category("3D")]
        [Description("前面颜色")]
        public Color FrontFaceColor
        {
            get => shapeStyle.FrontFaceColor;
            set
            {
                shapeStyle.FrontFaceColor = value;
                Invalidate();
            }
        }

        [Category("3D")]
        [Description("右面颜色")]
        public Color RightFaceColor
        {
            get => shapeStyle.RightFaceColor;
            set
            {
                shapeStyle.RightFaceColor = value;
                Invalidate();
            }
        }

        [Category("3D")]
        [Description("水平视角角度(圆柱体：-1到1, 立方体：-90到90, 球体：0-360)")]
        [DefaultValue(30f)]
        public float HorizontalViewAngle
        {
            get => shapeStyle.HorizontalViewAngle;
            set
            {
                shapeStyle.HorizontalViewAngle = value;
                Invalidate();
            }
        }

        [Category("3D")]
        [Description("垂直视角角度(圆柱体/立方体：-90到90, 球体：0-90)")]
        [DefaultValue(30f)]
        public float VerticalViewAngle
        {
            get => shapeStyle.VerticalViewAngle;
            set
            {
                shapeStyle.VerticalViewAngle = value;
                Invalidate();
            }
        }

        [Category("3D")]
        [Description("3D深度比例")]
        [DefaultValue(0.3f)]
        public float DepthRatio
        {
            get => shapeStyle.DepthRatio;
            set
            {
                shapeStyle.DepthRatio = value;
                Invalidate();
            }
        }

        [Category("Shadow")]
        [Description("显示阴影")]
        [DefaultValue(false)]
        public bool ShowShapeShadow
        {
            get => shapeStyle.ShowShadow;
            set
            {
                shapeStyle.ShowShadow = value;
                Invalidate();
            }
        }

        [Category("Shadow")]
        [Description("阴影颜色")]
        public Color ShadowColor
        {
            get => shapeStyle.ShadowColor;
            set
            {
                shapeStyle.ShadowColor = value;
                Invalidate();
            }
        }

        [Category("Shadow")]
        [Description("阴影X偏移")]
        [DefaultValue(3)]
        public int ShadowOffsetX
        {
            get => shapeStyle.ShadowOffsetX;
            set
            {
                shapeStyle.ShadowOffsetX = value;
                Invalidate();
            }
        }

        [Category("Shadow")]
        [Description("阴影Y偏移")]
        [DefaultValue(3)]
        public int ShadowOffsetY
        {
            get => shapeStyle.ShadowOffsetY;
            set
            {
                shapeStyle.ShadowOffsetY = value;
                Invalidate();
            }
        }

        #endregion

        #region 方法

        private void InitializeShapeStyle()
        {
            BackColor = Color.Transparent;
            shapeStyle = ShapeStyle.CreateDefault();
        }

        private void UpdateShapeRenderer()
        {
            switch (shapeType)
            {
                case ShapeType.Line:
                    shapeRenderer = new LineRenderer
                    {
                        Mode = LineMode,
                        Margin = LineMargin
                    };
                    break;
                case ShapeType.Rectangle:
                    shapeRenderer = new RectangleRenderer();
                    break;
                case ShapeType.RoundedRectangle:
                    shapeRenderer = new RoundedRectangleRenderer { CornerRadius = CornerRadius };
                    break;
                case ShapeType.Ellipse:
                    shapeRenderer = new EllipseRenderer();
                    break;
                case ShapeType.Circle:
                    shapeRenderer = new CircleRenderer();
                    break;
                case ShapeType.Triangle:
                    shapeRenderer = new TriangleRenderer();
                    break;
                case ShapeType.Trapezoid:
                    shapeRenderer = new TrapezoidRenderer { TopWidthRatio = TrapezoidTopWidthRatio };
                    break;
                case ShapeType.Polygon:
                    shapeRenderer = new PolygonRenderer
                    {
                        Sides = PolygonSides,
                        StartAngle = PolygonStartAngle
                    };
                    break;
                case ShapeType.Star:
                    shapeRenderer = new StarRenderer
                    {
                        Points = StarPoints,
                        InnerRadiusRatio = StarInnerRadiusRatio,
                        StartAngle = StarStartAngle
                    };
                    break;
                case ShapeType.Arrow:
                    shapeRenderer = new ArrowRenderer
                    {
                        Direction = ArrowDirection,
                        ArrowHeadWidthRatio = ArrowHeadWidthRatio,
                        ArrowTailWidthRatio = ArrowTailWidthRatio,
                        TailNotchDepthRatio = TailNotchDepthRatio
                    };
                    break;
                case ShapeType.Cylinder:
                    shapeRenderer = new CylinderRenderer
                    {
                        TopEllipseRatio = 0.2f,
                        HorizontalViewAngle = HorizontalViewAngle / 100f, // 归一化
                        VerticalViewAngle = VerticalViewAngle
                    };
                    break;
                case ShapeType.Sphere:
                    shapeRenderer = new SphereRenderer
                    {
                        LightHorizontalAngle = HorizontalViewAngle,
                        LightVerticalAngle = VerticalViewAngle
                    };
                    break;
                case ShapeType.Cuboid:
                    shapeRenderer = new CuboidRenderer
                    {
                        HorizontalViewAngle = HorizontalViewAngle,
                        VerticalViewAngle = VerticalViewAngle,
                        DepthRatio = DepthRatio
                    };
                    break;
                default:
                    shapeRenderer = new RectangleRenderer();
                    break;
            }

            // 为有顶点的渲染器添加事件监听
            if (shapeRenderer is TriangleRenderer triangle)
            {
                triangle.CustomVertices.CollectionChanged += (s, e) => RefreshControl();
            }
            else if (shapeRenderer is TrapezoidRenderer trapezoid)
            {
                trapezoid.CustomVertices.CollectionChanged += (s, e) => RefreshControl();
            }
            else if (shapeRenderer is PolygonRenderer polygon)
            {
                polygon.CustomVertices.CollectionChanged += (s, e) => RefreshControl();
            }
        }

        /// <summary>
        /// 设置自定义渲染器
        /// </summary>
        public void SetCustomRenderer(IShapeRenderer renderer)
        {
            if (renderer != null)
            {
                shapeType = ShapeType.Custom;
                shapeRenderer = renderer;
                Invalidate();
            }
        }

        /// <summary>
        /// 判断顶点属性是否应该显示
        /// </summary>
        private bool ShouldSerializeVertices()
        {
            return shapeType == ShapeType.Triangle ||
                   shapeType == ShapeType.Trapezoid ||
                   shapeType == ShapeType.Polygon;
        }

        /// <summary>
        /// 重置顶点属性
        /// </summary>
        private void ResetVertices()
        {
            if (shapeRenderer is TriangleRenderer triangle)
            {
                triangle.CustomVertices.Clear();
            }
            else if (shapeRenderer is TrapezoidRenderer trapezoid)
            {
                trapezoid.CustomVertices.Clear();
            }
            else if (shapeRenderer is PolygonRenderer polygon)
            {
                polygon.CustomVertices.Clear();
            }

            RefreshControl();
        }

        /// <summary>
        /// 获取当前形状的顶点
        /// </summary>
        public PointF[] GetCurrentVertices()
        {
            var bounds = GetShapeBounds();

            if (shapeRenderer is TriangleRenderer triangle)
            {
                return triangle.GetCurrentVertices(bounds);
            }
            else if (shapeRenderer is TrapezoidRenderer trapezoid)
            {
                return trapezoid.GetCurrentVertices(bounds);
            }
            else if (shapeRenderer is PolygonRenderer polygon)
            {
                return polygon.GetCurrentVertices(bounds);
            }

            return new PointF[0];
        }

        /// <summary>
        /// 设置形状顶点
        /// </summary>
        public void SetVertices(params PointF[] vertices)
        {
            if (shapeRenderer is TriangleRenderer triangle)
            {
                triangle.SetVertices(vertices);
                Invalidate();
            }
            else if (shapeRenderer is TrapezoidRenderer trapezoid)
            {
                trapezoid.SetVertices(vertices);
                Invalidate();
            }
            else if (shapeRenderer is PolygonRenderer polygon)
            {
                polygon.SetVertices(vertices);
                Invalidate();
            }
        }

        /// <summary>
        /// 添加组合形状
        /// </summary>
        public void AddCompositeShape(ShapeItem shape)
        {
            compositeShape.AddShape(shape);
            Invalidate();
        }

        /// <summary>
        /// 移除组合形状
        /// </summary>
        public void RemoveCompositeShape(ShapeItem shape)
        {
            compositeShape.RemoveShape(shape);
            Invalidate();
        }

        /// <summary>
        /// 清空组合形状
        /// </summary>
        public void ClearCompositeShapes()
        {
            compositeShape.Clear();
            Invalidate();
        }

        /// <summary>
        /// 设置组合形状位置
        /// </summary>
        public void SetCompositePosition(Point position)
        {
            compositeShape.Position = position;
            Invalidate();
        }

        private void RefreshControl()
        {
            Invalidate();
            Update();

            // 通知设计器刷新
            if (Site != null && Site.DesignMode)
            {
                var service = GetService(typeof(System.ComponentModel.Design.IComponentChangeService))
                    as System.ComponentModel.Design.IComponentChangeService;

                service?.OnComponentChanged(this, null, null, null);
            }
        }

        #endregion

        #region 绘制

        protected override void DrawBackground(Graphics g)
        {
            if (BackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }
        }

        protected override void DrawContent(Graphics g)
        {
            if (useCompositeMode)
            {
                // 绘制组合形状
                compositeShape.Draw(g);
            }
            else
            {
                // 绘制单一形状
                if (shapeRenderer != null)
                {
                    // 更新某些渲染器的动态属性
                    UpdateRendererProperties();
                    var bounds = GetShapeBounds();
                    shapeRenderer.Draw(g, bounds, shapeStyle);
                }
            }

            // 绘制文本
            if (textItems != null && textItems.Count > 0)
            {
                textItems.DrawAll(g);
            }
        }

        protected override void DrawBorder(Graphics g)
        {
            // 边框由形状渲染器处理, 这里不需要额外绘制
        }

        private void UpdateRendererProperties()
        {
            if (shapeRenderer is LineRenderer line)
            {
                line.Mode = LineMode;
                line.Margin = LineMargin;
            }
            else if (shapeRenderer is PolygonRenderer polygon)
            {
                polygon.Sides = PolygonSides;
                polygon.StartAngle = PolygonStartAngle;
            }
            else if (shapeRenderer is StarRenderer star)
            {
                star.Points = StarPoints;
                star.InnerRadiusRatio = StarInnerRadiusRatio;
                star.StartAngle = StarStartAngle;
            }
            else if (shapeRenderer is RoundedRectangleRenderer rounded)
            {
                rounded.CornerRadius = CornerRadius;
            }
            else if (shapeRenderer is TrapezoidRenderer trapezoid)
            {
                trapezoid.TopWidthRatio = TrapezoidTopWidthRatio;
            }
            else if (shapeRenderer is ArrowRenderer arrow)
            {
                arrow.Direction = ArrowDirection;
                arrow.ArrowHeadWidthRatio = ArrowHeadWidthRatio;
                arrow.ArrowTailWidthRatio = ArrowTailWidthRatio;
                arrow.TailNotchDepthRatio = TailNotchDepthRatio;
            }
            else if (shapeRenderer is CylinderRenderer cylinder)
            {
                cylinder.HorizontalViewAngle = HorizontalViewAngle / 100f;
                cylinder.VerticalViewAngle = VerticalViewAngle;
            }
            else if (shapeRenderer is CuboidRenderer cuboid)
            {
                cuboid.HorizontalViewAngle = HorizontalViewAngle;
                cuboid.VerticalViewAngle = VerticalViewAngle;
                cuboid.DepthRatio = DepthRatio;
            }
            else if (shapeRenderer is SphereRenderer sphere)
            {
                sphere.LightHorizontalAngle = HorizontalViewAngle;
                sphere.LightVerticalAngle = VerticalViewAngle;
            }
        }

        private Rectangle GetShapeBounds()
        {
            int padding = shapeStyle.ShowShadow ?
                Math.Max(shapeStyle.ShadowOffsetX, shapeStyle.ShadowOffsetY) + 5 : 5;

            return new Rectangle(
                padding,
                padding,
                Width - padding * 2,
                Height - padding * 2);
        }

        protected override void ApplyThemeStyles()
        {
            base.ApplyThemeStyles();

            if (UseTheme && Theme != null)
            {
                shapeStyle.FillColor = GetThemeColor(c => c.Primary, shapeStyle.FillColor);
                shapeStyle.BorderColor = GetThemeColor(c => c.Border, shapeStyle.BorderColor);
                shapeStyle.GradientStartColor = GetThemeColor(c => c.PrimaryLight, shapeStyle.GradientStartColor);
                shapeStyle.GradientEndColor = GetThemeColor(c => c.PrimaryDark, shapeStyle.GradientEndColor);
            }
        }

        #endregion
    }

    #region 形状渲染器

    /// <summary>
    /// 形状渲染器抽象基类
    /// </summary>
    public abstract class ShapeRendererBase : IShapeRenderer
    {
        public abstract ShapeType ShapeType { get; }

        public virtual void Draw(Graphics g, Rectangle bounds, ShapeStyle style)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // 绘制阴影
            if (style.ShowShadow)
            {
                DrawShadow(g, bounds, style);
            }

            // 绘制形状
            using (var path = GetPath(bounds))
            {
                if (path != null && path.PointCount > 0)
                {
                    DrawShape(g, path, bounds, style);
                }
            }
        }

        protected virtual void DrawShadow(Graphics g, Rectangle bounds, ShapeStyle style)
        {
            var shadowBounds = new Rectangle(
                bounds.X + style.ShadowOffsetX,
                bounds.Y + style.ShadowOffsetY,
                bounds.Width,
                bounds.Height);

            using (var path = GetPath(shadowBounds))
            {
                if (path != null && path.PointCount > 0)
                {
                    using (var brush = new SolidBrush(style.ShadowColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        protected virtual void DrawShape(Graphics g, GraphicsPath path, Rectangle bounds, ShapeStyle style)
        {
            // 填充
            if (style.FillColor.A > 0 || style.UseGradient)
            {
                using (Brush brush = CreateFillBrush(bounds, style))
                {
                    g.FillPath(brush, path);
                }
            }

            // 边框
            if (style.BorderWidth > 0 && style.BorderColor.A > 0)
            {
                using (var pen = new Pen(style.BorderColor, style.BorderWidth))
                {
                    pen.DashStyle = style.BorderStyle;
                    g.DrawPath(pen, path);
                }
            }
        }

        protected virtual Brush CreateFillBrush(Rectangle bounds, ShapeStyle style)
        {
            if (style.UseGradient)
            {
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    return new LinearGradientBrush(
                        bounds,
                        ApplyOpacity(style.GradientStartColor, style.Opacity),
                        ApplyOpacity(style.GradientEndColor, style.Opacity),
                        style.GradientMode);
                }
            }

            return new SolidBrush(ApplyOpacity(style.FillColor, style.Opacity));
        }

        protected Color ApplyOpacity(Color color, float opacity)
        {
            opacity = Math.Max(0, Math.Min(1, opacity));
            return Color.FromArgb((int)(color.A * opacity), color);
        }

        public abstract GraphicsPath GetPath(Rectangle bounds);

        public virtual IShapeRenderer Clone()
        {
            return (IShapeRenderer)this.MemberwiseClone();
        }

        #region 辅助方法

        protected GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0 || rect.Width <= 0 || rect.Height <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // 左上角
            path.AddArc(arc, 180, 90);
            // 右上角
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            // 右下角
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // 左下角
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        #endregion
    }

    #region 基础形状

    /// <summary>
    /// 线段渲染器
    /// </summary>
    public class LineRenderer : ShapeRendererBase
    {
        public override ShapeType ShapeType => ShapeType.Line;

        /// <summary>
        /// 线段模式
        /// </summary>
        public LineMode Mode { get; set; } = LineMode.DiagonalDown;

        /// <summary>
        /// 自定义起点(相对坐标 0-1)
        /// </summary>
        public PointF StartPoint { get; set; } = new PointF(0, 0);

        /// <summary>
        /// 自定义终点(相对坐标 0-1)
        /// </summary>
        public PointF EndPoint { get; set; } = new PointF(1, 1);

        /// <summary>
        /// 线段边距(像素)
        /// </summary>
        public int Margin { get; set; } = 5;

        public override void Draw(Graphics g, Rectangle bounds, ShapeStyle style)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var points = GetLinePoints(bounds);
            var start = points.Item1;
            var end = points.Item2;

            if (style.BorderWidth > 0 && style.FillColor.A > 0)
            {
                using (var pen = new Pen(ApplyOpacity(style.FillColor, style.Opacity), style.BorderWidth))
                {
                    pen.DashStyle = style.BorderStyle;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, start, end);
                }
            }
        }

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            var points = GetLinePoints(bounds);
            path.AddLine(points.Item1, points.Item2);
            return path;
        }

        private Tuple<PointF, PointF> GetLinePoints(Rectangle bounds)
        {
            PointF start, end;

            switch (Mode)
            {
                case LineMode.Horizontal:
                    start = new PointF(bounds.Left + Margin, bounds.Top + bounds.Height / 2f);
                    end = new PointF(bounds.Right - Margin, bounds.Top + bounds.Height / 2f);
                    break;

                case LineMode.Vertical:
                    start = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + Margin);
                    end = new PointF(bounds.Left + bounds.Width / 2f, bounds.Bottom - Margin);
                    break;

                case LineMode.DiagonalDown:
                    start = new PointF(bounds.Left + Margin, bounds.Top + Margin);
                    end = new PointF(bounds.Right - Margin, bounds.Bottom - Margin);
                    break;

                case LineMode.DiagonalUp:
                    start = new PointF(bounds.Left + Margin, bounds.Bottom - Margin);
                    end = new PointF(bounds.Right - Margin, bounds.Top + Margin);
                    break;

                case LineMode.Custom:
                default:
                    start = new PointF(
                        bounds.X + bounds.Width * StartPoint.X,
                        bounds.Y + bounds.Height * StartPoint.Y);
                    end = new PointF(
                        bounds.X + bounds.Width * EndPoint.X,
                        bounds.Y + bounds.Height * EndPoint.Y);
                    break;
            }

            return Tuple.Create(start, end);
        }
    }

    /// <summary>
    /// 矩形渲染器
    /// </summary>
    public class RectangleRenderer : ShapeRendererBase
    {
        public override ShapeType ShapeType => ShapeType.Rectangle;

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            path.AddRectangle(bounds);
            return path;
        }
    }

    /// <summary>
    /// 圆角矩形渲染器
    /// </summary>
    public class RoundedRectangleRenderer : ShapeRendererBase
    {
        public int CornerRadius { get; set; } = 10;

        public override ShapeType ShapeType => ShapeType.RoundedRectangle;

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            return GetRoundedRectanglePath(bounds, CornerRadius);
        }
    }

    /// <summary>
    /// 椭圆渲染器
    /// </summary>
    public class EllipseRenderer : ShapeRendererBase
    {
        public override ShapeType ShapeType => ShapeType.Ellipse;

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            path.AddEllipse(bounds);
            return path;
        }
    }

    /// <summary>
    /// 圆形渲染器
    /// </summary>
    public class CircleRenderer : ShapeRendererBase
    {
        public override ShapeType ShapeType => ShapeType.Circle;

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            // 使用最小边作为直径, 保持圆形
            int size = Math.Min(bounds.Width, bounds.Height);
            int offsetX = (bounds.Width - size) / 2;
            int offsetY = (bounds.Height - size) / 2;
            var circleBounds = new Rectangle(
                bounds.X + offsetX,
                bounds.Y + offsetY,
                size,
                size);
            path.AddEllipse(circleBounds);
            return path;
        }
    }

    /// <summary>
    /// 三角形渲染器
    /// </summary>
    public class TriangleRenderer : ShapeRendererBase
    {
        public override ShapeType ShapeType => ShapeType.Triangle;

        /// <summary>
        /// 自定义顶点(如果为空则使用默认位置)
        /// </summary>
        public VertexCollection CustomVertices { get; set; }

        public TriangleRenderer()
        {
            CustomVertices = new VertexCollection();
        }

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            var points = GetTrianglePoints(bounds);

            if (points.Length >= 3)
            {
                path.AddPolygon(points);
            }

            return path;
        }

        private PointF[] GetTrianglePoints(Rectangle bounds)
        {
            // 如果有自定义顶点且数量为3, 使用自定义顶点
            if (CustomVertices != null && CustomVertices.Count == 3)
            {
                return CustomVertices.ToArray();
            }

            // 否则使用默认三角形(顶点在上, 底边在下)
            return new PointF[]
            {
                new PointF(bounds.X + bounds.Width / 2f, bounds.Y), // 顶点
                new PointF(bounds.Right, bounds.Bottom),             // 右下
                new PointF(bounds.Left, bounds.Bottom)               // 左下
            };
        }

        /// <summary>
        /// 获取当前顶点位置(用于属性显示)
        /// </summary>
        public PointF[] GetCurrentVertices(Rectangle bounds)
        {
            return GetTrianglePoints(bounds);
        }

        /// <summary>
        /// 设置顶点位置
        /// </summary>
        public void SetVertices(params PointF[] vertices)
        {
            CustomVertices.Clear();
            if (vertices != null && vertices.Length == 3)
            {
                foreach (var vertex in vertices)
                {
                    CustomVertices.Add(vertex);
                }
            }
        }
    }

    /// <summary>
    /// 梯形渲染器
    /// </summary>
    public class TrapezoidRenderer : ShapeRendererBase
    {
        public float TopWidthRatio { get; set; } = 0.6f;
        public VertexCollection CustomVertices { get; set; }

        public override ShapeType ShapeType => ShapeType.Trapezoid;

        public TrapezoidRenderer()
        {
            CustomVertices = new VertexCollection();
        }

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            var points = GetTrapezoidPoints(bounds);

            if (points.Length >= 4)
            {
                path.AddPolygon(points);
            }

            return path;
        }

        private PointF[] GetTrapezoidPoints(Rectangle bounds)
        {
            // 如果有自定义顶点且数量为4, 使用自定义顶点
            if (CustomVertices != null && CustomVertices.Count == 4)
            {
                return CustomVertices.ToArray();
            }

            // 否则使用默认梯形
            float topWidth = bounds.Width * TopWidthRatio;
            float offset = (bounds.Width - topWidth) / 2f;

            return new PointF[]
            {
                new PointF(bounds.X + offset, bounds.Y),                    // 左上
                new PointF(bounds.X + offset + topWidth, bounds.Y),         // 右上
                new PointF(bounds.Right, bounds.Bottom),                     // 右下
                new PointF(bounds.Left, bounds.Bottom)                       // 左下
            };
        }

        public PointF[] GetCurrentVertices(Rectangle bounds)
        {
            return GetTrapezoidPoints(bounds);
        }

        public void SetVertices(params PointF[] vertices)
        {
            CustomVertices.Clear();
            if (vertices != null && vertices.Length == 4)
            {
                foreach (var vertex in vertices)
                {
                    CustomVertices.Add(vertex);
                }
            }
        }
    }

    #endregion

    #region 多边形/箭头/星形

    /// <summary>
    /// 多边形渲染器
    /// </summary>
    public class PolygonRenderer : ShapeRendererBase
    {
        public int Sides { get; set; } = 6;
        public float StartAngle { get; set; } = -90f;
        public VertexCollection CustomVertices { get; set; }

        public override ShapeType ShapeType => ShapeType.Polygon;

        public PolygonRenderer()
        {
            CustomVertices = new VertexCollection();
        }

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            var points = GetPolygonPoints(bounds);

            if (points.Length >= 3)
            {
                path.AddPolygon(points);
            }

            return path;
        }

        private PointF[] GetPolygonPoints(Rectangle bounds)
        {
            // 如果有自定义顶点且数量>=3, 使用自定义顶点
            if (CustomVertices != null && CustomVertices.Count >= 3)
            {
                return CustomVertices.ToArray();
            }

            // 否则根据边数生成规则多边形
            if (Sides < 3)
            {
                return new PointF[0];
            }

            var points = new PointF[Sides];
            var center = new PointF(
                bounds.X + bounds.Width / 2f,
                bounds.Y + bounds.Height / 2f);

            var radiusX = bounds.Width / 2f;
            var radiusY = bounds.Height / 2f;

            for (int i = 0; i < Sides; i++)
            {
                var angle = StartAngle + (360f / Sides * i);
                var radians = angle * Math.PI / 180;

                points[i] = new PointF(
                    center.X + (float)(radiusX * Math.Cos(radians)),
                    center.Y + (float)(radiusY * Math.Sin(radians)));
            }

            return points;
        }

        public PointF[] GetCurrentVertices(Rectangle bounds)
        {
            return GetPolygonPoints(bounds);
        }

        public void SetVertices(params PointF[] vertices)
        {
            CustomVertices.Clear();
            if (vertices != null && vertices.Length >= 3)
            {
                foreach (var vertex in vertices)
                {
                    CustomVertices.Add(vertex);
                }
            }
        }
    }

    /// <summary>
    /// 星形渲染器
    /// </summary>
    public class StarRenderer : ShapeRendererBase
    {
        public int Points { get; set; } = 5;
        public float InnerRadiusRatio { get; set; } = 0.4f;
        public float StartAngle { get; set; } = -90f;

        public override ShapeType ShapeType => ShapeType.Star;

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();

            if (Points < 3)
            {
                return path;
            }

            var starPoints = GetStarPoints(bounds);
            if (starPoints.Length >= 3)
            {
                path.AddPolygon(starPoints);
            }

            return path;
        }

        private PointF[] GetStarPoints(Rectangle bounds)
        {
            var points = new List<PointF>();
            var center = new PointF(
                bounds.X + bounds.Width / 2f,
                bounds.Y + bounds.Height / 2f);

            var outerRadiusX = bounds.Width / 2f;
            var outerRadiusY = bounds.Height / 2f;
            var innerRadiusX = outerRadiusX * InnerRadiusRatio;
            var innerRadiusY = outerRadiusY * InnerRadiusRatio;

            for (int i = 0; i < Points * 2; i++)
            {
                var angle = StartAngle + (360f / (Points * 2) * i);
                var radians = angle * Math.PI / 180;

                var isOuter = i % 2 == 0;
                var radiusX = isOuter ? outerRadiusX : innerRadiusX;
                var radiusY = isOuter ? outerRadiusY : innerRadiusY;

                points.Add(new PointF(
                    center.X + (float)(radiusX * Math.Cos(radians)),
                    center.Y + (float)(radiusY * Math.Sin(radians))));
            }

            return points.ToArray();
        }
    }

    /// <summary>
    /// 箭头渲染器
    /// </summary>
    public class ArrowRenderer : ShapeRendererBase
    {
        public override ShapeType ShapeType => ShapeType.Arrow;

        /// <summary>
        /// 箭头方向
        /// </summary>
        public ArrowDirection Direction { get; set; } = ArrowDirection.Right;

        /// <summary>
        /// 箭头头部宽度比例(0-1, 相对于整体宽度)
        /// </summary>
        public float ArrowHeadWidthRatio { get; set; } = 0.4f;

        /// <summary>
        /// 箭头尾部宽度比例(0-1, 相对于整体高度/宽度)
        /// </summary>
        public float ArrowTailWidthRatio { get; set; } = 0.6f;

        /// <summary>
        /// 箭头尾部缺口深度比例(0-1, 相对于整体宽度)
        /// </summary>
        public float TailNotchDepthRatio { get; set; } = 0.2f;

        public override GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return path;
            }

            // 确保比例在有效范围内
            float headRatio = Math.Max(0.1f, Math.Min(0.9f, ArrowHeadWidthRatio));
            float tailRatio = Math.Max(0.1f, Math.Min(0.9f, ArrowTailWidthRatio));
            float notchRatio = Math.Max(0f, Math.Min(0.5f, TailNotchDepthRatio));

            PointF[] points = null;

            switch (Direction)
            {
                case ArrowDirection.Left:
                    points = GetLeftArrowPoints(bounds, headRatio, tailRatio, notchRatio);
                    break;
                case ArrowDirection.Right:
                    points = GetRightArrowPoints(bounds, headRatio, tailRatio, notchRatio);
                    break;
                case ArrowDirection.Up:
                    points = GetUpArrowPoints(bounds, headRatio, tailRatio, notchRatio);
                    break;
                case ArrowDirection.Down:
                    points = GetDownArrowPoints(bounds, headRatio, tailRatio, notchRatio);
                    break;
            }

            if (points != null && points.Length > 0)
            {
                path.AddPolygon(points);
                path.CloseFigure();
            }

            return path;
        }

        /// <summary>
        /// 获取向左箭头的点
        /// </summary>
        private PointF[] GetLeftArrowPoints(Rectangle bounds, float headRatio, float tailRatio, float notchRatio)
        {
            float width = bounds.Width;
            float height = bounds.Height;
            float x = bounds.X;
            float y = bounds.Y;

            // 计算关键尺寸
            float headWidth = width * headRatio;        // 箭头头部宽度
            float bodyWidth = width - headWidth;        // 箭头身体宽度
            float tailHeight = height * tailRatio;      // 箭头尾部高度
            float notchDepth = bodyWidth * notchRatio;  // 尾部缺口深度

            float centerY = y + height / 2;
            float tailTop = centerY - tailHeight / 2;
            float tailBottom = centerY + tailHeight / 2;

            return new PointF[]
            {
                new PointF(x, centerY),                              // 箭头尖端(左侧中心)
                new PointF(x + headWidth, y),                        // 箭头头部右上角
                new PointF(x + headWidth, tailTop),                  // 箭头身体右上角
                new PointF(x + width, tailTop),                      // 箭头尾部右上角
                new PointF(x + width - notchDepth, centerY),         // 箭头尾部缺口
                new PointF(x + width, tailBottom),                   // 箭头尾部右下角
                new PointF(x + headWidth, tailBottom),               // 箭头身体右下角
                new PointF(x + headWidth, y + height)                // 箭头头部右下角
            };
        }

        /// <summary>
        /// 获取向右箭头的点
        /// </summary>
        private PointF[] GetRightArrowPoints(Rectangle bounds, float headRatio, float tailRatio, float notchRatio)
        {
            float width = bounds.Width;
            float height = bounds.Height;
            float x = bounds.X;
            float y = bounds.Y;

            float headWidth = width * headRatio;
            float bodyWidth = width - headWidth;
            float tailHeight = height * tailRatio;
            float notchDepth = bodyWidth * notchRatio;

            float centerY = y + height / 2;
            float tailTop = centerY - tailHeight / 2;
            float tailBottom = centerY + tailHeight / 2;

            return new PointF[]
            {
                new PointF(x + width, centerY),                      // 箭头尖端(右侧中心)
                new PointF(x + bodyWidth, y + height),               // 箭头头部左下角
                new PointF(x + bodyWidth, tailBottom),               // 箭头身体左下角
                new PointF(x, tailBottom),                           // 箭头尾部左下角
                new PointF(x + notchDepth, centerY),                 // 箭头尾部缺口
                new PointF(x, tailTop),                              // 箭头尾部左上角
                new PointF(x + bodyWidth, tailTop),                  // 箭头身体左上角
                new PointF(x + bodyWidth, y)                         // 箭头头部左上角
            };
        }

        /// <summary>
        /// 获取向上箭头的点
        /// </summary>
        private PointF[] GetUpArrowPoints(Rectangle bounds, float headRatio, float tailRatio, float notchRatio)
        {
            float width = bounds.Width;
            float height = bounds.Height;
            float x = bounds.X;
            float y = bounds.Y;

            float headHeight = height * headRatio;
            float bodyHeight = height - headHeight;
            float tailWidth = width * tailRatio;
            float notchDepth = bodyHeight * notchRatio;

            float centerX = x + width / 2;
            float tailLeft = centerX - tailWidth / 2;
            float tailRight = centerX + tailWidth / 2;

            return new PointF[]
            {
                new PointF(centerX, y),                              // 箭头尖端(顶部中心)
                new PointF(x + width, y + headHeight),               // 箭头头部右下角
                new PointF(tailRight, y + headHeight),               // 箭头身体右下角
                new PointF(tailRight, y + height),                   // 箭头尾部右下角
                new PointF(centerX, y + height - notchDepth),        // 箭头尾部缺口
                new PointF(tailLeft, y + height),                    // 箭头尾部左下角
                new PointF(tailLeft, y + headHeight),                // 箭头身体左下角
                new PointF(x, y + headHeight)                        // 箭头头部左下角
            };
        }

        /// <summary>
        /// 获取向下箭头的点
        /// </summary>
        private PointF[] GetDownArrowPoints(Rectangle bounds, float headRatio, float tailRatio, float notchRatio)
        {
            float width = bounds.Width;
            float height = bounds.Height;
            float x = bounds.X;
            float y = bounds.Y;

            float headHeight = height * headRatio;
            float bodyHeight = height - headHeight;
            float tailWidth = width * tailRatio;
            float notchDepth = bodyHeight * notchRatio;

            float centerX = x + width / 2;
            float tailLeft = centerX - tailWidth / 2;
            float tailRight = centerX + tailWidth / 2;

            return new PointF[]
            {
                new PointF(centerX, y + height),                     // 箭头尖端(底部中心)
                new PointF(x, y + bodyHeight),                       // 箭头头部左上角
                new PointF(tailLeft, y + bodyHeight),                // 箭头身体左上角
                new PointF(tailLeft, y),                             // 箭头尾部左上角
                new PointF(centerX, y + notchDepth),                 // 箭头尾部缺口
                new PointF(tailRight, y),                            // 箭头尾部右上角
                new PointF(tailRight, y + bodyHeight),               // 箭头身体右上角
                new PointF(x + width, y + bodyHeight)                // 箭头头部右上角
            };
        }

        public override IShapeRenderer Clone()
        {
            return new ArrowRenderer
            {
                Direction = this.Direction,
                ArrowHeadWidthRatio = this.ArrowHeadWidthRatio,
                ArrowTailWidthRatio = this.ArrowTailWidthRatio,
                TailNotchDepthRatio = this.TailNotchDepthRatio
            };
        }
    }

    #endregion

    #region 3D形状

    /// <summary>
    /// 圆柱体渲染器
    /// </summary>
    public class CylinderRenderer : IShapeRenderer
    {
        public ShapeType ShapeType => ShapeType.Cylinder;

        public float TopEllipseRatio { get; set; } = 0.2f;
        public float HorizontalViewAngle { get; set; } = 0f;
        public float VerticalViewAngle { get; set; } = 30f;

        public void Draw(Graphics g, Rectangle bounds, ShapeStyle style)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // 计算实际偏移量
            var horizontalOffset = bounds.Width * HorizontalViewAngle * 0.15f;
            var absHorizontalOffset = Math.Abs(horizontalOffset);

            // 限制垂直角度
            var vAngle = Math.Max(10, Math.Min(80, VerticalViewAngle));
            var verticalFactor = (float)Math.Sin(vAngle * Math.PI / 180);

            // 调整bounds
            var drawBounds = new Rectangle(
                bounds.X + (int)Math.Max(0, absHorizontalOffset / 2),
                bounds.Y,
                bounds.Width - (int)absHorizontalOffset,
                bounds.Height
            );

            if (drawBounds.Width <= 0 || drawBounds.Height <= 0)
            {
                return;
            }

            var ellipseHeight = drawBounds.Height * TopEllipseRatio * verticalFactor;
            var bodyHeight = drawBounds.Height - ellipseHeight;

            var frontColor = style.FrontFaceColor.A > 0 ? style.FrontFaceColor : style.FillColor;
            var topColor = style.TopFaceColor.A > 0 ? style.TopFaceColor : LightenColor(frontColor, 0.3f);

            // 计算实际绘制位置(考虑水平偏移方向)
            float actualX = drawBounds.X;
            if (horizontalOffset < 0)
            {
                actualX = drawBounds.X + absHorizontalOffset;
            }

            // 绘制阴影
            if (style.ShowShadow)
            {
                DrawCylinderShadow(g, drawBounds, actualX, ellipseHeight, bodyHeight,
                    horizontalOffset, style);
            }

            // 绘制圆柱体主体
            var bodyRect = new RectangleF(
                actualX,
                drawBounds.Y + ellipseHeight / 2,
                drawBounds.Width - absHorizontalOffset,
                bodyHeight);

            // 根据水平偏移决定渐变方向
            if (Math.Abs(HorizontalViewAngle) > 0.05f)
            {
                Color leftColor, rightColor;
                if (horizontalOffset > 0)
                {
                    leftColor = DarkenColor(frontColor, 0.3f);
                    rightColor = LightenColor(frontColor, 0.1f);
                }
                else
                {
                    leftColor = LightenColor(frontColor, 0.1f);
                    rightColor = DarkenColor(frontColor, 0.3f);
                }

                using (var brush = new LinearGradientBrush(
                    bodyRect,
                    leftColor,
                    rightColor,
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, bodyRect);
                }
            }
            else
            {
                using (var brush = new SolidBrush(frontColor))
                {
                    g.FillRectangle(brush, bodyRect);
                }
            }

            // 绘制底部椭圆
            if (vAngle < 85)
            {
                var bottomEllipse = new RectangleF(
                    actualX,
                    drawBounds.Bottom - ellipseHeight,
                    bodyRect.Width,
                    ellipseHeight);

                var bottomColor = DarkenColor(frontColor, 0.4f);
                using (var brush = new SolidBrush(bottomColor))
                {
                    g.FillEllipse(brush, bottomEllipse);
                }

                if (style.BorderWidth > 0 && style.BorderColor.A > 0)
                {
                    using (var pen = new Pen(DarkenColor(style.BorderColor, 0.3f), style.BorderWidth))
                    {
                        g.DrawArc(pen, bottomEllipse, 0, 180);
                    }
                }
            }

            // 绘制顶部椭圆
            var topEllipse = new RectangleF(
                actualX,
                drawBounds.Y,
                bodyRect.Width,
                ellipseHeight);

            using (var brush = new SolidBrush(topColor))
            {
                g.FillEllipse(brush, topEllipse);
            }

            // 绘制边框
            if (style.BorderWidth > 0 && style.BorderColor.A > 0)
            {
                using (var pen = new Pen(style.BorderColor, style.BorderWidth))
                {
                    g.DrawEllipse(pen, topEllipse);

                    // 侧边线
                    float sideY1 = bodyRect.Top;
                    float sideY2 = bodyRect.Bottom;

                    // 左侧线
                    if (horizontalOffset >= 0 || Math.Abs(HorizontalViewAngle) < 0.05f)
                    {
                        g.DrawLine(pen, bodyRect.Left, sideY1, bodyRect.Left, sideY2);
                    }
                    // 右侧线
                    if (horizontalOffset <= 0 || Math.Abs(HorizontalViewAngle) < 0.05f)
                    {
                        g.DrawLine(pen, bodyRect.Right, sideY1, bodyRect.Right, sideY2);
                    }
                }
            }
        }

        private void DrawCylinderShadow(Graphics g, Rectangle bounds, float actualX,
            float ellipseHeight, float bodyHeight, float horizontalOffset, ShapeStyle style)
        {
            var shadowBounds = new Rectangle(
                bounds.X + style.ShadowOffsetX,
                bounds.Y + style.ShadowOffsetY,
                bounds.Width,
                bounds.Height
            );

            float shadowActualX = actualX + style.ShadowOffsetX;

            var shadowRect = new RectangleF(
                shadowActualX,
                shadowBounds.Y + ellipseHeight / 2,
                bounds.Width - Math.Abs(horizontalOffset),
                bodyHeight);

            using (var brush = new SolidBrush(style.ShadowColor))
            {
                g.FillRectangle(brush, shadowRect);

                var bottomShadowEllipse = new RectangleF(
                    shadowActualX,
                    shadowBounds.Bottom - ellipseHeight,
                    shadowRect.Width,
                    ellipseHeight);

                g.FillEllipse(brush, bottomShadowEllipse);
            }
        }

        public GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            path.AddRectangle(bounds);
            return path;
        }

        public IShapeRenderer Clone()
        {
            return (IShapeRenderer)this.MemberwiseClone();
        }

        private Color LightenColor(Color color, float amount)
        {
            return Color.FromArgb(color.A,
                Math.Min(255, color.R + (int)((255 - color.R) * amount)),
                Math.Min(255, color.G + (int)((255 - color.G) * amount)),
                Math.Min(255, color.B + (int)((255 - color.B) * amount)));
        }

        private Color DarkenColor(Color color, float amount)
        {
            return Color.FromArgb(color.A,
                (int)(color.R * (1 - amount)),
                (int)(color.G * (1 - amount)),
                (int)(color.B * (1 - amount)));
        }
    }

    /// <summary>
    /// 椭球体渲染器
    /// </summary>
    public class SphereRenderer : IShapeRenderer
    {
        public ShapeType ShapeType => ShapeType.Sphere;

        /// <summary>
        /// 光源水平角度(0-360)
        /// </summary>
        public float LightHorizontalAngle { get; set; } = 135f;

        /// <summary>
        /// 光源垂直角度(0-90)
        /// </summary>
        public float LightVerticalAngle { get; set; } = 45f;

        public void Draw(Graphics g, Rectangle bounds, ShapeStyle style)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(bounds);

                // 根据光源角度计算高光位置
                var hRad = LightHorizontalAngle * Math.PI / 180;
                var vRad = LightVerticalAngle * Math.PI / 180;

                var centerX = bounds.X + bounds.Width / 2f;
                var centerY = bounds.Y + bounds.Height / 2f;

                // 高光偏移
                var highlightOffsetX = (float)(Math.Cos(hRad) * Math.Cos(vRad)) * bounds.Width * 0.25f;
                var highlightOffsetY = (float)(Math.Sin(hRad) * Math.Cos(vRad)) * bounds.Height * 0.25f;

                var highlightCenter = new PointF(
                    centerX + highlightOffsetX,
                    centerY - highlightOffsetY);

                using (var pgb = new PathGradientBrush(path))
                {
                    var lightColor = style.TopFaceColor.A > 0 ?
                        style.TopFaceColor : LightenColor(style.FillColor, 0.5f);
                    var darkColor = style.FrontFaceColor.A > 0 ?
                        style.FrontFaceColor : DarkenColor(style.FillColor, 0.3f);

                    pgb.CenterPoint = highlightCenter;
                    pgb.CenterColor = lightColor;
                    pgb.SurroundColors = new[] { darkColor };

                    // 设置混合模式以获得更平滑的渐变
                    var blend = new Blend();
                    blend.Positions = new[] { 0f, 0.3f, 0.7f, 1f };
                    blend.Factors = new[] { 1f, 0.6f, 0.3f, 0f };
                    pgb.Blend = blend;

                    g.FillPath(pgb, path);
                }

                // 绘制边框
                if (style.BorderWidth > 0 && style.BorderColor.A > 0)
                {
                    using (var pen = new Pen(style.BorderColor, style.BorderWidth))
                    {
                        g.DrawEllipse(pen, bounds);
                    }
                }
            }
        }

        public GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            path.AddEllipse(bounds);
            return path;
        }

        public IShapeRenderer Clone()
        {
            return (IShapeRenderer)this.MemberwiseClone();
        }

        private Color LightenColor(Color color, float amount)
        {
            return Color.FromArgb(color.A,
                Math.Min(255, color.R + (int)((255 - color.R) * amount)),
                Math.Min(255, color.G + (int)((255 - color.G) * amount)),
                Math.Min(255, color.B + (int)((255 - color.B) * amount)));
        }

        private Color DarkenColor(Color color, float amount)
        {
            return Color.FromArgb(color.A,
                (int)(color.R * (1 - amount)),
                (int)(color.G * (1 - amount)),
                (int)(color.B * (1 - amount)));
        }
    }

    /// <summary>
    /// 立方体渲染器
    /// </summary>
    public class CuboidRenderer : IShapeRenderer
    {
        public ShapeType ShapeType => ShapeType.Cuboid;

        public float HorizontalViewAngle { get; set; } = 30f;
        public float VerticalViewAngle { get; set; } = 30f;
        public float DepthRatio { get; set; } = 0.3f;

        public void Draw(Graphics g, Rectangle bounds, ShapeStyle style)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            // 限制角度范围
            var hAngle = Math.Max(-70, Math.Min(70, HorizontalViewAngle));
            var vAngle = Math.Max(-70, Math.Min(70, VerticalViewAngle));

            // 计算深度
            var hRadians = hAngle * Math.PI / 180;
            var vRadians = vAngle * Math.PI / 180;

            var maxDepthRatio = Math.Min(DepthRatio, 0.4f);
            var depthX = bounds.Width * maxDepthRatio * (float)Math.Abs(Math.Sin(hRadians));
            var depthY = bounds.Height * maxDepthRatio * (float)Math.Abs(Math.Sin(vRadians));

            // 计算边距
            var shadowPadding = style.ShowShadow ?
                Math.Max(style.ShadowOffsetX, style.ShadowOffsetY) + 3 : 0;
            var borderPadding = style.BorderWidth + 2;
            var totalPadding = shadowPadding + borderPadding;

            // 计算前面矩形的大小
            var frontWidth = bounds.Width - depthX - totalPadding * 2;
            var frontHeight = bounds.Height - depthY - totalPadding * 2;

            if (frontWidth <= 0 || frontHeight <= 0)
                return;

            // 根据倾斜方向确定前面的位置和深度偏移
            float frontLeft, frontTop, offsetX, offsetY;
            bool showLeftSide, showTopSide;

            if (hAngle > 0) // 向右倾斜, 看到左侧面
            {
                frontLeft = bounds.X + depthX + totalPadding;
                offsetX = -depthX; // 从前面到左侧面, 向左(负方向)
                showLeftSide = true;
            }
            else // 向左倾斜或垂直, 看到右侧面
            {
                frontLeft = bounds.X + totalPadding;
                offsetX = depthX; // 从前面到右侧面, 向右(正方向)
                showLeftSide = false;
            }

            if (vAngle > 0) // 向上倾斜, 看到顶面
            {
                frontTop = bounds.Y + depthY + totalPadding;
                offsetY = -depthY; // 从前面到顶面, 向上(负方向)
                showTopSide = true;
            }
            else // 向下倾斜或水平, 看到底面
            {
                frontTop = bounds.Y + totalPadding;
                offsetY = depthY; // 从前面到底面, 向下(正方向)
                showTopSide = false;
            }

            var frontRect = new RectangleF(frontLeft, frontTop, frontWidth, frontHeight);

            // 获取颜色
            var frontColor = style.FrontFaceColor.A > 0 ? style.FrontFaceColor : style.FillColor;
            var topColor = style.TopFaceColor.A > 0 ? style.TopFaceColor : LightenColor(frontColor, 0.3f);
            var bottomColor = DarkenColor(topColor, 0.4f);
            var sideColor = style.RightFaceColor.A > 0 ? style.RightFaceColor : DarkenColor(frontColor, 0.2f);

            // 绘制阴影
            if (style.ShowShadow)
            {
                DrawCuboidShadow(g, frontRect, offsetX, offsetY, showTopSide, showLeftSide, style);
            }

            // 绘制可见的面(从后到前)
            // 1. 顶面或底面
            if (showTopSide)
            {
                DrawTopFace(g, frontRect, offsetX, offsetY, topColor, style.BorderColor, style.BorderWidth);
            }
            else
            {
                DrawBottomFace(g, frontRect, offsetX, offsetY, bottomColor, style.BorderColor, style.BorderWidth);
            }

            // 2. 左侧面或右侧面
            if (showLeftSide)
            {
                DrawLeftFace(g, frontRect, offsetX, offsetY, sideColor, style.BorderColor, style.BorderWidth);
            }
            else
            {
                DrawRightFace(g, frontRect, offsetX, offsetY, sideColor, style.BorderColor, style.BorderWidth);
            }

            // 3. 前面
            using (var brush = new SolidBrush(frontColor))
            {
                g.FillRectangle(brush, frontRect);
            }

            if (style.BorderWidth > 0 && style.BorderColor.A > 0)
            {
                using (var pen = new Pen(style.BorderColor, style.BorderWidth))
                {
                    g.DrawRectangle(pen, frontRect.X, frontRect.Y, frontRect.Width, frontRect.Height);
                }
            }
        }

        private void DrawTopFace(Graphics g, RectangleF frontRect, float offsetX, float offsetY,
            Color color, Color borderColor, int borderWidth)
        {
            var points = new PointF[]
            {
                new PointF(frontRect.Left, frontRect.Top),
                new PointF(frontRect.Left + offsetX, frontRect.Top + offsetY),
                new PointF(frontRect.Right + offsetX, frontRect.Top + offsetY),
                new PointF(frontRect.Right, frontRect.Top)
            };

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }

            if (borderWidth > 0 && borderColor.A > 0)
            {
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    g.DrawPolygon(pen, points);
                }
            }
        }

        private void DrawBottomFace(Graphics g, RectangleF frontRect, float offsetX, float offsetY,
            Color color, Color borderColor, int borderWidth)
        {
            var points = new PointF[]
            {
                new PointF(frontRect.Left, frontRect.Bottom),
                new PointF(frontRect.Left + offsetX, frontRect.Bottom + offsetY),
                new PointF(frontRect.Right + offsetX, frontRect.Bottom + offsetY),
                new PointF(frontRect.Right, frontRect.Bottom)
            };

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }

            if (borderWidth > 0 && borderColor.A > 0)
            {
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    g.DrawPolygon(pen, points);
                }
            }
        }

        private void DrawLeftFace(Graphics g, RectangleF frontRect, float offsetX, float offsetY,
            Color color, Color borderColor, int borderWidth)
        {
            var points = new PointF[]
            {
                new PointF(frontRect.Left, frontRect.Top),
                new PointF(frontRect.Left + offsetX, frontRect.Top + offsetY),
                new PointF(frontRect.Left + offsetX, frontRect.Bottom + offsetY),
                new PointF(frontRect.Left, frontRect.Bottom)
            };

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }

            if (borderWidth > 0 && borderColor.A > 0)
            {
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    g.DrawPolygon(pen, points);
                }
            }
        }

        private void DrawRightFace(Graphics g, RectangleF frontRect, float offsetX, float offsetY,
            Color color, Color borderColor, int borderWidth)
        {
            var points = new PointF[]
            {
                new PointF(frontRect.Right, frontRect.Top),
                new PointF(frontRect.Right + offsetX, frontRect.Top + offsetY),
                new PointF(frontRect.Right + offsetX, frontRect.Bottom + offsetY),
                new PointF(frontRect.Right, frontRect.Bottom)
            };

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }

            if (borderWidth > 0 && borderColor.A > 0)
            {
                using (var pen = new Pen(borderColor, borderWidth))
                {
                    g.DrawPolygon(pen, points);
                }
            }
        }

        private void DrawCuboidShadow(Graphics g, RectangleF frontRect, float offsetX, float offsetY,
            bool showTopSide, bool showLeftSide, ShapeStyle style)
        {
            var shadowOffsetX = style.ShadowOffsetX;
            var shadowOffsetY = style.ShadowOffsetY;

            var shadowFrontRect = new RectangleF(
                frontRect.X + shadowOffsetX,
                frontRect.Y + shadowOffsetY,
                frontRect.Width,
                frontRect.Height);

            using (var brush = new SolidBrush(style.ShadowColor))
            {
                // 前面阴影
                g.FillRectangle(brush, shadowFrontRect);

                // 顶面/底面阴影
                if (showTopSide)
                {
                    var points = new PointF[]
                    {
                        new PointF(shadowFrontRect.Left, shadowFrontRect.Top),
                        new PointF(shadowFrontRect.Left + offsetX, shadowFrontRect.Top + offsetY),
                        new PointF(shadowFrontRect.Right + offsetX, shadowFrontRect.Top + offsetY),
                        new PointF(shadowFrontRect.Right, shadowFrontRect.Top)
                    };
                    g.FillPolygon(brush, points);
                }
                else
                {
                    var points = new PointF[]
                    {
                        new PointF(shadowFrontRect.Left, shadowFrontRect.Bottom),
                        new PointF(shadowFrontRect.Left + offsetX, shadowFrontRect.Bottom + offsetY),
                        new PointF(shadowFrontRect.Right + offsetX, shadowFrontRect.Bottom + offsetY),
                        new PointF(shadowFrontRect.Right, shadowFrontRect.Bottom)
                    };
                    g.FillPolygon(brush, points);
                }

                // 侧面阴影
                if (showLeftSide)
                {
                    var points = new PointF[]
                    {
                        new PointF(shadowFrontRect.Left, shadowFrontRect.Top),
                        new PointF(shadowFrontRect.Left + offsetX, shadowFrontRect.Top + offsetY),
                        new PointF(shadowFrontRect.Left + offsetX, shadowFrontRect.Bottom + offsetY),
                        new PointF(shadowFrontRect.Left, shadowFrontRect.Bottom)
                    };
                    g.FillPolygon(brush, points);
                }
                else
                {
                    var points = new PointF[]
                    {
                        new PointF(shadowFrontRect.Right, shadowFrontRect.Top),
                        new PointF(shadowFrontRect.Right + offsetX, shadowFrontRect.Top + offsetY),
                        new PointF(shadowFrontRect.Right + offsetX, shadowFrontRect.Bottom + offsetY),
                        new PointF(shadowFrontRect.Right, shadowFrontRect.Bottom)
                    };
                    g.FillPolygon(brush, points);
                }
            }
        }

        public GraphicsPath GetPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            path.AddRectangle(bounds);
            return path;
        }

        public IShapeRenderer Clone()
        {
            return (IShapeRenderer)this.MemberwiseClone();
        }

        private Color LightenColor(Color color, float amount)
        {
            return Color.FromArgb(color.A,
                Math.Min(255, color.R + (int)((255 - color.R) * amount)),
                Math.Min(255, color.G + (int)((255 - color.G) * amount)),
                Math.Min(255, color.B + (int)((255 - color.B) * amount)));
        }

        private Color DarkenColor(Color color, float amount)
        {
            return Color.FromArgb(color.A,
                (int)(color.R * (1 - amount)),
                (int)(color.G * (1 - amount)),
                (int)(color.B * (1 - amount)));
        }
    }

    #endregion

    #endregion

    #region 图形组合

    /// <summary>
    /// 组合形状项
    /// </summary>
    public class ShapeItem
    {
        public ShapeItem()
        {
            Style = ShapeStyle.CreateDefault();
        }

        public ShapeItem(IShapeRenderer renderer, Rectangle bounds) : this()
        {
            Renderer = renderer;
            Bounds = bounds;
        }

        public ShapeItem(IShapeRenderer renderer, Rectangle bounds, ShapeStyle style)
        {
            Renderer = renderer;
            Bounds = bounds;
            Style = style ?? ShapeStyle.CreateDefault();
        }

        public string Name { get; set; }
        public IShapeRenderer Renderer { get; set; }
        public Rectangle Bounds { get; set; }
        public ShapeStyle Style { get; set; }
        public Point Offset { get; set; }
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 克隆形状项
        /// </summary>
        public ShapeItem Clone()
        {
            return new ShapeItem
            {
                Name = this.Name,
                Renderer = this.Renderer?.Clone(),
                Bounds = this.Bounds,
                Style = this.Style?.Clone(),
                Offset = this.Offset,
                Visible = this.Visible
            };
        }

        /// <summary>
        /// 绘制形状项
        /// </summary>
        public void Draw(Graphics g, Point baseOffset)
        {
            if (!Visible || Renderer == null)
            {
                return;
            }

            var actualBounds = new Rectangle(
                Bounds.X + Offset.X + baseOffset.X,
                Bounds.Y + Offset.Y + baseOffset.Y,
                Bounds.Width,
                Bounds.Height);

            Renderer.Draw(g, actualBounds, Style);
        }
    }

    /// <summary>
    /// 组合形状集合
    /// </summary>
    public class CompositeShape
    {
        private List<ShapeItem> shapes = new List<ShapeItem>();
        private Point position;

        public CompositeShape()
        {
        }

        public CompositeShape(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public Point Position
        {
            get => position;
            set
            {
                position = value;
                OnPositionChanged();
            }
        }

        public List<ShapeItem> Shapes => shapes;

        /// <summary>
        /// 添加形状
        /// </summary>
        public void AddShape(ShapeItem shape)
        {
            if (shape != null)
            {
                Shapes.Add(shape);
            }
        }

        /// <summary>
        /// 移除形状
        /// </summary>
        public void RemoveShape(ShapeItem shape)
        {
            Shapes.Remove(shape);
        }

        /// <summary>
        /// 清空形状
        /// </summary>
        public void Clear()
        {
            Clear();
        }

        /// <summary>
        /// 绘制所有形状
        /// </summary>
        public void Draw(Graphics g)
        {
            foreach (var shape in shapes)
            {
                shape.Draw(g, Position);
            }
        }

        /// <summary>
        /// 获取边界矩形
        /// </summary>
        public Rectangle GetBounds()
        {
            if (Shapes.Count == 0)
            {
                return Rectangle.Empty;
            }

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (var shape in shapes)
            {
                if (!shape.Visible)
                {
                    continue;
                }

                var bounds = shape.Bounds;
                minX = Math.Min(minX, bounds.Left);
                minY = Math.Min(minY, bounds.Top);
                maxX = Math.Max(maxX, bounds.Right);
                maxY = Math.Max(maxY, bounds.Bottom);
            }

            return new Rectangle(
                minX + Position.X,
                minY + Position.Y,
                maxX - minX,
                maxY - minY);
        }

        /// <summary>
        /// 克隆组合形状
        /// </summary>
        public CompositeShape Clone()
        {
            var clone = new CompositeShape(this.Name)
            {
                Position = this.Position
            };

            foreach (var shape in shapes)
            {
                clone.AddShape(shape.Clone());
            }

            return clone;
        }

        private void OnPositionChanged()
        {
            // 位置改变时的处理
        }
    }

    #endregion

    #region 枚举和辅助类

    /// <summary>
    /// 形状类型枚举
    /// </summary>
    public enum ShapeType
    {
        Line,              // 线段
        Rectangle,         // 矩形
        RoundedRectangle,  // 圆角矩形
        Ellipse,           // 椭圆
        Circle,            // 圆形
        Triangle,          // 三角形
        Trapezoid,         // 梯形
        Polygon,           // 多边形
        Star,              // 星形
        Arrow,             // 箭头
        Cylinder,          // 圆柱体
        Sphere,            // 椭球体
        Cuboid,            // 六面体
        Custom             // 自定义
    }

    /// <summary>
    /// 线段显示模式
    /// </summary>
    public enum LineMode
    {
        Custom,         // 自定义(使用起点和终点)
        Horizontal,     // 水平线(从左到右)
        Vertical,       // 垂直线(从上到下)
        DiagonalDown,   // 对角线(左上到右下)
        DiagonalUp      // 对角线(右上到左下)
    }

    /// <summary>
    /// 箭头方向
    /// </summary>
    public enum ArrowDirection
    {
        Left,   // 向左
        Right,  // 向右
        Up,     // 向上
        Down    // 向下
    }

    /// <summary>
    /// 形状渲染器接口
    /// </summary>
    public interface IShapeRenderer
    {
        /// <summary>
        /// 形状类型
        /// </summary>
        ShapeType ShapeType { get; }

        /// <summary>
        /// 绘制形状
        /// </summary>
        void Draw(Graphics g, Rectangle bounds, ShapeStyle style);

        /// <summary>
        /// 获取形状路径
        /// </summary>
        GraphicsPath GetPath(Rectangle bounds);

        /// <summary>
        /// 克隆渲染器
        /// </summary>
        IShapeRenderer Clone();
    }

    /// <summary>
    /// 形状样式配置
    /// </summary>
    public class ShapeStyle
    {
        #region 基础样式

        public Color FillColor { get; set; } = Color.DodgerBlue;
        public Color BorderColor { get; set; } = Color.Navy;
        public int BorderWidth { get; set; } = 1;
        public float Opacity { get; set; } = 1.0f;
        public DashStyle BorderStyle { get; set; } = DashStyle.Solid;

        #endregion

        #region 渐变样式

        public bool UseGradient { get; set; } = false;
        public Color GradientStartColor { get; set; } = Color.LightBlue;
        public Color GradientEndColor { get; set; } = Color.DarkBlue;
        public LinearGradientMode GradientMode { get; set; } = LinearGradientMode.Vertical;

        #endregion

        #region 3D效果样式

        public Color TopFaceColor { get; set; } = Color.LightBlue;
        public Color FrontFaceColor { get; set; } = Color.Blue;
        public Color RightFaceColor { get; set; } = Color.DarkBlue;
        public float HorizontalViewAngle { get; set; } = 30f;
        public float VerticalViewAngle { get; set; } = 30f;
        public float DepthRatio { get; set; } = 0.3f;

        #endregion

        #region 阴影样式

        public bool ShowShadow { get; set; } = false;
        public Color ShadowColor { get; set; } = Color.FromArgb(100, Color.Black);
        public int ShadowOffsetX { get; set; } = 3;
        public int ShadowOffsetY { get; set; } = 3;
        public int ShadowBlur { get; set; } = 5;

        #endregion

        /// <summary>
        /// 克隆样式
        /// </summary>
        public ShapeStyle Clone()
        {
            return (ShapeStyle)this.MemberwiseClone();
        }

        /// <summary>
        /// 创建默认样式
        /// </summary>
        public static ShapeStyle CreateDefault()
        {
            return new ShapeStyle();
        }
    }

    /// <summary>
    /// 顶点集合
    /// </summary>
    [Editor(typeof(VertexCollectionEditor), typeof(UITypeEditor))]
    public class VertexCollection : IList<PointF>
    {
        private List<PointF> vertices = new List<PointF>();

        public event EventHandler CollectionChanged;

        #region IList<PointF> 实现

        public PointF this[int index]
        {
            get => vertices[index];
            set
            {
                vertices[index] = value;
                OnCollectionChanged();
            }
        }

        public int Count => vertices.Count;
        public bool IsReadOnly => false;

        public void Add(PointF item)
        {
            vertices.Add(item);
            OnCollectionChanged();
        }

        public void Clear()
        {
            vertices.Clear();
            OnCollectionChanged();
        }

        public bool Contains(PointF item) => vertices.Contains(item);
        public void CopyTo(PointF[] array, int arrayIndex) => vertices.CopyTo(array, arrayIndex);
        public IEnumerator<PointF> GetEnumerator() => vertices.GetEnumerator();
        public int IndexOf(PointF item) => vertices.IndexOf(item);

        public void Insert(int index, PointF item)
        {
            vertices.Insert(index, item);
            OnCollectionChanged();
        }

        public bool Remove(PointF item)
        {
            bool result = vertices.Remove(item);
            if (result)
            {
                OnCollectionChanged();
            }

            return result;
        }

        public void RemoveAt(int index)
        {
            vertices.RemoveAt(index);
            OnCollectionChanged();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public PointF[] ToArray() => vertices.ToArray();

        public VertexCollection Clone()
        {
            var clone = new VertexCollection();
            clone.vertices.AddRange(this.vertices);
            return clone;
        }

        protected virtual void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 形状文本类
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShapeText
    {
        public ShapeText()
        {
            Color = Color.Black;
            Font = new Font("微软雅黑", 9f);
        }

        public ShapeText(string text, PointF position)
        {
            Text = text;
            Position = position;
            Color = Color.Black;
            Font = new Font("微软雅黑", 9f);
        }

        public ShapeText(string text, PointF position, Color color, Font font) : this(text, position)
        {
            Color = color;
            Font = font;
        }

        [Category("内容")]
        [Description("文本位置(相对于控件的坐标)")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF Position { get; set; }

        [Category("内容")]
        [Description("文本内容")]
        public string Text { get; set; }

        [Category("外观")]
        [Description("文本颜色")]
        public Color Color { get; set; }

        [Category("外观")]
        [Description("文本字体")]
        public Font Font { get; set; }

        [Category("外观")]
        [Description("文本对齐方式")]
        [DefaultValue(StringAlignment.Near)]
        public StringAlignment Alignment { get; set; } = StringAlignment.Near;

        [Category("外观")]
        [Description("文本行对齐方式")]
        [DefaultValue(StringAlignment.Near)]
        public StringAlignment LineAlignment { get; set; } = StringAlignment.Near;

        [Category("行为")]
        [Description("是否启用抗锯齿")]
        [DefaultValue(true)]
        public bool AntiAlias { get; set; } = true;

        [Category("行为")]
        [Description("是否可见")]
        [DefaultValue(true)]
        public bool Visible { get; set; } = true;

        public void Draw(Graphics g)
        {
            if (!Visible || string.IsNullOrEmpty(Text) || Font == null)
            {
                return;
            }

            var oldTextRenderingHint = g.TextRenderingHint;

            if (AntiAlias)
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }

            using (var brush = new SolidBrush(Color))
            using (var format = new StringFormat())
            {
                format.Alignment = Alignment;
                format.LineAlignment = LineAlignment;

                g.DrawString(Text, Font, brush, Position, format);
            }

            g.TextRenderingHint = oldTextRenderingHint;
        }

        public ShapeText Clone()
        {
            return new ShapeText
            {
                Position = this.Position,
                Text = this.Text,
                Color = this.Color,
                Font = this.Font != null ? (Font)this.Font.Clone() : null,
                Alignment = this.Alignment,
                LineAlignment = this.LineAlignment,
                AntiAlias = this.AntiAlias,
                Visible = this.Visible
            };
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Text)
                ? $"{Text} at ({Position.X:F1}, {Position.Y:F1})"
                : $"ShapeText at ({Position.X:F1}, {Position.Y:F1})";
        }
    }

    /// <summary>
    /// 形状文本集合
    /// </summary>
    public class ShapeTextItemCollection : CollectionBase
    {
        public event EventHandler CollectionChanged;

        /// <summary>
        /// 索引器
        /// </summary>
        public ShapeText this[int index]
        {
            get => (ShapeText)List[index];
            set
            {
                List[index] = value;
                OnCollectionChanged();
            }
        }

        /// <summary>
        /// 添加文本项
        /// </summary>
        public int Add(ShapeText item)
        {
            int index = List.Add(item);
            OnCollectionChanged();
            return index;
        }

        /// <summary>
        /// 添加文本
        /// </summary>
        public int Add(string text, PointF position)
        {
            return Add(new ShapeText(text, position));
        }

        /// <summary>
        /// 添加文本
        /// </summary>
        public int Add(string text, PointF position, Color color, Font font)
        {
            return Add(new ShapeText(text, position, color, font));
        }

        /// <summary>
        /// 插入
        /// </summary>
        public void Insert(int index, ShapeText item)
        {
            List.Insert(index, item);
            OnCollectionChanged();
        }

        /// <summary>
        /// 移除
        /// </summary>
        public void Remove(ShapeText item)
        {
            List.Remove(item);
            OnCollectionChanged();
        }

        /// <summary>
        /// 是否包含
        /// </summary>
        public bool Contains(ShapeText item)
        {
            return List.Contains(item);
        }

        /// <summary>
        /// 索引
        /// </summary>
        public int IndexOf(ShapeText item)
        {
            return List.IndexOf(item);
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        public void CopyTo(ShapeText[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        /// 绘制所有文本
        /// </summary>
        public void DrawAll(Graphics g)
        {
            foreach (ShapeText text in List)
            {
                text?.Draw(g);
            }
        }

        /// <summary>
        /// 克隆集合
        /// </summary>
        public ShapeTextItemCollection Clone()
        {
            var clone = new ShapeTextItemCollection();
            foreach (ShapeText item in List)
            {
                clone.Add(item?.Clone());
            }
            return clone;
        }

        protected override void OnClear()
        {
            base.OnClear();
            OnCollectionChanged();
        }

        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            OnCollectionChanged();
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            OnCollectionChanged();
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            base.OnSetComplete(index, oldValue, newValue);
            OnCollectionChanged();
        }

        protected virtual void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region 设计器

    public class ShapeTextCollectionEditor : CollectionEditor
    {
        public ShapeTextCollectionEditor(Type type) : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(ShapeText);
        }

        protected override object CreateInstance(Type itemType)
        {
            return new ShapeText
            {
                Text = "新文本",
                Position = new PointF(10, 10),
                Color = Color.Black,
                Font = new Font("微软雅黑", 9f)
            };
        }

        protected override string GetDisplayText(object value)
        {
            if (value is ShapeText text)
            {
                return !string.IsNullOrEmpty(text.Text)
                    ? $"{text.Text} ({text.Position.X}, {text.Position.Y})"
                    : $"({text.Position.X}, {text.Position.Y})";
            }
            return base.GetDisplayText(value);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(ShapeText) };
        }
    }

    /// <summary>
    /// PointF 类型转换器
    /// </summary>
    public class PointFConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string str)
            {
                try
                {
                    string[] parts = str.Split(',');
                    if (parts.Length == 2)
                    {
                        float x = float.Parse(parts[0].Trim());
                        float y = float.Parse(parts[1].Trim());
                        return new PointF(x, y);
                    }
                }
                catch { }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return destinationType == typeof(string) && value is PointF point
                ? $"{point.X}, {point.Y}"
                : base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(PointF), attributes);
        }
    }

    /// <summary>
    /// 顶点集合编辑器
    /// </summary>
    public class VertexCollectionEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService editorService =
                    provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                if (editorService != null && value is VertexCollection collection)
                {
                    // 获取当前控件实例
                    FluentShape shapeControl = context.Instance as FluentShape;
                    Rectangle shapeBounds = Rectangle.Empty;
                    ShapeType shapeType = ShapeType.Triangle;

                    if (shapeControl != null)
                    {
                        shapeBounds = shapeControl.ClientRectangle;
                        shapeType = shapeControl.ShapeType;
                    }

                    // 打开编辑器
                    using (var editor = new VertexCollectionEditorForm(collection, shapeBounds, shapeType))
                    {
                        if (editorService.ShowDialog(editor) == DialogResult.OK)
                        {
                            // 只有点击确定才更新集合
                            collection.Clear();
                            foreach (var vertex in editor.EditedCollection)
                            {
                                collection.Add(vertex);
                            }

                            return collection;
                        }
                    }
                }
            }

            return value;
        }

    }

    /// <summary>
    /// 顶点集合编辑器窗体
    /// </summary>
    public class VertexCollectionEditorForm : Form
    {
        private ListBox lstVertices;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Button btnClear;
        private Button btnInitDefault;
        private Button btnOK;
        private Button btnCancel;
        private PropertyGrid propertyGrid;
        private Panel panelPreview;

        private Button btnMoveLeft;
        private Button btnMoveRight;
        private Button btnMoveUpPos;
        private Button btnMoveDownPos;
        private Button btnMoveAllLeft;
        private Button btnMoveAllRight;
        private Button btnMoveAllUp;
        private Button btnMoveAllDown;
        private NumericUpDown nudMoveStep;

        private VertexCollection workingCollection;
        private Rectangle originalBounds;
        private ShapeType shapeType;
        private bool isDragging = false;
        private int draggedVertexIndex = -1;
        private const int VertexHitRadius = 8;

        public VertexCollection EditedCollection { get; private set; }

        public VertexCollectionEditorForm(VertexCollection collection, Rectangle bounds, ShapeType type)
        {
            // 直接克隆现有集合, 不做任何修改
            workingCollection = collection.Clone();
            EditedCollection = new VertexCollection();
            originalBounds = bounds;
            shapeType = type;

            InitializeComponents();
            LoadVertices();
        }

        private void InitializeComponents()
        {
            this.Text = "顶点编辑器";
            this.Size = new Size(705, 470);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 左侧列表
            lstVertices = new ListBox
            {
                Location = new Point(10, 10),
                Size = new Size(150, 280),
                DisplayMember = "Display"
            };
            lstVertices.SelectedIndexChanged += LstVertices_SelectedIndexChanged;

            // 列表操作按钮
            int btnY = 300;
            btnAdd = new Button { Text = "添加", Location = new Point(10, btnY), Size = new Size(70, 25) };
            btnRemove = new Button { Text = "删除", Location = new Point(90, btnY), Size = new Size(70, 25) };

            btnY += 30;
            btnMoveUp = new Button { Text = "上移", Location = new Point(10, btnY), Size = new Size(70, 25) };
            btnMoveDown = new Button { Text = "下移", Location = new Point(90, btnY), Size = new Size(70, 25) };

            btnY += 30;
            btnClear = new Button { Text = "清空", Location = new Point(10, btnY), Size = new Size(70, 25) };
            btnInitDefault = new Button { Text = "初始化", Location = new Point(90, btnY), Size = new Size(70, 25) };

            btnAdd.Click += BtnAdd_Click;
            btnRemove.Click += BtnRemove_Click;
            btnMoveUp.Click += BtnMoveUp_Click;
            btnMoveDown.Click += BtnMoveDown_Click;
            btnClear.Click += BtnClear_Click;
            btnInitDefault.Click += BtnInitDefault_Click;

            // 属性网格
            propertyGrid = new PropertyGrid
            {
                Location = new Point(170, 10),
                Size = new Size(200, 280),
                PropertySort = PropertySort.NoSort
            };
            propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;

            // 预览面板
            panelPreview = new Panel
            {
                Location = new Point(380, 10),
                Size = new Size(300, 280),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            panelPreview.Paint += PanelPreview_Paint;
            panelPreview.MouseDown += PanelPreview_MouseDown;
            panelPreview.MouseMove += PanelPreview_MouseMove;
            panelPreview.MouseUp += PanelPreview_MouseUp;

            // 移动控制面板
            var grpMove = new GroupBox
            {
                Text = "移动顶点",
                Location = new Point(btnRemove.Right + 10, btnRemove.Top),
                Size = new Size(570, 84)
            };

            var lblStep = new Label
            {
                Text = "步长:",
                Location = new Point(10, 25),
                AutoSize = true
            };

            nudMoveStep = new NumericUpDown
            {
                Location = new Point(10, 52),
                Font = new Font("微软雅黑", 9.5f),
                TextAlign = HorizontalAlignment.Center,
                Size = new Size(60, 25),
                Minimum = 1,
                Maximum = 50,
                Value = 5
            };

            var lblSingle = new Label
            {
                Text = "选中顶点:",
                Location = new Point(nudMoveStep.Right + 10, 25),
                AutoSize = true
            };

            btnMoveLeft = new Button { Text = "←", Location = new Point(nudMoveStep.Right + 10, 50), Size = new Size(40, 30) };
            btnMoveUpPos = new Button { Text = "↑", Location = new Point(nudMoveStep.Right + 55, 50), Size = new Size(40, 30) };
            btnMoveDownPos = new Button { Text = "↓", Location = new Point(nudMoveStep.Right + 100, 50), Size = new Size(40, 30) };
            btnMoveRight = new Button { Text = "→", Location = new Point(nudMoveStep.Right + 145, 50), Size = new Size(40, 30) };

            var lblAll = new Label
            {
                Text = "所有顶点:",
                Location = new Point(btnMoveRight.Right + 10, 25),
                AutoSize = true
            };

            btnMoveAllLeft = new Button { Text = "全部←", Location = new Point(btnMoveRight.Right + 10, 50), Size = new Size(55, 30) };
            btnMoveAllUp = new Button { Text = "全部↑", Location = new Point(btnMoveRight.Right + 70, 50), Size = new Size(55, 30) };
            btnMoveAllDown = new Button { Text = "全部↓", Location = new Point(btnMoveRight.Right + 130, 50), Size = new Size(55, 30) };
            btnMoveAllRight = new Button { Text = "全部→", Location = new Point(btnMoveRight.Right + 190, 50), Size = new Size(55, 30) };

            btnMoveLeft.Click += (s, e) => MoveVertex(-1, 0);
            btnMoveRight.Click += (s, e) => MoveVertex(1, 0);
            btnMoveUpPos.Click += (s, e) => MoveVertex(0, -1);
            btnMoveDownPos.Click += (s, e) => MoveVertex(0, 1);

            btnMoveAllLeft.Click += (s, e) => MoveAllVertices(-1, 0);
            btnMoveAllRight.Click += (s, e) => MoveAllVertices(1, 0);
            btnMoveAllUp.Click += (s, e) => MoveAllVertices(0, -1);
            btnMoveAllDown.Click += (s, e) => MoveAllVertices(0, 1);

            grpMove.Controls.AddRange(new Control[]
            {
                lblStep, nudMoveStep, lblSingle,
                btnMoveLeft, btnMoveUpPos, btnMoveDownPos, btnMoveRight,
                lblAll, btnMoveAllLeft, btnMoveAllUp, btnMoveAllDown, btnMoveAllRight
            });

            // 确定/取消按钮
            btnOK = new Button { Text = "确定", Location = new Point(470, 390), Size = new Size(100, 30) };
            btnCancel = new Button { Text = "取消", Location = new Point(580, 390), Size = new Size(100, 30) };

            btnOK.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnOK.Click += BtnOK_Click;

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // 添加控件
            this.Controls.AddRange(new Control[]
            {
                lstVertices, btnAdd, btnRemove, btnMoveUp, btnMoveDown, btnClear, btnInitDefault,
                propertyGrid, panelPreview, grpMove, btnOK, btnCancel
            });
        }

        private void LoadVertices()
        {
            int previousSelectedIndex = lstVertices.SelectedIndex;

            lstVertices.Items.Clear();
            for (int i = 0; i < workingCollection.Count; i++)
            {
                lstVertices.Items.Add(new VertexItem(i, workingCollection[i]));
            }

            if (lstVertices.Items.Count > 0)
            {
                if (previousSelectedIndex >= 0 && previousSelectedIndex < lstVertices.Items.Count)
                {
                    lstVertices.SelectedIndex = previousSelectedIndex;
                }
                else if (lstVertices.SelectedIndex < 0)
                {
                    lstVertices.SelectedIndex = 0;
                }
            }
            else
            {
                propertyGrid.SelectedObject = null;
            }

            panelPreview.Invalidate();
        }

        private void BtnInitDefault_Click(object sender, EventArgs e)
        {
            if (workingCollection.Count > 0)
            {
                var result = MessageBox.Show(
                    "当前已有顶点数据, 是否要清空并初始化为默认顶点?",
                    "确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;
            }

            workingCollection.Clear();
            InitializeDefaultVertices();
            LoadVertices();
        }

        private void InitializeDefaultVertices()
        {
            var bounds = originalBounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                bounds = new Rectangle(0, 0, panelPreview.Width, panelPreview.Height);
            }

            // 添加边距
            int margin = 20;
            var drawBounds = new Rectangle(
                margin,
                margin,
                bounds.Width - margin * 2,
                bounds.Height - margin * 2);

            // 确保drawBounds有效
            if (drawBounds.Width <= 0 || drawBounds.Height <= 0)
            {
                drawBounds = new Rectangle(20, 20,
                    Math.Max(100, panelPreview.Width - 40),
                    Math.Max(100, panelPreview.Height - 40));
            }

            switch (shapeType)
            {
                case ShapeType.Triangle:
                    workingCollection.Add(new PointF(drawBounds.X + drawBounds.Width / 2f, drawBounds.Y));
                    workingCollection.Add(new PointF(drawBounds.Right, drawBounds.Bottom));
                    workingCollection.Add(new PointF(drawBounds.Left, drawBounds.Bottom));
                    break;

                case ShapeType.Trapezoid:
                    float topWidth = drawBounds.Width * 0.6f;
                    float offset = (drawBounds.Width - topWidth) / 2f;
                    workingCollection.Add(new PointF(drawBounds.X + offset, drawBounds.Y));
                    workingCollection.Add(new PointF(drawBounds.X + offset + topWidth, drawBounds.Y));
                    workingCollection.Add(new PointF(drawBounds.Right, drawBounds.Bottom));
                    workingCollection.Add(new PointF(drawBounds.Left, drawBounds.Bottom));
                    break;

                case ShapeType.Polygon:
                    int sides = 6;
                    float centerX = drawBounds.X + drawBounds.Width / 2f;
                    float centerY = drawBounds.Y + drawBounds.Height / 2f;
                    float radiusX = drawBounds.Width / 2f;
                    float radiusY = drawBounds.Height / 2f;
                    float startAngle = -90f;

                    for (int i = 0; i < sides; i++)
                    {
                        var angle = startAngle + (360f / sides * i);
                        var radians = angle * Math.PI / 180;
                        workingCollection.Add(new PointF(
                            centerX + (float)(radiusX * Math.Cos(radians)),
                            centerY + (float)(radiusY * Math.Sin(radians))));
                    }
                    break;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var newPoint = new PointF(panelPreview.Width / 2f, panelPreview.Height / 2f);
            workingCollection.Add(newPoint);
            LoadVertices();
            lstVertices.SelectedIndex = lstVertices.Items.Count - 1;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (lstVertices.SelectedIndex >= 0)
            {
                int index = lstVertices.SelectedIndex;
                workingCollection.RemoveAt(index);
                LoadVertices();
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空所有顶点吗?", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                workingCollection.Clear();
                LoadVertices();
            }
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            int index = lstVertices.SelectedIndex;
            if (index > 0)
            {
                var temp = workingCollection[index];
                workingCollection[index] = workingCollection[index - 1];
                workingCollection[index - 1] = temp;
                LoadVertices();
                lstVertices.SelectedIndex = index - 1;
            }
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            int index = lstVertices.SelectedIndex;
            if (index >= 0 && index < workingCollection.Count - 1)
            {
                var temp = workingCollection[index];
                workingCollection[index] = workingCollection[index + 1];
                workingCollection[index + 1] = temp;
                LoadVertices();
                lstVertices.SelectedIndex = index + 1;
            }
        }

        private void MoveVertex(int dx, int dy)
        {
            if (lstVertices.SelectedIndex >= 0)
            {
                int index = lstVertices.SelectedIndex;
                float step = (float)nudMoveStep.Value;
                var current = workingCollection[index];

                workingCollection[index] = new PointF(
                    Math.Max(0, Math.Min(panelPreview.Width, current.X + dx * step)),
                    Math.Max(0, Math.Min(panelPreview.Height, current.Y + dy * step)));

                LoadVertices();
            }
        }

        private void MoveAllVertices(int dx, int dy)
        {
            float step = (float)nudMoveStep.Value;

            for (int i = 0; i < workingCollection.Count; i++)
            {
                var current = workingCollection[i];
                workingCollection[i] = new PointF(
                    Math.Max(0, Math.Min(panelPreview.Width, current.X + dx * step)),
                    Math.Max(0, Math.Min(panelPreview.Height, current.Y + dy * step)));
            }

            LoadVertices();
        }

        private void LstVertices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstVertices.SelectedItem is VertexItem item)
            {
                propertyGrid.SelectedObject = new VertexPropertyWrapper(
                    workingCollection,
                    item.Index,
                    () => LoadVertices());
            }

            panelPreview.Invalidate();
        }

        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            LoadVertices();
        }

        #region 预览面板拖动

        private void PanelPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < workingCollection.Count; i++)
                {
                    var vertex = workingCollection[i];
                    var distance = Math.Sqrt(
                        Math.Pow(e.X - vertex.X, 2) +
                        Math.Pow(e.Y - vertex.Y, 2));

                    if (distance <= VertexHitRadius)
                    {
                        isDragging = true;
                        draggedVertexIndex = i;
                        lstVertices.SelectedIndex = i;
                        panelPreview.Cursor = Cursors.Hand;
                        break;
                    }
                }
            }
        }

        private void PanelPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedVertexIndex >= 0)
            {
                float x = Math.Max(0, Math.Min(panelPreview.Width, e.X));
                float y = Math.Max(0, Math.Min(panelPreview.Height, e.Y));

                workingCollection[draggedVertexIndex] = new PointF(x, y);
                LoadVertices();
            }
            else
            {
                bool onVertex = false;
                for (int i = 0; i < workingCollection.Count; i++)
                {
                    var vertex = workingCollection[i];
                    var distance = Math.Sqrt(
                        Math.Pow(e.X - vertex.X, 2) +
                        Math.Pow(e.Y - vertex.Y, 2));

                    if (distance <= VertexHitRadius)
                    {
                        onVertex = true;
                        break;
                    }
                }

                panelPreview.Cursor = onVertex ? Cursors.Hand : Cursors.Default;
            }
        }

        private void PanelPreview_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            draggedVertexIndex = -1;
            panelPreview.Cursor = Cursors.Default;
        }

        #endregion

        private void PanelPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 绘制网格
            using (var pen = new Pen(Color.LightGray))
            {
                for (int x = 0; x < panelPreview.Width; x += 20)
                    g.DrawLine(pen, x, 0, x, panelPreview.Height);
                for (int y = 0; y < panelPreview.Height; y += 20)
                    g.DrawLine(pen, 0, y, panelPreview.Width, y);
            }

            if (workingCollection.Count == 0)
            {
                // 显示提示信息
                string hint = "顶点列表为空\n点击\"初始化\"按钮生成默认顶点\n或点击\"添加\"按钮手动添加";
                using (var font = new Font("微软雅黑", 10))
                using (var brush = new SolidBrush(Color.Gray))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(hint, font, brush,
                        new RectangleF(0, 0, panelPreview.Width, panelPreview.Height), sf);
                }
                return;
            }

            // 绘制顶点连线和填充
            if (workingCollection.Count >= 2)
            {
                using (var pen = new Pen(Color.Blue, 2))
                {
                    for (int i = 0; i < workingCollection.Count; i++)
                    {
                        int nextIndex = (i + 1) % workingCollection.Count;
                        g.DrawLine(pen, workingCollection[i], workingCollection[nextIndex]);
                    }
                }

                if (workingCollection.Count >= 3)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(50, Color.LightBlue)))
                    {
                        g.FillPolygon(brush, workingCollection.ToArray());
                    }
                }
            }

            // 绘制顶点
            for (int i = 0; i < workingCollection.Count; i++)
            {
                var point = workingCollection[i];
                var rect = new RectangleF(point.X - 5, point.Y - 5, 10, 10);
                bool isSelected = lstVertices.SelectedIndex == i;

                using (var brush = new SolidBrush(isSelected ? Color.Red : Color.Blue))
                {
                    g.FillEllipse(brush, rect);
                }

                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawEllipse(pen, rect);
                }

                using (var brush = new SolidBrush(Color.Black))
                using (var font = new Font("Arial", 9, FontStyle.Bold))
                {
                    g.DrawString(i.ToString(), font, brush, point.X + 8, point.Y - 12);
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            EditedCollection.Clear();
            foreach (var vertex in workingCollection)
            {
                EditedCollection.Add(vertex);
            }
        }
    }

    internal class VertexItem
    {
        public VertexItem(int index, PointF point)
        {
            Index = index;
            Point = point;
        }

        public int Index { get; }
        public PointF Point { get; }

        public override string ToString()
        {
            return $"顶点 {Index}: ({Point.X:F1}, {Point.Y:F1})";
        }
    }

    internal class VertexPropertyWrapper
    {
        private VertexCollection collection;
        private int index;
        private Action onChanged;

        public VertexPropertyWrapper(VertexCollection collection, int index, Action onChanged)
        {
            this.collection = collection;
            this.index = index;
            this.onChanged = onChanged;
        }

        [Category("位置")]
        [Description("顶点X坐标")]
        public float X
        {
            get => collection[index].X;
            set
            {
                collection[index] = new PointF(value, collection[index].Y);
                onChanged?.Invoke();
            }
        }

        [Category("位置")]
        [Description("顶点Y坐标")]
        public float Y
        {
            get => collection[index].Y;
            set
            {
                collection[index] = new PointF(collection[index].X, value);
                onChanged?.Invoke();
            }
        }

        [Category("信息")]
        [Description("顶点索引")]
        [ReadOnly(true)]
        public int Index => index;

        public override string ToString()
        {
            return $"顶点 {index}";
        }
    }

    #endregion
}
