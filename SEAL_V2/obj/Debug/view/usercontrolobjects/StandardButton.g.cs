﻿#pragma checksum "..\..\..\..\view\usercontrolobjects\StandardButton.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "818F32AB2DE8AB452FDE78EB1AD3775208FAFA593648860E9E9765F2931486C9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using MaterialDesignThemes.Wpf.Transitions;
using SEAL_V2.view.usercontrolobjects;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SEAL_V2.view.usercontrolobjects {
    
    
    /// <summary>
    /// StandardButton
    /// </summary>
    public partial class StandardButton : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\..\..\view\usercontrolobjects\StandardButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid MainGrid;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\..\view\usercontrolobjects\StandardButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MaterialDesignThemes.Wpf.PackIcon buttonIcon;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\..\view\usercontrolobjects\StandardButton.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock buttonText;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SEAL_V2;component/view/usercontrolobjects/standardbutton.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\view\usercontrolobjects\StandardButton.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.MainGrid = ((System.Windows.Controls.Grid)(target));
            
            #line 9 "..\..\..\..\view\usercontrolobjects\StandardButton.xaml"
            this.MainGrid.MouseEnter += new System.Windows.Input.MouseEventHandler(this.MainGrid_MouseEnter);
            
            #line default
            #line hidden
            
            #line 9 "..\..\..\..\view\usercontrolobjects\StandardButton.xaml"
            this.MainGrid.MouseLeave += new System.Windows.Input.MouseEventHandler(this.MainGrid_MouseLeave);
            
            #line default
            #line hidden
            
            #line 9 "..\..\..\..\view\usercontrolobjects\StandardButton.xaml"
            this.MainGrid.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.MainGrid_MouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 2:
            this.buttonIcon = ((MaterialDesignThemes.Wpf.PackIcon)(target));
            return;
            case 3:
            this.buttonText = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
