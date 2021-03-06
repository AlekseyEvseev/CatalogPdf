﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XMLDBLib;

namespace CatalogPdf
{
    public partial class FrmTableCatalog : Form
    {
        internal DataPresenter presenter;

        private CatalogDocuments CatalogDocs
        {
            get
            {
                _catalog = new CatalogDocuments(presenter);
                return _catalog;
            }
            set => _catalog = value;
        }
        private CatalogDocuments _catalog;
        public FrmTableCatalog()
        {
            InitializeComponent();
            Width = 1000;
            Height = 500;
            DialogResult = DialogResult.None;

            info = new Label();
            Controls.Add(info);
            info.Name = "Lb_info";
            info.Text = "";
            info.AutoSize = true;
            info.Visible = false;
            info.Top = dataGridView1.Top + dataGridView1.Height + 10;
            info.Left = 10;
            info.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            info.ForeColor = Color.Red;
        }

        private readonly Label info;

        public void SetDataGreed()
        {
            try
            {
                GetData();
                SetTable();
                dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void GetData()
        {
            //List<XMLDBLib.Document> docs = presenter.Catalog.Documents.OrderBy(a => a.Tome).ThenBy(b => b.StartPage).ToList(); 
            CatalogDocs = null;
            dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.Tome).ThenBy(b => b.StartPage).ToList();
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

            int row = e.RowIndex;
            string path = dataGridView1.Rows[row].Cells[GetNumberColumn("Путь")].Value.ToString();

            XMLDBLib.Document doc = presenter.Catalog.GetByPath(path);

            if (doc != null)
            {
                int.TryParse(dataGridView1.Rows[row].Cells[GetNumberColumn("Том")].Value.ToString(), out int tome);
                switch (dataGridView1.Columns[e.ColumnIndex].HeaderText)
                {
                    case "Том":
                        doc.Tome = tome;
                        break;
                    case "Название тома":
                        string tomeName = dataGridView1.Rows[row].Cells[GetNumberColumn("Название тома")].Value.ToString();
                        doc.TomeName = tomeName;
                        break;
                    case "Номер":
                        int.TryParse(dataGridView1.Rows[row].Cells[GetNumberColumn("Номер")].Value.ToString(), out int number);
                        doc.Number = number;
                        break;
                    case "Название"://6:
                        string name = dataGridView1.Rows[row].Cells[GetNumberColumn("Название")].Value.ToString();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            doc.Name = name;
                        }

                        break;
                    case "Дата"://8:
                        DateTime.TryParse(dataGridView1.Rows[row].Cells[GetNumberColumn("Дата")].Value.ToString(), out DateTime date);
                        doc.Date = date;
                        break;
                    case "Тип"://7:
                        string type = dataGridView1.Rows[row].Cells[GetNumberColumn("Тип")].Value.ToString();
                        if (!string.IsNullOrWhiteSpace(type))
                        {
                            doc.DocType = type;
                        }

                        break;
                    case "Начало"://3:
                        int.TryParse(dataGridView1.Rows[row].Cells[GetNumberColumn("Начало")].Value.ToString(), out int startPage);

                        if (startPage != 0)
                        {
                            int.TryParse(dataGridView1.Rows[row].Cells[GetNumberColumn("Страниц")].Value.ToString(), out int amount);
                            int endPage = startPage + amount - 1;
                            dataGridView1.Rows[row].Cells[GetNumberColumn("Конец")].Value = endPage;
                            doc.StartPage = startPage;
                            doc.EndPage = endPage;
                            if (presenter.isFreeRangePage(startPage, endPage, tome, path))
                            {
                                info.Visible = false;
                                info.Text = "";
                            }
                            else
                            {
                                info.Visible = true;
                                info.Text = "Внимание! пересечение диапазонов страниц.";
                            }
                        }

                        break;

                    default:
                        break;
                }
                presenter.Save();
                GetData();
                // dataGridView1.Update();
            }
        }

        private void SetTable()
        {
            int i = 0;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.FromArgb(242, 249, 250);
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(237, 246, 255);
            dataGridView1.RowHeadersVisible = false;

            dataGridView1.Columns[i].Visible = false;  //HeaderText = "Код"; //№0
            dataGridView1.Columns[i].Width = 0;

            dataGridView1.Columns[++i].HeaderText = "Том";  //№1
            dataGridView1.Columns[i].Width = 36;

            dataGridView1.Columns[++i].HeaderText = "Номер"; //№3
            dataGridView1.Columns[i].Width = 45;


            dataGridView1.Columns[++i].HeaderText = "Начало"; //№4
            dataGridView1.Columns[i].Width = 45;


            dataGridView1.Columns[++i].HeaderText = "Конец";//№5
            dataGridView1.Columns[i].Width = 45;

            dataGridView1.Columns[++i].Visible = false;
            dataGridView1.Columns[i].HeaderText = "Страниц";//№6
            dataGridView1.Columns[i].Width = 0;


            dataGridView1.Columns[++i].HeaderText = "Название тома";//№2
            dataGridView1.Columns[i].Width = 160;

            dataGridView1.Columns[++i].HeaderText = "Название";//№7
            dataGridView1.Columns[i].Width = 200;


            dataGridView1.Columns[++i].HeaderText = "Тип";//№8
            dataGridView1.Columns[i].Width = 120;


            dataGridView1.Columns[++i].HeaderText = "Дата";//№9
            dataGridView1.Columns[i].Width = 85;


            dataGridView1.Columns[++i].HeaderText = "Путь";//№10
            dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            DataGridViewCellStyle linkStyle = new DataGridViewCellStyle();
            linkStyle.Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Underline);
            linkStyle.ForeColor = Color.DarkBlue;
            dataGridView1.Columns[GetNumberColumn("Путь")].DefaultCellStyle = linkStyle;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            presenter.Save();
            Close();
        }


        private int GetNumberColumn(string title)
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.HeaderText == title)
                {
                    return col.Index;
                }
            }
            MessageBox.Show($"Столбец {title} не найден");
            return 0;
        }


        private void btnNumerDocumentByPage_Click(object sender, EventArgs e)
        {
            SortedSet<int> tomes = presenter.GetAllTomsNumbers();
            foreach (int tom in tomes)
            {
                List<XMLDBLib.Document> docs = presenter.Catalog.Documents.Where(x => x.Tome == tom).OrderBy(b => b.StartPage).ToList();
                int i = 1;
                docs.ForEach(n => n.Number = i++);
            }
            presenter.Save();
            GetData();
        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex > 0)
            {
                string header = dataGridView1.Columns[e.ColumnIndex].HeaderText;
                if (header.Contains("Путь"))
                {
                    dataGridView1.Cursor = Cursors.Hand;
                }
            }

        }

        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.Cursor = Cursors.Default;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string header = dataGridView1.Columns[e.ColumnIndex].HeaderText;
            if (header.Contains("Путь"))
            {
                string path = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                if (File.Exists(path))
                {
                    Process.Start(path);
                }
            }
        }


        /// <summary>
        /// Сортировка при нажатии на заголовок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            string title = dataGridView1.Columns[e.ColumnIndex].HeaderText;
            SortData(title);
        }

        void SortData(string title)
        {
            switch (title)
            {
                case "Номер":
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.Number).ThenBy(b => b.StartPage).ToList();
                    break;
                case "Том":
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.Tome).ThenBy(b => b.Number).ToList();
                    break;

                case "Название тома":
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.TomeName).ThenBy(b => b.StartPage).ToList();
                    break;

                case "Тип":
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.DocType).ThenBy(b => b.StartPage).ToList();
                    break;
                case "Путь":
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.FullName).ThenBy(b => b.StartPage).ToList();
                    break;
                case "Название":
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.Name).ThenBy(b => b.StartPage).ToList();
                    break;
                case "Дата":
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.DateCreate).ThenBy(b => b.StartPage).ToList();
                    break;
                default:
                    dataGridView1.DataSource = CatalogDocs.Catalog.OrderBy(a => a.Tome).ThenBy(b => b.StartPage).ToList();
                    break;
            }
        }


        /// <summary>
        /// Расставить номера страниц
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetPageNumber_Click(object sender, EventArgs e)
        {
            int rows = dataGridView1.Rows.Count;
            if (rows == 0) return;
            SortData("Номер");
            SortedSet<int> tomes = new SortedSet<int>();
            for (int i = 0; i < rows; i++)
            {
                int tome = int.TryParse(dataGridView1.Rows[i].Cells[GetNumberColumn("Том")].Value.ToString(), out int t) ? t : 0;
                tomes.Add(tome);
            }
            foreach (int tome in tomes)
            {
                int endPage = 0;
                int num = 0;
                for (int i = 0; i < rows; i++)
                {
                    int tomeRow = int.TryParse(dataGridView1.Rows[i].Cells[GetNumberColumn("Том")].Value.ToString(), out int t) ? t : 0;
                    if (tomeRow == tome)
                    {
                        string path = dataGridView1.Rows[i].Cells[GetNumberColumn("Путь")].Value.ToString();
                        XMLDBLib.Document doc = presenter.Catalog.GetByPath(path);
                        doc.Number = ++num;
                        doc.StartPage = ++endPage;
                        doc.EndPage = doc.StartPage + doc.AmountPage-1;
                        endPage = doc.EndPage;
                        //dataGridView1.Rows[i].Cells[GetNumberColumn("Номер")].Value=num ;
                        // dataGridView1.Rows[i].Cells[GetNumberColumn("Том")].Value;

                    }
                }

            }
            presenter.Save();
            GetData();
        }
    }
}
