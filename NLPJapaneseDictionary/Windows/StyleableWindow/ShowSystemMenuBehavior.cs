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
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Input;

namespace NLPJapaneseDictionary.StyleableWindow
{
    public static class ShowSystemMenuBehavior
    {        
        #region TargetWindow

        public static Window GetTargetWindow(DependencyObject obj)
        {
            return (Window)obj.GetValue(TargetWindow);
        }

        public static void SetTargetWindow(DependencyObject obj, Window window)
        {
            obj.SetValue(TargetWindow, window);
        }

        public static readonly DependencyProperty TargetWindow = DependencyProperty.RegisterAttached("TargetWindow", typeof(Window), typeof(ShowSystemMenuBehavior));        

        #endregion

        #region LeftButtonShowAt

        public static UIElement GetLeftButtonShowAt(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(LeftButtonShowAt);
        }

        public static void SetLeftButtonShowAt(DependencyObject obj, UIElement element)
        {
            obj.SetValue(LeftButtonShowAt, element);
        }

        public static readonly DependencyProperty LeftButtonShowAt = DependencyProperty.RegisterAttached("LeftButtonShowAt",
            typeof(UIElement), typeof(ShowSystemMenuBehavior),
            new UIPropertyMetadata(null, LeftButtonShowAtChanged));

        #endregion

        #region RightButtonShow

        public static bool GetRightButtonShow(DependencyObject obj)
        {
            return (bool)obj.GetValue(RightButtonShow);
        }

        public static void SetRightButtonShow(DependencyObject obj, bool arg)
        {
            obj.SetValue(RightButtonShow, arg);
        }

        public static readonly DependencyProperty RightButtonShow = DependencyProperty.RegisterAttached("RightButtonShow",
            typeof(bool), typeof(ShowSystemMenuBehavior),
            new UIPropertyMetadata(false, RightButtonShowChanged));

        #endregion

        #region LeftButtonShowAt
        
        static void LeftButtonShowAtChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as UIElement;

            if (element != null)
            {
                element.MouseLeftButtonDown += LeftButtonDownShow;
            }
        }

        static bool leftButtonToggle = true;

        static void LeftButtonDownShow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (leftButtonToggle)
            {
                var element = ((UIElement)sender).GetValue(LeftButtonShowAt);

                var showMenuAt = ((Visual)element).PointToScreen(new Point(0, 0));

                var targetWindow = ((UIElement)sender).GetValue(TargetWindow) as Window;

                SystemMenuManager.ShowMenu(targetWindow, showMenuAt);

                leftButtonToggle = !leftButtonToggle;
            }
            else
            {
                leftButtonToggle = !leftButtonToggle;
            }
        }

        #endregion

        #region RightButtonShow handlers

        private static void RightButtonShowChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as UIElement;

            if (element != null)
            {
                element.MouseRightButtonDown += RightButtonDownShow;
            }
        }

        static void RightButtonDownShow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            var targetWindow = element.GetValue(TargetWindow) as Window;

            var showMenuAt = targetWindow.PointToScreen(Mouse.GetPosition((targetWindow)));

            SystemMenuManager.ShowMenu(targetWindow, showMenuAt);
        }

        #endregion       
    }
}
