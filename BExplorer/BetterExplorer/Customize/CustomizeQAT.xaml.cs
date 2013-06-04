using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Fluent;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for CustomizeQAT.xaml
    /// </summary>
    public partial class CustomizeQAT : Window
    {
        public MainWindow MainForm;

        public bool DirectConfigMode = true;

        public CustomizeQAT()
        {
            InitializeComponent();
        }

        public void AddToQAT(IRibbonControl item)
        {
            
            //MainForm.RibbonUI.AddToQuickAccessToolBar((item as UIElement));
            Refresh();
        }

        public void RemoveFromQAT(IRibbonControl item)
        {
            //MainForm.RibbonUI.RemoveFromQuickAccessToolBar((item as UIElement));
            Refresh();
        }

        public void Refresh()
        {
            AllControls.Items.Clear();
            QATControls.Items.Clear();
            MainForm.Refresh(this);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (DirectConfigMode == true)
            {
                //int sel = AllControls.SelectedIndex;
                //AddToQAT((AllControls.SelectedItem as RibbonItemListDisplay).SourceControl);
                //if (sel != 0)
                //{
                //    AllControls.SelectedIndex = sel - 1;
                //}
                //else
                //{
                //    if (AllControls.Items.Count != 0)
                //    {
                //        AllControls.SelectedIndex = 0;
                //    }
                //    else
                //    {
                //        btnRemove.IsEnabled = true;
                //        btnAdd.IsEnabled = false;
                //    }
                //}
                int sel = AllControls.SelectedIndex;
                RibbonItemListDisplay item = AllControls.SelectedValue as RibbonItemListDisplay;
                AllControls.Items.Remove(item);
                QATControls.Items.Add(item);
                //MainForm.QatItems.Add((item.SourceControl as FrameworkElement).Name);
                //AddToList(item, true);
                CheckAgainstList();
                if (sel != 0)
                {
                    AllControls.SelectedIndex = sel - 1;
                }
                else
                {
                    if (AllControls.Items.Count != 0)
                    {
                        AllControls.SelectedIndex = 0;
                    }
                    else
                    {
                        btnRemove.IsEnabled = true;
                        btnAdd.IsEnabled = false;
                    }
                }
            }
            else
            {
                int sel = AllControls.SelectedIndex;
                RibbonItemListDisplay item = AllControls.SelectedValue as RibbonItemListDisplay;
                AllControls.Items.Remove(item);
                AddToList(item, true);
                CheckAgainstList();
                if (sel != 0)
                {
                    AllControls.SelectedIndex = sel - 1;
                }
                else
                {
                    if (AllControls.Items.Count != 0)
                    {
                        AllControls.SelectedIndex = 0;
                    }
                    else
                    {
                        btnRemove.IsEnabled = true;
                        btnAdd.IsEnabled = false;
                    }
                }
            }
        }

        public void AddToList(RibbonItemListDisplay source, bool qatlist = true)
        {
            if (qatlist == true)
            {
                IRibbonControl item = source.SourceControl;
                RibbonItemListDisplay rils = new RibbonItemListDisplay();
                if (item.Icon != null)
                {
                    rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + item.Icon.ToString(), UriKind.Relative));
                }
                rils.Header = (item.Header as string);
                rils.SourceControl = item;
                rils.ItemName = (item as FrameworkElement).Name;
                rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                if (item is Fluent.DropDownButton || item is Fluent.SplitButton)
                {
                    rils.ShowMenuArrow = true;
                }
                this.QATControls.Items.Add(rils);
            }
            else
            {
                IRibbonControl item = source.SourceControl;
                RibbonItemListDisplay rils = new RibbonItemListDisplay();
                if (item.Icon != null)
                {
                  rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + item.Icon.ToString(), UriKind.Relative));
                }
                rils.Header = (item.Header as string);
                rils.SourceControl = item;
                rils.ItemName = (item as FrameworkElement).Name;
                rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                if (item is Fluent.DropDownButton || item is Fluent.SplitButton)
                {
                    rils.ShowMenuArrow = true;
                }
                this.AllControls.Items.Add(rils);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (DirectConfigMode == true)
            {
                //int sel = QATControls.SelectedIndex;
                //RemoveFromQAT((QATControls.SelectedItem as RibbonItemListDisplay).SourceControl);
                //if (sel != 0)
                //{
                //    QATControls.SelectedIndex = sel - 1;
                //}
                //else
                //{
                //    if (QATControls.Items.Count != 0)
                //    {
                //        QATControls.SelectedIndex = 0;
                //    }
                //    else
                //    {
                //        btnRemove.IsEnabled = false;
                //        btnAdd.IsEnabled = true;
                //    }
                //}
                int sel = QATControls.SelectedIndex;
                RibbonItemListDisplay item = QATControls.SelectedValue as RibbonItemListDisplay;
                QATControls.Items.Remove(item);
                //MainForm.QatItems.Remove((item.SourceControl as FrameworkElement).Name);
                //AddToList(item, false);
                this.AllControls.Items.Clear();
                foreach (IRibbonControl thing in MainForm.GetAllButtons())
                {
                    RibbonItemListDisplay rils = new RibbonItemListDisplay();
                    if (thing.Icon != null)
                    {
                      rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + thing.Icon.ToString(), UriKind.Relative));
                    }
                    rils.Header = (thing.Header as string);
                    rils.SourceControl = thing;
                    rils.ItemName = (thing as FrameworkElement).Name;
                    rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    if (thing is Fluent.DropDownButton || thing is Fluent.SplitButton)
                    {
                        rils.ShowMenuArrow = true;
                    }
                    this.AllControls.Items.Add(rils);
                }
                CheckAgainstList();
                if (sel != 0)
                {
                    QATControls.SelectedIndex = sel - 1;
                }
                else
                {
                    if (QATControls.Items.Count != 0)
                    {
                        QATControls.SelectedIndex = 0;
                    }
                    else
                    {
                        btnRemove.IsEnabled = false;
                        btnAdd.IsEnabled = true;
                    }
                }
            }
            else
            {
                int sel = QATControls.SelectedIndex;
                RibbonItemListDisplay item = QATControls.SelectedValue as RibbonItemListDisplay;
                QATControls.Items.Remove(item);
                //AddToList(item, false);
                this.AllControls.Items.Clear();
                foreach (IRibbonControl thing in MainForm.GetAllButtons())
                {
                    RibbonItemListDisplay rils = new RibbonItemListDisplay();
                    if (thing.Icon != null)
                    {
                      rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + thing.Icon.ToString(), UriKind.Relative));
                    }
                    rils.Header = (thing.Header as string);
                    rils.SourceControl = thing;
                    rils.ItemName = (thing as FrameworkElement).Name;
                    rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    if (thing is Fluent.DropDownButton || thing is Fluent.SplitButton)
                    {
                        rils.ShowMenuArrow = true;
                    }
                    this.AllControls.Items.Add(rils);
                }
                CheckAgainstList();
                if (sel != 0)
                {
                    QATControls.SelectedIndex = sel - 1;
                }
                else
                {
                    if (QATControls.Items.Count != 0)
                    {
                        QATControls.SelectedIndex = 0;
                    }
                    else
                    {
                        btnRemove.IsEnabled = false;
                        btnAdd.IsEnabled = true;
                    }
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void MoveUp()
        {
            if (QATControls.SelectedIndex != 0)
            {
                RibbonItemListDisplay r1 = (QATControls.SelectedValue as RibbonItemListDisplay);
                RibbonItemListDisplay r2 = (QATControls.Items[QATControls.SelectedIndex - 1] as RibbonItemListDisplay);

                int sel = QATControls.SelectedIndex;

                Object oItem = QATControls.Items.GetItemAt(sel);
                QATControls.Items.RemoveAt(sel);
                QATControls.Items.Insert(sel - 1, oItem);
                QATControls.SelectedIndex = QATControls.Items.IndexOf(oItem);
                QATControls.ScrollIntoView(QATControls.SelectedItem);
                //RibbonItemListDisplay r3 = r1;
                //r1.Icon = r2.Icon;
                //r1.SourceControl = r2.SourceControl;
                //r1.ShowMenuArrow = r2.ShowMenuArrow;
                //r1.Header = r2.Header;
                //r1.ItemName = r2.ItemName;

                //r2.Icon = r3.Icon;
                //r2.Header = r3.Header;
                //r2.SourceControl = r3.SourceControl;
                //r2.ItemName = r3.ItemName;
                //r2.ShowMenuArrow = r3.ShowMenuArrow;
                //QATControls.Items.Remove(r1);
                //QATControls.Items.Remove(r2);
                //QATControls.Items[QATControls.SelectedIndex - 1] = r1;
                //QATControls.Items[QATControls.SelectedIndex] = r2;
            }
        }

        public void MoveDown()
        {
            if (QATControls.SelectedIndex != (QATControls.Items.Count - 1))
            {
                RibbonItemListDisplay r1 = (QATControls.SelectedValue as RibbonItemListDisplay);
                RibbonItemListDisplay r2 = (QATControls.Items[QATControls.SelectedIndex + 1] as RibbonItemListDisplay);

                int sel = QATControls.SelectedIndex;

                Object oItem = QATControls.Items.GetItemAt(sel);
                QATControls.Items.RemoveAt(sel);
                QATControls.Items.Insert(sel + 1, oItem);
                QATControls.SelectedIndex = QATControls.Items.IndexOf(oItem);
                QATControls.ScrollIntoView(QATControls.SelectedItem);
                //RibbonItemListDisplay r3 = r1;
                //r1.Icon = r2.Icon;
                //r1.SourceControl = r2.SourceControl;
                //r1.ShowMenuArrow = r2.ShowMenuArrow;
                //r1.Header = r2.Header;
                //r1.ItemName = r2.ItemName;

                //r2.Icon = r3.Icon;
                //r2.Header = r3.Header;
                //r2.SourceControl = r3.SourceControl;
                //r2.ItemName = r3.ItemName;
                //r2.ShowMenuArrow = r3.ShowMenuArrow;
                //QATControls.Items.Remove(r1);
                //QATControls.Items.Remove(r2);
                //QATControls.Items[QATControls.SelectedIndex + 1] = r1;
                //QATControls.Items[QATControls.SelectedIndex] = r2;
            }
        }

        public void LoadIndirectConfigMode(string file)
        {
            LoadFile(file);
            this.AllControls.Items.Clear();
            foreach (IRibbonControl item in MainForm.GetAllButtons())
            {
                RibbonItemListDisplay rils = new RibbonItemListDisplay();
                if (item.Icon != null)
                {
                  rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + item.Icon.ToString(), UriKind.Relative));
                }
                rils.Header = (item.Header as string);
                rils.SourceControl = item;
                rils.ItemName = (item as FrameworkElement).Name;
                rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                if (item is Fluent.DropDownButton || item is Fluent.SplitButton)
                {
                    rils.ShowMenuArrow = true;
                }
                this.AllControls.Items.Add(rils);
            }
            DirectConfigMode = false;
            CheckAgainstList();
        }

        public void CheckAgainstList()
        {
            List<RibbonItemListDisplay> torem = new List<RibbonItemListDisplay>();
            foreach (RibbonItemListDisplay item in this.QATControls.Items)
            {
                foreach (RibbonItemListDisplay thing in this.AllControls.Items)
                {
                    if (thing.ItemName == item.ItemName)
                    {
                        torem.Add(thing);
                    }
                }
            }

            foreach (RibbonItemListDisplay item in torem)
            {
                this.AllControls.Items.Remove(item);
            }
        }

        public void LoadFile(string file)
        {
            //this.QATControls.Items.Clear();
            //foreach (RibbonItemListDisplay item in MainForm.ImportQATConfigForEditor(file))
            //{
            //    QATControls.Items.Add(item);
            //}
        }

        public void StoreToFile(string file)
        {
            List<string> list = new List<string>();
            foreach (RibbonItemListDisplay item in this.QATControls.Items)
            {
                list.Add(item.ItemName);
            }

            //XMLio.XMLio.WriteFile(file, list);
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveUp();
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveDown();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            foreach (RibbonItemListDisplay item in this.QATControls.Items)
            {
                list.Add(item.ItemName);
            }

            MainForm.PutItemsOnQAT(list);
            MainForm.LoadInternalList();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            foreach (RibbonItemListDisplay item in this.QATControls.Items)
            {
                list.Add(item.ItemName);
            }

            MainForm.PutItemsOnQAT(list);
            MainForm.LoadInternalList();
            this.Close();
        }

    }
}
