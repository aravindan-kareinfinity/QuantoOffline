using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quanto
{
    /// <summary>
    /// First-run / reconfigure wizard: MASTER or CLIENT without editing App.config.
    /// </summary>
    public class SetupWizardForm : Form
    {
        public enum WizardMode
        {
            FirstRun,
            Reconfigure,
            ChangeMasterOnly
        }

        private readonly WizardMode _mode;
        private Panel _panelRole;
        private Panel _panelMasterConfirm;
        private Panel _panelMasterReady;
        private Panel _panelClientSearch;
        private Panel _panelClientSelect;
        private Panel _panelClientReady;

        private Label _lblMasterReadyBody;
        private Label _lblClientSearchStatus;
        private Label _lblClientReadyBody;
        private Label _lblFoundMasterDetail;
        private ListBox _lstMasters;
        private Button _btnConnect;
        private Button _btnSelectBack;
        private Button _btnSearchAgain;

        private List<MasterDiscovery.FoundMaster> _foundMasters = new List<MasterDiscovery.FoundMaster>();
        private MasterDiscovery.FoundMaster _selectedMaster;

        public SetupWizardForm(WizardMode mode = WizardMode.FirstRun)
        {
            _mode = mode;
            Text = "Quanto Client Setup";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 500);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            BuildPanels();

            if (mode == WizardMode.ChangeMasterOnly)
            {
                ShowPanel(_panelClientSearch);
                BeginSearch();
            }
            else
            {
                ShowPanel(_panelRole);
            }
        }

        private void BuildPanels()
        {
            _panelRole = CreatePanel();
            AddTitle(_panelRole, "Quanto Client Setup");
            AddSubtitle(_panelRole, "Configure this computer for your network.");

            var btnMaster = CreateOptionButton(
                "MASTER COMPUTER",
                "This computer stores the main business data\nand acts as the central server.",
                90);
            btnMaster.Click += (s, e) => ShowPanel(_panelMasterConfirm);
            _panelRole.Controls.Add(btnMaster);

            var btnClient = CreateOptionButton(
                "CLIENT COMPUTER",
                "This computer connects to the Master\nand provides local printer/hardware services.",
                220);
            btnClient.Click += (s, e) => OnChooseClient();
            _panelRole.Controls.Add(btnClient);

            if (_mode != WizardMode.FirstRun)
            {
                var btnCancel = CreateSecondaryButton("Cancel", 400);
                btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
                _panelRole.Controls.Add(btnCancel);
            }

            // Master confirm
            _panelMasterConfirm = CreatePanel();
            AddTitle(_panelMasterConfirm, "MASTER COMPUTER");
            AddMultiline(_panelMasterConfirm,
                "This computer will be configured as the MASTER.\n\n" +
                "• This computer owns the master business data.\n" +
                "• Other client computers will connect to this computer.\n" +
                "• This computer must remain powered on when clients need business data.\n\n" +
                "Changing role does not copy or delete business data files.",
                70);
            var btnMasterBack = CreateSecondaryButton("Back", 420);
            btnMasterBack.Click += (s, e) => ShowPanel(_panelRole);
            _panelMasterConfirm.Controls.Add(btnMasterBack);
            var btnMasterContinue = CreatePrimaryButton("Continue", 420, 160, 176);
            btnMasterContinue.Click += (s, e) => CompleteMasterSetup();
            _panelMasterConfirm.Controls.Add(btnMasterContinue);

            // Master ready
            _panelMasterReady = CreatePanel();
            AddTitle(_panelMasterReady, "MASTER CONFIGURED");
            _lblMasterReadyBody = AddMultiline(_panelMasterReady, "", 70, 330);
            var btnMasterFinish = CreatePrimaryButton("FINISH", 420, 200);
            btnMasterFinish.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            _panelMasterReady.Controls.Add(btnMasterFinish);

            // Client search
            _panelClientSearch = CreatePanel();
            AddTitle(_panelClientSearch, "CLIENT COMPUTER");
            AddSubtitle(_panelClientSearch, "This computer will connect to an existing MASTER.");
            _lblClientSearchStatus = AddMultiline(_panelClientSearch, "Searching for MASTER...", 90, 280);
            _btnSearchAgain = CreateSecondaryButton("Search Again", 420, 176);
            _btnSearchAgain.Click += (s, e) => BeginSearch();
            _btnSearchAgain.Visible = false;
            _btnSearchAgain.Size = new Size(140, 40);
            _panelClientSearch.Controls.Add(_btnSearchAgain);
            var btnClientBack = CreateSecondaryButton("Back", 420);
            btnClientBack.Click += (s, e) =>
            {
                if (_mode == WizardMode.ChangeMasterOnly)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
                else ShowPanel(_panelRole);
            };
            _panelClientSearch.Controls.Add(btnClientBack);

            // Client select / found
            _panelClientSelect = CreatePanel();
            AddTitle(_panelClientSelect, "MASTER FOUND");
            _lblFoundMasterDetail = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(40, 58),
                Size = new Size(480, 90),
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            _panelClientSelect.Controls.Add(_lblFoundMasterDetail);
            _lstMasters = new ListBox
            {
                Location = new Point(40, 155),
                Size = new Size(480, 145),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                ItemHeight = 24,
                Visible = false
            };
            _lstMasters.SelectedIndexChanged += (s, e) =>
            {
                _selectedMaster = _lstMasters.SelectedItem as MasterDiscovery.FoundMaster;
                UpdateFoundDetail();
            };
            _panelClientSelect.Controls.Add(_lstMasters);
            _btnConnect = CreatePrimaryButton("CONNECT TO MASTER", 420, 220, 176);
            _btnConnect.Click += (s, e) => ConnectToSelectedMaster();
            _panelClientSelect.Controls.Add(_btnConnect);
            _btnSelectBack = CreateSecondaryButton("Back", 420);
            _btnSelectBack.Click += (s, e) =>
            {
                ShowPanel(_panelClientSearch);
                BeginSearch();
            };
            _panelClientSelect.Controls.Add(_btnSelectBack);

            // Client ready
            _panelClientReady = CreatePanel();
            AddTitle(_panelClientReady, "CLIENT READY");
            _lblClientReadyBody = AddMultiline(_panelClientReady, "", 70, 330);
            var btnClientFinish = CreatePrimaryButton("FINISH", 420, 200);
            btnClientFinish.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            _panelClientReady.Controls.Add(btnClientFinish);

            Controls.Add(_panelRole);
            Controls.Add(_panelMasterConfirm);
            Controls.Add(_panelMasterReady);
            Controls.Add(_panelClientSearch);
            Controls.Add(_panelClientSelect);
            Controls.Add(_panelClientReady);
        }

        private void OnChooseClient()
        {
            if (_mode == WizardMode.Reconfigure && MachineConfig.IsMaster)
            {
                var confirm = MessageBox.Show(
                    "Current role:\nMASTER\n\nChange to CLIENT?\n\n" +
                    "WARNING:\nThis computer will no longer act as the central business-data server.\n" +
                    "Existing local data files are NOT deleted or migrated automatically.\n\nContinue?",
                    "Change Role",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (confirm != DialogResult.OK)
                    return;
            }

            try
            {
                MachineConfig.ConfigureAsClientPendingMaster();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Could not save client configuration:\n" + exp.Message, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ShowPanel(_panelClientSearch);
            BeginSearch();
        }

        private void CompleteMasterSetup()
        {
            if (_mode == WizardMode.Reconfigure && MachineConfig.IsClient)
            {
                var confirm = MessageBox.Show(
                    "Current role:\nCLIENT\n\nChange to MASTER?\n\n" +
                    "WARNING:\nMaster owns the central business data.\n" +
                    "This does NOT copy data from another MASTER and does NOT create a new database silently.\n" +
                    "Existing local data files on this PC (if any) remain as-is.\n\nContinue?",
                    "Change Role",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (confirm != DialogResult.OK)
                    return;
            }

            try
            {
                MachineConfig.ConfigureAsMaster();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Could not save MASTER configuration:\n" + exp.Message, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!FirewallHelper.EnsureApiPortAllowed(MachineConfig.ApiPort, out var fwMsg))
            {
                MessageBox.Show(
                    "Administrator permission is required to configure LAN access.\n\n" + fwMsg,
                    "Firewall",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            var ips = NetworkInfo.GetPrivateIPv4Addresses();
            var lan = ips.Count > 0 ? ips[0] : "localhost";
            _lblMasterReadyBody.Text =
                "Device ID:\n" + MachineConfig.DeviceId + "\n\n" +
                "API:\nhttp://" + lan + ":" + MachineConfig.ApiPort + "\n\n" +
                "Status:\n● Master configured — services start after Finish";
            ShowPanel(_panelMasterReady);
        }

        private void BeginSearch()
        {
            _lblClientSearchStatus.Text = "Searching for MASTER...";
            _btnSearchAgain.Visible = false;
            _btnSearchAgain.Enabled = false;

            Task.Run(() =>
            {
                var results = MasterDiscovery.DiscoverAllMasters(3500);
                BeginInvoke(new Action(() => OnSearchComplete(results)));
            });
        }

        private void OnSearchComplete(List<MasterDiscovery.FoundMaster> results)
        {
            _foundMasters = results ?? new List<MasterDiscovery.FoundMaster>();
            _btnSearchAgain.Visible = true;
            _btnSearchAgain.Enabled = true;

            if (_foundMasters.Count == 0)
            {
                _lblClientSearchStatus.Text =
                    "No MASTER found on the local network.\n\n" +
                    "Make sure the MASTER computer is running Quanto.Client.exe\n" +
                    "and is on the same Wi-Fi / LAN.\n\n" +
                    "Then click Search Again.";
                return;
            }

            if (_foundMasters.Count == 1)
            {
                _selectedMaster = _foundMasters[0];
                LayoutMasterSelect(false);
                UpdateFoundDetail();
                ShowPanel(_panelClientSelect);
                return;
            }

            _selectedMaster = null;
            LayoutMasterSelect(true);
            _lstMasters.DataSource = _foundMasters;
            if (_lstMasters.Items.Count > 0)
                _lstMasters.SelectedIndex = 0;
            _lblFoundMasterDetail.Text = "Select MASTER\n\nMore than one MASTER was found. Choose one:";
            ShowPanel(_panelClientSelect);
        }

        private void LayoutMasterSelect(bool showList)
        {
            _lstMasters.Visible = showList;
            _lstMasters.DataSource = null;
            if (showList)
            {
                _lblFoundMasterDetail.Size = new Size(480, 90);
                _lstMasters.Location = new Point(40, 155);
                _lstMasters.Size = new Size(480, 230);
                _lstMasters.BringToFront();
            }
            else
            {
                _lblFoundMasterDetail.Size = new Size(480, 280);
            }
            _btnSelectBack.Location = new Point(40, 420);
            _btnConnect.Location = new Point(176, 420);
        }

        private void UpdateFoundDetail()
        {
            if (_selectedMaster == null)
            {
                if (!_lstMasters.Visible)
                    return;
                _lblFoundMasterDetail.Text = "Select MASTER\n\nMore than one MASTER was found. Choose one:";
                return;
            }

            if (_lstMasters.Visible)
            {
                _lblFoundMasterDetail.Text =
                    "Select MASTER\n\n" +
                    (_selectedMaster.MachineName ?? "(unknown)") +
                    "  —  " + _selectedMaster.IpAddress + ":" + _selectedMaster.Port;
                return;
            }

            _lblFoundMasterDetail.Text =
                "MASTER FOUND\n\n" +
                "Computer:\n" + (_selectedMaster.MachineName ?? "(unknown)") + "\n\n" +
                "Device ID:\n" + _selectedMaster.DeviceId + "\n\n" +
                "IP Address:\n" + _selectedMaster.IpAddress + "\n\n" +
                "API Port:\n" + _selectedMaster.Port + "\n\n" +
                "Status:\n● Available";
        }

        private void ConnectToSelectedMaster()
        {
            if (_selectedMaster == null && _lstMasters.Visible)
                _selectedMaster = _lstMasters.SelectedItem as MasterDiscovery.FoundMaster;

            if (_selectedMaster == null)
            {
                MessageBox.Show("Please select a MASTER first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _btnConnect.Enabled = false;
            try
            {
                if (!MachineConfig.VerifyRemoteMaster(
                        _selectedMaster.IpAddress,
                        _selectedMaster.Port,
                        _selectedMaster.DeviceId,
                        out var machineName,
                        out var error))
                {
                    MessageBox.Show(
                        "Could not verify MASTER via /health.\n\n" + (error ?? "Unknown error"),
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MachineConfig.SaveMasterConnection(
                    _selectedMaster.DeviceId,
                    machineName ?? _selectedMaster.MachineName,
                    _selectedMaster.IpAddress,
                    _selectedMaster.Port);

                if (!FirewallHelper.EnsureApiPortAllowed(MachineConfig.ApiPort, out var fwMsg))
                {
                    MessageBox.Show(
                        "Administrator permission is required to configure LAN access.\n\n" + fwMsg,
                        "Firewall",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                var localIps = NetworkInfo.GetPrivateIPv4Addresses();
                var localIp = localIps.Count > 0 ? localIps[0] : "localhost";

                _lblClientReadyBody.Text =
                    "✓ MASTER CONNECTED\n\n" +
                    "Device:\n" + MachineConfig.DeviceId + "\n\n" +
                    "Master:\n" + MachineConfig.MasterDeviceId + "\n\n" +
                    "Master PC:\n" + (machineName ?? _selectedMaster.MachineName ?? "") + "\n\n" +
                    "Connection:\n● Connected  (" + _selectedMaster.IpAddress + ":" + _selectedMaster.Port + ")\n\n" +
                    "Local API (starts after Finish):\nhttp://" + localIp + ":" + MachineConfig.ApiPort + "\n\n" +
                    "Printer:\n● Will run locally on this computer";

                ShowPanel(_panelClientReady);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Failed to connect:\n" + exp.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnConnect.Enabled = true;
            }
        }

        private void ShowPanel(Panel panel)
        {
            foreach (Control c in Controls)
            {
                if (c is Panel p)
                    p.Visible = p == panel;
            }
        }

        private Panel CreatePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
        }

        private void AddTitle(Panel panel, string text)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 16F),
                Location = new Point(40, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(30, 30, 30)
            });
        }

        private void AddSubtitle(Panel panel, string text)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(40, 55),
                AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 80)
            });
        }

        private Label AddMultiline(Panel panel, string text, int top, int height = 180)
        {
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(40, top),
                Size = new Size(480, height),
                ForeColor = Color.FromArgb(40, 40, 40)
            };
            panel.Controls.Add(lbl);
            return lbl;
        }

        private Button CreateOptionButton(string title, string description, int top)
        {
            var btn = new Button
            {
                Location = new Point(40, top),
                Size = new Size(480, 100),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 247, 250),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 8, 16, 8),
                Font = new Font("Segoe UI Semibold", 11F),
                Text = title + "\n\n" + description,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 210, 220);
            return btn;
        }

        private Button CreatePrimaryButton(string text, int top, int width = 200, int left = 40)
        {
            return new Button
            {
                Text = text,
                Location = new Point(left, top),
                Size = new Size(width, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 118, 210),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10F),
                Cursor = Cursors.Hand
            };
        }

        private Button CreateSecondaryButton(string text, int top, int left = 40)
        {
            return new Button
            {
                Text = text,
                Location = new Point(left, top),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
        }
    }
}
