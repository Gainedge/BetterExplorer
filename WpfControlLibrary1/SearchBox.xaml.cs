using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterExplorerControls {

	/// <summary> Interaction logic for SearchBox.xaml </summary>
	public partial class SearchBox : UserControl {

		#region Events

		public event SearchEventHandler BeginSearch {
			add { AddHandler(BeginSearchEvent, value); }
			remove { RemoveHandler(BeginSearchEvent, value); }
		}

		public event SearchEventHandler RequestCriteriaChange;

		public event EventHandler RequestCancel;

		public delegate void SearchEventHandler(object sender, SearchRoutedEventArgs e);

		// An event that clients can use to be notified whenever the elements of the list change:
		public event EventHandler FiltersCleared;


		/// <summary> Invoke the Changed event; called whenever list changes: </summary>
		protected virtual void OnFiltersCleared(EventArgs e) {
			if (FiltersCleared != null)
				FiltersCleared(this, e);
		}

		/// <summary> Invoke the Changed event; called whenever list changes: </summary>
		protected virtual void OnCriteriaChangeRequested(SearchRoutedEventArgs e) {
			if (RequestCriteriaChange != null)
				RequestCriteriaChange(this, e);
		}


		#endregion Events

		#region Properties
		public string FullSearchTerms { get { return CompileTerms(); } }

		public string KindCondition { private get; set; }

		private static readonly RoutedEvent BeginSearchEvent =
			EventManager.RegisterRoutedEvent("BeginSearch", RoutingStrategy.Direct, typeof(SearchEventHandler), typeof(SearchBox));


		private string esc = "ext:";
		private bool useesc = false;

		public string ExtensionCondition {
			get { return esc; }
			set {
				esc = value;
				useesc = esc.Length > 4;
				ShowFilterMenu();
			}
		}

		public bool UseExtensionCondition {
			get { return useesc; }
			set {
				useesc = value;
				ShowFilterMenu();
			}
		}

		private string ssc = "size:";
		private bool usessc = false;

		public string SizeCondition {
			get { return ssc; }
			set {
				ssc = value;
				usessc = ssc.Length > 5;
				ShowFilterMenu();
			}
		}

		public bool UseSizeCondition {
			get { return usessc; }
			set {
				usessc = value;
				ShowFilterMenu();
			}
		}

		private string asc = "author:";
		private bool useasc = false;

		public string AuthorCondition {
			get { return asc; }
			set {
				asc = value;
				useasc = asc.Length > 7;
				ShowFilterMenu();
			}
		}

		public bool UseAuthorCondition {
			get { return useasc; }
			set {
				useasc = value;
				ShowFilterMenu();
			}
		}

		private string dsc = "date:";
		private bool usedsc = false;

		public string DateCondition {
			get { return dsc; }
			set {
				dsc = value;
				usedsc = dsc.Length > 5;
				ShowFilterMenu();
			}
		}

		public bool UseDateCondition {
			get { return usedsc; }
			set {
				usedsc = value;
				ShowFilterMenu();
			}
		}

		private string msc = "modified:";
		private bool usemsc = false;

		public string ModifiedCondition {
			get { return msc; }
			set {
				msc = value;
				usemsc = msc.Length > 9;
				ShowFilterMenu();
			}
		}

		public bool UseModifiedCondition {
			get { return usemsc; }
			set {
				usemsc = value;
				ShowFilterMenu();
			}
		}

		private string usc = "subject:";
		private bool useusc = false;

		public string SubjectCondition {
			get { return usc; }
			set {
				usc = value;
				useusc = usc.Length > 8;
				ShowFilterMenu();
			}
		}

		public bool UseSubjectCondition {
			get { return useusc; }
			set {
				useusc = value;
				ShowFilterMenu();
			}
		}



		#endregion Properties

		#region Control Events

		private void textBox1_TextChanged(object sender, TextChangedEventArgs e) {
			if (SStartEnd != null)
				SStartEnd.IsEnabled = SearchCriteriatext.Text.Length != 0 && !SearchCriteriatext.IsWatermarkShown;
		}

		private void SStartEnd_Click(object sender, RoutedEventArgs e) {
			//RaiseBeginSearchEvent();
			RaiseEvent(new SearchRoutedEventArgs(CompileTerms(), SearchBox.BeginSearchEvent));
		}

		private void SFilters_DropDownOpened(object sender, EventArgs e) {
			SetUpFiltersMenu();
		}

		private void cfd_Click(object sender, RoutedEventArgs e) {
			ssc = "size:";
			asc = "author:";
			esc = "ext:";
			dsc = "date:";
			msc = "modified:";
			usc = "subject:";

			useasc = false;
			usedsc = false;
			useesc = false;
			usemsc = false;
			usessc = false;
			useusc = false;
			OnFiltersCleared(EventArgs.Empty);
			SetUpFiltersMenu();
			ShowFilterMenu();
		}

		private void a_Click(object sender, RoutedEventArgs e) {
			OnCriteriaChangeRequested(new SearchRoutedEventArgs((string)((Fluent.MenuItem)sender).Header, e.RoutedEvent));
		}


		protected override void OnKeyUp(KeyEventArgs e) {
			//base.OnKeyUp(e);
			e.Handled = true;
			if (e.Key == Key.Escape) {
				if (this.RequestCancel != null) {
					this.RequestCancel.Invoke(this, EventArgs.Empty);
				}
			}
			if (e.Key == Key.Enter) {
				//RaiseBeginSearchEvent();
				RaiseEvent(new SearchRoutedEventArgs(CompileTerms(), SearchBox.BeginSearchEvent));
			}
		}

		#endregion Control Events

		#region Helpers

		private string CompileTerms() {
			string full = SearchCriteriatext.Text;

			if (KindCondition != ":null:")
				full += " " + KindCondition;

			if (useesc)
				full += " " + esc;

			if (usessc)
				full += " " + ssc;

			if (useasc)
				full += " " + asc;

			if (usedsc)
				full += " " + dsc;

			if (usemsc)
				full += " " + msc;

			if (useusc)
				full += " " + usc;

			return full;
		}

		private void ShowFilterMenu() {
			//this.SFilters.Visibility = System.Windows.Visibility.Visible;
			//this.SearchCriteriatext.Margin = new Thickness(0, 0, 54, 0);

			if (!useasc && !usessc && !useesc && !useusc && !usemsc && !usedsc) {
				this.SFilters.Visibility = Visibility.Collapsed;
				this.SearchCriteriatext.Margin = new Thickness(0, 0, 24, 0);
			}
			else {
				this.SFilters.Visibility = Visibility.Visible;
				this.SearchCriteriatext.Margin = new Thickness(0, 0, 54, 0);
			}
		}

		public void SetUpFiltersMenu() {
			SFilters.Items.Clear();

			Fluent.MenuItem cfd = new Fluent.MenuItem();
			cfd.Header = "Clear All Filters";
			cfd.Click += new RoutedEventHandler(cfd_Click);
			SFilters.Items.Add(cfd);

			SFilters.Items.Add(new Separator());


			////Aaron Campf
			//useasc = true;
			//usedsc = true;
			//useesc = true;
			//usemsc = true;
			//usessc = true;
			//useusc = true;
			////Aaron Campf

			Action<string> AddMenuItem = (Header) => {
				Fluent.MenuItem a = new Fluent.MenuItem();
				a.Header = Header;
				a.Click += new RoutedEventHandler(a_Click);
				SFilters.Items.Add(a);
			};

			if (useesc) AddMenuItem(esc);
			if (usessc) AddMenuItem(ssc);
			if (useasc) AddMenuItem(asc);
			if (usedsc) AddMenuItem(dsc);
			if (usemsc) AddMenuItem(msc);
			if (useusc) AddMenuItem(usc);
		}

		#endregion Helpers


		public SearchBox() {
			InitializeComponent();
			ShowFilterMenu();

			KindCondition = ":null:";

			SFilters.Visibility = System.Windows.Visibility.Hidden;
		}

		///// <summary> This method raises the Tap event </summary>
		//private void RaiseBeginSearchEvent() {
		//	RaiseEvent(new SearchRoutedEventArgs(CompileTerms(), SearchBox.BeginSearchEvent));
		//}

	}
}

public class SearchRoutedEventArgs : RoutedEventArgs {
	public string SearchTerms { get; set; }

	//public SearchRoutedEventArgs(string terms) {
	//	SearchTerms = terms;
	//}

	//public SearchRoutedEventArgs(RoutedEvent routedevent) {
	//	SearchTerms = "";
	//	base.RoutedEvent = routedevent;
	//}

	public SearchRoutedEventArgs(string terms, RoutedEvent routedevent) {
		SearchTerms = terms;
		base.RoutedEvent = routedevent;
	}

	//public SearchRoutedEventArgs(string terms, RoutedEvent routedevent, string source) {
	//	SearchTerms = terms;
	//	base.RoutedEvent = routedevent;
	//	base.Source = source;
	//}

}