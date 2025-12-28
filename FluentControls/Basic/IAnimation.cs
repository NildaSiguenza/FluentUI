namespace FluentControls
{

    public delegate double EasingFunction(double progress);

    /// <summary>
    /// 动画系统接口
    /// </summary>
    public interface IAnimation
    {
        int FastDuration { get; }      // 150ms

        int NormalDuration { get; }    // 300ms

        int SlowDuration { get; }      // 500ms

        EasingFunction DefaultEasing { get; }

        EasingFunction AccelerateEasing { get; }

        EasingFunction DecelerateEasing { get; }
    }


}