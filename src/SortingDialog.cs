using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeePassSorter
{
    public class SortingDialog : Form
    {
        private ComboBox m_cmbCriteria;
        private ComboBox m_cmbOrder;
        private CheckBox m_chkRecursive;
        private CheckBox m_chkCaseSensitive;
        private CheckBox m_chkVietnamese;
        private Button m_btnOk;
        private Button m_btnCancel;

        public SortingDialog()
        {
            this.Text = "KeePass Sorter - Sắp xếp";
            this.Size = new Size(380, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 15;

            // Tiêu chí
            Label lblCriteria = new Label();
            lblCriteria.Text = "Tiêu chí:";
            lblCriteria.Location = new Point(15, y + 3);
            lblCriteria.AutoSize = true;
            this.Controls.Add(lblCriteria);

            m_cmbCriteria = new ComboBox();
            m_cmbCriteria.DropDownStyle = ComboBoxStyle.DropDownList;
            m_cmbCriteria.Items.AddRange(new object[] {
                "Tên (Title)",
                "Tên người dùng (Username)",
                "URL",
                "Thời gian tạo",
                "Thời gian sửa đổi",
                "Ghi chú (Notes)"
            });
            m_cmbCriteria.SelectedIndex = 0;
            m_cmbCriteria.Location = new Point(140, y);
            m_cmbCriteria.Size = new Size(210, 21);
            this.Controls.Add(m_cmbCriteria);

            y += 35;

            // Thứ tự
            Label lblOrder = new Label();
            lblOrder.Text = "Thứ tự:";
            lblOrder.Location = new Point(15, y + 3);
            lblOrder.AutoSize = true;
            this.Controls.Add(lblOrder);

            m_cmbOrder = new ComboBox();
            m_cmbOrder.DropDownStyle = ComboBoxStyle.DropDownList;
            m_cmbOrder.Items.AddRange(new object[] {
                "Tăng dần (A → Z)",
                "Giảm dần (Z → A)"
            });
            m_cmbOrder.SelectedIndex = 0;
            m_cmbOrder.Location = new Point(140, y);
            m_cmbOrder.Size = new Size(210, 21);
            this.Controls.Add(m_cmbOrder);

            y += 40;

            // Checkboxes
            m_chkRecursive = new CheckBox();
            m_chkRecursive.Text = "Sắp xếp đệ quy (bao gồm nhóm con)";
            m_chkRecursive.Location = new Point(15, y);
            m_chkRecursive.AutoSize = true;
            m_chkRecursive.Checked = true;
            this.Controls.Add(m_chkRecursive);

            y += 28;

            m_chkCaseSensitive = new CheckBox();
            m_chkCaseSensitive.Text = "Phân biệt chữ hoa/thường";
            m_chkCaseSensitive.Location = new Point(15, y);
            m_chkCaseSensitive.AutoSize = true;
            m_chkCaseSensitive.Checked = false;
            this.Controls.Add(m_chkCaseSensitive);

            y += 28;

            m_chkVietnamese = new CheckBox();
            m_chkVietnamese.Text = "Hỗ trợ sắp xếp tiếng Việt";
            m_chkVietnamese.Location = new Point(15, y);
            m_chkVietnamese.AutoSize = true;
            m_chkVietnamese.Checked = false;
            this.Controls.Add(m_chkVietnamese);

            y += 40;

            // Buttons
            m_btnOk = new Button();
            m_btnOk.Text = "Sắp xếp";
            m_btnOk.Size = new Size(90, 28);
            m_btnOk.Location = new Point(150, y);
            m_btnOk.Click += delegate(object sender, EventArgs e)
            {
                SortingDialogOptionsState.SaveOptions(GetOptions());
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(m_btnOk);

            m_btnCancel = new Button();
            m_btnCancel.Text = "Hủy";
            m_btnCancel.Size = new Size(90, 28);
            m_btnCancel.Location = new Point(250, y);
            m_btnCancel.Click += delegate(object sender, EventArgs e)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(m_btnCancel);

            this.AcceptButton = m_btnOk;
            this.CancelButton = m_btnCancel;

            ApplyOptions(SortingDialogOptionsState.GetInitialOptions());
        }

        private void ApplyOptions(SortingOptions options)
        {
            m_cmbCriteria.SelectedIndex = (int)options.Criteria;
            m_cmbOrder.SelectedIndex = options.Ascending ? 0 : 1;
            m_chkRecursive.Checked = options.Recursive;
            m_chkCaseSensitive.Checked = options.CaseSensitive;
            m_chkVietnamese.Checked = options.UseVietnamese;
        }

        public SortingOptions GetOptions()
        {
            SortingOptions opts = new SortingOptions();
            opts.Criteria = (SortCriteria)m_cmbCriteria.SelectedIndex;
            opts.Ascending = (m_cmbOrder.SelectedIndex == 0);
            opts.Recursive = m_chkRecursive.Checked;
            opts.CaseSensitive = m_chkCaseSensitive.Checked;
            opts.UseVietnamese = m_chkVietnamese.Checked;
            return opts;
        }
    }
}
