using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FluentControls.Animation
{
    public class AnimationState
    {
        public delegate double EasingFunction(double t);

        public Timer Timer { get; set; }
        public Control Control { get; set; }


        #region 通用属性

        public int CurrentStep { get; set; }

        public int TotalSteps { get; set; }

        public EasingFunction Easing { get; set; }

        public Action OnComplete { get; set; }

        public Action<double> OnProgress { get; set; }

        #endregion

        #region 尺寸动画

        public Size StartSize { get; set; }

        public Size TargetSize { get; set; }

        public bool AnimateWidth { get; set; }

        public bool AnimateHeight { get; set; }

        #endregion

        #region 位置动画

        public Point StartLocation { get; set; }

        public Point TargetLocation { get; set; }

        public bool AnimateLocation { get; set; }

        #endregion

        #region 颜色动画

        public Color StartColor { get; set; }

        public Color TargetColor { get; set; }

        public bool AnimateColor { get; set; }

        public Action<Color> ColorSetter { get; set; }

        #endregion

        #region 透明度动画

        public double StartOpacity { get; set; }

        public double TargetOpacity { get; set; }

        public bool AnimateOpacity { get; set; }

        #endregion

        #region 旋转动画(需要自定义绘制)

        public float StartAngle { get; set; }

        public float TargetAngle { get; set; }

        public bool AnimateRotation { get; set; }

        public Action<float> AngleSetter { get; set; }

        #endregion

    }
}
