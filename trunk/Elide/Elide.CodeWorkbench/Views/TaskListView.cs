using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Elide.CodeEditor.Views;
using Elide.Core;
using Elide.Environment;
using Elide.Environment.Views;
using Elide.Scintilla;

namespace Elide.CodeWorkbench.Views
{
    public sealed class TaskListView : AbstractView
    {
        private TaskManager man;

        public TaskListView()
        {
            
        }

        public override void Initialize(IApp app)
        {
            base.Initialize(app);

            if (man == null)
                man = new TaskManager(app, (TaskListService)app.GetService<ITaskListService>());
        }

        public override void Activate()
        {
            man.TreeView.Select();
        }
        
        public override object Control
        {
            get { return man.TreeView; }
        }
    }
}
