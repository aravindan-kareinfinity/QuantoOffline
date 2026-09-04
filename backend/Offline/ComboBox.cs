using System;
using System.Drawing.Text;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Quanto.Controls
{
	/// <summary>
	/// Summary description for ComboBox.
	/// </summary>

    public class WndProcInterceptorCMB : NativeWindow
    {
        int WM_MOUSEMOVE = (int)0x0200;
        int WM_MOUSEDOWN = (int)0x201;
        public WndProcInterceptorCMB(IntPtr intptr)
        {
            this.AssignHandle(intptr);
        }
        private bool isReadonly = true;
        public void SetReadOnly(bool isreadonly)
        {
            isReadonly = isreadonly;
        }
        protected override void WndProc(ref Message m)
        {
            // http://social.msdn.microsoft.com/Forums/en/winforms/thread/e3490e28-7c11-4e74-8dbd-a6c6913cf884
            // kill WM_MOUSEMOVE event in listview control when it is owner drawn.
            // this does solve the issue with the extra draw event which did lead to crappy MS example code where
            // the item under the mouse was redrawn every time the mouse did move.
            if (!isReadonly)
            {
                base.WndProc(ref m);
            }
            else if (m.Msg != WM_MOUSEMOVE && m.Msg != WM_MOUSEDOWN )
            {
                base.WndProc(ref m);
            }
        }
    }
    public class ComboBox : System.Windows.Forms.ComboBox
	{

        //private BorderDrawer borderDrawer = new BorderDrawer();

        //protected override void WndProc(ref Message m)
        //{
        //    base.WndProc(ref m);
        //    borderDrawer.DrawBorder(ref m, this.Width, this.Height);
        //}

        //public Color BorderColor
        //{
        //    get { return borderDrawer.BorderColor; }
        //    set
        //    {
        //        borderDrawer.BorderColor = value;
        //        Invalidate();
        //    }
        //}

        public const int WM_PAINT = 0xF;
        [DllImport("user32")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        //protected override void WndProc(ref Message m)
        //{
        //    IntPtr hDC = IntPtr.Zero;
        //    Graphics gdc = null;
        //    switch (m.Msg)
        //    {
        //        case WM_PAINT:
        //            base.WndProc(ref m);
        //            hDC = GetWindowDC(this.Handle);
        //            gdc = Graphics.FromHdc(hDC);
        //            PaintFlatControlBorder(this, gdc);
        //            ReleaseDC(m.HWnd, hDC);
        //            gdc.Dispose();

        //            break;
        //        default:
        //            base.WndProc(ref m);
        //            break;
        //    }
        //}

        private void PaintFlatControlBorder(Control ctrl, Graphics g)
        {
            Rectangle outer = new Rectangle(0, 0, this.Width, this.Height);
            Rectangle inner = new Rectangle(1, 1, this.Width - 2, this.Height - 2);

            //ControlPaint.DrawBorder(g, new Rectangle(inner.Right - 20, inner.Top, inner.Width, inner.Height), Color.DarkGray, ButtonBorderStyle.Solid);
            ControlPaint.DrawComboButton(g, inner.Right - 20, inner.Top, inner.Width, inner.Height, ButtonState.Flat);
            GraphicsPath path = new GraphicsPath();
            Point pt1 = new Point(inner.Right-15, inner.Top+8);
            Point pt2 = new Point(inner.Right - 10, inner.Bottom-8);
            Point pt3 = new Point(inner.Right-5, inner.Top+8);
            path.AddPolygon(new Point[] { pt1, pt2, pt3 });

            g.SmoothingMode = SmoothingMode.AntiAlias;
            if(this.Enabled)
                g.FillPath(new SolidBrush(Color.DarkBlue), path);
            else
                g.FillPath(new SolidBrush(Color.LightBlue), path);
            //g.DrawPath(new Pen(Color.DarkGray, 4), path);


            ControlPaint.DrawBorder(g, outer, Color.DarkGray, ButtonBorderStyle.Solid);
            ControlPaint.DrawBorder(g, inner, this.BackColor, ButtonBorderStyle.Solid);
        }


		public delegate void UpdateDataSourceDelegate(ComboBox sender);
		public event UpdateDataSourceDelegate UpdateDataSource;
		public event System.ComponentModel.CancelEventHandler NotInList;
		private bool _limitToList = true;
        private bool _inEditMode = false;

        public bool InEditMode
        {
            get { return _inEditMode; }
            set { _inEditMode = value; }
        }
		private bool _AutoUpdate = true;
		Color _FocusColor = Color.FromKnownColor(System.Drawing.KnownColor.Lavender);
		Color _BackGroundColor = Color.FromKnownColor(System.Drawing.KnownColor.PowderBlue);
		private bool _EntryKey = true;
		private System.Windows.Forms.Form frmEntryScreen;

		public ComboBox() : base()
		{
            this.Font = new Font("Verdana", 9.75F);
			//this.BackColor = _BackGroundColor;
            this.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
		}

        void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

		public int GetSelectedIntValue()
		{
			if(this.SelectedValue !=null)
				return int.Parse(this.SelectedValue.ToString());
			return -1;
		}

        public bool AutoFocus = true;
		protected override void OnEnter(EventArgs e)
		{
            try
            {
                if (AutoFocus)
                {
                    this.SelectionStart = 0;
                    this.SelectionLength = 0;
                }
                _BackGroundColor = this.BackColor;
                //this.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.White);
                //this.BackColor = _FocusColor;
                //this.Font = new Font(this.Font.Name, this.Font.Size,FontStyle.Bold);
                //System.Windows.Forms.SendKeys.Send("%{DOWN}");
            }catch(Exception exp)
            {
            }
			base.OnEnter (e);
		}

		protected override void OnLeave(EventArgs e)
		{
            try
            {
                if (AutoFocus)
                {
                    this.SelectionStart = 0;
                    this.SelectionLength = 0;
                }
                //this.Font = new Font(this.Font.Name, this.Font.Size,FontStyle.Regular);
                //this.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.Black);
                //this.BackColor = _BackGroundColor;
            }
            catch (Exception exp)
            {
                exp = null;
            }
			base.OnLeave (e);
		}

		public bool LimitToList
		{
			get { return _limitToList; }
			set { _limitToList = value; }
		}

		protected virtual void OnNotInList(System.ComponentModel.CancelEventArgs e)
		{
			if (NotInList != null)
			{
				NotInList(this, e);
			}
		}   

		protected override void OnTextChanged(System.EventArgs e)
		{
            if (_inEditMode && AutoFocus)
			{
				string input = Text;
				int index = FindString(input);
				if (index >= 0)
				{
					_inEditMode = false;
					SelectedIndex = index;
					_inEditMode = true;
					Select(input.Length, Text.Length);
				}
			}

			base.OnTextChanged(e);
		}

        //protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
        //{
        //    if (this.LimitToList)
        //    {
        //        int pos = this.FindStringExact(this.Text);
        
        //        if (pos == -1)
        //        {
        //            OnNotInList(e);
        //        }
        //        else
        //        {
        //            this.SelectedIndex = pos;
        //        }
        //    }

        //    base.OnValidating(e);
        //}

        private void ValidateValue()
        {
            if (this.SelectedItem == null && !string.IsNullOrEmpty(this.Text))
            {
                string input = Text;
                int index = FindString(input);
                if (index >= 0)
                {
                    _inEditMode = false;
                    SelectedIndex = index;
                    _inEditMode = true;
                    Select(input.Length, Text.Length);
                }
            }
        }

		public long GetLongValue()
		{
            ValidateValue();
			if(this.SelectedItem!=null)
			{
                if (this.SelectedItem is System.Data.DataRowView)
                {
                    if (string.IsNullOrEmpty(this.ValueMember))
                        return -1;
                    return (long)((System.Data.DataRowView)this.SelectedItem)[this.ValueMember];
                }
                else
                    return (long)this.SelectedValue;
			}
			return -1;
		}

		public object GetValue()
		{
            ValidateValue();
			if(this.SelectedItem!=null)
			{
                if (this.SelectedItem is System.Data.DataRowView)
                {
                    if (string.IsNullOrEmpty(this.ValueMember))
                        return null;
                    return ((System.Data.DataRowView)this.SelectedItem)[this.ValueMember];
                }
                else
                    return this.SelectedValue;
			}
			return null;
		}

		public int GetIntValue()
		{
            ValidateValue();
			if(this.SelectedItem!=null)
			{
				return (int) this.SelectedValue;
			}
			return -1;
		}

		public Int16 GetInt16Value()
		{
            ValidateValue();
			if(this.SelectedItem!=null)
			{
				return (Int16) this.SelectedValue;
			}
			return -1;
		}

		public byte GetTinyIntValue()
		{
            ValidateValue();
			if(this.SelectedItem!=null)
			{
				return (byte) this.SelectedValue;
			}
			return 0;
		}

		protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == System.Windows.Forms.Keys.F5)
			{
				RefreshDatasource();
			}
			else if(e.KeyCode == System.Windows.Forms.Keys.F2)
			{
				if(frmEntryScreen != null)
				{
					frmEntryScreen.ShowDialog();
					RefreshDatasource();
				}
			}
			else
			{
                if (readOnly)
                    e.Handled = true;
                else
                    _inEditMode = (e.KeyCode != System.Windows.Forms.Keys.Back && e.KeyCode != System.Windows.Forms.Keys.Delete);
				base.OnKeyDown(e);
			}
		}

		public System.Windows.Forms.Form EntryScreen
		{
			get
			{
				return frmEntryScreen;
			}
			set
			{
				frmEntryScreen = value;
			}
		}

		



		public void RefreshDatasource()
		{
			if(this.UpdateDataSource!=null)
			{
				object obj = this.SelectedValue;
				this.UpdateDataSource(this);
				if(obj != null && obj != DBNull.Value)
					this.SelectedValue = obj;
			}
		}

        WndProcInterceptorCMB WndProcInterceptorcmb = null;
        private bool readOnly;
        public bool ReadOnly
        {
            get { return readOnly; }
            set { 
                readOnly = value;
                if (WndProcInterceptorcmb == null)
                {
                    WndProcInterceptorcmb = new WndProcInterceptorCMB(this.Handle);
                }
                WndProcInterceptorcmb.SetReadOnly(value);
            }
        }

		public bool EnterKeyAsTab
		{
			set
			{
				_EntryKey = value;
			}
			get
			{
				return _EntryKey;
			}
		}

	}
}
