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
    partial class WorkloadIdentityConfigurationSheet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkloadIdentityConfigurationSheet));
            this.poolGroup = new System.Windows.Forms.GroupBox();
            this.audienceTextBox = new System.Windows.Forms.TextBox();
            this.audienceLabel = new System.Windows.Forms.Label();
            this.providerIdLabel = new System.Windows.Forms.Label();
            this.poolIsLabel = new System.Windows.Forms.Label();
            this.projectNumberLabel = new System.Windows.Forms.Label();
            this.providerIdTextBox = new System.Windows.Forms.TextBox();
            this.poolIdTextBox = new System.Windows.Forms.TextBox();
            this.projectNumberTextBox = new System.Windows.Forms.TextBox();
            this.poolPictureBox = new System.Windows.Forms.PictureBox();
            this.poolHelpLabel = new System.Windows.Forms.Label();
            this.impersonateSaCheckBox = new System.Windows.Forms.CheckBox();
            this.serviceAccountTextBox = new System.Windows.Forms.TextBox();
            this.emailAddressLabel = new System.Windows.Forms.Label();
            this.poolGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.poolPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // poolGroup
            // 
            this.poolGroup.Controls.Add(this.emailAddressLabel);
            this.poolGroup.Controls.Add(this.serviceAccountTextBox);
            this.poolGroup.Controls.Add(this.impersonateSaCheckBox);
            this.poolGroup.Controls.Add(this.audienceTextBox);
            this.poolGroup.Controls.Add(this.audienceLabel);
            this.poolGroup.Controls.Add(this.providerIdLabel);
            this.poolGroup.Controls.Add(this.poolIsLabel);
            this.poolGroup.Controls.Add(this.projectNumberLabel);
            this.poolGroup.Controls.Add(this.providerIdTextBox);
            this.poolGroup.Controls.Add(this.poolIdTextBox);
            this.poolGroup.Controls.Add(this.projectNumberTextBox);
            this.poolGroup.Controls.Add(this.poolPictureBox);
            this.poolGroup.Controls.Add(this.poolHelpLabel);
            this.poolGroup.Location = new System.Drawing.Point(3, 3);
            this.poolGroup.Name = "poolGroup";
            this.poolGroup.Size = new System.Drawing.Size(344, 290);
            this.poolGroup.TabIndex = 0;
            this.poolGroup.TabStop = false;
            this.poolGroup.Text = "Workload identity federation";
            // 
            // audienceTextBox
            // 
            this.audienceTextBox.Location = new System.Drawing.Point(66, 154);
            this.audienceTextBox.Name = "audienceTextBox";
            this.audienceTextBox.ReadOnly = true;
            this.audienceTextBox.Size = new System.Drawing.Size(257, 20);
            this.audienceTextBox.TabIndex = 4;
            // 
            // audienceLabel
            // 
            this.audienceLabel.AutoSize = true;
            this.audienceLabel.Location = new System.Drawing.Point(63, 137);
            this.audienceLabel.Name = "audienceLabel";
            this.audienceLabel.Size = new System.Drawing.Size(55, 13);
            this.audienceLabel.TabIndex = 3;
            this.audienceLabel.Text = "Audience:";
            // 
            // providerIdLabel
            // 
            this.providerIdLabel.AutoSize = true;
            this.providerIdLabel.Location = new System.Drawing.Point(63, 111);
            this.providerIdLabel.Name = "providerIdLabel";
            this.providerIdLabel.Size = new System.Drawing.Size(63, 13);
            this.providerIdLabel.TabIndex = 3;
            this.providerIdLabel.Text = "Provider ID:";
            // 
            // poolIsLabel
            // 
            this.poolIsLabel.AutoSize = true;
            this.poolIsLabel.Location = new System.Drawing.Point(63, 85);
            this.poolIsLabel.Name = "poolIsLabel";
            this.poolIsLabel.Size = new System.Drawing.Size(45, 13);
            this.poolIsLabel.TabIndex = 3;
            this.poolIsLabel.Text = "Pool ID:";
            // 
            // projectNumberLabel
            // 
            this.projectNumberLabel.AutoSize = true;
            this.projectNumberLabel.Location = new System.Drawing.Point(63, 58);
            this.projectNumberLabel.Name = "projectNumberLabel";
            this.projectNumberLabel.Size = new System.Drawing.Size(81, 13);
            this.projectNumberLabel.TabIndex = 3;
            this.projectNumberLabel.Text = "Project number:";
            // 
            // providerIdTextBox
            // 
            this.providerIdTextBox.Location = new System.Drawing.Point(147, 108);
            this.providerIdTextBox.Name = "providerIdTextBox";
            this.providerIdTextBox.Size = new System.Drawing.Size(176, 20);
            this.providerIdTextBox.TabIndex = 2;
            // 
            // poolIdTextBox
            // 
            this.poolIdTextBox.Location = new System.Drawing.Point(147, 82);
            this.poolIdTextBox.Name = "poolIdTextBox";
            this.poolIdTextBox.Size = new System.Drawing.Size(176, 20);
            this.poolIdTextBox.TabIndex = 1;
            // 
            // projectNumberTextBox
            // 
            this.projectNumberTextBox.Location = new System.Drawing.Point(147, 56);
            this.projectNumberTextBox.Name = "projectNumberTextBox";
            this.projectNumberTextBox.Size = new System.Drawing.Size(176, 20);
            this.projectNumberTextBox.TabIndex = 0;
            // 
            // poolPictureBox
            // 
            this.poolPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("poolPictureBox.Image")));
            this.poolPictureBox.Location = new System.Drawing.Point(15, 31);
            this.poolPictureBox.Name = "poolPictureBox";
            this.poolPictureBox.Size = new System.Drawing.Size(32, 32);
            this.poolPictureBox.TabIndex = 1;
            this.poolPictureBox.TabStop = false;
            // 
            // poolHelpLabel
            // 
            this.poolHelpLabel.AutoSize = true;
            this.poolHelpLabel.Location = new System.Drawing.Point(63, 31);
            this.poolHelpLabel.Name = "poolHelpLabel";
            this.poolHelpLabel.Size = new System.Drawing.Size(196, 13);
            this.poolHelpLabel.TabIndex = 0;
            this.poolHelpLabel.Text = "Use workload identity pool and provider:";
            // 
            // impersonateSaCheckBox
            // 
            this.impersonateSaCheckBox.AutoSize = true;
            this.impersonateSaCheckBox.Location = new System.Drawing.Point(66, 206);
            this.impersonateSaCheckBox.Name = "impersonateSaCheckBox";
            this.impersonateSaCheckBox.Size = new System.Drawing.Size(163, 17);
            this.impersonateSaCheckBox.TabIndex = 5;
            this.impersonateSaCheckBox.Text = "Impersonate service account";
            this.impersonateSaCheckBox.UseVisualStyleBackColor = true;
            // 
            // serviceAccountTextBox
            // 
            this.serviceAccountTextBox.Location = new System.Drawing.Point(84, 250);
            this.serviceAccountTextBox.Name = "serviceAccountTextBox";
            this.serviceAccountTextBox.Size = new System.Drawing.Size(239, 20);
            this.serviceAccountTextBox.TabIndex = 6;
            // 
            // emailAddressLabel
            // 
            this.emailAddressLabel.AutoSize = true;
            this.emailAddressLabel.Location = new System.Drawing.Point(81, 234);
            this.emailAddressLabel.Name = "emailAddressLabel";
            this.emailAddressLabel.Size = new System.Drawing.Size(75, 13);
            this.emailAddressLabel.TabIndex = 7;
            this.emailAddressLabel.Text = "Email address:";
            // 
            // WorkloadIdentityConfigurationSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.poolGroup);
            this.Name = "WorkloadIdentityConfigurationSheet";
            this.Size = new System.Drawing.Size(350, 400);
            this.poolGroup.ResumeLayout(false);
            this.poolGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.poolPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox poolGroup;
        private System.Windows.Forms.Label poolHelpLabel;
        private System.Windows.Forms.PictureBox poolPictureBox;
        private System.Windows.Forms.Label providerIdLabel;
        private System.Windows.Forms.Label poolIsLabel;
        private System.Windows.Forms.Label projectNumberLabel;
        private System.Windows.Forms.TextBox providerIdTextBox;
        private System.Windows.Forms.TextBox poolIdTextBox;
        private System.Windows.Forms.TextBox projectNumberTextBox;
        private System.Windows.Forms.TextBox audienceTextBox;
        private System.Windows.Forms.Label audienceLabel;
        private System.Windows.Forms.Label emailAddressLabel;
        private System.Windows.Forms.TextBox serviceAccountTextBox;
        private System.Windows.Forms.CheckBox impersonateSaCheckBox;
    }
}
