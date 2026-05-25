using System;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePassLib;

namespace KeePassSorter
{
    public sealed class KeePassSorterExt : Plugin
    {
        private IPluginHost m_host = null;

        public override bool Initialize(IPluginHost host)
        {
            m_host = host;
            return true;
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            if (t == PluginMenuType.Main)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem("KeePass Sorter");
                ToolStripMenuItem tsmiSort = new ToolStripMenuItem("Sort Entries...");
                tsmiSort.Click += new EventHandler(OnSort);
                tsmi.DropDownItems.Add(tsmiSort);
                return tsmi;
            }
            else if (t == PluginMenuType.Group)
            {
                ToolStripMenuItem tsmiSort = new ToolStripMenuItem("Sort Entries inside Group...");
                tsmiSort.Click += new EventHandler(OnSort);
                return tsmiSort;
            }
            return null;
        }

        private void OnSort(object sender, EventArgs e)
        {
            if (m_host == null || m_host.Database == null || !m_host.Database.IsOpen)
            {
                MessageBox.Show("Vui lòng mở một cơ sở dữ liệu KeePass trước khi thực hiện sắp xếp.", 
                    "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PwGroup group = m_host.MainWindow.GetSelectedGroup();
            if (group == null)
            {
                MessageBox.Show("Không tìm thấy nhóm dữ liệu được chọn.", 
                    "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SortingDialog dlg = new SortingDialog())
            {
                if (dlg.ShowDialog(m_host.MainWindow) == DialogResult.OK)
                {
                    SortingOptions opts = dlg.GetOptions();
                    SortingEngine engine = new SortingEngine();
                    int count = engine.SortGroup(group, opts);

                    if (count > 0)
                    {
                        m_host.MainWindow.UpdateUI(false, null, false, null, true, null, true);
                    }

                    string message = (count > 0) ?
                        string.Format("Đã sắp xếp thành công {0} mục dữ liệu!", count) :
                        "Thứ tự hiện tại đã đúng, không có mục nào cần thay đổi.";

                    MessageBox.Show(message, "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public override void Terminate() { }
    }
}
