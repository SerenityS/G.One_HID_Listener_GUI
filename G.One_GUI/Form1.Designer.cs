
namespace G.One_GUI
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.DB_bt = new System.Windows.Forms.Button();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.LED_ON_OFF = new System.Windows.Forms.Button();
            this.MULTITAP_ON_OFF = new System.Windows.Forms.Button();
            this.Tray_Icon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // DB_bt
            // 
            this.DB_bt.Location = new System.Drawing.Point(668, 394);
            this.DB_bt.Name = "DB_bt";
            this.DB_bt.Size = new System.Drawing.Size(120, 50);
            this.DB_bt.TabIndex = 1;
            this.DB_bt.Text = "센서 상태 확인/\r\n키보드 연결";
            this.DB_bt.UseVisualStyleBackColor = true;
            this.DB_bt.Click += new System.EventHandler(this.DB_btn_Click);
            // 
            // richTextBox
            // 
            this.richTextBox.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.richTextBox.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.richTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.richTextBox.Location = new System.Drawing.Point(12, 12);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox.Size = new System.Drawing.Size(776, 273);
            this.richTextBox.TabIndex = 2;
            this.richTextBox.Text = "";
            // 
            // LED_ON_OFF
            // 
            this.LED_ON_OFF.Location = new System.Drawing.Point(12, 394);
            this.LED_ON_OFF.Name = "LED_ON_OFF";
            this.LED_ON_OFF.Size = new System.Drawing.Size(120, 50);
            this.LED_ON_OFF.TabIndex = 4;
            this.LED_ON_OFF.Text = "전등 ON/OFF";
            this.LED_ON_OFF.UseVisualStyleBackColor = true;
            this.LED_ON_OFF.Click += new System.EventHandler(this.LED_ON_OFF_Click);
            // 
            // MULTITAP_ON_OFF
            // 
            this.MULTITAP_ON_OFF.Location = new System.Drawing.Point(138, 394);
            this.MULTITAP_ON_OFF.Name = "MULTITAP_ON_OFF";
            this.MULTITAP_ON_OFF.Size = new System.Drawing.Size(120, 50);
            this.MULTITAP_ON_OFF.TabIndex = 5;
            this.MULTITAP_ON_OFF.Text = "멀티탭 ON/OFF";
            this.MULTITAP_ON_OFF.UseVisualStyleBackColor = true;
            this.MULTITAP_ON_OFF.Click += new System.EventHandler(this.MULTITAP_ON_OFF_Click);
            // 
            // Tray_Icon
            // 
            this.Tray_Icon.ContextMenuStrip = this.contextMenuStrip;
            this.Tray_Icon.Icon = ((System.Drawing.Icon)(resources.GetObject("Tray_Icon.Icon")));
            this.Tray_Icon.Text = "G.One_Background";
            this.Tray_Icon.Visible = true;
            this.Tray_Icon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Tray_Icon_MouseDoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(163, 48);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.showToolStripMenuItem.Text = "프로그램 활성화";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ToolStrip_Open_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.exitToolStripMenuItem.Text = "프로그램 종료";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ToolStrip_Close_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.MULTITAP_ON_OFF);
            this.Controls.Add(this.LED_ON_OFF);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.DB_bt);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "G.One 키보드 IoT 연결/제어 프로그램";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Tray_Icon_Resize);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button DB_bt;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button LED_ON_OFF;
        private System.Windows.Forms.Button MULTITAP_ON_OFF;
        private System.Windows.Forms.NotifyIcon Tray_Icon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
    }
}

