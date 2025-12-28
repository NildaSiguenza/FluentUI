using System.Drawing;

namespace FluentControls
{
    /// <summary>
    /// 颜色调色板接口
    /// </summary>
    public interface IColorPalette
    {
        #region 主色

        Color Primary { get; }
        Color PrimaryLight { get; }
        Color PrimaryDark { get; }

        #endregion

        #region  强调色

        Color Accent { get; }
        Color AccentLight { get; }
        Color AccentDark { get; }

        #endregion

        #region  背景色

        Color Background { get; }
        Color BackgroundSecondary { get; }
        Color Surface { get; }
        Color SurfaceHover { get; }
        Color SurfacePressed { get; }

        #endregion

        #region  文本色

        Color TextPrimary { get; }
        Color TextSecondary { get; }
        Color TextDisabled { get; }
        Color TextOnPrimary { get; }

        #endregion

        #region  边框色

        Color Border { get; }
        Color BorderLight { get; }
        Color BorderFocused { get; }

        #endregion

        #region  状态色

        Color Success { get; }
        Color Warning { get; }
        Color Error { get; }
        Color Info { get; }

        #endregion

        #region  阴影色

        Color Shadow { get; }

        #endregion

        /// <summary>
        /// 获取带透明度的颜色
        /// </summary>
        Color GetColorWithOpacity(Color baseColor, float opacity);
    }
}