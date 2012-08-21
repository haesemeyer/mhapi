﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace MHApi.GUI
{
    public class WindowAwareView : UserControl
    {
        /// <summary>
        /// The parent window
        /// </summary>
        protected Window parent { get; private set; }
        
        /// <summary>
        /// Constructor to hook up the loaded event
        /// </summary>
        public WindowAwareView()
        {
            //We can't get a reference to the window directly in the constructor
            //so we obtain it in the loading event of the user control
            this.Loaded += ThisLoaded;
        }

        /// <summary>
        /// Event handler for the loading event to retrieve a handle to the parent window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThisLoaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.Assert(sender == this);
            //Walk the resource tree to obtain a handle to the parent window
            parent = Window.GetWindow(this);
            //Hook our virtual method up as an event handler for the closing event
            //so that child classes can know when their parent is closing
            if (parent != null)
                parent.Closing += WindowClosing;
            else
                System.Diagnostics.Debug.WriteLine("Parent Window null");
        }

        //For some reason can't make that abstract - WPF does require base-classes to be intantiatable
        protected virtual void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) { }
    }
}
