using System;
using System.Drawing;
using System.Windows.Forms;

namespace Keytrace
{
    public class CustomRangeDialog : Form
    {
        private readonly DateTimePicker fromPicker;
        private readonly DateTimePicker toPicker;

        public DateTime FromDate { get { return fromPicker.Value.Date; } }
        public DateTime ToDate { get { return toPicker.Value.Date; } }

        public CustomRangeDialog(DateTime from, DateTime to)
        {
            Text = "自定义时间段";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(Ui.Px(380), Ui.Px(200));

            Label l1 = new Label();
            l1.Text = "开始日期";
            l1.Font = Ui.F12;
            l1.ForeColor = Ui.Text2;
            l1.AutoSize = true;
            l1.Location = new Point(Ui.Px(24), Ui.Px(24));
            Controls.Add(l1);

            fromPicker = new DateTimePicker();
            fromPicker.Format = DateTimePickerFormat.Short;
            fromPicker.Font = Ui.F12;
            fromPicker.Location = new Point(Ui.Px(24), Ui.Px(46));
            fromPicker.Width = Ui.Px(150);
            fromPicker.Value = from;
            Controls.Add(fromPicker);

            Label l2 = new Label();
            l2.Text = "结束日期";
            l2.Font = Ui.F12;
            l2.ForeColor = Ui.Text2;
            l2.AutoSize = true;
            l2.Location = new Point(Ui.Px(200), Ui.Px(24));
            Controls.Add(l2);

            toPicker = new DateTimePicker();
            toPicker.Format = DateTimePickerFormat.Short;
            toPicker.Font = Ui.F12;
            toPicker.Location = new Point(Ui.Px(200), Ui.Px(46));
            toPicker.Width = Ui.Px(150);
            toPicker.Value = to;
            Controls.Add(toPicker);

            Button ok = new Button();
            ok.Text = "确定";
            ok.Font = Ui.F12;
            ok.FlatStyle = FlatStyle.System;
            ok.Location = new Point(Ui.Px(190), Ui.Px(130));
            ok.Width = Ui.Px(78);
            ok.Height = Ui.Px(32);
            ok.DialogResult = DialogResult.OK;
            Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "取消";
            cancel.Font = Ui.F12;
            cancel.FlatStyle = FlatStyle.System;
            cancel.Location = new Point(Ui.Px(278), Ui.Px(130));
            cancel.Width = Ui.Px(78);
            cancel.Height = Ui.Px(32);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
        }
    }
}
