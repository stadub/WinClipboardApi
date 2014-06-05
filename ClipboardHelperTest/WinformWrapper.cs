using System;
using System.Net.Mime;
using System.Windows.Forms;

namespace ClipboardHelperTest
{
    internal class WinformWrapper : IDisposable
    {
        private FormWrapper form;
        private bool disposed;

        public void CreateWindow()
        {
            if (form == null)
            {
                form = new FormWrapper();
            }
            if (form.Visible)
                    return;
                form.Show();
                form.GotFocus += form_GotFocus;
        }

        void form_GotFocus(object sender, EventArgs e)
        {
           
        }

        public void CloseWindow()
        {
            if(form!=null)
                form.Close();
        }
        private class FormWrapper : Form
        {
            public Predicate<Message> WinProcHandler { get; set; }
            protected override void WndProc(ref Message m)
            {
                if (WinProcHandler != null)
                {
                    WinProcHandler(m);

                }
                base.WndProc(ref m);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public IntPtr Handle { get { return form.Handle; } }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (form != null)
                    {
                        var tempForm = form;
                        form = null;
                        tempForm.Close();
                        tempForm.Dispose();
                        
                    }
                }
                disposed = true;
            }
        }
    }
}
