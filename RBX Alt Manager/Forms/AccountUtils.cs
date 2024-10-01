using RBX_Alt_Manager.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RBX_Alt_Manager
{
    public partial class AccountUtils : Form
    {
        // sorry i got lazy with this part of code and didnt name any elemennts

        public AccountUtils()
        {
            AccountManager.SetDarkBar(Handle);

            InitializeComponent();
            this.Rescale();

            AccountManager.Instance.SelectedAccountsChanged += (s, e) =>
            {
                if (AccountManager.SelectedAccounts.Count == 0)
                    OverrideFPSCB.CheckState = CheckState.Unchecked;
                else
                    OverrideFPSCB.CheckState = AccountManager.SelectedAccounts.Exists(x => x.HasField("MaxFPS") != AccountManager.SelectedAccounts[0].HasField("MaxFPS")) ? CheckState.Indeterminate : (AccountManager.SelectedAccounts[0].HasField("MaxFPS") ? CheckState.Checked : CheckState.Unchecked);
            };
        }

        public void ApplyTheme()
        {
            BackColor = ThemeEditor.FormsBackground;
            ForeColor = ThemeEditor.FormsForeground;

            ApplyTheme(Controls);
        }

        public void ApplyTheme(Control.ControlCollection _Controls)
        {
            foreach (Control control in _Controls)
            {
                if (control is Button || control is CheckBox)
                {
                    if (control is Button)
                    {
                        Button b = control as Button;
                        b.FlatStyle = ThemeEditor.ButtonStyle;
                        b.FlatAppearance.BorderColor = ThemeEditor.ButtonsBorder;
                    }

                    if (!(control is CheckBox)) control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;
                }
                else if (control is TextBox || control is RichTextBox)
                {
                    if (control is Classes.BorderedTextBox)
                    {
                        Classes.BorderedTextBox b = control as Classes.BorderedTextBox;
                        b.BorderColor = ThemeEditor.TextBoxesBorder;
                    }

                    if (control is Classes.BorderedRichTextBox)
                    {
                        Classes.BorderedRichTextBox b = control as Classes.BorderedRichTextBox;
                        b.BorderColor = ThemeEditor.TextBoxesBorder;
                    }

                    control.BackColor = ThemeEditor.TextBoxesBackground;
                    control.ForeColor = ThemeEditor.TextBoxesForeground;
                }
                else if (control is Label)
                {
                    control.BackColor = ThemeEditor.LabelTransparent ? Color.Transparent : ThemeEditor.LabelBackground;
                    control.ForeColor = ThemeEditor.LabelForeground;
                }
                else if (control is ListBox)
                {
                    control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;
                }
                else if (control is TableLayoutPanel)
                    ApplyTheme(control.Controls);
            }
        }

        private void AccountUtils_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void WhoFollow_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Account account in AccountManager.SelectedAccounts)
                account.SetFollowPrivacy(WhoFollow.SelectedIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want sign out of all other sessions?", "Account Utilities", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
                foreach (Account account in AccountManager.SelectedAccounts)
                    account.LogOutOfOtherSessions();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (Account account in AccountManager.SelectedAccounts)
                account.UnlockPin(textBox5.Text);
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                foreach (Account account in AccountManager.SelectedAccounts)
                    account.UnlockPin(textBox5.Text);

                button7.Focus();

                return;
            }

            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void textBox5_Enter(object sender, EventArgs e)
        {
            textBox5.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (Account account in AccountManager.SelectedAccounts)
                account.ChangePassword(textBox1.Text, textBox2.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Account account in AccountManager.SelectedAccounts)
                account.ChangeEmail(textBox1.Text, textBox3.Text);
        }

        private void Block_Click(object sender, EventArgs e)
        {
            bool WasUnblocked = false;

            foreach (Account account in AccountManager.SelectedAccounts)
                try
                {
                    if (account.TogglePlayerBlocked(Username.Text, ref WasUnblocked))
                        MessageBox.Show($"[{account.Username}] {(WasUnblocked ? "Unb" : "B")}locked {Username.Text}");
                    else
                        MessageBox.Show($"[{account.Username}] Failed to {(WasUnblocked ? "Unb" : "B")}lock {Username.Text}");
                }
                catch (Exception x) { MessageBox.Show($"[{account.Username}] Failed to {(WasUnblocked ? "Unb" : "B")}lock {Username.Text}\n\n{x.Message}"); }
        }

        private void SetDisplayName_Click(object sender, EventArgs e)
        {
            foreach (Account account in AccountManager.SelectedAccounts)
                try
                {
                    account.SetDisplayName(DisplayName.Text);
                    MessageBox.Show($"Successfully set {account.Username}'s Display Name to {DisplayName.Text}", "Account Utilities", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception x) { MessageBox.Show(x.Message, "Account Utilities", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void AddFriend_Click(object sender, EventArgs e)
        {
            foreach (Account account in AccountManager.SelectedAccounts)
                account.SendFriendRequest(Username.Text);
        }

        private void OverrideFPSCB_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine(sender);
            FPSCapNum.Enabled = OverrideFPSCB.Checked;

            if (OverrideFPSCB.Checked)
                foreach (var Account in AccountManager.SelectedAccounts)
                    Account.SetField("MaxFPS", $"{FPSCapNum.Value:0}");
            else
                foreach (var Account in AccountManager.SelectedAccounts)
                    Account.RemoveField("MaxFPS");
        }

        private void FPSCapNum_ValueChanged(object sender, EventArgs e)
        {
            foreach (var Account in AccountManager.SelectedAccounts)
                Account.SetField("MaxFPS", $"{FPSCapNum.Value:0}");
        }
    }
}