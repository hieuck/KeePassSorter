using System;
using System.IO;
using System.Net;
using System.Threading;
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
            StartAutoUpdateCheck();
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

                tsmi.DropDownItems.Add(new ToolStripSeparator());

                ToolStripMenuItem tsmiUpdate = new ToolStripMenuItem("Check for Updates...");
                tsmiUpdate.Click += new EventHandler(OnCheckForUpdates);
                tsmi.DropDownItems.Add(tsmiUpdate);

                ToolStripMenuItem tsmiAbout = new ToolStripMenuItem("About KeePass Sorter...");
                tsmiAbout.Click += new EventHandler(OnAbout);
                tsmi.DropDownItems.Add(tsmiAbout);

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

        private void OnAbout(object sender, EventArgs e)
        {
            string message =
                "KeePass Sorter " + UpdateChecker.GetCurrentVersion() + Environment.NewLine +
                Environment.NewLine +
                "Advanced natural sorting plugin for KeePass 2.x." + Environment.NewLine +
                "Natural sort example: photos, photos1, photos2." + Environment.NewLine +
                Environment.NewLine +
                UpdateChecker.ReleasesUrl;

            MessageBox.Show(GetOwner(), message, "About KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnCheckForUpdates(object sender, EventArgs e)
        {
            CheckForUpdatesAsync(true);
        }

        private void StartAutoUpdateCheck()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Thread.Sleep(4000);
                CheckForUpdates(false);
            });
        }

        private void CheckForUpdatesAsync(bool interactive)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                CheckForUpdates(interactive);
            });
        }

        private void CheckForUpdates(bool interactive)
        {
            try
            {
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | (SecurityProtocolType)3072;
                UpdateInfo info = UpdateChecker.CheckLatest();

                if (info == null || !info.IsUpdateAvailable)
                {
                    if (interactive)
                    {
                        ShowOnUi(delegate
                        {
                            MessageBox.Show(GetOwner(),
                                "Bạn đang dùng phiên bản mới nhất: " + UpdateChecker.GetCurrentVersion(),
                                "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    return;
                }

                ShowOnUi(delegate
                {
                    PromptForUpdate(info);
                });
            }
            catch (Exception ex)
            {
                if (!interactive) return;

                ShowOnUi(delegate
                {
                    MessageBox.Show(GetOwner(),
                        "Không thể kiểm tra cập nhật." + Environment.NewLine + ex.Message,
                        "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
            }
        }

        private void PromptForUpdate(UpdateInfo info)
        {
            string message =
                "Có phiên bản mới của KeePass Sorter." + Environment.NewLine +
                Environment.NewLine +
                "Đang dùng: " + UpdateChecker.GetCurrentVersion() + Environment.NewLine +
                "Mới nhất: " + info.LatestVersion + Environment.NewLine +
                Environment.NewLine +
                "Tải và cài đặt bản cập nhật ngay bây giờ?";

            DialogResult result = MessageBox.Show(GetOwner(), message, "KeePass Sorter Update",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                InstallUpdateAsync(info);
            }
        }

        private void InstallUpdateAsync(UpdateInfo info)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | (SecurityProtocolType)3072;
                    string targetPath = GetPluginPackagePath();
                    string tempPath = targetPath + ".download";

                    using (WebClient client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.UserAgent] = "KeePassSorter";
                        client.DownloadFile(info.AssetUrl, tempPath);
                    }

                    try
                    {
                        File.Copy(tempPath, targetPath, true);
                        File.Delete(tempPath);

                        ShowOnUi(delegate
                        {
                            MessageBox.Show(GetOwner(),
                                "Đã cập nhật KeePass Sorter. Vui lòng khởi động lại KeePass để dùng phiên bản mới.",
                                "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    catch (Exception copyEx)
                    {
                        string pendingPath = targetPath + ".new";
                        File.Copy(tempPath, pendingPath, true);
                        File.Delete(tempPath);

                        ShowOnUi(delegate
                        {
                            MessageBox.Show(GetOwner(),
                                "Đã tải bản cập nhật nhưng chưa thể thay file plugin đang dùng." + Environment.NewLine +
                                "File mới: " + pendingPath + Environment.NewLine +
                                "Lý do: " + copyEx.Message,
                                "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        });
                    }
                }
                catch (Exception ex)
                {
                    ShowOnUi(delegate
                    {
                        MessageBox.Show(GetOwner(),
                            "Không thể tải bản cập nhật." + Environment.NewLine + ex.Message,
                            "KeePass Sorter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                }
            });
        }

        private string GetPluginPackagePath()
        {
            string keepassDir = Path.GetDirectoryName(Application.ExecutablePath);
            string pluginsDir = Path.Combine(keepassDir, "Plugins");
            Directory.CreateDirectory(pluginsDir);
            return Path.Combine(pluginsDir, "KeePassSorter.plgx");
        }

        private void ShowOnUi(MethodInvoker action)
        {
            Form owner = GetOwner();
            if (owner != null && !owner.IsDisposed)
            {
                if (owner.InvokeRequired)
                    owner.BeginInvoke(action);
                else
                    action();
            }
            else
            {
                action();
            }
        }

        private Form GetOwner()
        {
            return (m_host != null) ? m_host.MainWindow : null;
        }

        public override void Terminate() { }
    }
}
