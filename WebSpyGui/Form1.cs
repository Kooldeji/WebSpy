using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSpy;


namespace WebSpyGui
{
    public delegate void QueryDelegate(string arg);
    public partial class Form1 : Form
    {
        private Corpus _corpus;
        private Querier _querier;
        private Func<string, CancellationTokenSource, HashSet<string>> _suggestMethod;
        private CancellationTokenSource _suggestCts;
        private QueryDelegate _queryMethod;
        private CancellationTokenSource _searchCts;

        private int _currentPage = 0;
        private Panel _resultsGroup;
        private Panel _buttonsGroup;
        private Panel _resultPageGroup;
        private Label _timeLabel;

        private const int MAX_PAGE_RESULTS = 4;

        public Form1()
        {
            InitializeComponent();

            _corpus = Corpus.init();
            var crawler = new Crawler(_corpus);
            _querier = new Querier(_corpus);

            _suggestMethod = new Func<string, CancellationTokenSource, HashSet<string>>(_querier.AutoCompleteWord);
            _queryMethod = new QueryDelegate(_querier.Query);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void onTextChanged(object sender, EventArgs e)
        {
            showDialogBox();
            if (_suggestCts != null)
            {
                _suggestCts.Cancel();
            }
            _suggestCts = new CancellationTokenSource();
            _suggestMethod.BeginInvoke(searchTb.Text, _suggestCts, new AsyncCallback(new AsyncPoppulateSuggestion(_suggestMethod, searchDropDown, _suggestCts).PopulateSuggestion), null);
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
                searchTb.Text = searchTb.Text + searchDropDown.Text + " ";
            }

        }

        private void ShowResultsPage()
        {
            this.BackgroundImage = new Bitmap(@"Images\Web_Spy_Result-01.jpg");
            searchPanel.Location = new Point(115, 0);

            _resultPageGroup = new Panel();
            _resultPageGroup.Location = new Point(120, searchPanel.Height);
            _resultPageGroup.Width = this.Width - 240;
            _resultPageGroup.Height = this.Height - 120;
            _resultPageGroup.BackColor = Color.Transparent;
            _resultPageGroup.Click += new EventHandler((source, e) =>
            {
                this.ActiveControl = _resultPageGroup;
            });

            _timeLabel = new Label();
            _timeLabel.AutoSize = true;
            _timeLabel.Font = new Font("Roboto", 10);
            _timeLabel.Location = new Point(0, 0);

            _resultsGroup = new Panel();
            _resultsGroup.BackColor = Color.Transparent;
            _resultsGroup.Location = new Point(0, _timeLabel.Height + 10);
            _resultsGroup.Width = _resultPageGroup.Width;
            _resultsGroup.Height = _resultPageGroup.Height - 60;
            _resultsGroup.Click += new EventHandler((source, e) =>
            {
                this.ActiveControl = _resultPageGroup;
            });


            _buttonsGroup = new Panel();
            _buttonsGroup.Location = new Point(0, _resultPageGroup.Height - 23);

            _buttonsGroup.BackColor = Color.Transparent;
            _buttonsGroup.Width = _resultPageGroup.Width;

            _resultPageGroup.Controls.Add(_timeLabel);
            _resultPageGroup.Controls.Add(_resultsGroup);
            _resultPageGroup.Controls.Add(_buttonsGroup);


            this.Controls.Add(_resultPageGroup);
            _currentPage = 1;




        }

        private void _resultPageGroup_Click1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _resultPageGroup_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            if (_currentPage == 0)
            {
                ShowResultsPage();
            }
            if (_searchCts != null)
            {
                Console.WriteLine("cancel");
                _searchCts.Cancel();
            }
            _searchCts = new CancellationTokenSource();
            _queryMethod.BeginInvoke(searchTb.Text, new AsyncCallback(new AsyncPoppulateSearch(_queryMethod, this, _searchCts).PopulateSearch), null);

        }


        private void showDialogBox()
        {
            if ((this.ActiveControl == searchTb) || (this.ActiveControl == searchDropDown))
            {
                searchDropDown.Show();
                searchPanel.Height = 275;
            }
        }

        private void hideDialogBox()
        {
            if ((this.ActiveControl != searchTb) && (this.ActiveControl != searchDropDown))
            {
                searchDropDown.Hide();
                searchPanel.Height = 60;
            }
        }
        
        private void searchTb_Leave(object sender, EventArgs e)
        {
            if (this.ActiveControl != searchTb || this.ActiveControl != searchDropDown)
            hideDialogBox();
        }
        private void searchPanel_Paint(object sender, PaintEventArgs e)
        {

        }
        public class AsyncPoppulateSuggestion
        {
            private CancellationTokenSource _cts;
            private Func<string, CancellationTokenSource, HashSet<string>> _suggestMethod;
            private ListBox _searchDropDown;

            public AsyncPoppulateSuggestion(Func<string, CancellationTokenSource, HashSet<string>> suggestMethod, ListBox searchDropDown, CancellationTokenSource cts)
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

        public class AsyncPoppulateSearch
        {
            private CancellationTokenSource _cts;
            private Form1 _form;
            private QueryDelegate _queryMethod;

            public AsyncPoppulateSearch(QueryDelegate queryMethod, Form1 form, CancellationTokenSource cts)
            {
                _cts = cts;
                _form = form;
                _queryMethod = queryMethod;
            }
            public void PopulateSearch(IAsyncResult ar)
            {

                if (_cts.IsCancellationRequested) return;
                _queryMethod.EndInvoke(ar);
                if (_form._timeLabel.InvokeRequired)
                {
                    Action<string> setLabel = new Action<string>(t => _form._timeLabel.Text = _form._querier.Results.Count + " Results (Approximately " + _form._querier.duration + "ms) ");
                    _form._timeLabel.Invoke(setLabel, "");

                }
                else
                {
                    _form._timeLabel.Text = _form._querier.Results.Count + " Results (Approximately " + _form._querier.duration + "ms) ";
                }
                GeneratePages();
            }



            private void GeneratePages()
            {
                int count = _form._querier.Results.Count;

                int noPages = (count - 1) / MAX_PAGE_RESULTS + 1;
                Button[] btns = new Button[noPages];
                Console.WriteLine("no" + noPages);
                for (int i = 0; i < noPages; i++)
                {
                    int c = i;
                    Button but = new Button();
                    but.Location = new Point(50 * i, 0);
                    but.Text = (i + 1).ToString();
                    but.Click += new EventHandler((source, e) =>
                    {
                        generateResult((c + 1) * MAX_PAGE_RESULTS);
                    });
                    btns[i] = but;
                    but.Width = 50;
                }
                if (_form._buttonsGroup.InvokeRequired)
                {
                    Action<string> addbuttons = new Action<string>(t => { _form._buttonsGroup.Controls.Clear(); _form._buttonsGroup.Controls.AddRange(btns); });
                    _form._buttonsGroup.Invoke(addbuttons, "");

                }
                else
                {
                    _form._buttonsGroup.Controls.AddRange(btns);
                }
                generateResult(MAX_PAGE_RESULTS);

            }

            private void generateResult(int index)
            {
                var bound = Math.Min(index, _form._querier.Results.Count);
                Panel[] panels = new Panel[bound];
                Console.WriteLine(index + " " + bound + " " + (index - MAX_PAGE_RESULTS));
                int pos = 0;
                for (int i = (index - MAX_PAGE_RESULTS); i < bound; i++, pos++)
                {
                    var result = _form._querier.Results.ElementAt(i);
                    Panel panel = new Panel();
                    panel.Location = new Point(0, 150 * pos);
                    panel.Width = _form._resultsGroup.Width;
                    panel.AutoSize = true;
                    panel.BackColor = Color.LightGray;


                    Label titleLabel = new Label();
                    titleLabel.Text = result.Title.ToString().ToUpper();
                    titleLabel.Font = new Font("Roboto", 16);
                    titleLabel.Location = new Point(70, 0);
                    titleLabel.Width = _form._resultsGroup.Width;
                    titleLabel.AutoEllipsis = true;
                    titleLabel.ForeColor = Color.Blue;

                    titleLabel.Click += new EventHandler((source, e) =>
                    {
                        
                        panel.BackColor = Color.LightBlue;
                        Process.Start(result.FullPath);
                        panel.BackColor = Color.LightGray;
                    });

                    panel.Controls.Add(titleLabel);


                    Label extentionLabel = new Label();
                    extentionLabel.Text = "[" + result.Extension.ToString().ToUpper() + "]";
                    extentionLabel.Font = new Font("Roboto", 13);
                    extentionLabel.Location = new Point(5, 0);
                    extentionLabel.Width = _form._resultsGroup.Width;
                    extentionLabel.ForeColor = Color.Blue;
                    panel.Controls.Add(extentionLabel);

                    Label pathLabel = new Label();
                    pathLabel.Text = result.FullPath.ToString();
                    pathLabel.Font = new Font("Roboto", 10);
                    pathLabel.Location = new Point(5, 23);
                    pathLabel.Width = _form._resultsGroup.Width;
                    pathLabel.ForeColor = Color.Green;
                    panel.Controls.Add(pathLabel);



                    //Label snippetLabel = new Label();
                    //snippetLabel.Text = result.SubText.ToString();
                    //snippetLabel.Font = new Font("Roboto", 12);
                    //snippetLabel.Location = new Point(5, 46);
                    //snippetLabel.Width = _form._resultsGroup.Width;
                    //snippetLabel.AutoEllipsis = true;
                    //panel.Controls.Add(snippetLabel);

                    //Label authorLabel = new Label();
                    //authorLabel.Text = result.Author.ToString();
                    //authorLabel.Font = new Font("Roboto", 16);
                    //authorLabel.Location = new Point(5, 69);
                    //authorLabel.Width = _form._resultsGroup.Width;
                    //panel.Controls.Add(authorLabel);

                    Label dateCreatedLabel = new Label();
                    dateCreatedLabel.Text = "Date Created: " + result.LastModified.ToString();
                    dateCreatedLabel.Font = new Font("Roboto", 8);
                    dateCreatedLabel.Location = new Point(5, 92);
                    dateCreatedLabel.Width = _form._resultsGroup.Width;
                    panel.Controls.Add(dateCreatedLabel);

                    Label lastModifiedLabel = new Label();
                    lastModifiedLabel.Text ="Date Last Modified: " + result.LastModified.ToString();
                    lastModifiedLabel.Font = new Font("Roboto", 8);
                    lastModifiedLabel.Location = new Point(5, 115);
                    lastModifiedLabel.Width = _form._resultsGroup.Width;
                    panel.Controls.Add(lastModifiedLabel);

                    panels[i] = panel;
                }
                if (_form._resultsGroup.InvokeRequired)
                {
                    Action<string> addbuttons = new Action<string>(t => { _form._resultsGroup.Controls.Clear(); _form._resultsGroup.Controls.AddRange(panels); });
                    _form._resultsGroup.Invoke(addbuttons, "");

                }
                else
                {
                    _form._resultsGroup.Controls.Clear();
                    _form._resultsGroup.Controls.AddRange(panels);
                }




            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            this.ActiveControl = searchButton;
        }

        private void searchTb_MouseLeave(object sender, EventArgs e)
        {
            searchDropDown.Hide();
            searchPanel.Height = 60;
        }
    }
}
