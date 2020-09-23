#region Copyright
//      .NET Sample
//
//      Copyright (c) 2012 by Autodesk, Inc.
//
//      Permission to use, copy, modify, and distribute this software
//      for any purpose and without fee is hereby granted, provided
//      that the above copyright notice appears in all copies and
//      that both that copyright notice and the limited warranty and
//      restricted rights notice below appear in all supporting
//      documentation.
//
//      AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
//      AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
//      MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
//      DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
//      UNINTERRUPTED OR ERROR FREE.
//
//      Use, duplication, or disclosure by the U.S. Government is subject to
//      restrictions set forth in FAR 52.227-19 (Commercial Computer
//      Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
//      (Rights in Technical Data and Computer Software), as applicable.
//
#endregion

#region imports
using System;
using System.Windows;
using System.Diagnostics;

using UiViewModels.Actions;
using Autodesk.Max;

#endregion

namespace ADNMenuSample
{
    public class MenuItemStrings
    {
        // just convienence for globals strings. Normally strings would probably be loaded from resources
        public static string actionWindow = "3d Search";
        public static string actionCategory = "3d Search";
    }

    public class RestructureModelWindow : CuiActionCommandAdapter
    {
        public bool isopen = false;
        Window mainDialog = null;
        public override void Execute(object parameter)
        {
            IGlobal global = Autodesk.Max.GlobalInterface.Instance;
            IInterface14 ip = global.COREInterface14;
            try
            {
                if (mainDialog == null)
                {
                    mainDialog = new ApplicationView();
                }
                if (mainDialog.Visibility == Visibility.Hidden)


                    isopen = false;
                if (!isopen)
                {
                    isopen = true;
                    mainDialog.Closed += new EventHandler(SearchSimilar_Closed);
                    System.Windows.Interop.WindowInteropHelper windowHandle = new System.Windows.Interop.WindowInteropHelper(mainDialog);
                    windowHandle.Owner = ManagedServices.AppSDK.GetMaxHWND();
                    ManagedServices.AppSDK.ConfigureWindowForMax(mainDialog);
                    mainDialog.Show();
                }
                else
                {
                    isopen = false;
                    mainDialog.Hide();
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception occurred: " + e.Message);
            }
        }

        void SearchSimilar_Closed(object sender, EventArgs e)
        {
            isopen = false;
            mainDialog.Visibility = Visibility.Hidden;
        }
        public override string InternalActionText
        {
            get { return MenuItemStrings.actionWindow; }
        }

        public override string InternalCategory
        {
            get { return MenuItemStrings.actionCategory; }
        }

        public override string ActionText
        {
            get { return InternalActionText; }
        }

        public override string Category
        {
            get { return InternalCategory; }
        }
    }
}
