﻿//MIT License

//Copyright(c) 2015 Jay Chase

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NLPJapaneseDictionary.StyleableWindow
{
    public static class WindowResizeBehavior
    {
        public static Window GetTopLeftResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(TopLeftResize);
        }

        public static void SetTopLeftResize(DependencyObject obj, Window window)
        {
            obj.SetValue(TopLeftResize, window);
        }

        public static readonly DependencyProperty TopLeftResize = DependencyProperty.RegisterAttached("TopLeftResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnTopLeftResizeChanged));

        private static void OnTopLeftResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragTopLeft;
            }
        }

        public static Window GetTopRightResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(TopRightResize);
        }

        public static void SetTopRightResize(DependencyObject obj, Window window)
        {
            obj.SetValue(TopRightResize, window);
        }

        public static readonly DependencyProperty TopRightResize = DependencyProperty.RegisterAttached("TopRightResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnTopRightResizeChanged));

        private static void OnTopRightResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragTopRight;
            }
        }

        public static Window GetBottomRightResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(BottomRightResize);
        }

        public static void SetBottomRightResize(DependencyObject obj, Window window)
        {
            obj.SetValue(BottomRightResize, window);
        }

        public static readonly DependencyProperty BottomRightResize = DependencyProperty.RegisterAttached("BottomRightResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnBottomRightResizeChanged));

        private static void OnBottomRightResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragBottomRight;
            }
        }

        public static Window GetBottomLeftResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(BottomLeftResize);
        }

        public static void SetBottomLeftResize(DependencyObject obj, Window window)
        {
            obj.SetValue(BottomLeftResize, window);
        }

        public static readonly DependencyProperty BottomLeftResize = DependencyProperty.RegisterAttached("BottomLeftResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnBottomLeftResizeChanged));

        private static void OnBottomLeftResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragBottomLeft;
            }
        }

        public static Window GetLeftResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(LeftResize);
        }

        public static void SetLeftResize(DependencyObject obj, Window window)
        {
            obj.SetValue(LeftResize, window);
        }

        public static readonly DependencyProperty LeftResize = DependencyProperty.RegisterAttached("LeftResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnLeftResizeChanged));

        private static void OnLeftResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragLeft;
            }
        }

        public static Window GetRightResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(RightResize);
        }

        public static void SetRightResize(DependencyObject obj, Window window)
        {
            obj.SetValue(RightResize, window);
        }

        public static readonly DependencyProperty RightResize = DependencyProperty.RegisterAttached("RightResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnRightResizeChanged));

        private static void OnRightResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragRight;
            }
        }

        public static Window GetTopResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(TopResize);
        }

        public static void SetTopResize(DependencyObject obj, Window window)
        {
            obj.SetValue(TopResize, window);
        }

        public static readonly DependencyProperty TopResize = DependencyProperty.RegisterAttached("TopResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnTopResizeChanged));

        private static void OnTopResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragTop;
            }
        }

        public static Window GetBottomResize(DependencyObject obj)
        {
            return (Window)obj.GetValue(BottomResize);
        }

        public static void SetBottomResize(DependencyObject obj, Window window)
        {
            obj.SetValue(BottomResize, window);
        }

        public static readonly DependencyProperty BottomResize = DependencyProperty.RegisterAttached("BottomResize",
            typeof(Window), typeof(WindowResizeBehavior),
            new UIPropertyMetadata(null, OnBottomResizeChanged));

        private static void OnBottomResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                thumb.DragDelta += DragBottom;
            }
        }

        private static void DragLeft(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(LeftResize) as Window;

            if (window != null)
            {
                var horizontalChange = window.SafeWidthChange(e.HorizontalChange, false);
                window.Width -= horizontalChange;
                window.Left += horizontalChange;

            }
        }

        private static void DragRight(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(RightResize) as Window;

            if (window != null)
            {
                var horizontalChange = window.SafeWidthChange(e.HorizontalChange);
                window.Width += horizontalChange;
            }
        }

        private static void DragTop(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(TopResize) as Window;

            if (window != null)
            {
                var verticalChange = window.SafeHeightChange(e.VerticalChange,false);
                window.Height -= verticalChange;
                window.Top += verticalChange;
            }
        }

        private static void DragBottom(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(BottomResize) as Window;

            if (window != null)
            {
                var verticalChange = window.SafeHeightChange(e.VerticalChange);
                window.Height += verticalChange;
            }
        }

        private static void DragTopLeft(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;

            var window = thumb.GetValue(TopLeftResize) as Window;

            if (window != null)
            {
                var verticalChange = window.SafeHeightChange(e.VerticalChange,false);
                var horizontalChange = window.SafeWidthChange(e.HorizontalChange,false);

                window.Width -= horizontalChange;
                window.Left += horizontalChange;
                window.Height -= verticalChange;
                window.Top += verticalChange;
            }
        }

        private static void DragTopRight(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(TopRightResize) as Window;

            if (window != null)
            {
                var verticalChange = window.SafeHeightChange(e.VerticalChange, false);
                var horizontalChange = window.SafeWidthChange(e.HorizontalChange);

                window.Width += horizontalChange;
                window.Height -= verticalChange;
                window.Top += verticalChange;
            }
        }

        private static void DragBottomRight(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(BottomRightResize) as Window;

            if (window != null)
            {
                var verticalChange = window.SafeHeightChange(e.VerticalChange);
                var horizontalChange = window.SafeWidthChange(e.HorizontalChange);

                window.Width += horizontalChange;
                window.Height += verticalChange;
            }
        }

        private static void DragBottomLeft(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var window = thumb.GetValue(BottomLeftResize) as Window;

            if (window != null)
            {
                var verticalChange = window.SafeHeightChange(e.VerticalChange);
                var horizontalChange = window.SafeWidthChange(e.HorizontalChange, false);

                window.Width -= horizontalChange;
                window.Left += horizontalChange;
                window.Height += verticalChange;
            }
        }

        private static double SafeWidthChange(this Window window, double change, bool positive = true)
        {
            var result = positive ? window.Width + change : window.Width - change;

            if (result <= window.MinWidth)
            {
                return 0;
            } else if(result >= window.MaxWidth)
            {
                return 0;
            } else if(result < 0)
            {
                return 0;
            }
            else
            {
                return change;
            }
        }

        private static double SafeHeightChange(this Window window, double change, bool positive = true)
        {
            var result = positive ? window.Height + change : window.Height - change;

            if (result <= window.MinHeight)
            {
                return 0;
            }
            else if (result >= window.MaxHeight)
            {
                return 0;
            }
            else if (result < 0)
            {
                return 0;
            }
            else
            {
                return change;
            }
        }
    }
}
