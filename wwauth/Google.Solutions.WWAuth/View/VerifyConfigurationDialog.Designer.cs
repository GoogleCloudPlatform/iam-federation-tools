//
// Copyright 2022 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//


namespace Google.Solutions.WWAuth.View
{
    partial class VerifyConfigurationDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.headlineLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.acuireTokenLabel = new System.Windows.Forms.Label();
            this.exchangeTokenLabel = new System.Windows.Forms.Label();
            this.impersonateLabel = new System.Windows.Forms.Label();
            this.acuireTokenPictureBox = new System.Windows.Forms.PictureBox();
            this.exchangeTokenPictureBox = new System.Windows.Forms.PictureBox();
            this.impersonatePictureBox = new System.Windows.Forms.PictureBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.resultPanel = new System.Windows.Forms.Panel();
            this.resultPictureBox = new System.Windows.Forms.PictureBox();
            this.resultLabel = new System.Windows.Forms.Label();
            this.showExternalTokenDetailsLink = new System.Windows.Forms.LinkLabel();
            this.logsButton = new System.Windows.Forms.Button();
            this.showServiceAccountTokenDetailsLink = new System.Windows.Forms.LinkLabel();
            this.showStsTokenDetailsLink = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.acuireTokenPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.exchangeTokenPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.impersonatePictureBox)).BeginInit();
            this.resultPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.headlineLabel);
            this.groupBox1.Location = new System.Drawing.Point(-3, -7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(501, 80);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // headlineLabel
            // 
            this.headlineLabel.AutoSize = true;
            this.headlineLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headlineLabel.Location = new System.Drawing.Point(18, 25);
            this.headlineLabel.Name = "headlineLabel";
            this.headlineLabel.Size = new System.Drawing.Size(185, 30);
            this.headlineLabel.TabIndex = 10;
            this.headlineLabel.Text = "Test configuration";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(309, 276);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // acuireTokenLabel
            // 
            this.acuireTokenLabel.AutoSize = true;
            this.acuireTokenLabel.Location = new System.Drawing.Point(21, 99);
            this.acuireTokenLabel.Name = "acuireTokenLabel";
            this.acuireTokenLabel.Size = new System.Drawing.Size(182, 13);
            this.acuireTokenLabel.TabIndex = 6;
            this.acuireTokenLabel.Text = "Acquire token from identity provider...";
            // 
            // exchangeTokenLabel
            // 
            this.exchangeTokenLabel.AutoSize = true;
            this.exchangeTokenLabel.Location = new System.Drawing.Point(21, 124);
            this.exchangeTokenLabel.Name = "exchangeTokenLabel";
            this.exchangeTokenLabel.Size = new System.Drawing.Size(138, 13);
            this.exchangeTokenLabel.TabIndex = 6;
            this.exchangeTokenLabel.Text = "Obtain Google STS token...";
            // 
            // impersonateLabel
            // 
            this.impersonateLabel.AutoSize = true;
            this.impersonateLabel.Location = new System.Drawing.Point(21, 149);
            this.impersonateLabel.Name = "impersonateLabel";
            this.impersonateLabel.Size = new System.Drawing.Size(153, 13);
            this.impersonateLabel.TabIndex = 6;
            this.impersonateLabel.Text = "Impersonate service account...";
            // 
            // acuireTokenPictureBox
            // 
            this.acuireTokenPictureBox.Location = new System.Drawing.Point(204, 97);
            this.acuireTokenPictureBox.Name = "acuireTokenPictureBox";
            this.acuireTokenPictureBox.Size = new System.Drawing.Size(16, 16);
            this.acuireTokenPictureBox.TabIndex = 8;
            this.acuireTokenPictureBox.TabStop = false;
            // 
            // exchangeTokenPictureBox
            // 
            this.exchangeTokenPictureBox.Location = new System.Drawing.Point(204, 122);
            this.exchangeTokenPictureBox.Name = "exchangeTokenPictureBox";
            this.exchangeTokenPictureBox.Size = new System.Drawing.Size(16, 16);
            this.exchangeTokenPictureBox.TabIndex = 8;
            this.exchangeTokenPictureBox.TabStop = false;
            // 
            // impersonatePictureBox
            // 
            this.impersonatePictureBox.Location = new System.Drawing.Point(204, 147);
            this.impersonatePictureBox.Name = "impersonatePictureBox";
            this.impersonatePictureBox.Size = new System.Drawing.Size(16, 16);
            this.impersonatePictureBox.TabIndex = 8;
            this.impersonatePictureBox.TabStop = false;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(390, 276);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // resultPanel
            // 
            this.resultPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultPanel.BackColor = System.Drawing.SystemColors.Info;
            this.resultPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.resultPanel.Controls.Add(this.resultPictureBox);
            this.resultPanel.Controls.Add(this.resultLabel);
            this.resultPanel.Location = new System.Drawing.Point(24, 184);
            this.resultPanel.Name = "resultPanel";
            this.resultPanel.Size = new System.Drawing.Size(441, 86);
            this.resultPanel.TabIndex = 9;
            // 
            // resultPictureBox
            // 
            this.resultPictureBox.Location = new System.Drawing.Point(8, 8);
            this.resultPictureBox.Name = "resultPictureBox";
            this.resultPictureBox.Size = new System.Drawing.Size(16, 16);
            this.resultPictureBox.TabIndex = 9;
            this.resultPictureBox.TabStop = false;
            // 
            // resultLabel
            // 
            this.resultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultLabel.AutoEllipsis = true;
            this.resultLabel.Location = new System.Drawing.Point(30, 8);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(404, 66);
            this.resultLabel.TabIndex = 0;
            this.resultLabel.Text = "...";
            // 
            // showExternalTokenDetailsLink
            // 
            this.showExternalTokenDetailsLink.AutoSize = true;
            this.showExternalTokenDetailsLink.Location = new System.Drawing.Point(226, 99);
            this.showExternalTokenDetailsLink.Name = "showExternalTokenDetailsLink";
            this.showExternalTokenDetailsLink.Size = new System.Drawing.Size(67, 13);
            this.showExternalTokenDetailsLink.TabIndex = 10;
            this.showExternalTokenDetailsLink.TabStop = true;
            this.showExternalTokenDetailsLink.Text = "Show details";
            // 
            // logsButton
            // 
            this.logsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.logsButton.Location = new System.Drawing.Point(24, 276);
            this.logsButton.Name = "logsButton";
            this.logsButton.Size = new System.Drawing.Size(75, 23);
            this.logsButton.TabIndex = 0;
            this.logsButton.Text = "&Logs";
            this.logsButton.UseVisualStyleBackColor = true;
            // 
            // showServiceAccountTokenDetailsLink
            // 
            this.showServiceAccountTokenDetailsLink.AutoSize = true;
            this.showServiceAccountTokenDetailsLink.Location = new System.Drawing.Point(226, 149);
            this.showServiceAccountTokenDetailsLink.Name = "showServiceAccountTokenDetailsLink";
            this.showServiceAccountTokenDetailsLink.Size = new System.Drawing.Size(67, 13);
            this.showServiceAccountTokenDetailsLink.TabIndex = 11;
            this.showServiceAccountTokenDetailsLink.TabStop = true;
            this.showServiceAccountTokenDetailsLink.Text = "Show details";
            // 
            // showStslTokenDetailsLink
            // 
            this.showStsTokenDetailsLink.AutoSize = true;
            this.showStsTokenDetailsLink.Location = new System.Drawing.Point(226, 125);
            this.showStsTokenDetailsLink.Name = "showStslTokenDetailsLink";
            this.showStsTokenDetailsLink.Size = new System.Drawing.Size(67, 13);
            this.showStsTokenDetailsLink.TabIndex = 12;
            this.showStsTokenDetailsLink.TabStop = true;
            this.showStsTokenDetailsLink.Text = "Show details";
            // 
            // VerifyConfigurationDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96, 96);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 311);
            this.Controls.Add(this.showStsTokenDetailsLink);
            this.Controls.Add(this.showServiceAccountTokenDetailsLink);
            this.Controls.Add(this.showExternalTokenDetailsLink);
            this.Controls.Add(this.resultPanel);
            this.Controls.Add(this.impersonatePictureBox);
            this.Controls.Add(this.exchangeTokenPictureBox);
            this.Controls.Add(this.acuireTokenPictureBox);
            this.Controls.Add(this.impersonateLabel);
            this.Controls.Add(this.exchangeTokenLabel);
            this.Controls.Add(this.acuireTokenLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.logsButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "VerifyConfigurationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Diagnose configuration";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.acuireTokenPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.exchangeTokenPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.impersonatePictureBox)).EndInit();
            this.resultPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.resultPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label headlineLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label acuireTokenLabel;
        private System.Windows.Forms.Label exchangeTokenLabel;
        private System.Windows.Forms.Label impersonateLabel;
        private System.Windows.Forms.PictureBox acuireTokenPictureBox;
        private System.Windows.Forms.PictureBox exchangeTokenPictureBox;
        private System.Windows.Forms.PictureBox impersonatePictureBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel resultPanel;
        private System.Windows.Forms.PictureBox resultPictureBox;
        private System.Windows.Forms.Label resultLabel;
        private System.Windows.Forms.LinkLabel showExternalTokenDetailsLink;
        private System.Windows.Forms.Button logsButton;
        private System.Windows.Forms.LinkLabel showServiceAccountTokenDetailsLink;
        private System.Windows.Forms.LinkLabel showStsTokenDetailsLink;
    }
}