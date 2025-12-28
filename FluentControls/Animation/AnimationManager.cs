using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Animation
{
    /// <summary>
    /// Fluent动画管理器
    /// </summary>
    public static class AnimationManager
    {
        private static Dictionary<Control, List<AnimationState>> animations = new Dictionary<Control, List<AnimationState>>();

        private const int FRAME_RATE = 60;
        private const int FRAME_INTERVAL = 1000 / FRAME_RATE;

        #region 核心动画方法

        /// <summary>
        /// 启动动画
        /// </summary>
        private static void StartAnimation(Control control, AnimationState state)
        {
            if (control == null)
            {
                return;
            }

            // 停止该控件的同类型动画
            StopSimilarAnimations(control, state);

            // 添加到动画列表
            if (!animations.ContainsKey(control))
            {
                animations[control] = new List<AnimationState>();
            }

            animations[control].Add(state);

            // 创建并启动定时器
            var timer = new Timer { Interval = FRAME_INTERVAL };
            timer.Tick += (sender, e) => AnimationTick(control, state);
            state.Timer = timer;
            state.Control = control;
            timer.Start();
        }

        /// <summary>
        /// 动画帧更新
        /// </summary>
        private static void AnimationTick(Control control, AnimationState state)
        {
            if (control.IsDisposed)
            {
                StopAnimation(control, state);
                return;
            }

            state.CurrentStep++;
            double progress = Math.Min(1.0, (double)state.CurrentStep / state.TotalSteps);
            double easedProgress = state.Easing(progress);

            // 执行进度回调
            state.OnProgress?.Invoke(easedProgress);

            // 应用动画效果
            control.SuspendLayout();

            // 尺寸动画
            if (state.AnimateWidth || state.AnimateHeight)
            {
                int newWidth = state.AnimateWidth
                    ? state.StartSize.Width + (int)((state.TargetSize.Width - state.StartSize.Width) * easedProgress)
                    : control.Width;

                int newHeight = state.AnimateHeight
                    ? state.StartSize.Height + (int)((state.TargetSize.Height - state.StartSize.Height) * easedProgress)
                    : control.Height;

                control.Size = new Size(newWidth, newHeight);
            }

            // 位置动画
            if (state.AnimateLocation)
            {
                int newX = state.StartLocation.X + (int)((state.TargetLocation.X - state.StartLocation.X) * easedProgress);
                int newY = state.StartLocation.Y + (int)((state.TargetLocation.Y - state.StartLocation.Y) * easedProgress);
                control.Location = new Point(newX, newY);
            }

            // 颜色动画
            if (state.AnimateColor && state.ColorSetter != null)
            {
                Color newColor = InterpolateColor(state.StartColor, state.TargetColor, easedProgress);
                state.ColorSetter(newColor);
            }

            // 透明度动画
            if (state.AnimateOpacity && control is Form form)
            {
                form.Opacity = state.StartOpacity + (state.TargetOpacity - state.StartOpacity) * easedProgress;
            }

            // 旋转动画
            if (state.AnimateRotation && state.AngleSetter != null)
            {
                float newAngle = state.StartAngle + (float)((state.TargetAngle - state.StartAngle) * easedProgress);
                state.AngleSetter(newAngle);
            }

            control.ResumeLayout();

            // 检查是否完成
            if (state.CurrentStep >= state.TotalSteps)
            {
                // 确保最终值准确
                if (state.AnimateWidth || state.AnimateHeight)
                {
                    control.Size = new Size(
                        state.AnimateWidth ? state.TargetSize.Width : control.Width,
                        state.AnimateHeight ? state.TargetSize.Height : control.Height);
                }

                if (state.AnimateLocation)
                {
                    control.Location = state.TargetLocation;
                }

                if (state.AnimateColor && state.ColorSetter != null)
                {
                    state.ColorSetter(state.TargetColor);
                }

                if (state.AnimateOpacity && control is Form f)
                {
                    f.Opacity = state.TargetOpacity;
                }

                if (state.AnimateRotation && state.AngleSetter != null)
                {
                    state.AngleSetter(state.TargetAngle);
                }

                StopAnimation(control, state);
                state.OnComplete?.Invoke();
            }
        }

        /// <summary>
        /// 停止指定动画
        /// </summary>
        private static void StopAnimation(Control control, AnimationState state)
        {
            state.Timer?.Stop();
            state.Timer?.Dispose();

            if (animations.ContainsKey(control))
            {
                animations[control].Remove(state);
                if (animations[control].Count == 0)
                {
                    animations.Remove(control);
                }
            }
        }

        /// <summary>
        /// 停止相似动画
        /// </summary>
        private static void StopSimilarAnimations(Control control, AnimationState newState)
        {
            if (!animations.ContainsKey(control))
            {
                return;
            }

            var toStop = new List<AnimationState>();
            foreach (var state in animations[control])
            {
                bool similar = (newState.AnimateWidth && state.AnimateWidth) ||
                              (newState.AnimateHeight && state.AnimateHeight) ||
                              (newState.AnimateLocation && state.AnimateLocation) ||
                              (newState.AnimateColor && state.AnimateColor) ||
                              (newState.AnimateOpacity && state.AnimateOpacity) ||
                              (newState.AnimateRotation && state.AnimateRotation);

                if (similar)
                {
                    toStop.Add(state);
                }
            }

            foreach (var state in toStop)
            {
                StopAnimation(control, state);
            }
        }

        #endregion

        #region 公共动画方法

        /// <summary>
        /// 尺寸动画
        /// </summary>
        public static void AnimateSize(Control control, Size targetSize, int duration = 300,
            AnimationState.EasingFunction easing = null, Action onComplete = null)
        {
            var state = new AnimationState
            {
                StartSize = control.Size,
                TargetSize = targetSize,
                AnimateWidth = true,
                AnimateHeight = true,
                CurrentStep = 0,
                TotalSteps = Math.Max(1, duration / FRAME_INTERVAL),
                Easing = easing ?? Easing.CubicOut,
                OnComplete = onComplete
            };

            StartAnimation(control, state);
        }

        /// <summary>
        /// 位置动画
        /// </summary>
        public static void AnimateLocation(Control control, Point targetLocation, int duration = 300,
            AnimationState.EasingFunction easing = null, Action onComplete = null)
        {
            var state = new AnimationState
            {
                StartLocation = control.Location,
                TargetLocation = targetLocation,
                AnimateLocation = true,
                CurrentStep = 0,
                TotalSteps = Math.Max(1, duration / FRAME_INTERVAL),
                Easing = easing ?? Easing.CubicOut,
                OnComplete = onComplete
            };

            StartAnimation(control, state);
        }

        /// <summary>
        /// 颜色动画
        /// </summary>
        public static void AnimateColor(Control control, Color targetColor, Action<Color> colorSetter,
            int duration = 300, AnimationState.EasingFunction easing = null, Action onComplete = null)
        {
            Color startColor = control.BackColor;

            var state = new AnimationState
            {
                StartColor = startColor,
                TargetColor = targetColor,
                AnimateColor = true,
                ColorSetter = colorSetter ?? (c => control.BackColor = c),
                CurrentStep = 0,
                TotalSteps = Math.Max(1, duration / FRAME_INTERVAL),
                Easing = easing ?? Easing.CubicOut,
                OnComplete = onComplete
            };

            StartAnimation(control, state);
        }

        /// <summary>
        /// 淡入淡出动画
        /// </summary>
        public static void AnimateFade(Form form, double targetOpacity, int duration = 300,
            AnimationState.EasingFunction easing = null, Action onComplete = null)
        {
            var state = new AnimationState
            {
                StartOpacity = form.Opacity,
                TargetOpacity = targetOpacity,
                AnimateOpacity = true,
                CurrentStep = 0,
                TotalSteps = Math.Max(1, duration / FRAME_INTERVAL),
                Easing = easing ?? Easing.CubicOut,
                OnComplete = onComplete
            };

            StartAnimation(form, state);
        }

        /// <summary>
        /// 滑入动画
        /// </summary>
        public static void AnimateSlideIn(Control control, SlideDirection direction,
            int distance = 100, int duration = 300, Action onComplete = null)
        {
            Point startLocation = control.Location;
            Point targetLocation = control.Location;

            switch (direction)
            {
                case SlideDirection.Left:
                    control.Left -= distance;
                    break;
                case SlideDirection.Right:
                    control.Left += distance;
                    break;
                case SlideDirection.Top:
                    control.Top -= distance;
                    break;
                case SlideDirection.Bottom:
                    control.Top += distance;
                    break;
            }

            AnimateLocation(control, targetLocation, duration, Easing.CubicOut, onComplete);
        }

        /// <summary>
        /// 震动动画
        /// </summary>
        public static void AnimateShake(Control control, int intensity = 10, int duration = 500)
        {
            var originalLocation = control.Location;
            var random = new Random();
            int steps = duration / 50;
            int currentStep = 0;

            var timer = new Timer { Interval = 50 };
            timer.Tick += (s, e) =>
            {
                currentStep++;
                if (currentStep >= steps)
                {
                    control.Location = originalLocation;
                    timer.Stop();
                    timer.Dispose();
                }
                else
                {
                    int offsetX = random.Next(-intensity, intensity);
                    int offsetY = random.Next(-intensity, intensity);
                    control.Location = new Point(
                        originalLocation.X + offsetX,
                        originalLocation.Y + offsetY);
                }
            };
            timer.Start();
        }

        /// <summary>
        /// 停止控件的所有动画
        /// </summary>
        public static void StopAllAnimations(Control control)
        {
            if (!animations.ContainsKey(control))
            {
                return;
            }

            var states = animations[control].ToArray();
            foreach (var state in states)
            {
                StopAnimation(control, state);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 颜色插值
        /// </summary>
        private static Color InterpolateColor(Color start, Color end, double progress)
        {
            int r = start.R + (int)((end.R - start.R) * progress);
            int g = start.G + (int)((end.G - start.G) * progress);
            int b = start.B + (int)((end.B - start.B) * progress);
            int a = start.A + (int)((end.A - start.A) * progress);

            return Color.FromArgb(
                Math.Max(0, Math.Min(255, a)),
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, b)));
        }

        #endregion
    }

    /// <summary>
    /// 滑动方向
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
}

