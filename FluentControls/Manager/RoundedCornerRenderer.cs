using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls
{
    public class RoundedCornerRenderer
    {
        #region 图形路径创建

        /// <summary>
        /// 创建圆角矩形路径
        /// </summary>
        public static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            return CreateRoundedRectPath(rect, CornerRadii.All(radius));
        }

        /// <summary>
        /// 创建圆角矩形路径
        /// (基于配置)
        /// </summary>
        public static GraphicsPath CreateRoundedRectPath(Rectangle rect, CornerRadii radii)
        {
            var path = new GraphicsPath();

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return path;
            }

            // 限制圆角大小
            radii = radii.Clamp(rect.Width, rect.Height);

            if (!radii.HasRadius)
            {
                path.AddRectangle(rect);
                return path;
            }

            int x = rect.X;
            int y = rect.Y;
            int w = rect.Width;
            int h = rect.Height;

            // 左上角
            if (radii.TopLeft > 0)
            {
                int d = radii.TopLeft * 2;
                path.AddArc(x, y, d, d, 180, 90);
            }
            else
            {
                path.AddLine(x, y, x, y);
            }

            // 上边
            path.AddLine(x + radii.TopLeft, y, x + w - radii.TopRight, y);

            // 右上角
            if (radii.TopRight > 0)
            {
                int d = radii.TopRight * 2;
                path.AddArc(x + w - d, y, d, d, 270, 90);
            }
            else
            {
                path.AddLine(x + w, y, x + w, y);
            }

            // 右边
            path.AddLine(x + w, y + radii.TopRight, x + w, y + h - radii.BottomRight);

            // 右下角
            if (radii.BottomRight > 0)
            {
                int d = radii.BottomRight * 2;
                path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            }
            else
            {
                path.AddLine(x + w, y + h, x + w, y + h);
            }

            // 下边
            path.AddLine(x + w - radii.BottomRight, y + h, x + radii.BottomLeft, y + h);

            // 左下角
            if (radii.BottomLeft > 0)
            {
                int d = radii.BottomLeft * 2;
                path.AddArc(x, y + h - d, d, d, 90, 90);
            }
            else
            {
                path.AddLine(x, y + h, x, y + h);
            }

            // 左边
            path.AddLine(x, y + h - radii.BottomLeft, x, y + radii.TopLeft);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// 创建顶部圆角路径
        /// </summary>
        public static GraphicsPath CreateRoundedTopPath(Rectangle rect, int radius)
        {
            return CreateRoundedRectPath(rect, CornerRadii.Top(radius));
        }

        /// <summary>
        /// 创建底部圆角路径
        /// </summary>
        public static GraphicsPath CreateRoundedBottomPath(Rectangle rect, int radius)
        {
            return CreateRoundedRectPath(rect, CornerRadii.Bottom(radius));
        }

        /// <summary>
        /// 创建左侧圆角路径
        /// </summary>
        public static GraphicsPath CreateRoundedLeftPath(Rectangle rect, int radius)
        {
            return CreateRoundedRectPath(rect, CornerRadii.Left(radius));
        }

        /// <summary>
        /// 创建右侧圆角路径
        /// </summary>
        public static GraphicsPath CreateRoundedRightPath(Rectangle rect, int radius)
        {
            return CreateRoundedRectPath(rect, CornerRadii.Right(radius));
        }

        #endregion

        #region 背景绘制

        /// <summary>
        /// 绘制圆角背景
        /// </summary>
        public static void FillRoundedRectangle(Graphics g, Rectangle rect, int radius, Color color)
        {
            FillRoundedRectangle(g, rect, CornerRadii.All(radius), color);
        }

        /// <summary>
        /// 绘制圆角背景
        /// </summary>
        public static void FillRoundedRectangle(Graphics g, Rectangle rect, CornerRadii radii, Color color)
        {
            if (rect.Width <= 0 || rect.Height <= 0 || color.A == 0)
            {
                return;
            }

            using (var brush = new SolidBrush(color))
            {
                FillRoundedRectangle(g, rect, radii, brush);
            }
        }

        /// <summary>
        /// 绘制圆角背景
        /// </summary>
        public static void FillRoundedRectangle(Graphics g, Rectangle rect, int radius, Brush brush)
        {
            FillRoundedRectangle(g, rect, CornerRadii.All(radius), brush);
        }

        /// <summary>
        /// 绘制圆角背景
        /// </summary>
        public static void FillRoundedRectangle(Graphics g, Rectangle rect, CornerRadii radii, Brush brush)
        {
            if (rect.Width <= 0 || rect.Height <= 0 || brush == null)
            {
                return;
            }

            var oldMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                if (!radii.HasRadius)
                {
                    g.FillRectangle(brush, rect);
                }
                else
                {
                    using (var path = CreateRoundedRectPath(rect, radii))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
            finally
            {
                g.SmoothingMode = oldMode;
            }
        }

        /// <summary>
        /// 绘制圆角渐变背景
        /// </summary>
        public static void FillRoundedGradient(Graphics g, Rectangle rect, int radius,
            Color startColor, Color endColor, LinearGradientMode mode = LinearGradientMode.Vertical)
        {
            FillRoundedGradient(g, rect, CornerRadii.All(radius), startColor, endColor, mode);
        }

        /// <summary>
        /// 绘制圆角渐变背景
        /// </summary>
        public static void FillRoundedGradient(Graphics g, Rectangle rect, CornerRadii radii,
            Color startColor, Color endColor, LinearGradientMode mode = LinearGradientMode.Vertical)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            using (var brush = new LinearGradientBrush(rect, startColor, endColor, mode))
            {
                FillRoundedRectangle(g, rect, radii, brush);
            }
        }

        #endregion


        #region 绘制辅助

        /// <summary>
        /// 配置高质量绘制
        /// </summary>
        public static void ConfigureHighQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        }

        /// <summary>
        /// 使用高质量设置执行绘制操作
        /// </summary>
        public static void WithHighQuality(Graphics g, Action<Graphics> drawAction)
        {
            var oldSmoothing = g.SmoothingMode;
            var oldPixelOffset = g.PixelOffsetMode;
            var oldCompositing = g.CompositingQuality;
            var oldInterpolation = g.InterpolationMode;

            try
            {
                ConfigureHighQuality(g);
                drawAction(g);
            }
            finally
            {
                g.SmoothingMode = oldSmoothing;
                g.PixelOffsetMode = oldPixelOffset;
                g.CompositingQuality = oldCompositing;
                g.InterpolationMode = oldInterpolation;
            }
        }

        /// <summary>
        /// 绘制带阴影的圆角矩形
        /// </summary>
        public static void DrawRoundedShadow(Graphics g, Rectangle rect, int radius, int shadowDepth, Color shadowColor)
        {
            if (shadowDepth <= 0 || rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            var oldMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                // 绘制多层阴影实现渐变效果
                for (int i = shadowDepth; i > 0; i--)
                {
                    int alpha = (int)(shadowColor.A * (1.0 - (float)i / shadowDepth) * 0.5);
                    var layerColor = Color.FromArgb(alpha, shadowColor);
                    var shadowRect = new Rectangle(rect.X + i, rect.Y + i, rect.Width, rect.Height);

                    using (var path = CreateRoundedRectPath(shadowRect, radius))
                    using (var brush = new SolidBrush(layerColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
            finally
            {
                g.SmoothingMode = oldMode;
            }
        }

        #endregion
    }

    public struct CornerRadii
    {
        public CornerRadii(int all)
        {
            TopLeft = TopRight = BottomLeft = BottomRight = all;
        }

        public CornerRadii(int topLeft, int topRight, int bottomRight, int bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public int TopLeft { get; set; }
        public int TopRight { get; set; }
        public int BottomLeft { get; set; }
        public int BottomRight { get; set; }

        /// <summary>
        /// 仅顶部圆角
        /// </summary>
        public static CornerRadii Top(int radius) => new CornerRadii(radius, radius, 0, 0);

        /// <summary>
        /// 仅底部圆角
        /// </summary>
        public static CornerRadii Bottom(int radius) => new CornerRadii(0, 0, radius, radius);

        /// <summary>
        /// 仅左侧圆角
        /// </summary>
        public static CornerRadii Left(int radius) => new CornerRadii(radius, 0, 0, radius);

        /// <summary>
        /// 仅右侧圆角
        /// </summary>
        public static CornerRadii Right(int radius) => new CornerRadii(0, radius, radius, 0);

        /// <summary>
        /// 所有角相同
        /// </summary>
        public static CornerRadii All(int radius) => new CornerRadii(radius);

        /// <summary>
        /// 无圆角
        /// </summary>
        public static CornerRadii None => new CornerRadii(0);

        /// <summary>
        /// 是否有任何圆角
        /// </summary>
        public bool HasRadius => TopLeft > 0 || TopRight > 0 || BottomLeft > 0 || BottomRight > 0;

        /// <summary>
        /// 是否所有圆角相同
        /// </summary>
        public bool IsUniform => TopLeft == TopRight && TopRight == BottomRight && BottomRight == BottomLeft;

        /// <summary>
        /// 获取统一的圆角值
        /// </summary>
        public int UniformRadius => Math.Max(Math.Max(TopLeft, TopRight), Math.Max(BottomLeft, BottomRight));

        /// <summary>
        /// 根据矩形尺寸限制圆角大小
        /// </summary>
        public CornerRadii Clamp(int width, int height)
        {
            int maxRadius = Math.Min(width, height) / 2;
            return new CornerRadii(
                Math.Min(TopLeft, maxRadius),
                Math.Min(TopRight, maxRadius),
                Math.Min(BottomRight, maxRadius),
                Math.Min(BottomLeft, maxRadius));
        }
    }

}
