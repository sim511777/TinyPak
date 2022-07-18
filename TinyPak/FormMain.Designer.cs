namespace TinyPak {
    partial class FormMain {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent() {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.pakToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unpakToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tbxConsole = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pakToolStripMenuItem,
            this.unpakToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // pakToolStripMenuItem
            // 
            this.pakToolStripMenuItem.Name = "pakToolStripMenuItem";
            this.pakToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.pakToolStripMenuItem.Text = "Pak";
            this.pakToolStripMenuItem.Click += new System.EventHandler(this.pakToolStripMenuItem_Click);
            // 
            // unpakToolStripMenuItem
            // 
            this.unpakToolStripMenuItem.Name = "unpakToolStripMenuItem";
            this.unpakToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.unpakToolStripMenuItem.Text = "Unpak";
            this.unpakToolStripMenuItem.Click += new System.EventHandler(this.unpakToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // tbxConsole
            // 
            this.tbxConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxConsole.Font = new System.Drawing.Font("굴림체", 9F);
            this.tbxConsole.Location = new System.Drawing.Point(0, 24);
            this.tbxConsole.Multiline = true;
            this.tbxConsole.Name = "tbxConsole";
            this.tbxConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxConsole.Size = new System.Drawing.Size(800, 426);
            this.tbxConsole.TabIndex = 5;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tbxConsole);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem pakToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unpakToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox tbxConsole;
    }
}

