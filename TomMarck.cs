﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatalogPdf
{
    public partial class TomMarck : LineCatalog
    {
        public int Tome { get; set; }
      
        public TomMarck()
        {
            InitializeComponent();
           
        }

        public delegate void TomeSelect(int tome);
        public event TomeSelect ClickTomeSelect;

        public void Init()
        {
          if (Tome == 0)
            {
                LbTitle.Text = "Неотсортированные документы";
            }
            else
            {
            LbTitle.Text = $"Том {Tome} ";        

            }
        }

        private void btnSelectTome_Click(object sender, EventArgs e)
        {
            ClickTomeSelect?.Invoke(Tome);
        }

    }
}
