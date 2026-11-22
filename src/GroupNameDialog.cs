using System;
using System.Drawing;
using System.Windows.Forms;

namespace Keytrace
{
    public class GroupNameDialog : Form
    {
        private readonly TextBox box;

        public string Value { get { return box.Text.Trim(); } }

        public GroupNameDialog(string title, string initial)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(Ui.Px(400), Ui.Px(190));

            Label l1 = new Label();
            l1.Text = "分组名称";
            l1.Font = Ui.F12;
            l1.ForeColor = Ui.Text2;
            l1.AutoSize = true;
            l1.Location = new Point(Ui.Px(24), Ui.Px(20));
            Controls.Add(l1);

            box = new TextBox();
            box.Font = Ui.F13;
            box.Location = new Point(Ui.Px(24), Ui.Px(42));
            box.Width = Ui.Px(352);
            box.MaxLength = 20;
            box.Text = initial ?? "";
            box.SelectAll();
            Controls.Add(box);

            Label hint = new Label();
            hint.Text = "最多 20 个字。每个分组各自记录，互不影响。";
            hint.Font = Ui.F10;
            hint.ForeColor = Ui.Text3;
            hint.AutoSize = true;
            hint.Location = new Point(Ui.Px(24), Ui.Px(74));
            Controls.Add(hint);

            Button ok = new Button();
            ok.Text = "确定";
            ok.Font = Ui.F12;
            ok.FlatStyle = FlatStyle.System;
            ok.Location = new Point(Ui.Px(212), Ui.Px(120));
            ok.Width = Ui.Px(78);
            ok.Height = Ui.Px(32);
            ok.Click += delegate
            {
                if (Value.Length == 0)
                {
                    MessageBox.Show(this, "请输入分组名称。", "Keytrace");
                    box.Focus();
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "取消";
            cancel.Font = Ui.F12;
            cancel.FlatStyle = FlatStyle.System;
            cancel.Location = new Point(Ui.Px(298), Ui.Px(120));
            cancel.Width = Ui.Px(78);
            cancel.Height = Ui.Px(32);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
        }
    }
}
