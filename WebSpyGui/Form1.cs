using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSpy;

namespace WebSpyGui
{
    public partial class Form1 : Form
    {
        Corpus _corpus;
        Querier _querier;
        Func<string, CancellationTokenSource, HashSet<string>> suggestMethod;
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
            //searchDropDown.Hide();

            _corpus = Corpus.init();
            //_corpus.Default().Wait();
            var crawler = new Crawler(_corpus);
            _querier = new Querier(_corpus);
            suggestMethod = new Func<string, CancellationTokenSource, HashSet<string>>(_querier.AutoCompleteWord);
            //searchDropDown.Columns.Add("", searchDropDown.Width-50, HorizontalAlignment.Center);
            //searchDropDown.View = View.Details;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
        
        private void onTextChanged(object sender, EventArgs e)
        {
            searchDropDown.Visible = true;
            //suggestMethod.EndInvoke()
            if (cts != null)
            {
                cts.Cancel();
            }
            cts = new CancellationTokenSource();
            suggestMethod.BeginInvoke(searchTb.Text, cts, new AsyncCallback(new AsyncPoppulate(suggestMethod, searchDropDown, cts).PopulateSuggestion), null);
        }

        public class AsyncPoppulate
        {
            private CancellationTokenSource _cts;
            private Func<string, CancellationTokenSource, HashSet<string>> _suggestMethod;
            private ListBox _searchDropDown;

            public AsyncPoppulate(Func<string, CancellationTokenSource, HashSet<string>> suggestMethod, ListBox searchDropDown, CancellationTokenSource cts)
            {
                _cts = cts;
                _searchDropDown = searchDropDown;
                _suggestMethod = suggestMethod;
            }
            public void PopulateSuggestion(IAsyncResult ar)
            {
                //Console.WriteLine(_cts.IsCancellationRequested);
                if (_cts.IsCancellationRequested) return;
                var data = _suggestMethod.EndInvoke(ar);
                if (_searchDropDown.InvokeRequired)
                {

                    Action<string> clearItem = new Action<string>(t => _searchDropDown.Items.Clear());
                    _searchDropDown.Invoke(clearItem, "");
                    foreach (var item in data)
                    {
                        Action<string> setItem = new Action<string>(t => _searchDropDown.Items.Add(t));
                        _searchDropDown.Invoke(setItem, item);
                    }
                }
                else
                {

                    _searchDropDown.Items.Clear();
                    foreach (var item in data)
                    {
                        _searchDropDown.Items.Add(item);
                    }
                }
            }
        }

        private void searchDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            var replace = searchTb.Text.Split().Last();
            if (replace.Length > 0)
            {
                searchTb.Text = searchTb.Text.Replace(replace, searchDropDown.Text + " ");
            }
            else
            {
                searchTb.Text = searchTb.Text + searchDropDown.Text+ " ";
            }
            
        }
        
    }
}
