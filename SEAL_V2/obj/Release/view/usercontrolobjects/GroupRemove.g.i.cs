﻿#pragma checksum "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "CDF7C2EC72173CEE670F2B4CD6E69CFD61317623799FA77C45F767563744B585"
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
    /// GroupRemove
    /// </summary>
    public partial class GroupRemove : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock Title;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock InfoText;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button RemoveGroupButton;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ReturnButton;
        
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
            System.Uri resourceLocater = new System.Uri("/SEAL_V2;component/view/usercontrolobjects/groupremove.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml"
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
            this.Title = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.InfoText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.RemoveGroupButton = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml"
            this.RemoveGroupButton.Click += new System.Windows.RoutedEventHandler(this.RemoveGroupButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ReturnButton = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\..\view\usercontrolobjects\GroupRemove.xaml"
            this.ReturnButton.Click += new System.Windows.RoutedEventHandler(this.ReturnButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

