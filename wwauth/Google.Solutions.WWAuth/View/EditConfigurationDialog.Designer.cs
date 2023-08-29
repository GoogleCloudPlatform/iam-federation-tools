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


using System.Windows.Forms;

namespace Google.Solutions.WWAuth.View
{
    partial class EditConfigurationDialog
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator separatorMenuItem;
            System.Windows.Forms.ToolStripSeparator separatorMenuItem2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditConfigurationDialog));
            this.verifyButton = new Google.Solutions.WWAuth.View.DropDownButton();
            this.testMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.verifyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verifyAsUserMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOutputMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gcloudMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adcMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.authenticateGcloudToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            separatorMenuItem = new System.Windows.Forms.ToolStripSeparator();
            separatorMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.testMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // separatorMenuItem
            // 
            separatorMenuItem.Name = "separatorMenuItem";
            separatorMenuItem.Size = new System.Drawing.Size(301, 6);
            // 
            // separatorMenuItem2
            // 
            separatorMenuItem2.Name = "separatorMenuItem2";
            separatorMenuItem2.Size = new System.Drawing.Size(301, 6);
            // 
            // verifyButton
            // 
            this.verifyButton.Location = new System.Drawing.Point(5, 412);
            this.verifyButton.Menu = this.testMenu;
            this.verifyButton.Name = "verifyButton";
            this.verifyButton.Size = new System.Drawing.Size(75, 23);
            this.verifyButton.TabIndex = 7;
            this.verifyButton.Text = "&Test";
            this.verifyButton.UseVisualStyleBackColor = true;
            this.verifyButton.Click += new System.EventHandler(this.verifyButton_Click);
            // 
            // testMenu
            // 
            this.testMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verifyMenuItem,
            this.verifyAsUserMenuItem,
            separatorMenuItem,
            this.showOutputMenuItem,
            separatorMenuItem2,
            this.gcloudMenuItem,
            this.adcMenuItem});
            this.testMenu.Name = "testMenu";
            this.testMenu.Size = new System.Drawing.Size(305, 126);
            // 
            // verifyMenuItem
            // 
            this.verifyMenuItem.Name = "verifyMenuItem";
            this.verifyMenuItem.Size = new System.Drawing.Size(304, 22);
            this.verifyMenuItem.Text = "&Test configuration";
            this.verifyMenuItem.Click += new System.EventHandler(this.verifyButton_Click);
            // 
            // verifyAsUserMenuItem
            // 
            this.verifyAsUserMenuItem.Name = "verifyAsUserMenuItem";
            this.verifyAsUserMenuItem.Size = new System.Drawing.Size(304, 22);
            this.verifyAsUserMenuItem.Text = "Test configuration &as user...";
            this.verifyAsUserMenuItem.Click += new System.EventHandler(this.verifyAsUserMenuItem_Click);
            // 
            // showOutputMenuItem
            // 
            this.showOutputMenuItem.Name = "showOutputMenuItem";
            this.showOutputMenuItem.Size = new System.Drawing.Size(304, 22);
            this.showOutputMenuItem.Text = "Show &output";
            this.showOutputMenuItem.Click += new System.EventHandler(this.showOutputMenuItem_Click);
            // 
            // gcloudMenuItem
            // 
            this.gcloudMenuItem.Name = "gcloudMenuItem";
            this.gcloudMenuItem.Size = new System.Drawing.Size(304, 22);
            this.gcloudMenuItem.Text = "Use with &gcloud";
            this.gcloudMenuItem.Click += new System.EventHandler(this.gcloudMenuItem_Click);
            // 
            // adcMenuItem
            // 
            this.adcMenuItem.Name = "adcMenuItem";
            this.adcMenuItem.Size = new System.Drawing.Size(304, 22);
            this.adcMenuItem.Text = "Use as application &default credentials (ADC)";
            this.adcMenuItem.Click += new System.EventHandler(this.adcMenuItem_Click);
            // 
            // testConfigurationToolStripMenuItem
            // 
            this.testConfigurationToolStripMenuItem.Name = "testConfigurationToolStripMenuItem";
            this.testConfigurationToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // authenticateGcloudToolStripMenuItem
            // 
            this.authenticateGcloudToolStripMenuItem.Name = "authenticateGcloudToolStripMenuItem";
            this.authenticateGcloudToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // EditConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 441);
            this.Controls.Add(this.verifyButton);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditConfigurationDialog";
            this.ShowIcon = true;
            this.ShowInTaskbar = true;
            this.Text = "EditConfigurationDialog";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.EditConfigurationDialog_HelpButtonClicked);
            this.Controls.SetChildIndex(this.verifyButton, 0);
            this.testMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem testConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem authenticateGcloudToolStripMenuItem;
        private DropDownButton verifyButton;
        private ContextMenuStrip testMenu;
        private ToolStripMenuItem verifyMenuItem;
        private ToolStripMenuItem verifyAsUserMenuItem;
        private ToolStripMenuItem gcloudMenuItem;
        private ToolStripMenuItem adcMenuItem;
        private ToolStripMenuItem showOutputMenuItem;
    }
}