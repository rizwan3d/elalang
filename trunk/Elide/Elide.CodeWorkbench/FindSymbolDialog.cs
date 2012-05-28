﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Elide.Forms.State;

namespace Elide.CodeWorkbench
{
    public partial class FindSymbolDialog : StateForm
    {
        public FindSymbolDialog()
        {
            InitializeComponent();
        }

        [StateItem]
        public string Symbol
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        [StateItem]
        public bool OnlyGlobals
        {
            get { return globals.Checked; }
            set { globals.Checked = value; }
        }

        [StateItem]
        public bool AllFiles
        {
            get { return allFiles.Checked; }
            set { allFiles.Checked = value; }
        }
    }
}
