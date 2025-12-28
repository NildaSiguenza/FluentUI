using System.Reflection.Emit;
using System.Windows.Forms;

namespace FluentControls
{
    /// <summary>
    /// 间距系统接口
    /// </summary>
    public interface ISpacing
    {
        int XXSmall { get; }  // 2px

        int XSmall { get; }   // 4px

        int Small { get; }    // 8px

        int Medium { get; }   // 12px

        int Large { get; }    // 16px

        int XLarge { get; }   // 24px

        int XXLarge { get; }  // 32px

        Padding GetPadding(SpacingSize size);

        int GetSpace(SpacingSize size);
    }

    public enum SpacingSize
    {
        XXSmall, 
        XSmall, 
        Small, 
        Medium, 
        Large, 
        XLarge, 
        XXLarge
    }
}