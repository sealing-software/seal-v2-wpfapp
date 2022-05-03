﻿#pragma checksum "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "912B8A0253F3C2B32E0C85B01CFDD81EE439A8B18B01FBC8A1195173E97990E2"
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
    /// DatabaseStatus
    /// </summary>
    public partial class DatabaseStatus : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 21 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MaterialDesignThemes.Wpf.PackIcon statusIcon;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Popup statusPopup;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid popupGrid;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle popupBox;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock statusInfoMain;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock statusInfoSub;
        
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
            System.Uri resourceLocater = new System.Uri("/SEAL_V2;component/view/usercontrolobjects/databasestatus.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
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
            
            #line 17 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseEnter += new System.Windows.Input.MouseEventHandler(this.statusInfo_MouseEnter);
            
            #line default
            #line hidden
            
            #line 17 "..\..\..\..\view\usercontrolobjects\DatabaseStatus.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseLeave += new System.Windows.Input.MouseEventHandler(this.statusInfo_MouseLeave);
            
            #line default
            #line hidden
            return;
            case 2:
            this.statusIcon = ((MaterialDesignThemes.Wpf.PackIcon)(target));
            return;
            case 3:
            this.statusPopup = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 4:
            this.popupGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 5:
            this.popupBox = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 6:
            this.statusInfoMain = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.statusInfoSub = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
