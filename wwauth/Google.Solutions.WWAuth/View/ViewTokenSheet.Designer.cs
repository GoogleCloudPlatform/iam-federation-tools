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
    partial class ViewTokenSheet
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
            this.attributesList = new System.Windows.Forms.ListView();
            this.keyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.valueHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.issuerLabel = new System.Windows.Forms.Label();
            this.issuerTextBox = new System.Windows.Forms.TextBox();
            this.audienceTextBox = new System.Windows.Forms.TextBox();
            this.audienceLabel = new System.Windows.Forms.Label();
            this.expiryTextBox = new System.Windows.Forms.TextBox();
            this.expiryLabel = new System.Windows.Forms.Label();
            this.attributesLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // attributesList
            // 
            this.attributesList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.keyHeader,
            this.valueHeader});
            this.attributesList.FullRowSelect = true;
            this.attributesList.GridLines = true;
            this.attributesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.attributesList.HideSelection = false;
            this.attributesList.Location = new System.Drawing.Point(6, 110);
            this.attributesList.MultiSelect = false;
            this.attributesList.Name = "attributesList";
            this.attributesList.Size = new System.Drawing.Size(380, 276);
            this.attributesList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.attributesList.TabIndex = 0;
            this.attributesList.UseCompatibleStateImageBehavior = false;
            this.attributesList.View = System.Windows.Forms.View.Details;
            // 
            // keyHeader
            // 
            this.keyHeader.Text = "Key";
            this.keyHeader.Width = 130;
            // 
            // valueHeader
            // 
            this.valueHeader.Text = "Value";
            this.valueHeader.Width = 200;
            // 
            // issuerLabel
            // 
            this.issuerLabel.AutoSize = true;
            this.issuerLabel.Location = new System.Drawing.Point(3, 17);
            this.issuerLabel.Name = "issuerLabel";
            this.issuerLabel.Size = new System.Drawing.Size(38, 13);
            this.issuerLabel.TabIndex = 1;
            this.issuerLabel.Text = "Issuer:";
            // 
            // issuerTextBox
            // 
            this.issuerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.issuerTextBox.Location = new System.Drawing.Point(66, 14);
            this.issuerTextBox.Name = "issuerTextBox";
            this.issuerTextBox.ReadOnly = true;
            this.issuerTextBox.Size = new System.Drawing.Size(320, 20);
            this.issuerTextBox.TabIndex = 2;
            // 
            // audienceTextBox
            // 
            this.audienceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.audienceTextBox.Location = new System.Drawing.Point(66, 40);
            this.audienceTextBox.Name = "audienceTextBox";
            this.audienceTextBox.ReadOnly = true;
            this.audienceTextBox.Size = new System.Drawing.Size(320, 20);
            this.audienceTextBox.TabIndex = 2;
            // 
            // audienceLabel
            // 
            this.audienceLabel.AutoSize = true;
            this.audienceLabel.Location = new System.Drawing.Point(3, 43);
            this.audienceLabel.Name = "audienceLabel";
            this.audienceLabel.Size = new System.Drawing.Size(55, 13);
            this.audienceLabel.TabIndex = 1;
            this.audienceLabel.Text = "Audience:";
            // 
            // expiryTextBox
            // 
            this.expiryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.expiryTextBox.Location = new System.Drawing.Point(66, 66);
            this.expiryTextBox.Name = "expiryTextBox";
            this.expiryTextBox.ReadOnly = true;
            this.expiryTextBox.Size = new System.Drawing.Size(320, 20);
            this.expiryTextBox.TabIndex = 2;
            // 
            // expiryLabel
            // 
            this.expiryLabel.AutoSize = true;
            this.expiryLabel.Location = new System.Drawing.Point(3, 69);
            this.expiryLabel.Name = "expiryLabel";
            this.expiryLabel.Size = new System.Drawing.Size(38, 13);
            this.expiryLabel.TabIndex = 1;
            this.expiryLabel.Text = "Expiry:";
            // 
            // attributesLabel
            // 
            this.attributesLabel.AutoSize = true;
            this.attributesLabel.Location = new System.Drawing.Point(3, 94);
            this.attributesLabel.Name = "attributesLabel";
            this.attributesLabel.Size = new System.Drawing.Size(54, 13);
            this.attributesLabel.TabIndex = 1;
            this.attributesLabel.Text = "Attributes:";
            // 
            // ViewTokenSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.expiryTextBox);
            this.Controls.Add(this.audienceTextBox);
            this.Controls.Add(this.issuerTextBox);
            this.Controls.Add(this.attributesLabel);
            this.Controls.Add(this.expiryLabel);
            this.Controls.Add(this.audienceLabel);
            this.Controls.Add(this.issuerLabel);
            this.Controls.Add(this.attributesList);
            this.Name = "ViewTokenSheet";
            this.Size = new System.Drawing.Size(400, 400);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView attributesList;
        private System.Windows.Forms.ColumnHeader keyHeader;
        private System.Windows.Forms.ColumnHeader valueHeader;
        private System.Windows.Forms.Label issuerLabel;
        private System.Windows.Forms.TextBox issuerTextBox;
        private System.Windows.Forms.TextBox audienceTextBox;
        private System.Windows.Forms.TextBox expiryTextBox;
        private System.Windows.Forms.Label expiryLabel;
        private System.Windows.Forms.Label attributesLabel;
        private System.Windows.Forms.Label audienceLabel;
    }
}
