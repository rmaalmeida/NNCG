namespace NNOBB
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.nbFolds = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.CheckBox();
            this.thread = new System.Windows.Forms.CheckBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.nbProf = new System.Windows.Forms.NumericUpDown();
            this.lst_body = new System.Windows.Forms.ListBox();
            this.nbSeed = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.go = new System.Windows.Forms.CheckBox();
            this.maxdist = new System.Windows.Forms.CheckBox();
            this.minBiclass = new System.Windows.Forms.CheckBox();
            this.bound = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.nbFolds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbProf)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbSeed)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(71, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(53, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Vinho";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(53, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Flor";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(130, 10);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(53, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Vidro";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(189, 10);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(53, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Vogal";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // nbFolds
            // 
            this.nbFolds.Location = new System.Drawing.Point(94, 45);
            this.nbFolds.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nbFolds.Name = "nbFolds";
            this.nbFolds.Size = new System.Drawing.Size(74, 20);
            this.nbFolds.TabIndex = 2;
            this.nbFolds.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "No. Folds";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(366, 10);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(53, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "Abortar";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // log
            // 
            this.log.AutoSize = true;
            this.log.Location = new System.Drawing.Point(183, 20);
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(73, 17);
            this.log.TabIndex = 8;
            this.log.Text = "Salvar log";
            this.log.UseVisualStyleBackColor = true;
            // 
            // thread
            // 
            this.thread.AutoSize = true;
            this.thread.Checked = true;
            this.thread.CheckState = System.Windows.Forms.CheckState.Checked;
            this.thread.Location = new System.Drawing.Point(183, 46);
            this.thread.Name = "thread";
            this.thread.Size = new System.Drawing.Size(85, 17);
            this.thread.TabIndex = 8;
            this.thread.Text = "New Thread";
            this.thread.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(248, 10);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(53, 23);
            this.button6.TabIndex = 9;
            this.button6.Text = "Segm.";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(307, 10);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(53, 23);
            this.button7.TabIndex = 10;
            this.button7.Text = "Carro";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Profundidade";
            // 
            // nbProf
            // 
            this.nbProf.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nbProf.Location = new System.Drawing.Point(94, 71);
            this.nbProf.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nbProf.Name = "nbProf";
            this.nbProf.Size = new System.Drawing.Size(74, 20);
            this.nbProf.TabIndex = 2;
            this.nbProf.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // lst_body
            // 
            this.lst_body.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lst_body.FormattingEnabled = true;
            this.lst_body.Location = new System.Drawing.Point(12, 153);
            this.lst_body.Name = "lst_body";
            this.lst_body.Size = new System.Drawing.Size(398, 420);
            this.lst_body.TabIndex = 1;
            // 
            // nbSeed
            // 
            this.nbSeed.Location = new System.Drawing.Point(95, 19);
            this.nbSeed.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nbSeed.Name = "nbSeed";
            this.nbSeed.Size = new System.Drawing.Size(73, 20);
            this.nbSeed.TabIndex = 2;
            this.nbSeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Seed";
            // 
            // go
            // 
            this.go.AutoSize = true;
            this.go.Location = new System.Drawing.Point(183, 72);
            this.go.Name = "go";
            this.go.Size = new System.Drawing.Size(62, 17);
            this.go.TabIndex = 11;
            this.go.Text = "Lookup";
            this.go.UseVisualStyleBackColor = true;
            // 
            // maxdist
            // 
            this.maxdist.AutoSize = true;
            this.maxdist.Location = new System.Drawing.Point(301, 20);
            this.maxdist.Name = "maxdist";
            this.maxdist.Size = new System.Drawing.Size(91, 17);
            this.maxdist.TabIndex = 12;
            this.maxdist.Text = "Max Plan Dist";
            this.maxdist.UseVisualStyleBackColor = true;
            // 
            // minBiclass
            // 
            this.minBiclass.AutoSize = true;
            this.minBiclass.Location = new System.Drawing.Point(301, 46);
            this.minBiclass.Name = "minBiclass";
            this.minBiclass.Size = new System.Drawing.Size(80, 17);
            this.minBiclass.TabIndex = 12;
            this.minBiclass.Text = "Min BiClass";
            this.minBiclass.UseVisualStyleBackColor = true;
            // 
            // bound
            // 
            this.bound.AutoSize = true;
            this.bound.Location = new System.Drawing.Point(301, 72);
            this.bound.Name = "bound";
            this.bound.Size = new System.Drawing.Size(62, 17);
            this.bound.TabIndex = 12;
            this.bound.Text = "Bounds";
            this.bound.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.bound);
            this.groupBox1.Controls.Add(this.nbFolds);
            this.groupBox1.Controls.Add(this.minBiclass);
            this.groupBox1.Controls.Add(this.nbProf);
            this.groupBox1.Controls.Add(this.maxdist);
            this.groupBox1.Controls.Add(this.nbSeed);
            this.groupBox1.Controls.Add(this.go);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.log);
            this.groupBox1.Controls.Add(this.thread);
            this.groupBox1.Location = new System.Drawing.Point(12, 47);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(398, 100);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Configurações";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(422, 573);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.lst_body);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "OBHB";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nbFolds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbProf)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbSeed)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.NumericUpDown nbFolds;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.CheckBox log;
        private System.Windows.Forms.CheckBox thread;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nbProf;
        private System.Windows.Forms.ListBox lst_body;
        private System.Windows.Forms.NumericUpDown nbSeed;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox go;
        private System.Windows.Forms.CheckBox maxdist;
        private System.Windows.Forms.CheckBox minBiclass;
        private System.Windows.Forms.CheckBox bound;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

