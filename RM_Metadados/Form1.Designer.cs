namespace RM_Metadados
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            pictureBox1 = new PictureBox();
            btnSelecionar = new Button();
            btnRemoverMetadados = new Button();
            btnDownload = new Button();
            lblInfo = new Label();
            label1 = new Label();
            btnDownloadMetadados = new Button();
            dataGridViewMetadados = new DataGridView();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMetadados).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(12, 36);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(618, 278);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // btnSelecionar
            // 
            btnSelecionar.Location = new Point(22, 319);
            btnSelecionar.Name = "btnSelecionar";
            btnSelecionar.Size = new Size(139, 34);
            btnSelecionar.TabIndex = 1;
            btnSelecionar.Text = "Select";
            btnSelecionar.UseVisualStyleBackColor = true;
            btnSelecionar.Click += btnSelecionar_Click;
            // 
            // btnRemoverMetadados
            // 
            btnRemoverMetadados.Location = new Point(167, 319);
            btnRemoverMetadados.Name = "btnRemoverMetadados";
            btnRemoverMetadados.Size = new Size(143, 34);
            btnRemoverMetadados.TabIndex = 2;
            btnRemoverMetadados.Text = "Remove";
            btnRemoverMetadados.UseVisualStyleBackColor = true;
            btnRemoverMetadados.Click += btnRemoverMetadados_Click;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(316, 320);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(143, 34);
            btnDownload.TabIndex = 3;
            btnDownload.Text = "Download";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(12, 9);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(146, 15);
            lblInfo.TabIndex = 4;
            lblInfo.Text = "Removedor de Metadados";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(292, 36);
            label1.Name = "label1";
            label1.Size = new Size(0, 15);
            label1.TabIndex = 5;
            // 
            // btnDownloadMetadados
            // 
            btnDownloadMetadados.Location = new Point(465, 320);
            btnDownloadMetadados.Name = "btnDownloadMetadados";
            btnDownloadMetadados.Size = new Size(143, 34);
            btnDownloadMetadados.TabIndex = 6;
            btnDownloadMetadados.Text = "Down Met";
            btnDownloadMetadados.UseVisualStyleBackColor = true;
            btnDownloadMetadados.Click += btnDownloadMetadados_Click;
            // 
            // dataGridViewMetadados
            // 
            dataGridViewMetadados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewMetadados.Location = new Point(12, 361);
            dataGridViewMetadados.Name = "dataGridViewMetadados";
            dataGridViewMetadados.Size = new Size(618, 213);
            dataGridViewMetadados.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(506, 9);
            label2.Name = "label2";
            label2.Size = new Size(125, 15);
            label2.TabIndex = 8;
            label2.Text = "Trillobit3s Games Indie";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(642, 586);
            Controls.Add(label2);
            Controls.Add(dataGridViewMetadados);
            Controls.Add(btnDownloadMetadados);
            Controls.Add(label1);
            Controls.Add(lblInfo);
            Controls.Add(btnDownload);
            Controls.Add(btnRemoverMetadados);
            Controls.Add(btnSelecionar);
            Controls.Add(pictureBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Remove Metadata";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewMetadados).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnSelecionar;
        private Button btnRemoverMetadados;
        private Button btnDownload;
        private Label lblInfo;
        private Label label1;
        private Button btnDownloadMetadados;
        private DataGridView dataGridViewMetadados;
        private Label label2;
    }
}
