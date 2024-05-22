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
    partial class AdfsConfigurationSheet
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdfsConfigurationSheet));
            this.asfsBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.adfsIssuerUriText = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.adfsIssuerUriLabel = new System.Windows.Forms.Label();
            this.rpBox = new System.Windows.Forms.GroupBox();
            this.browseCertificateButton = new Google.Solutions.WWAuth.View.DropDownButton();
            this.certificateMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewCertificateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.signRequestCheckBox = new System.Windows.Forms.CheckBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.protocolComboBox = new System.Windows.Forms.ComboBox();
            this.acsUrlLabel = new System.Windows.Forms.Label();
            this.clientIdLabel = new System.Windows.Forms.Label();
            this.protocolLabel = new System.Windows.Forms.Label();
            this.rpIdLabel = new System.Windows.Forms.Label();
            this.acsUrlTextBox = new System.Windows.Forms.TextBox();
            this.clientIdTextBox = new System.Windows.Forms.TextBox();
            this.signingCertificateTextBox = new System.Windows.Forms.TextBox();
            this.rpIdTextBox = new System.Windows.Forms.TextBox();
            this.asfsBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.rpBox.SuspendLayout();
            this.certificateMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // asfsBox
            // 
            this.asfsBox.Controls.Add(this.label1);
            this.asfsBox.Controls.Add(this.adfsIssuerUriText);
            this.asfsBox.Controls.Add(this.pictureBox1);
            this.asfsBox.Controls.Add(this.adfsIssuerUriLabel);
            this.asfsBox.Location = new System.Drawing.Point(3, 3);
            this.asfsBox.Name = "asfsBox";
            this.asfsBox.Size = new System.Drawing.Size(344, 110);
            this.asfsBox.TabIndex = 0;
            this.asfsBox.TabStop = false;
            this.asfsBox.Text = "AD FS Server";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(63, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(181, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Example: https://example.com/adfs/";
            // 
            // adfsIssuerUriText
            // 
            this.adfsIssuerUriText.Location = new System.Drawing.Point(66, 47);
            this.adfsIssuerUriText.Name = "adfsIssuerUriText";
            this.adfsIssuerUriText.Size = new System.Drawing.Size(257, 20);
            this.adfsIssuerUriText.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(15, 31);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // adfsIssuerUriLabel
            // 
            this.adfsIssuerUriLabel.AutoSize = true;
            this.adfsIssuerUriLabel.Location = new System.Drawing.Point(63, 31);
            this.adfsIssuerUriLabel.Name = "adfsIssuerUriLabel";
            this.adfsIssuerUriLabel.Size = new System.Drawing.Size(138, 13);
            this.adfsIssuerUriLabel.TabIndex = 0;
            this.adfsIssuerUriLabel.Text = "Issuer URI of AD FS server:";
            // 
            // rpBox
            // 
            this.rpBox.Controls.Add(this.browseCertificateButton);
            this.rpBox.Controls.Add(this.signRequestCheckBox);
            this.rpBox.Controls.Add(this.pictureBox2);
            this.rpBox.Controls.Add(this.protocolComboBox);
            this.rpBox.Controls.Add(this.acsUrlLabel);
            this.rpBox.Controls.Add(this.clientIdLabel);
            this.rpBox.Controls.Add(this.protocolLabel);
            this.rpBox.Controls.Add(this.rpIdLabel);
            this.rpBox.Controls.Add(this.acsUrlTextBox);
            this.rpBox.Controls.Add(this.clientIdTextBox);
            this.rpBox.Controls.Add(this.signingCertificateTextBox);
            this.rpBox.Controls.Add(this.rpIdTextBox);
            this.rpBox.Location = new System.Drawing.Point(3, 119);
            this.rpBox.Name = "rpBox";
            this.rpBox.Size = new System.Drawing.Size(344, 246);
            this.rpBox.TabIndex = 1;
            this.rpBox.TabStop = false;
            this.rpBox.Text = "AD FS relying party";
            // 
            // browseCertificateButton
            // 
            this.browseCertificateButton.Location = new System.Drawing.Point(236, 198);
            this.browseCertificateButton.Menu = this.certificateMenuStrip;
            this.browseCertificateButton.Name = "browseCertificateButton";
            this.browseCertificateButton.Size = new System.Drawing.Size(87, 23);
            this.browseCertificateButton.TabIndex = 7;
            this.browseCertificateButton.Text = "Browse...";
            this.browseCertificateButton.UseVisualStyleBackColor = true;
            this.browseCertificateButton.Click += new System.EventHandler(this.browseCertificateButton_Click);
            // 
            // certificateMenuStrip
            // 
            this.certificateMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewCertificateMenuItem});
            this.certificateMenuStrip.Name = "certificateMenuStrip";
            this.certificateMenuStrip.Size = new System.Drawing.Size(155, 26);
            // 
            // viewCertificateMenuItem
            // 
            this.viewCertificateMenuItem.Name = "viewCertificateMenuItem";
            this.viewCertificateMenuItem.Size = new System.Drawing.Size(154, 22);
            this.viewCertificateMenuItem.Text = "&View certificate";
            this.viewCertificateMenuItem.Click += new System.EventHandler(this.viewCertificateMenuItem_Click);
            // 
            // signRequestCheckBox
            // 
            this.signRequestCheckBox.AutoSize = true;
            this.signRequestCheckBox.Location = new System.Drawing.Point(66, 181);
            this.signRequestCheckBox.Name = "signRequestCheckBox";
            this.signRequestCheckBox.Size = new System.Drawing.Size(170, 17);
            this.signRequestCheckBox.TabIndex = 5;
            this.signRequestCheckBox.Text = "Sign requests using certificate:";
            this.signRequestCheckBox.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Google.Solutions.WWAuth.Properties.Resources.RelyingParty_32;
            this.pictureBox2.Location = new System.Drawing.Point(15, 31);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(32, 32);
            this.pictureBox2.TabIndex = 4;
            this.pictureBox2.TabStop = false;
            // 
            // protocolComboBox
            // 
            this.protocolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.protocolComboBox.FormattingEnabled = true;
            this.protocolComboBox.Location = new System.Drawing.Point(66, 47);
            this.protocolComboBox.Name = "protocolComboBox";
            this.protocolComboBox.Size = new System.Drawing.Size(121, 21);
            this.protocolComboBox.TabIndex = 1;
            // 
            // acsUrlLabel
            // 
            this.acsUrlLabel.AutoSize = true;
            this.acsUrlLabel.Location = new System.Drawing.Point(63, 129);
            this.acsUrlLabel.Name = "acsUrlLabel";
            this.acsUrlLabel.Size = new System.Drawing.Size(167, 13);
            this.acsUrlLabel.TabIndex = 1;
            this.acsUrlLabel.Text = "Assertion Consumer Service URL:";
            // 
            // clientIdLabel
            // 
            this.clientIdLabel.AutoSize = true;
            this.clientIdLabel.Location = new System.Drawing.Point(63, 129);
            this.clientIdLabel.Name = "clientIdLabel";
            this.clientIdLabel.Size = new System.Drawing.Size(50, 13);
            this.clientIdLabel.TabIndex = 1;
            this.clientIdLabel.Text = "Client ID:";
            // 
            // protocolLabel
            // 
            this.protocolLabel.AutoSize = true;
            this.protocolLabel.Location = new System.Drawing.Point(63, 31);
            this.protocolLabel.Name = "protocolLabel";
            this.protocolLabel.Size = new System.Drawing.Size(49, 13);
            this.protocolLabel.TabIndex = 1;
            this.protocolLabel.Text = "Protocol:";
            // 
            // rpIdLabel
            // 
            this.rpIdLabel.AutoSize = true;
            this.rpIdLabel.Location = new System.Drawing.Point(63, 81);
            this.rpIdLabel.Name = "rpIdLabel";
            this.rpIdLabel.Size = new System.Drawing.Size(85, 13);
            this.rpIdLabel.TabIndex = 1;
            this.rpIdLabel.Text = "Relying party ID:";
            // 
            // acsUrlTextBox
            // 
            this.acsUrlTextBox.Location = new System.Drawing.Point(66, 145);
            this.acsUrlTextBox.Name = "acsUrlTextBox";
            this.acsUrlTextBox.Size = new System.Drawing.Size(257, 20);
            this.acsUrlTextBox.TabIndex = 3;
            // 
            // clientIdTextBox
            // 
            this.clientIdTextBox.Location = new System.Drawing.Point(66, 145);
            this.clientIdTextBox.Name = "clientIdTextBox";
            this.clientIdTextBox.Size = new System.Drawing.Size(257, 20);
            this.clientIdTextBox.TabIndex = 3;
            // 
            // signingCertificateTextBox
            // 
            this.signingCertificateTextBox.Location = new System.Drawing.Point(87, 200);
            this.signingCertificateTextBox.Name = "signingCertificateTextBox";
            this.signingCertificateTextBox.ReadOnly = true;
            this.signingCertificateTextBox.Size = new System.Drawing.Size(143, 20);
            this.signingCertificateTextBox.TabIndex = 2;
            // 
            // rpIdTextBox
            // 
            this.rpIdTextBox.Location = new System.Drawing.Point(66, 97);
            this.rpIdTextBox.Name = "rpIdTextBox";
            this.rpIdTextBox.Size = new System.Drawing.Size(257, 20);
            this.rpIdTextBox.TabIndex = 2;
            // 
            // AdfsConfigurationSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96, 96);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.rpBox);
            this.Controls.Add(this.asfsBox);
            this.Name = "AdfsConfigurationSheet";
            this.Size = new System.Drawing.Size(350, 400);
            this.asfsBox.ResumeLayout(false);
            this.asfsBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.rpBox.ResumeLayout(false);
            this.rpBox.PerformLayout();
            this.certificateMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox asfsBox;
        private System.Windows.Forms.Label adfsIssuerUriLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox adfsIssuerUriText;
        private System.Windows.Forms.GroupBox rpBox;
        private System.Windows.Forms.Label rpIdLabel;
        private System.Windows.Forms.TextBox rpIdTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label clientIdLabel;
        private System.Windows.Forms.Label protocolLabel;
        private System.Windows.Forms.TextBox clientIdTextBox;
        private System.Windows.Forms.ComboBox protocolComboBox;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label acsUrlLabel;
        private System.Windows.Forms.TextBox acsUrlTextBox;
        private DropDownButton browseCertificateButton;
        private System.Windows.Forms.CheckBox signRequestCheckBox;
        private System.Windows.Forms.TextBox signingCertificateTextBox;
        private System.Windows.Forms.ContextMenuStrip certificateMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem viewCertificateMenuItem;
    }
}
