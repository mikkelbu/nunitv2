﻿// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;
using System.Windows.Forms;

namespace NUnit.ProjectEditor.ViewElements
{
    public class MessageDisplay : IMessageDisplay
    {
        private string caption;

        public MessageDisplay(string caption)
        {
            this.caption = caption;
        }

        public void Error(string message)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public bool AskYesNoQuestion(string question)
        {
            return MessageBox.Show(question, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}