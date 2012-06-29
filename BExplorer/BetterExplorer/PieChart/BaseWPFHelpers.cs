// _________________________________________________________________________________________
//  Name:       BaseWPHHelpers
//  Purpose:    Common helpers for tree walking etc
//  Author:     Andrew Whiddett
//  (c) Copyright 2006 REZN8 Productions Inc
// _________________________________________________________________________________________

#region Using
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
#endregion


namespace BaseWPFHelpers
{
    public class Helpers
    {
        /// <summary>
        /// Helper method to create a snapshot of a visual item as a bitmap image. Typically used for things like drag and drop
        /// or any time we want to do something where we don't want the hit of a video running in an animating object
        /// </summary>
        /// <param name="element">Element to take a snapshot of</param>
        /// <returns>Bitmap image of the element</returns>
        public static RenderTargetBitmap CreateImageBrushFromVisual(FrameworkElement element)
        {

            RenderTargetBitmap bitmapImage =
                new RenderTargetBitmap((int)element.ActualWidth,
                            (int)element.ActualHeight,
                            96, 96,
                            PixelFormats.Pbgra32);
            bitmapImage.Render(element);

            return bitmapImage;
        }

        /// <summary>
        /// Base Interface that describes the visual match pattern
        /// Part of the Visual tree walkers
        /// </summary>
        public interface IFinderMatchVisualHelper
        {
            /// <summary>
            /// Does this item match the input visual item
            /// </summary>
            /// <param name="item">Item to check </param>
            /// <returns>True if matched, else false</returns>
            bool DoesMatch(DependencyObject item);

            /// <summary>
            /// Property that defines if we should stop walking the tree after the first match is found
            /// </summary>
            bool StopAfterFirst
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Visual tree walker class that matches based on Type
        /// </summary>
        public class FinderMatchType : IFinderMatchVisualHelper
        {
            private Type _ty = null;
            private bool _stopafterfirst = false;

            public FinderMatchType(Type ty)
            {
                _ty = ty;
            }

            public FinderMatchType(Type ty, bool StopAfterFirst)
            {
                _ty = ty;
                _stopafterfirst = StopAfterFirst;
            }

            public bool DoesMatch(DependencyObject item)
            {
                return _ty.IsInstanceOfType(item);
            }

            public bool StopAfterFirst
            {
                get
                {
                    return _stopafterfirst;
                }
                set
                {
                    _stopafterfirst = value;
                }
            }
        }

        /// <summary>
        /// Visual tree walker function that matches on name of an element
        /// </summary>
        public class FinderMatchName : IFinderMatchVisualHelper
        {
            private String _name = "";

            public FinderMatchName(String name)
            {
                _name = name;
            }

            public bool DoesMatch(DependencyObject item)
            {
                bool bMatch = false;

                if (item is FrameworkElement)
                {
                    if ((item as FrameworkElement).Name == _name) bMatch = true;
                }

                return bMatch;
            }

            /// <summary>
            /// StopAfterFirst Property.. always true, you can't have more than one of the same name..
            /// </summary>
            public bool StopAfterFirst
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }
        }

        /// <summary>
        /// Visual tree helper that matches if the item is focused
        /// </summary>
        public class FinderMatchFocused : IFinderMatchVisualHelper
        {
            public bool DoesMatch(DependencyObject item)
            {
                bool bMatch = false;

                if (item is FrameworkElement)
                {
                    if ((item as FrameworkElement).IsFocused) bMatch = true;
                }

                return bMatch;
            }

            /// <summary>
            /// StopAfterFirst Property.. always true, you can't have more than one item in focus..
            /// </summary>
            public bool StopAfterFirst
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }
           
        }

        /// <summary>
        /// Visual tree helper that matches is the item is an itemshost. Typically used in ItemControls
        /// </summary>
        public class FinderMatchItemHost : IFinderMatchVisualHelper
        {
            public bool DoesMatch(DependencyObject item)
            {
                bool bMatch = false;

                if (item is Panel)
                {
                    if ((item as Panel).IsItemsHost) bMatch = true;
                }

                return bMatch;
            }

            /// <summary>
            /// StopAfterFirst Property.. always true, you can't have more than one item host in an item control..
            /// </summary>
            public bool StopAfterFirst
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }

        }

        /// <summary>
        /// Typically used method that walks down the visual tree from a given point to locate a given match only
        /// once. Typically used with Name/ItemHost etc type matching.
        /// 
        /// Only returns one element
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>Null if no match, else returns the first element that matches</returns>
        public static FrameworkElement SingleFindDownInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            helper.StopAfterFirst = true;

            List<FrameworkElement> lst = FindDownInTree(parent, helper);

            FrameworkElement feRet = null;

            if (lst.Count > 0) feRet = lst[0];

            return feRet;
        }

        /// <summary>
        /// All way visual tree helper that searches UP and DOWN in a tree for the matching pattern.
        /// 
        /// This is used to walk for name matches or type matches typically.
        /// 
        /// Returns only the first matching element
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>Null if no match, else returns the first element that matches</returns>
        public static FrameworkElement SingleFindInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            helper.StopAfterFirst = true;

            List<FrameworkElement> lst = FindInTree(parent, helper);

            FrameworkElement feRet = null;

            if (lst.Count > 0) feRet = lst[0];

            return feRet;
        }

        /// <summary>
        /// Walker that looks down in the visual tree for any matching elements, typically used with Type
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>List of matching FrameworkElements</returns>
        public static List<FrameworkElement> FindDownInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            List<FrameworkElement> lst = new List<FrameworkElement>();

            FindDownInTree(lst, parent, null, helper);

            return lst;
        }

        /// <summary>
        /// Walker that looks both UP and down in the visual tree for any matching elements, typically used with Type
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>List of matching FrameworkElements</returns>
        public static List<FrameworkElement> FindInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            List<FrameworkElement> lst = new List<FrameworkElement>();

            FindUpInTree(lst, parent, null, helper);

            return lst;
        }

        /// <summary>
        /// Really a helper for FindDownInTree, typically not called directly.
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="parent"></param>
        /// <param name="ignore"></param>
        /// <param name="helper"></param>
        public static void FindDownInTree(List<FrameworkElement> lst, DependencyObject parent, DependencyObject ignore, IFinderMatchVisualHelper helper)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                if (lst.Count > 0 && helper.StopAfterFirst) break;

                DependencyObject visual = VisualTreeHelper.GetChild(parent, i);

                if (visual is FrameworkElement)
                {
                    (visual as FrameworkElement).ApplyTemplate();
                }

                if (helper.DoesMatch(visual))
                {
                    lst.Add(visual as FrameworkElement);
                }

                if (lst.Count > 0 && helper.StopAfterFirst) break;

                if (visual != ignore)
                {
                    FindDownInTree(lst, visual, ignore, helper);
                }
            }
        }

        /// <summary>
        /// Really a helper to look Up in a tree, typically not called directly.
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="parent"></param>
        /// <param name="ignore"></param>
        /// <param name="helper"></param>
        public static void FindUpInTree(List<FrameworkElement> lst, Visual parent, Visual ignore, IFinderMatchVisualHelper helper)
        {
            // First thing to do is find Down in the existing node...
            //FindDownInTree(lst, parent, ignore, helper);

            if (helper.DoesMatch(parent))
            {
                lst.Add(parent as FrameworkElement);
            }


            // Ok, now check to see we are not at a stop.. i.e. got it.
            if (lst.Count > 0 && helper.StopAfterFirst)
            {
                // Hum, don't think we need to do anything here: yet.
            }
            else
            {
                // Ok, now try to get a new parent...
                FrameworkElement feCast = parent as FrameworkElement;
                if (feCast != null)
                {
                    FrameworkElement feNewParent = feCast.Parent as FrameworkElement;
                    if (feNewParent == null || feNewParent == feCast)
                    {
                        // Try to get the templated parent
                        feNewParent = feCast.TemplatedParent as FrameworkElement;
                    }

                    // Now check to see that we have a valid parent
                    if (feNewParent != null && feNewParent != feCast)
                    {
                        // Pass up
                        FindUpInTree(lst, feNewParent, feCast, helper);
                    }
                }
            }
        }

        /// <summary>
        /// Simple form call that returns all elements of a given type down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static List<FrameworkElement> FindElementsOfType(Visual parent, Type ty)
        {
            return FindDownInTree(parent, new FinderMatchType(ty));

        }

        /// <summary>
        /// Simple form call that returns the first element of a given type down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static FrameworkElement FindElementOfType(Visual parent, Type ty)
        {
            return SingleFindDownInTree(parent, new FinderMatchType(ty));
        }

        /// <summary>
        /// Simple form call that returns the first element of a given type up in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static FrameworkElement FindElementOfTypeUp(Visual parent, Type ty)
        {
            return SingleFindInTree(parent, new FinderMatchType(ty));
        }
        
        /// <summary>
        /// Helper to pause any media elements down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        public static void TurnOffMediaElements(Visual parent)
        {
            List<FrameworkElement> lst = FindElementsOfType(parent,typeof(MediaElement));

            foreach (FrameworkElement me in lst)
            {
                MediaElement meCast = me as MediaElement;

                if (meCast != null)
                {
                    if (meCast.CanPause)
                    {
                        try
                        {
                            meCast.Pause();
                        }
                        catch //(Exception e)
                        {
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Helper to resume playing any media elements down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        public static void TurnOnMediaElements(Visual parent)
        {
            List<FrameworkElement> lst = FindElementsOfType(parent, typeof(MediaElement));

            foreach (FrameworkElement me in lst)
            {
                MediaElement meCast = me as MediaElement;

                if (meCast != null)
                {
                    try
                    {
                        meCast.Play();
                    }
                    catch //(Exception e)
                    {
                    }
                }
            }
        }


        /// <summary>
        /// Helper to find the currently focused element
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static FrameworkElement FindFocusedElement(Visual parent)
        {
            return SingleFindInTree(parent, new FinderMatchFocused());
        }

        /// <summary>
        /// Helper to find a items host (e.g. in a listbox etc) down in the tree
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static FrameworkElement FindItemsHost(Visual parent)
        {
            return SingleFindDownInTree(parent, new FinderMatchItemHost());
        }

        /// <summary>
        /// Helper to find the given named element down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ElementName"></param>
        /// <returns></returns>
        public static FrameworkElement FindVisualElement(Visual parent, String ElementName)
        {
            return SingleFindDownInTree(parent, new FinderMatchName(ElementName));
        }

        /// <summary>
        /// Helper to find the given named element up in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ElementName"></param>
        /// <returns></returns>
        public static FrameworkElement FindVisualElementUp(Visual parent, String ElementName)
        {
            return SingleFindInTree(parent, new FinderMatchName(ElementName));
        }

    }

}
