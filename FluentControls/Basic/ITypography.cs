using System.Drawing;

namespace FluentControls
{
    /// <summary>
    /// 字体排版接口
    /// </summary>
    public interface ITypography
    {
        /// <summary>
        /// 展示型文字
        /// </summary>
        Font Display { get; }      

        /// <summary>
        /// 标题
        /// </summary>
        Font Headline { get; }     

        /// <summary>
        /// 子标题
        /// </summary>
        Font Title { get; }        

        /// <summary>
        /// 正文
        /// </summary>
        Font Body { get; }         

        /// <summary>
        /// 说明文字
        /// </summary>
        Font Caption { get; }      

        /// <summary>
        /// 按钮文字
        /// </summary>
        Font Button { get; }       

        string FontFamily { get; }

        float BaseSize { get; }

        Font GetFont(FontStyle style, float sizeMultiplier = 1.0f);
    }
}